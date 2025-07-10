using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business.Testing;
using Enterprise.Customs.US.Messaging.Business.MessageBuildingBlocks;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageProcessors.Testing
{
	[TestedType(typeof(LineReleaseDeclarationFromShipmentPuller))]
	public class LineReleaseDeclarationFromShipmentPullerTest : NonPersistentBusinessObjectTestCase
	{
		public void TestShipmentsAdditionalFilter()
		{
			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_JS = shipment2.PK;

			LineReleaseDeclarationFromShipmentPuller puller = (LineReleaseDeclarationFromShipmentPuller)GetNewBusinessObject();
			ZQuery query = new ZQuery(JobShipmentSchema.PK, shipment1.PK);
			query.AddToFilter(JoinCondition.Or, JobShipmentSchema.PK, shipment2.PK);
			ForwardingShipmentCollection shipments = puller.Shipments;
			shipments.Load(query);
			AssertEquals(2, shipments.Count);
			AssertCollectionContains(shipment1, shipments);
			AssertCollectionContains(shipment2, shipments);
			AssertEquals("This Shipment has already a Declaration linked to it; please select another Shipment.", shipments.GetAllNotificationsWhenAdditionalFilterNotMet(shipment1));
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			DeclarationTestHelper.SetEntryFilerCode("XJ5");
			DeclarationTestHelper.SetProcessingDistrictPortCode("8888");
			StringBuilder messageTextBuilder = new StringBuilder();
			messageTextBuilder.Append("B018888XJ5XR".PadRight(80));
			messageTextBuilder.Append("X10A95-2041006008888XJ5 412412341206051019L89".PadRight(80));
			messageTextBuilder.Append("X20870323              NIS1NIS5231MPVMX        00000015NUMMXNISMEX1958MEX".PadRight(80));
			messageTextBuilder.Append("X25UPRR106695071114                00000015".PadRight(80));
			messageTextBuilder.Append("X400001000100000002".PadRight(80));
			messageTextBuilder.Append("Y018888XJ5XR00004".PadRight(80));
			LineReleaseMQEDIMessage message = Factory.New<LineReleaseMQEDIMessage>();
			message.EM_MessageType = ApplicationIdentifierCodeList.Codes.LineRelease;
			message.EM_ApplicationReference = "41241234";
			message.EM_ReceiveTransmit = MQEDIMessage.Direction.Receive;
			message.EM_MessageText = messageTextBuilder.ToString();
			Factory.Save();
			return new LineReleaseDeclarationFromShipmentPuller(new LineReleaseDeclarationCreator(message));
		}
	}
}
