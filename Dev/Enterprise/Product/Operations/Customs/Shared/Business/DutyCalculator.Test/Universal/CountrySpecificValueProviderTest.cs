using System.Collections;
using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

sealed class CountrySpecificValueProviderTest : TestCase
{
	public void TestRegistry()
	{
		var providers = ObjectFactory.Get<Hashtable>(nameof(Integration.Customs.ICountrySpecificValueProvider));
		var defaultProvider = (ObjectHandle)providers["DEFAULT"];
		AssertNotNull("Default ICountrySpecificValueProvider", defaultProvider);
		AssertEquals("ICountrySpecificValueProvider type", typeof(CountrySpecificValueProvider), defaultProvider.GetObjectType());
	}
}
