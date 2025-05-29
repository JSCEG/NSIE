document.addEventListener('DOMContentLoaded', function () {
    const sectionsWrapper = document.querySelector('.sections-wrapper');
    const prevButton = document.querySelector('.prev-section');
    const nextButton = document.querySelector('.next-section');
    const sectionPills = document.querySelectorAll('.section-pill');
    const sectionContents = document.querySelectorAll('.section-content');
    const sectionTitle = document.querySelector('.section-title');

    function centerSelectedPill(pill) {
        // Obtener las dimensiones
        const pillRect = pill.getBoundingClientRect();
        const wrapperRect = sectionsWrapper.getBoundingClientRect();

        // Calcular la posición central
        const centerPosition = pillRect.left - wrapperRect.left -
            (wrapperRect.width / 2) + (pillRect.width / 2);

        // Animar el scroll
        sectionsWrapper.scrollTo({
            left: sectionsWrapper.scrollLeft + centerPosition,
            behavior: 'smooth'
        });
    }

    function activateSection(pill) {
        // Remover active de todas las pills y contenidos
        sectionPills.forEach(p => {
            p.classList.remove('active');
            p.style.transform = 'scale(1)';
        });
        sectionContents.forEach(c => c.classList.remove('active'));

        // Activar la pill seleccionada y su contenido
        pill.classList.add('active');
        pill.style.transform = 'scale(1.1)'; // Efecto de zoom

        const sectionId = pill.dataset.section;
        const content = document.querySelector(`.section-content[data-section="${sectionId}"]`);
        if (content) {
            content.classList.add('active');
        }

        // Actualizar título con animación
        if (sectionTitle) {
            sectionTitle.style.opacity = '0';
            sectionTitle.style.transform = 'translateY(-10px)';

            setTimeout(() => {
                sectionTitle.textContent = pill.textContent;
                sectionTitle.style.opacity = '1';
                sectionTitle.style.transform = 'translateY(0)';
            }, 200);
        }

        // Centrar la pill seleccionada
        centerSelectedPill(pill);
    }

    // Event Listeners para las pills
    sectionPills.forEach(pill => {
        pill.addEventListener('click', () => {
            activateSection(pill);
        });
    });

    // Navegación con botones
    prevButton.addEventListener('click', () => {
        sectionsWrapper.scrollBy({
            left: -300,
            behavior: 'smooth'
        });
    });

    nextButton.addEventListener('click', () => {
        sectionsWrapper.scrollBy({
            left: 300,
            behavior: 'smooth'
        });
    });

    // Activar primera sección por defecto
    if (sectionPills.length > 0) {
        activateSection(sectionPills[0]);
    }
});