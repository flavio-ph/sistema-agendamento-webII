using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;

namespace SistemaAgendamentoWebII.Controllers;

[Authorize]
public class ProfessionalsController : Controller
{
    private readonly AppDbContext _context;

    public ProfessionalsController(AppDbContext context)
    {
        _context = context;
    }

    // 1. Perfil Público (ou do próprio profissional se não passar ID)
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> Profile(Guid? id)
    {
        Professional professional = null;

        if (id.HasValue && id.Value != Guid.Empty)
        {
            professional = await _context.Professionals
                .Include(p => p.User)
                .Include(p => p.Services)
                .FirstOrDefaultAsync(p => p.Id == id.Value);
        }
        else if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdString, out var userId))
            {
                professional = await _context.Professionals
                    .Include(p => p.User)
                    .Include(p => p.Services)
                    .FirstOrDefaultAsync(p => p.UserId == userId);
            }
        }

        if (professional == null || !professional.IsActive) return NotFound();

        return View(professional);
    }

    // 2. GET: Edição (Carrega o formulário para o profissional logado)
    [HttpGet]
    public async Task<IActionResult> Edit()
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

        var professional = await _context.Professionals
            .FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));

        if (professional == null) return NotFound("Perfil profissional não encontrado.");

        return View(professional);
    }

    // 3. POST: Edição (Processa a gravação dos dados)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Guid id, Professional model)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");
        var userId = Guid.Parse(userIdString);

        var professional = await _context.Professionals.FirstOrDefaultAsync(p => p.Id == id);

        if (professional == null) return NotFound();

        // Segurança: Impede que um profissional edite o perfil de outro
        if (professional.UserId != userId) return Forbid();

        professional.Specialty = model.Specialty;
        professional.ExperienceYears = model.ExperienceYears;
        professional.RegistrationNumber = model.RegistrationNumber;
        professional.Biography = model.Biography;

        _context.Professionals.Update(professional);
        await _context.SaveChangesAsync();

        return RedirectToAction("DashboardProfissional", "Dashboard");
    }
}