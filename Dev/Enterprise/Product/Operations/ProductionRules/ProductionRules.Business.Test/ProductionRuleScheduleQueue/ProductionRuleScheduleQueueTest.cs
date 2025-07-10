namespace Enterprise.ProductionRules.Business.Testing
{
	using CargoWise.Application;
	using CargoWise.EntityFramework;
	using Enterprise.Warehouse.Integration;
	using Enterprise.ZArchitecture.Business.Testing;
	using NUnit.Framework;

	[TestedType(typeof(ProductionRuleScheduleQueue))]
	class ProductionRuleScheduleQueueTest : EnterpriseBusinessObjectTestCase
	{
		public void TestRule()
		{
			var queue = Factory.New<ProductionRuleScheduleQueue>();
			AssertNull("Rule should be null.", queue.Rule);

			var rule = Factory.New<ProductionRule>();
			queue.PRQ_PRL_Rule = rule.PK;
			AssertEquals("Should return setup rule.", rule, queue.Rule);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var warehouse = Factory.NewWithValidTestData(ObjectFactory.GetType<IWhsWarehouse>());

			var ruleSet = Helper.CreateRuleSet("TEST", "TEST", warehousePK: warehouse.PK);
			var rule = Helper.CreateRule(ruleSet, "TEST", "TEST");
			rule.PRL_RuleDefinition = $@"{{
    ""conditions"": [
    ],
    ""action"": {{
        ""$type"": ""PutawayActionState"",
        ""conditions"": [
        ],
        ""sortByCriteria"": [
        ]
    }}
}}";
			var queue = Factory.New<ProductionRuleScheduleQueue>();
			queue.PRQ_PRL_Rule = rule.PK;
			return queue;
		}

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;
	}
}
