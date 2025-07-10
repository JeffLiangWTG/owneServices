CREATE TABLE [dbo].[RefComplianceList]
(
   [RCL_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_PK] DEFAULT NEWID(),
   [RCL_IsActive] BIT NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_IsActive] DEFAULT 1,
   [RCL_ListCode] VARCHAR(30) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_ListCode] DEFAULT '',
   [RCL_ListName] NVARCHAR(120) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_ListName] DEFAULT '',
   [RCL_ListDescription] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_ListDescription] DEFAULT '',
   [RCL_ListPublisher] NVARCHAR(120) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_ListPublisher] DEFAULT '',
   [RCL_ListType] VARCHAR(30) NOT NULL CONSTRAINT [DF_RefComplianceList_ListType] DEFAULT '',
   [RCL_PublisherJurisdiction] VARCHAR(30) NOT NULL CONSTRAINT [DF_RefComplianceList_PublisherJurisdiction] DEFAULT '',
   [RCL_PublisherDescription] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefComplianceList_PublisherDescription] DEFAULT '',
   [RCL_IntegrationDate] DATE NULL,
   [RCL_LastUpdatedDate] DATE NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_LastUpdatedDate] DEFAULT '2021-06-30',
   [RCL_MainSourceURL] NVARCHAR(254) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_MainSourceURL] DEFAULT '',
   [RCL_SecondarySourceURL] NVARCHAR(254) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_SecondarySourceURL] DEFAULT '',
   [RCL_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_RefComplianceList_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
   [RCL_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_RefComplianceList_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
   PERIOD FOR SYSTEM_TIME ([RCL_SysStartTime], [RCL_SysEndTime]),
   CONSTRAINT [PK_RefComplianceList] PRIMARY KEY CLUSTERED ([RCL_PK] ASC)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefComplianceListHistory))
GO 
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefComplianceList_RCL_ListCode] ON [RefComplianceList] ([RCL_ListCode] ASC)
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefComplianceList_RCL_ListName] ON [RefComplianceList] ([RCL_ListName] ASC)
GO
ALTER TABLE RefComplianceList SET (LOCK_ESCALATION = DISABLE);
