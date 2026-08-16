using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaSeguros;

public class AB_ModuloSiniestros
{
    // Variable para usar la conexion de SQL Server.
    private AB_Conexion AB_BaseDatos = AB_Conexion.AB_GetInstance();

    // Metodo para mostrar el menu del modulo de siniestros.
    public void AB_MenuSiniestros()
    {
        // Variable para guardar la opcion seleccionada.
        string AB_Opcion = "";

        do
        {
            Console.Clear();
            Console.WriteLine("=======================================================");
            Console.WriteLine("                 MODULO DE SINIESTROS                  ");
            Console.WriteLine("=======================================================");
            Console.WriteLine(" 1. Registrar Nuevo Siniestro");
            Console.WriteLine(" 2. Consultar Valores del Siniestro");
            Console.WriteLine(" 3. Modificar Estado / Datos de Reclamo");
            Console.WriteLine(" 4. Eliminar");
            Console.WriteLine(" 5. Filtrar Siniestros por Cliente");
            Console.WriteLine(" 6. Volver al Menu Principal");
            Console.WriteLine("=======================================================");
            Console.Write("Seleccione una opcion [1-6]: ");
            AB_Opcion = Console.ReadLine();

            // Se ejecuta la accion correspondiente a la opcion seleccionada.
            switch (AB_Opcion)
            {
                case "1":
                    AB_RegistrarSiniestro();
                    break;
                case "2":
                    AB_ConsultarSiniestro();
                    break;
                case "3":
                    AB_ModificarSiniestro();
                    break;
                case "4":
                    AB_AnularSiniestro();
                    break;
                case "5":
                    AB_FiltrarSiniestrosPorCliente();
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

    // --------------------------------------------------
    // 1. Metodo para REGISTRAR un nuevo siniestro.
    // --------------------------------------------------
    private void AB_RegistrarSiniestro()
    {
        // Se solicita la poliza afectada por el reclamo.
        Console.Clear();
        Console.WriteLine("--- REGISTRAR NUEVO SINIESTRO ---");
        Console.Write("Ingrese el numero de poliza: ");

        // Se recibe el numero de poliza sobre la que se registrara el reclamo.
        string AB_NumeroPoliza = Console.ReadLine();

        // Consulta que busca la poliza y el cliente relacionados con el reclamo.
        string AB_ConsultaPoliza =
            "SELECT p.AB_IdPoliza AS AB_PolizaId, p.AB_IdCliente AS AB_PolizaIdCliente, " +
            "p.AB_IdRamo AS AB_PolizaIdRamo, p.AB_NumeroPoliza, p.AB_CapitalAsegurado, " +
            "p.AB_TasaRiesgo, p.AB_PrimaBase, p.AB_SuperBancos, p.AB_SeguroCampesino, " +
            "p.AB_DerechosEmision, p.AB_IVA, p.AB_PrimaTotal, p.AB_CapitalRemanente, p.AB_Estado, " +
            "c.AB_IdCliente, c.AB_Cedula, c.AB_Nombres, c.AB_Apellidos, " +
            "c.AB_Direccion, c.AB_Telefono, c.AB_Correo " +
            "FROM AB_Poliza p " +
            "INNER JOIN AB_Cliente c " +
            "ON p.AB_IdCliente = c.AB_IdCliente " +
            "WHERE p.AB_NumeroPoliza = @NumeroPoliza";

        // Se guardan los datos de la poliza y del cliente encontrados.
        DataTable AB_TablaPolizas = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaPoliza,
            new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });

        // Si la poliza no existe, se cancela el registro.
        if (AB_TablaPolizas.Rows.Count == 0)
        {
            // Se prepara el error que informa que la poliza no existe.
            try
            {
                // Se lanza la excepcion cuando la poliza no existe.
                throw new AB_PolizaInvalidaException("No se puede registrar un siniestro para una poliza inexistente.");
            }
            // Se muestra el error de la poliza y se cancela el registro.
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

        // Se convierten la poliza y el cliente relacionados en objetos.
        DataRow AB_FilaPoliza = AB_TablaPolizas.Rows[0];

        AB_Poliza AB_PolizaAfectada = AB_CrearPolizaRelacionadaDesdeFila(AB_FilaPoliza);
        AB_Cliente AB_ClienteAfectado = AB_CrearClienteDesdeFila(AB_FilaPoliza);

        // Solo las polizas activas pueden registrar siniestros.
        if (AB_PolizaAfectada.AB_Estado != "ACTIVA")
        {
            // Se prepara el error que informa que la poliza no esta activa.
            try
            {
                // Se lanza la excepcion porque la poliza no puede recibir reclamos.
                throw new AB_SiniestroInvalidoException("La poliza no se encuentra activa.");
            }
            // Se muestra el error del siniestro y se cancela el registro.
            catch (AB_SiniestroInvalidoException AB_Excepcion)
            {
                // Se muestra y registra el error del siniestro.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nERROR DE SINIESTRO {AB_Excepcion.Message}");
                Console.ForegroundColor = ConsoleColor.White;
                AB_BaseDatos.AB_RegistrarLog("SINIESTROS", "ERROR", AB_Excepcion.Message);
                AB_Pausar();
                return;
            }
        }

        // Se obtienen los identificadores y el capital que aun puede usarse.
        int AB_IdPoliza = AB_PolizaAfectada.AB_IdPoliza;
        int AB_IdCliente = AB_ClienteAfectado.AB_IdCliente;
        double AB_CapitalRemanente = AB_PolizaAfectada.AB_CapitalRemanente;

        Console.WriteLine($"Cliente: {AB_ClienteAfectado.AB_Nombres} {AB_ClienteAfectado.AB_Apellidos}");
        Console.WriteLine($"Capital remanente: ${AB_CapitalRemanente:N2}");

        // Se solicita el valor de los danos que se reclaman.
        double AB_Danos = 0;
        Console.Write("Ingrese el monto de danos reclamados: ");

        // Se repite la lectura hasta que se ingrese un monto positivo.
        while (!double.TryParse(Console.ReadLine(), out AB_Danos) || AB_Danos <= 0)
        {
            Console.Write("Ingrese un valor numerico mayor a cero: ");
        }

        // El dano no puede superar el capital disponible.
        if (AB_Danos > AB_CapitalRemanente)
        {
            // Se prepara el error que informa que el dano supera la cobertura.
            try
            {
                // Se lanza la excepcion porque el reclamo supera la cobertura.
                throw new AB_SiniestroInvalidoException("El monto supera el capital remanente de la poliza.");
            }
            // Se muestra el error de cobertura y se cancela el registro.
            catch (AB_SiniestroInvalidoException AB_Excepcion)
            {
                // Se muestra y registra el error del siniestro.
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\nERROR DE SINIESTRO {AB_Excepcion.Message}");
                Console.ForegroundColor = ConsoleColor.White;
                AB_BaseDatos.AB_RegistrarLog("SINIESTROS", "ERROR", AB_Excepcion.Message);
                AB_Pausar();
                return;
            }
        }

        // Se solicita el porcentaje de deducible del siniestro.
        double AB_PorcentajeDeducible = 0;
        Console.Write("Ingrese el porcentaje de deducible: ");

        // Se repite la lectura hasta que se ingrese un porcentaje entre 0 y 100.
        while (!double.TryParse(Console.ReadLine(), out AB_PorcentajeDeducible) ||
            AB_PorcentajeDeducible < 0 || AB_PorcentajeDeducible > 100)
        {
            Console.Write("Ingrese un porcentaje entre 0 y 100: ");
        }

        // Se calcula el deducible y el valor que debe pagar el seguro.
        double AB_Deducible = AB_Danos * (AB_PorcentajeDeducible / 100.0);
        double AB_PagoNeto = AB_Danos - AB_Deducible;
        string AB_EstadoAuditoria = AB_Danos > 5000 ?"AUDITORIA REQUERIDA" : "PAGO EXPRES";

        // Se guardan las alertas UAF del cliente para definir el estado del reclamo.
        DataTable AB_TablaAlertas = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdAlerta, AB_IdCliente, AB_CodigoAlerta, AB_NivelRiesgo, AB_FechaReporte " +
            "FROM AB_AlertaUAF " +
            "WHERE AB_IdCliente = @IdCliente",
            new string[] { "@IdCliente" }, new object[] { AB_IdCliente });

        // Ciclo para revisar cada alerta encontrada.
        for (int AB_Indice = 0; AB_Indice < AB_TablaAlertas.Rows.Count; AB_Indice++)
        {
            // Se convierte cada fila en una alerta antes de aplicar su regla.
            DataRow AB_FilaAlerta = AB_TablaAlertas.Rows[AB_Indice];
            AB_AlertaUAF AB_AlertaActual = new AB_AlertaUAF(
                Convert.ToInt32(AB_FilaAlerta["AB_IdAlerta"]),
                Convert.ToInt32(AB_FilaAlerta["AB_IdCliente"]),
                AB_FilaAlerta["AB_CodigoAlerta"].ToString(),
                AB_FilaAlerta["AB_NivelRiesgo"].ToString(),
                Convert.ToDateTime(AB_FilaAlerta["AB_FechaReporte"]));

            // El codigo 999 cambia el estado a posible fraude.
            if (AB_AlertaActual.AB_CodigoAlerta == "999")
            {
                AB_PagoNeto = 0;
                AB_EstadoAuditoria = "RECHAZADO POR FRAUDE UAF 999";
                break;
            }

            // El codigo 404 registra una observacion documental.
            if (AB_AlertaActual.AB_CodigoAlerta == "404")
            {
                AB_EstadoAuditoria = "APROBADO CON CONDICION 404";
            }
        }

        // Se consulta el ultimo identificador para formar el siguiente numero de reclamo.
        DataTable AB_TablaUltimoSiniestro = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT MAX(AB_IdSiniestro) AS AB_UltimoId " +
            "FROM AB_Siniestro");

        int AB_UltimoIdSiniestro = 0;

        // Se toma el ultimo identificador guardado cuando existe un siniestro anterior.
        if (AB_TablaUltimoSiniestro.Rows.Count > 0 &&AB_TablaUltimoSiniestro.Rows[0]["AB_UltimoId"] != DBNull.Value)
        {
            AB_UltimoIdSiniestro =Convert.ToInt32(AB_TablaUltimoSiniestro.Rows[0]["AB_UltimoId"]);
        }

        // Se genera el numero del reclamo y se actualiza el capital disponible.
        string AB_NumeroReclamo = "SIN-" + (AB_UltimoIdSiniestro + 1).ToString("000");
        double AB_NuevoCapitalRemanente = AB_CapitalRemanente - AB_Danos;

        // Se crea el siniestro con los montos calculados y el estado de auditoria.
        AB_Siniestro AB_NuevoSiniestro = new AB_Siniestro(0, AB_IdPoliza,
            AB_NumeroReclamo, DateTime.Now, AB_Danos, AB_Deducible,
            AB_PagoNeto, AB_EstadoAuditoria);

        // Consulta que guarda el nuevo siniestro.
        string AB_ConsultaInsertar =
            "INSERT INTO AB_Siniestro " +
            "(AB_IdPoliza, AB_NumeroReclamo, AB_FechaSiniestro, AB_DanosReclamados, " +
            "AB_DeducibleAsumido, AB_PagoNeto, AB_EstadoAuditoria)" +
            " VALUES " +
            "(@IdPoliza, @NumeroReclamo, @FechaSiniestro, @DanosReclamados, " +
            "@DeducibleAsumido, @PagoNeto, @EstadoAuditoria)";

        // Consulta que descuenta el dano del capital remanente de la poliza.
        string AB_ConsultaActualizar =
            "UPDATE AB_Poliza " +
            "SET AB_CapitalRemanente = @CapitalRemanente " +
            "WHERE AB_IdPoliza = @IdPoliza";

        // Si la transaccion falla, se cancela el registro del siniestro.
        if (!AB_GuardarSiniestroTransaccion(AB_ConsultaInsertar, AB_ConsultaActualizar,
            AB_NuevoSiniestro, AB_NuevoCapitalRemanente))
        {
            Console.WriteLine("No se pudo guardar el siniestro.");
            AB_Pausar();
            return;
        }

        // Se registra el nuevo siniestro en la tabla de logs.
        AB_BaseDatos.AB_RegistrarLog("SINIESTROS", "REGISTRAR SINIESTRO", "Siniestro " + AB_NumeroReclamo + " registrado correctamente.");

        // Salida.
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("=======================================================");
        Console.WriteLine("          SINIESTRO REGISTRADO CON EXITO               ");
        Console.WriteLine("=======================================================");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine($"Reclamo:\t\t{AB_NumeroReclamo}");
        Console.WriteLine($"Danos Reclamados:\t${AB_Danos:F2}");
        Console.WriteLine($"Deducible Asumido:\t${AB_Deducible:F2}");
        Console.WriteLine($"Pago Neto Seguro:\t${AB_PagoNeto:F2}");
        Console.WriteLine($"Estado Auditoria:\t{AB_EstadoAuditoria}");
        Console.WriteLine($"Capital Remanente:\t${AB_NuevoCapitalRemanente:F2}");
        Console.WriteLine("=======================================================");
        AB_Pausar();
    }

    // Metodo para GUARDAR el siniestro y actualizar el capital en una transaccion.
    // Uso de COMMIT Y ROLLBACK.
    private bool AB_GuardarSiniestroTransaccion(string AB_ConsultaInsertar,string AB_ConsultaActualizar, AB_Siniestro AB_NuevoSiniestro,
                                                double AB_NuevoCapitalRemanente)
    {
        // Se intenta abrir la conexion e iniciar la transaccion del siniestro.
        try
        {
            // Se abre la conexion antes de iniciar la transaccion.
            AB_BaseDatos.AB_OpenConnection();
            SqlConnection AB_Conexion = AB_BaseDatos.AB_ObtenerConexion();

            // Se inicia la transaccion para guardar todo o no guardar nada.
            using (SqlTransaction AB_Transaccion = AB_Conexion.BeginTransaction())
            {
                // Se intentan guardar el siniestro y el nuevo capital como una sola operacion.
                try
                {
                    // Se guarda el nuevo siniestro en la base de datos.
                    using (SqlCommand AB_ComandoInsertar = new SqlCommand(AB_ConsultaInsertar, AB_Conexion, AB_Transaccion))
                    {
                        // Se agregan los datos del siniestro mediante parametros.
                        AB_ComandoInsertar.Parameters.AddWithValue("@IdPoliza", AB_NuevoSiniestro.AB_IdPoliza);
                        AB_ComandoInsertar.Parameters.AddWithValue("@NumeroReclamo", AB_NuevoSiniestro.AB_NumeroReclamo);
                        AB_ComandoInsertar.Parameters.AddWithValue("@FechaSiniestro", AB_NuevoSiniestro.AB_FechaSiniestro);
                        AB_ComandoInsertar.Parameters.AddWithValue("@DanosReclamados", AB_NuevoSiniestro.AB_DanosReclamados);
                        AB_ComandoInsertar.Parameters.AddWithValue("@DeducibleAsumido", AB_NuevoSiniestro.AB_DeducibleAsumido);
                        AB_ComandoInsertar.Parameters.AddWithValue("@PagoNeto", AB_NuevoSiniestro.AB_PagoNeto);
                        AB_ComandoInsertar.Parameters.AddWithValue("@EstadoAuditoria", AB_NuevoSiniestro.AB_EstadoAuditoria);

                        // Se valida que el siniestro se haya insertado.
                        if (AB_ComandoInsertar.ExecuteNonQuery() == 0)
                        {
                            throw new Exception("No se pudo guardar el siniestro.");
                        }
                    }

                    // Se actualiza el capital remanente de la poliza.
                    using (SqlCommand AB_ComandoActualizar = new SqlCommand(AB_ConsultaActualizar, AB_Conexion, AB_Transaccion))
                    {
                        // Se agregan el capital y la poliza mediante parametros.
                        AB_ComandoActualizar.Parameters.AddWithValue("@CapitalRemanente", AB_NuevoCapitalRemanente);
                        AB_ComandoActualizar.Parameters.AddWithValue("@IdPoliza", AB_NuevoSiniestro.AB_IdPoliza);

                        // Se valida que el capital se haya actualizado.
                        if (AB_ComandoActualizar.ExecuteNonQuery() == 0)
                        {
                            throw new Exception("No se pudo actualizar el capital remanente.");
                        }
                    }

                    // Se confirman las dos operaciones del siniestro.
                    AB_Transaccion.Commit();
                    return true;
                }
                // Si una operacion falla, se revierten los dos cambios.
                catch (Exception AB_Excepcion)
                {
                    // Se deshacen los cambios si una operacion falla.
                    AB_Transaccion.Rollback();
                    Console.WriteLine($"Error al guardar el siniestro: {AB_Excepcion.Message}");
                    return false;
                }
            }
        }
        // Se informa si no fue posible abrir la conexion o iniciar la transaccion.
        catch (Exception AB_Excepcion)
        {
            // Se muestra el error si no se puede iniciar la transaccion.
            Console.WriteLine($"Error de conexion al guardar el siniestro: {AB_Excepcion.Message}");
            return false;
        }
        // La conexion se cierra tanto si el siniestro se guarda como si ocurre un error.
        finally
        {
            // Se cierra la conexion despues de terminar la transaccion.
            AB_BaseDatos.AB_CloseConnection();
        }
    }

    // --------------------------------------------------
    // 2. Metodo para CONSULTAR uno o varios siniestros.
    // --------------------------------------------------
    private void AB_ConsultarSiniestro()
    {
        Console.Clear();
        Console.WriteLine("--- CONSULTAR SINIESTRO ---");
        Console.Write("Ingrese el numero de reclamo o presione ENTER para listar todos: ");

        // Se recibe el numero de reclamo que se desea consultar.
        string AB_NumeroReclamo = Console.ReadLine();

        // Consulta que obtiene los datos del reclamo solicitado.
        string AB_Consulta =
            "SELECT s.AB_IdSiniestro, s.AB_IdPoliza, s.AB_NumeroReclamo, p.AB_NumeroPoliza, " +
            "s.AB_DanosReclamados, s.AB_DeducibleAsumido, s.AB_PagoNeto, " +
            "s.AB_EstadoAuditoria, s.AB_FechaSiniestro " +
            "FROM AB_Siniestro s " +
            "INNER JOIN AB_Poliza p " +
            "ON s.AB_IdPoliza = p.AB_IdPoliza";

        DataTable AB_TablaSiniestros;

        // Si se ingresa un numero, se busca solo ese siniestro.
        if (AB_NumeroReclamo != "")
        {
            AB_Consulta = AB_Consulta + " " +
                "WHERE s.AB_NumeroReclamo = @NumeroReclamo";

            // Se guardan solamente los datos del reclamo solicitado.
            AB_TablaSiniestros = AB_BaseDatos.AB_ExecuteQuery(AB_Consulta,
                new string[] { "@NumeroReclamo" }, new object[] { AB_NumeroReclamo });
        }
        else
        {
            // Se guardan todos los reclamos cuando no se ingreso un numero.
            AB_TablaSiniestros = AB_BaseDatos.AB_ExecuteQuery(AB_Consulta);
        }

        // Se informa si la busqueda no encontro siniestros para mostrar.
        if (AB_TablaSiniestros.Rows.Count == 0)
        {
            Console.WriteLine("No hay siniestros para mostrar.");
            AB_Pausar();
            return;
        }

        // Ciclo para mostrar cada siniestro encontrado.
        for (int AB_Indice = 0; AB_Indice < AB_TablaSiniestros.Rows.Count; AB_Indice++)
        {
            // Se convierte cada fila encontrada en una entidad siniestro.
            DataRow AB_Fila = AB_TablaSiniestros.Rows[AB_Indice];

            AB_Siniestro AB_SiniestroConsultado = new AB_Siniestro(
                Convert.ToInt32(AB_Fila["AB_IdSiniestro"]),
                Convert.ToInt32(AB_Fila["AB_IdPoliza"]),
                AB_Fila["AB_NumeroReclamo"].ToString(),
                Convert.ToDateTime(AB_Fila["AB_FechaSiniestro"]),
                Convert.ToDouble(AB_Fila["AB_DanosReclamados"]),
                Convert.ToDouble(AB_Fila["AB_DeducibleAsumido"]),
                Convert.ToDouble(AB_Fila["AB_PagoNeto"]),
                AB_Fila["AB_EstadoAuditoria"].ToString());

            // Salida.
            Console.WriteLine("\n=======================================================");
            Console.WriteLine($"DETALLE DE: {AB_SiniestroConsultado.AB_NumeroReclamo}");
            Console.WriteLine("=======================================================");
            Console.WriteLine($"Poliza Asociada:\t{AB_Fila["AB_NumeroPoliza"]}");
            Console.WriteLine($"Monto del Siniestro:\t${AB_SiniestroConsultado.AB_DanosReclamados:F2}");
            Console.WriteLine($"Deducible Cliente:\t${AB_SiniestroConsultado.AB_DeducibleAsumido:F2}");
            Console.WriteLine($"Neto Pagado Seguro:\t${AB_SiniestroConsultado.AB_PagoNeto:F2}");
            Console.WriteLine($"Estado Auditoria:\t{AB_SiniestroConsultado.AB_EstadoAuditoria}");
            Console.WriteLine("=======================================================");
        }
        AB_Pausar();
    }

    // --------------------------------------------------
    // 3. Metodo para MODIFICAR los valores de un reclamo.
    // --------------------------------------------------
    private void AB_ModificarSiniestro()
    {
        Console.Clear();
        Console.WriteLine("--- MODIFICAR SINIESTRO ---");
        Console.Write("Ingrese el numero de reclamo: ");

        // Se recibe el numero de reclamo que se va a modificar.
        string AB_NumeroReclamo = Console.ReadLine();

        // Consulta que busca el siniestro y la poliza que se van a recalcular.
        string AB_Consulta =
            "SELECT s.AB_IdSiniestro, s.AB_IdPoliza, s.AB_NumeroReclamo, s.AB_FechaSiniestro, " +
            "s.AB_DanosReclamados, s.AB_DeducibleAsumido, s.AB_PagoNeto, " +
            "s.AB_EstadoAuditoria, p.AB_IdPoliza AS AB_PolizaId, " +
            "p.AB_IdCliente AS AB_PolizaIdCliente, p.AB_IdRamo AS AB_PolizaIdRamo, " +
            "p.AB_NumeroPoliza, p.AB_CapitalAsegurado, p.AB_TasaRiesgo, p.AB_PrimaBase, " +
            "p.AB_SuperBancos, p.AB_SeguroCampesino, p.AB_DerechosEmision, p.AB_IVA, " +
            "p.AB_PrimaTotal, p.AB_CapitalRemanente, p.AB_Estado " +
            "FROM AB_Siniestro s " +
            "INNER JOIN AB_Poliza p " +
            "ON s.AB_IdPoliza = p.AB_IdPoliza " +
            "WHERE s.AB_NumeroReclamo = @NumeroReclamo";

        // Se guardan los valores actuales del siniestro y de su poliza.
        DataTable AB_TablaSiniestros = AB_BaseDatos.AB_ExecuteQuery(AB_Consulta,
            new string[] { "@NumeroReclamo" }, new object[] { AB_NumeroReclamo });

        // Se valida que el numero de reclamo corresponda a un siniestro registrado.
        if (AB_TablaSiniestros.Rows.Count == 0)
        {
            Console.WriteLine("Siniestro no encontrado.");
            AB_Pausar();
            return;
        }

        // Se convierte la fila encontrada en el siniestro que se va a modificar.
        DataRow AB_FilaSiniestro = AB_TablaSiniestros.Rows[0];

        AB_Siniestro AB_SiniestroActual = new AB_Siniestro(
            Convert.ToInt32(AB_FilaSiniestro["AB_IdSiniestro"]),
            Convert.ToInt32(AB_FilaSiniestro["AB_IdPoliza"]),
            AB_FilaSiniestro["AB_NumeroReclamo"].ToString(),
            Convert.ToDateTime(AB_FilaSiniestro["AB_FechaSiniestro"]),
            Convert.ToDouble(AB_FilaSiniestro["AB_DanosReclamados"]),
            Convert.ToDouble(AB_FilaSiniestro["AB_DeducibleAsumido"]),
            Convert.ToDouble(AB_FilaSiniestro["AB_PagoNeto"]),
            AB_FilaSiniestro["AB_EstadoAuditoria"].ToString());

        // Objeto nuevo.
        AB_Poliza AB_PolizaActual = AB_CrearPolizaRelacionadaDesdeFila(AB_FilaSiniestro);

        string AB_EstadoAnterior = AB_SiniestroActual.AB_EstadoAuditoria;

        // Un siniestro anulado no puede ser modificado.
        if (AB_EstadoAnterior == "ANULADO")
        {
            Console.WriteLine("Un siniestro anulado no puede ser modificado.");
            AB_Pausar();
            return;
        }

        // Se recuperan el siniestro, la poliza, el cliente y la cobertura disponible.
        int AB_IdSiniestro = AB_SiniestroActual.AB_IdSiniestro;
        int AB_IdPoliza = AB_SiniestroActual.AB_IdPoliza;
        int AB_IdCliente = AB_PolizaActual.AB_IdCliente;
        double AB_DanosAnteriores = AB_SiniestroActual.AB_DanosReclamados;
        double AB_RemanenteActual = AB_PolizaActual.AB_CapitalRemanente;
        double AB_CoberturaDisponible = AB_RemanenteActual + AB_DanosAnteriores;

        Console.WriteLine($"Monto anterior: ${AB_DanosAnteriores:N2}");
        Console.WriteLine($"Cobertura disponible: ${AB_CoberturaDisponible:N2}");

        // Se solicita el nuevo monto de danos para el reclamo.
        double AB_NuevosDanos = 0;
        Console.Write("Ingrese el nuevo monto de danos: ");

        // Se repite la lectura hasta que se ingrese un monto positivo.
        while (!double.TryParse(Console.ReadLine(), out AB_NuevosDanos) || AB_NuevosDanos <= 0)
        {
            Console.Write("Ingrese un valor numerico mayor a cero: ");
        }

        // Se valida que el nuevo dano no supere la cobertura.
        if (AB_NuevosDanos > AB_CoberturaDisponible)
        {
            Console.WriteLine("El monto supera la cobertura disponible.");
            AB_Pausar();
            return;
        }

        // Se solicita el nuevo porcentaje de deducible.
        double AB_PorcentajeDeducible = 0;
        Console.Write("Ingrese el nuevo porcentaje de deducible: ");

        // Se repite la lectura hasta que se ingrese un porcentaje entre 0 y 100.
        while (!double.TryParse(Console.ReadLine(), out AB_PorcentajeDeducible) ||
            AB_PorcentajeDeducible < 0 || AB_PorcentajeDeducible > 100)
        {
            Console.Write("Ingrese un porcentaje entre 0 y 100: ");
        }

        // Se recalculan el deducible, el pago y el nuevo estado de revision.
        double AB_NuevoDeducible = AB_NuevosDanos * (AB_PorcentajeDeducible / 100.0);
        double AB_NuevoPagoNeto = AB_NuevosDanos - AB_NuevoDeducible;
        string AB_NuevoEstado = AB_NuevosDanos > 5000 ?"AUDITORIA REQUERIDA" : "PAGO EXPRES";

        // Se busca la alerta UAF 999 del cliente.
        DataTable AB_TablaAlertasFraude = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdAlerta, AB_IdCliente, AB_CodigoAlerta, AB_NivelRiesgo, AB_FechaReporte " +
            "FROM AB_AlertaUAF " +
            "WHERE AB_IdCliente = @IdCliente " +
            "AND AB_CodigoAlerta = '999'",
            new string[] { "@IdCliente" }, new object[] { AB_IdCliente });

        bool AB_TieneFraudeUAF = false;

        // La alerta encontrada se convierte en objeto antes de confirmar el fraude.
        if (AB_TablaAlertasFraude.Rows.Count > 0)
        {
            DataRow AB_FilaAlertaFraude = AB_TablaAlertasFraude.Rows[0];
            AB_AlertaUAF AB_AlertaFraude = new AB_AlertaUAF(
                Convert.ToInt32(AB_FilaAlertaFraude["AB_IdAlerta"]),
                Convert.ToInt32(AB_FilaAlertaFraude["AB_IdCliente"]),
                AB_FilaAlertaFraude["AB_CodigoAlerta"].ToString(),
                AB_FilaAlertaFraude["AB_NivelRiesgo"].ToString(),
                Convert.ToDateTime(AB_FilaAlertaFraude["AB_FechaReporte"]));

            AB_TieneFraudeUAF = AB_AlertaFraude.AB_CodigoAlerta == "999";
        }

        // Si existe alerta critica, se marca el reclamo como fraude.
        if (AB_TieneFraudeUAF)
        {
            AB_NuevoPagoNeto = 0;
            AB_NuevoEstado = "RECHAZADO POR FRAUDE UAF 999";
        }

        double AB_NuevoRemanente = AB_CoberturaDisponible - AB_NuevosDanos;

        // Se actualiza el objeto antes de construir la consulta SQL.
        AB_SiniestroActual.AB_DanosReclamados = AB_NuevosDanos;
        AB_SiniestroActual.AB_DeducibleAsumido = AB_NuevoDeducible;
        AB_SiniestroActual.AB_PagoNeto = AB_NuevoPagoNeto;
        AB_SiniestroActual.AB_EstadoAuditoria = AB_NuevoEstado;

        // Consulta que guarda los cambios realizados en el siniestro.
        string AB_ConsultaActualizarSiniestro =
            "UPDATE AB_Siniestro " +
            "SET " +
            "AB_DanosReclamados = @DanosReclamados, " +
            "AB_DeducibleAsumido = @DeducibleAsumido, " +
            "AB_PagoNeto = @PagoNeto, " +
            "AB_EstadoAuditoria = @EstadoAuditoria " +
            "WHERE AB_IdSiniestro = @IdSiniestro";

        // Si el siniestro se actualiza, tambien se guarda el nuevo capital de la poliza.
        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaActualizarSiniestro,
            new string[] { "@DanosReclamados", "@DeducibleAsumido", "@PagoNeto",
                "@EstadoAuditoria", "@IdSiniestro" },
            new object[] { AB_SiniestroActual.AB_DanosReclamados,
                AB_SiniestroActual.AB_DeducibleAsumido, AB_SiniestroActual.AB_PagoNeto,
                AB_SiniestroActual.AB_EstadoAuditoria, AB_SiniestroActual.AB_IdSiniestro }) > 0)
        {
            // Consulta que guarda el nuevo capital disponible de la poliza.
            string AB_ConsultaActualizarPoliza =
                "UPDATE AB_Poliza " +
                "SET AB_CapitalRemanente = @CapitalRemanente " +
                "WHERE AB_IdPoliza = @IdPoliza";

            // Se actualiza el capital despues de modificar el monto del siniestro.
            AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaActualizarPoliza,
                new string[] { "@CapitalRemanente", "@IdPoliza" },
                new object[] { AB_NuevoRemanente, AB_IdPoliza });

            // Se registra la modificacion del siniestro en la tabla de logs.
            AB_BaseDatos.AB_RegistrarLog("SINIESTROS", "MODIFICAR SINIESTRO", "Siniestro " + AB_NumeroReclamo + " actualizado correctamente.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSiniestro recalculado exitosamente.");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.WriteLine("No se pudo actualizar el siniestro.");
        }

        AB_Pausar();
    }

    // --------------------------------------------------
    // 4. Metodo para ANULAR un siniestro y devolver el capital a la poliza.
    // --------------------------------------------------
    private void AB_AnularSiniestro()
    {
        Console.Clear();
        Console.WriteLine("--- ANULAR SINIESTRO ---");
        Console.Write("Ingrese el numero de reclamo: ");

        // Se recibe el numero de reclamo que se va a anular.
        string AB_NumeroReclamo = Console.ReadLine();

        // Consulta que busca los datos del siniestro que se va a anular.
        string AB_Consulta =
            "SELECT AB_IdSiniestro, AB_IdPoliza, AB_NumeroReclamo, AB_FechaSiniestro, " +
            "AB_DanosReclamados, AB_DeducibleAsumido, AB_PagoNeto, AB_EstadoAuditoria " +
            "FROM AB_Siniestro " +
            "WHERE AB_NumeroReclamo = @NumeroReclamo";

        // Se guarda el siniestro encontrado para comprobar su estado y sus montos.
        DataTable AB_TablaSiniestros = AB_BaseDatos.AB_ExecuteQuery(AB_Consulta,
            new string[] { "@NumeroReclamo" }, new object[] { AB_NumeroReclamo });

        // Se comprueba si se encontro el siniestro que se desea anular.
        if (AB_TablaSiniestros.Rows.Count == 0)
        {
            Console.WriteLine("Siniestro no encontrado.");
            AB_Pausar();
            return;
        }

        // Se convierte la fila encontrada en el siniestro que se va a anular.
        DataRow AB_FilaSiniestro = AB_TablaSiniestros.Rows[0];
        AB_Siniestro AB_SiniestroAnulado = new AB_Siniestro(
            Convert.ToInt32(AB_FilaSiniestro["AB_IdSiniestro"]),
            Convert.ToInt32(AB_FilaSiniestro["AB_IdPoliza"]),
            AB_FilaSiniestro["AB_NumeroReclamo"].ToString(),
            Convert.ToDateTime(AB_FilaSiniestro["AB_FechaSiniestro"]),
            Convert.ToDouble(AB_FilaSiniestro["AB_DanosReclamados"]),
            Convert.ToDouble(AB_FilaSiniestro["AB_DeducibleAsumido"]),
            Convert.ToDouble(AB_FilaSiniestro["AB_PagoNeto"]),
            AB_FilaSiniestro["AB_EstadoAuditoria"].ToString());

        // Se evita anular un siniestro que ya fue anulado.
        if (AB_SiniestroAnulado.AB_EstadoAuditoria == "ANULADO")
        {
            Console.WriteLine("El siniestro ya se encuentra anulado.");
            AB_Pausar();
            return;
        }

        Console.Write("Confirma la anulacion del siniestro (S/N): ");

        // Se guarda la confirmacion del usuario antes de anular el reclamo.
        string AB_Confirmacion = Console.ReadLine();

        // Si el usuario no confirma, se cancela la anulacion.
        if (AB_Confirmacion != "S" && AB_Confirmacion != "s")
        {
            Console.WriteLine("Operacion cancelada.");
            AB_Pausar();
            return;
        }

        // Se obtienen el siniestro, la poliza y el dano que se devolvera al capital.
        int AB_IdSiniestro = AB_SiniestroAnulado.AB_IdSiniestro;
        int AB_IdPoliza = AB_SiniestroAnulado.AB_IdPoliza;
        double AB_DanosAnteriores = AB_SiniestroAnulado.AB_DanosReclamados;

        // Cambio de datos.
        AB_SiniestroAnulado.AB_EstadoAuditoria = "ANULADO";
        AB_SiniestroAnulado.AB_DanosReclamados = 0;
        AB_SiniestroAnulado.AB_DeducibleAsumido = 0;
        AB_SiniestroAnulado.AB_PagoNeto = 0;

        // Consulta que marca el siniestro como anulado.
        string AB_ConsultaActualizarSiniestro =
            "UPDATE AB_Siniestro " +
            "SET AB_EstadoAuditoria = @EstadoAuditoria, " +
            "AB_DanosReclamados = @DanosReclamados, " +
            "AB_DeducibleAsumido = @DeducibleAsumido, " +
            "AB_PagoNeto = @PagoNeto " +
            "WHERE AB_IdSiniestro = @IdSiniestro";

        // Si se anula el siniestro, se devuelve su capital a la poliza.
        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaActualizarSiniestro,
            new string[] { "@EstadoAuditoria", "@DanosReclamados", "@DeducibleAsumido",
                "@PagoNeto", "@IdSiniestro" },
            new object[] { AB_SiniestroAnulado.AB_EstadoAuditoria,
                AB_SiniestroAnulado.AB_DanosReclamados, AB_SiniestroAnulado.AB_DeducibleAsumido,
                AB_SiniestroAnulado.AB_PagoNeto, AB_SiniestroAnulado.AB_IdSiniestro }) > 0)
        {
            // Consulta que devuelve a la poliza los danos del siniestro anulado.
            string AB_ConsultaActualizarPoliza =
                "UPDATE AB_Poliza " +
                "SET AB_CapitalRemanente = AB_CapitalRemanente + @DanosAnteriores " +
                "WHERE AB_IdPoliza = @IdPoliza";

            // Se suma nuevamente el monto anulado al capital disponible.
            AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaActualizarPoliza,
                new string[] { "@DanosAnteriores", "@IdPoliza" },
                new object[] { AB_DanosAnteriores, AB_IdPoliza });

            // Se registra la anulacion del siniestro en la tabla de logs.
            AB_BaseDatos.AB_RegistrarLog("SINIESTROS", "ANULAR SINIESTRO", "Siniestro " + AB_NumeroReclamo + " anulado correctamente.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSiniestro anulado con exito (Montos revertidos a 0 y capital restituido).");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.WriteLine("No se pudo anular el siniestro.");
        }

        AB_Pausar();
    }

    // --------------------------------------------------
    // 5. Metodo para LISTAR los siniestros de un cliente por cedula.
    // --------------------------------------------------
    private void AB_FiltrarSiniestrosPorCliente()
    {
        Console.Clear();
        Console.WriteLine("--- FILTRAR SINIESTROS POR CLIENTE ---");
        Console.Write("Ingrese la cedula del cliente: ");
        string AB_Cedula = Console.ReadLine();

        // Se guardan el cliente y sus siniestros encontrados mediante la cedula.
        DataTable AB_Siniestros = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT c.AB_IdCliente, c.AB_Cedula, c.AB_Nombres, c.AB_Apellidos, " +
            "c.AB_Direccion, c.AB_Telefono, c.AB_Correo, s.AB_IdSiniestro, " +
            "s.AB_IdPoliza, s.AB_NumeroReclamo, p.AB_NumeroPoliza, " +
            "s.AB_FechaSiniestro, s.AB_DanosReclamados, s.AB_DeducibleAsumido, " +
            "s.AB_PagoNeto, s.AB_EstadoAuditoria " +
            "FROM AB_Siniestro s " +
            "INNER JOIN AB_Poliza p " +
            "ON s.AB_IdPoliza = p.AB_IdPoliza " +
            "INNER JOIN AB_Cliente c " +
            "ON p.AB_IdCliente = c.AB_IdCliente " +
            "WHERE c.AB_Cedula = @Cedula",
            new string[] { "@Cedula" }, new object[] { AB_Cedula });

        // Si no existen resultados, se informa al usuario.
        if (AB_Siniestros.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\nEl cliente no tiene siniestros registrados.");
            Console.ForegroundColor = ConsoleColor.White;
            AB_Pausar();
            return;
        }

        // Salida.
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("=======================================================");
        Console.WriteLine("           SINIESTROS DEL CLIENTE                      ");
        Console.WriteLine("=======================================================");
        Console.ForegroundColor = ConsoleColor.White;

        // Se crea el cliente del reporte antes de presentar sus datos.
        AB_Cliente AB_ClienteFiltrado = AB_CrearClienteDesdeFila(AB_Siniestros.Rows[0]);
        Console.WriteLine($"Cliente:\t{AB_ClienteFiltrado.AB_Nombres} {AB_ClienteFiltrado.AB_Apellidos}");
        Console.WriteLine($"Cedula:\t\t{AB_ClienteFiltrado.AB_Cedula}");
        Console.WriteLine("-------------------------------------------------------");

        // Ciclo para mostrar cada siniestro encontrado.
        for (int AB_Indice = 0; AB_Indice < AB_Siniestros.Rows.Count; AB_Indice++)
        {
            // Se convierte cada fila en un siniestro antes de mostrar sus datos.
            DataRow AB_Fila = AB_Siniestros.Rows[AB_Indice];

            AB_Siniestro AB_SiniestroCliente = new AB_Siniestro(
                Convert.ToInt32(AB_Fila["AB_IdSiniestro"]),
                Convert.ToInt32(AB_Fila["AB_IdPoliza"]),
                AB_Fila["AB_NumeroReclamo"].ToString(),
                Convert.ToDateTime(AB_Fila["AB_FechaSiniestro"]),
                Convert.ToDouble(AB_Fila["AB_DanosReclamados"]),
                Convert.ToDouble(AB_Fila["AB_DeducibleAsumido"]),
                Convert.ToDouble(AB_Fila["AB_PagoNeto"]),
                AB_Fila["AB_EstadoAuditoria"].ToString());

            // Se muestran los datos.
            Console.WriteLine($"Reclamo:\t{AB_SiniestroCliente.AB_NumeroReclamo}");
            Console.WriteLine($"Poliza:\t\t{AB_Fila["AB_NumeroPoliza"]}");
            Console.WriteLine($"Danos:\t\t${AB_SiniestroCliente.AB_DanosReclamados:F2}");
            Console.WriteLine($"Pago Neto:\t${AB_SiniestroCliente.AB_PagoNeto:F2}");
            Console.WriteLine($"Estado:\t\t{AB_SiniestroCliente.AB_EstadoAuditoria}");
            Console.WriteLine("-------------------------------------------------------");
        }

        AB_Pausar();
    }

    // ===========================================================================

    // ---------------------------------------------------------------------------
    //                         6. Metodos ADICIONALES
    // ---------------------------------------------------------------------------

    // ===========================================================================

    // Metodo que transforma las columnas relacionadas de una consulta en una poliza.
    private AB_Poliza AB_CrearPolizaRelacionadaDesdeFila(DataRow AB_Fila)
    {
        // Los valores permitidos como NULL se convierten en cero antes de crear el objeto.
        double AB_SuperBancos = AB_Fila["AB_SuperBancos"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_SuperBancos"]);
        double AB_SeguroCampesino = AB_Fila["AB_SeguroCampesino"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_SeguroCampesino"]);
        double AB_DerechosEmision = AB_Fila["AB_DerechosEmision"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_DerechosEmision"]);
        double AB_IVA = AB_Fila["AB_IVA"] == DBNull.Value ? 0 : Convert.ToDouble(AB_Fila["AB_IVA"]);

        return new AB_Poliza(
            Convert.ToInt32(AB_Fila["AB_PolizaId"]),
            Convert.ToInt32(AB_Fila["AB_PolizaIdCliente"]),
            Convert.ToInt32(AB_Fila["AB_PolizaIdRamo"]),
            AB_Fila["AB_NumeroPoliza"].ToString(),
            Convert.ToDouble(AB_Fila["AB_CapitalAsegurado"]),
            Convert.ToDouble(AB_Fila["AB_TasaRiesgo"]),
            Convert.ToDouble(AB_Fila["AB_PrimaBase"]),
            AB_SuperBancos, AB_SeguroCampesino, AB_DerechosEmision, AB_IVA,
            Convert.ToDouble(AB_Fila["AB_PrimaTotal"]),
            Convert.ToDouble(AB_Fila["AB_CapitalRemanente"]),
            AB_Fila["AB_Estado"].ToString());
    }

    // Metodo que transforma las columnas del cliente en una entidad AB_Cliente.
    private AB_Cliente AB_CrearClienteDesdeFila(DataRow AB_Fila)
    {
        return new AB_Cliente(
            Convert.ToInt32(AB_Fila["AB_IdCliente"]),
            AB_Fila["AB_Cedula"].ToString(),
            AB_Fila["AB_Nombres"].ToString(),
            AB_Fila["AB_Apellidos"].ToString(),
            AB_Fila["AB_Direccion"].ToString(),
            AB_Fila["AB_Telefono"].ToString(),
            AB_Fila["AB_Correo"].ToString());
    }

    // Metodo que espera ENTER antes de volver al menu.
    private void AB_Pausar()
    {
        Console.WriteLine("\nPresione ENTER para continuar...");
        Console.ReadLine();
    }
}

