using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models.ViewModels;
using System.Security.Claims;

namespace SistemaAgendamentoWebII.Controllers;

[Authorize(Roles = "Profissional")]
public class DashboardProfissionalController : Controller
{
    private readonly AppDbContext _context;

    public DashboardProfissionalController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        var prof = await _context.Professionals.Include(p => p.User).FirstOrDefaultAsync(p => p.UserId == userId);

        var viewModel = new ProfessionalDashboardViewModel
        {
            User = user,
            Professional = prof,
            AppointmentsTodayCount = 5, // Exemplo: substitua pelo seu cálculo real
            PendingAppointmentsCount = 2,
            MonthlyEarnings = 12450.00m,
            AverageRating = 4.9
        };

        return View(viewModel); // Retorna a ViewModel fortemente tipada
    }
}