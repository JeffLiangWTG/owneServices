IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'TempTableeHubTransactions'))
BEGIN
    DROP TABLE TempTableeHubTransactions
END

CREATE TABLE TempTableeHubTransactions (ClientCode VARCHAR(3), ClientPass VARCHAR(200))
DECLARE @Password AS NVARCHAR

BULK INSERT TempTableeHubTransactions
FROM #CSVPath#
WITH (FIELDTERMINATOR = ',', ROWTERMINATOR = '\n')

INSERT INTO eHubClient(CC_PK, CC_ID, CC_FriendlyName, CC_Odyssey_OH, CC_EmailAddress, CC_Password, CC_OwnerCategory, CC_SystemCategory) 
SELECT NEWID(), ClientCode+ClientCode+ClientCode, 'LoadTestDummyClients', NEWID(), '', '', 'Client',  'Enterprise' FROM TempTableeHubTransactions

DROP TABLE TempTableeHubTransactions