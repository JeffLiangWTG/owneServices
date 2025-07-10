using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(CUSDECItemDetailsDocumentsWrapper))]
sealed class CUSDECItemDetailsDocumentsWrapperTest : TestCaseWithFactory
{
	public void TestConstructor() => CombineAssertions(() =>
	{
		AssertExceptionThrown<ArgumentNullException>("When entryLineFee is null", () => _ = new CUSDECItemDetailsDocumentsWrapper(null));
		AssertNoExceptionThrown("When happy path", () => _ = new CUSDECItemDetailsDocumentsWrapper(document));
	});

	public void TestDocumentCode()
	{
		document.CSI_Code = "SER";
		var wrapper = CUSDECItemDetailsDocuments();
		AssertEquals("DocumentCode", "SER", wrapper.DocumentCode);
	}

	public void TestDocumenReference()
	{
		document.CSI_ReferenceNumber = "2025/1234";
		var wrapper = CUSDECItemDetailsDocuments();
		AssertEquals("ReferenceNumber", "2025/1234", wrapper.DocumentNumberOrText);
	}

	ICUSDECMessageDataProvider.IItemDetailsSupportingDocuments CUSDECItemDetailsDocuments()
	{
		return new CUSDECItemDetailsDocumentsWrapper(document);
	}

	protected override void SetUp()
	{
		base.SetUp();
		document = Factory.New<SupportingDocument>();
	}

	SupportingDocument document;
}
