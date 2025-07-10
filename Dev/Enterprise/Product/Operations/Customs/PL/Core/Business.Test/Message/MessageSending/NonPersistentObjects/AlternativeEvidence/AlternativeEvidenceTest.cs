using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(AlternativeEvidence))]
sealed class AlternativeEvidenceTest : NonPersistentBusinessObjectTestCase
{
	public void TestEvidenceTypeCaption() => AssertEquals("Alternative Evidence Type", DataBoundResourceStrings.GetDataForProperty(alternativeEvidence.EvidenceTypeInfo).Caption);

	public void TestTransportDocumentType_ReadOnly() => CombineAssertions(() =>
	{
		var propertyInfo = alternativeEvidence.DocTypeInfo;
		AssertEquals("Default: readonly", true, propertyInfo.ReadOnly);

		alternativeEvidence.EvidenceType = "11";
		AssertEquals("Evidence type is 11, editable", false, propertyInfo.ReadOnly);

		alternativeEvidence.EvidenceType = "1";
		AssertEquals("Evidence type is 1, readonly", true, propertyInfo.ReadOnly);
	});

	public void TestTransportDocumentType_Caption() => AssertEquals("Transport Document Type", DataBoundResourceStrings.GetDataForProperty(alternativeEvidence.DocTypeInfo).Caption);

	protected override BusinessObject GetNewBusinessObject() => alternativeEvidence;

	protected override void SetUp()
	{
		base.SetUp();
		var declaration = Factory.New<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		var messageSendingObjectParent = new BaseMessageSendingObjectParent(declaration);
		alternativeEvidence = new AlternativeEvidence(messageSendingObjectParent);
	}

	AlternativeEvidence alternativeEvidence;
}
