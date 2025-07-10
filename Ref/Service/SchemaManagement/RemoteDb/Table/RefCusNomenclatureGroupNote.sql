CREATE TABLE RefCusNomenclatureGroupNote
(
	ZZL_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusNomenclatureGroupNote_ZZL_PK DEFAULT (NEWID()),
	ZZL_ZZ5_NomenclatureGroup UNIQUEIDENTIFIER NOT NULL,
	ZZL_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,
	ZZL_ZX6_NKLanguage VARCHAR(3) NOT NULL,
	ZZL_NoteType VARCHAR(3) NOT NULL,
	ZZL_Note NVARCHAR(MAX) NOT NULL,
	CONSTRAINT PK_RefCusNomenclatureGroupNote PRIMARY KEY CLUSTERED (ZZL_PK ASC),
	CONSTRAINT FK_RefCusNomenclatureGroupNote_RefCusNomenclatureGroup FOREIGN KEY (ZZL_ZZ5_NomenclatureGroup) REFERENCES RefCusNomenclatureGroup (ZZ5_PK) ON DELETE CASCADE,
	CONSTRAINT FK_RefCusNomenclatureGroupNote_RefLanguageType FOREIGN KEY (ZZL_ZX6_NKLanguage) REFERENCES RefLanguageType (ZX6_Language) ON DELETE CASCADE,
	CONSTRAINT CK_RefCusNomenclatureGroupNote_ZZL_ZZZ_NKDataGrouping CHECK (ZZL_ZZZ_NKDataGrouping <> ''),
	CONSTRAINT FK_RefCusNomenclatureGroupNote_RefDataGrouping  FOREIGN KEY (ZZL_ZZZ_NKDataGrouping) REFERENCES RefDataGrouping (ZZZ_DataGrouping),
	CONSTRAINT CK_RefCusNomenclatureGroupNote_ZZL_ZX6_NKLanguage CHECK (ZZL_ZX6_NKLanguage <> ''),
	CONSTRAINT CK_RefCusNomenclatureGroupNote_ZZL_NoteType CHECK (ZZL_NoteType <> ''),
	CONSTRAINT CK_RefCusNomenclatureGroupNote_ZZL_Note CHECK (ZZL_Note <> ''),
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusNomenclatureGroupNote_ZZL_ZZ5_NomenclatureGroup_ZZL_ZZZ_NKDataGrouping_ZZL_ZX6_NKLanguage_ZZL_NoteType ON RefCusNomenclatureGroupNote (ZZL_ZZ5_NomenclatureGroup ASC, ZZL_ZZZ_NKDataGrouping ASC, ZZL_ZX6_NKLanguage ASC, ZZL_NoteType ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusNomenclatureGroupNote_ZZL_ZX6_NKLanguage ON RefCusNomenclatureGroupNote (ZZL_ZX6_NKLanguage ASC)
GO
CREATE NONCLUSTERED INDEX IX_RefCusNomenclatureGroupNote_ZZL_ZZZ_NKDataGrouping ON RefCusNomenclatureGroupNote (ZZL_ZZZ_NKDataGrouping ASC)
GO