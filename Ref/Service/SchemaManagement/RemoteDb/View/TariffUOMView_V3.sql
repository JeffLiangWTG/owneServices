CREATE VIEW TariffUOMView_V3
	WITH SCHEMABINDING
AS
SELECT ZZ8_PK
	,ZZ8_ZZ1_Tariff AS ZZ8_ZZ1_ParentTariffOrNationalCode
	,'ZZ1' AS ZZ8_ParentTableType
	,ZZ8_Type
	,ZZ8_UOM
	,ZZ8_ZZA_TradeGroup
	,ZZ8_ZZA_SecondTradeGroup
	,ZZ8_StartDate
	,ZZ8_EndDate
	,ZZ8_ZZZ_NKDataGrouping
FROM dbo.RefCusTariffUOM
INNER JOIN dbo.RefCusTariff
	ON ZZ1_PK = ZZ8_ZZ1_Tariff

UNION ALL

SELECT ZZ8_PK
	,ZZ8_ZZW_TariffNationalCode AS ZZ8_ZZ1_ParentTariffOrNationalCode
	,'ZZW' AS ZZ8_ParentTableType
	,ZZ8_Type
	,ZZ8_UOM
	,ZZ8_ZZA_TradeGroup
	,ZZ8_ZZA_SecondTradeGroup
	,ZZ8_StartDate
	,ZZ8_EndDate
	,ZZ8_ZZZ_NKDataGrouping
FROM dbo.RefCusTariffUOM
INNER JOIN dbo.RefCusTariffNationalCode
	ON ZZW_PK = ZZ8_ZZW_TariffNationalCode
