using System.Linq;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using WTG.RTUS.Interface;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class RTUSConsignmentDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestDataContextContainsEnterpriseServerAndCompanyIDs()
		{
			var consignment = PopulateAndGetConsignment();

			var currentItem = consignment.Items.Cast<HVLVItem>()
				.First(item => item.HVI_ShipperReference == "SHIPPERREF2");

			var writer = new RTUSConsignmentDataObjectWriter(
					new DataWritingManager(new ActionInfo(null, consignment)),
					currentItem.PK,
					RequestType.Booking);

			var dataObject = writer.GetDataObject(consignment);
			var ids = dataObject.DataContext.GetEnterpriseServerAndCompanyIDs();
			AssertNotNullOrEmpty("Company code should not be null or empty.", ids.CompanyCode);
			AssertNotNullOrEmpty("Enterprise ID should not be null or empty.", ids.EnterpriseID);
			AssertNotNullOrEmpty("Server ID should not be null or empty.", ids.ServerID);
		}

		public void TestDataSourceCollectionContainsShipmentNumber()
		{
			var consignment = PopulateAndGetConsignment();

			var currentItem = consignment.Items.Cast<HVLVItem>()
				.First(item => item.HVI_ShipperReference == "SHIPPERREF2");

			var writer = new RTUSConsignmentDataObjectWriter(
					new DataWritingManager(new ActionInfo(null, consignment)),
					currentItem.PK,
					RequestType.Booking);

			var dataObject = writer.GetDataObject(consignment);
			AssertEquals("S0008888", dataObject.DataContext.DataSourceCollection.Single(d => d.Type.Equals(nameof(DataContextType.ForwardingShipment))).Key);
		}

		public void TestDataSourceCollectionCOntainsConsignmentNumber()
		{
			var consignment = PopulateAndGetConsignment();

			var currentItem = consignment.Items.Cast<HVLVItem>().First(item => item.HVI_ShipperReference == "SHIPPERREF2");

			var writer = new RTUSConsignmentDataObjectWriter(
				new DataWritingManager(new ActionInfo(null, consignment)),
				currentItem.PK,
				RequestType.Booking);

			var dataObject = writer.GetDataObject(consignment);
			AssertEquals("CONSIGN1", dataObject.DataContext.DataSourceCollection.Single(d => d.Type.Equals(nameof(DataContextType.HVLVConsignment))).Key);
		}

		public void TestGetDataObject_ConsignmentData()
		{
			var consignment = PopulateAndGetConsignment();

			var currentItem = consignment.Items.Cast<HVLVItem>()
				.First(item => item.HVI_ShipperReference == "SHIPPERREF2");

			var writer = new RTUSConsignmentDataObjectWriter(
					new DataWritingManager(new ActionInfo(null, consignment)),
					currentItem.PK,
					RequestType.Booking);

			var dataObject = writer.GetDataObject(consignment);

			CombineAssertions(() =>
			{
				AssertEquals("XYZ456", dataObject.OwnerRef);
				AssertEquals(3, dataObject.TotalNoOfPieces);
				AssertEquals(50m, dataObject.GoodsValue);
				AssertEquals("USD", dataObject.GoodsValueCurrency.Code);
				AssertEquals(25m, dataObject.ManifestedWeight);
				AssertEquals(40m, dataObject.TotalWeight);
				AssertEquals(Weight.Kilograms, dataObject.TotalWeightUnit.Code);
				AssertEquals(2.5m, dataObject.ManifestedVolume);
				AssertEquals(3.5m, dataObject.TotalVolume);
				AssertEquals(Volume.CubicMetres, dataObject.TotalVolumeUnit.Code);
				AssertEquals("Biscuits", dataObject.GoodsDescription);
				AssertEquals(false, dataObject.IsHazardous);
				AssertEquals(false, dataObject.IsSignatureRequired);
				AssertEquals(false, dataObject.IsAuthorizedToLeave);
				AssertEquals("VID0001", dataObject.VendorIdentifier);

				AssertEquals(1, dataObject.InstructionCollection.Count);
				AssertEquals("Leave at back", dataObject.InstructionCollection.Single().ServiceInstruction);

				var arrivalCFSAddress1 = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
				AssertEquals("13 Destination St", arrivalCFSAddress1.Address1);

				var consigneeAddress1 = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress));
				AssertEquals("Bobs Company", consigneeAddress1.CompanyName);
				AssertEquals("12 Something St", consigneeAddress1.Address1);
				AssertEquals("Sydney", consigneeAddress1.City);
				AssertEquals("NSW", consigneeAddress1.State);
				AssertEquals("2000", consigneeAddress1.Postcode);
				AssertEquals("AU", consigneeAddress1.Country.Code);
				AssertEquals("Bob", consigneeAddress1.Contact);
				AssertEquals("bob@bob.com", consigneeAddress1.Email);

				var consignorAddress1 = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress));
				AssertEquals("Another Company", consignorAddress1.CompanyName);
				AssertEquals("23 Another St", consignorAddress1.Address1);
				AssertEquals("Melbourne", consignorAddress1.City);
				AssertEquals("VIC", consignorAddress1.State);
				AssertEquals("3000", consignorAddress1.Postcode);
				AssertEquals("AU", consignorAddress1.Country.Code);
				AssertEquals("Vic", consignorAddress1.Contact);
				AssertEquals("victor@fakedomain.com", consignorAddress1.Email);
			});
		}

		public void TestGetDataObject_ConsignmentLastMileCarrierDetails()
		{
			var carrier = Factory.New<OrgHeader>();
			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			carrier.MiscServ = miscServ;

			var servicelevel = miscServ.CarrierServiceLevels.AddNew();
			servicelevel.PL_Code = "ABC";
			servicelevel.PL_CarrierServiceCode = "ABC1";
			servicelevel.PL_CarrierServiceLevelDescription = "ABC Description";

			var carrierAccount = carrier.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "123456";
			carrierAccount.OAN_DepotID = "Test Depot ID";

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_OH_LastMileCarrier = carrier.PK;
			consignment.HVC_CarrierAccountNumber = "123456";
			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "ABC";

			var currentItem = consignment.Items.AddNew();
			currentItem.HVI_ItemId = "D9901239123";

			var writer = new RTUSConsignmentDataObjectWriter(
					new DataWritingManager(new ActionInfo(null, consignment)),
					currentItem.PK,
					RequestType.Booking);

			var dataObject = writer.GetDataObject(consignment);

			CombineAssertions(() =>
			{
				AssertEquals("Test Depot ID", dataObject.CarrierAccount.DepotID);
				AssertEquals("ABC1", dataObject.CarrierServiceLevel.CarrierServiceCode);
				AssertEquals("ABC Description", dataObject.CarrierServiceLevel.Description);
			});
		}

		public void TestGetDataObject_Booking()
		{
			var consignment = PopulateAndGetConsignment();

			var currentItem = consignment.Items.Cast<HVLVItem>()
				.First(item => item.HVI_ShipperReference == "SHIPPERREF2");

			var dataWritingManager = new DataWritingManager(new ActionInfo(null, consignment));

			var writer = new RTUSConsignmentDataObjectWriter(
				dataWritingManager,
				currentItem.PK,
				RequestType.Booking);

			var dataObject = writer.GetDataObject(consignment);

			var dataSource = dataObject.DataContext.DataSourceCollection.Single(d => d.Type.Equals(nameof(DataContextType.HVLVItem)));
			CombineAssertions("DataSource contains item's details", () =>
			{
				AssertEquals("Type", nameof(DataContextType.HVLVItem), dataSource.Type);
				AssertEquals("Key", "D5465421481", dataSource.Key);
			});

			var packingLines = dataObject.PackingLineCollection;
			AssertEquals("CollectionContent.Partial", CollectionContent.Partial, packingLines.Content);
			AssertEquals("Only current item is exported", 1, packingLines.Count);

			CombineAssertions("PackingLine contents", () =>
			{
				var packingLine = packingLines.First();
				AssertEquals("ReferenceNumber contains BarCode", "1024204840968192", packingLine.ReferenceNumber);

				AssertEquals("SHIPPERREF2", packingLine.OrderReference);
				AssertEquals("1024204840968192", packingLine.Barcode);
				AssertEquals("BOX", packingLine.PackType.Code);

				AssertEquals(3m, packingLine.Height);
				AssertEquals(4m, packingLine.Length);
				AssertEquals(5m, packingLine.Width);
				AssertEquals(Length.Metres, packingLine.LengthUnit.Code);

				AssertEquals(10m, packingLine.ManifestedWeight);
				AssertEquals(20m, packingLine.Weight);
				AssertEquals(1m, packingLine.ManifestedVolume);
				AssertEquals(2m, packingLine.Volume);

				AssertEquals(1, packingLine.OutturnDamagedQty);
				AssertEquals(0, packingLine.OutturnPillagedQty);
			});
		}

		public void TestGetDataObject_Cancellation()
		{
			var consignment = PopulateAndGetConsignment();

			var currentItem = consignment.Items.Cast<HVLVItem>()
				.First(item => item.HVI_ShipperReference == "SHIPPERREF2");

			var dataWritingManager = new DataWritingManager(new ActionInfo(null, consignment));

			var writer = new RTUSConsignmentDataObjectWriter(
				dataWritingManager,
				currentItem.PK,
				RequestType.Cancellation);

			var dataObject = writer.GetDataObject(consignment);

			var dataSource = dataObject.DataContext.DataSourceCollection.Single(d => d.Type.Equals(nameof(DataContextType.HVLVItem)));
			CombineAssertions("DataSource contains item's details", () =>
			{
				AssertEquals("Type", nameof(DataContextType.HVLVItem), dataSource.Type);
				AssertEquals("Key", "D5465421481", dataSource.Key);
			});

			var packingLines = dataObject.PackingLineCollection;
			AssertEquals("CollectionContent.Partial", CollectionContent.Partial, packingLines.Content);
			AssertEquals("Empty PackingLineCollection", 0, packingLines.Count);
		}

		#region Implementation

		HVLVConsignment PopulateAndGetConsignment()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "S0008888";
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();

			var consignment = bookingHeader.Consignments.AddNew();
			consignment.HVC_ConsignmentId = "CONSIGN1";
			consignment.HVC_ShipperReference = "XYZ456";
			consignment.HVC_ItemCount = 1;
			consignment.HVC_GoodsValue = 50;
			consignment.HVC_RX_NKGoodsValueCurrency = "USD";
			consignment.HVC_WeightUQ = Weight.Kilograms;
			consignment.HVC_VolumeUQ = Volume.CubicMetres;
			consignment.HVC_GoodsDescription = "Biscuits";
			consignment.HVC_ConsigneeInstructions = "Leave at back";
			consignment.HVC_IsHazardous = false;
			consignment.HVC_IsSignatureRequired = false;
			consignment.HVC_AuthorityToLeave = false;
			consignment.HVC_RequiresFumigation = true;
			consignment.HVC_IsPersonalEffects = true;
			consignment.HVC_IsTimber = true;
			consignment.HVC_IsPerishable = true;

			var destinationDepotOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg2.MainAddress.Address1 = "13 Destination St";
			consignment.HVC_OA_DestinationDepot = destinationDepotOrg2.MainAddress.PK;

			consignment.HVC_ConsigneeName = "Bobs Company";
			consignment.HVC_ConsigneeAddress1 = "12 Something St";
			consignment.HVC_ConsigneeCity = "Sydney";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_ConsigneePostcode = "2000";
			consignment.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment.HVC_ConsigneeContact = "Bob";
			consignment.HVC_ConsigneeEmail = "bob@bob.com";

			consignment.HVC_ShipperName = "Another Company";
			consignment.HVC_ShipperAddress1 = "23 Another St";
			consignment.HVC_ShipperCity = "Melbourne";
			consignment.HVC_ShipperState = "VIC";
			consignment.HVC_ShipperPostcode = "3000";
			consignment.HVC_RN_NKShipperCountryCode = "AU";
			consignment.HVC_ShipperContact = "Vic";
			consignment.HVC_ShipperEmail = "victor@fakedomain.com";

			consignment.HVC_VendorIdentifier = "VID0001";

			var item1 = consignment.Items.AddNew();
			item1.HVI_ItemId = "D9901239028";
			item1.HVI_ShipperReference = "S999887";
			item1.HVI_CurrentBarcode = "1234567890";
			item1.HVI_F3_NKPackType = "BOX";
			item1.HVI_ManifestedWeight = 5;
			item1.HVI_ActualWeight = 10;
			item1.HVI_ManifestedVolume = 0.5;
			item1.HVI_ActualVolume = 0.5;
			item1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;

			var item2 = consignment.Items.AddNew();
			item2.HVI_ItemId = "D5465421481";
			item2.HVI_ShipperReference = "SHIPPERREF2";
			item2.HVI_CurrentBarcode = "1024204840968192";
			item2.HVI_F3_NKPackType = "BOX";
			item2.HVI_Height = 3m;
			item2.HVI_Length = 4m;
			item2.HVI_Width = 5m;
			item2.HVI_UnitOfDimension = Length.Metres;
			item2.HVI_ManifestedWeight = 10;
			item2.HVI_ActualWeight = 20;
			item2.HVI_ManifestedVolume = 1;
			item2.HVI_ActualVolume = 2;
			item2.HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;
			item2.HVI_IsDamaged = true;
			item2.HVI_IsPillaged = false;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var item3 = consignment.Items.AddNew();
			item3.HVI_ItemId = "X0012931292";
			item3.HVI_ShipperReference = "SHIPPERREF3";
			item3.HVI_F3_NKPackType = "PKG";
			item3.HVI_ManifestedWeight = 10;
			item3.HVI_ActualWeight = 10;
			item3.HVI_ManifestedVolume = 1;
			item3.HVI_ActualVolume = 1;
			item3.HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;

			return consignment;
		}

		#endregion
	}
}
