#!/usr/bin/env node

/**
 * MCP Server para SQL Server - SNIER (Producción)
 * Conecta a la base de datos de producción que funciona
 */

const sql = require('mssql');

// Configuración para base de datos de producción (que sabemos que funciona)
const config = {
    server: 'servidorsqljavidev.database.windows.net',
    database: 'BDPruebasSNIER',
    user: 'adminsql',
    password: 'J@v1sql2025*',
    options: {
        encrypt: true,
        trustServerCertificate: false
    }
};

// Funciones principales
async function conectar() {
    try {
        const pool = await sql.connect(config);
        console.log('✅ Conectado a SQL Server producción');
        return pool;
    } catch (error) {
        console.error('❌ Error de conexión:', error.message);
        throw error;
    }
}

async function listarTablas() {
    const pool = await conectar();
    const result = await pool.request().query(`
        SELECT TABLE_SCHEMA, TABLE_NAME
        FROM INFORMATION_SCHEMA.TABLES
        WHERE TABLE_TYPE = 'BASE TABLE'
        ORDER BY TABLE_SCHEMA, TABLE_NAME
    `);
    await pool.close();
    return result.recordset;
}

async function describir(tableName) {
    const pool = await conectar();
    const result = await pool.request()
        .input('tableName', sql.VarChar, tableName)
        .query(`
            SELECT 
                COLUMN_NAME,
                DATA_TYPE,
                IS_NULLABLE,
                CHARACTER_MAXIMUM_LENGTH
            FROM INFORMATION_SCHEMA.COLUMNS
            WHERE TABLE_NAME = @tableName
            ORDER BY ORDINAL_POSITION
        `);
    await pool.close();
    return result.recordset;
}

async function consultar(query) {
    if (!query.trim().toLowerCase().startsWith('select')) {
        throw new Error('Solo se permiten consultas SELECT');
    }

    const pool = await conectar();
    const result = await pool.request().query(query);
    await pool.close();
    return result.recordset;
}

// Interfaz de comandos
const commands = {
    'test': async () => {
        try {
            const pool = await conectar();
            await pool.close();
            console.log('🎉 Conexión exitosa!');
        } catch (error) {
            console.log('❌ Error:', error.message);
        }
    },

    'tables': async () => {
        try {
            const tables = await listarTablas();
            console.log(`📋 Total de tablas: ${tables.length}`);

            // Agrupar por esquema
            const bySchema = {};
            tables.forEach(t => {
                if (!bySchema[t.TABLE_SCHEMA]) bySchema[t.TABLE_SCHEMA] = [];
                bySchema[t.TABLE_SCHEMA].push(t.TABLE_NAME);
            });

            Object.keys(bySchema).forEach(schema => {
                console.log(`\n🗂️ ${schema}: ${bySchema[schema].length} tablas`);
                console.log(`   ${bySchema[schema].slice(0, 5).join(', ')}${bySchema[schema].length > 5 ? '...' : ''}`);
            });
        } catch (error) {
            console.log('❌ Error:', error.message);
        }
    },

    'describe': async (tableName) => {
        if (!tableName) {
            console.log('❌ Uso: node mcp-sql-production.js describe NombreTabla');
            return;
        }
        try {
            const columns = await describir(tableName);
            console.log(`\n📋 Estructura de ${tableName}:`);
            columns.forEach(col => {
                const nullable = col.IS_NULLABLE === 'YES' ? 'NULL' : 'NOT NULL';
                const length = col.CHARACTER_MAXIMUM_LENGTH ? `(${col.CHARACTER_MAXIMUM_LENGTH})` : '';
                console.log(`  📝 ${col.COLUMN_NAME}: ${col.DATA_TYPE}${length} - ${nullable}`);
            });
        } catch (error) {
            console.log('❌ Error:', error.message);
        }
    },

    'query': async (query) => {
        if (!query) {
            console.log('❌ Uso: node mcp-sql-production.js query "SELECT * FROM tabla"');
            return;
        }
        try {
            const results = await consultar(query);
            console.log(`\n📊 Resultados: ${results.length} registros`);
            if (results.length > 0) {
                console.log('\n📄 Primeros 5 registros:');
                results.slice(0, 5).forEach((row, i) => {
                    console.log(`${i + 1}. ${JSON.stringify(row, null, 2)}`);
                });
            }
        } catch (error) {
            console.log('❌ Error:', error.message);
        }
    }
};

// Procesamiento de argumentos de línea de comandos
if (require.main === module) {
    const args = process.argv.slice(2);
    const command = args[0] || 'test';
    const param = args.slice(1).join(' ');

    console.log('🗄️ MCP SQL Server - SNIER');
    console.log(`📍 Servidor: ${config.server}`);
    console.log(`📊 Base de datos: ${config.database}\n`);

    if (commands[command]) {
        commands[command](param);
    } else {
        console.log('❌ Comando no reconocido');
        console.log('\n📋 Comandos disponibles:');
        console.log('  test       - Probar conexión');
        console.log('  tables     - Listar todas las tablas');
        console.log('  describe   - Describir una tabla');
        console.log('  query      - Ejecutar una consulta SELECT');
        console.log('\n📝 Ejemplos:');
        console.log('  node mcp-sql-production.js test');
        console.log('  node mcp-sql-production.js tables');
        console.log('  node mcp-sql-production.js describe Usuarios');
        console.log('  node mcp-sql-production.js query "SELECT TOP 5 * FROM Usuarios"');
    }
}

module.exports = { conectar, listarTablas, describir, consultar };
