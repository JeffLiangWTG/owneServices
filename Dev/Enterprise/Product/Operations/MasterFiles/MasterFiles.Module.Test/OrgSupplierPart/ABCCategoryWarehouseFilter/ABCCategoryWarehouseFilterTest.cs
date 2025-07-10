using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(ABCCategoryWarehouseFilter))]
	sealed class ABCCategoryWarehouseFilterTest : ModuleFilterTestCase<ABCCategoryWarehouseFilter>
	{
		#region TestIsEmpty

		public void TestIsEmpty()
		{
			var filter = new ABCCategoryWarehouseFilter();
			AssertEquals(true, filter.IsEmpty);

			filter.WJ_Category = "A";
			AssertEquals(false, filter.IsEmpty);

			filter.WJ_WW_Warehouse = ZGuid.NewZGuid();
			AssertEquals(false, filter.IsEmpty);

			filter.WJ_Category = "";
			AssertEquals(false, filter.IsEmpty);

			filter.WJ_WW_Warehouse = ZGuid.Empty;
			AssertEquals(true, filter.IsEmpty);
		}

		#endregion

		#region TestIsExpensiveQuery

		public override void TestIsExpensiveQuery()
		{
			AssertEquals(false, Filter.IsExpensiveQuery);
		}

		#endregion

		#region TestQuery

		[TestDate(2012, 2, 17)]
		public void TestQuery()
		{
			var warehouse1 = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var part1 = GetPart("BOW", "Arrow Bow", warehouse1.PK, "A");
			var abcCategory1 = Factory.LoadTop1(ObjectFactory.GetType<IWhsABCCategory>(), new ZQuery());
			var outOfDateABCCategory = Factory.New(ObjectFactory.GetType<IWhsABCCategory>());
			outOfDateABCCategory[WhsABCCategorySchema.WJ_OH_Client] = abcCategory1[WhsABCCategorySchema.WJ_OH_Client];
			outOfDateABCCategory[WhsABCCategorySchema.WJ_OP_Product] = abcCategory1[WhsABCCategorySchema.WJ_OP_Product];
			outOfDateABCCategory[WhsABCCategorySchema.WJ_WW_Warehouse] = abcCategory1[WhsABCCategorySchema.WJ_WW_Warehouse];
			outOfDateABCCategory[WhsABCCategorySchema.WJ_Category] = "B";
			outOfDateABCCategory[WhsABCCategorySchema.WJ_AnalysisDateFrom] = new ZDateTime(2011, 1, 1);
			outOfDateABCCategory[WhsABCCategorySchema.WJ_AnalysisDateTo] = new ZDateTime(2011, 1, 1);

			var part2 = GetPart("WOD", "Wood Arrow", warehouse1.PK, "B");
			var warehouse2 = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());
			var part3 = GetPart("EXE", "Executable", warehouse2.PK, "A");
			Factory.Save();

			var filterStripBizO = new OrgSupplierPartFilterStripBusinessObject();
			var filter = (ABCCategoryWarehouseFilter)filterStripBizO["ABC Category / Warehouse"];
			filter.IsActive = true;
			var products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Since filter is empty loads all products", new OrgSupplierPart[] { part1, part2, part3 }, products);

			filter.WJ_Category = "A";
			products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should load all products with ABC Category A", new OrgSupplierPart[] { part1, part3 }, products);

			filter.WJ_WW_Warehouse = warehouse1.PK;
			products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should load all products with ABC Category A and warehouse 1", new OrgSupplierPart[] { part1 }, products);

			filter.WJ_WW_Warehouse = warehouse2.PK;
			filter.WJ_Category = "B";
			products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertEquals("Should load no products", 0, products.Length);

			filter.WJ_WW_Warehouse = warehouse1.PK;
			products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should load all products with ABC Category B and warehouse 1", new OrgSupplierPart[] { part2 }, products);

			filter.WJ_WW_Warehouse = warehouse2.PK;
			filter.WJ_Category = "A";
			products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should load all products with ABC Category A and warehouse 2", new OrgSupplierPart[] { part3 }, products);

			var abcCategory2 = Factory.LoadTop1(ObjectFactory.GetType<IWhsABCCategory>(), new ZQuery(WhsABCCategorySchema.WJ_OP_Product, part3.PK));
			var newABCCategory = Factory.New(ObjectFactory.GetType<IWhsABCCategory>());
			newABCCategory[WhsABCCategorySchema.WJ_OP_Product] = abcCategory2[WhsABCCategorySchema.WJ_OP_Product];
			newABCCategory[WhsABCCategorySchema.WJ_OH_Client] = abcCategory2[WhsABCCategorySchema.WJ_OH_Client];
			newABCCategory[WhsABCCategorySchema.WJ_WW_Warehouse] = warehouse1.PK;
			newABCCategory[WhsABCCategorySchema.WJ_Category] = "B";
			newABCCategory[WhsABCCategorySchema.WJ_AnalysisDateFrom] = ZDateTime.Today;
			newABCCategory[WhsABCCategorySchema.WJ_AnalysisDateTo] = ZDateTime.Today;
			Factory.Save();

			filter.WJ_WW_Warehouse = ZGuid.Empty;
			filter.WJ_Category = "B";
			products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should load all products with ABC Category B", new OrgSupplierPart[] { part2, part3 }, products);

			filter.WJ_Category = "A";
			products = Factory.Load<OrgSupplierPart>(filterStripBizO.Filter);
			AssertContainsExactElementsInAnyOrder("Should load all products with ABC Category A", new OrgSupplierPart[] { part1, part3 }, products);
		}

		OrgSupplierPart GetPart(ZString partNumber, ZString partDescription, ZGuid warehouse, ZString abcCategoryName)
		{
			var part = Factory.New<OrgSupplierPart>();
			part.OP_PartNum = partNumber;
			part.OP_Desc = partDescription;

			var client = Factory.NewWithValidTestData<OrgHeader>();
			var relation = part.RelatedOrganisations.AddNew();
			relation.OU_OH = client.PK;
			relation.OU_Relationship = OrgPartRelation.RelationshipTypes.Owner;

			var abcCategory = Factory.New(ObjectFactory.GetType<IWhsABCCategory>());
			abcCategory[WhsABCCategorySchema.WJ_OP_Product] = part.PK;
			abcCategory[WhsABCCategorySchema.WJ_OH_Client] = client.PK;
			abcCategory[WhsABCCategorySchema.WJ_WW_Warehouse] = warehouse;
			abcCategory[WhsABCCategorySchema.WJ_Category] = abcCategoryName;
			abcCategory[WhsABCCategorySchema.WJ_AnalysisDateFrom] = ZDateTime.Today;
			abcCategory[WhsABCCategorySchema.WJ_AnalysisDateTo] = ZDateTime.Today;

			return part;
		}

		#endregion

		#region Implementation

		protected override FilterCategory InitialTestCatergory
		{
			get { return FilterCategories.ModesAndTypes; }
		}

		protected override FilterCategory ExpectedDefaultCategory
		{
			get { return FilterCategories.Other; }
		}

		protected override ABCCategoryWarehouseFilter GetNewModuleFilter()
		{
			return new ABCCategoryWarehouseFilter();
		}

		protected override ZString ExpectedDescription
		{
			get { return "ABC Category / Warehouse"; }
		}

		#endregion
	}
}
