DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `DeleteSettingsGroup`(
	IN _ID BIGINT
)
BEGIN
	DELETE
		FROM `application-configuration`.`settings-groups`
		WHERE (`ID` = _ID)
		LIMIT 1;
END$$

DELIMITER ;