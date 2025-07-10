using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationSupportingDocSendingObjectCollection))]
	sealed class SupportingDocSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationSupportingDocSendingObjectCollection>
	{
		public void TestAllowNew()
		{
			Assert(GetCollectionToTest().AllowNew);
		}

		protected override JobDeclarationSupportingDocSendingObjectCollection GetCollectionToTest()
		{
			var declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			return new JobDeclarationSupportingDocSendingObjectCollection(declaration);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return SupportingDocSendingObject.New(declaration);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			_ = declaration.CustomsEntryHeaders.AddNew();
		}
		BaseJobDeclaration declaration;
	}
}
