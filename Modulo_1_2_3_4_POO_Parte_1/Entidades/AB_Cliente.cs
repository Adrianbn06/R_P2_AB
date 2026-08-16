using System;

namespace SistemaSeguros;

// Clase para representar un cliente dentro del sistema.
public class AB_Cliente
{
    // Variables privadas que almacenan los datos del cliente.
    private int AB_CampoIdCliente;
    private string AB_CampoCedula;
    private string AB_CampoNombres;
    private string AB_CampoApellidos;
    private string AB_CampoDireccion;
    private string AB_CampoTelefono;
    private string AB_CampoCorreo;

    // Propiedad para identificar al cliente.
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

    // Propiedad para guardar la cedula del cliente.
    public string AB_Cedula
    {
        get 
        { 
            return AB_CampoCedula;
        }
        set
        {
            // Se valida que la cedula no este vacia.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Error: La cedula del cliente no puede estar vacia.");
            AB_CampoCedula = value;
        }
    }

    // Propiedad para guardar los nombres del cliente.
    public string AB_Nombres
    {
        get 
        {
            return AB_CampoNombres;
        }
        set
        {
            // Se valida que los nombres no esten vacios.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Error: Los nombres del cliente no pueden estar vacios.");
            AB_CampoNombres = value;
        }
    }

    // Propiedad para guardar los apellidos del cliente.
    public string AB_Apellidos
    {
        get 
        { 
            return AB_CampoApellidos;
        }
        set 
        { 
            AB_CampoApellidos = value; 
        }
    }

    // Propiedad para guardar la direccion del cliente.
    public string AB_Direccion 
    {
        get
        {
            return AB_CampoDireccion;
        } 
        set 
        { 
            AB_CampoDireccion = value; 
        } 
    }
    // Propiedad para guardar el telefono del cliente.
    public string AB_Telefono 
    {
        get
        { 
            return AB_CampoTelefono; 
        } 
        set 
        { 
            AB_CampoTelefono = value;
        } 
    }
    // Propiedad para guardar el correo del cliente.
    public string AB_Correo 
    {
        get
        { 
            return AB_CampoCorreo;
        } 
        set 
        { 
            AB_CampoCorreo = value;
        }
    }

    // Constructor que recibe y asigna los datos del cliente.
    public AB_Cliente(int AB_Id, string AB_CedulaIngresada, string AB_NombresIngresados, string AB_ApellidosIngresados, string AB_DireccionIngresada, string AB_TelefonoIngresado, string AB_CorreoIngresado)
    {
        // Se asignan los datos recibidos para crear el cliente.
        AB_IdCliente = AB_Id;
        AB_Cedula = AB_CedulaIngresada;
        AB_Nombres = AB_NombresIngresados;
        AB_Apellidos = AB_ApellidosIngresados;
        AB_Direccion = AB_DireccionIngresada;
        AB_Telefono = AB_TelefonoIngresado;
        AB_Correo = AB_CorreoIngresado;
    }
}

