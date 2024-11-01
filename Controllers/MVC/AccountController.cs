using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Security.Cryptography;
using HorariosIPBejaMVC.Data;
using Microsoft.AspNetCore.Authorization;
using HorariosIPBejaMVC.Models.ViewModels;

namespace HorariosIPBejaMVC.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão de contas de utilizadores, incluindo autenticação e autorização.
    /// </summary>
    public class AccountController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        /// <summary>
        /// Construtor do <see cref="AccountController"/>.
        /// </summary>
        /// <param name="context">Contexto da base de dados da aplicação.</param>
        /// <param name="logger">Logger para registar eventos e erros.</param>
        public AccountController(ApplicationDbContext context, ILogger<AccountController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Exibe a página de login para os utilizadores.
        /// </summary>
        /// <returns>Vista da página de login.</returns>
        /// <remarks>
        /// [Referência](https://learn.microsoft.com/pt-pt/aspnet/core/security/authentication/cookie?view=aspnetcore-6.0)
        /// </remarks>
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                _logger.LogInformation("Utilizador já autenticado tentou aceder à página de login.");
                return RedirectToAction("Index", "Home");
            }
            return View(new LoginViewModel());
        }

        /// <summary>
        /// Processa as credenciais de login submetidas pelo utilizador.
        /// </summary>
        /// <param name="model">Modelo de vista contendo as credenciais de login.</param>
        /// <returns>Redireciona para a página inicial ou retorna à vista de login com erros.</returns>
        /// <remarks>
        /// [Referência](https://learn.microsoft.com/pt-pt/aspnet/core/security/authentication/cookie?view=aspnetcore-6.0#signing-in)
        /// </remarks>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Falha no login: campos inválidos preenchidos por {Email}", model.Email);
                ViewBag.Error = "Por favor, preencha todos os campos corretamente.";
                return View(model);
            }

            // Hash da palavra-passe de entrada utilizando SHA256
            byte[] palavraPasseHash;
            using (var sha256 = SHA256.Create())
            {
                palavraPasseHash = sha256.ComputeHash(Encoding.UTF8.GetBytes(model.PalavraPasse));
            }

            // Procurar o utilizador na base de dados com email e palavra-passe correspondentes
            var utilizador = await _context.UTILIZADORs
                .Include(u => u.tipo_utilizadors) // Carregar as roles associadas
                .FirstOrDefaultAsync(u => u.email == model.Email && u.senha == palavraPasseHash);

            if (utilizador != null)
            {
                // Criar as claims do utilizador
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, utilizador.id.ToString()),
                    new Claim(ClaimTypes.Name, utilizador.nome),
                    new Claim(ClaimTypes.Email, utilizador.email)
                };

                // Obter os tipos de utilizador (roles)
                var tiposUtilizador = utilizador.tipo_utilizadors.Select(t => t.descricao).ToList();

                // Adicionar as roles às claims
                foreach (var tipo in tiposUtilizador)
                {
                    claims.Add(new Claim(ClaimTypes.Role, tipo));
                }

                // Criar a identidade do utilizador com as claims e o esquema de autenticação por cookies
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Definir as propriedades de autenticação
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false, // A autenticação não persiste após fechar o navegador
                };

                // Efetuar o login assinando o utilizador
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                _logger.LogInformation("Login bem-sucedido para {Email}", model.Email);

                // Redirecionar para a página inicial após o login bem-sucedido
                return RedirectToAction("Index", "Home");
            }
            else
            {
                _logger.LogWarning("Falha no login: email ou palavra-passe inválidos para {Email}", model.Email);
                ViewBag.Error = "Email ou palavra-passe inválidos.";
                return View(model);
            }
        }

        /// <summary>
        /// Efetua o logout do utilizador autenticado.
        /// </summary>
        /// <returns>Redireciona para a página de login.</returns>
        /// <remarks>
        /// [Referência](https://learn.microsoft.com/pt-pt/aspnet/core/security/authentication/cookie?view=aspnetcore-6.0#signing-out)
        /// </remarks>
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            if (User.Identity.IsAuthenticated)
            {
                _logger.LogInformation("Utilizador {Email} efetuou logout.", User.Identity.Name);
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
            return RedirectToAction("Login", "Account");
        }

        /// <summary>
        /// Exibe a página de acesso negado para utilizadores sem permissões adequadas.
        /// </summary>
        /// <returns>Vista da página de acesso negado.</returns>
        /// <remarks>
        /// [Referência](https://learn.microsoft.com/pt-pt/aspnet/core/security/authorization/policies?view=aspnetcore-6.0)
        /// </remarks>
        [HttpGet]
        public IActionResult AccessDenied()
        {
            _logger.LogWarning("Acesso negado para o utilizador {Email}.", User.Identity.IsAuthenticated ? User.Identity.Name : "Não autenticado");
            return View();
        }
    }
}
