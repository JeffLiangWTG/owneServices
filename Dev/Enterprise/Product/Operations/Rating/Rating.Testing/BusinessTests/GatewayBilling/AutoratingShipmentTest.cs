using System.Collections.Generic;
using Enterprise.Rating.Business.Gateway;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AutoratingShipmentTest : BaseGatewayBillingIntegrationTest
	{
		#region Scenario 1

		public void TestAutoratingCost_S1_1_ScenarioS1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.1) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" }, new AssertionCharge { ChargeCode = "GWFWD" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.1"], testCase);
		}

		public void TestAutoratingCost_S1_2_ScenarioS1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.2) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" }, new AssertionCharge { ChargeCode = "GWFWD" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.2"], testCase);
		}

		public void TestAutoratingCost_S1_3_ScenarioS1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.3) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.3"], testCase);
		}

		public void TestAutoratingCost_S1_4_ScenarioS1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.4) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.4"], testCase);
		}

		public void TestAutoratingCost_S1_5_ScenarioS1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.5) under Job Invoicing menu when login company is SGSIN",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SGSIN"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.5"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S1_1_ScenarioS1_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1PlanedLoadRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.1) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" }, new AssertionCharge { ChargeCode = "GWFWD" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.1"], testCase);
		}

		public void TestAutoratingCost_S1_2_ScenarioS1_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1PlanedLoadRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.2) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" }, new AssertionCharge { ChargeCode = "GWFWD" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.2"], testCase);
		}

		public void TestAutoratingCost_S1_3_ScenarioS1_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1PlanedLoadRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.3) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.3"], testCase);
		}

		public void TestAutoratingCost_S1_4_ScenarioS1_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1PlanedLoadRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.4) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.4"], testCase);
		}

		public void TestAutoratingCost_S1_5_ScenarioS1_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS1PlanedLoadRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S1 - Autorating shipment (S1.5) under Job Invoicing menu when login company is SGSIN",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SGSIN"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD" } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S1.5"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 2

		public void TestAutoratingCost_S2_1_ScenarioS2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.1) under Job Invoicing menu when login company is CNSZX",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CNSZX"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.1"], testCase);
		}

		public void TestAutoratingCost_S2_2_ScenarioS2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.2) under Job Invoicing menu when login company is CNSZX",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CNSZX"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.2"], testCase);
		}

		public void TestAutoratingCost_S2_3_ScenarioS2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.3) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.3"], testCase);
		}

		public void TestAutoratingCost_S2_4_ScenarioS2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.4) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.4"], testCase);
		}

		public void TestAutoratingCost_S2_5_ScenarioS2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.5) under Job Invoicing menu when login company is BEANR",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEANR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 6 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.5"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S2_2_ScenarioS2_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.2) under Job Invoicing menu when login company is CNSZX",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CNSZX"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.2"], testCase);
		}

		public void TestAutoratingCost_S2_4_ScenarioS2_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.4) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.4"], testCase);
		}

		public void TestAutoratingCost_S2_5_ScenarioS2_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS2PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S2 - Autorating shipment (S2.5) under Job Invoicing menu when login company is BEANR",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEANR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 6 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S2.5"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 3

		public void TestAutoratingCost_S3_1_ScenarioS3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S3 - Autorating shipment (S3.1) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV", JR_OSCostAmt = 3 }, new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S3.1"], testCase);
		}

		public void TestAutoratingCost_S3_2_ScenarioS3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S3 - Autorating shipment (S3.2) under Job Invoicing menu when login company is SGSIN",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SGSIN"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S3.2"], testCase);
		}

		public void TestAutoratingCost_S3_3_ScenarioS3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S3 - Autorating shipment (S3.3) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S3.3"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S3_1_ScenarioS3_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S3 - Autorating shipment (S3.1) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV", JR_OSCostAmt = 3 }, new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S3.1"], testCase);
		}

		public void TestAutoratingCost_S3_2_ScenarioS3_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S3 - Autorating shipment (S3.2) under Job Invoicing menu when login company is SGSIN",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SGSIN"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S3.2"], testCase);
		}

		public void TestAutoratingCost_S3_3_ScenarioS3_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS3PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S3 - Autorating shipment (S3.3) under Job Invoicing menu when login company is THBKK",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["THBKK"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWPRV", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S3.3"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 5

		public void TestAutoratingCost_S5_1_ScenarioS5()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.1) under Job Invoicing menu when login company is DKCPH",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DKCPH"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 1m } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S5.1"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 2m } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S5.2"], testCase);
		}

		public void TestAutoratingCost_S5_3_ScenarioS5()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.3) under Job Invoicing menu when login company is BEANR",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEANR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 11m } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S5.3"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S5.2"], testCase);
		}

		#region Planned Load

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoadAlternative()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesAlternative();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForC1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForC1();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForC2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForC2();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 5 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForC3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForC3();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 6 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForC4()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForC4();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 7 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForC1C2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForC1C2();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForC2C4()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForC2C4();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 5 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForAll()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForAll();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForAllGTT()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForAllGTT();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEANR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 8 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S5.3"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedLoad_ForAllGTTAlternative()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PLRatesForAllGTTAlternative();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEANR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S5.3"], testCase);
		}
		#endregion

		#region Planned Discharge
		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesAlternative();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_C1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForC1();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_C2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForC2();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 5 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_C3()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForC3();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 6 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_C4()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForC4();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 7 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_C1C2()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForC1C2();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 5 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_C2C4()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForC2C4();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 7 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_All()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForAll();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_AllGTT()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForAllGTT();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEBRE"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 8 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["SSS"], testCase);
		}

		public void TestAutoratingCost_S5_2_ScenarioS5_PlannedDischarge_New_AllGTT1()
		{
			var scenario = ScenarioGenerator.SetupScenarioS5PDRatesForAllGTTAlternative();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S5 - Autorating shipment (S5.2) under Job Invoicing menu when login company is DEBRE",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEANR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S5.3"], testCase);
		}
		#endregion

		#endregion

		#endregion

		#region Scenario 6

		public void TestAutoratingCost_S6_1_ScenarioS6()
		{
			var scenario = ScenarioGenerator.SetupScenarioS6NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S6 - Autorating shipment (S6.1) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 1m } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S6.1"], testCase);
		}

		public void TestAutoratingCost_S6_2_ScenarioS6()
		{
			var scenario = ScenarioGenerator.SetupScenarioS6NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S6 - Autorating shipment (S6.2) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 1m } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S6.2"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S6_1_ScenarioS6_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS6PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S6 - Autorating shipment (S6.1) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S6.1"], testCase);
		}

		public void TestAutoratingCost_S6_2_ScenarioS6_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS6PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S6 - Autorating shipment (S6.2) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S6.2"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 7

		public void TestAutoratingCost_S7_1_ScenarioS7()
		{
			var scenario = ScenarioGenerator.SetupScenarioS7NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S7 - Autorating shipment (S7.1) under Job Invoicing menu when login company is SESTO",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SESTO"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S7.1"], testCase);
		}

		public void TestAutoratingCost_S7_2_ScenarioS7()
		{
			var scenario = ScenarioGenerator.SetupScenarioS7NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S7 - Autorating shipment (S7.2) under Job Invoicing menu when login company is SESTO",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SESTO"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S7.2"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S7_1_ScenarioS7_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS7PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S7 - Autorating shipment (S7.1) under Job Invoicing menu when login company is SESTO",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SESTO"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S7.1"], testCase);
		}

		public void TestAutoratingCost_S7_2_ScenarioS7_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS7PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S7 - Autorating shipment (S7.2) under Job Invoicing menu when login company is SESTO",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SESTO"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S7.2"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 8

		public void TestAutoratingCost_S8_1_ScenarioS8()
		{
			var scenario = ScenarioGenerator.SetupScenarioS8NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S8 - Autorating shipment (S8.1) under Job Invoicing menu when login company is CHZRH",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CHZRH"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = System.Array.Empty<AssertionCharge>()
				}
			};

			AutorateAndAssert(scenario.Shipments["S8.1"], testCase);
		}

		public void TestAutoratingCost_S8_2_ScenarioS8()
		{
			var scenario = ScenarioGenerator.SetupScenarioS8NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S8 - Autorating shipment (S8.2) under Job Invoicing menu when login company is CHZRH",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CHZRH"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S8.2"], testCase);
		}

		public void TestAutoratingCost_S8_3_ScenarioS8()
		{
			var scenario = ScenarioGenerator.SetupScenarioS8NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S8 - Autorating shipment (S8.3) under Job Invoicing menu when login company is DEFRA",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEFRA"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S8.3"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S8_1_ScenarioS8_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS8PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S8 - Autorating shipment (S8.1) under Job Invoicing menu when login company is CHZRH",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CHZRH"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = System.Array.Empty<AssertionCharge>()
				}
			};

			AutorateAndAssert(scenario.Shipments["S8.1"], testCase);
		}

		public void TestAutoratingCost_S8_2_ScenarioS8_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS8PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S8 - Autorating shipment (S8.2) under Job Invoicing menu when login company is CHZRH",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CHZRH"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S8.2"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 9

		public void TestAutoratingCost_S9_1_ScenarioS9()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S9 - Autorating shipment (S9.1) under Job Invoicing menu when login company is DESTR",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DESTR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S9.1"], testCase);
		}

		public void TestAutoratingCost_S9_2_ScenarioS9()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S9 - Autorating shipment (S9.2) under Job Invoicing menu when login company is DEFRA",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEFRA"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S9.2"], testCase);
		}

		public void TestAutoratingCost_S9_3_ScenarioS9()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S9 - Autorating shipment (S9.3) under Job Invoicing menu when login company is DEFRA",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEFRA"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S9.3"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S9_1_ScenarioS9_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S9 - Autorating shipment (S9.1) under Job Invoicing menu when login company is DESTR",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DESTR"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S9.1"], testCase);
		}

		public void TestAutoratingCost_S9_2_ScenarioS9_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S9 - Autorating shipment (S9.2) under Job Invoicing menu when login company is DEFRA",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEFRA"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S9.2"], testCase);
		}

		public void TestAutoratingCost_S9_3_ScenarioS9_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS9PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S9 - Autorating shipment (S9.3) under Job Invoicing menu when login company is DEFRA",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["DEFRA"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S9.3"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 11

		public void TestAutoratingCost_S11_1_ScenarioS11()
		{
			var scenario = ScenarioGenerator.SetupScenarioS11NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S11 - Autorating shipment (S11.1) under Job Invoicing menu when login company is SESTO",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SESTO"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S11.1"], testCase);
		}

		public void TestAutoratingCost_S11_2_ScenarioS11()
		{
			var scenario = ScenarioGenerator.SetupScenarioS11NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S11 - Autorating shipment (S11.2) under Job Invoicing menu when login company is  NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S11.2"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S11_1_ScenarioS11_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS11PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S11 - Autorating shipment (S11.1) under Job Invoicing menu when login company is SESTO",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["SESTO"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S11.1"], testCase);
		}

		public void TestAutoratingCost_S11_2_ScenarioS11_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS11PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S11 - Autorating shipment (S11.2) under Job Invoicing menu when login company is  NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S11.2"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 12

		public void TestAutoratingCost_S12_1_ScenarioS12()
		{
			var scenario = ScenarioGenerator.SetupScenarioS12NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S12 - Autorating shipment (S12.1) under Job Invoicing menu when login company is NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S12.1"], testCase);
		}

		public void TestAutoratingCost_S12_2_ScenarioS12()
		{
			var scenario = ScenarioGenerator.SetupScenarioS12NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S12 - Autorating shipment (S12.2) under Job Invoicing menu when login company is BEBRU",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEBRU"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 6 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S12.2"], testCase);
		}

		public void TestAutoratingCost_S12_3_ScenarioS12()
		{
			var scenario = ScenarioGenerator.SetupScenarioS12NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S12 - Autorating shipment (S12.3) under Job Invoicing menu when login company is NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S12.3"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S12_2_ScenarioS12_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS12PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S12 - Autorating shipment (S12.2) under Job Invoicing menu when login company is BEBRU",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["BEBRU"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 6 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S12.2"], testCase);
		}

		public void TestAutoratingCost_S12_3_ScenarioS12_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS12PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S12 - Autorating shipment (S12.3) under Job Invoicing menu when login company is NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S12.3"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 16

		public void TestAutoratingCost_S16_1_ScenarioS16()
		{
			var scenario = ScenarioGenerator.SetupScenarioS16NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S16 - Autorating shipment (S16.1) under Job Invoicing menu when login company is NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S16.1"], testCase);
		}

		public void TestAutoratingCost_S16_2_ScenarioS16()
		{
			var scenario = ScenarioGenerator.SetupScenarioS16NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S16 - Autorating shipment (S16.2) under Job Invoicing menu when login company is  NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S16.2"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S16_1_ScenarioS16_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS16PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S16 - Autorating shipment (S16.1) under Job Invoicing menu when login company is NLAMS",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["NLAMS"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S16.1"], testCase);
		}

		#endregion

		#endregion

		#region Scenario 18

		public void TestAutoratingCost_S18_1_ScenarioS18()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.1) under Job Invoicing menu when login company is CNSZX",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CNSZX"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 3 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.1"], testCase);
		}

		public void TestAutoratingCost_S18_1_1_ScenarioS18()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.1_1) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 5 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.1"], testCase);
		}

		public void TestAutoratingCost_S18_2_ScenarioS18()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.2) under Job Invoicing menu when login company is CNSZX",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CNSZX"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.2"], testCase);
		}

		public void TestAutoratingCost_S18_3_ScenarioS18()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.3) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = System.Array.Empty<AssertionCharge>()
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.3"], testCase);
		}

		public void TestAutoratingCost_S18_4_ScenarioS18()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18NormalRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.4) under Job Invoicing menu when login company is USMIA",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["USMIA"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = System.Array.Empty<AssertionCharge>()
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.4"], testCase);
		}

		#region Planned Load/Planned Discharge

		public void TestAutoratingCost_S18_1_1_ScenarioS18_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.1_1) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 5 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.1"], testCase);
		}

		public void TestAutoratingCost_S18_2_ScenarioS18_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.2) under Job Invoicing menu when login company is CNSZX",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["CNSZX"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = new[] { new AssertionCharge { ChargeCode = "GWFWD", JR_OSCostAmt = 4 } }
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.2"], testCase);
		}

		public void TestAutoratingCost_S18_3_ScenarioS18_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.3) under Job Invoicing menu when login company is HKHKG",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["HKHKG"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = System.Array.Empty<AssertionCharge>()
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.3"], testCase);
		}

		public void TestAutoratingCost_S18_4_ScenarioS18_PlannedLoadPlannedDischarge()
		{
			var scenario = ScenarioGenerator.SetupScenarioS18PLPDRates();

			var testCase = new GatewayBillingTestCase
			{
				ScenarioDescription = "Scenario S18 - Autorating shipment (S18.4) under Job Invoicing menu when login company is USMIA",
				UserContext = new GatewayBillingTestCaseUserContext
				{
					Branch = scenario.Agents["USMIA"].branch
				},
				AssertionParameters = new AutoratingParameters
				{
					AutorateCosts = true,
					ExpectedCharges = System.Array.Empty<AssertionCharge>()
				}
			};

			AutorateAndAssert(scenario.Shipments["S18.4"], testCase);
		}

		#endregion

		#endregion

		protected override void AutorateAndAssert<T>(T jobParent, IEnumerable<GatewayBillingTestCase> testCases)
		{
			foreach (var testCase in testCases)
			{
				using (SetTemporaryUserContext(testCase.UserContext))
				using (SetTemporaryGatewayConfiguration(testCase))
				{
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
	}
}
