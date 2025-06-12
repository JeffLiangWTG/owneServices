IF (EXISTS (SELECT * 
                 FROM INFORMATION_SCHEMA.TABLES 
                 WHERE TABLE_NAME = 'TempTableAuthentication'))
BEGIN
    DROP TABLE TempTableAuthentication
END

CREATE TABLE TempTableAuthentication (ClientCode VARCHAR(3), ClientPass VARCHAR(200))
DECLARE @Password AS NVARCHAR

BULK INSERT TempTableAuthentication
FROM #CSVPath#
WITH (FIELDTERMINATOR = ',', ROWTERMINATOR = '\n')

INSERT INTO Authentication(AT_EnterpriseCode, AT_ServerCode, AT_SystemID, AT_Password) 
SELECT ClientCode, ClientCode, ClientCode, (select CONVERT(VARCHAR(128), HASHBYTES('SHA2_512', CONVERT(VARCHAR(128), [dbo].[Base27Decode](ClientCode)) + ClientPass), 2)) FROM TempTableAuthentication

DROP TABLE TempTableAuthentication