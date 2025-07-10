using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.TW.Business
{
	public static class TariffHelper
	{
		public static IDictionary<RefCusTariffType, List<TariffView>> GetValidRefCusTariffSortedDictionary(BusinessObjectFactory factory, TariffView universalTariff, ZString tariff, ZDateTime assessmentDate)
		{
			var sortedDictionary = new SortedDictionary<RefCusTariffType, List<TariffView>>(new RefCusTariffTypeComparer());
			var relatedTariffs = new TariffView.Loader(factory).GetEffectiveChildTariffs(Core.Constants.CountryCodes.Taiwan, TariffTypes.HarmonizedSystem, tariff, assessmentDate);
			foreach (var relatedTariff in relatedTariffs)
			{
				var relatedTariffFromType = relatedTariff.CusTariffType;
				var tariffType = relatedTariffFromType.ZZI_TariffType;
				if (CorrspondingAdditionalTaxTariffType.TryGetValue(tariffType, out var attributeCodes) && attributeCodes.Any(attributeCode => universalTariff.HasTariffCustomsRequirementsAttribute(attributeCode)))
				{
					if (!sortedDictionary.TryGetValue(relatedTariffFromType, out var list))
					{
						list = new List<TariffView>();
						sortedDictionary.Add(relatedTariffFromType, list);
					}
					list.Add(relatedTariff);
				}
			}
			return sortedDictionary;
		}

		public static ZBool HasTariffCustomsRequirementsAttribute(this TariffView universalTariff, ZString attributeCode)
		{
			return universalTariff?.HasAttribute(Constants.UniversalReferenceConstants.CusTariffAttributeName.CustomsRequirements, attributeCode) ?? ZBool.False;
		}

		static Dictionary<string, List<string>> CorrspondingAdditionalTaxTariffType
		{
			get
			{
				if (TWCustomsDataRegistry.Instance.AlwaysCalculateAdditionalTax.Value)
				{
					return new Dictionary<string, List<string>>()
						{
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.CT, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.T, Constants.UniversalReferenceConstants.CusTariffAttributeValue.TPartially } },
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.AT, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.B, Constants.UniversalReferenceConstants.CusTariffAttributeValue.BPartially } },
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.TT, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.C } },
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.SS, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.LPartially } }
						};
				}
				else
				{
					return new Dictionary<string, List<string>>()
						{
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.CT, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.T } },
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.AT, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.B } },
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.TT, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.C } },
							{ Constants.UniversalReferenceConstants.RefCusRateTypes.SS, new List<string>() { Constants.UniversalReferenceConstants.CusTariffAttributeValue.LPartially } }
						};
				}
			}
		}
	}

	class RefCusTariffTypeComparer : Comparer<RefCusTariffType>
	{
		public override int Compare(RefCusTariffType x, RefCusTariffType y)
		{
			return string.Compare(x.ZZI_TariffType, y.ZZI_TariffType, StringComparison.Ordinal);
		}
	}
}
