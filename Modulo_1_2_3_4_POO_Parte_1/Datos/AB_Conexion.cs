using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaSeguros;

// Clase que controla la conexion con SQL Server.
public class AB_Conexion
{
    // Variable que guarda la unica instancia de conexion.
    private static AB_Conexion AB_Instancia = null;

    // Variable para abrir la conexion a la base de datos.
    private SqlConnection AB_ConexionSql;

    // Cadena de conexion de la base de datos del proyecto.
    private string AB_CadenaConexion =
        "Server=(localdb)\\MSSQLLocalDB;Database=AB_SegurosDB2;Trusted_Connection=True;TrustServerCertificate=True;";

    // Constructor privado para evitar crear varias conexiones.
    private AB_Conexion()
    {
        AB_ConexionSql = new SqlConnection(AB_CadenaConexion);
    }

    // Metodo para obtener la conexion existente.
    public static AB_Conexion AB_GetInstance()
    {
        // Si no existe una instancia, se crea por primera vez.
        if (AB_Instancia == null)
        {
            AB_Instancia = new AB_Conexion();
        }

        // Se retorna la instancia creada.
        return AB_Instancia;
    }

    // Metodo para abrir la conexion cuando se va a usar SQL.
    public void AB_OpenConnection()
    {
        // Se revisa que la conexion no este abierta.
        if (AB_ConexionSql.State == ConnectionState.Closed)
        {
            AB_ConexionSql.Open();
        }
    }

    // Metodo para cerrar la conexion al terminar la consulta.
    public void AB_CloseConnection()
    {
        // Se revisa que la conexion este abierta antes de cerrarla.
        if (AB_ConexionSql.State == ConnectionState.Open)
        {
            AB_ConexionSql.Close();
        }
    }

    // Metodo para obtener la conexion que se usara en una transaccion.
    public SqlConnection AB_ObtenerConexion()
    {
        return AB_ConexionSql;
    }

    // Metodo para agregar los valores variables al comando SQL.
    private void AB_AgregarParametros(SqlCommand AB_Comando,
        string[] AB_NombresParametros, object[] AB_ValoresParametros)
    {
        // Se comprueba que exista la misma cantidad de nombres y valores.
        if (AB_NombresParametros.Length != AB_ValoresParametros.Length)
        {
            throw new ArgumentException("La cantidad de nombres y valores de parametros debe ser igual.");
        }

        // Se recorre cada parametro para agregar su valor al comando.
        for (int AB_Indice = 0; AB_Indice < AB_NombresParametros.Length; AB_Indice++)
        {
            AB_Comando.Parameters.AddWithValue(
                AB_NombresParametros[AB_Indice], AB_ValoresParametros[AB_Indice] ?? DBNull.Value);
        }
    }

    // Metodo para ejecutar consultas SELECT y devolver una tabla.
    public DataTable AB_ExecuteQuery(string AB_Consulta)
    {
        // Se ejecuta el SELECT sin parametros y se devuelve su tabla.
        return AB_ExecuteQuery(AB_Consulta, new string[0], new object[0]);
    }

    // Metodo para ejecutar SELECT con valores enviados como parametros.
    public DataTable AB_ExecuteQuery(string AB_Consulta,
        string[] AB_NombresParametros, object[] AB_ValoresParametros)
    {
        // Tabla donde se guardan los datos de la consulta.
        DataTable AB_Tabla = new DataTable();

        // Se intenta ejecutar la consulta SELECT y llenar la tabla de resultados.
        try
        {
            // Se abre la conexion para ejecutar la consulta.
            AB_OpenConnection();

            // Comando que recibe la consulta SQL.
            using (SqlCommand AB_Comando = new SqlCommand(AB_Consulta, AB_ConexionSql))
            {
                // Se agregan los parametros antes de ejecutar la consulta.
                AB_AgregarParametros(AB_Comando, AB_NombresParametros, AB_ValoresParametros);

                // Adaptador que llena la tabla con los resultados.
                using (SqlDataAdapter AB_Adaptador = new SqlDataAdapter(AB_Comando))
                {
                    AB_Adaptador.Fill(AB_Tabla);
                }
            }
        }
        // Se informa si la consulta SELECT no pudo ejecutarse.
        catch (Exception AB_Excepcion)
        {
            Console.WriteLine($"Error al consultar la base de datos: {AB_Excepcion.Message}");
        }
        // La conexion se cierra aunque la consulta SELECT produzca un error.
        finally
        {
            // La conexion siempre se cierra al terminar.
            AB_CloseConnection();
        }

        return AB_Tabla;
    }

    // Metodo para ejecutar INSERT, UPDATE o DELETE.
    public int AB_ExecuteNonQuery(string AB_Consulta)
    {
        // Se ejecuta el cambio sin parametros y se devuelve la cantidad de filas afectadas.
        return AB_ExecuteNonQuery(AB_Consulta, new string[0], new object[0]);
    }

    // Metodo para ejecutar INSERT, UPDATE o DELETE con parametros.
    public int AB_ExecuteNonQuery(string AB_Consulta,string[] AB_NombresParametros, object[] AB_ValoresParametros)
    {
        // Variable para guardar el numero de filas afectadas.
        int AB_FilasAfectadas = 0;

        // Se intenta ejecutar el INSERT, UPDATE o DELETE recibido.
        try
        {
            // Se abre la conexion para ejecutar la operacion.
            AB_OpenConnection();

            // Comando para insertar, actualizar o eliminar datos.
            using (SqlCommand AB_Comando = new SqlCommand(AB_Consulta, AB_ConexionSql))
            {
                // Se agregan los parametros antes de ejecutar la operacion.
                AB_AgregarParametros(AB_Comando, AB_NombresParametros, AB_ValoresParametros);
                AB_FilasAfectadas = AB_Comando.ExecuteNonQuery();
            }
        }
        // Se informa si el INSERT, UPDATE o DELETE no pudo ejecutarse.
        catch (Exception AB_Excepcion)
        {
            Console.WriteLine($"Error al ejecutar la operacion: {AB_Excepcion.Message}");
        }
        // La conexion se cierra aunque el cambio en la base de datos fuera un error.
        finally
        {
            // La conexion siempre se cierra al terminar.
            AB_CloseConnection();
        }

        return AB_FilasAfectadas;
    }

    // Metodo para guardar las acciones importantes en la tabla de logs.
    public void AB_RegistrarLog(string AB_Modulo, string AB_Accion, string AB_Mensaje)
    {
        // Se crea el objeto 
        AB_LogSistema AB_Log = new AB_LogSistema(0, DateTime.Now, "INFORMACION",AB_Modulo, AB_Accion, AB_Mensaje, "", "", "");

        // Consulta INSERT para guardar el registro del sistema.
        string AB_ConsultaRegistro =
            "INSERT INTO AB_LogSistema " +
            "(AB_FechaHora, AB_Nivel, AB_Modulo, AB_Accion, AB_Mensaje, AB_DetalleTecnico, AB_Usuario, AB_DireccionIP) " +
            "VALUES " +
            "(@FechaHora, @Nivel, @Modulo, @Accion, @Mensaje, NULL, NULL, NULL)";

        // Se ejecuta el INSERT sin interrumpir el proceso principal.
        AB_ExecuteNonQuery(AB_ConsultaRegistro,
            new string[] { "@FechaHora", "@Nivel", "@Modulo", "@Accion", "@Mensaje" },
            new object[] { AB_Log.AB_FechaHora, AB_Log.AB_Nivel, AB_Log.AB_Modulo,
                AB_Log.AB_Accion, AB_Log.AB_Mensaje });
    }
}

