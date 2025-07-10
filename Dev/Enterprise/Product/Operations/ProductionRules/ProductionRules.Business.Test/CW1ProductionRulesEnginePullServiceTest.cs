using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.ProductionRules.Core;
using WTG.ProductionRules.TestFramework;

namespace Enterprise.ProductionRules.Business.Testing
{
	[UseSnapshotProtection]
	class CW1ProductionRulesEnginePullServiceTest : TestCase
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<CW1ProductionRulesEnginePullService>(ObjectFactory.Get<IProductionRulesEnginePullService>());
		}

		public void TestRunRulesEngine_EndToEnd()
		{
			var factory = new BusinessObjectFactory();
			DeactivateExistingActiveRuleSetAndConstraint(factory);

			var helper = new Helper(factory);
			var ruleSet = helper.CreateRuleSet("Dummy Ruleset", "Dummy Ruleset", true, context: "TST");
			var rule = helper.CreateRule(ruleSet, "Increase Pay", "Increase Hourly Rate Of Active Employees.", 1);
			rule.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""IsActive"",
      ""operation"": ""equals"",
      ""value"": true
    },
  ],
  ""action"": {
    ""$type"": ""IncreasePayActionState"",
    ""amountToIncrease"": 2.50 
  }
}".TrimStart();

			factory.Save();

			var activeEmployee = new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true };
			var inactiveEmployee = new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = false };

			var results = new CW1ProductionRulesEnginePullService().RunRulesEngine(RulesContextType.SecondDummyForTesting, RulesContextSubType.None, ProductionRuleSetFilter.Empty, new[] { rule.GetProductionRuleWrapper() }, new[] { activeEmployee, inactiveEmployee }, new CancellationToken(false));
			AssertEquals("Should have completed succesfully.", "", results.Notifications);
			AssertEquals("Should have completed succesfully.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", new[] { activeEmployee, inactiveEmployee }, results.Facts);
			AssertEquals("Should have increased HourlyRate of active employee.", 7.50m, activeEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of active employee.", 5.0m, inactiveEmployee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
		}

		public void TestRunRulesEngine_EndToEnd_PushMethodsThrow()
		{
			var activeEmployee = new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = true };
			var inactiveEmployee = new EmployeeFactDummy { HourlyRate = 5.00m, IsActive = false };

			AssertExceptionThrown<InvalidOperationException>(() => new CW1ProductionRulesEnginePullService().RunRulesEngine(RulesContextType.DummyForTesting, new[] { activeEmployee, inactiveEmployee }, new CancellationToken(false)));
		}

		void WaitForStrayDependencyInjectionTask()
		{
			// See WI00348604 - Fix Amnesty test failure
			const int Timeout = 5000;
			var tasks = (Task[])typeof(Task).GetMethod("GetActiveTasks", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, Array.Empty<object>());

			AssertEquals("All tasks should have completed.", true, Task.WaitAll(tasks, Timeout));
		}

		static void DeactivateExistingActiveRuleSetAndConstraint(BusinessObjectFactory factory)
		{
			var existingRuleSetQuery = new ZQuery(ProductionRuleSetSchema.PRS_IsLive, true);
			existingRuleSetQuery.AddToFilter(ProductionRuleSetSchema.PRS_Context, "TST");

			var existingRuleSet = factory.LoadTop1<ProductionRuleSet>(existingRuleSetQuery);
			if (existingRuleSet != null)
			{
				existingRuleSet.PRS_IsLive = false;
			}

			((IDbConnected)factory).Connection.ExecuteScalar("IF (OBJECT_ID('dbo.Constraint_PRS_Context', 'C') IS NOT NULL) BEGIN ALTER TABLE dbo.ProductionRuleSet DROP Constraint_PRS_Context END");
		}
	}
}
