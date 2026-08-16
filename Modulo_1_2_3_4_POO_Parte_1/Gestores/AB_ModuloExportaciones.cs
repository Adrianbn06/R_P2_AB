using System;
using System.Data;

namespace SistemaSeguros;

public class AB_ModuloExportaciones
{
    // Variable para usar la conexion de SQL Server.
    private AB_Conexion AB_BaseDatos = AB_Conexion.AB_GetInstance();

    // Ruta fija dentro del proyecto para guardar las exportaciones manuales.
    private string AB_CarpetaExportaciones = 
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "AB_Exportaciones"));

    public void AB_MenuExportaciones()
    {
        // Variable para guardar la opcion seleccionada.
        string AB_Opcion = "";

        do
        {
            Console.Clear();
            Console.WriteLine("=======================================================");
            Console.WriteLine("             MODULO DE EXPORTACION MANUAL              ");
            Console.WriteLine("=======================================================");
            Console.WriteLine(" 1. Exportar todos los archivos");
            Console.WriteLine(" 2. Exportar polizas");
            Console.WriteLine(" 3. Exportar siniestros");
            Console.WriteLine(" 4. Exportar reaseguros");
            Console.WriteLine(" 5. Exportar asientos contables");
            Console.WriteLine(" 6. Exportar logs del sistema");
            Console.WriteLine(" 7. Exportar clientes");
            Console.WriteLine(" 8. Volver al Menu Principal");
            Console.WriteLine("=======================================================");
            Console.Write("Seleccione una opcion [1-8]: ");
            AB_Opcion = Console.ReadLine();

            // Se ejecuta la exportacion correspondiente a la opcion seleccionada.
            switch (AB_Opcion)
            {
                case "1":
                    AB_ExportarTodo();
                    break;
                case "2":
                    AB_ExportarPolizas();
                    break;
                case "3":
                    AB_ExportarSiniestros();
                    break;
                case "4":
                    AB_ExportarReaseguros();
                    break;
                case "5":
                    AB_ExportarAsientos();
                    break;
                case "6":
                    AB_ExportarLogs();
                    break;
                case "7":
                    AB_ExportarClientes();
                    break;
                case "8":
                    break;
                default:
                    Console.WriteLine("Opcion no valida.");
                    AB_Pausar();
                    break;
            }
        }
        while (AB_Opcion != "8");
    }

    // Metodo para exportar todos los archivos disponibles.
    private void AB_ExportarTodo()
    {
        // Se exportan los datos de cada modulo.
        AB_ExportarArchivo("AB_ExportacionPolizas.txt", "Poliza|Cliente|Ramo|Capital|Prima|Estado",
            "SELECT p.AB_NumeroPoliza, c.AB_Nombres + ' ' + c.AB_Apellidos AS AB_Cliente, " +
            "r.AB_NombreRamo, p.AB_CapitalAsegurado, p.AB_PrimaTotal, p.AB_Estado " +
            "FROM AB_Poliza p " +
            "INNER JOIN AB_Cliente c " +
            "ON p.AB_IdCliente = c.AB_IdCliente " +
            "INNER JOIN AB_Ramo r " +
            "ON p.AB_IdRamo = r.AB_IdRamo",
            new string[] { "AB_NumeroPoliza", "AB_Cliente", "AB_NombreRamo", "AB_CapitalAsegurado", "AB_PrimaTotal", "AB_Estado" });

        AB_ExportarArchivo("AB_ExportacionSiniestros.txt", "Reclamo|Poliza|Danos|Deducible|PagoNeto|Estado",
            "SELECT s.AB_NumeroReclamo, p.AB_NumeroPoliza, s.AB_DanosReclamados, " +
            "s.AB_DeducibleAsumido, s.AB_PagoNeto, s.AB_EstadoAuditoria " +
            "FROM AB_Siniestro s " +
            "INNER JOIN AB_Poliza p " +
            "ON s.AB_IdPoliza = p.AB_IdPoliza",
            new string[] { "AB_NumeroReclamo", "AB_NumeroPoliza", "AB_DanosReclamados", "AB_DeducibleAsumido", "AB_PagoNeto", "AB_EstadoAuditoria" });

        AB_ExportarArchivo("AB_ExportacionReaseguros.txt", "Poliza|Reaseguradora|Retencion|Contrato|Facultativo",
            "SELECT p.AB_NumeroPoliza, r.AB_Codigo AS AB_Reaseguradora, rr.AB_RetencionPropia, " +
            "rr.AB_CapitalContrato, rr.AB_CapitalFacultativo " +
            "FROM AB_RepartoReaseguro rr " +
            "INNER JOIN AB_Poliza p " +
            "ON rr.AB_IdPoliza = p.AB_IdPoliza " +
            "INNER JOIN AB_Reaseguradora r " +
            "ON rr.AB_IdReaseguradora = r.AB_IdReaseguradora",
            new string[] { "AB_NumeroPoliza", "AB_Reaseguradora", "AB_RetencionPropia", "AB_CapitalContrato", "AB_CapitalFacultativo" });

        AB_ExportarArchivo("AB_ExportacionAsientos.txt", "Comprobante|Fecha|Modulo|Cuenta|Debe|Haber",
            "SELECT c.AB_NumeroComprobante, c.AB_FechaTransaccion, c.AB_ModuloOrigen, " +
            "cu.AB_CodigoCuenta, d.AB_ValorDebe, d.AB_ValorHaber " +
            "FROM AB_CabeceraAsiento c " +
            "INNER JOIN AB_DetalleAsiento d " +
            "ON c.AB_IdAsiento = d.AB_IdAsiento " +
            "INNER JOIN AB_CuentaContable cu " +
            "ON d.AB_IdCuenta = cu.AB_IdCuenta",
            new string[] { "AB_NumeroComprobante", "AB_FechaTransaccion", "AB_ModuloOrigen", "AB_CodigoCuenta", "AB_ValorDebe", "AB_ValorHaber" });

        AB_ExportarArchivo("AB_ExportacionLogs.txt", "Fecha|Nivel|Modulo|Accion|Mensaje",
            "SELECT AB_FechaHora, AB_Nivel, AB_Modulo, AB_Accion, AB_Mensaje " +
            "FROM AB_LogSistema",
            new string[] { "AB_FechaHora", "AB_Nivel", "AB_Modulo", "AB_Accion", "AB_Mensaje" });

        AB_ExportarArchivo("AB_ExportacionClientes.txt", "Cedula|Nombres|Apellidos|Telefono|Correo",
            "SELECT AB_Cedula, AB_Nombres, AB_Apellidos, AB_Telefono, AB_Correo " +
            "FROM AB_Cliente",
            new string[] { "AB_Cedula", "AB_Nombres", "AB_Apellidos", "AB_Telefono", "AB_Correo" });

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nTodos los archivos fueron exportados correctamente.");
        Console.ForegroundColor = ConsoleColor.White;
        AB_Pausar();
    }

    // Metodo para exportar solo las polizas.
    private void AB_ExportarPolizas()
    {
        AB_ExportarArchivo("AB_ExportacionPolizas.txt", "Poliza|Cliente|Ramo|Capital|Prima|Estado",
            "SELECT p.AB_NumeroPoliza, c.AB_Nombres + ' ' + c.AB_Apellidos AS AB_Cliente, " +
            "r.AB_NombreRamo, p.AB_CapitalAsegurado, p.AB_PrimaTotal, p.AB_Estado " +
            "FROM AB_Poliza p " +
            "INNER JOIN AB_Cliente c " +
            "ON p.AB_IdCliente = c.AB_IdCliente " +
            "INNER JOIN AB_Ramo r " +
            "ON p.AB_IdRamo = r.AB_IdRamo",
            new string[] { "AB_NumeroPoliza", "AB_Cliente", "AB_NombreRamo", "AB_CapitalAsegurado", "AB_PrimaTotal", "AB_Estado" });
        AB_Pausar();
    }

    // Metodo para exportar solo los siniestros.
    private void AB_ExportarSiniestros()
    {
        AB_ExportarArchivo("AB_ExportacionSiniestros.txt", "Reclamo|Poliza|Danos|Deducible|PagoNeto|Estado",
            "SELECT s.AB_NumeroReclamo, p.AB_NumeroPoliza, s.AB_DanosReclamados, " +
            "s.AB_DeducibleAsumido, s.AB_PagoNeto, s.AB_EstadoAuditoria " +
            "FROM AB_Siniestro s " +
            "INNER JOIN AB_Poliza p " +
            "ON s.AB_IdPoliza = p.AB_IdPoliza",
            new string[] { "AB_NumeroReclamo", "AB_NumeroPoliza", "AB_DanosReclamados", "AB_DeducibleAsumido", "AB_PagoNeto", "AB_EstadoAuditoria" });
        AB_Pausar();
    }

    // Metodo para exportar solo los reaseguros.
    private void AB_ExportarReaseguros()
    {
        AB_ExportarArchivo("AB_ExportacionReaseguros.txt", "Poliza|Reaseguradora|Retencion|Contrato|Facultativo",
            "SELECT p.AB_NumeroPoliza, r.AB_Codigo AS AB_Reaseguradora, rr.AB_RetencionPropia, " +
            "rr.AB_CapitalContrato, rr.AB_CapitalFacultativo " +
            "FROM AB_RepartoReaseguro rr " +
            "INNER JOIN AB_Poliza p " +
            "ON rr.AB_IdPoliza = p.AB_IdPoliza " +
            "INNER JOIN AB_Reaseguradora r " +
            "ON rr.AB_IdReaseguradora = r.AB_IdReaseguradora",
            new string[] { "AB_NumeroPoliza", "AB_Reaseguradora", "AB_RetencionPropia", "AB_CapitalContrato", "AB_CapitalFacultativo" });
        AB_Pausar();
    }

    // Metodo para exportar solo los asientos contables.
    private void AB_ExportarAsientos()
    {
        AB_ExportarArchivo("AB_ExportacionAsientos.txt", "Comprobante|Fecha|Modulo|Cuenta|Debe|Haber",
            "SELECT c.AB_NumeroComprobante, c.AB_FechaTransaccion, c.AB_ModuloOrigen, " +
            "cu.AB_CodigoCuenta, d.AB_ValorDebe, d.AB_ValorHaber " +
            "FROM AB_CabeceraAsiento c " +
            "INNER JOIN AB_DetalleAsiento d " +
            "ON c.AB_IdAsiento = d.AB_IdAsiento " +
            "INNER JOIN AB_CuentaContable cu " +
            "ON d.AB_IdCuenta = cu.AB_IdCuenta",
            new string[] { "AB_NumeroComprobante", "AB_FechaTransaccion", "AB_ModuloOrigen", "AB_CodigoCuenta", "AB_ValorDebe", "AB_ValorHaber" });
        AB_Pausar();
    }

    // Metodo para exportar solo los logs del sistema.
    private void AB_ExportarLogs()
    {
        AB_ExportarArchivo("AB_ExportacionLogs.txt", "Fecha|Nivel|Modulo|Accion|Mensaje",
            "SELECT AB_FechaHora, AB_Nivel, AB_Modulo, AB_Accion, AB_Mensaje " +
            "FROM AB_LogSistema",
            new string[] { "AB_FechaHora", "AB_Nivel", "AB_Modulo", "AB_Accion", "AB_Mensaje" });
        AB_Pausar();
    }

    // Metodo para exportar solo los clientes.
    private void AB_ExportarClientes()
    {
        AB_ExportarArchivo("AB_ExportacionClientes.txt", "Cedula|Nombres|Apellidos|Telefono|Correo",
            "SELECT AB_Cedula, AB_Nombres, AB_Apellidos, AB_Telefono, AB_Correo " +
            "FROM AB_Cliente",
            new string[] { "AB_Cedula", "AB_Nombres", "AB_Apellidos", "AB_Telefono", "AB_Correo" });
        AB_Pausar();
    }

    // Metodo para GUARDAR una consulta SQL dentro de un archivo TXT (.txt).
    private void AB_ExportarArchivo(string AB_NombreArchivo, string AB_Encabezado, string AB_Consulta, string[] AB_Columnas)
    {
        // Se guardan los datos de SQL Server que se escribiran en el archivo TXT.
        DataTable AB_Tabla = AB_BaseDatos.AB_ExecuteQuery(AB_Consulta);

        // Se forma la ruta completa del archivo que se va a crear.
        string AB_RutaArchivo = Path.Combine(AB_CarpetaExportaciones, AB_NombreArchivo);

        // Se intenta crear o reemplazar el archivo de exportacion.
        try
        {
            // Se crea la carpeta solo si todavia no existe.
            if (!Directory.Exists(AB_CarpetaExportaciones))
            {
                Directory.CreateDirectory(AB_CarpetaExportaciones);
            }

            // El archivo se reemplaza para evitar registros duplicados.
            using (StreamWriter AB_Escritor = new StreamWriter(AB_RutaArchivo, false))
            {
                // Se escribe el encabezado para identificar las columnas.
                AB_Escritor.WriteLine(AB_Encabezado);

                // Ciclo para guardar cada fila de la consulta.
                foreach (DataRow AB_Fila in AB_Tabla.Rows)
                {
                    // Variable que arma una linea del archivo con los datos de cada fila.
                    string AB_Linea = "";

                    // Ciclo para armar una linea separada por el caracter pipe.
                    for (int AB_Indice = 0; AB_Indice < AB_Columnas.Length; AB_Indice++)
                    {
                        // Se agrega el separador despues de la primera columna.
                        if (AB_Indice > 0)
                        {
                            AB_Linea = AB_Linea + "|";
                        }

                        AB_Linea = AB_Linea + AB_Fila[AB_Columnas[AB_Indice]].ToString();
                    }

                    AB_Escritor.WriteLine(AB_Linea);
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nArchivo exportado: {AB_RutaArchivo}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        // Se captura el error producido al crear o escribir el archivo.
        catch (IOException AB_Excepcion)
        {
            // Se informa si el archivo esta abierto o no se puede crear.
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"No se pudo exportar {AB_RutaArchivo}: {AB_Excepcion.Message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
    }

    // Metodo que espera ENTER antes de volver al menu.
    private void AB_Pausar()
    {
        Console.WriteLine("\nPresione ENTER para continuar...");
        Console.ReadLine();
    }
}
