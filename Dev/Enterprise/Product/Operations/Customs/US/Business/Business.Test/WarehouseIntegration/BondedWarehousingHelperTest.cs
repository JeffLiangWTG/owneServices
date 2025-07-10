using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class BondedWarehousingHelperTest : TestCaseWithFactory
	{
		public void TestIsMarkedForBondedWarehousing()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EnableENS = true;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var helper = new BondedWarehousingHelper(declaration);
			AssertEquals(true, helper.IsMarkedForBondedWarehousing(invoiceLine));
			declaration.US_EnableENS = false;
			AssertEquals(false, helper.IsMarkedForBondedWarehousing(invoiceLine));
			declaration.US_EnableENS = true;
			AssertEquals(true, helper.IsMarkedForBondedWarehousing(invoiceLine));
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_ParentID = invoiceLine2.PK;
			AssertEquals(false, helper.IsMarkedForBondedWarehousing(invoiceLine));
			invoiceLine.JI_ParentID = ZGuid.Empty;
			AssertEquals(true, helper.IsMarkedForBondedWarehousing(invoiceLine));

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			AssertEquals(true, helper.IsMarkedForBondedWarehousing(invoiceLine));

			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			AssertEquals(true, helper.IsMarkedForBondedWarehousing(invoiceLine));
		}

		public void TestHasBondedWarehouseEntryDetails()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = "PART321";
			var orgRel = part.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			var helper = new BondedWarehousingHelper(declaration);
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			declaration.US_EnableENS = true;
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.US_WHSEntryLineNo = 1;
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			declaration.US_EntryType = EntryTypeList.Codes.ConsumptionFTZ;
			declaration.US_EnableENS = true;
			declaration.US_EnableCRL = false;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.US_WHSEntryNumber = "SDF";
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.US_WHSEntryLineNo = 0;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));

			declaration.JE_MessageType = JobMessageTypeList.Codes.ImportByExternalBroker;
			AssertEquals(false, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.US_WHSEntryLineNo = 1;
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
			invoiceLine.US_WHSEntryNumber = "";
			AssertEquals(true, helper.HasBondedWarehouseEntryDetails(invoiceLine, false, false, false));
		}

		public void TestUpdateWarehouseWithdrawal()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;

			declaration.US_EntryType = EntryTypeList.Codes.WarehouseWithdrawalConsumption;
			declaration.US_EnableCRL = true;
			declaration.US_EnableENS = false;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 50m;
			invoiceLine1.SetDeclarationForTesting(declaration);
			invoiceLine1.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 75m;
			invoiceLine2.SetDeclarationForTesting(declaration);
			invoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			var whsDataHelper = new WhsDataTestHelper(Factory);
			whsDataHelper.Importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = whsDataHelper.Importer.PK;
			var helper = new BondedWarehousingHelper(declaration);

			declaration.US_QtyInWHBeforeWithdrawal = 150m;
			declaration.US_QtyBeingWithdrawn = 0m;
			AssertEquals(150m, declaration.US_QtyInWHBeforeWithdrawal);
			AssertEquals(0m, declaration.US_QtyBeingWithdrawn);
			AssertEquals(150m, declaration.US_QtyInWHAfterWithdrawal);
			AssertEquals(false, declaration.US_IsFinalWHS);

			helper.UpdateWarehouseWithdrawal();
			AssertEquals(150m, declaration.US_QtyInWHBeforeWithdrawal);
			AssertEquals(125m, declaration.US_QtyBeingWithdrawn);
			AssertEquals(25m, declaration.US_QtyInWHAfterWithdrawal);
			AssertEquals(false, declaration.US_IsFinalWHS);

			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_InvoiceQuantity = 25m;
			invoiceLine3.SetDeclarationForTesting(declaration);
			invoiceLine3.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			helper.UpdateWarehouseWithdrawal();
			AssertEquals(150m, declaration.US_QtyInWHBeforeWithdrawal);
			AssertEquals(150m, declaration.US_QtyBeingWithdrawn);
			AssertEquals(0m, declaration.US_QtyInWHAfterWithdrawal);
			AssertEquals(true, declaration.US_IsFinalWHS);
		}
	}
}
