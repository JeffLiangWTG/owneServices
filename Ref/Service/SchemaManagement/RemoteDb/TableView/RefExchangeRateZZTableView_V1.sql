CREATE VIEW RefExchangeRateZZTableView_V1 AS
SELECT ZZN_PK,
ZZN_ExRateType,
ZZN_StartDate,
ZZN_EndDate,
ZZN_Rate,
ZZN_RX_NKExCurrency,
ZZN_RN_NKCountry,
ZZN_AsPublished
FROM RefExchangeRateZZ
WHERE LEN(ZZN_AsPublished) <= 10
