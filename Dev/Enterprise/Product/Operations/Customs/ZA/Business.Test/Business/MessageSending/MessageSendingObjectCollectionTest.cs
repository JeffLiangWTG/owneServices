using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectCollection))]
	sealed class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection>
	{
		public void TestAllowNew()
		{
			Assert(!GetCollectionToTest().AllowNew);
		}

		protected override MessageSendingObjectCollection GetCollectionToTest()
		{
			return new MessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new MessageSendingObject(header);
		}

		protected override void SetUp()
		{
			base.SetUp();
			declaration = Factory.NewWithValidTestData<JobDeclaration>();
			header = declaration.CustomsEntryHeaders.AddNew();
		}

		JobDeclaration declaration;
		CusEntryHeader header;
	}
}
