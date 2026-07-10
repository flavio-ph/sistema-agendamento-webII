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

        professional.UserId = userId;
        professional.AverageRating = 0;
        professional.IsActive = true;

        _context.Professionals.Add(professional);

        var user = await _context.Users.FindAsync(userId);
        if (user != null)
        {
            user.Role = "Profissional";
            _context.Users.Update(user);
        }

        await _context.SaveChangesAsync();

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        return RedirectToAction("Login", "Account");
    }

    // GET: /Professionals/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id)
    {
        var professional = await _context.Professionals.FindAsync(id);
        if (professional == null) return NotFound();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (professional.UserId.ToString() != userIdString) return Forbid();

        return View(professional);
    }

    // POST: /Professionals/Edit/{id}
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Professional model)
    {
        if (id != model.Id) return BadRequest();

        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var professional = await _context.Professionals.FindAsync(id);

        if (professional == null) return NotFound();
        if (professional.UserId.ToString() != userIdString) return Forbid();


        professional.Specialty = model.Specialty;
        professional.ExperienceYears = model.ExperienceYears;
        professional.RegistrationNumber = model.RegistrationNumber;
        professional.Biography = model.Biography;

        _context.Professionals.Update(professional);
        await _context.SaveChangesAsync();

        return RedirectToAction("DashboardProfissional", "Dashboard");
    }

    // GET: Professionals/Vincular
    [Authorize(Roles = "Empresa")]
    public async Task<IActionResult> Vincular(Guid establishmentId, string searchString)
    {
        ViewBag.EstablishmentId = establishmentId;

        var query = _context.Professionals
            .Include(p => p.User)
            .Where(p => p.EstablishmentId == null);

        if (!string.IsNullOrEmpty(searchString))
        {
            query = query.Where(p => p.User.Name.Contains(searchString) || p.User.Email.Contains(searchString));
        }

        var profissionaisDisponiveis = await query.ToListAsync();
        return View(profissionaisDisponiveis);
    }

    // POST: Professionals/VincularConfirmar
    [HttpPost]
    [Authorize(Roles = "Empresa")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VincularConfirmar(Guid professionalId, Guid establishmentId)
    {
        var profissional = await _context.Professionals
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == professionalId);

        var establishment = await _context.Set<Establishment>().FindAsync(establishmentId);

        if (profissional == null || establishment == null) return NotFound();

        profissional.EstablishmentId = establishmentId;
        _context.Update(profissional);

        var notificacao = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = establishment.UserId,
            Title = "Novo Membro na Equipe",
            Message = $"O profissional {profissional.User?.Name} foi adicionado à sua equipe com sucesso.",
            Type = "Equipe",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Notification>().Add(notificacao);

        await _context.SaveChangesAsync();

        return RedirectToAction("GerirEquipe");
    }


    // POST: Professionals/RemoverVinculo
    [HttpPost]
    [Authorize(Roles = "Empresa")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverVinculo(Guid professionalId)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

        var profissional = await _context.Professionals
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == professionalId);

        if (profissional == null) return NotFound();

        var agendamentosDoProfissional = await _context.Agendamentos
            .Where(a => a.ProfessionalId == professionalId)
            .ToListAsync();

        if (agendamentosDoProfissional.Any())
        {
            _context.Agendamentos.RemoveRange(agendamentosDoProfissional);
        }

        profissional.EstablishmentId = null;
        _context.Update(profissional);

        var notificacao = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = Guid.Parse(userIdString),
            Title = "Membro Removido",
            Message = $"O profissional {profissional.User?.Name} foi removido da sua equipe e os seus agendamentos atrelados à empresa foram cancelados.",
            Type = "Equipe",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.Set<Notification>().Add(notificacao);

        await _context.SaveChangesAsync();

        return RedirectToAction("GerirEquipe");
    }

    // GET: Professionals/GerirEquipe
    [Authorize(Roles = "Empresa")]
    public async Task<IActionResult> GerirEquipe()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

        var userId = Guid.Parse(userIdString);
       
        var establishment = await _context.Set<Establishment>()
            .FirstOrDefaultAsync(e => e.UserId == userId);

        if (establishment == null) return NotFound("Empresa não encontrada.");

        var equipe = await _context.Professionals
            .Include(p => p.User)
            .Where(p => p.EstablishmentId == establishment.Id)
            .ToListAsync();

        ViewBag.EstablishmentId = establishment.Id;

        return View(equipe);
    }

}