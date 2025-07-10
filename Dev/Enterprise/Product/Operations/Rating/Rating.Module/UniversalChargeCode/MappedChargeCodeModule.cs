using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Rating;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Internal;
using Enterprise.ZArchitecture.Schema;
using WiseRates.Api.Model;

namespace Enterprise.Rating.Module;

public abstract class MappedChargeCodeModule<T> : ZFilterGridModule
	where T : MappedChargeCodeBizo
{
	#region ZFilterGridModule overrides

	public override bool AllowDelete => false;
	public override bool AllowEdit => false;
	public override bool AllowNew => false;
	public override bool AllowView => false;
	public override bool SupportsWorkflow => false;
	protected override bool ShowRecentItemsCore() => false;
	public override bool AllowCopyFilterGridHyperlinkToClipboard => false;
	protected override bool IsModuleAllowAsync => false;

	#endregion

	protected override void OnCustomGridLoad(IBusinessObjectCollection collection, PerformSearchResult searchResult)
	{
		OnCustomGridLoadCore((BusinessObjectCollection<T>)collection, searchResult);

		collection.RunPreSaveValidation();

		if (collection.Count == 0)
		{
			Globals.Message.ShowWarning(Business.RatingConstants.NoResults);
		}
	}

	protected virtual void OnCustomGridLoadCore(BusinessObjectCollection<T> collection, PerformSearchResult searchResult)
	{
		var chargeCodesFromApi = AccChargeCodeUniversalCodeMappingLookups.GetAllUniversalChargeCodesWithMappingInfo(Factory);
		var (matchingCharges, chargeCodeToGlobalCode, chargeCodeToLocalCode) = GenerateChargeCodeMappings(chargeCodesFromApi);

		LoadChargeCodesCore(
			collection,
			searchResult,
			chargeCodesFromApi,
			(chargeCode, apiChargeCode) => MapChargeCode(chargeCode, apiChargeCode, chargeCodeToGlobalCode, chargeCodeToLocalCode, matchingCharges));
	}

	protected virtual void LoadChargeCodesCore(BusinessObjectCollection<T> collection, PerformSearchResult searchResult, ChargeCodeWithMappingInfo[] chargeCodesFromApi, Action<T, ChargeCodeWithMappingInfo> mapChargeCode)
	{
		collection.RemoveAndDeleteAll();

		// Maps the apiChargeCode and copies all the values into the Mapped Charge Code Bizo, which is then checked against the provided filters.
		chargeCodesFromApi
			.Where(apiChargeCode => !string.IsNullOrEmpty(apiChargeCode.Code))
			.ForEach(apiChargeCode =>
			{
				var chargeCode = collection.AddNew();
				mapChargeCode(chargeCode, apiChargeCode);

				if (!chargeCode.MatchesFilter(searchResult.Query))  // Removes all Charge Codes that don't match the Filter or matches the Custom Remove Filter Check.
				{
					collection.RemoveAndDelete(chargeCode);
				}
			});
	}

	protected virtual (AccChargeCode[], Dictionary<string, string>, Dictionary<string, string>) GenerateChargeCodeMappings(ChargeCodeWithMappingInfo[] chargeCodesFromApi)
	{
		var codesFromApi = chargeCodesFromApi.Select(c => c.Code).Distinct().ToArray();

		var matchingCharges = LoadChargeCodes(Factory, codesFromApi);

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

	protected AccChargeCode[] LoadChargeCodes(BusinessObjectFactory factory, string[] codesFromApi, string universalOrCarrier = AccChargeCodeUniversalCodeMapping.Constants.ChargeCodeMappingTypes.Universal)
	{
		var chargeCodeQuery = new ZDBOnlyQuery(typeof(AccChargeCode));
		chargeCodeQuery.AddToFilter(AccChargeCodeSchema.AC_IsActive, true);

		var mappingQuery = new ZDBOnlySubQuery(typeof(AccChargeCodeUniversalCodeMapping), AccChargeCodeUniversalCodeMappingSchema.AUP_AC);
		mappingQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Code, codesFromApi);
		mappingQuery.AddToFilter(AccChargeCodeUniversalCodeMappingSchema.AUP_Type, universalOrCarrier);

		chargeCodeQuery.AddSubQuery(AccChargeCodeSchema.PK, mappingQuery, JoinCondition.And);

		var companyQuery = new ZQuery(AccChargeCodeSchema.AC_GC, null);
		companyQuery.AddToFilter(JoinCondition.Or, AccChargeCodeSchema.AC_GC, Env.CurrentCompanyPK);

		chargeCodeQuery.AddToFilter(companyQuery);

		return factory.Load<AccChargeCode>(chargeCodeQuery);
	}

	/// <summary>
	/// Maps <see cref="ChargeCodeWithMappingInfo"/> to <see cref="MappedChargeCodeBizo"/> and adds to `chargeCodeCollection`.
	/// </summary>
	static void MapChargeCode(
		MappedChargeCodeBizo chargeCode,
		ChargeCodeWithMappingInfo chargeCodeFromApi,
		Dictionary<string, string> chargeCodeToGlobal,
		Dictionary<string, string> chargeCodeToLocal,
		IEnumerable<AccChargeCode> matchingCharges)
	{
		chargeCode.UCC_Code = chargeCodeFromApi.Code;
		chargeCode.UCC_Description = chargeCodeFromApi.Description;
		chargeCode.UCC_RateProvider = chargeCodeFromApi.Provider;
		chargeCode.UCC_Carrier = chargeCodeFromApi.Carrier;
		chargeCode.UCC_ForeignCode = chargeCodeFromApi.ForeignCode;
		chargeCode.UCC_ForeignName = chargeCodeFromApi.ForeignName;

		if (chargeCodeToGlobal.TryGetValue(chargeCodeFromApi.Code, out var mappedGlobalCode))
		{
			chargeCode.UCC_GlobalChargeCode = mappedGlobalCode;
			chargeCode.UCC_GlobalChargeCodeDescription = matchingCharges.First(c => c.AC_Code == mappedGlobalCode).AC_DescMultilingual;
		}
		else
		{
			chargeCode.UCC_GlobalChargeCode = ZString.Empty;
			chargeCode.UCC_GlobalChargeCodeDescription = ZString.Empty;
		}

		if (chargeCodeToLocal.TryGetValue(chargeCodeFromApi.Code, out var mappedLocalCode))
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

	#region Security Checkpoint

	public override SecurityCheckpoint SecurityCheckpoint => Env.Security.None;
	protected override LicenceCheckpoint LicenceCheckPointCore => Env.Licence.AlwaysAllow;

	#endregion

	#region Default security overrides

	public override bool AllowUniversalCopy => false;

	protected override IModuleDecisionProvider GetModuleDecisionProviderForFindBoxPopupCore(IFindBox findbox) =>
		new UniversalChargeCodePopupModuleDecisionProvider(findbox);

	class UniversalChargeCodePopupModuleDecisionProvider(IFindBox findBox) : PopupModuleDecisionProvider(findBox)
	{
		public override bool AllowExcelExport => true;
	}

	#endregion

	protected override PerformSearchResult LoadCollection(BusinessObjectFactory factory, Type type, ZQuery query) => PerformSearchResult.CustomGridLoad(factory, query);

	// Not needed for a non-persistent bizo.
	protected override bool CanReloadWithFilter(BusinessObjectFactory newFactory, BusinessObject selectedBusinessObject, ZQuery filter) => true;

	protected override ZController GetNewController(BusinessObject selectedBusinessObject) => null;
}
