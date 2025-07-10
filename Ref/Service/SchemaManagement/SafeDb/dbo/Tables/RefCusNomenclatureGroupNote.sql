CREATE TABLE RefCusNomenclatureGroupNote(
	[ZZL_PK] UNIQUEIDENTIFIER NOT NULL CONSTRAINT [DF_RefCusNomenclatureGroupNote_ZZL_PK] DEFAULT (NEWID()),
	[ZZL_ZZ5_NomenclatureGroup] UNIQUEIDENTIFIER NOT NULL,
	[ZZL_ZZZ_NKDataGrouping] VARCHAR(3) NOT NULL,
	[ZZL_ZX6_NKLanguage] VARCHAR(3) NOT NULL,
	[ZZL_NoteType] VARCHAR(3) NOT NULL,
	[ZZL_Note] NVARCHAR(MAX) NOT NULL,
	[ZZL_SysStartTime] DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL CONSTRAINT [DF_ZZL_SysStartTime] DEFAULT CONVERT(DATETIME2, '1900-01-01 0:0:0.0000000'),
	[ZZL_SysEndTime] DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL CONSTRAINT [DF_ZZL_SysEndTime] DEFAULT CONVERT(DATETIME2, '9999-12-31 23:59:59.9999999'),
	PERIOD FOR SYSTEM_TIME ([ZZL_SysStartTime], [ZZL_SysEndTime]),
	CONSTRAINT [PK_RefCusNomenclatureGroupNote] PRIMARY KEY CLUSTERED ([ZZL_PK] ASC),
	CONSTRAINT [FK_RefCusNomenclatureGroupNote_RefCusNomenclatureGroup] FOREIGN KEY ([ZZL_ZZ5_NomenclatureGroup]) REFERENCES RefCusNomenclatureGroup ([ZZ5_PK]),
	CONSTRAINT [FK_RefCusNomenclatureGroupNote_RefLanguageType] FOREIGN KEY ([ZZL_ZX6_NKLanguage]) REFERENCES RefLanguageType ([ZX6_Language]),
	CONSTRAINT [CK_RefCusNomenclatureGroupNote_ZZL_ZZZ_NKDataGrouping] CHECK ([ZZL_ZZZ_NKDataGrouping] <> ''),
	CONSTRAINT [FK_RefCusNomenclatureGroupNote_RefDataGrouping]  FOREIGN KEY ([ZZL_ZZZ_NKDataGrouping]) REFERENCES RefDataGrouping ([ZZZ_DataGrouping]),
	CONSTRAINT [CK_RefCusNomenclatureGroupNote_ZZL_ZX6_NKLanguage] CHECK ([ZZL_ZX6_NKLanguage] <> ''),
	CONSTRAINT [CK_RefCusNomenclatureGroupNote_ZZL_NoteType] CHECK ([ZZL_NoteType] <> ''),
	CONSTRAINT [CK_RefCusNomenclatureGroupNote_ZZL_Note] CHECK ([ZZL_Note] <> ''),
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.RefCusNomenclatureGroupNoteHistory))
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusNomenclatureGroupNote_ZZL_ZZ5_NomenclatureGroup_ZZL_ZZZ_NKDataGrouping_ZZL_ZX6_NKLanguage_ZZL_NoteType ON RefCusNomenclatureGroupNote (ZZL_ZZ5_NomenclatureGroup ASC, ZZL_ZZZ_NKDataGrouping ASC, ZZL_ZX6_NKLanguage ASC, ZZL_NoteType ASC)
GO
ALTER TABLE RefCusNomenclatureGroupNote SET (LOCK_ESCALATION = DISABLE);
