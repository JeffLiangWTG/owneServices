CREATE USER [SCOM] FOR LOGIN [SCOM]
GO

EXEC sp_addrolemember db_datareader, [SCOM]
GO