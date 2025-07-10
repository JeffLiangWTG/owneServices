CREATE VIEW TariffRelationshipView_V1
	WITH SCHEMABINDING
AS
SELECT ZZH_PK
	,ZZH_ZZ1_Tariff as ZZH_ZZ1_LinkedTariffOrNationalCode
	,'ZZ1' AS ZZH_LinkedTableType
	,ZZ1_TariffCode AS ZZH_RelatedTariffCode
	,ZZ1_ZZI_TariffType AS ZZH_ZZI_RelatedTariffType
	,ZZ1_ZZZ_NKDataGrouping AS ZZH_ZZZ_RelatedTariffDataGrouping
	,ZZH_ZZI_TariffType
	,ZZH_TariffCode
FROM dbo.RefCusTariffRelationship
JOIN dbo.RefCusTariff ON ZZ1_PK = ZZH_ZZ1_Tariff