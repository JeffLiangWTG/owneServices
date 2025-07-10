using System.Linq;
using CargoWise.Application;
using CargoWise.Definitions.Ecommerce;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.eTail.Business;
using Enterprise.eTail.Business.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;
using RefCusCodeListTypes = Enterprise.Core.Constants.Customs.Universal.RefCusCodeListTypes;
using UniversalIncoTerm = Enterprise.UniversalDataBuss.DataObjects.Universal.IncoTerm;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class HVLVConsignmentDataObjectWriterTest : TestCaseWithFactory
	{
		public void TestCommercialInvoiceLineCollectionWriterStrategy()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();
			var item = consignment.Items.AddNew();
			item.HVI_IsActive = true;
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_GoodsDescription = "HELLO";
			var writer = new HVLVConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, consignment),
					writerStrategy: new DataObjectWriterStrategyTestClass(s => s != nameof(CommercialInvoiceHeader.CommercialInvoiceLineCollection))));
			var shipment = writer.GetDataObject(consignment);
			var commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			AssertNull("CommercialInvoiceLineCollection - writerStrategy not allow", commercialInvoiceHeader.CommercialInvoiceLineCollection);
			var packingLine = shipment.PackingLineCollection.Single();
			var packedItem = packingLine.PackedItemCollection.Single();
			AssertNull("packedItem.CommercialInvoiceLineLink", packedItem.CommercialInvoiceLineLink);

			writer = new HVLVConsignmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.BRO, consignment)));
			shipment = writer.GetDataObject(consignment);
			commercialInvoiceHeader = shipment.CommercialInfo.CommercialInvoiceCollection.Single();
			var commercialInvoiceLine = commercialInvoiceHeader.CommercialInvoiceLineCollection.Single();
			AssertEquals("Link", 1, commercialInvoiceLine.Link);
			AssertEquals("Description", "HELLO", commercialInvoiceLine.Description);
			packingLine = shipment.PackingLineCollection.Single();
			packedItem = packingLine.PackedItemCollection.Single();
			AssertEquals("packedItem.CommercialInvoiceLineLink", 1, packedItem.CommercialInvoiceLineLink.Value);
		}

		public void TestGetDataObject_PopulateNotes()
		{
			var dataObject = GetConsignmentDataObject();
			AssertEquals("LTTSTORE", dataObject.NoteCollection.Single().NoteText);
		}

		public void TestGetDataObject_PopulateInstructions()
		{
			var dataObject = GetConsignmentDataObject();
			AssertEquals("HANDLE WITH CARE", dataObject.InstructionCollection.Single().ServiceInstruction);
		}

		public void TestGetDataObject_PopulateCustomReferences()
		{
			var dataObject = GetConsignmentDataObject();
			AssertContainsExactElementsInAnyOrder(new[] { "DECLARATION007", "DECLARATION008" }, dataObject.CustomsReferenceCollection.Select(r => r.Reference.Value));
		}

		public void TestGetDataObject_PopulateAdditionalReferences()
		{
			var dataObject = GetConsignmentDataObject();
			AssertEquals("ISF12345", dataObject.AdditionalReferenceCollection.Single(x => x.Type.Code.Value == "ISF").ReferenceNumber);
		}

		public void TestGetDataObject_PopulateLastMileCarrierDetails()
		{
			var dataObject = GetConsignmentDataObject();
			CombineAssertions(() =>
			{
				AssertEquals("123456", dataObject.CarrierAccount.AccountNumber);
				AssertEquals("Test Depot ID", dataObject.CarrierAccount.DepotID);
				AssertEquals("999", dataObject.CarrierServiceLevel.CarrierServiceCode);
				AssertEquals("EXP", dataObject.CarrierServiceLevel.Code);
				AssertEquals("99.9% faster service", dataObject.CarrierServiceLevel.Description);
			});
		}

		public void TestGetDataObject_PopulateCustomsClearanceStatus()
		{
			var dataObject = GetConsignmentDataObject();
			AssertEquals("REJ", dataObject.ConsolidatedCargoStatus.Code);
		}

		public void TestGetDataObject_PopulateConsigneeIdentifier()
		{
			var consignment = CreateTestConsignmentForExport();
			var cusEntryNum = consignment.CustomsReferenceNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "EIN123";
			cusEntryNum.CE_EntryType = "EIN";
			cusEntryNum.CE_RN_NKCountryCode = "US";

			var writingmanager = new DataWritingManager(new ActionInfo(null, consignment));
			writingmanager.FilteredDataContextType = DataContextType.USCustomsLowValueEntriesClearance;
			var dataObject = GetConsignmentDataObject(consignment, writingmanager);

			AssertEquals("ConsigneeIdentifier is set from entryNum when the FilteredDataContextType is US LowValue", "EIN123", dataObject.ConsigneeIdentifier);

			var dataObjectWithoutFilteredDataContextType = GetConsignmentDataObject(consignment);

			AssertNull("Don't set ConsigneeIdentifier if FilteredDataContextType is not USCustomsLowValueEntriesClearance", dataObjectWithoutFilteredDataContextType.ConsigneeIdentifier);
		}

		public void TestGetDataObject_PopulateOrganisations()
		{
			var dataObject = GetConsignmentDataObject();

			var consigneeDocumentaryAddress = dataObject.OrganizationAddressCollection.First(o => o.AddressType.Value == nameof(DocAddressType.ConsigneeDocumentaryAddress));
			CombineAssertions(() =>
			{
				AssertEquals("Test Address 1", consigneeDocumentaryAddress.Address1);
				AssertEquals("Test Address 2", consigneeDocumentaryAddress.Address2);
				AssertEquals("Test City", consigneeDocumentaryAddress.City);
				AssertEquals("Test State", consigneeDocumentaryAddress.State);
				AssertEquals("54321", consigneeDocumentaryAddress.Postcode);
				AssertEquals("Test Name", consigneeDocumentaryAddress.CompanyName);
				AssertEquals("Contact Name", "Consignee Contact", consigneeDocumentaryAddress.Contact);
				AssertEquals("Phone", "222333", consigneeDocumentaryAddress.Phone);
				AssertEquals("Mobile", "13722223333", consigneeDocumentaryAddress.Mobile);
				AssertEquals("Email", "consignee@test.org", consigneeDocumentaryAddress.Email);
				AssertEquals("Fax", "12", consigneeDocumentaryAddress.Fax);
			});

			var consignorDocumentaryAddress = dataObject.OrganizationAddressCollection.First(o => o.AddressType.Value == nameof(DocAddressType.ConsignorDocumentaryAddress));
			CombineAssertions(() =>
			{
				AssertEquals("Test Address 1", consignorDocumentaryAddress.Address1);
				AssertEquals("Test Address 2", consignorDocumentaryAddress.Address2);
				AssertEquals("Test City", consignorDocumentaryAddress.City);
				AssertEquals("Test State", consignorDocumentaryAddress.State);
				AssertEquals("54321", consignorDocumentaryAddress.Postcode);
				AssertEquals("Test Name", consignorDocumentaryAddress.CompanyName);
				AssertEquals("Contact Name", "Shipper Contact", consignorDocumentaryAddress.Contact);
				AssertEquals("Phone", "222333", consignorDocumentaryAddress.Phone);
				AssertEquals("Mobile", "13722223333", consignorDocumentaryAddress.Mobile);
				AssertEquals("Email", "shipper@test.org", consignorDocumentaryAddress.Email);
				AssertEquals("Fax", "34", consignorDocumentaryAddress.Fax);
			});

			var returnAddress = dataObject.OrganizationAddressCollection.First(o => o.AddressType.Value == nameof(DocAddressType.ReturnAddress));
			CombineAssertions(() =>
			{
				AssertEquals("Test Address 1", returnAddress.Address1);
				AssertEquals("Test Address 2", returnAddress.Address2);
				AssertEquals("Test City", returnAddress.City);
				AssertEquals("Test State", returnAddress.State);
				AssertEquals("54321", returnAddress.Postcode);
				AssertEquals("Test Name", returnAddress.CompanyName);
				AssertEquals("Contact Name", "Return Contact", returnAddress.Contact);
				AssertEquals("Phone", "222333", returnAddress.Phone);
				AssertEquals("Mobile", "13722223333", returnAddress.Mobile);
				AssertEquals("Email", "return@test.org", returnAddress.Email);
				AssertEquals("Fax", "56", returnAddress.Fax);
			});
		}

		public void TestGetDataObject_PopulatePackingLines()
		{
			var dataObject = GetConsignmentDataObject();
			var packingLine = dataObject.PackingLineCollection.Single();

			CombineAssertions(() =>
			{
				AssertEquals("LTTSTOREDOTCOM", packingLine.OrderReference.Value);
				AssertEquals("BOX", packingLine.PackType.Code);
				AssertEquals((ZDecimal)1.2, packingLine.ManifestedWeight);
				AssertEquals((ZDecimal)10, packingLine.Weight);
				AssertEquals((ZDecimal)0.5, packingLine.ManifestedVolume);
				AssertEquals((ZDecimal)0.5, packingLine.Volume);
				AssertEquals(1, packingLine.OutturnQty);
				AssertEquals(1, packingLine.OutturnDamagedQty);
				AssertEquals(0, packingLine.OutturnPillagedQty);
			});
		}

		public void TestGetDataObject_PopulatePackedItem()
		{
			var dataObject = GetConsignmentDataObject();
			var packingLine = dataObject.PackingLineCollection.Single();
			var packedItem = packingLine.PackedItemCollection.Single();

			AssertEquals(21M, packedItem.GoodsValue);
			AssertEquals("Mouse", packedItem.Description);
			AssertEquals(3.4M, packedItem.NetWeight);
			AssertEquals(1.2M, packedItem.GrossWeight);
			AssertEquals(1M, packedItem.PackedQuantity);
		}

		public void TestGetDataObject_PopulateCommercialInvoiceLine()
		{
			var dataObject = GetConsignmentDataObject();
			var commericalInvoiceHeader = dataObject.CommercialInfo.CommercialInvoiceCollection.Single();
			var commericalInvoiceLine = commericalInvoiceHeader.CommercialInvoiceLineCollection.Single();

			CombineAssertions(() =>
			{
				AssertEquals("321321", commericalInvoiceLine.HarmonisedCode);
				AssertEquals("Mouse", commericalInvoiceLine.Description);
				AssertEquals(3.4M, commericalInvoiceLine.NetWeight);
				AssertEquals(1.2M, commericalInvoiceLine.Weight);
				AssertEquals("KG", commericalInvoiceLine.WeightUnit.Code);
				AssertEquals("FR", commericalInvoiceLine.CountryOfOrigin.Code);
				AssertEquals(1M, commericalInvoiceLine.CustomsQuantity);
			});
		}

		public void TestGetDataObject_PopulateConsignmentDetails()
		{
			var dataObject = GetConsignmentDataObject();

			CombineAssertions(() =>
			{
				AssertEquals("HWB", dataObject.WayBillType.Code);
				AssertEquals("HVC001", dataObject.WayBillNumber);
				AssertEquals("LINUS", dataObject.OwnerRef);
				AssertEquals(1, dataObject.TotalNoOfPacks);
				AssertEquals(1, dataObject.TotalNoOfPieces);
				AssertEquals((ZDecimal)10, dataObject.GoodsValue);
				AssertEquals((ZDecimal)20, dataObject.InsuranceValue);
				AssertEquals((ZDecimal)30, dataObject.TransportValue);
				AssertEquals("USD", dataObject.GoodsValueCurrency.Code);
				AssertEquals("BOX", dataObject.TotalNoOfPacksPackageType.Code);
				AssertEquals(1, dataObject.OuterPacks);
				AssertEquals("BOX", dataObject.OuterPacksPackageType.Code);
				AssertEquals((ZDecimal)1.2, dataObject.ManifestedWeight);
				AssertEquals((ZDecimal)0.5, dataObject.ManifestedVolume);
				AssertEquals("RTX 5090 Ti", dataObject.GoodsDescription);
				AssertEquals(true, dataObject.IsHazardous);
				AssertEquals(false, dataObject.IsSignatureRequired);
				AssertEquals(true, dataObject.IsAuthorizedToLeave);
				AssertEquals(false, dataObject.IsTracked);
				AssertEquals("123456", dataObject.CarrierAccount.AccountNumber);
				AssertEquals("EXP", dataObject.CarrierServiceLevel.Code);
				AssertEquals("HVL", dataObject.ShipmentType.Code);
				AssertEquals("FOB", dataObject.ShipmentIncoTerm.Code);
				AssertEquals("STD", dataObject.ServiceLevel.Code);
				AssertEquals("HLD", dataObject.WarehouseReleaseStatus.Code);
				AssertEquals(true, dataObject.AddInfoCollection.GetZBoolValue("IsGSTPrePaid"));
			});
		}

		public void TestGetDataObject_PopulateWarehouseReleaseStatus()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();
			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.None;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;

			var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager);
			var dataObject = writer.GetDataObject(consignment);

			AssertEquals("HVC_ExportReleaseStatus exported as WarehouseReleaseStatus", HVLVReleaseStatus.Held, dataObject.WarehouseReleaseStatus.Code);

			consignment.HVC_ImportReleaseStatus = HVLVReleaseStatus.Cleared;
			consignment.HVC_ExportReleaseStatus = HVLVReleaseStatus.Held;
			dataObject = writer.GetDataObject(consignment);

			AssertEquals("HVC_ImportReleaseStatus exported as WarehouseReleaseStatus", HVLVReleaseStatus.Cleared, dataObject.WarehouseReleaseStatus.Code);
		}

		public void TestGetDataObject_PopulateConsignmentDetails_AddsIsGSTPrePaid_WhenTrueOrVendorIdentifierNotEmpty()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();
			consignment.HVC_IsTaxPrePaid = true;
			consignment.HVC_VendorIdentifier = ZString.Empty;

			var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager);
			var dataObject = writer.GetDataObject(consignment);

			var isGSTPrePaid = dataObject.AddInfoCollection.GetZStringValue("IsGSTPrePaid");
			AssertEquals("IsGSTPrePaid is Y if HVC_IsTaxPrePaid is true", "Y", isGSTPrePaid);

			consignment.HVC_IsTaxPrePaid = false;
			dataObject = writer.GetDataObject(consignment);

			isGSTPrePaid = dataObject.AddInfoCollection.GetZStringValue("IsGSTPrePaid");
			AssertEquals("IsGSTPrePaid is empty if HVC_IsTaxPrePaid is false", ZString.Empty, isGSTPrePaid);

			consignment.HVC_VendorIdentifier = "12345678";
			dataObject = writer.GetDataObject(consignment);

			isGSTPrePaid = dataObject.AddInfoCollection.GetZStringValue("IsGSTPrePaid");
			AssertEquals("IsGSTPrePaid is N if HVC_IsTaxPrePaid is false and HVC_VendorIdentifier is not empty", "N", isGSTPrePaid);
		}

		public void TestGetDataObject_OnlyActiveItemsAreExportedInXUS()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();

			var activeItem = consignment.Items.AddNew();
			activeItem.HVI_IsActive = true;
			activeItem.HVI_ShipperReference = "NVIDIA001";
			activeItem.HVI_JS_LoadedOnShipment = shipment.PK;

			var inactiveItem = consignment.Items.AddNew();
			inactiveItem.HVI_IsActive = false;
			inactiveItem.HVI_ShipperReference = "AMD001";
			inactiveItem.HVI_JS_LoadedOnShipment = shipment.PK;

			var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager);
			var dataObject = writer.GetDataObject(consignment);

			var packingLine = dataObject.PackingLineCollection.Single();
			AssertEquals("The packing line created is from the active item", "NVIDIA001", packingLine.OrderReference.Value);
		}

		public void TestGetDataObject_HarmonisedCodeShouldNotBeFormattedWhenWriteToXML()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "D9901239028";
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_GoodsDescription = "Keyboard";
			itemLine.HVS_DestinationTariff = "1234.56.7890";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKDestination = "USLAX";

			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager);
			var dataObject = writer.GetDataObject(consignment);

			var commercialInvoiceHeader = dataObject.CommercialInfo.CommercialInvoiceCollection.Single();

			var packingLine = dataObject.PackingLineCollection.Single(x => x.OrderReference.Value == "D9901239028");
			var packedItem = packingLine.PackedItemCollection.Single(x => x.Description.Value == "Keyboard");
			var commercialInvoiceLine = commercialInvoiceHeader.CommercialInvoiceLineCollection.Single(x => x.Link == packedItem.CommercialInvoiceLineLink);

			AssertEquals("Harmonised code should not be formatted", "1234567890", commercialInvoiceLine.HarmonisedCode);
		}

		public void TestGetDataObject_WaybillTypeIsStandardHouseWhenRecipientRoleTypeIsASY()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.Items.AddNew();

			var writeManager = new DataWritingManager(new ActionInfo(RecipientRoleType.ASY, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager);
			var dataObject = writer.GetDataObject(consignment);

			AssertEquals("STD", dataObject.WayBillType.Code);
		}

		public void TestGetDataObject_IfCacheDoesNotExistStoresConsignmentValues()
		{
			var bookingHeader = new HVLVBookingHeaderDataObjectWriterTest().PopulateAndGetBookingHeader();
			var consignment = (HVLVConsignment)bookingHeader.Consignments.First();

			var cache = new CodeDataObjectCache();
			var goodsValueCurrencyKey = cache.BuildKey(consignment.Lookups.GoodsValueCurrencies.GetType().FullName, consignment.HVC_RX_NKGoodsValueCurrency);
			var totalWeightUnitKey = cache.BuildKey(nameof(UnitOfWeight), consignment.HVC_WeightUQ);
			var totalVolumeUnitKey = cache.BuildKey(nameof(UnitOfVolume), consignment.HVC_VolumeUQ);
			var incoTermKey = cache.BuildKey(consignment.Lookups.INCOTermsList.GetType().FullName, consignment.HVC_INCO);
			var consigneeCountryKey = cache.BuildKey(consignment.Lookups.ConsigneeCountryCodes.GetType().FullName, consignment.HVC_RN_NKConsigneeCountryCode);
			var consignorCountryKey = cache.BuildKey(consignment.Lookups.ShipperCountryCodes.GetType().FullName, consignment.HVC_RN_NKShipperCountryCode);

			CombineAssertions("Precondition: All these values should be empty for the cache", () =>
			{
				AssertNull(cache.GetValue<Currency>(goodsValueCurrencyKey));
				AssertNull(cache.GetValue<UnitOfWeight>(totalWeightUnitKey));
				AssertNull(cache.GetValue<UnitOfVolume>(totalVolumeUnitKey));
				AssertNull(cache.GetValue<UniversalIncoTerm>(incoTermKey));
				AssertNull(cache.GetValue<Country>(consigneeCountryKey));
				AssertNull(cache.GetValue<Country>(consignorCountryKey));
			});

			var dataObject = new HVLVConsignmentDataObjectWriterForCacheTest(new DataWritingManager(new ActionInfo(null, bookingHeader)), cache).GetDataObject(consignment);

			CombineAssertions("Cached consignment properties do not match the dataObject's properties, it doesn't appear they've been cached", () =>
			{
				AssertEquals("GoodsValueCurrency:", dataObject.GoodsValueCurrency, cache.GetValue<Currency>(goodsValueCurrencyKey));
				AssertEquals("TotalWeightUnit:", dataObject.TotalWeightUnit, cache.GetValue<UnitOfWeight>(totalWeightUnitKey));
				AssertEquals("TotalVolumeUnit:", dataObject.TotalVolumeUnit, cache.GetValue<UnitOfVolume>(totalVolumeUnitKey));
				AssertEquals("ShipmentIncoTerm:", dataObject.ShipmentIncoTerm, cache.GetValue<UniversalIncoTerm>(incoTermKey));

				var dataObjectConsigneeCountry = dataObject.OrganizationAddressCollection.First(o => o.AddressType == (ZString?)nameof(DocAddressType.ConsigneeDocumentaryAddress)).Country;
				var dataObjectConsignorCountry = dataObject.OrganizationAddressCollection.First(o => o.AddressType == (ZString?)nameof(DocAddressType.ConsignorDocumentaryAddress)).Country;

				AssertEquals("Consignee country:", dataObjectConsigneeCountry, cache.GetValue<Country>(consigneeCountryKey));
				AssertEquals("Consignor country:", dataObjectConsignorCountry, cache.GetValue<Country>(consignorCountryKey));
			});
		}

		public void TestGetDataObject_IfCacheExistsUseCache()
		{
			var bookingHeader = new HVLVBookingHeaderDataObjectWriterTest().PopulateAndGetBookingHeader();
			var consignment = (HVLVConsignment)bookingHeader.Consignments.First();

			var cache = new CodeDataObjectCache();
			var goodsValueCurrencyKey = cache.BuildKey(consignment.Lookups.GoodsValueCurrencies.GetType().FullName, consignment.HVC_RX_NKGoodsValueCurrency);
			var totalWeightUnitKey = cache.BuildKey(nameof(UnitOfWeight), consignment.HVC_WeightUQ);
			var totalVolumeUnitKey = cache.BuildKey(nameof(UnitOfVolume), consignment.HVC_VolumeUQ);
			var incoTermKey = cache.BuildKey(consignment.Lookups.INCOTermsList.GetType().FullName, consignment.HVC_INCO);
			var consigneeCountryKey = cache.BuildKey(consignment.Lookups.ConsigneeCountryCodes.GetType().FullName, consignment.HVC_RN_NKConsigneeCountryCode);
			var consignorCountryKey = cache.BuildKey(consignment.Lookups.ShipperCountryCodes.GetType().FullName, consignment.HVC_RN_NKShipperCountryCode);

			var currencyCacheValue = cache.GetValue(goodsValueCurrencyKey, () => ListHelper.GetWithDescription<Currency>(CurrencyCodes.Belgium, consignment.Lookups.GoodsValueCurrencies));
			var weightUnitCacheValue = cache.GetValue(totalWeightUnitKey, () => ListHelper.GetWithDescription<UnitOfWeight>(Weight.Ounces, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight)));
			var volumeUnitCacheValue = cache.GetValue(totalVolumeUnitKey, () => ListHelper.GetWithDescription<UnitOfVolume>(Volume.CubicDecimetres, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Volume)));
			var incoTermCacheValue = cache.GetValue(incoTermKey, () => ListHelper.GetWithDescription<UniversalIncoTerm>(IncoTerms.DeliveredAtFrontier, consignment.Lookups.INCOTermsList));
			var consigneeCountryCacheValue = cache.GetValue(consigneeCountryKey, () => ListHelper.GetWithName<Country>(CountryCodes.Belgium, consignment.Lookups.ConsigneeCountryCodes));
			var consignorCountryCacheValue = cache.GetValue(consignorCountryKey, () => ListHelper.GetWithName<Country>(CountryCodes.Belgium, consignment.Lookups.ShipperCountryCodes));

			var dataObject = new HVLVConsignmentDataObjectWriterForCacheTest(new DataWritingManager(new ActionInfo(null, bookingHeader)), cache).GetDataObject(consignment);

			CombineAssertions("The writer should retrieve values from the cache we explicitly inserted previously, to verify the writer does use the cache.", () =>
			{
				AssertEquals("GoodsValueCurrency:", currencyCacheValue.Code, dataObject.GoodsValueCurrency.Code);
				AssertEquals("TotalWeightUnit:", weightUnitCacheValue.Code, dataObject.TotalWeightUnit.Code);
				AssertEquals("TotalVolumeUnit:", volumeUnitCacheValue.Code, dataObject.TotalVolumeUnit.Code);
				AssertEquals("Incoterm:", incoTermCacheValue.Code, dataObject.ShipmentIncoTerm.Code);

				var dataObjectConsigneeCountry = dataObject.OrganizationAddressCollection.First(o => o.AddressType == (ZString?)nameof(DocAddressType.ConsigneeDocumentaryAddress)).Country;
				var dataObjectConsignorCountry = dataObject.OrganizationAddressCollection.First(o => o.AddressType == (ZString?)nameof(DocAddressType.ConsignorDocumentaryAddress)).Country;

				AssertEquals("Consignee country:", consigneeCountryCacheValue.Code, dataObjectConsigneeCountry.Code);
				AssertEquals("Consignor country:", consignorCountryCacheValue.Code, dataObjectConsignorCountry.Code);
			});
		}

		public void TestPopulateCommercialInvoiceAndLines()
		{
			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			var item = consignment.Items.AddNew();
			item.Lines.AddNew();
			item.Lines.AddNew();

			var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager);

			var shipment = new Shipment();
			writer.WriteCommercialInvoiceAndLines(consignment, shipment);

			CombineAssertions("Invoice headers and lines are populated", () =>
			{
				AssertEquals("Should populated 1 invoice header", 1, shipment.CommercialInfo.CommercialInvoiceCollection.Count);
				AssertEquals("Should populated 2 invoice line", 2, shipment.CommercialInfo.CommercialInvoiceCollection[0].CommercialInvoiceLineCollection.Count);
			});
		}

		public void TestAirConsolWaybillNumberIsFormatted()
		{
			var consignment = CreateTestConsignmentForExport();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_MasterBillNum = "123-456";

			var shipment = consol.Shipments.AddNew();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager, HVLVConsignmentToDeclarationDataExportStrategy.Instance);
			var dataObject =  writer.GetDataObject(consignment);

			AssertEquals("Waybill number is formatted for air consol1", "123456", dataObject.WayBillNumber);
		}

		public void TestDataObjectCacheFactoryService_ForConsolDataObject()
		{
			var dataObjectCacheService = DataObjectCacheFactoryService.GetOrCreateNewInstance(Factory);

			var forwardingConsolDataContextManagerMock = new Mock<IDataContextManager>();
			ObjectFactory.Substitute("ForwardingConsolDataContextManager", forwardingConsolDataContextManagerMock.Object);

			var consol1 = Factory.New<ForwardingConsol>();
			WriteDataObjectWithConsol(consol1);

			CombineAssertions("First time writing", () =>
			{
				forwardingConsolDataContextManagerMock.Verify(x => x.Init(consol1), Times.Once());
				AssertEquals(1, dataObjectCacheService.cache.Count);
				Assert("Key", dataObjectCacheService.cache.ContainsKey(dataObjectCacheService.BuildKey(JobConsolSchema.Constants.TableName, consol1.PK.ToString())));
			});

			forwardingConsolDataContextManagerMock.Reset();
			WriteDataObjectWithConsol(consol1);

			CombineAssertions("Second time writing, with same consol", () =>
			{
				forwardingConsolDataContextManagerMock.Verify(x => x.Init(consol1), Times.Never(), "The consol data object should be cached from the previous write, so this method should not be called.");
				AssertEquals(1, dataObjectCacheService.cache.Count);
				Assert("Key", dataObjectCacheService.cache.ContainsKey(dataObjectCacheService.BuildKey(JobConsolSchema.Constants.TableName, consol1.PK.ToString())));
			});

			forwardingConsolDataContextManagerMock.Reset();
			var consol2 = Factory.New<ForwardingConsol>();
			WriteDataObjectWithConsol(consol2);

			CombineAssertions("Third time writing, with different consol", () =>
			{
				forwardingConsolDataContextManagerMock.Verify(x => x.Init(consol2), Times.Once(), "Consol2 data object was not previously cached, so this should be called.");
				AssertEquals(2, dataObjectCacheService.cache.Count);
				Assert("Key for consol1", dataObjectCacheService.cache.ContainsKey(dataObjectCacheService.BuildKey(JobConsolSchema.Constants.TableName, consol1.PK.ToString())));
				Assert("Key for consol2", dataObjectCacheService.cache.ContainsKey(dataObjectCacheService.BuildKey(JobConsolSchema.Constants.TableName, consol2.PK.ToString())));
			});

			static void WriteDataObjectWithConsol(ForwardingConsol consol)
			{
				var shipment = consol.Shipments.AddNew();
				var consignment = shipment.GetOrCreateHVLVConsignmentHeader().Consignments.AddNew();
				consignment.Items.AddNew();

				var writeManager = new DataWritingManager(new ActionInfo(null, consignment));
				var writer = new HVLVConsignmentDataObjectWriter(writeManager, HVLVConsignmentToDeclarationDataExportStrategy.Instance);
				writer.GetDataObject(consignment);
			}
		}

		Shipment GetConsignmentDataObject(HVLVConsignment consignment = null, DataWritingManager writeManager = null)
		{
			consignment ??= CreateTestConsignmentForExport();
			writeManager ??=  new DataWritingManager(new ActionInfo(null, consignment));
			var writer = new HVLVConsignmentDataObjectWriter(writeManager);
			return writer.GetDataObject(consignment);
		}

		HVLVConsignment CreateTestConsignmentForExport()
		{
			var bookingHeader = Factory.NewWithValidTestData<HVLVBookingHeader>();
			var consignment = bookingHeader.Consignments.AddNew();

			// Consignment Details
			consignment.HVC_WaybillNumber = "HVC001";
			consignment.HVC_ShipperReference = "LINUS";
			consignment.HVC_GoodsValue = 10;
			consignment.HVC_InsuranceValue = 20;
			consignment.HVC_TransportValue = 30;
			consignment.HVC_RX_NKGoodsValueCurrency = "USD";
			consignment.HVC_GoodsDescription = "RTX 5090 Ti";
			consignment.HVC_IsHazardous = true;
			consignment.HVC_IsSignatureRequired = false;
			consignment.HVC_AuthorityToLeave = true;
			consignment.HVC_IsTracked = false;
			consignment.HVC_IsTaxPrePaid = true;
			consignment.HVC_VendorIdentifier = ZString.Empty;

			// Notes
			consignment.Notes.AddNew(false, "LTT", "LTTSTORE");

			// Instructions
			consignment.HVC_ConsigneeInstructions = "HANDLE WITH CARE";

			// Customs References
			var importDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			var exportDeclaration = Factory.NewWithValidTestData<BaseJobDeclaration>();
			importDeclaration.JE_DeclarationReference = "DECLARATION007";
			exportDeclaration.JE_DeclarationReference = "DECLARATION008";
			consignment.HVC_JE_ImportDeclaration = importDeclaration.PK;
			consignment.HVC_JE_ExportDeclaration = exportDeclaration.PK;

			// Additional References
			var cusEntryNum = consignment.CustomsReferenceNumbers.AddNew();
			cusEntryNum.CE_EntryNum = "ISF12345";
			cusEntryNum.CE_EntryType = "ISF";

			// Last Mile Carrier Details
			var lastMileDelivery = Factory.NewWithValidTestData<OrgHeader>();
			lastMileDelivery.OH_IsShippingProvider = true;
			lastMileDelivery.OH_IsLocalTransport = true;
			lastMileDelivery.MainAddress.Address1 = "123 Linus Street";

			var carrierAccount = lastMileDelivery.CarrierAccounts.AddNew();
			carrierAccount.OAN_AccountNumber = "123456";
			carrierAccount.OAN_DepotID = "Test Depot ID";

			var miscServ = Factory.NewWithValidTestData<OrgMiscServ>();
			lastMileDelivery.MiscServ = miscServ;

			var serviceLevel = miscServ.CarrierServiceLevels.AddNew();
			serviceLevel.PL_Code = "EXP";
			serviceLevel.PL_CarrierServiceCode = "999";
			serviceLevel.PL_CarrierServiceLevelDescription = "99.9% faster service";

			consignment.HVC_OH_LastMileCarrier = lastMileDelivery.PK;
			consignment.HVC_CarrierAccountNumber = carrierAccount.OAN_AccountNumber;
			consignment.HVC_PL_NKLastMileCarrierServiceLevel = "EXP";

			// Customs Clearance Status
			HVLVCustomsStatusTestHelper.AddRefCusCodeList(Factory, "REJ", "COVID-19 go away!", "HLD", RefCusCodeListTypes.Codes.ExportCustomsStatus);
			consignment.HVC_ExportCustomsClearanceStatus = "REJ";

			// Consignee Address
			var consigneeAddress = Factory.NewWithValidTestData<OrgAddress>();
			consigneeAddress.Address1 = "Test Address 1";
			consigneeAddress.Address2 = "Test Address 2";
			consigneeAddress.City = "Test City";
			consigneeAddress.State = "Test State";
			consigneeAddress.Postcode = "54321";
			consigneeAddress.CompanyName = "Test Name";

			consignment.HVC_OA_ConsigneeAddress = consigneeAddress.PK;
			consignment.HVC_ConsigneeContact = "Consignee Contact";
			consignment.HVC_ConsigneePhone = "222333";
			consignment.HVC_ConsigneeMobile = "13722223333";
			consignment.HVC_ConsigneeFax = "12";
			consignment.HVC_ConsigneeEmail = "consignee@test.org";

			// Shipper Address
			var shipperAddress = Factory.NewWithValidTestData<OrgAddress>();
			shipperAddress.Address1 = "Test Address 1";
			shipperAddress.Address2 = "Test Address 2";
			shipperAddress.City = "Test City";
			shipperAddress.State = "Test State";
			shipperAddress.Postcode = "54321";
			shipperAddress.CompanyName = "Test Name";

			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			consignment.HVC_ShipperContact = "Shipper Contact";
			consignment.HVC_ShipperPhone = "222333";
			consignment.HVC_ShipperMobile = "13722223333";
			consignment.HVC_ShipperFax = "34";
			consignment.HVC_ShipperEmail = "shipper@test.org";

			// Return Address
			var returnAddress = Factory.NewWithValidTestData<OrgAddress>();
			returnAddress.Address1 = "Test Address 1";
			returnAddress.Address2 = "Test Address 2";
			returnAddress.City = "Test City";
			returnAddress.State = "Test State";
			returnAddress.Postcode = "54321";
			returnAddress.CompanyName = "Test Name";

			consignment.HVC_OA_ReturnLocation = returnAddress.PK;
			consignment.HVC_ReturnContact = "Return Contact";
			consignment.HVC_ReturnPhone = "222333";
			consignment.HVC_ReturnMobile = "13722223333";
			consignment.HVC_ReturnFax = "56";
			consignment.HVC_ReturnEmail = "return@test.org";

			// Item
			var item = consignment.Items.AddNew();
			item.HVI_ShipperReference = "LTTSTOREDOTCOM";
			item.HVI_F3_NKPackType = "BOX";
			item.HVI_ManifestedWeight = 10;
			item.HVI_ActualWeight = 10;
			item.HVI_ManifestedVolume = 0.5;
			item.HVI_ActualVolume = 0.5;
			item.HVI_Status = HVLVItemStatus.Codes.Delivered;
			item.HVI_IsDamaged = true;
			item.HVI_IsPillaged = false;

			// Item Line
			var itemLine = item.Lines.AddNew();
			itemLine.HVS_Quantity = 1;
			itemLine.HVS_OriginTariff = "123123";
			itemLine.HVS_RN_NKOriginCountryCode = "FR";
			itemLine.HVS_DestinationTariff = "321321";
			itemLine.HVS_GrossWeight = 1.2;
			itemLine.HVS_NetWeight = 3.4;
			itemLine.HVS_WeightUnit = "KG";
			itemLine.HVS_GoodsDescription = "Mouse";
			itemLine.HVS_IntrinsicValue = 21;

			Factory.Save();

			return consignment;
		}

		sealed class HVLVConsignmentDataObjectWriterForCacheTest : HVLVConsignmentDataObjectWriter
		{
			public HVLVConsignmentDataObjectWriterForCacheTest(IDataWritingManager manager, CodeDataObjectCache cache)
				: base(manager)
			{
				codeDataObjectCache = cache;
			}
		}
	}
}
