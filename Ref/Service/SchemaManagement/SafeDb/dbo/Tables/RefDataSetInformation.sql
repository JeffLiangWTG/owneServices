CREATE TABLE [dbo].[RefDataSetInformation]
(
	RDS_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefDataSetInformation_RDS_PK DEFAULT (NEWID()),
	RDS_DataSetId SMALLINT NOT NULL,
	RDS_TableName VARCHAR(50) NOT NULL,
	RDS_DataSetName NVARCHAR(50) NOT NULL,
	RDS_DataSetTableCode VARCHAR(3) NOT NULL,
	RDS_PriorityLevel SMALLINT NOT NULL,
	RDS_LastUpdatedUTC DATETIME2,
	RDS_IsPush BIT NOT NULL CONSTRAINT DF_RefDataSetInformation_RDS_IsPush DEFAULT 0,

	CONSTRAINT PK_RefDataSetInformation PRIMARY KEY CLUSTERED( RDS_PK ASC ),
	CONSTRAINT CK_RefDataSetInformation_RVC_DataSetId CHECK (RDS_DataSetId > 0),
	CONSTRAINT CK_RefDataSetInformation_RVC_DataSetName CHECK (RDS_DataSetName <> '' AND CHARINDEX(' ',RDS_DataSetName) < 1),
	CONSTRAINT CK_RefDataSetInformation_RDS_DataSetTableCode CHECK (RDS_DataSetTableCode <> ''),
	CONSTRAINT CK_RefDataSetInformation_RDS_TableName CHECK (RDS_TableName <> ''),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefDataSetInformation_RDS_DataSetId ON RefDataSetInformation (RDS_DataSetId)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefDataSetInformation_RDS_DataSetTableCode_RDS_PriorityLevel ON RefDataSetInformation (RDS_DataSetTableCode, RDS_PriorityLevel)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefDataSetInformation_RDS_DataSetName ON RefDataSetInformation (RDS_DataSetName)
GO
CREATE NONCLUSTERED INDEX IX_RefDataSetInformation_RDS_DataSetId_RDS_IsPush ON RefDataSetInformation (RDS_DataSetId, RDS_IsPush)
GO
ALTER TABLE RefDataSetInformation SET (LOCK_ESCALATION = DISABLE);
