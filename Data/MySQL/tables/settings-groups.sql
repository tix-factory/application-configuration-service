USE `application-configuration`;

CREATE TABLE IF NOT EXISTS `settings-groups` (
	`ID` BIGINT NOT NULL AUTO_INCREMENT,
	`Name` VARCHAR(128) NOT NULL,
	`Updated` DATETIME NOT NULL,
	`Created` DATETIME NOT NULL,

	PRIMARY KEY (`ID`),
	CONSTRAINT `UC_Name` UNIQUE(`Name`)
);
