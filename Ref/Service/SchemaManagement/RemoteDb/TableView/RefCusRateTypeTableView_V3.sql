CREATE VIEW RefCusRateTypeTableView_V3 AS
SELECT ZZR_PK,
ZZR_RateType,
ZZR_Description,
ZZR_IsPayable,
ZZR_ZZZ_NKDataGrouping,
ZZR_RX_NKFormulaCurrency,
ZZR_CustomsValueFormula,
ZZR_IsExport
FROM RefCusRateType
