USE [eHubTransactions]
GO
/****** Object:  StoredProcedure [dbo].[SelectAsyncPollingRegistrationPerStaffIDCount]    Script Date: 3/01/2020 10:07:55 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
ALTER PROCEDURE [dbo].[SelectAsyncPollingRegistrationPerStaffIDCount]
@RegistrationTypeId VARCHAR(50) = NULL
AS
BEGIN

	SET NOCOUNT ON;

	SELECT COUNT(1) 
	FROM [dbo].[eHubAsyncPollingRegistration] reg INNER JOIN eHubRegistrationType regType
		ON reg.PR_RT = regType.RT_PK
	WHERE ((reg.PR_RT IS NULL AND @RegistrationTypeId IS NULL) OR regType.RT_ID = @RegistrationTypeId)
		AND PR_XML.value('(//State)[1]','varchar(max)') = '0'

END