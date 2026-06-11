using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SistemaAgendamentoWebII.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. FASE DE REGISTRO DE SERVIÇOS (Sempre ANTES do builder.Build)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllersWithViews();

// O bloco de Autenticação precisa ficar aqui, antes do Build
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

// 2. CONSTRUÇÃO DA APLICAÇÃO (A trava da fundação)
var app = builder.Build();

// 3. FASE DE MIDDLEWARES (A ordem destas linhas é rigorosa)
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// UseAuthentication DEVE vir antes de UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();