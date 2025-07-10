CREATE VIEW RefCusTariffTableView_V1 AS
SELECT ZZ1_PK,
ZZ1_ZZI_TariffType,
ZZ1_TariffCode,
ZZ1_IAMUnique,
ZZ1_Description,
ZZ1_StartDate,
ZZ1_EndDate,
ZZ1_PublishedDate,
SUBSTRING(ZZ1_ZZF_NKTaxOrFeeCode, 1, 3) AS ZZ1_ZZF_NKTaxOrFeeCode,
ZZ1_ZZZ_NKDataGrouping,
ZZ1_CompositeKeyOnZZ5
FROM RefCusTariff
