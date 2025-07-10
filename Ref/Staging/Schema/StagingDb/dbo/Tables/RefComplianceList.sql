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
   [RCL_MainSourceURL] NVARCHAR(254) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_MainSourceURL] DEFAULT '',
   [RCL_SecondarySourceURL] NVARCHAR(254) NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_SecondarySourceURL] DEFAULT '',
   [RCL_LastUpdatedDate] DATE NOT NULL CONSTRAINT [DF_RefComplianceList_RCL_LastUpdatedDate] DEFAULT '2021-06-30',
   [RCL_IntegrationDate] DATE NULL,
   CONSTRAINT [PK_RefComplianceList] PRIMARY KEY CLUSTERED ([RCL_PK] ASC)
)
