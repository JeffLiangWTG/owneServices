CREATE TABLE SourceData
(
	SDA_PK uniqueidentifier NOT NULL CONSTRAINT DF_SourceData_SDA_PK DEFAULT NEWID(),
	SDA_Source CHAR(3) NOT NULL,
	SDA_Filename VARCHAR(max) NULL,
	SDA_Filetype VARCHAR(3) NULL,
	SDA_FileHash CHAR(64) NULL,
	SDA_Content VARBINARY(MAX) NULL,
	SDA_ContentText VARCHAR(MAX) NOT NULL CONSTRAINT DF_SourceData_SDA_ContentText DEFAULT '',
	SDA_Status CHAR(3) CONSTRAINT DF_SourceData_SDA_Status DEFAULT 'QUE',
	SDA_CreatedTime datetime2 NOT NULL CONSTRAINT DF_SourceData_SDA_CreatedTime DEFAULT sysutcdatetime(),
	SDA_ContentType CHAR(3) NULL,
	SDA_SourceTime datetime2,
	SDA_SubSource VARCHAR(75) NOT NULL CONSTRAINT DF_SourceData_SDA_SubSource DEFAULT '',
	SDA_NotProcessedUntil datetime2 NULL,
	SDA_Contacts VARCHAR(1000) NULL,
	CONSTRAINT PK_SourceData PRIMARY KEY NONCLUSTERED (SDA_PK),
)
GO

GO
CREATE NONCLUSTERED INDEX IX_SourceData_SDA_SubSource_SDA_Status_SDA_CreatedTime
ON SourceData (SDA_SubSource, SDA_Status, SDA_CreatedTime DESC)
GO

CREATE TRIGGER SourceData_Modify
ON SourceData
FOR UPDATE, INSERT
AS
	INSERT INTO DataChangeCapture(DCC_Column, DCC_EventTimeUTC, DCC_NewValue, DCC_OldValue, DCC_ParentCode, DCC_ParentPK, DCC_PK)
	SELECT 'SDA_Status', SYSUTCDATETIME(), inserted.SDA_Status, deleted.SDA_Status, 'SDA', inserted.SDA_PK, NEWID() FROM inserted
	LEFT JOIN deleted ON deleted.SDA_PK = inserted.SDA_PK
	WHERE inserted.SDA_Status IN ('ERR','MER') AND (deleted.SDA_Status IS NULL OR deleted.SDA_Status <> inserted.SDA_Status)
