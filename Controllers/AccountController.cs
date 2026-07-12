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
    [HttpPost]
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
        // Captura a role do formulário (que vem via JS/Alpine.js)
        string roleSelecionada = Request.Form["Role"].ToString();
        user.Role = !string.IsNullOrEmpty(roleSelecionada) ? roleSelecionada : "Cliente";

        // 1. Validações de duplicidade
        // Verifica se e-mail já existe
        if (await _context.Users.AnyAsync(u => u.Email == user.Email))
        {
            ModelState.AddModelError("Email", "Este e-mail já está cadastrado.");
        }

        // Verifica se telefone já existe
        if (await _context.Users.AnyAsync(u => u.Phone == user.Phone))
        {
            ModelState.AddModelError("Phone", "Este telefone já está em uso.");
        }

        // Validação específica de CNPJ para Empresas
        if (user.Role == "Empresa" && !string.IsNullOrEmpty(CNPJ))
        {
            if (await _context.Establishments.AnyAsync(e => e.CNPJ == CNPJ))
            {
                ModelState.AddModelError("CNPJ", "Este CNPJ já está cadastrado.");
            }
        }

        // 2. Validação básica de modelo e senhas
        // Se algum erro foi adicionado acima, ModelState.IsValid será false
        if (!ModelState.IsValid) return View(user);

        if (user.PasswordHash != PasswordConfirmation)
        {
            ModelState.AddModelError("PasswordHash", "As senhas não conferem.");
            return View(user);
        }

        // 3. Persistência do utilizador
        user.Id = Guid.NewGuid();
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
        user.IsActive = true;
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // 4. Criação de Perfis Vinculados
        if (user.Role == "Profissional")
        {
            _context.Professionals.Add(new Professional
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Specialty = Specialty ?? "Não informada",
                Biography = Biography ?? "Biografia não informada.",
                Description = Description ?? "Descrição não informada.",
                ExperienceYears = ExperienceYears ?? 0,
                RegistrationNumber = RegistrationNumber ?? "SEM-REGISTRO",
                IsActive = true
            });
        }
        else if (user.Role == "Empresa")
        {
            _context.Establishments.Add(new Establishment
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = EstablishmentName ?? "Empresa sem nome",
                CNPJ = CNPJ,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Salva o perfil (Profissional ou Empresa)
        await _context.SaveChangesAsync();

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