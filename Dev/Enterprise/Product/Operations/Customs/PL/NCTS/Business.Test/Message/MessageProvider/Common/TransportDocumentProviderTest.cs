using System;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class TransportDocumentProviderTest : Customs.Business.Testing.DataProviderTestCase<TransportDocumentProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null NctsAdditionalInfo", "Value cannot be null.\r\nParameter name: transportDocument", () => new TransportDocumentProvider(1, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestDocumentType() => AssertEquals("123", Provider.DocumentType);

	public void TestReferenceNumber() => AssertEquals("referenceNumber", Provider.ReferenceNumber);

	protected override TransportDocumentProvider GetProvider() => new TransportDocumentProvider(99, nctsAdditionalInfo);

	protected override void SetUp()
	{
		base.SetUp();
		nctsAdditionalInfo = Factory.New<NctsAdditionalInfo>();
		nctsAdditionalInfo.CSI_Code = "123";
		nctsAdditionalInfo.CSI_ReferenceNumber = "referenceNumber";
	}
	NctsAdditionalInfo nctsAdditionalInfo;
}
