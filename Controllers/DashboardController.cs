using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using System.Security.Claims;

namespace SistemaAgendamentoWebII.Controllers;

[Authorize(Roles = "Cliente")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

        var userId = Guid.Parse(userIdString);

        // Busca agendamentos do cliente
        var agendamentos = await _context.Agendamentos
            .Where(a => a.ClienteId == userId)
            .Include("Servico") // Garante que carrega os dados do serviço
            .OrderBy(a => a.DataHoraInicio)
            .ToListAsync();

        return View(agendamentos);
    }
}