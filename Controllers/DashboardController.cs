using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;
using SistemaAgendamentoWebII.Models.ViewModels;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaAgendamentoWebII.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Profissional"))
                return RedirectToAction("DashboardProfissional");

            if (User.IsInRole("Empresa"))    
            return RedirectToAction("DashboardEmpresa");

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);

            var agendamentos = await _context.Agendamentos
                .Where(a => a.ClienteId == userId)
                .Include(a => a.Service)
                .OrderBy(a => a.AppointmentDate)
                .ToListAsync();

            return View(agendamentos);
        }

        [AllowAnonymous] 
        public async Task<IActionResult> Perfil(Guid id)
        {
            var profissional = await _context.Professionals
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (profissional == null)
            {
                profissional = await _context.Professionals
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            if (profissional == null) return NotFound();

            return View(profissional);
        }

        [Authorize(Roles = "Profissional")]
        [HttpGet]
        public async Task<IActionResult> DashboardProfissional()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var prof = await _context.Professionals
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));

            if (prof == null) return Forbid();

            var dataHoje = DateTime.Now;
            var hojeDateOnly = DateOnly.FromDateTime(dataHoje);

            var agendamentosBase = _context.Agendamentos
                .Include(a => a.Service)
                .Where(a => a.ProfessionalId == prof.Id);

            var agendamentosHojeCount = await agendamentosBase
                .CountAsync(a => a.AppointmentDate == hojeDateOnly && a.Status != "Cancelado");

            var pendentesCount = await agendamentosBase
                .CountAsync(a => a.Status == "Pendente" || a.Status == "Aguardando");

            var ganhosMensais = await agendamentosBase
                .Where(a => a.AppointmentDate.Month == dataHoje.Month
                         && a.AppointmentDate.Year == dataHoje.Year
                         && a.Status != "Cancelado")
                .SumAsync(a => a.Service.Price);
     
            var proximosAtendimentos = await agendamentosBase
                .Include(a => a.Client)
                .Where(a => a.AppointmentDate >= hojeDateOnly)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Take(5)
                .ToListAsync();

            ViewBag.ProximosAtendimentos = proximosAtendimentos;
           
            var viewModel = new SistemaAgendamentoWebII.Models.ViewModels.ProfessionalDashboardViewModel
            {
                User = prof.User,
                Professional = prof,
                AppointmentsTodayCount = agendamentosHojeCount,
                PendingAppointmentsCount = pendentesCount,
                MonthlyEarnings = ganhosMensais,
                AverageRating = (double)prof.AverageRating 
            };

            return View(viewModel);
        }

        [Authorize(Roles = "Empresa")]
        public async Task<IActionResult> DashboardEmpresa()
        {
            if (!User.IsInRole("Empresa")) return Forbid();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            var establishment = await _context.Set<Establishment>()
                .Include(e => e.Professionals)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(e => e.UserId == userId);

            if (establishment == null) return NotFound("Empresa não encontrada.");

            var dataAtual = DateTime.Now;
            var dataAtualDateOnly = DateOnly.FromDateTime(dataAtual);
   
            var agendamentosDaEmpresa = _context.Agendamentos
                .Include(a => a.Professional)
                .ThenInclude(p => p.User)
                .Include(a => a.Service)
                .Where(a => a.Professional.EstablishmentId == establishment.Id);

            
            var recentAppointments = await agendamentosDaEmpresa
                .Include(a => a.Client) 
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Take(5) 
                .ToListAsync();

            var totalAppointments = await agendamentosDaEmpresa.CountAsync();
            var appointmentsToday = await agendamentosDaEmpresa.CountAsync(a => a.AppointmentDate == dataAtualDateOnly);
            var monthlyRevenue = await agendamentosDaEmpresa
                .Where(a => a.AppointmentDate.Month == dataAtual.Month && a.AppointmentDate.Year == dataAtual.Year)
                .SumAsync(a => a.Service.Price);
            var platformNotices = await _context.Set<Notification>()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5) 
                .ToListAsync();
            var viewModel = new EstablishmentDashboardViewModel
            {
                EstablishmentId = establishment.Id,
                User = user,
                Establishment = establishment,

                ActiveProfessionalsCount = establishment.Professionals?.Count() ?? 0,
                TotalProfessionals = establishment.Professionals?.Count() ?? 0,
                Professionals = establishment.Professionals?.ToList() ?? new List<Professional>(),
                TotalAppointments = totalAppointments,
                AppointmentsTodayCount = appointmentsToday,
                MonthlyRevenue = monthlyRevenue,
                PendingApprovalsCount = platformNotices.Count(n => !n.IsRead),

                RecentAppointments = recentAppointments,
                PlatformNotices = platformNotices
            };

            return View(viewModel);
        }

        // POST: Dashboard/LimparAvisos
        [HttpPost]
        [Authorize(Roles = "Empresa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LimparAvisos()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);

            var notificacoes = await _context.Set<Notification>()
                .Where(n => n.UserId == userId)
                .ToListAsync();

            if (notificacoes.Any())
            {
                _context.Set<Notification>().RemoveRange(notificacoes);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(DashboardEmpresa));
        }

        // POST: Dashboard/RemoverAvisoIndividual
        [HttpPost]
        [Authorize(Roles = "Empresa")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoverAviso(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);

            // Busca apenas o aviso específico, garantindo que ele pertence ao gestor logado (segurança)
            var notificacao = await _context.Set<Notification>()
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notificacao != null)
            {
                _context.Set<Notification>().Remove(notificacao);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(DashboardEmpresa));
        }

    }
}