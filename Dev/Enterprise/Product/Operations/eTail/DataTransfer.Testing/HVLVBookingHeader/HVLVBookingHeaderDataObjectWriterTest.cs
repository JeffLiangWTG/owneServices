using System.Linq;
using CargoWise.Common;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.Integration;
using static Enterprise.Core.Constants;
using OrgSupplierPart = Enterprise.MasterFiles.Business.OrgSupplierPart;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVBookingHeaderDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestWriteToDataObject()
		{
			var bookingHeader = PopulateAndGetBookingHeader();

			var dataObject = new HVLVBookingHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingHeader))).GetDataObject(bookingHeader);

			#region Assertions

			CombineAssertions(() =>
			{
				AssertEquals(true, dataObject.IsLastMileDeliverySelfBooked);
				AssertEquals("STD", dataObject.ServiceLevel.Code);
				AssertEquals(3, dataObject.TotalNoOfPieces);
				AssertEquals((ZDecimal)30, dataObject.TotalWeight);
				AssertEquals(Weight.Kilograms, dataObject.TotalWeightUnit.Code);
				AssertEquals((ZDecimal)2, dataObject.TotalVolume);
				AssertEquals(Volume.CubicMetres, dataObject.TotalVolumeUnit.Code);
				AssertEquals(Freight.Integration.ShipmentStatusList.Codes.Confirmed, dataObject.BookingConfirmationReference.Value);

				var localClientAddress = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == AddressTypes.SendersLocalClient);
				AssertEquals("45 Bill To Street", localClientAddress.Address1);
				AssertEquals("Bill", localClientAddress.Contact);

				var consignorPickupDeliveryAddress = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsignorPickupDeliveryAddress));
				AssertEquals("56 Dispatch Avenue", consignorPickupDeliveryAddress.Address1);

				var exportBrokerAddress = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ExportBroker));
				AssertEquals("67 Agent Lane", exportBrokerAddress.Address1);

				var bookingPartyAddress = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.BookingPartyDocumentaryAddress));
				AssertEquals("78 Booking Street", bookingPartyAddress.Address1);
				AssertEquals("Brooke", bookingPartyAddress.Contact);

				var departureCFSAddress = dataObject.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.DepartureCFSAddress));
				AssertEquals("89 Depot Road", departureCFSAddress.Address1);

				AssertEquals(2, dataObject.SubShipmentCollection.Count);

				var subShipment1 = dataObject.SubShipmentCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Single().Key.Equals("CONSIGN2"));

				var dataSource1 = subShipment1.DataContext.DataSourceCollection.Single();
				AssertEquals(nameof(DataContextType.HVLVConsignment), dataSource1.Type);
				AssertEquals("CONSIGN2", dataSource1.Key);

				AssertEquals("XYZ456", subShipment1.OwnerRef);
				AssertEquals(1, subShipment1.TotalNoOfPieces);
				AssertEquals((ZDecimal)50, subShipment1.GoodsValue);
				AssertEquals("USD", subShipment1.GoodsValueCurrency.Code);
				AssertEquals((ZDecimal)10, subShipment1.ManifestedWeight);
				AssertEquals((ZDecimal)10, subShipment1.TotalWeight);
				AssertEquals(Weight.Kilograms, subShipment1.TotalWeightUnit.Code);
				AssertEquals((ZDecimal)0.5, subShipment1.ManifestedVolume);
				AssertEquals((ZDecimal)1, subShipment1.TotalVolume);
				AssertEquals(Volume.CubicMetres, subShipment1.TotalVolumeUnit.Code);
				AssertEquals("Biscuits", subShipment1.GoodsDescription);
				AssertEquals(false, subShipment1.IsHazardous);
				AssertEquals(false, subShipment1.IsSignatureRequired);
				AssertEquals(false, subShipment1.IsAuthorizedToLeave);
				AssertEquals(false, subShipment1.IsTracked);
				AssertEquals("VID0001", subShipment1.VendorIdentifier);
				AssertEquals("PKG", subShipment1.TotalNoOfPacksPackageType.Code);
				AssertEquals(IncoTerms.DeliveredAtPlace, subShipment1.ShipmentIncoTerm.Code);
				AssertEquals("D2D", subShipment1.ServiceLevel.Code);

				AssertEquals(1, subShipment1.InstructionCollection.Count);
				AssertEquals("Leave at back", subShipment1.InstructionCollection.Single().ServiceInstruction);

				var declarationReference1 = subShipment1.CustomsReferenceCollection.Single(r => (string)r.SubType.Code == FreightShipmentDirection.Code.Export);
				AssertEquals("HaltInTheNameOfTheLaw", declarationReference1.Reference);

				AssertNull("No additional references", subShipment1.AdditionalReferenceCollection);

				var arrivalCFSAddress1 = subShipment1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
				AssertEquals("13 Destination St", arrivalCFSAddress1.Address1);

				var consigneeAddress1 = subShipment1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress));
				AssertEquals("Bobs Company", consigneeAddress1.CompanyName);
				AssertEquals("12 Something St", consigneeAddress1.Address1);
				AssertEquals("Sydney", consigneeAddress1.City);
				AssertEquals("NSW", consigneeAddress1.State);
				AssertEquals("2000", consigneeAddress1.Postcode);
				AssertEquals("AU", consigneeAddress1.Country.Code);
				AssertEquals("Bob", consigneeAddress1.Contact);
				AssertEquals("bob@bob.com", consigneeAddress1.Email);
				Assert(consigneeAddress1.AddressOverride.Value);

				var consignorAddress1 = subShipment1.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress));
				AssertEquals("Another Company", consignorAddress1.CompanyName);
				AssertEquals("23 Another St", consignorAddress1.Address1);
				AssertEquals("Melbourne", consignorAddress1.City);
				AssertEquals("VIC", consignorAddress1.State);
				AssertEquals("3000", consignorAddress1.Postcode);
				AssertEquals("AU", consignorAddress1.Country.Code);
				AssertEquals("Vic", consignorAddress1.Contact);
				AssertEquals("victor@fakedomain.com", consignorAddress1.Email);
				Assert(consignorAddress1.AddressOverride.Value);
				AssertEquals(1, subShipment1.PackingLineCollection.Count);
				AssertEquals("HVL", subShipment1.ShipmentType.Code.Value);
				AssertEquals("N", subShipment1.AddInfoCollection.Single(a => a.Key.Value == "IsGSTPrePaid").Value.Value);

				var subShipment2 = dataObject.SubShipmentCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Single().Key.Equals("CONSIGN1"));

				var dataSource2 = subShipment2.DataContext.DataSourceCollection.Single();
				AssertEquals(nameof(DataContextType.HVLVConsignment), dataSource2.Type);
				AssertEquals("CONSIGN1", dataSource2.Key);
				AssertEquals("2345DEF", subShipment2.WayBillNumber);
				AssertEquals("HWB", subShipment2.WayBillType.Code);
				AssertEquals("House Waybill", subShipment2.WayBillType.Description);
				AssertEquals("ABC123", subShipment2.OwnerRef);
				AssertEquals(2, subShipment2.TotalNoOfPieces);
				AssertEquals((ZDecimal)200, subShipment2.GoodsValue);
				AssertEquals("AUD", subShipment2.GoodsValueCurrency.Code);
				AssertEquals((ZDecimal)11.23, subShipment2.ManifestedWeight);
				AssertEquals((ZDecimal)20, subShipment2.TotalWeight);
				AssertEquals(Weight.Kilograms, subShipment2.TotalWeightUnit.Code);
				AssertEquals((ZDecimal)1, subShipment2.ManifestedVolume);
				AssertEquals((ZDecimal)1, subShipment2.TotalVolume);
				AssertEquals(Volume.CubicMetres, subShipment1.TotalVolumeUnit.Code);
				AssertEquals("Explosives", subShipment2.GoodsDescription);
				AssertEquals(true, subShipment2.IsHazardous);
				AssertEquals(true, subShipment2.IsSignatureRequired);
				AssertEquals(true, subShipment2.IsAuthorizedToLeave);
				AssertEquals(true, subShipment2.IsTracked);
				AssertEquals("EXP", subShipment2.CarrierServiceLevel.Code);
				AssertEquals("BOX", subShipment2.TotalNoOfPacksPackageType.Code);
				AssertEquals(IncoTerms.FreeOnBoard, subShipment2.ShipmentIncoTerm.Code);
				AssertEquals("STD", subShipment2.ServiceLevel.Code);

				AssertEquals(1, subShipment2.InstructionCollection.Count);
				AssertEquals("HVL", subShipment2.ShipmentType.Code.Value);
				AssertEquals("Y", subShipment2.AddInfoCollection.Single(a => a.Key.Value == "IsGSTPrePaid").Value.Value);
				AssertEquals("Leave at front", subShipment2.InstructionCollection.Single().ServiceInstruction);

				var declarationReference2 = subShipment2.CustomsReferenceCollection.Single(r => (string)r.SubType.Code == FreightShipmentDirection.Code.Import);
				AssertEquals("InnocuousWhitePowder", declarationReference2.Reference);

				var arrivalCFSAddress2 = subShipment2.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ArrivalCFSAddress));
				AssertEquals("12 Destination St", arrivalCFSAddress2.Address1);

				var lastMileCarrier2 = subShipment2.OrganizationAddressCollection.Single(x => x.AddressType.Value == AddressTypes.DeliveryLocalCartage);
				AssertEquals("99 Somewhere St", lastMileCarrier2.Address1);

				var lastMileCarrierAgent2 = subShipment2.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.CarrierBookingAgent));
				AssertEquals("38 LastMileCarrierAgent St", lastMileCarrierAgent2.Address1);

				var consigneeAddress2 = subShipment2.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress));
				AssertEquals("Murray", consigneeAddress2.CompanyName);
				AssertEquals("99 Consignee Road", consigneeAddress2.Address1);
				AssertEquals("Downtown", consigneeAddress2.Address2);
				AssertEquals("New York", consigneeAddress2.City);
				AssertEquals("NY", consigneeAddress2.State);
				AssertEquals("12345", consigneeAddress2.Postcode);
				AssertEquals("US", consigneeAddress2.Country.Code);
				AssertEquals("Moo ray", consigneeAddress2.Contact);
				AssertEquals("murray.hewitt@usconsulate.gov.nz", consigneeAddress2.Email);
				AssertEquals("7", consigneeAddress2.Phone);
				AssertEquals("+1234567890", consigneeAddress2.Mobile);
				AssertEquals("+0987654321", consigneeAddress2.Fax);
				Assert(consigneeAddress2.AddressOverride.Value);
				var consignorAddress2 = subShipment2.OrganizationAddressCollection.Single(x => x.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress));
				AssertEquals("Some Company", consignorAddress2.CompanyName);
				AssertEquals("22 Shipper Street", consignorAddress2.Address1);
				AssertEquals("Wellington", consignorAddress2.City);
				AssertEquals("WLG", consignorAddress2.State);
				AssertEquals("54321", consignorAddress2.Postcode);
				AssertEquals("NZ", consignorAddress2.Country.Code);
				AssertEquals("Randy", consignorAddress2.Contact);
				AssertEquals("randy@randysdomain.com", consignorAddress2.Email);
				AssertEquals("01189998819991197253", consignorAddress2.Phone);
				AssertEquals("+555 5555", consignorAddress2.Mobile);
				AssertEquals("8", consignorAddress2.Fax);

				AssertEquals(2, subShipment2.PackingLineCollection.Count);
				AssertNull("No notes", subShipment2.NoteCollection);

				var packingLine1_1 = subShipment1.PackingLineCollection.Single(x => x.OrderReference.Value == "X0012931292");
				AssertEquals("PKG", packingLine1_1.PackType.Code);
				AssertEquals((ZDecimal)10, packingLine1_1.ManifestedWeight);
				AssertEquals((ZDecimal)10, packingLine1_1.Weight);
				AssertEquals((ZDecimal)0.5, packingLine1_1.ManifestedVolume);
				AssertEquals((ZDecimal)1, packingLine1_1.Volume);
				AssertEquals(true, packingLine1_1.RequiresFumigationCertificate);
				AssertEquals(true, packingLine1_1.IsPersonalEffects);
				AssertEquals(true, packingLine1_1.IsTimber);
				AssertEquals(true, packingLine1_1.IsPerishable);
				AssertEquals(1, packingLine1_1.OutturnQty);
				AssertEquals(0, packingLine1_1.OutturnDamagedQty);
				AssertEquals(1, packingLine1_1.OutturnPillagedQty);
				AssertEquals("CTNR654321", packingLine1_1.ContainerNumber);
				AssertEquals("HVI00003", packingLine1_1.ReferenceNumber);

				var packingLine2_1 = subShipment2.PackingLineCollection.Single(x => x.OrderReference.Value == "D9901239028");
				AssertEquals("12345678901234567890123456789012345678901234567890", packingLine2_1.Barcode);
				AssertEquals("BOX", packingLine2_1.PackType.Code);
				AssertEquals((ZDecimal)1.23, packingLine2_1.ManifestedWeight);
				AssertEquals((ZDecimal)10, packingLine2_1.Weight);
				AssertEquals(false, packingLine2_1.RequiresFumigationCertificate);
				AssertEquals(false, packingLine2_1.IsPersonalEffects);
				AssertEquals(false, packingLine2_1.IsTimber);
				AssertEquals(false, packingLine2_1.IsPerishable);
				AssertEquals(0, packingLine2_1.OutturnQty);
				AssertEquals(0, packingLine2_1.OutturnDamagedQty);
				AssertEquals(0, packingLine2_1.OutturnPillagedQty);
				AssertEquals("HVI00001", packingLine2_1.ReferenceNumber);

				AssertEquals(3m, packingLine2_1.Height);
				AssertEquals(4m, packingLine2_1.Length);
				AssertEquals(5m, packingLine2_1.Width);
				AssertEquals((ZDecimal)0.5, packingLine2_1.ManifestedVolume);
				AssertEquals((ZDecimal)0.5, packingLine2_1.Volume);
				AssertEquals(Length.Metres, packingLine2_1.LengthUnit.Code);
				AssertEquals(Volume.CubicMetres, packingLine2_1.VolumeUnit.Code);

				AssertEquals(1, packingLine2_1.UNDGCollection.Count);
				AssertEquals("1.1D", packingLine2_1.UNDGCollection[0].IMOClass);

				AssertEquals(2, packingLine2_1.PackedItemCollection.Count);
				var packedItem2_1_1 = packingLine2_1.PackedItemCollection.Single(x => x.Description.Value == "Keyboard");
				AssertEquals(54.321M, packedItem2_1_1.GoodsValue);
				AssertEquals(1.23M, packedItem2_1_1.GrossWeight);
				AssertEquals("KG", packedItem2_1_1.GrossWeightUnit.Code);
				AssertEquals(3.21M, packedItem2_1_1.NetWeight);
				AssertEquals("KG", packedItem2_1_1.NetWeightUnit.Code);
				AssertEquals(2M, packedItem2_1_1.PackedQuantity);
				AssertEquals("KBD001", packedItem2_1_1.Product.Code);
				AssertEquals(12.345M, packedItem2_1_1.CIFValue);
				AssertEquals("www.google.com/keyboard", packedItem2_1_1.ItemSpecificationUrl);

				var commercialInvoiceLine2_1_1 = subShipment2.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single(x => x.Link == packedItem2_1_1.CommercialInvoiceLineLink);
				AssertEquals("654321", commercialInvoiceLine2_1_1.HarmonisedCode);
				AssertEquals(12.345M, commercialInvoiceLine2_1_1.CustomsValue);
				AssertEquals(3.21M, commercialInvoiceLine2_1_1.NetWeight);
				AssertEquals(1.23M, commercialInvoiceLine2_1_1.Weight);
				AssertEquals("KG", commercialInvoiceLine2_1_1.WeightUnit.Code);
				AssertEquals(2M, commercialInvoiceLine2_1_1.CustomsQuantity);

				var customsSupportInfo = commercialInvoiceLine2_1_1.CustomsSupportingInformationCollection.Single();
				AssertEquals("123456", customsSupportInfo.Tariff);
				AssertEquals("AU", customsSupportInfo.Country.Code);

				var packingLine2_2 = subShipment2.PackingLineCollection.Single(x => x.OrderReference.Value == "D5465421481");
				AssertEquals("BOX", packingLine2_2.PackType.Code);
				AssertEquals((ZDecimal)10, packingLine2_2.ManifestedWeight);
				AssertEquals((ZDecimal)10, packingLine2_2.Weight);
				AssertEquals((ZDecimal)0.5, packingLine2_2.ManifestedVolume);
				AssertEquals((ZDecimal)0.5, packingLine2_2.Volume);
				AssertEquals(false, packingLine2_2.RequiresFumigationCertificate);
				AssertEquals(false, packingLine2_2.IsPersonalEffects);
				AssertEquals(false, packingLine2_2.IsTimber);
				AssertEquals(false, packingLine2_2.IsPerishable);
				AssertEquals(1, packingLine2_2.OutturnQty);
				AssertEquals(1, packingLine2_2.OutturnDamagedQty);
				AssertEquals(0, packingLine2_2.OutturnPillagedQty);
				AssertEquals("HVI00002", packingLine2_2.ReferenceNumber);

				AssertEquals(2, packingLine2_2.UNDGCollection.Count);
				AssertEquals("1.1D", packingLine2_2.UNDGCollection[0].IMOClass);
				AssertEquals("1.1D", packingLine2_2.UNDGCollection[1].IMOClass);

				var additionalReference = subShipment2.AdditionalReferenceCollection.Single();
				AssertEquals("Additional Reference number correct", "ISF0000001", additionalReference.ReferenceNumber);
				AssertEquals("Additional Reference type correct", "ISF", additionalReference.Type.Code);
			});

			#endregion
		}

		public void TestWriteToDataObject_ShipmentLocalProcessing()
		{
			var bookingHeader = PopulateAndGetBookingHeader();
			bookingHeader.DocsAndCartage.JP_PickupRequiredFrom = new ZDateTime(2022, 12, 8);
			bookingHeader.DocsAndCartage.JP_PickupRequiredBy = new ZDateTime(2022, 12, 9);
			bookingHeader.DocsAndCartage.JP_PickupCartageAdvised = new ZDateTime(2022, 12, 9);
			bookingHeader.DocsAndCartage.JP_EstimatedPickup = new ZDateTime(2022, 12, 10);
			bookingHeader.DocsAndCartage.JP_PickupCartageCompleted = new ZDateTime(2022, 12, 10);

			var dataObject = new HVLVBookingHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingHeader))).GetDataObject(bookingHeader);

			AssertNotNull(dataObject.LocalProcessing);
			AssertEquals(new ZDateTime(2022, 12, 9), dataObject.LocalProcessing.PickupRequiredBy);
			AssertEquals(new ZDateTime(2022, 12, 8), dataObject.LocalProcessing.PickupRequiredFrom);
			AssertEquals(new ZDateTime(2022, 12, 9), dataObject.LocalProcessing.PickupCartageAdvised);
			AssertEquals(new ZDateTime(2022, 12, 10), dataObject.LocalProcessing.EstimatedPickup);
			AssertEquals(new ZDateTime(2022, 12, 10), dataObject.LocalProcessing.PickupCartageCompleted);
		}

		public void TestWeightFallsBackToManifested_HVLVConsignment()
		{
			var bookingHeader = PopulateAndGetBookingHeader();
			var consignment = (HVLVConsignment)bookingHeader.Consignments.First();

			AssertNotNull("Precondition: Consignment should be created", consignment);

			consignment.HVC_ActualWeight = 0;
			consignment.HVC_ManifestedWeight = 5;

			consignment.Items.Cast<HVLVItem>().ForEach(item => item.HVI_ActualWeight = 0);

			var dataObject = new HVLVBookingHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingHeader))).GetDataObject(bookingHeader);
			var subShipment = dataObject.SubShipmentCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Single().Key.Equals("CONSIGN1"));

			AssertEquals("Should have defaulted to manifested weight when actual weight is 0", consignment.HVC_ManifestedWeight, subShipment.TotalWeight);
		}

		public void TestWeightFallsBackToManifested_HVLVItem()
		{
			var bookingHeader = PopulateAndGetBookingHeader();
			var consignment = (HVLVConsignment)bookingHeader.Consignments.First();

			AssertNotNull("Precondition: Consignment should be created", consignment);

			var item = (HVLVItem)consignment.Items.First();

			AssertNotNull("Precondition: Consignment should have item", item);

			item.HVI_ActualWeight = 0;
			item.HVI_ManifestedWeight = 10;
			var itemReferenceNumber = item.HVI_ShipperReference;

			var dataObject = new HVLVBookingHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingHeader))).GetDataObject(bookingHeader);
			var subShipment = dataObject.SubShipmentCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Single().Key.Equals("CONSIGN1"));
			var packingLine = subShipment.PackingLineCollection.Single(packlingLine => packlingLine.OrderReference.Value == itemReferenceNumber);

			AssertEquals("Should have defaulted to manifested weight when actual weight is 0", item.HVI_ManifestedWeight, packingLine.Weight);
		}

		public void TestVolumeFallsBackToManifested_HVLVConsignment()
		{
			var bookingHeader = PopulateAndGetBookingHeader();
			var consignment = (HVLVConsignment)bookingHeader.Consignments.First();

			AssertNotNull("Precondition: Consignment should be created", consignment);

			consignment.HVC_ActualVolume = 0;
			consignment.HVC_ManifestedVolume = 10;

			consignment.Items.Cast<HVLVItem>().ForEach(item => item.HVI_ActualVolume = 0);

			var dataObject = new HVLVBookingHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingHeader))).GetDataObject(bookingHeader);
			var subShipment = dataObject.SubShipmentCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Single().Key.Equals("CONSIGN1"));

			AssertEquals("Should have defaulted to manifested volume when actual volume is 0", consignment.HVC_ManifestedVolume, subShipment.TotalVolume);
		}

		public void TestVolumeFallsBackToManifested_HVLVItem()
		{
			var bookingHeader = PopulateAndGetBookingHeader();
			var consignment = (HVLVConsignment)bookingHeader.Consignments.First();

			AssertNotNull("Precondition: Consignment should be created", consignment);

			var item = (HVLVItem)consignment.Items.First();

			AssertNotNull("Precondition: Consignment should have item", item);

			item.HVI_ActualVolume = 0;
			item.HVI_ManifestedVolume = 15;
			var itemReferenceNumber = item.HVI_ShipperReference;

			var dataObject = new HVLVBookingHeaderDataObjectWriter(new DataWritingManager(new ActionInfo(null, bookingHeader))).GetDataObject(bookingHeader);
			var subShipment = dataObject.SubShipmentCollection.FirstOrDefault(x => x.DataContext.DataSourceCollection.Single().Key.Equals("CONSIGN1"));
			var packingLine = subShipment.PackingLineCollection.Single(packlingLine => packlingLine.OrderReference.Value == itemReferenceNumber);

			AssertEquals("Should have defaulted to manifested volume when actual volume is 0", item.HVI_ManifestedVolume, packingLine.Volume);
		}

		#region Implementation

		public HVLVBookingHeader PopulateAndGetBookingHeader()
		{
			#region Setup

			#region Booking Header

			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.HVH_BookingReference = "M00001015";
			bookingHeader.HVH_UseShipperDeliveryAccount = true;
			bookingHeader.HVH_RS_NKBookingServiceLevel = "STD";
			bookingHeader.HVH_GrossWeightUQ = Weight.Kilograms;
			bookingHeader.HVH_GrossVolumeUQ = Volume.CubicMetres;
			bookingHeader.HVH_IsBookingConfirmed = true;

			var billToParty = Factory.NewWithValidTestData<OrgHeader>();
			billToParty.MainAddress.Address1 = "45 Bill To Street";
			billToParty.Contacts.AddNew().OC_ContactName = "Bill";
			bookingHeader.HVH_OA_BillToParty = billToParty.MainAddress.PK;
			bookingHeader.HVH_OC_BillToPartyContact = billToParty.Contacts[0].PK;

			var dispatchOrg = Factory.NewWithValidTestData<OrgHeader>();
			dispatchOrg.MainAddress.Address1 = "56 Dispatch Avenue";
			bookingHeader.HVH_OA_DispatchAddress = dispatchOrg.MainAddress.PK;

			var freightAgent = Factory.NewWithValidTestData<OrgHeader>();
			freightAgent.MainAddress.Address1 = "67 Agent Lane";
			bookingHeader.HVH_OH_FreightAgent = freightAgent.PK;

			var bookingParty = Factory.NewWithValidTestData<OrgHeader>();
			bookingParty.MainAddress.Address1 = "78 Booking Street";
			var bookingContact = bookingParty.Contacts.AddNew();
			bookingContact.OC_ContactName = "Brooke";
			bookingContact.OC_OA_OrgAddress = bookingParty.MainAddress.PK;
			bookingHeader.HVH_OC_BookedBy = bookingParty.Contacts[0].PK;

			var originDepot = Factory.NewWithValidTestData<OrgHeader>();
			originDepot.MainAddress.Address1 = "89 Depot Road";
			bookingHeader.HVH_OA_OriginDepot = originDepot.MainAddress.PK;

			#endregion

			#region Consignments

			var consignment1 = bookingHeader.Consignments.AddNew();
			consignment1.HVC_ConsignmentId = "CONSIGN1";
			consignment1.HVC_WaybillNumber = "2345DEF";
			consignment1.HVC_ShipperReference = "ABC123";
			consignment1.HVC_ItemCount = 0;
			consignment1.HVC_GoodsValue = 200;
			consignment1.HVC_RX_NKGoodsValueCurrency = "AUD";
			consignment1.HVC_WeightUQ = Weight.Kilograms;
			consignment1.HVC_VolumeUQ = Volume.CubicMetres;
			consignment1.HVC_GoodsDescription = "Explosives";
			consignment1.HVC_ConsigneeInstructions = "Leave at front";
			consignment1.HVC_INCO = IncoTerms.FreeOnBoard;
			consignment1.HVC_IsHazardous = true;
			consignment1.HVC_IsSignatureRequired = true;
			consignment1.HVC_AuthorityToLeave = true;
			consignment1.HVC_IsTracked = true;
			consignment1.HVC_RequiresFumigation = false;
			consignment1.HVC_IsPersonalEffects = false;
			consignment1.HVC_IsTimber = false;
			consignment1.HVC_IsPerishable = false;
			consignment1.HVC_UndgClass = "1.1D";
			consignment1.HVC_PL_NKLastMileCarrierServiceLevel = "EXP";
			consignment1.HVC_IsTaxPrePaid = true;
			consignment1.HVC_RS_NKServiceLevel = "STD";
			consignment1.Notes.AddNew();

			var destinationDepotOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg1.MainAddress.Address1 = "12 Destination St";
			consignment1.HVC_OA_DestinationDepot = destinationDepotOrg1.MainAddress.PK;

			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.MainAddress.Address1 = "99 Somewhere St";
			consignment1.HVC_OH_LastMileCarrier = lastMileDelivery.PK;

			var lastMileCarrierBookingAgent = Factory.NewWithValidTestData<OrgHeader>();
			lastMileCarrierBookingAgent.MainAddress.Address1 = "38 LastMileCarrierAgent St";
			consignment1.HVC_OH_LastMileCarrierBookingAgent = lastMileCarrierBookingAgent.PK;

			consignment1.HVC_ConsigneeName = "Murray";
			consignment1.HVC_ConsigneeAddress1 = "99 Consignee Road";
			consignment1.HVC_ConsigneeAddress2 = "Downtown";
			consignment1.HVC_ConsigneeCity = "New York";
			consignment1.HVC_ConsigneeState = "NY";
			consignment1.HVC_ConsigneePostcode = "12345";
			consignment1.HVC_RN_NKConsigneeCountryCode = "US";
			consignment1.HVC_ConsigneeContact = "Moo ray";
			consignment1.HVC_ConsigneeEmail = "murray.hewitt@usconsulate.gov.nz";
			consignment1.HVC_ConsigneePhone = "7";
			consignment1.HVC_ConsigneeMobile = "+1234567890";
			consignment1.HVC_ConsigneeFax = "+0987654321";

			consignment1.HVC_ShipperName = "Some Company";
			consignment1.HVC_ShipperAddress1 = "22 Shipper Street";
			consignment1.HVC_ShipperCity = "Wellington";
			consignment1.HVC_ShipperState = "WLG";
			consignment1.HVC_ShipperPostcode = "54321";
			consignment1.HVC_RN_NKShipperCountryCode = "NZ";
			consignment1.HVC_ShipperContact = "Randy";
			consignment1.HVC_ShipperEmail = "randy@randysdomain.com";
			consignment1.HVC_ShipperPhone = "01189998819991197253";
			consignment1.HVC_ShipperMobile = "+555 5555";
			consignment1.HVC_ShipperFax = "8";

			var customsDeclaration1 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			customsDeclaration1.JE_DeclarationReference = "InnocuousWhitePowder";
			consignment1.HVC_JE_ImportDeclaration = customsDeclaration1.PK;
			var customsReferenceNumbers = consignment1.CustomsReferenceNumbers;
			var customsReferenceNumber = customsReferenceNumbers.AddNew();
			customsReferenceNumber.CE_EntryType = "ISF";
			customsReferenceNumber.CE_EntryNum = "ISF0000001";

			var consignment2 = bookingHeader.Consignments.AddNew();
			consignment2.HVC_ConsignmentId = "CONSIGN2";
			consignment2.HVC_ShipperReference = "XYZ456";
			consignment2.HVC_ItemCount = 0;
			consignment2.HVC_GoodsValue = 50;
			consignment2.HVC_RX_NKGoodsValueCurrency = "USD";
			consignment2.HVC_WeightUQ = Weight.Kilograms;
			consignment2.HVC_VolumeUQ = Volume.CubicMetres;
			consignment2.HVC_GoodsDescription = "Biscuits";
			consignment2.HVC_ConsigneeInstructions = "Leave at back";
			consignment2.HVC_INCO = IncoTerms.DeliveredAtPlace;
			consignment2.HVC_IsHazardous = false;
			consignment2.HVC_IsSignatureRequired = false;
			consignment2.HVC_AuthorityToLeave = false;
			consignment2.HVC_IsTracked = false;
			consignment2.HVC_RequiresFumigation = true;
			consignment2.HVC_IsPersonalEffects = true;
			consignment2.HVC_IsTimber = true;
			consignment2.HVC_IsPerishable = true;
			consignment2.HVC_IsTaxPrePaid = false;
			consignment2.HVC_RS_NKServiceLevel = "D2D";

			var destinationDepotOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			destinationDepotOrg2.MainAddress.Address1 = "13 Destination St";
			consignment2.HVC_OA_DestinationDepot = destinationDepotOrg2.MainAddress.PK;

			consignment2.HVC_ConsigneeName = "Bobs Company";
			consignment2.HVC_ConsigneeAddress1 = "12 Something St";
			consignment2.HVC_ConsigneeCity = "Sydney";
			consignment2.HVC_ConsigneeState = "NSW";
			consignment2.HVC_ConsigneePostcode = "2000";
			consignment2.HVC_RN_NKConsigneeCountryCode = "AU";
			consignment2.HVC_ConsigneeContact = "Bob";
			consignment2.HVC_ConsigneeEmail = "bob@bob.com";

			consignment2.HVC_ShipperName = "Another Company";
			consignment2.HVC_ShipperAddress1 = "23 Another St";
			consignment2.HVC_ShipperCity = "Melbourne";
			consignment2.HVC_ShipperState = "VIC";
			consignment2.HVC_ShipperPostcode = "3000";
			consignment2.HVC_RN_NKShipperCountryCode = "AU";
			consignment2.HVC_ShipperContact = "Vic";
			consignment2.HVC_ShipperEmail = "victor@fakedomain.com";

			var customsDeclaration2 = Factory.NewWithValidTestData<BaseJobDeclaration>();
			customsDeclaration2.JE_DeclarationReference = "HaltInTheNameOfTheLaw";
			consignment2.HVC_JE_ExportDeclaration = customsDeclaration2.PK;

			consignment2.HVC_VendorIdentifier = "VID0001";
			#endregion

			#region Items

			var item1_1 = consignment1.Items.AddNew();
			item1_1.HVI_ShipperReference = "D9901239028";
			item1_1.HVI_CurrentBarcode = "12345678901234567890123456789012345678901234567890";
			item1_1.HVI_F3_NKPackType = "BOX";
			item1_1.HVI_Height = 3m;
			item1_1.HVI_Length = 4m;
			item1_1.HVI_Width = 5m;
			item1_1.HVI_UnitOfDimension = Length.Metres;
			item1_1.HVI_ManifestedWeight = 5;
			item1_1.HVI_ActualWeight = 10;
			item1_1.HVI_ManifestedVolume = 0.5;
			item1_1.HVI_ActualVolume = 0.5;
			item1_1.HVI_Status = HVLVItemStatus.Codes.ManifestedByETailer;
			item1_1.HVI_IsDamaged = true;
			item1_1.HVI_IsPillaged = true;
			item1_1.HVI_ItemId = "HVI00001";

			var undg1 = item1_1.UNDGs.AddNew();
			undg1.DI_IMOClass = "1.1D";

			var item1_2 = consignment1.Items.AddNew();
			item1_2.HVI_ShipperReference = "D5465421481";
			item1_2.HVI_F3_NKPackType = "BOX";
			item1_2.HVI_ManifestedWeight = 10;
			item1_2.HVI_ActualWeight = 10;
			item1_2.HVI_ManifestedVolume = 0.5;
			item1_2.HVI_ActualVolume = 0.5;
			item1_2.HVI_Status = HVLVItemStatus.Codes.PendingClearanceAtDestinationDepot;
			item1_2.HVI_IsDamaged = true;
			item1_2.HVI_IsPillaged = false;
			item1_2.HVI_ItemId = "HVI00002";

			var undg2 = item1_2.UNDGs.AddNew();
			undg2.DI_IMOClass = "1.1D";

			var undg3 = item1_2.UNDGs.AddNew();
			undg3.DI_IMOClass = "1.1D";

			var item2_1 = consignment2.Items.AddNew();
			item2_1.HVI_ShipperReference = "X0012931292";
			item2_1.HVI_F3_NKPackType = "PKG";
			item2_1.HVI_ManifestedWeight = 10;
			item2_1.HVI_ActualWeight = 10;
			item2_1.HVI_ManifestedVolume = 0.5;
			item2_1.HVI_ActualVolume = 1;
			item2_1.HVI_Status = HVLVItemStatus.Codes.ReadyForLastMileDelivery;
			item2_1.HVI_IsDamaged = false;
			item2_1.HVI_IsPillaged = true;
			item2_1.HVI_ContainerNumber = "CTNR654321";
			item2_1.HVI_ItemId = "HVI00003";

			#endregion

			#region Item Lines

			var line1_1_1 = item1_1.Lines.AddNew();
			line1_1_1.HVS_OriginTariff = "123456";
			line1_1_1.HVS_RN_NKOriginCountryCode = "AU";
			line1_1_1.HVS_CustomsValue = 12.345;
			line1_1_1.HVS_IntrinsicValue = 54.321;
			line1_1_1.HVS_DestinationTariff = "654321";
			line1_1_1.HVS_GoodsDescription = "Keyboard";
			line1_1_1.HVS_GrossWeight = 1.23;
			line1_1_1.HVS_ItemURL = "www.google.com/keyboard";
			line1_1_1.HVS_NetWeight = 3.21;
			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "KBD001";
			var relation = product.RelatedOrganisations.AddNew();
			relation.OU_OH = bookingHeader.BillToParty.OA_OH;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Supplier;
			line1_1_1.HVS_ProductCode = "KBD001";
			line1_1_1.HVS_Quantity = 2;
			line1_1_1.HVS_WeightUnit = "KG";

			var line1_1_2 = item1_1.Lines.AddNew();
			line1_1_2.HVS_OriginTariff = "12345678";
			line1_1_2.HVS_RN_NKOriginCountryCode = "NZ";

			var line2_1_1 = item2_1.Lines.AddNew();
			line2_1_1.HVS_OriginTariff = "123123";
			line2_1_1.HVS_RN_NKOriginCountryCode = "FR";

			#endregion

			#endregion

			return bookingHeader;
		}

		#endregion
	}
}
