using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.Business
{
	public class AsycudaCustomsCountryProvider : Integration.Customs.Shared.IAsycudaCustomsCountryProvider
	{
		public AsycudaCustomsCountryProvider()
		{
		}

		public ZString[] GetAsycudaCustomsCountryCodes()
		{
			if (asycudaCustomsCountryCodes.Value == null)
			{
				var codeDescriptionPairs = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(
					new ReadOnlyBusinessObjectFactory(),
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda);

				asycudaCustomsCountryCodes.Value = codeDescriptionPairs.GetAllCodesZString();
			}

			return asycudaCustomsCountryCodes.Value;
		}
		readonly Overridable<ZString[]> asycudaCustomsCountryCodes = new Overridable<ZString[]>();

		public bool IsAsycudaCustomsCountry(ZString countryCode)
		{
			var asycudaCustomsCountries = GetAsycudaCustomsCountryCodes();
			return asycudaCustomsCountries.Contains(countryCode);
		}

		public bool IsSelfManagedTariffCountry(ZString countryCode)
		{
			var selfManagedCountries = RefCusCodeListTypes.GetCachedList(
				new ReadOnlyBusinessObjectFactory(),
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SelfManagedCountry,
				ZDateTime.Today);
			return selfManagedCountries.ContainsCode(countryCode);
		}

		public ZString[] GetSelfManagedTariffCountryCodes()
		{
			var selfManagedCountries = AsycudaUniversalReference.RefCusCodeListTypes.GetCachedList(
				new ReadOnlyBusinessObjectFactory(),
				Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
				Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SelfManagedCountry);
			return selfManagedCountries.GetAllCodesZString();
		}

		public ZString[] GetAsycudaXMLCountryCodes()
		{
			if (asycudaXMLCountryCodes == null)
			{
				asycudaXMLCountryCodes = Universal.RefCusCodeListTypes.GetCachedListMatchAllAttributes(new ReadOnlyBusinessObjectFactory(),
					Core.Constants.Customs.Universal.RefDataGrouping.Codes.CommonDataGrouping,
					Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Asycuda,
					ZDateTime.Today,
					new[] { new KeyValuePair<ZString, ZString>(UniversalReferenceConstants.RefCusCodeListAttributes.Name.AsycudaXML, UniversalReferenceConstants.RefCusCodeListAttributes.Value.Yes) }).GetAllCodesZString();
			}

			return asycudaXMLCountryCodes;
		}
		ZString[] asycudaXMLCountryCodes;

		public bool IsAsycudaXMLCountry(ZString countryCode) => GetAsycudaXMLCountryCodes().Contains(countryCode);

#if DEBUG
		public void ResetCachingForTest()
		{
			asycudaCustomsCountryCodes.ResetValue();
		}
#endif
	}
}
