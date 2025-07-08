# Ejemplos de Uso - MCP Analytics Server

## Descripción
El servidor MCP Analytics de SNIER proporciona funciones avanzadas para análisis energético, métricas de rendimiento y generación de reportes especializados.

## Funciones Disponibles

### 1. Análisis de Consumo Energético
```javascript
// Función: analyze_energy_consumption
// Analiza patrones de consumo energético por región/tipo
{
  "region": "Norte",
  "periodo": "2024-Q1",
  "tipo_energia": "electrica"
}
```

### 2. Métricas de Eficiencia
```javascript
// Función: calculate_efficiency_metrics
// Calcula métricas de eficiencia energética
{
  "unidad_id": "UE001",
  "periodo": "2024-01",
  "tipo_metrica": "general"
}
```

### 3. Proyecciones de Demanda
```javascript
// Función: forecast_demand
// Proyecta demanda futura basada en datos históricos
{
  "region": "Centro",
  "horizonte_meses": 12,
  "tipo_energia": "gas_natural"
}
```

## Casos de Uso Prácticos

### Caso 1: Dashboard Ejecutivo
**Objetivo**: Generar métricas para el dashboard principal
```javascript
// 1. Obtener consumo nacional actual
await analytics.analyze_energy_consumption({
  region: "Nacional",
  periodo: "2024-12",
  tipo_energia: "total"
});

// 2. Calcular eficiencia promedio
await analytics.calculate_efficiency_metrics({
  unidad_id: "ALL",
  periodo: "2024-12",
  tipo_metrica: "promedio_nacional"
});
```

### Caso 2: Análisis Regional
**Objetivo**: Comparar rendimiento entre regiones
```javascript
// Análisis por cada región
const regiones = ["Norte", "Centro", "Sur", "Golfo"];
for (const region of regiones) {
  await analytics.analyze_energy_consumption({
    region: region,
    periodo: "2024-Q4",
    tipo_energia: "electrica"
  });
}
```

### Caso 3: Planificación Estratégica
**Objetivo**: Proyecciones para planificación a mediano plazo
```javascript
// Proyección de demanda por tipo de energía
const tipos = ["electrica", "gas_natural", "renovable"];
for (const tipo of tipos) {
  await analytics.forecast_demand({
    region: "Nacional",
    horizonte_meses: 24,
    tipo_energia: tipo
  });
}
```

## Integración con Controladores

### En IndicadoresController.cs
```csharp
// Ejemplo de integración con MCP Analytics
public async Task<IActionResult> GetAnalyticsDashboard()
{
    // Llamar al servidor MCP Analytics
    var consumoData = await _mcpService.CallAnalytics("analyze_energy_consumption", new {
        region = "Nacional",
        periodo = DateTime.Now.ToString("yyyy-MM"),
        tipo_energia = "total"
    });
    
    return Json(consumoData);
}
```

### En ProyectosController.cs
```csharp
public async Task<IActionResult> GetProyecciones(string region)
{
    var proyecciones = await _mcpService.CallAnalytics("forecast_demand", new {
        region = region,
        horizonte_meses = 12,
        tipo_energia = "renovable"
    });
    
    ViewBag.Proyecciones = proyecciones;
    return View();
}
```
