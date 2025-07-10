using Enterprise.Customs.Business.Testing;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class AESCustomsOfficeProviderTest : DataProviderTestCase<AESCustomsOfficeProvider>
{
	public void TestReferenceNumber()
	{
		AssertEquals("1234", Provider.ReferenceNumber);
	}

	protected override AESCustomsOfficeProvider GetProvider() => new AESCustomsOfficeProvider("1234");
}
