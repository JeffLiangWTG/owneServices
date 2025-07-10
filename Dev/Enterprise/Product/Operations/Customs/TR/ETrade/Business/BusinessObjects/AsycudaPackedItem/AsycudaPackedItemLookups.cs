using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaPackedItemLookups : ASYCUDA.Business.AsycudaPackedItemLookups
	{
		public AsycudaPackedItemLookups(AsycudaPackedItem parent)
			: base(parent)
		{ }

		public new AsycudaPackedItem Parent => (AsycudaPackedItem)base.Parent;

		public CodeDescriptionPairList TRUOMCodeList => Factory.GetCachedValue("TR.ETrade.AsycudaPackedItemLookups.TRUOMCodeList", () => new TRUOMCodeList());

		public RefCurrencyCollection CurrencyList => new RefCurrencyCollection(Factory);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Key value")]
		public CodeDescriptionPairList TariffAdditionalCodeList
		{
			get
			{
				var parent = Parent;
				var bill = (AsycudaBill)parent.Bill;

				var cacheKey = Parent.API_Tariff + Parent.EffectiveDateForDutyRate.ToString() + bill.ExportCountry;
				return Factory.GetCachedValue("TR eTrade" + cacheKey, () =>
				{
					var result = new CodeDescriptionPairList();

					var currentTariff = new TariffView.Loader(parent.Factory).LoadMostRecentCachedTariff(Core.Constants.CountryCodes.Turkey, Constants.TariffTypes.HarmonizedSystem, Parent.API_Tariff, Parent.EffectiveDateForDutyRate);
					if (currentTariff != null)
					{
						var criteria = new SpecificRateSelectionCriteria(bill.ExportCountry, Core.Constants.CountryCodes.Turkey, ZString.Empty, ZString.Empty, null, parent.EffectiveDateForDutyRate, TaxCodeList.RelatedMiscCodes.SpecialCustomsDutyCode, TaxCodeList.Codes.CustomsDuty);
						var addcodes = currentTariff.GetRateSelectionCriteriaInfo(new IZZRateSelectionCriteria[] { criteria }).Where(r => !r.ZZT_AdditionalCode.IsEmpty && r.Match(criteria)).Select(r => r.ZZT_AdditionalCode).Distinct();

						foreach (var addCode in addcodes)
						{
							var addCodeDesc = CachedListOfAdditionalCodeDescriptions.GetDescriptionFromCode(addCode);
							result.AddPair(addCode, addCodeDesc);
						}
					}

					return result;
				});
			}
		}

		CodeDescriptionPairList CachedListOfAdditionalCodeDescriptions
			=> RefCusCodeListTypes.GetCachedList(Factory,
				Parent.CountryCode,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes,
				Parent.EffectiveDateForDutyRate);
	}
}
