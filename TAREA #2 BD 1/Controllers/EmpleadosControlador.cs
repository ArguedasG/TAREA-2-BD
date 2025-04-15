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
                var codigoError = await _databaseService.LoginUsuarioAsync(model.Username, model.Password);

                switch (codigoError)
                {
                    case 0:
                        // Login exitoso
                        return RedirectToAction("Index");

                    case 50001:
                        ModelState.AddModelError("", "El nombre de usuario no existe.");
                        break;

                    case 50002:
                        ModelState.AddModelError("", "Contraseña incorrecta.");
                        break;

                    case 50003:
                        ModelState.AddModelError("", "Demasiados intentos fallidos. Acceso bloqueado temporalmente.");
                        break;

                    case 50008:
                        ModelState.AddModelError("", "Error del sistema. Intente más tarde.");
                        break;

                    default:
                        ModelState.AddModelError("", "Error desconocido. Código: " + codigoError);
                        break;
                }
            }

            return View(model);
        }


        // Listar empleados
        public async Task<IActionResult> Index(string filtro = "")
        {
            /*
            var empleados = await _databaseService.ListarEmpleadosAsync(filtro);
            ViewBag.Filtro = filtro;
            return View(empleados);
            */
            return View();
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
