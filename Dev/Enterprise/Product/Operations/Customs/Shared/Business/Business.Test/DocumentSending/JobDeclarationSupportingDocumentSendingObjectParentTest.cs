using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationSupportingDocSendingObjectParent))]
	sealed class JobDeclarationSupportingDocumentSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		public void TestSendingObjectsCollection()
		{
			var testWrapper = new JobDeclarationSupportingDocSendingObjectParent(declaration);
			AssertEquals(0, testWrapper.SendingObjectsCollection.Count);

			declaration.CustomsEntryHeaders.AddNew();
			declaration.CustomsEntryHeaders.AddNew();
			AssertEquals(0, testWrapper.SendingObjectsCollection.Count);

			testWrapper.SendingObjectsCollection.AddNew();
			testWrapper.SendingObjectsCollection.AddNew();
			AssertEquals(2, testWrapper.SendingObjectsCollection.Count);
		}

		public void TestSecurityCheckpointToSendWithMessageError()
		{
			var testWrapper = new JobDeclarationSupportingDocSendingObjectParent(declaration);
			AssertEquals(Env.Security.CustomsDeclarationSendWithMessageErrors, testWrapper.SecurityCheckpointToSendWithMessageError);
		}

		protected override BusinessObject GetNewBusinessObject() => new JobDeclarationSupportingDocSendingObjectParent(declaration);

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
		}
		BaseJobDeclaration declaration;
	}
}
