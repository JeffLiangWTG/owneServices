using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Gateway;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AutoratingConsolUnderJobInvoicingTest : BaseGatewayBillingIntegrationTest
	{
		public void TestAutoratingCost_C1_1_ScenarioS1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase
				{
					ScenarioDescription = "Scenario S1 - Autorating Cost under Job Invoicing menu at Consol level (C1.1) when Login company is THBKK",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["THBKK"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedInvoicingCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>(),
						ExpectedCosts = new List<AssertionCost>()
					}
				},
				new GatewayBillingTestCase
				{
					ScenarioDescription = "Scenario S1 - Autorating Cost under Job Invoicing menu at Consol level (C1.1) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedInvoicingCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>(),
						ExpectedCosts = new List<AssertionCost>()
					}
				}
			};

			AutorateAndAssert(scenario.Consols["C1.1"], testCases);
		}

		public void TestAutoratingCost_C1_2_ScenarioS1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription = "Scenario S1 - Autorating Cost under Job Invoicing menu at Consol level (C1.2) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedInvoicingCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>(),
						ExpectedCosts = new List<AssertionCost>()
					},
				},
				new GatewayBillingTestCase
				{
					ScenarioDescription = "Scenario S1 - Autorating Cost under Job Invoicing menu at Consol level (C1.2) when login company is NLRTM",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLRTM"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedInvoicingCharges = new Dictionary<IJobInvoicingPlugIn, IEnumerable<AssertionCharge>>(),
						ExpectedCosts = new List<AssertionCost>()
					}
				}
			};

			AutorateAndAssert(scenario.Consols["C1.2"], testCases);
		}

		#region Autorate and Assert

		protected override void AutorateAndAssert<T>(T jobParent, IEnumerable<GatewayBillingTestCase> testCases)
		{
			foreach (var testCase in testCases)
			{
				using (SetTemporaryUserContext(testCase.UserContext))
				using (SetTemporaryGatewayConfiguration(testCase))
				{
					if (testCase.preconditionAssertions != null)
					{
						CombineAssertions("Pre-Conditions", testCase.preconditionAssertions);
					}

					AutoCostAndAssert(
						testCase.ScenarioDescription,
						testCase.AssertionParameters.ExpectedInvoicingCharges,
						testCase.AssertionParameters.ExpectedCosts,
						jobParent as IGenericJobCostPlugIn,
						testCase.AssertionParameters.AutorateRevenue,
						testCase.AssertionParameters.AutorateCosts,
						testCase.AssertionParameters.DeleteExistingCosts,
						testCase.AssertionParameters.ExpectedWarnings,
						testCase.AssertionParameters.ExpectedErrors);
				}
			}

			Assert("This test uses FluentAssertions", true);
		}

		#endregion
	}
}
