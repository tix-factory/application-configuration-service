DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `UpdateSettingsGroup`(
	IN _Name VARBINARY(128),
	IN _ID BIGINT
)
BEGIN
	UPDATE `application-configuration`.`settings-groups`
	SET
		`Name` = _Name,
		`Updated` = UTC_Timestamp()
	WHERE (`ID` = _ID)
	LIMIT 1;
END$$

DELIMITER ;