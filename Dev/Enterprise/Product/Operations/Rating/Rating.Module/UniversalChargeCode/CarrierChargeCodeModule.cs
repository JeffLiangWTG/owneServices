using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Module;

public class CarrierChargeCodeModule() : MappedChargeCodeModule<CarrierChargeCodeBizo>
{
	public override ModuleIdentifier ID => ModuleIDs.CarrierChargeCode;

	protected override FilterBusinessObject GetNewFilterBusinessObject() =>
		new CarrierChargeCodeFilterBusinessObject();

	protected override IFilterControl GetNewFilterControl() =>
		new CarrierChargeCodeFilterControl(GridCollection, (CarrierChargeCodeFilterBusinessObject)FilterBusinessObject);

	protected override IBusinessObjectCollection GetNewGridCollection() =>
		new CarrierChargeCodeBizoCollection(Factory);

	protected override void OnCustomGridLoadCore(BusinessObjectCollection<CarrierChargeCodeBizo> collection, PerformSearchResult searchResult)
	{
		var chargeCodesFromApi = AccChargeCodeUniversalCodeMappingLookups.GetAllUniversalChargeCodesWithMappingInfo(Factory);
		var (matchingCharges, chargeCodeToGlobalCode, chargeCodeToLocalCode) = GenerateChargeCodeMappings(chargeCodesFromApi);
		var (matchingForeignCharges, foreignChargeCodeToGlobalCode, foreignChargeCodeToLocalCode) = GenerateForeignChargeCodeMappings(chargeCodesFromApi);

		LoadChargeCodesCore(
			collection,
			searchResult,
			chargeCodesFromApi,
			(chargeCode, apiChargeCode) => MapChargeCode(chargeCode, apiChargeCode, chargeCodeToGlobalCode, chargeCodeToLocalCode, foreignChargeCodeToGlobalCode, foreignChargeCodeToLocalCode, matchingCharges, matchingForeignCharges));
	}

	(AccChargeCode[], Dictionary<string, string>, Dictionary<string, string>) GenerateForeignChargeCodeMappings(ChargeCodeWithMappingInfo[] chargeCodesFromApi)
	{
		// Finds all the type "CAR" Charge Code Mappings
		var foreignCodesFromApi = chargeCodesFromApi.Select(c => c.ForeignCode).Where(code => !string.IsNullOrEmpty(code)).Distinct().ToArray();
		var matchingCharges = LoadChargeCodes(Factory, foreignCodesFromApi, AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Carrier); 

		var chargeCodeToGlobalCode = new Dictionary<string, string>(matchingCharges.Length);
		var chargeCodeToLocalCode = new Dictionary<string, string>(matchingCharges.Length);
		foreach (var chargeCode in matchingCharges)
		{
			var map = chargeCode.AC_GC.IsEmpty ? chargeCodeToGlobalCode : chargeCodeToLocalCode;
			chargeCode.UniversalChargeCodeMappingsCollection
				.Cast<AccChargeCodeUniversalCodeMapping>()
				.Where(mapping => !map.ContainsKey(mapping.AUP_Code) || chargeCode.IsInDatabase)
				.ForEach(mapping => map[mapping.AUP_Code] = chargeCode.AC_Code);
		}

		return (matchingCharges, chargeCodeToGlobalCode, chargeCodeToLocalCode);
	}

	void MapChargeCode(
		MappedChargeCodeBizo chargeCode,
		ChargeCodeWithMappingInfo chargeCodeFromApi,
		Dictionary<string, string> chargeCodeToGlobal,
		Dictionary<string, string> chargeCodeToLocal,
		Dictionary<string, string> foreignChargeCodeToGlobal,
		Dictionary<string, string> foreignChargeCodeToLocal,
		IEnumerable<AccChargeCode> matchingCharges,
		IEnumerable<AccChargeCode> matchingForeignCharges)
	{
		chargeCode.UCC_Code = chargeCodeFromApi.Code;
		chargeCode.UCC_Description = chargeCodeFromApi.Description;
		chargeCode.UCC_RateProvider = chargeCodeFromApi.Provider;
		chargeCode.UCC_Carrier = chargeCodeFromApi.Carrier;
		chargeCode.UCC_ForeignCode = chargeCodeFromApi.ForeignCode;
		chargeCode.UCC_ForeignName = chargeCodeFromApi.ForeignName;

		// Maps direct Carrier Charge Codes with Fallback to Universal Charge Code
		if (!string.IsNullOrEmpty(chargeCodeFromApi.ForeignCode) && foreignChargeCodeToGlobal.TryGetValue(chargeCodeFromApi.ForeignCode, out var mappedGlobalCode))
		{
			chargeCode.UCC_GlobalChargeCode = mappedGlobalCode;
			chargeCode.UCC_GlobalChargeCodeDescription = matchingForeignCharges.First(c => c.AC_Code == mappedGlobalCode).AC_DescMultilingual;
		}
		else if (chargeCodeToGlobal.TryGetValue(chargeCodeFromApi.Code, out mappedGlobalCode))
		{
			chargeCode.UCC_GlobalChargeCode = mappedGlobalCode;
			chargeCode.UCC_GlobalChargeCodeDescription = matchingCharges.First(c => c.AC_Code == mappedGlobalCode).AC_DescMultilingual;
		}
		else
		{
			chargeCode.UCC_GlobalChargeCode = ZString.Empty;
			chargeCode.UCC_GlobalChargeCodeDescription = ZString.Empty;
		}

		if (!string.IsNullOrEmpty(chargeCodeFromApi.ForeignCode) && foreignChargeCodeToLocal.TryGetValue(chargeCodeFromApi.ForeignCode, out var mappedLocalCode))
		{
			chargeCode.UCC_LocalChargeCode = mappedLocalCode;
			chargeCode.UCC_LocalChargeCodeDescription = matchingForeignCharges.First(c => c.AC_Code == mappedLocalCode).AC_DescMultilingual;
		}
		else if (chargeCodeToLocal.TryGetValue(chargeCodeFromApi.Code, out mappedLocalCode))
		{
			chargeCode.UCC_LocalChargeCode = mappedLocalCode;
			chargeCode.UCC_LocalChargeCodeDescription = matchingCharges.First(c => c.AC_Code == mappedLocalCode).AC_DescMultilingual;
		}
		else
		{
			chargeCode.UCC_LocalChargeCode = ZString.Empty;
			chargeCode.UCC_LocalChargeCodeDescription = ZString.Empty;
		}
	}
}
