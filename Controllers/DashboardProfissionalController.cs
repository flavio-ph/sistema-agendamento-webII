using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace SistemaAgendamentoWebII.Controllers;

[Authorize]
public class DashboardProfissionalController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}