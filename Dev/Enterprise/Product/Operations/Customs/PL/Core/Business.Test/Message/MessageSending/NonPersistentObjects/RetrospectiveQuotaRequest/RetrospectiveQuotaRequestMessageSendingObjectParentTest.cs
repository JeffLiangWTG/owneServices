using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;
using static Enterprise.Customs.PL.Business.Constants;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(RetrospectiveQuotaRequestMessageSendingObjectParent))]
sealed class RetrospectiveQuotaRequestMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestAnyZCX05()
	{
		var sendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
		var sendingObj = sendingObjectParent.SendingObjectsCollection.OfType<RetrospectiveQuotaRequestMessageSendingObject>().First();
		CombineAssertions(() =>
		{
			sendingObj.Action = ZString.Empty;
			AssertEquals("No ZCX05", false, sendingObjectParent.AnyZCX05);
			sendingObj.Action = MessageSendingObjectActionCodes.ZCX05;
			AssertEquals("With ZCX05", true, sendingObjectParent.AnyZCX05);
		});
	}

	public void TestObjectsToSendType()
	{
		var sendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
		var sendingObj = sendingObjectParent.SendingObjectsCollection.First();

		AssertType<RetrospectiveQuotaRequestMessageSendingObject>(sendingObj);
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
		return new RetrospectiveQuotaRequestMessageSendingObjectParent(declaration);
	}

	RetrospectiveQuotaRequestMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent() => (RetrospectiveQuotaRequestMessageSendingObjectParent)GetNewBusinessObject();
}
