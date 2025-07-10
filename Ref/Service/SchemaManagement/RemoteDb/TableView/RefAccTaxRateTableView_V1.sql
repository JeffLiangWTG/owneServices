CREATE VIEW RefAccTaxRateTableView_V1 AS
SELECT ZAT_PK,
ZAT_RN_NKCountry,
ZAT_ReferenceRateType,
ZAT_StartDate,
ZAT_EndDate,
ZAT_RateNumerator,
ZAT_RateDenominator
FROM RefAccTaxRate