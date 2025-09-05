window.dataProcessor = {
    // Función auxiliar para identificar si un nodo es un energético específico o un contenedor
    isSpecificEnergetic: function(nodeName) {
        // Los contenedores típicamente tienen palabras clave como estas
        const containerKeywords = [
            "Importación", "Exportación", "Producción", "Variación", "Inventarios", 
            "Diferencia", "Estadística", "Pérdidas", "Oferta", "Bruta", "Consumo", 
            "Propio", "Sector", "Centrales", "Eléctricas", "Coquizadoras", "Hornos", 
            "Plantas", "Gas", "Fraccionadoras", "Refinerías", "Despuntadoras", 
            "Industrial", "Transporte", "Agropecuario", "Comercial", "Público", 
            "Residencial", "Petroquímica", "PEMEX", "Total", "V.I.", "Dif.", "Est."
        ];
        
        // Si el nombre contiene alguna palabra clave de contenedor, no es un energético específico
        return !containerKeywords.some(keyword => nodeName.includes(keyword));
    },

    // Función auxiliar para obtener el color del energético correcto
    getEnergeticColor: function(nodeName, nodeData, config) {
        // Si el nombre del nodo es un energético específico, usar su color
        if (this.isSpecificEnergetic(nodeName) && config.energeticColors[nodeName]) {
            return config.energeticColors[nodeName];
        }
        
        // Si es un nodo hijo y el nombre del hijo es un energético específico
        if (nodeData && nodeData["Nodo Hijo"] && this.isSpecificEnergetic(nodeData["Nodo Hijo"])) {
            return config.energeticColors[nodeData["Nodo Hijo"]];
        }
        
        // Si no es un energético específico, intentar con el nombre directo del nodo (para contenedores)
        if (config.energeticColors[nodeName]) {
            return config.energeticColors[nodeName];
        }
        
        // Para nodos sin color específico, usar color genérico
        return '#888';
    },

    processSankeyData: function(data, year, config) {
        const nodes = new Map();
        const links = [];

        // 1. Construir todos los nodos a partir de la configuración
        config.columnas.forEach((col, colIdx) => {
            let nodosCol = [];
            if (col.nodos && col.nodos.length > 0) {
                nodosCol = col.nodos.filter(n => n.visible !== false);
            } else if (col.mostrar === "Hijo" && col.filtroTipo) {
                data.Datos.forEach(padre => {
                    padre["Nodos Hijo"].forEach(hijo => {
                        if (hijo.tipo === col.filtroTipo) {
                            nodosCol.push({ nombre: hijo["Nodo Hijo"], tipo: "Hijo", padre: padre["Nodo Padre"] });
                        }
                    });
                });
            }

            nodosCol.forEach(nodo => {
                if (!nodes.has(nodo.nombre)) {
                    let nodeData = this.findNodeData(data, nodo.nombre, nodo.tipo);
                    const nodeConfig = {
                        ...(nodeData || {}), // Copy all properties from nodeData
                        name: nodo.nombre,
                        description: nodeData ? nodeData.descripcion : '',
                        tipo: nodeData ? nodeData.tipo : nodo.tipo,
                        padre: nodo.padre || null,
                        columna: colIdx,
                        depth: nodo.depth !== undefined ? nodo.depth : colIdx,
                        flow: nodo.flow || 'default',
                        esEspaciador: nodo.esEspaciador || false
                    };

                    // Lógica para espaciadores
                    if (nodeConfig.esEspaciador) {
                        nodeConfig.itemStyle = { 
                            color: 'rgba(0,0,0,0)', // Completamente transparente
                            borderColor: 'rgba(0,0,0,0)',
                            borderWidth: 0,
                            opacity: 0
                        };
                        nodeConfig.label = { show: false };
                        nodeConfig.silent = true; // Deshabilitar todos los eventos de mouse
                        nodeConfig.value = nodo.valorEspaciador || 0.1; // Asignar valor para que ocupe espacio
                    } else {
                        // Solo asignar color a nodos no espaciadores - usar función inteligente para obtener color del energético
                        nodeConfig.itemStyle = { color: this.getEnergeticColor(nodo.nombre, nodeData, config) };
                    }

                    nodes.set(nodo.nombre, nodeConfig);
                }
            });
        });

        // 2. Crear enlaces basados en el flujo de datos
        data.Datos.forEach(padre => {
            const padreNode = nodes.get(padre["Nodo Padre"]);
            if (padreNode) {
                padre["Nodos Hijo"].forEach(hijo => {
                    if (nodes.has(hijo["Nodo Hijo"])) {
                        const value = hijo[year] || 0;
                        if (value !== 0) {
                            let source, target;

                            // La dirección del flujo la determina el NODO PADRE
                            if (value < 0 || (padreNode && padreNode.flow === 'sink')) {
                                // Si el padre es un sink (consumidor), el flujo va del hijo hacia el padre
                                source = hijo["Nodo Hijo"];
                                target = padre["Nodo Padre"];
                            } else {
                                // Comportamiento por defecto: el padre es una fuente, el flujo va del padre al hijo
                                source = padre["Nodo Padre"];
                                target = hijo["Nodo Hijo"];
                            }
                            // Determinar el color del energético que fluye
                            // En la mayoría de casos, el hijo es el energético específico
                            const energeticName = hijo["Nodo Hijo"];
                            const energeticColor = this.getEnergeticColor(energeticName, hijo, config);
                            
                            links.push({
                                source: source,
                                target: target,
                                value: Math.abs(value),
                                lineStyle: {
                                    color: energeticColor,
                                    opacity: 0.7
                                } // Usar color del energético que fluye
                            });
                        }
                    }
                });
            }
        });

        // 3. Calcular el valor neto de cada nodo (entrada - salida) y adjuntar inflow/outflow
        const nodeInflow = new Map();
        const nodeOutflow = new Map();

        links.forEach(link => {
            nodeInflow.set(link.target, (nodeInflow.get(link.target) || 0) + link.value);
            nodeOutflow.set(link.source, (nodeOutflow.get(link.source) || 0) + link.value);
        });

        Array.from(nodes.values()).forEach(node => {
            if (!node.esEspaciador) {
                let inflow = nodeInflow.get(node.name) || 0;
                let outflow = nodeOutflow.get(node.name) || 0;

                // Forzar comportamiento de source/sink
                if (node.flow === 'sink') {
                    outflow = 0; // Un sink no tiene salidas
                } else if (node.flow === 'source') {
                    inflow = 0; // Un source no tiene entradas
                }

                node.value = Math.max(inflow, outflow); // El valor del nodo es el mayor de los flujos
                node.inflow = inflow;
                node.outflow = outflow;
            }
        });

        return { nodes: Array.from(nodes.values()), links };
    },

    findNodeData: function(data, nodeName, nodeType) {
        if (nodeType === "Padre") {
            return data.Datos.find(d => d["Nodo Padre"] === nodeName);
        }
        for (const padre of data.Datos) {
            const hijo = padre["Nodos Hijo"].find(h => h["Nodo Hijo"] === nodeName);
            if (hijo) return hijo;
        }
        return null;
    }
};