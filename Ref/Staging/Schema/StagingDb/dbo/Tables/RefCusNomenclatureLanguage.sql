CREATE TABLE RefCusNomenclatureLanguage
(
	ZX8_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusNomenclatureLanguage_ZX8_PK DEFAULT(NEWID()),
	ZX8_ZX6_NKLanguage VARCHAR(3) NOT NULL,
	ZX8_ZZ5_NomenclatureGroup UNIQUEIDENTIFIER NOT NULL,
	ZX8_Description NVARCHAR(MAX) NOT NULL CONSTRAINT DF_RefCusNomenclatureLanguage_ZX8_Description DEFAULT(''),
	CONSTRAINT PK_RefCusNomenclatureLanguage PRIMARY KEY CLUSTERED (ZX8_PK ASC),
	CONSTRAINT FK_RefCusNomenclatureLanguage_RefCusNomenclatureGroup FOREIGN KEY(ZX8_ZZ5_NomenclatureGroup) REFERENCES RefCusNomenclatureGroup (ZZ5_PK),
)
GO
CREATE NONCLUSTERED INDEX [IX_RefCusNomenclatureLanguage_ZX8_ZZ5_NomenclatureGroup] ON [RefCusNomenclatureLanguage] ([ZX8_ZZ5_NomenclatureGroup])
