CREATE TABLE [dbo].[UniversalXmlSchema]
(
	[XSD_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_UniversalXmlSchema_XSD_PK] DEFAULT (newid()),
	[XSD_SchemaName] varchar(255) NOT NULL,
	[XSD_SchemaVersion] int NOT NULL,
	[XSD_TimeStamp] datetime NOT NULL CONSTRAINT [DF_UniversalXmlSchema_XSD_TimeStamp] DEFAULT (getdate()),
	[XSD_SchemaMD5HashCode] VARCHAR(32) NOT NULL,
	[XSD_SchemaContent] XML NOT NULL,
	CONSTRAINT [PK_UniversalXmlSchema] PRIMARY KEY NONCLUSTERED ([XSD_PK] ASC),
)
GO

CREATE UNIQUE INDEX [IX_XSD_SchemaName_XSD_SchemaVersion] ON [dbo].[UniversalXmlSchema] ([XSD_SchemaName], [XSD_SchemaVersion])
GO
