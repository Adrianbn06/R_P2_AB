using System;

namespace SistemaSeguros;

// Clase para representar una alerta UAF asociada con un cliente.
public class AB_AlertaUAF
{
    // Variables privadas que guardan los datos de la alerta.
    private int AB_CampoIdAlerta;
    private int AB_CampoIdCliente;
    private string AB_CampoCodigoAlerta;
    private string AB_CampoNivelRiesgo;
    private DateTime AB_CampoFechaReporte;

    // Propiedad para identificar la alerta.
    public int AB_IdAlerta
    {
        get 
        { 
            return AB_CampoIdAlerta;
        }
        set
        { 
            AB_CampoIdAlerta = value; 
        }
    }

    // Propiedad que relaciona la alerta con el cliente.
    public int AB_IdCliente
    {
        get
        { 
            return AB_CampoIdCliente;
        }
        set
        {
            AB_CampoIdCliente = value; 
        }
    }

    // Propiedad para guardar el codigo de la alerta.
    public string AB_CodigoAlerta
    {
        get 
        {
            return AB_CampoCodigoAlerta; 
        }
        set
        {
            // Se valida que el codigo de la alerta no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El codigo de la alerta es obligatorio.");
            AB_CampoCodigoAlerta = value;
        }
    }

    // Propiedad para guardar el nivel de riesgo.
    public string AB_NivelRiesgo
    {
        get 
        {
            return AB_CampoNivelRiesgo;
        }
        set
        {
            // Se valida que el nivel de riesgo no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nivel de riesgo es obligatorio.");
            AB_CampoNivelRiesgo = value;
        }
    }

    // Propiedad para guardar la fecha del reporte.
    public DateTime AB_FechaReporte
    {
        get
        { 
            return AB_CampoFechaReporte; 
        }
        set
        {
            AB_CampoFechaReporte = value;
        }
    }

    // Constructor que recibe y asigna los datos de la alerta.
    public AB_AlertaUAF(int AB_IdAlertaIngresado, int AB_IdClienteIngresado,string AB_CodigoIngresado, string AB_NivelIngresado,
        DateTime AB_FechaIngresada)
    {
        AB_IdAlerta = AB_IdAlertaIngresado;
        AB_IdCliente = AB_IdClienteIngresado;
        AB_CodigoAlerta = AB_CodigoIngresado;
        AB_NivelRiesgo = AB_NivelIngresado;
        AB_FechaReporte = AB_FechaIngresada;
    }
}
