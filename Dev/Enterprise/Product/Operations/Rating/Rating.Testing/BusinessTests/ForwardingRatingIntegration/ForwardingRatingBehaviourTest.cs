using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.GUI.JobInvoicing.ConsolCosting;
using Enterprise.Accounting.Integration;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class ForwardingRatingBehaviourTest : BaseRatingIntegrationTest
	{
		(ForwardingConsol, ForwardingShipment, JobConsolCost) CreateLCLConsolWithExistingConsolCostAndOneChargeCode(string existingCostChargeCode, decimal existingCostAmount, string existingCostRatingBehaviour, string providerChargeCode, decimal providerRateAmount)
		{
			if (providerRateAmount != 0m)
			{
				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "GBSUN", Helper.ChargeCodes[providerChargeCode].AC_Code, providerRateAmount, "KG");
			}

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 1m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes[existingCostChargeCode]);
			consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);
			quickCalculator.QuantityDescription = "Weight";
			quickCalculator.UpdateCost = true;
			quickCalculator.CostRate = existingCostAmount;
			quickCalculator.SetCalculationResults();

			consolCost.E6_RatingBehaviour = existingCostRatingBehaviour;

			Factory.Save();

			return (consol, shipment, consolCost);
		}

		public void TestRatingBehaviour_WhenAutoRateLCLConsol_WithNoExistingCost()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "GBSUN", "FRT", 100m);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 100m);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};
			AutoCostAndAssert("Should create new cost charge with REA rating behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateLCLConsol_WithDuplicateCosts_NEW_REA()
		{
			var (consol, _, _) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100m, RatingBehaviours.CreateNewCharge, "FRT", 200m);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = "NEW",
				}
			};
			AutoCostAndAssert("Should create new cost charge with NEW rating behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			Factory.Save();

			//Re-Autorate
			AutoCostAndAssert("Should not create new cost", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateLCLConsol_WithDuplicateCosts()
		{
			var (consol, _, _) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100m, RatingBehaviours.CreateNewCharge, "FRT", 200m);

			var consolCost1 = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost1.E6_LocalCostAmount = 150m;
			consolCost1.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost1.E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;

			var consolCost2 = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost2.E6_LocalCostAmount = 300m;
			consolCost2.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost2.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 150m,
					E6_RatingBehaviour = RatingBehaviours.StopFromAutorating,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = "NEW",
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 300m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should create a new charge as the existing costs doesn't have creditor", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateLCLConsol_WithExistingCost_NEW()
		{
			var (consol, _, _) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("BAF", 100m, RatingBehaviours.CreateNewCharge, "FRT", 200m);

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost.E6_LocalCostAmount = 150m;
			consolCost.E6_ApportionmentMethod = AllocationMethod.Manual;
			consolCost.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;
			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = "NEW",
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 150m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};
			AutoCostAndAssert("Since there is no payment basis for the FRT = 150, therefore, it cannot be reAutorated", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateLCLConsol_WithExistingCost_REA()
		{
			var (consol, _, _) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100m, RatingBehaviours.ReAutorateCharge, "FRT", 200m);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should delete existing cost and recreate", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(consol, "should contain expected logs",
				"The existing charge 'FRT = 100' with 'Rating Behavior = REA' has been deleted as a new charge has been found with similar Charge Code, Creditor, Chargeable Unit and Supplier Cost Reference.");
		}

		public void TestRatingBehaviour_WhenAutoRateLCLConsol_WithExistingCost_REA_NoRateFound_ExistingCostsShouldNotBeRemoved()
		{
			var (consol, _, _) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100, RatingBehaviours.ReAutorateCharge, "FRT", 0m);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge
				}
			};

			AutoCostAndAssert("Should not remove existing cost", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateLCLConsol_WithExistingCost_STP()
		{
			var (consol, _, _) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100m, RatingBehaviours.StopFromAutorating, "FRT", 200m);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.StopFromAutorating,
				}
			};
			AutoCostAndAssert("Should change nothing with STP rating behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_ShipmentCharges_FromConsol()
		{
			var (consol, shipment, _) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100m, RatingBehaviours.ReAutorateCharge, "FRT", 200m);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should update existing cost charge with REA rating behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			var charges = ((Job)shipment.Job).Charges;
			var charge = charges.OfType<Charge>().Single();
			AssertEquals("The local cost amount should match the expected value.", (ZDecimal)200m, charge.JR_LocalCostAmt);
			AssertEquals("The cost rating behavior should match the expected stop-from-autorating value.", RatingBehaviours.StopFromAutorating, charge.JR_Calc_CostRatingBehavior);
			AssertEquals("The charge should be marked as apportioned.", ZBool.True, charge.JR_IsApportioned);

			var expectedRatingBehaviors = new[] { RatingBehaviours.StopFromAutorating, "NEW" };
			AssertContainsExactElementsInAnyOrder(
				"The rating behaviors should match the expected collection.",
				expectedRatingBehaviors,
				charge.Lookups.RatingBehaviors_Cost.GetAllCodes()
			);
		}

		public void TestRatingBehaviour_ShipmentCharges_UpdateApportionedCost()
		{
			var (consol, shipment, consolCost) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100, RatingBehaviours.StopFromAutorating, "FRT", 200m);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.StopFromAutorating,
				}
			};
			AutoCostAndAssert("Should change nothing with STP rating behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			var charges = ((Job)shipment.Job).Charges;

			var charge = charges.OfType<Charge>().Single();
			AssertEquals("JR_LocalCostAmt should match expected value", (ZDecimal)100m, charge.JR_LocalCostAmt);
			AssertEquals("JR_Calc_CostRatingBehavior should match expected behavior", RatingBehaviours.StopFromAutorating, charge.JR_Calc_CostRatingBehavior);
			AssertEquals("JR_IsApportioned should be true", ZBool.True, charge.JR_IsApportioned);

			var actualRatingBehaviors = charge.Lookups.RatingBehaviors_Cost.GetAllCodes().ToArray();
			var expectedRatingBehaviors = new[] { RatingBehaviours.StopFromAutorating, "NEW" };
			AssertContainsExactElementsInAnyOrder("Rating behaviors should match expected values", expectedRatingBehaviors, actualRatingBehaviors);

			charge.JR_Calc_CostRatingBehavior = "NEW";
			Factory.Save();

			AssertEquals("E6_RatingBehaviour on consolCost should remain unchanged", RatingBehaviours.StopFromAutorating, consolCost.E6_RatingBehaviour);
		}

		public void TestRatingBehaviour_ShipmentCharges_AfterReverseInvoiceInConsol()
		{
			var (consol, shipment, consolCost) = CreateLCLConsolWithExistingConsolCostAndOneChargeCode("FRT", 100m, RatingBehaviours.CreateNewCharge, "FRT", 200m);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = "NEW",
				}
			};
			AutoCostAndAssert("Test REA Behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			var consolAPInvoice = Factory.New<Accounting.Business.ARAP.Invoicing.APInvoice>();
			consolAPInvoice.AH_TransactionNum = "1111";
			consolCost.E6_AH_APInvoice = consolAPInvoice.PK; // posting
			AssertEquals(true, consolCost.IsPosted);

			var charges = ((Job)shipment.Job).Charges;
			AssertEquals(2, charges.Count);
			var charge = charges[0];
			AssertEquals(RatingBehaviours.StopFromAutorating, charge.JR_Calc_CostRatingBehavior);
			charge.JR_Calc_SellRatingBehavior = RatingBehaviours.ReAutorateCharge;
			consolCost.E6_AH_APInvoice = Guid.Empty; // unpost
			AssertEquals(false, consolCost.IsPosted);

			AssertEquals(RatingBehaviours.StopFromAutorating, charge.JR_Calc_CostRatingBehavior);
		}

		public void TestRatingBehaviour_WhenAutoRateFCLConsol_WithExistingCost_NEW()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);
			quickCalculator.QuantityDescription = "Container Count";
			quickCalculator.UpdateCost = true;
			quickCalculator.Containers[0].Cost = 200m;
			quickCalculator.SetCalculationResults();

			consolCost.E6_RatingBehaviour = RatingBehaviours.CreateNewCharge;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = "NEW",
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should create new cost charge with REA rating behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateFCLConsol_WithExistingCost_REA()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost.E6_OH_Creditor = TransportProvider1.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);
			quickCalculator.QuantityDescription = "Container Count";
			quickCalculator.UpdateCost = true;
			quickCalculator.Containers[0].Cost = 200m;
			quickCalculator.SetCalculationResults();

			consolCost.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};
			AutoCostAndAssert("Should delete existing cost and recreate", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestAutoRateConsol_WhenRatingBehaviourIsREA_WithExistingCostWithoutChargeCode()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost.E6_OH_Creditor = TransportProvider1.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);
			quickCalculator.QuantityDescription = "Container Count";
			quickCalculator.UpdateCost = true;
			quickCalculator.Containers[0].Cost = 200m;
			quickCalculator.SetCalculationResults();
			consolCost.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;

			Factory.Save();

			consolCost.E6_AC_ChargeCode = ZGuid.Empty;

			var businessEntity = consol as IBusiness;
			using (var form = new ZForm(businessEntity))
			{
				form.PlugIns.Add(ControllerIDs.Apportionment);
				form.DisplayMode = ODisplayMode.Edit;

				using (var plugin = (ApportionmentPlugin)form.PlugIns.Instances[0])
				{
					var item = plugin.TopLevelMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Autorate Costs");
					item.PerformClick();

					AssertNoExceptionThrown(() => item.PerformClick());
				}
			}
		}

		public void TestAutoRateConsol_WhenRatingBehaviourIsSTP_WithExistingCostWithoutChargeCode()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost.E6_OH_Creditor = TransportProvider1.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);
			quickCalculator.QuantityDescription = "Container Count";
			quickCalculator.UpdateCost = true;
			quickCalculator.Containers[0].Cost = 200m;
			quickCalculator.SetCalculationResults();
			consolCost.E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;

			Factory.Save();

			consolCost.E6_AC_ChargeCode = ZGuid.Empty;

			var businessEntity = consol as IBusiness;
			using (var form = new ZForm(businessEntity))
			{
				form.PlugIns.Add(ControllerIDs.Apportionment);
				form.DisplayMode = ODisplayMode.Edit;

				using (var plugin = (ApportionmentPlugin)form.PlugIns.Instances[0])
				{
					var item = plugin.TopLevelMenu.MenuItems.Cast<MenuItem>().FirstOrDefault(x => x.Text == "Autorate Costs");
					item.PerformClick();

					AssertNoExceptionThrown(() => item.PerformClick());
				}
			}
		}

		public void TestRatingBehaviour_WhenAutoRateFCLConsol_WithExistingCost_STP_ButCostReferenceIsDifferent()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost.E6_OH_Creditor = TransportProvider1.PK;
			consolCost.E6_CostReference = "A100";

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);
			quickCalculator.QuantityDescription = "Container Count";
			quickCalculator.UpdateCost = true;
			quickCalculator.Containers[0].Cost = 200m;
			quickCalculator.SetCalculationResults();

			consolCost.E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.StopFromAutorating,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};
			AutoCostAndAssert("Should still create FRT charge as creditor is different", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateFCLConsol_FlatRateAndUnitRate_REA_MergedCharges()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");
			rateEntry.AddFlatRateLine("BAF", 150m);
			rateEntry.AddRateLine("FRT", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 110m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var consolCost1 = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost1.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost1.E6_OH_Creditor = TransportProvider1.PK;

			var quickCalculator1 = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost1);
			quickCalculator1.QuantityDescription = "Container Count";
			quickCalculator1.UpdateCost = true;
			quickCalculator1.Containers[0].Cost = 200m;
			quickCalculator1.SetCalculationResults();

			consolCost1.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;

			var consolCost2 = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost2.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost2.E6_OH_Creditor = TransportProvider1.PK;

			var quickCalculator2 = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost2);
			quickCalculator2.QuantityDescription = "Container Count";
			quickCalculator2.UpdateCost = true;
			quickCalculator2.Containers[0].Cost = 300m;
			quickCalculator2.SetCalculationResults();

			consolCost2.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;

			var consolCost3 = CreateConsolCost(consol, Helper.ChargeCodes["BAF"]);
			consolCost3.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost3.E6_OH_Creditor = TransportProvider1.PK;

			var quickCalculator3 = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost3);
			quickCalculator3.QuantityDescription = "Container Count";
			quickCalculator3.UpdateCost = true;
			quickCalculator3.Containers[0].Cost = 400m;
			quickCalculator3.SetCalculationResults();

			consolCost3.E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 210m, // calculate fromm 100 + 110
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 150m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 400m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
			};
			AutoCostAndAssert("two charges from costing module are merged into one and this charge should delete two existing FRT charge", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenAutoRateFCLConsol_FlatRateAndUnitRate_STP()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");
			rateEntry.AddFlatRateLine("BAF", 150);

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			var consolCost = CreateConsolCost(consol, Helper.ChargeCodes["FRT"]);
			consolCost.E6_ApportionmentMethod = AllocationMethod.ChargeableUnits;
			consolCost.E6_OH_Creditor = TransportProvider1.PK;

			var quickCalculator = new JobChargeQuickCalculateBusinessObject(consol.RatingAdapter, consolCost);
			quickCalculator.QuantityDescription = "Container Count";
			quickCalculator.UpdateCost = true;
			quickCalculator.Containers[0].Cost = 200m;
			quickCalculator.SetCalculationResults();

			consolCost.E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200,
					E6_RatingBehaviour = RatingBehaviours.StopFromAutorating,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 150m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};
			AutoCostAndAssert("should only create the BAF charge", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenReAutoRateFCLConsol_PreviouslyAutoRated()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var entry = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");
			entry.AddRateLine("BAF", UnitCalculator.Code, "CN").GetCalculator<UnitCalculator>().PerUnit = 110m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 110m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should create FRT and BAF", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK)).ToArray();
			AssertEquals(2, consolCosts.Length);

			consolCosts.First(c => c.E6_AC_ChargeCode == Helper.ChargeCodes["BAF"].PK).E6_RatingBehaviour = RatingBehaviours.CreateNewCharge;
			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 110m,
					E6_RatingBehaviour = "NEW",
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 110m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should delete FRT and recreate it again, keep BAF and create a new BAF as well", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenReAutoRateFCLConsol_MultiplePaymentBasesForOneCharge()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var entry = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");
			var calculator = entry.AddRateLine("BAF", CombinedCalculator.Code, "CN").GetCalculator<CombinedCalculator>();
			calculator.PerUnit = 110m;
			calculator.BaseRate = 50;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = GP20.PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 160m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should create FRT and BAF", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			// remove the existing charges and create a different charge
			entry.RateLines.RemoveAndDeleteAll();
			entry.AddFlatRateLine("BAF", 444m);

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 444m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should delete the existing BAF and recreate a new one", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		public void TestRatingBehaviour_WhenReAutoRateFCLConsol_NEW_STP_REA()
		{
			var costing = Helper.NewCosting(TransportProvider1);
			costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 100m, "CN", container: "20GP");
			var gp40Entry = costing.AddRateEntryWithUnitRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "FRT", 200m, "CN", container: "40GP");

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "FAKE4100011";
			container1.JC_RC = GP20.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "FAKE4100022";
			container2.JC_RC = GP40.PK;
			container1.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				}
			};

			AutoCostAndAssert("Should create new charge with REA as rating behaviour", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			consolCosts.Single(x => x.E6_OSCostAmount == 100m).E6_RatingBehaviour = RatingBehaviours.CreateNewCharge;
			consolCosts.Single(x => x.E6_OSCostAmount == 200m).E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = "NEW", // 20GP
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge, // 20GP
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.StopFromAutorating, // 40GP
				}
			};

			AutoCostAndAssert("Should create a new charge for FRT 20GP and do not create FRT for 40GP", null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);

			consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			consolCosts.Single(x => x.E6_OSCostAmount == 200m).E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge; // now both existing costs have REA as rating behaviour

			var toDelete = consolCosts.Single(x => x.E6_OSCostAmount == 100m && x.E6_RatingBehaviour == RatingBehaviours.CreateNewCharge);
			toDelete.Delete();

			gp40Entry.Delete(); // deleting this rate and so only 20GP remains

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
				},
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge, // 40GP
				}
			};

			AutoCostAndAssert("as we don't have charge for 40GP (FRT=200), then it should not delete the related charge for 40GP and only the 20GP one (FRT = 100) should be re-created",
				null, expectedCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
			AssertAutoratingAuditLogNoteContainsLines(consol, "should contain expected logs",
				@"	The following costs were found:
	  • FRT charge from Costing TRASPROV1
	Charges created: FRT",
				"The existing charge 'FRT = 100' with 'Rating Behavior = REA' has been deleted as a new charge has been found with similar Charge Code, Creditor, Chargeable Unit and Supplier Cost Reference.");
		}

		public void TestRatingBehavior_STP_JobCreditorAndCarrierDifferent_DoesNotChangeJobCreditorCharge()
		{
			TestRatingBehaviorWhenJobCreditorAndCarrierDifferent(
				(cost) =>
				{
					cost.E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;
				},
				"There should be exactly one cost, referring to the first charge",
				new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 100m,
						E6_RatingBehaviour = RatingBehaviours.StopFromAutorating,
						E6_OH_Creditor = TransportProvider2.PK
					},
				});
		}

		public void TestRatingBehavior_REA_JobCreditorAndCarrierDifferent_ReautoratesJobCreditorCharge()
		{
			TestRatingBehaviorWhenJobCreditorAndCarrierDifferent(
				(cost) => { },
				"There should be exactly one cost, referring to the new BAF and not the initial one",
				new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 200m,
						E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
						E6_OH_Creditor = TransportProvider2.PK
					},
				});
		}

		public void TestRatingBehavior_STP_JobCreditorAndCarrierDifferent_AllowsCarrierChargeWhenCreditorChargeIsSTP()
		{
			TestRatingBehaviorWhenJobCreditorAndCarrierDifferent(
				(cost) =>
				{
					cost.E6_RatingBehaviour = RatingBehaviours.StopFromAutorating;
					cost.E6_OH_Creditor = TransportProvider1.PK;
				},
				"There should be exactly two costs. One for the original, one for the second autorate",
				new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 100m,
						E6_RatingBehaviour = RatingBehaviours.StopFromAutorating,
						E6_OH_Creditor = TransportProvider1.PK,
					},
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 200m,
						E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
						E6_OH_Creditor = TransportProvider2.PK
					},
				});
		}

		public void TestRatingBehavior_REA_JobCreditorAndCarrierDifferent_OrignalChargeRemainsAndNewAdded()
		{
			TestRatingBehaviorWhenJobCreditorAndCarrierDifferent(
				(cost) =>
				{
					cost.E6_OH_Creditor = TransportProvider1.PK;
				},
				"There should be exactly two costs. One for the original, one for the second autorate",
				new[]
				{
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 100m,
						E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
						E6_OH_Creditor = TransportProvider1.PK,
					},
					new AssertionCost
					{
						ChargeCode = "BAF",
						E6_OSCostAmount = 200m,
						E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
						E6_OH_Creditor = TransportProvider2.PK
					},
				});
		}

		void TestRatingBehaviorWhenJobCreditorAndCarrierDifferent(Action<JobConsolCost> visitJobCost, string message, AssertionCost[] expectedFinalCosts)
		{
			var costing = Helper.NewCosting(TransportProvider1);
			var rateEntry = costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "BAF", 100m);
			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "AUBNE", "GBSUN", 0m);
			shipment.JS_PackingMode = ContainerModes.FCL;
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUBNE", "GBSUN", TransportProvider1, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			TransportProvider2.OH_IsCreditor = true;
			consol.JK_OA_CreditorAddress = TransportProvider2.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = TransportProvider1.MainAddress.PK;

			Factory.Save();

			var expectedCostsAfterFirstAutorate = new[]
			{
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 100m,
					E6_RatingBehaviour = RatingBehaviours.ReAutorateCharge,
					// Final carrier on the charge.
					E6_OH_Creditor = TransportProvider2.PK
				},
			};
			AutoCostAndAssert("Should create new charge with ReAutorateCharge as rating behaviour", null, expectedCostsAfterFirstAutorate, consol, autorateRevenue: false, deleteExistingCosts: false);

			var consolCosts = Factory.Load<JobConsolCost>(new ZQuery(JobConsolCostSchema.E6_ParentID, consol.PK));
			var bafCost = consolCosts.Single(x => x.E6_OSCostAmount == 100m);
			visitJobCost(bafCost);

			// Re-create a the available BAF charge on the costing to witness
			// if it changes by being very different to before
			rateEntry.Delete(); // remove BAF 100
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUBNE", "GBSUN", "BAF", 200m);
			Factory.Save();

			AutoCostAndAssert(message, null, expectedFinalCosts, consol, autorateRevenue: false, deleteExistingCosts: false);
		}

		#region SetUp/TearDown

		protected override void SetUp()
		{
			base.SetUp();

			tempRatesServiceSearch = DataRegistryRating.Instance.RatesServiceSubscription.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, RatesServiceRegistrySettingsCollection.GetDisabled());
		}

		protected override void TearDown()
		{
			tempRatesServiceSearch.Dispose();

			base.TearDown();
		}

		IDisposable tempRatesServiceSearch = DisposableAction.NoAction;

		#endregion
	}
}
