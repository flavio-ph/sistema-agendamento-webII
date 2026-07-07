using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;
using System.Security.Claims;

namespace SistemaAgendamentoWebII.Controllers
{
    [Authorize(Roles = "Profissional")]
    public class ServiceController : Controller
    {
        private readonly AppDbContext _context;

        public ServiceController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Ajustado para _context.Categoria (nome exato no AppDbContext)
            var categories = await _context.Categories.ToListAsync();

            if (!categories.Any())
            {
                var defaultCategory = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Geral"
                    // Removido o campo Description, pois não existe na Model
                };
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
                categories.Add(defaultCategory);
            }

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Verificação de segurança para os Warnings de nulo
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));

            if (prof == null) return Forbid();

            model.Id = Guid.NewGuid();
            model.ProfessionalId = prof.Id;
            model.IsActive = true;

            ModelState.Remove("Professional");
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId); return View(model);
            }

            _context.Services.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction("DashboardProfissional", "Dashboard");
        }
    }
}