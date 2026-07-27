using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Controllers;
using NSIE.Models;
using System.Text.Json;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace NSIE.Servicios
{
    public interface IRepositorioSiliMx
    {
        // USUARIO
        Task<SilimxRegistroUsuario> ObtenerRegistroPorUsuario(int idUsuario);
        Task<UsuarioDatosBase> ObtenerDatosBaseUsuario(int idUsuario);
        Task<int> GuardarRegistro(SilimxRegistroRequest request, string usuarioIdGenerado);
        Task ActualizarRegistro(int idRegistro, SilimxRegistroRequest request);

        // PROYECTO
        Task<int> GuardarProyecto(SilimxProyectoRequest request, int idUsuario);
        Task GuardarPermisosProyecto(int idProyecto, List<SilimxPermisoRequest> permisos);
        Task ActualizarIdVisibleProyecto(int idProyecto, string idVisible);
        Task<IEnumerable<SilimxProyecto>> ObtenerProyectosPorUsuario(int idUsuario);
        //Task<PerfilUsuario> ObtenerPerfilUsuario(int idUsuario);  para saber si es Directivo
        Task<IEnumerable<Municipio>> ObtenerMunicipiosPorEstado(int estadoId);
        Task<IEnumerable<Estado>> ObtenerTodos();

        // CATÁLOGO
        // En la interfaz IRepositorioSiliMx:
        Task<IEnumerable<SilimxProyectoCatalogo>> ObtenerCatalogoProyectos(int idUsuario);
        Task<SilimxProyecto> ObtenerProyectoPorId(int idProyecto, int idUsuario);
        Task InhabilitarProyecto(int idProyecto, int idUsuario);

        // ACTUALIZAR
        // Interfaz:
        Task<IEnumerable<SilimxProyectoSelector>> ObtenerSelectorProyectos(int idUsuario);
        Task<SilimxProyectoCompleto> ObtenerProyectoCompleto(int idProyecto, int idUsuario);
        Task<int> DuplicarYActualizarProyecto(SilimxProyectoRequest request, int idProyecto, int idUsuario);

        // BARRENOS
        Task<string> GuardarBarreno(SilimxBarrenoRequest request, int idUsuario, string barrenoIdVisible);
        Task GuardarIntervalosBarreno(int idBarreno, List<SilimxBarrenoIntervaloRequest> intervalos);
        Task<IEnumerable<SilimxBarrenoCatalogo>> ObtenerCatalogoBarrenos(int idUsuario);
        Task InhabilitarBarreno(int idBarreno, int idUsuario);

        // MUESTRAS
        Task<(int IdMuestra, string IdVisible)> GuardarMuestra(
            SilimxMuestraRequest request, int idUsuario, string muestraIdVisible);
        Task<IEnumerable<SilimxMuestraCatalogo>> ObtenerCatalogoMuestras(int idUsuario);
        Task InhabilitarMuestra(int idMuestra, int idUsuario);
        Task<IEnumerable<SilimxBarrenoSelector>> ObtenerSelectorBarrenos(int idUsuario);

    }

    public class RepositorioSiliMx : IRepositorioSiliMx
    {
        private readonly string connectionString;

        // USUARIO

        public RepositorioSiliMx(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<SilimxRegistroUsuario> ObtenerRegistroPorUsuario(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new { IdUsuario = idUsuario };

            var resultado = await connection.QueryFirstOrDefaultAsync<SilimxRegistroUsuario>(
                "SELECT * FROM [dbo].[SILIMX_REGISTRO_USUARIO] WHERE IdUsuario = @IdUsuario",
                parameters);

            return resultado;
        }

        public async Task<UsuarioDatosBase> ObtenerDatosBaseUsuario(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new { IdUsuario = idUsuario };

            var resultado = await connection.QueryFirstOrDefaultAsync<UsuarioDatosBase>(
                @"SELECT IdUsuario, Correo, Nombre, Unidad_de_Adscripcion, Cargo 
                  FROM [dbo].[USUARIO] WHERE IdUsuario = @IdUsuario",
                parameters);

            return resultado;
        }

        public async Task<int> GuardarRegistro(SilimxRegistroRequest request, string usuarioIdGenerado)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new
            {
                request.IdUsuario,
                request.ApellidoPaterno,
                request.ApellidoMaterno,
                request.Nombres,
                request.TipoInstitucion,
                request.Institucion,
                request.Puesto,
                request.CorreoInstitucional,
                request.TelefonoContacto,
                request.PerfilSolicitado,
                UsuarioID = usuarioIdGenerado
            };

            var query = @"
                INSERT INTO [dbo].[SILIMX_REGISTRO_USUARIO]
                    (IdUsuario, ApellidoPaterno, ApellidoMaterno, Nombres, TipoInstitucion,
                     Institucion, Puesto, CorreoInstitucional, TelefonoContacto, PerfilSolicitado,
                     UsuarioID, Estatus, Vigente, FechaRegistro)
                VALUES
                    (@IdUsuario, @ApellidoPaterno, @ApellidoMaterno, @Nombres, @TipoInstitucion,
                     @Institucion, @Puesto, @CorreoInstitucional, @TelefonoContacto, @PerfilSolicitado,
                     @UsuarioID, 'Pendiente', 1, GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var idRegistro = await connection.QuerySingleAsync<int>(query, parameters);

            return idRegistro;
        }

        public async Task ActualizarRegistro(int idRegistro, SilimxRegistroRequest request)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new
            {
                IdRegistro = idRegistro,
                request.ApellidoPaterno,
                request.ApellidoMaterno,
                request.Nombres,
                request.TipoInstitucion,
                request.Institucion,
                request.Puesto,
                request.CorreoInstitucional,
                request.TelefonoContacto,
                request.PerfilSolicitado
            };

            var query = @"
                UPDATE [dbo].[SILIMX_REGISTRO_USUARIO]
                SET ApellidoPaterno = @ApellidoPaterno,
                    ApellidoMaterno = @ApellidoMaterno,
                    Nombres = @Nombres,
                    TipoInstitucion = @TipoInstitucion,
                    Institucion = @Institucion,
                    Puesto = @Puesto,
                    CorreoInstitucional = @CorreoInstitucional,
                    TelefonoContacto = @TelefonoContacto,
                    PerfilSolicitado = @PerfilSolicitado,
                    Estatus = CASE WHEN Estatus = 'Rechazado' THEN 'Pendiente' ELSE Estatus END,
                    FechaActualizacion = GETDATE()
                WHERE IdRegistro = @IdRegistro";

            await connection.ExecuteAsync(query, parameters);
        }

        // PROYECTO
        public async Task<int> GuardarProyecto(SilimxProyectoRequest request, int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
                INSERT INTO [dbo].[SILIMX_PROYECTO]
                    (IdUsuario, TipoProyecto, ProyectoID, NombreProyecto, DescripcionObjetivo,
                    InstitucionEmpresa, TipoInstitucion, Financiamiento, Responsable,
                    FechaInicio, FechaFin, EstadoActual, Avance, EntidadFederativa,
                    Municipio, Localidad, NotasFinales, Evidencias, Estatus, FechaRegistro)
                VALUES
                    (@IdUsuario, @TipoProyecto, @ProyectoID, @NombreProyecto, @DescripcionObjetivo,
                    @InstitucionEmpresa, @TipoInstitucion, @Financiamiento, @Responsable,
                    @FechaInicio, @FechaFin, @EstadoActual, @Avance, @EntidadFederativa,
                    @Municipio, @Localidad, @NotasFinales, @Evidencias, 'Guardado', GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.QuerySingleAsync<int>(query, new
            {
                IdUsuario = idUsuario,
                request.TipoProyecto,
                request.ProyectoID,
                request.NombreProyecto,
                request.DescripcionObjetivo,
                request.InstitucionEmpresa,
                request.TipoInstitucion,
                request.Financiamiento,
                request.Responsable,
                FechaInicio = string.IsNullOrEmpty(request.FechaInicio) ? (DateTime?)null : DateTime.Parse(request.FechaInicio),
                FechaFin = string.IsNullOrEmpty(request.FechaFin) ? (DateTime?)null : DateTime.Parse(request.FechaFin),
                request.EstadoActual,
                request.Avance,
                request.EntidadFederativa,
                request.Municipio,
                request.Localidad,
                request.NotasFinales,
                request.Evidencias
            });
        }

        public async Task GuardarPermisosProyecto(int idProyecto, List<SilimxPermisoRequest> permisos)
        {
            using var connection = new SqlConnection(connectionString);

            foreach (var permiso in permisos)
            {
                var permisosStr = string.Join("|", permiso.PermisosSeleccionados ?? new List<string>());

                await connection.ExecuteAsync(@"
                    INSERT INTO [dbo].[SILIMX_PROYECTO_PERMISO]
                        (IdProyecto, Institucion, PermisosSeleccionados, OtroEspecifique)
                    VALUES
                        (@IdProyecto, @Institucion, @PermisosSeleccionados, @OtroEspecifique)",
                    new
                    {
                        IdProyecto = idProyecto,
                        permiso.Institucion,
                        PermisosSeleccionados = permisosStr,
                        permiso.OtroEspecifique
                    });
            }
        }

        public async Task ActualizarIdVisibleProyecto(int idProyecto, string idVisible)
        {
            using var connection = new SqlConnection(connectionString);

            var parameters = new
            {
                IdProyecto = idProyecto,
                IdVisible = idVisible
            };

            var query = @"
                UPDATE [dbo].[SILIMX_PROYECTO]
                SET IdVisible = @IdVisible
                WHERE IdProyecto = @IdProyecto";

            await connection.ExecuteAsync(query, parameters);
        }

        public async Task<IEnumerable<SilimxProyecto>> ObtenerProyectosPorUsuario(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<SilimxProyecto>(
                "SELECT * FROM [dbo].[SILIMX_PROYECTO] WHERE IdUsuario = @IdUsuario AND Vigente = 1 ORDER BY FechaRegistro DESC",
                new { IdUsuario = idUsuario });
        }

        public async Task<IEnumerable<Municipio>> ObtenerMunicipiosPorEstado(int estadoId)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<Municipio>(
                "SELECT MunicipioID, Municipio_Nombre FROM Municipios WHERE EF_ID = @estadoId",
                new { estadoId }
            );
        }

        public async Task<IEnumerable<Estado>> ObtenerTodos()
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Estado>("SELECT EF_ID, EF_Nombre FROM Entidades_Federativas");
        }

        // Implementación en RepositorioSiliMx:
        public async Task<IEnumerable<SilimxProyectoCatalogo>> ObtenerCatalogoProyectos(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            // Hacemos JOIN con SILIMX_REGISTRO_USUARIO para obtener el UsuarioID (AAMMDDL)
            var query = @"
                SELECT 
                    p.IdProyecto,
                    p.TipoProyecto,
                    p.ProyectoID,
                    r.UsuarioID,
                    p.NombreProyecto,
                    p.EntidadFederativa,
                    p.Municipio,
                    p.Localidad,
                    p.FechaInicio,
                    p.FechaFin,
                    p.FechaRegistro,
                    p.InstitucionEmpresa,
                    p.TipoInstitucion,
                    p.Financiamiento,
                    p.EstadoActual,
                    p.Avance,
                    p.Estatus,
                    P.IdVisible
                FROM [dbo].[SILIMX_PROYECTO] p
                INNER JOIN [dbo].[SILIMX_REGISTRO_USUARIO] r ON p.IdUsuario = r.IdUsuario
                WHERE p.IdUsuario = @IdUsuario
                AND p.Vigente = 1
                ORDER BY p.FechaRegistro DESC";

            return await connection.QueryAsync<SilimxProyectoCatalogo>(query, new { IdUsuario = idUsuario });
        }

        public async Task<SilimxProyecto> ObtenerProyectoPorId(int idProyecto, int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            // El idUsuario en el WHERE evita que un usuario vea proyectos ajenos
            return await connection.QueryFirstOrDefaultAsync<SilimxProyecto>(
                @"SELECT * FROM [dbo].[SILIMX_PROYECTO] 
                WHERE IdProyecto = @IdProyecto AND IdUsuario = @IdUsuario AND Vigente = 1",
                new { IdProyecto = idProyecto, IdUsuario = idUsuario });
        }

        public async Task InhabilitarProyecto(int idProyecto, int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            // Solo inhabilita si pertenece al usuario y está en Completo
            await connection.ExecuteAsync(
                @"UPDATE [dbo].[SILIMX_PROYECTO]
                SET Vigente = 0, FechaActualizacion = GETDATE()
                WHERE IdProyecto = @IdProyecto 
                    AND IdUsuario = @IdUsuario
                    AND Estatus = 'Completo'",
                new { IdProyecto = idProyecto, IdUsuario = idUsuario });
        }

        // Implementación:
        public async Task<IEnumerable<SilimxProyectoSelector>> ObtenerSelectorProyectos(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<SilimxProyectoSelector>(
                @"SELECT IdProyecto, IdVisible, NombreProyecto, Estatus
                FROM [dbo].[SILIMX_PROYECTO]
                WHERE IdUsuario = @IdUsuario AND Vigente = 1
                ORDER BY FechaRegistro DESC",
                new { IdUsuario = idUsuario });
        }

        public async Task<SilimxProyectoCompleto> ObtenerProyectoCompleto(int idProyecto, int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            var proyecto = await connection.QueryFirstOrDefaultAsync<SilimxProyecto>(
                @"SELECT * FROM [dbo].[SILIMX_PROYECTO]
                WHERE IdProyecto = @IdProyecto AND IdUsuario = @IdUsuario AND Vigente = 1",
                new { IdProyecto = idProyecto, IdUsuario = idUsuario });

            if (proyecto == null) return null;

            var permisos = await connection.QueryAsync<SilimxProyectoPermiso>(
                "SELECT * FROM [dbo].[SILIMX_PROYECTO_PERMISO] WHERE IdProyecto = @IdProyecto",
                new { IdProyecto = idProyecto });

            return new SilimxProyectoCompleto
            {
                Proyecto = proyecto,
                Permisos = permisos
            };
        }

        public async Task<int> DuplicarYActualizarProyecto(
            SilimxProyectoRequest request, int idProyectoOriginal, int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            // El ID visible del duplicado lleva prefijo "A." como dice la imagen
            // Lo generamos en el controller y lo pasamos via request.ProyectoID
            var query = @"
                INSERT INTO [dbo].[SILIMX_PROYECTO]
                    (IdUsuario, TipoProyecto, ProyectoID, IdVisible, NombreProyecto, DescripcionObjetivo,
                    InstitucionEmpresa, TipoInstitucion, Financiamiento, Responsable,
                    FechaInicio, FechaFin, EstadoActual, Avance, EntidadFederativa,
                    Municipio, Localidad, NotasFinales, Evidencias,
                    ResumenAvances, Estatus, Vigente, FechaRegistro)
                SELECT
                    IdUsuario, TipoProyecto, ProyectoID, @NuevoIdVisible, NombreProyecto, DescripcionObjetivo,
                    InstitucionEmpresa, TipoInstitucion, Financiamiento, Responsable,
                    FechaInicio, @FechaFin, @EstadoActual, @Avance, @EntidadFederativa,
                    @Municipio, @Localidad, @NotasFinales, @Evidencias,
                    @ResumenAvances, 'Completo', 1, GETDATE()
                FROM [dbo].[SILIMX_PROYECTO]
                WHERE IdProyecto = @IdProyectoOriginal AND IdUsuario = @IdUsuario;
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.QuerySingleAsync<int>(query, new
            {
                IdProyectoOriginal = idProyectoOriginal,
                IdUsuario = idUsuario,
                NuevoIdVisible = request.IdVisible,
                FechaFin = string.IsNullOrEmpty(request.FechaFin) ?
                    (DateTime?)null : DateTime.Parse(request.FechaFin),
                EstadoActual = request.EstadoActual,
                Avance = request.Avance,
                EntidadFederativa = request.EntidadFederativa,
                Municipio = request.Municipio,
                Localidad = request.Localidad,
                NotasFinales = request.NotasFinales,
                Evidencias = request.Evidencias,
                ResumenAvances = request.ResumenAvances,
            });
        }

        // BARRENOS

        public async Task<string> GuardarBarreno(
            SilimxBarrenoRequest request, int idUsuario, string barrenoIdVisible)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
                INSERT INTO [dbo].[SILIMX_BARRENO]
                    (IdUsuario, IdProyecto, ProyectoEEID, BarrenoID, BarrenoIDVisible,
                    Responsable, EmpresaPerforista, ResponsableDescNucleo,
                    Estado, Municipio, Localidad,
                    LatitudN, LongitudO, Altitud, Azimut, Inclinacion, TipoBarrenacion,
                    FechaInicio, FechaFin, LongitudPerforada, LongitudRecuperada,
                    Diametro, RQD, NumeroCajas, NombrePrimeraCaja, Gravimetria,
                    NotasFinales, Evidencias, Estatus, FechaRegistro)
                VALUES
                    (@IdUsuario, @IdProyecto, @ProyectoEEID, @BarrenoID, @BarrenoIDVisible,
                    @Responsable, @EmpresaPerforista, @ResponsableDescNucleo,
                    @Estado, @Municipio, @Localidad,
                    @LatitudN, @LongitudO, @Altitud, @Azimut, @Inclinacion, @TipoBarrenacion,
                    @FechaInicio, @FechaFin, @LongitudPerforada, @LongitudRecuperada,
                    @Diametro, @RQD, @NumeroCajas, @NombrePrimeraCaja, @Gravimetria,
                    @NotasFinales, @Evidencias, 'Pendiente', GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var idBarreno = await connection.QuerySingleAsync<int>(query, new
            {
                IdUsuario = idUsuario,
                request.IdProyecto,
                request.ProyectoEEID,
                request.BarrenoID,
                BarrenoIDVisible = barrenoIdVisible,
                request.Responsable,
                request.EmpresaPerforista,
                request.ResponsableDescNucleo,
                request.Estado,
                request.Municipio,
                request.Localidad,
                request.LatitudN,
                request.LongitudO,
                request.Altitud,
                request.Azimut,
                request.Inclinacion,
                request.TipoBarrenacion,
                FechaInicio = string.IsNullOrEmpty(request.FechaInicio) ?
                    (DateTime?)null : DateTime.Parse(request.FechaInicio),
                FechaFin = string.IsNullOrEmpty(request.FechaFin) ?
                    (DateTime?)null : DateTime.Parse(request.FechaFin),
                request.LongitudPerforada,
                request.LongitudRecuperada,
                request.Diametro,
                request.RQD,
                request.NumeroCajas,
                request.NombrePrimeraCaja,
                request.Gravimetria,
                request.NotasFinales,
                request.Evidencias
            });

            return barrenoIdVisible;
        }

        public async Task GuardarIntervalosBarreno(
            int idBarreno, List<SilimxBarrenoIntervaloRequest> intervalos)
        {
            using var connection = new SqlConnection(connectionString);

            foreach (var intervalo in intervalos)
            {
                await connection.ExecuteAsync(@"
                    INSERT INTO [dbo].[SILIMX_BARRENO_INTERVALO]
                        (IdBarreno, Nombre, DesdeM, HastaM, DeInteres)
                    VALUES
                        (@IdBarreno, @Nombre, @DesdeM, @HastaM, @DeInteres)",
                    new
                    {
                        IdBarreno = idBarreno,
                        intervalo.Nombre,
                        intervalo.DesdeM,
                        intervalo.HastaM,
                        intervalo.DeInteres
                    });
            }
        }

        public async Task<IEnumerable<SilimxBarrenoCatalogo>> ObtenerCatalogoBarrenos(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<SilimxBarrenoCatalogo>(@"
                SELECT
                    b.IdBarreno,
                    b.BarrenoIDVisible,
                    b.ProyectoEEID,
                    p.IdVisible,
                    b.LongitudRecuperada,
                    b.RQD,
                    b.Estado,
                    b.Municipio,
                    b.Localidad,
                    b.FechaFin,
                    b.FechaRegistro,
                    p.InstitucionEmpresa,
                    b.Estatus
                FROM [dbo].[SILIMX_BARRENO] b
                LEFT JOIN [dbo].[SILIMX_PROYECTO] p ON b.IdProyecto = p.IdProyecto
                WHERE b.IdUsuario = @IdUsuario AND b.Vigente = 1
                ORDER BY b.FechaRegistro DESC",
                new { IdUsuario = idUsuario });
        }

        public async Task InhabilitarBarreno(int idBarreno, int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            await connection.ExecuteAsync(@"
                UPDATE [dbo].[SILIMX_BARRENO]
                SET Vigente = 0, FechaActualizacion = GETDATE()
                WHERE IdBarreno = @IdBarreno
                AND IdUsuario = @IdUsuario
                AND Estatus = 'Completo'",
                new { IdBarreno = idBarreno, IdUsuario = idUsuario });
        }

        // MUESTRAS

        public async Task<(int IdMuestra, string IdVisible)> GuardarMuestra(
            SilimxMuestraRequest request, int idUsuario, string muestraIdVisible)
        {
            using var connection = new SqlConnection(connectionString);

            var query = @"
                INSERT INTO [dbo].[SILIMX_MUESTRA]
                    (IdUsuario, IdProyecto, ProyectoEEID, MuestraID, MuestraIDVisible,
                    TamanoMuestra, ResponsableMuestreo, FechaMuestreo,
                    Estado, Municipio, Localidad,
                    Fuente, TipoCampo,
                    SP_Campo, SP_Pozo, SP_LatitudN, SP_LongitudO, SP_Altitud,
                    SP_Profundidad, SP_IntervaloInicio, SP_IntervaloFin,
                    SP_CorteAgua, SP_Presion, SP_Temperatura, SP_pH, SP_OxigenoDisuelto,
                    SG_Campo, SG_PuntoMuestra, SG_LatitudN, SG_LongitudO, SG_Altitud,
                    SG_Profundidad, SG_Temperatura, SG_pH, SG_OxigenoDisuelto,
                    AS_LatitudN, AS_LongitudO, AS_Altitud,
                    AP_BarrenoID, AP_IntervaloID, AP_DesdeM, AP_HastaM, AP_Estructuras,
                    AR_Litologia, AR_Color, AR_Textura, AR_NotasDescripcion,
                    NotasFinales, Evidencias, Estatus, FechaRegistro)
                VALUES
                    (@IdUsuario, @IdProyecto, @ProyectoEEID, @MuestraID, @MuestraIDVisible,
                    @TamanoMuestra, @ResponsableMuestreo, @FechaMuestreo,
                    @Estado, @Municipio, @Localidad,
                    @Fuente, @TipoCampo,
                    @SP_Campo, @SP_Pozo, @SP_LatitudN, @SP_LongitudO, @SP_Altitud,
                    @SP_Profundidad, @SP_IntervaloInicio, @SP_IntervaloFin,
                    @SP_CorteAgua, @SP_Presion, @SP_Temperatura, @SP_pH, @SP_OxigenoDisuelto,
                    @SG_Campo, @SG_PuntoMuestra, @SG_LatitudN, @SG_LongitudO, @SG_Altitud,
                    @SG_Profundidad, @SG_Temperatura, @SG_pH, @SG_OxigenoDisuelto,
                    @AS_LatitudN, @AS_LongitudO, @AS_Altitud,
                    @AP_BarrenoID, @AP_IntervaloID, @AP_DesdeM, @AP_HastaM, @AP_Estructuras,
                    @AR_Litologia, @AR_Color, @AR_Textura, @AR_NotasDescripcion,
                    @NotasFinales, @Evidencias, 'Pendiente', GETDATE());
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var idMuestra = await connection.QuerySingleAsync<int>(query, new
            {
                IdUsuario = idUsuario,
                request.IdProyecto,
                request.ProyectoEEID,
                request.MuestraID,
                MuestraIDVisible = muestraIdVisible,
                request.TamanoMuestra,
                request.ResponsableMuestreo,
                FechaMuestreo = string.IsNullOrEmpty(request.FechaMuestreo) ?
                    (DateTime?)null : DateTime.Parse(request.FechaMuestreo),
                request.Estado, request.Municipio, request.Localidad,
                request.Fuente, request.TipoCampo,
                request.SP_Campo, request.SP_Pozo,
                request.SP_LatitudN, request.SP_LongitudO, request.SP_Altitud,
                request.SP_Profundidad, request.SP_IntervaloInicio, request.SP_IntervaloFin,
                request.SP_CorteAgua, request.SP_Presion, request.SP_Temperatura,
                request.SP_pH, request.SP_OxigenoDisuelto,
                request.SG_Campo, request.SG_PuntoMuestra,
                request.SG_LatitudN, request.SG_LongitudO, request.SG_Altitud,
                request.SG_Profundidad, request.SG_Temperatura,
                request.SG_pH, request.SG_OxigenoDisuelto,
                request.AS_LatitudN, request.AS_LongitudO, request.AS_Altitud,
                request.AP_BarrenoID, request.AP_IntervaloID,
                request.AP_DesdeM, request.AP_HastaM, request.AP_Estructuras,
                request.AR_Litologia, request.AR_Color,
                request.AR_Textura, request.AR_NotasDescripcion,
                request.NotasFinales, request.Evidencias
            });

            return (idMuestra, muestraIdVisible);
        }

        public async Task<IEnumerable<SilimxMuestraCatalogo>> ObtenerCatalogoMuestras(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            return await connection.QueryAsync<SilimxMuestraCatalogo>(@"
                SELECT
                    m.IdMuestra,
                    m.MuestraIDVisible,
                    m.ProyectoEEID,
                    p.IdVisible,
                    m.Estado,
                    m.Municipio,
                    m.Localidad,
                    m.FechaMuestreo,
                    m.FechaRegistro,
                    p.InstitucionEmpresa,
                    m.Estatus
                FROM [dbo].[SILIMX_MUESTRA] m
                LEFT JOIN [dbo].[SILIMX_PROYECTO] p ON m.IdProyecto = p.IdProyecto
                WHERE m.IdUsuario = @IdUsuario AND m.Vigente = 1
                ORDER BY m.FechaRegistro DESC",
                new { IdUsuario = idUsuario });
        }

        public async Task InhabilitarMuestra(int idMuestra, int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"
                UPDATE [dbo].[SILIMX_MUESTRA]
                SET Vigente = 0, FechaActualizacion = GETDATE()
                WHERE IdMuestra = @IdMuestra AND IdUsuario = @IdUsuario AND Estatus = 'Completo'",
                new { IdMuestra = idMuestra, IdUsuario = idUsuario });
        }

        public async Task<IEnumerable<SilimxBarrenoSelector>> ObtenerSelectorBarrenos(int idUsuario)
        {
            using var connection = new SqlConnection(connectionString);

            // Barrenos del usuario con sus intervalos como JSON
            return await connection.QueryAsync<SilimxBarrenoSelector>(@"
                SELECT
                    b.IdBarreno,
                    b.BarrenoIDVisible,
                    (SELECT i.Nombre, i.DesdeM, i.HastaM
                    FROM [dbo].[SILIMX_BARRENO_INTERVALO] i
                    WHERE i.IdBarreno = b.IdBarreno
                    FOR JSON PATH) AS IntervalosJson
                FROM [dbo].[SILIMX_BARRENO] b
                WHERE b.IdUsuario = @IdUsuario AND b.Vigente = 1
                ORDER BY b.FechaRegistro DESC",
                new { IdUsuario = idUsuario });
        }

    }

    public class UsuarioDatosBase
    {
        public int IdUsuario { get; set; }
        public string Correo { get; set; }
        public string Nombre { get; set; }
        public string Unidad_de_Adscripcion { get; set; }
        public string Cargo { get; set; }
    }
}