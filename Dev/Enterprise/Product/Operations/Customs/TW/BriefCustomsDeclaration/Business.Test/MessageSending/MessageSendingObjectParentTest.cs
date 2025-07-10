using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.TW.Business;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(MessageSendingObjectParent))]
	sealed class MessageSendingObjectParentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return new MessageSendingObjectParent(header, MessageTypeList.Codes.IBC);
		}

		public void TestSendingObjectType()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var sendingObjectParent = new MessageSendingObjectParent(header, MessageTypeList.Codes.IBC);
			AssertType<N5135MessageSendingObject>(sendingObjectParent.SendingObjectsCollection.FirstOrDefault());

			sendingObjectParent = new MessageSendingObjectParent(header, MessageTypeList.Codes.EBC);
			AssertType<N5205MessageSendingObject>(sendingObjectParent.SendingObjectsCollection.FirstOrDefault());
		}

		public void TestSendingObjectsCollectionType()
		{
			var sendingObjectParent = GetNewBusinessObject() as MessageSendingObjectParent;
			AssertType<MessageSendingObjectCollection>(sendingObjectParent.SendingObjectsCollection);
		}

		public void TestSecurityRightToSendWithMessageErrors()
		{
			var sendingObjectParent = GetNewBusinessObject() as MessageSendingObjectParent;
			AssertEquals(Env.Security.CustomsDeclarationSendWithMessageErrors, sendingObjectParent.SecurityCheckpointToSendWithMessageError);
		}

		public void TestMessageSendingObjectParentDefaultValues()
		{
			var sendingObjectParent = GetNewBusinessObject() as MessageSendingObjectParent;
			AssertEquals("Count of SendingObjectsCollection", 1, sendingObjectParent.SendingObjectsCollection.Count);
		}
	}
}
