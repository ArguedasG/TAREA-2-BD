using Microsoft.AspNetCore.Mvc;
using TAREA__2_BD_1.Models;
using TAREA__2_BD_1.Services;

namespace TAREA__2_BD_1.Controllers
{
    public class EmpleadosControlador : Controller
    {
        private readonly DatabaseService _databaseService;

        public EmpleadosControlador(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        // Login
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(UsuarioLogin model)
        {
            if (ModelState.IsValid)
            {
                var loginExitoso = await _databaseService.LoginUsuarioAsync(model.Username, model.Password);
                if (loginExitoso)
                {
                    // Simula autenticación (podemos usar cookies o Identity más adelante)
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", "Usuario o contraseña incorrectos.");
            }
            return View(model);
        }

        // Listar empleados
        public async Task<IActionResult> Index(string filtro = "")
        {
            var empleados = await _databaseService.ListarEmpleadosAsync(filtro);
            ViewBag.Filtro = filtro;
            return View(empleados);
        }

        // Insertar empleado
        public async Task<IActionResult> Crear()
        {
            ViewBag.Puestos = await _databaseService.ObtenerPuestosAsync();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Empleado empleado)
        {
            if (ModelState.IsValid)
            {
                var codigoError = await _databaseService.InsertarEmpleadoAsync(empleado);
                if (codigoError == 0)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", $"Error al insertar empleado: Código {codigoError}");
            }
            ViewBag.Puestos = await _databaseService.ObtenerPuestosAsync();
            return View(empleado);
        }
    }
}
