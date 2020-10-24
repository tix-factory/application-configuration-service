DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `GetSettingsBySettingsGroupId`(
	IN _SettingsGroupID BIGINT,
	IN _Count INT
)
BEGIN
	SELECT *
		FROM `application-configuration`.`settings`
		WHERE `SettingsGroupID` = _SettingsGroupID
		LIMIT _Count;
END$$

DELIMITER ;