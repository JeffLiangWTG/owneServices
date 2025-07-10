using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CountryOfRoutingOfConsignmentsProvider))]
sealed class CountryOfRoutingOfConsignmentsProviderTest : Customs.Business.Testing.DataProviderTestCase<CountryOfRoutingOfConsignmentsProvider>
{
	public void TestSequenceNumeric()
	{
		AssertEquals(1, provider.SequenceNumeric);
	}

	public void TestCountry()
	{
		AssertEquals(Core.Constants.CountryCodes.Netherlands, provider.Country);
	}

	protected override void SetUp()
	{
		base.SetUp();

		provider = CreateProvider(Core.Constants.CountryCodes.Netherlands, 1);
	}

	CountryOfRoutingOfConsignmentsProvider provider;

	static CountryOfRoutingOfConsignmentsProvider CreateProvider(ZString countryCode, int sequenceNumber) => new CountryOfRoutingOfConsignmentsProvider(countryCode, sequenceNumber);

	protected override CountryOfRoutingOfConsignmentsProvider GetProvider() => provider;
}
