using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaAgendamentoWebII.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // 1. TELA PÚBLICA: Landing Page Institucional
    public IActionResult Index()
    {
        // Se já estiver logado, atira o utilizador para o seu respectivo painel
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            if (User.IsInRole("Profissional"))
                return RedirectToAction("DashboardProfissional", "Dashboard");

            return RedirectToAction("Index", "Dashboard");
        }

        // Retorna uma View simples, sem modelo de profissionais
        return View();
    }

    // 2. TELA PRIVADA: Listagem de profissionais para clientes logados
    [Authorize]
    public async Task<IActionResult> Explorar()
    {
        var profissionais = await _context.Professionals
            .Include(p => p.User)
            .Include(p => p.Services)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.AverageRating)
            .ToListAsync();

        return View(profissionais);
    }
}