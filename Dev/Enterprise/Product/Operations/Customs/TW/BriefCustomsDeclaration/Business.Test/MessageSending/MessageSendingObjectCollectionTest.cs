using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectCollection))]
	sealed class MessageSendingObjectCollectionTest : NonPersistentBusinessObjectCollectionTestCase<MessageSendingObjectCollection>
	{
		protected override MessageSendingObjectCollection GetCollectionToTest()
		{
			return new MessageSendingObjectCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new N5205MessageSendingObject(Factory.NewWithValidTestData<AsycudaManifestHeader>());
		}
	}
}
