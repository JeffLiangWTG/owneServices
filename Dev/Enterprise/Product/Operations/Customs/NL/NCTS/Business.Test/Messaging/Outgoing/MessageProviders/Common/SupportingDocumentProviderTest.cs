using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(SupportingDocumentProvider))]
sealed class SupportingDocumentProviderTest : DocumentProviderAbstractTest<SupportingDocumentProvider>
{
	public void TestComplementOfInformation()
	{
		info.CSI_ReferenceNumber2 = "SupDocRef";
		AssertEquals("SupDocRef", Provider.ComplementOfInformation);
	}

	public void TestDocumentLineItemNumber()
	{
		info.CSI_ItemNumber = 4;
		AssertEquals(4, Provider.DocumentLineItemNumber);
	}

	protected override string DocType => CusSupportingInfoTypeList.Codes.SupportingDocument;
}
