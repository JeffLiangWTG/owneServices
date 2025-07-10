using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.eTail.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business.eServices;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Testing;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	class MergeDataObjectWriterManagerTest : OrganizationAddressTestHelper
	{
		#region Declaration Data merge

		public void TestDeclarationDataOverwriteShipmentData()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipmentBO = GetShipmentForDeclarationMerge();
				var newFactory = new BusinessObjectFactory();
				shipmentBO = newFactory.Load<ForwardingShipment>(shipmentBO.PK);
				var consignor = shipmentBO.Consignor; //This is due to a bug. You can find detailed information in WI00147294.
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true);
				var dataObject = writer.GetDataObject(shipmentBO);
				AssertEquals("WayBillNumber", "HB78785", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.House, dataObject.WayBillType.Code);
				AssertEquals("GoodsDescription", "DECLARATION GOODS", dataObject.GoodsDescription);
				AssertNotNull("MergeBy", dataObject.MergeBy);
				AssertEquals("MergeBy.Code", "TRF", dataObject.MergeBy.Code);
				AssertNotNull("PortOfDestination", dataObject.PortOfDestination);
				AssertEquals("PortOfDestination.Code", "AUBNE", dataObject.PortOfDestination.Code);
				AssertNotNull("PortOfDischarge", dataObject.PortOfDischarge);
				AssertEquals("PortOfDischarge.Code", "NZCHC", dataObject.PortOfDischarge.Code);
				AssertNotNull("PortOfFirstArrival", dataObject.PortOfFirstArrival);
				AssertEquals("PortOfFirstArrival.Code", "NZAKL", dataObject.PortOfFirstArrival.Code);
				AssertNotNull("PortOfLoading", dataObject.PortOfLoading);
				AssertEquals("PortOfLoading.Code", "USCHI", dataObject.PortOfLoading.Code);
				AssertNotNull("PortOfOrigin", dataObject.PortOfOrigin);
				AssertEquals("PortOfOrigin.Code", "USLAX", dataObject.PortOfOrigin.Code);

				var organizationAddressCollection = dataObject.OrganizationAddressCollection;
				AssertNotNull("OrganizationAddressCollection", organizationAddressCollection);
				AssertEquals("OrganizationAddressCollection.Count", 9, organizationAddressCollection.Count);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ConsignorDocumentaryAddress", "ConsignorDocumentaryAddress", true);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ConsignorPickupDeliveryAddress", "ConsignorPickupDeliveryAddress", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ConsigneeDocumentaryAddress", "ConsigneeDocumentaryAddress", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ConsigneePickupDeliveryAddress", "ConsigneePickupDeliveryAddress", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "NotifyParty", "NotifyParty", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "SupplierDocumentaryAddress", "SupplierDocumentaryAddress", false);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ImporterDocumentaryAddress", "ImporterDocumentaryAddress", false);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "Forwarder", "Forwarder");
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ShippingLine", "ShippingLine");

				var dateCollection = dataObject.DateCollection;
				AssertNotNull("DateCollection", dateCollection);
				dateCollection.AssertDateExists(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 3, 11));
				dateCollection.AssertDateExists(DateType.Received, ZBool.False, new ZDateTime(2011, 3, 12));
				dateCollection.AssertDateExists(DateType.ShippedOnBoard, ZBool.False, new ZDateTime(2011, 3, 15));
				dateCollection.AssertDateExists(DateType.BillIssued, ZBool.False, new ZDateTime(2011, 3, 16));
				dateCollection.AssertDateExists(DateType.Departure, ZBool.True, new ZDateTime(2011, 7, 17));
				dateCollection.AssertDateExists(DateType.LoadingDate, ZBool.False, new ZDateTime(2011, 6, 12));
				dateCollection.AssertDateExists(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2011, 6, 11));
				dateCollection.AssertDateExists(DateType.DischargeDate, ZBool.False, new ZDateTime(2011, 6, 10));
				dateCollection.AssertDateExists(DateType.Arrival, ZBool.True, new ZDateTime(2011, 7, 13));
				dateCollection.AssertDateExists(DateType.EntrySubmitted, ZBool.False, new ZDateTime(2011, 7, 14));
				dateCollection.AssertDateExists(DateType.EntryAuthorisation, ZBool.False, new ZDateTime(2011, 7, 15));
				dateCollection.AssertDateExists(DateType.WarehouseRelease, ZBool.False, new ZDateTime(2011, 7, 16));
				dateCollection.AssertDateExists(DateType.EntryDate, ZBool.False, new ZDateTime(2011, 7, 18));
				dateCollection.AssertDateExists(DateType.PickupReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DeliveryReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.PickupDispatchRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DeliveryDispatchRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DeliveryDueDate, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.RevisedDeliveryDueDate, ZBool.False, ZDateTime.Empty);
				AssertEquals("Checking dates left, and found dates not expected.", 0, dateCollection.Count);

				var additionalBillCollection = dataObject.AdditionalBillCollection;
				AssertNotNull("AdditionalBillCollection", additionalBillCollection);
				AssertEquals("AdditionalBillCollection.Count", 2, additionalBillCollection.Count);
				var additionalBill1 = additionalBillCollection[0];
				var additionalBill2 = additionalBillCollection[1];
				if (additionalBill2.BillNumber.GetValueOrDefault() == "MB32423")
				{
					additionalBill1 = additionalBillCollection[1];
					additionalBill2 = additionalBillCollection[0];
				}
				AssertEquals("additionalBill1.BillNumber", "MB32423", additionalBill1.BillNumber);
				AssertNotNull("additionalBill1.BillType", additionalBill1.BillType);
				AssertEquals("additionalBill1.BillType.Code", WayBillTypeList.Codes.Master, additionalBill1.BillType.Code);
				AssertEquals("additionalBill2.BillNumber", "HB78785", additionalBill2.BillNumber);
				AssertNotNull("additionalBill2.BillType", additionalBill2.BillType);
				AssertEquals("additionalBill2.BillType.Code", WayBillTypeList.Codes.House, additionalBill2.BillType.Code);

				var commercialInfo = dataObject.CommercialInfo;
				AssertNotNull("CommercialInfo", commercialInfo);
				var commercialInvoiceCollection = commercialInfo.CommercialInvoiceCollection;
				AssertNotNull("CommercialInvoiceCollection", commercialInvoiceCollection);
				AssertEquals("CommercialInvoiceCollection.Count", 1, commercialInvoiceCollection.Count);
				var commercialInvoice = commercialInvoiceCollection[0];
				AssertEquals("CommercialInvoice.InvoiceNumber", "INV123", commercialInvoice.InvoiceNumber);

				var commercialInvoiceLineCollection = commercialInvoice.CommercialInvoiceLineCollection;
				AssertNotNull("CommercialInvoiceLineCollection", commercialInvoiceLineCollection);
				AssertEquals("CommercialInvoiceLineCollection.Count", 1, commercialInvoiceLineCollection.Count);
				var commercialInvoiceLine = commercialInvoiceLineCollection[0];
				AssertEquals("CommercialInvoiceLine.HarmonisedCode", "1020.30.40", commercialInvoiceLine.HarmonisedCode);

				var packingLineCollection = dataObject.PackingLineCollection;
				AssertNotNull("PackingLineCollection", packingLineCollection);
				AssertEquals("PackingLineCollection.Count", 1, packingLineCollection.Count);
				var packingLine = packingLineCollection[0];
				//Packing Details should always be from Shipment unless standalone
				AssertEquals("PackingLine.HarmonisedCode", "234234242", packingLine.HarmonisedCode);
				AssertEquals("PackingLine.BillNumber", null, packingLine.BillNumber);
				AssertNull("PackingLine.BillType", packingLine.BillType);
				AssertEquals("PackingLine.MarksAndNos", "SHIPMENT MARKS", packingLine.MarksAndNos);
			}
		}

		public void TestDeclarationDataDoesNotOverwriteShipmentData()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			{
				var shipmentBO = GetShipmentForDeclarationMerge();
				var newFactory = new BusinessObjectFactory();
				shipmentBO = newFactory.Load<ForwardingShipment>(shipmentBO.PK);
				var consignor = shipmentBO.Consignor; //This is due to a bug. You can find detailed information in WI00147294.
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				eAdaptorRegistry.Instance.UseBrokerageDataFirstWhenExportUniversalXML.SetValue(Environment.Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false);
				writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, shipmentBO)), true, true);
				var dataObject = writer.GetDataObject(shipmentBO);
				AssertEquals("WayBillNumber", "SHOUSE1232", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.House, dataObject.WayBillType.Code);
				AssertEquals("GoodsDescription", "SHIPMENT GOODS", dataObject.GoodsDescription);
				AssertNotNull("MergeBy", dataObject.MergeBy);
				AssertEquals("MergeBy.Code", "TRF", dataObject.MergeBy.Code);
				AssertNotNull("PortOfDestination", dataObject.PortOfDestination);
				AssertEquals("PortOfDestination.Code", "SGSIN", dataObject.PortOfDestination.Code);
				AssertNotNull("PortOfDischarge", dataObject.PortOfDischarge);
				AssertEquals("PortOfDischarge.Code", "NZCHC", dataObject.PortOfDischarge.Code);
				AssertNotNull("PortOfFirstArrival", dataObject.PortOfFirstArrival);
				AssertEquals("PortOfFirstArrival.Code", "NZAKL", dataObject.PortOfFirstArrival.Code);
				AssertNotNull("PortOfLoading", dataObject.PortOfLoading);
				AssertEquals("PortOfLoading.Code", "USCHI", dataObject.PortOfLoading.Code);
				AssertNotNull("PortOfOrigin", dataObject.PortOfOrigin);
				AssertEquals("PortOfOrigin.Code", "GBLON", dataObject.PortOfOrigin.Code);

				var organizationAddressCollection = dataObject.OrganizationAddressCollection;
				AssertNotNull("OrganizationAddressCollection", organizationAddressCollection);
				AssertEquals("OrganizationAddressCollection.Count", 9, organizationAddressCollection.Count);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ConsignorDocumentaryAddress", "ConsignorDocumentaryAddress", true);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ConsignorPickupDeliveryAddress", "ConsignorPickupDeliveryAddress", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ConsigneeDocumentaryAddress", "ConsigneeDocumentaryAddress", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ConsigneePickupDeliveryAddress", "ConsigneePickupDeliveryAddress", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "NotifyParty", "NotifyParty", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "SupplierDocumentaryAddress", "SupplierDocumentaryAddress", false);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ImporterDocumentaryAddress", "ImporterDocumentaryAddress", false);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "Forwarder", "Forwarder");
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ShippingLine", "ShippingLine");

				var dateCollection = dataObject.DateCollection;
				AssertNotNull("DateCollection", dateCollection);
				dateCollection.AssertDateExists(DateType.BookingConfirmed, ZBool.False, new ZDateTime(2011, 3, 11));
				dateCollection.AssertDateExists(DateType.Received, ZBool.False, new ZDateTime(2011, 3, 12));
				dateCollection.AssertDateExists(DateType.Departure, ZBool.True, new ZDateTime(2011, 3, 13));
				dateCollection.AssertDateExists(DateType.Arrival, ZBool.True, new ZDateTime(2011, 3, 14));
				dateCollection.AssertDateExists(DateType.ShippedOnBoard, ZBool.False, new ZDateTime(2011, 3, 15));
				dateCollection.AssertDateExists(DateType.BillIssued, ZBool.False, new ZDateTime(2011, 3, 16));
				dateCollection.AssertDateExists(DateType.LoadingDate, ZBool.False, new ZDateTime(2011, 6, 12));
				dateCollection.AssertDateExists(DateType.FirstArrivalInCountry, ZBool.False, new ZDateTime(2011, 6, 11));
				dateCollection.AssertDateExists(DateType.DischargeDate, ZBool.False, new ZDateTime(2011, 6, 10));
				dateCollection.AssertDateExists(DateType.EntrySubmitted, ZBool.False, new ZDateTime(2011, 7, 14));
				dateCollection.AssertDateExists(DateType.EntryAuthorisation, ZBool.False, new ZDateTime(2011, 7, 15));
				dateCollection.AssertDateExists(DateType.WarehouseRelease, ZBool.False, new ZDateTime(2011, 7, 16));
				dateCollection.AssertDateExists(DateType.EntryDate, ZBool.False, new ZDateTime(2011, 7, 18));
				dateCollection.AssertDateExists(DateType.PickupReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DeliveryReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.PickupDispatchRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DeliveryDispatchRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DeliveryDueDate, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.RevisedDeliveryDueDate, ZBool.False, ZDateTime.Empty);
				AssertEquals("Checking dates left, and found dates not expected.", 0, dateCollection.Count);

				var additionalBillCollection = dataObject.AdditionalBillCollection;
				AssertNotNull("AdditionalBillCollection", additionalBillCollection);
				AssertEquals("AdditionalBillCollection.Count", 2, additionalBillCollection.Count);
				var additionalBill1 = additionalBillCollection[0];
				var additionalBill2 = additionalBillCollection[1];
				if (additionalBill2.BillNumber.GetValueOrDefault() == "MB32423")
				{
					additionalBill1 = additionalBillCollection[1];
					additionalBill2 = additionalBillCollection[0];
				}
				AssertEquals("additionalBill1.BillNumber", "MB32423", additionalBill1.BillNumber);
				AssertNotNull("additionalBill1.BillType", additionalBill1.BillType);
				AssertEquals("additionalBill1.BillType.Code", WayBillTypeList.Codes.Master, additionalBill1.BillType.Code);
				AssertEquals("additionalBill2.BillNumber", "HB78785", additionalBill2.BillNumber);
				AssertNotNull("additionalBill2.BillType", additionalBill2.BillType);
				AssertEquals("additionalBill2.BillType.Code", WayBillTypeList.Codes.House, additionalBill2.BillType.Code);

				var commercialInfo = dataObject.CommercialInfo;
				AssertNotNull("CommercialInfo", commercialInfo);
				var commercialInvoiceCollection = commercialInfo.CommercialInvoiceCollection;
				AssertNotNull("CommercialInvoiceCollection", commercialInvoiceCollection);
				AssertEquals("CommercialInvoiceCollection.Count", 1, commercialInvoiceCollection.Count);
				var commercialInvoice = commercialInvoiceCollection[0];
				AssertEquals("CommercialInvoice.InvoiceNumber", "INV123", commercialInvoice.InvoiceNumber);

				var commercialInvoiceLineCollection = commercialInvoice.CommercialInvoiceLineCollection;
				AssertNotNull("CommercialInvoiceLineCollection", commercialInvoiceLineCollection);
				AssertEquals("CommercialInvoiceLineCollection.Count", 1, commercialInvoiceLineCollection.Count);
				var commercialInvoiceLine = commercialInvoiceLineCollection[0];
				AssertEquals("CommercialInvoiceLine.HarmonisedCode", "1020.30.40", commercialInvoiceLine.HarmonisedCode);

				var packingLineCollection = dataObject.PackingLineCollection;
				AssertNotNull("PackingLineCollection", packingLineCollection);
				AssertEquals("PackingLineCollection.Count", 1, packingLineCollection.Count);
				var packingLine = packingLineCollection[0];
				AssertEquals("PackingLine.HarmonisedCode", "234234242", packingLine.HarmonisedCode);
				AssertEquals("PackingLine.BillNumber", null, packingLine.BillNumber);
				AssertNull("PackingLine.BillType", packingLine.BillType);
				AssertEquals("PackingLine.MarksAndNos", "SHIPMENT MARKS", packingLine.MarksAndNos);
			}
		}

		ForwardingShipment GetShipmentForDeclarationMerge()
		{
			var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
			var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
			var shipmentBO = Factory.New<ForwardingShipment>();
			shipmentBO.JS_UniqueConsignRef = "S3210001";
			shipmentBO.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
			shipmentBO.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
			shipmentBO.JS_HouseBill = "SHOUSE1232";
			shipmentBO.JS_RL_NKDestination = "SGSIN";
			shipmentBO.JS_RL_NKOrigin = "GBLON";
			shipmentBO.JS_GoodsDescription = "SHIPMENT GOODS";

			shipmentBO.JS_TransportMode = "SEA"; // Sea Freight
			shipmentBO.JS_PackingMode = "LCL";
			shipmentBO.JS_ShipmentType = "STD"; // Standard House

			shipmentBO.JS_A_BKD = new ZDateTime(2011, 3, 11);
			shipmentBO.JS_A_RCV = new ZDateTime(2011, 3, 12);
			shipmentBO.JS_E_DEP = new ZDateTime(2011, 3, 13);
			shipmentBO.JS_E_ARV = new ZDateTime(2011, 3, 14);
			shipmentBO.JS_ShippedOnBoardDate = new ZDateTime(2011, 3, 15);
			shipmentBO.JS_HouseBillIssueDate = new ZDateTime(2011, 3, 16);

			var packLine = shipmentBO.OuterPackLines[0];
			packLine.JL_HarmonisedCode = "234234242";
			packLine.JL_MarksAndNumbers = "SHIPMENT MARKS";
			packLine.JL_Description = "SHIPMENT DESCRIPTION";

			var declarationBO = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobDeclaration>();
			declarationBO.FillWithValidTestData();
			declarationBO[JobDeclarationSchema.JE_OverrideFreightDefaults] = ZBool.True;
			declarationBO[JobDeclarationSchema.JE_JS] = shipmentBO.PK;
			declarationBO[JobDeclarationSchema.JE_OH_Supplier] = org2.PK;
			declarationBO[JobDeclarationSchema.JE_OH_Importer] = org1.PK;
			declarationBO[JobDeclarationSchema.JE_OH_ShippingLine] = org2.PK;
			declarationBO[JobDeclarationSchema.JE_OH_Forwarder] = org1.PK;
			declarationBO[JobDeclarationSchema.JE_MasterBill] = "MB32423";
			declarationBO[JobDeclarationSchema.JE_HouseBill] = "HB78785";
			declarationBO[JobDeclarationSchema.JE_RL_NKOrigin] = "USLAX";
			declarationBO[JobDeclarationSchema.JE_RL_NKPortOfLoading] = "USCHI";
			declarationBO[JobDeclarationSchema.JE_RL_NKPortOfArrival] = "NZCHC";
			declarationBO[JobDeclarationSchema.JE_RL_NKPortOfFirstArrival] = "NZAKL";
			declarationBO[JobDeclarationSchema.JE_RL_NKFinalDestination] = "AUBNE";
			declarationBO[JobDeclarationSchema.JE_GoodsDescription] = "DECLARATION GOODS";
			declarationBO[JobDeclarationSchema.JE_MergeBy] = "TRF";
			declarationBO[JobDeclarationSchema.JE_DateOfArrival] = new ZDateTime(2011, 6, 10);
			declarationBO[JobDeclarationSchema.JE_DateOfFirstArrival] = new ZDateTime(2011, 6, 11);
			declarationBO[JobDeclarationSchema.JE_ExportDate] = new ZDateTime(2011, 6, 12);
			declarationBO[JobDeclarationSchema.JE_DateAtFinalDestination] = new ZDateTime(2011, 7, 13);
			declarationBO[JobDeclarationSchema.JE_EntrySubmittedDate] = new ZDateTime(2011, 7, 14);
			declarationBO[JobDeclarationSchema.JE_EntryAuthorisationDate] = new ZDateTime(2011, 7, 15);
			declarationBO[JobDeclarationSchema.JE_WarehouseReleaseDate] = new ZDateTime(2011, 7, 16);
			declarationBO[JobDeclarationSchema.JE_DateAtOrigin] = new ZDateTime(2011, 7, 17);
			declarationBO[JobDeclarationSchema.JE_EntryDate] = new ZDateTime(2011, 7, 18);

			var masterBillQuery = new ZQuery(CusDecHouseBillSchema.CU_JE, declarationBO.PK);
			masterBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, "MB32423");
			var masterBillBO = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBill>(masterBillQuery);
			if (masterBillBO == null)
			{
				masterBillBO = Factory.BOFactory.New<Enterprise.Integration.Customs.IBill>();
				masterBillBO.CU_BillNum = "MB32423";
				masterBillBO.CU_BillType = "MB";
				masterBillBO.CU_JE = declarationBO.PK;
				masterBillBO.CU_GUIPresentationRecord = ZBool.True;
			}
			masterBillBO.CU_IssueDate = new ZDateTime(2011, 5, 20);

			var houseBillQuery = new ZQuery(CusDecHouseBillSchema.CU_JE, declarationBO.PK);
			houseBillQuery.AddToFilter(CusDecHouseBillSchema.CU_BillNum, "HB78785");
			var houseBillBO = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBill>(houseBillQuery);
			if (houseBillBO == null)
			{
				houseBillBO = Factory.BOFactory.New<Enterprise.Integration.Customs.IBill>();
				houseBillBO.CU_BillNum = "HB78785";
				houseBillBO.CU_BillType = "HB";
				houseBillBO.CU_JE = declarationBO.PK;
				houseBillBO.CU_GUIPresentationRecord = ZBool.True;
				houseBillBO.CU_CU_ParentBill = masterBillBO.PK;
			}
			houseBillBO.CU_IssueDate = new ZDateTime(2011, 5, 21);

			var packingGroup = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBasePackingGroup>(new ZQuery(CusDecHouseContainerPivotSchema.CR_CU_HouseBill, houseBillBO.PK));
			if (packingGroup == null)
			{
				packingGroup = Factory.BOFactory.New<Enterprise.Integration.Customs.IBasePackingGroup>();
				packingGroup.CR_CU_HouseBill = houseBillBO.PK;
			}

			var package = Factory.BOFactory.LoadTop1<Enterprise.Integration.Customs.IBasePackage>(new ZQuery(CusDecHouseContainerPackSchema.CW_CR_HouseContainer, packingGroup.PK));
			if (package == null)
			{
				package = Factory.BOFactory.New<Enterprise.Integration.Customs.IBasePackage>();
				package.CW_CR_HouseContainer = packingGroup.PK;
			}
			package.CW_MarksAndNos = "DEC MARKS NO";

			var invoice = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.Shared.IBaseJobComInvoiceHeader>();
			invoice[JobComInvoiceHeaderSchema.JZ_JE] = declarationBO.PK;
			invoice[JobComInvoiceHeaderSchema.JZ_InvoiceNumber] = "INV123";

			var invoiceLine = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.IBaseJobComInvoiceLine>();
			invoiceLine[JobComInvoiceLineSchema.JI_JZ] = invoice.PK;
			invoiceLine[JobComInvoiceLineSchema.JI_Tariff] = "1020.30.40";

			Factory.SaveForTesting();
			return shipmentBO;
		}

		#endregion

		#region CusHAWB Data Merge

		public void TestAUCusHAWBDataDoesOverwriteShipmentDataForAAD()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var consolBO = Factory.New<ForwardingConsol>();
				consolBO.JK_RL_NKDischargePort = "AUSYD";
				var shipmentBO = consolBO.Shipments.AddNew();
				shipmentBO.JS_UniqueConsignRef = "S3210001";
				shipmentBO.JS_TransportMode = "AIR";
				shipmentBO.JS_ShipmentType = "STD";
				shipmentBO.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
				shipmentBO.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
				shipmentBO.JS_HouseBill = "SHOUSE1232";
				shipmentBO.JS_RL_NKOrigin = "GBLON";
				shipmentBO.JS_RL_NKDestination = "SGSIN";
				shipmentBO.JS_GoodsDescription = "SHIPMENT GOODS";

				var mawbBO = Factory.BOFactory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
				mawbBO.CM_JK = consolBO.PK;
				var hawbBO = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
				hawbBO.FillWithValidTestData();
				hawbBO[CusHAWBSchema.CS_CM] = mawbBO.PK;
				hawbBO[CusHAWBSchema.CS_JS] = shipmentBO.PK;
				hawbBO[CusHAWBSchema.CS_OA_ConsignorAddress] = org2.MainAddress.PK;
				hawbBO[CusHAWBSchema.CS_OA_ConsigneeAddress] = org1.MainAddress.PK;
				hawbBO[CusHAWBSchema.CS_HAWB] = "HB32423";
				hawbBO[CusHAWBSchema.CS_MasterHouseBill] = "HB78785";
				hawbBO[CusHAWBSchema.CS_RL_NKOrigin] = "USLAX";
				hawbBO[CusHAWBSchema.CS_RL_NKDestination] = "NZAKL";
				hawbBO[CusHAWBSchema.CS_GoodsDescription] = "HAWB GOODS";
				hawbBO[CusHAWBSchema.CS_ShipmentType] = "STT";
				Factory.SaveForTesting();

				var newFactory = new BusinessObjectFactory();
				shipmentBO = newFactory.Load<ForwardingShipment>(shipmentBO.PK);
				var consignor = shipmentBO.Consignor; //This is due to a bug. You can find detailed information in WI00147294.
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AAD, shipmentBO)), true, true);

				var consolDataObject = writer.GetDataObject(shipmentBO);
				AssertNotNull("Should have AirManifestLine", consolDataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				var dataObject = consolDataObject.SubShipmentCollection[0];
				AssertNotNull("Should have AirManifestLine", dataObject.GetMatchingDataSource(DataContextType.AirManifestLine));
				AssertEquals("WayBillNumber", "HB32423", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.House, dataObject.WayBillType.Code);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", "STT", dataObject.ShipmentType.Code);
				AssertEquals("GoodsDescription", "HAWB GOODS", dataObject.GoodsDescription);
				AssertNotNull("PortOfOrigin", dataObject.PortOfOrigin);
				AssertEquals("PortOfOrigin.Code", "USLAX", dataObject.PortOfOrigin.Code);
				AssertNotNull("PortOfDestination", dataObject.PortOfDestination);
				AssertEquals("PortOfDestination.Code", "NZAKL", dataObject.PortOfDestination.Code);

				var organizationAddressCollection = dataObject.OrganizationAddressCollection;
				AssertNotNull("OrganizationAddressCollection", organizationAddressCollection);
				AssertEquals("OrganizationAddressCollection.Count", 5, organizationAddressCollection.Count);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ConsignorDocumentaryAddress", "ConsignorDocumentaryAddress");
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ConsignorPickupDeliveryAddress", "ConsignorPickupDeliveryAddress", true);
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, "ConsigneeDocumentaryAddress", "ConsigneeDocumentaryAddress");
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "ConsigneePickupDeliveryAddress", "ConsigneePickupDeliveryAddress", true);
				AssertAddress("NotifyParty", GetOrganizationAddressByType(dataObject, DocAddressType.NotifyParty), nameof(DocAddressType.NotifyParty), null, ZString.Empty, null, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, ZString.Empty, null, null, null, ZString.Empty);

				var additionalBillCollection = dataObject.AdditionalBillCollection;
				AssertNotNull("AdditionalBillCollection", additionalBillCollection);
				AssertEquals("AdditionalBillCollection.Count", 1, additionalBillCollection.Count);
				var additionalBill = additionalBillCollection[0];
				AssertEquals("additionalBill.BillNumber", "HB32423", additionalBill.BillNumber);
				AssertNotNull("additionalBill.BillType", additionalBill.BillType);
				AssertEquals("additionalBill.BillType.Code", WayBillTypeList.Codes.House, additionalBill.BillType.Code);
				AssertEquals("additionalBill.ParentBillNumber", "HB78785", additionalBill.ParentBillNumber);
			}
		}

		public void TestAUCusHAWBDataDoesOverwriteMasterBillDetailsForHAC()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var consolBO = Factory.New<ForwardingConsol>();
				consolBO.JK_RL_NKDischargePort = "AUSYD";
				consolBO.JK_TransportMode = "AIR";
				var leg = consolBO.Transports[0];
				leg.JW_VoyageFlight = "QF123";
				leg.JW_ETA = ZDateTime.Today.AddDays(1);

				var shipmentBO = consolBO.Shipments.AddNew();
				shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
				shipmentBO.JS_UniqueConsignRef = "S3210001";
				shipmentBO.JS_TransportMode = "AIR";
				shipmentBO.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
				shipmentBO.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
				shipmentBO.JS_HouseBill = "SHOUSE1232";
				shipmentBO.JS_RL_NKOrigin = "GBLON";
				shipmentBO.JS_RL_NKDestination = "SGSIN";
				shipmentBO.JS_GoodsDescription = "SHIPMENT GOODS";

				var consignment = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
				consignment.HVC_JS_ManifestedOnShipment = shipmentBO.PK;
				consignment.Items.AddNew();

				var consignment1 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
				consignment1.HVC_JS_ManifestedOnShipment = shipmentBO.PK;
				consignment1.Items.AddNew();

				var mawbBO = Factory.BOFactory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
				mawbBO.CM_JK = consolBO.PK;
				mawbBO.CM_FlightNo = "QF879";//updated by CARST
				mawbBO.CM_ArrivalDate = ZDateTime.Today;//updated by CARST
				var hawbBO = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.AU.ICusHAWB>();
				hawbBO.FillWithValidTestData();
				hawbBO[CusHAWBSchema.CS_CM] = mawbBO.PK;
				hawbBO[CusHAWBSchema.CS_JS] = shipmentBO.PK;
				hawbBO[CusHAWBSchema.CS_OH_Consignor] = org2.PK;
				hawbBO[CusHAWBSchema.CS_OH_Consignee] = org1.PK;
				hawbBO[CusHAWBSchema.CS_HAWB] = "HB32423";
				hawbBO[CusHAWBSchema.CS_MasterHouseBill] = "HB78785";
				hawbBO[CusHAWBSchema.CS_RL_NKOrigin] = "USLAX";
				hawbBO[CusHAWBSchema.CS_RL_NKDestination] = "NZAKL";
				hawbBO[CusHAWBSchema.CS_GoodsDescription] = "HAWB GOODS";
				hawbBO[CusHAWBSchema.CS_ShipmentType] = "STT";
				Factory.SaveForTesting();

				var newFactory = new BusinessObjectFactory();
				shipmentBO = newFactory.Load<ForwardingShipment>(shipmentBO.PK);
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.HCA, shipmentBO)), true, true);

				var consolDataObject = writer.GetDataObject(shipmentBO);
				AssertEquals("QF879", consolDataObject.VoyageFlightNo);
				AssertEquals(mawbBO.CM_ArrivalDate, consolDataObject.DateCollection.Find(x => x.Type.Value == DateType.DischargeDate).Value);

				var shipmentCollection = consolDataObject.SubShipmentCollection;
				AssertEquals(2, shipmentCollection[0].SubShipmentCollection.Count);
			}
		}

		#endregion

		#region CusMAWB Data Merge

		public void TestAUCusMAWBDataDoesOverwriteConsolDataForAAD()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var consolBO = Factory.New<ForwardingConsol>();
				consolBO.JK_UniqueConsignRef = "C3210001";
				consolBO.JK_AgentType = Core.Constants.AgentType.CoLoad;
				consolBO.JK_TransportMode = "AIR";
				consolBO.JK_RL_NKLoadPort = "GBLON";
				consolBO.JK_RL_NKDischargePort = "AUSYD";
				consolBO.JK_MasterBillNum = "OB2342";
				consolBO.JK_BookingReference = "BK2342";
				consolBO.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
				consolBO.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

				var mawbBO = Factory.BOFactory.New<Enterprise.Integration.Customs.AU.ICusMAWB>();
				((BusinessObject)mawbBO).FillWithValidTestData();
				mawbBO.CM_JK = consolBO.PK;
				mawbBO.CM_MAWB = "MB2342";
				mawbBO.CM_MasterHouseBill = "MHB234";
				mawbBO.CM_OH_ResponsibleParty = org2.PK;
				mawbBO.CM_RL_NKLoadPort = "NZCHC";
				mawbBO.CM_RL_NKDischargePort = "AUMEL";
				mawbBO.CM_ArrivalDate = ZDateTime.BrettsBirthday;
				Factory.SaveForTesting();

				var newFactory = new BusinessObjectFactory();
				consolBO = newFactory.Load<ForwardingConsol>(consolBO.PK);
				var writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.AAD, consolBO)));
				var dataObject = writer.GetDataObject(consolBO);
				AssertNotNull("Should have AirManifest", dataObject.GetMatchingDataSource(DataContextType.AirManifest));
				AssertEquals("WayBillNumber", "MB2342", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.Master, dataObject.WayBillType.Code);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", Core.Constants.AgentType.CoLoad, dataObject.ShipmentType.Code);
				AssertNotNull("PortOfLoading", dataObject.PortOfLoading);
				AssertEquals("PortOfLoading.Code", "NZCHC", dataObject.PortOfLoading.Code);
				AssertNotNull("PortOfDischarge", dataObject.PortOfDischarge);
				AssertEquals("PortOfDischarge.Code", "AUMEL", dataObject.PortOfDischarge.Code);

				var organizationAddressCollection = dataObject.OrganizationAddressCollection;
				AssertNotNull("OrganizationAddressCollection", organizationAddressCollection);
				AssertEquals("OrganizationAddressCollection.Count", 4, organizationAddressCollection.Count);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "NotifyParty", "NotifyParty", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, nameof(DocAddressType.ReceivingForwarderAddress), nameof(DocAddressType.ReceivingForwarderAddress));
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, nameof(DocAddressType.SendingForwarderAddress), nameof(DocAddressType.SendingForwarderAddress));
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, AddressTypes.ResponsibleParty, AddressTypes.ResponsibleParty);

				var dateCollection = dataObject.DateCollection;
				AssertNotNull("DateCollection", dateCollection);
				dateCollection.AssertDateExists(DateType.ShippedOnBoard, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.BillIssued, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.FirstArrivalInCountry, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.LoadingDate, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DischargeDate, ZBool.False, ZDateTime.BrettsBirthday);
				dateCollection.AssertDateExists(DateType.CutOffDate, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.FirstForeignArrival, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.LastForeignDeparture, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DepartureReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.ArrivalReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DepartureDispatchRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.ArrivalDispatchRequested, ZBool.False, ZDateTime.Empty);
				AssertEquals("Checking dates left, and found dates not expected.", 0, dateCollection.Count);

				writer = new ConsolDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.ORP, consolBO)));
				dataObject = writer.GetDataObject(consolBO);
				AssertEquals("WayBillNumber", "OB2-342", dataObject.WayBillNumber);
				AssertNotNull("WayBillType", dataObject.WayBillType);
				AssertEquals("WayBillType.Code", WayBillTypeList.Codes.Master, dataObject.WayBillType.Code);
				AssertEquals("BookingConfirmationReference", "BK2342", dataObject.BookingConfirmationReference);
				AssertNotNull("ShipmentType", dataObject.ShipmentType);
				AssertEquals("ShipmentType.Code", Core.Constants.AgentType.CoLoad, dataObject.ShipmentType.Code);
				AssertNotNull("PortOfLoading", dataObject.PortOfLoading);
				AssertEquals("PortOfLoading.Code", "GBLON", dataObject.PortOfLoading.Code);
				AssertNotNull("PortOfDischarge", dataObject.PortOfDischarge);
				AssertEquals("PortOfDischarge.Code", "AUSYD", dataObject.PortOfDischarge.Code);

				organizationAddressCollection = dataObject.OrganizationAddressCollection;
				AssertNotNull("OrganizationAddressCollection", organizationAddressCollection);
				AssertEquals("OrganizationAddressCollection.Count", 3, organizationAddressCollection.Count);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, "NotifyParty", "NotifyParty", true);
				AssertOrganizationBO_CRAHOLSYDExists(organizationAddressCollection, nameof(DocAddressType.ReceivingForwarderAddress), nameof(DocAddressType.ReceivingForwarderAddress));
				AssertOrganizationBO_WUFSHIJNBExists(organizationAddressCollection, nameof(DocAddressType.SendingForwarderAddress), nameof(DocAddressType.SendingForwarderAddress));

				dateCollection = dataObject.DateCollection;
				AssertNotNull("DateCollection", dateCollection);
				dateCollection.AssertDateExists(DateType.ShippedOnBoard, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.BillIssued, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.FirstArrivalInCountry, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.CutOffDate, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.FirstForeignArrival, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.LastForeignDeparture, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DepartureReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.ArrivalReceiptRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.DepartureDispatchRequested, ZBool.False, ZDateTime.Empty);
				dateCollection.AssertDateExists(DateType.ArrivalDispatchRequested, ZBool.False, ZDateTime.Empty);
				AssertEquals("Checking dates left, and found dates not expected.", 0, dateCollection.Count);
			}
		}

		#endregion

		#region CusSCAOceanBill Data Merger

		public void TestAUCUsSCAOceanBillDoesOverrideMasterBillDetailsForHSA()
		{
			using (SchemaVersionManager.SetNamespaceForTesting(UniversalXmlInfo.Namespace_2011_11))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Australia))
			{
				var org1 = GetOrganizationBO_WUFSHIJNB(Factory.BOFactory);
				var org2 = GetOrganizationBO_CRAHOLSYD(Factory.BOFactory);
				var consolBO = Factory.New<ForwardingConsol>();
				consolBO.JK_RL_NKDischargePort = "AUSYD";
				consolBO.JK_TransportMode = "SEA";
				var leg = consolBO.Transports[0];
				leg.JW_VoyageFlight = "QF123";
				leg.JW_ETA = ZDateTime.Today.AddDays(1);

				var shipmentBO = consolBO.Shipments.AddNew();
				shipmentBO.JS_ShipmentType = Core.Constants.ShipmentTypes.HighVolumeLowValue;
				shipmentBO.JS_UniqueConsignRef = "S3210001";
				shipmentBO.JS_TransportMode = "SEA";
				shipmentBO.ConsignorDocumentaryAddress.OrganisationPK = org1.PK;
				shipmentBO.ConsigneeDocumentaryAddress.OrganisationPK = org2.PK;
				shipmentBO.JS_HouseBill = "SHOUSE1232";
				shipmentBO.JS_RL_NKOrigin = "GBLON";
				shipmentBO.JS_RL_NKDestination = "SGSIN";
				shipmentBO.JS_GoodsDescription = "SHIPMENT GOODS";

				var consignment = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
				consignment.HVC_JS_ManifestedOnShipment = shipmentBO.PK;
				consignment.Items.AddNew();

				var consignment1 = (IHVLVConsignment)Factory.NewWithValidTestData(ObjectFactory.GetType(typeof(IHVLVConsignment)));
				consignment1.HVC_JS_ManifestedOnShipment = shipmentBO.PK;
				consignment1.Items.AddNew();

				var oceanBill = Factory.BOFactory.New<Enterprise.Integration.Customs.AU.ICusSCAOceanBill>();
				oceanBill.CB_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
				oceanBill.CB_ParentId = consolBO.PK;
				oceanBill.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
				oceanBill.CB_Voyage = "QF879";//updated by CARST
				oceanBill.CB_DateOfArrival = ZDateTime.Today;//updated by CARST
				var house = (BusinessObject)Factory.BOFactory.New<Enterprise.Integration.Customs.AU.ICusSCAHouse>();
				house.FillWithValidTestData();
				house[CusSCAHouseSchema.CA_CB] = oceanBill.PK;
				house[CusSCAHouseSchema.CA_JS] = shipmentBO.PK;
				house[CusSCAHouseSchema.CA_OH_Consignor] = org2.PK;
				house[CusSCAHouseSchema.CA_OH_Consignee] = org1.PK;
				house[CusSCAHouseSchema.CA_HouseBill] = "HB32423";
				house[CusSCAHouseSchema.CA_MasterHouseBill] = "HB78785";
				house[CusSCAHouseSchema.CA_RL_NK_PortOfOrigin] = "USLAX";
				house[CusSCAHouseSchema.CA_RL_NK_PortOfDestination] = "NZAKL";
				Factory.SaveForTesting();

				var newFactory = new BusinessObjectFactory();
				shipmentBO = newFactory.Load<ForwardingShipment>(shipmentBO.PK);
				var writer = new ShipmentDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, shipmentBO)), true, true);

				var consolDataObject = writer.GetDataObject(shipmentBO);
				AssertEquals("QF879", consolDataObject.VoyageFlightNo);
				AssertEquals(oceanBill.CB_DateOfArrival, consolDataObject.DateCollection.Find(x => x.Type.Value == DateType.DischargeDate).Value);

				var shipmentCollection = consolDataObject.SubShipmentCollection;
				AssertEquals(2, shipmentCollection[0].SubShipmentCollection.Count);
			}
		}

		#endregion

		static OrganizationAddress GetOrganizationAddressByType(Shipment shipment, DocAddressType type)
		{
			return shipment.OrganizationAddressCollection.Single(org => org != null && org.AddressType.HasValue && org.AddressType.Value == type.ToString());
		}
	}
}
