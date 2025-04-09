//Deshabilitar el clic derecho

document.addEventListener('contextmenu', function (e) {
    e.preventDefault();
});

//Detectar la apertura de las Herramientas de desarrollo
var checkStatus;

window.addEventListener('resize', function () {
    clearTimeout(checkStatus);
    checkStatus = setTimeout(function () {
        if ((window.outerHeight - window.innerHeight) > 100) {
            // Probablemente las herramientas de desarrollo est�n abiertas
            window.location.reload();
        }
    }, 500);
});

//ratar de deshabilitar combinaciones de teclas que suelen abrir las herramientas de desarrollo, como Ctrl+Shift+I o F12
document.onkeydown = function (e) {
    if (e.keyCode == 123 || (e.ctrlKey && e.shiftKey && e.keyCode == 73) || (e.ctrlKey && e.keyCode == 85)) {
        return false;
    }
};
