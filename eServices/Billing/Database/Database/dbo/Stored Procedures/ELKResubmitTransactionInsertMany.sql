CREATE PROCEDURE [dbo].[ELKResubmitTransactionInsertMany]
	@Transactions TVP_UsageTransaction READONLY
AS
	INSERT INTO [dbo].[ELKResubmitTransaction] (RT_PK, RT_JsonData) SELECT RT_PK, RT_JsonData FROM @Transactions

RETURN 0
