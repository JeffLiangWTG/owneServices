CREATE TABLE [dbo].[Authentication]
(
	[AT_SystemID] VARCHAR(7) NOT NULL PRIMARY KEY, 
    [AT_Password] VARCHAR(200) NOT NULL, 
    [AT_EnterpriseCode] VARCHAR(3) NOT NULL, 
    [AT_ServerCode] VARCHAR(3) NOT NULL, 
    [AT_SystemCreateTimeUTC] SMALLDATETIME NULL, 
    [AT_SystemLastModifiedTimeUTC] SMALLDATETIME NULL 
)
GO

ALTER TABLE [dbo].[Authentication]
SET
(
	LOCK_ESCALATION = DISABLE
)
GO

CREATE NONCLUSTERED INDEX NR_RX_AU_AT_EnterpriseCode_AT_ServerCode_AT_Password
ON [dbo].[Authentication] (AT_EnterpriseCode, AT_ServerCode, AT_Password)
GO

CREATE TRIGGER [dbo].[Trigger_CreateTimeUTC]
    ON [dbo].[Authentication]
    FOR INSERT
    AS
    BEGIN
        UPDATE [dbo].[Authentication]
			SET AT_SystemCreateTimeUTC = SYSUTCDATETIME()
			FROM inserted i
			WHERE [dbo].[Authentication].AT_SystemID = i.AT_SystemID
    END
GO

CREATE TRIGGER [dbo].[Trigger_LastModifiedTimeUTC]
    ON [dbo].[Authentication]
    FOR UPDATE
    AS
    BEGIN
        UPDATE [dbo].[Authentication]
			SET AT_SystemLastModifiedTimeUTC = SYSUTCDATETIME()
			FROM inserted i
			WHERE [dbo].[Authentication].AT_SystemID = i.AT_SystemID
    END
GO