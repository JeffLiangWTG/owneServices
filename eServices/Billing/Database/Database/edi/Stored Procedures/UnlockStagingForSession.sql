CREATE PROCEDURE [edi].[UnlockStagingForSession]
AS
	declare @rc int;
	EXEC @rc = sp_releaseapplock @Resource = 'ProcessStagingLock', @LockOwner = 'Session';	
RETURN @rc
