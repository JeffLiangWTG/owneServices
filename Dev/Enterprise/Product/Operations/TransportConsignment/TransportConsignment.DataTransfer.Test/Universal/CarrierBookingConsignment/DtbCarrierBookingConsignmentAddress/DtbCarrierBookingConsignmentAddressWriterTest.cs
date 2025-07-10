using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.TransportConsignment.DataTransfer.Universal.Testing
{
	class DtbCarrierBookingConsignmentAddressWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		#region TestPopulateConsignmentAddress
		public void TestPopulateConsignmentAddress()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var depot1 = Helper.CreateOrganisation("D1");
			depot1.MainAddress.OA_City = "Sydney";
			var depot2 = Helper.CreateOrganisation("D2");
			depot1.MainAddress.OA_City = "Melbourne";
			var pickUpAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp, depot1.MainAddress);
			var dropOffAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.Delivery, depot2.MainAddress);
			Factory.SaveForTesting();

			var shipmentDataObject = new DtbCarrierBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(consignment);

			AssertEquals("shipmentDataObject.WayBillNumber", consignment.LTC_ConnoteNumber, shipmentDataObject.WayBillNumber);
			AssertEquals("shipmentDataObject.OrganizationAddressCollection.Count", 2, shipmentDataObject.OrganizationAddressCollection.Count);
			#region PickupAddress
			AssertEquals("shipmentDataObject.PIC.AddressType", "PickUpAddress", shipmentDataObject.OrganizationAddressCollection[0].AddressType);
			AssertEquals("shipmentDataObject.PIC.Address1", pickUpAddress.Address.E2_Address1, shipmentDataObject.OrganizationAddressCollection[0].Address1);
			AssertEquals("shipmentDataObject.PIC.Address2", pickUpAddress.Address.E2_Address2, shipmentDataObject.OrganizationAddressCollection[0].Address2);
			AssertEquals("shipmentDataObject.PIC.City", pickUpAddress.Address.E2_City, shipmentDataObject.OrganizationAddressCollection[0].City);
			AssertEquals("shipmentDataObject.PIC.Email", pickUpAddress.Address.E2_Email, shipmentDataObject.OrganizationAddressCollection[0].Email);
			AssertEquals("shipmentDataObject.PIC.Fax", pickUpAddress.Address.E2_Fax, shipmentDataObject.OrganizationAddressCollection[0].Fax);
			AssertEquals("shipmentDataObject.PIC.Phone", pickUpAddress.Address.E2_Phone, shipmentDataObject.OrganizationAddressCollection[0].Phone);
			#endregion
			#region DropoffAddress
			AssertEquals("shipmentDataObject.DLV.AddressType", "DropOffAddress", shipmentDataObject.OrganizationAddressCollection[1].AddressType);
			AssertEquals("shipmentDataObject.DLV.Address1", dropOffAddress.Address.E2_Address1, shipmentDataObject.OrganizationAddressCollection[1].Address1);
			AssertEquals("shipmentDataObject.DLV.Address2", dropOffAddress.Address.E2_Address2, shipmentDataObject.OrganizationAddressCollection[1].Address2);
			AssertEquals("shipmentDataObject.DLV.City", dropOffAddress.Address.E2_City, shipmentDataObject.OrganizationAddressCollection[1].City);
			AssertEquals("shipmentDataObject.DLV.Email", dropOffAddress.Address.E2_Email, shipmentDataObject.OrganizationAddressCollection[1].Email);
			AssertEquals("shipmentDataObject.DLV.Fax", dropOffAddress.Address.E2_Fax, shipmentDataObject.OrganizationAddressCollection[1].Fax);
			AssertEquals("shipmentDataObject.DLV.Phone", dropOffAddress.Address.E2_Phone, shipmentDataObject.OrganizationAddressCollection[1].Phone);
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
