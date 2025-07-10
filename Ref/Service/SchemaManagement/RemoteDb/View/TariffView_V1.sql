CREATE VIEW TariffView_V1
	WITH SCHEMABINDING
AS
SELECT ZZ1_PK AS ZZ1_PK
	, ZZ1_PK AS ZZ1_ZZ1_Tariff
	, 'ZZ1' AS ZZ1_TableType
	, ZZ1_ZZI_TariffType
	, ZZ1_TariffCode
	, ZZ1_Description
	, ZZ1_CompositeKeyOnZZ5
	, ZZ1_StartDate
	, ZZ1_EndDate
	, ZZ1_PublishedDate
	, ZZ1_ZZF_NKTaxOrFeeCode
	, ZZ1_ZZZ_NKDataGrouping
FROM dbo.RefCusTariff
UNION ALL
SELECT ZZW_PK ZZ1_PK
	, ZZ1_PK AS ZZ1_ZZ1_Tariff
	, 'ZZW' AS ZZ1_TableType
	, ZZ1_ZZI_TariffType
	, (ZZ1_TariffCode + ZZW_NationalCode) AS ZZ1_TariffCode
	, ZZW_Description ZZ1_Description
	, (ZZ1_CompositeKeyOnZZ5 + ZZW_NationalCode) AS ZZ1_CompositeKeyOnZZ5
	, ZZW_StartDate ZZ1_StartDate
	, ZZW_EndDate ZZ1_EndDate
	, ZZW_PublishedDate ZZ1_PublishedDate
	, ZZW_ZZF_NKTaxOrFeeCode ZZ1_ZZF_NKTaxOrFeeCode
	, ZZW_ZZZ_NKDataGrouping ZZ1_ZZZ_NKDataGrouping
FROM dbo.RefCusTariff
INNER JOIN dbo.RefCusTariffNationalCode ON ZZW_ZZ1_Tariff = ZZ1_PK