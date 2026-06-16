using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;

namespace SistemaAgendamentoWebII.Controllers;

[Authorize] // Protege a criação e edição por padrão
public class ProfessionalsController : Controller
{
    private readonly AppDbContext _context;

    public ProfessionalsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Professionals/Profile/{id} (PÚBLICO)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Profile(Guid id)
    {
        var professional = await _context.Professionals
            .Include(p => p.User)
            .Include(p => p.Services)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (professional == null || !professional.IsActive) return NotFound();

        return View(professional);
    }

    // GET: /Professionals/Create (RESTRITO)
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return RedirectToAction("Login", "Account");

        // Se já for profissional, redireciona para o painel para não duplicar
        var isAlreadyPro = await _context.Professionals.AnyAsync(p => p.UserId == userId);
        if (isAlreadyPro) return RedirectToAction("Index", "Dashboard");

        return View();
    }

    // POST: /Professionals/Create (RESTRITO)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Professional professional)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdString, out var userId)) return RedirectToAction("Login", "Account");

        // Injeta os dados sistémicos invisíveis ao utilizador
        professional.UserId = userId;
        professional.AverageRating = 0;
        professional.IsActive = true;

        _context.Professionals.Add(professional);

        // Promove a Role do User no banco de dados
        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Role = "Profissional";
            _context.Users.Update(user);
        }

        await _context.SaveChangesAsync();

        // Como a Role mudou, o ideal num sistema real é forçar a atualização do Cookie.
        // Aqui fechamos a sessão para forçar o login com o novo painel administrativo.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "Account");
    }
}