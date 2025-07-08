# 🚀 Consideraciones para Publish con MCP Servers - SNIER

## ⚠️ **IMPORTANTE: Qué Cambia en el Publish**

### **🔍 ANÁLISIS DE TU CONFIGURACIÓN ACTUAL**

Tu `mcp.json` tiene estas dependencias:
```json
"sql-analytics": {
    "command": "npx",
    "args": ["@modelcontextprotocol/server-sqlite"]
},
"excel-reports": {
    "command": "npx", 
    "args": ["@modelcontextprotocol/server-excel"]
}
```

## 📋 **LO QUE SÍ NECESITAS PARA PUBLISH**

### **1. Dependencias Node.js en Servidor de Producción**
```bash
# En tu servidor de producción, instalar:
npm install -g @modelcontextprotocol/server-sqlite
npm install -g @modelcontextprotocol/server-excel

# O localmente en el proyecto:
npm install @modelcontextprotocol/server-sqlite @modelcontextprotocol/server-excel
```

### **2. Configuración del Servidor Web**
Tu servidor necesita:
- **Node.js instalado** (para ejecutar los servidores MCP)
- **Permisos de escritura** en `c:\Proyectos\SNIER\Reportes`
- **Conectividad** a tu base de datos SQL Server

### **3. Variables de Entorno en Producción**
```bash
# En tu servidor de producción:
SNIER_DB_SERVER=servidorsqljavidev.database.windows.net
SNIER_DB_NAME=BDPruebasSNIER
EXCEL_OUTPUT_DIR=C:\inetpub\wwwroot\SNIER\Reportes
EXCEL_TEMPLATE_DIR=C:\inetpub\wwwroot\SNIER\Templates
```

### **4. Actualizar Rutas para Producción**
```json
{
    "mcpServers": {
        "excel-reports": {
            "command": "npx",
            "args": ["@modelcontextprotocol/server-excel"],
            "env": {
                "EXCEL_OUTPUT_DIR": "C:\\inetpub\\wwwroot\\SNIER\\Reportes",
                "EXCEL_TEMPLATE_DIR": "C:\\inetpub\\wwwroot\\SNIER\\Templates"
            }
        }
    }
}
```

## 🎯 **OPCIÓN RECOMENDADA: Configuración Condicional**

### **Crear `mcp.production.json`**
```json
{
    "mcpServers": {
        "filesystem": {
            "command": "npx",
            "args": [
                "@modelcontextprotocol/server-filesystem",
                "C:\\inetpub\\wwwroot\\SNIER"
            ],
            "env": {
                "MCP_FILESYSTEM_ROOT": "C:\\inetpub\\wwwroot\\SNIER"
            }
        },
        "sql-snier": {
            "command": "node",
            "args": ["mcp-sql-production.js"],
            "env": {
                "MCP_SQL_DATABASE": "BDPruebasSNIER"
            }
        },
        "excel-reports": {
            "command": "npx",
            "args": ["@modelcontextprotocol/server-excel"],
            "env": {
                "EXCEL_OUTPUT_DIR": "C:\\inetpub\\wwwroot\\SNIER\\Reportes",
                "EXCEL_TEMPLATE_DIR": "C:\\inetpub\\wwwroot\\SNIER\\Templates"
            }
        }
    }
}
```

### **Modificar tu `McpService.cs` para usar configuración dinámica**
```csharp
public class McpService : IMcpService
{
    private readonly string _analyticsUrl;
    private readonly string _excelUrl;

    public McpService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Usar URLs diferentes según el entorno
        if (environment.IsProduction())
        {
            _analyticsUrl = configuration["MCP:Analytics:ProductionUrl"] ?? "http://localhost:3001";
            _excelUrl = configuration["MCP:Excel:ProductionUrl"] ?? "http://localhost:3002";
        }
        else
        {
            _analyticsUrl = configuration["MCP:Analytics:Url"] ?? "http://localhost:3001";
            _excelUrl = configuration["MCP:Excel:Url"] ?? "http://localhost:3002";
        }
    }
}
```

## 🔧 **LO QUE NO CAMBIA EN TU APLICACIÓN .NET**

### **✅ Tu código C# sigue igual:**
- Los controladores (`AnalyticsController.cs`)
- Los servicios (`McpService.cs`) 
- Las vistas (`.cshtml`)
- La lógica de negocio

### **✅ Tu publish normal funciona:**
```bash
dotnet publish -c Release
```

## 🚨 **ALTERNATIVA SIN DEPENDENCIAS EXTERNAS**

### **Opción 1: Analytics Integrado (Sin MCP)**
```csharp
// En lugar de usar MCP, consultar directamente tu BD
public async Task<object> GetAnalyticsData()
{
    using var connection = new SqlConnection(connectionString);
    var result = await connection.QueryAsync(@"
        SELECT 
            Sector,
            SUM(Consumo_TWh) as ConsumoTotal
        FROM ConsumoEnergetico 
        WHERE YEAR(Fecha) = @año
        GROUP BY Sector
    ", new { año = DateTime.Now.Year });
    
    return result;
}
```

### **Opción 2: Excel sin MCP**
```csharp
// Usar EPPlus o ClosedXML directamente
public async Task<byte[]> GenerarReporteExcel(object datos)
{
    using var package = new ExcelPackage();
    var worksheet = package.Workbook.Worksheets.Add("Reporte SNIER");
    
    // Agregar datos...
    worksheet.Cells["A1"].Value = "Consumo Energético";
    
    return package.GetAsByteArray();
}
```

## 📝 **RECOMENDACIÓN PARA SNIER**

### **FASE 1: Publicar Sin MCP (Más Seguro)**
1. Usar analytics integrado con tu BD actual
2. Excel con librerías .NET (EPPlus/ClosedXML)
3. **Ventaja**: Sin dependencias externas

### **FASE 2: Agregar MCP Después (Opcional)**
1. Una vez estable, agregar servidores MCP
2. Configurar en servidor de producción
3. **Ventaja**: Más funcionalidades avanzadas

## 🎯 **DECISIÓN RECOMENDADA**

Para **minimizar riesgos en producción**, te sugiero:

1. **Mantener tu publish actual** sin cambios
2. **Usar los ejemplos de Analytics integrado** (sin MCP)
3. **Implementar Excel con EPPlus** (sin dependencias)
4. **Agregar MCP más adelante** si necesitas funcionalidades avanzadas

### **¿Prefieres que te ayude con la implementación sin MCP o con la configuración completa para producción?**

La opción sin MCP es más sencilla para el publish y igualmente poderosa para SNIER.
