using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ImporterSupplierModuleGuidsWithListFilter))]
	sealed class ImporterSupplierModuleGuidsWithListFilterTest : ModuleGuidsFilterTest
	{
		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		protected override FilterCategory InitialTestCatergory
		{
			get { return FilterCategories.Other; }
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Organisations; }
		}

		protected override ModuleGuidsFilter GetNewModuleFilter()
		{
			return new ImporterSupplierModuleGuidsWithListFilter(new ConsigneeCollection(Factory), new ConsignorCollection(Factory), null);
		}

		protected override ZString ExpectedDescription
		{
			get { return "Importer/Supplier"; }
		}

		[ExpectNoExceptions]
		public void TestImporterSupplierFilterWithParentRelationship()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			var supplierParent = OrgParent(supplier, "SUP_PARENT");
			var buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";
			var buyerParent = OrgParent(buyer, "BUY_PARENT");
			AssertFilter(buyerParent, supplierParent, buyer, supplier);
		}

		[ExpectNoExceptions]
		public void TestImporterSupplierFilter()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			var buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";
			AssertFilter(buyer, supplier, buyer, supplier);
		}

		void AssertFilter(
			OrgHeader relatedBuyer,
			OrgHeader relatedSupplier,
			OrgHeader filterBuyer,
			OrgHeader filterSupplier) => CombineAssertions(() =>
		{
			var otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.OH_Code = "OTHERORG1";

			var part1 = NewPart("PART1", relatedBuyer, OrgPartRelation.RelationshipTypes.Owner, relatedSupplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part2 = NewPart("PART2", null, "", relatedSupplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part3 = NewPart("PART3", relatedBuyer, OrgPartRelation.RelationshipTypes.Owner, null, "");
			var part4 = NewPart("PART4", relatedBuyer, OrgPartRelation.RelationshipTypes.Both, relatedSupplier, OrgPartRelation.RelationshipTypes.Both);
			var part5 = NewPart("PART5", null, "", relatedSupplier, OrgPartRelation.RelationshipTypes.Both);
			var part6 = NewPart("PART6", relatedBuyer, OrgPartRelation.RelationshipTypes.Both, null, "");
			var part7 = NewPart("PART7", relatedBuyer, OrgPartRelation.RelationshipTypes.Owner, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			var part8 = NewPart("PART8", relatedBuyer, OrgPartRelation.RelationshipTypes.Both, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);

			Factory.Save();

			var filter = new ImporterSupplierModuleGuidsWithListFilter(new ConsigneeCollection(Factory), new ConsignorCollection(Factory), null);
			filter.FilterCondition = "~";
			AssertEquals("Unknown Filter Condition", "?", filter.FilterCondition);

			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Loose;
			filter.Property1 = filterBuyer.PK;
			filter.Property2 = filterSupplier.PK;

			var parts = new OrgSupplierPartCollection(Factory, filter.Query);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Loose match",
				new OrgSupplierPart[] { part1, part2, part3, part4, part5, part6, part7, part8 }, parts);

			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Exact;
			var parts2 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts2.Load();

			AssertContainsExactElementsInAnyOrder("Exact match", new OrgSupplierPart[] { part1, part4 }, parts2);

			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Exclusive;
			var parts3 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts3.Load();

			AssertEquals("Exclusive match", part1, parts3.Single());

			var changer = new OrgSupplierBulkRelationshipChanger(Factory);

			changer.FromOrganisationPK = relatedBuyer.PK;
			changer.FromRelationship = OrgPartRelation.RelationshipTypes.Owner;
			changer.ToOrganisationPK = otherOrg1.PK;
			changer.ToRelationship = OrgPartRelation.RelationshipTypes.Both;

			changer.ChangeRelatedOrganisations(filter.Query);
		});

		[ExpectNoExceptions]
		public void TestFilterForExactCondition() => CombineAssertions(() =>
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";

			var owner = OrgHeader.New(Factory);
			owner.OH_Code = "BUYER";

			var otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.OH_Code = "OTHERORG1";

			var part1 = NewPart("PART1", owner, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty);
			var part2 = NewPart("PART2", owner, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty);
			var part3 = NewPart("PART3", owner, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty);
			var part4 = NewPart("PART4", owner, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part5 = NewPart("PART5", owner, OrgPartRelation.RelationshipTypes.Owner, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			Factory.Save();

			var filter = new ImporterSupplierModuleGuidsWithListFilter(new ConsigneeCollection(Factory), new ConsignorCollection(Factory), null);
			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Exact;
			filter.Property1 = owner.PK;

			var parts = new OrgSupplierPartCollection(Factory, filter.Query);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Test Case1", new OrgSupplierPart[] { part1, part2, part3 }, parts);

			var part6 = NewPart("PART6", owner, OrgPartRelation.RelationshipTypes.Both, null, ZString.Empty);
			Factory.Save();

			var parts2 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts2.Load();

			AssertContainsExactElementsInAnyOrder("Test Case2", new OrgSupplierPart[] { part1, part2, part3, part6 }, parts2);

			filter.Property2 = supplier.PK;

			var parts3 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts3.Load();

			AssertEquals("Test Case3", part4, parts3.Single());

			var part7 = NewPart("PART7", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part8 = NewPart("PART8", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part9 = NewPart("PART9", null, ZString.Empty, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			Factory.Save();

			filter.Property1 = ZGuid.Empty;

			var parts4 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts4.Load();

			AssertContainsExactElementsInAnyOrder("Test Case4", new OrgSupplierPart[] { part7, part8 }, parts4);
		});

		[ExpectNoExceptions]
		public void TestFilterForExclusiveCondition() => CombineAssertions(() =>
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";

			var owner = OrgHeader.New(Factory);
			owner.OH_Code = "BUYER";

			var otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.OH_Code = "OTHERORG1";

			var part1 = NewPart("PART1", owner, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty);
			var part2 = NewPart("PART2", owner, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty);
			var part3 = NewPart("PART3", owner, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty);
			var part4 = NewPart("PART4", owner, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part5 = NewPart("PART5", owner, OrgPartRelation.RelationshipTypes.Owner, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			var part6 = NewPart("PART6", owner, OrgPartRelation.RelationshipTypes.Both, null, ZString.Empty);
			var part7 = NewPart("PART7", owner, OrgPartRelation.RelationshipTypes.Both, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			Factory.Save();

			var filter = new ImporterSupplierModuleGuidsWithListFilter(new ConsigneeCollection(Factory), new ConsignorCollection(Factory), null);
			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Exclusive;
			filter.Property1 = owner.PK;

			var parts = new OrgSupplierPartCollection(Factory, filter.Query);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Test Case1", new OrgSupplierPart[] { part1, part2, part3 }, parts);

			var part8 = NewPart("PART8", owner, OrgPartRelation.RelationshipTypes.Both, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			Factory.Save();

			var parts2 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts2.Load();

			AssertContainsExactElementsInAnyOrder("Test Case2", new OrgSupplierPart[] { part1, part2, part3 }, parts2);

			filter.Property2 = supplier.PK;

			var parts3 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts3.Load();

			AssertEquals("Test Case3", part4, parts3.Single());

			var part9 = NewPart("PART9", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Both);
			var part10 = NewPart("PART10", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part11 = NewPart("PART11", null, ZString.Empty, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			Factory.Save();

			filter.Property1 = ZGuid.Empty;

			var parts4 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts4.Load();

			AssertContainsExactElementsInAnyOrder("Test Case4", new OrgSupplierPart[] { part8, part10 }, parts4);
		});

		[ExpectNoExceptions]
		public void TestBothImporterSupplierFilter()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			var buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";
			var otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.OH_Code = "OTHERORG1";

			var part1 = NewPart("PART1", buyer, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part2 = NewPart("PART2", buyer, OrgPartRelation.RelationshipTypes.Owner, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			var part3 = NewPart("PART3", buyer, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part4 = NewPart("PART4", buyer, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty);
			var part5 = NewPart("PART5", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part6 = NewPart("PART6", null, ZString.Empty, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier);
			var part7 = NewPart("PART7", otherOrg1, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part8 = NewPart("PART8", otherOrg1, OrgPartRelation.RelationshipTypes.Supplier, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part9 = NewPart("PART9", otherOrg1, OrgPartRelation.RelationshipTypes.Owner, buyer, OrgPartRelation.RelationshipTypes.Owner);
			var part10 = NewPart("PART10", buyer, OrgPartRelation.RelationshipTypes.Both, supplier, OrgPartRelation.RelationshipTypes.Supplier);
			var part11 = NewPart("PART11", buyer, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Both);
			var part12 = NewPart("PART12", buyer, OrgPartRelation.RelationshipTypes.Both, null, ZString.Empty);
			var part13 = NewPart("PART13", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Both);

			Factory.Save();

			var filter = new ImporterSupplierModuleGuidsWithListFilter(new ConsigneeCollection(Factory), new ConsignorCollection(Factory), null);
			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Both;
			filter.Property1 = buyer.PK;
			filter.Property2 = supplier.PK;

			var parts = new OrgSupplierPartCollection(Factory, filter.Query);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Test Case1",
				new OrgSupplierPart[] { part1, part3, part4, part5, part8, part9, part10, part11, part12, part13 }, parts);

			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Both;
			filter.Property2 = ZGuid.Empty;
			var parts2 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts2.Load();

			AssertContainsExactElementsInAnyOrder("Test Case2", new OrgSupplierPart[] { part4, part9, part12 }, parts2);

			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Both;
			filter.Property1 = ZGuid.Empty;
			filter.Property2 = supplier.PK;

			var parts3 = new OrgSupplierPartCollection(Factory, filter.Query);
			parts3.Load();

			AssertContainsExactElementsInAnyOrder("Test Case3", new OrgSupplierPart[] { part5, part8, part13 }, parts3);
		}

		[ExpectNoExceptions]
		public void TestBothImporterSupplierFilterWithProductPredicates()
		{
			OrgSupplierPartFilterStripBusinessObject filterStrip = new OrgSupplierPartFilterStripBusinessObject();

			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			var buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";
			var otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.OH_Code = "OTHERORG1";

			var part1 = NewPart("PART1", buyer, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier, "Description1");
			var part2 = NewPart("PART2", buyer, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier, "Description2");
			var part3 = NewPart("PART3", buyer, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier, "Description3");
			var part4 = NewPart("PART4", buyer, OrgPartRelation.RelationshipTypes.Owner, null, ZString.Empty, "Description4");
			var part5 = NewPart("PART5", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Supplier, "Description5");
			var part6 = NewPart("PART6", null, ZString.Empty, otherOrg1, OrgPartRelation.RelationshipTypes.Supplier, "Description6");
			var part7 = NewPart("PART7", otherOrg1, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Supplier, "Description7");
			var part8 = NewPart("PART8", otherOrg1, OrgPartRelation.RelationshipTypes.Supplier, supplier, OrgPartRelation.RelationshipTypes.Supplier, "Description8");
			var part9 = NewPart("PART9", otherOrg1, OrgPartRelation.RelationshipTypes.Owner, buyer, OrgPartRelation.RelationshipTypes.Owner, "Description9");
			var part10 = NewPart("PART10", buyer, OrgPartRelation.RelationshipTypes.Both, supplier, OrgPartRelation.RelationshipTypes.Supplier, "Description10");
			var part11 = NewPart("PART11", buyer, OrgPartRelation.RelationshipTypes.Owner, supplier, OrgPartRelation.RelationshipTypes.Both, "Description11");
			var part12 = NewPart("PART12", buyer, OrgPartRelation.RelationshipTypes.Both, null, ZString.Empty, "Description12");
			var part13 = NewPart("PART13", null, ZString.Empty, supplier, OrgPartRelation.RelationshipTypes.Both, "Description13");

			Factory.Save();

			var filterImporterSupplier = (ImporterSupplierModuleGuidsWithListFilter)filterStrip["Importer/Supplier"];
			filterImporterSupplier.FilterCondition = ImporterSuplierFilterConditions.Codes.Both;
			filterImporterSupplier.Property1 = buyer.PK;
			filterImporterSupplier.Property2 = supplier.PK;
			filterImporterSupplier.IsActive = true;
			var filterProductCode1 = (ModuleTextFilter)filterStrip["Product Code"];
			filterProductCode1.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filterProductCode1.Property = "PART1";
			filterProductCode1.IsActive = true;

			AssertContains("OP_PartNum like 'PART1%'", filterImporterSupplier.Query.LiteralTextADO);

			var parts = new OrgSupplierPartCollection(Factory, filterStrip.Filter);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Test Case1", new OrgSupplierPart[] { part1, part10, part11, part12, part13 }, parts);

			filterProductCode1.IsActive = false;

			var filterProductDescription1 = (ModuleTextFilter)filterStrip["Product Description"];
			filterProductDescription1.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filterProductDescription1.Property = "2";
			filterProductDescription1.IsActive = true;

			AssertNotContains("OP_PartNum like 'PART1%'", filterImporterSupplier.Query.LiteralTextADO);
			AssertContains("OP_Desc like '%2%'", filterImporterSupplier.Query.LiteralTextADO);

			parts = new OrgSupplierPartCollection(Factory, filterStrip.Filter);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Test Case2", new OrgSupplierPart[] { part2, part12 }, parts);

			filterProductDescription1.SqlComparisonOperator = SQLComparisonOperator.NotContains;
			filterProductDescription1.Property = "3";
			filterProductCode1.Property = "PART1";
			filterProductCode1.IsActive = true;

			AssertContains("OP_PartNum like 'PART1%' and OP_Desc not like '%3%'", filterImporterSupplier.Query.LiteralTextADO);

			parts = new OrgSupplierPartCollection(Factory, filterStrip.Filter);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Test Case3", new OrgSupplierPart[] { part1, part10, part11, part12 }, parts);

			filterProductCode1.IsActive = false;
			filterProductDescription1.IsActive = false;

			AssertNotContains("OP_PartNum", filterImporterSupplier.Query.LiteralTextADO);
			AssertNotContains("OP_Desc", filterImporterSupplier.Query.LiteralTextADO);

			// Test ImporterSupplierFilter with multiple OR category groups
			filterProductCode1.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filterProductCode1.Property = "PART1";
			filterProductCode1.OrCategory = FilterOrCategory.Red;
			filterProductCode1.IsActive = true;
			var filterProductCode2 = filterStrip.ModuleFilters.AddTextFilter("Product Code (1)", OrgSupplierPartSchema.OP_PartNum);
			filterProductCode2.SqlComparisonOperator = SQLComparisonOperator.StartsWith;
			filterProductCode2.Property = "PART2";
			filterProductCode2.OrCategory = FilterOrCategory.Red;
			filterProductCode2.IsActive = true;
			filterProductDescription1.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filterProductDescription1.Property = "2";
			filterProductDescription1.OrCategory = FilterOrCategory.Blue;
			filterProductDescription1.IsActive = true;
			var filterProductDescription2 = filterStrip.ModuleFilters.AddTextFilter("Product Description (1)", OrgSupplierPartSchema.OP_Desc);
			filterProductDescription2.SqlComparisonOperator = SQLComparisonOperator.Contains;
			filterProductDescription2.Property = "3";
			filterProductDescription2.OrCategory = FilterOrCategory.Blue;
			filterProductDescription2.IsActive = true;

			AssertContains("(OP_PartNum like 'PART1%' or OP_PartNum like 'PART2%') and (OP_Desc like '%2%' or OP_Desc like '%3%')", filterImporterSupplier.Query.LiteralTextADO);

			parts = new OrgSupplierPartCollection(Factory, filterStrip.Filter);
			parts.Load();

			AssertContainsExactElementsInAnyOrder("Test Case4", new OrgSupplierPart[] { part2, part12, part13 }, parts);
		}

		[ExpectNoExceptions()]
		public void TestNoSQLInjectionExceptionThrown()
		{
			var supplier = OrgHeader.New(Factory);
			supplier.OH_Code = "SUPPLIER";
			var buyer = OrgHeader.New(Factory);
			buyer.OH_Code = "BUYER";

			var filter = new ImporterSupplierModuleGuidsWithListFilter(new ConsigneeCollection(Factory), new ConsignorCollection(Factory), null);
			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Both;
			filter.Property1 = buyer.PK;
			filter.Property2 = supplier.PK;

			DynamicBusinessObjectCollection collection = new DynamicBusinessObjectCollection(Factory);
			collection.Load("select OP_PK from dbo.OrgSupplierPart" + filter.Query.GetAsWhereAndOrderByClause(false), filter.Query.Params);
		}

		public void TestPerformance()
		{
			// performance tested on SYD-WBJG-1 OdysseyToll database, which contains 1,990,228 products.

			var supplier = OrgHeader.New(Factory);
			supplier.FillWithValidTestData();
			supplier.OH_Code = "SUPPLIER";

			var buyer = OrgHeader.New(Factory);
			buyer.FillWithValidTestData();
			buyer.OH_Code = "BUYER";

			var otherOrg1 = OrgHeader.New(Factory);
			otherOrg1.FillWithValidTestData();
			otherOrg1.OH_Code = "OTHERORG1";

			var filter = new ImporterSupplierModuleGuidsWithListFilter(new ConsigneeCollection(Factory), new ConsignorCollection(Factory), null);
			filter.FilterCondition = ImporterSuplierFilterConditions.Codes.Both;
			filter.Property1 = buyer.PK;
			filter.Property2 = supplier.PK;

			AssertEquals(@"Performance tested on SYD-WBJG-1 OdysseyToll database, which contains approximately 2 million (1,990,228) products.
if you will plan to change Imported/Supplier filter Query - please verify performance.",
				string.Format(@"
OP_PK IN 
(
	SELECT OrgSupplierPart.OP_PK
	FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK

	JOIN 

	(
		SELECT OP_PK
		FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK
		WHERE (OU_Relationship = 'SUP' OR OU_Relationship = 'BTH') AND OU_OH IN (CONVERT('{1}', 'System.Guid'))
	) AS relation2 ON OrgSupplierPart.OP_PK = relation2.OP_PK

	WHERE (OU_Relationship = 'OWN' OR OU_Relationship = 'BTH') AND OU_OH IN (CONVERT('{0}', 'System.Guid'))

	UNION ALL

	SELECT OrgSupplierPart.OP_PK
	FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK
	WHERE (OU_Relationship = 'OWN' OR OU_Relationship = 'BTH') AND OU_OH IN (CONVERT('{0}', 'System.Guid'))

	AND OrgSupplierPart.OP_PK NOT IN
		(
			SELECT OU_OP 
			FROM dbo.OrgPartRelation 
			WHERE OU_Relationship = 'SUP' 
			OR 
			(OU_Relationship = 'BTH' AND OU_OH NOT IN (CONVERT('{0}', 'System.Guid')))) 

	UNION ALL

	SELECT OrgSupplierPart.OP_PK
	FROM dbo.OrgSupplierPart JOIN dbo.OrgPartRelation ON OU_OP = OP_PK
	WHERE (OU_Relationship = 'SUP' OR OU_Relationship = 'BTH') AND OU_OH IN (CONVERT('{1}', 'System.Guid'))

	AND OrgSupplierPart.OP_PK NOT IN
	(
		SELECT OU_OP 
		FROM dbo.OrgPartRelation 
		WHERE OU_Relationship = 'OWN' 
		OR 
		(OU_Relationship = 'BTH' AND OU_OH NOT IN (CONVERT('{1}', 'System.Guid')))) 
)", buyer.PK, supplier.PK),
				filter.Query.LiteralTextADO);
		}

		OrgSupplierPart NewPart(string partCode, OrgHeader owner, string ownerRealtionship, OrgHeader supplier, string supplierRelationship, string partDescription = null)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partCode;
			part.OP_Desc = string.IsNullOrEmpty(partDescription) ? partCode : partDescription;
			if (owner != null)
			{
				var relation1 = part.RelatedOrganisations.AddNew();
				relation1.OU_Relationship = ownerRealtionship;
				relation1.OU_OH = owner.PK;
			}
			if (supplier != null)
			{
				var relation2 = part.RelatedOrganisations.AddNew();
				relation2.OU_Relationship = supplierRelationship;
				relation2.OU_OH = supplier.PK;
			}
			return part;
		}

		OrgHeader OrgParent(OrgHeader org, string parentOrgCode)
		{
			var orgParent = OrgHeader.New(Factory);
			orgParent.OH_Code = parentOrgCode;
			var relatedParty = orgParent.AllRelatedParties.AddNew();
			relatedParty.PR_OH_Parent = orgParent.PK;
			relatedParty.PR_OH_RelatedParty = org.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.ProductRelationship;
			return orgParent;
		}
	}
}
