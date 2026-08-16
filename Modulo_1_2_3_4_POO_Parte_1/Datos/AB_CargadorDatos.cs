using System;
using System.Data;
using System.IO;

namespace SistemaSeguros;

// Clase que carga los datos iniciales desde archivos de texto (.txt).
public class AB_CargadorDatos
{
    // Variable para usar la conexion de SQL Server.
    private AB_Conexion AB_BaseDatos = AB_Conexion.AB_GetInstance();

    // Metodo para obtener cada archivo de la carpeta Datos del programa.
    private string AB_ObtenerRutaDatos(string AB_NombreArchivo)
    {
        return Path.Combine(AppContext.BaseDirectory, "Datos", AB_NombreArchivo);
    }

    // Metodo principal que CARGA clientes, ramos, reaseguradoras, cuentas y alertas.
    public void AB_QuemarDatosIniciales()
    {
        // Encabezado 
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("==================================================");
        Console.WriteLine("      CARGA DE ARCHIVOS TXT A AB_SEGUROSDB2       ");
        Console.WriteLine("==================================================");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;

        // Se llama a cada proceso para cargar los archivos TXT.
        AB_CargarClientes();
        AB_CargarRamos();
        AB_CargarReaseguradoras();
        AB_CargarCatalogoContable();
        AB_CargarAlertasUAF();

        // Mensaje de confirmacion.
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("==================================================");
        Console.WriteLine("            Carga inicial terminada               ");
        Console.WriteLine("==================================================\n");
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.White;
    }

    // Metodo para cargar los CLIENTES que no existan en la base.
    private void AB_CargarClientes()
    {
        // Ruta del archivo que contiene los clientes.
        string AB_Ruta = AB_ObtenerRutaDatos("AB_clientes.txt");

        // Se comprueba si el archivo de clientes existe antes de leerlo.
        if (!File.Exists(AB_Ruta))
        {
            Console.WriteLine($"No se encontro el archivo {AB_Ruta}.");
            return;
        }

        // Contador de clientes nuevos guardados.
        int AB_Cargado = 0;

        // Se intenta leer el archivo completo de clientes.
        try
        {
            // Bloque using para cerrar el archivo al terminar.
            using (StreamReader AB_Lector = new StreamReader(AB_Ruta))
            {
                // Variable donde se guarda cada linea leida del archivo.
                string AB_Linea;

                // Ciclo que recorre cada linea del archivo.
                while ((AB_Linea = AB_Lector.ReadLine()) != null)
                {
                    // Se separan los datos de la linea para poder usarlos.
                    string[] AB_Datos = AB_Linea.Split('|');

                    // Se descartan las lineas de clientes que no tengan seis datos.
                    if (AB_Datos.Length != 6)
                    {
                        continue;
                    }

                    AB_Cliente AB_ClienteArchivo;

                    // Se intenta crear el cliente con los datos leidos del archivo.
                    try
                    {
                        AB_ClienteArchivo = new AB_Cliente(0, AB_Datos[0],AB_Datos[1], AB_Datos[2], AB_Datos[3],
                                            AB_Datos[4], AB_Datos[5]);
                    }
                    // Si los datos del cliente son invalidos, se omite esa linea.
                    catch (ArgumentException AB_Excepcion)
                    {
                        Console.WriteLine($"Cliente omitido: {AB_Excepcion.Message}");
                        continue;
                    }

                    // Consulta que busca al cliente por su cedula.
                    string AB_ConsultaBuscar = "SELECT AB_IdCliente " +
                        "FROM AB_Cliente " +
                        "WHERE AB_Cedula = @Cedula";

                    // Se guarda el cliente encontrado para evitar registros duplicados.
                    DataTable AB_Tabla = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaBuscar,
                        new string[] { "@Cedula" },
                        new object[] { AB_ClienteArchivo.AB_Cedula });

                    // Si el cliente no existe, se prepara la consulta INSERT.
                    if (AB_Tabla.Rows.Count == 0)
                    {
                        // Consulta que guarda los datos del cliente nuevo.
                        string AB_ConsultaInsertar =
                            "INSERT INTO AB_Cliente " +
                            "(AB_Cedula, AB_Nombres, AB_Apellidos, AB_Direccion, AB_Telefono, AB_Correo) " +
                            "VALUES " +
                            "(@Cedula, @Nombres, @Apellidos, @Direccion, @Telefono, @Correo)";

                        // Si el cliente se guarda, aumenta el contador de clientes nuevos.
                        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaInsertar,
                            new string[] { "@Cedula", "@Nombres", "@Apellidos", "@Direccion", "@Telefono", "@Correo" },
                            new object[] { AB_ClienteArchivo.AB_Cedula, AB_ClienteArchivo.AB_Nombres,
                                AB_ClienteArchivo.AB_Apellidos, AB_ClienteArchivo.AB_Direccion,
                                AB_ClienteArchivo.AB_Telefono, AB_ClienteArchivo.AB_Correo }) > 0)
                        {
                            AB_Cargado++;
                        }
                    }
                }
            }

            Console.WriteLine($"Clientes nuevos cargados: {AB_Cargado}.");
        }
        // Se informa si el archivo de clientes no puede leerse.
        catch (IOException)
        {
            Console.WriteLine($"ERROR. Cierre el archivo {AB_Ruta} antes de usarlo aqui.");
        }
    }

    // Metodo para cargar los RAMOS.
    private void AB_CargarRamos()
    {
        // Ruta del archivo que contiene los ramos.
        string AB_Ruta = AB_ObtenerRutaDatos("AB_ramos.txt");

        // Se comprueba si el archivo de ramos existe antes de leerlo.
        if (!File.Exists(AB_Ruta))
        {
            Console.WriteLine($"No se encontro el archivo {AB_Ruta}.");
            return;
        }

        // Contador de ramos nuevos guardados.
        int AB_Cargado = 0;

        // Se intenta leer el archivo completo de ramos.
        try
        {
            using (StreamReader AB_Lector = new StreamReader(AB_Ruta))
            {
                // Variable donde se guarda cada linea leida del archivo.
                string AB_Linea;

                // Ciclo que recorre cada ramo del archivo.
                while ((AB_Linea = AB_Lector.ReadLine()) != null)
                {
                    // Se separan los datos de la linea para poder usarlos.
                    string[] AB_Datos = AB_Linea.Split('|');

                    // Se descartan las lineas de ramos que no tengan dos datos.
                    if (AB_Datos.Length != 2)
                    {
                        continue;
                    }

                    AB_Ramo AB_RamoArchivo;

                    // Se intenta crear el ramo con los datos leidos del archivo.
                    try
                    {
                        AB_RamoArchivo = new AB_Ramo(0, AB_Datos[0], AB_Datos[1]);
                    }
                    // Si los datos del ramo son invalidos, se omite esa linea.
                    catch (ArgumentException AB_Excepcion)
                    {
                        Console.WriteLine($"Ramo omitido: {AB_Excepcion.Message}");
                        continue;
                    }

                    // Consulta para comprobar si el ramo ya esta registrado.
                    string AB_ConsultaBuscar = "SELECT AB_IdRamo " +
                        "FROM AB_Ramo " +
                        "WHERE AB_CodigoRamo = @CodigoRamo";

                    // Se guarda el ramo encontrado para evitar registros duplicados.
                    DataTable AB_Tabla = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaBuscar,
                        new string[] { "@CodigoRamo" },
                        new object[] { AB_RamoArchivo.AB_CodigoRamo });

                    // Si el ramo no existe, se inserta en la base.
                    if (AB_Tabla.Rows.Count == 0)
                    {
                        // Consulta que guarda el ramo nuevo.
                        string AB_ConsultaInsertar =
                            "INSERT INTO AB_Ramo (AB_CodigoRamo, AB_NombreRamo) " +
                            "VALUES " +
                            "(@CodigoRamo, @NombreRamo)";

                        // Se comprueba que el ramo se haya guardado correctamente.
                        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaInsertar,
                            new string[] { "@CodigoRamo", "@NombreRamo" },
                            new object[] { AB_RamoArchivo.AB_CodigoRamo, AB_RamoArchivo.AB_NombreRamo }) > 0)
                        {
                            AB_Cargado++;
                        }
                    }
                }
            }

            Console.WriteLine($"Ramos nuevos cargados: {AB_Cargado}.");
        }
        // Se informa si el archivo de ramos no puede leerse.
        catch (IOException)
        {
            Console.WriteLine($"ERROR. Cierre el archivo {AB_Ruta} antes de usarlo aqui.");
        }
    }

    // Metodo para cargar las REASEGURADORAS.
    private void AB_CargarReaseguradoras()
    {
        // Ruta del archivo que contiene las reaseguradoras.
        string AB_Ruta = AB_ObtenerRutaDatos("AB_reaseguradoras.txt");

        // Se comprueba si el archivo de reaseguradoras existe antes de leerlo.
        if (!File.Exists(AB_Ruta))
        {
            Console.WriteLine($"No se encontro el archivo {AB_Ruta}.");
            return;
        }

        // Contador de reaseguradoras nuevas guardadas.
        int AB_Cargado = 0;

        // Se intenta leer el archivo completo de reaseguradoras.
        try
        {
            using (StreamReader AB_Lector = new StreamReader(AB_Ruta))
            {
                // Variable donde se guarda cada linea leida del archivo.
                string AB_Linea;

                // Ciclo que recorre cada reaseguradora del archivo.
                while ((AB_Linea = AB_Lector.ReadLine()) != null)
                {
                    // Se separan los datos de la linea para poder usarlos.
                    string[] AB_Datos = AB_Linea.Split('|');

                    // Se descartan las lineas de reaseguradoras que no tengan siete datos.
                    if (AB_Datos.Length != 7)
                    {
                        continue;
                    }

                    double AB_LimitePorcentual;
                    double AB_LimiteValorativo;
                    double AB_LimiteAnual;

                    // Se omite la linea si sus valores numericos no son validos.
                    if (!double.TryParse(AB_Datos[4], out AB_LimitePorcentual) ||
                        !double.TryParse(AB_Datos[5], out AB_LimiteValorativo) ||
                        !double.TryParse(AB_Datos[6], out AB_LimiteAnual))
                    {
                        Console.WriteLine($"Reaseguradora {AB_Datos[0]} omitida: valores incorrectos.");
                        continue;
                    }

                    AB_Reaseguradora AB_ReaseguradoraArchivo;

                    // Se intenta crear la reaseguradora con los datos leidos del archivo.
                    try
                    {
                        AB_ReaseguradoraArchivo = new AB_Reaseguradora(0,
                            AB_Datos[0], AB_Datos[1], AB_Datos[2], AB_Datos[3],
                            AB_LimitePorcentual, AB_LimiteValorativo, AB_LimiteAnual);
                    }
                    // Si los datos de la reaseguradora son invalidos, se omite esa linea.
                    catch (ArgumentException AB_Excepcion)
                    {
                        Console.WriteLine($"Reaseguradora omitida: {AB_Excepcion.Message}");
                        continue;
                    }

                    // Consulta para comprobar si la reaseguradora ya esta registrada.
                    string AB_ConsultaBuscar = "SELECT AB_IdReaseguradora " +
                        "FROM AB_Reaseguradora " +
                        "WHERE AB_Codigo = @Codigo";

                    // Se guarda la reaseguradora encontrada para evitar registros duplicados.
                    DataTable AB_Tabla = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaBuscar,
                        new string[] { "@Codigo" },
                        new object[] { AB_ReaseguradoraArchivo.AB_Codigo });

                    // Si no existe, se prepara el INSERT de la reaseguradora.
                    if (AB_Tabla.Rows.Count == 0)
                    {
                        // Consulta que guarda la reaseguradora nueva.
                        string AB_ConsultaInsertar =
                            "INSERT INTO AB_Reaseguradora " +
                            "(AB_Codigo, AB_Nombre, AB_Grupo, AB_CodigoGeneral, AB_LimitePorcentual, AB_LimiteValorativo, AB_LimiteAnual) " +
                            "VALUES " +
                            "(@Codigo, @Nombre, @Grupo, @CodigoGeneral, @LimitePorcentual, @LimiteValorativo, @LimiteAnual)";

                        // Si la reaseguradora se guarda, se aumenta el contador.
                        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaInsertar,
                            new string[] { "@Codigo", "@Nombre", "@Grupo", "@CodigoGeneral",
                                "@LimitePorcentual", "@LimiteValorativo", "@LimiteAnual" },
                            new object[] { AB_ReaseguradoraArchivo.AB_Codigo, AB_ReaseguradoraArchivo.AB_Nombre,
                                AB_ReaseguradoraArchivo.AB_Grupo, AB_ReaseguradoraArchivo.AB_CodigoGeneral,
                                AB_ReaseguradoraArchivo.AB_LimitePorcentual, AB_ReaseguradoraArchivo.AB_LimiteValorativo,
                                AB_ReaseguradoraArchivo.AB_LimiteAnual }) > 0)
                        {
                            AB_Cargado++;
                        }
                    }
                }
            }

            Console.WriteLine($"Reaseguradoras nuevas cargadas: {AB_Cargado}.");
        }
        // Se informa si el archivo de reaseguradoras no puede leerse.
        catch (IOException)
        {
            Console.WriteLine($"ERROR. Cierre el archivo {AB_Ruta} antes de usarlo aqui.");
        }
    }

    // Metodo para cargar las CUENTAS del catalogo contable.
    private void AB_CargarCatalogoContable()
    {
        // Ruta del archivo que contiene las cuentas contables.
        string AB_Ruta = AB_ObtenerRutaDatos("AB_catalogo_contable.txt");

        // Se comprueba si el archivo del catalogo contable existe antes de leerlo.
        if (!File.Exists(AB_Ruta))
        {
            Console.WriteLine($"No se encontro el archivo {AB_Ruta}.");
            return;
        }

        // Contador de cuentas contables nuevas guardadas.
        int AB_Cargado = 0;

        // Se intenta leer el archivo completo del catalogo contable.
        try
        {
            using (StreamReader AB_Lector = new StreamReader(AB_Ruta))
            {
                // Variable donde se guarda cada linea leida del archivo.
                string AB_Linea;

                // Ciclo que recorre cada cuenta del archivo.
                while ((AB_Linea = AB_Lector.ReadLine()) != null)
                {
                    // Se separan los datos de la linea para poder usarlos.
                    string[] AB_Datos = AB_Linea.Split('|');

                    // Se descartan las lineas de cuentas que no tengan cuatro datos.
                    if (AB_Datos.Length != 4)
                    {
                        continue;
                    }

                    AB_CuentaContable AB_CuentaArchivo;

                    // Se intenta crear la cuenta contable con los datos leidos del archivo.
                    try
                    {
                        AB_CuentaArchivo = new AB_CuentaContable(0,
                            AB_Datos[0], AB_Datos[1], AB_Datos[2], AB_Datos[3]);
                    }
                    // Si los datos de la cuenta son invalidos, se omite esa linea.
                    catch (ArgumentException AB_Excepcion)
                    {
                        Console.WriteLine($"Cuenta omitida: {AB_Excepcion.Message}");
                        continue;
                    }

                    // Consulta para comprobar si la cuenta ya esta registrada.
                    string AB_ConsultaBuscar = "SELECT AB_IdCuenta " +
                        "FROM AB_CuentaContable " +
                        "WHERE AB_CodigoCuenta = @CodigoCuenta";

                    // Se guarda la cuenta encontrada para evitar registros duplicados.
                    DataTable AB_Tabla = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaBuscar,
                        new string[] { "@CodigoCuenta" },
                        new object[] { AB_CuentaArchivo.AB_CodigoCuenta });

                    // Si la cuenta no existe, se inserta en la base.
                    if (AB_Tabla.Rows.Count == 0)
                    {
                        // Consulta que guarda la cuenta contable nueva.
                        string AB_ConsultaInsertar =
                            "INSERT INTO AB_CuentaContable " +
                            "(AB_CodigoCuenta, AB_NombreCuenta, AB_Naturaleza, AB_TipoCuenta) " +
                            "VALUES " +
                            "(@CodigoCuenta, @NombreCuenta, @Naturaleza, @TipoCuenta)";

                        // Si la cuenta se guarda, se aumenta el contador.
                        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaInsertar,
                            new string[] { "@CodigoCuenta", "@NombreCuenta", "@Naturaleza", "@TipoCuenta" },
                            new object[] { AB_CuentaArchivo.AB_CodigoCuenta, AB_CuentaArchivo.AB_NombreCuenta,
                                AB_CuentaArchivo.AB_Naturaleza, AB_CuentaArchivo.AB_TipoCuenta }) > 0)
                        {
                            AB_Cargado++;
                        }
                    }
                }
            }

            Console.WriteLine($"Cuentas contables nuevas cargadas: {AB_Cargado}.");
        }
        // Se informa si el archivo del catalogo contable no puede leerse.
        catch (IOException)
        {
            Console.WriteLine($"ERROR. Cierre el archivo {AB_Ruta} antes de usarlo aqui.");
        }
    }

    // Metodo para cargar las ALERTAS UAF.
    private void AB_CargarAlertasUAF()
    {
        // Ruta del archivo que contiene las alertas UAF.
        string AB_Ruta = AB_ObtenerRutaDatos("AB_alertas_uaf.txt");

        // Se comprueba si el archivo de alertas existe antes de leerlo.
        if (!File.Exists(AB_Ruta))
        {
            Console.WriteLine($"No se encontro el archivo {AB_Ruta}.");
            return;
        }

        // Contador de alertas nuevas guardadas.
        int AB_Cargado = 0;

        // Se intenta leer el archivo completo de alertas UAF.
        try
        {
            using (StreamReader AB_Lector = new StreamReader(AB_Ruta))
            {
                // Variable donde se guarda cada linea leida del archivo.
                string AB_Linea;

                // Ciclo que recorre cada cliente con sus alertas.
                while ((AB_Linea = AB_Lector.ReadLine()) != null)
                {
                    // Se separan los datos de la linea para poder usarlos.
                    string[] AB_Datos = AB_Linea.Split('|');

                    // Se descartan las lineas de alertas que no tengan dos datos.
                    if (AB_Datos.Length != 2)
                    {
                        continue;
                    }

                    // Se busca al cliente para relacionar la alerta con el.
                    string AB_ConsultaCliente = "SELECT AB_IdCliente, AB_Cedula, AB_Nombres, AB_Apellidos, " +
                        "AB_Direccion, AB_Telefono, AB_Correo " +
                        "FROM AB_Cliente " +
                        "WHERE AB_Cedula = @Cedula";
                    // Se guardan los datos del cliente encontrados en la consulta.
                    DataTable AB_TablaClientes = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaCliente,
                        new string[] { "@Cedula" },
                        new object[] { AB_Datos[0] });

                    // Si el cliente no existe, se pasa a la siguiente linea.
                    if (AB_TablaClientes.Rows.Count == 0)
                    {
                        continue;
                    }

                    // Se toma la fila del cliente para crear el objeto usado por sus alertas.
                    DataRow AB_FilaCliente = AB_TablaClientes.Rows[0];
                    AB_Cliente AB_ClienteAlerta;

                    // Se intenta crear el cliente relacionado con las alertas del archivo.
                    try
                    {
                        AB_ClienteAlerta = new AB_Cliente(
                            Convert.ToInt32(AB_FilaCliente["AB_IdCliente"]),
                            AB_FilaCliente["AB_Cedula"].ToString(),
                            AB_FilaCliente["AB_Nombres"].ToString(),
                            AB_FilaCliente["AB_Apellidos"].ToString(),
                            AB_FilaCliente["AB_Direccion"].ToString(),
                            AB_FilaCliente["AB_Telefono"].ToString(),
                            AB_FilaCliente["AB_Correo"].ToString());
                    }
                    // Si los datos del cliente son invalidos, se omiten sus alertas.
                    catch (ArgumentException AB_Excepcion)
                    {
                        Console.WriteLine($"Alertas del cliente omitidas: {AB_Excepcion.Message}");
                        continue;
                    }

                    string[] AB_CodigosAlerta = AB_Datos[1].Split(',');

                    // Ciclo para guardar cada codigo de alerta del cliente.
                    for (int AB_Indice = 0; AB_Indice < AB_CodigosAlerta.Length; AB_Indice++)
                    {
                        // Se define el nivel de riesgo inicial de la alerta.
                        string AB_NivelRiesgo = "NORMAL";

                        // El codigo 999 se considera de riesgo critico.
                        if (AB_CodigosAlerta[AB_Indice] == "999")
                        {
                            AB_NivelRiesgo = "CRITICO";
                        }
                        // El codigo 500 se considera de riesgo alto.
                        else if (AB_CodigosAlerta[AB_Indice] == "500")
                        {
                            AB_NivelRiesgo = "ALTO";
                        }
                        // El codigo 404 se considera de riesgo medio.
                        else if (AB_CodigosAlerta[AB_Indice] == "404")
                        {
                            AB_NivelRiesgo = "MEDIO";
                        }

                        AB_AlertaUAF AB_AlertaArchivo;

                        // Se intenta crear la alerta con el codigo y el riesgo asignado.
                        try
                        {
                            AB_AlertaArchivo = new AB_AlertaUAF(0, AB_ClienteAlerta.AB_IdCliente,
                                AB_CodigosAlerta[AB_Indice], AB_NivelRiesgo, DateTime.Now);
                        }
                        // Si el codigo o el riesgo son invalidos, se omite esa alerta.
                        catch (ArgumentException AB_Excepcion)
                        {
                            Console.WriteLine($"Alerta omitida: {AB_Excepcion.Message}");
                            continue;
                        }

                        // Se busca la alerta del cliente antes de intentar guardarla.
                        string AB_ConsultaAlerta =
                            "SELECT AB_IdAlerta " +
                            "FROM AB_AlertaUAF " +
                            "WHERE AB_IdCliente = @IdCliente " +
                            "AND AB_CodigoAlerta = @CodigoAlerta";
                        // Se guarda la alerta encontrada para evitar registros duplicados.
                        DataTable AB_TablaAlertas = AB_BaseDatos.AB_ExecuteQuery(AB_ConsultaAlerta,
                            new string[] { "@IdCliente", "@CodigoAlerta" },
                            new object[] { AB_AlertaArchivo.AB_IdCliente, AB_AlertaArchivo.AB_CodigoAlerta });

                        // Si la alerta ya existe, se continua con el siguiente codigo.
                        if (AB_TablaAlertas.Rows.Count > 0)
                        {
                            continue;
                        }

                        // Consulta que guarda una nueva alerta UAF del cliente.
                        string AB_ConsultaInsertar =
                            "INSERT INTO AB_AlertaUAF " +
                            "(AB_IdCliente, AB_CodigoAlerta, AB_NivelRiesgo, AB_FechaReporte) " +
                            "VALUES " +
                            "(@IdCliente, @CodigoAlerta, @NivelRiesgo, @FechaReporte)";

                        // Se comprueba que la alerta se haya guardado correctamente.
                        if (AB_BaseDatos.AB_ExecuteNonQuery(AB_ConsultaInsertar,
                            new string[] { "@IdCliente", "@CodigoAlerta", "@NivelRiesgo", "@FechaReporte" },
                            new object[] { AB_AlertaArchivo.AB_IdCliente, AB_AlertaArchivo.AB_CodigoAlerta,
                                AB_AlertaArchivo.AB_NivelRiesgo, AB_AlertaArchivo.AB_FechaReporte }) > 0)
                        {
                            AB_Cargado++;
                        }
                    }
                }
            }

            Console.WriteLine($"Alertas UAF nuevas cargadas: {AB_Cargado}.");
        }
        // Se informa si el archivo de alertas UAF no puede leerse.
        catch (IOException)
        {
            Console.WriteLine($"ERROR. Cierre el archivo {AB_Ruta} antes de usarlo aqui.");
        }
    }
}

