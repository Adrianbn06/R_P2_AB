using System;
using System.Data;

namespace SistemaSeguros;

class AB_Program
{
    static void Main(string[] AB_Argumentos)
    {
        // -----------------------------------------
        // PRIMERA PARTE - CONEXION
        // -----------------------------------------

        // Prueba inicial para verificar la conexion con SQL Server.
        AB_Conexion AB_ServidorSql = AB_Conexion.AB_GetInstance();

        // Se guarda el nombre de la base conectada para comprobar la conexion.
        DataTable AB_PruebaConexion = AB_ServidorSql.AB_ExecuteQuery("SELECT DB_NAME() AS AB_BaseDatos");

        // Si no se obtiene el nombre de la base, se detiene el programa.
        if (AB_PruebaConexion.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("No se pudo establecer conexion con AB_SegurosDB2.");
            Console.ReadKey();
            return;
        }

        // Mensaje de confirmacion de la base conectada.
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Conexion lista con: {AB_PruebaConexion.Rows[0]["AB_BaseDatos"]}");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;

        // Se crea el objeto encargado de cargar los datos iniciales.
        AB_CargadorDatos AB_Cargador = new AB_CargadorDatos();

        // Se leen los archivos de texto y se guardan sus datos en SQL Server.
        AB_Cargador.AB_QuemarDatosIniciales();
        Console.ReadKey();

        // Se crean los objetos que permiten abrir cada modulo del sistema.
        AB_ModuloEmisiones AB_Emisiones = new AB_ModuloEmisiones();
        AB_ModuloSiniestros AB_Siniestros = new AB_ModuloSiniestros();
        AB_ModuloReaseguros AB_Reaseguros = new AB_ModuloReaseguros();
        AB_ModuloContabilidad AB_Contabilidad = new AB_ModuloContabilidad();
        AB_ModuloExportaciones AB_Exportaciones = new AB_ModuloExportaciones();

        // -----------------------------------------
        // SEGUNDA PARTE - MENU
        // -----------------------------------------

        // Variable para controlar la opcion seleccionada por el usuario en el menu.
        string AB_Opcion = "";

        // El menu se repite hasta que el usuario seleccione la opcion 6.
        while (AB_Opcion != "6")
        {
            Console.Clear();
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("===================================================================");
            Console.WriteLine("                          ASEGURADORA AB                           ");
            Console.WriteLine("===================================================================");
            Console.WriteLine(" 1. Modulo de Emisiones");
            Console.WriteLine(" 2. Modulo de Siniestros");
            Console.WriteLine(" 3. Modulo de Reaseguros");
            Console.WriteLine(" 4. Modulo de Contabilidad");
            Console.WriteLine(" 5. Modulo de Exportacion Manual");
            Console.WriteLine(" 6. Salir");
            Console.WriteLine("===================================================================");
            Console.Write("Seleccione la opcion donde desea ingresar [1-6]: ");

            // Se guarda la opcion ingresada por el usuario.
            AB_Opcion = Console.ReadLine();

            // Depende de la eleccion del usuario.
            switch (AB_Opcion)
            {
                case "1":
                    AB_Emisiones.AB_MenuEmisiones();
                    break;
                case "2":
                    AB_Siniestros.AB_MenuSiniestros();
                    break;
                case "3":
                    AB_Reaseguros.AB_MenuReaseguros();
                    break;
                case "4":
                    AB_Contabilidad.AB_MenuContabilidad();
                    break;
                case "5":
                    AB_Exportaciones.AB_MenuExportaciones();
                    break;
                case "6":
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine("\nSaliendo.........................");
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOpcion incorrecta. Ingrese una opcion entre 1 y 6.");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Presione cualquier tecla para continuar...");
                    Console.ReadKey();
                    break;
            }
        }
    }
}


