CREATE VIEW TariffAttributeView_V1
	WITH SCHEMABINDING
AS
SELECT ZZ3_PK
, ZZ3_ZZ1_Tariff as ZZ3_ZZ1_ParentTariffOrNationalCode
, 'ZZ1' as ZZ3_ParentTableType
, ZZ3_Name
, ZZ3_Value
FROM dbo.RefCusTariffAttribute
WHERE ZZ3_ZZ1_Tariff IS NOT NULL

UNION ALL

SELECT ZZ3_PK
, ZZ3_ZZW_TariffNationalCode as ZZ3_ZZ1_ParentTariffOrNationalCode
, 'ZZW' as ZZ3_ParentTableType
, ZZ3_Name
, ZZ3_Value
FROM dbo.RefCusTariffAttribute
WHERE ZZ3_ZZW_TariffNationalCode IS NOT NULL