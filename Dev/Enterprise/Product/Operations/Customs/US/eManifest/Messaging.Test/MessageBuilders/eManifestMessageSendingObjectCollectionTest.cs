using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.Customs.US.eManifest.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.Messaging.Testing
{
	[TestedType(typeof(eManifestMessageSendingObjectCollection))]
	sealed class eManifestMessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<eManifestMessageSendingObjectCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals(false, collection.AllowNew);
		}

		public void TestAllowRemove()
		{
			AssertEquals(false, collection.AllowRemove);
		}

		protected override eManifestMessageSendingObjectCollection GetCollectionToTest()
		{
			return new eManifestMessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new eManifestMessageSendingObject(new eManifestMessageSendingObjectParent(Factory.New<Trip>()), MessageTypes.Codes.eManifest, MessageSubTypes.Create, 1);
		}

		protected override void SetUp()
		{
			base.SetUp();
			collection = new eManifestMessageSendingObjectCollection(Factory);
		}
		eManifestMessageSendingObjectCollection collection;
	}
}
