using System;

namespace SistemaSeguros;

// Clase para representar la cabecera de un asiento contable.
public class AB_AsientoContable
{
    // Variables privadas que guardan los datos del asiento.
    private int AB_CampoIdAsiento;
    private int AB_CampoIdPoliza;
    private int AB_CampoIdSiniestro;
    private string AB_CampoNumeroComprobante;
    private DateTime AB_CampoFechaTransaccion;
    private string AB_CampoModuloOrigen;
    private double AB_CampoTotalDebe;
    private double AB_CampoTotalHaber;

    // Propiedad para identificar el asiento.
    public int AB_IdAsiento
    {
        get
        {
            return AB_CampoIdAsiento; 
        }
        set
        {
            AB_CampoIdAsiento = value;
        }
    }

    // Propiedad que relaciona el asiento con la poliza.
    public int AB_IdPoliza
    {
        get 
        { 
            return AB_CampoIdPoliza;
        }
        set
        { 
            AB_CampoIdPoliza = value;
        }
    }

    // Propiedad que relaciona el asiento con el siniestro.
    public int AB_IdSiniestro
    {
        get
        { 
            return AB_CampoIdSiniestro; 
        }
        set 
        {
            AB_CampoIdSiniestro = value; 
        }
    }

    // Propiedad para guardar el numero de comprobante.
    public string AB_NumeroComprobante
    {
        get
        {
            return AB_CampoNumeroComprobante; 
        }
        set
        {
            // Se valida que el numero de comprobante no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El numero de comprobante es obligatorio.");
            AB_CampoNumeroComprobante = value;
        }
    }

    // Propiedad para guardar la fecha de la transaccion.
    public DateTime AB_FechaTransaccion
    {
        get
        {
            return AB_CampoFechaTransaccion; 
        }
        set
        { 
            AB_CampoFechaTransaccion = value; 
        }
    }

    // Propiedad para guardar el modulo que origino el asiento.
    public string AB_ModuloOrigen
    {
        get
        { 
            return AB_CampoModuloOrigen; 
        }
        set
        {
            // Se valida que el modulo de origen no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El modulo de origen es obligatorio.");
            AB_CampoModuloOrigen = value;
        }
    }

    // Propiedad para guardar el total del debe.
    public double AB_TotalDebe
    {
        get
        { 
            return AB_CampoTotalDebe; 
        }
        set
        {
            // Se valida que el total del debe no sea negativo.
            if (value < 0)
                throw new ArgumentException("El total debe no puede ser negativo.");
            AB_CampoTotalDebe = value;
        }
    }

    // Propiedad para guardar el total del haber.
    public double AB_TotalHaber
    {
        get 
        { 
            return AB_CampoTotalHaber; 
        }
        set
        {
            // Se valida que el total del haber no sea negativo.
            if (value < 0)
                throw new ArgumentException("El total haber no puede ser negativo.");
            AB_CampoTotalHaber = value;
        }
    }

    // Constructor que recibe y asigna los datos del asiento.
    public AB_AsientoContable(int AB_IdAsientoIngresado, int AB_IdPolizaIngresado,int AB_IdSiniestroIngresado, string AB_ComprobanteIngresado,
        DateTime AB_FechaIngresada, string AB_ModuloIngresado,
        double AB_DebeIngresado, double AB_HaberIngresado)
    {
        AB_IdAsiento = AB_IdAsientoIngresado;
        AB_IdPoliza = AB_IdPolizaIngresado;
        AB_IdSiniestro = AB_IdSiniestroIngresado;
        AB_NumeroComprobante = AB_ComprobanteIngresado;
        AB_FechaTransaccion = AB_FechaIngresada;
        AB_ModuloOrigen = AB_ModuloIngresado;
        AB_TotalDebe = AB_DebeIngresado;
        AB_TotalHaber = AB_HaberIngresado;
    }

    // Metodo que confirma el principio de partida doble.
    public bool AB_EstaBalanceado()
    {
        return AB_TotalDebe == AB_TotalHaber;
    }
}
