using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.Documents.Business.US;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.US;
using Enterprise.MasterFiles.Business;

namespace Enterprise.eTail.Documents.Business.Testing.US
{
	public class HVLVAirCargoAdvanceScreeningBuilderTest : TestCaseWithFactory
	{
		public void TestSourceDetails()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ConsignmentId = "123456";

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			AssertEquals("SourceType should be HVLVConsignment", "HVLVConsignment", acas.SourceType);
			AssertEquals("SourceId should be HVC_ConsignmentId", "123456", acas.SourceID);
		}

		public void TestHAWBNumber()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WaybillNumber = "123456";

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			AssertEquals("HAWB number should be mapped from consignment waybill number", "123456", acas.HAWB);
		}

		public void TestHarmonizedCodes()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var line1 = item.Lines.AddNew();
			line1.HVS_Quantity = 1;
			line1.HVS_DestinationTariff = "12345678";

			var line2 = item.Lines.AddNew();
			line2.HVS_Quantity = 1;
			line2.HVS_DestinationTariff = "12345679";

			Factory.Save();

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			AssertContainsExactElementsInAnyOrder(new ZString[] { "1234.56.78", "1234.56.79" }, acas.HarmonizedCodes);
		}

		public void TestGoodsDescription()
		{
			var shipment = Factory.NewWithValidTestData<HVLVForwardingShipment>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.ManagingShipment = shipment;
			consignment.HVC_GoodsDescription = "Test Goods";

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			AssertEquals("Goods description should be mapped from consignment goods description if no item line exists", "Test Goods", acas.GoodsDescription);

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			var line1 = item.Lines.AddNew();
			line1.HVS_Quantity = 1;
			line1.HVS_GoodsDescription = "Test Item Line";
			Factory.Save();

			builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			acas = builder.Build();

			AssertEquals("Consignment goods description should be dropped when item line exists", "Test Item Line", acas.GoodsDescription);

			var line2 = item.Lines.AddNew();
			line2.HVS_Quantity = 1;
			line2.HVS_GoodsDescription = "Test Item Line 2";
			Factory.Save();

			builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			acas = builder.Build();
			AssertEquals("Should combine multiple item lines", "Test Item Line, Test Item Line 2", acas.GoodsDescription);
		}

		public void TestPacks()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ItemCount = 123;

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			AssertEquals("Number of packs should be mapped from item count", 123, acas.NumberOfPacks);
		}

		public void TestWeight()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ActualWeight = 123m;
			consignment.HVC_WeightUQ = "KG";

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			AssertEquals("Weight value should be mapped from actual weight", 123m, acas.Weight.Value);
			AssertEquals("Weight unit should be mapped from weight UQ", "KG", acas.Weight.Unit.Code);

			consignment.HVC_WeightUQ = "T";
			acas = builder.Build();

			AssertEquals("Weight value should be scaled to fit KG", 123000m, acas.Weight.Value);
			AssertEquals("Weight unit should be converted from T to KG", "KG", acas.Weight.Unit.Code);

			consignment.HVC_WeightUQ = "OZ";
			acas = builder.Build();

			AssertEquals("Weight value should be scaled to fit LB", 7.6875m, acas.Weight.Value);
			AssertEquals("Weight unit should be converted from OZ to LB", "LB", acas.Weight.Unit.Code);

			consignment.HVC_ActualWeight = 0m;
			consignment.HVC_ManifestedWeight = 123m;
			consignment.HVC_WeightUQ = "KG";
			acas = builder.Build();

			AssertEquals("Weight value should be mapped from manifested weight when actual weight is not available", 123m, acas.Weight.Value);
			AssertEquals("Weight unit should be mapped from weight UQ", "KG", acas.Weight.Unit.Code);
		}

		public void TestShipper_WhenConsignmentShipperIsOrganisation()
		{
			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "TSTSH";
			shipper.OH_FullName = "Test Shipper";

			var address = shipper.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";
			address.OA_Address2 = "Room 456";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "2000";
			address.OA_RN_NKCountryCode = "AU";

			var contact = shipper.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Phone = "12345678";
			contact.OC_Email = "abc@123.com";
			contact.OC_Fax = "87654321";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = address.PK;

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			CombineAssertions("Shipper mappings", () =>
			{
				AssertEquals("Company Name", "Test Shipper", acas.Shipper.CompanyName);
				AssertEquals("AddressLine1", "No.123 abc street", acas.Shipper.AddressLine1);
				AssertEquals("AddressLine2", "Room 456", acas.Shipper.AddressLine2);
				AssertEquals("City", "Sydney", acas.Shipper.City);
				AssertEquals("State", "NSW", acas.Shipper.State);
				AssertEquals("Postcode", "2000", acas.Shipper.Postcode);
				AssertEquals("Country", "AU", acas.Shipper.Country.Code);

				AssertEquals("Contact", "Test Contact", acas.Shipper.Contact);
				AssertEquals("Phone", "12345678", acas.Shipper.Phone);
				AssertEquals("Email", "abc@123.com", acas.Shipper.Email);
				AssertEquals("Fax", "87654321", acas.Shipper.Fax);
			});
		}

		public void TestShipper_WhenConsignmentShipperIsNotOrganisation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ShipperName = "Test Shipper";
			consignment.HVC_ShipperAddress1 = "No.123 abc street";
			consignment.HVC_ShipperAddress2 = "Room 456";
			consignment.HVC_ShipperCity = "Sydney";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_ShipperPostcode = "2000";
			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_ShipperContact = "Test Contact";
			consignment.HVC_ShipperPhone = "12345678";
			consignment.HVC_ShipperEmail = "abc@123.com";
			consignment.HVC_ShipperFax = "87654321";

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			CombineAssertions("Shipper mappings", () =>
			{
				AssertEquals("Company Name", "Test Shipper", acas.Shipper.CompanyName);
				AssertEquals("AddressLine1", "No.123 abc street", acas.Shipper.AddressLine1);
				AssertEquals("AddressLine2", "Room 456", acas.Shipper.AddressLine2);
				AssertEquals("City", "Sydney", acas.Shipper.City);
				AssertEquals("State", "NSW", acas.Shipper.State);
				AssertEquals("Postcode", "2000", acas.Shipper.Postcode);
				AssertEquals("Country", "AU", acas.Shipper.Country.Code);

				AssertEquals("Contact", "Test Contact", acas.Shipper.Contact);
				AssertEquals("Phone", "12345678", acas.Shipper.Phone);
				AssertEquals("Email", "abc@123.com", acas.Shipper.Email);
				AssertEquals("Fax", "87654321", acas.Shipper.Fax);
			});
		}

		public void TestConsignee_WhenConsignmentConsigneeIsOrganisation()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TSTCN";
			consignee.OH_FullName = "Test Consignee";

			var address = consignee.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";
			address.OA_Address2 = "Room 456";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "2000";
			address.OA_RN_NKCountryCode = "AU";

			var contact = consignee.Contacts.AddNew();
			contact.OC_ContactName = "Test Contact";
			contact.OC_Phone = "12345678";
			contact.OC_Email = "abc@123.com";
			contact.OC_Fax = "87654321";

			Factory.Save();

			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ConsigneeAddress = address.PK;

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			CombineAssertions("Consignee mappings", () =>
			{
				AssertEquals("Company Name", "Test Consignee", acas.Consignee.CompanyName);
				AssertEquals("AddressLine1", "No.123 abc street", acas.Consignee.AddressLine1);
				AssertEquals("AddressLine2", "Room 456", acas.Consignee.AddressLine2);
				AssertEquals("City", "Sydney", acas.Consignee.City);
				AssertEquals("State", "NSW", acas.Consignee.State);
				AssertEquals("Postcode", "2000", acas.Consignee.Postcode);
				AssertEquals("Country", "AU", acas.Consignee.Country.Code);

				AssertEquals("Contact", "Test Contact", acas.Consignee.Contact);
				AssertEquals("Phone", "12345678", acas.Consignee.Phone);
				AssertEquals("Email", "abc@123.com", acas.Consignee.Email);
				AssertEquals("Fax", "87654321", acas.Consignee.Fax);
			});
		}

		public void TestConsignee_WhenConsignmentConsigneeIsNotOrganisation()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ConsigneeName = "Test Consignee";
			consignment.HVC_ConsigneeAddress1 = "No.123 abc street";
			consignment.HVC_ConsigneeAddress2 = "Room 456";
			consignment.HVC_ConsigneeCity = "Sydney";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_ConsigneePostcode = "2000";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_ConsigneeContact = "Test Contact";
			consignment.HVC_ConsigneePhone = "12345678";
			consignment.HVC_ConsigneeEmail = "abc@123.com";
			consignment.HVC_ConsigneeFax = "87654321";

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();

			CombineAssertions("Consignee mappings", () =>
			{
				AssertEquals("Company Name", "Test Consignee", acas.Consignee.CompanyName);
				AssertEquals("AddressLine1", "No.123 abc street", acas.Consignee.AddressLine1);
				AssertEquals("AddressLine2", "Room 456", acas.Consignee.AddressLine2);
				AssertEquals("City", "Sydney", acas.Consignee.City);
				AssertEquals("State", "NSW", acas.Consignee.State);
				AssertEquals("Postcode", "2000", acas.Consignee.Postcode);
				AssertEquals("Country", "AU", acas.Consignee.Country.Code);

				AssertEquals("Contact", "Test Contact", acas.Consignee.Contact);
				AssertEquals("Phone", "12345678", acas.Consignee.Phone);
				AssertEquals("Email", "abc@123.com", acas.Consignee.Email);
				AssertEquals("Fax", "87654321", acas.Consignee.Fax);
			});
		}

		public void TestStatus()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();
			AssertEquals(AcasState.None, acas.State);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			acas = builder.Build();
			AssertEquals(AcasState.OriginalSent, acas.State);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			acas = builder.Build();
			AssertEquals(AcasState.AcknowledgementRequired, acas.State);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementSent;
			acas = builder.Build();
			AssertEquals(AcasState.AcknowledgementSent, acas.State);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			acas = builder.Build();
			AssertEquals(AcasState.AmendmentRequired, acas.State);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentSent;
			acas = builder.Build();
			AssertEquals(AcasState.AmendmentSent, acas.State);
		}

		public void TestDisplayInformation()
		{
			var shipment = Factory.New<ForwardingShipment>();

			var consignment = Factory.New<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var parameters = new DummyDocDataObjectParameters();
			var builder = new HVLVAirCargoAdvanceScreeningBuilder(shipment, consignment, parameters);
			var acas = builder.Build();
			AssertEquals(ZString.Empty, acas.DisplayInformation);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.OriginalSent;
			acas = builder.Build();
			AssertEquals(@"The message previously sent has not yet received a response.
Please wait for a response before resending.", acas.DisplayInformation);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementRequired;
			acas = builder.Build();
			AssertEquals(@"The latest response from CBP is ""On Hold"" (6H, 7H or 8H).
Use the 'Send Message' option to send an Acknowledgement message.", acas.DisplayInformation);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AcknowledgementSent;
			acas = builder.Build();
			AssertEquals(@"An Acknowledgement message has been sent and has not received a response.
Please wait for a response before further action.", acas.DisplayInformation);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentRequired;
			acas = builder.Build();
			AssertEquals(@"The latest status received from US Customs is Selectee Data Issue Hold.
Amend the data and resend the message to resolve the hold.", acas.DisplayInformation);

			consignment.HVC_ACASMessageStatus = HVLVACASMessageStatusList.Codes.AmendmentSent;
			acas = builder.Build();
			AssertEquals(@"An amendment message in response to Selectee Data Issues has been sent.
Please wait for a response before further action.", acas.DisplayInformation);
		}
	}
}
