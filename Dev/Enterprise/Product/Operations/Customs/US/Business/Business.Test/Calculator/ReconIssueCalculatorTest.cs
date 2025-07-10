using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconIssueCalculatorTest : TestCaseWithFactory
	{
		public void TestRecalculateRecon_AfterInvoiceLineManufacturer_Changed()
		{
			var factory1 = new BusinessObjectFactory();
			var importer = factory1.Load<OrgHeader>(declarationImporter.PK);
			var manufacturer = factory1.Load<OrgHeader>(declarationImporter.PK);
			manufacturer.MainAddress.CustomsCodes.AddNew("MID", "JP432978HAS");

			importer.OH_RL_NKClosestPort = "USLAX";
			var supplierLink = importer.SupplierLinks.Cast<OrgSupplierBuyerLink>().FirstOrDefault(x => x.OL_OH_Supplier == manufacturer.PK) ?? importer.SupplierLinks.AddNew(manufacturer);
			supplierLink.OL_RN_NKImporterCountry = "US";

			var importerSupplierLinkAddInfo = supplierLink.GetAddInfo();
			importerSupplierLinkAddInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ValueRecon;
			factory1.Save();

			var jobDeclaration = Factory.New<JobDeclaration>();
			jobDeclaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDeclaration.JE_OH_Importer = importer.PK;
			Assert("Job Declaration Recon is NOT set when created", jobDeclaration.US_OtherReconIndicator.IsEmpty);

			var product = Factory.New<OrgSupplierPart>();
			product.OP_SystemLastEditTimeUtc = ZDateTime.Now;
			product.OP_PartNum = "Z!Z2Z";

			var invoiceHeader = jobDeclaration.Invoices.AddNew();
			var invoiceLine1 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = product.OP_PartNum;

			var invoiceLine2 = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = product.OP_PartNum;

			jobDeclaration.MarkReconIndicatorsDirty();
			jobDeclaration.RecalculateReconIndicators();
			Assert("Job Declaration Recon is NOT set yet", jobDeclaration.US_OtherReconIndicator.IsEmpty);

			invoiceLine1.JI_OA_ManufacturerAddress = manufacturer.MainAddress.PK;

			jobDeclaration.RecalculateReconIndicators();
			AssertEquals("Job Declaration Recon is now set to the supplier link's indicator", ReconIssueCodeList.Codes.ValueRecon, jobDeclaration.US_OtherReconIndicator);
		}

		public void TestEndToEndUpdateDeclarationReconIssue()
		{
			var declaration = GetDeclaration();
			declaration.JE_RL_NKFinalDestination = "USLAX";
			Factory.Save();
			AssertEquals("Recon Issue defaulted from Supplier/Buyer link", ReconIssueCodeList.Codes._9802Recon, declaration.US_OtherReconIndicator);

			var invoice = declaration.Invoices.AddNew();
			AssertEquals("Nothing changed: if declaration > ReconIndicatorsDirty is true, recalculation will be on factory saving.", ReconIssueCodeList.Codes._9802Recon, declaration.US_OtherReconIndicator);

			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;
			Factory.Save();
			AssertEquals(ReconIssueCodeList.Codes.Class9802Recon, declaration.US_OtherReconIndicator);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;
			Factory.Save();
			AssertEquals("Other Recon Indicator calculated from Declaration Supplier link, Invoice Supplier Link and Product",
						ReconIssueCodeList.Codes.ValueClass9802Recon, declaration.US_OtherReconIndicator);

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = product2.OP_PartNum;
			Factory.Save();
			AssertEquals("Other Recon Indicator calculated from Declaration Supplier link, Invoice Supplier Link and Products from invoice lines",
						ReconIssueCodeList.Codes.ValueClass9802Recon, declaration.US_OtherReconIndicator);
		}

		public void TestDoNotRecalculateReconForAddNewInvoiceLineAndDeleteInvoiceWithoutProduct()
		{
			var declaration = GetDeclaration();
			declaration.JE_RL_NKFinalDestination = "USLAX";
			var invoice = declaration.Invoices.AddNew();
			Factory.Save();
			AssertEquals("Recon Issue defaulted from Supplier/Buyer link", ReconIssueCodeList.Codes._9802Recon, declaration.US_OtherReconIndicator);
			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.Class9802Recon;
			AssertEquals("Recon Issue should not be reset", ReconIssueCodeList.Codes.Class9802Recon, declaration.US_OtherReconIndicator);
			AssertEquals("Recon Issue should not be reset", ReconIssueCodeList.Codes.Class9802Recon, declaration.US_OtherReconIndicator);
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			declaration.RecalculateReconIndicators();
			AssertEquals("Recon Issue should not be reset", ReconIssueCodeList.Codes.Class9802Recon, declaration.US_OtherReconIndicator);
			invoiceLine.Delete();
			declaration.RecalculateReconIndicators();
			AssertEquals("Recon Issue should not be reset", ReconIssueCodeList.Codes.Class9802Recon, declaration.US_OtherReconIndicator);
		}

		public void TestDoNoEmptyExistedReconIssueWhenIORDoesNotHaveReconIssue()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_IsConsignee = true;
			org1.OH_RL_NKClosestPort = "USCHI";
			var iorWrapper1 = OrgHeaderWrapper.New(org1);
			iorWrapper1.ZO_OtherReconIndicator = "";
			iorWrapper1.ZO_NAFTAReconIndicator = true;

			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org2.OH_IsConsignee = true;
			org2.OH_RL_NKClosestPort = "USCHI";
			var iorWrapper2 = OrgHeaderWrapper.New(org2);
			iorWrapper2.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.Value9802Recon;
			iorWrapper2.ZO_NAFTAReconIndicator = true;

			declaration.US_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			AssertEquals("Recon. Issue", ReconIssueCodeList.Codes.ClassRecon, declaration.US_OtherReconIndicator);

			declaration.IOROrgPK = org1.PK;
			declaration.RecalculateReconIndicators();
			AssertEquals("Recon. Issue does not updated with empty", ReconIssueCodeList.Codes.ClassRecon, declaration.US_OtherReconIndicator);

			declaration.IOROrgPK = org2.PK;
			declaration.RecalculateReconIndicators();
			AssertEquals("Recon. Issue Updated from Org2", ReconIssueCodeList.Codes.Value9802Recon, declaration.US_OtherReconIndicator);
		}

		public void TestRecalculateReconAfterProductReconChanged()
		{
			var factory1 = new BusinessObjectFactory();
			var declaration = factory1.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_RL_NKFinalDestination = "USLAX";
			factory1.Save();
			AssertEquals("Recon Issue defaulted from Supplier/Buyer link", ReconIssueCodeList.Codes._9802Recon, declaration.US_OtherReconIndicator);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			JobComInvoiceLinePartSynchronisationManager.SetCurrentPartSyncManagerActiveDeciderPK(invoiceLine.Factory, invoiceLine.PartSyncManagerActiveDeciderPK);

			var factory2 = new BusinessObjectFactory();
			var product = factory2.New<OrgSupplierPart>();
			product.OP_SystemLastEditTimeUtc = ZDateTime.Now;
			product.OP_PartNum = "Z!Z2Z";
			product.RelatedOrganisations.AddOwner(declarationImporter);
			product.RelatedOrganisations.AddSupplier(declarationSupplier);

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "10000000";
			pivot.CD_ReconIssue = ReconIssueCodeList.Codes._9802Recon;
			pivot.CD_NAFTARecon = true;
			factory2.Save();

			invoiceLine.JI_PartNo = product.OP_PartNum;

			declaration.RecalculateReconIndicators();
			AssertEquals("Recon Issue should be recalculated", ReconIssueCodeList.Codes._9802Recon, declaration.US_OtherReconIndicator);

			product.OP_SystemLastEditTimeUtc = ZDateTime.Now;
			pivot.CD_ReconIssue = ReconIssueCodeList.Codes.ClassRecon;

			factory2.Save();
			declaration.RecalculateReconIndicators();
			AssertEquals("Recon Issue should be recalculated", ReconIssueCodeList.Codes.Class9802Recon, declaration.US_OtherReconIndicator);
		}

		public void TestDoNotRecalculateReconForNonImportDec()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Export;

			var invoice = declaration.Invoices.AddNew();
			invoice.Delete();

			invoice = declaration.Invoices.AddNew();
			var line = declaration.InvoiceLines.AddNew();
			line.Delete();

			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;
			Factory.Save();
			AssertEquals("Should not calculate recon indicators", ZString.Empty, declaration.US_OtherReconIndicator);
		}

		public void TestGetListOfIssuesToCheckAgainst()
		{
			var declaration = GetDeclaration();

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = product2.OP_PartNum;

			var result = new List<ReconIssues>();
			result.AddRange(ReconIssueCalculator.GetListOfIssuesToCheckAgainst(declaration));
			AssertEquals("Should be 4 members", 4, result.Count);
			AssertEquals(ReconIssues._98, result[0]);
			AssertEquals(ReconIssues.CL, result[1]);
			AssertEquals(ReconIssues.VL | ReconIssues._98, result[2]);
			AssertEquals(ReconIssues.VL | ReconIssues.CL, result[3]);
		}

		public void TestGetReconCodeFromReconValuesWhenImporterHasNA()
		{
			var importer = Factory.New<OrgHeader>();
			var importerWrapper = OrgHeaderWrapper.New(importer);
			importerWrapper.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.NotApplicable;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST";
			product.OP_Desc = "TEST";
			product.RelatedOrganisations.AddNew().OU_OH = importer.PK;

			var pivot = product.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CD_ReconIssue = ReconIssueCodeList.Codes.ClassRecon;
			pivot.CI_TariffNum = "980.125.1000";
			pivot.CI_DateStart = ZDateTime.Now.AddDays(-10);

			invoiceLine.JI_PartNo = "TEST";
			AssertNotNull(invoiceLine.Part);

			declaration.RecalculateReconIndicators();
			AssertEquals(ReconIssueCodeList.Codes.ClassRecon, declaration.US_OtherReconIndicator);
		}

		public void TestNAShouldNotBeDefaultedIfNothingSpecifiesIt()
		{
			var importer = Factory.New<OrgHeader>();
			var importerWrapper = OrgHeaderWrapper.New(importer);
			importerWrapper.ZO_OtherReconIndicator = ZString.Empty;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFreeDutiable;

			declaration.RecalculateReconIndicators();
			AssertEquals(ZString.Empty, declaration.US_OtherReconIndicator);
		}

		public void TestGetNAFTAIndicToCheckAgainst()
		{
			var declaration = GetDeclaration();
			declaration.IOROrgPK = declarationImporter.PK;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_OH_Supplier = invoiceSupplier.PK;
			invoice.JZ_OH_Buyer = invoiceImporter.PK;

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = product1.OP_PartNum;

			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = product2.OP_PartNum;

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_Tariff = USCTariff.CottonFeeApplicable;

			var invoice2 = declaration.Invoices.AddNew();

			var result = new List<ZBool>();
			result.AddRange(ReconIssueCalculator.GetNAFTAIndicToCheckAgainst(declaration));
			AssertEquals("Should be 5 members", 5, result.Count);
			AssertEquals(false, result[0]);
			AssertEquals(false, result[1]);
			AssertEquals(true, result[2]);
			AssertEquals(true, result[3]);
			AssertEquals(false, result[4]);
		}

		public void TestGetNAFTAIndicatorWhenIndicatedOnImporterCS00389140()
		{
			OrgHeaderWrapper.New(declarationImporter).ZO_NAFTAReconIndicator = true;
			importerSupplierLinkAddInfo.ZO_NAFTAReconIndicator = false;//having a link indicated as false should not exclude getting defaults from importer as this is a boolean flag.

			var declaration = GetDeclaration();
			declaration.IOROrgPK = declarationImporter.PK;
			declaration.RecalculateReconIndicators();
			Assert(declaration.US_NAFTAReconIndicator);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declarationSupplier = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignor, true).AddToFilter(OrgHeaderSchema.OH_Code, "COLFAB"));
			declarationImporter = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_IsConsignee, true).AddToFilter(OrgHeaderSchema.OH_Code, "UNMATCHED"));
			declarationImporter.OH_RL_NKClosestPort = "USLAX";
			var link1 = declarationImporter.SupplierLinks.AddNew(declarationSupplier);
			link1.OL_RN_NKImporterCountry = "US";
			importerSupplierLinkAddInfo = new USOrgSupplierBuyerLinkAddInfo(link1.GetAddInfo());
			importerSupplierLinkAddInfo.ZO_OtherReconIndicator = ReconIssueCodeList.Codes._9802Recon;
			importerSupplierLinkAddInfo.ZO_NAFTAReconIndicator = false;

			invoiceSupplier = Factory.NewWithValidTestData<OrgHeader>();
			invoiceImporter = Factory.NewWithValidTestData<OrgHeader>();

			var link2 = invoiceImporter.SupplierLinks.AddNew(invoiceSupplier);
			link2.OL_RN_NKImporterCountry = "US";
			var addInfo2 = link2.GetAddInfo();
			addInfo2.ZO_OtherReconIndicator = ReconIssueCodeList.Codes.ClassRecon;
			addInfo2.ZO_NAFTAReconIndicator = true;

			product1 = Factory.New<OrgSupplierPart>();
			product1.OP_PartNum = "Z!Z2Z";
			product1.RelatedOrganisations.AddOwner(invoiceImporter);
			product1.RelatedOrganisations.AddSupplier(invoiceSupplier);

			var pivot1 = product1.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CD_ReconIssue = ReconIssueCodeList.Codes.Value9802Recon;
			pivot1.CD_NAFTARecon = true;

			product2 = Factory.New<OrgSupplierPart>();
			product2.OP_PartNum = "Test Product 2";
			product2.RelatedOrganisations.AddOwner(invoiceImporter);
			product2.RelatedOrganisations.AddSupplier(invoiceSupplier);

			var pivot2 = product2.PivotsForBinding.AddNew();
			pivot2.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot2.CI_TariffNum = "10000000";
			pivot2.CD_ReconIssue = ReconIssueCodeList.Codes.ValueClassRecon;
			pivot2.CD_NAFTARecon = false;
			Factory.Save();
		}
		OrgHeader declarationSupplier;
		OrgHeader declarationImporter;
		USOrgSupplierBuyerLinkAddInfo importerSupplierLinkAddInfo;
		OrgHeader invoiceSupplier;
		OrgHeader invoiceImporter;
		OrgSupplierPart product1;
		OrgSupplierPart product2;

		JobDeclaration GetDeclaration()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Supplier = declarationSupplier.PK;
			declaration.JE_OH_Importer = declarationImporter.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			return declaration;
		}
	}
}
