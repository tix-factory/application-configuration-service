DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `InsertSetting`(
	IN _SettingsGroupID BIGINT,
	IN _Name VARBINARY(128),
	IN _Value TEXT
)
BEGIN
	INSERT INTO `application-configuration`.`settings`
	(
		`SettingsGroupID`,
		`Name`,
		`Value`,
		`Created`,
		`Updated`
	)
	VALUES
	(
		_SettingsGroupID,
		_Name,
		_Value,
		UTC_Timestamp(),
		UTC_Timestamp()
	);
	
	SELECT LAST_INSERT_ID() as `ID`;
END$$

DELIMITER ;