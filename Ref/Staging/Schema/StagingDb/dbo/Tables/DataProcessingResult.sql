CREATE TABLE [dbo].[DataProcessingResult]
(
	DPR_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DataProcessingResult_DPI_PK DEFAULT (NEWID()),
	DPR_SubSource VARCHAR(75) NOT NULL CONSTRAINT DF_DataProcessingResult_SubSource DEFAULT '',
	DPR_PublicationTime DATETIME2 NOT NULL,
	DPR_ParentTableCode VARCHAR(3) NOT NULL,
	DPR_ParentPK UNIQUEIDENTIFIER NOT NULL,
	DPR_ExpirableAncestorPK UNIQUEIDENTIFIER NULL,
	DPR_DatasetPK UNIQUEIDENTIFIER NULL,
	DPR_Status VARCHAR(3) NOT NULL CONSTRAINT DF_DataProcessingResult_Status DEFAULT 'QUE',
	DPR_ExpirationTime DATETIME2 NULL,
	CONSTRAINT [PK_DataProcessingResult_DPR_PK] PRIMARY KEY CLUSTERED (DPR_PK)
)
GO

CREATE NONCLUSTERED INDEX [UX_DataProcessingResult_DPR_ParentPK] ON [DataProcessingResult] (DPR_ParentPK)
GO
CREATE NONCLUSTERED INDEX [IX_DataProcessingResult_DPR_SubSource_DPR_ParentPK_DPR_PublicationTime] ON [DataProcessingResult] (DPR_SubSource, DPR_ParentPK, DPR_PublicationTime DESC) INCLUDE (DPR_PK)
GO
CREATE NONCLUSTERED INDEX [UX_DataProcessingResult_DPR_SubSource_DPR_ExpirableAncestorPK_DPR_PublicationTime] ON [DataProcessingResult] (DPR_SubSource, DPR_ExpirableAncestorPK, DPR_PublicationTime)
GO
