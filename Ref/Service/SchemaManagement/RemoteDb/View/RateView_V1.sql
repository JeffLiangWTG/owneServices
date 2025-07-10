CREATE VIEW RateView_V1
WITH SCHEMABINDING
AS
SELECT ZZ2_PK
, ZZ2_ZZ1_Tariff AS ZZ2_ZZ1_ParentTariffOrNationalCode
, 'ZZ1' AS ZZ2_ParentTableType
, ZZ2_StartDate
, ZZ2_EndDate
, ZZ2_ZY1_RateCode
, ZZ2_RateFormula
, ZZ2_ZZS_Preference
, ZZ2_RateFormulaDerivedFrom
, CASE ZZ2_ZZZ_NKDataGrouping 
WHEN '' THEN ZZ1_ZZZ_NKDataGrouping
ELSE ZZ2_ZZZ_NKDataGrouping
END AS ZZ2_ZZZ_NKDataGrouping
,ZZ2_RX_NKCurrencyOverride
FROM dbo.RefCusRate AS rate
INNER JOIN dbo.RefCusTariff
on ZZ2_ZZ1_Tariff = ZZ1_PK
UNION ALL
SELECT 
ZZ2_PK
, ZZ2_ZZW_TariffNationalCode AS ZZ2_ZZ1_ParentTariffOrNationalCode
, 'ZZW' AS ZZ2_ParentTableType
, ZZ2_StartDate
, ZZ2_EndDate
, ZZ2_ZY1_RateCode
, ZZ2_RateFormula
, ZZ2_ZZS_Preference
, ZZ2_RateFormulaDerivedFrom
, CASE ZZ2_ZZZ_NKDataGrouping 
WHEN '' THEN ZZW_ZZZ_NKDataGrouping
ELSE ZZ2_ZZZ_NKDataGrouping
END AS ZZ2_ZZZ_NKDataGrouping
,ZZ2_RX_NKCurrencyOverride
FROM dbo.RefCusRate AS rate
INNER JOIN dbo.RefCusTariffNationalCode
on ZZ2_ZZW_TariffNationalCode = ZZW_PK