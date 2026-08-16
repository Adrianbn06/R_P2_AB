using System;

namespace SistemaSeguros;

// Clase para representar una linea del detalle de un asiento contable.
public class AB_DetalleAsiento
{
    // Variables privadas que guardan los datos del detalle.
    private int AB_CampoIdDetalle;
    private int AB_CampoIdAsiento;
    private int AB_CampoIdCuenta;
    private double AB_CampoValorDebe;
    private double AB_CampoValorHaber;

    // Propiedad para identificar el detalle.
    public int AB_IdDetalle
    {
        get 
        {
            return AB_CampoIdDetalle;
        }
        set 
        {
            AB_CampoIdDetalle = value;
        }
    }

    // Propiedad que relaciona el detalle con el asiento.
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

    // Propiedad que relaciona el detalle con la cuenta.
    public int AB_IdCuenta
    {
        get
        { 
            return AB_CampoIdCuenta;
        }
        set 
        {
            AB_CampoIdCuenta = value;
        }
    }

    // Propiedad para guardar el valor del debe.
    public double AB_ValorDebe
    {
        get 
        { 
            return AB_CampoValorDebe; 
        }
        set
        {
            // Se valida que el valor del debe no sea negativo.
            if (value < 0)
                throw new ArgumentException("El valor debe no puede ser negativo.");
            AB_CampoValorDebe = value;
        }
    }

    // Propiedad para guardar el valor del haber.
    public double AB_ValorHaber
    {
        get
        { 
            return AB_CampoValorHaber;
        }
        set
        {
            // Se valida que el valor del haber no sea negativo.
            if (value < 0)
                throw new ArgumentException("El valor haber no puede ser negativo.");
            AB_CampoValorHaber = value;
        }
    }

    // Constructor que recibe y asigna los datos del detalle.
    public AB_DetalleAsiento(int AB_IdDetalleIngresado, int AB_IdAsientoIngresado,int AB_IdCuentaIngresado, double AB_DebeIngresado,
        double AB_HaberIngresado)
    {
        AB_IdDetalle = AB_IdDetalleIngresado;
        AB_IdAsiento = AB_IdAsientoIngresado;
        AB_IdCuenta = AB_IdCuentaIngresado;
        AB_ValorDebe = AB_DebeIngresado;
        AB_ValorHaber = AB_HaberIngresado;
    }
}
