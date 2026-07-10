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

            // --- INÍCIO DA TRAVA DE SEGURANÇA ---
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (servico.Professional.UserId.ToString() == userIdString)
            {
                // Impede o auto-agendamento e redireciona com uma mensagem de erro
                TempData["ErrorMessage"] = "Você não pode agendar um horário consigo mesmo.";
                return RedirectToAction("Index", "Home");
            }
            // --- FIM DA TRAVA DE SEGURANÇA ---

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

            // Busca o serviço e as informações da Empresa vinculada ao profissional (se existir)
            var servico = await _context.Services
                .Include(s => s.Professional)
                    .ThenInclude(p => p.User)
                .Include(s => s.Professional)
                    .ThenInclude(p => p.Establishment)
                .FirstOrDefaultAsync(s => s.Id == model.ServiceId);

            if (servico == null) return NotFound();

            // 1. Mapeamento de campos e cálculo do horário de término
            model.Id = Guid.NewGuid();
            model.ClienteId = Guid.Parse(userIdString);
            model.ProfessionalId = servico.ProfessionalId;

            // O sistema calcula que horas o serviço acaba baseado na duração
            model.EndTime = model.StartTime.Add(TimeSpan.FromMinutes(servico.DurationMinutes));

            model.Status = "Confirmado";
            model.CreatedAt = DateTime.UtcNow;

            // 2. Validação de data e hora retroativas
            var dataAtual = DateOnly.FromDateTime(DateTime.Now);
            var horaAtual = DateTime.Now.TimeOfDay;

            if (model.AppointmentDate < dataAtual || (model.AppointmentDate == dataAtual && model.StartTime <= horaAtual))
            {
                ModelState.AddModelError("AppointmentDate", "A data e hora do agendamento devem ser no futuro.");
            }

            // ==========================================
            // 3. NOVA TRAVA: VERIFICAÇÃO DE CHOQUE DE HORÁRIOS
            // ==========================================

            // Busca se existe ALGUM agendamento para este profissional, nesta mesma data,
            // que esteja Confirmado ou Pendente, e que conflite com o horário solicitado.
            var choqueDeHorario = await _context.Agendamentos
                .AnyAsync(a => a.ProfessionalId == model.ProfessionalId
                            && a.AppointmentDate == model.AppointmentDate
                            && a.Status != "Cancelado" // Ignora agendamentos cancelados
                                                       // A regra de sobreposição: 
                                                       // O novo começa ANTES do existente terminar E o novo termina DEPOIS do existente começar
                            && model.StartTime < a.EndTime
                            && model.EndTime > a.StartTime);

            if (choqueDeHorario)
            {
                // Se achou conflito, adiciona o erro no ModelState para travar o salvamento e avisar o usuário
                ModelState.AddModelError("StartTime", "Este horário não está disponível. O profissional já possui um compromisso neste intervalo.");
            }
            // ==========================================

            // 4. Limpeza do ModelState e verificação final
            ModelState.Remove("Service");
            ModelState.Remove("Client");
            ModelState.Remove("Professional");
            ModelState.Remove("ClienteId");
            ModelState.Remove("ProfessionalId");
            ModelState.Remove("EndTime");
            ModelState.Remove("Status");

            // Se houve erro de data retroativa OU choque de horário, a model NÃO será válida
            if (!ModelState.IsValid)
            {
                ViewBag.Servico = servico;
                return View(model); // Devolve para a tela mostrando a mensagem de erro
            }

            // Se passou por tudo, salva no banco
            _context.Agendamentos.Add(model);
            if (servico.Professional.EstablishmentId != null && servico.Professional.Establishment != null)
            {
                var notificacao = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = servico.Professional.Establishment.UserId, // Direciona para o Gestor da Empresa
                    Title = "Novo Agendamento na Equipe",
                    Message = $"Novo agendamento com {servico.Professional.User.Name} no dia {model.AppointmentDate:dd/MM/yyyy} às {model.StartTime:hh\\:mm}.",
                    Type = "Agendamento",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };
                _context.Set<Notification>().Add(notificacao);
            }
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "O seu agendamento foi confirmado com sucesso!";

            return RedirectToAction("Index", "Dashboard");
        }

        [Authorize(Roles = "Profissional")]
        [HttpGet]
        public async Task<IActionResult> ProfessionalAgenda()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            // 1. Localiza o ID de Profissional correspondente ao usuário logado
            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
            if (prof == null) return Forbid();

            // 2. Busca todos os agendamentos vinculados a este profissional ordenados por data e hora
            var agendamentos = await _context.Agendamentos
                .Include(a => a.Service)
                .Include(a => a.Client)
                .Where(a => a.ProfessionalId == prof.Id)
                .OrderBy(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync();

            return View(agendamentos);
        }

        [HttpPost]
        [Authorize(Roles = "Profissional")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, string status)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            // 1. Busca o profissional logado
            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
            if (prof == null) return Forbid();

            // 2. Busca o agendamento no banco
            var appointment = await _context.Agendamentos.FindAsync(id);
            if (appointment == null) return NotFound();

            // 3. Trava de Segurança: Garante que o profissional só pode alterar seus próprios agendamentos
            if (appointment.ProfessionalId != prof.Id) return Forbid();

            // 4. Atualiza e salva
            appointment.Status = status;
            _context.Agendamentos.Update(appointment);
            await _context.SaveChangesAsync();

            // 5. Devolve o usuário para a tela de agenda
            return RedirectToAction("ProfessionalAgenda");
        }
    }
}