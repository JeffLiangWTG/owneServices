using System.Data;
using CargoWise.RefDbRepo.Common.DbUpgrade;

namespace CargoWise.RefDbRepo.Service.UpgradeManagerRunner
{
	public class PopulateRefDataSetInitialInformation : DataTransformation, IDataTransformationTask
	{
		public PopulateRefDataSetInitialInformation(int version) : base(version)
		{
		}

		public void Run(IDbTransaction trans)
		{
			var sql = @"
IF (SELECT COUNT(*) FROM RefDataSetInformation) = 0
BEGIN
	INSERT RefDataSetInformation(RDS_PK, RDS_DataSetId, RDS_DataSetName, RDS_TableName, RDS_DataSetTableCode, RDS_PriorityLevel)
	VALUES(newid(), 1, 'RefDataGrouping', 'RefDataGrouping', 'ZZZ', 0)
	, (newid(), 2,  'RefLanguageType', 'RefLanguageType', 'ZX6', 0)
	, (newid(), 3,  'RefCusCodeType', 'RefCusCodeType', 'ZZK', 0)
	, (newid(), 4,  'RefCusRateType', 'RefCusRateType', 'ZZR', 0)
	, (newid(), 5,  'RefExchangeRateZZ', 'RefExchangeRateZZ', 'ZZN', 0)
	, (newid(), 6,  'RefCusProcedure', 'RefCusProcedure', 'ZZ6', 0)
	, (newid(), 7,  'RefCusTaxOrFeeType', 'RefCusTaxOrFeeType', 'ZX0', 0)
	, (newid(), 8,  'RefCusNomenclatureGroupType', 'RefCusNomenclatureGroupType', 'ZZ9', 0)
	, (newid(), 9,  'RefCusTariffType', 'RefCusTariffType', 'ZZI', 0)
	, (newid(), 10, 'RefCusCodeList', 'RefCusCodeList', 'ZZD', 0)
	, (newid(), 11, 'RefCusNomenclatureGroup', 'RefCusNomenclatureGroup', 'ZZ5', 0)
	, (newid(), 12, 'RefCusTradeGroup', 'RefCusTradeGroup', 'ZZA', 0)
	, (newid(), 13, 'RefCarrierCode', 'RefCarrierCode', 'ZZ4', 0)
	, (newid(), 14, 'RefCusMapType', 'RefCusMapType', 'ZZP', 0)
	, (newid(), 15, 'RefCusMap', 'RefCusMap', 'ZZM', 0)
	, (newid(), 16, 'UNDGSubstance', 'UNDGSubstance', 'DG', 0)
	, (newid(), 17, 'UNDGSubstanceRID', 'UNDGSubstanceRID', 'RID', 0)
	, (newid(), 18, 'UNDGCommonData', 'UNDGCommonData', 'DC', 0)
	, (newid(), 19, 'UNDGSubstanceADR', 'UNDGSubstanceADR', 'ADR', 0)
	, (newid(), 20, 'RefCusPreference', 'RefCusPreference', 'ZZS', 0)
	, (newid(), 21, 'RefCusConditionValueType', 'RefCusConditionValueType', 'ZX4', 0)
	, (newid(), 22, 'RefVesselZZ', 'RefVesselZZ', 'ZZO', 0)
	, (newid(), 23, 'RefCusTariff', 'RefCusTariff', 'ZZ1', 0)
	, (newid(), 24, 'RefCusTariffAdditionalCodeCategory', 'RefCusTariffAdditionalCodeCategory', 'ZY3', 0)
	, (newid(), 25, 'RefCusConditionType', 'RefCusConditionType', 'ZX2', 0)
	, (newid(), 26, 'RefCountryStates', 'RefCountryStates', 'RW', 0)
	, (newid(), 27, 'RefAccTaxRate', 'RefAccTaxRate', 'ZAT', 0)
	, (newid(), 28, 'RefTimeZoneSet', 'RefTimeZoneSet', 'R3', 0)
	, (newid(), 29, 'RefUNLOCO', 'RefUNLOCO', 'RL', 0)
	, (newid(), 30, 'RefUNLOCOPortMapping', 'RefUNLOCOPortMapping', 'RLM', 0)
	, (newid(), 32, 'RefCountry', 'RefCountry', 'RN', 0)
	, (newid(), 33, 'RefCusRuling', 'RefCusRuling', 'ZZX', 0)
	, (newid(), 34, 'RefCurrency', 'RefCurrency', 'RX', 0)
	, (newid(), 35, 'RefHarbourRate', 'RefHarbourRate', 'ZXF', 0)
	, (newid(), 36, 'RefCusAUNexdocECMCode', 'RefCusAUNexdocECMCode', 'ZY5', 0)
	, (newid(), 37, 'RefShippingLine', 'RefShippingLine', 'RSL', 0)
	, (newid(), 38, 'RefDocOrgCusCode', 'RefDocOrgCusCode', 'DOC', 0)
	, (newid(), 39, 'RefCusCodeListAttributeName', 'RefCusCodeListAttributeName ', 'ZXE', 0)
	, (newid(), 200, 'FRFallback', 'RefCusCodeList', 'ZZD', 1);

-- FR Fallback dataset.
	INSERT INTO RefDataSetInformationDefinition (RDD_PK, RDD_DataSetId, RDD_ColumnName, RDD_ColumnValue)
	VALUES (newid(), 200, 'ZZD_ZZZ_NKDataGrouping','FR'),
	(newid(), 200, 'ZZD_ZZK_NKCodeType', 'FBK')

--Cater for existing records.
;with orderedDataSetInformation AS
(
	SELECT CASE
			WHEN ZZD_PK IS NOT NULL AND ZZD_ZZZ_NKDataGrouping = 'FR' AND ZZD_ZZK_NKCodeType = 'FBK' THEN 200
									ELSE RDS_DataSetId
			END DataSetId,
			RVC_ParentPK
	FROM RefDbVersionControl
	JOIN RefDataSetInformation ON RVC_ParentCode = RDS_DataSetTableCode AND RDS_PriorityLevel = 0
	LEFT JOIN RefCusCodeList ON RVC_ParentPK = ZZD_PK
)

UPDATE r
SET r.RVC_DataSetId = d.DataSetId
FROM RefDbVersionControl r
JOIN orderedDataSetInformation d ON d.RVC_ParentPK = r.RVC_ParentPK

UPDATE RefDataSetInformation SET RDS_LastUpdatedUTC = (SELECT MAX(RVC_LastUpdatedUTC)
FROM RefDbVersionControl WHERE RVC_DataSetId = RDS_DataSetId)

END
";

			DbHelper.ExecuteNonQuery(trans, sql, 300);
		}
	}
}
