USE eHubTransactions
GO

BEGIN TRANSACTION

DECLARE @RT_ACAS_BRProtocol uniqueidentifier = 'AF8F8014-8168-4469-9196-C5C6B7F3CE47'

INSERT INTO [eHubTransactions].[dbo].[eHubRegistrationType]
				           ([RT_PK]
				           ,[RT_ID]
				           ,[RT_Description]
				           ,[RT_RegistrantType])
				     VALUES
				           (@RT_ACAS_BRProtocol
				           ,'ACAS_BRProtocol'
				           ,'ACAS Brazil Protocol'
				           ,'AsyncPolling')

--COMMIT

WHILE @@TRANCOUNT > 0
	ROLLBACK
