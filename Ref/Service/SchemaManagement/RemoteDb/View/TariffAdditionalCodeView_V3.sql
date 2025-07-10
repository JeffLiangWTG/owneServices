CREATE VIEW TariffAdditionalCodeView_V3
	WITH SCHEMABINDING
AS
SELECT ZY2_PK
, ISNULL(ZY2_ZZ1_Tariff, ZY2_ZZW_NationalCode) AS ZY2_ZZ1_ParentTariffOrNationalCode
, CASE 
WHEN ZY2_ZZW_NationalCode IS NOT NULL THEN 'ZZW'
ELSE 'ZZ1'
END AS ZY2_ParentTableType
, ZY2_AdditionalCode
, ZY2_Description
, ZY2_ZY3_NKCategory
, ZY2_IsMandatory
, ZY2_ZZZ_NKDataGrouping
, ZY2_ParentAdditionalCode
, ZY2_ZY3_NKParentCategory
, ZY2_StartDate
, ZY2_EndDate
FROM dbo.RefCusTariffAdditionalCode
