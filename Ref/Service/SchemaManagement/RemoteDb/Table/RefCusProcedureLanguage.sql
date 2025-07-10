CREATE TABLE RefCusProcedureLanguage
(
[ZXV_PK] UNIQUEIDENTIFIER NOT NULL,
[ZXV_ZZ6_Procedure] UNIQUEIDENTIFIER NOT NULL,
[ZXV_ZX6_NKLanguage] VARCHAR(3) NOT NULL CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage] DEFAULT '',
[ZXV_Description] NVARCHAR(500) NOT NULL CONSTRAINT [DF_RefCusProcedureLanguage_ZXV_Description] DEFAULT '',

CONSTRAINT [PK_RefCusProcedureLanguage] PRIMARY KEY (ZXV_PK),
CONSTRAINT [FK_RefCusProcedureLanguage_ZXV_ZZ6_Procedure] FOREIGN KEY ([ZXV_ZZ6_Procedure]) REFERENCES [RefCusProcedure] ([ZZ6_PK]),
CONSTRAINT [FK_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage] FOREIGN KEY(ZXV_ZX6_NKLanguage) REFERENCES [RefLanguageType] ([ZX6_Language])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusProcedureLanguage_ZXV_ZX6_NKLanguage_ZXV_ZZ6_Procedure ON RefCusProcedureLanguage(ZXV_ZX6_NKLanguage, ZXV_ZZ6_Procedure)
GO
CREATE NONCLUSTERED INDEX IX_RefCusProcedureLanguage_ZXV_ZZ6_Procedure ON RefCusProcedureLanguage(ZXV_ZZ6_Procedure)
GO
