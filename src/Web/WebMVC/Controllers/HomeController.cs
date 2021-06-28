﻿using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Dominio.Repositorio;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebMVC.Models;
using X.PagedList;

namespace WebMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private IDoseRepositorio _doseRepositorio;
        public HomeController(ILogger<HomeController> logger, IDoseRepositorio doseRepositorio)
        {
            _logger = logger;
            _doseRepositorio = doseRepositorio;
        }

        public async Task<IActionResult> Index(int? pagina)
        {
            const int itensPorPagina = 20; //TODO: Buscar isso do appsettings
            int numeroPagina = (pagina ?? 1);

            var doses = await _doseRepositorio.Todos();
            return View(doses.ToPagedList(numeroPagina, itensPorPagina));
        }

        [HttpGet("login")]
        public IActionResult Login(string returnUrl)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Validar(string usuario, string senha, string returnUrl)
        {
            ViewData["ReturnUrl"] = string.IsNullOrEmpty(returnUrl) ? "/importar" : returnUrl;
            if (usuario == "admin" && senha == "admin")
            {
                var claims = new List<Claim>
                {
                    new Claim("usario", usuario),
                    new Claim(ClaimTypes.NameIdentifier, usuario),
                    new Claim(ClaimTypes.Name, "Administrador"),
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                await HttpContext.SignInAsync(claimsPrincipal);
                return Redirect(returnUrl);
            }
            TempData["Erro"] = "Erro! Usuário ou Senha inválidos!";
            return View("login");
        }
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return Redirect("/");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
