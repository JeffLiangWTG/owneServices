using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AddressFormatterForTest : AddressFormatter
	{
		public AddressFormatterForTest(BusinessObjectFactory factory, OrgAddress address) : base(factory, address)
		{
		}

		public AddressFormatterForTest(BusinessObjectFactory factory, OrgAddress address, bool shouldSetupCountryFromRelatedCountry) : base(factory, address, shouldSetupCountryFromRelatedCountry)
		{
		}

		public AddressFormatterForTest(BusinessObjectFactory factory, string name, string additionalAddressInformation, string address1, string address2, string city, string state, string postCode, MultilingualString countryName, bool includeCountryEvenIfSame)
			: base(factory, name, additionalAddressInformation, address1, address2, city, state, postCode, countryName, includeCountryEvenIfSame)
		{
		}

		public new string CountryName { get => base.CountryName; }

		public new string CountryCode { get => base.CountryCode; }

		public new bool ShouldSetupCountryFromRelatedCountry { get => base.ShouldSetupCountryFromRelatedCountry; }
	}
}
