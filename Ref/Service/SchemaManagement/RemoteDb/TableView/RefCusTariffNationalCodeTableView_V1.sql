CREATE VIEW RefCusTariffNationalCodeTableView_V1 AS
SELECT ZZW_PK, 
ZZW_ZZ1_Tariff, 
ZZW_NationalCode, 
ZZW_Description, 
SUBSTRING(ZZW_ZZF_NKTaxOrFeeCode, 1, 3) AS ZZW_ZZF_NKTaxOrFeeCode, 
ZZW_StartDate, 
ZZW_EndDate, 
ZZW_PublishedDate, 
ZZW_ZZZ_NKDataGrouping
FROM RefCusTariffNationalCode
