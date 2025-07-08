#!/usr/bin/env node

/**
 * MCP Server para SQL Server Local - SNIER
 * Versión usando msnodesqlv8 directamente
 */

const sql = require('msnodesqlv8');

// Connection string para la base SNIER local
const connectionString = "Driver={ODBC Driver 17 for SQL Server};Server=.\\SQLEXPRESS;Database=SNIER;Trusted_Connection=yes;";

// Funciones helper para promisificar las operaciones
function queryPromise(query, params = []) {
    return new Promise((resolve, reject) => {
        sql.query(connectionString, query, params, (err, rows) => {
            if (err) {
                reject(err);
            } else {
                resolve(rows);
            }
        });
    });
}

// Funciones para interactuar con la base de datos
async function conectar() {
    try {
        // Probar conexión con una consulta simple
        await queryPromise('SELECT 1 as test');
        console.log('✅ Conectado a SQL Server local');
        return true;
    } catch (error) {
        console.error('❌ Error de conexión:', error.message);
        throw error;
    }
}

async function listarTablas() {
    const result = await queryPromise(`
        SELECT TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_NAME
    `);
    return result.map(row => row.TABLE_NAME);
}

async function describir(tableName) {
    const result = await queryPromise(`
        SELECT 
            COLUMN_NAME,
            DATA_TYPE,
            IS_NULLABLE,
            CHARACTER_MAXIMUM_LENGTH
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_NAME = ?
        ORDER BY ORDINAL_POSITION
    `, [tableName]);
    return result;
}

async function consultar(query) {
    if (!query.trim().toLowerCase().startsWith('select')) {
        throw new Error('Solo se permiten consultas SELECT');
    }

    const result = await queryPromise(query);
    return result;
}

// Servidor MCP simple
class SimpleMCPServer {
    constructor() {
        this.tools = {
            'test_connection': this.testConnection.bind(this),
            'list_tables': this.listTables.bind(this),
            'describe_table': this.describeTable.bind(this),
            'execute_query': this.executeQuery.bind(this)
        };
    }

    async testConnection() {
        try {
            await conectar();
            return { success: true, message: 'Conexión exitosa a SQL Server local' };
        } catch (error) {
            return { success: false, message: `Error: ${error.message}` };
        }
    }

    async listTables() {
        try {
            const tables = await listarTablas();
            return {
                success: true,
                data: tables,
                message: `${tables.length} tablas encontradas`
            };
        } catch (error) {
            return { success: false, message: `Error: ${error.message}` };
        }
    }

    async describeTable(tableName) {
        try {
            const columns = await describir(tableName);
            return {
                success: true,
                data: columns,
                message: `Estructura de ${tableName}`
            };
        } catch (error) {
            return { success: false, message: `Error: ${error.message}` };
        }
    }

    async executeQuery(query) {
        try {
            const results = await consultar(query);
            return {
                success: true,
                data: results.slice(0, 10), // Limitar a 10 registros
                message: `${results.length} registros encontrados`
            };
        } catch (error) {
            return { success: false, message: `Error: ${error.message}` };
        }
    }

    async handleRequest(toolName, args) {
        if (this.tools[toolName]) {
            return await this.tools[toolName](args);
        } else {
            return { success: false, message: `Herramienta ${toolName} no encontrada` };
        }
    }
}

// Modo de prueba directo
if (require.main === module) {
    const server = new SimpleMCPServer();

    console.log('🧪 Probando MCP SQL Server Local (msnodesqlv8)...');
    console.log('🔗 Connection String:', connectionString);

    async function runTests() {
        try {
            // Probar conexión
            const connectionResult = await server.testConnection();
            console.log('🔍 Test de conexión:', connectionResult.message);

            if (connectionResult.success) {
                // Probar listar tablas
                const tablesResult = await server.listTables();
                console.log('📋 Tablas:', tablesResult.message);
                console.log('📄 Primeras 5 tablas:', tablesResult.data.slice(0, 5).join(', '));

                // Verificar si existe la base SNIER
                const snierCheck = await queryPromise("SELECT COUNT(*) as existe FROM sys.databases WHERE name = 'SNIER'");
                if (snierCheck[0].existe > 0) {
                    console.log('✅ Base de datos SNIER encontrada');
                } else {
                    console.log('⚠️  Base de datos SNIER no encontrada');
                }

                console.log('\n🎉 ¡MCP Server local funcionando correctamente!');
            }
        } catch (error) {
            console.error('💥 Error durante las pruebas:', error.message);
        }
    }

    runTests();
}

module.exports = SimpleMCPServer;
