using System;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business.Declaration.MultiLineAddInfos;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class CC044CAdditionalReferenceProviderTest : DataProviderTestCase<CC044CAdditionalReferenceProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null AdditionalInfo", "Value cannot be null.\r\nParameter name: additionalReference", () => new CC044CAdditionalReferenceProvider(99, null));
	}

	public void TestSequenceNumber() => AssertEquals("99", Provider.SequenceNumber);

	public void TestDocumentType() => AssertEquals("123", Provider.DocumentType);

	public void TestReferenceNumber() => AssertEquals("ReferenceNumber1", Provider.ReferenceNumber);

	protected override CC044CAdditionalReferenceProvider GetProvider() => new CC044CAdditionalReferenceProvider(99, additionalReference);

	protected override void SetUp()
	{
		base.SetUp();
		additionalReference = Factory.New<AdditionalInfo>();
		additionalReference.CSI_Code = "123";
		additionalReference.CSI_ReferenceNumber = "ReferenceNumber1";
		additionalReference.CSI_ReferenceNumber2 = "ReferenceNumber2";
	}
	AdditionalInfo additionalReference;
}
