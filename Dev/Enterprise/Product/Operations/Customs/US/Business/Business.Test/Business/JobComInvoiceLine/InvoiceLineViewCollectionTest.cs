using System.ComponentModel;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(InvoiceLineViewCollection))]
	sealed class InvoiceLineViewCollectionTest : Customs.Business.Testing.InvoiceLineCollectionBOTest<InvoiceLineViewCollection>
	{
		public void TestIsThisPartOfTheCollection()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			var originalEntry1 = reconDeclaration.OriginalEntries.AddNew();
			var invoice1 = originalEntry1.Invoice;
			invoice1.JobComInvoiceLines.AddNew();

			var originalEntry2 = reconDeclaration.OriginalEntries.AddNew();
			var invoice2 = originalEntry2.Invoice;
			var invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();

			AssertEquals(2, reconDeclaration.FilteredInvoiceLines.Count);

			reconDeclaration.SelectedOriginalEntry = originalEntry2.CH_PK;
			AssertEquals("only contains one invoice line", 1, reconDeclaration.FilteredInvoiceLines.Count);
			AssertEquals("only contains one invoice line", invoiceLine2, reconDeclaration.FilteredInvoiceLines[0]);
		}

		public void TestSetDefaultDrawbackClaimsValues()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("invoiceLine.US_DRWDeclaredVFD", 0m, invoiceLine.US_DRWDeclaredVFD);
			AssertEquals("invoiceLine.US_DRWDeclaredTax", 0m, invoiceLine.US_DRWDeclaredTax);
			AssertEquals("invoiceLine.US_DRWDeclaredHMF", 0m, invoiceLine.US_DRWDeclaredHMF);
			AssertEquals("invoiceLine.US_DRWDeclaredMPF", 0m, invoiceLine.US_DRWDeclaredMPF);
			AssertEquals("invoiceLine.US_DRWDeclaredOtherFees", 0m, invoiceLine.US_DRWDeclaredOtherFees);
			AssertEquals("invoiceLine.US_DRWDutyRate_New", 0m, invoiceLine.US_DRWDutyRate_New);
			AssertEquals("invoiceLine.US_DRWWeightedRatio", 0m, invoiceLine.US_DRWWeightedRatio);
			AssertEquals("invoiceLine.US_DRWMPFWeightedRatio", 0m, invoiceLine.US_DRWMPFWeightedRatio);
			AssertEquals("invoiceLine.US_DRWLineDuty", 0m, invoiceLine.US_DRWLineDuty);
		}

		public void TestPGACorrectionRequiredIsSetForNonCommitted()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableCRL = true;
			declaration.US_CargoReleaseType = CargoReleaseTypeList.Codes.SE;
			declaration.FilteredInvoiceLines.CopyLastLineDetailsToNewLines = true;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			declaration.DoMerge(new SendsMessagesToCustomsShutterUpperer());
			line.US_APHISInd = OGAIndicatorList.Codes.Declared;
			var uncommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
			uncommittedLine.JI_Description = "B";
			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(1);
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);

			uncommittedLine.Delete();
			uncommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			uncommittedLine.JI_Description = "B";
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseAdd;
			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(1);
			AssertEquals(YesNoList.Codes.Yes, declaration.US_PGAReplaceUpdateNeeded);

			uncommittedLine.Delete();
			declaration.US_PGAReplaceUpdateNeeded = ZString.Empty;
			uncommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
			AssertEquals(false, uncommittedLine.HasChanges);
			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(1);
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);

			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.AwaitingACECargoReleaseAdd;
			declaration.ActiveEntryHeaders.SimplifiedEntry.Logs.RemoveAndDeleteAll();
			declaration.US_PGAReplaceUpdateNeeded = "#";
			uncommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			AssertEquals("#", declaration.US_PGAReplaceUpdateNeeded);
			uncommittedLine.JI_Description = "B";
			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(1);
			AssertEquals("#", declaration.US_PGAReplaceUpdateNeeded);

			uncommittedLine.Delete();
			declaration.ActiveEntryHeaders.SimplifiedEntry.CH_Status = ImportMessageStatusList.Codes.ClearACECargoReleaseReplace;
			declaration.US_PGAReplaceUpdateNeeded = ZString.Empty;
			uncommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
			uncommittedLine.JI_Description = "B";
			((ICancelAddNew)declaration.FilteredInvoiceLines).CancelNew(1);
			AssertEquals(ZString.Empty, declaration.US_PGAReplaceUpdateNeeded);
		}

		public void TestChildLinesAreRefreshedIfChildLineIsAdded()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("NO child line yet", 0, line.ChildLines.Count());

			var uncommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			uncommittedLine.JI_ParentID = line.PK;
			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(1);
			AssertEquals("one child line added", 1, line.ChildLines.Count());
		}

		public void TestProductRelatedLinesAreRefreshedOnAdd()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var invoice = declaration.Invoices.AddNew();
			var line = invoice.InvoiceLines.AddNew();
			AssertEquals(0, line.ProductRelatedLines.Count());

			var uncommittedLine = (JobComInvoiceLine)((IBindingList)declaration.FilteredInvoiceLines).AddNew();
			uncommittedLine.US_JI_ParentProduct = line.PK;
			((ICancelAddNew)declaration.FilteredInvoiceLines).EndNew(1);
			AssertEquals(1, line.ProductRelatedLines.Count());
		}

		public void TestDefaultParentIDForNewChild()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.Invoices.AddNew();
			var line1 = declaration.FilteredInvoiceLines.AddNew();
			line1.US_SecondarySPI = SecondarySpecProgIndicatorList.Codes.X;

			var line2 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("JI_ParentID is set", line1.PK, line2.JI_ParentID);
			AssertEquals("IsSetComponentLine", true, line2.IsSetVLine);

			var line3 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("JI_ParentID is set", line1.PK, line3.JI_ParentID);
			AssertEquals("IsSetComponentLine", true, line3.IsSetVLine);
		}

		public void TestTypedIndexer()
		{
			var declaration = Factory.New<JobDeclaration>();
			var collection = new InvoiceLineViewCollection(declaration);
			var invoiceLine = collection.AddNew();
			AssertEquals(invoiceLine, collection[0]);
		}

		public void TestDefaultsForNewChild()
		{
			var invoice = JobDeclaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.FDAs.AddNew();

			invoiceLine.FCCs.AddNew();
			invoiceLine.FCCs.AddNew();

			invoiceLine.DOTs.AddNew();
			invoiceLine.DOTs.AddNew();

			invoiceLine.FeeCusCodes.AddNew();
			invoiceLine.Charges.AddNew();
			invoiceLine.Charges.AddNew();

			invoiceLine.LaceyActLines.AddNew();
			invoiceLine.LaceyActLines[0].US_PGACommercialDescription = "clone pga line";
			invoiceLine.LaceyActLines[0].PG04ConstituentElements.AddNew();
			invoiceLine.LaceyActLines[0].PG04ConstituentElements[0].US_PGANameOfTheConstituentElement = "PINE";
			invoiceLine.LaceyActLines[0].PG04ConstituentElements[0].ScientificDataCollection.AddNew();
			invoiceLine.LaceyActLines[0].PG04ConstituentElements[0].ScientificDataCollection[0].US_PGAScientificGenusName = "Genus Name";

			invoiceLine.PSTLines.AddNew();
			invoiceLine.PSTLines.AddNew();

			invoiceLine.NHTSALines.AddNew();
			invoiceLine.NHTSALines.AddNew();

			invoiceLine.CPSCHeaders.AddNew();
			invoiceLine.CPSCHeaders.AddNew();
			invoiceLine.DEAHeaders.AddNew();
			invoiceLine.DEAHeaders.AddNew();

			var collection = GetCollectionToTest();
			collection.CopyLastLineDetailsToNewLines = true;
			var invoiceLine1 = collection.AddNew();

			AssertEquals(1, invoiceLine1.FeeCusCodes.Count);
			AssertEquals(2, invoiceLine1.FCCs.Count);
			AssertEquals(1, invoiceLine1.FDAs.Count);
			AssertEquals(2, invoiceLine1.DOTs.Count);
			AssertEquals(2, invoiceLine1.Charges.Count);
			AssertEquals(1, invoiceLine1.LaceyActLines.Count);
			AssertEquals(2, invoiceLine1.PSTLines.Count);
			AssertEquals(2, invoiceLine1.NHTSALines.Count);
			AssertEquals(2, invoiceLine1.CPSCHeaders.Count);
			AssertEquals(2, invoiceLine1.DEAHeaders.Count);
			AssertEquals("clone pga line", invoiceLine1.LaceyActLines[0].US_PGACommercialDescription);
			AssertEquals("PINE", invoiceLine1.LaceyActLines[0].PG04ConstituentElements[0].US_PGANameOfTheConstituentElement);
			AssertEquals("Genus Name", invoiceLine1.LaceyActLines[0].PG04ConstituentElements[0].ScientificDataCollection[0].US_PGAScientificGenusName);
		}

		public void TestCS00390450_CopyPreviousInvoiceLineDetails()
		{
			var org1 = Factory.New<MasterFiles.Business.OrgHeader>();
			org1.OH_Code = "TSTKNZ";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST1";
			var orgRel = product.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, MasterFiles.Business.OrgPartRelation.RelationshipTypes.Both);
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CI_OH = orgRel.OU_OH;

			var child1 = pivot1.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child1.CI_TariffNum = "10000001";
			child1.CI_OH = orgRel.OU_OH;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TEST1";

			AssertEquals("Invoice Lines created from product", 2, declaration.FilteredInvoiceLines.Count);

			var invoiceLine2 = declaration.FilteredInvoiceLines[1];
			AssertEquals("TEST1", invoiceLine2.JI_PartNo);
			AssertEquals("Read only", true, invoiceLine2.JI_PartNoInfo.ReadOnly);

			declaration.FilteredInvoiceLines.CopyLastLineDetailsToNewLines = true;

			var invoiceLine3 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine3.JI_PartNo);
			AssertEquals("Should not be read only", false, invoiceLine3.JI_PartNoInfo.ReadOnly);
		}

		public void TestCS00390450_InvLineProductIsNotSetWhenX_VsetCompletedFromProduct()
		{
			var org1 = Factory.New<MasterFiles.Business.OrgHeader>();
			org1.OH_Code = "TSTKNZ";

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST1";
			var orgRel = product.RelatedOrganisations.AddOrganisationIfNotExist(org1.PK, MasterFiles.Business.OrgPartRelation.RelationshipTypes.Both);
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CI_OH = orgRel.OU_OH;
			pivot1.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.X;

			var child1 = pivot1.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child1.CI_TariffNum = "10000000";
			child1.CI_OH = orgRel.OU_OH;
			child1.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;

			var child2 = pivot1.Children.AddNew();
			child2.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child2.CI_TariffNum = "10000002";
			child2.CI_OH = orgRel.OU_OH;
			child2.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.V;

			Factory.Save();

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org1.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TEST1";

			AssertEquals("Invoice Lines created from product", 3, declaration.FilteredInvoiceLines.Count);

			var invoiceLine4 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine4.JI_PartNo);
			AssertEquals("Should not be read only", false, invoiceLine4.JI_PartNoInfo.ReadOnly);
			AssertEquals(ZString.Empty, invoiceLine4.US_SecondarySPI);
		}

		public void TestCS00390450_InvLineProductCopiedDownIfEditable()
		{
			var importer = Factory.NewWithValidTestData<MasterFiles.Business.OrgHeader>();

			var product = Factory.New<OrgSupplierPart>();
			product.OP_PartNum = "TEST1";
			product.RelatedOrganisations.AddOwner(importer);

			var orgRel = product.RelatedOrganisations.AddOrganisationIfNotExist(importer.PK, MasterFiles.Business.OrgPartRelation.RelationshipTypes.Both);
			var pivot1 = product.PivotsForBinding.AddNew();
			pivot1.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot1.CI_TariffNum = "10000000";
			pivot1.CI_OH = orgRel.OU_OH;
			pivot1.CD_ProductClaim = SecondarySpecProgIndicatorList.Codes.X;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = importer.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_EnableENS = true;

			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_RX_NKInvoice_Currency = "USD";

			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "TEST1";
			AssertEquals("Product Number is editable", false, invoiceLine.JI_PartNoInfo.ReadOnly);

			var incoiceLine2 = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals("TEST1", incoiceLine2.JI_PartNo);
		}

		public void TestCS00390450_CreateInvoiceLinesFromProduct()
		{
			var consignee = Factory.New<MasterFiles.Business.OrgHeader>();
			consignee.FillWithValidTestData();
			consignee.OH_Code = "ORGUSCHI";
			consignee.OH_IsConsignee = ZBool.True;
			consignee.OH_RL_NKClosestPort = "USCHI";

			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "CHOCOLATE CHEWS";
			part.OP_Weight = 100m;
			part.OP_WeightUQ = "HG";
			part.RelatedOrganisations.AddOwner(consignee);

			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_UsageComment = "U1";
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_TariffNum = "1010101010";
			pivot.CI_SupplementalTariff = "9802008068";
			pivot.CD_UC_NKCountryOfOrigin = "CN";

			var child1 = pivot.Children.AddNew();
			child1.CI_ChildType = ClassificationChildTypeList.Codes.COMPONENT;
			child1.CI_TariffNum = "10000001";
			child1.CI_OH = consignee.PK;
			child1.CD_UC_NKCountryOfOrigin = "AU";

			var child2 = pivot.Children.AddNew();
			child2.CI_ChildType = ClassificationChildTypeList.Codes.Related;
			child2.CI_TariffNum = "10000002";
			child2.CI_OH = consignee.PK;
			child2.CD_UC_NKCountryOfOrigin = "AU";

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = consignee.PK;
			declaration.US_EntryFilerCode = "XJ5";
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;

			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = declaration.InvoiceLines.AddNew();
			invoiceLine.JI_PartNo = "CHOCOLATE CHEWS";
			AssertEquals("data copied", "CN", invoiceLine.US_UC_NKCountryOfOrigin);
			AssertEquals("1010101010", invoiceLine.JI_Tariff);
			AssertEquals("9802008068", invoiceLine.US_SupTariff);

			AssertEquals("Lines created from product", 3, declaration.FilteredInvoiceLines.Count);

			var invoiceLine2 = declaration.FilteredInvoiceLines[1];
			AssertEquals("data copied", "AU", invoiceLine2.US_UC_NKCountryOfOrigin);
			AssertEquals("10000001", invoiceLine2.JI_Tariff);
		}

		public void TestDefaultValueForACEDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.US_DRWExamWitness = true;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			Assert(invoiceLine.US_DRWExpNoticeInd);
		}

		public void TestDefaultAccountingMethodForACEDrawback()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Drawback;
			declaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
			declaration.US_DRWPurpose = DrawbackDeclarationPurposeList.Codes.CD;
			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._52;
			declaration.Invoices.AddNew();
			var invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(DrawbackAccountingMethodCodeList.Codes._00, invoiceLine.US_DRWAccMethod);

			declaration.US_EntryType = ACEDrawbackProvisionsList.Codes._51;
			invoiceLine = declaration.FilteredInvoiceLines.AddNew();
			AssertEquals(ZString.Empty, invoiceLine.US_DRWAccMethod);
		}

		protected override InvoiceLineViewCollection GetCollectionToTest() => new InvoiceLineViewCollection(JobDeclaration);

		new JobDeclaration JobDeclaration => (JobDeclaration)base.JobDeclaration;
	}
}
