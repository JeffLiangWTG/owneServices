using System;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CSupportingDocumentProvider))]
sealed class CC044CSupportingDocumentProviderTest : DocumentProviderAbstractTest<CC044CSupportingDocumentProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CSupportingDocumentProvider(null));

	public override void TestReferenceNumber() => CombineAssertions(() =>
	{
		info.CSI_ReferenceNumber = "reference";
		AssertNullOrEmpty(Provider.ReferenceNumber);
		info.CSI_Status = "NEW";
		AssertEquals("reference", Provider.ReferenceNumber);
	});

	public override void TestType() => CombineAssertions(() =>
	{
		info.CSI_Code = "CODE";
		AssertNullOrEmpty(Provider.Type);
		info.CSI_Status = "NEW";
		AssertEquals("CODE", Provider.Type);
	});

	public void TestDocumentLineItemNumber()
	{
		AssertEquals(0, Provider.DocumentLineItemNumber);
	}

	public void TestComplementOfInformation()
	{
		info.CSI_ReferenceNumber2 = "SupDocRef";
		AssertNullOrEmpty(Provider.ComplementOfInformation);
		info.CSI_Status = "NEW";
		AssertEquals("SupDocRef", Provider.ComplementOfInformation);
	}

	protected override string DocType => CusSupportingInfoTypeList.Codes.SupportingDocument;
}
