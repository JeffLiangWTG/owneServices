using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbCarrierBookingConsignmentAddressInstructionWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestPopulateConsignmentAddressInstruction
		public void TestPopulateConsignmentAddressInstruction()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var depot1 = Helper.CreateOrganisation("D1");
			depot1.MainAddress.OA_City = "Sydney";
			var depot2 = Helper.CreateOrganisation("D2");
			depot1.MainAddress.OA_City = "Melbourne";
			var pickUpAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, depot1.MainAddress);
			pickUpAddress.LTS_Notes = "PICKUP NOTE";
			var dropOffAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, depot2.MainAddress);
			dropOffAddress.LTS_Notes = "DELIVERY NOTE";

			Factory.SaveForTesting();

			var shipmentDataObject = new DtbCarrierBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(consignment);
			AssertEquals("shipmentDataObject.Notes.Count", 2, shipmentDataObject.NoteCollection.Count);
			#region PickupAddress
			AssertEquals("shipmentDataObject.Pickup.Note.Description", "pickup instructions", shipmentDataObject.NoteCollection[0].Description.ToString().ToLower());
			AssertEquals("shipmentDataObject.Pickup.Note.Text", pickUpAddress.LTS_Notes, shipmentDataObject.NoteCollection[0].NoteText);
			#endregion
			#region DeliveryNote
			AssertEquals("shipmentDataObject.Delivery.Note.Description", "delivery instructions", shipmentDataObject.NoteCollection[1].Description.ToString().ToLower());
			AssertEquals("shipmentDataObject.Delivery.Note.Text", dropOffAddress.LTS_Notes, shipmentDataObject.NoteCollection[1].NoteText);
			#endregion
		}
		#endregion

		#region Helper
		CarrierBookingTestHelper Helper
		{
			get
			{
				return helper ?? (helper = new CarrierBookingTestHelper(Factory.BOFactory));
			}
		}

		CarrierBookingTestHelper helper;
		#endregion
	}
}
