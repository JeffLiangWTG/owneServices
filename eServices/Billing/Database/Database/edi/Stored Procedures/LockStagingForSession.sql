CREATE PROCEDURE [edi].[LockStagingForSession]
	@LockTimeoutMs int = 0
AS
	declare @gotLock int;
	EXEC @gotLock = sp_getapplock @Resource = 'ProcessStagingLock',
				@LockMode = 'Exclusive',
				@LockOwner = 'Session',
				@LockTimeout = @LockTimeoutMs;


	IF (@gotLock < 0)
	BEGIN
		declare @Error varchar(200) = 'Could not obtain staging lock - another session is executing ' + CAST(@gotLock as varchar(10));
		THROW 50000, @Error, 1
	END
	
RETURN @gotLock

