using Microsoft.EntityFrameworkCore;
using HorariosIPBejaMVC.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner com filtro global de autorização (exceto para páginas públicas)
builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
});

// Configurar o ApplicationDbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar a autenticação baseada em cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2); // Defina conforme necessário
        options.SlidingExpiration = true;
    });

// Adicionar suporte para sessão
builder.Services.AddDistributedMemoryCache(); // Necessário para usar sessões
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true; // Necessário para conformidade com GDPR
    options.Cookie.SameSite = SameSiteMode.Lax; // Permite o envio do cookie em solicitações POST
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest; // Alinha com o protocolo da solicitação
});


var app = builder.Build();

// Configurar o pipeline de requisições HTTP.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Adicionar o middleware de sessão antes de autenticação e autorização
app.UseSession(); // Adiciona suporte para sessões

// Adicionar o middleware de autenticação e autorização
app.UseAuthentication();
app.UseAuthorization();

// Definir a rota padrão para Account/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
