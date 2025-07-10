using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class OrgAddressEffectiveAddress : IEffectiveAddress
	{
		public OrgAddressEffectiveAddress(OrgAddress address)
		{
			orgAddress = Argument.NotNull(address, nameof(address));
		}

		readonly OrgAddress orgAddress;

		ZString IEffectiveAddress.CompanyName => orgAddress.CompanyName;

		ZString IEffectiveAddress.Address1 => orgAddress.Address1;

		ZString IEffectiveAddress.Address2 => orgAddress.Address2;

		ZString IEffectiveAddress.AdditionalAddressInformation => orgAddress.OA_AdditionalAddressInformation;

		ZString IEffectiveAddress.City => orgAddress.City;

		ZString IEffectiveAddress.StateCode => orgAddress.StateCode;

		ZString IEffectiveAddress.GetStateDescription(ZString languageCode)
		{
			var desc = (ZString?)orgAddress.RelatedState?.RW_DescriptionMultilingual?.GetLocalizedValue(languageCode) ?? ZString.Empty;
			return string.IsNullOrWhiteSpace(desc) ? orgAddress.OA_State : desc;
		}

		ZString IEffectiveAddress.Postcode => orgAddress.Postcode;

		ZString IEffectiveAddress.CountryCode => orgAddress.Country?.Code ?? ZString.Empty;

		ZString IEffectiveAddress.GetCountryName(ZString languageCode) => (ZString?)orgAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(languageCode) ?? ZString.Empty;

		ZString IEffectiveAddress.Phone => orgAddress.OA_Phone;

		ZString IEffectiveAddress.EMail => orgAddress.OA_Email;
	}
}
