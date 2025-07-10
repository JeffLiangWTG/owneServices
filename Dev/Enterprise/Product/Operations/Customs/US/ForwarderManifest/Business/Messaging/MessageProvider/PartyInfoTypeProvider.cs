using System.Collections.ObjectModel;
using CargoWise.Customs.US.MessageContracts.Interfaces;

namespace Enterprise.Customs.US.ForwarderManifest.Business
{
	public class PartyInfoTypeProvider : IPartyInfoType
	{
		public PartyInfoTypeProvider(string partyType, string partyName, string addr1, string addr2, string city, string state, string postal, string country, string phone)
		{
			PartyType = new ManifestStringTypeProvider(partyType);
			PartyIdentificationNumber = new ManifestStringTypeProvider("");
			PartyIdentificationTypeCode = new ManifestStringTypeProvider("");
			PartyName = new ManifestStringTypeProvider(partyName);
			PartyAddressLine1 = new ManifestStringTypeProvider(addr1);
			PartyAddressLine2 = new ManifestStringTypeProvider(addr2);
			PartyAddressLine3 = new ManifestStringTypeProvider("");
			CityName = new ManifestStringTypeProvider(city);
			StateCode = new ManifestStringTypeProvider(state);
			PostalCode = new ManifestStringTypeProvider(postal);
			CountryCode = new ManifestStringTypeProvider(country);
			ContactName = new ManifestStringTypeProvider("");
			ContactPhoneNumber = new ManifestStringTypeProvider(phone);
		}

		public IManifestStringType PartyType { get; }

		public IManifestStringType PartyIdentificationNumber { get; }

		public IManifestStringType PartyIdentificationTypeCode { get; }

		public IManifestStringType PartyName { get; }

		public IManifestStringType PartyAddressLine1 { get; }

		public IManifestStringType PartyAddressLine2 { get; }

		public IManifestStringType PartyAddressLine3 { get; }

		public IManifestStringType CityName { get; }

		public IManifestStringType StateCode { get; }

		public IManifestStringType PostalCode { get; }

		public IManifestStringType CountryCode { get; }

		public IManifestStringType ContactName { get; }

		public IManifestStringType ContactPhoneNumber { get; }

		public Collection<IErrorType> ResponseMessage => new Collection<IErrorType>(System.Array.Empty<ErrorTypeProvider>());
	}
}
