using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class OrgTranslatedAddressEffectiveAddress : IEffectiveAddress
	{
		public OrgTranslatedAddressEffectiveAddress(OrgTranslatedAddress address)
		{
			orgTranslatedAddress = Argument.NotNull(address, nameof(address));
			Argument.NotNull(address.Address, nameof(address.Address));
		}

		readonly OrgTranslatedAddress orgTranslatedAddress;

		ZString IEffectiveAddress.CompanyName => orgTranslatedAddress.CompanyName;

		ZString IEffectiveAddress.Address1 => orgTranslatedAddress.Address1;

		ZString IEffectiveAddress.Address2 => orgTranslatedAddress.Address2;

		ZString IEffectiveAddress.AdditionalAddressInformation => orgTranslatedAddress.OTA_AdditionalAddressInformation;

		ZString IEffectiveAddress.City => orgTranslatedAddress.City;

		ZString IEffectiveAddress.StateCode => orgTranslatedAddress.StateCode;

		ZString IEffectiveAddress.GetStateDescription(ZString languageCode)
		{
			var result = orgTranslatedAddress.Country?.States.Cast<RefCountryStates>().FirstOrDefault(x => x.RW_Code == orgTranslatedAddress.OTA_State);
			var desc = (ZString?)result?.RW_DescriptionMultilingual?.GetLocalizedValue(languageCode) ?? ZString.Empty;
			return string.IsNullOrWhiteSpace(desc) ? orgTranslatedAddress.OTA_State : desc;
		}

		ZString IEffectiveAddress.Postcode => orgTranslatedAddress.Postcode;

		ZString IEffectiveAddress.CountryCode => orgTranslatedAddress.Country?.Code ?? ZString.Empty;

		ZString IEffectiveAddress.GetCountryName(ZString languageCode) => (ZString?)orgTranslatedAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(languageCode) ?? ZString.Empty;

		ZString IEffectiveAddress.Phone => orgTranslatedAddress.Address.OA_Phone;

		ZString IEffectiveAddress.EMail => orgTranslatedAddress.Address.OA_Email;
	}
}
