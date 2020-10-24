DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `GetSettingsGroupByName`(
	IN _Name VARCHAR(128),
	IN _Count INT
)
BEGIN
	SELECT *
		FROM `application-configuration`.`settings-groups`
		WHERE `Name` = _Name
		LIMIT _Count;
END$$

DELIMITER ;