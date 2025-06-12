USE eHubTransactions

BEGIN TRAN
DECLARE @CNCustomsSW_RegistrationType_PK UNIQUEIDENTIFIER = '4A144D64-5E94-43AB-A43F-1C5ACC20EBD5' --SELECT NEWID()

INSERT INTO eHubRegistrationType
(RT_PK, RT_ID, RT_Description, RT_RegistrantType)
SELECT @CNCustomsSW_RegistrationType_PK, 'CNCustomsSW', 'CN Customs Single Window Authentication', 'Client'

ROLLBACK
--COMMIT