CREATE USER [eHubReader] FOR LOGIN [eHubReader]
GO

EXEC sp_addrolemember db_datareader, [eHubReader]
GO