CREATE TABLE RefCusRateCodeLanguage
(
[ZXC_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusRateCodeLanguage_ZXC_PK] DEFAULT (NEWID()),
[ZXC_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
[ZXC_ZY1_RateCode] UNIQUEIDENTIFIER NOT NULL,
[ZXC_Description] NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCusRateCodeLanguage_ZXC_Description DEFAULT '',

CONSTRAINT [PK_RefCusRateCodeLanguage] PRIMARY KEY ([ZXC_PK]),
CONSTRAINT [FK_RefCusRateCodeLanguage_RefCusRateCode] FOREIGN KEY ([ZXC_ZY1_RateCode]) REFERENCES [RefCusRateCode]([ZY1_PK]),
CONSTRAINT [FK_RefCusRateCodeLanguage_RefLanguageType] FOREIGN KEY ([ZXC_ZX6_NKLanguage]) REFERENCES [RefLanguageType]([ZX6_Language]),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusRateCodeLanguage_ZXC_ZX6_NKLanguage_ZXC_ZY1_RateCode ON RefCusRateCodeLanguage(ZXC_ZX6_NKLanguage, ZXC_ZY1_RateCode)
GO
CREATE NONCLUSTERED INDEX IX_RefCusRateCodeLanguage_ZXC_ZY1_RateCode ON RefCusRateCodeLanguage(ZXC_ZY1_RateCode)
GO
