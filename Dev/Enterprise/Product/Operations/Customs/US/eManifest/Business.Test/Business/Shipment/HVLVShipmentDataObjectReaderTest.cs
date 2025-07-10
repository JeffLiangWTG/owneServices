using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.DataTransfer.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;
using Country = Enterprise.UniversalDataBuss.DataObjects.Universal.Country;
using Currency = Enterprise.UniversalDataBuss.DataObjects.Universal.Currency;
using UniversalXml = Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Customs.US.eManifest.Business.Testing
{
	sealed class HVLVShipmentDataObjectReaderTest : DataObjectReaderTest
	{
		public void TestShipmentB0_DescriptionOfCargoNotPopulated()
		{
			var dataObject = new UniversalXml.Shipment();
			dataObject.GoodsDescription = "Nice cup of coffee";

			var shipment = new HVLVShipmentDataObjectReader(dataObject, Factory.New<Trip>(), new UniversalXml.Shipment(), new UniversalXml.Shipment(), new DummyLogger(), Factory).ReadIntoBusinessObject();

			var newNotesCount = Factory.BOFactory.GetChanges().GetAddedObjects().OfType<StmNote>().Count();
			AssertEquals("Should not create any StmNote", 0, newNotesCount);
			AssertNullOrEmpty("B0_DescriptionOfCargo should not be populated", shipment.B0_DescriptionOfCargo);
		}

		public void TestShipmentB0_PortOfLadingKCodePolulated()
		{
			var unloco = Factory.New<RefUNLOCO>();
			unloco.RL_Code = "AUWTG";
			var locoMap = unloco.RefLocoMaps.AddNew();
			locoMap.RY_RN = new RefCountry.Loader(Factory.BOFactory).LoadForCountry(CountryCodes.UnitedStates).PK;
			locoMap.RY_LocalPortCode = "74731";
			locoMap.RY_SystemUsage = USLocoMapSystemUsageList.Codes.SCK;

			Factory.SaveForTesting();

			var consolDataObject = new UniversalXml.Shipment();
			consolDataObject.PortOfLoading = UNLOCO.New(unloco);
			var shipment = new HVLVShipmentDataObjectReader(new UniversalXml.Shipment(), Factory.New<Trip>(), new UniversalXml.Shipment(), consolDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			AssertEquals("precondition", "AUWTG", shipment.B0_RL_NKPortOfLading);
			AssertEquals("74731", shipment.B0_PortOfLadingKCode);
		}

		public void TestB0_IssuerSCACIsFromTripSCAC()
		{
			var consolDataObject = new UniversalXml.Shipment();
			var trip = Factory.NewWithValidTestData<Trip>();
			trip.BH_CarrierSCAC = "CARR";

			var shipment = new HVLVShipmentDataObjectReader(new UniversalXml.Shipment(), trip, new UniversalXml.Shipment(), consolDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			AssertEquals("CARR", shipment.B0_IssuerSCAC);
		}

		public void TestShipmentIsCopyingSetToFalseAfterReading()
		{
			var shipment = new HVLVShipmentDataObjectReader(new UniversalXml.Shipment(), Factory.New<Trip>(), new UniversalXml.Shipment(), new UniversalXml.Shipment(), new DummyLogger(), Factory).ReadIntoBusinessObject();
			var isCopyingPropertyInfo = typeof(BusinessObject).GetProperty("IsCopying", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
			Assert(!(bool)isCopyingPropertyInfo.GetValue(shipment));
		}

		public void TestShipmentTotalLinesValuIsConvertedToUSD()
		{
			SetupCurrencyExchangeRate();
			var consignmentDataObject = new UniversalXml.Shipment();
			consignmentDataObject.GoodsValueCurrency = Currency.New(RefCurrency.LoadFromCurrencyCode(Factory.BOFactory, CurrencyCodes.Australia));
			consignmentDataObject.CommercialInfo = new CommercialInfo();
			consignmentDataObject.CommercialInfo.CommercialInvoiceCollection = new DataObjectList<CommercialInvoiceHeader>();
			var invoice = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentDataObject.CommercialInfo.CommercialInvoiceCollection.Add(invoice);
			invoice.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine();
			invoice.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.CustomsValue = 56m;
			invoiceLine.InvoiceQuantity = 1;

			var shipment = new HVLVShipmentDataObjectReader(consignmentDataObject, Factory.New<Trip>(), new UniversalXml.Shipment(), new UniversalXml.Shipment(), new DummyLogger(), Factory).ReadIntoBusinessObject();

			AssertEquals(35m, shipment.B0_GoodsValue);
		}

		public void TestWhenPartyReadInto_ThenIsCopyingSetToTrue()
		{
			var consolLevelDataObject = GetConsolLevelDataObjectForTest(out var shipmentLevelDataObject);
			var isCopyingPropertyInfo = typeof(BusinessObject).GetProperty("IsCopying", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);
			var trip = new HVLVTripDataObjectReader(consolLevelDataObject, shipmentLevelDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();

			var parties = trip.Shipments.SelectMany(s => s.Parties);

			Assert("new Party should have IsCopying set to true", parties.All(party => (bool)isCopyingPropertyInfo.GetValue(party)));
		}

		public void TestPopulateShipmentDetails()
		{
			var trip = new HVLVeManifestDataTransferTestHelper(Factory).Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");

			CombineAssertions("Shipment details", () =>
			{
				AssertNotNull(shipment);
				AssertEquals("Type", "SEC", shipment.B0_ShipmentType);
				AssertEquals("Master Bill Number", "TEST WAYBILL", shipment.B0_MasterBillNumber);
				AssertEquals("Shipment ID", "TEST SHIPPER", shipment.B0_ReferenceID);
				AssertEquals("Port of Loading", "CATOR", shipment.B0_RL_NKPortOfLading);
				AssertEquals("Schedule K", "01535", shipment.B0_PortOfLadingKCode);
				// AssertEquals("Place of Receipt", "USLAX", shipment.B0_PlaceOfReceipt); // HVLVConsignment doesn't have PlaceOfReceipt mapped

				AssertEquals("Quantity", 1, shipment.B0_ManifestQty);
				AssertEquals("UQ", "PCS", shipment.B0_ManifestUQ);
				AssertEquals("Weight", 0.002m, shipment.B0_Weight);
				AssertEquals("Weight UQ", "KG", shipment.B0_WeightUQ);
				AssertEquals("Volume", 0.01m, shipment.B0_Volume);
				AssertEquals("Volume UQ", "L", shipment.B0_VolumeUQ);
				AssertEquals("Shipment Value", 120m, shipment.B0_GoodsValue);
				AssertEquals("Country of Origin", CountryCodes.Belgium, shipment.B0_RN_NKCountryOfExport);
			});
		}

		public void TestMatchAndSyncShipmentDetails()
		{
			var helper = new HVLVeManifestDataTransferTestHelper(Factory);
			var trip = helper.Trip;
			var originalShipment = trip.Shipments.FirstOrDefault(s => s.B0_MasterBillNumber == "TEST WAYBILL");

			CombineAssertions("Preconditions:", () =>
			{
				AssertNotNull(originalShipment);
				AssertEquals("Weight", 0.002m, originalShipment.B0_Weight);
				AssertEquals("Weight UQ", "KG", originalShipment.B0_WeightUQ);
				AssertEquals("Volume", 0.01m, originalShipment.B0_Volume);
				AssertEquals("Volume UQ", "L", originalShipment.B0_VolumeUQ);
			});

			var forwardingShipmentLevelDataObject = helper.ForwardingShipmentLevelDataObject;
			var consignmentDataObject = forwardingShipmentLevelDataObject.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.Value == "Test Waybill");

			consignmentDataObject.TotalWeight = 0.004m;
			consignmentDataObject.TotalWeightUnit = new UnitOfWeight { Code = "G" };
			consignmentDataObject.TotalVolume = 0.02m;
			consignmentDataObject.TotalVolumeUnit = new UnitOfVolume { Code = "ML" };

			trip = new HVLVTripDataObjectReader(helper.ForwardingConsolLevelDataObject, forwardingShipmentLevelDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_MasterBillNumber == "TEST WAYBILL");

			CombineAssertions("Shipment should be matched and synced", () =>
			{
				AssertNotNull(shipment);
				AssertEquals("Original and current shipment is the same", originalShipment.PK, shipment.PK);
				AssertEquals("Weight", 0.004m, shipment.B0_Weight);
				AssertEquals("Weight UQ", "G", shipment.B0_WeightUQ);
				AssertEquals("Volume", 0.02m, shipment.B0_Volume);
				AssertEquals("Volume UQ", "ML", shipment.B0_VolumeUQ);
			});
		}

		public void TestPopulateShipmentDetails_WhenItemLinesGoodsOriginIsEmpty_ThenPopulateShipmentCountryOfOriginWithFirstConsolPortOfLoading()
		{
			var trip = new HVLVeManifestDataTransferTestHelper(Factory).Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "CONSIGNMENT2 SHIPPER REF");

			AssertEquals("Expected Shipment's Country of Origin to be equal to the first consol's port of loading when item line's goods origin is empty", CountryCodes.Canada, shipment.B0_RN_NKCountryOfExport);
		}

		public void TestPopulateCommodityDetails_WhenItemLinesGoodsOriginIsEmpty_ThenPopulateCommodityCountryOfOriginWithFirstConsolPortOfLoading()
		{
			var trip = new HVLVeManifestDataTransferTestHelper(Factory).Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "CONSIGNMENT3 SHIPPER REF");
			var commodity = shipment.Commodities.FirstOrDefault(c => c.BY_Description.Equals("Clay"));

			AssertEquals("Expected Commodity's Country of Origin to be equal to the first consol's port of loading when item line's goods origin is empty", CountryCodes.Canada, commodity.BY_RN_NKCountryOfOrigin);
		}

		public void TestPopulateCommodityDetails_ItemLinesGoodsDescriptionFallback()
		{
			var helper = new HVLVeManifestDataTransferTestHelper(Factory);
			var shipmentDataObject = helper.ForwardingShipmentLevelDataObject;
			var consignmentDataObject = shipmentDataObject.SubShipmentCollection.FirstOrDefault(s => s.OwnerRef.Equals("Test Shipper"));
			var itemDataObject = consignmentDataObject.PackingLineCollection.FirstOrDefault();
			var itemLineDataObject = consignmentDataObject.CommercialInfo
				.CommercialInvoiceCollection.FirstOrDefault()
				.CommercialInvoiceLineCollection.FirstOrDefault();

			var trip = helper.Trip;
			var tripShipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");
			var commodity = tripShipment.FirstCommodity;

			AssertEquals("Expected Commodity's Goods Description equal to the ItemLine's Goods Description", "Meds", commodity.BY_Description);

			itemLineDataObject.Description = "";
			Factory.SaveForTesting();
			trip = helper.Trip;
			tripShipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");
			commodity = tripShipment.FirstCommodity;

			AssertEquals("Expected when ItemLine's Goods Description is empty, Commodity's Goods Description to be equal to the Item's Goods Description", "covid-19 vaccine", commodity.BY_Description);

			itemDataObject.GoodsDescription = "";
			Factory.SaveForTesting();
			trip = helper.Trip;
			tripShipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");
			commodity = tripShipment.FirstCommodity;

			AssertEquals("Expected when ItemLine and Item's Goods Description is empty, Commodity's Goods Description to be equal to the Consignment's Goods Description", "medicine", commodity.BY_Description);
		}

		public void TestPopulateFirstCommodity()
		{
			var trip = new HVLVeManifestDataTransferTestHelper(Factory).Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");

			CombineAssertions("First Commodity Details", () =>
			{
				AssertNotNull(shipment);
				AssertEquals("First commodity description", "Meds", shipment.FirstCommodityDescription);

				AssertEquals("First commodity value", 120m, shipment.FirstCommodityMonetaryValue);
				AssertEquals("First commodity weight", 0.12m, shipment.FirstCommodityWeight);
				AssertEquals("First commodity weight UQ", "KG", shipment.FirstCommodityWeightUnit);
				AssertEquals("First commodity country of origin", CountryCodes.Belgium, shipment.FirstCommodityCountryOfOrigin);

				AssertEquals("First commodity harmonized numbers", "8382.38.93", shipment.FirstCommodityHarmonizedNumbers);
				AssertEquals("First commodity packages", 5, shipment.FirstCommodityPieceCount);
				AssertEquals("First commodity package type", "PCS", shipment.FirstCommodityManifestUnitCode);
			});
		}

		public void TestPopulateMultipleCommoditiesFromItemLines()
		{
			var trip = new HVLVeManifestDataTransferTestHelper(Factory).Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "CONSIGNMENT3 SHIPPER REF");
			var commodity1 = shipment.Commodities.FirstOrDefault(c => c.BY_Description == "Clay");
			var commodity2 = shipment.Commodities.FirstOrDefault(c => c.BY_Description == "Gold");

			CombineAssertions("First Commodity Details", () =>
			{
				AssertNotNull(commodity1);
				AssertEquals("First commodity description", "Clay", commodity1.BY_Description);

				AssertEquals("First commodity value", 100m, commodity1.BY_MonetaryValue);
				AssertEquals("First commodity weight", 0.1m, commodity1.BY_GrossWeight);
				AssertEquals("First commodity weight UQ", "KG", commodity1.BY_GrossWeightUnit);

				AssertEquals("First commodity packages", 3, commodity1.BY_PieceCount);
				AssertEquals("First commodity package type", "PCS", commodity1.BY_ManifestUnitCode);
			});

			CombineAssertions("Second Commodity Details", () =>
			{
				AssertNotNull(commodity2);
				AssertEquals("Second commodity description", "Gold", commodity2.BY_Description);

				AssertEquals("Second commodity value", 150m, commodity2.BY_MonetaryValue);
				AssertEquals("Second commodity weight", 0.15m, commodity2.BY_GrossWeight);
				AssertEquals("Second commodity weight UQ", "KG", commodity2.BY_GrossWeightUnit);
				AssertEquals("Second commodity country of origin", CountryCodes.Canada, commodity2.BY_RN_NKCountryOfOrigin);

				AssertEquals("Second commodity harmonized numbers", "5739.37.36", commodity2.BY_HarmonizedNumbers);
				AssertEquals("Second commodity packages", 7, commodity2.BY_PieceCount);
				AssertEquals("Second commodity package type", "PCS", commodity2.BY_ManifestUnitCode);
			});
		}

		public void TestPopulateCommodity_DeleteAndRecreateCommodityWhenSyncing()
		{
			var helper = new HVLVeManifestDataTransferTestHelper(Factory);
			var trip = helper.Trip;
			var orignalCommodity = trip.Shipments.Single(s => s.B0_MasterBillNumber == "TEST WAYBILL").Commodities.Single();

			CombineAssertions("Preconditions", () =>
			{
				AssertNotNull(orignalCommodity);
				AssertEquals("Original commodity description", "Meds", orignalCommodity.BY_Description);
				AssertEquals("Original commodity value", 120m, orignalCommodity.BY_MonetaryValue);
				AssertEquals("Original commodity weight", 0.12m, orignalCommodity.BY_GrossWeight);
				AssertEquals("Original commodity weight UQ", "KG", orignalCommodity.BY_GrossWeightUnit);
			});

			var forwardingShipmentLevelDataObject = helper.ForwardingShipmentLevelDataObject;
			var consignmentDataObject = forwardingShipmentLevelDataObject.SubShipmentCollection.FirstOrDefault(x => x.WayBillNumber.Value == "Test Waybill");
			var commercialInvoiceLine = consignmentDataObject.CommercialInfo.CommercialInvoiceCollection.Single().CommercialInvoiceLineCollection.Single();

			commercialInvoiceLine.Description = "Not Meds";

			trip = new HVLVTripDataObjectReader(helper.ForwardingConsolLevelDataObject, forwardingShipmentLevelDataObject, new DummyLogger(), Factory).ReadIntoBusinessObject();
			var commodity = trip.Shipments.Single(s => s.B0_MasterBillNumber == "TEST WAYBILL").Commodities.Single();

			CombineAssertions("Existing commodity is deleted and a new one is created", () =>
			{
				AssertNotNull(commodity);
				AssertNotEquals("The original and current commodity is different", commodity.PK, orignalCommodity.PK);
				AssertEquals("Commodity description changed", "Not Meds", commodity.BY_Description);
				AssertEquals("New Commodity value still the same", 120m, commodity.BY_MonetaryValue);
				AssertEquals("New commodity weight still the same", 0.12m, commodity.BY_GrossWeight);
				AssertEquals("New commodity weight UQ still the same", "KG", commodity.BY_GrossWeightUnit);
			});
		}

		public void TestCommodityMonetaryValueIsConvertedToUSD()
		{
			SetupCurrencyExchangeRate();

			var helper = new HVLVeManifestDataTransferTestHelper(Factory);
			var shipmentDataObject = helper.ForwardingShipmentLevelDataObject;
			var consignmentDataObject = shipmentDataObject.SubShipmentCollection.FirstOrDefault(s => s.OwnerRef.Equals("Test Shipper"));

			consignmentDataObject.GoodsValueCurrency = Currency.New(RefCurrency.LoadFromCurrencyCode(Factory.BOFactory, CurrencyCodes.Australia));

			var itemDataObject = consignmentDataObject.PackingLineCollection.FirstOrDefault();
			var itemLineDataObject = consignmentDataObject.CommercialInfo
				.CommercialInvoiceCollection.FirstOrDefault()
				.CommercialInvoiceLineCollection.FirstOrDefault();

			itemLineDataObject.CustomsValue = 100m;

			var trip = helper.Trip;
			var tripShipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");
			var commodity = tripShipment.FirstCommodity;

			AssertEquals(62m, commodity.BY_MonetaryValue);
		}

		public void TestPopulateConsignee()
		{
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.OA_Address1 = "Consignee Address1";
			consigneeAddress.OA_Address2 = "Consignee Address2";
			consigneeAddress.OA_City = "LAX";
			consigneeAddress.OA_CompanyNameOverride = "Consignee Company";
			consigneeAddress.OA_PostCode = "55555";
			consigneeAddress.OA_RN_NKCountryCode = "US";
			consigneeAddress.OA_State = "CA";
			consigneeAddress.OA_Phone = "1234567890";
			consigneeAddress.OA_Fax = "1234567891";
			consigneeAddress.OA_Email = "abc@123.com";

			var consignee = consigneeAddress.Header;
			consignee.OH_FullName = "Consignee Organization";

			var helper = new HVLVeManifestDataTransferTestHelper(Factory);
			helper.ConsignmentConsigneeOrgAddress = consigneeAddress;
			var trip = helper.Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");

			CombineAssertions(() =>
			{
				AssertEquals("Full name", "Consignee Organization", shipment.Consignee.Organisation.OH_FullName);
				AssertEquals("Company name", "Consignee Company", shipment.Consignee.CompanyName);
				AssertEquals("Contact name", "Mike", shipment.Consignee.E2_Contact);
				AssertEquals("Address1", "Consignee Address1", shipment.Consignee.Address1);
				AssertEquals("Address2", "Consignee Address2", shipment.Consignee.Address2);
				AssertEquals("City", "LAX", shipment.Consignee.City);
				AssertEquals("Postcode", "55555", shipment.Consignee.Postcode);
				AssertEquals("Country", "US", shipment.Consignee.Country.Code);
				AssertEquals("State", "California", shipment.Consignee.State);
				AssertEquals("Phone", "1234567890", shipment.Consignee.E2_Phone);
				AssertEquals("Fax", "1234567891", shipment.Consignee.E2_Fax);
				AssertEquals("Email", "abc@123.com", shipment.Consignee.E2_Email);
				Assert("Address Override", !shipment.Consignee.E2_AddressOverride);
				AssertEquals("When Address is not override, E2_ValidationStatus matches linked address OA_ValidationStatus", consigneeAddress.OA_ValidationStatus, shipment.Consignee.E2_ValidationStatus);
			});
		}

		public void TestPopulateConsigneeFromAddressOverride()
		{
			var trip = new HVLVeManifestDataTransferTestHelper(Factory).Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");

			CombineAssertions(() =>
			{
				AssertEquals(true, shipment.Consignee.E2_SuppressAddressValidationError);
				AssertEquals("Company name", "Bob", shipment.Consignee.E2_CompanyName);
				AssertEquals("Address1", "123 Spender St", shipment.Consignee.E2_Address1);
				AssertEquals("Address2", "Backyard", shipment.Consignee.E2_Address2);
				AssertEquals("City", "Chicago", shipment.Consignee.E2_City);
				AssertEquals("Postcode", "60000", shipment.Consignee.E2_Postcode);
				AssertEquals("Country", "US", shipment.Consignee.E2_RN_NKCountryCode);
				AssertEquals("State", "IL", shipment.Consignee.E2_State);
				AssertEquals("Contact name", "Mike", shipment.Consignee.E2_Contact);
				AssertEquals("Phone", "567823478", shipment.Consignee.E2_Phone);
				AssertEquals("Fax", "783478234", shipment.Consignee.E2_Fax);
				AssertEquals("Email", "Mike@purchase.com", shipment.Consignee.E2_Email);
				Assert("Address Override", shipment.Consignee.E2_AddressOverride);
				AssertEquals("Validation Status", AddressValidationStatus.ManuallyVerified, shipment.Consignee.E2_ValidationStatus);
			});
		}

		public void TestPopulateShipper()
		{
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.OA_Address1 = "Shipper Address1";
			shipperAddress.OA_Address2 = "Shipper Address2";
			shipperAddress.OA_City = "AUSYD";
			shipperAddress.OA_CompanyNameOverride = "Shipper Company";
			shipperAddress.OA_PostCode = "2000";
			shipperAddress.OA_RN_NKCountryCode = "AU";
			shipperAddress.OA_State = "NSW";
			shipperAddress.OA_Phone = "1234567892";
			shipperAddress.OA_Fax = "1234567893";
			shipperAddress.OA_Email = "def@456.com";

			var shipper = shipperAddress.Header;
			shipper.OH_FullName = "Shipper Organization";

			var helper = new HVLVeManifestDataTransferTestHelper(Factory);
			helper.ConsignmentShipperOrgAddress = shipperAddress;
			var trip = helper.Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");

			CombineAssertions(() =>
			{
				AssertEquals("Full name", "Shipper Organization", shipment.Shipper.Organisation.OH_FullName);
				AssertEquals("Company name", "Shipper Company", shipment.Shipper.CompanyName);
				AssertEquals("Contact name", "John", shipment.Shipper.E2_Contact);
				AssertEquals("Address1", "Shipper Address1", shipment.Shipper.Address1);
				AssertEquals("Address2", "Shipper Address2", shipment.Shipper.Address2);
				AssertEquals("City", "AUSYD", shipment.Shipper.City);
				AssertEquals("Postcode", "2000", shipment.Shipper.Postcode);
				AssertEquals("Country", "AU", shipment.Shipper.Country.Code);
				AssertEquals("State", "New South Wales", shipment.Shipper.State);
				AssertEquals("Phone", "1234567892", shipment.Shipper.E2_Phone);
				AssertEquals("Fax", "1234567893", shipment.Shipper.E2_Fax);
				AssertEquals("Email", "def@456.com", shipment.Shipper.E2_Email);
				Assert("Address Override", !shipment.Shipper.E2_AddressOverride);
				AssertEquals("When Address is not override, E2_ValidationStatus matches linked address OA_ValidationStatus", shipperAddress.OA_ValidationStatus, shipment.Shipper.E2_ValidationStatus);
			});
		}

		public void TestPopulateShipperFromAddressOverride()
		{
			var trip = new HVLVeManifestDataTransferTestHelper(Factory).Trip;
			var shipment = trip.Shipments.FirstOrDefault(s => s.B0_ReferenceID == "TEST SHIPPER");

			CombineAssertions(() =>
			{
				AssertEquals(true, shipment.Shipper.E2_SuppressAddressValidationError);
				AssertEquals("Company name", "AMAZON CA", shipment.Shipper.E2_CompanyName);
				AssertEquals("Address1", "1 Online Shopping Ave", shipment.Shipper.E2_Address1);
				AssertEquals("Address2", "Mail Box", shipment.Shipper.E2_Address2);
				AssertEquals("City", "Toronto", shipment.Shipper.E2_City);
				AssertEquals("Postcode", "50000", shipment.Shipper.E2_Postcode);
				AssertEquals("Country", "CA", shipment.Shipper.E2_RN_NKCountryCode);
				AssertEquals("State", "ON", shipment.Shipper.E2_State);
				AssertEquals("Contact name", "John", shipment.Shipper.E2_Contact);
				AssertEquals("Phone", "923893478", shipment.Shipper.E2_Phone);
				AssertEquals("Fax", "78923789234", shipment.Shipper.E2_Fax);
				AssertEquals("Email", "John@amazon.ca", shipment.Shipper.E2_Email);
				Assert("Address Override", shipment.Shipper.E2_AddressOverride);
				AssertEquals("Validation Status", AddressValidationStatus.ManuallyVerified, shipment.Shipper.E2_ValidationStatus);
			});
		}

		UniversalXml.Shipment GetConsolLevelDataObjectForTest(out UniversalXml.Shipment shipmentLevelDataObject, bool isAddressOverride = false)
		{
			var result = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "CARRIER";
			carrier.OH_FullName = "We transport";
			var consigneeAddress = carrier.MainAddress;
			consigneeAddress.OA_CompanyNameOverride = "Carrier Company";
			consigneeAddress.OA_Address1 = "Carrier Address1";
			consigneeAddress.OA_Address2 = "Carrier Address2";
			consigneeAddress.OA_City = "USLAX";
			consigneeAddress.OA_PostCode = "78347834";
			consigneeAddress.OA_State = "CA";
			consigneeAddress.OA_RN_NKCountryCode = "US";
			var consigneeContact = carrier.Contacts.AddNew();
			consigneeContact.OC_ContactName = "Carrier Contact";
			consigneeContact.OC_Phone = "4878232";
			consigneeContact.OC_Fax = "34578978";
			consigneeContact.OC_Email = "carrier@global.com";

			var carrierDAddressDataObject = new OrganizationAddress();

			if (isAddressOverride)
			{
				carrierDAddressDataObject.AddressOverride = isAddressOverride;
				carrierDAddressDataObject.CompanyName = "Carrier Company Override";
				carrierDAddressDataObject.Address1 = "Carrier Address1 Override";
				carrierDAddressDataObject.Address2 = "Carrier Address2 Override";
				carrierDAddressDataObject.City = "Carrier City Override";
				carrierDAddressDataObject.Postcode = "60066";
				carrierDAddressDataObject.State = "Carrier State Override";
				carrierDAddressDataObject.Country = new Country() { Code = "US" };
				carrierDAddressDataObject.Contact = "Carrier Contact Override";
				carrierDAddressDataObject.Phone = "67234678";
				carrierDAddressDataObject.Fax = "12667833";
				carrierDAddressDataObject.Email = "Carrier Email Override";
			}
			else
			{
				carrierDAddressDataObject.OrganizationCode = carrier.OH_Code;
			}

			result.SetOrganizationAddressCollection(() => new List<OrganizationAddress>()
			{
				carrierDAddressDataObject
			});

			var newShipmentLevelDataObject = GetShipmentLevelDataObjectForTest(isAddressOverride);
			shipmentLevelDataObject = newShipmentLevelDataObject;

			result.SetSubShipmentCollection(() => new DataObjectList<UniversalXml.Shipment>()
			{
				newShipmentLevelDataObject
			});

			return result;
		}

		UniversalXml.Shipment GetShipmentLevelDataObjectForTest(bool isAddressOverride = false)
		{
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CSN";
			consignee.OH_FullName = "Consignee Organization";
			var consigneeAddress = consignee.MainAddress;
			consigneeAddress.OA_CompanyNameOverride = "Consignee Company";
			consigneeAddress.OA_Address1 = "Consignee Address1";
			consigneeAddress.OA_Address2 = "Consignee Address2";
			consigneeAddress.OA_City = "USLAX";
			consigneeAddress.OA_PostCode = "12345678";
			consigneeAddress.OA_State = "CA";
			consigneeAddress.OA_RN_NKCountryCode = "US";
			var consigneeContact = consignee.Contacts.AddNew();
			consigneeContact.OC_ContactName = "Consignee Contact";
			consigneeContact.OC_Phone = "1234567890";
			consigneeContact.OC_Fax = "1234567891";
			consigneeContact.OC_Email = "abc@123.com";

			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_Code = "SHP";
			shipper.OH_FullName = "Shipper Organization";
			var shipperAddress = shipper.MainAddress;
			shipperAddress.OA_CompanyNameOverride = "Shipper Company";
			shipperAddress.OA_Address1 = "Shipper Address1";
			shipperAddress.OA_Address2 = "Shipper Address2";
			shipperAddress.OA_City = "AUSYD";
			shipperAddress.OA_PostCode = "2000";
			shipperAddress.OA_State = "NSW";
			shipperAddress.OA_RN_NKCountryCode = "AU";
			var shipperContact = shipper.Contacts.AddNew();
			shipperContact.OC_ContactName = "Shipper Contact";
			shipperContact.OC_Phone = "1234567892";
			shipperContact.OC_Fax = "1234567893";
			shipperContact.OC_Email = "def@456.com";

			Factory.SaveForTesting();

			var shipment = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);

			shipment.VoyageFlightNo = "QF01";
			shipment.PortOfDischarge = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };
			shipment.WayBillNumber = "12345678";

			shipment.SetAddInfoCollection(() => new List<AddInfo>());
			shipment.AddInfoCollection.Add(new AddInfo() { Key = DataTransferConstants.AddInfoKeys.CarrierSCAC, Value = "ABC" });
			shipment.AddInfoCollection.Add(new AddInfo() { Key = DataTransferConstants.AddInfoKeys.PortOfDischargeScheduleD, Value = "DDDD" });

			shipment.SetTransportLegCollection(() => new DataObjectList<TransportLeg>());
			shipment.TransportLegCollection.Add(new TransportLeg());
			var firstLeg = shipment.TransportLegCollection[0];
			firstLeg.Carrier = new OrganizationAddress(DefaultDataObjectWriterStrategy.TestInstance);
			firstLeg.Carrier.SetRegistrationNumberCollection(() => new List<UniversalXml.RegistrationNumber>());
			firstLeg.Carrier.RegistrationNumberCollection.Add(new UniversalXml.RegistrationNumber()
			{
				Type = new RegistrationNumberType() { Code = "CCC" },
				CountryOfIssue = new Country() { Code = "US" },
				Value = "XYZ",
			});

			shipment.SetSubShipmentCollection(() => new DataObjectList<UniversalXml.Shipment>());
			var consignmentSubShipment = new UniversalXml.Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			shipment.SubShipmentCollection.Add(consignmentSubShipment);

			consignmentSubShipment.WayBillNumber = "87654321";
			consignmentSubShipment.PortOfLoading = new UNLOCO() { Code = "AUSYD", Name = "Sydney" };
			consignmentSubShipment.PlaceOfReceipt = new UNLOCO() { Code = "USLAX", Name = "Los Angeles" };

			consignmentSubShipment.GoodsDescription = "Test Commodity";
			consignmentSubShipment.OuterPacks = 1;
			consignmentSubShipment.OuterPacksPackageType = new PackageType() { Code = "PKG" };
			consignmentSubShipment.TotalWeight = 12;
			consignmentSubShipment.TotalWeightUnit = new UnitOfWeight() { Code = "KG" };
			consignmentSubShipment.TotalVolume = 34;
			consignmentSubShipment.TotalVolumeUnit = new UnitOfVolume() { Code = "M3" };
			consignmentSubShipment.GoodsValue = 56m;
			consignmentSubShipment.GoodsValueCurrency = Currency.New(RefCurrency.LoadFromCurrencyCode(Factory.BOFactory, CurrencyCodes.Australia));

			consignmentSubShipment.SetAddInfoCollection(() => new List<AddInfo>());
			var portOfLadingKCode = new AddInfo() { Key = "PortOfDischargeScheduleD", Value = "KKKK" };
			consignmentSubShipment.AddInfoCollection.Add(portOfLadingKCode);

			consignmentSubShipment.CommercialInfo = new CommercialInfo();
			consignmentSubShipment.CommercialInfo.SetWriterStrategy(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentSubShipment.CommercialInfo.SetCommercialInvoiceCollection(() => new DataObjectList<CommercialInvoiceHeader>());
			var invoiceHeader = new CommercialInvoiceHeader(DefaultDataObjectWriterStrategy.TestInstance);
			consignmentSubShipment.CommercialInfo.CommercialInvoiceCollection.Add(invoiceHeader);
			invoiceHeader.InvoiceAmount = 100m;
			invoiceHeader.Weight = 12m;
			invoiceHeader.WeightUnit = new UnitOfWeight() { Code = "KG" };

			invoiceHeader.SetCommercialInvoiceLineCollection(() => new DataObjectList<CommercialInvoiceLine>());
			var invoiceLine = new CommercialInvoiceLine(DefaultDataObjectWriterStrategy.TestInstance);
			invoiceHeader.CommercialInvoiceLineCollection.Add(invoiceLine);
			invoiceLine.HarmonisedCode = "1234.56.78";
			invoiceLine.InvoiceQuantity = 1;
			invoiceLine.InvoiceQuantityUnit = new CodeDescriptionPair() { Code = "PKG", Description = "Package" };

			consignmentSubShipment.SetOrganizationAddressCollection(() => new List<OrganizationAddress>());
			var consigneeAddressDataObject = new OrganizationAddress();
			if (isAddressOverride)
			{
				consigneeAddressDataObject.AddressOverride = isAddressOverride;
				consigneeAddressDataObject.CompanyName = "Consignee Company Override";
				consigneeAddressDataObject.Address1 = "Consignee Address1 Override";
				consigneeAddressDataObject.Address2 = "Consignee Address2 Override";
				consigneeAddressDataObject.City = "Consignee City Override";
				consigneeAddressDataObject.Postcode = "0000000000";
				consigneeAddressDataObject.State = "Consignee State Override";
				consigneeAddressDataObject.Country = new Country() { Code = "US" };
				consigneeAddressDataObject.Contact = "Consignee Contact Override";
				consigneeAddressDataObject.Phone = "1111111111";
				consigneeAddressDataObject.Fax = "2222222222";
				consigneeAddressDataObject.Email = "Consignee Email Override";
			}
			else
			{
				consigneeAddressDataObject.OrganizationCode = consignee.OH_Code;
			}
			consigneeAddressDataObject.AddressType = nameof(DocAddressType.ConsigneeDocumentaryAddress);
			consignmentSubShipment.OrganizationAddressCollection.Add(consigneeAddressDataObject);

			var shipperAddressDataObject = new OrganizationAddress();
			if (isAddressOverride)
			{
				shipperAddressDataObject.AddressOverride = isAddressOverride;
				shipperAddressDataObject.CompanyName = "Shipper Company Override";
				shipperAddressDataObject.Address1 = "Shipper Address1 Override";
				shipperAddressDataObject.Address2 = "Shipper Address2 Override";
				shipperAddressDataObject.City = "Shipper City Override";
				shipperAddressDataObject.Postcode = "3333333333";
				shipperAddressDataObject.State = "Shipper State Override";
				shipperAddressDataObject.Country = new Country() { Code = "AU" };
				shipperAddressDataObject.Contact = "Shipper Contact Override";
				shipperAddressDataObject.Phone = "4444444444";
				shipperAddressDataObject.Fax = "5555555555";
				shipperAddressDataObject.Email = "Shipper Email Override";
			}
			else
			{
				shipperAddressDataObject.OrganizationCode = shipper.OH_Code;
			}
			shipperAddressDataObject.AddressType = nameof(DocAddressType.ConsignorDocumentaryAddress);
			consignmentSubShipment.OrganizationAddressCollection.Add(shipperAddressDataObject);

			return shipment;
		}

		void SetupCurrencyExchangeRate()
		{
			var uSDExchRate = Factory.New<RefExchangeRate>();
			uSDExchRate.RE_ExpiryDate = ZDateTime.Today;
			uSDExchRate.RE_StartDate = ZDateTime.Today;
			uSDExchRate.RE_GC = Env.CurrentCompany.PK;
			uSDExchRate.RE_RX_NKExCurrency = CurrencyCodes.UnitedStates;
			uSDExchRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			uSDExchRate.RE_SellRate = 0.62m;

			var currencyAUD = CurrencyCodes.Australia;

			var query = new ZQuery(RefExchangeRateSchema.RE_GC, Env.CurrentCompany.PK);
			query.AddToFilter(RefExchangeRateSchema.RE_RX_NKExCurrency, currencyAUD);
			query.AddToFilter(RefExchangeRateSchema.RE_ExRateType, ExchangeRateTypes.Code.CustomsRate);
			query.AddToFilter(RefExchangeRateSchema.RE_ExpiryDate, SQLComparisonOperator.GreaterThanOrEqualTo, ZDate.Today);

			var audExchRate = Factory.LoadTop1<RefExchangeRate>(query) ?? Factory.New<RefExchangeRate>();

			audExchRate.RE_ExpiryDate = ZDateTime.Today;
			audExchRate.RE_StartDate = ZDateTime.Today;
			audExchRate.RE_GC = Env.CurrentCompany.PK;
			audExchRate.RE_RX_NKExCurrency = currencyAUD;
			audExchRate.RE_SellRate = 0.65m;
			audExchRate.RE_ExRateType = ExchangeRateTypes.Code.SellRate;

			Factory.SaveForTesting();
		}
	}
}
