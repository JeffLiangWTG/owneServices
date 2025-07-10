using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(PreviousDocumentProvider))]
sealed class PreviousDocumentProviderTest : DocumentProviderAbstractTest<PreviousDocumentProvider>
{
	public void TestComplementOfInformation()
	{
		info.CSI_ReferenceNumber2 = "PrevDocCOI";
		AssertEquals("PrevDocCOI", Provider.ComplementOfInformation);
	}

	protected override string DocType => CusSupportingInfoTypeList.Codes.PreviousDocument;
}
