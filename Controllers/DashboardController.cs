using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;

namespace SistemaAgendamentoWebII.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        // Extração do ID do utilizador a partir do Cookie assinado
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(userIdString, out var userId))
        {
            return RedirectToAction("Logout", "Account");
        }

        // Identificação do perfil para futura bifurcação das regras de negócio
        var userRole = User.FindFirstValue(ClaimTypes.Role);

        // Preparamos as variáveis dinâmicas para substituir os valores fixos no HTML
        // Numa próxima iteração, faremos as consultas reais (ex: _context.Appointments.CountAsync(...))
        ViewBag.Role = userRole;
        ViewBag.AgendamentosHoje = 0;
        ViewBag.NovosClientes = 0;
        ViewBag.ReceitaEstimada = 0.00m;

        return View();
    }
}