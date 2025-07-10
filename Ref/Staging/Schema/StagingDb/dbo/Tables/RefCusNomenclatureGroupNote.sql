CREATE TABLE RefCusNomenclatureGroupNote(
	ZZL_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusNomenclatureGroupNote_ZZL_PK DEFAULT (NEWID()),
	ZZL_ZZ5_NomenclatureGroup UNIQUEIDENTIFIER NOT NULL,
	ZZL_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,
	ZZL_ZX6_NKLanguage VARCHAR(3) NOT NULL,
	ZZL_NoteType VARCHAR(3) NOT NULL,
	ZZL_Note NVARCHAR(MAX) NOT NULL,
	CONSTRAINT PK_RefCusNomenclatureGroupNote PRIMARY KEY CLUSTERED (ZZL_PK ASC),
	CONSTRAINT FK_RefCusNomenclatureGroupNote_RefCusNomenclatureGroup FOREIGN KEY (ZZL_ZZ5_NomenclatureGroup) REFERENCES RefCusNomenclatureGroup (ZZ5_PK),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusNomenclatureGroupNote_ZZL_ZZ5_NomenclatureGroup] ON [RefCusNomenclatureGroupNote] ([ZZL_ZZ5_NomenclatureGroup])
