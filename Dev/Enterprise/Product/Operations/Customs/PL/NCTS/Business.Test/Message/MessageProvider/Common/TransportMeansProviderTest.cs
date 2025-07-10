using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportMeansProvider>
{
	public void TestTypeOfIdentification() => AssertEquals("ID", Provider.TypeOfIdentification);

	public void TestIdentificationNumber() => AssertEquals("IdentificationNumber", Provider.IdentificationNumber);

	public void TestNationality() => AssertEquals(CountryCodes.Poland, Provider.Nationality);

	protected override TransportMeansProvider GetProvider() => new TransportMeansProvider("ID", "IdentificationNumber", CountryCodes.Poland);
}
