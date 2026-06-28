using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Models;
using SistemaAgendamentoWebII.Data;
using System.Security.Claims;

namespace SistemaAgendamentoWebII.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user, string PasswordConfirmation, string? Specialty, string? EstablishmentName)
    {
        if (!ModelState.IsValid) return View(user);

        // 1. Validações de Negócio
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
            return View(user);
        }

        if (user.PasswordHash != PasswordConfirmation)
        {
            ModelState.AddModelError("PasswordHash", "As senhas não conferem.");
            return View(user);
        }

        // 2. Persistência do Usuário
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;
        user.Role ??= "Cliente";

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 3. Persistência de Dados Adicionais (Caso seja Profissional ou Empresa)
        if (user.Role == "Profissional" && !string.IsNullOrEmpty(Specialty))
        {
            _context.Professionals.Add(new Professional { UserId = user.Id, Specialty = Specialty });
            await _context.SaveChangesAsync();
        }
        else if (user.Role == "Empresa" && !string.IsNullOrEmpty(EstablishmentName))
        {
            _context.Establishments.Add(new Establishment { UserId = user.Id, Name = EstablishmentName });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "Usuário ou senha inválidos.";
            return View();
        }

        if (!user.IsActive)
        {
            ViewBag.Error = "Esta conta está desativada.";
            return View();
        }

        // Criar Claims
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        // Direcionamento Inteligente conforme o Perfil
        return user.Role switch
        {
            "Profissional" => RedirectToAction("Index", "DashboardProfissional"),
            "Empresa" => RedirectToAction("Index", "DashboardEmpresa"),
            _ => RedirectToAction("Index", "Dashboard") // Padrão para Cliente
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}