using Microsoft.Data.SqlClient;
using NSIE.Models;
using Dapper;
using System.Data;

namespace NSIE.Servicios
{
    public interface IRepositorioUsuarios
    {

        //Obtiene los usuarios de la plataforma
        Task<IEnumerable<UserViewModel>> ObtenerListadeUsuarios();
        Task<UserViewModel> ObtenerUsuarioPorId(int id);
        Task<IEnumerable<Rol>> ObtenerTodosLosRoles();
        Task<IEnumerable<Mercado>> ObtenerTodosLosMercados();

        //Notificaciones del usuario
        Task<List<Notificacion>> GetNotificationsByUserIdAsync(int userId);
        Task<int> GetUnreadNotificationsCountAsync(int userId);
        Task<bool> MarkNotificationAsReadAsync(int notificationId);
        Task<IEnumerable<Notificacion>> GetAllNotificationsAsync(int userId);
        Task<bool> GenerateNotificationsScriptAsync();
        Task<Notificacion> ObtenerNotificacionPorId(int id);

        //Actualiza los Usuarios
        Task<bool> ActualizarUsuario(UserViewModel usuario);
        Task<bool> ActualizarRolUsuario(RolesUsuarioViewModel rolUsuario);
        Task<int> RegistraUsuario(UserViewModel nuevoUsuario);
        Task<bool> RegistraRolUsuario(RolesUsuarioViewModel rolUsuario);


        ////////////////////////
        Task<UsuarioApp> BuscarUsuarioPorEmail(string emailNomarlizado);
        Task<int> CrearUsuario(UsuarioApp usuario);
        Task<bool> EliminarUsuario(int id);

        ///Créditos 
        ///
        Task<IEnumerable<Credito>> ObtenerCreditos();

        Task<Credito> ObtenerCreditoPorId(int creditoId);

        Task InsertarEncuesta(Encuesta encuesta);
    }




    public class RepositorioUsuarios : IRepositorioUsuarios
    {


        private readonly string connectionString;
        private readonly ILogger<RepositorioUsuarios> _logger;

        public RepositorioUsuarios(IConfiguration configuration, ILogger<RepositorioUsuarios> logger)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
            _logger = logger;
        }

        //Encuesta de Usuarios
        public async Task InsertarEncuesta(Encuesta encuesta)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                var parameters = new DynamicParameters();
                parameters.Add("@Correo", encuesta.Correo);
                parameters.Add("@Nombre", encuesta.Nombre);
                parameters.Add("@EncontroInformacion", encuesta.EncontroInformacion);
                parameters.Add("@FueUtil", encuesta.FueUtil);
                parameters.Add("@InformacionBuscada", encuesta.InformacionBuscada);
                parameters.Add("@CalificacionExperiencia", encuesta.CalificacionExperiencia);
                parameters.Add("@AgregarComentario", encuesta.AgregarComentario);
                parameters.Add("@ComentarioAdicional", encuesta.ComentarioAdicional);

                await conn.ExecuteAsync("InsertarEncuesta", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public async Task<IEnumerable<UserViewModel>> ObtenerListadeUsuarios()
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = "EXEC [dbo].[spObtener_ListaUsuarios] ";
                    var usuarios = await connection.QueryAsync<UserViewModel>(query);
                    return usuarios;
                }
            }
            catch (SqlException ex)
            {
                _logger.LogError($"Error de SQL: {ex.Message}");
                throw;
            }
        }


        public async Task<UserViewModel> ObtenerUsuarioPorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Obtener el usuario basado en el id utilizando el procedimiento almacenado
                var user = await connection.QuerySingleOrDefaultAsync<UserViewModel>(
                    "sp_ObtenerUsuarioSession",
                    new { IdUsuario = id },
                    commandType: CommandType.StoredProcedure
                );

                return user;
            }
        }



        public async Task<IEnumerable<Rol>> ObtenerTodosLosRoles()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Suponiendo que tienes una tabla llamada "Roles" en tu base de datos
                var roles = await connection.QueryAsync<Rol>("SELECT * FROM Roles");

                return roles;
            }
        }



        public async Task<IEnumerable<Mercado>> ObtenerTodosLosMercados()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Suponiendo que tienes una tabla llamada "Roles" en tu base de datos
                var mercados = await connection.QueryAsync<Mercado>("SELECT * FROM Mercados");

                return mercados;
            }
        }


        public async Task<List<Notificacion>> GetNotificationsByUserIdAsync(int userId)
        {
            var notificaciones = new List<Notificacion>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT TOP 4
                        ID,
                        Titulo_Notificacion, 
                        Mensaje, 
                        Fecha_Notificacion, 
                        Link,
                        ID_Usuario
                    FROM Notificaciones 
                    WHERE ID_Usuario = @UserId AND Visto = 0
                    ORDER BY Fecha_Notificacion DESC";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var notificacion = new Notificacion
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            Titulo_Notificacion = reader.GetString(reader.GetOrdinal("Titulo_Notificacion")),
                            Mensaje = reader.GetString(reader.GetOrdinal("Mensaje")),
                            Fecha_Notificacion = reader.GetDateTime(reader.GetOrdinal("Fecha_Notificacion")),
                            Link = reader.GetString(reader.GetOrdinal("Link")),
                            ID_Usuario = reader.GetInt32(reader.GetOrdinal("ID_Usuario"))  // Asegúrate de asignar el ID_Usuario
                        };
                        notificaciones.Add(notificacion);
                    }
                }
            }

            return notificaciones;
        }

        public async Task<int> GetUnreadNotificationsCountAsync(int userId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT COUNT(*) 
                    FROM Notificaciones 
                    WHERE ID_Usuario = @UserId AND Visto = 0";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                return (int)await command.ExecuteScalarAsync();
            }
        }

        public async Task<bool> MarkNotificationAsReadAsync(int notificationId)
        {
            try
            {
                using (var connection = new SqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    var query = @"
                        UPDATE Notificaciones
                        SET Visto = 1, Fecha_Visto = @FechaVisto
                        WHERE ID = @NotificationId";

                    var command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@FechaVisto", DateTime.Now);
                    command.Parameters.AddWithValue("@NotificationId", notificationId);

                    int affectedRows = await command.ExecuteNonQueryAsync();
                    return affectedRows > 0;
                }
            }
            catch (Exception ex)
            {
                // Loguear el error (opcional)
                // _logger.LogError(ex, "Error al actualizar la notificación");
                throw;
            }
        }

        public async Task<IEnumerable<Notificacion>> GetAllNotificationsAsync(int userId)
        {
            var notificaciones = new List<Notificacion>();

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    SELECT 
                        ID, 
                        ID_Notificacion, 
                        Titulo_Notificacion, 
                        Mensaje, 
                        Fecha_Notificacion, 
                        Link, 
                        ID_Usuario, 
                        Visto, 
                        Fecha_Visto,
                         Imagen
                    FROM Notificaciones 
                    WHERE ID_Usuario = @UserId
                    ORDER BY Fecha_Notificacion DESC";

                var command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserId", userId);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var notificacion = new Notificacion
                        {
                            ID = reader.GetInt32(reader.GetOrdinal("ID")),
                            ID_Notificacion = reader.GetGuid(reader.GetOrdinal("ID_Notificacion")),
                            Titulo_Notificacion = reader.GetString(reader.GetOrdinal("Titulo_Notificacion")),
                            Mensaje = reader.GetString(reader.GetOrdinal("Mensaje")),
                            Fecha_Notificacion = reader.GetDateTime(reader.GetOrdinal("Fecha_Notificacion")),
                            Link = reader.GetString(reader.GetOrdinal("Link")),
                            ID_Usuario = reader.GetInt32(reader.GetOrdinal("ID_Usuario")),
                            Visto = reader.GetBoolean(reader.GetOrdinal("Visto")),
                            Fecha_Visto = reader.IsDBNull(reader.GetOrdinal("Fecha_Visto")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("Fecha_Visto")),
                            Imagen = reader.IsDBNull(reader.GetOrdinal("Imagen")) ? null : reader.GetString(reader.GetOrdinal("Imagen")) // Nueva propiedad
                        };
                        notificaciones.Add(notificacion);
                    }
                }
            }

            return notificaciones;
        }

        //Prueba para agregar notificaciones
        public async Task<bool> GenerateNotificationsScriptAsync()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                var script = @"
                DECLARE @ID_Usuario INT;
                DECLARE @Titulo_Notificacion NVARCHAR(255) = 'Sistema de Coordenadas de Georreferenciación';
                DECLARE @Mensaje NVARCHAR(MAX) = '¡Te invitamos a consultar el documento del Sistema de Coordenadas de Georreferenciación! Conoce más sobre el Sistema de Coordenadas Geográficas, sus generalidades, las unidades de medición y cómo verificar que las coordenadas estén en territorio nacional';
                DECLARE @Fecha_Notificacion DATETIME = GETDATE();
                DECLARE @Link NVARCHAR(255) = '/documents/Coordenadas-de-Geo-referenciación-Póster.pdf';
                DECLARE @Visto BIT = 0;
                DECLARE @Fecha_Visto DATETIME = NULL;
                DECLARE @Imagen NVARCHAR(255) = '/img/mapam.png';

                DECLARE UserCursor CURSOR FOR
                SELECT TOP 500 IdUsuario 
                FROM [dbo].[USUARIO]
                WHERE Vigente = 1
                ORDER BY IdUsuario;

                OPEN UserCursor;
                FETCH NEXT FROM UserCursor INTO @ID_Usuario;

                WHILE @@FETCH_STATUS = 0
                BEGIN
                    IF NOT EXISTS (
                        SELECT 1
                        FROM [dbo].[Notificaciones]
                        WHERE [ID_Usuario] = @ID_Usuario
                        AND [Titulo_Notificacion] = @Titulo_Notificacion
                        AND [Mensaje] = @Mensaje
                    )
                    BEGIN
                        INSERT INTO [dbo].[Notificaciones] 
                            ([ID_Notificacion], [Titulo_Notificacion], [Mensaje], [Fecha_Notificacion], [Link], [ID_Usuario], [Visto], [Fecha_Visto], [Imagen])
                        VALUES 
                            (NEWID(), @Titulo_Notificacion, @Mensaje, @Fecha_Notificacion, @Link, @ID_Usuario, @Visto, @Fecha_Visto, @Imagen);
                    END

                    FETCH NEXT FROM UserCursor INTO @ID_Usuario;
                END

                CLOSE UserCursor;
                DEALLOCATE UserCursor;";

                using (var command = new SqlCommand(script, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }

                return true;
            }
        }

        // Muestra el detalle de la Notificación por ID
        public async Task<Notificacion> ObtenerNotificacionPorId(int id)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                SELECT 
                    ID_Notificacion, 
                    Titulo_Notificacion, 
                    Mensaje, 
                    Fecha_Notificacion, 
                    Link, 
                    ID_Usuario, 
                    Visto, 
                    Fecha_Visto 
                FROM Notificaciones 
                WHERE ID_Notificacion = @Id";

                return await connection.QuerySingleOrDefaultAsync<Notificacion>(query, new { Id = id });
            }
        }

        //Actualiza Usuarios

        // Tus otros métodos...

        public async Task<bool> ActualizarUsuario(UserViewModel usuario)
        {
            var sql = "UPDATE USUARIO SET Nombre = @Nombre, Correo = @Correo, RFC=@RFC, Cargo=@Cargo, Unidad_de_Adscripcion=@Unidad_de_Adscripcion, ClaveEmpleado=@ClaveEmpleado, SesionActiva=@SesionActiva, Vigente=@Vigente  WHERE IdUsuario = @IdUsuario;";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(sql, usuario);
                return result > 0;
            }
        }

        public async Task<bool> ActualizarRolUsuario(RolesUsuarioViewModel rolUsuario)
        {
            var sql = "UPDATE Roles_Usuarios SET Rol_ID = @Rol_ID, Mercado_ID = @Mercado_ID, RolUsuario_Comentarios = @RolUsuario_Comentarios WHERE IdUsuario = @IdUsuario;";
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                var result = await connection.ExecuteAsync(sql, rolUsuario);
                return result > 0;
            }
        }

        //Agregar un Nuevo Usuario

        public async Task<int> RegistraUsuario(UserViewModel nuevoUsuario)
        {
            // SQL query para insertar un nuevo usuario y obtener el ID generado.
            var sql = @"INSERT INTO USUARIO (Nombre, Correo, RFC, Cargo, Unidad_de_Adscripcion, ClaveEmpleado, SesionActiva, Vigente, Clave) 
                VALUES (@Nombre, @Correo, @RFC, @Cargo, @Unidad_de_Adscripcion, @ClaveEmpleado, @SesionActiva, @Vigente, @Clave);
                SELECT CAST(SCOPE_IDENTITY() as int);";  // SCOPE_IDENTITY() se utiliza para recuperar el ID recién insertado.

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Ejecutando la query y obteniendo el nuevo ID.
                var newUserId = await connection.QuerySingleAsync<int>(sql, nuevoUsuario);

                return newUserId;
            }
        }


        public async Task<bool> RegistraRolUsuario(RolesUsuarioViewModel rolUsuario)
        {
            // SQL query para insertar la información de roles del usuario.
            var sql = @"INSERT INTO Roles_Usuarios (IdUsuario, Rol_ID, Mercado_ID, RolUsuario_Vigente, RolUsuario_QuienRegistro, RolUsuario_FechaMod, RolUsuario_Comentarios) 
                VALUES (@IdUsuario, @Rol_ID, @Mercado_ID, @RolUsuario_Vigente, @RolUsuario_QuienRegistro, @RolUsuario_FechaMod, @RolUsuario_Comentarios);";

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Ejecutando la query y verificando si la inserción fue exitosa.
                var affectedRows = await connection.ExecuteAsync(sql, rolUsuario);

                return affectedRows > 0;
            }
        }



        //Elimina un usuario
        public async Task<bool> EliminarUsuario(int usuarioId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                // Comenzamos una transacción para asegurarnos de que ambas operaciones de eliminación sean exitosas.
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Eliminar primero el rol del usuario para mantener la integridad referencial.
                        var sqlRolesUsuario = "DELETE FROM [Roles_Usuarios] WHERE IdUsuario = @IdUsuario;";
                        await connection.ExecuteAsync(sqlRolesUsuario, new { IdUsuario = usuarioId }, transaction);

                        // Luego eliminar el usuario.
                        var sqlUsuario = "DELETE FROM [USUARIO] WHERE IdUsuario = @IdUsuario;";
                        await connection.ExecuteAsync(sqlUsuario, new { IdUsuario = usuarioId }, transaction);

                        // Si ambas eliminaciones fueron exitosas, confirmamos la transacción.
                        transaction.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        // Si ocurre algún error, revertimos la transacción.
                        try
                        {
                            transaction.Rollback();
                        }
                        catch (Exception)
                        {
                            // Log o manejo del error al intentar revertir la transacción.
                        }
                        return false;
                    }
                }
            }
        }




        //private readonly string connectionString;
        //public RepositorioUsuarios(IConfiguration configuration)
        //{
        //    connectionString = configuration.GetConnectionString("JAV");
        //}

        public async Task<int> CrearUsuario(UsuarioApp usuario)
        {

            // usuario.EmailNormalizado = usuario.Email.ToUpper();
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
            INSERT INTO [dbo].[UsuariosPrueba]
               ([Usuario]
               ,[Email]
               ,[EmailNormalizado]
               ,[PasswordHash])
            VALUES (@Usuario, @Email, @EmailNormalizado, @PasswordHash)
            ", usuario);
            var Query = connection.Query("SELECT 1").FirstOrDefault();
            return id;


        }
        public async Task<UsuarioApp> BuscarUsuarioPorEmail(string emailNomarlizado)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QuerySingleOrDefaultAsync<UsuarioApp>(
                "SELECT * FROM UsuariosPrueba Where EmailNormalizado=@emailNormalizado",

                new { emailNomarlizado }
                );
        }



        //Créditos
        public async Task<IEnumerable<Credito>> ObtenerCreditos()
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = @"
            SELECT CreditoID, Nombre, Cargo, ImagenUrl, Seccion, PaginaWeb, Actividades, Resena
            FROM Creditos";

                var creditos = await connection.QueryAsync<Credito>(query);
                return creditos;
            }
        }
        //Crédito por ID
        public async Task<Credito> ObtenerCreditoPorId(int creditoId)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var query = "SELECT * FROM Creditos WHERE CreditoID = @CreditoID";
                var credito = await connection.QuerySingleOrDefaultAsync<Credito>(query, new { CreditoID = creditoId });
                return credito;
            }
        }

    }




}

