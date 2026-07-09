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

        public async Task<IActionResult> DashboardProfissional()
        {
            if (!User.IsInRole("Profissional")) return Forbid();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            var prof = await _context.Professionals
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == userId);

            var viewModel = new ProfessionalDashboardViewModel
            {
                User = user,
                Professional = prof,
                AppointmentsTodayCount = 5,
                PendingAppointmentsCount = 2,
                MonthlyEarnings = 12450.00m,
                AverageRating = 4.9
            };

            return View(viewModel);
        }

        public async Task<IActionResult> DashboardEmpresa()
        {
            
            if (!User.IsInRole("Empresa")) return Forbid();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            
            var establishment = await _context.Set<Establishment>()
                .Include(e => e.Professionals)
                .FirstOrDefaultAsync(e => e.UserId == userId);

           
            var viewModel = new EstablishmentDashboardViewModel
            {
                User = user,
                Establishment = establishment,
                TotalProfessionals = establishment?.Professionals?.Count ?? 0,
                AppointmentsTodayCount = 24, 
                PendingApprovalsCount = 3, 
                MonthlyRevenue = 34500.00m 
            };

            return View(viewModel);
        }

    }
}