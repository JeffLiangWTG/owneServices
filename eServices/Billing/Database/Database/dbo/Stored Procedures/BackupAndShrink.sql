CREATE PROCEDURE [dbo].[BackupAndShrink] AS
BEGIN
	EXECUTE master.dbo.xp_delete_file 0,N'\\uat-sbak-smb.sand.wtg.zone\sql_backup\CargoWise.eServices.Billing.bak'
	EXECUTE master.dbo.xp_delete_file 0,N'\\uat-sbak-smb.sand.wtg.zone\sql_backup\CargoWise.eServices.Billing_log.bak'
	BACKUP DATABASE [CargoWise.eServices.Billing] TO DISK = '\\uat-sbak-smb.sand.wtg.zone\sql_backup\CargoWise.eServices.Billing.bak';

	DECLARE @logReuseWaitDesc VARCHAR(50)
	DECLARE @count INT = 0
	SELECT TOP 1 @logReuseWaitDesc = log_reuse_wait_desc FROM sys.databases WHERE [name] = 'CargoWise.eServices.Billing'

	WHILE @logReuseWaitDesc = 'LOG_BACKUP'
	BEGIN
		IF @count >= 10
		BEGIN
			RAISERROR ('Log backup is still required after reaching max retry', 16, 1);
		END
		BACKUP LOG [CargoWise.eServices.Billing] TO DISK = '\\uat-sbak-smb.sand.wtg.zone\sql_backup\CargoWise.eServices.Billing_log.bak';
		SELECT TOP 1 @logReuseWaitDesc = log_reuse_wait_desc FROM sys.databases WHERE [name] = 'CargoWise.eServices.Billing'
		SET @count = @count + 1
	END

	DECLARE db_cursor CURSOR FOR SELECT file_id FROM [CargoWise.eServices.Billing].sys.database_files
	DECLARE @file_id INT

	OPEN db_cursor
	FETCH NEXT FROM db_cursor INTO @file_id
	WHILE @@FETCH_STATUS = 0
	BEGIN
		DBCC SHRINKFILE (@file_id, 10);

		FETCH NEXT FROM db_cursor INTO @file_id
	END
END
