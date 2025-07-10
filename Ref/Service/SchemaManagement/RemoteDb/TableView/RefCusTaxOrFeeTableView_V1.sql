CREATE VIEW RefCusTaxOrFeeTableView_V1 AS
SELECT ZZF_PK,
ZZF_Code,
ZZF_Description,
ZZF_Value,
ZZF_StartDate,
ZZF_EndDate,
ZZF_ZZZ_NKDataGrouping,
ZZF_Minimum,
ZZF_Maximum,
ZZF_Threshold,
ZZF_ZX0_NKTaxOrFeeType
FROM RefCusTaxOrFee