IF DB_ID(N'GestionHotel') IS NULL
BEGIN
    CREATE DATABASE GestionHotel;
END;
GO

USE GestionHotel;
GO

CREATE TABLE Clientes
(
    id INT IDENTITY(1, 1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    apellido VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    telefono VARCHAR(15) NOT NULL,
    documento_identidad VARCHAR(20) NOT NULL UNIQUE
);
GO
CREATE TABLE Habitaciones
(
    id INT IDENTITY(1, 1) PRIMARY KEY,
    numero VARCHAR(10) NOT NULL UNIQUE,
    tipo VARCHAR(50) NOT NULL,
    precio_por_noche DECIMAL(10, 2) NOT NULL,
    estado VARCHAR(20) NOT NULL DEFAULT 'Disponible',
    CONSTRAINT CK_Habitaciones_PrecioPositivo CHECK (precio_por_noche > 0)
);
GO

CREATE TABLE Metodos_Pago
(
    id INT IDENTITY(1, 1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL UNIQUE
);
GO

CREATE TABLE Usuarios
(
    id INT IDENTITY(1, 1) PRIMARY KEY,
    nombre VARCHAR(50) NOT NULL,
    email VARCHAR(100) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    rol VARCHAR(20) NOT NULL,
    estado BIT NOT NULL DEFAULT 1,
    creado_en DATETIME NOT NULL DEFAULT GETUTCDATE(),
    actualizado_en DATETIME NULL,
    CONSTRAINT CK_Usuarios_Rol CHECK (rol IN ('Administrador', 'Recepcionista'))
);
GO

CREATE TABLE Reservas
(
    id INT IDENTITY(1, 1) PRIMARY KEY,
    fecha_inicio DATE NOT NULL,
    fecha_fin DATE NOT NULL,
    estado VARCHAR(20) NOT NULL,
    habitacion_id INT NOT NULL,
    cliente_id INT NOT NULL,
    total DECIMAL(10, 2) NOT NULL,
    monto_pagado DECIMAL(10, 2) NOT NULL DEFAULT 0,
    saldo_pendiente AS (total - monto_pagado) PERSISTED,
    estado_pago VARCHAR(20) NOT NULL DEFAULT 'Pendiente',
    CONSTRAINT FK_Reservas_Habitaciones FOREIGN KEY (habitacion_id) REFERENCES Habitaciones(id),
    CONSTRAINT FK_Reservas_Clientes FOREIGN KEY (cliente_id) REFERENCES Clientes(id),
    CONSTRAINT CK_Reservas_Fechas CHECK (fecha_fin > fecha_inicio),
    CONSTRAINT CK_Reservas_Montos CHECK (total >= 0 AND monto_pagado >= 0 AND monto_pagado <= total),
    CONSTRAINT CK_Reservas_Estado CHECK (estado IN ('Pendiente', 'Confirmada', 'Cancelada', 'Completada')),
    CONSTRAINT CK_Reservas_EstadoPago CHECK (estado_pago IN ('Pendiente', 'Parcial', 'Pagado'))
);
GO

CREATE TABLE Pagos
(
    id INT IDENTITY(1, 1) PRIMARY KEY,
    reserva_id INT NOT NULL,
    fecha_pago DATE NOT NULL,
    monto_pago DECIMAL(10, 2) NOT NULL,
    metodo_pago_id INT NOT NULL,
    referencia_transaccion VARCHAR(100) NULL,
    detalles_pago VARCHAR(255) NULL,
    CONSTRAINT FK_Pagos_Reservas FOREIGN KEY (reserva_id) REFERENCES Reservas(id),
    CONSTRAINT FK_Pagos_MetodosPago FOREIGN KEY (metodo_pago_id) REFERENCES Metodos_Pago(id),
    CONSTRAINT CK_Pagos_MontoPositivo CHECK (monto_pago > 0)
);
GO

INSERT INTO Metodos_Pago (nombre)
SELECT nombre
FROM (VALUES ('Efectivo'), ('Tarjeta'), ('Transferencia')) AS metodos(nombre)
WHERE NOT EXISTS
(
    SELECT 1 FROM Metodos_Pago existente WHERE existente.nombre = metodos.nombre
);
GO
