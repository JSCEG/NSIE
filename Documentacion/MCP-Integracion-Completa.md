# Guía de Integración MCP - SNIER

## Configuración Inicial

### 1. Configurar appsettings.json
```json
{
  "MCP": {
    "Analytics": {
      "Url": "http://localhost:3001",
      "Timeout": 30000,
      "RetryAttempts": 3
    },
    "Excel": {
      "Url": "http://localhost:3002", 
      "Timeout": 60000,
      "RetryAttempts": 2
    },
    "OutputPath": "C:\\Proyectos\\SNIER\\Reportes",
    "TemplatePath": "C:\\Proyectos\\SNIER\\Templates"
  }
}
```

### 2. Registrar Servicios en Program.cs
```csharp
// Agregar al Program.cs
builder.Services.AddHttpClient<McpService>();
builder.Services.AddScoped<IMcpService, McpService>();
```

## Ejemplos de Integración en Controladores

### IndicadoresController.cs - Métricas Dashboard
```csharp
using NSIE.Servicios;
using NSIE.Servicios.Extensions;

public class IndicadoresController : Controller
{
    private readonly IMcpService _mcpService;

    public IndicadoresController(IMcpService mcpService)
    {
        _mcpService = mcpService;
    }

    [HttpGet]
    public async Task<IActionResult> DashboardMetrics()
    {
        try
        {
            // Obtener métricas usando MCP Analytics
            var metricas = await _mcpService.GetDashboardMetrics("Nacional");
            
            var viewModel = new DashboardViewModel
            {
                ConsumoNacional = metricas.ConsumoTotal,
                EficienciaPromedio = metricas.EficienciaPromedio,
                ProyectosActivos = metricas.ProyectosActivos,
                AhorroEnergetico = metricas.AhorroEnergetico,
                DatosRegionales = metricas.DatosRegionales
            };

            return Json(viewModel);
        }
        catch (Exception ex)
        {
            return Json(new { error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> GenerarReporteEjecutivo()
    {
        try
        {
            // 1. Obtener datos analytics
            var metricas = await _mcpService.GetDashboardMetrics();
            
            // 2. Generar Excel dashboard
            var resultado = await _mcpService.GenerateDashboard(metricas);
            
            // 3. Retornar archivo para descarga
            var filePath = Path.Combine("Reportes", resultado.FileName);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            
            return File(fileBytes, 
                       "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                       resultado.FileName);
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }
}
```

### ProyectosController.cs - Análisis de Proyectos
```csharp
[HttpGet]
public async Task<IActionResult> AnalisisProyectos(string region = "Nacional")
{
    try
    {
        // Análisis de consumo energético por proyectos
        var analisisConsumo = await _mcpService.CallAnalytics<object>("analyze_energy_consumption", new {
            region = region,
            periodo = DateTime.Now.ToString("yyyy-MM"),
            tipo_energia = "renovable",
            grupo_por = "proyecto"
        });

        // Calcular eficiencia de proyectos
        var eficienciaProyectos = await _mcpService.CallAnalytics<object>("calculate_efficiency_metrics", new {
            unidad_id = "ALL",
            periodo = DateTime.Now.ToString("yyyy-MM"),
            tipo_metrica = "por_proyecto",
            region = region
        });

        ViewBag.AnalisisConsumo = analisisConsumo;
        ViewBag.EficienciaProyectos = eficienciaProyectos;

        return View();
    }
    catch (Exception ex)
    {
        ViewBag.Error = ex.Message;
        return View();
    }
}

[HttpPost]
public async Task<IActionResult> ExportarComparativoRegional(string[] regiones)
{
    try
    {
        var resultado = await _mcpService.ExportRegionalComparison(regiones);
        
        var filePath = Path.Combine("Reportes", resultado.FileName);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        
        return File(fileBytes, 
                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   $"comparativo_regional_{DateTime.Now:yyyyMMdd}.xlsx");
    }
    catch (Exception ex)
    {
        return Json(new { success = false, error = ex.Message });
    }
}
```

### FacturasController.cs - Reportes de Facturación
```csharp
[HttpPost]
public async Task<IActionResult> ReporteMensualFacturacion(int mes, int año)
{
    try
    {
        // 1. Obtener datos de facturación de la BD
        var datosFacturacion = await _context.Facturas
            .Where(f => f.Fecha.Month == mes && f.Fecha.Year == año)
            .GroupBy(f => new { f.Region, f.TipoEnergia })
            .Select(g => new {
                Region = g.Key.Region,
                TipoEnergia = g.Key.TipoEnergia,
                MontoTotal = g.Sum(f => f.Monto),
                ConsumoTotal = g.Sum(f => f.Consumo),
                NumeroFacturas = g.Count()
            })
            .ToListAsync();

        // 2. Generar reporte Excel
        var resultado = await _mcpService.CallExcel<ExcelResult>("generate_report", new {
            data = datosFacturacion,
            template = "reporte_facturacion",
            filename = $"facturacion_{mes:00}_{año}.xlsx",
            configuracion = new {
                incluir_graficos = true,
                formato_moneda = "MXN",
                mostrar_totales = true
            }
        });

        // 3. Log y retorno
        _logger.LogInformation($"Reporte de facturación generado: {resultado.FileName}");
        
        var filePath = Path.Combine("Reportes", resultado.FileName);
        var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
        
        return File(fileBytes, 
                   "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                   resultado.FileName);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error generando reporte de facturación");
        return Json(new { success = false, error = ex.Message });
    }
}
```

## Ejemplos de JavaScript (Frontend)

### Dashboard Dinámico
```javascript
// dashboard.js - Actualización dinámica de métricas
class SNIERDashboard {
    constructor() {
        this.apiBase = '/api';
        this.refreshInterval = 300000; // 5 minutos
        this.init();
    }

    async init() {
        await this.loadMetrics();
        this.startAutoRefresh();
        this.bindEvents();
    }

    async loadMetrics() {
        try {
            const response = await fetch(`${this.apiBase}/indicadores/dashboard-metrics`);
            const data = await response.json();
            
            if (data.error) {
                throw new Error(data.error);
            }

            this.updateMetricsDisplay(data);
        } catch (error) {
            console.error('Error loading metrics:', error);
            this.showError('Error cargando métricas del dashboard');
        }
    }

    updateMetricsDisplay(data) {
        // Actualizar tarjetas de métricas
        document.getElementById('consumo-nacional').textContent = 
            this.formatNumber(data.consumoNacional) + ' MWh';
        
        document.getElementById('eficiencia-promedio').textContent = 
            data.eficienciaPromedio.toFixed(1) + '%';
            
        document.getElementById('proyectos-activos').textContent = 
            this.formatNumber(data.proyectosActivos);
            
        document.getElementById('ahorro-energetico').textContent = 
            data.ahorroEnergetico.toFixed(2) + '%';

        // Actualizar gráficos regionales si existen
        if (data.datosRegionales) {
            this.updateRegionalCharts(data.datosRegionales);
        }
    }

    async generateExecutiveReport() {
        try {
            this.showLoading('Generando reporte ejecutivo...');
            
            const response = await fetch(`${this.apiBase}/indicadores/generar-reporte-ejecutivo`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            });

            if (response.ok) {
                // Descargar archivo
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `reporte_ejecutivo_${new Date().toISOString().slice(0,10)}.xlsx`;
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
                
                this.showSuccess('Reporte generado exitosamente');
            } else {
                throw new Error('Error generando reporte');
            }
        } catch (error) {
            console.error('Error:', error);
            this.showError('Error generando reporte ejecutivo');
        } finally {
            this.hideLoading();
        }
    }

    bindEvents() {
        // Botón generar reporte
        document.getElementById('btn-generar-reporte')?.addEventListener('click', () => {
            this.generateExecutiveReport();
        });

        // Selección de región
        document.getElementById('select-region')?.addEventListener('change', (e) => {
            this.loadRegionalMetrics(e.target.value);
        });
    }

    formatNumber(num) {
        return new Intl.NumberFormat('es-MX').format(num);
    }

    startAutoRefresh() {
        setInterval(() => {
            this.loadMetrics();
        }, this.refreshInterval);
    }
}

// Inicializar dashboard
document.addEventListener('DOMContentLoaded', () => {
    new SNIERDashboard();
});
```

## Automatización con PowerShell

### Script de Reportes Automáticos
```powershell
# reportes-automaticos.ps1
param(
    [string]$Environment = "Production",
    [string]$OutputPath = "C:\Proyectos\SNIER\Reportes\Automaticos"
)

$ApiBase = if ($Environment -eq "Production") { 
    "https://snier.gob.mx/api" 
} else { 
    "https://localhost:5001/api" 
}

# Función para generar reporte
function New-SNIERReport {
    param(
        [string]$ReportType,
        [hashtable]$Parameters = @{}
    )
    
    try {
        Write-Host "Generando reporte: $ReportType" -ForegroundColor Green
        
        $Body = $Parameters | ConvertTo-Json
        $Response = Invoke-RestMethod -Uri "$ApiBase/$ReportType" -Method POST -Body $Body -ContentType "application/json"
        
        if ($Response.success) {
            Write-Host "✓ Reporte $ReportType generado exitosamente" -ForegroundColor Green
            return $Response.filename
        } else {
            Write-Error "Error: $($Response.error)"
            return $null
        }
    }
    catch {
        Write-Error "Error generando reporte $ReportType : $($_.Exception.Message)"
        return $null
    }
}

# Generar reportes del día
$Fecha = Get-Date
$Mes = $Fecha.Month
$Año = $Fecha.Year

Write-Host "=== SNIER - Generación Automática de Reportes ===" -ForegroundColor Cyan
Write-Host "Fecha: $($Fecha.ToString('yyyy-MM-dd'))" -ForegroundColor Yellow

# 1. Dashboard Ejecutivo
New-SNIERReport -ReportType "indicadores/generar-reporte-ejecutivo"

# 2. Reporte Mensual de Facturación
if ($Fecha.Day -eq 1) {  # Primer día del mes
    $MesAnterior = if ($Mes -eq 1) { 12 } else { $Mes - 1 }
    $AñoAnterior = if ($Mes -eq 1) { $Año - 1 } else { $Año }
    
    New-SNIERReport -ReportType "facturas/reporte-mensual-facturacion" -Parameters @{
        mes = $MesAnterior
        año = $AñoAnterior
    }
}

# 3. Análisis Regional (semanal)
if ($Fecha.DayOfWeek -eq 'Monday') {  # Lunes
    $Regiones = @("Norte", "Centro", "Sur", "Golfo")
    New-SNIERReport -ReportType "proyectos/exportar-comparativo-regional" -Parameters @{
        regiones = $Regiones
    }
}

Write-Host "=== Proceso completado ===" -ForegroundColor Cyan
```

## Próximos Pasos

1. **Configurar los servidores MCP** ejecutando:
   ```bash
   npm install -g @modelcontextprotocol/server-analytics
   npm install -g @modelcontextprotocol/server-excel
   ```

2. **Iniciar los servidores**:
   ```bash
   mcp-analytics-server --port 3001
   mcp-excel-server --port 3002
   ```

3. **Integrar el servicio MCP** en el `Program.cs`

4. **Configurar tareas automáticas** en Windows Task Scheduler para ejecutar los scripts de PowerShell

Con esta implementación, SNIER tendrá capacidades avanzadas de analytics y generación de reportes Excel completamente automatizadas.
