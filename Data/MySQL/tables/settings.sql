USE `application-configuration`;

CREATE TABLE IF NOT EXISTS `settings` (
	`ID` BIGINT NOT NULL AUTO_INCREMENT,
	`SettingsGroupID` BIGINT NOT NULL,
	`Name` VARCHAR(128) NOT NULL,
	`Value` TEXT NOT NULL,
	`Updated` DATETIME NOT NULL,
	`Created` DATETIME NOT NULL,

	PRIMARY KEY (`ID`),
	CONSTRAINT `UC_SettingsGroupIDName` UNIQUE(`SettingsGroupID`, `Name`),
	FOREIGN KEY `FK_Settings_SettingsGroups_SettingsGroupID` (`SettingsGroupID`) REFERENCES `settings-groups`(`ID`) ON DELETE CASCADE
);
