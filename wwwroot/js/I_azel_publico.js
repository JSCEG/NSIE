// --- Botón personalizado para centrar el mapa (Home) ---
function agregarBotonCentrarMapa() {
    if (!window.L || !mapas || !mapas[0]) return;
    var homeControl = L.Control.extend({
        options: { position: 'topright' },
        onAdd: function (map) {
            var container = L.DomUtil.create('div', 'leaflet-control leaflet-bar leaflet-control-custom');
            container.title = 'Centrar mapa';
            container.innerHTML = '<svg width="22" height="22" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="12" x2="16" y2="12"/></svg>';
            container.style.cursor = 'pointer';
            container.onclick = function () {
                // Ajusta los valores a la vista inicial de tu mapa
                map.setView([23.6345, -102.5528], 5); // México
            };
            return container;
        }
    });
    mapas[0].addControl(new homeControl());
}
//Campos Visbles de los popup*@

//



//Busqueda
var availableTerms = [];//Variable global para almacenar los terminos de búqueda Sugerencia de Terminos
var datosExpendios = []; // variable global para almacenar todos los expendios de Petrolíferos
var camposVisiblesGlobal = [];
var datosExpendiosAcumulados = [];
let tpet = 0;
let tglp = 0;
let tgn = 0;
let te = 0;
var tpetg = [];
var tglpg = [];
var tgng = [];
let teg = [];


// Crea los Iconos y Define su tamaño
var iconoBase = L.Icon.extend({
    options: {
        iconSize: [36, 36],
        iconAnchor: [18, 16],
        popupAnchor: [0, -26]
    }
});

// Asignación de Iconos
//var iconoSolicitudes = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/Solicitudes.png' });
//iconoAprobado = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/Aprobado.png' });
//iconoNoaprobado = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/NoAprobado.png' });


var currentMarker = null; // Referencia al marcador actual
// var seleccionado = 'estado'; // Estado inicial
var municipiosFiltrados = null;



//Colores
var initialStyle = {
    color: '#222', // Color de línea (negro/gris oscuro)
    fillColor: '#222', // Color de relleno (gris oscuro, opacidad baja)
    fillOpacity: 0.01, // Opacidad del relleno
    weight: 3 // Ancho de la línea
};

// Estilo para el hover
var highlightStyle = {
    color: '#555', // Gris medio para hover
    fillColor: '#555', // Gris medio para hover
    fillOpacity: 0.01,
    weight: 3
};

// Se eliminó el marcador de vista de calle al hacer clic en el mapa por solicitud del usuario.

// Capa de estados
var estadosLayer = L.geoJSON(estados, {
    style: initialStyle, // Aplicar estilo inicial
    onEachFeature: function (feature, layer) {
        layer.bindTooltip('<div class="custom-tooltip">' + feature.properties.NOMGEO + '</div>');
        layer.on('click', function (e) {
            cargarMunicipios(feature.properties.CVE_ENT);
            mapas[0].fitBounds(layer.getBounds()); // Centra el mapa en el estado
        });
        // Efecto de hover
        layer.on('mouseover', function (e) {
            layer.setStyle(highlightStyle);
        });
        layer.on('mouseout', function (e) {
            estadosLayer.resetStyle(layer);
        });
    }
}).addTo(mapas[0]);

// Capa de municipios (inicialmente vacía)
var municipiosLayer = L.geoJSON(null, {
    style: initialStyle, // Aplicar estilo inicial
    onEachFeature: onEachMunicipio
}).addTo(mapas[0]);

function onEachMunicipio(feature, layer) {
    layer.bindTooltip('<div class="custom-tooltip">' + feature.properties.NOM_MUN + ', ' + feature.properties.NOMGEO + '</div>');
    // Efecto de hover
    layer.on('mouseover', function (e) {
        layer.setStyle(highlightStyle);
    });
    layer.on('mouseout', function (e) {
        municipiosLayer.resetStyle(layer);
    });

}

// Función para cargar los municipios correspondientes a un estado
function cargarMunicipios(cveEnt) {
    municipiosLayer.clearLayers();

    if (currentMarker) {
        mapas[0].removeLayer(currentMarker);
        currentMarker = null;
    }

    municipiosFiltrados = {
        type: "FeatureCollection",
        features: municipios_mapa.features.filter(function (feature) {
            return feature.properties.CVE_ENT === cveEnt;
        })
    };

    municipiosLayer.addData(municipiosFiltrados);
}

//Búsquedas
//Busquedas
var lastSearchedEstadoLayer = null; // para almacenar la última entidad federativa buscada
var lastSearchedMunicipioLayer = null; // para almacenar el último municipio buscado


function buscarGeneral() {
    var terminoBuscado = document.getElementById('busquedaGeneralInput').value.trim();

    if (!terminoBuscado) {
        alert("Por favor, introduce un término de búsqueda.");
        return;  // Termina la ejecución de la función si el campo está vacío
    }

    // Intenta buscar por número de permiso primero
    var encontrado = false;
    for (var i = 0; i < datosExpendios.length; i++) {
        var expendio = datosExpendios[i];
        if (expendio.numeroPermiso === terminoBuscado) {
            var lat = expendio.latitudGeo;
            var lon = expendio.longitudGeo;
            mapas[0].setView([lat, lon], 17);
            encontrado = true;
            break;
        }
    }

    // Si no se encontró por número de permiso, busca por entidad federativa
    if (!encontrado) {
        estadosLayer.eachLayer(function (layer) {
            if (layer.feature.properties.NOMGEO === terminoBuscado) {
                mapas[0].fitBounds(layer.getBounds());

                // Si ya había una entidad federativa buscada anteriormente, restablecemos su estilo
                if (lastSearchedEstadoLayer) {
                    estadosLayer.resetStyle(lastSearchedEstadoLayer);
                }

                // Resalta la entidad federativa encontrada
                layer.setStyle({
                    color: '#FF0000',
                    fillColor: '#FF0000',
                    fillOpacity: 0.5
                });

                lastSearchedEstadoLayer = layer;

                // Reiniciar el estilo de la entidad después de 5 segundos
                setTimeout(function () {
                    estadosLayer.resetStyle(lastSearchedEstadoLayer);
                    lastSearchedEstadoLayer = null;
                }, 5000);

                encontrado = true;
            }
        });
    }

    // Si aún no se encontró, busca por municipio en la fuente de datos completa
    if (!encontrado) {
        // Divide el término de búsqueda en municipio y estado
        var terminos = terminoBuscado.split(',');
        var buscadoMunicipio = terminos[0].trim();
        var buscadoEstado = terminos.length > 1 ? terminos[1].trim() : '';


        for (var i = 0; i < municipios_mapa.features.length; i++) {
            var municipio = municipios_mapa.features[i];
            var nombreMunicipio = municipio.properties.NOM_MUN;
            var nombreEstado = municipio.properties.NOMGEO;

            // Comprueba si el nombre del municipio y del estado coinciden con el término de búsqueda
            if (nombreMunicipio === buscadoMunicipio && (buscadoEstado === '' || nombreEstado === buscadoEstado)) {
                var bounds = L.geoJSON(municipio).getBounds();
                mapas[0].fitBounds(bounds);

                // Si ya había un municipio buscado anteriormente, lo elimina
                if (lastSearchedMunicipioLayer) {
                    mapas[0].removeLayer(lastSearchedMunicipioLayer);
                }

                // Agrega el municipio encontrado al mapa y lo resalta
                lastSearchedMunicipioLayer = L.geoJSON(municipio, {
                    style: {
                        color: '#FF0000',
                        fillColor: '#FF0000',
                        fillOpacity: 0.5
                    }
                }).addTo(mapas[0]);

                // Reiniciar el estilo y eliminar el municipio después de 5 segundos
                setTimeout(function () {
                    mapas[0].removeLayer(lastSearchedMunicipioLayer);
                    lastSearchedMunicipioLayer = null;
                }, 5000);

                encontrado = true;
                break;
            }
        }
    }


    if (!encontrado) {
        alert("Término no encontrado.");
    }




}

// Cuando se dibuja una línea, calcula la distancia
mapas[0].on('draw:created', function (e) {
    var type = e.layerType,
        layer = e.layer;

    if (type === 'polyline') {
        var latlngs = layer.getLatLngs();
        var distance = 0;
        for (var i = 1; i < latlngs.length; i++) {
            distance += latlngs[i - 1].distanceTo(latlngs[i]);
        }
        // Convertir la distancia a km y redondear a 2 decimales
        distance = Math.round((distance / 1000) * 100) / 100;
        // Crear un popup con la distancia
        layer.bindPopup('Distancia: ' + distance + ' km').openPopup();
        // Añadir la línea al mapa
        layer.addTo(mapas[0]);
    }
});




//Funciones de los botones y del Mapa*@



function limpiarMarcadores() {
    // Limpiar todas las capas de marcadores y círculos
    mapas[0].eachLayer(function (layer) {
        if (layer instanceof L.Marker || layer instanceof L.MarkerClusterGroup || layer instanceof L.Circle) {
            mapas[0].removeLayer(layer);
        }
    });


}
function handleNull(value) {
    return value ? value : "S/D-Sin Dato";
}
Highcharts.setOptions({
    lang: {
        decimalPoint: '.',
        thousandsSep: ','
    }
});


//Electricidad
function CargaElectricidad() {
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) { // Asegúrate de que la propiedad existe
            availableTerms.push(layer.feature.properties.NOMGEO);
        }
    });

    for (var i = 0; i < municipios_mapa.features.length; i++) {
        var municipio = municipios_mapa.features[i].properties.NOM_MUN;
        var estado = municipios_mapa.features[i].properties.NOMGEO; // Asumiendo que esta es la propiedad correcta para el estado

        // Asegúrate de que ambas propiedades existen antes de concatenar
        if (municipio && estado) {
            var nombreCompleto = municipio + ", " + estado;
            availableTerms.push(nombreCompleto);
        }
    }

    // Primera llamada AJAX como una promesa
    function cargarCamposVisibles() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesElectricidad_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);  // Resuelve la promesa con los campos visibles
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);  // Rechaza la promesa si hay un error
                }
            });
        });
    }
    //No hay ductos


    cargarCamposVisibles().then(camposVisibles => {
        // Limpiar marcadores existentes
        limpiarMarcadores()
        // Carga los Marcadores y Ductos

        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
            type: 'GET',
            // data: JSON.stringify(datos_mun),
            contentType: 'application/json',
            success: function (response) {
                console.log("Estos son los Expendios Autorizados de Electricidad:", response); // ver la respuesta en consola
                // Filtra la respuesta para incluir solo elementos con fuenteEnergia igual a 'Convencional'
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "Solar Fotovoltaica";
                });

                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>" +
                        ".popup-content {" +
                        "    width: 280px;" +
                        "    max-height: 180px;" +
                        "    overflow-y: auto;" +
                        "    padding: 10px;" +
                        "}" +
                        "h2, h3, h4, p, li {" +
                        "    margin: 0 0 10px 0;" +
                        "}" +
                        "ul {" +
                        "    padding-left: 20px;" +
                        "}" +
                        "img {" +
                        "    vertical-align: middle;" +
                        "    margin-right: 10px;" +
                        "}" +
                        "</style>";

                    contenido += "<div class='popup-content'>";

                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        var imgSrc; // La URL de la imagen que se mostrará en el pop-up
                        switch (coordenada.tipoPermiso) {
                            case "vacío":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png';
                                break;
                            /* case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png';
                                break;
                            case "Expendio al Público de Gas Licuado de Petróleo mediante Estación de Servicio con fin Específico":
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa.png';
                                break; */
                            default:
                                imgSrc = 'https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png'; // Ícono por defecto si no hay coincidencia
                                break;
                        }
                        contenido += "<h2 class='subtitulo'><img src='" + imgSrc + "' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }

                    contenido += "<ul>";

                    if (camposVisiblesGlobal.includes("EfId")) {//NO TENEMOS EL NOMBRE DE LA EF EN CAMPOS VISIBLES SOLO EL ID LO CRUZO EN LA CONSULTA DEL REPOSITORIO
                        contenido += "<li><strong>Clave y Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>NumeroPermiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Clave y Municipio:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Numero de Expediente:</strong> " + handleNull(coordenada.numeroDeExpediente) + "</li>";
                    }

                    //if (camposVisiblesGlobal.includes("EfId")) {
                    //  contenido += "<li><strong>EfId:</strong> " + handleNull(coordenada.efId) + "</li>";
                    //}

                    /* if (camposVisiblesGlobal.includes("Expediente")) {
                            contenido += "<li><strong>Expediente:</strong> " + handleNull(coordenada.expediente) + "</li>";
                        }*/


                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>RazonSocial:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>LatitudGeo:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>LongitudGeo:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("RFC")) {
                        contenido += "<li><strong>Rfc:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>Fecha de Recepción:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>Estatus de Instalación:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InicioVigencia")) {
                        contenido += "<li><strong>Inicio de Vigencia:</strong> " + handleNull(coordenada.inicioVigencia) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("InicioOperaciones")) {
                        contenido += "<li><strong>Inicio  de Operaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("CapacidadAutorizadaMW")) {
                        contenido += "<li><strong>Capacidad Autorizada  en MW:</strong> " + handleNull(coordenada.capacidadAutorizadaMW) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Generación_estimada_anual")) {
                        contenido += "<li><strong>Generación Estimada Anual:</strong> " + handleNull(coordenada.generacion_estimada_anual) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("Inversion_estimada_mdls")) {
                        contenido += "<li><strong>Inversión Estimada en mdls:</strong> " + handleNull(coordenada.inversion_estimada_mdls) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Energetico_primario")) {
                        contenido += "<li><strong>Energético Primario:</strong> " + handleNull(coordenada.energetico_primario) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Actividad_economica")) {
                        contenido += "<li><strong>Actividad Económica:</strong> " + handleNull(coordenada.actividad_economica) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Tecnología")) {
                        contenido += "<li><strong>Tecnología:</strong> " + handleNull(coordenada.tecnologia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EmpresaLíder")) {
                        contenido += "<li><strong>Empresa Líder:</strong> " + handleNull(coordenada.empresaLider) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("PaísDeOrigen")) {
                        contenido += "<li><strong>País de Origen:</strong> " + handleNull(coordenada.paisDeOrigen) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Subasta")) {
                        contenido += "<li><strong>Subasta:</strong> " + handleNull(coordenada.subasta) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("Planta")) {
                        contenido += "<li><strong>Planta:</strong> " + handleNull(coordenada.planta) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("Combustible")) {
                        contenido += "<li><strong>Combustible:</strong> " + handleNull(coordenada.combustible) + "</li>";
                    }

                    if (camposVisiblesGlobal.includes("FuenteEnergía")) {
                        contenido += "<li><strong>Fuente de Energía:</strong> " + handleNull(coordenada.fuenteEnergia) + "</li>";
                    }


                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
                    }
                    /*
                        if (camposVisiblesGlobal.includes("SiglasTipo")) {
                            contenido += "<li><strong>SiglasTipo:</strong> " + handleNull(coordenada.siglasTipo) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Otorgamiento")) {
                            contenido += "<li><strong>Otorgamiento:</strong> " + handleNull(coordenada.otorgamiento) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("FechaAcuse")) {
                            contenido += "<li><strong>FechaAcuse:</strong> " + handleNull(coordenada.fechaAcuse) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("EstatusSAT")) {
                            contenido += "<li><strong>EstatusSAT:</strong> " + handleNull(coordenada.estatusSAT) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("SuspensionInicio")) {
                            contenido += "<li><strong>SuspensionInicio:</strong> " + handleNull(coordenada.suspensionInicio) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("SuspensionFin")) {
                            contenido += "<li><strong>SuspensionFin:</strong> " + handleNull(coordenada.suspensionFin) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("NumeroTanques")) {
                            contenido += "<li><strong>NumeroTanques:</strong> " + handleNull(coordenada.numeroTanques) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("CapacidadLitros")) {
                            contenido += "<li><strong>CapacidadLitros:</strong> " + handleNull(coordenada.capacidadLitros) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("NumeroUnidades")) {
                            contenido += "<li><strong>NumeroUnidades:</strong> " + handleNull(coordenada.numeroUnidades) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("NumeroDeCentralesDeGuarda")) {
                            contenido += "<li><strong>NumeroDeCentralesDeGuarda:</strong> " + handleNull(coordenada.numeroDeCentralesDeGuarda) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("DomicilioDeGuarda")) {
                            contenido += "<li><strong>DomicilioDeGuarda:</strong> " + handleNull(coordenada.domicilioDeGuarda) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("SuministroRecepcion")) {
                            contenido += "<li><strong>SuministroRecepcion:</strong> " + handleNull(coordenada.suministroRecepcion) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("PermisoSuministro")) {
                            contenido += "<li><strong>PermisoSuministro:</strong> " + handleNull(coordenada.permisoSuministro) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("CompartenTanques")) {
                            contenido += "<li><strong>CompartenTanques:</strong> " + handleNull(coordenada.compartenTanques) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Modificacion")) {
                            contenido += "<li><strong>Modificacion:</strong> " + handleNull(coordenada.modificacion) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Asociacion")) {
                            contenido += "<li><strong>Asociacion:</strong> " + handleNull(coordenada.asociacion) + "</li>";
                        }

                        if (camposVisiblesGlobal.includes("Gie")) {
                            contenido += "<li><strong>Gie:</strong> " + handleNull(coordenada.gie) + "</li>";
                        }
                        */
                    contenido += "</ul>";

                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }

                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";


                    contenido += "</div>";

                    return contenido;
                }
                //Mapa de Resultados/////////////////////////////////////////////////////////////////////
                // Crea un grupo de marcadores
                var markers = L.markerClusterGroup();
                // Crea los iconos
                var iconoBase = L.Icon.extend({
                    options: {
                        iconSize: [36, 36],
                        iconAnchor: [12, 18],
                        popupAnchor: [0, -26]
                    }
                });

                var iconoExpendio = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png' });
                /*  var iconoAlmacenamiento = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_alma.png' });
                    var iconoDistribucion = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/glpmapa_dist.png' });*/
                // Agrega los marcadores para las coordenadas del mapa actual
                for (var j = 0; j < response.length; j++) {
                    var coordenada = response[j];
                    //Determina el icono a Usar
                    // Determinar el ícono basado en tipoPermiso
                    var iconoActual;



                    //var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                    //var contenidoPopup = generarContenidoPopup(coordenada);
                    //marker.bindPopup(contenidoPopup);
                    //markers.addLayer(marker);
                    if (coordenada.clasificacion === "Solar Fotovoltaica") {
                        var iconoActual;
                        // Aquí iría tu código para determinar el icono actual basado en tipoPermiso
                        //
                        switch (coordenada.tipoPermiso) {
                            /*  case "SD":
                                iconoActual = iconoAlmacenamiento;
                                break;
                            case "Distribución de Gas Licuado de Petróleo mediante Planta de Distribución":
                                iconoActual = iconoDistribucion;
                                break; */
                            case "vacío":
                                iconoActual = iconoExpendio;
                                break;
                            default:
                                iconoActual = iconoExpendio; // Ícono por defecto si no hay coincidencia
                                break;
                        }


                        var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoActual });
                        var contenidoPopup = generarContenidoPopup(coordenada);
                        marker.bindPopup(contenidoPopup);

                        markers.addLayer(marker);
                    }
                }

                mapas[0].addLayer(markers);

                //******Tarjetas Totales******///

                // Contar el número total de elementos en el array 'datosExpendios'
                var totalElementos = datosExpendios.length;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de permisos (Solar Fotovoltaica) formateado:", totalFormateado);

                // Actualizar la tarjeta de permisos
                const tarjetaTitulo = document.getElementById('titulo-permiso');
                const tarjetaTotal = document.getElementById('total-permisos');
                const tarjetaIcono = document.getElementById('icono-permiso');

                if (tarjetaTitulo) tarjetaTitulo.textContent = 'Solar Fotovoltaica';
                if (tarjetaIcono) tarjetaIcono.src = 'https://cdn.sassoapps.com/img_snier/vistas/fotovoltaica.png';
                if (tarjetaTotal) tarjetaTotal.textContent = "Proyectos Autorizados: " + totalFormateado;


                //******Fin Tarjetas Totales******///

                // Función para procesar los datos
                function processData(response, key) {
                    var counts = {};
                    response.forEach(function (item) {
                        // Elimina los números al principio si el key es 'efId'
                        if (key === 'efId') {
                            item[key] = item[key].replace(/^\d+\s/, '');
                        }

                        if (!counts[item[key]]) {
                            counts[item[key]] = 0;
                        }
                        counts[item[key]]++;
                    });

                    var categories = [];
                    var data = [];
                    for (var entidad in counts) {
                        categories.push(entidad);
                        data.push(counts[entidad]);
                    }

                    return { categories, data };
                }
                var convencional = processData(datosExpendios, 'efId');
                //Grafico de Columnas
                // Paso 1: Procesa la respuesta
                var counts = {};
                datosExpendios.forEach(function (coordenada) {
                    if (!counts[coordenada.efId]) {
                        counts[coordenada.efId] = 0;
                    }
                    counts[coordenada.efId]++;
                });

                // Paso 2: Extrae categorías y datos
                var categories = [];
                var dataPermisos = [];
                for (var entidad in counts) {
                    categories.push(entidad);
                    dataPermisos.push(counts[entidad]);
                }

                // Paso 3: Configura las opciones del gráfico
                var options = {
                    chart: {
                        type: 'column',
                        backgroundColor: 'transparent',
                        style: {
                            fontFamily: 'inherit'
                        },
                        borderRadius: 10
                    },
                    title: {
                        text: 'Total de Proyectos Autorizados por Entidad Federativa',
                        style: {
                            color: '#333',
                            fontSize: '18px',
                            fontWeight: 'bold'
                        }
                    },
                    subtitle: {
                        text: 'Fuente: Solar Fotovoltaica',
                        style: {
                            color: '#666'
                        }
                    },
                    xAxis: {
                        categories: convencional.categories,
                        labels: {
                            style: {
                                color: '#666'
                            }
                        }
                    },
                    yAxis: {
                        title: {
                            text: 'Número de Proyectos',
                            style: {
                                color: '#666'
                            }
                        },
                        gridLineColor: '#e6e6e6'
                    },
                    series: [{
                        name: 'Proyectos Autorizados',
                        data: dataPermisos,
                        color: {
                            linearGradient: { x1: 0, x2: 0, y1: 0, y2: 1 },
                            stops: [
                                [0, '#4facfe'],
                                [1, '#00f2fe']
                            ]
                        },
                        dataLabels: {
                            enabled: true,
                            rotation: 0,
                            color: '#333',
                            align: 'center',
                            format: '{point.y:,.0f}',
                            y: -15,
                            style: {
                                fontSize: '11px',
                                fontWeight: 'bold'
                            },
                            backgroundColor: 'rgba(255, 255, 255, 0.7)',
                            padding: 3,
                            borderRadius: 3
                        }
                    }],
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        style: {
                            color: '#f0f0f0'
                        },
                        headerFormat: '<span style="font-size: 12px">{point.key}</span><br/>',
                        pointFormat: '<span style="color:{series.color}">●</span> {series.name}: <b>{point.y:,.0f}</b>'
                    },
                    plotOptions: {
                        series: {
                            animation: {
                                duration: 1000
                            },
                            states: {
                                hover: {
                                    brightness: -0.1
                                }
                            },
                            borderRadius: 5
                        }
                    },
                    credits: {
                        enabled: false
                    }
                };


                // Renderizar el gráfico en el contenedor con el ID 'grafico'
                Highcharts.chart('grafico', options);
                // Usamos un objeto Set para filtrar los duplicados, ya que un Set solo permite valores únicos
                var uniqueTerms = [...new Set(availableTerms)];

                // Inicializa el autocompletar
                autocomplete(document.getElementById("busquedaGeneralInput"), uniqueTerms);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });



}


//Funcionalidades de Búsqueda y Menú*@



CargaElectricidad();



// Función para inicializar el autocompletar (solo una vez, limpia listeners previos)
function autocomplete(inp, arr) {
    // Remueve listeners previos usando clonación del nodo
    var oldInput = inp;
    var newInput = oldInput.cloneNode(true);
    oldInput.parentNode.replaceChild(newInput, oldInput);
    inp = newInput;

    var currentFocus;
    inp.addEventListener("input", function (e) {
        var a, b, i, val = this.value;
        closeAllLists();
        if (!val) { return false; }
        currentFocus = -1;
        a = document.getElementById("autocomplete-list");
        if (!a) {
            a = document.createElement("div");
            a.setAttribute("id", "autocomplete-list");
            a.setAttribute("class", "autocomplete-items");
            inp.parentNode.appendChild(a);
        }
        a.innerHTML = "";
        for (i = 0; i < arr.length; i++) {
            if (arr[i].substr(0, val.length).toUpperCase() === val.toUpperCase()) {
                b = document.createElement("DIV");
                b.innerHTML = "<strong>" + arr[i].substr(0, val.length) + "</strong>";
                b.innerHTML += arr[i].substr(val.length);
                b.addEventListener("click", function (e) {
                    inp.value = this.innerText;
                    closeAllLists();
                    buscarGeneral();
                });
                a.appendChild(b);
            }
        }
    });
    function closeAllLists(elmnt) {
        var x = document.getElementById("autocomplete-list");
        if (x && elmnt != x && elmnt != inp) {
            x.innerHTML = "";
        }
    }
    document.addEventListener("click", function (e) {
        closeAllLists(e.target);
    });
}



//Links activos*@
function activarElemento(elementoID) {
    // Obtén todos los elementos de tu menú
    var elementos = document.querySelectorAll('.navbarmapag a');

    // Itera sobre ellos para eliminar la clase 'active'
    elementos.forEach(function (el) {
        el.classList.remove('active');
    });

    // Añade la clase 'active' al elemento clickeado
    var elementoActivo = document.getElementById(elementoID);
    if (elementoActivo) {
        elementoActivo.classList.add('active');
    }
}




//Capa de radiación*@


// --- Definición de capas raster desde CDN ---
var rasterBaseLayers = {};

// Potencial Fotovoltaico (por defecto)
rasterBaseLayers["🌞 Potencial Fotovoltaico"] = L.imageOverlay(
    'https://cdn.sassoapps.com/Geovisualizador/rasters/potencialfotovoltaico_4326_0.png',
    L.latLngBounds([32.85, -118.5167], [14.40, -86.5667])
);

// Radiación Horizontal
rasterBaseLayers["☀️ Radiación Horizontal"] = L.imageOverlay(
    'https://cdn.sassoapps.com/Geovisualizador/rasters/radiacionhorizontal_4326_0.png',
    L.latLngBounds([32.85, -118.5175], [14.40, -86.5675])
);

// Velocidad del Viento por alturas
var vientoConfig = [
    { altura: 10, pane: 'pane_Velocidaddevientoa10mdealtura_4', zIndex: 200, sufijo: 4 },
    { altura: 50, pane: 'pane_Velocidaddevientoa50mdealtura_3', zIndex: 200, sufijo: 3 },
    { altura: 100, pane: 'pane_Velocidaddevientoa100mdealtura_2', zIndex: 200, sufijo: 2 },
    { altura: 150, pane: 'pane_Velocidaddevientoa150mdealtura_1', zIndex: 200, sufijo: 1 },
    { altura: 200, pane: 'pane_Velocidaddevientoa200mdealtura_0', zIndex: 200, sufijo: 0 }
];
var vientoBounds = [[12.103268747733761, -122.18342731781249], [32.71826874773478, -84.64092731781062]];
vientoConfig.forEach(function (cfg) {
    mapas[0].createPane(cfg.pane);
    mapas[0].getPane(cfg.pane).style.zIndex = cfg.zIndex;
    // Usar el sufijo correcto para el PNG de cada altura
    rasterBaseLayers["💨 Viento " + cfg.altura + "m"] = L.imageOverlay(
        'https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa' + cfg.altura + 'mdealtura_' + cfg.sufijo + '.png',
        vientoBounds,
        { pane: cfg.pane }
    );
});

// --- Control de capas ---

var control = L.control.layers(rasterBaseLayers, null, { collapsed: false, position: 'topright' }).addTo(mapas[0]);



// --- Funciones para cargar y limpiar marcadores ---
// --- Validación segura para carga de KML ---
function validarNombreKML(input) {
    // No permitir vacíos, espacios solo, ni caracteres peligrosos
    var value = input.value.trim();
    // Expresión regular: solo letras, números, guiones, guion bajo, punto y espacios
    var regex = /^[\w\d\-_. ]+$/;
    if (!value) {
        input.setCustomValidity('El nombre no puede estar vacío.');
        return false;
    }
    if (!regex.test(value)) {
        input.setCustomValidity('Nombre inválido: solo letras, números, guiones, guion bajo, punto y espacios.');
        return false;
    }
    // Validación básica para evitar inyección de dependencias (no <, >, ;, |, &, etc)
    if (/[<>;|&$]/.test(value)) {
        input.setCustomValidity('Nombre inválido: no se permiten caracteres especiales.');
        return false;
    }
    input.setCustomValidity('');
    return true;
}

// Si tienes un input para el nombre del KML, agrega el listener:
setTimeout(function () {
    var kmlInput = document.getElementById('kmlFileNameInput');
    if (kmlInput) {
        kmlInput.addEventListener('input', function () {
            validarNombreKML(this);
        });
        kmlInput.addEventListener('blur', function () {
            validarNombreKML(this);
        });
    }
}, 1000);


// --- Links de descarga de capas ---
// Coloca los links directamente en el div desde el JS al cargar la página
function ponerLinksDescargaDirectos() {
    var contenedor = document.getElementById('descarga-capas');
    if (!contenedor) return;
    contenedor.innerHTML = `
        <ul class="list-group">
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>🌞 Potencial Fotovoltaico</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/potencialfotovoltaico_4326_0.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>☀️ Radiación Horizontal</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/radiacionhorizontal_4326_0.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 10m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa10mdealtura_4.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 50m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa50mdealtura_3.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 100m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa100mdealtura_2.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 150m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa150mdealtura_1.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
            <li class="list-group-item d-flex justify-content-between align-items-center">
                <span>💨 Velocidad de Viento 200m</span>
                <a href="https://cdn.sassoapps.com/Geovisualizador/rasters/Velocidaddevientoa200mdealtura_0.png" download target="_blank" class="btn btn-sm btn-outline-primary">Descargar</a>
            </li>
        </ul>
    `;
}

// Al cargar la página, asegurar capa por defecto y links reales
document.addEventListener('DOMContentLoaded', function () {
    // Mostrar panel de descarga de capas si está oculto
    var descargaPanel = document.getElementById('descarga-capas');
    if (descargaPanel) {
        descargaPanel.style.display = '';
    }
    // Coloca los links de descarga directamente
    ponerLinksDescargaDirectos();
    // Forzar que solo el raster de Potencial Fotovoltaico esté activo al cargar la página
    // Elimina todos los overlays raster (imageOverlay) antes de agregar el de fotovoltaico
    mapas[0].eachLayer(function (layer) {
        if (layer instanceof L.ImageOverlay) {
            mapas[0].removeLayer(layer);
        }
    });
    if (rasterBaseLayers["🌞 Potencial Fotovoltaico"]) {
        rasterBaseLayers["🌞 Potencial Fotovoltaico"].addTo(mapas[0]);
        rasterActivo = rasterBaseLayers["🌞 Potencial Fotovoltaico"];
    }
    // Mostrar leyenda de Potencial Fotovoltaico
    hideAllColorRamps();
    var defaultRamp = document.getElementById('simbol-fotovoltaico');
    if (defaultRamp) {
        defaultRamp.style.display = '';
    }
    setTimeout(agregarBotonCentrarMapa, 200);
    // Cargar marcadores fotovoltaico solo si no se ha llamado ya
    if (typeof cargarMarcadoresFotovoltaico === 'function') {
        cargarMarcadoresFotovoltaico();
    }
});


function limpiarMarcadores() {
    mapas[0].eachLayer(function (layer) {
        if (layer instanceof L.Marker || layer instanceof L.MarkerClusterGroup || layer instanceof L.Circle) {
            mapas[0].removeLayer(layer);
        }
    });
}

function cargarMarcadoresFotovoltaico() {
    // Limpiar todos los marcadores y clusters antes de cargar los nuevos
    limpiarMarcadores();
    // Limpiar input del buscador
    var input = document.getElementById("busquedaGeneralInput");
    if (input) input.value = "";
    // Lógica original de marcadores fotovoltaico
    if (typeof CargaElectricidad === 'function') {
        CargaElectricidad();
        // Actualiza el autocompletado después de cargar datos
        setTimeout(function () {
            autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
        }, 500);
    }
}

function cargarMarcadoresViento() {
    // Limpiar todos los marcadores y clusters antes de cargar los nuevos
    limpiarMarcadores();
    // Limpiar input del buscador
    var input = document.getElementById("busquedaGeneralInput");
    if (input) input.value = "";
    // Lógica de marcadores, buscador y gráfico para eólica
    // Reiniciando availableTerms y las demas variables
    availableTerms = [];
    datosExpendios = [];
    camposVisiblesGlobal = [];
    totalpermisos = 0;

    // Asignando a la búsqueda de términos
    estadosLayer.eachLayer(function (layer) {
        if (layer.feature.properties.NOMGEO) {
            availableTerms.push(layer.feature.properties.NOMGEO);
        }
    });
    for (var i = 0; i < municipios_mapa.features.length; i++) {
        var municipio = municipios_mapa.features[i].properties.NOM_MUN;
        var estado = municipios_mapa.features[i].properties.NOMGEO;
        if (municipio && estado) {
            var nombreCompleto = municipio + ", " + estado;
            availableTerms.push(nombreCompleto);
        }
    }

    // AJAX para campos visibles
    function cargarCamposVisibles() {
        return new Promise((resolve, reject) => {
            $.ajax({
                url: '/Indicadores/GetCamposVisiblesElectricidad_Infra',
                type: 'GET',
                contentType: 'application/json',
                success: function (camposVisibles) {
                    camposVisiblesGlobal = camposVisibles;
                    resolve(camposVisibles);
                },
                error: function (error) {
                    console.error("Error al obtener campos visibles", error);
                    reject(error);
                }
            });
        });
    }

    cargarCamposVisibles().then(camposVisibles => {
        limpiarMarcadores();
        $.ajax({
            url: '/Indicadores/GetExpendiosAutorizadosElectricidad_Infra',
            type: 'GET',
            contentType: 'application/json',
            success: function (response) {
                console.log("Response for Viento:", response); // Log the raw response
                // Solo Eólica
                datosExpendios = response.filter(function (coordenada) {
                    return coordenada.clasificacion === "Eólica";
                });
                console.log("Filtered datosExpendios for Viento:", datosExpendios); // Log the filtered data
                // Limpiar availableTerms y solo agregar los de viento
                availableTerms = [];
                estadosLayer.eachLayer(function (layer) {
                    if (layer.feature.properties.NOMGEO) {
                        availableTerms.push(layer.feature.properties.NOMGEO);
                    }
                });
                for (var i = 0; i < municipios_mapa.features.length; i++) {
                    var municipio = municipios_mapa.features[i].properties.NOM_MUN;
                    var estado = municipios_mapa.features[i].properties.NOMGEO;
                    if (municipio && estado) {
                        var nombreCompleto = municipio + ", " + estado;
                        availableTerms.push(nombreCompleto);
                    }
                }
                for (var i = 0; i < datosExpendios.length; i++) {
                    availableTerms.push(datosExpendios[i].numeroPermiso);
                    datosExpendiosAcumulados.push(datosExpendios[i].numeroPermiso);
                }

                // --- AGREGAR MARCADORES DE VIENTO AL MAPA ---
                var markers = L.markerClusterGroup();
                var iconoBase = L.Icon.extend({
                    options: {
                        iconSize: [36, 36],
                        iconAnchor: [12, 18],
                        popupAnchor: [0, -26]
                    }
                });
                var iconoEolica = new iconoBase({ iconUrl: 'https://cdn.sassoapps.com/img_snier/vistas/eolica.png' });
                function generarContenidoPopup(coordenada) {
                    var contenido = "<style>"
                        + ".popup-content { width: 280px; max-height: 180px; overflow-y: auto; padding: 10px; }"
                        + "h2, h3, h4, p, li { margin: 0 0 10px 0; }"
                        + "ul { padding-left: 20px; }"
                        + "img { vertical-align: middle; margin-right: 10px; }"
                        + "</style>";
                    contenido += "<div class='popup-content'>";
                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<h2 class='subtitulo'><img src='https://cdn.sassoapps.com/img_snier/vistas/eolica.png' style='height: 24px; width: 24px;'/><strong>" + handleNull(coordenada.razonSocial) + "</strong></h2><br>";
                    }
                    contenido += "<ul>";
                    if (camposVisiblesGlobal.includes("EfId")) {
                        contenido += "<li><strong>Entidad Federativa:</strong> " + handleNull(coordenada.efId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<li><strong>Número de Permiso:</strong> " + handleNull(coordenada.numeroPermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("MpoId")) {
                        contenido += "<li><strong>Municipio:</strong> " + handleNull(coordenada.mpoId) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("NumeroDeExpediente")) {
                        contenido += "<li><strong>Número de Expediente:</strong> " + handleNull(coordenada.numeroDeExpediente) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("RazonSocial")) {
                        contenido += "<li><strong>Razón Social:</strong> " + handleNull(coordenada.razonSocial) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaOtorgamiento")) {
                        contenido += "<li><strong>Fecha de Otorgamiento:</strong> " + handleNull(coordenada.fechaOtorgamiento) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LatitudGeo")) {
                        contenido += "<li><strong>Latitud:</strong> " + handleNull(coordenada.latitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("LongitudGeo")) {
                        contenido += "<li><strong>Longitud:</strong> " + handleNull(coordenada.longitudGeo) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Dirección")) {
                        contenido += "<li><strong>Dirección:</strong> " + handleNull(coordenada.direccion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Estatus")) {
                        contenido += "<li><strong>Estatus del Permiso:</strong> " + handleNull(coordenada.estatus) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("RFC")) {
                        contenido += "<li><strong>RFC:</strong> " + handleNull(coordenada.rfc) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FechaRecepcion")) {
                        contenido += "<li><strong>Fecha de Recepción:</strong> " + handleNull(coordenada.fechaRecepcion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EstatusInstalacion")) {
                        contenido += "<li><strong>Estatus de Instalación:</strong> " + handleNull(coordenada.estatusInstalacion) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("TipoPermiso")) {
                        contenido += "<li><strong>Tipo de Permiso:</strong> " + handleNull(coordenada.tipoPermiso) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InicioVigencia")) {
                        contenido += "<li><strong>Inicio de Vigencia:</strong> " + handleNull(coordenada.inicioVigencia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("InicioOperaciones")) {
                        contenido += "<li><strong>Inicio de Operaciones:</strong> " + handleNull(coordenada.inicioOperaciones) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("CapacidadAutorizadaMW")) {
                        contenido += "<li><strong>Capacidad Autorizada (MW):</strong> " + handleNull(coordenada.capacidadAutorizadaMW) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Generación_estimada_anual")) {
                        contenido += "<li><strong>Generación Estimada Anual:</strong> " + handleNull(coordenada.generacion_estimada_anual) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Inversion_estimada_mdls")) {
                        contenido += "<li><strong>Inversión Estimada (mdls):</strong> " + handleNull(coordenada.inversion_estimada_mdls) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Energetico_primario")) {
                        contenido += "<li><strong>Energético Primario:</strong> " + handleNull(coordenada.energetico_primario) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Actividad_economica")) {
                        contenido += "<li><strong>Actividad Económica:</strong> " + handleNull(coordenada.actividad_economica) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Tecnología")) {
                        contenido += "<li><strong>Tecnología:</strong> " + handleNull(coordenada.tecnologia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("EmpresaLíder")) {
                        contenido += "<li><strong>Empresa Líder:</strong> " + handleNull(coordenada.empresaLider) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("PaísDeOrigen")) {
                        contenido += "<li><strong>País de Origen:</strong> " + handleNull(coordenada.paisDeOrigen) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Subasta")) {
                        contenido += "<li><strong>Subasta:</strong> " + handleNull(coordenada.subasta) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Planta")) {
                        contenido += "<li><strong>Planta:</strong> " + handleNull(coordenada.planta) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Combustible")) {
                        contenido += "<li><strong>Combustible:</strong> " + handleNull(coordenada.combustible) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("FuenteEnergía")) {
                        contenido += "<li><strong>Fuente de Energía:</strong> " + handleNull(coordenada.fuenteEnergia) + "</li>";
                    }
                    if (camposVisiblesGlobal.includes("Comentarios")) {
                        contenido += "<li><strong>Comentarios:</strong> " + handleNull(coordenada.comentarios) + "</li>";
                    }
                    contenido += "</ul>";
                    if (camposVisiblesGlobal.includes("NumeroPermiso")) {
                        contenido += "<a class='btn btn-cre-rojo' target='_blank' href='/Indicadores/DetalleExpendio?NumeroPermiso=" + coordenada.numeroPermiso + "'>Ver detalle</a>";
                    }
                    contenido += "<a class='street-view-link btn btn-cre-verde' href='http://maps.google.com/maps?q=&layer=c&cbll=" + coordenada.latitudGeo + "," + coordenada.longitudGeo + "&cbp=11,0,0,0,0' target='_blank'><b> Ver vista de calle </b></a>";
                    contenido += "</div>";
                    return contenido;
                }
                for (var j = 0; j < datosExpendios.length; j++) {
                    var coordenada = datosExpendios[j];
                    var marker = L.marker([coordenada.latitudGeo, coordenada.longitudGeo], { icon: iconoEolica });
                    var contenidoPopup = generarContenidoPopup(coordenada);
                    marker.bindPopup(contenidoPopup);
                    markers.addLayer(marker);
                }
                mapas[0].addLayer(markers);
                // --- FIN AGREGAR MARCADORES DE VIENTO ---

                //******Tarjetas Totales******///
                // Contar el número total de elementos en el array 'datosExpendios'
                var totalElementos = datosExpendios.length;
                // Formatear el número con separadores de miles
                var totalFormateado = totalElementos.toLocaleString();

                console.log("Total de permisos (Eólica) formateado:", totalFormateado);

                // Actualizar la tarjeta de permisos
                const tarjetaTitulo = document.getElementById('titulo-permiso');
                const tarjetaTotal = document.getElementById('total-permisos');
                const tarjetaIcono = document.getElementById('icono-permiso');

                if (tarjetaTitulo) tarjetaTitulo.textContent = 'Eólica';
                if (tarjetaIcono) tarjetaIcono.src = 'https://cdn.sassoapps.com/img_snier/vistas/eolica.png'; // Asegúrate que el ícono exista
                if (tarjetaTotal) tarjetaTotal.textContent = "Proyectos Autorizados: " + totalFormateado;
                //******Fin Tarjetas Totales******///

                // --- INICIO GRÁFICO VIENTO ---
                var counts = {};
                datosExpendios.forEach(function (coordenada) {
                    if (!counts[coordenada.efId]) {
                        counts[coordenada.efId] = 0;
                    }
                    counts[coordenada.efId]++;
                });

                var categories = [];
                var dataPermisos = [];
                for (var entidad in counts) {
                    categories.push(entidad);
                    dataPermisos.push(counts[entidad]);
                }

                var options = {
                    chart: {
                        type: 'column',
                        backgroundColor: 'transparent',
                        style: {
                            fontFamily: 'inherit'
                        },
                        borderRadius: 10
                    },
                    title: {
                        text: 'Total de Proyectos Autorizados por Entidad Federativa',
                        style: {
                            color: '#333',
                            fontSize: '18px',
                            fontWeight: 'bold'
                        }
                    },
                    subtitle: {
                        text: 'Fuente: Eólica',
                        style: {
                            color: '#666'
                        }
                    },
                    xAxis: {
                        categories: categories,
                        labels: {
                            style: {
                                color: '#666'
                            }
                        }
                    },
                    yAxis: {
                        title: {
                            text: 'Número de Proyectos',
                            style: {
                                color: '#666'
                            }
                        },
                        gridLineColor: '#e6e6e6'
                    },
                    series: [{
                        name: 'Proyectos Autorizados',
                        data: dataPermisos,
                        color: {
                            linearGradient: { x1: 0, x2: 0, y1: 0, y2: 1 },
                            stops: [
                                [0, '#2af598'],
                                [1, '#009efd']
                            ]
                        },
                        dataLabels: {
                            enabled: true,
                            rotation: 0,
                            color: '#333',
                            align: 'center',
                            format: '{point.y:,.0f}',
                            y: -15,
                            style: {
                                fontSize: '11px',
                                fontWeight: 'bold'
                            },
                            backgroundColor: 'rgba(255, 255, 255, 0.7)',
                            padding: 3,
                            borderRadius: 3
                        }
                    }],
                    tooltip: {
                        backgroundColor: 'rgba(0, 0, 0, 0.8)',
                        style: {
                            color: '#f0f0f0'
                        },
                        headerFormat: '<span style="font-size: 12px">{point.key}</span><br/>',
                        pointFormat: '<span style="color:{series.color}">●</span> {series.name}: <b>{point.y:,.0f}</b>'
                    },
                    plotOptions: {
                        series: {
                            animation: {
                                duration: 1000
                            },
                            states: {
                                hover: {
                                    brightness: -0.1
                                }
                            },
                            borderRadius: 5
                        }
                    },
                    credits: {
                        enabled: false
                    }
                };
                Highcharts.chart('grafico', options);
                // --- FIN GRÁFICO VIENTO ---


                setTimeout(function () {
                    autocomplete(document.getElementById("busquedaGeneralInput"), availableTerms);
                }, 500);
            },
            error: function (error) {
                // Maneja el error si ocurre.
            }
        });
    }).catch(error => {
        // Manejo de errores del primer AJAX
    });
}


// --- Control de opacidad para raster activo ---
var rasterActivo = rasterBaseLayers["🌞 Potencial Fotovoltaico"];

// Crear el control visual (slider)
var opacityControl = L.control({ position: 'topright' });
opacityControl.onAdd = function (map) {
    var div = L.DomUtil.create('div', 'leaflet-bar leaflet-control');
    div.style.background = '#fff';
    div.style.padding = '6px 10px 2px 10px';
    div.style.width = '160px';
    div.innerHTML = '<label style="font-size:12px;">Opacidad raster</label><br><input id="raster-opacity-slider" type="range" min="0" max="1" step="0.01" value="1" style="width:130px;">';
    return div;
};
opacityControl.addTo(mapas[0]);

// Evento para cambiar opacidad
setTimeout(function () {
    var slider = document.getElementById('raster-opacity-slider');
    if (slider) {
        slider.addEventListener('input', function () {
            if (rasterActivo && typeof rasterActivo.setOpacity === 'function') {
                rasterActivo.setOpacity(parseFloat(this.value));
            }
        });
    }
}, 500);

function hideAllColorRamps() {
    var rampIds = ['simbol-fotovoltaico', 'simbol-radiacion', 'simbol-viento'];
    rampIds.forEach(function (id) {
        var el = document.getElementById(id);
        if (el) {
            el.style.display = 'none';
        }
    });
}

// Cargar capa y marcadores por defecto
rasterActivo.addTo(mapas[0]);
cargarMarcadoresFotovoltaico();

// Ocultar todas las rampas al inicio y luego mostrar la por defecto
hideAllColorRamps();
var defaultRamp = document.getElementById('simbol-fotovoltaico');
if (defaultRamp) {
    defaultRamp.style.display = '';
}

// --- Evento para cambio de capa raster ---
mapas[0].on('baselayerchange', function (e) {
    limpiarMarcadores();
    // Quitar opacidad al raster anterior
    if (rasterActivo && typeof rasterActivo.setOpacity === 'function') {
        rasterActivo.setOpacity(1);
    }
    rasterActivo = e.layer;
    // Sincronizar slider con la opacidad actual
    var slider = document.getElementById('raster-opacity-slider');
    if (slider && rasterActivo && typeof rasterActivo.options.opacity !== 'undefined') {
        slider.value = rasterActivo.options.opacity;
    } else if (slider) {
        slider.value = 1;
    }

    // Oculta todas las leyendas y rampas (solo si existen)
    var tablas = [
        'simbol-fotovoltaico',
        'simbol-radiacion',
        'simbol-viento'
    ];
    tablas.forEach(function (id) {
        var el = document.getElementById(id);
        if (el) el.style.display = 'none';
    });

    // Determina qué leyenda/rampa mostrar (solo si existen)
    if (e.name === '🌞 Potencial Fotovoltaico') {
        var t = document.getElementById('simbol-fotovoltaico');
        if (t) t.style.display = '';
    } else if (e.name === '☀️ Radiación Horizontal') {
        var t = document.getElementById('simbol-radiacion');
        if (t) t.style.display = '';
    } else if (e.name && e.name.startsWith('💨 Viento')) {
        var t = document.getElementById('simbol-viento');
        if (t) t.style.display = '';
    }

    // Limpiar input del buscador y términos
    var input = document.getElementById("busquedaGeneralInput");
    if (input) input.value = "";
    availableTerms = [];

    if (e.name === "🌞 Potencial Fotovoltaico" || e.name === "☀️ Radiación Horizontal") {
        cargarMarcadoresFotovoltaico();
    } else if (e.name && e.name.startsWith("💨 Viento")) {
        cargarMarcadoresViento();
    }
    // El autocompletado se actualiza dentro de las funciones de carga
});
