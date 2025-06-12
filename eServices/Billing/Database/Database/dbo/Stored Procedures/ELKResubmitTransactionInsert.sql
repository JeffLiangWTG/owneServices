CREATE PROCEDURE [dbo].[ELKResubmitTransactionInsert]
	@jsonData varchar(max)
AS
	INSERT INTO [dbo].[ELKResubmitTransaction] (RT_PK, RT_JsonData) VALUES (NEWID(), @jsonData)

RETURN 0
