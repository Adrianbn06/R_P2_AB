using System;

namespace SistemaSeguros;

// Clase para representar una reaseguradora y sus limites.
public class AB_Reaseguradora
{
    // Variables privadas que guardan los datos de la reaseguradora.
    private int AB_CampoIdReaseguradora;
    private string AB_CampoCodigo;
    private string AB_CampoNombre;
    private string AB_CampoGrupo;
    private string AB_CampoCodigoGeneral;
    private double AB_CampoLimitePorcentual;
    private double AB_CampoLimiteValorativo;
    private double AB_CampoLimiteAnual;

    // Propiedad para identificar la reaseguradora.
    public int AB_IdReaseguradora
    {
        get 
        {
            return AB_CampoIdReaseguradora; 
        }
        set 
        { 
            AB_CampoIdReaseguradora = value;
        }
    }

    // Propiedad para guardar el codigo de la reaseguradora.
    public string AB_Codigo
    {
        get 
        {
            return AB_CampoCodigo;
        }
        set
        {
            // Se valida que el codigo de la reaseguradora no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El codigo de la reaseguradora es obligatorio.");
            AB_CampoCodigo = value;
        }
    }

    // Propiedad para guardar el nombre de la reaseguradora.
    public string AB_Nombre
    {
        get 
        { 
            return AB_CampoNombre;
        }
        set
        {
            // Se valida que el nombre de la reaseguradora no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre de la reaseguradora es obligatorio.");
            AB_CampoNombre = value;
        }
    }

    // Propiedad para guardar el grupo de la reaseguradora.
    public string AB_Grupo
    {
        get 
        { 
            return AB_CampoGrupo;
        }
        set 
        { 
            AB_CampoGrupo = value;
        }
    }

    // Propiedad para guardar el codigo general.
    public string AB_CodigoGeneral
    {
        get 
        { 
            return AB_CampoCodigoGeneral; 
        }
        set
        {
            AB_CampoCodigoGeneral = value; 
        }
    }

    // Propiedad para guardar el limite porcentual.
    public double AB_LimitePorcentual
    {
        get 
        {
            return AB_CampoLimitePorcentual; 
        }
        set
        {
            // Se valida que el porcentaje este entre 0 y 100.
            if (value < 0 || value > 100)
                throw new ArgumentException("El limite porcentual debe estar entre 0 y 100.");
            AB_CampoLimitePorcentual = value;
        }
    }

    // Propiedad para guardar el limite valorativo.
    public double AB_LimiteValorativo
    {
        get
        { 
            return AB_CampoLimiteValorativo;
        }
        set
        {
            // Se valida que el limite valorativo no sea negativo.
            if (value < 0)
                throw new ArgumentException("El limite valorativo no puede ser negativo.");
            AB_CampoLimiteValorativo = value;
        }
    }

    // Propiedad para guardar el limite anual.
    public double AB_LimiteAnual
    {
        get
        { 
            return AB_CampoLimiteAnual;
        }
        set
        {
            // Se valida que el limite anual no sea negativo.
            if (value < 0)
                throw new ArgumentException("El limite anual no puede ser negativo.");
            AB_CampoLimiteAnual = value;
        }
    }

    // Constructor que recibe y asigna los datos de la reaseguradora.
    public AB_Reaseguradora(int AB_IdIngresado, string AB_CodigoIngresado,string AB_NombreIngresado, string AB_GrupoIngresado,
        string AB_CodigoGeneralIngresado, double AB_PorcentajeIngresado,
        double AB_LimiteIngresado, double AB_LimiteAnualIngresado)
    {
        AB_IdReaseguradora = AB_IdIngresado;
        AB_Codigo = AB_CodigoIngresado;
        AB_Nombre = AB_NombreIngresado;
        AB_Grupo = AB_GrupoIngresado;
        AB_CodigoGeneral = AB_CodigoGeneralIngresado;
        AB_LimitePorcentual = AB_PorcentajeIngresado;
        AB_LimiteValorativo = AB_LimiteIngresado;
        AB_LimiteAnual = AB_LimiteAnualIngresado;
    }
}
