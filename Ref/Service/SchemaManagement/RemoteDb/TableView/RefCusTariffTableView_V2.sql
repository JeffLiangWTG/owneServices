CREATE VIEW RefCusTariffTableView_V2 AS
SELECT ZZ1_PK,
ZZ1_ZZI_TariffType,
ZZ1_TariffCode,
ZZ1_IAMUnique,
ZZ1_Description,
ZZ1_StartDate,
ZZ1_EndDate,
ZZ1_PublishedDate,
ZZ1_ZZF_NKTaxOrFeeCode,
ZZ1_ZZZ_NKDataGrouping,
ZZ1_CompositeKeyOnZZ5
FROM RefCusTariff
