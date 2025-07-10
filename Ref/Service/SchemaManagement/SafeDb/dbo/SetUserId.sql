CREATE PROCEDURE [dbo].[SetUserId]
	@userId VARCHAR(50)
AS
	EXEC sys.sp_set_session_context @key = N'userId', @value = @userId;  
RETURN 0
