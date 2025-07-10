CREATE TABLE [dbo].[UserAuthorization]
(
	UA_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_UserAuthorization_UA_PK DEFAULT (NEWID()),
	UA_User VARCHAR(50) NOT NULL,
	UA_DataSetName VARCHAR(200) NOT NULL,
	UA_TableName VARCHAR(50) NOT NULL,
	UA_ColumnName VARCHAR(50) NOT NULL,
	UA_ColumnValue VARCHAR(200) NOT NULL,
	CONSTRAINT PK_UserAuthorization PRIMARY KEY CLUSTERED (UA_PK),
	CONSTRAINT CK_UserAuthorization_UA_User CHECK (UA_User <> ''),
	CONSTRAINT CK_UserAuthorization_UA_DataSetName CHECK (UA_DataSetName <> ''),
	CONSTRAINT CK_UserAuthorization_UA_TableName CHECK (UA_TableName <> '')
)
GO
CREATE NONCLUSTERED INDEX IX_UserAuthorization_UA_User_UA_TableName ON UserAuthorization (UA_User, UA_TableName)
GO
ALTER TABLE UserAuthorization SET (LOCK_ESCALATION = DISABLE);
