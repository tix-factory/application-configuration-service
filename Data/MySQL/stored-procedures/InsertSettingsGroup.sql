DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `InsertSettingsGroup`(
	IN _Name VARBINARY(128)
)
BEGIN
	INSERT INTO `application-configuration`.`settings-groups`
	(
		`Name`,
		`Created`,
		`Updated`
	)
	VALUES
	(
		_Name,
		UTC_Timestamp(),
		UTC_Timestamp()
	);
	
	SELECT LAST_INSERT_ID() as `ID`;
END$$

DELIMITER ;