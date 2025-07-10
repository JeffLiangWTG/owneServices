using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	[TestedType(typeof(JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject>))]
	class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject>>
	{
		public virtual void TestAllowNewCore()
		{
			var testItem = new JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject>(Factory);
			Assert(!testItem.AllowNew);
		}

		#region Overrides of BusinessObjectCollectionBaseTestCase<BusinessObjectCollection>

		protected override JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject> GetCollectionToTest()
		{
			return new JobDeclarationMessageSendingObjectCollection<JobDeclarationMessageSendingObject>(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new JobDeclarationMessageSendingObject(Factory.New<CusEntryHeader>());
		}

		#endregion
	}
}
