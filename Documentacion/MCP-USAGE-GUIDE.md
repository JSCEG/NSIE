# 🔧 Guía de uso de MCP en tu proyecto SNIER

## 1. Comandos de GitHub Copilot Chat con MCP

### Análisis de proyecto:
- `@workspace Explica la arquitectura de este proyecto`
- `@workspace ¿Qué patrones de diseño se están usando?`
- `@workspace Lista todos los controladores y sus acciones`
- `@workspace Analiza los modelos de datos principales`

### Mejoras de código:
- `@workspace Sugiere optimizaciones para el rendimiento`
- `@workspace Revisa la seguridad del código`
- `@workspace ¿Hay código duplicado que pueda refactorizar?`
- `@workspace Sugiere mejores prácticas para ASP.NET Core`

### Depuración y solución de problemas:
- `@workspace Ayúdame a depurar este error [describe el error]`
- `@workspace ¿Por qué no funciona esta funcionalidad?`
- `@workspace Revisa la configuración de la base de datos`

## 2. Funcionalidades específicas de MCP

### Exploración de archivos:
El servidor MCP filesystem puede:
- Leer cualquier archivo del proyecto
- Analizar la estructura de directorios
- Buscar patrones en múltiples archivos
- Comparar archivos similares

### Contexto mejorado:
- Comprende las relaciones entre controladores y vistas
- Entiende el flujo de datos en tu aplicación
- Analiza configuraciones y dependencias
- Sugiere mejoras basadas en el código completo

## 3. Casos de uso prácticos

### Para tu proyecto SNIER específicamente:

1. **Análisis de controladores energéticos**:
   - `@workspace Analiza los controladores de energía (Electricidad.cs, EnergiasLimpias.cs)`
   - `@workspace ¿Cómo puedo optimizar las consultas en estos controladores?`

2. **Mejora de vistas**:
   - `@workspace Revisa las vistas de mapas y sugiere mejoras de UX`
   - `@workspace ¿Cómo puedo hacer más responsivas las vistas de Hidrocarburos?`

3. **Optimización de servicios**:
   - `@workspace Analiza el RepositorioChat.cs y sugiere mejoras`
   - `@workspace ¿Cómo puedo mejorar la integración con OpenAI?`

4. **Seguridad y validación**:
   - `@workspace Revisa los filtros de autorización`
   - `@workspace ¿Hay vulnerabilidades en las validaciones de entrada?`

## 4. Tips para mejores resultados

### Sé específico:
❌ "Mejora mi código"
✅ "Analiza el AccesoController.cs y sugiere mejoras en la validación de usuarios"

### Proporciona contexto:
❌ "Este código no funciona"
✅ "El método Login en AccesoController.cs devuelve un error 500. ¿Puedes revisar la lógica de autenticación?"

### Usa comandos paso a paso:
1. Primero: `@workspace Analiza este archivo [nombre]`
2. Luego: `@workspace ¿Qué problemas encuentras?`
3. Finalmente: `@workspace Dame una solución detallada`

## 5. Comandos de terminal útiles

### Verificar MCP:
```powershell
npx @modelcontextprotocol/server-filesystem --version
```

### Reinstalar si hay problemas:
```powershell
npm uninstall -g @modelcontextprotocol/server-filesystem
npm install -g @modelcontextprotocol/server-filesystem
```

### Ver logs de MCP:
```powershell
# Los logs aparecen en la consola de VS Code
```

## 6. Troubleshooting

### Si MCP no funciona:
1. Reinicia VS Code completamente
2. Verifica que Node.js esté en el PATH
3. Reinstala los servidores MCP
4. Revisa la configuración en settings.json

### Si Copilot no ve el contexto:
1. Asegúrate de usar `@workspace` en tus comandos
2. Verifica que la extensión Copilot MCP esté activa
3. Reinicias el workspace de VS Code
