using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.PL.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.PL.Business.Testing;

[TestedType(typeof(CustomsDeclarationMessageSendingObjectParent))]
sealed class CustomsDeclarationMessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
{
	public void TestObjectsToSendType()
	{
		var sendingObjectParent = GetJobDeclarationMessageSendingObjectParent();
		var sendingObj = sendingObjectParent.SendingObjectsCollection.First();

		AssertType<CustomsDeclarationMessageSendingObject>(sendingObj);
	}

	protected override BusinessObject GetNewBusinessObject() => new CustomsDeclarationMessageSendingObjectParent(declaration);

	CustomsDeclarationMessageSendingObjectParent GetJobDeclarationMessageSendingObjectParent() => (CustomsDeclarationMessageSendingObjectParent)GetNewBusinessObject();

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.NewWithValidTestData<JobDeclaration>();
		declaration.CustomsEntryHeaders.AddNew();
	}

	JobDeclaration declaration;
}
