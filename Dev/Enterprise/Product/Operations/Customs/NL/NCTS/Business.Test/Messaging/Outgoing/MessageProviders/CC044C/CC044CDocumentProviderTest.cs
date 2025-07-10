using System;
using Enterprise.Customs.Common.EU;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(CC044CDocumentProvider))]
sealed class CC044CDocumentProviderTest : DocumentProviderAbstractTest<CC044CDocumentProvider>
{
	public void TestConstructor() => AssertExceptionThrown<ArgumentNullException>(() => new CC044CDocumentProvider(null));

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

	protected override string DocType => CusSupportingInfoTypeList.Codes.AdditionalInfo;
}
