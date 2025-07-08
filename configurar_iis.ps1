# Script para configurar SNIEr en IIS
# Ejecutar como Administrador

Write-Host "=== Configuración de SNIEr en IIS ===" -ForegroundColor Green

# 1. Habilitar IIS y ASP.NET Core Hosting Bundle (si no está instalado)
Write-Host "Verificando IIS y ASP.NET Core..." -ForegroundColor Yellow

# 2. Crear el sitio web en IIS
$siteName = "SNIEr"
$sitePath = "C:\inetpub\wwwroot\Snier"
$port = 8080

Write-Host "Creando sitio web: $siteName" -ForegroundColor Yellow

# Verificar si el sitio ya existe
$existingSite = Get-IISSite -Name $siteName -ErrorAction SilentlyContinue
if ($existingSite) {
    Write-Host "El sitio '$siteName' ya existe. Eliminando..." -ForegroundColor Yellow
    Remove-IISSite -Name $siteName -Confirm:$false
}

# Crear el nuevo sitio
New-IISSite -Name $siteName -PhysicalPath $sitePath -Port $port

Write-Host "Sitio creado en puerto $port" -ForegroundColor Green

# 3. Configurar el Application Pool
$poolName = "SNIEr_Pool"
Write-Host "Configurando Application Pool: $poolName" -ForegroundColor Yellow

# Verificar si el pool ya existe
$existingPool = Get-IISAppPool -Name $poolName -ErrorAction SilentlyContinue
if ($existingPool) {
    Write-Host "Application Pool '$poolName' ya existe. Eliminando..." -ForegroundColor Yellow
    Remove-IISAppPool -Name $poolName -Confirm:$false
}

# Crear el nuevo Application Pool
New-IISAppPool -Name $poolName
Set-IISAppPool -Name $poolName -ProcessModel.IdentityType ApplicationPoolIdentity
Set-IISAppPool -Name $poolName -ManagedRuntimeVersion ""  # No Managed Code para .NET Core

# Asignar el pool al sitio
Set-IISSite -Name $siteName -ApplicationPool $poolName

Write-Host "Application Pool configurado" -ForegroundColor Green

# 4. Configurar permisos
Write-Host "Configurando permisos..." -ForegroundColor Yellow

$acl = Get-Acl $sitePath
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS_IUSRS", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($accessRule)
Set-Acl $sitePath $acl

Write-Host "Permisos configurados" -ForegroundColor Green

# 5. Verificar el estado
Write-Host "=== Estado Final ===" -ForegroundColor Green
Write-Host "Sitio: $siteName" -ForegroundColor White
Write-Host "Ruta: $sitePath" -ForegroundColor White
Write-Host "Puerto: $port" -ForegroundColor White
Write-Host "URL: http://localhost:$port" -ForegroundColor Cyan

Write-Host "`n=== Comandos útiles ===" -ForegroundColor Green
Write-Host "Ver sitios: Get-IISSite" -ForegroundColor White
Write-Host "Ver pools: Get-IISAppPool" -ForegroundColor White
Write-Host "Iniciar sitio: Start-IISSite -Name '$siteName'" -ForegroundColor White
Write-Host "Detener sitio: Stop-IISSite -Name '$siteName'" -ForegroundColor White

Write-Host "`n¡Configuración completada!" -ForegroundColor Green
Write-Host "Accede a tu aplicación en: http://localhost:$port" -ForegroundColor Cyan
