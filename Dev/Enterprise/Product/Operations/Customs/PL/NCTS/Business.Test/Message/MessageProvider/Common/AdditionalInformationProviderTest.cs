using System;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class AdditionalInformationProviderTest : Customs.Business.Testing.DataProviderTestCase<AdditionalInformationProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsAdditionalInfo", "Value cannot be null.\r\nParameter name: additionalInformation", () => new AdditionalInformationProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestCode() => AssertEquals("123", Provider.Code);

	public void TestText() => AssertEquals("someDescription", Provider.Text);

	protected override AdditionalInformationProvider GetProvider() => new AdditionalInformationProvider(99, nctsAdditionalInfo);

	protected override void SetUp()
	{
		base.SetUp();
		nctsAdditionalInfo = Factory.New<NctsAdditionalInfo>();
		nctsAdditionalInfo.CSI_Code = "123";
		nctsAdditionalInfo.CSI_Description = "someDescription";
	}
	NctsAdditionalInfo nctsAdditionalInfo;
}
