using System;
using System.Data;
using System.Globalization;
using System.IO;

namespace SistemaSeguros;

public class AB_ModuloEmisiones
{
    // Variable para usar la conexion de SQL Server.
    private AB_Conexion AB_BaseDatos = AB_Conexion.AB_GetInstance();

    // Ruta fija dentro del proyecto para guardar el respaldo de polizas.
    private string AB_RutaPolizasEmitidas = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "AB_Exportaciones", "AB_PolizasEmitidas.txt"));

    // MENU
    public void AB_MenuEmisiones()
    {
        // Variable para guardar la opcion seleccionada.
        string AB_Opcion = "";

        do
        {
            Console.Clear();
            Console.WriteLine("=======================================================");
            Console.WriteLine("                 MODULO DE EMISIONES                   ");
            Console.WriteLine("=======================================================");
            Console.WriteLine(" 1. Emitir Nueva Poliza");
            Console.WriteLine(" 2. Consultar Poliza Guardada");
            Console.WriteLine(" 3. Modificar Capital de Poliza");
            Console.WriteLine(" 4. Dar de Baja Poliza");
            Console.WriteLine(" 5. Consultar Total de Primas Recaudadas");
            Console.WriteLine(" 6. Volver al Menu Principal");
            Console.WriteLine("=======================================================");
            Console.Write("Seleccione una opcion [1-6]: ");

            AB_Opcion = Console.ReadLine();

            // Se ejecuta la accion correspondiente a la opcion seleccionada por el usuario.
            switch (AB_Opcion)
            {
                case "1":
                    AB_EmitirPoliza();
                    break;
                case "2":
                    AB_ConsultarPoliza();
                    break;
                case "3":
                    AB_ModificarCapitalPoliza();
                    break;
                case "4":
                    AB_DarDeBajaPoliza();
                    break;
                case "5":
                    AB_ConsultarTotalPrimas();
                    break;
                case "6":
                    break;
                default:
                    Console.WriteLine("Opcion no valida.");
                    AB_Pausar();
                    break;
            }

        }
        while (AB_Opcion != "6");
    }

    // ---------------------------------------------------------------------------
    // 1. Metodo para REGISTRAR una nueva poliza
    // ---------------------------------------------------------------------------

    private void AB_EmitirPoliza()
    {
        // Se solicita la cedula para buscar al cliente.
        Console.Clear();
        Console.WriteLine("--- EMITIR NUEVA POLIZA ---");
        Console.Write("Ingrese la cedula del cliente: ");
        string AB_Cedula = Console.ReadLine();

        // Consulta para obtener los datos del cliente.
        string AB_ConsultaCliente =
            "SELECT AB_IdCliente, AB_Cedula, AB_Nombres, AB_Apellidos, " +
            "AB_Direccion, AB_Telefono, AB_Correo " +
            "FROM AB_Cliente " +
            "WHERE AB_Cedula = @Cedula";

        // Se guardan los datos del cliente encontrados en la consulta.
        DataTable AB_TablaClientes = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaCliente,
            new string[] { "@Cedula" }, new object[] { AB_Cedula });

        // Si el cliente no existe, se cancela la emision.
        if (AB_TablaClientes.Rows.Count == 0)
        {
            // Se prepara el error que informa que el cliente no fue encontrado.
            try
            {
                // Se lanza la excepcion cuando no existe el cliente para emitir.
                throw new AB_PolizaInvalidaException("No se puede emitir una poliza para un cliente no encontrado en la base de datos.");
            }
            // Se muestra el error de la poliza y se cancela la emision.
            catch (AB_PolizaInvalidaException AB_Excepcion)
            {
                // Se muestra el mensaje de la excepcion y se vuelve al menu.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nERROR DE POLIZA {AB_Excepcion.Message}");
                Console.ForegroundColor = ConsoleColor.White;
                AB_Pausar();
                return;
            }
        }

        // Se convierte la fila encontrada en un objeto cliente.
        DataRow AB_FilaCliente = AB_TablaClientes.Rows[0];

        AB_Cliente AB_ClienteEncontrado = new AB_Cliente(
            Convert.ToInt32(AB_FilaCliente["AB_IdCliente"]),
            AB_FilaCliente["AB_Cedula"].ToString(),
            AB_FilaCliente["AB_Nombres"].ToString(),
            AB_FilaCliente["AB_Apellidos"].ToString(),
            AB_FilaCliente["AB_Direccion"].ToString(),
            AB_FilaCliente["AB_Telefono"].ToString(),
            AB_FilaCliente["AB_Correo"].ToString());

        int AB_IdCliente = AB_ClienteEncontrado.AB_IdCliente;

        // Se unen los nombres y apellidos para mostrar el nombre completo.
        string AB_NombreCliente = AB_ClienteEncontrado.AB_Nombres + " " +AB_ClienteEncontrado.AB_Apellidos;

        // Se guardan las alertas UAF del cliente para revisar sus codigos.
        DataTable AB_TablaAlertas = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdAlerta, AB_IdCliente, AB_CodigoAlerta, AB_NivelRiesgo, AB_FechaReporte " +
            "FROM AB_AlertaUAF " +
            "WHERE AB_IdCliente = @IdCliente",
            new string[] { "@IdCliente" }, new object[] { AB_IdCliente });

        // Ciclo para revisar cada alerta encontrada.
        for (int AB_Indice = 0; AB_Indice < AB_TablaAlertas.Rows.Count; AB_Indice++)
        {
            // Se convierte la fila en una alerta antes de revisar su codigo.
            DataRow AB_FilaAlerta = AB_TablaAlertas.Rows[AB_Indice];

            AB_AlertaUAF AB_AlertaActual = new AB_AlertaUAF(
                Convert.ToInt32(AB_FilaAlerta["AB_IdAlerta"]),
                Convert.ToInt32(AB_FilaAlerta["AB_IdCliente"]),
                AB_FilaAlerta["AB_CodigoAlerta"].ToString(),
                AB_FilaAlerta["AB_NivelRiesgo"].ToString(),
                Convert.ToDateTime(AB_FilaAlerta["AB_FechaReporte"]));

            // El codigo 999 bloquea la emision de la poliza.
            if (AB_AlertaActual.AB_CodigoAlerta == "999")
            {
                // Se prepara el error que bloquea la emision por posible fraude.
                try
                {
                    // Se lanza la excepcion critica por fraude UAF.
                    throw new AB_FraudeUAFException("Emision bloqueada: el cliente tiene alerta UAF 999.");
                }
                // Se muestra la alerta de fraude y se cancela la emision.
                catch (AB_FraudeUAFException AB_Excepcion)
                {
                    // Se muestra el mensaje de fraude y se detiene la emision.
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\nALERTA UAF {AB_Excepcion.Message}");
                    Console.ForegroundColor = ConsoleColor.White;
                    AB_Pausar();
                    return;
                }
            }

            // El codigo 404 solo muestra una advertencia documental.
            if (AB_AlertaActual.AB_CodigoAlerta == "404")
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Advertencia UAF 404: error documental detectado.");
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.White;
            }
        }

        // Se guardan los ramos disponibles para que el usuario elija uno.
        DataTable AB_TablaRamos = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdRamo, AB_CodigoRamo, AB_NombreRamo " +
            "FROM AB_Ramo " +
            "ORDER BY AB_IdRamo");

        // Si no hay ramos, no se puede emitir la poliza.
        if (AB_TablaRamos.Rows.Count == 0)
        {
            Console.WriteLine("No hay ramos registrados.");
            AB_Pausar();
            return;
        }

        Console.WriteLine("\nCatalogo de ramos:");

        // Ciclo para mostrar cada ramo disponible.
        for (int AB_Indice = 0; AB_Indice < AB_TablaRamos.Rows.Count; AB_Indice++)
        {
            // Cada ramo se presenta 
            DataRow AB_FilaCatalogo = AB_TablaRamos.Rows[AB_Indice];

            AB_Ramo AB_RamoCatalogo = new AB_Ramo(
                Convert.ToInt32(AB_FilaCatalogo["AB_IdRamo"]),
                AB_FilaCatalogo["AB_CodigoRamo"].ToString(),
                AB_FilaCatalogo["AB_NombreRamo"].ToString());
            Console.WriteLine($"{AB_Indice + 1}. {AB_RamoCatalogo.AB_NombreRamo}");
        }

        // Variable donde se guarda la opcion de ramo seleccionada.
        int AB_OpcionRamo = 0;

        Console.Write("Seleccione un ramo: ");

        // Ciclo para validar la opcion de ramo ingresada.
        while (!int.TryParse(Console.ReadLine(), out AB_OpcionRamo) || AB_OpcionRamo < 1 || AB_OpcionRamo > AB_TablaRamos.Rows.Count)
        {
            Console.Write("Opcion invalida. Seleccione nuevamente: ");
        }

        // Se convierte el ramo elegido en un objeto.
        DataRow AB_FilaRamo = AB_TablaRamos.Rows[AB_OpcionRamo - 1];

        AB_Ramo AB_RamoSeleccionado = new AB_Ramo(
            Convert.ToInt32(AB_FilaRamo["AB_IdRamo"]),
            AB_FilaRamo["AB_CodigoRamo"].ToString(),
            AB_FilaRamo["AB_NombreRamo"].ToString());

        int AB_IdRamo = AB_RamoSeleccionado.AB_IdRamo;

        string AB_NombreRamo = AB_RamoSeleccionado.AB_NombreRamo;

        // Se solicita el capital asegurado de la poliza.
        double AB_Capital = 0;
        Console.Write("Ingrese el capital asegurado: ");

        // Se repite la lectura hasta que se ingrese un capital positivo.
        while (!double.TryParse(Console.ReadLine(), out AB_Capital) || AB_Capital <= 0)
        {
            Console.Write("Ingrese un valor mayor a cero: ");
        }

        // Se solicita la tasa de riesgo de la poliza.
        double AB_TasaRiesgo = 0;
        Console.Write("Ingrese la tasa de riesgo (%): ");

        // Se repite la lectura hasta que se ingrese una tasa valida
        while (!double.TryParse(Console.ReadLine(), out AB_TasaRiesgo) ||
            AB_TasaRiesgo <= 0 || AB_TasaRiesgo > 100)
        {
            Console.Write("Ingrese un porcentaje mayor a 0: ");
        }

        // Calculos 
        double AB_PrimaBase = AB_Capital * (AB_TasaRiesgo / 100.0);
        double AB_DerechoEmision = AB_Capital > 40000 ? 2.00 :AB_Capital > 10000 ? 1.00 : 0.50;
        double AB_SuperBancos = AB_PrimaBase * 0.035;
        double AB_SeguroCampesino = AB_PrimaBase * 0.005;
        double AB_Subtotal = AB_PrimaBase + AB_SuperBancos + AB_SeguroCampesino + AB_DerechoEmision;
        double AB_IVA = AB_Subtotal * 0.15;
        double AB_PrimaTotal = AB_Subtotal + AB_IVA;

        // Se consulta el ultimo identificador para formar el siguiente numero de poliza.
        DataTable AB_TablaUltimaPoliza = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT MAX(AB_IdPoliza) AS AB_UltimoId " +
            "FROM AB_Poliza");

        int AB_UltimoIdPoliza = 0;

        // Se toma el ultimo identificador guardado cuando existe una poliza anterior.
        if (AB_TablaUltimaPoliza.Rows.Count > 0 &&AB_TablaUltimaPoliza.Rows[0]["AB_UltimoId"] != DBNull.Value)
        {
            AB_UltimoIdPoliza =Convert.ToInt32(AB_TablaUltimaPoliza.Rows[0]["AB_UltimoId"]);
        }

        // Se genera el numero que identificara la nueva poliza.
        string AB_NumeroPoliza = "POL-" + (AB_UltimoIdPoliza + 1).ToString("000");

        // Se crea el objeto poliza con todos los valores calculados.
        AB_Poliza AB_NuevaPoliza = new AB_Poliza(0, AB_IdCliente, AB_IdRamo,
            AB_NumeroPoliza, AB_Capital, AB_TasaRiesgo, AB_PrimaBase,
            AB_SuperBancos, AB_SeguroCampesino, AB_DerechoEmision, AB_IVA,
            AB_PrimaTotal, AB_Capital, "ACTIVA");

        // INSERT para guardar la nueva poliza.
        string AB_ConsultaInsertar =
            "INSERT INTO AB_Poliza " +
            "(AB_IdCliente, AB_IdRamo, AB_NumeroPoliza, AB_CapitalAsegurado, AB_TasaRiesgo, " +
            "AB_PrimaBase, AB_SuperBancos, AB_SeguroCampesino, AB_DerechosEmision, AB_IVA, " +
            "AB_PrimaTotal, AB_CapitalRemanente, AB_Estado) " +
            "VALUES " +
            "(@IdCliente, @IdRamo, @NumeroPoliza, @CapitalAsegurado, @TasaRiesgo, " +
            "@PrimaBase, @SuperBancos, @SeguroCampesino, @DerechosEmision, @IVA, " +
            "@PrimaTotal, @CapitalRemanente, @Estado)";

        // Se ejecuta el guardado y se comprueba si se registro la poliza.
        int AB_FilasAfectadas = AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaInsertar,
            new string[] { "@IdCliente", "@IdRamo", "@NumeroPoliza", "@CapitalAsegurado",
                "@TasaRiesgo", "@PrimaBase", "@SuperBancos", "@SeguroCampesino",
                "@DerechosEmision", "@IVA", "@PrimaTotal", "@CapitalRemanente", "@Estado" },
            new object[] { AB_NuevaPoliza.AB_IdCliente, AB_NuevaPoliza.AB_IdRamo,
                AB_NuevaPoliza.AB_NumeroPoliza, AB_NuevaPoliza.AB_CapitalAsegurado,
                AB_NuevaPoliza.AB_TasaRiesgo, AB_NuevaPoliza.AB_PrimaBase,
                AB_NuevaPoliza.AB_SuperBancos, AB_NuevaPoliza.AB_SeguroCampesino,
                AB_NuevaPoliza.AB_DerechosEmision, AB_NuevaPoliza.AB_IVA,
                AB_NuevaPoliza.AB_PrimaTotal, AB_NuevaPoliza.AB_CapitalRemanente,
                AB_NuevaPoliza.AB_Estado });

        // Si el INSERT falla, se muestra el mensaje de error.
        if (AB_FilasAfectadas == 0)
        {
            Console.WriteLine("No se pudo guardar la poliza.");
            AB_Pausar();
            return;
        }

        // Se guarda una copia de la poliza emitida en el archivo de respaldo.
        AB_GuardarPolizaEnArchivo(AB_NuevaPoliza.AB_NumeroPoliza,
            AB_ClienteEncontrado.AB_Cedula, AB_NuevaPoliza.AB_IdRamo,
            AB_NuevaPoliza.AB_CapitalAsegurado, AB_NuevaPoliza.AB_PrimaTotal,
            AB_NuevaPoliza.AB_Estado);

        // Se registra la emision correcta en la tabla de logs.
        AB_BaseDatos.AB_RegistrarLog("EMISIONES", "EMITIR POLIZA", "Poliza " + AB_NumeroPoliza + " emitida correctamente.");

        // Salida
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=======================================================");
        Console.WriteLine("          POLIZA GENERADA CON EXITO                    ");
        Console.WriteLine("=======================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Cliente:\t\t{AB_NombreCliente}");
        Console.WriteLine($"Cedula:\t\t{AB_Cedula}");
        Console.WriteLine($"Poliza:\t\t{AB_NumeroPoliza}");
        Console.WriteLine($"Ramo:\t\t\t{AB_NombreRamo}");
        Console.WriteLine($"Capital Asegurado:\t${AB_Capital:F2}");
        Console.WriteLine($"Capital Remanente:\t${AB_Capital:F2}");
        Console.WriteLine("=======================================================");
        Console.WriteLine("\n--- DATOS FINANCIEROS ---");
        Console.WriteLine("=======================================================");
        Console.WriteLine($"Prima Base:\t\t${AB_PrimaBase:F2}");
        Console.WriteLine($"Super de Bancos:\t${AB_SuperBancos:F2}");
        Console.WriteLine($"Seguro Campesino:\t${AB_SeguroCampesino:F2}");
        Console.WriteLine($"Derecho Emision:\t${AB_DerechoEmision:F2}");
        Console.WriteLine($"IVA (15%):\t\t${AB_IVA:F2}");
        Console.WriteLine("=======================================================");
        Console.WriteLine($"PRIMA TOTAL NETA:\t${AB_PrimaTotal:F2}");
        Console.WriteLine("=======================================================");
        AB_Pausar();
    }

    // ---------------------------------------------------------------------------
    // 2. Metodo para CONSULTAR una poliza o listar todas.
    // ---------------------------------------------------------------------------

    private void AB_ConsultarPoliza()
    {
        Console.Clear();
        Console.WriteLine("--- CONSULTAR POLIZAS ---");
        Console.Write("Ingrese el numero de poliza o presione ENTER para listar todas: ");

        // Se recibe el numero de poliza que se desea consultar.
        string AB_NumeroPoliza = Console.ReadLine();

        // Consulta que muestra una poliza especifica o todas las registradas.
        string AB_Consulta =
            "SELECT p.AB_IdPoliza, p.AB_IdCliente, p.AB_IdRamo, p.AB_NumeroPoliza, c.AB_Cedula, " +
            "c.AB_Nombres + ' ' + c.AB_Apellidos AS AB_Cliente, " +
            "r.AB_NombreRamo, p.AB_CapitalAsegurado, p.AB_TasaRiesgo, p.AB_PrimaBase, " +
            "p.AB_SuperBancos, p.AB_SeguroCampesino, p.AB_DerechosEmision, p.AB_IVA, " +
            "p.AB_PrimaTotal, p.AB_CapitalRemanente, p.AB_Estado " +
            "FROM AB_Poliza p " +
            "INNER JOIN AB_Cliente c " +
            "ON p.AB_IdCliente = c.AB_IdCliente " +
            "INNER JOIN AB_Ramo r " +
            "ON p.AB_IdRamo = r.AB_IdRamo";

        // Si se ingreso un numero, se agrega el filtro para buscar solo esa poliza.
        DataTable AB_TablaPolizas;

        // Si se escribio un numero de poliza, la consulta se ejecuta con ese filtro.
        if (AB_NumeroPoliza != "")
        {
            // Se le agrega una condicion, para hacer el filtrado.
            AB_Consulta = AB_Consulta + " " +
                "WHERE p.AB_NumeroPoliza = @NumeroPoliza";

            // Se guardan solamente los datos de la poliza solicitada.
            AB_TablaPolizas = AB_BaseDatos.AB_ExecuteQuery(AB_Consulta,
                new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });
        }
        else
        {
            // Se guardan todas las polizas cuando no se ingreso un numero.
            AB_TablaPolizas = AB_BaseDatos.AB_ExecuteQuery(AB_Consulta);
        }

        // Se informa si no existen polizas que coincidan con la busqueda.
        if (AB_TablaPolizas.Rows.Count == 0)
        {
            Console.WriteLine("No hay polizas para mostrar.");
            AB_Pausar();
            return;
        }

        // Se recorre cada póliza encontrada para mostrar su información.
        for (int AB_Indice = 0; AB_Indice < AB_TablaPolizas.Rows.Count; AB_Indice++)
        {
            // Se toma la fila actual para mostrar los datos de esa poliza.
            DataRow AB_Fila = AB_TablaPolizas.Rows[AB_Indice];

            // Se convierte la fila SQL en una poliza
            AB_Poliza AB_PolizaConsultada = AB_CrearPolizaDesdeFila(AB_Fila);

            // Salida.
            Console.WriteLine("\n=======================================================");
            Console.WriteLine($"INFORMACION DE LA POLIZA: {AB_PolizaConsultada.AB_NumeroPoliza}");
            Console.WriteLine("=======================================================");
            Console.WriteLine($"Cedula Cliente:\t\t{AB_Fila["AB_Cedula"]}");
            Console.WriteLine($"Ramo Contratado:\t{AB_Fila["AB_NombreRamo"]}");
            Console.WriteLine($"Capital Original:\t${AB_PolizaConsultada.AB_CapitalAsegurado:F2}");
            Console.WriteLine($"Capital Remanente:\t${AB_PolizaConsultada.AB_CapitalRemanente:F2}");
            Console.WriteLine($"Estado Actual:\t\t{AB_PolizaConsultada.AB_Estado}");
            Console.WriteLine("=======================================================");
        }
        AB_Pausar();
    }

    // ---------------------------------------------------------------------------
    // 3. Metodo para MODIFICAR una poliza con nuevo capital.
    // ---------------------------------------------------------------------------

    private void AB_ModificarCapitalPoliza()
    {
        Console.Clear();
        Console.WriteLine("--- MODIFICAR CAPITAL DE POLIZA ---");
        Console.Write("Ingrese el numero de poliza: ");

        // Se recibe el numero de la poliza que se va a modificar.
        string AB_NumeroPoliza = Console.ReadLine();

        // Se guardan los valores actuales de la poliza activa que se va a modificar.
        DataTable AB_TablaPolizas = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdPoliza, AB_IdCliente, AB_IdRamo, AB_NumeroPoliza, " +
            "AB_CapitalAsegurado, AB_TasaRiesgo, AB_PrimaBase, AB_SuperBancos, " +
            "AB_SeguroCampesino, AB_DerechosEmision, AB_IVA, AB_PrimaTotal, " +
            "AB_CapitalRemanente, AB_Estado " +
            "FROM AB_Poliza " +
            "WHERE AB_NumeroPoliza = @NumeroPoliza " +
            "AND AB_Estado = 'ACTIVA'",
            new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });

        // Se valida que la poliza exista y se encuentre activa.
        if (AB_TablaPolizas.Rows.Count == 0)
        {
            Console.WriteLine("Poliza no encontrada o inactiva.");
            AB_Pausar();
            return;
        }

        // Se convierte el registro en un objeto.
        AB_Poliza AB_PolizaModificada = AB_CrearPolizaDesdeFila(AB_TablaPolizas.Rows[0]);
        double AB_CapitalAnterior = AB_PolizaModificada.AB_CapitalAsegurado;
        double AB_RemanenteAnterior = AB_PolizaModificada.AB_CapitalRemanente;

        // Muestra e ingreso de capital
        Console.WriteLine($"Capital actual: ${AB_CapitalAnterior:N2}");
        double AB_NuevoCapital = 0;
        Console.Write("Ingrese el nuevo capital asegurado: ");

        // Se repite la lectura hasta que se ingrese un capital positivo.
        while (!double.TryParse(Console.ReadLine(), out AB_NuevoCapital) || AB_NuevoCapital <= 0)
        {
            Console.Write("Ingrese un valor numerico mayor a cero: ");
        }

        // Muestra e ingreso de tasa.
        double AB_NuevaTasa = 0;
        Console.Write("Ingrese la nueva tasa de riesgo (%): ");

        // Se repite la lectura hasta que se ingrese una tasa entre 0.01 y 100.
        while (!double.TryParse(Console.ReadLine(), out AB_NuevaTasa) ||
            AB_NuevaTasa <= 0 || AB_NuevaTasa > 100)
        {
            Console.Write("Ingrese un porcentaje entre 0.01 y 100: ");
        }

        // Se recalculan los valores de la poliza con el nuevo capital.
        double AB_NuevaPrimaBase = AB_NuevoCapital* (AB_NuevaTasa / 100.0);
        double AB_NuevoDerechoEmision = AB_NuevoCapital > 40000 ? 2.00 :AB_NuevoCapital > 10000 ? 1.00 : 0.50;
        double AB_NuevoSuperBancos = AB_NuevaPrimaBase * 0.035;
        double AB_NuevoSeguroCampesino = AB_NuevaPrimaBase * 0.005;
        double AB_NuevoSubtotal = AB_NuevaPrimaBase + AB_NuevoSuperBancos +AB_NuevoSeguroCampesino + AB_NuevoDerechoEmision;
        double AB_NuevoIVA = AB_NuevoSubtotal * 0.15;
        double AB_NuevaPrimaTotal = AB_NuevoSubtotal + AB_NuevoIVA;
        double AB_NuevoRemanente = AB_RemanenteAnterior + (AB_NuevoCapital - AB_CapitalAnterior);

        // Se evita que el capital remanente quede con un valor negativo.
        if (AB_NuevoRemanente < 0)
        {
            AB_NuevoRemanente = 0;
        }

        // Se actualizan los valores del objeto
        AB_PolizaModificada.AB_CapitalAsegurado = AB_NuevoCapital;
        AB_PolizaModificada.AB_TasaRiesgo = AB_NuevaTasa;
        AB_PolizaModificada.AB_PrimaBase = AB_NuevaPrimaBase;
        AB_PolizaModificada.AB_SuperBancos = AB_NuevoSuperBancos;
        AB_PolizaModificada.AB_SeguroCampesino = AB_NuevoSeguroCampesino;
        AB_PolizaModificada.AB_DerechosEmision = AB_NuevoDerechoEmision;
        AB_PolizaModificada.AB_IVA = AB_NuevoIVA;
        AB_PolizaModificada.AB_PrimaTotal = AB_NuevaPrimaTotal;
        AB_PolizaModificada.AB_CapitalRemanente = AB_NuevoRemanente;

        // Consulta que actualiza los valores recalculados de la poliza.
        string AB_ConsultaActualizar =
            "UPDATE AB_Poliza " +
            "SET " +
            "AB_CapitalAsegurado = @CapitalAsegurado, " +
            "AB_TasaRiesgo = @TasaRiesgo, " +
            "AB_PrimaBase = @PrimaBase, " +
            "AB_SuperBancos = @SuperBancos, " +
            "AB_SeguroCampesino = @SeguroCampesino, " +
            "AB_DerechosEmision = @DerechosEmision, " +
            "AB_IVA = @IVA, " +
            "AB_PrimaTotal = @PrimaTotal, " +
            "AB_CapitalRemanente = @CapitalRemanente " +
            "WHERE AB_IdPoliza = @IdPoliza";

        // Si la actualizacion funciona, se registra y se muestran los nuevos valores.
        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaActualizar,
            new string[] { "@CapitalAsegurado", "@TasaRiesgo", "@PrimaBase", "@SuperBancos",
                "@SeguroCampesino", "@DerechosEmision", "@IVA", "@PrimaTotal",
                "@CapitalRemanente", "@IdPoliza" },
            new object[] { AB_PolizaModificada.AB_CapitalAsegurado, AB_PolizaModificada.AB_TasaRiesgo,
                AB_PolizaModificada.AB_PrimaBase, AB_PolizaModificada.AB_SuperBancos,
                AB_PolizaModificada.AB_SeguroCampesino, AB_PolizaModificada.AB_DerechosEmision,
                AB_PolizaModificada.AB_IVA, AB_PolizaModificada.AB_PrimaTotal,
                AB_PolizaModificada.AB_CapitalRemanente, AB_PolizaModificada.AB_IdPoliza }) > 0)
        {
            // Se registra la modificacion correcta en la tabla de logs.
            AB_BaseDatos.AB_RegistrarLog("EMISIONES", "MODIFICAR POLIZA", "Poliza " + AB_NumeroPoliza + " recalculada correctamente.");
            
            // Salida.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("=======================================================");
            Console.WriteLine("        POLIZA RECALCULADA Y ACTUALIZADA CON EXITO    ");
            Console.WriteLine("=======================================================");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Codigo Poliza:\t\t{AB_NumeroPoliza}");
            Console.WriteLine($"Nuevo Capital:\t\t${AB_PolizaModificada.AB_CapitalAsegurado:F2}");
            Console.WriteLine($"Nuevo Remanente:\t${AB_PolizaModificada.AB_CapitalRemanente:F2}");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine($"Nueva Prima Base:\t${AB_PolizaModificada.AB_PrimaBase:F2}");
            Console.WriteLine($"Contribucion Super:\t${AB_PolizaModificada.AB_SuperBancos:F2}");
            Console.WriteLine($"Seguro Campesino:\t${AB_PolizaModificada.AB_SeguroCampesino:F2}");
            Console.WriteLine($"Derecho Emision:\t${AB_PolizaModificada.AB_DerechosEmision:F2}");
            Console.WriteLine($"IVA (15%):\t\t${AB_PolizaModificada.AB_IVA:F2}");
            Console.WriteLine($"NUEVA PRIMA TOTAL NETA:\t${AB_PolizaModificada.AB_PrimaTotal:F2}");
            Console.WriteLine("=======================================================");
        }
        else
        {
            Console.WriteLine("No se pudo actualizar la poliza.");
        }

        AB_Pausar();
    }


    // ---------------------------------------------------------------------------
    // 4. Metodo para DAR DE BAJA una poliza mediante el cambio de estado.
    // ---------------------------------------------------------------------------

    private void AB_DarDeBajaPoliza()
    {
        Console.Clear();
        Console.WriteLine("--- DAR DE BAJA POLIZA ---");
        Console.Write("Ingrese el numero de poliza: ");

        // Se recibe el numero de la poliza que se va a dar de baja.
        string AB_NumeroPoliza = Console.ReadLine();

        // Se guardan los datos y el estado de la poliza que se va a dar de baja.
        DataTable AB_TablaPolizas = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdPoliza, AB_IdCliente, AB_IdRamo, AB_NumeroPoliza, " +
            "AB_CapitalAsegurado, AB_TasaRiesgo, AB_PrimaBase, AB_SuperBancos, " +
            "AB_SeguroCampesino, AB_DerechosEmision, AB_IVA, AB_PrimaTotal, " +
            "AB_CapitalRemanente, AB_Estado " +
            "FROM AB_Poliza " +
            "WHERE AB_NumeroPoliza = @NumeroPoliza",
            new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });

        // Se valida que la poliza indicada exista.
        if (AB_TablaPolizas.Rows.Count == 0)
        {
            Console.WriteLine("La poliza no existe.");
            AB_Pausar();
            return;
        }

        // Se convierte en objeto.
        AB_Poliza AB_PolizaBaja = AB_CrearPolizaDesdeFila(AB_TablaPolizas.Rows[0]);

        // Se impide dar de baja una poliza que ya no esta activa.
        if (AB_PolizaBaja.AB_Estado != "ACTIVA")
        {
            Console.WriteLine($"La poliza ya se encuentra {AB_PolizaBaja.AB_Estado}");
            AB_Pausar();
            return;
        }

        Console.Write("Confirma la baja de la poliza (S/N): ");

        // Se guarda la confirmacion del usuario antes de realizar el cambio.
        string AB_Confirmacion = Console.ReadLine();

        // Si el usuario no confirma, se cancela la baja de la poliza.
        if (AB_Confirmacion != "S" && AB_Confirmacion != "s")
        {
            Console.WriteLine("Operacion cancelada.");
            AB_Pausar();
            return;
        }

        // Se modifica el estado.
        AB_PolizaBaja.AB_Estado = "INACTIVA";

        // Consulta que cambia el estado de la poliza a INACTIVA.
        string AB_ConsultaActualizar =
            "UPDATE AB_Poliza " +
            "SET AB_Estado = @Estado " +
            "WHERE AB_NumeroPoliza = @NumeroPoliza";

        // Si el estado cambia en SQL Server, tambien se actualiza el respaldo TXT.
        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaActualizar,
            new string[] { "@Estado", "@NumeroPoliza" },
            new object[] { AB_PolizaBaja.AB_Estado, AB_PolizaBaja.AB_NumeroPoliza }) > 0)
        {
            AB_ActualizarArchivoPolizas(AB_PolizaBaja.AB_NumeroPoliza);

            // Se registra la eliminacion correcta en la tabla de logs.
            AB_BaseDatos.AB_RegistrarLog("EMISIONES", "DAR DE BAJA POLIZA", "Poliza " + AB_PolizaBaja.AB_NumeroPoliza + " marcada como inactiva.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"\nEliminacion Exitosa. Poliza {AB_PolizaBaja.AB_NumeroPoliza} ha sido cancelada.");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.WriteLine("No se pudo dar de baja la poliza.");
        }

        AB_Pausar();
    }

    // ---------------------------------------------------------------------------
    // 5. Metodo para CALCULAR el TOTAL DE PRIMAS de las polizas activas.
    // ---------------------------------------------------------------------------

    private void AB_ConsultarTotalPrimas()
    {
        Console.Clear();
        Console.WriteLine("--- REPORTE DE PRIMAS RECAUDADAS ---");

        // Se guardan la cantidad de polizas activas y la suma de sus primas.
        DataTable AB_Reporte = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT COUNT(*) AS AB_TotalPolizas, ISNULL(SUM(AB_PrimaTotal), 0) AS AB_TotalPrimas " +
            "FROM AB_Poliza " +
            "WHERE AB_Estado = 'ACTIVA'");

        // Se toman la cantidad de polizas activas y la suma de sus primas.
        // Se toma la cantidad de polizas activas obtenida en el reporte.
        int AB_TotalPolizas = Convert.ToInt32(AB_Reporte.Rows[0]["AB_TotalPolizas"]);

        // Se toma la suma de primas activas obtenida en el reporte.
        double AB_TotalPrimas = Convert.ToDouble(AB_Reporte.Rows[0]["AB_TotalPrimas"]);

        // Se muestran los totales obtenidos en el reporte.
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=======================================================");
        Console.WriteLine("            TOTAL DE PRIMAS RECAUDADAS                 ");
        Console.WriteLine("=======================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Polizas Activas:\t\t{AB_TotalPolizas}");
        Console.WriteLine($"Total Primas Recaudadas:\t${AB_TotalPrimas:F2}");
        Console.WriteLine("=======================================================");
        AB_Pausar();
    }

    // ===========================================================================

    // ---------------------------------------------------------------------------
    //                         5. Metodos ADICIONALES
    // ---------------------------------------------------------------------------

    // ===========================================================================

    // Metodo para guardar una copia de la poliza en el archivo TXT.
    private void AB_GuardarPolizaEnArchivo(string AB_NumeroPoliza, string AB_Cedula, int AB_IdRamo,
        double AB_Capital, double AB_PrimaTotal, string AB_Estado)
    {
        // Se intenta agregar la poliza al archivo de respaldo.
        try
        {
            // Se obtiene la carpeta donde se guardara el archivo.
            string AB_Carpeta = Path.GetDirectoryName(AB_RutaPolizasEmitidas);

            // Se crea la carpeta cuando todavia no existe.
            if (!Directory.Exists(AB_Carpeta))
            {
                Directory.CreateDirectory(AB_Carpeta);
            }

            // Se abre el archivo y se agrega la nueva poliza al final.
            using (StreamWriter AB_Escritor = new StreamWriter(AB_RutaPolizasEmitidas, true))
            {
                // Los numeros se escriben con punto decimal para mantener el formato del TXT.
                AB_Escritor.WriteLine(AB_NumeroPoliza + "|" + AB_Cedula + "|" + AB_IdRamo + "|" +
                    AB_Capital.ToString(CultureInfo.InvariantCulture) + "|" +
                    AB_PrimaTotal.ToString(CultureInfo.InvariantCulture) + "|" + AB_Estado);
            }
        }
        // Se informa si el archivo de respaldo esta abierto o no puede escribirse.
        catch (IOException)
        {
            Console.WriteLine("La poliza fue guardada en SQL Server, pero el archivo TXT esta en uso.");
        }
    }

    // Metodo para actualizar el estado de la poliza en el archivo TXT.
    private void AB_ActualizarArchivoPolizas(string AB_NumeroPoliza)
    {
        // Ruta del archivo donde se guarda la copia de las polizas.
        string AB_Ruta = AB_RutaPolizasEmitidas;

        // Si el respaldo no existe, no hay ningun estado que actualizar.
        if (!File.Exists(AB_Ruta))
        {
            return;
        }

        // Se intenta cambiar la poliza de ACTIVA a INACTIVA en el respaldo.
        try
        {
            // Se leen todas las lineas del archivo de respaldo.
            string[] AB_Lineas = File.ReadAllLines(AB_Ruta);

            // Se recorren las lineas para encontrar la poliza indicada.
            for (int AB_Indice = 0; AB_Indice < AB_Lineas.Length; AB_Indice++)
            {
                // Se valida si la linea pertenece a la poliza que se va a actualizar.
                if (AB_Lineas[AB_Indice].StartsWith(AB_NumeroPoliza + "|"))
                {
                    AB_Lineas[AB_Indice] = AB_Lineas[AB_Indice].Replace("|ACTIVA", "|INACTIVA");
                }
            }

            // Se guardan nuevamente todas las lineas con el estado actualizado.
            File.WriteAllLines(AB_Ruta, AB_Lineas);
        }
        // Se informa si el respaldo esta abierto o no puede actualizarse.
        catch (IOException)
        {
            Console.WriteLine("La baja se guardo en SQL Server, pero el archivo TXT esta en uso.");
        }
    }

    // Metodo que transforma una fila de AB_Poliza en una entidad del sistema.
    private AB_Poliza AB_CrearPolizaDesdeFila(DataRow AB_Fila)
    {
        // Las columnas permitidas como NULL se convierten en cero para conservar la validacion.
        double AB_SuperBancos = AB_Fila["AB_SuperBancos"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_SuperBancos"]);
        double AB_SeguroCampesino = AB_Fila["AB_SeguroCampesino"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_SeguroCampesino"]);
        double AB_DerechosEmision = AB_Fila["AB_DerechosEmision"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_DerechosEmision"]);
        double AB_IVA = AB_Fila["AB_IVA"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_IVA"]);

        return new AB_Poliza(
            Convert.ToInt32(AB_Fila["AB_IdPoliza"]),
            Convert.ToInt32(AB_Fila["AB_IdCliente"]),
            Convert.ToInt32(AB_Fila["AB_IdRamo"]),
            AB_Fila["AB_NumeroPoliza"].ToString(),
            Convert.ToDouble(AB_Fila["AB_CapitalAsegurado"]),
            Convert.ToDouble(AB_Fila["AB_TasaRiesgo"]),
            Convert.ToDouble(AB_Fila["AB_PrimaBase"]),
            AB_SuperBancos, AB_SeguroCampesino, AB_DerechosEmision, AB_IVA,
            Convert.ToDouble(AB_Fila["AB_PrimaTotal"]),
            Convert.ToDouble(AB_Fila["AB_CapitalRemanente"]),
            AB_Fila["AB_Estado"].ToString());
    }

    // Metodo que espera ENTER antes de volver al menu.
    private void AB_Pausar()
    {
        Console.WriteLine("\nPresione ENTER para continuar...");
        Console.ReadLine();
    }
}




