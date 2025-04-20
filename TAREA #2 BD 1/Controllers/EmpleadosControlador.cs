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

        // Acción para mostrar el formulario de inicio de sesión
        public IActionResult Login()
        {
            return View();
        }

        // Acción para procesar el inicio de sesión
        [HttpPost]
        public async Task<IActionResult> Login(UsuarioLogin model)
        {
            if (ModelState.IsValid)
            {
                var resultado = await _databaseService.LoginUsuarioAsync(model.Username, model.Password);
                int codigoError = resultado.CodigoError;
                int? idUsuario = resultado.UserId;

                switch (codigoError)
                {
                    case 0:
                        HttpContext.Session.SetInt32("idUsuario", idUsuario.Value);
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


        // Acción para mostrar la lista de empleados
        public async Task<IActionResult> Index(string filtro)
        {
            int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;
            var empleados = await _databaseService.ListarEmpleadosAsync(filtro, idUsuario);
            ViewBag.Filtro = filtro;
            return View(empleados);
        }

        // Acción para mostrar el formulario de creación
        public async Task<IActionResult> Crear()
        {
            try
            {
                int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;
                var puestos = await _databaseService.ObtenerPuestosAsync(idUsuario);
                ViewBag.Puestos = puestos ?? new List<Puesto>();
            }
            catch (Exception ex)
            {
                ViewBag.Puestos = new List<Puesto>();
                ModelState.AddModelError("", "Error al obtener los puestos. Intente más tarde.");
            }
            return View(new Empleado());
        }

        // Acción para procesar la creación
        [HttpPost]
        public async Task<IActionResult> Crear(Empleado empleado)
        {
            int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;
            if (ModelState.IsValid)
            {
                var codigoError = await _databaseService.InsertarEmpleadoAsync(empleado, idUsuario);
                if (codigoError == 0)
                {
                    return RedirectToAction("Index");
                }
                ModelState.AddModelError("", $"Error al insertar empleado: Código {codigoError}");
            }
            ViewBag.Puestos = await _databaseService.ObtenerPuestosAsync(idUsuario);
            return View(empleado);
        }

        // Acción para mostrar el formulario de edición
        public async Task<IActionResult> Editar(int id)
        {
            try
            {
                int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;
                var empleados = await _databaseService.ListarEmpleadosAsync("", idUsuario);
                var empleado = empleados.FirstOrDefault(e => e.Id == id);
                if (empleado == null)
                {
                    return NotFound("Empleado no encontrado.");
                }
                var puestos = await _databaseService.ObtenerPuestosAsync(idUsuario);
                ViewBag.Puestos = puestos ?? new List<Puesto>();

                return View(empleado);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar los datos del empleado. Intente más tarde.");
                return RedirectToAction("Index");
            }
        }

        // Acción para procesar la actualización
        [HttpPost]
        public async Task<IActionResult> Editar(Empleado empleado)
        {
            int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;

            if (ModelState.IsValid)
            {
                try
                {
                    var codigoError = await _databaseService.ActualizarEmpleadoAsync(empleado, idUsuario);
                    if (codigoError == 0)
                    {
                        return RedirectToAction("Index");
                    }
                    ModelState.AddModelError("", $"Error al actualizar el empleado: Código {codigoError}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al actualizar el empleado. Intente más tarde.");
                    Console.Error.WriteLine($"Error en Editar: {ex.Message}");
                }
            }
            ViewBag.Puestos = await _databaseService.ObtenerPuestosAsync(idUsuario);
            return View(empleado);
        }

        // Accion para eliminar un empleado
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;
                var empleados = await _databaseService.ListarEmpleadosAsync("", idUsuario);
                var empleado = empleados.FirstOrDefault(e => e.Id == id);
                if (empleado == null)
                {
                    return NotFound("Empleado no encontrado.");
                }
                return View(empleado);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar los datos del empleado. Intente más tarde.");
                return RedirectToAction("Index");
            }
        }

        // Acción para procesar la eliminación
        [HttpPost]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;

            try
            {
                var empleados = await _databaseService.ListarEmpleadosAsync("", idUsuario);
                var empleado = empleados.FirstOrDefault(e => e.Id == id);
                if (empleado == null)
                {
                    return NotFound("Empleado no encontrado.");
                }

                var codigoError = await _databaseService.EliminarEmpleadoAsync(empleado, idUsuario);
                if (codigoError == 0)
                {
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", $"Error al eliminar el empleado: Código {codigoError}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al eliminar el empleado. Intente más tarde.");
            }
            return RedirectToAction("Eliminar", new { id });
        }
    }
}
