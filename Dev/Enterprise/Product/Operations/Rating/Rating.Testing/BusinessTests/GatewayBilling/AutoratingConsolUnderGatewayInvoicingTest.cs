using System.Collections.Generic;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business.Gateway;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AutoratingConsolUnderGatewayInvoicingTest : BaseGatewayBillingIntegrationTest
	{
		#region WI00317849 - DHL G/W: Enhance Autorating Logic of ICT for Gateways based on new registry

		// WI00317849 - DHL G/W: Enhance Autorating Logic of ICT for Gateways based on new registry
		// Link to the Sample Fille: WI00317849 Gateway Scenarios with ICT Rates Setup.xlsx
		// edient:Command=ShowStorageDoc&BusinessEntityPK=64b02606-c702-4cd2-8b8b-6cd925fece39&StorageDocPK=9b645500-a2cb-45f7-872d-514811949ab6&Hash=%2bGTdIYsTDGqqlhksduzF0XAGiozICoNwX&Domain=wtg.zone&Instance=ediProd

		#region Scenario S1

		public void TestScenarioS1_AutoratingCostAndRevenue_C1_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCases = new[]
			{
				// Scenario S1 - Autorating Cost under Gateway Invoicing menu at Consol level (C1.1) when Login company is THBKK
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S1 - Autorating Cost under Gateway Invoicing menu at Consol level (C1.1) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true, AutorateRevenue = false, ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S1 - Autorating Revenue under Gateway Invoicing menu at Consol level (C1.1) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWPRV",
								RelatedJobNumber = scenario.Shipments["S1.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWPRV",
								RelatedJobNumber = scenario.Shipments["S1.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWPRV",
								RelatedJobNumber = scenario.Shipments["S1.3"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWPRV",
								RelatedJobNumber = scenario.Shipments["S1.4"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								RelatedJobNumber = scenario.Shipments["S1.3"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								RelatedJobNumber = scenario.Shipments["S1.4"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C1.1"], testCases);
		}

		public void TestScenarioS1_AutoratingCostAndRevenue_C1_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase
				{
					ScenarioDescription =
						"Scenario S1 - Autorating Cost under Gateway Invoicing menu at Consol level (C1.2) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true, AutorateRevenue = false, ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S1 - Autorating Cost under Gateway Invoicing menu at Consol level (C1.2) when login company is NLRTM
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S1 - Autorating Revenue under Gateway Invoicing menu at Consol level (C1.2) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								RelatedJobNumber = scenario.Shipments["S1.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								RelatedJobNumber = scenario.Shipments["S1.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								RelatedJobNumber = scenario.Shipments["S1.5"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C1.2"], testCases);
		}

		#endregion

		#region Scenario S2

		public void TestScenarioS2_AutoratingCostAndRevenue_C2_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S2 - Autorating Cost under Gateway Invoicing menu at Consol level (C2.1) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S2.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S2.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S2 - Autorating Cost under Gateway Invoicing menu at Consol level (C2.1) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					}
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S2 - Autorating Revenue under Gateway Invoicing menu at Consol level (C2.1) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S2.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S2.3"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S2.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S2.4"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S2 - Autorating Revenue under Gateway Invoicing menu at Consol level (C2.1) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 7m,
								RelatedJobNumber = scenario.Shipments["S2.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 7m,
								RelatedJobNumber = scenario.Shipments["S2.4"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C2.1"], testCases);
		}

		public void TestScenarioS2_AutoratingCostAndRevenue_C2_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S2 - Autorating Cost under Gateway Invoicing menu at Consol level (C2.2) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true, AutorateRevenue = false, ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S2 - Autorating Cost under Gateway Invoicing menu at Consol level (C2.2) when login company is BRSSZ
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S2 - Autorating Revenue under Gateway Invoicing menu at Consol level (C2.2) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S2.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S2.3"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S2.5"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C2.2"], testCases);
		}

		#endregion

		#region Scenario S3

		public void TestScenarioS3_AutoratingCostAndRevenue_C3_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3NormalRates();

			var testCases = new[]
			{
				// Scenario S3 - Autorating Cost under Gateway Invoicing menu at Consol level (C3.1) when login company is THBKK
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S3 - Autorating Cost under Gateway Invoicing menu at Consol level (C3.1) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true, AutorateRevenue = false, ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S3 - Autorating Revenue  under Gateway Invoicing menu at Consol level (C3.1) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWPRV",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S3.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWPRV",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S3.3"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S3.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C3.1"], testCases);
		}

		public void TestScenarioS3_AutoratingCostAndRevenue_C3_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S3 - Autorating Cost under Gateway Invoicing menu at Consol level (C3.2) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true, AutorateRevenue = false, ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S3 - Autorating Cost under Gateway Invoicing menu at Consol level (C3.2) when login company is HUBUD
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S3 - Autorating Revenue under Gateway Invoicing menu at Consol level (C3.2) when login company is SGSIN",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SGSIN"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S3.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S3.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C3.2"], testCases);
		}

		#endregion

		#region Scenario S5

		public void TestScenarioS5_AutoratingCostAndRevenue_C5_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5NormalRates();

			var testCases = new[]
			{
				// Scenario S5 - Autorating Cost under Gateway Invoicing menu at Consol level (C5.1) when login company is DKCPH
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Cost under Gateway Invoicing menu at Consol level (C5.1) when login company is DEBRE",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEBRE"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S5 - Autorating Revenue under Gateway Invoicing menu at Consol level (C5.1) when login company is DKCPH
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Revenue under Gateway Invoicing menu at Consol level (C5.1) when login company is DEBRE",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEBRE"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C5.1"], testCases);
		}

		public void TestScenarioS5_AutoratingCostAndRevenue_C5_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Cost under Gateway Invoicing menu at Consol level (C5.2) when login company is DEBRE",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEBRE"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 11m,
								RelatedJobNumber = scenario.Shipments["S5.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 12m,
								RelatedJobNumber = scenario.Shipments["S5.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Cost under Gateway Invoicing menu at Consol level (C5.2) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Revenue under Gateway Invoicing menu at Consol level (C5.2) when login company is DEBRE",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEBRE"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 1m,
								RelatedJobNumber = scenario.Shipments["S5.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 2m,
								RelatedJobNumber = scenario.Shipments["S5.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Revenue under Gateway Invoicing menu at Consol level (C5.2) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C5.2"], testCases);
		}

		public void TestScenarioS5_AutoratingCostAndRevenue_C5_3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Cost under Gateway Invoicing menu at Consol level (C5.3) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S5 - Autorating Cost under Gateway Invoicing menu at Consol level (C5.3) when login company is CRSJO
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S5 - Autorating Revenue under Gateway Invoicing menu at Consol level (C5.3) when login company is BEANR",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEANR"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 11m,
								RelatedJobNumber = scenario.Shipments["S5.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 11m,
								RelatedJobNumber = scenario.Shipments["S5.3"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 12m,
								RelatedJobNumber = scenario.Shipments["S5.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C5.3"], testCases);
		}

		#endregion

		#region Scenario S6

		public void TestScenarioS6_AutoratingCostAndRevenue_C6_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS6NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S6 - Autorating Cost under Gateway Invoicing menu at Consol level (C6.1) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 11m,
								RelatedJobNumber = scenario.Shipments["S6.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge // This is an edge case that we need to think if we need to handle it.
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 11m,
								RelatedJobNumber = scenario.Shipments["S6.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S6 - Autorating Cost under Gateway Invoicing menu at Consol level (C6.1) when login company is USLAX",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USLAX"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S6 - Autorating Revenue under Gateway Invoicing menu at Consol level (C6.1) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 1m,
								RelatedJobNumber = scenario.Shipments["S6.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 1m,
								RelatedJobNumber = scenario.Shipments["S6.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S6 - Autorating Revenue under Gateway Invoicing menu at Consol level (C6.1) when login company is USLAX",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USLAX"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge // This is an edge case that we need to think if we need to handle it.
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 12m,
								RelatedJobNumber = scenario.Shipments["S6.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 12m,
								RelatedJobNumber = scenario.Shipments["S6.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C6.1"], testCases);
		}

		public void TestScenarioS6_AutoratingCostAndRevenue_C6_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS6NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S6 - Autorating Cost under Gateway Invoicing menu at Consol level (C6.2) when login company is USLAX",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USLAX"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S6 - Autorating Revenue under Gateway Invoicing menu at Consol level (C6.2) when login company is USLAX",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USLAX"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 11m,
								RelatedJobNumber = scenario.Shipments["S6.1"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C6.2"], testCases);
		}

		#endregion

		#region Scenario S7

		public void TestScenarioS7_AutoratingCostAndRevenue_C7_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS7NormalRates();

			var testCases = new[]
			{
				// Scenario S7 - Autorating Cost under Gateway Invoicing menu at Consol level (C7.1) when login company is SESTO
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Cost under Gateway Invoicing menu at Consol level (C7.1) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S7 - Autorating Revenue under Gateway Invoicing menu at Consol level (C7.1) when login company is SESTO
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Revenue under Gateway Invoicing menu at Consol level (C7.1) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C7.1"], testCases);
		}

		public void TestScenarioS7_AutoratingCostAndRevenue_C7_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS7NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Cost under Gateway Invoicing menu at Consol level (C7.2) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S7.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge // This is an edge case that we need to think if we need to handle it.
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S7.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Cost under Gateway Invoicing menu at Consol level (C7.2) when login company is ZAJNB",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["ZAJNB"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Revenue under Gateway Invoicing menu at Consol level (C7.2) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S7.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S7.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Revenue under Gateway Invoicing menu at Consol level (C7.2) when login company is ZAJNB",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["ZAJNB"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge // This is an edge case that we need to think if we need to handle it.
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S7.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S7.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C7.2"], testCases);
		}

		public void TestScenarioS7_AutoratingCostAndRevenue_C7_3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS7NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Cost under Gateway Invoicing menu at Consol level (C7.3) when login company is ZAJNB",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["ZAJNB"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S7 - Autorating Cost under Gateway Invoicing menu at Consol level (C7.3) when login company is ZADUR
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S7 - Autorating Revenue under Gateway Invoicing menu at Consol level (C7.3) when login company is ZAJNB",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["ZAJNB"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S7.1"].JS_UniqueConsignRef,
							},
						},
					},
				},
				// Scenario S7 - Autorating Revenue under Gateway Invoicing menu at Consol level (C7.3) when login company is ZADUR
				// It's not applicable, because Consol is not a gateway
			};

			AutorateAndAssert(scenario.Consols["C7.3"], testCases);
		}

		#endregion

		#region Scenario S8

		public void TestScenarioS8_AutoratingCostAndRevenue_C8_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS8NormalRates();

			var testCases = new[]
			{
				// Scenario S8 - Autorating Cost under Gateway Invoicing menu at Consol level (C8.1) when login company is CHZRH
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S8 - Autorating Cost under Gateway Invoicing menu at Consol level (C8.1) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S8 - Autorating Revenue under Gateway Invoicing menu at Consol level (C8.1) when login company is CHZRH
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S8 - Autorating Revenue under Gateway Invoicing menu at Consol level (C8.1) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S8.1"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C8.1"], testCases);
		}

		public void TestScenarioS8_AutoratingCostAndRevenue_C8_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS8NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S8 - Autorating Cost under Gateway Invoicing menu at Consol level (C8.2) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S8 - Autorating Cost under Gateway Invoicing menu at Consol level (C8.2) when login company is USJFK
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S8 - Autorating Revenue under Gateway Invoicing menu at Consol level (C8.2) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S8.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S8.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C8.2"], testCases);
		}

		#endregion

		#region Scenario S9

		public void TestScenarioS9_AutoratingCostAndRevenue_C9_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9NormalRates();

			var testCases = new[]
			{
				// Scenario S9 - Autorating Cost under Gateway Invoicing menu at Consol level (C9.1) when login company is DESTR
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S9 - Autorating Cost under Gateway Invoicing menu at Consol level (C9.1) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S9 - Autorating Revenue under Gateway Invoicing menu at Consol level (C9.1) when login company is DESTR
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S9 - Autorating Revenue under Gateway Invoicing menu at Consol level (C9.1) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C9.1"], testCases);
		}

		public void TestScenarioS9_AutoratingCostAndRevenue_C9_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S9 - Autorating Cost under Gateway Invoicing menu at Consol level (C9.2) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S9 - Autorating Cost under Gateway Invoicing menu at Consol level (C9.2) when login company is HKHKG
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S9 - Autorating Revenue under Gateway Invoicing menu at Consol level (C9.2) when login company is DEFRA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["DEFRA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S9.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S9.3"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S9.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C9.2"], testCases);
		}

		#endregion

		#region Scenario S11

		public void TestScenarioS11_AutoratingCostAndRevenue_C11_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS11NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Cost under Gateway Invoicing menu at Consol level (C11.1) when login company is SESTO",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SESTO"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S11.1"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Cost under Gateway Invoicing menu at Consol level (C11.1) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Revenue under Gateway Invoicing menu at Consol level (C11.1) when login company is SESTO",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["SESTO"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S11.1"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Revenue under Gateway Invoicing menu at Consol level (C11.1) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C11.1"], testCases);
		}

		public void TestScenarioS11_AutoratingCostAndRevenue_C11_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS11NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Cost under Gateway Invoicing menu at Consol level (C11.2) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Cost under Gateway Invoicing menu at Consol level (C11.2) when login company is CLSCL",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["CLSCL"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Revenue under Gateway Invoicing menu at Consol level (C11.2) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S11.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S11.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S11 - Autorating Revenue under Gateway Invoicing menu at Consol level (C11.2) when login company is CLSCL",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["CLSCL"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new AssertionCharge[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S11.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S11.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C11.2"], testCases);
		}

		#endregion

		#region Scenario S12

		public void TestScenarioS12_AutoratingCostAndRevenue_C12_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS12NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Cost under Gateway Invoicing menu at Consol level (C12.1) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S12.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S12.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Cost under Gateway Invoicing menu at Consol level (C12.1) when login company is BEBRU",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEBRU"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Revenue under Gateway Invoicing menu at Consol level (C12.1) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S12.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S12.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Revenue under Gateway Invoicing menu at Consol level (C12.1) when login company is BEBRU",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEBRU"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C12.1"], testCases);
		}

		public void TestScenarioS12_AutoratingCostAndRevenue_C12_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS12NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Cost under Gateway Invoicing menu at Consol level (C12.2) when login company is BEBRU",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEBRU"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Cost under Gateway Invoicing menu at Consol level (C12.2) when login company is RUMOW",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["RUMOW"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Revenue under Gateway Invoicing menu at Consol level (C12.2) when login company is BEBRU",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["BEBRU"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S12.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S12.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S12.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S12 - Autorating Revenue under Gateway Invoicing menu at Consol level (C12.2) when login company is RUMOW",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["RUMOW"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 7m,
								RelatedJobNumber = scenario.Shipments["S12.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 7m,
								RelatedJobNumber = scenario.Shipments["S12.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C12.2"], testCases);
		}

		#endregion

		#region Scenario S16

		public void TestScenarioS16_AutoratingCostAndRevenue_C16_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS16NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S16 - Autorating Cost under Gateway Invoicing menu at Consol level (C16.1) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S16 - Autorating Cost under Gateway Invoicing menu at Consol level (C16.1) when login company is SGSIN
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S16 - Autorating Revenue under Gateway Invoicing menu at Consol level (C16.1) when login company is NLAMS",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["NLAMS"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S16.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S16.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				// Scenario S16 - Autorating Revenue under Gateway Invoicing menu at Consol level (C16.1) when login company is SGSIN
				// It's not applicable, because Consol is not a gateway
			};

			AutorateAndAssert(scenario.Consols["C16.1"], testCases);
		}

		public void TestScenarioS16_AutoratingCostAndRevenue_C16_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS16NormalRates();

			var testCases = System.Array.Empty<GatewayBillingTestCase>();

			// Scenario S16 - Autorating Cost under Gateway Invoicing menu at Consol level (C16.2) when login company is SGSIN
			// It's not applicable, because Consol is not a gateway

			// Scenario S16 - Autorating Cost under Gateway Invoicing menu at Consol level (C16.2) when login company is IDBTH
			// It's not applicable, because Consol is not a gateway

			AutorateAndAssert(scenario.Consols["C16.2"], testCases);
		}

		#endregion

		#region Scenario S18

		public void TestScenarioS18_AutoratingCostAndRevenue_C18_1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCases = new[]
			{
				// Scenario S18 - Autorating Cost under Gateway Invoicing menu at Consol level (C18.1) when login company is CNSZX
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Cost under Gateway Invoicing menu at Consol level (C18.1) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S18 - Autorating Revenue under Gateway Invoicing menu at Consol level (C18.1) when login company is CNSZX
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Revenue under Gateway Invoicing menu at Consol level (C18.1) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C18.1"], testCases);
		}

		public void TestScenarioS18_AutoratingCostAndRevenue_C18_2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Cost under Gateway Invoicing menu at Consol level (C18.2) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Cost under Gateway Invoicing menu at Consol level (C18.2) when login company is USMIA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USMIA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Revenue under Gateway Invoicing menu at Consol level (C18.2) when login company is HKHKG",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["HKHKG"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 3m,
								RelatedJobNumber = scenario.Shipments["S18.1"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 4m,
								RelatedJobNumber = scenario.Shipments["S18.2"].JS_UniqueConsignRef,
							},
						},
					},
				},
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Revenue under Gateway Invoicing menu at Consol level (C18.2) when login company is USMIA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USMIA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S18.2"].JS_UniqueConsignRef,
							},
							new AssertionCharge
							{
								ChargeCode = "GWIMP",
								JR_OSCostAmt = 6m,
								RelatedJobNumber = scenario.Shipments["S18.3"].JS_UniqueConsignRef,
							},
						},
					},
				},
			};

			AutorateAndAssert(scenario.Consols["C18.2"], testCases);
		}

		public void TestScenarioS18_AutoratingCostAndRevenue_C18_3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCases = new[]
			{
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Cost under Gateway Invoicing menu at Consol level (C18.3) when login company is USMIA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USMIA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = true,
						AutorateRevenue = false,
						ExpectedCharges = System.Array.Empty<AssertionCharge>(),
					},
				},
				// Scenario S18 - Autorating Cost under Gateway Invoicing menu at Consol level (C18.3) when login company is ARBUE
				// It's not applicable, because Consol is not a gateway
				new GatewayBillingTestCase()
				{
					ScenarioDescription =
						"Scenario S18 - Autorating Revenue under Gateway Invoicing menu at Consol level (C18.3) when login company is USMIA",
					UserContext = new GatewayBillingTestCaseUserContext
					{
						Branch = scenario.Agents["USMIA"].branch
					},
					AssertionParameters = new AutoratingParameters
					{
						AutorateCosts = false,
						AutorateRevenue = true,
						ExpectedCharges = new[]
						{
							new AssertionCharge
							{
								ChargeCode = "GWFWD",
								JR_OSCostAmt = 5m,
								RelatedJobNumber = scenario.Shipments["S18.1"].JS_UniqueConsignRef,
							},
						},
					},
				},
				// Scenario S18 - Autorating Revenue under Gateway Invoicing menu at Consol level (C18.3) when login company is ARBUE
				// It's not applicable, because Consol is not a gateway
			};

			AutorateAndAssert(scenario.Consols["C18.3"], testCases);
		}

		#endregion

		#endregion

		#region Autorate and Assert

		protected override void AutorateAndAssert<T>(T jobParent, IEnumerable<GatewayBillingTestCase> testCases)
		{
			foreach (var testCase in testCases)
			{
				using (SetTemporaryUserContext(testCase.UserContext))
				using (SetTemporaryGatewayConfiguration(testCase))
				{
					if (testCase.AssertionParameters.Job == null)
					{
						Assert($@"Pre-Condition: Parent Job has to be a Gateway! {testCase.ScenarioDescription}", jobParent.IsGatewayBillingEnabled());
					}

					AutorateAndAssert(
						testCase.ScenarioDescription,
						testCase.AssertionParameters.ExpectedCharges,
						jobParent,
						testCase.AssertionParameters.LocalClient,
						testCase.AssertionParameters.Agent,
						testCase.AssertionParameters.Job,
						testCase.AssertionParameters.AutorateRevenue,
						testCase.AssertionParameters.AutorateCosts,
						testCase.AssertionParameters.IsConsolLevelChargeExcluded,
						testCase.AssertionParameters.StandaloneShipmentOnly,
						testCase.AssertionParameters.ExpectedWarnings,
						testCase.AssertionParameters.ExpectedErrors,
						testCase.AssertionParameters.Interactor);
				}
			}

			Assert("This test uses FluentAssertions", true);
		}

		#endregion
	}
}
