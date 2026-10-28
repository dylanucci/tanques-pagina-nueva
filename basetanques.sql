-- --------------------------------------------------------
-- Host:                         127.0.0.1
-- Versión del servidor:         8.0.40 - MySQL Community Server - GPL
-- SO del servidor:              Win64
-- HeidiSQL Versión:             12.10.0.7000
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Volcando estructura de base de datos para tanques
CREATE DATABASE IF NOT EXISTS `tanques` /*!40100 DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci */ /*!80016 DEFAULT ENCRYPTION='N' */;
USE `tanques`;

-- Volcando estructura para tabla tanques.roles
CREATE TABLE IF NOT EXISTS `roles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Roles_Nombre` (`Nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Volcando datos para la tabla tanques.roles: ~2 rows (aproximadamente)
INSERT INTO `roles` (`Id`, `Nombre`) VALUES
	(2, 'Admin'),
	(1, 'Dueño');

-- Volcando estructura para tabla tanques.solicitudes
CREATE TABLE IF NOT EXISTS `solicitudes` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Apellido` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Dni` varchar(30) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Direccion` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Fecha` date NOT NULL,
  `Hora` time NOT NULL,
  `Estado` tinyint(1) NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Volcando datos para la tabla tanques.solicitudes: ~1 rows (aproximadamente)
INSERT INTO `solicitudes` (`Id`, `Nombre`, `Apellido`, `Dni`, `Direccion`, `Fecha`, `Hora`, `Estado`) VALUES
	(1, 'Dylan', 'Sarzuri', '48314131', 'Campana 1313', '2026-09-30', '18:02:00', 1);

-- Volcando estructura para tabla tanques.tanques
CREATE TABLE IF NOT EXISTS `tanques` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `TipoContenido` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci DEFAULT NULL,
  `CapacidadActualCM3` decimal(18,2) NOT NULL,
  `CapacidadTotalCM3` decimal(18,2) NOT NULL,
  `Diametro` decimal(10,2) NOT NULL,
  `Ubicacion` varchar(50) DEFAULT NULL,
  `AlturaCM` double NOT NULL DEFAULT '0',
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=4 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Volcando datos para la tabla tanques.tanques: ~1 rows (aproximadamente)
INSERT INTO `tanques` (`Id`, `TipoContenido`, `CapacidadActualCM3`, `CapacidadTotalCM3`, `Diametro`, `Ubicacion`, `AlturaCM`) VALUES
	(3, 'Agua', 307.88, 384.85, 7.00, 'Campana 1313', 10);

-- Volcando estructura para tabla tanques.usuarios
CREATE TABLE IF NOT EXISTS `usuarios` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Usuario` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `Contrasena` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `RolId` int NOT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `IX_Usuarios_Usuario` (`Usuario`),
  KEY `IX_Usuarios_RolId` (`RolId`),
  CONSTRAINT `FK_Usuarios_Roles_RolId` FOREIGN KEY (`RolId`) REFERENCES `roles` (`Id`) ON DELETE RESTRICT
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Volcando datos para la tabla tanques.usuarios: ~1 rows (aproximadamente)
INSERT INTO `usuarios` (`Id`, `Nombre`, `Usuario`, `Contrasena`, `RolId`) VALUES
	(1, 'Administrador', 'admin', '123456', 2);

-- Volcando estructura para tabla tanques.usuariotanque
CREATE TABLE IF NOT EXISTS `usuariotanque` (
  `UsuarioId` int NOT NULL,
  `TanqueId` int NOT NULL,
  PRIMARY KEY (`UsuarioId`,`TanqueId`),
  KEY `IX_UsuarioTanque_TanqueId` (`TanqueId`),
  CONSTRAINT `FK_UsuarioTanque_Tanques_TanqueId` FOREIGN KEY (`TanqueId`) REFERENCES `tanques` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_UsuarioTanque_Usuarios_UsuarioId` FOREIGN KEY (`UsuarioId`) REFERENCES `usuarios` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Volcando datos para la tabla tanques.usuariotanque: ~0 rows (aproximadamente)

-- Volcando estructura para tabla tanques.__efmigrationshistory
CREATE TABLE IF NOT EXISTS `__efmigrationshistory` (
  `MigrationId` varchar(150) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  `ProductVersion` varchar(32) CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci NOT NULL,
  PRIMARY KEY (`MigrationId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

-- Volcando datos para la tabla tanques.__efmigrationshistory: ~3 rows (aproximadamente)
INSERT INTO `__efmigrationshistory` (`MigrationId`, `ProductVersion`) VALUES
	('20260930192837_InitialCreate', '8.0.20'),
	('20260930210000_AddAlturaAndSolicitudes', '8.0.20'),
	('20260930210055_solicitudUpdate', '8.0.20');

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;
