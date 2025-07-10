using System.Collections;
using CargoWise.Application;
using NUnit.Framework;

namespace Enterprise.Customs.DutyCalculator.Testing;

sealed class CustomsRateUnitConvertersProviderTest : TestCase
{
	public void TestRegistry()
	{
		var providers = ObjectFactory.Get<Hashtable>(nameof(Integration.Customs.ICustomsRateUnitConvertersProvider));
		var defaultProvider = (ObjectHandle)providers["DEFAULT"];
		AssertNotNull("Default ICustomsRateUnitConvertersProvider", defaultProvider);
		AssertEquals("ICustomsRateUnitConvertersProvider type", typeof(CustomsRateUnitConvertersProvider), defaultProvider.GetObjectType());
	}
}
