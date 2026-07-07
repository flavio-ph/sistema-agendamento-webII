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

    public async Task<IActionResult> Index()
    {
        // Busca profissionais ativos, limitando a 20 para evitar gargalos de performance iniciais
        var profissionais = await _context.Professionals
            .Include(p => p.User)
            .Include(p => p.Services)
            .Where(p => p.IsActive)
            .OrderByDescending(p => p.AverageRating)
            .Take(20)
            .ToListAsync();

        return View(profissionais);
    }
}