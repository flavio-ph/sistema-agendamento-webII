using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;
using SistemaAgendamentoWebII.Models;

namespace SistemaAgendamentoWebII.Controllers
{
    [Authorize(Roles = "Empresa")]
    public class EstablishmentsController : Controller
    {
        private readonly AppDbContext _context;

        public EstablishmentsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Establishments/Edit
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            // Busca a empresa associada ao UserId
            var establishment = await _context.Establishments
                .FirstOrDefaultAsync(e => e.UserId == Guid.Parse(userIdString));

            if (establishment == null)
                return NotFound("Perfil da empresa não encontrado.");

            return View(establishment);
        }

        // POST: /Establishments/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Establishment model)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login", "Account");

            var establishment = await _context.Establishments.FirstOrDefaultAsync(e => e.Id == id);

            if (establishment == null) return NotFound();

            // Segurança: Garante que apenas a empresa dona da conta pode editar
            if (establishment.UserId != Guid.Parse(userIdString))
                return Forbid();

            // Atualiza apenas os campos permitidos
            establishment.Name = model.Name;
            establishment.CNPJ = model.CNPJ;

            _context.Establishments.Update(establishment);
            await _context.SaveChangesAsync();

            return RedirectToAction("DashboardEmpresa", "Dashboard");
        }
    }
}