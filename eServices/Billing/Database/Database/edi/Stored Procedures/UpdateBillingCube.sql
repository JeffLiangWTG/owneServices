CREATE PROCEDURE [edi].[UpdateBillingCube]
	@isTestServer bit = 0
AS
SET NOCOUNT ON;

IF @isTestServer = 0
BEGIN

	BEGIN TRY
		EXEC msdb.dbo.sp_start_job N'Process Billing Cube (Partial, Multidimensional)' 
	END TRY

	BEGIN CATCH
		DECLARE @SqlAgentErrorMessage NVARCHAR(MAX) = N'Error message: ' + ISNULL(ERROR_MESSAGE(), N'')

		BEGIN TRY
			EXEC msdb.dbo.sp_notify_operator  
				@name = 'BI Team (Failure)', 
				@subject = N'[The job cannot be started]: Could not run "Process Billing Cube (Partial, Multidimensional)" job on au2co-ssql-403a|b\INSTANCE1 ([edi].[ProcessStaging])',
				@body = @SqlAgentErrorMessage
		END TRY

		BEGIN CATCH
			--Do nothing
		END CATCH

	END CATCH
END
		
RETURN 0
