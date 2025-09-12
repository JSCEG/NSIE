// ===== SISTEMA DE SEGURIDAD SNIER =====
// Protección contra inspección de código y herramientas de desarrollo

(function() {
    'use strict';
    
    // Variables de control
    let devToolsOpen = false;
    let checkStatus;
    let warningCount = 0;
    const MAX_WARNINGS = 3;
    
    // Deshabilitar clic derecho
    document.addEventListener('contextmenu', function (e) {
        e.preventDefault();
        showSecurityWarning('Clic derecho deshabilitado por seguridad');
        return false;
    });

    // Detectar apertura de herramientas de desarrollo por cambio de tamaño
    window.addEventListener('resize', function () {
        clearTimeout(checkStatus);
        checkStatus = setTimeout(function () {
            const heightDiff = window.outerHeight - window.innerHeight;
            const widthDiff = window.outerWidth - window.innerWidth;
            
            if (heightDiff > 100 || widthDiff > 100) {
                handleDevToolsDetection();
            }
        }, 500);
    });

    // Detectar herramientas de desarrollo por timing (solo en producción)
    if (window.location.hostname !== 'localhost' && window.location.hostname !== '127.0.0.1') {
        setInterval(function() {
            const start = performance.now();
            debugger;
            const end = performance.now();
            
            if (end - start > 100) {
                handleDevToolsDetection();
            }
        }, 2000); // Intervalo más largo para mejor rendimiento
    }

    // Deshabilitar combinaciones de teclas peligrosas
    document.addEventListener('keydown', function (e) {
        // F12 - Herramientas de desarrollo
        if (e.keyCode === 123) {
            e.preventDefault();
            showSecurityWarning('Tecla F12 deshabilitada');
            return false;
        }
        
        // Ctrl+Shift+I - Inspeccionar elemento
        if (e.ctrlKey && e.shiftKey && e.keyCode === 73) {
            e.preventDefault();
            showSecurityWarning('Inspeccionar elemento deshabilitado');
            return false;
        }
        
        // Ctrl+U - Ver código fuente
        if (e.ctrlKey && e.keyCode === 85) {
            e.preventDefault();
            showSecurityWarning('Ver código fuente deshabilitado');
            return false;
        }
        
        // Ctrl+Shift+C - Selector de elementos
        if (e.ctrlKey && e.shiftKey && e.keyCode === 67) {
            e.preventDefault();
            showSecurityWarning('Selector de elementos deshabilitado');
            return false;
        }
        
        // Ctrl+Shift+J - Consola
        if (e.ctrlKey && e.shiftKey && e.keyCode === 74) {
            e.preventDefault();
            showSecurityWarning('Consola deshabilitada');
            return false;
        }
        
        // Ctrl+A - Seleccionar todo (en campos sensibles)
        if (e.ctrlKey && e.keyCode === 65 && isPasswordField(e.target)) {
            e.preventDefault();
            return false;
        }
        
        // Ctrl+C - Copiar (en campos de contraseña)
        if (e.ctrlKey && e.keyCode === 67 && isPasswordField(e.target)) {
            e.preventDefault();
            showSecurityWarning('Copiar contraseña no permitido');
            return false;
        }
    });

    // Detectar si es un campo de contraseña
    function isPasswordField(element) {
        return element && (element.type === 'password' || element.id === 'password' || element.name === 'Clave');
    }

    // Manejar detección de herramientas de desarrollo
    function handleDevToolsDetection() {
        // Solo aplicar en producción
        if (window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1') {
            console.warn('Herramientas de desarrollo detectadas - Modo desarrollo');
            return;
        }
        
        if (!devToolsOpen) {
            devToolsOpen = true;
            warningCount++;
            
            if (warningCount >= MAX_WARNINGS) {
                // Redirigir a página de seguridad después de múltiples intentos
                window.location.href = '/Acceso/ActividadSospechosa';
            } else {
                showSecurityWarning(`Herramientas de desarrollo detectadas. Advertencia ${warningCount}/${MAX_WARNINGS}`);
                // Recargar página como medida de seguridad (solo en producción)
                setTimeout(() => {
                    window.location.reload();
                }, 3000);
            }
        }
    }

    // Mostrar advertencia de seguridad
    function showSecurityWarning(message) {
        // Crear modal de advertencia si no existe
        if (!document.getElementById('securityWarningModal')) {
            const modal = document.createElement('div');
            modal.id = 'securityWarningModal';
            modal.innerHTML = `
                <div class="security-modal-overlay">
                    <div class="security-modal-content">
                        <div class="security-modal-icon">
                            <i class="bi bi-shield-exclamation"></i>
                        </div>
                        <h4 class="security-modal-title">Advertencia de Seguridad</h4>
                        <p id="securityMessage" class="security-modal-message"></p>
                        <button onclick="closeSecurityWarning()" class="btn btn-primary">
                            Entendido
                        </button>
                    </div>
                </div>
            `;
            document.body.appendChild(modal);
        }
        
        document.getElementById('securityMessage').textContent = message;
        document.getElementById('securityWarningModal').style.display = 'flex';
    }

    // Cerrar advertencia de seguridad
    window.closeSecurityWarning = function() {
        const modal = document.getElementById('securityWarningModal');
        if (modal) {
            modal.style.display = 'none';
        }
    };

    // Protección contra manipulación del DOM
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList') {
                // Detectar scripts inyectados
                mutation.addedNodes.forEach(function(node) {
                    if (node.tagName === 'SCRIPT' && node.src && !node.src.includes('cdn.') && !node.src.includes('localhost') && !node.src.includes('bpcontent.cloud') && !node.src.includes('unpkg.com')) {
                        node.remove();
                        showSecurityWarning('Script malicioso detectado y removido');
                    }
                });
            }
        });
    });

    observer.observe(document.body, {
        childList: true,
        subtree: true
    });

    // Protección contra console.log y debugging
    /* if (typeof console !== 'undefined') {
        console.log = function() {};
        console.warn = function() {};
        console.error = function() {};
        console.info = function() {};
        console.debug = function() {};
        console.trace = function() {};
    } */

    // Detectar si la página está siendo ejecutada en un iframe (clickjacking)
    if (window.top !== window.self) {
        window.top.location = window.self.location;
    }

    // Limpiar datos sensibles al salir de la página
    window.addEventListener('beforeunload', function() {
        // Limpiar campos de contraseña
        const passwordFields = document.querySelectorAll('input[type="password"]');
        passwordFields.forEach(field => field.value = '');
        
        // Limpiar localStorage y sessionStorage si contienen datos sensibles
        try {
            localStorage.removeItem('tempLoginData');
            sessionStorage.removeItem('tempLoginData');
        } catch(e) {
            // Ignorar errores de storage
        }
    });

    // Protección adicional: detectar herramientas de desarrollo por console
    let devtools = {
        open: false,
        orientation: null
    };
    
    setInterval(function() {
        if (window.outerHeight - window.innerHeight > 200 || window.outerWidth - window.innerWidth > 200) {
            if (!devtools.open) {
                devtools.open = true;
                handleDevToolsDetection();
            }
        } else {
            devtools.open = false;
        }
    }, 500);

    console.log('%cALTO!', 'color: red; font-size: 50px; font-weight: bold;');
    console.log('%cEsta es una función del navegador destinada a desarrolladores. Si alguien te dijo que copies y pegues algo aquí para habilitar una función o "hackear" la cuenta de alguien, es una estafa y le dará acceso a tu cuenta.', 'color: red; font-size: 16px;');

})();
