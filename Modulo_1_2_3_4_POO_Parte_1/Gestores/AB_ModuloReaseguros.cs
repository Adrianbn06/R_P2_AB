using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace SistemaSeguros;

public class AB_ModuloReaseguros
{
    // Variable para usar la conexion de SQL Server.
    private AB_Conexion AB_BaseDatos = AB_Conexion.AB_GetInstance();

    // Metodo para mostrar el menu del modulo de reaseguros.
    public void AB_MenuReaseguros()
    {
        // Variable para guardar la opcion seleccionada.
        string AB_Opcion = "";

        do
        {
            Console.Clear();
            Console.WriteLine("=======================================================");
            Console.WriteLine("                  MODULO DE REASEGUROS                 ");
            Console.WriteLine("=======================================================");
            Console.WriteLine(" 1. Consultar Reaseguro");
            Console.WriteLine(" 2. Modificar");
            Console.WriteLine(" 3. Eliminar");
            Console.WriteLine(" 4. Volver al Menu Principal");
            Console.WriteLine("=======================================================");
            Console.Write("Seleccione una opcion [1-4]: ");
            AB_Opcion = Console.ReadLine();

            // Se ejecuta la accion correspondiente a la opcion seleccionada.
            switch (AB_Opcion)
            {
                case "1":
                case "2":
                    AB_ProcesarReaseguroExcedente();
                    break;
                case "3":
                    AB_EliminarReaseguro();
                    break;
                case "4":
                    break;
                default:
                    Console.WriteLine("Opcion no valida.");
                    AB_Pausar();
                    break;
            }
        }
        while (AB_Opcion != "4");
    }

    // --------------------------------------------------
    // 1. Metodo para CALCULAR / ACTUALIZAR el reaseguro de una poliza.
    // --------------------------------------------------
    private void AB_ProcesarReaseguroExcedente()
    {
        // Se solicita la poliza para calcular su reparto.
        Console.Clear();
        Console.WriteLine("--- CALCULAR REASEGURO ---");
        Console.Write("Ingrese el numero de poliza: ");

        // Se recibe el numero de la poliza que se repartira entre reaseguradoras.
        string AB_NumeroPoliza = Console.ReadLine();

        // Consulta que busca la poliza junto con su ramo y su cliente.
        string AB_ConsultaPoliza =
            "SELECT p.AB_IdPoliza, p.AB_IdCliente, p.AB_IdRamo, p.AB_NumeroPoliza, " +
            "p.AB_CapitalAsegurado, p.AB_TasaRiesgo, p.AB_PrimaBase, p.AB_SuperBancos, " +
            "p.AB_SeguroCampesino, p.AB_DerechosEmision, p.AB_IVA, p.AB_PrimaTotal, " +
            "p.AB_CapitalRemanente, p.AB_Estado, r.AB_IdRamo AS AB_RamoId, " +
            "r.AB_CodigoRamo, r.AB_NombreRamo, c.AB_IdCliente AS AB_ClienteIdRelacionado, " +
            "c.AB_Cedula, c.AB_Nombres, c.AB_Apellidos, c.AB_Direccion, c.AB_Telefono, c.AB_Correo " +
            "FROM AB_Poliza p " +
            "INNER JOIN AB_Cliente c " +
            "ON p.AB_IdCliente = c.AB_IdCliente " +
            "INNER JOIN AB_Ramo r " +
            "ON p.AB_IdRamo = r.AB_IdRamo " +
            "WHERE p.AB_NumeroPoliza = @NumeroPoliza";

        // Se guardan la poliza, su cliente y su ramo mediante el numero ingresado.
        DataTable AB_TablaPolizas = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaPoliza,
            new string[] { "@NumeroPoliza" }, new object[] { AB_NumeroPoliza });

        // Se valida que la poliza exista antes de crear el objeto.
        if (AB_TablaPolizas.Rows.Count == 0)
        {
            Console.WriteLine("La poliza no existe o no se encuentra activa.");
            AB_Pausar();
            return;
        }

        // Se convierte el registro SQL en una entidad poliza.
        AB_Poliza AB_PolizaReasegurada = AB_CrearPolizaDesdeFila(AB_TablaPolizas.Rows[0]);

        // Se toma la misma fila para crear el ramo y el cliente relacionados.
        DataRow AB_FilaRelacionada = AB_TablaPolizas.Rows[0];

        AB_Ramo AB_RamoReasegurado = new AB_Ramo(
            Convert.ToInt32(AB_FilaRelacionada["AB_RamoId"]),
            AB_FilaRelacionada["AB_CodigoRamo"].ToString(),
            AB_FilaRelacionada["AB_NombreRamo"].ToString());

        // Se crea el cliente para mostrar su nombre en el reparto.
        AB_Cliente AB_ClienteReasegurado = new AB_Cliente(
            Convert.ToInt32(AB_FilaRelacionada["AB_ClienteIdRelacionado"]),
            AB_FilaRelacionada["AB_Cedula"].ToString(),
            AB_FilaRelacionada["AB_Nombres"].ToString(),
            AB_FilaRelacionada["AB_Apellidos"].ToString(),
            AB_FilaRelacionada["AB_Direccion"].ToString(),
            AB_FilaRelacionada["AB_Telefono"].ToString(),
            AB_FilaRelacionada["AB_Correo"].ToString());

        // Se valida que la poliza exista y se encuentre activa antes de repartir el riesgo.
        if (AB_PolizaReasegurada.AB_Estado != "ACTIVA")
        {
            Console.WriteLine("La poliza no existe o no se encuentra activa.");
            AB_Pausar();
            return;
        }

        // Se toman el capital, la prima, el ramo y el cliente para calcular el reparto.
        int AB_IdPoliza = AB_PolizaReasegurada.AB_IdPoliza;
        double AB_Capital = AB_PolizaReasegurada.AB_CapitalAsegurado;
        double AB_Prima = AB_PolizaReasegurada.AB_PrimaTotal;
        string AB_NombreRamo = AB_RamoReasegurado.AB_NombreRamo;
        string AB_NombreCliente = AB_ClienteReasegurado.AB_Nombres + " " + AB_ClienteReasegurado.AB_Apellidos;

        // Valores iniciales que se completan segun las reaseguradoras disponibles.
        double AB_LimiteRetencion = 0;
        double AB_PorcentajeRetencion = 0;
        double AB_LimiteContrato = 0;
        double AB_PorcentajeContrato = 0;

        // Procedimiento 1: se BUSCAN los limites de reaseguro.
        AB_BuscarLimites(ref AB_LimiteRetencion, ref AB_PorcentajeRetencion,ref AB_LimiteContrato, ref AB_PorcentajeContrato);

        // Si no hay limite de contrato, no se puede calcular.
        if (AB_LimiteContrato == 0)
        {
            Console.WriteLine("No se encontraron limites de reaseguro configurados.");
            AB_Pausar();
            return;
        }

        // Valores donde se guarda la distribucion final del capital.
        double AB_MontoRetencion = 0;
        double AB_MontoContrato = 0;
        double AB_MontoFacultativo = 0;
        string AB_Alertas = "";
        string AB_TextoRetencion = "";
        string AB_TextoContrato = "";
        string AB_TextoFacultativo = "";

        // Procedimiento 2: se CALCULAN los valores del reparto.
        AB_CalcularReparto(
            AB_Capital, AB_LimiteRetencion, AB_PorcentajeRetencion,
            AB_LimiteContrato, AB_PorcentajeContrato,
            ref AB_MontoRetencion, ref AB_MontoContrato, ref AB_MontoFacultativo,
            ref AB_Alertas, ref AB_TextoRetencion, ref AB_TextoContrato,
            ref AB_TextoFacultativo);

        // Procedimiento 3: se MUESTRAN los resultados en pantalla.
        AB_ImprimirResultados(
            AB_NumeroPoliza, AB_NombreRamo, AB_NombreCliente, AB_Capital, AB_Prima,
            AB_MontoRetencion, AB_MontoContrato, AB_MontoFacultativo,
            AB_Alertas, AB_TextoRetencion, AB_TextoContrato, AB_TextoFacultativo);

        // Se obtiene la reaseguradora que cubrira la parte por contrato.
        AB_Reaseguradora AB_ReaseguradoraContrato = AB_ObtenerReaseguradoraContrato();

        // Se valida que exista la reaseguradora del contrato.
        if (AB_ReaseguradoraContrato == null)
        {
            Console.WriteLine("No se encontro una reaseguradora para el contrato 0020.");
            AB_Pausar();
            return;
        }

        // Se guarda el reparto anterior de la poliza, si ya existe uno.
        DataTable AB_TablaDistribucion = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdReparto " +
            "FROM AB_RepartoReaseguro " +
            "WHERE AB_IdPoliza = @IdPoliza",
            new string[] { "@IdPoliza" }, new object[] { AB_IdPoliza });

        int AB_IdReparto = 0;

        // Se valida si la poliza ya tiene un reparto de reaseguro guardado.
        if (AB_TablaDistribucion.Rows.Count > 0)
        {
            // Se toma el identificador del reparto que se va a actualizar.
            AB_IdReparto = Convert.ToInt32(AB_TablaDistribucion.Rows[0]["AB_IdReparto"]);
        }

        // Se crea el objeto que representa el reparto calculado.
        AB_RepartoReaseguro AB_RepartoCalculado = new AB_RepartoReaseguro(
            AB_IdReparto, AB_IdPoliza, AB_ReaseguradoraContrato.AB_IdReaseguradora,
            AB_MontoRetencion, AB_MontoContrato, AB_MontoFacultativo);

        // Variable que guarda la consulta para crear o actualizar el reparto.
        string AB_ConsultaGuardar;

        // Si existe reparto anterior se actualiza, caso contrario se inserta.
        if (AB_TablaDistribucion.Rows.Count > 0)
        {
            AB_ConsultaGuardar =
                "UPDATE AB_RepartoReaseguro SET " +
                "AB_IdReaseguradora = @IdReaseguradora, " +
                "AB_RetencionPropia = @RetencionPropia, " +
                "AB_CapitalContrato = @CapitalContrato, " +
                "AB_CapitalFacultativo = @CapitalFacultativo " +
                "WHERE AB_IdPoliza = @IdPoliza";
        }
        else
        {
            AB_ConsultaGuardar =
                "INSERT INTO AB_RepartoReaseguro " +
                "(AB_IdPoliza, AB_IdReaseguradora, AB_RetencionPropia, AB_CapitalContrato, AB_CapitalFacultativo) " +
                "VALUES " +
                "(@IdPoliza, @IdReaseguradora, @RetencionPropia, @CapitalContrato, @CapitalFacultativo)";
        }

        // Se confirma si el reparto fue guardado correctamente.
        if (AB_GuardarRepartoTransaccion(AB_ConsultaGuardar, AB_RepartoCalculado))
        {
            // Se registra el reparto calculado en la tabla de logs.
            AB_BaseDatos.AB_RegistrarLog("REASEGUROS", "CALCULAR REPARTO", "Reparto de la poliza " + AB_NumeroPoliza + " guardado correctamente.");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nReparto de reaseguro guardado correctamente.");
            Console.ForegroundColor = ConsoleColor.White;
        }
        else
        {
            Console.WriteLine("No se pudo guardar el reparto de reaseguro.");
        }

        AB_Pausar();
    }

    // Metodo para guardar el reparto completo dentro de una transaccion.
    // Uso de COMMIT y ROLLBACK.
    private bool AB_GuardarRepartoTransaccion(string AB_ConsultaGuardar,AB_RepartoReaseguro AB_RepartoCalculado)
    {
        // Se intenta abrir la conexion e iniciar la transaccion del reparto.
        try
        {
            // Se abre la conexion antes de iniciar la transaccion.
            AB_BaseDatos.AB_OpenConnection();
            SqlConnection AB_Conexion = AB_BaseDatos.AB_ObtenerConexion();

            // Se inicia la transaccion para guardar o revertir el reparto.
            using (SqlTransaction AB_Transaccion = AB_Conexion.BeginTransaction())
            {
                // Se intenta insertar o actualizar el reparto dentro de la transaccion.
                try
                {
                    // Comando para insertar o actualizar el reparto calculado.
                    using (SqlCommand AB_Comando = new SqlCommand(AB_ConsultaGuardar, AB_Conexion, AB_Transaccion))
                    {
                        // Se agregan los datos del reparto mediante parametros.
                        AB_Comando.Parameters.AddWithValue("@IdPoliza", AB_RepartoCalculado.AB_IdPoliza);
                        AB_Comando.Parameters.AddWithValue("@IdReaseguradora", AB_RepartoCalculado.AB_IdReaseguradora);
                        AB_Comando.Parameters.AddWithValue("@RetencionPropia", AB_RepartoCalculado.AB_RetencionPropia);
                        AB_Comando.Parameters.AddWithValue("@CapitalContrato", AB_RepartoCalculado.AB_CapitalContrato);
                        AB_Comando.Parameters.AddWithValue("@CapitalFacultativo", AB_RepartoCalculado.AB_CapitalFacultativo);

                        // Se valida que el reparto haya afectado una fila.
                        if (AB_Comando.ExecuteNonQuery() == 0)
                        {
                            throw new Exception("No se pudo guardar el reparto de reaseguro.");
                        }
                    }

                    // Se confirman los datos del reparto en SQL Server.
                    AB_Transaccion.Commit();
                    return true;
                }
                // Si el reparto no se guarda, se revierten los cambios.
                catch (Exception AB_Excepcion)
                {
                    // Se deshacen los cambios si ocurre un error.
                    AB_Transaccion.Rollback();
                    Console.WriteLine($"Error al guardar el reparto: {AB_Excepcion.Message}");
                    return false;
                }
            }
        }
        // Se informa si no fue posible abrir la conexion o iniciar la transaccion.
        catch (Exception AB_Excepcion)
        {
            // Se muestra el error si no se puede iniciar la transaccion.
            Console.WriteLine($"Error de conexion al guardar el reparto: {AB_Excepcion.Message}");
            return false;
        }
        // La conexion se cierra tanto si el reparto se guarda como si ocurre un error.
        finally
        {
            // Se cierra la conexion despues de terminar la transaccion.
            AB_BaseDatos.AB_CloseConnection();
        }
    }

    // Metodo para BUSCAR LIMITES dentro de las reaseguradoras.
    private void AB_BuscarLimites(ref double AB_LimiteRetencion, ref double AB_PorcentajeRetencion,
                                     ref double AB_LimiteContrato, ref double AB_PorcentajeContrato)
    {
        // Se guardan las reaseguradoras para comparar sus porcentajes y limites.
        DataTable AB_TablaReaseguradoras = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdReaseguradora, AB_Codigo, AB_Nombre, AB_Grupo, AB_CodigoGeneral, " +
            "AB_LimitePorcentual, AB_LimiteValorativo, AB_LimiteAnual " +
            "FROM AB_Reaseguradora");

        // Revisar los limites de cada reaseguradora.
        for (int AB_Indice = 0; AB_Indice < AB_TablaReaseguradoras.Rows.Count; AB_Indice++)
        {
            // Se convierte cada fila en una reaseguradora antes de revisar sus limites.
            DataRow AB_Fila = AB_TablaReaseguradoras.Rows[AB_Indice];

            AB_Reaseguradora AB_ReaseguradoraActual = new AB_Reaseguradora(
                Convert.ToInt32(AB_Fila["AB_IdReaseguradora"]),
                AB_Fila["AB_Codigo"].ToString(),
                AB_Fila["AB_Nombre"].ToString(),
                AB_Fila["AB_Grupo"].ToString(),
                AB_Fila["AB_CodigoGeneral"].ToString(),
                Convert.ToDouble(AB_Fila["AB_LimitePorcentual"]),
                Convert.ToDouble(AB_Fila["AB_LimiteValorativo"]),
                Convert.ToDouble(AB_Fila["AB_LimiteAnual"]));

            // Se leen los limites de cada reaseguradora disponible.
            string AB_CodigoGeneral = AB_ReaseguradoraActual.AB_CodigoGeneral;
            double AB_Porcentaje = AB_ReaseguradoraActual.AB_LimitePorcentual;
            double AB_Limite = AB_ReaseguradoraActual.AB_LimiteValorativo;
            double AB_LimiteAnual = AB_ReaseguradoraActual.AB_LimiteAnual;

            // Se respeta el limite anual si es menor al limite valorativo.
            if (AB_LimiteAnual > 0 && AB_Limite > AB_LimiteAnual)
            {
                AB_Limite = AB_LimiteAnual;
            }

            // Se toma el limite mas alto de retencion entre las reaseguradoras tipo 0010.
            if (AB_CodigoGeneral == "0010" && AB_Limite > AB_LimiteRetencion)
            {
                AB_LimiteRetencion = AB_Limite;
                AB_PorcentajeRetencion = AB_Porcentaje;
            }

            // Se toma el limite mas alto de contrato entre las reaseguradoras tipo 0020.
            if (AB_CodigoGeneral == "0020" && AB_Limite > AB_LimiteContrato)
            {
                AB_LimiteContrato = AB_Limite;
                AB_PorcentajeContrato = AB_Porcentaje;
            }
        }
    }

    // Metodo para CALCULAR los valores de retencion, contrato y facultativo.
    private void AB_CalcularReparto(double AB_Capital, double AB_LimiteRetencion, double AB_PorcentajeRetencion,
        double AB_LimiteContrato, double AB_PorcentajeContrato,
        ref double AB_MontoRetencion, ref double AB_MontoContrato,
        ref double AB_MontoFacultativo, ref string AB_Alertas,
        ref string AB_TextoRetencion, ref string AB_TextoContrato,
        ref string AB_TextoFacultativo)
    {
        // Capital que falta distribuir despues de cada cobertura.
        double AB_CapitalRemanente = AB_Capital;

        // Si el capital no supera $50.000, la aseguradora retiene el valor completo.
        // Regla propia.
        if (AB_Capital <= 50000)
        {
            AB_MontoRetencion = AB_Capital;
            AB_TextoRetencion = "100%";
        }
        else
        {
            // Se calcula cuanto puede retener directamente la aseguradora.
            double AB_CalculoRetencion = AB_Capital * (AB_PorcentajeRetencion / 100.0);

            AB_MontoRetencion = AB_CalculoRetencion > AB_LimiteRetencion ?AB_LimiteRetencion : AB_CalculoRetencion;
            AB_TextoRetencion = AB_PorcentajeRetencion.ToString("N2") + "%";

            // Se calcula el excedente cuando el capital supera la retención permitida.
            if (AB_Capital > AB_LimiteRetencion)
            {
                AB_Alertas = AB_Alertas +"ALERTA: Capital supera limite de retencion ($" +AB_LimiteRetencion.ToString("N2") + ").\n";
            }
        }

        AB_CapitalRemanente = AB_CapitalRemanente - AB_MontoRetencion;

        // Se revisa si queda capital pendiente despues de la retencion.
        if (AB_CapitalRemanente > 0)
        {
            AB_MontoContrato = AB_CapitalRemanente > AB_LimiteContrato ? AB_LimiteContrato : AB_CapitalRemanente;
            AB_TextoContrato = AB_PorcentajeContrato.ToString("N2") + "%";

            // Se limita el monto del contrato si el capital pendiente supera su limite.
            if (AB_CapitalRemanente > AB_LimiteContrato)
            {
                AB_Alertas = AB_Alertas + "ALERTA: Capital supera limite de contrato ($" + AB_LimiteContrato.ToString("N2") + ").\n";
            }
        }
        else
        {
            AB_MontoContrato = 0;
            AB_TextoContrato = "0%";
        }

        AB_CapitalRemanente = AB_CapitalRemanente - AB_MontoContrato;
        AB_MontoFacultativo = AB_CapitalRemanente;

        // Se agrega una alerta cuando una parte del capital pasa a cobertura facultativa.
        if (AB_MontoFacultativo > 0)
        {
            AB_TextoFacultativo = "Variable";
            AB_Alertas = AB_Alertas + "ALERTA: Poliza con cobertura facultativa.\n";
        }
        else
        {
            AB_TextoFacultativo = "0%";
        }
    }

    // Metodo para MOSTRAR el resumen del reparto calculado.
    private void AB_ImprimirResultados(string AB_NumeroPoliza, string AB_NombreRamo, string AB_NombreCliente,
        double AB_Capital, double AB_Prima, double AB_MontoRetencion,
        double AB_MontoContrato, double AB_MontoFacultativo, string AB_Alertas,
        string AB_TextoRetencion, string AB_TextoContrato, string AB_TextoFacultativo)
    {
        Console.WriteLine("\n==============================================");
        Console.WriteLine($"Poliza:\t\t{AB_NumeroPoliza}");
        Console.WriteLine($"Ramo:\t\t{AB_NombreRamo}");
        Console.WriteLine($"Cliente:\t{AB_NombreCliente}");
        Console.WriteLine($"Capital:\t${AB_Capital:F2}");
        Console.WriteLine($"Prima:\t${AB_Prima:F2}");
        Console.WriteLine("================================================");

        // Se muestran las alertas solo cuando existe alguna observacion del reparto.
        if (AB_Alertas != "")
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(AB_Alertas);
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.White;
        }

        Console.WriteLine("REPARTO DE REASEGURO");
        Console.WriteLine("Codigo\tNombre \t\tPorcentaje\tMonto Aceptado");
        Console.WriteLine($"0010\tRetencion\t{AB_TextoRetencion}\t\t${AB_MontoRetencion:F2}");

        // Se muestra la cobertura por contrato solo si tiene un monto asignado.
        if (AB_MontoContrato > 0)
        {
            Console.WriteLine($"0020\tContrato\t{AB_TextoContrato}\t\t${AB_MontoContrato:F2}");
        }

        // Se muestra la cobertura facultativa solo si tiene un monto asignado.
        if (AB_MontoFacultativo > 0)
        {
            Console.WriteLine($"0030\tFacultativo\t{AB_TextoFacultativo}\t\t${AB_MontoFacultativo:F2}");
        }

        Console.WriteLine("=======================================================");
    }

    // Metodo para obtener el identificador del contrato de reaseguro.
    private AB_Reaseguradora AB_ObtenerReaseguradoraContrato()
    {
        // Se guarda la primera reaseguradora configurada para el contrato 0020.
        DataTable AB_TablaReaseguradoras = AB_BaseDatos.AB_ExecuteQuery(
            "SELECT AB_IdReaseguradora, AB_Codigo, AB_Nombre, AB_Grupo, AB_CodigoGeneral, " +
            "AB_LimitePorcentual, AB_LimiteValorativo, AB_LimiteAnual " +
            "FROM AB_Reaseguradora " +
            "WHERE AB_CodigoGeneral = '0020'");

        // Si no existe una reaseguradora 0020, no se puede crear el reparto.
        if (AB_TablaReaseguradoras.Rows.Count == 0)
        {
            return null;
        }

        // Se convierte la primera reaseguradora 0020 en la entidad del contrato.
        DataRow AB_Fila = AB_TablaReaseguradoras.Rows[0];
        return new AB_Reaseguradora(
            Convert.ToInt32(AB_Fila["AB_IdReaseguradora"]),
            AB_Fila["AB_Codigo"].ToString(),
            AB_Fila["AB_Nombre"].ToString(),
            AB_Fila["AB_Grupo"].ToString(),
            AB_Fila["AB_CodigoGeneral"].ToString(),
            Convert.ToDouble(AB_Fila["AB_LimitePorcentual"]),
            Convert.ToDouble(AB_Fila["AB_LimiteValorativo"]),
            Convert.ToDouble(AB_Fila["AB_LimiteAnual"]));
    }

    // --------------------------------------------------
    // 2. Metodo para ELIMINAR el reparto guardado de una poliza.
    // --------------------------------------------------
    private void AB_EliminarReaseguro()
    {
        Console.Clear();
        Console.WriteLine("--- ELIMINAR REPARTO DE REASEGURO ---");
        Console.Write("Ingrese el numero de poliza: ");

        // Se recibe el numero de la poliza cuyo reparto se desea eliminar.
        string AB_NumeroPoliza = Console.ReadLine();

        // Se guardan los datos de la poliza cuyo reparto se desea eliminar.
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

        // Se convierte el registro en objeto antes de eliminar su reparto.
        AB_Poliza AB_PolizaSinReparto = AB_CrearPolizaDesdeFila(AB_TablaPolizas.Rows[0]);

        // Se elimina el reparto relacionado con la poliza encontrada.
        int AB_FilasAfectadas = AB_BaseDatos.AB_ExecuteNonQuery(
            "DELETE FROM AB_RepartoReaseguro " +
            "WHERE AB_IdPoliza = @IdPoliza",
            new string[] { "@IdPoliza" }, new object[] { AB_PolizaSinReparto.AB_IdPoliza });

        // Se informa si el reparto fue eliminado o si la poliza no tenia uno.
        if (AB_FilasAfectadas > 0)
        {
            // Se registra la eliminacion del reparto en la tabla de logs.
            AB_BaseDatos.AB_RegistrarLog("REASEGUROS", "ELIMINAR REPARTO", "Reparto de la poliza " + AB_NumeroPoliza + " eliminado correctamente.");
            Console.WriteLine("Reparto de reaseguro eliminado.");
        }
        else
        {
            Console.WriteLine("No existe reparto de reaseguro para esta poliza.");
        }

        AB_Pausar();
    }

    // Metodo que convierte una fila completa de AB_Poliza en un objeto.
    private AB_Poliza AB_CrearPolizaDesdeFila(DataRow AB_Fila)
    {
        // Se controlan las columnas financieras que la tabla permite guardar como NULL.
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

