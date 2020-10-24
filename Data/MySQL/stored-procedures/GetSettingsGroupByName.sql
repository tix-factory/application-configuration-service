DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `GetSettingsGroupByName`(
	IN _Name VARCHAR(128)
)
BEGIN
	SELECT *
		FROM `application-configuration`.`settings-groups`
		WHERE `Name` = _Name
		LIMIT 1;
END$$

DELIMITER ;