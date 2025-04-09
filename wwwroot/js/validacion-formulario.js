document.addEventListener("DOMContentLoaded", function () {
    const form = document.querySelector("form"); // Selecciona tu formulario
    if (!form) return;

    form.addEventListener("submit", function (event) {
        const inputs = form.querySelectorAll("input, textarea"); // Selecciona todos los campos de entrada
        const regex = /(--|;|'|"|\b(OR|AND)\b\s*\d+|=\s*\d+|UNION\s+SELECT|DROP\s+TABLE|INSERT\s+INTO|DELETE\s+FROM|UPDATE\s+\w+|<.*?>|1\s*=\s*1|script\s*:|javascript\s*:)/i;
        let isValid = true;

        inputs.forEach((input) => {
            if (regex.test(input.value)) {
                // Redirige al usuario a la página de error
                window.location.href = "/Acceso/ActividadSospechosa";
                isValid = false;
            }
        });

        if (!isValid) {
            event.preventDefault(); // Detiene el envío del formulario
        }
    });
});

