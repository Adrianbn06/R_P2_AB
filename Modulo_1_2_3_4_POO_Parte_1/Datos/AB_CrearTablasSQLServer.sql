-- Tabla que guarda la informacion de los clientes.
CREATE TABLE AB_Cliente (
    AB_IdCliente INT IDENTITY(1,1) PRIMARY KEY,
    AB_Cedula VARCHAR(10) NOT NULL UNIQUE,
    AB_Nombres VARCHAR(100) NOT NULL,
    AB_Apellidos VARCHAR(100) NOT NULL,
    AB_Direccion VARCHAR(200) NULL,
    AB_Telefono VARCHAR(20) NULL,
    AB_Correo VARCHAR(150) NULL
);
GO

-- Tabla que guarda los ramos de seguro disponibles.
CREATE TABLE AB_Ramo (
    AB_IdRamo INT IDENTITY(1,1) PRIMARY KEY,
    AB_CodigoRamo VARCHAR(20) NOT NULL UNIQUE,
    AB_NombreRamo VARCHAR(100) NOT NULL
);
GO

-- Tabla que guarda las reaseguradoras y sus limites de cobertura.
CREATE TABLE AB_Reaseguradora (
    AB_IdReaseguradora INT IDENTITY(1,1) PRIMARY KEY,
    AB_Codigo VARCHAR(20) NOT NULL UNIQUE,
    AB_Nombre VARCHAR(150) NOT NULL,
    AB_Grupo VARCHAR(100) NULL,
    AB_CodigoGeneral VARCHAR(50) NULL,
    AB_LimitePorcentual DECIMAL(18,2) NULL,
    AB_LimiteValorativo DECIMAL(18,2) NULL,
    AB_LimiteAnual DECIMAL(18,2) NULL
);
GO

-- Tabla que guarda las cuentas usadas por el modulo contable.
CREATE TABLE AB_CuentaContable (
    AB_IdCuenta INT IDENTITY(1,1) PRIMARY KEY,
    AB_CodigoCuenta VARCHAR(30) NOT NULL UNIQUE,
    AB_NombreCuenta VARCHAR(150) NOT NULL,
    AB_Naturaleza VARCHAR(30) NOT NULL,
    AB_TipoCuenta VARCHAR(50) NOT NULL
);
GO

-- Tabla que guarda las alertas UAF relacionadas con cada cliente.
CREATE TABLE AB_AlertaUAF (
    AB_IdAlerta INT IDENTITY(1,1) PRIMARY KEY,
    AB_IdCliente INT NOT NULL,
    AB_CodigoAlerta VARCHAR(10) NOT NULL,
    AB_NivelRiesgo VARCHAR(30) NOT NULL,
    AB_FechaReporte DATETIME NOT NULL,
    FOREIGN KEY (AB_IdCliente) REFERENCES AB_Cliente(AB_IdCliente)
);
GO

-- Tabla que guarda las polizas emitidas y sus valores financieros.
CREATE TABLE AB_Poliza (
    AB_IdPoliza INT IDENTITY(1,1) PRIMARY KEY,
    AB_IdCliente INT NOT NULL,
    AB_IdRamo INT NOT NULL,
    AB_NumeroPoliza VARCHAR(20) NOT NULL UNIQUE,
    AB_CapitalAsegurado DECIMAL(18,2) NOT NULL,
    AB_TasaRiesgo DECIMAL(18,2) NOT NULL,
    AB_PrimaBase DECIMAL(18,2) NOT NULL,
    AB_SuperBancos DECIMAL(18,2) NULL,
    AB_SeguroCampesino DECIMAL(18,2) NULL,
    AB_DerechosEmision DECIMAL(18,2) NULL,
    AB_IVA DECIMAL(18,2) NULL,
    AB_PrimaTotal DECIMAL(18,2) NOT NULL,
    AB_CapitalRemanente DECIMAL(18,2) NOT NULL,
    AB_Estado VARCHAR(30) NOT NULL,
    FOREIGN KEY (AB_IdCliente) REFERENCES AB_Cliente(AB_IdCliente),
    FOREIGN KEY (AB_IdRamo) REFERENCES AB_Ramo(AB_IdRamo)
);
GO

-- Tabla que guarda los siniestros registrados para cada poliza.
CREATE TABLE AB_Siniestro (
    AB_IdSiniestro INT IDENTITY(1,1) PRIMARY KEY,
    AB_IdPoliza INT NOT NULL,
    AB_NumeroReclamo VARCHAR(20) NOT NULL UNIQUE,
    AB_FechaSiniestro DATETIME NOT NULL,
    AB_DanosReclamados DECIMAL(18,2) NOT NULL,
    AB_DeducibleAsumido DECIMAL(18,2) NOT NULL,
    AB_PagoNeto DECIMAL(18,2) NOT NULL,
    AB_EstadoAuditoria VARCHAR(100) NOT NULL,
    FOREIGN KEY (AB_IdPoliza) REFERENCES AB_Poliza(AB_IdPoliza)
);
GO

-- Tabla que guarda la distribucion del riesgo entre las reaseguradoras.
CREATE TABLE AB_RepartoReaseguro (
    AB_IdReparto INT IDENTITY(1,1) PRIMARY KEY,
    AB_IdPoliza INT NOT NULL,
    AB_IdReaseguradora INT NOT NULL,
    AB_RetencionPropia DECIMAL(18,2) NOT NULL,
    AB_CapitalContrato DECIMAL(18,2) NOT NULL,
    AB_CapitalFacultativo DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (AB_IdPoliza) REFERENCES AB_Poliza(AB_IdPoliza),
    FOREIGN KEY (AB_IdReaseguradora) REFERENCES AB_Reaseguradora(AB_IdReaseguradora)
);
GO

-- Tabla que guarda la informacion principal de cada asiento contable.
CREATE TABLE AB_CabeceraAsiento (
    AB_IdAsiento INT IDENTITY(1,1) PRIMARY KEY,
    AB_IdPoliza INT NULL,
    AB_IdSiniestro INT NULL,
    AB_NumeroComprobante VARCHAR(30) NOT NULL UNIQUE,
    AB_FechaTransaccion DATETIME NOT NULL,
    AB_ModuloOrigen VARCHAR(50) NOT NULL,
    AB_TotalDebe DECIMAL(18,2) NOT NULL,
    AB_TotalHaber DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (AB_IdPoliza) REFERENCES AB_Poliza(AB_IdPoliza),
    FOREIGN KEY (AB_IdSiniestro) REFERENCES AB_Siniestro(AB_IdSiniestro)
);
GO

-- Tabla que guarda las cuentas y valores que forman cada asiento.
CREATE TABLE AB_DetalleAsiento (
    AB_IdDetalle INT IDENTITY(1,1) PRIMARY KEY,
    AB_IdAsiento INT NOT NULL,
    AB_IdCuenta INT NOT NULL,
    AB_ValorDebe DECIMAL(18,2) NOT NULL,
    AB_ValorHaber DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (AB_IdAsiento) REFERENCES AB_CabeceraAsiento(AB_IdAsiento),
    FOREIGN KEY (AB_IdCuenta) REFERENCES AB_CuentaContable(AB_IdCuenta)
);
GO

-- Tabla que guarda las acciones y errores registrados por el sistema.
CREATE TABLE AB_LogSistema (
    AB_IdLog INT IDENTITY(1,1) PRIMARY KEY,
    AB_FechaHora DATETIME NOT NULL,
    AB_Nivel VARCHAR(30) NOT NULL,
    AB_Modulo VARCHAR(50) NOT NULL,
    AB_Accion VARCHAR(100) NOT NULL,
    AB_Mensaje VARCHAR(500) NOT NULL,
    AB_DetalleTecnico VARCHAR(500) NULL,
    AB_Usuario VARCHAR(100) NULL,
    AB_DireccionIP VARCHAR(50) NULL
);
GO
