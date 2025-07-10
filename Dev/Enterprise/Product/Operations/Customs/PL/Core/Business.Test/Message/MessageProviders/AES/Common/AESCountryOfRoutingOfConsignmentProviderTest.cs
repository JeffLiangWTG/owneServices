using System;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESCountryOfRoutingOfConsignmentProviderTest : Customs.Business.Testing.DataProviderTestCase<AESCountryOfRoutingOfConsignmentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null Transport", "Value cannot be null.\r\nParameter name: itineraryCountry",
			() => new AESCountryOfRoutingOfConsignmentProvider(null, 1));
	}

	public void TestSequenceNumber() => AssertEquals(999, GetProvider().SequenceNumber);

	public void TestCountry()
	{
		country.CY_Code = CountryCodes.Poland;
		AssertEquals(CountryCodes.Poland, GetProvider().Country);
	}

	protected override AESCountryOfRoutingOfConsignmentProvider GetProvider() => new AESCountryOfRoutingOfConsignmentProvider(country, 999);

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Export;
		country = declaration.ItineraryCountries.AddNew();
	}
	ItineraryCountry country;
}
