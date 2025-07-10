using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

public class CountryProviderTest : Customs.Business.Testing.DataProviderTestCase<CountryProvider>
{
	public void TestConstructor()
	{
		AssertNoExceptionThrown(() => new CountryProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestCountry() => AssertEquals(CountryCodes.Poland, Provider.Country);

	protected override CountryProvider GetProvider() => new CountryProvider(99, CountryCodes.Poland);
}
