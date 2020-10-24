DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `UpdateSetting`(
	IN _SettingsGroupID BIGINT,
	IN _Name VARBINARY(128),
	IN _Value TEXT,
	IN _ID BIGINT
)
BEGIN
	UPDATE `application-configuration`.`settings`
	SET
		`SettingsGroupID` = _SettingsGroupID,
		`Name` = _Name,
		`Value` = _Value,
		`Updated` = UTC_Timestamp()
	WHERE (`ID` = _ID)
	LIMIT 1;
END$$

DELIMITER ;