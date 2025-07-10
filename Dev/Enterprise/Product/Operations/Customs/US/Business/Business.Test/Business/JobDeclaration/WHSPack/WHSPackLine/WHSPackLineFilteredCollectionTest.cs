using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(WHSPackLineFilteredCollection))]
	public class WHSPackLineFilteredCollectionTest : SubsetBusinessObjectCollectionTestCase<WHSPackLineFilteredCollection, WHSPackLine>
	{
		public void TestFilterData()
		{
			var org = Factory.New<OrgHeader>();
			org.CompanyData.OB_IMUsedBondedWhs = true;
			var classification = Factory.New<CusClassification>();
			classification.CC_LookupCode = "LK234";
			classification.CC_TariffNum = "2010304050";
			var part1 = Factory.New<OrgSupplierPart>();
			part1.OP_PartNum = "PD3234";
			var orgRel = part1.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			var pivot = part1.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var part2 = Factory.New<OrgSupplierPart>();
			part2.OP_PartNum = "PD2353";
			orgRel = part2.RelatedOrganisations.AddOrganisationIfNotExist(org.PK, OrgPartRelation.RelationshipTypes.Owner);
			pivot = part2.PivotsForBinding.AddNew();
			pivot.CI_ChildType = ClassificationTypeList.Codes.HTI;
			pivot.CI_OH = orgRel.OU_OH;
			pivot.CI_CC = classification.PK;
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_SystemCreateTimeUtc = ZDateTime.Now;
			declaration.JE_OH_Importer = org.PK;
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.US_EntryType = EntryTypeList.Codes.Warehouse;
			declaration.US_EnableENS = false;
			var whsPack1 = declaration.WHSPacks.AddNew();
			var whsPack2 = declaration.WHSPacks.AddNew();
			var whsPack3 = declaration.WHSPacks.AddNew();
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine1.JI_PartNo = part1.OP_PartNum;
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_PartNo = part2.OP_PartNum;
			var invoiceLine3 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine3.JI_PartNo = part2.OP_PartNum;
			var invoiceLine4 = invoice.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = part1.OP_PartNum;

			var whsPackLine1_1 = declaration.WHSPackLines.AddNew();
			whsPackLine1_1.US_B7_WHSPack = whsPack1.PK;
			whsPackLine1_1.US_JI_InvoiceLine = invoiceLine1.PK;
			var whsPackLine2_1 = declaration.WHSPackLines.AddNew();
			whsPackLine2_1.US_B7_WHSPack = whsPack2.PK;
			whsPackLine2_1.US_JI_InvoiceLine = invoiceLine1.PK;
			var whsPackLine3_1 = declaration.WHSPackLines.AddNew();
			whsPackLine3_1.US_B7_WHSPack = whsPack3.PK;
			whsPackLine3_1.US_JI_InvoiceLine = invoiceLine1.PK;
			var whsPackLine1_2 = declaration.WHSPackLines.AddNew();
			whsPackLine1_2.US_B7_WHSPack = whsPack1.PK;
			whsPackLine1_2.US_JI_InvoiceLine = invoiceLine2.PK;
			var whsPackLine2_2 = declaration.WHSPackLines.AddNew();
			whsPackLine2_2.US_B7_WHSPack = whsPack2.PK;
			whsPackLine2_2.US_JI_InvoiceLine = invoiceLine2.PK;
			var whsPackLine3_2 = declaration.WHSPackLines.AddNew();
			whsPackLine3_2.US_B7_WHSPack = whsPack3.PK;
			whsPackLine3_2.US_JI_InvoiceLine = invoiceLine2.PK;
			var whsPackLine1_3 = declaration.WHSPackLines.AddNew();
			whsPackLine1_3.US_B7_WHSPack = whsPack1.PK;
			whsPackLine1_3.US_JI_InvoiceLine = invoiceLine3.PK;
			var whsPackLine2_3 = declaration.WHSPackLines.AddNew();
			whsPackLine2_3.US_B7_WHSPack = whsPack2.PK;
			whsPackLine2_3.US_JI_InvoiceLine = invoiceLine3.PK;
			var whsPackLine3_3 = declaration.WHSPackLines.AddNew();
			whsPackLine3_3.US_B7_WHSPack = whsPack3.PK;
			whsPackLine3_3.US_JI_InvoiceLine = invoiceLine3.PK;
			var whsPackLine3_0 = declaration.WHSPackLines.AddNew();
			whsPackLine3_0.US_B7_WHSPack = whsPack3.PK;
			whsPackLine3_0.US_JI_InvoiceLine = ZGuid.Empty;
			var whsPackLine0_1 = declaration.WHSPackLines.AddNew();
			whsPackLine0_1.US_B7_WHSPack = ZGuid.Empty;
			whsPackLine0_1.US_JI_InvoiceLine = invoiceLine1.PK;
			var whsPackLine0_0 = declaration.WHSPackLines.AddNew();
			whsPackLine0_0.US_B7_WHSPack = ZGuid.Empty;
			whsPackLine0_0.US_JI_InvoiceLine = ZGuid.Empty;
			var whsPackLine1_4 = declaration.WHSPackLines.AddNew();
			whsPackLine1_4.US_B7_WHSPack = whsPack1.PK;
			whsPackLine1_4.US_JI_InvoiceLine = invoiceLine4.PK;
			var whsPackLine2_4 = declaration.WHSPackLines.AddNew();
			whsPackLine2_4.US_B7_WHSPack = whsPack2.PK;
			whsPackLine2_4.US_JI_InvoiceLine = invoiceLine4.PK;
			var whsPackLine3_4 = declaration.WHSPackLines.AddNew();
			whsPackLine3_4.US_B7_WHSPack = whsPack3.PK;
			whsPackLine3_4.US_JI_InvoiceLine = invoiceLine4.PK;

			AssertEquals(15, declaration.WHSPackFilteredLines.Count);
			declaration.WHSPackageFilter = whsPack1.PK;
			AssertEquals(4, declaration.WHSPackFilteredLines.Count);
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_2));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_3));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_4));

			declaration.WHSInvLineFilter = invoiceLine1.PK;
			AssertEquals(1, declaration.WHSPackFilteredLines.Count);
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_1));

			declaration.WHSPackageFilter = ZGuid.Empty;
			AssertEquals(4, declaration.WHSPackFilteredLines.Count);
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine0_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine2_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_1));

			declaration.WHSPackageFilter = whsPack3.PK;
			AssertEquals(1, declaration.WHSPackFilteredLines.Count);
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_1));

			declaration.WHSInvLineFilter = ZGuid.Empty;
			AssertEquals(5, declaration.WHSPackFilteredLines.Count);
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_0));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_2));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_3));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_4));

			declaration.WHSProductFilter = part1.OP_PartNum;
			AssertEquals(2, declaration.WHSPackFilteredLines.Count);
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_4));

			declaration.WHSPackageFilter = ZGuid.Empty;
			AssertEquals(7, declaration.WHSPackFilteredLines.Count);
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine0_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine1_4));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine2_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine2_4));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_1));
			AssertEquals(true, declaration.WHSPackFilteredLines.Contains(whsPackLine3_4));
		}

		public void TestAllowNew()
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
			var whsPackFilteredLines = declaration.WHSPackFilteredLines;
			AssertEquals("AllowNew", false, whsPackFilteredLines.AllowNew);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			AssertEquals("AllowNew", true, whsPackFilteredLines.AllowNew);
		}

		#region Implementation

		protected override WHSPackLineFilteredCollection GetCollectionToTest()
		{
			return WHSPackLines;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var result = Factory.New<WHSPackLine>();
			result.US_PackedQty = 1;
			return result;
		}

		WHSPackLineFilteredCollection WHSPackLines
		{
			get { return whsPackLines ?? (whsPackLines = new WHSPackLineFilteredCollection(Declaration)); }
		}
		WHSPackLineFilteredCollection whsPackLines;

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		#endregion
	}
}
