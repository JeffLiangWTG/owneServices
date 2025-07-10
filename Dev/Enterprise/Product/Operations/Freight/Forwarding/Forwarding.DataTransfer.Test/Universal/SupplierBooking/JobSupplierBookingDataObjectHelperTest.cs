using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.MasterFiles.DataTransfer.Universal.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.ZArchitecture.Schema;
using UniversalOrganizationAddress = Enterprise.UniversalDataBuss.DataObjects.Universal.OrganizationAddress;
using UniversalShipment = Enterprise.UniversalDataBuss.DataObjects.Universal.Shipment;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	public class JobSupplierBookingDataObjectHelperTest : OrganizationAddressTestHelper
	{
		public void TestFindByDocAddressType()
		{
			AssertNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(null, DocAddressType.Supplier));

			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			AssertNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.Supplier));

			dataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			AssertNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.Supplier));

			dataObject.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.ConsigneeAddress), "CA", "SGSIN"));
			AssertNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.Supplier));

			dataObject.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Supplier), "SP", "SGSIN"));
			AssertNotNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.Supplier));
			AssertEquals("SPSIN", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.Supplier).OrganizationCode);

			dataObject.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.LocalCartageCFS), "LC", "LCCFS"));
			AssertNotNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.LocalCartageCFS));
			AssertEquals("LCCFS", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.LocalCartageCFS).OrganizationCode);
		}

		public void TestFindByDocAddressTypeAndGetMatched()
		{
			var dataObject = new UniversalShipment(DefaultDataObjectWriterStrategy.TestInstance);
			dataObject.SetOrganizationAddressCollection(() => new List<UniversalOrganizationAddress>());
			dataObject.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.Supplier), "SP", "SGSIN"));
			dataObject.OrganizationAddressCollection.Add(GetAddressData(nameof(DocAddressType.LocalCartageCFS), "LC", "LCCFS"));
			AssertNull(JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.Supplier, Logger, Factory));
			AssertNull(JobSupplierBookingDataObjectHelper.FindByDocAddressTypeAndGetMatched(dataObject, DocAddressType.LocalCartageCFS, Logger, Factory));

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SPSIN";
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();

			var localCartageCFS = Factory.NewWithValidTestData<OrgHeader>();
			localCartageCFS.OH_Code = "LCCFS";
			localCartageCFS.Addresses.AddNew().FillWithValidTestData();
			localCartageCFS.Contacts.AddNew().FillWithValidTestData();

			Factory.SaveForTesting();

			AssertNotNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.Supplier));
			AssertEquals("SPSIN", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.Supplier).OrganizationCode);

			AssertNotNull(JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.LocalCartageCFS));
			AssertEquals("LCCFS", JobSupplierBookingDataObjectHelper.FindByDocAddressType(dataObject, DocAddressType.LocalCartageCFS).OrganizationCode);
		}

		public static JobSupplierBooking BuildJobSupplierBookingForTest(UniversalObjectFactory factory, string loadMode = Core.Constants.SupplierBookingLoadMode.ContainerFreightStation)
		{
			var order = factory.NewWithValidTestData<Order>();
			order.JD_OrderNumber = "ORD0001";
			order.JD_OrderNumberSplit = 1;
			var buyer = factory.NewWithValidTestData<OrgHeader>();
			buyer.OH_Code = "Buyer";
			buyer.Addresses.AddNew().FillWithValidTestData();
			buyer.Contacts.AddNew().FillWithValidTestData();
			order.JD_OA_BuyerAddress = buyer.Addresses[0].PK;
			order.JD_OC_BuyerContact = buyer.Contacts[0].PK;
			var orderLine = order.OrderLines.AddNew();
			orderLine.JO_LineNo = 1;
			orderLine.JO_SubLineNo = 2;
			orderLine.JO_LineReference = "ORL001";
			orderLine.JO_F3_NKPackType = "PLT";
			orderLine.JO_Partno = "ME100770267";
			orderLine.JO_Description = "T Shirts";
			orderLine.JO_ItemPrice = 5.5732;
			orderLine.JO_LinePrice = 2507.9544;
			orderLine.JO_ExWorksDate = new ZDateTime(2022, 3, 27);
			orderLine.JO_LineDropDate = new ZDateTime(2022, 4, 27);
			orderLine.JO_ShipmentWindowStart = new ZDate(2023, 1, 20);
			orderLine.JO_ShipmentWindowEnd = new ZDate(2023, 2, 20);

			var bookingParty = factory.NewWithValidTestData<OrgHeader>();
			bookingParty.OH_Code = "BKSIN";

			var controllingCustomer = factory.NewWithValidTestData<OrgHeader>();
			controllingCustomer.OH_Code = "CCSZX";
			controllingCustomer.Addresses.AddNew().FillWithValidTestData();
			controllingCustomer.Contacts.AddNew().FillWithValidTestData();

			var consignee = factory.NewWithValidTestData<OrgHeader>();
			consignee.OH_Code = "CNSHA";
			consignee.Addresses.AddNew().FillWithValidTestData();
			consignee.Contacts.AddNew().FillWithValidTestData();

			var supplier = factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_Code = "SPSIN";
			supplier.Addresses.AddNew().FillWithValidTestData();
			supplier.Contacts.AddNew().FillWithValidTestData();

			var manufacturer = factory.NewWithValidTestData<OrgHeader>();
			manufacturer.OH_Code = "MFAKL";
			manufacturer.Addresses.AddNew().FillWithValidTestData();
			manufacturer.Contacts.AddNew().FillWithValidTestData();

			var localCartageCFS = factory.NewWithValidTestData<OrgHeader>();
			localCartageCFS.OH_Code = "LCCFS";
			localCartageCFS.Addresses.AddNew().FillWithValidTestData();
			localCartageCFS.Contacts.AddNew().FillWithValidTestData();

			var cfsOrgHeader = factory.NewWithValidTestData<OrgHeader>();
			cfsOrgHeader.OH_Code = "CCCcC";
			cfsOrgHeader.Addresses.AddNew().FillWithValidTestData();
			cfsOrgHeader.Addresses[0].OA_RL_NKRelatedPortCode = "AUSYD";

			var supplierBooking = factory.New<JobSupplierBooking>();
			supplierBooking.JSB_TransportMode = Core.Constants.TransportModes.Sea;
			supplierBooking.JSB_BookingId = "SBK001";
			supplierBooking.JSB_LoadMode = loadMode;
			supplierBooking.JSB_RL_NKLoadPort = "AUSYD";
			supplierBooking.JSB_RL_NKDischargePort = "CNCAN";
			supplierBooking.JSB_RL_NKOrigin = "AUMEL";
			supplierBooking.JSB_RL_NKDestination = "SGSIN";
			supplierBooking.JSB_CargoAvailableDate = new ZDate(2022, 2, 3);
			supplierBooking.JSB_OH_BookingParty = bookingParty.PK;
			supplierBooking.JSB_BookedOnDate = new ZDate(2022, 2, 5);
			supplierBooking.JSB_Status = "PLC";
			supplierBooking.JSB_ContainerMode = "FCL";
			supplierBooking.JSB_OA_CFSAddress = factory.LoadTop1<OrgAddress>(new ZQuery(OrgAddressSchema.OA_OH, cfsOrgHeader.PK)).PK;
			supplierBooking.JSB_GoodsDescription = "Good Desc";
			supplierBooking.JSB_DetailedGoodsDescription = "Detailed Good Desc";
			supplierBooking.JSB_MarksAndNumbers = "Marks & Numbers";
			supplierBooking.JSB_IncoTerm = Core.Constants.IncoTerms.ExWorks;
			supplierBooking.SupplierAddress.OrganisationPK = supplier.PK;
			supplierBooking.SupplierAddress.E2_OA_Address = supplier.Addresses[0].PK;
			supplierBooking.SupplierAddress.ContactPK = supplier.Contacts[0].PK;
			supplierBooking.ControllingCustomerAddress.OrganisationPK = controllingCustomer.PK;
			supplierBooking.ControllingCustomerAddress.E2_OA_Address = controllingCustomer.Addresses[0].PK;
			supplierBooking.ControllingCustomerAddress.ContactPK = controllingCustomer.Contacts[0].PK;
			supplierBooking.LocalCartageCFSAddress.OrganisationPK = localCartageCFS.PK;
			supplierBooking.LocalCartageCFSAddress.E2_OA_Address = localCartageCFS.Addresses[0].PK;
			supplierBooking.LocalCartageCFSAddress.ContactPK = localCartageCFS.Contacts[0].PK;
			supplierBooking.ConsigneeDocumentaryAddress.OrganisationPK = consignee.PK;
			supplierBooking.ConsigneeDocumentaryAddress.E2_OA_Address = consignee.Addresses[0].PK;
			supplierBooking.ConsigneeDocumentaryAddress.ContactPK = consignee.Contacts[0].PK;

			supplierBooking.SetUserDefinedValue("STR1", new ZString("ME TOO"));
			supplierBooking.SetUserDefinedValue("DAT1", new ZDateTime(2022, 2, 27));
			supplierBooking.SetUserDefinedValue("DEC1", new ZDecimal(12.34m));
			supplierBooking.SetUserDefinedValue("INT1", new ZInt(12));

			var container = supplierBooking.PlannedContainers.AddNew();
			container.J1_RC = factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20FR")).PK;
			container.J1_ContainerCount = 3;

			var supplierBookingLine = supplierBooking.SupplierBookingLines.AddNew();
			supplierBookingLine.FillWithValidTestData();
			supplierBookingLine.JSL_JSB_Booking = supplierBooking.PK;
			supplierBookingLine.JSL_JO_OrderLine = orderLine.PK;
			supplierBookingLine.JSL_BookedQuantity = 7.1;
			supplierBookingLine.JSL_BookedPackages = 6;
			supplierBookingLine.JSL_Description = "Desc";
			supplierBookingLine.JSL_F3_NKBookedPackagesUnit = "PLT";
			supplierBookingLine.JSL_GrossWeight = 12;
			supplierBookingLine.JSL_GrossWeightUnit = "KG";
			supplierBookingLine.JSL_Volume = 15;
			supplierBookingLine.JSL_VolumeUnit = "M3";
			supplierBookingLine.JSL_MarksAndNumbers = "Line Marks&Num";
			supplierBookingLine.ManufacturerAddress.E2_OA_Address = manufacturer.MainAddress.PK;
			supplierBookingLine.JSL_BookingLineId = "JSL001";
			supplierBookingLine.JSL_RH_NKCommodityCode = "GEN";
			supplierBookingLine.JSL_ReceivedQuantity = 6;
			supplierBookingLine.JSL_ReceivedPackages = 5;
			supplierBookingLine.JSL_ReceivedWeight = 11;
			supplierBookingLine.JSL_ReceivedVolume = 14;
			supplierBookingLine.JSL_FirstReceiptDateUtc = new ZDateTime(2023, 10, 01);
			supplierBookingLine.JSL_LastReceiptDateUtc = new ZDateTime(2023, 10, 10);
			supplierBookingLine.JSL_ShipmentWindowStart = new ZDate(2023, 9, 1);
			supplierBookingLine.JSL_ShipmentWindowEnd = new ZDate(2023, 9, 3);
			supplierBookingLine.JSL_HarmonisedCode = "HC001";

			return supplierBooking;
		}
	}
}
