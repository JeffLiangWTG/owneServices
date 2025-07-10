using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	interface IEffectiveAddress
	{
		ZString CompanyName { get; }

		ZString Address1 { get; }

		ZString Address2 { get; }

		ZString AdditionalAddressInformation { get; }

		ZString City { get; }

		ZString StateCode { get; }

		ZString GetStateDescription(ZString languageCode);

		ZString Postcode { get; }

		ZString CountryCode { get; }

		ZString GetCountryName(ZString languageCode);

		ZString Phone { get; }

		ZString EMail { get; }
	}
}
