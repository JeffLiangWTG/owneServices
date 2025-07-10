using System;
using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService.Test
{
	public static class Helper
	{
		public static T CreateChild<T>(ObjectReferenceDataRepository repo, string parentProperty, Guid parentPK, string codeProperty = null, string code = null) where T : new()
		{
			var result = repo.Create<T>();
			var pk = Guid.NewGuid();
			typeof(T).GetProperties().FirstOrDefault(x => x.Name.EndsWith("_PK", StringComparison.InvariantCultureIgnoreCase)).SetValue(result, pk);
			typeof(T).GetProperty(parentProperty).SetValue(result, parentPK);
			if (!string.IsNullOrEmpty(codeProperty))
			{
				typeof(T).GetProperty(codeProperty).SetValue(result, code);
			}
			repo.Create(() => new RefDbVersionControl { RVC_ParentPK = pk });
			return result;
		}

		public static short GetDataSetId(DataSet dataSetId)
		{
			return (short)dataSetId;
		}
	}

	public enum DataSet
	{
		None = 0,
		RefDataGrouping = 1,
		RefLanguageType = 2,
		RefCusCodeType = 3,
		RefCusRateType = 4,
		RefExchangeRateZZ = 5,
		RefCusProcedure = 6,
		RefCusTaxOrFeeType = 7,
		RefCusNomenclatureGroupType = 8,
		RefCusTariffType = 9,
		RefCusCodeList = 10,
		RefCusNomenclatureGroup = 11,
		RefCusTradeGroup = 12,
		RefCarrierCode = 13,
		RefCusMapType = 14,
		RefCusMap = 15,
		UNDGSubstance = 16,
		UNDGSubstanceRID = 17,
		UNDGCommonData = 18,
		UNDGSubstanceADR = 19,
		RefCusPreference = 20,
		RefCusConditionValueType = 21,
		RefVesselZZ = 22,
		RefCusTariff = 23,
		RefCusTariffAdditionalCodeCategory = 24,
		RefCusConditionType = 25,
		RefCountryStates = 26,
		RefAccTaxRate = 27,
		RefTimeZoneSet = 28,
		RefUNLOCO = 29,
		RefUNLOCOPortMapping = 30,
		RefUNLOCOUtcOffset = 31,
		RefCountry = 32,
		RefCusRuling = 33,
		RefCurrency = 34,
		RefHarbourRate = 35,
		RefCusAUNexdocECMCode = 36,
		RefShippingLine = 37,
		RefDocOrgCusCode = 38,
		RefCusCodeListAttributeName = 39,
		UNDGSubstanceADN = 40,
		RefSysConfigType = 41,
		RefComplianceList = 42,
		RefAirline = 43,
		RefPortPolygon = 44,
		UNDGSubstanceJTT = 45,
		RefVessel = 46,
		UNDGSubstanceCFR = 47,
		UNDGCountryReference = 48,
		RefAirlineCommodityCode = 49,
		RefShippingLineMessagingRequirementType = 50,
		RefStlScript = 51,
		RefCusConfiguration = 52,
		RefCusQuota = 53,
		RefAirlineProductCode = 54,
		RefFacility = 55,
		RefStlFieldMapping = 56,
		RefClient = 57,
		RefCusTariffAttributeName = 58,
		RefCusConditionCode = 59,
		RefMessagingBussPackageInfo = 60,
		RefMaterial = 61,
		RefDamage = 62,
		RefMRComponentCode = 63,
		RefCusProfileType = 64,
		RefRepairCode = 65,
		RefUnitSection = 66,
		RefCusProfile = 67,
		RefCusProfileQuestion = 68,
		RefCusProfileQuestionPathway = 69,
		RefAccElectronicProcessingFee = 70,
		RefEquipmentGrade = 71,
		RefGlbReleaseNote = 72,
		RefComplianceCommodityAlert = 73,
		UNDGVersion = 74,
		RefAccessorial = 75,
		FRFallBack = 200,
	}
}
