function mostrarAyudaModulo(moduleInfo) {
    Swal.fire({
        title: moduleInfo.title,
        html: `
            <div class="text-start">
                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="fas fa-info-circle"></i> Descripción
                    </h6>
                    <p>${moduleInfo.description}</p>
                </div>

                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="fas fa-tasks"></i> Funcionalidad
                    </h6>
                    <p>${moduleInfo.functionality}</p>
                </div>

                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="fas fa-sync-alt"></i> Etapa del Ciclo
                    </h6>
                    <p>${moduleInfo.stage}</p>
                </div>

                <div class="mb-4">
                    <h6 class="border-bottom pb-2">
                        <i class="fas fa-users-cog"></i> Roles
                    </h6>
                    <ul class="list-unstyled">
                        ${moduleInfo.roles.map(role => 
                            `<li><i class="fas fa-${role.icon}"></i> ${role.text}</li>`
                        ).join('')}
                    </ul>
                </div>

                <div>
                    <h6 class="border-bottom pb-2">
                        <i class="fas fa-check-double"></i> Orden en el Ciclo
                    </h6>
                    <p class="mb-0">
                        <span class="badge bg-warning">Paso ${moduleInfo.order.step}</span>
                        <span class="ms-2">${moduleInfo.order.description}</span>
                    </p>
                </div>
            </div>
        `,
        width: '400px',
        confirmButtonText: 'Entendido'
    });
}