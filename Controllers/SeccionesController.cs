using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using NSIE.Models;
using NSIE.Servicios;
using System.Security.Cryptography;
using System.Text;
using System.Data;
using System.Configuration;

[ServiceFilter(typeof(ValidacionInputFiltro))]
[AutorizacionFiltro]
public class SeccionesController : Controller
{
    private readonly IRepositorioSecciones _repositorio;

    public SeccionesController(IRepositorioSecciones repositorio)
    {
        _repositorio = repositorio;
    }

    public async Task<IActionResult> Index()
    {
        var secciones = await _repositorio.ObtenerTodasLasSeccionesAsync();
        return View(secciones);
    }



    public async Task<IActionResult> EditarSeccion(int id)
    {
        var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
        if (seccion == null) return NotFound();
        return View("EditarSeccion", seccion);
    }

    [HttpPost]
    public async Task<IActionResult> EditarSeccion(SeccionConModulos seccion)
    {
        if (!ModelState.IsValid)
            return View("EditarSeccion", seccion);

        await _repositorio.ActualizarSeccionAsync(seccion);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> EditarModulos(int id)
    {
        var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
        if (seccion == null)
            return NotFound();

        return View("EditarModulos", seccion);
    }

    public IActionResult Crear()
    {
        return View(new SeccionConModulos());
    }

    [HttpPost]
    public async Task<IActionResult> Crear(SeccionConModulos seccion)
    {
        if (!ModelState.IsValid)
            return View(seccion);

        await _repositorio.CrearSeccionAsync(seccion);
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarSeccion(int id)
    {
        var seccion = await _repositorio.ObtenerSeccionPorIdAsync(id);
        if (seccion == null)
            return NotFound("La sección no existe.");

        await _repositorio.EliminarSeccionAsync(id);

        return Json(new { mensaje = "La sección se eliminó correctamente." });
    }


    // ✅ NUEVO: Renderizar vista parcial del modal
    public async Task<IActionResult> ModalEditarModulo(int id)
    {
        var modulo = await _repositorio.ObtenerModuloPorIdAsync(id);
        if (modulo == null)
            return NotFound();

        return PartialView("_EditarModuloPartial", modulo);
    }

    // ✅ NUEVO: Guardar edición del módulo desde el modal
    [HttpPost]
    public async Task<IActionResult> GuardarEdicionModulo(Modulo modulo)
    {
        if (!ModelState.IsValid)
            return BadRequest("Datos incompletos para actualizar el módulo.");

        await _repositorio.ActualizarModuloAsync(modulo);
        return Ok(new { mensaje = "Módulo actualizado correctamente." });
    }

    [HttpPost]
    public async Task<IActionResult> AgregarModulo(Modulo modulo)
    {
        if (!ModelState.IsValid)
            return BadRequest("Datos incompletos para el módulo.");

        await _repositorio.AgregarModuloAsync(modulo);
        return RedirectToAction("Editar", new { id = modulo.SeccionId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarModulo(int id, int seccionId)
    {
        await _repositorio.EliminarModuloAsync(id);
        return Json(new { mensaje = "El módulo se eliminó correctamente." }); // ✅ funciona con AJAX
    }


    // ✅ NUEVO: Crear un nuevo módulo desde el modaldentro de una sección existente
    public IActionResult CrearModulo(int seccionId)
    {
        var nuevoModulo = new Modulo
        {
            SeccionId = seccionId,
            Activo = true, // por defecto
            Orden = 1      // valor sugerido
        };
        return PartialView("_CrearModuloPartial", nuevoModulo);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarNuevoModulo(Modulo modulo)
    {
        Console.WriteLine(">>> Entró al método GuardarNuevoModulo");

        if (!ModelState.IsValid)
        {
            Console.WriteLine(">>> ModelState inválido");
            return BadRequest("Faltan datos para guardar el módulo.");
        }

        await _repositorio.AgregarModuloAsync(modulo);
        Console.WriteLine(">>> Insert ejecutado");

        return Ok(new { mensaje = "Módulo creado correctamente." });
    }

    //Crea y Guarda Nuevas Secciones desde el Index
    public IActionResult CrearSeccion()
    {
        var nueva = new SeccionSNIER { SeccionActiva = true };
        return PartialView("_CrearSeccionPartial", nueva);
    }

    [HttpPost]
    public async Task<IActionResult> GuardarNuevaSeccion(SeccionConModulos seccion)
    {
        if (!ModelState.IsValid)
            return BadRequest("Faltan datos para guardar la sección.");

        await _repositorio.CrearSeccionAsync(seccion);
        return Ok(new { mensaje = "Sección creada correctamente." });
    }


}
