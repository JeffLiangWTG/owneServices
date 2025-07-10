using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.TransportConsignment.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Writing;
using Enterprise.UniversalDataBuss.Management.Testing;

namespace Enterprise.TransportConsignment.DataTransfer.Testing
{
	public class DtbCarrierBookingObjectWriterTest : TestCaseWithFactoryAndMessagingHelpers
	{
		public void TestGetDataObject_Address()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var depot1 = Helper.CreateOrganisation("D1");
			depot1.MainAddress.OA_City = "Sydney";
			var depot2 = Helper.CreateOrganisation("D2");
			depot1.MainAddress.OA_City = "Melbourne";
			var pickUpAddress = Helper.CreateConsignmentAddress(consignment, ConsignmentAddressTypes.Codes.PickUp,depot1.MainAddress);
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

		public void TestGetDataObject_Instructions()
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
			consignment.Notes.AddNew(true, "Note 1 description", "note 1 Text");
			consignment.Notes.AddNew(true, "Note 2 description", "note 2 Text");
			Factory.SaveForTesting();

			var shipmentDataObject = new DtbCarrierBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(consignment);
			AssertEquals("shipmentDataObject.Notes.Count", 4, shipmentDataObject.NoteCollection.Count);
			#region PickupAddress
			AssertEquals("shipmentDataObject.Pickup.Note.Description", "pickup instructions", shipmentDataObject.NoteCollection[0].Description.ToString().ToLower());
			AssertEquals("shipmentDataObject.Pickup.Note.Text",pickUpAddress.LTS_Notes, shipmentDataObject.NoteCollection[0].NoteText);
			#endregion
			#region DeliveryNote
			AssertEquals("shipmentDataObject.Delivery.Note.Description", "delivery instructions", shipmentDataObject.NoteCollection[1].Description.ToString().ToLower());
			AssertEquals("shipmentDataObject.Delivery.Note.Text", dropOffAddress.LTS_Notes, shipmentDataObject.NoteCollection[1].NoteText);
			#endregion
			#region CustomNote1
			AssertEquals("shipmentDataObject.Delivery.Note.Description", "Note 1 description", shipmentDataObject.NoteCollection[2].Description.ToString());
			AssertEquals("shipmentDataObject.Delivery.Note.Text", "note 1 Text", shipmentDataObject.NoteCollection[2].NoteText);
			#endregion
			#region CustomNote2
			AssertEquals("shipmentDataObject.Delivery.Note.Description", "Note 2 description", shipmentDataObject.NoteCollection[3].Description.ToString());
			AssertEquals("shipmentDataObject.Delivery.Note.Text", "note 2 Text", shipmentDataObject.NoteCollection[3].NoteText);
			#endregion

		}

		public void TestGetDataObject_CarrierServiceLevel()
		{
			var consignment = Helper.CreateConsignment("CS001");
			var depot1 = Helper.CreateOrganisation("D1");
			depot1.MainAddress.OA_City = "Sydney";
			var depot2 = Helper.CreateOrganisation("D2");
			depot1.MainAddress.OA_City = "Melbourne";
			consignment.LTC_PL_NKCarrierServiceLevel = "ECO";
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var serviceLevel = carrier.MiscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "ECO";
			serviceLevel.PL_CarrierServiceLevelDescription = "service level description";
			serviceLevel.PL_CarrierServiceCode = "1234";
			serviceLevel.PL_ChargeCode = "1212121212";
			serviceLevel.PL_ProductCode = "555555";
			serviceLevel.PL_APProfileID = "AP3333";
			serviceLevel.PL_IsSignatureRequired = true;
			consignment.LTC_OH_Carrier = carrier.PK;
			Factory.SaveForTesting();

			var shipmentDataObject = new DtbCarrierBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(consignment);
			AssertEquals("shipmentDataObject.CarrierServiceLevel.CarrierServiceCode", serviceLevel.PL_CarrierServiceCode, shipmentDataObject.CarrierServiceLevel.CarrierServiceCode);
			AssertEquals("serviceLevelDataObject.Code", serviceLevel.PL_Code, shipmentDataObject.CarrierServiceLevel.Code);
			AssertEquals("serviceLevelDataObject.Description", serviceLevel.PL_CarrierServiceLevelDescription, shipmentDataObject.CarrierServiceLevel.Description);
			AssertEquals("serviceLevelDataObject.CarrierProductCode", serviceLevel.PL_ProductCode, shipmentDataObject.CarrierServiceLevel.CarrierProductCode);
			AssertEquals("serviceLevelDataObject.CarrierChargeCode", serviceLevel.PL_ChargeCode, shipmentDataObject.CarrierServiceLevel.CarrierChargeCode);
			AssertEquals("serviceLevelDataObject.CarrierProfileID", serviceLevel.PL_APProfileID, shipmentDataObject.CarrierServiceLevel.CarrierProfileID);
			AssertEquals("whsDocketData.IsSignatureRequired", true, shipmentDataObject.IsSignatureRequired);
		}

		public void TestGetDataObject_Packages()
		{
			var carrierBooking = Factory.NewWithValidTestData<DtbCarrierBookingConsignment>();
			var package1 = carrierBooking.PackageJob.Packages.AddNew();
			package1.KP_Weight = 150m;
			package1.KP_WeightUQ = "KG";
			package1.KP_GoodsDescription = "Package";
			package1.KP_PackageQty = 1;
			Factory.SaveForTesting();
			var shipmentDataObject = new DtbCarrierBookingConsignmentDataObjectWriter(new DataWritingManager(new DummyActionInfo())).GetDataObject(carrierBooking);

			AssertEquals("shipmentDataObject.PackingLineCollection.Count", 1, shipmentDataObject.PackingLineCollection.Count);
			AssertEquals("shipmentDataObject.PackType", package1.KP_GoodsDescription, shipmentDataObject.PackingLineCollection[0].GoodsDescription);
			AssertEquals("shipmentDataObject.PackQty", package1.KP_PackageQty.ToString(), shipmentDataObject.PackingLineCollection[0].PackQty.ToString());
			AssertEquals("shipmentDataObject.Weight", package1.KP_Weight.ToString(), shipmentDataObject.PackingLineCollection[0].Weight.ToString());
			AssertEquals("shipmentDataObject.WeightUnit", package1.KP_WeightUQ, shipmentDataObject.PackingLineCollection[0].WeightUnit.Code);
		}
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
