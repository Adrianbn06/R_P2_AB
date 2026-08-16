using System;

namespace SistemaSeguros;

// Clase para representar una accion registrada en el log del sistema.
public class AB_LogSistema
{
    // Variables privadas que guardan los datos del log.
    private int AB_CampoIdLog;
    private DateTime AB_CampoFechaHora;
    private string AB_CampoNivel;
    private string AB_CampoModulo;
    private string AB_CampoAccion;
    private string AB_CampoMensaje;
    private string AB_CampoDetalleTecnico;
    private string AB_CampoUsuario;
    private string AB_CampoDireccionIP;

    // Propiedad para identificar el log.
    public int AB_IdLog
    {
        get 
        {
            return AB_CampoIdLog;
        }
        set 
        {
            AB_CampoIdLog = value; 
        }
    }

    // Propiedad para guardar la fecha y hora del log.
    public DateTime AB_FechaHora
    {
        get 
        { 
            return AB_CampoFechaHora; 
        }
        set
        {
            AB_CampoFechaHora = value;
        }
    }

    // Propiedad para guardar el nivel del log.
    public string AB_Nivel
    {
        get
        { 
            return AB_CampoNivel; 
        }
        set
        {
            // Se valida que el nivel del log no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nivel del log es obligatorio.");
            AB_CampoNivel = value;
        }
    }

    // Propiedad para guardar el modulo relacionado.
    public string AB_Modulo
    {
        get 
        { 
            return AB_CampoModulo; 
        }
        set
        {
            // Se valida que el modulo del log no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El modulo del log es obligatorio.");
            AB_CampoModulo = value;
        }
    }

    // Propiedad para guardar la accion realizada.
    public string AB_Accion
    {
        get
        { 
            return AB_CampoAccion;
        }
        set
        {
            // Se valida que la accion del log no este vacia.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("La accion del log es obligatoria.");
            AB_CampoAccion = value;
        }
    }

    // Propiedad para guardar el mensaje del log.
    public string AB_Mensaje
    {
        get 
        { 
            return AB_CampoMensaje; 
        }
        set
        {
            // Se valida que el mensaje del log no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El mensaje del log es obligatorio.");
            AB_CampoMensaje = value;
        }
    }

    // Propiedad para guardar el detalle tecnico.
    public string AB_DetalleTecnico
    {
        get 
        { 
            return AB_CampoDetalleTecnico;
        }
        set 
        { 
            AB_CampoDetalleTecnico = value;
        }
    }

    // Propiedad para guardar el usuario relacionado.
    public string AB_Usuario
    {
        get
        {
            return AB_CampoUsuario; 
        }
        set
        {
            AB_CampoUsuario = value; 
        }
    }

    // Propiedad para guardar la direccion IP.
    public string AB_DireccionIP
    {
        get 
        { 
            return AB_CampoDireccionIP;
        }
        set
        { 
            AB_CampoDireccionIP = value; 
        }
    }

    // Constructor que recibe y asigna los datos del log.
    public AB_LogSistema(int AB_IdIngresado, DateTime AB_FechaIngresada,string AB_NivelIngresado, string AB_ModuloIngresado,
        string AB_AccionIngresada, string AB_MensajeIngresado,
        string AB_DetalleIngresado, string AB_UsuarioIngresado,
        string AB_DireccionIngresada)
    {
        AB_IdLog = AB_IdIngresado;
        AB_FechaHora = AB_FechaIngresada;
        AB_Nivel = AB_NivelIngresado;
        AB_Modulo = AB_ModuloIngresado;
        AB_Accion = AB_AccionIngresada;
        AB_Mensaje = AB_MensajeIngresado;
        AB_DetalleTecnico = AB_DetalleIngresado;
        AB_Usuario = AB_UsuarioIngresado;
        AB_DireccionIP = AB_DireccionIngresada;
    }
}
