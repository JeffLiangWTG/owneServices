CREATE VIEW [dbo].[VATApplicabilityView_V2]
	WITH SCHEMABINDING
AS
SELECT ZX5_PK
, ZX5_ZZ1_ParentTariffOrNationalCode
, CASE 
WHEN ZX5_ZZW_TariffNationalCode IS NOT NULL THEN 'ZZW'
ELSE 'ZZ1'
END AS ZX5_ParentTableType
, ZX5_ZZF_NKTaxOrFeeCode
, ZX5_StartDate
, ZX5_EndDate
, ZX5_AdditionalCode
, ZX5_Description
, ZX5_ZZZ_NKDataGrouping
, ZX5_ZZA_TradeGroup
, ZX5_VATCategory
FROM dbo.RefCusVATApplicability
