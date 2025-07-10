using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectCollection))]
	sealed class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection>
	{
		public void TestAllowNew()
		{
			Assert("AllowNew should be false", !GetCollectionToTest().AllowNew);
		}

		protected override MessageSendingObjectCollection GetCollectionToTest()
		{
			return new MessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var bill = Factory.NewWithValidTestData<AsycudaBill>();
			return new MessageSendingObject(bill);
		}
	}
}
