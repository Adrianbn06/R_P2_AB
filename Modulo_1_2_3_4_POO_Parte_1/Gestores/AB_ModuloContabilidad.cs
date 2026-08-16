using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaSeguros;

public class AB_ModuloContabilidad
{
    // Variable para usar la conexion de SQL Server.
    private AB_Conexion AB_ConexionBD = AB_Conexion.AB_GetInstance();

    // Metodo para mostrar el menu del modulo de contabilidad.
    public void AB_MenuContabilidad()
    {
        // Variable para guardar la opcion seleccionada.
        string AB_Opcion;

        do
        {
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("=======================================================");
            Console.WriteLine("                MODULO DE CONTABILIDAD                 ");
            Console.WriteLine("=======================================================");
            Console.WriteLine(" 1. Generar Asiento por Emision");
            Console.WriteLine(" 2. Generar Asiento por Reaseguro");
            Console.WriteLine(" 3. Generar Asiento por Siniestro");
            Console.WriteLine(" 4. Generar Asiento Total de la poliza");
            Console.WriteLine(" 5. Modificar Nombre de Cuenta");
            Console.WriteLine(" 6. Volver al Menu Principal");
            Console.WriteLine("=======================================================");
            Console.Write("Seleccione una opcion [1-6]: ");
            AB_Opcion = Console.ReadLine() ?? "";

            // Se ejecuta la accion correspondiente a la opcion seleccionada.
            switch (AB_Opcion)
            {
                case "1": 
                    AB_GenerarAsientoEmision(); 
                    break;
                case "2": 
                    AB_GenerarAsientoReaseguro(); 
                    break;
                case "3": 
                    AB_GenerarAsientoSiniestro(); 
                    break;
                case "4": 
                    AB_GenerarAsientoTotal(); 
                    break;
                case "5": 
                    AB_ModificarCuenta(); 
                    break;
                case "6": 
                    break;
                default:
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("\nOpcion incorrecta. Ingrese una opcion entre 1 y 6.");
                    AB_Pausa();
                    break;
            }
        } 
        while (AB_Opcion != "6");
    }

    // --------------------------------------------------
    // 1. Metodo para GENERAR el asiento por emision de POLIZA.
    // --------------------------------------------------
    private void AB_GenerarAsientoEmision()
    {
        Console.Clear();
        Console.WriteLine("--- GENERAR ASIENTO CONTABLE POR EMISION ---");
        Console.Write("Ingrese el codigo de la poliza (POL-001): ");

        // Se recibe el numero de la poliza.
        string AB_NumeroPoliza = Console.ReadLine();

        // Se guardan la poliza activa y su cliente para generar el asiento de emision.
        DataTable AB_DatosPoliza = AB_ConexionBD.AB_ExecuteQuery(
            "SELECT p.AB_IdPoliza, p.AB_IdCliente, p.AB_IdRamo, p.AB_NumeroPoliza, " +
            "p.AB_CapitalAsegurado, p.AB_TasaRiesgo, p.AB_PrimaBase, p.AB_SuperBancos, " +
            "p.AB_SeguroCampesino, p.AB_DerechosEmision, p.AB_IVA, p.AB_PrimaTotal, " +
            "p.AB_CapitalRemanente, p.AB_Estado, c.AB_IdCliente AS AB_ClienteId, " +
            "c.AB_Cedula, c.AB_Nombres, c.AB_Apellidos, c.AB_Direccion, c.AB_Telefono, c.AB_Correo " +
            "FROM AB_Poliza p " +
            "INNER JOIN AB_Cliente c ON p.AB_IdCliente = c.AB_IdCliente " +
            "WHERE p.AB_NumeroPoliza = @NumeroPoliza AND p.AB_Estado = 'ACTIVA'",
            new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });

        // Si no existe la poliza activa, no se genera el asiento.
        if (AB_DatosPoliza.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: La poliza ingresada no existe o se encuentra inactiva.");
            AB_Pausa();
            return;
        }

        // Se convierten la poliza y su cliente en objetos antes de calcular.
        DataRow AB_FilaPoliza = AB_DatosPoliza.Rows[0];

        AB_Poliza AB_PolizaEmision = AB_CrearPolizaDesdeFila(AB_FilaPoliza);
        AB_Cliente AB_ClienteEmision = AB_CrearClienteDesdeFila(AB_FilaPoliza);

        // Se obtienen los datos de la poliza y de su cliente.
        int AB_IdPoliza = AB_PolizaEmision.AB_IdPoliza;
        double AB_Capital = AB_PolizaEmision.AB_CapitalAsegurado;
        double AB_PrimaBase = AB_PolizaEmision.AB_PrimaBase;
        double AB_IVA = AB_PolizaEmision.AB_IVA;
        string AB_Cliente = AB_ClienteEmision.AB_Nombres + " " + AB_ClienteEmision.AB_Apellidos;

        // Se calculan los valores que se registraran en el asiento de emision.
        double AB_Impuestos = AB_PrimaBase * 0.04;
        double AB_DerechoEmision = AB_Capital > 40000 ? 2.00 : (AB_Capital > 10000 ? 1.00 : 0.50);
        double AB_FacturaTotal = AB_PrimaBase + AB_Impuestos + AB_DerechoEmision + AB_IVA;
        double AB_PrimasNoGanadas = AB_PrimaBase * 0.50;
        double AB_IngresosPrimas = (AB_PrimaBase * 0.50) + AB_DerechoEmision;
        double AB_ImpuestosPagar = AB_Impuestos + AB_IVA;

        // Si no se guarda la cabecera o el detalle, se cancela el asiento de emision.
        if (!AB_GuardarAsiento(AB_IdPoliza, 0, "EMISION", AB_FacturaTotal, AB_FacturaTotal,
                new string[] { "1010", "2020", "4010", "2030" },
                new double[] { AB_FacturaTotal, 0, 0, 0 },
                new double[] { 0, AB_PrimasNoGanadas, AB_IngresosPrimas, AB_ImpuestosPagar }))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: No fue posible guardar el asiento contable.");
            AB_Pausa();
            return;
        }

        // Se muestra la cabecera del comprobante de emision.
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=========================================================================");
        Console.WriteLine($"\tCOMPROBANTE DIARIO - EMISION: {AB_NumeroPoliza}");
        Console.WriteLine("=========================================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Asegurado: {AB_Cliente}\tCobertura: ${AB_Capital:F2}");
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.WriteLine("Cod\tNombre Cuenta\t\t\tDebe\t\tHaber");
        Console.WriteLine("-------------------------------------------------------------------------");
        AB_MostrarLinea("1010", AB_FacturaTotal, 0);
        AB_MostrarLinea("2020", 0, AB_PrimasNoGanadas);
        AB_MostrarLinea("4010", 0, AB_IngresosPrimas);
        AB_MostrarLinea("2030", 0, AB_ImpuestosPagar);

        // Se muestran los totales del comprobante guardado.
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"TOTALES:\t\t\t\t${AB_FacturaTotal:F2}\t${AB_FacturaTotal:F2}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=========================================================================");
        Console.WriteLine("\nAsiento guardado en AB_CabeceraAsiento y AB_DetalleAsiento.");
        AB_Pausa();
    }

    // --------------------------------------------------
    // 2. Metodo para GENERAR el asiento por REASEGURO.
    // --------------------------------------------------
    private void AB_GenerarAsientoReaseguro()
    {
        Console.Clear();
        Console.WriteLine("--- GENERAR ASIENTO CONTABLE POR REASEGURO ---");
        Console.Write("Ingrese el codigo de la poliza (POL-001): ");

        // Se recibe el numero de poliza.
        string AB_NumeroPoliza = Console.ReadLine();

        // Se guardan el reparto de reaseguro y el cliente de la poliza.
        DataTable AB_Reparto = AB_ConexionBD.AB_ExecuteQuery(
            "SELECT r.AB_IdReparto, r.AB_IdPoliza, r.AB_IdReaseguradora, r.AB_RetencionPropia, " +
            "r.AB_CapitalContrato, r.AB_CapitalFacultativo, c.AB_IdCliente AS AB_ClienteId, " +
            "c.AB_Cedula, c.AB_Nombres, c.AB_Apellidos, c.AB_Direccion, c.AB_Telefono, c.AB_Correo " +
            "FROM AB_RepartoReaseguro r INNER JOIN AB_Poliza p ON r.AB_IdPoliza = p.AB_IdPoliza " +
            "INNER JOIN AB_Cliente c ON p.AB_IdCliente = c.AB_IdCliente " +
            "WHERE p.AB_NumeroPoliza = @NumeroPoliza",
            new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });

        // Si no existe reparto, no se genera el asiento.
        if (AB_Reparto.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: No existe un reparto de reaseguro para esa poliza.");
            AB_Pausa();
            return;
        }

        // Se convierten el reparto y el cliente relacionados en objetos.
        DataRow AB_Fila = AB_Reparto.Rows[0];
        AB_RepartoReaseguro AB_RepartoContable = new AB_RepartoReaseguro(
            Convert.ToInt32(AB_Fila["AB_IdReparto"]),
            Convert.ToInt32(AB_Fila["AB_IdPoliza"]),
            Convert.ToInt32(AB_Fila["AB_IdReaseguradora"]),
            Convert.ToDouble(AB_Fila["AB_RetencionPropia"]),
            Convert.ToDouble(AB_Fila["AB_CapitalContrato"]),
            Convert.ToDouble(AB_Fila["AB_CapitalFacultativo"]));

        AB_Cliente AB_ClienteReaseguro = AB_CrearClienteDesdeFila(AB_Fila);

        // Se obtienen la poliza, el monto cedido y el nombre del cliente.
        int AB_IdPoliza = AB_RepartoContable.AB_IdPoliza;
        double AB_MontoCedido = AB_RepartoContable.AB_CapitalContrato + AB_RepartoContable.AB_CapitalFacultativo;
        string AB_Cliente = AB_ClienteReaseguro.AB_Nombres + " " + AB_ClienteReaseguro.AB_Apellidos;

        // Si no existe monto cedido, no se genera comprobante.
        if (AB_MontoCedido <= 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: La poliza no tiene capital cedido a reaseguro.");
            AB_Pausa();
            return;
        }

        // Se valida que el asiento de reaseguro se haya guardado correctamente.
        if (!AB_GuardarAsiento(AB_IdPoliza, 0, "REASEGURO", AB_MontoCedido, AB_MontoCedido,
                new string[] { "5020", "2010" }, new double[] { AB_MontoCedido, 0 }, new double[] { 0, AB_MontoCedido }))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: No fue posible guardar el asiento contable.");
            AB_Pausa();
            return;
        }

        // Se muestra la cabecera del comprobante de reaseguro.
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=========================================================================");
        Console.WriteLine($"\tCOMPROBANTE DIARIO - REASEGURO: {AB_NumeroPoliza}");
        Console.WriteLine("=========================================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Asegurado: {AB_Cliente}");
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.WriteLine("Cod\tNombre Cuenta\t\t\tDebe\t\tHaber");
        Console.WriteLine("-------------------------------------------------------------------------");
        AB_MostrarLinea("5020", AB_MontoCedido, 0);
        AB_MostrarLinea("2010", 0, AB_MontoCedido);

        // Se muestran los totales del comprobante guardado.
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"TOTALES:\t\t\t\t${AB_MontoCedido:F2}\t${AB_MontoCedido:F2}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=========================================================================");
        Console.WriteLine("\nAsiento guardado en AB_CabeceraAsiento y AB_DetalleAsiento.");
        AB_Pausa();
    }

    // --------------------------------------------------
    // 3. Metodo para GENERAR el asiento por SINIESTRO.
    // --------------------------------------------------
    private void AB_GenerarAsientoSiniestro()
    {
        Console.Clear();
        Console.WriteLine("--- GENERAR ASIENTO CONTABLE POR SINIESTRO ---");
        Console.Write("Ingrese el codigo del siniestro (SIN-001): ");

        // Se recibe el numero del siniestro.
        string AB_NumeroSiniestro = Console.ReadLine();

        // Se guardan el siniestro vigente, su poliza y el cliente relacionado.
        DataTable AB_DatosSiniestro = AB_ConexionBD.AB_ExecuteQuery(
            "SELECT s.AB_IdSiniestro, s.AB_IdPoliza, s.AB_NumeroReclamo, s.AB_FechaSiniestro, " +
            "s.AB_DanosReclamados, s.AB_DeducibleAsumido, s.AB_PagoNeto, s.AB_EstadoAuditoria, " +
            "p.AB_NumeroPoliza, c.AB_IdCliente AS AB_ClienteId, c.AB_Cedula, c.AB_Nombres, " +
            "c.AB_Apellidos, c.AB_Direccion, c.AB_Telefono, c.AB_Correo FROM AB_Siniestro s " +
            "INNER JOIN AB_Poliza p ON s.AB_IdPoliza = p.AB_IdPoliza " +
            "INNER JOIN AB_Cliente c ON p.AB_IdCliente = c.AB_IdCliente " +
            "WHERE s.AB_NumeroReclamo = @NumeroReclamo AND s.AB_EstadoAuditoria <> 'ANULADO'",
            new string[] { "@NumeroReclamo" }, new object[] { AB_NumeroSiniestro });

        // Si el siniestro no existe o fue anulado, se cancela el asiento.
        if (AB_DatosSiniestro.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: El siniestro ingresado no existe o se encuentra anulado.");
            AB_Pausa();
            return;
        }

        // Se convierten el siniestro y el cliente relacionados en objetos.
        DataRow AB_Fila = AB_DatosSiniestro.Rows[0];

        AB_Siniestro AB_SiniestroContable = AB_CrearSiniestroDesdeFila(AB_Fila);
        AB_Cliente AB_ClienteSiniestro = AB_CrearClienteDesdeFila(AB_Fila);

        // Se obtienen la poliza, el siniestro, el pago neto y el cliente.
        int AB_IdPoliza = AB_SiniestroContable.AB_IdPoliza;
        int AB_IdSiniestro = AB_SiniestroContable.AB_IdSiniestro;
        double AB_PagoNeto = AB_SiniestroContable.AB_PagoNeto;
        string AB_Cliente = AB_ClienteSiniestro.AB_Nombres + " " + AB_ClienteSiniestro.AB_Apellidos;

        // Se valida que el asiento del siniestro se haya guardado correctamente.
        if (!AB_GuardarAsiento(AB_IdPoliza, AB_IdSiniestro, "SINIESTRO", AB_PagoNeto, AB_PagoNeto,
                new string[] { "5030", "2040" }, new double[] { AB_PagoNeto, 0 }, new double[] { 0, AB_PagoNeto }))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: No fue posible guardar el asiento contable.");
            AB_Pausa();
            return;
        }

        // Se muestra la cabecera del comprobante del siniestro.
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=========================================================================");
        Console.WriteLine($"\tCOMPROBANTE DIARIO - SINIESTRO: {AB_NumeroSiniestro}");
        Console.WriteLine("=========================================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Asegurado: {AB_Cliente}");
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.WriteLine("Cod\tNombre Cuenta\t\t\tDebe\t\tHaber");
        Console.WriteLine("-------------------------------------------------------------------------");
        AB_MostrarLinea("5030", AB_PagoNeto, 0);
        AB_MostrarLinea("2040", 0, AB_PagoNeto);

        // Se muestran los totales del comprobante guardado.
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"TOTALES:\t\t\t\t${AB_PagoNeto:F2}\t${AB_PagoNeto:F2}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=========================================================================");
        Console.WriteLine("\nAsiento guardado en AB_CabeceraAsiento y AB_DetalleAsiento.");
        AB_Pausa();
    }

    // --------------------------------------------------
    // 4. Metodo para GENERAR el asiento TOTAL de una poliza.
    // --------------------------------------------------
    private void AB_GenerarAsientoTotal()
    {
        Console.Clear();
        Console.WriteLine("--- ASIENTO TOTAL DE POLIZA ---");
        Console.Write("Ingrese el codigo de la poliza (POL-001): ");

        // Se recibe el numero de poliza para generar su asiento completo.
        string AB_NumeroPoliza = Console.ReadLine();

        // Se guardan la poliza y su cliente para generar el asiento completo.
        DataTable AB_Polizas = AB_ConexionBD.AB_ExecuteQuery(
            "SELECT p.AB_IdPoliza, p.AB_IdCliente, p.AB_IdRamo, p.AB_NumeroPoliza, " +
            "p.AB_CapitalAsegurado, p.AB_TasaRiesgo, p.AB_PrimaBase, p.AB_SuperBancos, " +
            "p.AB_SeguroCampesino, p.AB_DerechosEmision, p.AB_IVA, p.AB_PrimaTotal, " +
            "p.AB_CapitalRemanente, p.AB_Estado, c.AB_IdCliente AS AB_ClienteId, " +
            "c.AB_Cedula, c.AB_Nombres, c.AB_Apellidos, c.AB_Direccion, c.AB_Telefono, c.AB_Correo " +
            "FROM AB_Poliza p " +
            "INNER JOIN AB_Cliente c " +
            "ON p.AB_IdCliente = c.AB_IdCliente " +
            "WHERE p.AB_NumeroPoliza = @NumeroPoliza",
            new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });

        // Si no existe la poliza, no se genera el asiento total.
        if (AB_Polizas.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: La poliza ingresada no existe.");
            AB_Pausa();
            return;
        }

        // Se crean los objetos de la poliza y el cliente del asiento total.
        DataRow AB_FilaPoliza = AB_Polizas.Rows[0];

        AB_Poliza AB_PolizaTotal = AB_CrearPolizaDesdeFila(AB_FilaPoliza);
        AB_Cliente AB_ClienteTotal = AB_CrearClienteDesdeFila(AB_FilaPoliza);

        // Se recuperan los datos base de la poliza y del cliente.
        int AB_IdPoliza = AB_PolizaTotal.AB_IdPoliza;
        double AB_Capital = AB_PolizaTotal.AB_CapitalAsegurado;
        double AB_Prima = AB_PolizaTotal.AB_PrimaBase;
        double AB_IVA = AB_PolizaTotal.AB_IVA;
        string AB_Cliente = AB_ClienteTotal.AB_Nombres + " " + AB_ClienteTotal.AB_Apellidos;

        // Se calculan los valores de facturacion que formaran el asiento.
        double AB_Impuestos = AB_Prima * 0.04;
        double AB_Derecho = AB_Capital > 40000 ? 2.00 : (AB_Capital > 10000 ? 1.00 : 0.50);
        double AB_Factura = AB_Prima + AB_Impuestos + AB_Derecho + AB_IVA;
        double AB_PrimasNoGanadas = AB_Prima * 0.50;
        double AB_Ingresos = (AB_Prima * 0.50) + AB_Derecho;
        double AB_ImpuestosPagar = AB_Impuestos + AB_IVA;

        // Se guarda el reparto de reaseguro para calcular el capital cedido.
        DataTable AB_Repartos = AB_ConexionBD.AB_ExecuteQuery("" +
            "SELECT AB_IdReparto, AB_IdPoliza, AB_IdReaseguradora, AB_RetencionPropia, " +
            "AB_CapitalContrato, AB_CapitalFacultativo " +
            "FROM AB_RepartoReaseguro " +
            "WHERE AB_IdPoliza = @IdPoliza",
            new string[] { "@IdPoliza" }, new object[] { AB_IdPoliza });
        
        double AB_MontoCedido = 0;

        // Se valida si la poliza tiene un reparto de reaseguro registrado.
        if (AB_Repartos.Rows.Count > 0)
        {
            // Se convierte el reparto encontrado antes de obtener el monto cedido.
            DataRow AB_FilaReparto = AB_Repartos.Rows[0];
            AB_RepartoReaseguro AB_RepartoTotal = new AB_RepartoReaseguro(
                Convert.ToInt32(AB_FilaReparto["AB_IdReparto"]),
                Convert.ToInt32(AB_FilaReparto["AB_IdPoliza"]),
                Convert.ToInt32(AB_FilaReparto["AB_IdReaseguradora"]),
                Convert.ToDouble(AB_FilaReparto["AB_RetencionPropia"]),
                Convert.ToDouble(AB_FilaReparto["AB_CapitalContrato"]),
                Convert.ToDouble(AB_FilaReparto["AB_CapitalFacultativo"]));

            AB_MontoCedido = AB_RepartoTotal.AB_CapitalContrato + AB_RepartoTotal.AB_CapitalFacultativo;
        }
       
        // Se guardan los siniestros vigentes para sumar sus pagos netos.
        DataTable AB_Siniestros = AB_ConexionBD.AB_ExecuteQuery("" +
            "SELECT AB_IdSiniestro, AB_IdPoliza, AB_NumeroReclamo, AB_FechaSiniestro, " +
            "AB_DanosReclamados, AB_DeducibleAsumido, AB_PagoNeto, AB_EstadoAuditoria " +
            "FROM AB_Siniestro " +
            "WHERE AB_IdPoliza = @IdPoliza AND AB_EstadoAuditoria <> 'ANULADO'",
            new string[] { "@IdPoliza" }, new object[] { AB_IdPoliza });

        // Acumulador para sumar los pagos de los siniestros encontrados.
        double AB_TotalSiniestros = 0;

        // Se suman los pagos de todos los siniestros activos.
        foreach (DataRow AB_FilaSiniestro in AB_Siniestros.Rows)
        {
            // Cada fila se convierte en objeto antes de sumar su pago.
            AB_Siniestro AB_SiniestroTotal = AB_CrearSiniestroDesdeFila(AB_FilaSiniestro);

            AB_TotalSiniestros += AB_SiniestroTotal.AB_PagoNeto;
        }
        
        // Se calculan los totales que deben quedar equilibrados en el asiento.
        double AB_TotalDebe = AB_Factura + AB_MontoCedido + AB_TotalSiniestros;
        double AB_TotalHaber = AB_Factura + AB_MontoCedido + AB_TotalSiniestros;

        // Los tres arreglos relacionan cada cuenta con su valor en debe y haber.
        string[] AB_Codigos = { "1010", "2020", "4010", "2030", "5020", "2010", "5030", "2040" };
        double[] AB_Debe = { AB_Factura, 0, 0, 0, AB_MontoCedido, 0, AB_TotalSiniestros, 0 };
        double[] AB_Haber = { 0, AB_PrimasNoGanadas, AB_Ingresos, AB_ImpuestosPagar, 0, AB_MontoCedido, 0, AB_TotalSiniestros };
        
        // Se valida que el asiento contable completo se haya guardado correctamente.
        if (!AB_GuardarAsiento(AB_IdPoliza, 0, "ASIENTO TOTAL", AB_TotalDebe, AB_TotalHaber, AB_Codigos, AB_Debe, AB_Haber))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: No fue posible guardar el asiento total.");
            AB_Pausa();
            return;
        }

        // Se muestra la cabecera del comprobante final.
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=========================================================================");
        Console.WriteLine($"\tCOMPROBANTE FINAL: {AB_NumeroPoliza}");
        Console.WriteLine("=========================================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Asegurado: {AB_Cliente}\tCobertura: ${AB_Capital:F2}");
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.WriteLine("Cod\tNombre Cuenta\t\t\tDebe\t\tHaber");
        Console.WriteLine("-------------------------------------------------------------------------");

        // Se recorren las cuentas para mostrar solo las que tienen valores registrados.
        for (int AB_Indice = 0; AB_Indice < AB_Codigos.Length; AB_Indice++)
        {
            // Se muestra la cuenta cuando tiene un valor en debe o en haber.
            if (AB_Debe[AB_Indice] > 0 || AB_Haber[AB_Indice] > 0)
                AB_MostrarLinea(AB_Codigos[AB_Indice], AB_Debe[AB_Indice], AB_Haber[AB_Indice]);
        }

        // Se muestran los totales del comprobante guardado.
        Console.WriteLine("-------------------------------------------------------------------------");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"TOTALES:\t\t\t\t${AB_TotalDebe:F2}\t${AB_TotalHaber:F2}");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("=========================================================================");
        Console.WriteLine("\nAsiento guardado en AB_CabeceraAsiento y AB_DetalleAsiento.");
        AB_Pausa();
    }

    // Metodo para guardar la cabecera y los detalles del asiento.
    // Uso de COMMIT y ROLLBACK.
    private bool AB_GuardarAsiento(int AB_IdPoliza, int AB_IdSiniestro, string AB_Modulo, double AB_TotalDebe, double AB_TotalHaber, string[] AB_Codigos, double[] AB_Debe, double[] AB_Haber)
    {
        // Se crea el objeto cabecera antes de iniciar el guardado.
        AB_AsientoContable AB_Asiento = new AB_AsientoContable(0, AB_IdPoliza,AB_IdSiniestro, "PENDIENTE", DateTime.Now, AB_Modulo,
                                         AB_TotalDebe, AB_TotalHaber);

        // Se valida que los tres arreglos tengan la misma cantidad de elementos.
        if (AB_Codigos.Length != AB_Debe.Length || AB_Codigos.Length != AB_Haber.Length)
        {
            // Se prepara el error que informa que los arreglos no coinciden.
            try
            {
                // Se lanza la excepcion porque las lineas del asiento no coinciden.
                throw new AB_AsientoContableInvalidoException("Las cuentas, valores debe y valores haber no coinciden.");
            }
            // Se muestra y registra el error de cantidad de elementos.
            catch (AB_AsientoContableInvalidoException AB_Excepcion)
            {
                // Se muestra y registra el error contable.
                Console.WriteLine($"Error de asiento: {AB_Excepcion.Message}");
                AB_ConexionBD.AB_RegistrarLog("CONTABILIDAD", "ERROR", AB_Excepcion.Message);
                return false;
            }
        }

        // Se valida que la partida doble tenga los mismos totales.
        if (!AB_Asiento.AB_EstaBalanceado())
        {
            // Se prepara el error que informa que el asiento no esta balanceado.
            try
            {
                // Se lanza la excepcion porque el asiento no esta balanceado.
                throw new AB_AsientoContableInvalidoException("El total debe y el total haber deben ser iguales.");
            }
            // Se muestra y registra el error de partida doble.
            catch (AB_AsientoContableInvalidoException AB_Excepcion)
            {
                // Se muestra y registra el error contable.
                Console.WriteLine($"Error de asiento: {AB_Excepcion.Message}");
                AB_ConexionBD.AB_RegistrarLog("CONTABILIDAD", "ERROR", AB_Excepcion.Message);
                return false;
            }
        }

        // Se prepara el valor nulo cuando el asiento no pertenece a un siniestro.
        object AB_IdSiniestroParametro = DBNull.Value;

        // Si el asiento pertenece a un siniestro, se guarda su identificador.
        if (AB_Asiento.AB_IdSiniestro != 0)
        {
            AB_IdSiniestroParametro = AB_Asiento.AB_IdSiniestro;
        }

        string AB_Comprobante = "";

        // Se intenta abrir la conexion e iniciar la transaccion del asiento.
        try
        {
            // Se abre la conexion antes de iniciar la transaccion.
            AB_ConexionBD.AB_OpenConnection();
            SqlConnection AB_Conexion = AB_ConexionBD.AB_ObtenerConexion();

            // Se inicia la transaccion para guardar todo o no guardar nada.
            using (SqlTransaction AB_Transaccion = AB_Conexion.BeginTransaction())
            {
                // Se intentan guardar la cabecera y todos sus detalles como una sola operacion.
                try
                {
                    // Se consulta el siguiente numero del comprobante.
                    using (SqlCommand AB_ComandoSiguiente = new SqlCommand(
                        "SELECT ISNULL(MAX(AB_IdAsiento), 0) + 1 AS AB_Siguiente" +
                        " FROM AB_CabeceraAsiento", AB_Conexion, AB_Transaccion))
                    {
                        DataTable AB_TablaSiguiente = new DataTable();

                        using (SqlDataAdapter AB_Adaptador = new SqlDataAdapter(AB_ComandoSiguiente))
                        {
                            AB_Adaptador.Fill(AB_TablaSiguiente);
                        }

                        // Se toma el siguiente numero para formar el comprobante contable.
                        int AB_Siguiente = Convert.ToInt32(AB_TablaSiguiente.Rows[0]["AB_Siguiente"]);
                        AB_Comprobante = "CMP-" + AB_Siguiente.ToString("000");
                        AB_Asiento.AB_NumeroComprobante = AB_Comprobante;
                    }

                    // Se guarda la cabecera del asiento contable.
                    using (SqlCommand AB_ComandoCabecera = new SqlCommand(
                        "INSERT INTO AB_CabeceraAsiento (AB_IdPoliza, AB_IdSiniestro, AB_NumeroComprobante, AB_FechaTransaccion, AB_ModuloOrigen, AB_TotalDebe, AB_TotalHaber) " +
                        "VALUES " +
                        "(@IdPoliza, @IdSiniestro, @NumeroComprobante, @FechaTransaccion, @ModuloOrigen, @TotalDebe, @TotalHaber)",
                        AB_Conexion, AB_Transaccion))
                    {
                        // Se agregan los datos de la cabecera mediante parametros.
                        AB_ComandoCabecera.Parameters.AddWithValue("@IdPoliza", AB_Asiento.AB_IdPoliza);
                        AB_ComandoCabecera.Parameters.AddWithValue("@IdSiniestro", AB_IdSiniestroParametro);
                        AB_ComandoCabecera.Parameters.AddWithValue("@NumeroComprobante", AB_Asiento.AB_NumeroComprobante);
                        AB_ComandoCabecera.Parameters.AddWithValue("@FechaTransaccion", AB_Asiento.AB_FechaTransaccion);
                        AB_ComandoCabecera.Parameters.AddWithValue("@ModuloOrigen", AB_Asiento.AB_ModuloOrigen);
                        AB_ComandoCabecera.Parameters.AddWithValue("@TotalDebe", AB_Asiento.AB_TotalDebe);
                        AB_ComandoCabecera.Parameters.AddWithValue("@TotalHaber", AB_Asiento.AB_TotalHaber);

                        // Se valida que la cabecera se haya guardado.
                        if (AB_ComandoCabecera.ExecuteNonQuery() == 0) 
                            throw new Exception("No se pudo guardar la cabecera del asiento.");
                    }

                    // Se obtiene el identificador de la cabecera guardada.
                    int AB_IdAsiento;

                    using (SqlCommand AB_ComandoAsiento = new SqlCommand("" +
                        "SELECT AB_IdAsiento " +
                        "FROM AB_CabeceraAsiento " +
                        "WHERE AB_NumeroComprobante = @NumeroComprobante", AB_Conexion, AB_Transaccion))
                    {
                        // Se agrega el comprobante que se desea buscar.
                        AB_ComandoAsiento.Parameters.AddWithValue("@NumeroComprobante", AB_Comprobante);

                        DataTable AB_TablaAsiento = new DataTable();

                        using (SqlDataAdapter AB_Adaptador = new SqlDataAdapter(AB_ComandoAsiento))
                        {
                            AB_Adaptador.Fill(AB_TablaAsiento);
                        }

                        // Se valida que el asiento exista antes de guardar el detalle.
                        if (AB_TablaAsiento.Rows.Count == 0) 
                            throw new Exception("No se encontro la cabecera del asiento.");

                        // Se toma el identificador de la cabecera para relacionar sus detalles.
                        AB_IdAsiento = Convert.ToInt32(AB_TablaAsiento.Rows[0]["AB_IdAsiento"]);

                        AB_Asiento.AB_IdAsiento = AB_IdAsiento;
                    }

                    // Ciclo para guardar cada linea del detalle contable.
                    for (int AB_Indice = 0; AB_Indice < AB_Codigos.Length; AB_Indice++)
                    {
                        // Se omiten las cuentas que no tengan valores.
                        if (AB_Debe[AB_Indice] == 0 && AB_Haber[AB_Indice] == 0) 
                            continue;

                        // Se consulta el identificador de la cuenta contable.
                        AB_CuentaContable AB_CuentaActual;

                        using (SqlCommand AB_ComandoCuenta = new SqlCommand("" +
                            "SELECT AB_IdCuenta, AB_CodigoCuenta, AB_NombreCuenta, AB_Naturaleza, AB_TipoCuenta " +
                            "FROM AB_CuentaContable " +
                            "WHERE AB_CodigoCuenta = @CodigoCuenta", AB_Conexion, AB_Transaccion))
                        {
                            // Se agrega el codigo de la cuenta que se desea buscar.
                            AB_ComandoCuenta.Parameters.AddWithValue("@CodigoCuenta", AB_Codigos[AB_Indice]);

                            DataTable AB_TablaCuenta = new DataTable();

                            using (SqlDataAdapter AB_Adaptador = new SqlDataAdapter(AB_ComandoCuenta))
                            {
                                AB_Adaptador.Fill(AB_TablaCuenta);
                            }

                            // Se valida que la cuenta exista antes de guardar el detalle.
                            if (AB_TablaCuenta.Rows.Count == 0) 
                                throw new Exception("No existe la cuenta contable " + AB_Codigos[AB_Indice] + ".");

                            // Se convierte la cuenta encontrada en un objeto para usar su identificador.
                            DataRow AB_FilaCuenta = AB_TablaCuenta.Rows[0];

                            AB_CuentaActual = new AB_CuentaContable(
                                Convert.ToInt32(AB_FilaCuenta["AB_IdCuenta"]),
                                AB_FilaCuenta["AB_CodigoCuenta"].ToString(),
                                AB_FilaCuenta["AB_NombreCuenta"].ToString(),
                                AB_FilaCuenta["AB_Naturaleza"].ToString(),
                                AB_FilaCuenta["AB_TipoCuenta"].ToString());
                        }

                        // Se crea la linea contable con la cuenta y sus valores de debe y haber.
                        AB_DetalleAsiento AB_Detalle = new AB_DetalleAsiento(0,AB_Asiento.AB_IdAsiento, AB_CuentaActual.AB_IdCuenta,
                                                         AB_Debe[AB_Indice], AB_Haber[AB_Indice]);

                        // Se guarda el detalle de la cuenta contable.
                        using (SqlCommand AB_ComandoDetalle = new SqlCommand(
                            "INSERT INTO AB_DetalleAsiento (AB_IdAsiento, AB_IdCuenta, AB_ValorDebe, AB_ValorHaber)" +
                            " VALUES " +
                            "(@IdAsiento, @IdCuenta, @ValorDebe, @ValorHaber)",
                            AB_Conexion, AB_Transaccion))
                        {
                            // Se agregan los valores del detalle mediante parametros.
                            AB_ComandoDetalle.Parameters.AddWithValue("@IdAsiento", AB_Detalle.AB_IdAsiento);
                            AB_ComandoDetalle.Parameters.AddWithValue("@IdCuenta", AB_Detalle.AB_IdCuenta);
                            AB_ComandoDetalle.Parameters.AddWithValue("@ValorDebe", AB_Detalle.AB_ValorDebe);
                            AB_ComandoDetalle.Parameters.AddWithValue("@ValorHaber", AB_Detalle.AB_ValorHaber);

                            // Se valida que el detalle se haya guardado.
                            if (AB_ComandoDetalle.ExecuteNonQuery() == 0) 
                                throw new Exception("No se pudo guardar el detalle del asiento.");
                        }
                    }
                    // Se confirman la cabecera y todos los detalles guardados.
                    AB_Transaccion.Commit();
                }
                // Si falla la cabecera o un detalle, se revierten todos los cambios.
                catch (Exception AB_Excepcion)
                {
                    // Se deshacen todos los cambios cuando ocurre un error.
                    AB_Transaccion.Rollback();
                    Console.WriteLine($"Error al guardar el asiento: {AB_Excepcion.Message}");
                    return false;
                }
            }
        }
        // Se informa si no fue posible abrir la conexion o iniciar la transaccion.
        catch (Exception AB_Excepcion)
        {
            // Se muestra el error cuando no se puede iniciar la transaccion.
            Console.WriteLine($"Error de conexion al guardar el asiento: {AB_Excepcion.Message}");
            return false;
        }
        // La conexion se cierra tanto si el asiento se guarda como si ocurre un error.
        finally
        {
            // Se cierra la conexion despues de terminar la transaccion.
            AB_ConexionBD.AB_CloseConnection();
        }

        // Se registra el asiento completo despues de confirmar la transaccion.
        AB_ConexionBD.AB_RegistrarLog("CONTABILIDAD", "GENERAR ASIENTO", "Comprobante " + AB_Comprobante + " generado desde " + AB_Modulo + ".");
        return true;
    }

    // Metodo que transforma una fila de SQL Server en una entidad poliza.
    private AB_Poliza AB_CrearPolizaDesdeFila(DataRow AB_Fila)
    {
        // Las columnas financieras que permiten NULL se convierten en cero.
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

    // Metodo que transforma las columnas relacionadas del cliente en un objeto.
    private AB_Cliente AB_CrearClienteDesdeFila(DataRow AB_Fila)
    {
        return new AB_Cliente(
            Convert.ToInt32(AB_Fila["AB_ClienteId"]),
            AB_Fila["AB_Cedula"].ToString(),
            AB_Fila["AB_Nombres"].ToString(),
            AB_Fila["AB_Apellidos"].ToString(),
            AB_Fila["AB_Direccion"].ToString(),
            AB_Fila["AB_Telefono"].ToString(),
            AB_Fila["AB_Correo"].ToString());
    }

    // Metodo que transforma una fila de la tabla AB_Siniestro en su entidad.
    private AB_Siniestro AB_CrearSiniestroDesdeFila(DataRow AB_Fila)
    {
        return new AB_Siniestro(
            Convert.ToInt32(AB_Fila["AB_IdSiniestro"]),
            Convert.ToInt32(AB_Fila["AB_IdPoliza"]),
            AB_Fila["AB_NumeroReclamo"].ToString(),
            Convert.ToDateTime(AB_Fila["AB_FechaSiniestro"]),
            Convert.ToDouble(AB_Fila["AB_DanosReclamados"]),
            Convert.ToDouble(AB_Fila["AB_DeducibleAsumido"]),
            Convert.ToDouble(AB_Fila["AB_PagoNeto"]),
            AB_Fila["AB_EstadoAuditoria"].ToString());
    }

    // --------------------------------------------------
    // 5. Metodo para ACTUALIZAR el nombre de una cuenta contable.
    // --------------------------------------------------
    private void AB_ModificarCuenta()
    {
        Console.Clear();
        Console.WriteLine("--- MODIFICAR NOMBRE DE CUENTA ---");
        Console.Write("Ingrese el codigo de la cuenta a editar (1010): ");

        // Se guarda el codigo de la cuenta que se desea modificar.
        string AB_Codigo = Console.ReadLine();

        // Se guarda la cuenta contable encontrada mediante su codigo.
        DataTable AB_Cuentas = AB_ConexionBD.AB_ExecuteQuery("" +
            "SELECT AB_IdCuenta, AB_CodigoCuenta, AB_NombreCuenta, AB_Naturaleza, AB_TipoCuenta " +
            "FROM AB_CuentaContable " +
            "WHERE AB_CodigoCuenta = @CodigoCuenta",
            new string[] { "@CodigoCuenta" }, new object[] { AB_Codigo });

        // Se valida que la cuenta exista antes de modificarla.
        if (AB_Cuentas.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: La cuenta ingresada no existe.");
            AB_Pausa();
            return;
        }

        // Se convierte la fila encontrada en la cuenta que se va a modificar.
        DataRow AB_FilaCuenta = AB_Cuentas.Rows[0];

        AB_CuentaContable AB_CuentaModificada = new AB_CuentaContable(
            Convert.ToInt32(AB_FilaCuenta["AB_IdCuenta"]),
            AB_FilaCuenta["AB_CodigoCuenta"].ToString(),
            AB_FilaCuenta["AB_NombreCuenta"].ToString(),
            AB_FilaCuenta["AB_Naturaleza"].ToString(),
            AB_FilaCuenta["AB_TipoCuenta"].ToString());

        Console.WriteLine($"Nombre actual: {AB_CuentaModificada.AB_NombreCuenta}");

        Console.Write("Ingrese el nuevo nombre: ");

        // Se guarda el nuevo nombre que tendra la cuenta.
        string AB_NuevoNombre = Console.ReadLine() ?? "";

        // Si el nuevo nombre esta vacio, se cancela la modificacion de la cuenta.
        if (string.IsNullOrWhiteSpace(AB_NuevoNombre))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\nError: El nombre no puede estar vacio.");
            AB_Pausa();
            return;
        }

        // Se actualizan la entidad y el registro de SQL Server con el nuevo nombre.
        AB_CuentaModificada.AB_NombreCuenta = AB_NuevoNombre;

        // Se guarda el nuevo nombre en la cuenta que tiene el codigo ingresado.
        int AB_Actualizados = AB_ConexionBD.AB_ExecuteNonQuery("" +
            "UPDATE AB_CuentaContable " +
            "SET AB_NombreCuenta = @NombreCuenta " +
            "WHERE AB_CodigoCuenta = @CodigoCuenta",
            new string[] { "@NombreCuenta", "@CodigoCuenta" },
            new object[] { AB_CuentaModificada.AB_NombreCuenta, AB_CuentaModificada.AB_CodigoCuenta });

        Console.ForegroundColor = AB_Actualizados > 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine(AB_Actualizados > 0 ? "\nCuenta actualizada con exito." : "\nNo se pudo actualizar la cuenta.");
        AB_Pausa();
    }

    // Metodo para mostrar una linea del detalle contable.
    private void AB_MostrarLinea(string AB_Codigo, double AB_Debe, double AB_Haber)
    {
        // Se guarda la cuenta consultada para mostrar su nombre en el detalle.
        DataTable AB_Cuenta = AB_ConexionBD.AB_ExecuteQuery("" +
            "SELECT AB_IdCuenta, AB_CodigoCuenta, AB_NombreCuenta, AB_Naturaleza, AB_TipoCuenta " +
            "FROM AB_CuentaContable " +
            "WHERE AB_CodigoCuenta = @CodigoCuenta",
            new string[] { "@CodigoCuenta" }, new object[] { AB_Codigo });

        string AB_Nombre = "Cuenta Desconocida";

        // Se valida si el codigo corresponde a una cuenta del catalogo contable.
        if (AB_Cuenta.Rows.Count > 0)
        {
            // Se convierte la fila para mostrar el nombre real de la cuenta.
            DataRow AB_Fila = AB_Cuenta.Rows[0];

            AB_CuentaContable AB_CuentaConsultada = new AB_CuentaContable(
                Convert.ToInt32(AB_Fila["AB_IdCuenta"]),
                AB_Fila["AB_CodigoCuenta"].ToString(),
                AB_Fila["AB_NombreCuenta"].ToString(),
                AB_Fila["AB_Naturaleza"].ToString(),
                AB_Fila["AB_TipoCuenta"].ToString());
            AB_Nombre = AB_CuentaConsultada.AB_NombreCuenta;
        }

        Console.WriteLine($"{AB_Codigo}\t{AB_Nombre}\t${AB_Debe:F2}\t${AB_Haber:F2}");
    }

    // Metodo que espera ENTER antes de volver al menu.
    private void AB_Pausa()
    {
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("\nPresione cualquier tecla para continuar...");
        Console.ReadKey();
    }
}


