CREATE TABLE [RefCusProcedureLanguage]
(
	[ZXV_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_PK] DEFAULT NEWID(),
	[ZXV_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage] DEFAULT '',
	[ZXV_ZZ6_Procedure]	UNIQUEIDENTIFIER NOT NULL,
	[ZXV_Description] NVARCHAR(500) CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_Description] DEFAULT '',
	[ZXV_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZXV_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZXV_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZXV_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZXV_SysStartTime], [ZXV_SysEndTime]),
	CONSTRAINT [CK_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage] CHECK (ZXV_ZX6_NKLanguage <> ''),
	CONSTRAINT [CK_RefCusProcedureLanguage_ZXV_Description] CHECK (ZXV_Description <> ''),
	CONSTRAINT [FK_RefCusProcedureLanguage_ZXV_ZZ6_Procedure] FOREIGN KEY([ZXV_ZZ6_Procedure]) REFERENCES [RefCusProcedure] ([ZZ6_PK]),
	CONSTRAINT [FK_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage] FOREIGN KEY([ZXV_ZX6_NKLanguage]) REFERENCES [RefLanguageType] ([ZX6_Language]),
	CONSTRAINT [PK_RefCusProcedureLanguage] PRIMARY KEY CLUSTERED( [ZXV_PK] ASC ),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusProcedureLanguageHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage_ZXV_ZZ6_Procedure] ON [dbo].[RefCusProcedureLanguage] ([ZXV_ZX6_NKLanguage] ASC,[ZXV_ZZ6_Procedure] ASC)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusProcedureLanguage_ZXV_ZZ6_Procedure] ON [dbo].[RefCusProcedureLanguage] ([ZXV_ZZ6_Procedure] ASC)
GO
ALTER TABLE RefCusProcedureLanguage SET (LOCK_ESCALATION = DISABLE);

