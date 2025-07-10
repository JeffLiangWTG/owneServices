namespace Enterprise.ProductionRules.Business.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.ProductionRules.Integration;
	using Enterprise.Warehouse.Integration;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ProductionRuleSet))]
	class ProductionRuleSetTest : EnterpriseBusinessObjectTestCase
	{
		public void TestWarehouse()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A");
			AssertNull(ruleSet.Warehouse);

			var warehouse = Factory.New<IWhsWarehouse>();
			ruleSet.PRS_WW_Warehouse = warehouse.PK;
			AssertEquals(warehouse, ruleSet.Warehouse);
		}

		public void TestRules()
		{
			var ruleSetA = Helper.CreateRuleSet("A", "A");
			var ruleSetB = Helper.CreateRuleSet("B", "B");

			var ruleA_1 = Helper.CreateRule(ruleSetA, "A1", "A1");
			var ruleA_2 = Helper.CreateRule(ruleSetA, "A2", "A2");
			var ruleA_3 = Helper.CreateRule(ruleSetA, "A3", "A3");

			var ruleB_1 = Helper.CreateRule(ruleSetB, "B1", "B1");
			var ruleB_2 = Helper.CreateRule(ruleSetB, "B2", "B2");

			AssertContainsExactElementsInAnyOrder(new[] { ruleA_1, ruleA_2, ruleA_3 }, ruleSetA.Rules);
			AssertContainsExactElementsInAnyOrder(new[] { ruleB_1, ruleB_2 }, ruleSetB.Rules);
		}

		public void TestIProductionRuleSetMembers()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();

			var ruleSet = Helper.CreateRuleSet("A", "A");
			ruleSet.PRS_Context = "ABC";
			ruleSet.PRS_ContextSubType = "DEF";
			ruleSet.PRS_WW_Warehouse = guid1;
			ruleSet.PRS_GC_Company = guid2;

			var iRuleSet = (IProductionRuleSet)ruleSet;
			AssertEquals(nameof(ruleSet.PRS_Context), "ABC", iRuleSet.PRS_Context);
			AssertEquals(nameof(ruleSet.PRS_ContextSubType), "DEF", iRuleSet.PRS_ContextSubType);
			AssertEquals(nameof(ruleSet.PRS_WW_Warehouse), guid1, iRuleSet.PRS_WW_Warehouse);
			AssertEquals(nameof(ruleSet.PRS_GC_Company), guid2, iRuleSet.PRS_GC_Company);
		}

		protected override BusinessObject GetNewBusinessObject() => GetProductionRuleSetForBaseTests(Helper);
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetProductionRuleSetForBaseTests(new Helper(factory));
		static ProductionRuleSet GetProductionRuleSetForBaseTests(Helper helper) => helper.CreateRuleSet("A", "A", false);

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}
}
