using static Enterprise.Core.Constants;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class DepartureTransportMeansProviderTest : Customs.Business.Testing.DataProviderTestCase<DepartureTransportMeansProvider>
{
	public void TestConstructor()
	{
		AssertNoExceptionThrown(() => new DepartureTransportMeansProvider(1, null, null, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestTypeOfIdentification() => AssertEquals("ID", Provider.TypeOfIdentification);

	public void TestIdentificationNumber() => AssertEquals("IdentificationNumber", Provider.IdentificationNumber);

	public void TestNationality() => AssertEquals(CountryCodes.Poland, Provider.Nationality);

	protected override DepartureTransportMeansProvider GetProvider() => new DepartureTransportMeansProvider(99, "ID", "IdentificationNumber", CountryCodes.Poland);
}
