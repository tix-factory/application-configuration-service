DELIMITER $$
USE `application-configuration`$$
CREATE PROCEDURE `DeleteSetting`(
	IN _ID BIGINT
)
BEGIN
	DELETE
		FROM `application-configuration`.`settings`
		WHERE (`ID` = _ID)
		LIMIT 1;
END$$

DELIMITER ;