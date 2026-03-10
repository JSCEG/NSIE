async function cargarColoresEnergeticos() {
    const res = await fetch('/SankeySener/GetEnergeticColor');
    const colors = await res.json();

    const energeticColors = {};

    colors.forEach(c => {
        energeticColors[c.nombre] = c.colorHex;
    });

    console.log("Colores cargados desde BD:", energeticColors);

    window.sankeyConfig = {

        energeticColors: energeticColors,

        // Colores para indicadores de eficiencia
        efficiencyColors: {
            "efficient": "#38a169",    // Verde para eficiente
            "warning": "#dd6b20",      // Naranja para alerta
            "critical": "#e53e3e"      // Rojo para crítico
        },
        // Configuración general del layout
        layoutConfig: {
            nodeAlign: 'left',
            nodeGap: 15,
            nodeWidth: 30, // Ancho por defecto
            layoutIterations: 0,
            curveness: 0.7
        },
        // Configuración de anchos personalizados por columna
        columnWidths: {
            0: 15,  // Columna 1: Origen (más estrecha)
            1: 35,  // Columna 2: Energía Primaria (más ancha)
            2: 30,  // Columna 3: Transformación (ancho medio)
            3: 25,  // Columna 4: Energía Secundaria (más estrecha)
            4: 1,  // Columna 5: Sectores (ancho medio)
            5: 20   // Columna 6: Destinos finales (más estrecha)
        },
        // Parámetros de comportamiento/legibilidad
        colorBy: 'child',               // 'child' | 'parent' | 'category'
        categoryColors: {               // usado cuando colorBy === 'category'
            'Fuentes Primarias': '#2E7D32',        // Verde oscuro para fuentes primarias
            'Fuentes Secundarias': '#1565C0',      // Azul para fuentes secundarias  
            'Energéticos Primarios': '#388E3C',    // Verde medio para energéticos primarios
            'Energéticos Secundarios': '#1976D2',  // Azul medio para energéticos secundarios
            'Transformación': '#F57C00',           // Naranja para transformación
            'Sectores de Consumo': '#D32F2F',      // Rojo para sectores de consumo
            'Pérdidas y Exportaciones': '#7B1FA2', // Púrpura para pérdidas y exportaciones
            'Otros': '#616161'                     // Gris para otros
        },
        // Configuración de patrones decal para accesibilidad (patrón uniforme de rayas diagonales)
        decalEnabled: true,             // Controla si se muestran los patrones decal
        linkMinValue: 0,                // no mostrar enlaces menores a este valor
        flowPolicy: 'bySign',           // 'bySign' | 'fixedParentToChild'
        normalizeBy: 'year',            // 'global' | 'year'
        curvenessAuto: false,           // ajusta la curvatura según distancia de columnas
        columnas: [
            {
                // Columna 1: Padres principales
                nombre: "Origen",
                mostrar: "Padre",
                filtroTipo: "Energía Primaria", // Solo hijos cuyo tipo sea "Energía Primaria"
                alineacionVertical: "abajo",
                nodos: [
                    // Puedes agregar más nodos padre aquí si lo deseas
                    { nombre: "SPACER_BIG_IEP_1", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 300 },
                    { nombre: "Importación EP", tipo: "Padre", visible: true, posicion: 0, y: 100, flow: 'source' },
                    { nombre: "V.I. y Dif. Est. EP", tipo: "Padre", visible: true, posicion: 3, flow: 'source' },
                    { nombre: "SPACER_BIG_IEP", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 2000 },
                    //{ nombre: "Variación de Inventarios EP (+)", tipo: "Padre", visible: true, posicion: 1, }, // Default para manejar valores +/- 
                    { nombre: "Producción", tipo: "Padre", visible: true, posicion: 2, y: 300, flow: 'source' },
                    //{ nombre: "Diferencia Estadística EP (+)", tipo: "Padre", visible: true, posicion: 3, flow: 'source' },
                ]
            },
            {
                // Columna 2: Hijos de Oferta Interna Bruta EP, solo los de tipo Energía Primaria
                nombre: "Energía Primaria",
                mostrar: "Hijo",
                filtroTipo: "Energía Primaria",
                padre: "Oferta Interna Bruta EP", // Solo hijos cuyo tipo sea "Energía Primaria"
                nodos: [
                    { nombre: "SPACER_BIG_EPP", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 2500 },
                    // Si dejas vacío, se autollenará con todos los hijos de Oferta Interna Bruta EP que sean tipo "Energía Primaria"
                    // O puedes forzar el orden así:
                    { nombre: "Petróleo crudo", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 0 },
                    { nombre: "Gas natural", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 1 },
                    { nombre: "Carbón mineral", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 2 },
                    { nombre: "Condensados", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 3 },
                    { nombre: "Energía Nuclear", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 4 },
                    { nombre: "Energía Hidráulica", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 5 },
                    { nombre: "Energía Geotérmica", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 6 },
                    { nombre: "Energía solar", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 7 },
                    { nombre: "Energía eólica", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 8 },
                    { nombre: "Bagazo de caña", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 9 },
                    { nombre: "Leña", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 10 },
                    { nombre: "Biogás", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 11 }
                    // ...
                ]
            },
            {
                // Columna 3: Salidas de Energía Primaria
                nombre: "Salidas de Energía Primaria",
                mostrar: "Padre",
                filtroTipo: "Energía Primaria",
                alineacionVertical: "abajo",
                nodos: [
                    // Un solo espaciador grande para empujar los nodos hacia abajo
                    { nombre: "SPACER_BIG", tipo: "Padre", visible: true, posicion: 0, depth: 2, esEspaciador: true, valorEspaciador: 15500 },
                    // Nodos reales posicionados después del espaciador
                    { nombre: "Exportación EP", tipo: "Padre", visible: true, posicion: 1, depth: 2, flow: 'sink' },
                    { nombre: "Energía No Aprovechada EP", tipo: "Padre", visible: true, posicion: 2, depth: 2, flow: 'sink' },
                    { nombre: "Consumo Propio del Sector EP", tipo: "Padre", visible: true, posicion: 3, depth: 2, flow: 'sink' },
                    { nombre: "Pérdidas técnicas por transporte, transmisión y distribución EP", tipo: "Padre", visible: true, posicion: 4, depth: 2, flow: 'sink' },
                    { nombre: "Variación de Inventarios EP (-)", tipo: "Padre", visible: true, posicion: 4, depth: 2, flow: 'sink' }, // Default para manejar valores +/- 
                    { nombre: "Diferencia Estadística EP (-)", tipo: "Padre", visible: true, posicion: 5, depth: 2, flow: 'sink' }, // N/A toso los años de EP en 0 pero queda la configuración lista 
                    // { nombre: "Pérdidas EP", tipo: "Padre", visible: true, posicion: 1, flow: 'sink' },
                ]
            },
            //Refinerías y Despuntadoras
            {
                // Columna 4: Entradas a Transformaciones
                nombre: "Transformaciones",
                mostrar: "Padre",
                filtroTipo: "Energía Primaria",
                alineacionVertical: "abajo",
                nodos: [
                    // Un solo espaciador grande para empujar los nodos hacia abajo
                    { nombre: "SPACER_BIG_3", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 7500 },
                    // Nodos reales posicionados después del espaciador
                    { nombre: "Coquizadoras y Hornos", tipo: "Padre", visible: true, posicion: 1, depth: 3 },
                    { nombre: "Plantas de Gas y Fraccionadoras", tipo: "Padre", visible: true, posicion: 2, depth: 3 },
                    { nombre: "Refinerías y Despuntadoras", tipo: "Padre", visible: true, posicion: 3, depth: 3 }

                ]
            },

            {
                // Columna 4: Padres principales
                nombre: "Origen Energía Secundaria",
                mostrar: "Padre",
                filtroTipo: "Energía Secundaria", // Solo hijos cuyo tipo sea "Energía Primaria"
                alineacionVertical: "abajo",
                nodos: [
                    { nombre: "SPACER_BIG_4", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 1000 },
                    // Puedes agregar más nodos padre aquí si lo deseas
                    { nombre: "V.I. y Dif. Est. ES", tipo: "Padre", visible: true, posicion: 1, flow: 'source' },
                    { nombre: "Importación ES", tipo: "Padre", visible: true, posicion: 0, flow: 'source' },
                    //{ nombre: "Variación de Inventarios ES (+)", tipo: "Padre", visible: true, posicion: 1, flow: 'source' }, // Default para manejar valores +/-         
                    //{ nombre: "Diferencia Estadística ES (+)", tipo: "Padre", visible: true, posicion: 3, flow: 'source' },
                ]
            },
            // Energéticos Secundarios
            {
                // Columna 5: Hijos de Oferta Interna Bruta EP, solo los de tipo Energía Secundaria
                nombre: "Energía Secundaria",
                mostrar: "Hijo",
                filtroTipo: "Energía Secundaria",
                padre: "Oferta Interna Bruta EP", // Solo hijos cuyo tipo sea "Energía Secundaria"
                nodos: [
                    { nombre: "SPACER_BIG_5", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 4000 },
                    // Puedes ajustar el orden y agregar más según tu catálogo
                    { nombre: "Coque de carbón", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 0, depth: 5 },
                    { nombre: "Coque de petróleo", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 1, depth: 5 },
                    { nombre: "Gas licuado de petróleo", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 2, depth: 5 },
                    { nombre: "Gasolinas y naftas", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 3, depth: 5 },
                    { nombre: "Querosenos", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 4, depth: 5 },
                    { nombre: "Diesel", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 5, depth: 5 },
                    { nombre: "Combustóleo", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 6, depth: 5 },
                    { nombre: "Otros energéticos", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 7, depth: 5 },
                    { nombre: "Gas natural seco", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 8, depth: 5 },
                    // { nombre: "Energía eléctrica", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 9, depth: 5 }
                    { nombre: "SPACER_BIG_5B", tipo: "Padre", visible: true, posicion: 9, esEspaciador: true, valorEspaciador: 3000 },
                    // // ...
                ]
            },
            //Un comentrio de Cambio de nombre de columna
            {
                // Columna : Entradas a Transformaciones Centrales Eléctricas
                nombre: "Transformaciones a Centrales Eléctricas",
                mostrar: "Padre",
                filtroTipo: "Todos",
                alineacionVertical: "abajo",
                nodos: [
                    // Un solo espaciador grande para empujar los nodos hacia abajo
                    // Nodos reales posicionados después del espaciador
                    { nombre: "SPACER_BIG_7", tipo: "Padre", visible: true, posicion: 1, esEspaciador: true, valorEspaciador: 11500 },
                    { nombre: "Centrales Eléctricas", tipo: "Padre", visible: true, posicion: 2, },


                ]
            },
            // Energía Eléctrica
            {
                // Columna 7: Energía Secundaria
                nombre: "Energía Secundaria",
                mostrar: "Hijo",
                filtroTipo: "Energía Secundaria",
                padre: "Oferta Interna Bruta EP", // Solo hijos cuyo tipo sea "Energía Secundaria"
                nodos: [
                    { nombre: "SPACER_BIG_E", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 11500 },
                    // Puedes ajustar el orden y agregar más según tu catálogo

                    //{ nombre: "Gas natural seco", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 8, depth: 5 },
                    { nombre: "Energía eléctrica", tipo: "Hijo", visible: true, padre: "Oferta Interna Bruta EP", posicion: 9, }
                    // // ...
                ]
            },

            {
                // Columna 6: Salidas de Energía Secundaria
                nombre: "Salidas de Energía Secundaria",
                mostrar: "Padre",
                filtroTipo: "Energía Secundaria",
                alineacionVertical: "abajo",
                nodos: [
                    // Un solo espaciador grande para empujar los nodos hacia abajo
                    { nombre: "SPACER_BIG_ES", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 16000 },
                    // Nodos reales posicionados después del espaciador
                    { nombre: "Exportación ES", tipo: "Padre", visible: true, posicion: 1, flow: 'sink' },
                    { nombre: "Energía No Aprovechada ES", tipo: "Padre", visible: true, posicion: 2, flow: 'sink' },
                    { nombre: "Consumo Propio del Sector ES", tipo: "Padre", visible: true, posicion: 3, flow: 'sink' },
                    { nombre: "Pérdidas técnicas por transporte, transmisión y distribución ES", tipo: "Padre", visible: true, posicion: 4, flow: 'sink' },
                    { nombre: "Variación de Inventarios ES (-)", tipo: "Padre", visible: true, posicion: 4, flow: 'sink' }, // Default para manejar valores +/- 
                    { nombre: "Diferencia Estadística ES (-)", tipo: "Padre", visible: true, posicion: 5, flow: 'sink' }, // Default para manejar valores +/- 
                    // { nombre: "Pérdidas ES", tipo: "Padre", visible: true, posicion: 1, flow: 'sink' },
                ]
            },
            {
                // Columna 5: Padres principales
                nombre: "Sectores de Consumo",
                mostrar: "Padre",
                filtroTipo: "Energía Secundaria", // Solo hijos cuyo tipo sea "Energía Primaria"
                alineacionVertical: "abajo",
                nodos: [
                    { nombre: "SPACER_BIG_sect", tipo: "Padre", visible: true, posicion: 0, esEspaciador: true, valorEspaciador: 3500 },
                    // Puedes agregar más nodos padre aquí si lo deseas
                    { nombre: "Industrial", tipo: "Padre", visible: true, posicion: 0, flow: 'sink' },
                    { nombre: "Transporte", tipo: "Padre", visible: true, posicion: 1, flow: 'sink' }, // Default para manejar valores +/-         
                    { nombre: "Agropecuario", tipo: "Padre", visible: true, posicion: 2, flow: 'sink' },
                    { nombre: "Comercial", tipo: "Padre", visible: true, posicion: 3, flow: 'sink' },
                    { nombre: "Público", tipo: "Padre", visible: true, posicion: 4, flow: 'sink' },
                    { nombre: "Residencial", tipo: "Padre", visible: true, posicion: 5, flow: 'sink' },

                    //{ nombre: "Petroquímica PEMEX", tipo: "Padre", visible: true, posicion: 6, flow: 'sink' },
                    //{ nombre: "Otras ramas económicas", tipo: "Padre", visible: true, posicion: 7, flow: 'sink' },

                    { nombre: "Consumo final no energético", tipo: "Padre", visible: true, posicion: 7, flow: 'sink' },

                ]
            },


        ],
        enlaces: [
            // Si quieres forzar enlaces explícitos, agrégalos aquí:
            // { source: "Oferta Interna Bruta EP", target: "Carbón mineral" },
            // { source: "Oferta Interna Bruta EP", target: "Petróleo crudo" },
            // ...
            // Si no, se generan automáticos por flujo de datos
        ],
        // Títulos/etiquetas de columnas (ajusta left en % y top en px)
        columnLabels: [
            // { id: 'col-ep', text: 'FEP', left: '5%', top: 80 },
            // { id: 'col-origen', text: 'Energéticos Primarios', left: '10%', top: 80 },
            // { id: 'col-origen', text: 'Energéticos Secundarios', left: '43%', top: 80 },
            // { id: 'col-salidas-ep', text: 'Transformaciones', left: '26%', top: 80 },
            // { id: 'col-transform', text: 'Transformaciones', left: '40%', top: 8 },
            // { id: 'col-origen-es', text: 'Origen ES', left: '52%', top: 8 },
            // { id: 'col-es', text: 'Energía Secundaria', left: '64%', top: 8 },
            // { id: 'col-ce', text: 'Centrales Eléctricas', left: '76%', top: 8 },
            // { id: 'col-ee', text: 'Energía Eléctrica', left: '84%', top: 8 },
            // { id: 'col-salidas-es', text: 'Salidas ES', left: '90%', top: 8 },
            // { id: 'col-sectores', text: 'Usos Finales', left: '76%', top: 80 },
        ],
    };

}