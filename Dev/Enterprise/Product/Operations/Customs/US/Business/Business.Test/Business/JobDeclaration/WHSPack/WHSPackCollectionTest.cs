using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(WHSPackCollection))]
	public class WHSPackCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestPackageReferenceDefaulting()
		{
			var whsPacks = Declaration.WHSPacks;
			var pack1 = whsPacks.AddNew();
			AssertEquals("PACK1", pack1.US_PackageReference);
			var pack2 = whsPacks.AddNew();
			AssertEquals("PACK1", pack1.US_PackageReference);
			AssertEquals("PACK2", pack2.US_PackageReference);
			var pack3 = whsPacks.AddNew();
			AssertEquals("PACK1", pack1.US_PackageReference);
			AssertEquals("PACK2", pack2.US_PackageReference);
			AssertEquals("PACK3", pack3.US_PackageReference);
			var pack4 = whsPacks.AddNew();
			AssertEquals("PACK1", pack1.US_PackageReference);
			AssertEquals("PACK2", pack2.US_PackageReference);
			AssertEquals("PACK3", pack3.US_PackageReference);
			AssertEquals("PACK4", pack4.US_PackageReference);
			pack3.Delete();
			AssertEquals("PACK1", pack1.US_PackageReference);
			AssertEquals("PACK2", pack2.US_PackageReference);
			AssertEquals("PACK4", pack4.US_PackageReference);

			pack2.US_PackageReference = "PACK1";
			AssertEquals("PACK1", pack1.US_PackageReference);
			AssertEquals("PACK1", pack2.US_PackageReference);
			AssertEquals("PACK4", pack4.US_PackageReference);
		}

		public void TestUniquePackageReferenceList()
		{
			var whsPack1 = Declaration.WHSPacks.AddNew();
			whsPack1.US_PackageReference = "BOX 2";
			var list = Declaration.WHSPacks.UniquePackageReferenceList;
			AssertEquals(1, list.Count);
			AssertEquals("BOX 2", ((ICodeDescription)list[0]).Code);
			var whsPack2 = Declaration.WHSPacks.AddNew();
			whsPack2.US_PackageReference = "BOX 1";
			list = Declaration.WHSPacks.UniquePackageReferenceList;
			AssertEquals(2, list.Count);
			AssertEquals("BOX 1", ((ICodeDescription)list[0]).Code);
			AssertEquals("BOX 2", ((ICodeDescription)list[1]).Code);
			var whsPack3 = Declaration.WHSPacks.AddNew();
			whsPack3.US_PackageReference = "BOX 3";
			list = Declaration.WHSPacks.UniquePackageReferenceList;
			AssertEquals(3, list.Count);
			AssertEquals("BOX 1", ((ICodeDescription)list[0]).Code);
			AssertEquals("BOX 2", ((ICodeDescription)list[1]).Code);
			AssertEquals("BOX 3", ((ICodeDescription)list[2]).Code);
			whsPack1.Delete();
			list = Declaration.WHSPacks.UniquePackageReferenceList;
			AssertEquals(2, list.Count);
			AssertEquals("BOX 1", ((ICodeDescription)list[0]).Code);
			AssertEquals("BOX 3", ((ICodeDescription)list[1]).Code);
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
			var whsPacks = declaration.WHSPacks;
			AssertEquals("AllowNew", false, whsPacks.AllowNew);
			var invoice = declaration.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = part.OP_PartNum;
			invoiceLine.JI_InvoiceQuantity = 1m;
			AssertEquals("AllowNew", true, whsPacks.AllowNew);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new WHSPackCollection(Declaration);
		}

		JobDeclaration Declaration
		{
			get { return declaration ?? (declaration = Factory.New<JobDeclaration>()); }
		}
		JobDeclaration declaration;

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<WHSPack>();
		}
	}
}
