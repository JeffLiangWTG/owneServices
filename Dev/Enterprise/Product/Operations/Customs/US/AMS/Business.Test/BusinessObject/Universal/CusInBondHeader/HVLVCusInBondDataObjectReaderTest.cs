using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.AMS;
using Enterprise.Environment;
using Enterprise.eTail.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.US.AMS.Business.Universal.Testing
{
	public class HVLVCusInBondDataObjectReaderTest : TestCaseWithFactory
	{
		UniversalObjectFactory universalObjectFactory;
		TestErrorLogger logger;

		protected override void SetUp()
		{
			base.SetUp();

			universalObjectFactory = new UniversalObjectFactory(Factory);
			logger = new TestErrorLogger();
		}

		public void TestHVLVCusInBondHeaderIsNVOCC()
		{
			var shipment = GetTestHVLVShipment();
			var universalShipment = GetTestHVLVShipmentDataObject(shipment);

			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			universalObjectFactory.FireCleanupAfterSaving();

			AssertEquals("Expected transit direction to be NVOCC", "N", header.BH_TransitDirection);
		}

		[TestDate(2020, 10, 16)]
		public void TestCusInBondHeaderMappings()
		{
			var shipment = GetTestHVLVShipment();

			var orgProxy = Factory.NewWithValidTestData<OrgHeader>();

			var code = orgProxy.CustomsCodes.AddNew();
			code.OK_CodeType = OrgCusCode.CodeTypes.CarrierCode;
			code.OK_RN_NKCodeCountry = CountryCodes.UnitedStates;
			code.OK_CustomsRegNo = "8CHN";

			var testBranch = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			testBranch.GB_OH_OrgProxy = orgProxy.PK;

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);

			using (Env.SetTemporaryUserContext(GlbStaff.CurrentUser.PK.ToGuid(), testBranch.PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
				var header = reader.ReadIntoBusinessObject();

				universalObjectFactory.FireCleanupAfterSaving();

				CombineAssertions(() =>
				{
					AssertEquals("BH_ParentID", shipment.PK, header.BH_ParentID);
					AssertEquals("BH_ParentTableCode", JobShipmentSchema.Constants.Prefix, header.BH_ParentTableCode);
					AssertEquals("BH_GB", testBranch.PK, header.BH_GB);
					AssertEquals("BH_ImportTransportMode", "11", header.BH_ImportTransportMode);
					AssertEquals("BH_CarrierSCAC", "8CHN", header.BH_CarrierSCAC);
					AssertEquals("BH_RL_NKImportLoadPort", "AUSYD", header.BH_RL_NKImportLoadPort);
					AssertEquals("BH_FirstExportDate", ZDate.Today, header.BH_FirstExportDate);
					AssertEquals("BH_RL_NKPortUnlading", "USCHI", header.BH_RL_NKPortUnlading);
					AssertEquals("BH_ETA", ZDate.Today.AddDays(1), header.BH_ETA);
					AssertEquals("BH_ImportConveyanceName", "TESTVESSEL", header.BH_ImportConveyanceName);
					AssertEquals("BH_VoyageNumber", "TESTVOYAGE", header.BH_VoyageNumber);
					AssertEquals("BH_ImportConveyanceCountry", "AU", header.BH_ImportConveyanceCountry);
				});
			}
		}

		public void TestCusInBondOceanBillMappings()
		{
			var shipment = GetTestHVLVShipment();
			var universalShipment = GetTestHVLVShipmentDataObject(shipment);

			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var oceanBill = header.OceanBill;
			AssertNotNull("Expected there to be an ocean bill on the CusInBondHeader", oceanBill);

			AssertEquals("Expected B0_BillStatus to be 'M'", "M", oceanBill.B0_BillStatus);
			AssertEquals("Expected B0_IssuerCode to be 'OTT1'", "OTT1", oceanBill.B0_IssuerCode);
			AssertEquals("Expected B0_MasterBillNumber to be '123456'", "123456", oceanBill.B0_MasterBillNumber);

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillsMappings()
		{
			var shipment = GetTestHVLVShipment();
			var universalShipment = GetTestHVLVShipmentDataObject(shipment);

			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to contain three bills", 3, header.Bills.Count);

			var bill1 = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");
			var bill2 = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 2");
			var smallBill = header.Bills.First(x => x.B0_MasterBillNumber == "SMALL CONSIGNMENT");
			AssertNotNull(bill1);
			AssertNotNull(bill2);
			AssertNotNull(smallBill);

			CombineAssertions(() =>
			{
				AssertEquals("1st Bill B0_IssuerCode", "OTT1", bill1.B0_IssuerCode);
				AssertEquals("1st Bill B0_BillStatus", "N", bill1.B0_BillStatus);
				AssertEquals("1st Bill B0_RL_NKPortOfLading", "AUSYD", bill1.B0_RL_NKPortOfLading);
				AssertEquals("1st Bill B0_PlaceOfReceipt", "SYDNEY", bill1.B0_PlaceOfReceipt);
				AssertEquals("1st Bill B0_RL_NKLastForeignPort", "AUSYD", bill1.B0_RL_NKLastForeignPort);
				AssertEquals("1st Bill B0_RL_NKForeignPortOfContract", "AUSYD", bill1.B0_RL_NKForeignPortOfContract);
				AssertEquals("1st Bill B0_ManifestQty", 3, bill1.B0_ManifestQty);
				AssertEquals("1st Bill B0_ManifestUQ", AMSConstants.PackageType.Pieces, bill1.B0_ManifestUQ);
				AssertEquals("1st Bill B0_Weight", 140m, bill1.B0_Weight);
				AssertEquals("1st Bill B0_WeightUQ", "KG", bill1.B0_WeightUQ);
				AssertEquals("1st Bill B0_Volume", 60m, bill1.B0_Volume);
				AssertEquals("1st Bill B0_VolumeUQ", "M3", bill1.B0_VolumeUQ);

				AssertEquals("2nd Bill B0_IssuerCode", "OTT1", bill2.B0_IssuerCode);
				AssertEquals("2nd Bill B0_BillStatus", "N", bill2.B0_BillStatus);
				AssertEquals("2nd Bill B0_RL_NKPortOfLading", "AUSYD", bill2.B0_RL_NKPortOfLading);
				AssertEquals("2nd Bill B0_PlaceOfReceipt", "SYDNEY", bill2.B0_PlaceOfReceipt);
				AssertEquals("2nd Bill B0_RL_NKLastForeignPort", "AUSYD", bill2.B0_RL_NKLastForeignPort);
				AssertEquals("2nd Bill B0_RL_NKForeignPortOfContract", "AUSYD", bill2.B0_RL_NKForeignPortOfContract);
				AssertEquals("2nd Bill B0_ManifestQty", 1, bill2.B0_ManifestQty);
				AssertEquals("2nd Bill B0_ManifestUQ", AMSConstants.PackageType.Pieces, bill2.B0_ManifestUQ);
				AssertEquals("2nd Bill B0_Weight", 40m, bill2.B0_Weight);
				AssertEquals("2nd Bill B0_WeightUQ", "KG", bill2.B0_WeightUQ);
				AssertEquals("2nd Bill B0_Volume", 40m, bill2.B0_Volume);
				AssertEquals("2nd Bill B0_VolumeUQ", "M3", bill2.B0_VolumeUQ);

				AssertEquals("Small Bill B0_IssuerCode", "OTT1", smallBill.B0_IssuerCode);
				AssertEquals("Small Bill B0_BillStatus", "N", smallBill.B0_BillStatus);
				AssertEquals("Small Bill B0_RL_NKPortOfLading", "AUSYD", smallBill.B0_RL_NKPortOfLading);
				AssertEquals("Small Bill B0_PlaceOfReceipt", "SYDNEY", smallBill.B0_PlaceOfReceipt);
				AssertEquals("Small Bill B0_RL_NKLastForeignPort", "AUSYD", smallBill.B0_RL_NKLastForeignPort);
				AssertEquals("Small Bill B0_RL_NKForeignPortOfContract", "AUSYD", smallBill.B0_RL_NKForeignPortOfContract);
				AssertEquals("Small Bill B0_ManifestQty", 1, smallBill.B0_ManifestQty);
				AssertEquals("Small Bill B0_ManifestUQ", AMSConstants.PackageType.Pieces, smallBill.B0_ManifestUQ);
				AssertEquals("Small Bill B0_Weight should be rounded to 1", 1m, smallBill.B0_Weight);
				AssertEquals("Small Bill B0_WeightUQ", "KG", smallBill.B0_WeightUQ);
				AssertEquals("Small Bill B0_Volume should be rounded to 1", 1m, smallBill.B0_Volume);
				AssertEquals("Small Bill B0_VolumeUQ", "M3", smallBill.B0_VolumeUQ);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillMappings_ExcludeSubShipmentNotFromConsignment()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = "HVL";

			var consignmentHeader = Factory.NewWithValidTestData<HVLVConsignmentHeader>();
			consignmentHeader.HCH_JS_Shipment = shipment.PK;

			var consignment = consignmentHeader.Consignments.AddNew();
			consignment.Items.AddNew();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);

			var subShipmentNotFromConsignment = new Shipment(DefaultDataObjectWriterStrategy.TestInstance);
			subShipmentNotFromConsignment.DataContext = DataContextFactory.New();
			subShipmentNotFromConsignment.DataContext.AddDataSource(DataContextType.InBond, "INBOND001");

			var shipmentDataObject = universalShipment.SubShipmentCollection[0];
			shipmentDataObject.SubShipmentCollection.Add(subShipmentNotFromConsignment);

			CombineAssertions("pre-condition", () =>
			{
				AssertEquals("Shipment data object contains 2 subshipments", 2, shipmentDataObject.SubShipmentCollection.Count);
				AssertContainsExactElementsInAnyOrder("1 subshipment has HVLVConsignment as data source, while other subshipment has InBond as data source",
					new ZString[] { nameof(DataContextType.HVLVConsignment), nameof(DataContextType.InBond) },
					shipmentDataObject.SubShipmentCollection.Select(subShipment => subShipment.DataContext.DataSourceCollection.First().Type));
			});

			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			AssertEquals("Expected to contain only 1 bill", 1, header.Bills.Count);
		}

		public void TestCusInBondBillForeignShipperMappings_WhenShipperIsOrganization()
		{
			var shipment = GetTestHVLVShipment();

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "TSTSH";
			shipper.OH_FullName = "Test Shipper";

			var address = shipper.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";

			var contact = shipper.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";

			var consignment = shipment.HVLVConsignments.FirstOrDefault(c => c.HVC_WaybillNumber == "Test Waybill 1") as HVLVConsignment;
			consignment.HVC_OA_ShipperAddress = address.PK;
			consignment.HVC_ShipperContact = contact.OC_ContactName;

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			AssertNotNull("Expected one CusInBondBill with master bill number 'TEST WAYBILL 1'", bill);

			CombineAssertions(() =>
			{
				AssertEquals("Expected 'No.123 abc street' for shipper address1", "No.123 abc street", bill.ForeignShipper.Address1);
				AssertEquals("Expected 'Test Contact' for shipper contact", "Test Contact", bill.ForeignShipper.E2_Contact);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillForeignShipperMappings_WhenShipperIsNotOrganization()
		{
			var shipment = GetTestHVLVShipment();

			var consignment = shipment.HVLVConsignments.FirstOrDefault(c => c.HVC_WaybillNumber == "Test Waybill 1") as HVLVConsignment;
			consignment.HVC_ShipperName = "AMAZON";
			consignment.HVC_ShipperContact = "Test Contact";
			consignment.HVC_ShipperAddress1 = "No.123 abc street";
			consignment.HVC_ShipperAddress2 = "12345";
			consignment.HVC_ShipperCity = "ALASKA";
			consignment.HVC_ShipperPostcode = "123";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_RN_NKShipperCountryCode = "US";

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			AssertNotNull("Expected one CusInBondBill with master bill number 'TEST WAYBILL 1'", bill);

			CombineAssertions(() =>
			{
				AssertEquals("AMAZON", bill.ForeignShipper.E2_CompanyName);
				AssertEquals("Test Contact", bill.ForeignShipper.E2_Contact);
				AssertEquals("No.123 abc street", bill.ForeignShipper.Address1);
				AssertEquals("12345", bill.ForeignShipper.Address2);
				AssertEquals("ALASKA", bill.ForeignShipper.City);
				AssertEquals("123", bill.ForeignShipper.Postcode);
				AssertEquals("NSW", bill.ForeignShipper.State);
				AssertEquals("US", bill.ForeignShipper.Country.Code);
				Assert(bill.ForeignShipper.E2_AddressOverride);
				AssertEquals(AddressValidationStatus.ManuallyVerified, bill.ForeignShipper.E2_ValidationStatus);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillForeignShipperMappings_WhenShipperIsNotOrganization_SyncUpdatesShipper()
		{
			var shipment = GetTestHVLVShipment();

			var consignment = shipment.HVLVConsignments.First() as HVLVConsignment;
			consignment.HVC_ShipperName = "AMAZON";
			consignment.HVC_ShipperContact = "Test Contact";
			consignment.HVC_ShipperAddress1 = "No.123 abc street";
			consignment.HVC_ShipperAddress2 = "12345";
			consignment.HVC_ShipperCity = "ALASKA";
			consignment.HVC_ShipperPostcode = "123";
			consignment.HVC_ShipperState = "NSW";
			consignment.HVC_RN_NKShipperCountryCode = "US";

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();
			Factory.Save();
			var shipperPK = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1").DocAddresses.FindDocAddressesByType(DocAddressType.ForeignShipperDocumentaryAddress).Single().PK;

			var shipmentParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, header.BH_JobReference)
				};
			shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());
			Factory.Save();

			// update with a shipper that is not an organization:
			consignment.HVC_ShipperName = "AMAZON Update";
			consignment.HVC_ShipperContact = "Test Contact Update";
			consignment.HVC_ShipperAddress1 = "No.123 update street";
			consignment.HVC_ShipperAddress2 = "update";
			consignment.HVC_ShipperCity = "update city";
			consignment.HVC_ShipperPostcode = "987";
			consignment.HVC_ShipperState = "VIC";
			consignment.HVC_RN_NKShipperCountryCode = "CA";
			Factory.Save();

			universalShipment = GetTestHVLVShipmentDataObject(shipment);
			reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var headerAfterSync = reader.ReadIntoBusinessObject();
			AssertEquals("Precondition: header has been matched for update", header.PK, headerAfterSync.PK);

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			CombineAssertions("After update when updated Shipper is not organization", () =>
			{
				AssertEquals("Should not create new DocAddress", 1, bill.DocAddresses.FindDocAddressesByType(DocAddressType.ForeignShipperDocumentaryAddress).Length);
				AssertEquals("JobDocAddress should be same PK", shipperPK, bill.ForeignShipper.PK);
				AssertEquals("AMAZON Update", bill.ForeignShipper.E2_CompanyName);
				AssertEquals("Test Contact Update", bill.ForeignShipper.E2_Contact);
				AssertEquals("No.123 update street", bill.ForeignShipper.Address1);
				AssertEquals("update", bill.ForeignShipper.Address2);
				AssertEquals("update city", bill.ForeignShipper.City);
				AssertEquals("987", bill.ForeignShipper.Postcode);
				AssertEquals("VIC", bill.ForeignShipper.State);
				AssertEquals("CA", bill.ForeignShipper.Country.Code);
				Assert(bill.ForeignShipper.E2_AddressOverride);
				AssertEquals(AddressValidationStatus.ManuallyVerified, bill.ForeignShipper.E2_ValidationStatus);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillForeignShipperMappings_WhenShipperIsOrganization_SyncUpdatesShipper()
		{
			var shipment = GetTestHVLVShipment();

			var shipper = Factory.New<OrgHeader>();
			shipper.OH_Code = "TSTCN";
			shipper.OH_FullName = "Test shipper";

			var address = shipper.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";

			var contact = shipper.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";

			var consignment = shipment.HVLVConsignments.FirstOrDefault(c => c.HVC_WaybillNumber == "Test Waybill 1") as HVLVConsignment;
			consignment.HVC_OA_ShipperAddress = address.PK;
			consignment.HVC_ShipperContact = contact.OC_ContactName;

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();
			Factory.Save();

			var shipperPK = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1").DocAddresses.FindDocAddressesByType(DocAddressType.ForeignShipperDocumentaryAddress).Single().PK;

			var shipmentParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, header.BH_JobReference)
				};
			shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());
			Factory.Save();

			// update with a shipper is not an organization:
			consignment.HVC_OA_ShipperAddress = ZGuid.Empty;
			consignment.HVC_ShipperName = "AMAZON Update";
			consignment.HVC_ShipperContact = "Test Contact Update";
			consignment.HVC_ShipperAddress1 = "No.123 update street";
			consignment.HVC_ShipperAddress2 = "update";
			consignment.HVC_ShipperCity = "update city";
			consignment.HVC_ShipperPostcode = "987";
			consignment.HVC_ShipperState = "VIC";
			consignment.HVC_RN_NKShipperCountryCode = "CA";
			Factory.Save();

			universalShipment = GetTestHVLVShipmentDataObject(shipment);
			reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var headerAfterSync = reader.ReadIntoBusinessObject();
			AssertEquals("Precondition: header has been matched for update", header.PK, headerAfterSync.PK);

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			CombineAssertions("After update when updated shipper is not organization", () =>
			{
				AssertEquals("Should not create new DocAddress", 1, bill.DocAddresses.FindDocAddressesByType(DocAddressType.ForeignShipperDocumentaryAddress).Length);
				AssertEquals("JobDocAddress should be same PK", shipperPK, bill.ForeignShipper.PK);
				AssertEquals("AMAZON Update", bill.ForeignShipper.E2_CompanyName);
				AssertEquals("Test Contact Update", bill.ForeignShipper.E2_Contact);
				AssertEquals("No.123 update street", bill.ForeignShipper.Address1);
				AssertEquals("update", bill.ForeignShipper.Address2);
				AssertEquals("update city", bill.ForeignShipper.City);
				AssertEquals("987", bill.ForeignShipper.Postcode);
				AssertEquals("VIC", bill.ForeignShipper.State);
				AssertEquals("CA", bill.ForeignShipper.Country.Code);
				Assert(bill.ForeignShipper.E2_AddressOverride);
				AssertEquals(AddressValidationStatus.ManuallyVerified, bill.ForeignShipper.E2_ValidationStatus);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillConsigneeMappings_WhenConsigneeIsOrganization()
		{
			var shipment = GetTestHVLVShipment();

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TSTCN";
			consignee.OH_FullName = "Test Consignee";

			var address = consignee.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";

			var contact = consignee.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";

			var consignment = shipment.HVLVConsignments.FirstOrDefault(c => c.HVC_WaybillNumber == "Test Waybill 1") as HVLVConsignment;
			consignment.HVC_OA_ConsigneeAddress = address.PK;
			consignment.HVC_ConsigneeContact = contact.OC_ContactName;

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			AssertNotNull("Expected one CusInBondBill with master bill number 'TEST WAYBILL 1'", bill);

			CombineAssertions(() =>
			{
				AssertEquals("Expected 'No.123 abc street' for consignee address1", "No.123 abc street", bill.Consignee.Address1);
				AssertEquals("Expected 'Test Contact' for consignee contact", "Test Contact", bill.Consignee.E2_Contact);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillConsigneeMappings_WhenConsigneeIsNotOrganization()
		{
			var shipment = GetTestHVLVShipment();

			var consignment = shipment.HVLVConsignments.First() as HVLVConsignment;
			consignment.HVC_ConsigneeName = "AMAZON";
			consignment.HVC_ConsigneeContact = "Test Contact";
			consignment.HVC_ConsigneeAddress1 = "No.123 abc street";
			consignment.HVC_ConsigneeAddress2 = "12345";
			consignment.HVC_ConsigneeCity = "ALASKA";
			consignment.HVC_ConsigneePostcode = "123";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_RN_NKConsigneeCountryCode = "US";

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			AssertNotNull("Expected one CusInBondBill with master bill number 'TEST WAYBILL 1'", bill);

			CombineAssertions(() =>
			{
				AssertEquals("AMAZON", bill.Consignee.E2_CompanyName);
				AssertEquals("Test Contact", bill.Consignee.E2_Contact);
				AssertEquals("No.123 abc street", bill.Consignee.Address1);
				AssertEquals("12345", bill.Consignee.Address2);
				AssertEquals("ALASKA", bill.Consignee.City);
				AssertEquals("123", bill.Consignee.Postcode);
				AssertEquals("NSW", bill.Consignee.State);
				AssertEquals("US", bill.Consignee.Country.Code);
				Assert(bill.Consignee.E2_AddressOverride);
				AssertEquals(AddressValidationStatus.ManuallyVerified, bill.Consignee.E2_ValidationStatus);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillConsigneeMappings_WhenConsigneeIsNotOrganization_SyncUpdatesConsignee()
		{
			var shipment = GetTestHVLVShipment();

			var consignment = shipment.HVLVConsignments.First() as HVLVConsignment;
			consignment.HVC_ConsigneeName = "AMAZON";
			consignment.HVC_ConsigneeContact = "Test Contact";
			consignment.HVC_ConsigneeAddress1 = "No.123 abc street";
			consignment.HVC_ConsigneeAddress2 = "12345";
			consignment.HVC_ConsigneeCity = "ALASKA";
			consignment.HVC_ConsigneePostcode = "123";
			consignment.HVC_ConsigneeState = "NSW";
			consignment.HVC_RN_NKConsigneeCountryCode = "US";

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();
			Factory.Save();
			var consigneePK = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1").DocAddresses.FindDocAddressesByType(DocAddressType.ConsigneeAddress).Single().PK;

			var shipmentParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, header.BH_JobReference)
				};
			shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());
			Factory.Save();

			// update with a consignee that is not an organization:
			consignment.HVC_ConsigneeName = "AMAZON Update";
			consignment.HVC_ConsigneeContact = "Test Contact Update";
			consignment.HVC_ConsigneeAddress1 = "No.123 update street";
			consignment.HVC_ConsigneeAddress2 = "update";
			consignment.HVC_ConsigneeCity = "update city";
			consignment.HVC_ConsigneePostcode = "987";
			consignment.HVC_ConsigneeState = "VIC";
			consignment.HVC_RN_NKConsigneeCountryCode = "CA";
			Factory.Save();

			universalShipment = GetTestHVLVShipmentDataObject(shipment);
			reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var headerAfterSync = reader.ReadIntoBusinessObject();
			AssertEquals("Precondition: header has been matched for update", header.PK, headerAfterSync.PK);

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			CombineAssertions("After update when updated consignee is not organization", () =>
			{
				AssertEquals("Should not create new DocAddress", 1, bill.DocAddresses.FindDocAddressesByType(DocAddressType.ConsigneeAddress).Length);
				AssertEquals("JobDocAddress should be same PK", consigneePK, bill.Consignee.PK);
				AssertEquals("AMAZON Update", bill.Consignee.E2_CompanyName);
				AssertEquals("Test Contact Update", bill.Consignee.E2_Contact);
				AssertEquals("No.123 update street", bill.Consignee.Address1);
				AssertEquals("update", bill.Consignee.Address2);
				AssertEquals("update city", bill.Consignee.City);
				AssertEquals("987", bill.Consignee.Postcode);
				AssertEquals("VIC", bill.Consignee.State);
				AssertEquals("CA", bill.Consignee.Country.Code);
				Assert(bill.Consignee.E2_AddressOverride);
				AssertEquals(AddressValidationStatus.ManuallyVerified, bill.Consignee.E2_ValidationStatus);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillConsigneeMappings_WhenConsigneeIsOrganization_SyncUpdatesConsignee()
		{
			var shipment = GetTestHVLVShipment();

			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "TSTCN";
			consignee.OH_FullName = "Test Consignee";

			var address = consignee.Addresses.AddNew();
			address.OA_Address1 = "No.123 abc street";

			var contact = consignee.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Test Contact";

			var consignment = shipment.HVLVConsignments.FirstOrDefault(c => c.HVC_WaybillNumber == "Test Waybill 1") as HVLVConsignment;
			consignment.HVC_OA_ConsigneeAddress = address.PK;
			consignment.HVC_ConsigneeContact = contact.OC_ContactName;

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();
			Factory.Save();

			var consigneePK = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1").DocAddresses.FindDocAddressesByType(DocAddressType.ConsigneeAddress).Single().PK;

			var shipmentParameters = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea),
					new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, header.BH_JobReference)
				};
			shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());
			Factory.Save();

			// update with a consignee that is not an organization:
			consignment.HVC_OA_ConsigneeAddress = ZGuid.Empty;
			consignment.HVC_ConsigneeName = "AMAZON Update";
			consignment.HVC_ConsigneeContact = "Test Contact Update";
			consignment.HVC_ConsigneeAddress1 = "No.123 update street";
			consignment.HVC_ConsigneeAddress2 = "update";
			consignment.HVC_ConsigneeCity = "update city";
			consignment.HVC_ConsigneePostcode = "987";
			consignment.HVC_ConsigneeState = "VIC";
			consignment.HVC_RN_NKConsigneeCountryCode = "CA";
			Factory.Save();

			universalShipment = GetTestHVLVShipmentDataObject(shipment);
			reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var headerAfterSync = reader.ReadIntoBusinessObject();
			AssertEquals("Precondition: header has been matched for update", header.PK, headerAfterSync.PK);

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			CombineAssertions("After update when updated consignee is not organization", () =>
			{
				AssertEquals("Should not create new DocAddress", 1, bill.DocAddresses.FindDocAddressesByType(DocAddressType.ConsigneeAddress).Length);
				AssertEquals("JobDocAddress should be same PK", consigneePK, bill.Consignee.PK);
				AssertEquals("AMAZON Update", bill.Consignee.E2_CompanyName);
				AssertEquals("Test Contact Update", bill.Consignee.E2_Contact);
				AssertEquals("No.123 update street", bill.Consignee.Address1);
				AssertEquals("update", bill.Consignee.Address2);
				AssertEquals("update city", bill.Consignee.City);
				AssertEquals("987", bill.Consignee.Postcode);
				AssertEquals("VIC", bill.Consignee.State);
				AssertEquals("CA", bill.Consignee.Country.Code);
				Assert(bill.Consignee.E2_AddressOverride);
				AssertEquals(AddressValidationStatus.ManuallyVerified, bill.Consignee.E2_ValidationStatus);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondBillNotifyPartyMappings()
		{
			var shipment = GetTestHVLVShipment();

			var notifyParty = Factory.New<OrgHeader>();
			notifyParty.OH_Code = "TSTCN";
			notifyParty.OH_FullName = "Test Party";

			var address = notifyParty.Addresses.MainAddress;
			address.OA_Address1 = "No.123 NP Street";

			var contact = notifyParty.Contacts.AddNew();
			contact.OC_OA_OrgAddress = address.PK;
			contact.OC_ContactName = "Notify Party Contact";

			shipment.NotifyPartyDocumentaryAddress.E2_OA_Address = address.PK;
			shipment.NotifyPartyDocumentaryAddress.E2_Contact = contact.OC_ContactName;

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			AssertNotNull("Expected one CusInBondBill with master bill number 'TEST WAYBILL 1'", bill);

			CombineAssertions(() =>
			{
				AssertEquals("Expected 'No.123 NP Street' for NotifyParty address1", "No.123 NP Street", bill.NotifyParty1.Address1);
				AssertEquals("Expected 'Notify Party Contact' for NotifyParty contact", "Notify Party Contact", bill.NotifyParty1.E2_Contact);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestCusInBondContainerMappings()
		{
			var shipment = GetTestHVLVShipment();

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill1 = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");
			var bill2 = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 2");
			var smallBill = header.Bills.First(x => x.B0_MasterBillNumber == "SMALL CONSIGNMENT");

			AssertEquals("Expected to contain 2 containers in 1st bill", 2, bill1.MovementDetail.Containers.Count);
			AssertEquals("Expected to contain 1 container in 2nd bill", 1, bill2.MovementDetail.Containers.Count);
			AssertEquals("Expected to contain 1 container in small bill", 1, smallBill.MovementDetail.Containers.Count);

			var container1 = bill1.MovementDetail.Containers.First(x => x.BC_ContainerNum == "TESTCONTAINER 1");
			var container2 = bill1.MovementDetail.Containers.First(x => x.BC_ContainerNum == "TESTCONTAINER 2");
			var container3 = bill2.MovementDetail.Containers.First(x => x.BC_ContainerNum == "TESTCONTAINER 2");
			var container4 = smallBill.MovementDetail.Containers.First(x => x.BC_ContainerNum == "TESTCONTAINER 2");

			CombineAssertions(() =>
			{
				AssertEquals("container1 RC_Code", "20XX", container1.Container.RC_Code);
				AssertEquals("container1 BC_Seal1", "TESTSEAL", container1.BC_Seal1);
				AssertEquals("container1 BC_TypeOfService", "CY", container1.BC_TypeOfService);

				AssertEquals("container2 RC_Code", "20XY", container2.Container.RC_Code);
				AssertEquals("container2 BC_Seal1", "TESTSEAL", container2.BC_Seal1);
				AssertEquals("container2 BC_TypeOfService", "CY", container2.BC_TypeOfService);

				AssertEquals("container3 RC_Code", "20XY", container3.Container.RC_Code);
				AssertEquals("container3 BC_Seal1", "TESTSEAL", container3.BC_Seal1);
				AssertEquals("container3 BC_TypeOfService", "CY", container3.BC_TypeOfService);

				AssertEquals("container4 RC_Code", "20XY", container4.Container.RC_Code);
				AssertEquals("container4 BC_Seal1", "TESTSEAL", container4.BC_Seal1);
				AssertEquals("container4 BC_TypeOfService", "CY", container4.BC_TypeOfService);
			});

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestPopulateContainer_StopWhenCannotFindAnyContainerInConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_HouseBill = "HB001";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WaybillNumber = "TEST WAYBILL 1";
			consignment.HVC_ManifestedWeight = 10;
			consignment.HVC_WeightUQ = "KG";
			consignment.HVC_ManifestedVolume = 10;
			consignment.HVC_VolumeUQ = "M3";

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			item.HVI_F3_NKPackType = PkgUnit.Piece;
			item.HVI_ManifestedWeight = 10;
			item.HVI_ManifestedVolume = 10;

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			AssertEquals("Expected not to populate any container", 0, bill.MovementDetail.Containers.Count);

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestPopulateContainer_RecreateCommoditiesWhenSyncingData()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "123456";

			var consolContainer = consol.Containers.AddNew();
			consolContainer.JC_ContainerNum = "TestContainer";

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WaybillNumber = "TEST WAYBILL";

			var item = consignment.Items.AddNew();
			item.HVI_CurrentBarcode = "NewTestItem";
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			item.HVI_ContainerNumber = "TestContainer";

			var itemLine = item.Lines.AddNew();
			itemLine.HVS_CustomsValue = 2m;
			itemLine.HVS_GrossWeight = 20m;
			itemLine.HVS_WeightUnit = "KG";
			itemLine.HVS_Quantity = 1;
			itemLine.HVS_RN_NKOriginCountryCode = "AU";

			var existingheader = Factory.NewWithValidTestData<CusInBondHeader>();
			existingheader.BH_ParentID = shipment.PK;
			existingheader.BH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			existingheader.BH_TransitDirection = "N";

			var existingBill = existingheader.Bills.AddNew();
			existingBill.B0_MasterBillNumber = "TEST WAYBILL";
			var existingContainer = existingBill.MovementDetail.Containers.AddNew();
			existingContainer.BC_ContainerNum = "TESTCONTAINER";

			var existingCommodity = existingContainer.Commodities.AddNew();
			existingCommodity.BY_MonetaryValue = 1;
			existingCommodity.BY_GrossWeight = 10m;
			existingCommodity.BY_PieceCount = 2;
			existingCommodity.BY_MarksAndNumbers = "TestNumber";

			Factory.Save();

			var shipmentParameters = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "AMS"),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.Mode, TransportModes.Sea),
				new KeyValuePair<string, string>(EventReferenceParameters.Codes.ReferenceNumber, existingheader.BH_JobReference)
			};
			shipment.Logs.AddNew(AutoEvents.Transferred, shipmentParameters.ToArray());

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.Single(x => x.B0_MasterBillNumber == "TEST WAYBILL");
			var commodities = bill.MovementDetail.Containers[0].Commodities;
			AssertEquals(1, commodities.Count);

			var commodity = commodities[0];
			CombineAssertions("Commodity details", () =>
			{
				AssertEquals("BY_MonetaryValue", 2m, commodity.BY_MonetaryValue);
				AssertEquals("BY_GrossWeight", 20m, commodity.BY_GrossWeight);
				AssertEquals("BY_PieceCount", 1, commodity.BY_PieceCount);
				AssertEquals("BY_MarksAndNumbers", "NEWTESTITEM", commodity.BY_MarksAndNumbers);
			});
		}

		public void TestCusInBondCommodityMappings()
		{
			using (HVLVDataRegistry.Instance.AutoGenerateConsignmentAndItemIDs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var shipment = GetTestHVLVShipment();
				Factory.Save();

				var universalShipment = GetTestHVLVShipmentDataObject(shipment);

				var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
				var header = reader.ReadIntoBusinessObject();

				var bill1 = header.Bills.Single(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");
				var bill2 = header.Bills.Single(x => x.B0_MasterBillNumber == "TEST WAYBILL 2");
				var smallBill = header.Bills.Single(x => x.B0_MasterBillNumber == "SMALL CONSIGNMENT");

				var container1 = bill1.MovementDetail.Containers.Single(x => x.BC_ContainerNum == "TESTCONTAINER 1");
				var container2 = bill1.MovementDetail.Containers.Single(x => x.BC_ContainerNum == "TESTCONTAINER 2");
				var container3 = bill2.MovementDetail.Containers.Single(x => x.BC_ContainerNum == "TESTCONTAINER 2");
				var container4 = smallBill.MovementDetail.Containers.Single(x => x.BC_ContainerNum == "TESTCONTAINER 2");

				CombineAssertions("Container commodity counts should be correct", () =>
				{
					AssertEquals("container1", 1, container1.Commodities.Count);
					AssertEquals("container2", 1, container2.Commodities.Count);
					AssertEquals("container3", 1, container3.Commodities.Count);
					AssertEquals("container4", 1, container4.Commodities.Count);
				});

				var commodity1 = container1.Commodities.Single();
				var commodity2 = container2.Commodities.Single();
				var commodity3 = container3.Commodities.Single();
				var smallCommodity = container4.Commodities.Single();

				CombineAssertions(() =>
				{
					AssertEquals("commodity1 BY_FormattedHarmonisedTariff", "1234.56", commodity1.BY_FormattedHarmonisedTariff);
					AssertEquals("commodity1 BY_MonetaryValue", 6m, commodity1.BY_MonetaryValue);
					AssertEquals("commodity1 BY_GrossWeight", 50m, commodity1.BY_GrossWeight);
					AssertEquals("commodity1 BY_GrossWeightUnit", "KG", commodity1.BY_GrossWeightUnit);
					AssertEquals("commodity1 BY_PieceCount", 2, commodity1.BY_PieceCount);
					AssertEquals("commodity1 BY_ManifestUnitCode", AMSConstants.PackageType.Pieces, commodity1.BY_ManifestUnitCode);
					AssertEquals("commodity1 BY_RN_NKCountryOfOrigin", "AU", commodity1.BY_RN_NKCountryOfOrigin);
					AssertEquals("commodity1 BY_MarksAndNumbers", "TESTITEM1,TESTITEM2", commodity1.BY_MarksAndNumbers);
					AssertEquals("commodity1 BY_Description", "TEST ITEM LINE 1A,TEST ITEM LINE 1B,1234.56,1234.56TEST ITEM LINE 2,6543.21", commodity1.BY_Description);

					AssertEquals("commodity2 BY_FormattedHarmonisedTariff", "1357.99", commodity2.BY_FormattedHarmonisedTariff);
					AssertEquals("commodity2 BY_MonetaryValue", 9m, commodity2.BY_MonetaryValue);
					AssertEquals("commodity2 BY_GrossWeight", 90m, commodity2.BY_GrossWeight);
					AssertEquals("commodity2 BY_GrossWeightUnit", "KG", commodity2.BY_GrossWeightUnit);
					AssertEquals("commodity2 BY_PieceCount", 1, commodity2.BY_PieceCount);
					AssertEquals("commodity2 BY_ManifestUnitCode", AMSConstants.PackageType.Pieces, commodity2.BY_ManifestUnitCode);
					AssertEquals("commodity2 BY_RN_NKCountryOfOrigin", "AU", commodity2.BY_RN_NKCountryOfOrigin);
					AssertEquals("commodity2 BY_MarksAndNumbers", "TESTITEM3", commodity2.BY_MarksAndNumbers);

					AssertEquals("commodity3 BY_FormattedHarmonisedTariff", "2468.00", commodity3.BY_FormattedHarmonisedTariff);
					AssertEquals("commodity3 BY_MonetaryValue", 4m, commodity3.BY_MonetaryValue);
					AssertEquals("commodity3 BY_GrossWeight", 40m, commodity3.BY_GrossWeight);
					AssertEquals("commodity3 BY_GrossWeightUnitt", "KG", commodity3.BY_GrossWeightUnit);
					AssertEquals("commodity3 BY_PieceCount", 1, commodity3.BY_PieceCount);
					AssertEquals("commodity3 BY_ManifestUnitCode", AMSConstants.PackageType.Pieces, commodity3.BY_ManifestUnitCode);
					AssertEquals("commodity3 BY_RN_NKCountryOfOrigin", "AU", commodity3.BY_RN_NKCountryOfOrigin);
					AssertEquals("commodity3 BY_MarksAndNumbers", "TESTITEM4", commodity3.BY_MarksAndNumbers);

					AssertEquals("commodity4 BY_FormattedHarmonisedTariff", "2468.00", smallCommodity.BY_FormattedHarmonisedTariff);
					AssertEquals("commodity4 BY_MonetaryValue should be rounded to 1", 1m, smallCommodity.BY_MonetaryValue);
					AssertEquals("commodity4 BY_GrossWeight should be rounded to 1", 1m, smallCommodity.BY_GrossWeight);
					AssertEquals("commodity4 BY_GrossWeightUnitt", "KG", smallCommodity.BY_GrossWeightUnit);
					AssertEquals("commodity4 BY_PieceCount", 1, smallCommodity.BY_PieceCount);
					AssertEquals("commodity4 BY_ManifestUnitCode", AMSConstants.PackageType.Pieces, smallCommodity.BY_ManifestUnitCode);
					AssertEquals("commodity4 BY_RN_NKCountryOfOrigin", "AU", smallCommodity.BY_RN_NKCountryOfOrigin);
					AssertEquals("commodity4 BY_MarksAndNumbers", "SMALLITEM", smallCommodity.BY_MarksAndNumbers);
				});

				universalObjectFactory.FireCleanupAfterSaving();
			}
		}

		public void TestCusInBondCommodityDescription_ShouldTruncateToMaxLength()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_ContainerNum = "TestContainer";
			container.JC_SealNum = "TestSeal";
			container.JC_DeliveryMode = "CY/CY";

			var refContainer = container.RefContainer_List.AddNew();
			refContainer.RC_Code = "20XX";
			container.JC_RC = refContainer.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_HouseBill = "HB001";

			var consignment = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment.HVC_WaybillNumber = "Test Waybill";

			var item = consignment.Items.AddNew();
			item.HVI_JS_LoadedOnShipment = shipment.PK;
			item.HVI_F3_NKPackType = PkgUnit.Piece;
			item.HVI_ContainerNumber = "TestContainer";

			for (var i = 0; i < 40; i++)
			{
				var itemline = item.Lines.AddNew();
				itemline.HVS_GoodsDescription = string.Concat(Enumerable.Repeat("A", HVLVItemLineSchema.HVS_GoodsDescription.MaxLength));
				itemline.HVS_Quantity = 1;
			}

			Factory.Save();

			var universalShipment = GetTestHVLVShipmentDataObject(shipment);
			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.Single(x => x.B0_MasterBillNumber == "TEST WAYBILL");
			var commodity = bill.MovementDetail.Containers.Single(x => x.BC_ContainerNum == "TESTCONTAINER").Commodities.Single();

			AssertEquals("HVS_GoodsDescription are added together and will exceed BY_Description, so we truncate", CusInBondCargoDescSchema.BY_Description.MaxLength, commodity.BY_Description.Length);

			universalObjectFactory.FireCleanupAfterSaving();
		}

		public void TestJobDocAddress_FromHVLVShipment_ValidationStatusIsMAN()
		{
			var shipment = GetTestHVLVShipment();
			var universalShipment = GetTestHVLVShipmentDataObject(shipment);

			var consignment = shipment.HVLVConsignments.First() as HVLVConsignment;
			consignment.HVC_ConsigneeAddress1 = "No.123 abc street";
			consignment.HVC_ConsigneeContact = "Test Contact";
			consignment.HVC_ShipperAddress1 = "No.456 edf street";
			consignment.HVC_ShipperContact = "Test Contact";

			var reader = new HVLVCusInBondHeaderDataObjectReader(universalShipment, universalShipment.SubShipmentCollection[0], shipment, logger, universalObjectFactory);
			var header = reader.ReadIntoBusinessObject();

			var bill = header.Bills.First(x => x.B0_MasterBillNumber == "TEST WAYBILL 1");

			AssertNotNull(bill);
			AssertEquals(AddressValidationStatus.ManuallyVerified, bill.ForeignShipper.E2_ValidationStatus);
			AssertEquals(AddressValidationStatus.ManuallyVerified, bill.Consignee.E2_ValidationStatus);

			universalObjectFactory.FireCleanupAfterSaving();
		}

		ForwardingShipment GetTestHVLVShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_MasterBillNum = "123456";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USCHI";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerMode = ContainerModes.FCL;
			container1.JC_ContainerNum = "TestContainer 1";
			container1.JC_SealNum = "TestSeal";
			container1.JC_DeliveryMode = "CY/CY";

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerMode = ContainerModes.FCL;
			container2.JC_ContainerNum = "TestContainer 2";
			container2.JC_SealNum = "TestSeal";
			container2.JC_DeliveryMode = "CY/CY";

			var refContainer1 = container1.RefContainer_List.AddNew();
			refContainer1.RC_Code = "20XX";
			container1.JC_RC = refContainer1.PK;

			var refContainer2 = container2.RefContainer_List.AddNew();
			refContainer2.RC_Code = "20XY";
			container2.JC_RC = refContainer2.PK;

			var transport = consol.Transports[0];
			transport.JW_TransportMode = TransportModes.Sea;
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "USCHI";
			transport.JW_ETD = ZDate.Today;
			transport.JW_ETA = ZDate.Today.AddDays(1);
			transport.JW_Vessel = "TestVessel";
			transport.JW_VoyageFlight = "TestVoyage";

			var masterBillIssuingParty = Factory.New<OrgHeader>();
			masterBillIssuingParty.OH_Code = "AAA";
			masterBillIssuingParty.OH_FullName = "BBB";
			masterBillIssuingParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", CountryCodes.UnitedStates);
			consol.MasterBillIssuingPartyDocumentaryAddress.OrganisationPK = masterBillIssuingParty.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_ShipmentType = ShipmentTypes.HighVolumeLowValue;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USCHI";
			shipment.JS_HouseBill = "HB001";

			var houseBillIssuingParty = Factory.New<OrgHeader>();
			houseBillIssuingParty.OH_Code = "CCC";
			houseBillIssuingParty.OH_FullName = "DDD";
			houseBillIssuingParty.CustomsCodes.UpdateOrAddCustomsCodesIfNoneExists(OrgCusCode.CodeTypes.CarrierCode, "OTT1", CountryCodes.UnitedStates);
			shipment.HouseBillIssuingPartyDocumentaryAddress.OrganisationPK = houseBillIssuingParty.PK;

			var consignment1 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment1.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment1.HVC_WaybillNumber = "Test Waybill 1";
			consignment1.HVC_ManifestedWeight = 10;
			consignment1.HVC_WeightUQ = "KG";
			consignment1.HVC_ManifestedVolume = 10;
			consignment1.HVC_VolumeUQ = "M3";

			var consignment2 = Factory.NewWithValidTestData<HVLVConsignment>();
			consignment2.HVC_JS_ManifestedOnShipment = shipment.PK;
			consignment2.HVC_WaybillNumber = "Test Waybill 2";
			consignment2.HVC_ManifestedWeight = 20;
			consignment2.HVC_WeightUQ = "KG";
			consignment2.HVC_ManifestedVolume = 20;
			consignment2.HVC_VolumeUQ = "M3";

			var smallConsignment = Factory.NewWithValidTestData<HVLVConsignment>();
			smallConsignment.HVC_JS_ManifestedOnShipment = shipment.PK;
			smallConsignment.HVC_WaybillNumber = "Small Consignment";
			smallConsignment.HVC_ManifestedWeight = 0.1m;
			smallConsignment.HVC_WeightUQ = "KG";
			smallConsignment.HVC_ManifestedVolume = 0.1m;
			smallConsignment.HVC_VolumeUQ = "M3";

			var item1 = consignment1.Items.AddNew();
			item1.HVI_CurrentBarcode = "TestItem1";
			item1.HVI_JS_LoadedOnShipment = shipment.PK;
			item1.HVI_F3_NKPackType = PkgUnit.Pallet;
			item1.HVI_ManifestedWeight = 10;
			item1.HVI_ManifestedVolume = 10;
			item1.HVI_ContainerNumber = "TestContainer 1";

			var item2 = consignment1.Items.AddNew();
			item2.HVI_CurrentBarcode = "TestItem2";
			item2.HVI_JS_LoadedOnShipment = shipment.PK;
			item2.HVI_F3_NKPackType = PkgUnit.Tube;
			item2.HVI_ManifestedWeight = 20;
			item2.HVI_ManifestedVolume = 20;
			item2.HVI_ContainerNumber = "TestContainer 1";

			var item3 = consignment1.Items.AddNew();
			item3.HVI_CurrentBarcode = "TestItem3";
			item3.HVI_JS_LoadedOnShipment = shipment.PK;
			item3.HVI_F3_NKPackType = PkgUnit.Package;
			item3.HVI_ManifestedWeight = 30;
			item3.HVI_ManifestedVolume = 30;
			item3.HVI_ContainerNumber = "TestContainer 2";

			var item4 = consignment2.Items.AddNew();
			item4.HVI_CurrentBarcode = "TestItem4";
			item4.HVI_JS_LoadedOnShipment = shipment.PK;
			item4.HVI_F3_NKPackType = PkgUnit.Reel;
			item4.HVI_ManifestedWeight = 40;
			item4.HVI_ManifestedVolume = 40;
			item4.HVI_ContainerNumber = "TestContainer 2";

			var smallItem = smallConsignment.Items.AddNew();
			smallItem.HVI_CurrentBarcode = "SmallItem";
			smallItem.HVI_JS_LoadedOnShipment = shipment.PK;
			smallItem.HVI_F3_NKPackType = PkgUnit.Reel;
			smallItem.HVI_ManifestedWeight = 0.1m;
			smallItem.HVI_ManifestedVolume = 0.1m;
			smallItem.HVI_ContainerNumber = "TestContainer 2";

			var itemLine1a = item1.Lines.AddNew();
			itemLine1a.HVS_OriginTariff = "1234.56";
			itemLine1a.HVS_CustomsValue = 1.1m;
			itemLine1a.HVS_GrossWeight = 10m;
			itemLine1a.HVS_WeightUnit = "KG";
			itemLine1a.HVS_Quantity = 1;
			itemLine1a.HVS_GoodsDescription = "Test Item Line 1a";
			itemLine1a.HVS_RN_NKOriginCountryCode = "AU";
			itemLine1a.HVS_HVI_HVLVItem = item1.PK;

			var itemLine1b = item1.Lines.AddNew();
			itemLine1b.HVS_OriginTariff = "1234.56";
			itemLine1b.HVS_CustomsValue = 1.5m;
			itemLine1b.HVS_GrossWeight = 20m;
			itemLine1b.HVS_WeightUnit = "KG";
			itemLine1b.HVS_Quantity = 2;
			itemLine1b.HVS_GoodsDescription = "Test Item Line 1b";
			itemLine1b.HVS_RN_NKOriginCountryCode = "AU";
			itemLine1b.HVS_HVI_HVLVItem = item1.PK;

			var itemLine2 = item2.Lines.AddNew();
			itemLine2.HVS_OriginTariff = "6543.21";
			itemLine2.HVS_CustomsValue = 2.9m;
			itemLine2.HVS_GrossWeight = 20m;
			itemLine2.HVS_WeightUnit = "KG";
			itemLine2.HVS_Quantity = 1;
			itemLine2.HVS_GoodsDescription = "Test Item Line 2";
			itemLine2.HVS_RN_NKOriginCountryCode = "AU";
			itemLine2.HVS_HVI_HVLVItem = item2.PK;

			var itemLine3a = item3.Lines.AddNew();
			itemLine3a.HVS_OriginTariff = "1357.99";
			itemLine3a.HVS_CustomsValue = 2.5m;
			itemLine3a.HVS_GrossWeight = 30m;
			itemLine3a.HVS_WeightUnit = "KG";
			itemLine3a.HVS_Quantity = 1;
			itemLine3a.HVS_GoodsDescription = "Test Item Line 3a";
			itemLine3a.HVS_RN_NKOriginCountryCode = "AU";
			itemLine3a.HVS_HVI_HVLVItem = item3.PK;

			var itemLine3b = item3.Lines.AddNew();
			itemLine3b.HVS_OriginTariff = "1357.99";
			itemLine3b.HVS_CustomsValue = 6m;
			itemLine3b.HVS_GrossWeight = 60m;
			itemLine3b.HVS_WeightUnit = "KG";
			itemLine3b.HVS_Quantity = 2;
			itemLine3b.HVS_GoodsDescription = "Test Item Line 3b";
			itemLine3b.HVS_RN_NKOriginCountryCode = "AU";
			itemLine3b.HVS_HVI_HVLVItem = item3.PK;

			var itemLine4 = item4.Lines.AddNew();
			itemLine4.HVS_OriginTariff = "2468.00";
			itemLine4.HVS_CustomsValue = 4m;
			itemLine4.HVS_GrossWeight = 40m;
			itemLine4.HVS_WeightUnit = "KG";
			itemLine4.HVS_Quantity = 1;
			itemLine4.HVS_GoodsDescription = "Test Item Line 4";
			itemLine4.HVS_RN_NKOriginCountryCode = "AU";
			itemLine4.HVS_HVI_HVLVItem = item4.PK;

			var smallLine1 = smallItem.Lines.AddNew();
			smallLine1.HVS_OriginTariff = "2468.00";
			smallLine1.HVS_CustomsValue = 0.1m;
			smallLine1.HVS_GrossWeight = 0.1m;
			smallLine1.HVS_WeightUnit = "KG";
			smallLine1.HVS_Quantity = 1;
			smallLine1.HVS_GoodsDescription = "Small Item Line";
			smallLine1.HVS_RN_NKOriginCountryCode = "AU";
			smallLine1.HVS_HVI_HVLVItem = smallItem.PK;

			var smallLine2 = smallItem.Lines.AddNew();
			smallLine2.HVS_OriginTariff = "2468.00";
			smallLine2.HVS_CustomsValue = 0.2m;
			smallLine2.HVS_GrossWeight = 0.2m;
			smallLine2.HVS_WeightUnit = "KG";
			smallLine2.HVS_Quantity = 1;
			smallLine2.HVS_GoodsDescription = "Small Item Line";
			smallLine2.HVS_RN_NKOriginCountryCode = "AU";
			smallLine2.HVS_HVI_HVLVItem = smallItem.PK;

			return shipment;
		}

		Shipment GetTestHVLVShipmentDataObject(ForwardingShipment shipment)
		{
			var result = default(Shipment);

			var writerManager = new DataWritingManager(new ActionInfo(RecipientRoleType.HSA, shipment));
			var writer = new ShipmentDataObjectWriter(writerManager, true, true);

			using (writerManager.SetIsPublishingInternally())
			{
				result = writer.GetDataObject(shipment);
			}

			var pair = new CodeDescriptionPair();
			pair.Code = DirectionTypeList.Codes.NVOCC;
			pair.Description = DirectionTypeList.Descriptions.NVOCC;

			result.MessageType = pair;

			return result;
		}
	}
}
