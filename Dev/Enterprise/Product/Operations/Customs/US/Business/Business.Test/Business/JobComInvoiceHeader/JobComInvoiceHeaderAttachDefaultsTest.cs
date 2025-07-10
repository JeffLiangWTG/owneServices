using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class JobComInvoiceHeaderAttachDefaultsTest : BaseJobComInvoiceHeaderAttachDefaultsTest
	{
		public void TestConcurrencyResolver_For_JZ_OH_Supplier()
		{
			var invoice = Factory.NewWithValidTestData<JobComInvoiceHeader>();
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();
			invoice.JZ_OH_Supplier = orgHeader.PK;
			Factory.Save();
			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			Factory.RefreshEnabled = false;
			factory2.RefreshEnabled = false;
			var invoice2 = factory2.Load<JobComInvoiceHeader>(invoice.PK);
			invoice2.JZ_OH_Supplier = orgHeader2.PK;
			factory2.Save();
			try
			{
				invoice.JZ_Weight = 2.0m;
				Factory.Save();
			}
			catch (Exception ex)
			{
				ZExceptionReporting.HandleSaveException(ex);
				Factory.Save();
			}

			AssertEquals("", ErrorReporter.LastMessageReported);
		}

		public void TestTrimBillNumberWhenSCACIncludedInReferenceNumber()
		{
			var carrier = Factory.New<USCarrierCombined>();
			carrier.UI_Code = "LMAG";
			var carrier2 = Factory.New<USCarrierCombined>();
			carrier2.UI_Code = "DDKK";

			var invoice = Factory.New<JobComInvoiceHeader>();
			var invoiceLine = invoice.InvoiceLines.AddNew();
			invoiceLine.JI_Description = "ComInvoiceLine";

			var ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
			ref1.J2_ReferenceNumber = "LMAG150150";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = InvoiceHeaderRefsTypeList.Codes.MB;
			ref1.J2_ReferenceNumber = "DDKK350350";
			Factory.Save();

			var testDec = Factory.New<JobDeclaration>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			testDec.JE_TransportMode = TransportTypeList.Codes.Sea;
			var testDecBill = testDec.Bills.AddNew();
			testDecBill.CU_BillNum = "150150";

			invoice.JZ_JE = testDec.PK;

			AssertEquals(2, testDec.Bills.Count);
			var bill = testDec.Bills.FindByBillNumberAndType("150150", BillTypeList.Codes.MasterBill);
			AssertEquals("150150", bill.CU_BillNum);
			var bill2 = testDec.Bills.FindByBillNumberAndType("350350", BillTypeList.Codes.MasterBill);
			AssertEquals("350350", bill2.CU_BillNum);

			var testDecAir = Factory.New<JobDeclaration>();
			testDecAir.JE_MessageType = JobMessageTypeList.Codes.Import;
			testDecAir.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			testDecAir.JE_TransportMode = TransportTypeList.Codes.Air;
			var testDecAirBill = testDecAir.Bills.AddNew();
			testDecAirBill.CU_BillNum = "150150";

			invoice.JZ_JE = testDecAir.PK;

			AssertEquals(3, testDecAir.Bills.Count);
			var bill3 = testDecAir.Bills.FindByBillNumberAndType("150150", BillTypeList.Codes.MasterBill);
			AssertEquals("AIR - 150150", "150150", bill3.CU_BillNum);
			var bill4 = testDecAir.Bills.FindByBillNumberAndType("LMAG150150", BillTypeList.Codes.MasterBill);
			AssertEquals("LMAG150150", bill4.CU_BillNum);
			var bill5 = testDecAir.Bills.FindByBillNumberAndType("DDKK350350", BillTypeList.Codes.MasterBill);
			AssertEquals("DDKK350350", bill5.CU_BillNum);

			var testDecExp = Factory.New<JobDeclaration>();
			testDecExp.JE_MessageType = JobMessageTypeList.Codes.Export;
			testDecExp.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			testDecExp.JE_TransportMode = TransportTypeList.Codes.Sea;
			var testDecExpBill = testDecExp.Bills.AddNew();
			testDecExpBill.CU_BillNum = "150150";

			invoice.JZ_JE = testDecExp.PK;

			AssertEquals(3, testDecExp.Bills.Count);
			var bill6 = testDecExp.Bills.FindByBillNumberAndType("150150", BillTypeList.Codes.MasterBill);
			AssertEquals("Export - 150150", "150150", bill6.CU_BillNum);
			AssertNull("Export - No Bill Number 350350", testDecExp.Bills.FindByBillNumberAndType("350350", BillTypeList.Codes.MasterBill));
			var bill7 = testDecAir.Bills.FindByBillNumberAndType("LMAG150150", BillTypeList.Codes.MasterBill);
			AssertEquals("Export - LMAG150150", "LMAG150150", bill7.CU_BillNum);
			var bill8 = testDecExp.Bills.FindByBillNumberAndType("DDKK350350", BillTypeList.Codes.MasterBill);
			AssertEquals("Export - DDKK350350", "DDKK350350", bill8.CU_BillNum);
		}

		public void TestInvoiceHeaderHasAnyPGADataEitherDeclaredOrDisclaimed()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			var header = declaration.Invoices.AddNew();
			var line = header.InvoiceLines.AddNew();
			AssertEquals("No Indicators On", false, header.HasAnyPGADataEitherDeclaredOrDisclaimed);
			line.US_VNEInd = OGAIndicatorList.Codes.Declared;
			var vneDetail = line.VehicleLines.AddNew();
			vneDetail.US_BodyDescription = "ABCE";
			AssertEquals("VNE Declared", true, declaration.PGAFlags.HasInvoiceLinesWithVNE);
			AssertEquals("Has PGA Data - VNE Declared", true, header.HasAnyPGADataEitherDeclaredOrDisclaimed);
			line.VehicleLines.RemoveAll();
			line.US_VNEInd = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("VNE Disclaimed", false, declaration.PGAFlags.HasInvoiceLinesWithVNE);
			AssertEquals("Has PGA Data - VNE Disclaimed", true, header.HasAnyPGADataEitherDeclaredOrDisclaimed);
			line.US_VNEInd = "";
			AssertEquals("No PGA Data", false, header.HasAnyPGADataEitherDeclaredOrDisclaimed);
			line.US_FDAIndicator = OGAIndicatorList.Codes.Disclaimed;
			AssertEquals("FDA PGA Disclaimed ", true, header.HasAnyPGADataEitherDeclaredOrDisclaimed);
		}

		public void TestUS_TariffTypeWhenAttachedToDeclaration()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.US_TariffType = TariffTypeList.Codes.HTS;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			var declaration = new BusinessObjectFactory().New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			var invoiceLoaded = declaration.Factory.Load<JobComInvoiceHeader>(invoice.PK);
			invoiceLoaded.JZ_JE = declaration.PK;
			invoiceLoaded.JZ_ClusterKey = declaration.JE_ClusterKey;
			AssertEquals("Tariff Type", TariffTypeList.Codes.HTS, invoiceLoaded.JobComInvoiceLines[0].US_TariffType);
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			AssertEquals("Tariff Type", TariffTypeList.Codes.HTS, invoiceLoaded.JobComInvoiceLines[0].US_TariffType);
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertEquals("Tariff Type", TariffTypeList.Codes.ScheduleB, invoiceLoaded.JobComInvoiceLines[0].US_TariffType);
		}

		public void TestUS_TariffTypeWhenAttachedToDeclaration2()
		{
			var invoice = Factory.New<JobComInvoiceHeader>();
			invoice.JZ_MessageType = JobMessageTypeList.Codes.Export;
			invoice.US_TariffType = TariffTypeList.Codes.HTS;
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			Factory.Save();
			var declaration = new BusinessObjectFactory().New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			declaration.US_TariffType = TariffTypeList.Codes.HTS;
			var invoiceLoaded = declaration.Factory.Load<JobComInvoiceHeader>(invoice.PK);
			invoiceLoaded.JZ_JE = declaration.PK;
			AssertEquals("Tariff Type", TariffTypeList.Codes.HTS, invoiceLoaded.JobComInvoiceLines[0].US_TariffType);
			declaration.US_TariffType = TariffTypeList.Codes.ScheduleB;
			AssertEquals("Tariff Type", TariffTypeList.Codes.ScheduleB, invoiceLoaded.JobComInvoiceLines[0].US_TariffType);
		}

		public override void TestAddHeaderRefsToDeclaration()
		{
			JobDeclaration decl = (JobDeclaration)declaration;
			CusContainer container = decl.CusContainers.AddNew();
			container.CO_ContainerNumber = "CNT007";
			Bill bill = decl.Bills.AddNew();
			bill.CU_BillType = Enterprise.Customs.Business.BillTypeList.Codes.MasterBill;
			bill.CU_BillNum = "MB800450";
			AssertEquals("Precondition: 1 container for declaration", 1, decl.CusContainers.Count);
			AssertEquals("CNT007", decl.CusContainers[0].CO_ContainerNumber);
			AssertEquals("Precondition: 1 MB for declaration", 1, decl.Bills.Count);
			AssertEquals("MB800450", decl.Bills[0].CU_BillNum);
			JobComInvoiceHeaderRefs ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.CN;
			ref1.J2_ReferenceNumber = "CNT002";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.CN;
			ref1.J2_ReferenceNumber = "CNT007";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.MB;
			ref1.J2_ReferenceNumber = "MB150";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.MB;
			ref1.J2_ReferenceNumber = "MB800450";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.HB;
			ref1.J2_ReferenceNumber = "HB002";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.SH;
			ref1.J2_ReferenceNumber = "SubHouseBill005";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = Enterprise.Customs.Business.InvoiceHeaderRefsTypeList.Codes.HB;
			ref1.J2_ReferenceNumber = "HB008";
			ref1 = invoice.InvoiceHeaderRefs.AddNew();
			ref1.J2_ReferenceType = "ERT";
			ref1.J2_ReferenceNumber = "123456";
			invoice.JZ_JE = decl.PK;
			AssertEquals(2, decl.CusContainers.Count);
			AssertNotNull(decl.CusContainers.Find("CNT007"));
			AssertNotNull(decl.CusContainers.Find("CNT002"));
			AssertEquals(5, decl.Bills.Count);
			bill = decl.Bills.FindByBillNumberAndType("MB800450", Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
			AssertEquals(ZGuid.Empty, bill.CU_CU_ParentBill);
			Bill bill1 = decl.Bills.FindByBillNumberAndType("MB150", Enterprise.Customs.Business.BillTypeList.Codes.MasterBill);
			AssertEquals(ZGuid.Empty, bill1.CU_CU_ParentBill);
			Bill bill2 = decl.Bills.FindByBillNumberAndType("HB002", Enterprise.Customs.Business.BillTypeList.Codes.HouseBill);
			AssertEquals(bill1.PK, bill2.CU_CU_ParentBill);
			Bill bill3 = decl.Bills.FindByBillNumberAndType("HB008", Enterprise.Customs.Business.BillTypeList.Codes.HouseBill);
			AssertEquals(bill1.PK, bill3.CU_CU_ParentBill);
			Bill bill4 = decl.Bills.FindByBillNumberAndType("SubHouseBill005", Enterprise.Customs.Business.BillTypeList.Codes.SubHouseBill);
			AssertEquals("Parent should not be set, because several HB", ZGuid.Empty, bill4.CU_CU_ParentBill);
			bill = decl.Bills.FindByBillNumberAndType("123456", "ERT");
			AssertEquals("Unknown Reference Code - object not added", null, bill);
			AssertNull("Unknown Reference Code - object not added", decl.CusContainers.Find("123456"));
		}

		public void TestRetrieveInvoicesForReleaseEntry()
		{
			var releaseDeclaration = Factory.New<JobDeclaration>();
			releaseDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			releaseDeclaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;
			releaseDeclaration.US_EnableENS = true;
			releaseDeclaration.US_EntryFilerCode = "XJ5";
			releaseDeclaration.ImportEntryNumber = "9342838";
			releaseDeclaration.JE_MergeBy = OrgConstants.MergeInvoiceLines.Tariff;
			var invoice1 = releaseDeclaration.Invoices.AddNew();
			invoice1.JZ_InvoiceNumber = "INV1";
			invoice1.JobComInvoiceLines.AddNew();
			releaseDeclaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			Factory.Save();
			var declaration = Factory.New<JobDeclaration>();
			declaration.US_ConsolACE = true;
			var invoice2 = declaration.Invoices.AddNew();
			var invoice3 = declaration.Invoices.AddNew();
			AssertEquals("Pre-condition: 2 invoices exist", 2, declaration.Invoices.Count);
			AssertEquals("Pre-condition: No invoice lines exist", 0, declaration.InvoiceLines.Count);
			invoice2.US_ReleaseEntryNumber = "XJ59342838";
			AssertEquals(3, declaration.Invoices.Count);
			AssertEquals(1, declaration.InvoiceLines.Count);
			invoice3.US_ReleaseEntryNumber = "XJ59342838";
			AssertEquals(3, declaration.Invoices.Count);
			AssertEquals(1, declaration.InvoiceLines.Count);
			releaseDeclaration.JobComInvoiceGroupHeaders[0].Charges.AddNew();
			Factory.Save();
			invoice1.US_ReleaseEntryNumber = "XJ59342838";
			AssertEquals("Can't import its own charges.", 1, releaseDeclaration.JobComInvoiceGroupHeaders[0].Charges.Count);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var htiPivot = part.PivotsForBinding.AddNew();
		}
	}
}
