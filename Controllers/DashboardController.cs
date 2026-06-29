using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using System.Security.Claims;
using System.Linq; // Necessário para .Where() e .Include()

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
            if (!User.IsInRole("Cliente")) return Forbid();

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
            // Ajustado: Busca o profissional pelo UserId ou ProfessionalId
            // Como passamos o NameIdentifier, estamos buscando o perfil do profissional pelo UserId
            var profissional = await _context.Professionals
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.UserId == id);

            if (profissional == null)
            {
                // Tenta buscar por ProfessionalId caso o ID passado não seja o UserId
                profissional = await _context.Professionals
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.Id == id);
            }

            if (profissional == null) return NotFound();

            return View(profissional);
        }

        public IActionResult DashboardProfissional()
        {
            if (!User.IsInRole("Profissional")) return Forbid();

            return View();
        }
    }
}