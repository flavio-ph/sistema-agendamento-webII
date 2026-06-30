using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data; // Necessário para acessar o AppDbContext
using System.Security.Claims;

namespace SistemaAgendamentoWebII.Controllers;

[Authorize(Roles = "Profissional")] // Restrição de acesso a nível de classe
public class DashboardProfissionalController : Controller
{
    private readonly AppDbContext _context;

    public DashboardProfissionalController(AppDbContext context)
    {
        _context = context;
    }

    // Renomeado para Index para facilitar a rota: /DashboardProfissional/Index
    public async Task<IActionResult> Index()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

        var userId = Guid.Parse(userIdString);

        var profissional = await _context.Professionals
            .Include(p => p.User)
            .Include(p => p.Establishment)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profissional == null)
        {
            // Retorna uma view específica para profissionais cujo perfil ainda não carregou
            return View("PerfilIncompleto");
        }

        return View("~/Views/Dashboard/DashboardProfissional.cshtml", profissional);
    }
}