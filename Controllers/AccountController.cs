using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
    [AllowAnonymous]
    public IActionResult Register() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
    public async Task<IActionResult> Register(
        User user,
        string PasswordConfirmation,
        string? Specialty,
        string? EstablishmentName,
        string? CNPJ,
        string? Biography,
        string? Description,
        int? ExperienceYears,
        string? RegistrationNumber)
    {
        string roleSelecionada = Request.Form["Role"].ToString();
        user.Role = !string.IsNullOrEmpty(roleSelecionada) ? roleSelecionada : "Cliente";

        if (!ModelState.IsValid) return View(user);

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

        user.Id = Guid.NewGuid();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        if (user.Role == "Profissional")
        {
            var novoProfissional = new Professional
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Specialty = Specialty ?? "Não informada",
                Biography = Biography ?? "Biografia não informada.",
                Description = Description ?? "Descrição não informada.",
                ExperienceYears = ExperienceYears ?? 0,
                RegistrationNumber = RegistrationNumber ?? "SEM-REGISTRO",
                IsActive = true
            };
            _context.Professionals.Add(novoProfissional);
            await _context.SaveChangesAsync();
        }
        else if (user.Role == "Empresa" && !string.IsNullOrEmpty(EstablishmentName))
        {
            _context.Establishments.Add(new Establishment
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = EstablishmentName,
                CNPJ = CNPJ,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AllowAnonymous]
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

        string userRole = user.Role ?? "Cliente";

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, userRole)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

        return userRole switch
        {
            "Profissional" => RedirectToAction("DashboardProfissional", "Dashboard"), // Rota corrigida
            "Empresa" => RedirectToAction("DashboardEmpresa", "Dashboard"),
            _ => RedirectToAction("Index", "Dashboard")
        };
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("DashboardProfissional", "Dashboard");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(string email)
    {
        // 1. Verifica se o campo veio vazio
        if (string.IsNullOrEmpty(email))
        {
            ModelState.AddModelError("Email", "Por favor, informe o seu e-mail.");
            return View();
        }

        // 2. Busca o usuário no banco de dados
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        // 3. Se NÃO encontrar, devolve um erro na mesma tela
        if (user == null)
        {
            ModelState.AddModelError("Email", "Este e-mail não foi encontrado em nosso banco de dados.");
            return View();
        }

        // 4. Se ENCONTRAR o usuário:
        // AQUI ENTRARÁ A LÓGICA DE ENVIO DE E-MAIL REAL NO FUTURO
        // Ex: _emailService.SendResetPasswordEmail(user.Email, token);

        // Manda a mensagem de sucesso e redireciona pro login
        TempData["SuccessMessage"] = "Um link de recuperação foi enviado para o seu e-mail.";

        return RedirectToAction("Login");
    }
}