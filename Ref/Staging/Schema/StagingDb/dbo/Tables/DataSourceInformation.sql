CREATE TABLE [dbo].[DataSourceInformation]
(
	DSI_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_DataSourceInformation_DSI_PK DEFAULT (NEWID()),
	DSI_SubSource VARCHAR(75) NOT NULL CONSTRAINT DF_DataSourceInformation_SubSource DEFAULT '',
	DSI_EnableAutoExpiration BIT NOT NULL CONSTRAINT DF_DataSourceInformation_EnableAUtoExpiration DEFAULT 0,
	CONSTRAINT [PK_DataSourceInformation_DPR_PK] PRIMARY KEY CLUSTERED (DSI_PK)
)
GO
CREATE UNIQUE NONCLUSTERED INDEX UX_DataSourceInformation_DSI_SubSource ON DataSourceInformation (DSI_SubSource)
