using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NSIE.Controllers;
using NSIE.Models;
using System.Text.Json;
using System.Collections.Generic;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;

namespace NSIE.Servicios
{
    public interface IRepositorioHome
    {



    }

    //El Select SCOPE IDENTITY devuelve el ID impactado en la BD
    public class RepositorioHome : IRepositorioHome
    {

        //Cadena de Conexión a BD
        private readonly string connectionString; //Variable en la que se coloca el valor del CS por default desde el proveedor de configuraciones
        public RepositorioHome(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }










    }

}











