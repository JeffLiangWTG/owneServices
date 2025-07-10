using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OverallStaffCommissionRuleFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchStrategy_FetchForView()
		{
			AddRulesForFetchStrategyTest();

			var properties = new[]
				{
					OverallStaffCommissionRule.Schema.Status,
					OverallStaffCommissionRule.Schema.CommissionBasis,
					OverallStaffCommissionRule.Schema.CommissionTriggerType,
				};

			var expectedDbHits = new Dictionary<string, int>();
			expectedDbHits.Add(AccCommissionRuleStaffDisableSchema.Constants.TableName, 1);
			expectedDbHits.Add(AccCommissionRuleStaffOverrideSchema.Constants.TableName, 1);

			foreach (var property in properties)
			{
				AssertFetchForViewDbHits(new[] { property }, expectedDbHits);
			}
			AssertFetchForViewDbHits(properties, expectedDbHits);
		}

		void AssertFetchForViewDbHits(string[] viewColumnNames, Dictionary<string, int> expectedDbHits)
		{
			var viewFactory = new BusinessObjectFactory();
			var testRules =
				viewFactory.Load<AccGroupCommissionRule>(new ZQuery(AccCommissionRuleSchema.PK, testGroupRulePks)).Select(groupRule => new OverallStaffCommissionRule(staff, groupRule))
				.Union(viewFactory.Load<AccStaffCommissionRule>(new ZQuery(AccCommissionRuleSchema.PK, testStaffRulePks)).Select(staffRule => new OverallStaffCommissionRule(staffRule)));
			viewFactory.ResetDatabaseLoadCount();

			foreach (var testRule in testRules)
			{
				testRule.FetchStrategy.FetchForView(viewColumnNames.Select(x => new TableColumn("", x)).ToArray());
			}

			foreach (var testRule in testRules)
			{
				object hitProperty;
				foreach (var columnName in viewColumnNames)
				{
					hitProperty = testRule[columnName];
				}
			}

			CombineAssertions(string.Join(", ", viewColumnNames), () =>
			{
				foreach (var expectedDbHit in expectedDbHits)
				{
					var actualHitCount = viewFactory.GetTableHitCount(expectedDbHit.Key);
					Assert("Hit count for table " + expectedDbHit.Key + " should be less than or equal to " + expectedDbHit.Value + " - but was " + actualHitCount, actualHitCount <= expectedDbHit.Value);
				}
			});
		}

		void AddRulesForFetchStrategyTest()
		{
			salesTeam = Factory.New<SalesTeam>();
			staff = salesTeam.Staff.AddNew();
			staff.FillWithValidTestData();

			testGroupRulePks = new List<ZGuid>();
			for (var i = 0; i < 5; i++)
			{
				var teamRule = salesTeam.CommissionRules.AddNew();
				teamRule.FillWithValidTestData();
				var rule = new OverallStaffCommissionRule(staff, teamRule);
				rule.Status = GroupCommissionRuleStatusTypes.Codes.Disabled;
				testGroupRulePks.Add(rule.PK);
			}

			for (var i = 0; i < 5; i++)
			{
				var teamRule = salesTeam.CommissionRules.AddNew();
				teamRule.FillWithValidTestData();
				var rule = new OverallStaffCommissionRule(staff, teamRule);
				rule.Status = GroupCommissionRuleStatusTypes.Codes.Inherited;
				testGroupRulePks.Add(rule.PK);
			}

			for (var i = 0; i < 5; i++)
			{
				var teamRule = salesTeam.CommissionRules.AddNew();
				teamRule.FillWithValidTestData();
				var rule = new OverallStaffCommissionRule(staff, teamRule);
				rule.Status = GroupCommissionRuleStatusTypes.Codes.Overridden;
				testGroupRulePks.Add(rule.PK);
			}

			testStaffRulePks = new List<ZGuid>();
			for (var i = 0; i < 5; i++)
			{
				var staffRule = staff.CommissionRules.AddNew();
				staffRule.FillWithValidTestData();
				var rule = new OverallStaffCommissionRule(staffRule);
				testStaffRulePks.Add(rule.PK);
			}

			Factory.Save();
		}

		SalesTeam salesTeam;
		GlbStaff staff;
		List<ZGuid> testGroupRulePks;
		List<ZGuid> testStaffRulePks;
	}
}
