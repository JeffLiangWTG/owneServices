using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ExportReleaseMessageCollectionView))]
	sealed class ExportReleaseMessageCollectionViewTest : BusinessObjectCollectionViewTestCase<ExportReleaseMessageCollectionView>
	{
		public void TestCollectionFiltering()
		{
			const string messageTypeDLO = "DLO";
			const string messageTypeGPM = "GPM";
			const string messageTypeIMP = "IMP";

			var ediMessage1 = (EDIMessage)GetNewElementToAddToTheCollection();
			ediMessage1.EM_MessageType = messageTypeDLO;
			var ediMessage2 = (EDIMessage)GetNewElementToAddToTheCollection();
			ediMessage2.EM_MessageType = messageTypeGPM;
			var collection = GetCollectionToTest();
			AssertEquals(0, collection.Count);
			ediMessage2.EM_MessageType = messageTypeIMP;
			AssertEquals(1, collection.Count);
			var ediMessages = collection.Cast<EDIMessage>();
			AssertNotNull(ediMessages.FirstOrDefault(x => x.EM_MessageType == messageTypeIMP));
			AssertNull(ediMessages.FirstOrDefault(x => x.EM_MessageType == messageTypeDLO));
			ediMessage1.EM_MessageType = messageTypeIMP;
			AssertEquals(2, collection.Count);
		}

		protected override ExportReleaseMessageCollectionView GetCollectionToTest() => Shipment.ExportReleaseMessages;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var message = Shipment.Messages.AddNew();
			message.EM_ApplicationCode = "ILC";
			return message;
		}

		protected override void SetUp()
		{
			base.SetUp();
			Shipment = Factory.New<ForwardingShipment>();
		}

		ForwardingShipment Shipment;
	}
}
