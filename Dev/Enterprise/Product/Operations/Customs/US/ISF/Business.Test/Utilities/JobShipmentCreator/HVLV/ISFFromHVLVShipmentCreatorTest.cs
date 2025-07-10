using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.ISF.DataRegistry.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EventReference.Constants;
using static Enterprise.Registry.Business.CustomsReferenceNumberType;

namespace Enterprise.Customs.US.ISF.Business.Testing
{
	[TestedType(typeof(ISFFromHVLVShipmentCreator))]
	public class ISFFromHVLVShipmentCreatorTest : ISFFromShipmentCreatorTest
	{
		public void TestISFHeaderIsNotInDataBaseWhenRunAfterOnSavingService()
		{
			var shipment = CreateHVLVShipment();

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			creator.Create(newFactory);

			newFactory.ServiceContainer.AddAfterOnSavingService(new AfterOnSavingBOProcessingServiceForTest(() =>
			{
				var isfHeader = newFactory.Load<CusISFHeader>(new ZQuery()).FirstOrDefault();
				Assert(!isfHeader.IsInDatabase);
			}));

			newFactory.Save();
		}

		public void TestCreateISFFromHVLVshipment()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = address.PK;

			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item.PK;
			itemLine1.HVS_DestinationTariff = "654321";
			itemLine1.HVS_RN_NKOriginCountryCode = "US";
			itemLine1.HVS_Quantity = 1;

			Factory.Save();

			var creator = new ISFFromHVLVShipmentCreator(shipment);
			var header = creator.Create(Factory);
			AssertEquals("Only HVLV ISF jobs can have this field", shipment.PK, header.BF_JS_Shipment);
		}

		public void TestCreateISFHeader_ISFHeaderOrderByJobReference()
		{
			var shipment = CreateHVLVShipment();

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			creator.Create(newFactory);

			newFactory.Save();

			var jobNumbers = creator
				.createdNewHeaders
				.Cast<CusISFHeader>()
				.OrderBy(header => header.BF_JobReference)
				.Select(x => x.BF_JobReference);

			AssertContainsExactElementsInExactOrder(new[] { "ISF0000001", "ISF0000002" }, jobNumbers);
		}

		public void TestCreateISFHeader_PopulatesConsignmentCustomsReferenceNumber()
		{
			var shipment = CreateHVLVShipment();

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			creator.Create(newFactory);

			newFactory.Save();

			var mappings = creator.consignmentToHeaderMappings;
			foreach (var consignment in mappings.Keys)
			{
				var references = consignment.CustomsReferenceNumbers.GetAllReferenceNumbersByType(CustomsAdditionalReferenceNumbersCodes.ImportSecurityFilingReference);
				var reference = references.Single();
				AssertEquals("The reference for the Customs reference number is the same as the ISF job number", mappings[consignment].BF_JobReference, reference);
			}
		}

		public void TestCreateISFHeader_PopulateISFHeaderJobReference_NumberFountainVisitedOnce()
		{
			var shipment = CreateHVLVShipment();

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			creator.Create(newFactory);

			NumberFountainProxy.NumberOfFountainCommands_ForTest.Value = 0;
			newFactory.Save();

			AssertEquals("Number fountain visited only once (GetNexts).", 1, NumberFountainProxy.NumberOfFountainCommands_ForTest.Value);
		}

		public void TestCreate_CreateOneHeaderPerConsignment()
		{
			var shipment = CreateHVLVShipment();

			for (var i = 0; i < 2; i++)
			{
				var consignment = Factory.New<IHVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			}

			Factory.Save();

			var loadAllHeaderQuery = new ZQuery(CusISFHeaderSchema.BF_ShipmentType, ShipmentTypeList.Codes.Informal);
			var newFactory = new BusinessObjectFactory();

			AssertEquals("pre condition", 0, newFactory.Load<CusISFHeader>(loadAllHeaderQuery).Length);

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				creator.Create(newFactory);
			}

			newFactory.Save();
			AssertEquals("2 headers should be created", 2, newFactory.Load<CusISFHeader>(loadAllHeaderQuery).Length);
		}

		public void TestFactoryValidationIsSuspendedDuringCreation()
		{
			var shipment = CreateHVLVShipment();
			var creator = new ISFFromHVLVShipmentCreator(shipment);
			var isValidationSuspended = Factory.IsValidationSuspended;
			AssertEquals("pre condition", false, isValidationSuspended);

			creator.ExtraOperationOnHeaderForTest = ((header) =>
			{
				isValidationSuspended = Factory.IsValidationSuspended;
			});

			var isfHeader = creator.CreateHeaders();
			AssertEquals("Validation should be suspended during creation", true, isValidationSuspended);
		}

		public void TestCreateSellingPartyFromHVLVConsignments_WhereConsignmentShipperIsOrg()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Org1";
			org.OH_FullName = "FASTEST ORGANIZATION";
			org.Contacts.AddNew();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = "address line 1";
			orgAddress.OA_Address2 = "address line 2";

			var consignmentShipperIsOrg = Factory.New<IHVLVConsignment>();
			consignmentShipperIsOrg.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentShipperIsOrg.HVC_OA_ShipperAddress = orgAddress.PK;

			Factory.Save();

			var creator = new ISFFromHVLVShipmentCreator(shipment);
			var isfHeaders = creator.CreateHeaders();

			AssertEquals(1, isfHeaders.Count);

			foreach (CusISFHeader isfHeader in isfHeaders)
			{
				var sellingParty = isfHeader.SellingParty;
				AssertEquals(consignmentShipperIsOrg.HVC_OA_ShipperAddress, sellingParty.E2_OA_Address);
			}
		}

		public void TestCreateSellingPartyFromHVLVConsignments_WhereConsignmentShipperIsNotOrg()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignmentShipperIsNotOrg = Factory.New<IHVLVConsignment>();
			consignmentShipperIsNotOrg.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentShipperIsNotOrg.HVC_ShipperAddress1 = "Consignment2 Address1";
			consignmentShipperIsNotOrg.HVC_ShipperAddress2 = "Consignment2 Address2";
			consignmentShipperIsNotOrg.HVC_ShipperEmail = "Consignment2 Email";
			consignmentShipperIsNotOrg.HVC_ShipperContact = "Consignment2 Contact";

			Factory.Save();

			var creator = new ISFFromHVLVShipmentCreator(shipment);
			var isfHeaders = creator.CreateHeaders();

			AssertEquals(1, isfHeaders.Count);

			foreach (CusISFHeader isfHeader in isfHeaders)
			{
				var sellingParty = isfHeader.SellingParty;
				AssertISFDocAddressEqualsToShipperOnConsignment(consignmentShipperIsNotOrg, sellingParty, true);
			}
		}

		public void TestCreateBuyingPartyFromHVLVConsignments_WhereConsignmentConsigneeIsOrg()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var org = Factory.New<OrgHeader>();
			org.OH_Code = "Org1";
			org.OH_FullName = "FASTEST ORGANIZATION";
			org.Contacts.AddNew();
			var orgAddress = org.Addresses.AddNew();
			orgAddress.OA_Address1 = "address line 1";
			orgAddress.OA_Address2 = "address line 2";

			var consignmentConsigneeIsOrg = Factory.New<IHVLVConsignment>();
			consignmentConsigneeIsOrg.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentConsigneeIsOrg.HVC_OA_ConsigneeAddress = orgAddress.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeaders = creator.CreateHeaders();

				AssertEquals(1, isfHeaders.Count);
				foreach (CusISFHeader isfHeader in isfHeaders)
				{
					var buyingParty = isfHeader.BuyingParty;
					AssertEquals(consignmentConsigneeIsOrg.HVC_OA_ConsigneeAddress, buyingParty.E2_OA_Address);
				}
			}
		}

		public void TestCreateBuyingPartyFromHVLVConsignments_EINNumberIsNotPopulatedIfConsignmentConsigneeIsNotOrg()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var shipmentConsignee = Factory.NewWithValidTestData<OrgHeader>();
			var country = Factory.Load<RefCountry>(Constants.CountryGuids.UnitedStates);
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = shipmentConsignee.PK;
			shipmentConsignee.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "12-3456789XY", country);

			var consignmentConsigneeIsNotOrg = Factory.New<IHVLVConsignment>();
			consignmentConsigneeIsNotOrg.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentConsigneeIsNotOrg.HVC_ConsigneeAddress1 = "Consignment2 Address1";
			consignmentConsigneeIsNotOrg.HVC_ConsigneeAddress2 = "Consignment2 Address2";
			consignmentConsigneeIsNotOrg.HVC_ConsigneeEmail = "Consignment2 Email";
			consignmentConsigneeIsNotOrg.HVC_ConsigneeContact = "Consignment2 Contact";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.CreateHeaders().Single() as CusISFHeader;
				var buyingParty = isfHeader.BuyingParty;

				CombineAssertions(() =>
				{
					AssertEquals(ZString.Empty, buyingParty.E2_GovRegNum);
					AssertEquals(ZString.Empty, buyingParty.E2_GovRegNumType);
				});
			}
		}

		public void TestCreateBuyingPartyFromHVLVConsignments_WhereConsignmentConsigneeIsNotOrg()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignmentConsigneeIsNotOrg = Factory.New<IHVLVConsignment>();
			consignmentConsigneeIsNotOrg.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignmentConsigneeIsNotOrg.HVC_ConsigneeAddress1 = "Consignment2 Address1";
			consignmentConsigneeIsNotOrg.HVC_ConsigneeAddress2 = "Consignment2 Address2";
			consignmentConsigneeIsNotOrg.HVC_ConsigneeEmail = "Consignment2 Email";
			consignmentConsigneeIsNotOrg.HVC_ConsigneeContact = "Consignment2 Contact";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeaders = creator.CreateHeaders();

				AssertEquals(1, isfHeaders.Count);
				foreach (CusISFHeader isfHeader in isfHeaders)
				{
					var buyingParty = isfHeader.BuyingParty;
					AssertISFDocAddressEqualsToShipperOnConsignment(consignmentConsigneeIsNotOrg, buyingParty, false);
				}
			}
		}

		void AssertISFDocAddressEqualsToShipperOnConsignment(IHVLVConsignment consignment, ISFDocAddress docAddress, bool isShipper)
		{
			if (isShipper)
			{
				AssertEquals(consignment.HVC_ShipperAddress1.ToUpper(), docAddress.E2_Address1);
				AssertEquals(consignment.HVC_ShipperAddress2.ToUpper(), docAddress.E2_Address2);
				AssertEquals(consignment.HVC_ShipperEmail.ToUpper(), docAddress.E2_Email);
				AssertEquals(consignment.HVC_ShipperContact.ToUpper(), docAddress.E2_Contact);
				AssertEquals(consignment.HVC_ShipperName.ToUpper(), docAddress.E2_CompanyName);
				AssertEquals(consignment.HVC_ShipperCity.ToUpper(), docAddress.E2_City);
				AssertEquals(consignment.HVC_ShipperState.ToUpper(), docAddress.E2_State);
				AssertEquals(consignment.HVC_ShipperPostcode.ToUpper(), docAddress.E2_Postcode);
				AssertEquals(consignment.HVC_RN_NKShipperCountryCode, docAddress.E2_RN_NKCountryCode);
				AssertEquals(consignment.HVC_ShipperMobile.ToUpper(), docAddress.E2_Phone);
				AssertEquals(consignment.HVC_ShipperFax.ToUpper(), docAddress.E2_Fax);
				AssertEquals(consignment.HVC_ShipperEmail.ToUpper(), docAddress.E2_Email);
			}
			else
			{
				AssertEquals(consignment.HVC_ConsigneeAddress1.ToUpper(), docAddress.E2_Address1);
				AssertEquals(consignment.HVC_ConsigneeAddress2.ToUpper(), docAddress.E2_Address2);
				AssertEquals(consignment.HVC_ConsigneeEmail.ToUpper(), docAddress.E2_Email);
				AssertEquals(consignment.HVC_ConsigneeContact.ToUpper(), docAddress.E2_Contact);
				AssertEquals(consignment.HVC_ConsigneeName.ToUpper(), docAddress.E2_CompanyName);
				AssertEquals(consignment.HVC_ConsigneeCity.ToUpper(), docAddress.E2_City);
				AssertEquals(consignment.HVC_ConsigneeState.ToUpper(), docAddress.E2_State);
				AssertEquals(consignment.HVC_ConsigneePostcode.ToUpper(), docAddress.E2_Postcode);
				AssertEquals(consignment.HVC_RN_NKConsigneeCountryCode, docAddress.E2_RN_NKCountryCode);
				AssertEquals(consignment.HVC_ConsigneeMobile.ToUpper(), docAddress.E2_Phone);
				AssertEquals(consignment.HVC_ConsigneeFax.ToUpper(), docAddress.E2_Fax);
				AssertEquals(consignment.HVC_ConsigneeEmail.ToUpper(), docAddress.E2_Email);
			}
		}

		public void TestGivenHVLVConsignment_WhenCreatingISFLine_ThenISFLineManufacturerJobDocAddressParentIsTheISFHeaderSellingParty()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = address.PK;

			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item.PK;
			itemLine1.HVS_DestinationTariff = "654321";
			itemLine1.HVS_RN_NKOriginCountryCode = "US";
			itemLine1.HVS_Quantity = 1;

			Factory.Save();

			var creator = new ISFFromHVLVShipmentCreator(shipment);
			var header = creator.Create(Factory);

			AssertEquals(1, header.Lines.Count);

			var line = header.Lines.FirstOrDefault();

			AssertEquals("Expected the consignment's shipper address to map to the ISF line's manufacturer address", consignment.HVC_OA_ShipperAddress, line.ManufacturerDocAddress.E2_OA_Address);
			AssertEquals("Expected ISF line's manufacturer address type to be MAN", AutoDocAddressTypes.Codes.Manufacturer, line.ManufacturerDocAddress.E2_AddressType);
		}

		public void TestImportLinesFromShipment_WhenShipperIsNotOrganization_LineManufacturerDocAddressPopulatedFromShipperAddressOverride()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ShipperAddress1 = "Consingment Address1";
			consignment.HVC_ShipperAddress2 = "Consingment Address2";
			consignment.HVC_ShipperEmail = "Consingment Email";
			consignment.HVC_ShipperContact = "Consingment Contact";
			consignment.HVC_ShipperPostcode = "abc123";

			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var part1 = Factory.New<MasterFiles.Business.OrgSupplierPart>();
			part1.OP_PartNum = "P001";
			part1.OP_Desc = "DESC";

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item.PK;
			itemLine1.HVS_ProductCode = part1.OP_PartNum;
			itemLine1.HVS_DestinationTariff = "654321";
			itemLine1.HVS_RN_NKOriginCountryCode = "US";
			itemLine1.HVS_Quantity = 1;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals(1, isfHeader.Lines.Count);
			}

			var line = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_TextProductCode, itemLine1.HVS_ProductCode));

			AssertEquals(consignment.HVC_ShipperAddress1.ToUpper(), line.ManufacturerDocAddress.E2_Address1);
			AssertEquals(consignment.HVC_ShipperAddress2.ToUpper(), line.ManufacturerDocAddress.E2_Address2);
			AssertEquals(consignment.HVC_ShipperEmail.ToUpper(), line.ManufacturerDocAddress.E2_Email);
			AssertEquals(consignment.HVC_ShipperContact.ToUpper(), line.ManufacturerDocAddress.E2_Contact);
			AssertEquals(consignment.HVC_ShipperName.ToUpper(), line.ManufacturerDocAddress.E2_CompanyName);
			AssertEquals(consignment.HVC_ShipperCity.ToUpper(), line.ManufacturerDocAddress.E2_City);
			AssertEquals(consignment.HVC_ShipperState.ToUpper(), line.ManufacturerDocAddress.E2_State);
			AssertEquals(consignment.HVC_ShipperPostcode.ToUpper(), line.ManufacturerDocAddress.E2_Postcode);
			AssertEquals(consignment.HVC_RN_NKShipperCountryCode, line.ManufacturerDocAddress.E2_RN_NKCountryCode);
			AssertEquals(consignment.HVC_ShipperMobile.ToUpper(), line.ManufacturerDocAddress.E2_Phone);
			AssertEquals(consignment.HVC_ShipperFax.ToUpper(), line.ManufacturerDocAddress.E2_Fax);
			AssertEquals(consignment.HVC_ShipperEmail.ToUpper(), line.ManufacturerDocAddress.E2_Email);
		}

		public void TestGivenItemLinesWithDuplicateHSCodes_WhenCreateISF_ThenRemoveISFLinesWithDuplicateHSCodes()
		{
			var supplier = Factory.NewWithValidTestData<OrgHeader>();

			var shipperAddress = supplier.Addresses.AddNew();
			shipperAddress.OA_Address1 = "Shipper Address";

			var shipment = CreateHVLVShipment();
			shipment.ConsignorPK = supplier.PK;
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item1.PK;
			itemLine1.HVS_DestinationTariff = "112233";
			itemLine1.HVS_RN_NKOriginCountryCode = Constants.CountryCodes.UnitedStates;
			itemLine1.HVS_Quantity = 1;

			var itemLine2 = Factory.New<IHVLVItemLine>();
			itemLine2.HVS_HVI_HVLVItem = item1.PK;
			itemLine2.HVS_DestinationTariff = "112233";
			itemLine2.HVS_RN_NKOriginCountryCode = Constants.CountryCodes.UnitedStates;
			itemLine2.HVS_Quantity = 1;

			var itemLine3 = Factory.New<IHVLVItemLine>();
			itemLine3.HVS_HVI_HVLVItem = item1.PK;
			itemLine3.HVS_DestinationTariff = "778899";
			itemLine3.HVS_RN_NKOriginCountryCode = Constants.CountryCodes.UnitedStates;
			itemLine3.HVS_Quantity = 1;

			var itemLine4 = Factory.New<IHVLVItemLine>();
			itemLine4.HVS_HVI_HVLVItem = item2.PK;
			itemLine4.HVS_DestinationTariff = "112233";
			itemLine4.HVS_RN_NKOriginCountryCode = Constants.CountryCodes.UnitedStates;
			itemLine4.HVS_Quantity = 1;

			var itemLine5 = Factory.New<IHVLVItemLine>();
			itemLine5.HVS_HVI_HVLVItem = item2.PK;
			itemLine5.HVS_DestinationTariff = "112233";
			itemLine5.HVS_RN_NKOriginCountryCode = Constants.CountryCodes.Rwanda;
			itemLine5.HVS_Quantity = 1;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals("Expected three ISF lines grouped by destination tariff, manufacturer address PK, origin country code", 3, isfHeader.Lines.Count);
				AssertEquals(1, isfHeader.ManufacturerAddresses.Count);

				var isfLines1 = isfHeader.Lines.Cast<CusISFLine>().Where(l => l.BL_HarmonisedNum == "112233" && l.BL_RN_NKGoodsOrigin == Constants.CountryCodes.UnitedStates).ToList();
				var isfLines2 = isfHeader.Lines.Cast<CusISFLine>().Where(l => l.BL_HarmonisedNum == "778899" && l.BL_RN_NKGoodsOrigin == Constants.CountryCodes.UnitedStates).ToList();
				var isfLines3 = isfHeader.Lines.Cast<CusISFLine>().Where(l => l.BL_HarmonisedNum == "112233" && l.BL_RN_NKGoodsOrigin == Constants.CountryCodes.Rwanda).ToList();

				AssertEquals("Expected only one ISFLine with 112233 HS Code with destination to United States, duplicate ISF lines based on if they have the same goods origin, manufacturer and HS code (112233).", 1, isfLines1.Count);
				AssertEquals("Expected only one ISFLine with 778899 HS Code with destination to United States", 1, isfLines2.Count);
				AssertEquals("Expected only one ISFLine with 112233 HS Code with destination to Rwanda", 1, isfLines3.Count);

				var isfLine1 = isfLines1.FirstOrDefault();
				var isfLine2 = isfLines2.FirstOrDefault();
				var isfLine3 = isfLines3.FirstOrDefault();

				AssertLineInISFHeaderEqualsToItemLineOnShipment(itemLine1, isfLine1);
				AssertEquals("Expected consignment shipper address to map to a ISFline1's manufacturer JobDocAddress", consignment.HVC_OA_ShipperAddress, isfLine1.ManufacturerDocAddress.E2_OA_Address);

				AssertLineInISFHeaderEqualsToItemLineOnShipment(itemLine3, isfLine2);
				AssertEquals("Expected consignment shipper address to map to a ISFline2's manufacturer JobDocAddress", consignment.HVC_OA_ShipperAddress, isfLine2.ManufacturerDocAddress.E2_OA_Address);

				AssertLineInISFHeaderEqualsToItemLineOnShipment(itemLine5, isfLine3);
				AssertEquals("Expected consignment shipper address to map to a ISFline3's manufacturer JobDocAddress", consignment.HVC_OA_ShipperAddress, isfLine3.ManufacturerDocAddress.E2_OA_Address);
			}
		}

		public void TestImportLinesFromShipment_WhenShipperIsOrganization_ThenLineManufacturerDocAddressPopulatedFromShipperAddress()
		{
			var importer = Factory.NewWithValidTestData<OrgHeader>();
			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.Contacts.AddNew();
			var shipperAddress = supplier.Addresses.AddNew();
			shipperAddress.OA_Address1 = "address line 1";
			shipperAddress.OA_Address2 = "address line 2";

			var part1 = Factory.New<MasterFiles.Business.OrgSupplierPart>();
			part1.RelatedOrganisations.AddOwner(importer);
			part1.RelatedOrganisations.AddSupplier(supplier);
			part1.OP_PartNum = "P001";
			part1.OP_Desc = "DESC";
			var part2 = Factory.New<MasterFiles.Business.OrgSupplierPart>();
			part2.RelatedOrganisations.AddOwner(importer);
			part2.RelatedOrganisations.AddSupplier(supplier);
			part2.OP_PartNum = "P002";
			part2.OP_Desc = "DESC";
			var part3 = Factory.New<MasterFiles.Business.OrgSupplierPart>();
			part3.RelatedOrganisations.AddOwner(importer);
			part3.RelatedOrganisations.AddSupplier(supplier);
			part3.OP_PartNum = "P003";
			part3.OP_Desc = "DESC";

			Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			shipment.ConsignorPK = supplier.PK;
			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_OA_ShipperAddress = shipperAddress.PK;
			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item.PK;
			itemLine1.HVS_ProductCode = part1.OP_PartNum;
			itemLine1.HVS_DestinationTariff = "654321";
			itemLine1.HVS_RN_NKOriginCountryCode = "US";
			itemLine1.HVS_Quantity = 1;

			var itemLine2 = Factory.New<IHVLVItemLine>();
			itemLine2.HVS_HVI_HVLVItem = item.PK;
			itemLine2.HVS_ProductCode = part2.OP_PartNum;
			itemLine2.HVS_DestinationTariff = "7654321";
			itemLine2.HVS_RN_NKOriginCountryCode = "AU";
			itemLine2.HVS_Quantity = 1;

			var itemLine3 = Factory.New<IHVLVItemLine>();
			itemLine3.HVS_HVI_HVLVItem = item.PK;
			itemLine3.HVS_ProductCode = part3.OP_PartNum;
			itemLine3.HVS_DestinationTariff = "87654321";
			itemLine3.HVS_RN_NKOriginCountryCode = "TU";
			itemLine3.HVS_Quantity = 1;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals(3, isfHeader.Lines.Count);
				AssertEquals(1, isfHeader.ManufacturerAddresses.Count);

				var line = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_TextProductCode, itemLine1.HVS_ProductCode));
				AssertLineInISFHeaderEqualsToItemLineOnShipment(itemLine1, line);
				AssertEquals("Expected consignment's shipper address to map to a ISFline1's manufacturer JobDocAddress", consignment.HVC_OA_ShipperAddress, line.ManufacturerDocAddress.E2_OA_Address);

				line = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_TextProductCode, itemLine2.HVS_ProductCode));
				AssertLineInISFHeaderEqualsToItemLineOnShipment(itemLine2, line);
				AssertEquals("Expected consignment's shipper address to map to a ISFline2's manufacturer JobDocAddress", consignment.HVC_OA_ShipperAddress, line.ManufacturerDocAddress.E2_OA_Address);

				line = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_TextProductCode, itemLine3.HVS_ProductCode));
				AssertLineInISFHeaderEqualsToItemLineOnShipment(itemLine3, line);
				AssertEquals("Expected consignment's shipper address to map to a ISFline3's manufacturer JobDocAddress", consignment.HVC_OA_ShipperAddress, line.ManufacturerDocAddress.E2_OA_Address);
			}
		}

		void AssertLineInISFHeaderEqualsToItemLineOnShipment(IHVLVItemLine itemLine, CusISFLine line)
		{
			CombineAssertions(() =>
			{
				AssertEquals(itemLine.HVS_DestinationTariff, line.BL_HarmonisedNum);
				AssertEquals(itemLine.HVS_RN_NKOriginCountryCode, line.BL_RN_NKGoodsOrigin);
			});
		}

		public void TestImportLinesFromShipment_GetsNoOfHTSDigits_FromRegistry()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var part1 = Factory.New<MasterFiles.Business.OrgSupplierPart>();
			part1.OP_PartNum = "P001";
			part1.OP_Desc = "DESC";

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item.PK;
			itemLine1.HVS_ProductCode = part1.OP_PartNum;
			itemLine1.HVS_DestinationTariff = "1234567890";
			itemLine1.HVS_RN_NKOriginCountryCode = "US";
			itemLine1.HVS_Quantity = 1;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals(1, isfHeader.Lines.Count);
			}

			var line1 = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_TextProductCode, itemLine1.HVS_ProductCode));

			AssertEquals("Default number of HTS digits is 10", "1234567890", line1.BL_HarmonisedNum);

			using (ISFRegistry.Instance.ImporterSecurityFilingNoOfHTSDigits.SetTemporaryValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, NumberOfHarmonizedDigitsList.Codes.Eight))
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals(1, isfHeader.Lines.Count);

				var line2 = Factory.Load<CusISFLine>(new ZQuery(CusISFLineSchema.BL_TextProductCode, itemLine1.HVS_ProductCode))[1];

				AssertEquals("Number of HTS digits is 8", "12345678", line2.BL_HarmonisedNum);
			}
		}

		public void TestImportLinesFromShipment_SetsBL_BB_Bill()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "waybill001";

			var consignment2 = Factory.New<IHVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "waybill002";

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item1.PK;
			itemLine1.HVS_DestinationTariff = "654321";
			itemLine1.HVS_RN_NKOriginCountryCode = "US";
			itemLine1.HVS_Quantity = 1;

			var itemLine2 = Factory.New<IHVLVItemLine>();
			itemLine2.HVS_HVI_HVLVItem = item1.PK;
			itemLine2.HVS_DestinationTariff = "7654321";
			itemLine2.HVS_RN_NKOriginCountryCode = "AU";
			itemLine2.HVS_Quantity = 1;

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment2.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine3 = Factory.New<IHVLVItemLine>();
			itemLine3.HVS_HVI_HVLVItem = item2.PK;
			itemLine3.HVS_DestinationTariff = "1234567890";
			itemLine3.HVS_RN_NKOriginCountryCode = "US";
			itemLine3.HVS_Quantity = 1;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				var bill1 = Factory.LoadTop1<CusISFBill>(new ZQuery(CusISFBillSchema.BB_BillNum, consignment1.HVC_WaybillNumber));
				var bill2 = Factory.LoadTop1<CusISFBill>(new ZQuery(CusISFBillSchema.BB_BillNum, consignment2.HVC_WaybillNumber));

				var line1 = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_HarmonisedNum, itemLine1.HVS_DestinationTariff));
				var line2 = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_HarmonisedNum, itemLine2.HVS_DestinationTariff));
				var line3 = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_HarmonisedNum, itemLine3.HVS_DestinationTariff));

				AssertEquals("precondition: bill number correct", "WAYBILL001", bill1.BB_BillNum);
				AssertEquals("precondition: bill number correct", "WAYBILL002", bill2.BB_BillNum);

				CombineAssertions("line BL_BB_Bill set to consignment bill", () =>
				{
					AssertEquals(bill1.PK, line1.BL_BB_Bill);
					AssertEquals(bill1.PK, line2.BL_BB_Bill);
					AssertEquals(bill2.PK, line3.BL_BB_Bill);
				});
			}
		}

		public void TestContainers_ImportedFromShipment()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "CONTAINER01";
			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_Code = "REF01";
			refContainer1.RC_ISOType = "1234";
			container1.JC_RC = refContainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "CONTAINER02";

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals(2, isfHeader.Equipments.Count);
			}

			var equipment = Factory.LoadTop1<CusISFEquip>(new ZQuery(CusISFEquipSchema.BE_ContainerNum, container1.JC_ContainerNum));
			AssertEquipmentEqualsToContainerOnShipment(container1, equipment);

			equipment = Factory.LoadTop1<CusISFEquip>(new ZQuery(CusISFEquipSchema.BE_ContainerNum, container2.JC_ContainerNum));
			AssertEquipmentEqualsToContainerOnShipment(container2, equipment);
		}

		void AssertEquipmentEqualsToContainerOnShipment(ForwardingContainer container, CusISFEquip equipment)
		{
			CombineAssertions(() =>
			{
				AssertEquals(container.JC_ContainerNum, equipment.BE_ContainerNum);
				AssertEquals(USContainerCodeList.Codes.CN, equipment.BE_EquipCode);
			});

			if (container.Container != null)
			{
				AssertEquals(container.Container.RC_ISOType, equipment.BE_ContainerISO);
			}
		}

		public void TestReferenceData_ImportedFromConsignments()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_WaybillNumber = "waybill001";
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals("Reference Data contains 2 default entries: Master Bill and House Bill.", 2, isfHeader.ReferenceDatas.Count);
				AssertContainsExactElementsInAnyOrder(new[] { isfHeader.BF_MasterBill.ToString(), "WAYBILL001" }, isfHeader.ReferenceDatas.Select(r => r.BB_BillNum));
			}
		}

		public void TestReferenceData_BillNumShouldPrefixSCACCode_IfSCACCodeNotInWaybillNumber()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var sendingAgentOrg = CreateOrgHeader("ABCD");
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_WaybillNumber = "waybill001";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				Assert("ISF Header should contain a reference SCAC code with the exact value ABCDWAYBILL001", isfHeader.ReferenceDatas.Any(r => r.BB_BillNum == "ABCDWAYBILL001"));
			}
		}

		public void TestReferenceData_BillNumShouldNotPrefixSCACCode_IfSCACCodeInWaybillNumber()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var sendingAgentOrg = CreateOrgHeader("ABCD");
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_WaybillNumber = "abcdwaybill001";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				Assert("ISF Header should contain a reference SCAC code with the exact value ABCDWAYBILL001", isfHeader.ReferenceDatas.Any(r => r.BB_BillNum == "ABCDWAYBILL001"));
			}
		}

		public void TestSCACCode_UsesHouseBillIssuingPartySCACCodeByDefault()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var houseBillIssuingOrg = CreateOrgHeader("ABCD");
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = houseBillIssuingOrg.PK;

			var sendingAgentOrg = CreateOrgHeader("EFGH");
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var companyOrgProxy = CreateOrgHeader("MNOP");
			var currentCompany = GlbCompany.CurrentCompany;
			currentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_WaybillNumber = "waybill001";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				Assert("ISF Header should contain a reference SCAC code with the exact value ABCDWAYBILL001", isfHeader.ReferenceDatas.Any(r => r.BB_BillNum == "ABCDWAYBILL001"));
			}
		}

		public void TestSCACCode_FallbackToSendingAgentOrganisation_IfHouseBillIssuingOrganisationDoesNotHaveSCACCode()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var houseBillIssuingOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = houseBillIssuingOrg.PK;

			var sendingAgentOrg = CreateOrgHeader("EFGH");
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var companyOrgProxy = CreateOrgHeader("MNOP");
			var currentCompany = GlbCompany.CurrentCompany;
			currentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_WaybillNumber = "waybill001";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				Assert("ISF Header should contain a reference SCAC code with the exact value EFGHWAYBILL001", isfHeader.ReferenceDatas.Any(r => r.BB_BillNum == "EFGHWAYBILL001"));
			}
		}

		public void TestSCACCode_FallbackToCompanyOrgProxy_IfSendingAgentOrganisationDoesNotHaveSCACCode()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var houseBillIssuingOrg = Factory.NewWithValidTestData<OrgHeader>();
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = houseBillIssuingOrg.PK;

			var sendingAgentOrg = Factory.NewWithValidTestData<OrgHeader>();
			consol.JK_OA_SendingForwarderAddress = sendingAgentOrg.MainAddress.PK;

			var companyOrgProxy = CreateOrgHeader("MNOP");
			var currentCompany = GlbCompany.CurrentCompany;
			currentCompany.GC_OH_OrgProxy = companyOrgProxy.PK;

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_WaybillNumber = "waybill001";
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				Assert("ISF Header should contain a reference SCAC code with the exact value MNOPWAYBILL001", isfHeader.ReferenceDatas.Any(r => r.BB_BillNum == "MNOPWAYBILL001"));
			}
		}

		public void TestBillNumMaxLength_AlwaysBiggerThanConsignmentWaybillNumberMaxLengthPlus4()
		{
			Assert(AutoCusISFBill.Schema.BB_BillNumMaxLength > HVLVConsignmentSchema.HVC_WaybillNumber.MaxLength + 4);
		}

		public void TestLowValueDetails_SumUpItemsAndItemLines()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item1 = Factory.New<IHVLVItem>();
			item1.HVI_HVC_Consignment = consignment1.PK;
			item1.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1_1 = Factory.New<IHVLVItemLine>();
			itemLine1_1.HVS_HVI_HVLVItem = item1.PK;
			itemLine1_1.HVS_GrossWeight = 900M;
			itemLine1_1.HVS_WeightUnit = "KG";
			itemLine1_1.HVS_CustomsValue = 9.99M;
			itemLine1_1.HVS_Quantity = 1;

			var itemLine1_2 = Factory.New<IHVLVItemLine>();
			itemLine1_2.HVS_HVI_HVLVItem = item1.PK;
			itemLine1_2.HVS_GrossWeight = 100M;
			itemLine1_2.HVS_WeightUnit = "KG";
			itemLine1_2.HVS_CustomsValue = 90M;
			itemLine1_2.HVS_Quantity = 10;

			var item2 = Factory.New<IHVLVItem>();
			item2.HVI_HVC_Consignment = consignment1.PK;
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_ManifestedWeight = 1000.00M;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals(ShipmentSubTypeList.Codes.LowValueEntriesShipments, isfHeader.BF_ShipmentSubType);
				AssertEquals(ShippingOrPackingingUnitList.PiecesCode, isfHeader.BF_EstimatedQuantityUQ);
				AssertEquals(2, isfHeader.BF_EstimatedQuantity);
				AssertEquals(100M, isfHeader.BF_EstimatedValue);
				AssertEquals(2000, isfHeader.BF_EstimatedWeight);
				AssertEquals("KG", isfHeader.BF_EstimatedWeightUQ);
			}
		}

		public void TestHeaderStuffingLocation_PopulatedByConsolDepartureCFSDocAddress()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				var consolCFSDepartureAddress = consol.GetDepartureCFSDocAddress;
				AssertNotNull(consolCFSDepartureAddress);

				AssertEquals(false, isfHeader.StuffingLocation.E2_AddressOverride);
				AssertEquals(consolCFSDepartureAddress.E2_OA_Address, isfHeader.StuffingLocation.E2_OA_Address);
			}
		}

		public void TestImporterAndConsigneeCodeType_ImportedFromImporterAndConsigneeCodeType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.UnitedStates))
			{
				var shipment = CreateHVLVShipment();
				var consol = CreateConsol();
				consol.Shipments.Add(shipment);

				var organisation = Factory.NewWithValidTestData<OrgHeader>();
				var uSC = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Constants.CountryCodes.UnitedStates);
				organisation.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.EmployerIdentificationNumber, "00-1234123US", uSC);
				shipment.ConsigneePK = organisation.PK;

				var consignment = Factory.New<IHVLVConsignment>();
				consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

				Factory.Save();

				using (shipment.SuspendDeclarationForDocuments())
				{
					var creator = new ISFFromHVLVShipmentCreator(shipment);
					var isfHeader = creator.Create(Factory);

					AssertEquals(isfHeader.BF_ImporterCode, "00-1234123US");
					AssertEquals(isfHeader.BF_ImporterCodeType, OrgCusCode.USACodeTypes.EmployerIdentificationNumber);
				}
			}
		}

		public void TestBondDetails_IsEmpty()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				AssertEquals(ZString.Empty, isfHeader.BF_BondNumberOrHolder);
				AssertEquals(ZString.Empty, isfHeader.BF_BondActivityCode);
				AssertEquals(ZString.Empty, isfHeader.BF_BondType);
				AssertEquals(ShipmentTypeList.Codes.Informal, isfHeader.BF_ShipmentType);
			}
		}

		public void TestJobDocAddress_FromHVLVSHipment_ValidationStatusIsMAN()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_ConsigneeAddress1 = "Consingee Address1";
			consignment.HVC_ConsigneeContact = "Consingee Contact";
			consignment.HVC_ShipperAddress1 = "Shipper Address1";
			consignment.HVC_ShipperContact = "Shipper Contact";

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);
				var buyingParty = isfHeader.BuyingParty;
				var sellingParty = isfHeader.SellingParty;

				AssertEquals(AddressValidationStatus.ManuallyVerified, buyingParty.E2_ValidationStatus);
				AssertEquals(AddressValidationStatus.ManuallyVerified, sellingParty.E2_ValidationStatus);
			}
		}

		public void TestImportLinesFromShipment_WhenDestinationTariffContainsDots_HarmonisedNumIsSetProperly()
		{
			var shipment = CreateHVLVShipment();
			shipment.JS_RL_NKDestination = "USCHI";
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			var item = Factory.New<IHVLVItem>();
			item.HVI_HVC_Consignment = consignment.PK;
			item.HVI_JS_LoadedOnShipment = shipment.PK;

			var itemLine1 = Factory.New<IHVLVItemLine>();
			itemLine1.HVS_HVI_HVLVItem = item.PK;
			itemLine1.HVS_DestinationTariff = "6543.21";
			itemLine1.HVS_Quantity = 1;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var isfHeader = creator.Create(Factory);

				var line = Factory.LoadTop1<CusISFLine>(new ZQuery(CusISFLineSchema.BL_TextProductCode, itemLine1.HVS_ProductCode));
				AssertEquals("654321", line.BL_HarmonisedNum);
			}
		}

		public void TestCreateISF_AddsLogToISFHeader()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var header = creator.Create(Factory);

				Factory.Save();

				var trfLogs = header.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
				AssertEquals("Log should have been added to ISF header", 1, trfLogs.Count());

				var trfLog = trfLogs.FirstOrDefault();
				AssertEquals(2, trfLog.Parameters.Count);
				AssertEquals("HVL", trfLog.Parameters[EventReferenceParameters.Codes.Type]);
				AssertEquals("S00001000", trfLog.Parameters[EventReferenceParameters.Codes.JobNumber]);
			}
		}

		public void TestCreateISF_AddsLogToShipment()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var header = creator.Create(Factory);

				Factory.Save();

				var trfLogs = shipment.Logs.Find(x => x.SL_SE_NKEvent == AutoEvents.TransferredCode);
				AssertEquals(1, trfLogs.Count());
				var trfLog = trfLogs.FirstOrDefault();
				AssertEquals("ISF", trfLog.Parameters[EventReferenceParameters.Codes.Type]);
			}
		}

		public void TestGivenShipmentWithOneConsignment_WhenCreateISFHeader_ThenISFHeaderLinksToShipment()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment = Factory.New<IHVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var header = creator.Create(Factory);

				Factory.Save();

				var isf = Factory.LoadTop1<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_JS_Shipment, shipment.PK));

				AssertNotNull("Expected to find CusISFHeader linked via ForwardingShipment PK", isf);
			}
		}

		public void TestGivenShipmentWithMultipleConsignments_WhenCreateISFHeaders_ThenAllISFHeadersLinkToShipment()
		{
			var shipment = CreateHVLVShipment();
			var consol = CreateConsol();
			consol.Shipments.Add(shipment);

			var consignment1 = Factory.New<IHVLVConsignment>();
			var consignment2 = Factory.New<IHVLVConsignment>();
			var consignment3 = Factory.New<IHVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment3.HVC_JS_ManifestedOnShipment = shipment.PK;

			Factory.Save();

			using (shipment.SuspendDeclarationForDocuments())
			{
				var creator = new ISFFromHVLVShipmentCreator(shipment);
				var header = creator.Create(Factory);

				Factory.Save();

				var isfs = Factory.Load<CusISFHeader>(new ZQuery(CusISFHeaderSchema.BF_JS_Shipment, shipment.PK));

				AssertEquals("Expected three CusISFHeaders", 3, isfs.Length);
			}
		}

		OrgHeader CreateOrgHeader(string customsRegNo, string codeType = "CCC")
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var cusCode = orgHeader.ConfigOrg.CustomsCodes.AddNew();
			cusCode.OK_CodeType = codeType;
			cusCode.OK_CustomsRegNo = customsRegNo;
			cusCode.OK_RN_NKCodeCountry = Constants.CountryCodes.UnitedStates;

			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = customsRegNo;

			return orgHeader;
		}

		ForwardingShipment CreateHVLVShipment()
		{
			var shipment = CreateShipment();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			return shipment;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.HighVolumeLowValue;
			return new ISFFromHVLVShipmentCreator(shipment);
		}

		class AfterOnSavingBOProcessingServiceForTest : IAfterOnSavingBOProcessingService
		{
			public AfterOnSavingBOProcessingServiceForTest(Action testFunction)
			{
				this.testFunction = testFunction;
			}

			readonly Action testFunction;

			public void ProcessBusinesObjects(IEnumerable<BusinessObject> businessObjectsInOnSavingOrder)
			{
				testFunction();
			}
		}
	}
}
