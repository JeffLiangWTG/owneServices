CREATE VIEW RefExchangeRateZZTableView_V3 AS
SELECT ZZN_PK,
ZZN_ExRateType,
ZZN_StartDate,
ZZN_EndDate,
ZZN_Rate,
ZZN_RX_NKExCurrency,
ZZN_RN_NKCountry,
ZZN_AsPublished
FROM RefExchangeRateZZ
WHERE ZZN_ExRateType = 'CUE' OR ZZN_ExRateType = 'CUS' OR ZZN_ExRateType = 'CUD' OR ZZN_ExRateType = 'IAT' OR ZZN_ExRateType = 'BNB' OR ZZN_ExRateType = 'BNS'
