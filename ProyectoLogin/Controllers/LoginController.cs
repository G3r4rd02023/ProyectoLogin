using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProyectoLogin.Models;
using ProyectoLogin.Services;
using System.Security.Claims;

namespace ProyectoLogin.Controllers
{
   
    public class LoginController : Controller
    {
        private readonly IUsuarioService _usuarioService;
        private readonly IFilesService _filesService;
        private readonly UsuarioContext _context;

        public LoginController(IUsuarioService usuarioService, IFilesService filesService, UsuarioContext context)
        {
            _usuarioService = usuarioService;
            _filesService = filesService;
            _context = context;
        }

        public IActionResult Registro()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registro(Usuario usuario, IFormFile Imagen)
        {
            Stream image = Imagen.OpenReadStream();
            string urlImagen = await _filesService.SubirArchivo(image, Imagen.FileName);

            usuario.Clave = Utilidades.EncriptarClave(usuario.Clave);
            usuario.URLFotoPerfil = urlImagen;

            Usuario usuarioCreado = await _usuarioService.SaveUsuario(usuario);

            if(usuarioCreado.Id > 0)
            {
                return RedirectToAction("IniciarSesion", "Login");
            }

            ViewData["Mensaje"] = "No se pudo crear el usuario";
            return View();
        }

        public IActionResult IniciarSesion()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> IniciarSesion(string correo, string clave)
        {
            Usuario usuarioEncontrado = await _usuarioService.GetUsuario(correo, Utilidades.EncriptarClave(clave));

            if(usuarioEncontrado == null)
            {
                ViewData["Mensaje"] = "Usuaio no encontrado";
                return View();
            }

            List<Claim> claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, usuarioEncontrado.NombreUsuario),
                new Claim("FotoPerfil", usuarioEncontrado.URLFotoPerfil),
            };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                properties
                );

            return RedirectToAction("Index", "Home");

        }




    }
}
