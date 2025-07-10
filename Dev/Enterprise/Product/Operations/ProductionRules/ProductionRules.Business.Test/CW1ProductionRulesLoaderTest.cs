using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Schema;
using WTG.ProductionRules.Core;

namespace Enterprise.ProductionRules.Business.Testing
{
	class CW1ProductionRulesLoaderTest : TestCaseWithFactory
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>(() => new CW1ProductionRulesLoader(null));
		}

		public void TestLoadRules()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", true);
			var rule1 = Helper.CreateRule(ruleSet, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet, "2", "2", 2);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules(ruleSet.PRS_Context, ruleSet.PRS_ContextSubType, ProductionRuleSetFilter.Empty);
			AssertRulesReturned(ruleSet.PRS_Context, ruleSet.PRS_ContextSubType, new[] { rule1, rule2 }, rules);
		}

		public void TestLoadRules_Empty()
		{
			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules("PRE", string.Empty, ProductionRuleSetFilter.Empty);
			AssertEquals(0, rules.Count());
		}

		public void TestLoadRules_DifferentContext()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", true);
			var rule1 = Helper.CreateRule(ruleSet, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet, "2", "2", 2);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules("ABC", string.Empty, ProductionRuleSetFilter.Empty);
			AssertEquals(0, rules.Count());
		}

		public void TestLoadRules_Warehouse_Filters()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS", "A");

			var anyOtherRuleSetsQuery = new ZQuery(ProductionRuleSetSchema.PRS_Context, "PWP");
			anyOtherRuleSetsQuery.AddToFilter(ProductionRuleSetSchema.PRS_IsLive, true);

			var anyOtherRuleSet = Factory.LoadTop1<ProductionRuleSet>(anyOtherRuleSetsQuery);
			if (anyOtherRuleSet != null)
			{
				anyOtherRuleSet.PRS_IsLive = false;
			}

			var ruleSet = Helper.CreateRuleSet("A", "A", true, warehousePK: whs1.PK);
			var rule1 = Helper.CreateRule(ruleSet, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet, "2", "2", 2);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rulesDifferentWarehouse = loader.LoadRules("PWP", string.Empty, ProductionRuleSetFilter.WithWarehouse(Guid.NewGuid()));
			AssertEquals(0, rulesDifferentWarehouse.Count());

			var rules = loader.LoadRules("PWP", string.Empty, ProductionRuleSetFilter.WithWarehouse(whs1.PK.ToGuid()));
			AssertRulesReturned(ruleSet.PRS_Context, ruleSet.PRS_ContextSubType, new[] { rule1, rule2 }, rules);

			var rulesNoWarehouse = loader.LoadRules("PWP", string.Empty, ProductionRuleSetFilter.Empty);
			AssertEquals(0, rulesNoWarehouse.Count());
		}

		public void TestLoadRules_Warehouse_PrefersMatch()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS", "A");

			var ruleSet1 = Helper.CreateRuleSet("A", "A", true, warehousePK: whs1.PK);
			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1", 1);
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2", 2);

			var ruleSet2 = Helper.CreateRuleSet("B", "B", true);
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1", 1);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules("PWP", string.Empty, ProductionRuleSetFilter.WithWarehouse(whs1.PK.ToGuid()));
			AssertRulesReturned(ruleSet1.PRS_Context, ruleSet1.PRS_ContextSubType, new[] { rule1_1, rule1_2 }, rules);
		}

		public void TestLoadRules_Warehouse_Fallsback()
		{
			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS", "A");
			var whs2 = whsHelper.CreateWarehouse("WH2", "A");

			var ruleSet1 = Helper.CreateRuleSet("A", "A", true, warehousePK: whs1.PK);
			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1", 1);
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2", 2);

			var ruleSet2 = Helper.CreateRuleSet("B", "B", true);
			var rule2_1 = Helper.CreateRule(ruleSet2, "1", "1", 1);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules("PWP", string.Empty, ProductionRuleSetFilter.WithWarehouse(whs2.PK.ToGuid()));
			AssertRulesReturned(ruleSet2.PRS_Context, ruleSet2.PRS_ContextSubType, new[] { rule2_1 }, rules);
		}

		public void TestLoadRules_Company()
		{
			var company1 = Factory.NewWithValidTestData<GlbCompany>();
			var company2 = Factory.NewWithValidTestData<GlbCompany>();

			var context = "JBR";
			var contextSubType = "SHP";

			var ruleSet1 = Helper.CreateRuleSet("A", "A", true, context: context, contextSubType: contextSubType, companyPK: company1.PK);
			var rule1_1 = Helper.CreateRule(ruleSet1, "1", "1", 1);
			var rule1_2 = Helper.CreateRule(ruleSet1, "2", "2", 2);

			var ruleSet2 = Helper.CreateRuleSet("B", "B", true, context: context, contextSubType: contextSubType);
			var rule2_1 = Helper.CreateRule(ruleSet2, "3", "3", 3);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules(context, contextSubType, ProductionRuleSetFilter.WithCompany(company2.PK.ToGuid()));
			AssertRulesReturned(context, contextSubType, new[] { rule2_1 }, rules); //should fallback to find ruleset with empty company

			rules = loader.LoadRules(context, contextSubType, ProductionRuleSetFilter.WithCompany(company1.PK.ToGuid()));
			AssertRulesReturned(context, contextSubType, new[] { rule1_2, rule1_1 }, rules);

			rules = loader.LoadRules(context, contextSubType, ProductionRuleSetFilter.Empty);
			AssertRulesReturned(context, contextSubType, new[] { rule2_1 }, rules);
		}

		public void TestLoadRules_NoActiveRuleSets()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", false);
			var rule1 = Helper.CreateRule(ruleSet, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet, "2", "2", 2);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules(ruleSet.PRS_Context, ruleSet.PRS_ContextSubType, ProductionRuleSetFilter.Empty);
			AssertEquals(0, rules.Count());
		}

		public void TestLoadRules_DifferentRuleSet()
		{
			var ruleSet1 = Helper.CreateRuleSet("A", "A", true);
			var rule1 = Helper.CreateRule(ruleSet1, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet1, "2", "2", 2);

			var ruleSet2 = Helper.CreateRuleSet("B", "B", false);
			var rule3 = Helper.CreateRule(ruleSet2, "3", "3", 3);
			var rule4 = Helper.CreateRule(ruleSet2, "4", "4", 4);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadRules(ruleSet1.PRS_Context, ruleSet1.PRS_ContextSubType, ProductionRuleSetFilter.Empty);
			AssertRulesReturned(ruleSet1.PRS_Context, ruleSet1.PRS_ContextSubType, new[] { rule1, rule2 }, rules);
		}

		public void TestLoadRules_DbHits()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", true);

			var rules = new List<ProductionRule>();
			for (int i = 0; i < 1000; i++)
			{
				var rule = Helper.CreateRule(ruleSet, i.ToString(), i.ToString(), 1);
				rule.PRL_RuleDefinition = @"{
	""conditions"": [
		{
			""propertyPath"": ""RequiredTemp"",
			""operation"": ""between"",
			""value"": 4,
			""value2"": 6
		}
	],
	""action"": {
		""type"": ""putAwayToLocation"",
		""actionState"": {
			""conditions"": [
				{
					""propertyPath"": ""AreaName"",
					""operation"": ""startsWith"",
					""value"": ""COOL1""
				}
			],
			""orderBy"": [
				{
				
					""propertyPath"": ""RowName"",
					""direction"": ""Asc""
				},
				{
					""propertyPath"": ""Column"",
					""direction"": ""Asc""
				},
				{
					""propertyPath"": ""Level"",
					""direction"": ""Asc""
				},
				{
					""propertyPath"": ""Tray"",
					""direction"": ""Asc""
				}
			]
		}
	}
}
";
				rules.Add(rule);
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loader = new CW1ProductionRulesLoader(newFactory);
			var loadedRules = loader.LoadRules(ruleSet.PRS_Context, ruleSet.PRS_ContextSubType, ProductionRuleSetFilter.Empty);
			AssertRulesReturned(ruleSet.PRS_Context, ruleSet.PRS_ContextSubType, rules, loadedRules);
			AssertTableHitCount("Should have hit the table once.", 1, ProductionRuleSchema.Constants.TableName, newFactory);
		}

		public void TestLoadAllRulesIncludingInactive()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.ProductionRule;");

			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS", "A");
			var whs2 = whsHelper.CreateWarehouse("WH2", "A");

			var ruleSet1 = Helper.CreateRuleSet("A", "A", true);
			var rule1 = Helper.CreateRule(ruleSet1, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet1, "2", "2", 2);

			var ruleSet2 = Helper.CreateRuleSet("B", "B", false);
			var rule3 = Helper.CreateRule(ruleSet2, "3", "3", 3);
			var rule4 = Helper.CreateRule(ruleSet2, "4", "4", 4);

			var ruleSet3 = Helper.CreateRuleSet("C", "C", true, warehousePK: whs1.PK);
			var rule5 = Helper.CreateRule(ruleSet3, "5", "5", 5);
			var rule6 = Helper.CreateRule(ruleSet3, "6", "6", 6);

			var ruleSet4 = Helper.CreateRuleSet("D", "D", false, warehousePK: whs2.PK);
			var rule7 = Helper.CreateRule(ruleSet3, "7", "7", 7);
			var rule8 = Helper.CreateRule(ruleSet3, "8", "8", 8);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadAllRulesIncludingInactive(ruleSet1.PRS_Context, ruleSet1.PRS_ContextSubType);
			AssertContainsExactElementsInAnyOrder(Enumerable.Range(1, 8), rules.Select(r => (int)r.Priority));
		}

		public void TestLoadAllRulesIncludingInactive_DifferentContext()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", true);
			var rule1 = Helper.CreateRule(ruleSet, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet, "2", "2", 2);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadAllRulesIncludingInactive("ABC", string.Empty);
			AssertEquals(0, rules.Count());
		}

		public void TestLoadAllRulesIncludingInactive_DifferentContextSubType()
		{
			var ruleSet = Helper.CreateRuleSet("A", "A", true, context: "JBR", contextSubType: "SHP");
			var rule1 = Helper.CreateRule(ruleSet, "1", "1", 1);
			var rule2 = Helper.CreateRule(ruleSet, "2", "2", 2);
			Factory.Save();

			var loader = new CW1ProductionRulesLoader(Factory);
			var rules = loader.LoadAllRulesIncludingInactive("JBR", "CON");
			AssertEquals(0, rules.Count());

			rules = loader.LoadAllRulesIncludingInactive("JBR", "SHP");
			AssertContainsExactElementsInAnyOrder(Enumerable.Range(1, 2), rules.Select(r => (int)r.Priority));
		}

		public void TestLoadAllRulesIncludingInactive_DbHits()
		{
			TestConnection.ExecuteNonQuery("DELETE FROM dbo.ProductionRule;");

			var whsHelper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
			var whs1 = whsHelper.CreateWarehouse("WHS", "A");
			var whs2 = whsHelper.CreateWarehouse("WH2", "A");

			var ruleSet1 = Helper.CreateRuleSet("A", "A", true);
			var ruleSet2 = Helper.CreateRuleSet("B", "B", false);
			var ruleSet3 = Helper.CreateRuleSet("C", "C", true, warehousePK: whs1.PK);
			var ruleSet4 = Helper.CreateRuleSet("D", "D", false, warehousePK: whs2.PK);
			var ruleSets = new[] { ruleSet1, ruleSet2, ruleSet3, ruleSet4 };

			var rules = new List<ProductionRule>();
			for (int i = 0; i < 1000; i++)
			{
				var rule = Helper.CreateRule(ruleSets[i % 4], i.ToString(), i.ToString(), 1);
				rule.PRL_RuleDefinition = @"{
	""conditions"": [
		{
			""propertyPath"": ""RequiredTemp"",
			""operation"": ""between"",
			""value"": 4,
			""value2"": 6
		}
	],
	""action"": {
		""type"": ""putAwayToLocation"",
		""actionState"": {
			""conditions"": [
				{
					""propertyPath"": ""AreaName"",
					""operation"": ""startsWith"",
					""value"": ""COOL1""
				}
			],
			""orderBy"": [
				{
				
					""propertyPath"": ""RowName"",
					""direction"": ""Asc""
				},
				{
					""propertyPath"": ""Column"",
					""direction"": ""Asc""
				},
				{
					""propertyPath"": ""Level"",
					""direction"": ""Asc""
				},
				{
					""propertyPath"": ""Tray"",
					""direction"": ""Asc""
				}
			]
		}
	}
}
";
				rules.Add(rule);
			}
			Factory.Save();

			var newFactory = new BusinessObjectFactory { RefreshEnabled = false };
			var loader = new CW1ProductionRulesLoader(newFactory);
			var loadedRules = loader.LoadAllRulesIncludingInactive(ruleSet1.PRS_Context, ruleSet1.PRS_ContextSubType);
			AssertRulesReturned(ruleSet1.PRS_Context, ruleSet1.PRS_ContextSubType, rules, loadedRules);
			AssertTableHitCount("Should have hit the table once.", 1, ProductionRuleSchema.Constants.TableName, newFactory);
		}

		#region Implementation

		void AssertRulesReturned(string expectedContext, string expectedContextSubType, IEnumerable<ProductionRule> expected, IEnumerable<IProductionRule> actual)
		{
			CombineAssertions(() =>
			{
				AssertContainsExactElementsInAnyOrder(nameof(IProductionRule.Name), expected.Select(r => r.PRL_Name), actual.Select(r => r.Name));
				AssertContainsExactElementsInAnyOrder(nameof(IProductionRule.Context), new[] { expectedContext }, actual.Select(r => r.Context).Distinct());
				AssertContainsExactElementsInAnyOrder(nameof(IProductionRule.ContextSubType), new[] { expectedContextSubType }, actual.Select(r => r.ContextSubType).Distinct());
				AssertContainsExactElementsInAnyOrder(nameof(IProductionRule.Description), expected.Select(r => r.PRL_Description), actual.Select(r => r.Description));
				AssertContainsExactElementsInAnyOrder(nameof(IProductionRule.Definition), expected.Select(r => r.PRL_RuleDefinition), actual.Select(r => r.Definition));
				AssertContainsExactElementsInAnyOrder(nameof(IProductionRule.Priority), expected.Select(r => r.PRL_Priority), actual.Select(r => r.Priority));

				AssertEquals("Should not use bizos to allow multi-threading.", false, actual.OfType<BusinessObject>().Any());
				AssertPersistentPropertiesHitCount("Should not use bizos to allow multi-threading.", typeof(ProductionRule), 0,
					() =>
					{
						if (actual.Any())
						{
							var properties = actual.First().GetType().GetProperties();

							foreach (var actualRule in actual)
							{
								_ = properties.Select(p => p.GetValue(actualRule)).ToArray();
							}
						}
					});
			});
		}

		Helper Helper => helper ?? (helper = new Helper(Factory));
		Helper helper;

		protected override void SetUp()
		{
			base.SetUp();

			var prepSql =
@"UPDATE dbo.ProductionRuleSet
SET
	PRS_IsLive = 0,
	PRS_SystemLastEditTimeUtc = SYSUTCDATETIME(),
	PRS_SystemLastEditUser = '~BP'
WHERE
	PRS_IsLive = 1 AND PRS_Context = 'PWP';";
			TestConnection.ExecuteNonQuery(prepSql);
		}

		#endregion
	}
}
