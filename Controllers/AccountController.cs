using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamento.Models;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;
using System.Security.Claims;

namespace SistemaAgendamentoWebII.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register() => View();

    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user, string PasswordConfirmation)
    {
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            ModelState.AddModelError("PasswordHash", "A senha é obrigatória.");
            return View(user);
        }

        // Busca assíncrona para evitar travamento de threads
        var emailExiste = await _context.Users.AnyAsync(u => u.Email == user.Email);
        if (emailExiste)
        {
            ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
            return View(user);
        }

        if (user.PasswordHash != PasswordConfirmation)
        {
            ModelState.AddModelError("PasswordHash", "As senhas não conferem.");
            return View(user);
        }

        // Criptografia segura com BCrypt
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.Role = "Cliente";
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Login));
    }

    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login() => View();

    // POST: /Account/Login
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

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        // Persistência assíncrona obrigatória com await
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return RedirectToAction("Index", "Home");
    }

    // GET: /Account/Logout
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }
}