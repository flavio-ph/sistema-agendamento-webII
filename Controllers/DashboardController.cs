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

            ViewBag.TotalAppointments = await agendamentosBase.CountAsync();

            // === INÍCIO DO CÁLCULO DO GRÁFICO SEMANAL (Segunda a Domingo) ===
            int diff = (7 + (dataHoje.DayOfWeek - DayOfWeek.Monday)) % 7;
            var inicioSemana = hojeDateOnly.AddDays(-1 * diff);
            var fimSemana = inicioSemana.AddDays(6);

            var agendamentosSemana = await agendamentosBase
                .Where(a => a.AppointmentDate >= inicioSemana
                         && a.AppointmentDate <= fimSemana
                         && a.Status != "Cancelado")
                .ToListAsync();

            int maxCount = 1;
            for (int i = 0; i < 7; i++)
            {
                int count = agendamentosSemana.Count(a => a.AppointmentDate == inicioSemana.AddDays(i));
                if (count > maxCount) maxCount = count;
            }

            var nomesDias = new[] { "Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom" };
            var listaDias = new List<ChartDayData>();

            for (int i = 0; i < 7; i++)
            {
                var data = inicioSemana.AddDays(i);
                int count = agendamentosSemana.Count(a => a.AppointmentDate == data);

                listaDias.Add(new ChartDayData
                {
                    DayName = nomesDias[i],
                    Count = count,
                    PercentageValue = (int)Math.Round((double)count / maxCount * 100),
                    IsToday = data == hojeDateOnly
                });
            }

            ViewBag.WeeklyOverview = listaDias;
            // === FIM DO CÁLCULO DO GRÁFICO ===

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

        [Authorize(Roles = "Cliente")]
        [HttpGet]
        public async Task<IActionResult> MeuPerfil()
        {
            // Pega o ID do usuário logado através do token/cookie de autenticação
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);

            // Busca os dados do usuário no banco
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return View(user); // Envia o modelo do usuário para a tela
        }

        [Authorize(Roles = "Cliente")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MeuPerfil(string email, string currentPassword, string newPassword, string confirmNewPassword)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var userId = Guid.Parse(userIdString);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            // 1. Atualização do E-mail
            if (string.IsNullOrWhiteSpace(email))
            {
                ModelState.AddModelError("Email", "O e-mail não pode ficar vazio.");
                return View(user);
            }

            // Verifica se o novo e-mail já está sendo usado por outro usuário
            var emailEmUso = await _context.Users.AnyAsync(u => u.Email == email && u.Id != userId);
            if (emailEmUso)
            {
                ModelState.AddModelError("Email", "Este e-mail já está cadastrado para outra conta.");
                return View(user);
            }

            user.Email = email;

            // 2. Atualização da Senha (só processa se o usuário digitou algo nos campos de senha)
            if (!string.IsNullOrEmpty(currentPassword) || !string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmNewPassword))
            {
                // Verifica se a senha atual confere
                if (string.IsNullOrEmpty(currentPassword) || !BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                {
                    ModelState.AddModelError("CurrentPassword", "A senha atual está incorreta.");
                    return View(user);
                }

                // Regra de tamanho da nova senha
                if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
                {
                    ModelState.AddModelError("NewPassword", "A nova senha deve ter pelo menos 6 caracteres.");
                    return View(user);
                }

                // Verifica se a confirmação bate
                if (newPassword != confirmNewPassword)
                {
                    ModelState.AddModelError("ConfirmNewPassword", "A confirmação não bate com a nova senha.");
                    return View(user);
                }

                // Criptografa e substitui a senha antiga
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "O seu perfil foi atualizado com sucesso!";
            return RedirectToAction("MeuPerfil");
        }

        [HttpPost]
        [Authorize(Roles = "Cliente")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AvaliarProfissional(Guid appointmentId, int nota)
        {
            // 1. Busca o agendamento e inclui os dados do profissional
            var appt = await _context.Agendamentos
                .Include(a => a.Professional)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);

            // 2. Validações de segurança
            if (appt == null || appt.Status != "Concluído")
            {
                TempData["ErrorMessage"] = "Apenas serviços concluídos podem ser avaliados.";
                return RedirectToAction("Index");
            }

            if (appt.Rating != null)
            {
                TempData["ErrorMessage"] = "Este atendimento já foi avaliado.";
                return RedirectToAction("Index");
            }

            // 3. Salva a nota (de 1 a 5)
            appt.Rating = nota;
            _context.Agendamentos.Update(appt);

            // 4. Recalcula a média de notas do profissional
            var todasNotas = await _context.Agendamentos
                .Where(a => a.ProfessionalId == appt.ProfessionalId && a.Rating != null)
                .Select(a => a.Rating.Value)
                .ToListAsync();

            todasNotas.Add(nota); // Inclui a nota atual no cálculo

            appt.Professional.AverageRating = (decimal)todasNotas.Average();
            _context.Professionals.Update(appt.Professional);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profissional avaliado com sucesso! Obrigado pelo seu feedback.";
            return RedirectToAction("Index");
        }

    }
}
public class ChartDayData
{
    public string DayName { get; set; }
    public int Count { get; set; }
    public int PercentageValue { get; set; }
    public bool IsToday { get; set; }
}