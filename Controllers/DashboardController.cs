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

        // Action para Clientes (Padrão)
        public async Task<IActionResult> Index()
        {
            // Se for um Profissional, desvia para o painel dele
            if (User.IsInRole("Profissional"))
                return RedirectToAction("DashboardProfissional");

            // Se for uma Empresa, desvia para o novo painel dela
            if (User.IsInRole("Empresa"))    
            return RedirectToAction("DashboardEmpresa");

            // Se não for nenhum dos dois, assume-se que é Cliente
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

        [AllowAnonymous] // Público: qualquer um pode ver o perfil
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

            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
            if (prof == null) return Forbid();

            // Filtra agendamentos de hoje em diante, ordena pelos mais próximos e pega apenas os 5 primeiros
            var hoje = DateOnly.FromDateTime(DateTime.Now);

            var proximosAtendimentos = await _context.Agendamentos
                .Include(a => a.Service)
                .Include(a => a.Client)
                .Where(a => a.ProfessionalId == prof.Id && a.AppointmentDate >= hoje)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .Take(5)
                .ToListAsync();

            // Envia a lista para a View
            ViewBag.ProximosAtendimentos = proximosAtendimentos;

            // (Se você tiver outras contas ou ViewBags aqui para os números lá de cima, pode manter!)

            return View();
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

            // Base de agendamentos filtrada pela empresa atual
            var agendamentosDaEmpresa = _context.Agendamentos
                .Include(a => a.Professional)
                .ThenInclude(p => p.User)
                .Include(a => a.Service)
                .Where(a => a.Professional.EstablishmentId == establishment.Id);

            // BUSCA OS AGENDAMENTOS RECENTES (Incluindo dados do Cliente e do Profissional)
            var recentAppointments = await agendamentosDaEmpresa
                .Include(a => a.Client) // Necessário para exibir o nome do cliente
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.StartTime)
                .Take(5) // Limita aos 5 mais recentes
                .ToListAsync();

            var totalAppointments = await agendamentosDaEmpresa.CountAsync();
            var appointmentsToday = await agendamentosDaEmpresa.CountAsync(a => a.AppointmentDate == dataAtualDateOnly);
            var monthlyRevenue = await agendamentosDaEmpresa
                .Where(a => a.AppointmentDate.Month == dataAtual.Month && a.AppointmentDate.Year == dataAtual.Year)
                .SumAsync(a => a.Service.Price);

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
                PendingApprovalsCount = 0,

                PlatformNotices = new List<Notification>(),

                // PREENCHE A NOVA PROPRIEDADE:
                RecentAppointments = recentAppointments
            };

            return View(viewModel);
        }

    }
}