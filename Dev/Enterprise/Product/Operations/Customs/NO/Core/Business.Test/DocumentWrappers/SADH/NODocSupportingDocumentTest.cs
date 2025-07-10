using CargoWise.Types;
using Enterprise.DocumentWrappers;
using Enterprise.DocumentWrappers.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NO.Business.Testing;

[TestedType(typeof(NODocSupportingDocument))]
sealed class NODocSupportingDocumentTest : DocBaseWrapperTest
{
	protected override DocBaseWrapper GetNewDocumentWrapper() => NODocSupportingDocument.New(supportingDocument, Factory);

	NODocSupportingDocument CreateNewNODocSupportingDocumentWrapper() => NODocSupportingDocument.New(supportingDocument, Factory);

	public void TestReferenceType()
	{
		AssertEquals("[PRE-CONDITION] ReferenceType", ZString.Empty, CreateNewNODocSupportingDocumentWrapper().ReferenceType);
		supportingDocument.CSI_Code = "ABC";
		AssertEquals("ABC", CreateNewNODocSupportingDocumentWrapper().ReferenceType);
	}

	public void TestReferenceNumber()
	{
		AssertEquals("[PRE-CONDITION] ReferenceNumber", ZString.Empty, CreateNewNODocSupportingDocumentWrapper().ReferenceNumber);
		supportingDocument.CSI_ReferenceNumber = "2024/123";
		AssertEquals("2024/123", CreateNewNODocSupportingDocumentWrapper().ReferenceNumber);
	}

	protected override void SetUp()
	{
		base.SetUp();
		supportingDocument = Factory.New<SupportingDocument>();
	}
	SupportingDocument supportingDocument;
}
