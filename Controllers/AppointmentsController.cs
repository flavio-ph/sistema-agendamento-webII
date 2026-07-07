using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SistemaAgendamentoWebII.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly AppDbContext _context;

        public AppointmentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create(Guid serviceId)
        {
            var servico = await _context.Services
                .Include(s => s.Professional)
                .ThenInclude(p => p.User)
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (servico == null) return NotFound();

            var appointment = new Appointment
            {
                ServiceId = servico.Id,
                ProfessionalId = servico.ProfessionalId,
                AppointmentDate = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
                StartTime = new TimeSpan(9, 0, 0),
                Status = "Confirmado"
            };

            ViewBag.Servico = servico;
            return View(appointment);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Appointment model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var servico = await _context.Services.FindAsync(model.ServiceId);
            if (servico == null) return NotFound();

            // Validação de data futura
            var dataAtual = DateOnly.FromDateTime(DateTime.Now);
            var horaAtual = DateTime.Now.TimeOfDay;

            if (model.AppointmentDate < dataAtual || (model.AppointmentDate == dataAtual && model.StartTime <= horaAtual))
            {
                ModelState.AddModelError("AppointmentDate", "A data e hora do agendamento devem ser no futuro.");
            }

            // Mapeamento de campos automáticos
            model.Id = Guid.NewGuid();
            model.ClienteId = Guid.Parse(userIdString);
            model.ProfessionalId = servico.ProfessionalId;
            model.EndTime = model.StartTime.Add(TimeSpan.FromMinutes(servico.DurationMinutes));
            model.Status = "Confirmado";
            model.CreatedAt = DateTime.UtcNow;

            // Remove propriedades de navegação e campos calculados do ModelState para evitar erro de validação
            ModelState.Remove("Service");
            ModelState.Remove("Client");
            ModelState.Remove("Professional");
            ModelState.Remove("ClienteId"); // Caso o binding tente validar novamente
            ModelState.Remove("ProfessionalId");
            ModelState.Remove("EndTime");
            ModelState.Remove("Status");

            if (!ModelState.IsValid)
            {
                ViewBag.Servico = servico;
                return View(model);
            }

            _context.Agendamentos.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "O seu agendamento foi confirmado com sucesso!";

            return RedirectToAction("Index", "Dashboard");
        }
    }
}