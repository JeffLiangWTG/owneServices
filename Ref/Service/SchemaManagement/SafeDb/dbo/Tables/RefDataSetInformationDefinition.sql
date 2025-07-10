CREATE TABLE [dbo].[RefDataSetInformationDefinition]
(
	RDD_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefDataSetInformationDefinition_RDD_PK DEFAULT (NEWID()),
	RDD_DataSetId smallint NOT NULL,
	RDD_ColumnName VARCHAR(50) NOT NULL,
	RDD_ColumnValue VARCHAR(200) NOT NULL,
	CONSTRAINT PK_RefDataSetInformationDefinition PRIMARY KEY CLUSTERED (RDD_PK),
	CONSTRAINT CK_RefDataSetInformationDefinition_RDD_DataSetId CHECK (RDD_DataSetId > 0),

	CONSTRAINT FK_RefDataSetInformationDefinition_RDD_DataSetId_RefDataSetInformation_RDS_DataSetId FOREIGN KEY(RDD_DataSetId)REFERENCES RefDataSetInformation (RDS_DataSetId),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefDataSetInformationDefinition_RDD_DataSetId_RDD_ColumnName_RDD_ColumnValue ON RefDataSetInformationDefinition (RDD_DataSetId, RDD_ColumnName, RDD_ColumnValue)
GO
ALTER TABLE RefDataSetInformationDefinition SET (LOCK_ESCALATION = DISABLE);
