using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	class CustomsDeliveryOrderDefaulterTest : TestCaseWithFactory
	{
		public void TestDefaulter()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_FullName = "Dummy Delivery Org";
			org.MainAddress.OA_Address1 = "Address 1";
			OrgContact contact1 = org.Contacts.AddNew();
			contact1.OC_ContactName = "MR BOB";
			var contact2 = org.Contacts.AddNew();
			contact2.OC_ContactName = "MR JACK";
			contact2.OC_Phone = "JACK123";
			var contact3 = org.Contacts.AddNew();
			contact3.OC_ContactName = "MR JOE";
			contact3.OC_Phone = "JOE123";

			OrgHeader consignee = Factory.New<OrgHeader>();
			consignee.OH_FullName = "Dummy Consignee Org";
			consignee.MainAddress.OA_Address1 = "Address 1";
			consignee.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Say hello world to everyone");

			OrgHeader cartageCompany = Factory.New<OrgHeader>();
			cartageCompany.OH_FullName = "Dummy Cartage Org";
			cartageCompany.MainAddress.OA_Address1 = "Address 1";

			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_TransportMode = TransportTypeList.Codes.Sea;
			declaration.US_ITDate = new ZDateTime(2008, 9, 11);
			declaration.JE_MasterBill = "MWB123456";
			declaration.JE_HouseBill = "HWB123456";
			declaration.PrimaryHouseBill.ITNumber = "ITNUMBER";
			declaration.JE_GoodsDescription = "GOODS SHORT DESCRIPTION";
			declaration.JE_TotalWeight = 1.56m;
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			declaration.JE_TotalNoOfPacks = 234;
			declaration.JE_TotalNoOfPacksPackType = ABIUnitOfMeasureList.Codes.Pairs;
			declaration.ImporterDeliveryAddress.E2_OA_Address = org.MainAddress.PK;
			declaration.ImporterDeliveryAddress.ContactPK = contact1.PK;
			declaration.US_UI_NKCarrierSCAC = "SJDL";
			declaration.DeliveryOrPickupCartageCoPK = cartageCompany.PK;

			OrderItem orderItem1 = declaration.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "ORDERREF1";
			OrderItem orderItem2 = declaration.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "ORDERREF2";

			var invoice1 = declaration.Invoices.AddNew();
			var invoice1Line1 = invoice1.JobComInvoiceLines.AddNew();
			var invoice1Line1undg = invoice1Line1.UNDGs.TryGetOrCreate("2794A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			invoice1Line1undg.DI_OC_DGContact = contact2.PK;
			var invoice1Line2 = invoice1.JobComInvoiceLines.AddNew();
			var invoice1Line2undg = invoice1Line2.UNDGs.TryGetOrCreate("2997B", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			invoice1Line2undg.DI_OC_DGContact = contact3.PK;

			var invoice2 = declaration.Invoices.AddNew();
			var invoice2Line1 = invoice2.JobComInvoiceLines.AddNew();
			var invoice2Line1undg = invoice2Line1.UNDGs.TryGetOrCreate("2794A", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			invoice2Line1undg.DI_OC_DGContact = contact3.PK;
			var invoice2Line2 = invoice2.JobComInvoiceLines.AddNew();
			var invoice2Line2undg = invoice2Line2.UNDGs.TryGetOrCreate("2997B", UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO);
			invoice2Line2undg.DI_OC_DGContact = contact3.PK;

			DeliveryOrderHeader header = Factory.New<DeliveryOrderHeader>();
			CustomsDeliveryOrderDefaulter.Defaulter(declaration, header);

			AssertEquals("ITNUMBER", header.US_PreviousITNo);
			AssertEquals(new ZDateTime(2008, 9, 11), header.US_PreviousITDate);
			AssertEquals("Say hello world to everyone", header.US_DeliveryInstructions);
			AssertEquals("Default - Delivery Addresses", AddressType.DLV, header.ForDeliveryToAddress.DefaultAddressType);
			AssertEquals(org.MainAddress.PK, header.ForDeliveryToAddress.E2_OA_Address);
			AssertEquals(false, header.ForDeliveryToAddress.E2_AddressOverride);
			AssertEquals(contact1.OC_ContactName, header.ForDeliveryToAddress.E2_Contact);
			AssertEquals("ORDERREF1", header.US_OrderReference);
			AssertEquals("SJDL", header.US_UI_NKCarriersLocalAgent);
			AssertEquals(cartageCompany.PK, header.US_OH_InlandCarrier);

			AssertEquals(0, header.DeliveryOrderLines.Count);
			AssertEquals(3, header.DeliveryOrderHazmats.Count);
			AssertNotNull(header.DeliveryOrderHazmats.Find(x => x.US_UNNumber == "2794A" && x.US_EmergencyContactNmber == "JACK123"));
			AssertNotNull(header.DeliveryOrderHazmats.Find(x => x.US_UNNumber == "2794A" && x.US_EmergencyContactNmber == "JOE123"));
			AssertNotNull(header.DeliveryOrderHazmats.Find(x => x.US_UNNumber == "2997B" && x.US_EmergencyContactNmber == "JOE123"));

			AssertEquals(2, header.DeliveryOrderBills.Count);

			DeliveryOrderBill bill = header.DeliveryOrderBills[0];
			AssertEquals("MWB123456", bill.CY_Data);
			AssertEquals(Customs.Business.BillTypeList.Codes.MasterBill, bill.CY_Code);
			bill = header.DeliveryOrderBills[1];
			AssertEquals("HWB123456", bill.CY_Data);
			AssertEquals(Customs.Business.BillTypeList.Codes.HouseBill, bill.CY_Code);
			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Say goodbye world to everyone");
			declaration.PrimaryHouseBill.ITNumber = "";
			header.US_PreviousITDate = ZDateTime.Empty;

			CustomsDeliveryOrderDefaulter.Defaulter(declaration, header);

			AssertEquals("Say goodbye world to everyone", header.US_DeliveryInstructions);

			declaration.ImporterDeliveryAddress.E2_AddressOverride = true;
			header = Factory.New<DeliveryOrderHeader>();
			CustomsDeliveryOrderDefaulter.Defaulter(declaration, header);
			AssertEquals("", header.US_PreviousITNo);
			AssertEquals(new ZDateTime(2008, 9, 11), header.US_PreviousITDate);
			AssertEquals("Say goodbye world to everyone", header.US_DeliveryInstructions);
			AssertEquals(0, header.DeliveryOrderLines.Count);
			AssertEquals(true, header.ForDeliveryToAddress.E2_AddressOverride);
			AssertEquals("Dummy Delivery Org", header.ForDeliveryToAddress.E2_CompanyName);
			AssertEquals(contact1.OC_ContactName, header.ForDeliveryToAddress.E2_Contact);

			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Say hello world to everyone from shipment");
			shipment.Notes.AddNew(false, PredefinedNoteTypes.Instance.DeliveryInstructionsNote.Description, "Say goodbye world to everyone from shipment");
			declaration.JE_JS = shipment.PK;
			ErrorReporter.Clear();

			CustomsDeliveryOrderDefaulter.Defaulter(declaration, header);
			AssertEquals("Say hello world to everyone from shipment" + System.Environment.NewLine + "Say goodbye world to everyone from shipment", header.US_DeliveryInstructions);
		}

		public void TestDefaulterCustomerRef()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = Factory.New<OrgHeader>().PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_OwnerRef = "OWNER REF";

			OrderItem orderItem1 = declaration.DocsAndCartage.OrderItems.AddNew();
			orderItem1.JT_OrderReference = "ORDERREF1";
			OrderItem orderItem2 = declaration.DocsAndCartage.OrderItems.AddNew();
			orderItem2.JT_OrderReference = "ORDERREF2";

			DeliveryOrderHeader header = Factory.New<DeliveryOrderHeader>();
			CustomsDeliveryOrderDefaulter.Defaulter(declaration, header);

			AssertEquals("OWNER REF", header.US_OrderReference);
		}

		public void TestDefaultOrderLine()
		{
			JobDeclaration declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_HouseBill = "HB1";
			declaration.JE_GoodsDescription = "GOODS SHORT DESCRIPTION";
			declaration.JE_TotalWeight = 1.56m;
			declaration.JE_TotalWeightUnit = "KH";
			declaration.JE_TotalNoOfPacks = 234;
			declaration.JE_TotalNoOfPacksPackType = ABIUnitOfMeasureList.Codes.Pairs;

			Customs.Business.BasePackage package = declaration.Packages[0];
			package.CW_MarksAndNos = "MARKS AND NUMBERS";
			package.CW_PackQty = 100;
			package.CW_PackType = ABIUnitOfMeasureList.Codes.Number;

			AssertEquals(1, declaration.Packages.Count);

			DeliveryOrderHeader header = declaration.DeliveryOrderHeaders.AddNew();
			header.DeliveryOrderLines.DeleteAll();
			CustomsDeliveryOrderDefaulter.DefaultOrderLine(declaration, header);
			AssertEquals(1, header.DeliveryOrderLines.Count);
			DeliveryOrderLine line = header.DeliveryOrderLines[0];
			AssertEquals("GOODS SHORT DESCRIPTION", line.US_GoodsDescription);
			AssertEquals(234, line.US_NoOfPackages);
			AssertEquals(ABIUnitOfMeasureList.Codes.Pairs, line.US_PackageType);
			AssertEquals(0m, line.US_WeightInKilograms);

			declaration.Notes.AddNew(false, PredefinedNoteTypes.Instance.DetailedGoodsDescription.Description, "LONG DETAILED GOODS DESCRIPTIONS");
			header.DeliveryOrderLines.DeleteAll();
			declaration.JE_TotalWeightUnit = Core.Constants.Weight.Tonnes;
			CustomsDeliveryOrderDefaulter.DefaultOrderLine(declaration, header);
			AssertEquals(1, header.DeliveryOrderLines.Count);
			line = header.DeliveryOrderLines[0];
			AssertEquals("LONG DETAILED GOODS DESCRIPTIONS", line.US_GoodsDescription);
			AssertEquals(234, line.US_NoOfPackages);
			AssertEquals(ABIUnitOfMeasureList.Codes.Pairs, line.US_PackageType);
			AssertEquals(1560m, line.US_WeightInKilograms);
		}
	}
}
