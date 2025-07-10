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
using WTG.ProductionRules.Core.Facts;
using WTG.ProductionRules.TestFramework;

namespace Enterprise.ProductionRules.Business.Testing
{
	[UseSnapshotProtection]
	class CW1ProductionRulesEngineServiceTest : TestCase
	{
		public void TestObjectFactoryConfiguration()
		{
			AssertType<CW1ProductionRulesEngineService>(ObjectFactory.Get<IProductionRulesEnginePushService>(nameof(IProductionRulesEnginePushService), new[] { new BusinessObjectFactory() }));
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

			var results = new CW1ProductionRulesEngineService(factory).RunRulesEngine(RulesContextType.DummyForTesting, new[] { activeEmployee, inactiveEmployee }, new CancellationToken(false));
			AssertEquals("Should have completed succesfully.", "", results.Notifications);
			AssertEquals("Should have completed succesfully.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", new[] { activeEmployee, inactiveEmployee }, results.Facts);
			AssertEquals("Should have increased HourlyRate of active employee.", 7.50m, activeEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of active employee.", 5.0m, inactiveEmployee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
		}

		public void TestRunRulesEngine_ForwardChaining_EndToEnd()
		{
			var factory = new BusinessObjectFactory();
			DeactivateExistingActiveRuleSetAndConstraint(factory);

			var helper = new Helper(factory);
			var ruleSet = helper.CreateRuleSet("Dummy Ruleset", "Dummy Ruleset", true, context: "TST");

			var userProperty = helper.CreateUserDefinedProperty("BOO", "GotPromotion", factUniqueKey: "TST");
			var rule1 = helper.CreateRule(ruleSet, "Promote Employees", "Promote all active employees.", 1);
			rule1.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""IsActive"",
      ""operation"": ""equals"",
      ""value"": true
    },
  ],
  ""action"": {
    ""$type"": ""SetPropertyActionState"",
    ""propertyPath"": ""GotPromotion"",
    ""value"": true
  }
}".TrimStart();

			var rule2 = helper.CreateRule(ruleSet, "Increase Pay", "Increase Hourly Rate Of Promoted Employees.", 10);
			rule2.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""GotPromotion"",
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

			var results = new CW1ProductionRulesEngineService(factory).RunRulesEngine(RulesContextType.DummyForTesting, new[] { activeEmployee, inactiveEmployee }, new CancellationToken(false));
			AssertEquals("Should have completed succesfully.", "", results.Notifications);
			AssertEquals("Should have completed succesfully.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", new[] { activeEmployee, inactiveEmployee }, results.Facts);
			AssertEquals("Should have increased HourlyRate of active employee.", 7.50m, activeEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of inactive employee.", 5.0m, inactiveEmployee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
		}

		public void TestRunRulesEngine_ForwardChaining_DoesNotInfinitelyRecurse_EndToEnd()
		{
			var factory = new BusinessObjectFactory();
			DeactivateExistingActiveRuleSetAndConstraint(factory);

			var helper = new Helper(factory);
			var ruleSet = helper.CreateRuleSet("Dummy Ruleset", "Dummy Ruleset", true, context: "TST");

			var userProperty = helper.CreateUserDefinedProperty("BOO", "SomeBool", factUniqueKey: "TST");
			var rule1 = helper.CreateRule(ruleSet, "Test 1", "Set SomeBool To True If False.", 5);
			rule1.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""SomeBool"",
      ""operation"": ""equals"",
      ""value"": false
    },
  ],
  ""action"": {
    ""$type"": ""SetPropertyActionState"",
    ""propertyPath"": ""SomeBool"",
    ""value"": true
  }
}".TrimStart();

			var rule2 = helper.CreateRule(ruleSet, "Test 2", "Set SomeBool To False If True.", 1);
			rule2.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""SomeBool"",
      ""operation"": ""equals"",
      ""value"": true
    },
  ],
  ""action"": {
    ""$type"": ""SetPropertyActionState"",
    ""propertyPath"": ""SomeBool"",
    ""value"": false
  }
}".TrimStart();

			var rule3 = helper.CreateRule(ruleSet, "Increase Pay", "Increase Hourly Rate Of Promoted Employees.", 10);
			rule3.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""SomeBool"",
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

			var results = new CW1ProductionRulesEngineService(factory).RunRulesEngine(RulesContextType.DummyForTesting, new[] { activeEmployee }, new CancellationToken(false));
			AssertEquals("Precondition: Should have run rule1.", true, activeEmployee.HasRuleAlreadySetUserDefinedProperty(userProperty.XC_Name, rule1.PRL_Name));
			AssertEquals("Precondition: Should have run rule2.", true, activeEmployee.HasRuleAlreadySetUserDefinedProperty(userProperty.XC_Name, rule2.PRL_Name));
			AssertEquals("Precondition: Rule 3 didnt get fired/set a property.", false, activeEmployee.HasRuleAlreadySetUserDefinedProperty(userProperty.XC_Name, rule3.PRL_Name));
			AssertEquals("Should have completed succesfully, and not infinitely recurse.", "", results.Notifications);
			AssertEquals("Should have completed succesfully, and not infinitely recurse.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", new[] { activeEmployee }, results.Facts);
			AssertEquals("Should *not* have increased HourlyRate as the non-persistent boolean should have been false.", 5.0m, activeEmployee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
		}

		public void TestRunRulesEngine_EndToEnd_UnitConversions()
		{
			var factory = new BusinessObjectFactory();
			DeactivateExistingActiveRuleSetAndConstraint(factory);

			var helper = new Helper(factory);
			var ruleSet = helper.CreateRuleSet("Dummy Ruleset", "Dummy Ruleset", true, context: "TST");
			var rule = helper.CreateRule(ruleSet, "Bad Discriminatory ~Bro~ Rule", "Increase HourlyRate Of Tall Person.", 1);
			rule.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""Height"",
      ""operation"": ""greaterThan"",
      ""value"": {
        ""value"": 6.0,
        ""unit"": ""FT""
      }
    },
  ],
  ""action"": {
    ""$type"": ""IncreasePayActionState"",
    ""amountToIncrease"": 2.50 
  }
}".TrimStart();

			factory.Save();

			var notTallEnoughUsEmployee = new EmployeeFactDummy { Height = new MeasureFact(5.9m, Core.Constants.Length.Feet), HourlyRate = 5.0m };
			var tallUsEmployee = new EmployeeFactDummy { Height = new MeasureFact(6.1m, Core.Constants.Length.Feet), HourlyRate = 5.0m };

			var notTallEnoughAusEmployee = new EmployeeFactDummy { Height = new MeasureFact(1.7m, Core.Constants.Length.Metres), HourlyRate = 5.0m };
			var tallAusEmployee = new EmployeeFactDummy { Height = new MeasureFact(1.85m, Core.Constants.Length.Metres), HourlyRate = 5.0m };

			var employeeFacts = new[] { notTallEnoughUsEmployee, tallUsEmployee, notTallEnoughAusEmployee, tallAusEmployee };

			var results = new CW1ProductionRulesEngineService(factory).RunRulesEngine(RulesContextType.DummyForTesting, employeeFacts, new CancellationToken(false));
			AssertEquals("Should have completed succesfully.", "", results.Notifications);
			AssertEquals("Should have completed succesfully.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", employeeFacts, results.Facts);
			AssertEquals("Should have increased HourlyRate of tall employee.", 7.50m, tallUsEmployee.HourlyRate);
			AssertEquals("Should have increased HourlyRate of tall employee.", 7.50m, tallAusEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of short employee.", 5.0m, notTallEnoughUsEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of short employee.", 5.0m, notTallEnoughAusEmployee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
		}

		[TestDate(2021, 01, 15)]
		public void TestRunRulesEngine_EndToEnd_DateTime()
		{
			var factory = new BusinessObjectFactory();
			DeactivateExistingActiveRuleSetAndConstraint(factory);

			var helper = new Helper(factory);
			var ruleSet = helper.CreateRuleSet("Dummy Ruleset", "Dummy Ruleset", true, context: "TST");
			var rule = helper.CreateRule(ruleSet, "Decrease Pay", "Decrease Hourly Rate Of Employees That Are Leaving.", 1);
			rule.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""DepartureDate"",
      ""operation"": ""IsInTheNext"",
      ""value"": {
        ""value"": 3,
        ""unit"": ""M""
      }
	},
  ],
  ""action"": {
    ""$type"": ""IncreasePayActionState"",
    ""amountToIncrease"": -10.00 
  }
}".TrimStart();

			factory.Save();

			var leavingEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = new DateTime(2021, 02, 28) };
			var leftEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = new DateTime(2020, 10, 10) };
			var hereForeverEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = new DateTime(2067, 12, 25) };
			var employee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = null };

			var employees = new[] { leavingEmployee, leftEmployee, hereForeverEmployee, employee };

			var results = new CW1ProductionRulesEngineService(factory).RunRulesEngine(RulesContextType.DummyForTesting, employees, new CancellationToken(false));
			AssertEquals("Should have completed succesfully.", "", results.Notifications);
			AssertEquals("Should have completed succesfully.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", employees, results.Facts);
			AssertEquals("Should have decreased HourlyRate of leaving employee.", 10.00m, leavingEmployee.HourlyRate);
			AssertEquals("Should *not* have decreased HourlyRate of other employees.", 20.00m, leftEmployee.HourlyRate);
			AssertEquals("Should *not* have decreased HourlyRate of other employees.", 20.00m, hereForeverEmployee.HourlyRate);
			AssertEquals("Should *not* have decreased HourlyRate of other employees.", 20.00m, employee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
		}

		[TestDate(2023, 07, 12)]
		public void TestRunRulesEngine_EndToEnd_DateTime_IsAfterTheNext()
		{
			var factory = new BusinessObjectFactory();
			DeactivateExistingActiveRuleSetAndConstraint(factory);

			var helper = new Helper(factory);
			var ruleSet = helper.CreateRuleSet("Dummy Ruleset", "Dummy Ruleset", true, context: "TST");
			var rule = helper.CreateRule(ruleSet, "Increase Pay", "Increasing Hourly Rate Of Employees That Are Leaving After X D/M/Y.", 1);
			rule.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""DepartureDate"",
      ""operation"": ""IsAfterTheNext"",
      ""value"": {
        ""value"": 3,
        ""unit"": ""M""
      }
	},
  ],
  ""action"": {
    ""$type"": ""IncreasePayActionState"",
    ""amountToIncrease"": 10.00 
  }
}".TrimStart();

			factory.Save();

			var leavingAfterEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = new DateTime(2023, 10, 13) };
			var leavingBeforeEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = new DateTime(2023, 10, 12) };
			var leftEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = new DateTime(2022, 10, 10) };
			var hereForeverEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = new DateTime(2067, 12, 25) };
			var employee = new EmployeeFactDummy { HourlyRate = 20.00m, DepartureDate = null };

			var employees = new[] { leavingAfterEmployee, leavingBeforeEmployee, leftEmployee, hereForeverEmployee, employee };

			var results = new CW1ProductionRulesEngineService(factory).RunRulesEngine(RulesContextType.DummyForTesting, employees, new CancellationToken(false));
			AssertEquals("Should have completed succesfully.", "", results.Notifications);
			AssertEquals("Should have completed succesfully.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", employees, results.Facts);
			AssertEquals("Should have increased HourlyRate of leaving after employee.", 30.00m, leavingAfterEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of leaving before employee.", 20.00m, leavingBeforeEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of left employees.", 20.00m, leftEmployee.HourlyRate);
			AssertEquals("Should have increased HourlyRate of forever employees.", 30.00m, hereForeverEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of other employees.", 20.00m, employee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
		}

		[TestDate(2023, 07, 12)]
		public void TestRunRulesEngine_EndToEnd_DateTime_WasBeforeTheLast()
		{
			var factory = new BusinessObjectFactory();
			DeactivateExistingActiveRuleSetAndConstraint(factory);

			var helper = new Helper(factory);
			var ruleSet = helper.CreateRuleSet("Dummy Ruleset", "Dummy Ruleset", true, context: "TST");
			var rule = helper.CreateRule(ruleSet, "Increase Pay", "Increasing Hourly Rate Of Employees That Were Started Before X D/M/Y.", 1);
			rule.PRL_RuleDefinition = @"
{
  ""conditions"": [
    {
      ""fieldPath"": ""StartDate"",
      ""operation"": ""WasBeforeTheLast"",
      ""value"": {
        ""value"": 3,
        ""unit"": ""M""
      }
	},
  ],
  ""action"": {
    ""$type"": ""IncreasePayActionState"",
    ""amountToIncrease"": 10.00 
  }
}".TrimStart();

			factory.Save();

			var startedBeforeEmployee = new EmployeeFactDummy { HourlyRate = 25.00m, StartDate = new DateTime(2023, 04, 04) };
			var startedALongBeforeEmployee = new EmployeeFactDummy { HourlyRate = 50.00m, StartDate = new DateTime(1970, 10, 13) };
			var startedAfterEmployee = new EmployeeFactDummy { HourlyRate = 20.00m, StartDate = new DateTime(2023, 06, 12) };
			var notYetStartedEmployee = new EmployeeFactDummy { HourlyRate = 15.00m, StartDate = new DateTime(2023, 10, 10) };

			var employees = new[] { startedBeforeEmployee, startedALongBeforeEmployee, startedAfterEmployee, notYetStartedEmployee };

			var results = new CW1ProductionRulesEngineService(factory).RunRulesEngine(RulesContextType.DummyForTesting, employees, new CancellationToken(false));
			AssertEquals("Should have completed succesfully.", "", results.Notifications);
			AssertEquals("Should have completed succesfully.", ResultStatus.Success, results.Status);
			AssertContainsExactElementsInAnyOrder("Should have returned all facts.", employees, results.Facts);
			AssertEquals("Should have increased HourlyRate of started before employee.", 35.00m, startedBeforeEmployee.HourlyRate);
			AssertEquals("Should have increased HourlyRate of started a long time before employee.", 60.00m, startedALongBeforeEmployee.HourlyRate);
			AssertEquals("Should *not*  have increased HourlyRate of started after employee.", 20.00m, startedAfterEmployee.HourlyRate);
			AssertEquals("Should *not* have increased HourlyRate of not started employees.", 15.00m, notYetStartedEmployee.HourlyRate);

			WaitForStrayDependencyInjectionTask();
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
