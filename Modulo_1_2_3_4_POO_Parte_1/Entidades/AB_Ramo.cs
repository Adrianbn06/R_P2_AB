using System;

namespace SistemaSeguros;

// Clase para representar los ramos de seguro del sistema.
public class AB_Ramo
{
    // Variables privadas que almacenan los datos del ramo.
    private int AB_CampoIdRamo;
    private string AB_CampoCodigoRamo;
    private string AB_CampoNombreRamo;

    // Propiedad para identificar el ramo.
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

    // Propiedad para guardar el codigo del ramo.
    public string AB_CodigoRamo
    {
        get 
        { 
            return AB_CampoCodigoRamo;
        }
        set
        {
            // Se valida que el codigo no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El codigo del ramo no puede estar vacio.");
            AB_CampoCodigoRamo = value;
        }
    }

    // Propiedad para guardar el nombre del ramo.
    public string AB_NombreRamo
    {
        get 
        { 
            return AB_CampoNombreRamo; 
        }
        set
        {
            // Se valida que el nombre no este vacio.
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("El nombre del ramo no puede estar vacio.");
            AB_CampoNombreRamo = value;
        }
    }

    // Constructor que inicializa los datos del ramo.
    public AB_Ramo(int AB_Id, string AB_Codigo, string AB_Nombre)
    {
        // Se asignan los datos recibidos para crear el ramo.
        AB_IdRamo = AB_Id;
        AB_CodigoRamo = AB_Codigo;
        AB_NombreRamo = AB_Nombre;
    }
}

