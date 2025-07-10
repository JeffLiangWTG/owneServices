CREATE TABLE RefCusRateTypeLanguage
(
[ZXT_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusRateTypeLanguage_ZXT_PK] DEFAULT (NEWID()),
[ZXT_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
[ZXT_ZZR_RateType] UNIQUEIDENTIFIER NOT NULL,
[ZXT_Description] NVARCHAR(MAX) NOT NULL,

CONSTRAINT [PK_RefCusRateTypeLanguage] PRIMARY KEY ([ZXT_PK]),
CONSTRAINT [FK_RefCusRateTypeLanguage_RefCusRateType] FOREIGN KEY ([ZXT_ZZR_RateType]) REFERENCES [RefCusRateType]([ZZR_PK]),
CONSTRAINT [FK_RefCusRateTypeLanguage_RefLanguageType] FOREIGN KEY ([ZXT_ZX6_NKLanguage]) REFERENCES [RefLanguageType]([ZX6_Language]),
CONSTRAINT [CK_RefCusRateTypeLanguage_ZXT_Description] CHECK ([ZXT_Description] <> ''),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusRateTypeLanguage_ZXT_ZX6_NKLanguage_ZXT_ZZR_RateType ON RefCusRateTypeLanguage(ZXT_ZX6_NKLanguage ASC, ZXT_ZZR_RateType ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusRateTypeLanguage_ZXT_ZZR_RateType ON RefCusRateTypeLanguage(ZXT_ZZR_RateType ASC)
GO
