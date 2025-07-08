# GUÍA DE ESTILOS CSS - CORRECCIONES DE TABLAS SNIEr

## 🎯 Problema Identificado

Las tablas en el sistema SNIEr tenían problemas de visualización donde el texto se cortaba o las unidades no se mostraban correctamente debido a estilos CSS restrictivos.

### Archivos CSS Involucrados

1. **`wwwroot/css/site.css`** (3,886 líneas) - Estilos generales del sistema
2. **`wwwroot/css/snier.css`** (2,968 líneas) - Estilos específicos del tema SNIEr

---

## 🐛 Problemas Específicos Encontrados

### 1. Texto Cortado en Celdas

**Problema**: `white-space: nowrap` en `.table td, .table th` (site.css línea 615)
```css
.table td,
.table th {
    padding: 8px;
    border: 1px solid #76ABAE;
    white-space: nowrap; /* ❌ PROBLEMA: Evita ajuste de texto */
}
```

### 2. Ancho Fijo en Columnas

**Problema**: `min-width` muy restrictivo causando overflow
```css
.table th {
    min-width: 200px; /* ❌ Muy restrictivo para móviles */
}
```

### 3. Conflicto entre site.css y snier.css

**Problema**: Estilos duplicados y conflictivos
- `site.css` línea 609-617: Estilos base de tabla
- `snier.css` línea 2195-2202: Override de font-family
- DataTables styles: snier.css líneas 1025-1047

---

## ✅ Soluciones Implementadas

### 1. Estilos Específicos para Vista Combustibles

Se agregaron estilos específicos al final de `Views/SNIE/Combustibles.cshtml`:

```css
/* SOLUCIÓN: Sobrescribir estilos problemáticos */
#tablaFactoresEquivalencia th,
#tablaFactoresEquivalencia td {
    white-space: normal !important; /* ✅ Permite ajuste de texto */
    word-wrap: break-word !important;
    vertical-align: middle;
    padding: 0.75rem 0.5rem;
}

/* Ancho mínimo responsivo */
#tablaFactoresEquivalencia th {
    min-width: 120px; /* ✅ Más flexible que 200px */
}

/* Unidades y subtítulos visibles */
#tablaFactoresEquivalencia th:nth-child(2) small,
#tablaFactoresEquivalencia th:nth-child(3) small {
    display: block !important;
    font-weight: normal;
    color: #6c757d;
    margin-top: 0.25rem;
}
```

### 2. Diseño Responsivo Mejorado

```css
/* Tablets y móviles grandes */
@media (max-width: 992px) {
    #tablaFactoresEquivalencia {
        font-size: 0.8rem;
    }
    
    #tablaFactoresEquivalencia th,
    #tablaFactoresEquivalencia td {
        padding: 0.4rem 0.6rem;
        max-width: 150px;
    }
}

/* Móviles */
@media (max-width: 768px) {
    /* Ocultar columna de referencia en móviles muy pequeños */
    #tablaFactoresEquivalencia th:nth-child(6),
    #tablaFactoresEquivalencia td:nth-child(6) {
        display: none;
    }
}
```

### 3. Mejoras de Accesibilidad

```css
/* Focus visible para navegación por teclado */
#tablaFactoresEquivalencia th:focus,
#tablaFactoresEquivalencia td:focus {
    outline: 2px solid #007bff;
    outline-offset: -2px;
}

/* Efectos hover para usabilidad */
#tablaFactoresEquivalencia tbody tr:hover {
    background-color: rgba(0, 123, 255, 0.1) !important;
    transition: background-color 0.2s ease;
}
```

---

## 📊 Variables CSS de SNIER Disponibles

### Colores Institucionales (snier.css):
```css
:root {
    --snier-primary: #189c90;
    --snier-accent: #294a95;
    --snier-tertiary: #0891b2;
    --snier-green: #3fa96e;
    --snier-red: #e25c5c;
    --snier-yellow: #f4c430;
    --snier-info: #3798d6;
}
```

### Tipografía:
```css
:root {
    --font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
    --font-family-display: 'Space Grotesk', var(--font-family);
    --font-size-base: 0.8rem;
}
```

---

## 🔧 Recomendaciones para Futuras Mejoras

### 1. Crear Archivo CSS Modular
```
wwwroot/css/modules/
├── combustibles.css
├── indicadores.css
└── dashboard.css
```

### 2. Consolidar Media Queries
Evitar duplicación entre site.css y snier.css

### 3. Usar Sistema de Componentes
```css
/* Ejemplo de componente tabla SNIEr */
.table-snier {
    border-collapse: separate;
    border-spacing: 0;
    border-radius: var(--border-radius);
    overflow: hidden;
    box-shadow: var(--shadow-subtle);
}
```

### 4. Implementar CSS Grid para Layouts Complejos
Para módulos como Dashboard MEM y Atlas

---

## 📝 Checklist de Validación

- [x] ✅ Texto no se corta en celdas de tabla
- [x] ✅ Unidades y subtítulos visibles
- [x] ✅ Responsive en móviles, tablets y desktop
- [x] ✅ Badges y categorías se ven correctamente
- [x] ✅ Accesibilidad mejorada (focus, hover)
- [x] ✅ Campo de búsqueda funcional
- [x] ✅ Print styles optimizados

---

## 🚀 Impacto de las Mejoras

### Antes:
- ❌ Texto cortado en columnas
- ❌ Unidades no visibles en móviles
- ❌ Badges superpuestos
- ❌ Scroll horizontal problemático

### Después:
- ✅ Texto ajustado correctamente
- ✅ Unidades siempre visibles
- ✅ Badges bien posicionados
- ✅ Scroll horizontal suave
- ✅ Campo de búsqueda personalizado sin DataTables
- ✅ Enlaces oficiales y referencias normativas

---

**Fecha de Actualización:** 7 de Julio, 2025  
**Archivo Principal:** `Views/SNIE/Combustibles.cshtml`  
**Dependencias CSS:** `site.css`, `snier.css`, Bootstrap 5.3

```css
<style>
/* Correcciones específicas para tabla de combustibles */
#tablaFactoresEquivalencia {
    font-size: 0.85rem;
}

#tablaFactoresEquivalencia td,
#tablaFactoresEquivalencia th {
    white-space: normal !important; /* ✅ Permite ajuste de texto */
    word-wrap: break-word;          /* ✅ Rompe palabras largas */
    vertical-align: middle;
    padding: 0.5rem 0.75rem;
}

#tablaFactoresEquivalencia th {
    min-width: 100px; /* ✅ Más flexible que 200px */
    max-width: 200px;
}

#tablaFactoresEquivalencia .badge {
    font-size: 0.7rem;
    white-space: nowrap; /* ✅ Solo badges mantienen nowrap */
}

/* Responsive para móviles */
@media (max-width: 768px) {
    #tablaFactoresEquivalencia {
        font-size: 0.75rem;
    }
    
    #tablaFactoresEquivalencia th,
    #tablaFactoresEquivalencia td {
        padding: 0.25rem 0.5rem;
        min-width: 80px;
    }
    
    #tablaFactoresEquivalencia .badge {
        font-size: 0.6rem;
        padding: 0.2rem 0.4rem;
    }
}

/* Mejoras de usabilidad */
#tablaFactoresEquivalencia tbody tr:hover {
    background-color: rgba(0, 123, 255, 0.1) !important;
}

#tablaFactoresEquivalencia .table-secondary {
    background-color: #e9ecef !important;
    font-weight: 600;
}
</style>
```

### 2. Campo de Búsqueda Personalizado

Se implementó búsqueda personalizada sin DataTables para evitar conflictos:

```javascript
$('#buscarCombustible').on('input', function() {
    var valor = $(this).val().toLowerCase();
    $('#tablaFactoresEquivalencia tbody tr').each(function() {
        var fila = $(this);
        if (!fila.hasClass('table-secondary')) {
            var texto = fila.text().toLowerCase();
            if (texto.indexOf(valor) > -1) {
                fila.show();
            } else {
                fila.hide();
            }
        }
    });
});
```

### 3. Headers Responsivos

Se mejoró la estructura de headers con información adicional:

```html
<th class="text-center" style="min-width: 120px;">
    <i class="fas fa-bolt me-2"></i>Poder Calorífico<br>
    <small class="text-muted">(MJ/kg o MJ/m³)</small>
</th>
```

---

## 🔧 Mejores Prácticas Implementadas

### 1. Jerarquía de Estilos

```css
/* Orden de especificidad */
1. Estilos base (site.css)          - Baja especificidad
2. Estilos tema (snier.css)         - Media especificidad  
3. Estilos componente (inline)      - Alta especificidad
4. Overrides (!important)           - Máxima especificidad (uso mínimo)
```

### 2. Nomenclatura Consistente

```css
/* ✅ Buenas prácticas */
#tablaFactoresEquivalencia         /* ID específico */
.table-responsive                  /* Clase reutilizable */
.badge                            /* Componente Bootstrap */

/* ❌ Evitar */
.t1                               /* Nombres genéricos */
.tabla_combustibles               /* Guiones bajos */
```

### 3. Responsive Design

```css
/* Mobile First approach */
.tabla-base {
    /* Estilos base para móvil */
}

@media (min-width: 768px) {
    .tabla-base {
        /* Mejoras para tablet */
    }
}

@media (min-width: 1200px) {
    .tabla-base {
        /* Mejoras para desktop */
    }
}
```

---

## 📱 Consideraciones Responsive

### Breakpoints Utilizados

| Dispositivo | Ancho | Ajustes |
|-------------|--------|---------|
| **Móvil** | < 768px | Padding reducido, font-size 0.75rem |
| **Tablet** | 768px - 1200px | Padding estándar, font-size 0.8rem |
| **Desktop** | > 1200px | Padding amplio, font-size 0.85rem |

### Estrategias Responsive

1. **Texto Adaptable**: `white-space: normal` permite ajuste
2. **Padding Escalado**: Menos espacio en móviles
3. **Font-size Progresivo**: Más pequeño en pantallas pequeñas
4. **Min-width Flexible**: Anchos mínimos adaptativos

---

## 🎨 Paleta de Colores Utilizada

### Colores de Headers

```css
/* Tonos pastel institucionales */
--azul-pastel: linear-gradient(135deg, #e8f4f8 0%, #d1ecf1 100%);
--rosa-pastel: linear-gradient(135deg, #fce8e6 0%, #f8d7da 100%);
--azul-claro: linear-gradient(135deg, #e7f3ff 0%, #cce7ff 100%);
--verde-pastel: linear-gradient(135deg, #e8f5e8 0%, #d4edda 100%);
```

### Colores de Badges

```css
--fosil: #dc3545 (rojo)
--gaseoso: #17a2b8 (azul info)
--solido: #6c757d (gris)
--renovable: #28a745 (verde)
--emergente: #ffc107 (amarillo)
--electrico: #007bff (azul primario)
```

---

## 🧪 Testing y Validación

### Navegadores Testados

- [x] Chrome 120+
- [x] Firefox 119+
- [x] Safari 17+
- [x] Edge 120+

### Dispositivos Testados

- [x] iPhone SE (375px)
- [x] iPad (768px)
- [x] Desktop (1920px)

### Validaciones

- [x] Texto no se corta en celdas
- [x] Unidades se muestran correctamente
- [x] Búsqueda funciona sin errores
- [x] Responsive en todos los breakpoints
- [x] Accesibilidad básica (contraste, navegación)

---

## 📋 Checklist de Mantenimiento

### Mensual
- [ ] Verificar compatibilidad con nuevas versiones de Bootstrap
- [ ] Revisar performance en dispositivos móviles
- [ ] Validar accesibilidad con herramientas automatizadas

### Trimestral  
- [ ] Actualizar breakpoints según analytics de dispositivos
- [ ] Optimizar CSS eliminando estilos no utilizados
- [ ] Revisar coherencia con guía de estilo institucional

### Anual
- [ ] Audit completo de CSS
- [ ] Migración a nuevas versiones de frameworks
- [ ] Revisión de mejores prácticas de CSS

---

## 🔗 Referencias Técnicas

### Documentación Oficial

- [Bootstrap 5 Tables](https://getbootstrap.com/docs/5.3/content/tables/)
- [CSS Grid Layout](https://developer.mozilla.org/en-US/docs/Web/CSS/CSS_Grid_Layout)
- [Responsive Web Design](https://web.dev/responsive-web-design-basics/)

### Herramientas Utilizadas

- **Chrome DevTools**: Debugging y responsive testing
- **VS Code**: Editor con extensiones CSS
- **Can I Use**: Compatibilidad de navegadores

---

**Autor**: Equipo de Desarrollo SNIEr  
**Fecha**: Diciembre 2024  
**Versión**: 1.0  
**Próxima revisión**: Marzo 2025
