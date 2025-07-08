# Ejemplos Prácticos - MCP Analytics SNIER

## 🔍 **Ejemplo 1: Dashboard en Tiempo Real**

### Controlador C# (IndicadoresController.cs)
```csharp
[HttpGet]
public async Task<IActionResult> DashboardTiempoReal()
{
    try
    {
        // 1. Análisis sectorial del consumo energético
        var consumoSectorial = await _mcpService.CallAnalytics<List<ConsumoSectorial>>(
            "analizarConsumoSectorial", 
            new { año = DateTime.Now.Year }
        );

        // 2. Tendencias de energías renovables (últimos 6 meses)
        var tendenciasRenovables = await _mcpService.CallAnalytics<List<TendenciaRenovable>>(
            "tendenciasRenovables", 
            new { rangoMeses = 6 }
        );

        // 3. Análisis de precios energéticos
        var analisisPrecios = await _mcpService.CallAnalytics<List<AnalisisPrecio>>(
            "analisisPreciosEnergia", 
            new { mercado = "MEM" }
        );

        // 4. Indicadores de sostenibilidad
        var indicadoresSostenibilidad = await _mcpService.CallAnalytics<List<IndicadorSostenibilidad>>(
            "indicadoresSostenibilidad", 
            new { }
        );

        var dashboard = new DashboardViewModel
        {
            ConsumoSectorial = consumoSectorial,
            TendenciasRenovables = tendenciasRenovables,
            AnalisisPrecios = analisisPrecios,
            IndicadoresSostenibilidad = indicadoresSostenibilidad,
            FechaActualizacion = DateTime.Now
        };

        return Json(dashboard);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error en dashboard tiempo real");
        return Json(new { error = ex.Message });
    }
}
```

### JavaScript Frontend
```javascript
// dashboard-analytics.js
class SNIERAnalyticsDashboard {
    constructor() {
        this.apiBase = '/api/indicadores';
        this.refreshInterval = 60000; // 1 minuto
        this.charts = {};
        this.init();
    }

    async init() {
        await this.loadDashboardData();
        this.initializeCharts();
        this.startAutoRefresh();
    }

    async loadDashboardData() {
        try {
            const response = await fetch(`${this.apiBase}/dashboard-tiempo-real`);
            const data = await response.json();
            
            if (data.error) {
                throw new Error(data.error);
            }

            this.updateDashboard(data);
        } catch (error) {
            console.error('Error cargando dashboard:', error);
        }
    }

    updateDashboard(data) {
        // 1. Actualizar consumo sectorial
        this.updateConsumoSectorial(data.consumoSectorial);
        
        // 2. Actualizar tendencias renovables
        this.updateTendenciasRenovables(data.tendenciasRenovables);
        
        // 3. Actualizar análisis de precios
        this.updateAnalisisPrecios(data.analisisPrecios);
        
        // 4. Actualizar indicadores de sostenibilidad
        this.updateIndicadoresSostenibilidad(data.indicadoresSostenibilidad);
    }

    updateConsumoSectorial(data) {
        // Crear gráfico de barras para consumo por sector
        const ctx = document.getElementById('chart-consumo-sectorial').getContext('2d');
        
        if (this.charts.consumoSectorial) {
            this.charts.consumoSectorial.destroy();
        }

        this.charts.consumoSectorial = new Chart(ctx, {
            type: 'bar',
            data: {
                labels: data.map(item => item.sector),
                datasets: [{
                    label: 'Consumo Total (TWh)',
                    data: data.map(item => item.consumoTotal),
                    backgroundColor: '#0D4B6E',
                    borderColor: '#1E88E5',
                    borderWidth: 1
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Consumo Energético por Sector'
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    updateTendenciasRenovables(data) {
        // Agrupar datos por tipo de energía
        const tiposEnergia = [...new Set(data.map(item => item.tipoEnergia))];
        const datasets = tiposEnergia.map((tipo, index) => {
            const datosFiltered = data.filter(item => item.tipoEnergia === tipo);
            return {
                label: tipo,
                data: datosFiltered.map(item => ({
                    x: `${item.año}-${item.mes.toString().padStart(2, '0')}`,
                    y: item.generacionTotal
                })),
                borderColor: this.getColorByIndex(index),
                backgroundColor: this.getColorByIndex(index, 0.1),
                tension: 0.4
            };
        });

        const ctx = document.getElementById('chart-renovables').getContext('2d');
        
        if (this.charts.renovables) {
            this.charts.renovables.destroy();
        }

        this.charts.renovables = new Chart(ctx, {
            type: 'line',
            data: {
                datasets: datasets
            },
            options: {
                responsive: true,
                plugins: {
                    title: {
                        display: true,
                        text: 'Tendencias Energías Renovables (6 meses)'
                    }
                },
                scales: {
                    x: {
                        type: 'time',
                        time: {
                            parser: 'YYYY-MM'
                        }
                    }
                }
            }
        });
    }
}

// Inicializar dashboard
document.addEventListener('DOMContentLoaded', () => {
    new SNIERAnalyticsDashboard();
});
```

## 📊 **Ejemplo 2: Análisis de Eficiencia Regional**

### Controlador C# (ProyectosController.cs)
```csharp
[HttpGet]
public async Task<IActionResult> AnalisisEficienciaRegional(string region = null)
{
    try
    {
        // Análisis de eficiencia por entidad federativa
        var eficienciaRegional = await _mcpService.CallAnalytics<List<EficienciaRegional>>(
            "analisisEficienciaRegional", 
            new { 
                entidadFederativa = region,
                periodo = "anual"
            }
        );

        // Ranking de eficiencia
        var rankingEficiencia = await _mcpService.CallAnalytics<List<RankingEficiencia>>(
            "rankingEficienciaEstados", 
            new { año = DateTime.Now.Year }
        );

        // Proyecciones de demanda
        var proyecciones = await _mcpService.CallAnalytics<List<ProyeccionDemanda>>(
            "proyeccionesDemanda", 
            new { entidadFederativa = region }
        );

        var modelo = new AnalisisRegionalViewModel
        {
            EficienciaRegional = eficienciaRegional,
            RankingEficiencia = rankingEficiencia,
            ProyeccionesDemanda = proyecciones,
            RegionSeleccionada = region
        };

        return View(modelo);
    }
    catch (Exception ex)
    {
        ViewBag.Error = ex.Message;
        return View();
    }
}

[HttpPost]
public async Task<IActionResult> ExportarAnalisisRegional(string[] entidades)
{
    try
    {
        // Obtener datos de múltiples entidades
        var datosComparativos = new List<object>();
        
        foreach (var entidad in entidades)
        {
            var datos = await _mcpService.CallAnalytics<object>(
                "analisisComparativoEstados", 
                new { entidadFederativa = entidad }
            );
            datosComparativos.Add(datos);
        }

        // Generar Excel con comparativo
        var resultado = await _mcpService.CallExcel<ExcelResult>(
            "export_comparison", 
            new {
                data = datosComparativos,
                template = "comparativo_regional",
                filename = $"analisis_regional_{DateTime.Now:yyyyMMdd}.xlsx",
                configuracion = new {
                    incluir_graficos = true,
                    formato_datos = "tabular",
                    mostrar_tendencias = true
                }
            }
        );

        // Retornar archivo para descarga
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
```

## 🔋 **Ejemplo 3: Monitoreo de Energías Renovables**

### Análisis en Tiempo Real
```csharp
[HttpGet]
public async Task<IActionResult> MonitoreoRenovables()
{
    try
    {
        // Estado actual de plantas renovables
        var estadoActual = await _mcpService.CallAnalytics<List<EstadoPlantaRenovable>>(
            "estadoPlantasRenovables", 
            new { }
        );

        // Producción por hora del día
        var produccionHoraria = await _mcpService.CallAnalytics<List<ProduccionHoraria>>(
            "produccionRenovablesHoraria", 
            new { fecha = DateTime.Today }
        );

        // Comparación con meta anual
        var comparacionMetas = await _mcpService.CallAnalytics<List<ComparacionMeta>>(
            "comparacionMetasRenovables", 
            new { año = DateTime.Now.Year }
        );

        // Predicción climática para generación
        var prediccionGeneracion = await _mcpService.CallAnalytics<List<PrediccionGeneracion>>(
            "prediccionGeneracionClimatica", 
            new { diasAdelante = 7 }
        );

        var monitoreo = new MonitoreoRenovablesViewModel
        {
            EstadoActual = estadoActual,
            ProduccionHoraria = produccionHoraria,
            ComparacionMetas = comparacionMetas,
            PrediccionGeneracion = prediccionGeneracion
        };

        return Json(monitoreo);
    }
    catch (Exception ex)
    {
        return Json(new { error = ex.Message });
    }
}
```

### WebSocket para Actualizaciones en Tiempo Real
```javascript
// monitoreo-renovables.js
class MonitoreoRenovablesRT {
    constructor() {
        this.websocket = null;
        this.reconnectAttempts = 0;
        this.maxReconnectAttempts = 5;
        this.init();
    }

    init() {
        this.conectarWebSocket();
        this.cargarDatosIniciales();
        this.bindEvents();
    }

    conectarWebSocket() {
        const wsUrl = `wss://${window.location.host}/ws/renovables`;
        this.websocket = new WebSocket(wsUrl);

        this.websocket.onopen = () => {
            console.log('🔌 Conectado al monitoreo en tiempo real');
            this.reconnectAttempts = 0;
            this.showStatus('Conectado', 'success');
        };

        this.websocket.onmessage = (event) => {
            const data = JSON.parse(event.data);
            this.procesarActualizacion(data);
        };

        this.websocket.onclose = () => {
            console.log('📡 Conexión cerrada, intentando reconectar...');
            this.showStatus('Desconectado', 'warning');
            this.intentarReconexion();
        };

        this.websocket.onerror = (error) => {
            console.error('❌ Error WebSocket:', error);
            this.showStatus('Error', 'danger');
        };
    }

    async cargarDatosIniciales() {
        try {
            const response = await fetch('/api/proyectos/monitoreo-renovables');
            const data = await response.json();
            
            this.actualizarInterfaz(data);
        } catch (error) {
            console.error('Error cargando datos iniciales:', error);
        }
    }

    procesarActualizacion(data) {
        switch (data.tipo) {
            case 'estado_planta':
                this.actualizarEstadoPlanta(data.planta);
                break;
            case 'produccion_actualizada':
                this.actualizarProduccion(data.produccion);
                break;
            case 'alerta_mantenimiento':
                this.mostrarAlerta(data.alerta);
                break;
            case 'meta_alcanzada':
                this.celebrarMeta(data.meta);
                break;
        }
    }

    actualizarEstadoPlanta(planta) {
        const elemento = document.getElementById(`planta-${planta.id}`);
        if (elemento) {
            elemento.querySelector('.estado').textContent = planta.estado;
            elemento.querySelector('.produccion').textContent = 
                `${planta.produccionActual} MW`;
            
            // Cambiar color según estado
            const statusClass = planta.estado === 'Operando' ? 'status-ok' : 'status-warning';
            elemento.className = `planta-card ${statusClass}`;
        }
    }
}

// Inicializar monitoreo
document.addEventListener('DOMContentLoaded', () => {
    new MonitoreoRenovablesRT();
});
```

## 💰 **Ejemplo 4: Análisis de Precios y Mercado**

### API para Análisis de Mercado
```csharp
[HttpGet]
public async Task<IActionResult> AnalisisMercadoEnergetico(
    string mercado = "MEM", 
    int dias = 30)
{
    try
    {
        // Análisis de precios
        var analisisPrecios = await _mcpService.CallAnalytics<AnalisisPreciosDetallado>(
            "analisisPreciosDetallado", 
            new { 
                mercado = mercado,
                diasHistoricos = dias
            }
        );

        // Volatilidad del mercado
        var volatilidad = await _mcpService.CallAnalytics<VolatilidadMercado>(
            "calcularVolatilidadMercado", 
            new { 
                mercado = mercado,
                periodo = "mensual"
            }
        );

        // Predicción de precios
        var prediccionPrecios = await _mcpService.CallAnalytics<List<PrediccionPrecio>>(
            "prediccionPreciosEnergia", 
            new { 
                mercado = mercado,
                diasPrediccion = 7
            }
        );

        // Análisis de correlaciones
        var correlaciones = await _mcpService.CallAnalytics<List<CorrelacionMercado>>(
            "correlacionesMercadoEnergia", 
            new { mercado = mercado }
        );

        var analisis = new AnalisisMercadoViewModel
        {
            AnalisisPrecios = analisisPrecios,
            Volatilidad = volatilidad,
            PrediccionPrecios = prediccionPrecios,
            Correlaciones = correlaciones,
            MercadoAnalizado = mercado
        };

        return Json(analisis);
    }
    catch (Exception ex)
    {
        return Json(new { error = ex.Message });
    }
}
```

## 🎯 **Ejemplo 5: Alertas Automáticas**

### Sistema de Alertas Inteligentes
```csharp
[HttpPost]
public async Task<IActionResult> ConfigurarAlertas([FromBody] ConfiguracionAlertas config)
{
    try
    {
        // Configurar umbrales de alerta
        var resultadoConfig = await _mcpService.CallAnalytics<object>(
            "configurarSistemaAlertas", 
            new {
                umbralConsumoAlto = config.UmbralConsumoAlto,
                umbralEficienciaBaja = config.UmbralEficienciaBaja,
                umbralPrecioAlto = config.UmbralPrecioAlto,
                emailsNotificacion = config.EmailsNotificacion,
                tiposAlerta = config.TiposAlerta
            }
        );

        // Verificar alertas actuales
        var alertasActivas = await _mcpService.CallAnalytics<List<AlertaActiva>>(
            "verificarAlertasActivas", 
            new { }
        );

        return Json(new { 
            success = true, 
            configuracion = resultadoConfig,
            alertasActivas = alertasActivas
        });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, error = ex.Message });
    }
}

[HttpGet]
public async Task<IActionResult> ObtenerAlertas()
{
    try
    {
        var alertas = await _mcpService.CallAnalytics<List<AlertaEnergetica>>(
            "obtenerAlertasRecientes", 
            new { horasAtras = 24 }
        );

        return Json(alertas);
    }
    catch (Exception ex)
    {
        return Json(new { error = ex.Message });
    }
}
```

## 🔄 **Automatización con PowerShell**

```powershell
# Script para generar reportes automáticos usando Analytics
param(
    [string]$TipoReporte = "completo",
    [string]$Periodo = "mensual"
)

function Invoke-SNIERAnalytics {
    param(
        [string]$Funcion,
        [hashtable]$Parametros
    )
    
    $Uri = "https://localhost:5001/api/indicadores/ejecutar-analytics"
    $Body = @{
        funcion = $Funcion
        parametros = $Parametros
    } | ConvertTo-Json
    
    try {
        $Response = Invoke-RestMethod -Uri $Uri -Method POST -Body $Body -ContentType "application/json"
        return $Response
    }
    catch {
        Write-Error "Error ejecutando función $Funcion : $($_.Exception.Message)"
        return $null
    }
}

# Ejecutar análisis automatizado
Write-Host "=== Análisis Automático SNIER ===" -ForegroundColor Cyan

# 1. Análisis sectorial
$ConsumoSectorial = Invoke-SNIERAnalytics -Funcion "analizarConsumoSectorial" -Parametros @{año = (Get-Date).Year}

# 2. Tendencias renovables
$TendenciasRenovables = Invoke-SNIERAnalytics -Funcion "tendenciasRenovables" -Parametros @{rangoMeses = 12}

# 3. Indicadores sostenibilidad
$Sostenibilidad = Invoke-SNIERAnalytics -Funcion "indicadoresSostenibilidad" -Parametros @{}

# 4. Generar reporte Excel
if ($ConsumoSectorial -and $TendenciasRenovables -and $Sostenibilidad) {
    $DatosReporte = @{
        consumoSectorial = $ConsumoSectorial
        tendenciasRenovables = $TendenciasRenovables
        sostenibilidad = $Sostenibilidad
        fechaGeneracion = Get-Date
    }
    
    $ReporteUri = "https://localhost:5001/api/facturas/generar-reporte-analytics"
    $ReporteBody = $DatosReporte | ConvertTo-Json -Depth 10
    
    try {
        $Archivo = Invoke-RestMethod -Uri $ReporteUri -Method POST -Body $ReporteBody -ContentType "application/json"
        Write-Host "✅ Reporte generado: $($Archivo.filename)" -ForegroundColor Green
    }
    catch {
        Write-Error "Error generando reporte: $($_.Exception.Message)"
    }
}

Write-Host "=== Proceso completado ===" -ForegroundColor Cyan
```

Estos ejemplos muestran cómo usar el servidor MCP Analytics para obtener insights profundos sobre los datos energéticos de SNIER, desde dashboards en tiempo real hasta análisis predictivos y sistemas de alertas automáticas.
