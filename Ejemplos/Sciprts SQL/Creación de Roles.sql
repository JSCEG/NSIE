CREATE TABLE [dbo].[Roles_SNIEr] (
    Rol_ID INT PRIMARY KEY,
    Rol_Clave VARCHAR(50) NOT NULL,                  -- Identificador �nico corto (ej. SENER, TICs)
    Rol_Nombre NVARCHAR(100) NOT NULL,               -- Nombre completo del rol
    Rol_Comentario NVARCHAR(500) NULL,               -- Descripci�n breve
    Rol_Tipo VARCHAR(100) NULL,                      -- P�blico, Gobierno, Academia, Privado
    Rol_Grupo VARCHAR(100) NULL,                     -- Consejo, T�cnico, Normativo, etc.
    Rol_NivelAcceso INT NULL,                        -- Nivel de jerarqu�a (1 = m�s alto)
    Rol_Ambito VARCHAR(50) NULL,                     -- Nacional, Estatal, Municipal
    Rol_Vigente BIT NOT NULL DEFAULT 1,              -- Activo o no
    Rol_FechaMod DATETIME DEFAULT GETDATE()          -- Fecha de creaci�n o actualizaci�n
);
