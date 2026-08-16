using System;

namespace SistemaSeguros;

// Clase para representar la distribucion de un riesgo de reaseguro.
public class AB_RepartoReaseguro
{
    // Variables privadas que guardan los datos del reparto.
    private int AB_CampoIdReparto;
    private int AB_CampoIdPoliza;
    private int AB_CampoIdReaseguradora;
    private double AB_CampoRetencionPropia;
    private double AB_CampoCapitalContrato;
    private double AB_CampoCapitalFacultativo;

    // Propiedad para identificar el reparto.
    public int AB_IdReparto
    {
        get 
        { 
            return AB_CampoIdReparto;
        }
        set
        { 
            AB_CampoIdReparto = value; 
        }
    }

    // Propiedad que relaciona el reparto con la poliza.
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

    // Propiedad que relaciona el reparto con la reaseguradora.
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

    // Propiedad para guardar la retencion propia.
    public double AB_RetencionPropia
    {
        get
        {
            return AB_CampoRetencionPropia;
        }
        set
        {
            // Se valida que la retencion propia no sea negativa.
            if (value < 0)
                throw new ArgumentException("La retencion propia no puede ser negativa.");
            AB_CampoRetencionPropia = value;
        }
    }

    // Propiedad para guardar el capital del contrato.
    public double AB_CapitalContrato
    {
        get
        { 
            return AB_CampoCapitalContrato;
        }
        set
        {
            // Se valida que el capital del contrato no sea negativo.
            if (value < 0)
                throw new ArgumentException("El capital de contrato no puede ser negativo.");
            AB_CampoCapitalContrato = value;
        }
    }

    // Propiedad para guardar el capital facultativo.
    public double AB_CapitalFacultativo
    {
        get 
        { 
            return AB_CampoCapitalFacultativo;
        }
        set
        {
            // Se valida que el capital facultativo no sea negativo.
            if (value < 0)
                throw new ArgumentException("El capital facultativo no puede ser negativo.");
            AB_CampoCapitalFacultativo = value;
        }
    }

    // Constructor que recibe y asigna los datos del reparto.
    public AB_RepartoReaseguro(int AB_IdRepartoIngresado, int AB_IdPolizaIngresado,int AB_IdReaseguradoraIngresado, double AB_RetencionIngresada,
        double AB_ContratoIngresado, double AB_FacultativoIngresado)
    {
        AB_IdReparto = AB_IdRepartoIngresado;
        AB_IdPoliza = AB_IdPolizaIngresado;
        AB_IdReaseguradora = AB_IdReaseguradoraIngresado;
        AB_RetencionPropia = AB_RetencionIngresada;
        AB_CapitalContrato = AB_ContratoIngresado;
        AB_CapitalFacultativo = AB_FacultativoIngresado;
    }
}
