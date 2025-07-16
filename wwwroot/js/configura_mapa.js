
// Configuración General de los Mapas

        var mapas = [];
        var bounds_group = new L.featureGroup([]);
        var autolinker = new Autolinker({ truncate: { length: 30, location: 'smart' } });


        function iniciarMapas() {
        
            ///Variable a la que le cargaremos los  mapas

                //Tipos de Mapas
            // Verifica si el ID 'map' existe "Mapa General"
                if (document.getElementById('map')) {
                    // Solo crea este mapa si el elemento existe
                    mapas.push(L.map('map', {zoomControl: true, maxZoom: 28, minZoom: 5, zoom: 3}).fitBounds([[16.032962888161236, -107.10661100519332], [29.296274774446957, -85.32517649609608]]));

                }
            
                if (document.getElementById('map_presionagua')) {
                    mapas.push(L.map('map_presionagua', { zoomControl: true, maxZoom: 28, minZoom: 5, zoom: 3 }).fitBounds([[16.032962888161236, -107.10661100519332], [29.296274774446957, -85.32517649609608]]));
                }

                if (document.getElementById('mapa_estres_h')) {
                    mapas.push(L.map('mapa_estres_h', {zoomControl: true, maxZoom: 28, minZoom: 5, zoom: 3}).fitBounds([[16.032962888161236, -107.10661100519332], [29.296274774446957, -85.32517649609608]]));
                }

                //Funcionalidades de los mapas

                //Herramientas alos mapas  
                function initializeMap(targetMap) {
                    // Hash, atribuciones y grupos de límites
                    new L.Hash(targetMap);
                    targetMap.attributionControl.setPrefix('<a href="https://github.com/tomchadwin/qgis2web" target="_blank">qgis2web</a> &middot; <a href="https://leafletjs.com" title="A JS library for interactive maps">Leaflet</a> &middot; <a href="https://qgis.org">QGIS</a>');
                    new Autolinker({ truncate: { length: 30, location: 'smart' } });
                    new L.featureGroup([]).addTo(targetMap);
                
                    // Vista Satélite
                    targetMap.createPane('pane_GoogleSatellite_0');
                    targetMap.getPane('pane_GoogleSatellite_0').style.zIndex = 0;
                    var layer_GoogleSatellite_0 = L.tileLayer('https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}', {
                        pane: 'pane_GoogleSatellite_0',
                        opacity: 1.0,
                        attribution: '<a href="https://www.google.at/permissions/geoguidelines/attr-guide.html">Map data ©2015 Google</a>',
                        minZoom: 1,
                        maxZoom: 28,
                        minNativeZoom: 0,
                        maxNativeZoom: 20
                    });
                
                    var cartoDBDarkAll = L.tileLayer('https://a.basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png', {
                        attribution: '&copy; <a href="https://carto.com/attributions">CARTO</a>'
                    });
                
                    // Configura los Base Layers y expónlos globalmente
                    var baseLayers = {
                        "Vista de Calle": L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
                            attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a> contributors'
                        }),
                        "Vista Satélite": layer_GoogleSatellite_0,
                        "Modo Oscuro": cartoDBDarkAll
                    };
                    // Asigna a una variable global
                    window.baseMaps = baseLayers;
                
                    // Control de capas y vista inicial
                    L.control.layers(baseLayers).addTo(targetMap);
                    baseLayers["Vista de Calle"].addTo(targetMap);
                
                    // Eventos y controles
                    targetMap.on('baselayerchange', function (eventLayer) {
                        if (eventLayer.name === "Vista Satélite") {
                            layer_GoogleSatellite_0.redraw();
                        }
                    });
                
                    L.control.scale().addTo(targetMap);
                
                    var resetZoomControl = L.control({ position: 'topleft' });
                    resetZoomControl.onAdd = function (map) {
                        var div = L.DomUtil.create('div', 'leaflet-control-custom');
                        div.innerHTML = '<button class="btn"><i class="bi bi-house-door"></i></button>';
                        div.firstChild.onclick = function() {
                            resetZoom(map);
                        };
                        return div;
                    };
                    resetZoomControl.addTo(targetMap);
                
                    // Agregar el control de dibujo (puedes seguir usándolo en este mapa)
                    var drawControl = new L.Control.Draw({
                        draw: {
                            polyline: true,
                            polygon: false,
                            rectangle: false,
                            circle: false,
                            marker: false,
                            circlemarker: false
                        },
                        edit: false
                    }).addTo(targetMap);
                }
                

                // Aplicar la configuración a cada mapa en el arreglo
                mapas.forEach(map => {
                    map.setView([24.572503, -101.768257], 5);
                    initializeMap(map);
                });

                //Zoom
                function resetZoom(map) {
                    map.setView([24.572503, -101.768257], 5);
                }   

                
            
            // Menu d eEstados para el mapa general "map"
            // Verifica si el elemento con ID 'estado' existe
            var selectEstado = document.getElementById("estado");
            if (selectEstado) {
                // Agrega un listener de evento 'change' al selector de estados
                selectEstado.addEventListener("change", function() {
                    // Obtiene el valor seleccionado del menú de estados
                    var estado = selectEstado.value;

                    // Asegúrate de tener una referencia al mapa que quieres ajustar
                    var map = mapas[0]; // Asume que quieres ajustar el primer mapa en tu arreglo

                    // Muestra el estado seleccionado en el mapa
                    switch (estado) {
                        // ... tu switch case como lo has definido ...
                                        case "ags":
                            map.setView([22.025278, -102.372778], 9);
                            break;
                        case "bc":
                            map.setView([30.391389, -115.291389], 7);
                            break;
                        case "bcs":
                            map.setView([26.044444, -112.399722], 7);
                            break;
                        case "camp":
                            map.setView([18.938333, -90.764722], 7);
                            break;
                        case "coah":
                            map.setView([27.158056, -101.719444], 7);
                            break;
                        case "col":
                            map.setView([19.101944, -103.014722], 9);
                            break;
                        case "chis":
                            map.setView([16.753056, -92.6375], 7);
                            break;
                        case "chih":
                            map.setView([28.673611, -106.102222], 7);
                            break;
                        case "cdmx":
                            map.setView([19.432778, -99.133333], 10);
                            break;
                        case "dgo":
                            map.setView([24.865278, -104.902222], 7);
                            break;
                        case "gto":
                            map.setView([20.875, -101.478611], 8);
                            break;
                        case "gro":
                            map.setView([17.814722, -100.353056], 8);
                            break;
                        case "hgo":
                            map.setView([20.5325, -98.870556], 8);
                            break;
                        case "jal":
                            map.setView([20.673611, -103.343333], 8);
                            break;
                        case "mex":
                            map.setView([19.483611, -99.689722], 8);
                            break;
                        case "mich":
                            map.setView([19.699722, -101.191389], 8);
                            break;
                        case "mor":
                            map.setView([18.858611, -99.223611], 9);
                            break;
                        case "nay":
                            map.setView([21.799444, -105.220833], 8);
                            break;
                        case "nl":
                            map.setView([25.649167, -100.443611], 9);
                            break;
                        case "oax":
                            map.setView([16.895833, -96.806667], 8);
                            break;
                        case "pue":
                            map.setView([19.051389, -98.193889], 8);
                            break;
                        case "qro":
                            map.setView([20.854722, -99.847222], 9);
                            break;
                        case "qroo":
                            map.setView([19.642778, -87.072222], 8);
                            break;
                        case "slp":
                            map.setView([22.156944, -100.985556], 8);
                            break;
                        case "sin":
                            map.setView([24.288611, -107.366944], 8);
                            break;
                        case "son":
                            map.setView([29.089444, -110.961667], 7);
                            break;
                        case "tab":
                            map.setView([17.980000, -92.930000], 8);
                            break;
                        case "tamps":

                            map.setView([24.014167, -98.844444], 7);
                            break;
                        case "tlax":
                            map.setView([19.312222, -98.239722], 10);
                            break;
                        case "ver":
                            map.setView([19.546389, -96.914167], 8);
                            break;
                        case "yuc":
                            map.setView([20.891944, -89.528611], 8);
                            break;
                        case "zac":
                            map.setView([22.770833, -102.583611], 8);
                            break;
                        default:
                            map.setView([23.634501, -102.552784], 5);
                            break;
                    }
                });
            }

        }
        iniciarMapas();

