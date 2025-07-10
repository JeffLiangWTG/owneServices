CREATE TABLE [RefCusProcedureLanguage]
(
	[ZXV_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_PK] DEFAULT NEWID(),
	[ZXV_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage] DEFAULT '',
	[ZXV_ZZ6_Procedure]	UNIQUEIDENTIFIER NOT NULL,
	[ZXV_Description] NVARCHAR(500) CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_Description] DEFAULT '',
	CONSTRAINT [FK_RefCusProcedureLanguage_ZXV_ZZ6_Procedure] FOREIGN KEY([ZXV_ZZ6_Procedure]) REFERENCES [RefCusProcedure] ([ZZ6_PK]),
	CONSTRAINT [PK_RefCusProcedureLanguage] PRIMARY KEY CLUSTERED( [ZXV_PK] ASC ),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusProcedureLanguage_ZXV_ZZ6_Procedure] ON [dbo].[RefCusProcedureLanguage] ([ZXV_ZZ6_Procedure] ASC)
GO
