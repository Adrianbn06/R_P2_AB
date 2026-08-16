using System;

namespace SistemaSeguros;

// Clase para representar un siniestro registrado en el sistema.
public class AB_Siniestro
{
    // Variables privadas que guardan los datos del siniestro.
    private int AB_CampoIdSiniestro;
    private int AB_CampoIdPoliza;
    private string AB_CampoNumeroReclamo;
    private DateTime AB_CampoFechaSiniestro;
    private double AB_CampoDanosReclamados;
    private double AB_CampoDeducibleAsumido;
    private double AB_CampoPagoNeto;
    private string AB_CampoEstadoAuditoria;

    // Propiedad para identificar el siniestro.
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

    // Propiedad que relaciona el siniestro con la poliza.
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

    // Propiedad para guardar el numero de reclamo.
    public string AB_NumeroReclamo
    {
        get 
        { 
            return AB_CampoNumeroReclamo; 
        }
        set
        {
            // Se valida que el numero de reclamo no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El numero de reclamo es obligatorio.");
            AB_CampoNumeroReclamo = value;
        }
    }

    // Propiedad para guardar la fecha del siniestro.
    public DateTime AB_FechaSiniestro
    {
        get 
        { 
            return AB_CampoFechaSiniestro;
        }
        set 
        { 
            AB_CampoFechaSiniestro = value;
        }
    }

    // Propiedad para guardar los danos reclamados.
    public double AB_DanosReclamados
    {
        get 
        {
            return AB_CampoDanosReclamados;
        }
        set
        {
            // Se valida que el monto de danos no sea negativo.
            if (value < 0)
                throw new ArgumentException("Los danos reclamados no pueden ser negativos.");
            AB_CampoDanosReclamados = value;
        }
    }

    // Propiedad para guardar el deducible asumido.
    public double AB_DeducibleAsumido
    {
        get 
        { 
            return AB_CampoDeducibleAsumido; 
        }
        set
        {
            // Se valida que el deducible no sea negativo.
            if (value < 0)
                throw new ArgumentException("El deducible no puede ser negativo.");
            AB_CampoDeducibleAsumido = value;
        }
    }

    // Propiedad para guardar el pago neto.
    public double AB_PagoNeto
    {
        get 
        { 
            return AB_CampoPagoNeto; 
        }
        set
        {
            // Se valida que el pago neto no sea negativo.
            if (value < 0)
                throw new ArgumentException("El pago neto no puede ser negativo.");
            AB_CampoPagoNeto = value;
        }
    }

    // Propiedad para guardar el estado de auditoria.
    public string AB_EstadoAuditoria
    {
        get 
        { 
            return AB_CampoEstadoAuditoria; 
        }
        set
        {
            // Se valida que el estado de auditoria no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El estado del siniestro es obligatorio.");
            AB_CampoEstadoAuditoria = value;
        }
    }

    // Constructor que recibe y asigna los datos del siniestro.
    public AB_Siniestro(int AB_IdSiniestroIngresado, int AB_IdPolizaIngresado,string AB_NumeroReclamoIngresado, DateTime AB_FechaIngresada,
        double AB_DanosIngresados, double AB_DeducibleIngresado,
        double AB_PagoIngresado, string AB_EstadoIngresado)
    {
        AB_IdSiniestro = AB_IdSiniestroIngresado;
        AB_IdPoliza = AB_IdPolizaIngresado;
        AB_NumeroReclamo = AB_NumeroReclamoIngresado;
        AB_FechaSiniestro = AB_FechaIngresada;
        AB_DanosReclamados = AB_DanosIngresados;
        AB_DeducibleAsumido = AB_DeducibleIngresado;
        AB_PagoNeto = AB_PagoIngresado;
        AB_EstadoAuditoria = AB_EstadoIngresado;
    }
}
