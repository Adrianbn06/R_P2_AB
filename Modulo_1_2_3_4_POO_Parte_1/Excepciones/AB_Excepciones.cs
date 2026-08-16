using System;

namespace SistemaSeguros;

// Excepcion personalizada para polizas con valores negativos o fuera de rango.
public class AB_PolizaInvalidaException : Exception
{
    // Constructores para crear la excepcion vacia, con mensaje o con un error interno.
    public AB_PolizaInvalidaException() { }
    public AB_PolizaInvalidaException(string AB_Mensaje) : base(AB_Mensaje) { }
    public AB_PolizaInvalidaException(string AB_Mensaje, Exception AB_ExcepcionInterna) : base(AB_Mensaje, AB_ExcepcionInterna) { }
}

// Excepcion critica para detener procesos si se detecta fraude o lavado de activos (codigo 999).
public class AB_FraudeUAFException : Exception
{
    // Constructores para crear la excepcion vacia, con mensaje o con un error interno.
    public AB_FraudeUAFException() { }
    public AB_FraudeUAFException(string AB_Mensaje) : base(AB_Mensaje) { }
    public AB_FraudeUAFException(string AB_Mensaje, Exception AB_ExcepcionInterna) : base(AB_Mensaje, AB_ExcepcionInterna) { }
}

// Excepcion para reclamos que no cumplen las reglas del siniestro.
public class AB_SiniestroInvalidoException : Exception
{
    // Constructores para crear la excepcion vacia, con mensaje o con un error interno.
    public AB_SiniestroInvalidoException() { }
    public AB_SiniestroInvalidoException(string AB_Mensaje) : base(AB_Mensaje) { }
    public AB_SiniestroInvalidoException(string AB_Mensaje, Exception AB_ExcepcionInterna) : base(AB_Mensaje, AB_ExcepcionInterna) { }
}

// Excepcion para asientos que no cumplen la partida doble.
public class AB_AsientoContableInvalidoException : Exception
{
    // Constructores para crear la excepcion vacia, con mensaje o con un error interno.
    public AB_AsientoContableInvalidoException() { }
    public AB_AsientoContableInvalidoException(string AB_Mensaje) : base(AB_Mensaje) { }
    public AB_AsientoContableInvalidoException(string AB_Mensaje, Exception AB_ExcepcionInterna) : base(AB_Mensaje, AB_ExcepcionInterna) { }
}
