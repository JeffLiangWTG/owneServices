CREATE TABLE [dbo].[DataProcessingInformation]
(
	[DPI_ID] UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DataProcessingInformation_DPI_ID DEFAULT (NEWID()),
	[DPI_Status] varchar(3) NOT NULL,
	[DPI_Message] VARCHAR(MAX) NULL,
	[DPI_SourceId] UNIQUEIDENTIFIER NOT NULL,
	[DPI_ParentTableCode] varchar(3) NOT NULL,
	[DPI_ParentPk] UNIQUEIDENTIFIER NULL,
	[DPI_HasDPRRecordWhenError] BIT NOT NULL CONSTRAINT [DF_DataProcessingInformation_DPI_HasDPRRecordWhenError] DEFAULT 0

	CONSTRAINT [PK_DataProcessingInformation_DPI_ID] PRIMARY KEY NONCLUSTERED (DPI_ID),
	CONSTRAINT [CK_DataProcessingInformation_DPI_ParentPk] CHECK ([DPI_ParentPk] <> '00000000-0000-0000-0000-000000000000'),
)
GO

ALTER TABLE [dbo].[DataProcessingInformation] ADD CONSTRAINT [DF_DPI_Status] DEFAULT 'QUE' FOR [DPI_Status]
GO

ALTER TABLE [dbo].[DataProcessingInformation] ADD CONSTRAINT [DF_DPI_Message] DEFAULT '' FOR [DPI_Message]
GO

CREATE NONCLUSTERED INDEX [UX_DataProcessingInformation_DPI_SourceId__DPI_Status_DPI_ParentPk] ON [DataProcessingInformation] (DPI_SourceId, DPI_Status, DPI_ParentPk)
GO

CREATE NONCLUSTERED INDEX [UX_DataProcessingInformation_DPI_ParentPk] ON [DataProcessingInformation] (DPI_ParentPk)
GO
