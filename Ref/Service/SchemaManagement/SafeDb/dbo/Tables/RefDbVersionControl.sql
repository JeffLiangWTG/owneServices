CREATE TABLE RefDbVersionControl
(
	RVC_ParentPK uniqueidentifier NOT NULL,
	RVC_ParentCode VARCHAR(3) NOT NULL,
	RVC_LastUpdatedUTC datetime2 NULL,
	RVC_Deleted BIT NOT NULL CONSTRAINT DF_RefDbVersionControl_RVC_Deleted DEFAULT 0,
	RVC_CreatedTimeUTC datetime2 NOT NULL CONSTRAINT DF_RefDbVersionControl_RVC_CreatedTimeUTC DEFAULT SYSUTCDATETIME(),
	RVC_LastEditedUser VARCHAR(50) CONSTRAINT DF_RefDbVersionControl_RVC_LastEditedUser DEFAULT dbo.GetUserId(),
	RVC_IsPublished BIT NOT NULL CONSTRAINT DF_DefDbVersionControl_RVC_IsPublished DEFAULT 0,
	RVC_DataSetId SMALLINT NULL,
	CONSTRAINT [PK_RefDbVersionControl] PRIMARY KEY CLUSTERED( [RVC_ParentPK] ASC ),

	CONSTRAINT FK_RefDbVersionControl_RVC_DataSetId_RefDataSetInformation_RDS_DataSetId FOREIGN KEY(RVC_DataSetId) REFERENCES RefDataSetInformation(RDS_DataSetId)
)
GO
CREATE NONCLUSTERED INDEX IX_RefDbVersionControl_RVC_LastUpdatedUTC ON RefDbVersionControl (RVC_LastUpdatedUTC ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefDbVersionControl_RVC_CreatedTimeUTC ON RefDbVersionControl (RVC_CreatedTimeUTC ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefDbVersionControl_RVC_ParentCode_RVC_IsPublished ON RefDbVersionControl (RVC_ParentCode, RVC_IsPublished) INCLUDE (RVC_DataSetId)
GO
CREATE NONCLUSTERED INDEX IX_RefDbVersionControl_RVC_IsPublished_RVC_DataSetId_RVC_LastUpdatedUTC ON RefDbVersionControl (RVC_IsPublished, RVC_DataSetId, RVC_LastUpdatedUTC) INCLUDE (RVC_Deleted)
GO
ALTER TABLE RefDbVersionControl SET (LOCK_ESCALATION = DISABLE);
