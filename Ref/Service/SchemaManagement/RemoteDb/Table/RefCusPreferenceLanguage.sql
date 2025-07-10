CREATE TABLE RefCusPreferenceLanguage
(
[ZX9_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusPreferenceLanguage_ZX9_PK] DEFAULT (NEWID()),
[ZX9_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
[ZX9_ZZS_Preference] UNIQUEIDENTIFIER NOT NULL,
[ZX9_Description] NVARCHAR(MAX) NOT NULL CONSTRAINT [DF_RefCusPreferenceLanguage_ZX9_Description] DEFAULT(''),

CONSTRAINT [PK_RefCusPreferenceLanguage] PRIMARY KEY CLUSTERED ([ZX9_PK] ASC),
CONSTRAINT [FK_RefCusPreferenceLanguage_RefLanguageType] FOREIGN KEY([ZX9_ZX6_NKLanguage]) REFERENCES [RefLanguageType] ([ZX6_Language]),
CONSTRAINT [FK_RefCusPreferenceLanguage_RefCusPreference] FOREIGN KEY([ZX9_ZZS_Preference]) REFERENCES [RefCusPreference] ([ZZS_PK])
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusPreferenceLanguage_ZX9_ZX6_NKLanguage_ZX9_ZZS_Preference ON RefCusPreferenceLanguage (ZX9_ZX6_NKLanguage, ZX9_ZZS_Preference)
GO
CREATE NONCLUSTERED INDEX IX_RefCusPreferenceLanguage_ZX9_ZZS_Preference ON RefCusPreferenceLanguage (ZX9_ZZS_Preference)
GO
