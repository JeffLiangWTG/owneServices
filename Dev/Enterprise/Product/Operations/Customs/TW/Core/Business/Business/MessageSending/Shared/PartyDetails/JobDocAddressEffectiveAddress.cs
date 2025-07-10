using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.TW.Business
{
	public class JobDocAddressEffectiveAddress : IEffectiveAddress
	{
		public JobDocAddressEffectiveAddress(JobDocAddress jobDocAddress)
		{
			this.jobDocAddress = Argument.NotNull(jobDocAddress, nameof(jobDocAddress));
		}

		readonly JobDocAddress jobDocAddress;

		ZString IEffectiveAddress.CompanyName => jobDocAddress.CompanyName;

		ZString IEffectiveAddress.Address1 => jobDocAddress.Address1;

		ZString IEffectiveAddress.Address2 => jobDocAddress.Address2;

		ZString IEffectiveAddress.AdditionalAddressInformation => jobDocAddress.AdditionalAddressInformation;

		ZString IEffectiveAddress.City => jobDocAddress.City;

		ZString IEffectiveAddress.StateCode => jobDocAddress.StateCode;

		ZString IEffectiveAddress.GetStateDescription(ZString languageCode)
		{
			var result = jobDocAddress.Country?.States.Cast<RefCountryStates>().FirstOrDefault(x => x.RW_Code == jobDocAddress.StateCode);
			var desc = (ZString?)result?.RW_DescriptionMultilingual?.GetLocalizedValue(languageCode) ?? ZString.Empty;
			return string.IsNullOrWhiteSpace(desc) ? jobDocAddress.StateCode : desc;
		}

		ZString IEffectiveAddress.Postcode => jobDocAddress.Postcode;

		ZString IEffectiveAddress.CountryCode => jobDocAddress.Country?.Code ?? ZString.Empty;

		ZString IEffectiveAddress.GetCountryName(ZString languageCode) => (ZString?)jobDocAddress.Country?.RN_DescMultilingual?.GetLocalizedValue(languageCode) ?? ZString.Empty;

		ZString IEffectiveAddress.Phone => jobDocAddress.E2_Phone;

		ZString IEffectiveAddress.EMail => jobDocAddress.E2_Email;
	}
}
