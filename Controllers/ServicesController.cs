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

            if (!categories.Any())
            {
                var defaultCategory = new Category
                {
                    Id = Guid.NewGuid(),
                    Name = "Geral"

                };
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
                categories.Add(defaultCategory);
            }

            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdString))
            {
                var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
                if (prof != null)
                {

                    ViewBag.MeusServicos = await _context.Services
                        .Where(s => s.ProfessionalId == prof.Id)
                        .OrderBy(s => s.Name)
                        .ToListAsync();
                }
            }
            return View("~/Views/Service/Create.cshtml");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Service model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

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
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", model.CategoryId);

                return View("~/Views/Service/Create.cshtml", model);
            }

            _context.Services.Add(model);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço cadastrado com sucesso!";
            return RedirectToAction("Create");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
            if (prof == null) return Forbid();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            if (service.ProfessionalId != prof.Id) return Forbid();

            await PrepareViewBagsAsync(service.CategoryId);


            ViewBag.IsEditing = true;
            return View("~/Views/Service/Create.cshtml", service);
        }

        // POST: Services/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Service model)
        {
            if (id != model.Id) return BadRequest();

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
            if (prof == null) return Forbid();

            var serviceToUpdate = await _context.Services.FindAsync(id);
            if (serviceToUpdate == null) return NotFound();

            if (serviceToUpdate.ProfessionalId != prof.Id) return Forbid();

            ModelState.Remove("Professional");
            ModelState.Remove("Category");

            if (!ModelState.IsValid)
            {
                await PrepareViewBagsAsync(model.CategoryId);
                ViewBag.IsEditing = true;
                return View("~/Views/Service/Create.cshtml", model);
            }

            serviceToUpdate.Name = model.Name;
            serviceToUpdate.CategoryId = model.CategoryId;
            serviceToUpdate.Price = model.Price;
            serviceToUpdate.DurationMinutes = model.DurationMinutes;
            serviceToUpdate.Description = model.Description;

            _context.Services.Update(serviceToUpdate);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço atualizado com sucesso!";
            return RedirectToAction("Create");
        }

        // POST: Services/Delete/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return RedirectToAction("Login", "Account");

            var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
            if (prof == null) return Forbid();

            var service = await _context.Services.FindAsync(id);
            if (service == null) return NotFound();

            if (service.ProfessionalId != prof.Id) return Forbid();

            service.IsActive = false;
            _context.Services.Update(service);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Serviço removido com sucesso!";
            return RedirectToAction("Create");
        }

        private async Task PrepareViewBagsAsync(Guid? selectedCategoryId = null)
        {
            var categories = await _context.Categories.ToListAsync();
            if (!categories.Any())
            {
                var defaultCategory = new Category { Id = Guid.NewGuid(), Name = "Geral" };
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
                categories.Add(defaultCategory);
            }

            ViewBag.Categories = new SelectList(categories, "Id", "Name", selectedCategoryId);

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrEmpty(userIdString))
            {
                var prof = await _context.Professionals.FirstOrDefaultAsync(p => p.UserId == Guid.Parse(userIdString));
                if (prof != null)
                {
                    ViewBag.MeusServicos = await _context.Services
                        .Where(s => s.ProfessionalId == prof.Id && s.IsActive)
                        .OrderBy(s => s.Name)
                        .ToListAsync();
                }
            }
        }
    }
}