CREATE VIEW RefCusVATApplicabilityTableView_V1 AS
SELECT ZX5_PK,
ZX5_ZZ1_Tariff,
ZX5_ZZW_TariffNationalCode,
SUBSTRING(ZX5_ZZF_NKTaxOrFeeCode, 1, 3) AS ZX5_ZZF_NKTaxOrFeeCode,
ZX5_StartDate,
ZX5_EndDate,
ZX5_AdditionalCode,
ZX5_Description,
ZX5_ZZZ_NKDataGrouping,
ZX5_ZZA_TradeGroup,
ZX5_VATCategory
FROM RefCusVATApplicability
