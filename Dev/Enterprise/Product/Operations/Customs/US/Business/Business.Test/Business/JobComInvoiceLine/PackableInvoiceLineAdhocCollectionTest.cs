using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(PackableInvoiceLineAdhocCollection))]
	sealed class PackableInvoiceLineAdhocCollectionTest : ActiveBusinessObjectCollectionTestCase<PackableInvoiceLineAdhocCollection>
	{
		public void TestWHSPacksAreRefreshAndDeltetedIfNeeded()
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
			var whsPacksRefreshed = false;
			((IBindingList)declaration.WHSPacks).ListChanged += (object sender, ListChangedEventArgs e) => { whsPacksRefreshed = true; };
			var whsPackLinesRefreshed = false;
			((IBindingList)declaration.WHSPackFilteredLines).ListChanged += (object sender, ListChangedEventArgs e) => { whsPackLinesRefreshed = true; };
			var invoice = declaration.Invoices.AddNew();
			whsPacksRefreshed = false;
			whsPackLinesRefreshed = false;
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			AssertEquals("whsPacksRefreshed", true, whsPacksRefreshed);
			AssertEquals("whsPackLinesRefreshed", true, whsPackLinesRefreshed);
			whsPacksRefreshed = false;
			whsPackLinesRefreshed = false;
			invoiceLine1.JI_PartNo = part.OP_PartNum;
			AssertEquals("whsPacksRefreshed", true, whsPacksRefreshed);
			AssertEquals("whsPackLinesRefreshed", true, whsPackLinesRefreshed);
			whsPacksRefreshed = false;
			whsPackLinesRefreshed = false;
			invoiceLine1.JI_InvoiceQuantity = 1m;
			AssertEquals("whsPacksRefreshed", true, whsPacksRefreshed);
			AssertEquals("whsPackLinesRefreshed", true, whsPackLinesRefreshed);
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			whsPacksRefreshed = false;
			whsPackLinesRefreshed = false;
			invoiceLine2.JI_InvoiceQuantity = 1m;
			AssertEquals("whsPacksRefreshed", true, whsPacksRefreshed);
			AssertEquals("whsPackLinesRefreshed", true, whsPackLinesRefreshed);
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = part.OP_PartNum;
			invoiceLine3.JI_InvoiceQuantity = 1m;
			AssertEquals(3, declaration.PackableInvoiceLines.Count);
			AssertCollectionContains(invoiceLine1, declaration.PackableInvoiceLines);
			AssertCollectionContains(invoiceLine2, declaration.PackableInvoiceLines);
			AssertCollectionContains(invoiceLine3, declaration.PackableInvoiceLines);

			var whsPacks1 = declaration.WHSPacks.AddNew();
			whsPacks1.US_PackageQty = 1;
			var whsPackLine1_1 = declaration.WHSPackLines.AddNew();
			whsPackLine1_1.US_B7_WHSPack = whsPacks1.PK;
			whsPackLine1_1.US_JI_InvoiceLine = invoiceLine1.PK;
			var whsPackLine1_2 = declaration.WHSPackLines.AddNew();
			whsPackLine1_2.US_B7_WHSPack = whsPacks1.PK;
			whsPackLine1_2.US_JI_InvoiceLine = invoiceLine2.PK;
			var whsPacks2 = declaration.WHSPacks.AddNew();
			whsPacks2.US_PackageQty = 1;
			var whsPackLine2_1 = declaration.WHSPackLines.AddNew();
			whsPackLine2_1.US_B7_WHSPack = whsPacks2.PK;
			whsPackLine2_1.US_JI_InvoiceLine = invoiceLine1.PK;
			var whsPackLine2_3 = declaration.WHSPackLines.AddNew();
			whsPackLine2_3.US_B7_WHSPack = whsPacks2.PK;
			whsPackLine2_3.US_JI_InvoiceLine = invoiceLine3.PK;
			var whsPacks3 = declaration.WHSPacks.AddNew();
			whsPacks3.US_PackageQty = 1;
			var whsPackLine3_3 = declaration.WHSPackLines.AddNew();
			whsPackLine3_3.US_B7_WHSPack = whsPacks3.PK;
			whsPackLine3_3.US_JI_InvoiceLine = invoiceLine3.PK;

			whsPacksRefreshed = false;
			whsPackLinesRefreshed = false;
			invoiceLine3.JI_InvoiceQuantity = 0;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertCollectionContains(invoiceLine1, declaration.PackableInvoiceLines);
			AssertCollectionContains(invoiceLine2, declaration.PackableInvoiceLines);
			AssertEquals("whsPacksRefreshed", true, whsPacksRefreshed);
			AssertEquals("whsPackLinesRefreshed", true, whsPackLinesRefreshed);
			AssertEquals("whsPacks1.IsDeleted", false, whsPacks1.IsDeleted);
			AssertEquals("whsPackLine1_1.IsDeleted", false, whsPackLine1_1.IsDeleted);
			AssertEquals("whsPackLine1_2.IsDeleted", false, whsPackLine1_2.IsDeleted);
			AssertEquals("whsPacks2.IsDeleted", false, whsPacks2.IsDeleted);
			AssertEquals("whsPackLine2_1.IsDeleted", false, whsPackLine2_1.IsDeleted);
			AssertEquals("whsPackLine2_3.IsDeleted", true, whsPackLine2_3.IsDeleted);
			AssertEquals("whsPacks3.IsDeleted", true, whsPacks3.IsDeleted);
			AssertEquals("whsPackLine3_3.IsDeleted", true, whsPackLine3_3.IsDeleted);

			whsPacksRefreshed = false;
			whsPackLinesRefreshed = false;
			invoiceLine2.Delete();
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertCollectionContains(invoiceLine1, declaration.PackableInvoiceLines);
			AssertEquals("whsPacksRefreshed", true, whsPacksRefreshed);
			AssertEquals("whsPackLinesRefreshed", true, whsPackLinesRefreshed);
			AssertEquals("whsPacks1.IsDeleted", false, whsPacks1.IsDeleted);
			AssertEquals("whsPackLine1_1.IsDeleted", false, whsPackLine1_1.IsDeleted);
			AssertEquals("whsPackLine1_2.IsDeleted", true, whsPackLine1_2.IsDeleted);
			AssertEquals("whsPacks2.IsDeleted", false, whsPacks2.IsDeleted);
			AssertEquals("whsPackLine2_1.IsDeleted", false, whsPackLine2_1.IsDeleted);

			whsPacksRefreshed = false;
			whsPackLinesRefreshed = false;
			invoiceLine1.Delete();
			AssertEquals(0, declaration.PackableInvoiceLines.Count);
			AssertEquals("whsPacksRefreshed", true, whsPacksRefreshed);
			AssertEquals("whsPackLinesRefreshed", true, whsPackLinesRefreshed);
			AssertEquals("whsPacks1.IsDeleted", true, whsPacks1.IsDeleted);
			AssertEquals("whsPackLine1_1.IsDeleted", true, whsPackLine1_1.IsDeleted);
			AssertEquals("whsPacks2.IsDeleted", true, whsPacks2.IsDeleted);
			AssertEquals("whsPackLine2_1.IsDeleted", true, whsPackLine2_1.IsDeleted);
		}

		public void TestWHSPacksAreDeletedWhenNoPackableLine()
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
			AssertEquals(typeof(PackableInvoiceLineAdhocCollection), declaration.PackableInvoiceLines.GetType());
			AssertEquals(0, declaration.PackableInvoiceLines.Count);
			var whsPacks1 = declaration.WHSPacks.AddNew();
			whsPacks1.US_PackageQty = 1;
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1m;
			AssertEquals(0, declaration.PackableInvoiceLines.Count);
			AssertEquals(0, declaration.WHSPacks.Count);
			whsPacks1 = declaration.WHSPacks.AddNew();
			whsPacks1.US_PackageQty = 1;
			invoiceLine1.JI_PartNo = part.OP_PartNum;
			AssertEquals(1, declaration.WHSPacks.Count);
			var whsPacks2 = declaration.WHSPacks.AddNew();
			whsPacks2.US_PackageQty = 2;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			AssertEquals(2, declaration.WHSPacks.Count);
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine2.JI_InvoiceQuantity = 0m;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(2, declaration.WHSPacks.Count);
			invoiceLine1.JI_InvoiceQuantity = 0m;
			AssertEquals(0, declaration.PackableInvoiceLines.Count);
			AssertEquals(0, declaration.WHSPacks.Count);
		}

		public void TestRightInvoiceLineIsAddedOrRemoved()
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
			AssertEquals(typeof(PackableInvoiceLineAdhocCollection), declaration.PackableInvoiceLines.GetType());
			AssertEquals(0, declaration.PackableInvoiceLines.Count);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_InvoiceQuantity = 1m;
			AssertEquals(0, declaration.PackableInvoiceLines.Count);
			invoiceLine1.JI_PartNo = part.OP_PartNum;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine2.JI_ParentID = invoiceLine1.PK;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			invoiceLine2.JI_ParentID = ZGuid.Empty;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine2.US_JI_ParentProduct = invoiceLine1.PK;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			invoiceLine2.US_JI_ParentProduct = ZGuid.Empty;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine2.JI_BondedWhsQuantity = 1m;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			invoiceLine2.JI_BondedWhsQuantity = ZDecimal.Zero;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine1.JI_OP = ZGuid.Empty;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine1.JI_OP = part.PK;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine2.JI_InvoiceQuantity = 0m;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			invoiceLine2.JI_InvoiceQuantity = 1m;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
			invoiceLine2.Delete();
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));

			var invoice2 = Factory.New<JobComInvoiceHeader>();
			invoiceLine2 = invoice2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_InvoiceQuantity = 1m;
			invoiceLine2.JI_OP = part.PK;
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			invoice2.JZ_JE = declaration.PK;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));

			declaration.PackableInvoiceLines.UnHookEvents();
			AssertEquals(0, declaration.PackableInvoiceLines.Count);
			invoiceLine2.JI_BondedWhsQuantity = 10m;
			AssertEquals(0, declaration.PackableInvoiceLines.Count);

			declaration.PackableInvoiceLines.HookEventsAndReload();
			AssertEquals(1, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			invoiceLine2.JI_BondedWhsQuantity = 0m;
			AssertEquals(2, declaration.PackableInvoiceLines.Count);
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine1));
			AssertEquals(true, declaration.PackableInvoiceLines.Contains(invoiceLine2));
		}

		protected override PackableInvoiceLineAdhocCollection GetCollectionToTest()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			return new PackableInvoiceLineAdhocCollection(declaration);
		}
	}
}
