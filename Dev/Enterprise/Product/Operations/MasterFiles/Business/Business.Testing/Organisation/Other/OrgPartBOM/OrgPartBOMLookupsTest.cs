using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgPartBOMLookupsTest : BusinessObjectLookupsTestCase
	{
		#region TestProductUQList

		public void TestProductUQList()
		{
			AssertNotNull(BOM.Lookups.ProductUQList);
		}

		#endregion

		#region TestSubParts

		public void TestSubParts()
		{
			OrgSupplierPart mainPart = Factory.New<OrgSupplierPart>();
			mainPart.OP_PartNum = "mainpart";

			OrgSupplierPart subPart = Factory.New<OrgSupplierPart>();
			subPart.OP_PartNum = "subpart";

			Factory.Save();

			BOM.OE_OP_MainProduct = mainPart.PK;
			OrgPartBOM secondLevelBOM = Factory.New<OrgPartBOM>();
			secondLevelBOM.OE_OP_MainProduct = subPart.PK;
			AssertNotNull(BOM.Lookups.SubParts);
			BOM.Lookups.SubParts.Load();
			AssertEquals(1, BOM.Lookups.SubParts.Count);
		}

		public void TestSubPartsBasic()
		{
			var orgPartBOM = Factory.New<OrgPartBOM>();
			AssertNotNull(orgPartBOM.Lookups.SubParts);
			AssertEquals(typeof(OrgSupplierPartCollection), orgPartBOM.Lookups.SubParts.GetType());
			AssertEquals("Collection should not be loaded.", 0, orgPartBOM.Lookups.SubParts.Count);
		}

		#endregion

		#region TestSubParts_FiltersCollectionByPartOwner

		public void TestSubParts_FiltersCollectionByPartOwner()
		{
			// Create a stock of inventory owned by a variety of orgs
			var owner1 = OrgHeader.New(Factory);
			owner1.OH_Code = "Owner1";
			var owner2 = OrgHeader.New(Factory);
			owner2.OH_Code = "Owner2";

			var owner1Part1 = CreateOrgOwnedSupplierPart("Owner1Part1", owner1);
			var owner1Part2 = CreateOrgOwnedSupplierPart("Owner1Part2", owner1);
			var owner2Part1 = CreateOrgOwnedSupplierPart("Owner2Part1", owner2);

			Factory.Save();

			// Create a Part and load the bom subparts for it, should return all
			var mainPart = OrgSupplierPart.New(Factory);
			mainPart.OP_PartNum = "MainPart";

			var orgBomPart = mainPart.BillOfMaterials.AddNew();
			var subParts = orgBomPart.Lookups.SubParts;
			subParts.Load();
			AssertEquals(3, subParts.Count);
			AssertContainsExactElementsInAnyOrder("Correct list of parts.", new[] { owner1Part1, owner1Part2, owner2Part1 }, subParts);

			// Set an owner for the part and recheck the subparts, should return subset
			var mainPartRel = mainPart.RelatedOrganisations.AddNew();
			mainPartRel.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;
			mainPartRel.OU_OH = owner1.PK;
			subParts = orgBomPart.Lookups.SubParts;
			subParts.Load();
			AssertEquals(2, subParts.Count);
			AssertContainsExactElementsInAnyOrder("Correct list of parts.", new[] { owner1Part1, owner1Part2 }, subParts);

			// Set to 2nd owner, recheck subparts, should return different subset
			mainPartRel.OU_OH = owner2.PK;
			subParts = orgBomPart.Lookups.SubParts;
			subParts.Load();
			AssertEquals(1, subParts.Count);
			AssertContainsExactElementsInAnyOrder("Correct list of parts.", new[] { owner2Part1 }, subParts);
		}

		OrgSupplierPart CreateOrgOwnedSupplierPart(ZString partNum, OrgHeader owner)
		{
			var part = OrgSupplierPart.New(Factory);
			part.OP_PartNum = partNum;

			var partRelationship = part.RelatedOrganisations.AddNew();
			partRelationship.OU_OH = owner.PK;
			partRelationship.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			return part;
		}

		#endregion

		#region Implementation

		OrgPartBOM BOM
		{
			get { return fBOM ?? (fBOM = Factory.New<OrgPartBOM>()); }
		}
		OrgPartBOM fBOM;

		#endregion

	}
}
