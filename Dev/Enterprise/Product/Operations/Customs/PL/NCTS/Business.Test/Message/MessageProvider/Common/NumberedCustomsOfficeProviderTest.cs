using System;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class NumberedCustomsOfficeProviderTest : Customs.Business.Testing.DataProviderTestCase<NumberedCustomsOfficeProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsEuOfficeCode", "Value cannot be null.\r\nParameter name: customsOffice", () => new NumberedCustomsOfficeProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	protected override NumberedCustomsOfficeProvider GetProvider() => new NumberedCustomsOfficeProvider(99, office);

	protected override void SetUp()
	{
		base.SetUp();
		office = Factory.New<NctsPLOfficeCode>();
	}
	NctsPLOfficeCode office;
}
