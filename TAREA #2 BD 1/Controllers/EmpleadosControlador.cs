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
            return RedirectToAction("Eliminar");
        }

        // Accion para consultar un empleado
        public async Task<IActionResult> Consulta(int id)
        {
            try
            {
                Console.WriteLine($"ID del empleado a consultar: {id}");
                int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;
                var empleado = await _databaseService.ConsultarEmpleadoAsync(id, idUsuario);
                Console.WriteLine($"Empleado consultado: {empleado}");

                if (empleado == null)
                {
                    TempData["Error"] = "El empleado no fue encontrado o está inactivo.";
                    return RedirectToAction("Index");
                }

                return View(empleado);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar los datos del empleado. Intente más tarde.");
                return RedirectToAction("Index");
            }
        }

        // Acción para listar movimientos de un empleado
        public async Task<IActionResult> Movimientos(string valorDocumentoIdentidad)
        {
            try
            {
                int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;

                // Llamar al servicio para obtener los movimientos
                var detalleMovimientos = await _databaseService.ListarMovimientosPorEmpleadoAsync(valorDocumentoIdentidad, idUsuario);

                if (detalleMovimientos == null || detalleMovimientos.Movimientos.Count == 0)
                {
                    TempData["Mensaje"] = "No se encontraron movimientos para el empleado.";
                }

                return View("ListarMovimientos", detalleMovimientos);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar los movimientos. Intente más tarde.");
                Console.Error.WriteLine($"Error en Movimientos: {ex.Message}");
                return RedirectToAction("Index");
            }
        }

        // Acción para mostrar el formulario de inserción de movimiento
        public async Task<IActionResult> InsertarMovimiento(string valorDocumentoIdentidad)
        {
            try
            {
                int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;

                // Obtener los tipos de movimiento desde la base de datos
                var tiposMovimiento = await _databaseService.ObtenerTiposMovimientoAsync(idUsuario);

                ViewBag.TiposMovimiento = tiposMovimiento ?? new List<TipoMovimiento>();
                ViewBag.ValorDocumentoIdentidad = valorDocumentoIdentidad;

                return View(new Movimiento());
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error al cargar el formulario de inserción de movimiento. Intente más tarde.");
                Console.Error.WriteLine($"Error en InsertarMovimiento: {ex.Message}");
                return RedirectToAction("Movimientos", new { valorDocumentoIdentidad });
            }
        }

        // Acción para procesar la inserción de movimiento
        [HttpPost]
        public async Task<IActionResult> InsertarMovimiento(Movimiento movimiento, string valorDocumentoIdentidad)
        {
            int idUsuario = HttpContext.Session.GetInt32("idUsuario") ?? 0;
            Console.WriteLine($"Insertando movimiento: {movimiento.Monto} para el empleado {valorDocumentoIdentidad}");
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("Error en ModelState: " + error.ErrorMessage);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var codigoError = await _databaseService.InsertarMovimientoAsync(
                        valorDocumentoIdentidad,
                        movimiento.IdTipoMovimiento, // Usar el ID del tipo de movimiento
                        movimiento.Monto,
                        idUsuario
                    );

                    if (codigoError == 0)
                    {
                        TempData["Mensaje"] = "Movimiento insertado correctamente.";
                        return RedirectToAction("Movimientos", new { valorDocumentoIdentidad });
                    }

                    // Manejar errores específicos devueltos por el procedimiento almacenado
                    switch (codigoError)
                    {
                        case 50004:
                            ModelState.AddModelError("", "El empleado no existe.");
                            break;
                        case 50008:
                            ModelState.AddModelError("", "El tipo de movimiento no es válido.");
                            break;
                        case 50011:
                            ModelState.AddModelError("", "El monto ingresado es inválido o el saldo resultante sería negativo.");
                            break;
                        default:
                            ModelState.AddModelError("", $"Error desconocido al insertar el movimiento. Código: {codigoError}");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al insertar el movimiento. Intente más tarde.");
                    Console.Error.WriteLine($"Error en InsertarMovimiento: {ex.Message}");
                }
            }

            // Recargar los datos necesarios para la vista en caso de error
            ViewBag.TiposMovimiento = await _databaseService.ObtenerTiposMovimientoAsync(idUsuario);
            ViewBag.ValorDocumentoIdentidad = valorDocumentoIdentidad;

            return View(movimiento);
        }
    }
}
