using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace SistemaAgendamentoWebII.Controllers
{
    [Authorize(Roles = "Profissional")]
    public class ServicesController : Controller
    {
        private readonly AppDbContext _context;

        public ServicesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _context.Categories.ToListAsync();

            // Defesa arquitetural: Se não existirem categorias no banco, cria uma padrão.
            if (!categories.Any())
            {
                var defaultCategory = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Geral",
                    Description = "Categoria Padrão"
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
            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));

            if (prof == null) return Forbid();

            // Injeta os dados de controlo sistémico
            model.Id = Guid.NewGuid();
            model.ProfessionalId = prof.Id;
            model.IsActive = true;

            // Remove a validação das propriedades de navegação para evitar falsos erros no ModelState
            ModelState.Remove("Professional");
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);
                return View(model);
            }

            _context.Services.Add(model);
            await _context.SaveChangesAsync();

            // Retorna ao painel do profissional após o sucesso
            return RedirectToAction("DashboardProfissional", "Dashboard");
        }
    }
}