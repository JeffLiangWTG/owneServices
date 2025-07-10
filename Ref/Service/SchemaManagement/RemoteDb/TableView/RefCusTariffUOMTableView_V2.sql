CREATE VIEW RefCusTariffUOMTableView_V2 AS
SELECT ZZ8_PK,
ZZ8_ZZ1_Tariff,
ZZ8_Type,
ZZ8_UOM,
ZZ8_StartDate,
ZZ8_EndDate,
ZZ8_ZZA_TradeGroup,
ZZ8_ZZA_SecondTradeGroup,
ZZ8_ZZW_TariffNationalCode,
ZZ8_ZZZ_NKDataGrouping
FROM RefCusTariffUOM
