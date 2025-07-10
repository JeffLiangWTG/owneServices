CREATE TABLE RefCusTariffTypeLanguage(
	[ZXK_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusTariffTypeLanguage_ZXK_PK] DEFAULT (NEWID()),
	[ZXK_ZZI_TariffType] UNIQUEIDENTIFIER NOT NULL,
	[ZXK_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
	[ZXK_Description] NVARCHAR(MAX) NOT NULL,

	CONSTRAINT [PK_RefCusTariffTypeLanguage] PRIMARY KEY CLUSTERED([ZXK_PK] ASC),
	CONSTRAINT [FK_RefCusTariffTypeLanguage_RefCusTariffType] FOREIGN KEY ([ZXK_ZZI_TariffType]) REFERENCES RefCusTariffType ([ZZI_PK]),
	CONSTRAINT [FK_RefCusTariffTypeLanguage_RefLanguageType] FOREIGN KEY ([ZXK_ZX6_NKLanguage]) REFERENCES RefLanguageType ([ZX6_Language]),
	CONSTRAINT [CK_RefCusTariffTypeLanguage_ZXK_Description] CHECK ([ZXK_Description] <> '')
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffTypeLanguage_ZXK_ZZI_TariffType_ZXK_ZX6_NKLanguage ON RefCusTariffTypeLanguage (ZXK_ZZI_TariffType ASC, ZXK_ZX6_NKLanguage ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffTypeLanguage_ZXK_ZZI_TariffType ON RefCusTariffTypeLanguage(ZXK_ZZI_TariffType ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusTariffTypeLanguage_ZXK_ZX6_NKLanguage ON RefCusTariffTypeLanguage(ZXK_ZX6_NKLanguage ASC)
GO
