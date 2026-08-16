using System;

namespace SistemaSeguros;

// Clase para representar una cuenta del catalogo contable.
public class AB_CuentaContable
{
    // Variables privadas que guardan los datos de la cuenta.
    private int AB_CampoIdCuenta;
    private string AB_CampoCodigoCuenta;
    private string AB_CampoNombreCuenta;
    private string AB_CampoNaturaleza;
    private string AB_CampoTipoCuenta;

    // Propiedad para identificar la cuenta.
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

    // Propiedad para guardar el codigo de la cuenta.
    public string AB_CodigoCuenta
    {
        get
        { 
            return AB_CampoCodigoCuenta;
        }
        set
        {
            // Se valida que el codigo de la cuenta no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El codigo de la cuenta es obligatorio.");
            AB_CampoCodigoCuenta = value;
        }
    }

    // Propiedad para guardar el nombre de la cuenta.
    public string AB_NombreCuenta
    {
        get
        { 
            return AB_CampoNombreCuenta;
        }
        set
        {
            // Se valida que el nombre de la cuenta no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre de la cuenta es obligatorio.");
            AB_CampoNombreCuenta = value;
        }
    }

    // Propiedad para guardar la naturaleza de la cuenta.
    public string AB_Naturaleza
    {
        get
        {
            return AB_CampoNaturaleza;
        }
        set
        {
            // Se valida que la naturaleza de la cuenta no este vacia.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("La naturaleza de la cuenta es obligatoria.");
            AB_CampoNaturaleza = value;
        }
    }

    // Propiedad para guardar el tipo de cuenta.
    public string AB_TipoCuenta
    {
        get
        { 
            return AB_CampoTipoCuenta; 
        }
        set
        {
            // Se valida que el tipo de cuenta no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El tipo de cuenta es obligatorio.");
            AB_CampoTipoCuenta = value;
        }
    }

    // Constructor que recibe y asigna los datos de la cuenta.
    public AB_CuentaContable(int AB_IdIngresado, string AB_CodigoIngresado,string AB_NombreIngresado, string AB_NaturalezaIngresada,
        string AB_TipoIngresado)
    {
        AB_IdCuenta = AB_IdIngresado;
        AB_CodigoCuenta = AB_CodigoIngresado;
        AB_NombreCuenta = AB_NombreIngresado;
        AB_Naturaleza = AB_NaturalezaIngresada;
        AB_TipoCuenta = AB_TipoIngresado;
    }
}
