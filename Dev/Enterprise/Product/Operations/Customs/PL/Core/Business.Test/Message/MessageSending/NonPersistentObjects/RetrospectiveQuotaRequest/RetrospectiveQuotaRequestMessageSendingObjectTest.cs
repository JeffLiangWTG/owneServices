using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(RetrospectiveQuotaRequestMessageSendingObject))]
sealed class RetrospectiveQuotaRequestMessageSendingObjectTest : NonPersistentBusinessObjectTestCase
{
	public void TestDefaultValues()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		AssertEquals("ZCX05", sendingObj.Action);
	}

	public void TestActionList()
	{
		var sendingObj = GetNewJobDeclarationMessageSendingObject();
		CombineAssertions(() =>
		{
			sendingObj.Header.Declaration.JE_MessageType = MessageTypeList.Codes.Import;
			var list = sendingObj.ActionList;
			AssertEquals("Import - CodesAsString", "ZCX05", list.CodesAsString);
			AssertSame("Import - Cached", list, sendingObj.ActionList);
		});
	}

	public void TestEntryNumberMaxLength() => AssertEquals(18, GetNewJobDeclarationMessageSendingObject().EntryNumberInfo.MaxLength);

	public void TestValidation() => AssertType<RetrospectiveQuotaRequestMessageSendingObjectValidation>(GetNewJobDeclarationMessageSendingObject().Validation);

	RetrospectiveQuotaRequestMessageSendingObject GetNewJobDeclarationMessageSendingObject() => (RetrospectiveQuotaRequestMessageSendingObject)GetNewBusinessObject();

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		var entryHeader = declaration.CustomsEntryHeaders.AddNew();
		var sendingObjectParent = new BaseMessageSendingObjectParent(declaration);
		return new RetrospectiveQuotaRequestMessageSendingObject(entryHeader);
	}
}
