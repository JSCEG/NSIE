# Ejemplos de Uso - MCP Excel Server

## Descripción
El servidor MCP Excel de SNIER permite generar reportes automatizados, crear gráficos avanzados y exportar datos en formatos Excel profesionales.

## Funciones Disponibles

### 1. Generar Reporte Básico
```javascript
// Función: generate_report
// Crea reportes Excel con datos y formato
{
  "data": [datos_array],
  "template": "reporte_basico",
  "filename": "consumo_enero_2024.xlsx"
}
```

### 2. Crear Dashboard Excel
```javascript
// Función: create_dashboard
// Genera dashboard interactivo en Excel
{
  "metrics": [metricas_array],
  "charts": ["bar", "line", "pie"],
  "filename": "dashboard_energetico.xlsx"
}
```

### 3. Exportar Análisis Comparativo
```javascript
// Función: export_comparison
// Exporta comparaciones entre períodos/regiones
{
  "dataset1": [datos_periodo1],
  "dataset2": [datos_periodo2],
  "comparison_type": "temporal",
  "filename": "comparativo_Q3_Q4.xlsx"
}
```

## Templates Disponibles

### Template: Reporte Mensual de Consumo
**Ubicación**: `Templates/reporte_mensual.xlsx`
**Campos**:
- Región
- Tipo de Energía
- Consumo (MWh)
- Eficiencia (%)
- Proyección

### Template: Dashboard Ejecutivo
**Ubicación**: `Templates/dashboard_ejecutivo.xlsx`
**Hojas**:
- Resumen Nacional
- Análisis Regional
- Tendencias
- Proyecciones

## Casos de Uso Prácticos

### Caso 1: Reporte Mensual Automatizado
**Objetivo**: Generar reporte mensual para la dirección
```javascript
// 1. Obtener datos del mes
const datosConsumo = await database.query(`
  SELECT region, tipo_energia, SUM(consumo) as total_consumo
  FROM consumo_energetico 
  WHERE fecha BETWEEN @inicio AND @fin
  GROUP BY region, tipo_energia
`);

// 2. Generar reporte Excel
await excel.generate_report({
  data: datosConsumo,
  template: "reporte_mensual",
  filename: `reporte_${new Date().getMonth() + 1}_2024.xlsx`
});
```

### Caso 2: Dashboard Interactivo
**Objetivo**: Dashboard para análisis ejecutivo
```javascript
// Métricas clave
const metricas = [
  { nombre: "Consumo Nacional", valor: 12500, unidad: "MWh" },
  { nombre: "Eficiencia Promedio", valor: 87.5, unidad: "%" },
  { nombre: "Proyectos Activos", valor: 145, unidad: "unidades" },
  { nombre: "Ahorro Energético", valor: 8.2, unidad: "%" }
];

// Crear dashboard
await excel.create_dashboard({
  metrics: metricas,
  charts: ["bar", "line", "gauge", "map"],
  filename: "dashboard_ejecutivo_2024.xlsx"
});
```

### Caso 3: Análisis Comparativo Regional
**Objetivo**: Comparar rendimiento entre regiones
```javascript
// Datos región Norte
const datosNorte = await analytics.analyze_energy_consumption({
  region: "Norte",
  periodo: "2024-Q4",
  tipo_energia: "total"
});

// Datos región Sur
const datosSur = await analytics.analyze_energy_consumption({
  region: "Sur",
  periodo: "2024-Q4",
  tipo_energia: "total"
});

// Exportar comparación
await excel.export_comparison({
  dataset1: datosNorte,
  dataset2: datosSur,
  comparison_type: "regional",
  filename: "comparativo_norte_sur_Q4.xlsx"
});
```

## Integración con Controladores

### En FacturasController.cs
```csharp
public async Task<IActionResult> ExportarFacturacion(DateTime inicio, DateTime fin)
{
    // Obtener datos de facturación
    var datos = await _context.Facturas
        .Where(f => f.Fecha >= inicio && f.Fecha <= fin)
        .Select(f => new {
            f.Region,
            f.TipoEnergia,
            f.Monto,
            f.Consumo,
            f.Fecha
        }).ToListAsync();

    // Generar Excel
    var fileName = await _mcpService.CallExcel("generate_report", new {
        data = datos,
        template = "reporte_facturacion",
        filename = $"facturacion_{inicio:yyyyMM}_{fin:yyyyMM}.xlsx"
    });

    // Retornar archivo para descarga
    var filePath = Path.Combine("Reportes", fileName);
    return File(System.IO.File.ReadAllBytes(filePath), 
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", 
                fileName);
}
```

### En IndicadoresController.cs
```csharp
public async Task<IActionResult> DashboardMensual()
{
    // Obtener métricas del mes
    var metricas = await _mcpService.CallAnalytics("get_monthly_metrics", new {
        mes = DateTime.Now.Month,
        año = DateTime.Now.Year
    });

    // Crear dashboard Excel
    await _mcpService.CallExcel("create_dashboard", new {
        metrics = metricas,
        charts = new[] { "bar", "line", "pie", "gauge" },
        filename = $"dashboard_{DateTime.Now:yyyyMM}.xlsx"
    });

    return Json(new { success = true, message = "Dashboard generado exitosamente" });
}
```

## Scripts de Automatización

### Script PowerShell para Reportes Automáticos
```powershell
# Generar reportes automáticos diarios
$fecha = Get-Date -Format "yyyyMMdd"
$reportes = @("consumo", "eficiencia", "proyecciones")

foreach ($reporte in $reportes) {
    Invoke-RestMethod -Uri "https://snier.gob.mx/api/reportes/$reporte" -Method POST -Body @{
        formato = "excel"
        fecha = $fecha
        automatico = $true
    }
}
```

## Configuración Avanzada

### Personalización de Templates
1. Crear template base en `Templates/`
2. Definir placeholders: `{{REGION}}`, `{{FECHA}}`, `{{CONSUMO}}`
3. Configurar estilos y gráficos
4. Registrar template en el servidor MCP

### Integración con Base de Datos
```javascript
// Configurar conexión directa desde MCP Excel
const dbConfig = {
  server: "sql-server-snier",
  database: "SNIER_DB",
  options: {
    trustedConnection: true
  }
};
```
