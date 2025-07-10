CREATE TABLE RefCusTariffType
(
	ZZI_PK UNIQUEIDENTIFIER NOT NULL CONSTRAINT DF_RefCusTariffType_ZZI_PK DEFAULT (NEWID()),
	ZZI_TariffType VARCHAR(5) NOT NULL,
	ZZI_Description NVARCHAR(100) NOT NULL,
	ZZI_ZZ9_NKNomenclatureGroupType VARCHAR(3) NOT NULL CONSTRAINT DF_RefCusTariffType_ZZI_ZZ9_NKNomenclatureGroupType DEFAULT (''),
	ZZI_ZZZ_NKDataGrouping VARCHAR(3) NOT NULL,
	CONSTRAINT PK_RefCusTariffType PRIMARY KEY CLUSTERED( ZZI_PK ASC ),
	CONSTRAINT CK_RefCusTariffType_ZZI_TariffType CHECK (ZZI_TariffType <>''),
	CONSTRAINT CK_RefCusTariffType_ZZI_Description CHECK (ZZI_Description <>''),
	CONSTRAINT CK_RefCusTariffType_ZZI_ZZZ_NKDataGrouping CHECK (ZZI_ZZZ_NKDataGrouping <>'')
)
GO
CREATE UNIQUE NONCLUSTERED INDEX IX_RefCusTariffType_ZZI_ZZZ_NKDataGrouping_ZZI_TariffType ON RefCusTariffType (ZZI_ZZZ_NKDataGrouping ASC, ZZI_TariffType ASC)
GO
