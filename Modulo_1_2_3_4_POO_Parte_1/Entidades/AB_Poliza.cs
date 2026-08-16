using System;

namespace SistemaSeguros;

// Clase para representar la informacion de una poliza de seguro.
public class AB_Poliza
{
    // Variables privadas que guardan la informacion de la poliza.
    private int AB_CampoIdPoliza;
    private int AB_CampoIdCliente;
    private int AB_CampoIdRamo;
    private string AB_CampoNumeroPoliza;
    private double AB_CampoCapitalAsegurado;
    private double AB_CampoTasaRiesgo;
    private double AB_CampoPrimaBase;
    private double AB_CampoSuperBancos;
    private double AB_CampoSeguroCampesino;
    private double AB_CampoDerechosEmision;
    private double AB_CampoIVA;
    private double AB_CampoPrimaTotal;
    private double AB_CampoCapitalRemanente;
    private string AB_CampoEstado;

    // Propiedad para identificar la poliza.
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
    // Propiedad que relaciona la poliza con su cliente.
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
    // Propiedad que relaciona la poliza con el ramo contratado.
    public int AB_IdRamo 
    { 
        get
        { 
            return AB_CampoIdRamo;
        } 
        set 
        {
            AB_CampoIdRamo = value;
        }
    }

    // Propiedad para guardar el numero de la poliza.
    public string AB_NumeroPoliza
    {
        get 
        {
            return AB_CampoNumeroPoliza;
        }
        set
        {
            // Se valida que el numero de poliza no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El numero de poliza es obligatorio.");
            AB_CampoNumeroPoliza = value;
        }
    }

    // Propiedad para guardar el capital asegurado.
    public double AB_CapitalAsegurado
    {
        get 
        { 
            return AB_CampoCapitalAsegurado; 
        }
        set
        {
            // Se valida que el capital asegurado sea mayor que cero.
            if (value <= 0)
                throw new ArgumentException("El capital asegurado debe ser un monto positivo.");
            AB_CampoCapitalAsegurado = value;
        }
    }

    // Propiedad para guardar la tasa de riesgo.
    public double AB_TasaRiesgo 
    { 
        get 
        {
            return AB_CampoTasaRiesgo;
        } 
        set 
        {
            // Se valida que la tasa de riesgo no sea negativa.
            if (value < 0) 
                throw new ArgumentException("La tasa debe ser mayor o igual a 0."); 
            AB_CampoTasaRiesgo = value;
        } 
    }
    // Propiedad para guardar la prima base de la poliza.
    public double AB_PrimaBase 
    { 
        get 
        { 
            return AB_CampoPrimaBase;
        }
        set
        { 
            // Se valida que la prima base no sea negativa.
            if (value < 0) 
                throw new ArgumentException("La prima base debe ser mayor o igual a 0.");
            AB_CampoPrimaBase = value;
        }
    }
    // Propiedad para guardar el aporte a SuperBancos.
    public double AB_SuperBancos
    {
        get 
        {
            return AB_CampoSuperBancos; 
        }
        set
        { 
            // Se valida que el aporte a SuperBancos no sea negativo.
            if (value < 0) 
                throw new ArgumentException("SuperBancos debe ser mayor o igual a 0.");
            AB_CampoSuperBancos = value; 
        } 
    }
    // Propiedad para guardar el aporte al Seguro Campesino.
    public double AB_SeguroCampesino
    {
        get 
        { 
            return AB_CampoSeguroCampesino;
        }
        set
        {
            // Se valida que el aporte al Seguro Campesino no sea negativo.
            if (value < 0) 
                throw new ArgumentException("SeguroCampesino debe ser mayor o igual a 0.");
            AB_CampoSeguroCampesino = value;
        } 
    }
    // Propiedad para guardar el derecho de emision de la poliza.
    public double AB_DerechosEmision
    { 
        get 
        { 
            return AB_CampoDerechosEmision;
        } 
        set
        {
            // Se valida que el derecho de emision no sea negativo.
            if (value < 0) 
                throw new ArgumentException("DerechosEmision debe ser mayor o igual a 0.");
            AB_CampoDerechosEmision = value;
        } 
    }
    // Propiedad para guardar el IVA calculado de la poliza.
    public double AB_IVA
    { 
        get 
        {
            return AB_CampoIVA; 
        } 
        set 
        {
            // Se valida que el IVA no sea negativo.
            if (value < 0) 
                throw new ArgumentException("IVA debe ser mayor o igual a 0."); 
            AB_CampoIVA = value; 
        } 
    }
    // Propiedad para guardar el valor total que paga el cliente.
    public double AB_PrimaTotal 
    { 
        get
        { 
            return AB_CampoPrimaTotal;
        } 
        set 
        { 
            // Se valida que la prima total no sea negativa.
            if (value < 0) 
                throw new ArgumentException("La prima total debe ser mayor o igual a 0."); 
            AB_CampoPrimaTotal = value; 
        }
    }
    // Propiedad para guardar el capital disponible despues de los siniestros.
    public double AB_CapitalRemanente
    {
        get 
        { 
            return AB_CampoCapitalRemanente; 
        }
        set
        { 
            // Se valida que el capital disponible de la poliza no sea negativo.
            if (value < 0) 
                throw new ArgumentException("El capital remanente debe ser mayor o igual a 0.");
            AB_CampoCapitalRemanente = value;
        }
    }
    // Propiedad para guardar el estado actual de la poliza.
    public string AB_Estado
    { 
        get 
        { 
            return AB_CampoEstado; 
        }
        set
        { 
            AB_CampoEstado = value;
        } 
    }

    // Constructor que recibe y asigna todos los datos de la poliza.
    public AB_Poliza(int AB_IdPolizaIngresado, int AB_IdClienteIngresado, int AB_IdRamoIngresado, string AB_NumeroPolizaIngresado, double AB_Capital, double AB_Tasa, double AB_PrimaBaseIngresada, double AB_Super, double AB_Campesino, double AB_Derechos, double AB_IVAIngresado, double AB_Total, double AB_Remanente, string AB_EstadoIngresado)
    {
        // Se asignan los datos recibidos para crear la poliza.
        AB_IdPoliza = AB_IdPolizaIngresado;
        AB_IdCliente = AB_IdClienteIngresado;
        AB_IdRamo = AB_IdRamoIngresado;
        AB_NumeroPoliza = AB_NumeroPolizaIngresado;
        AB_CapitalAsegurado = AB_Capital;
        AB_TasaRiesgo = AB_Tasa;
        AB_PrimaBase = AB_PrimaBaseIngresada;
        AB_SuperBancos = AB_Super;
        AB_SeguroCampesino = AB_Campesino;
        AB_DerechosEmision = AB_Derechos;
        AB_IVA = AB_IVAIngresado;
        AB_PrimaTotal = AB_Total;
        AB_CapitalRemanente = AB_Remanente;
        AB_Estado = AB_EstadoIngresado;
    }
}

