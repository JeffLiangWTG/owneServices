using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.ProfitShare;
using Enterprise.Accounting.Integration;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AccountingRatingIntegrationTest : BaseRatingIntegrationTest
	{
		#region Creditors

		public void TestAutoRatingCostDoesNotSetInvalidCreditorFromServiceProvider()
		{
			var carrier = Helper.NewOrgHeader();
			var costing = Helper.NewCosting(carrier);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUBNE", "NZAKL", "FRT", 30m);

			carrier.OH_IsCreditor = false;
			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUBNE", "NZAKL", 1000);
			var consol = CreateForwardingConsol(TransportModes.Air, "AUBNE", "NZAKL", carrier, shipment);
			consol.JK_PrepaidCollect = PaymentType.Prepaid;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 30m,
					CostAccountCode = ""
				}
			};

			AutorateAndAssert("Creditor field should be empty", expected, shipment, Consignor, autorateRevenue: false);

			carrier.OH_IsCreditor = true;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 30m,
					CostAccountCode = carrier.OH_Code
				}
			};

			AutorateAndAssert("Since creditor is payable it should be set", expected, shipment, Consignor, autorateRevenue: false);
		}

		public void TestAutoRatingRevenueDoesNotSetInvalidCreditorFromSupplier()
		{
			TransportProvider1.OH_IsCreditor = true;
			TransportProvider2.OH_IsCreditor = false;

			var clientRate = Helper.NewClientRate(NewClient);
			clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX", "FRT", 30m);

			var orgEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.LCL, "AUSYD", "USLAX", "OCART", 10m);
			orgEntry.TI_OH_Supplier = TransportProvider1.PK;

			var dstEntry = clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.LCL, "AUSYD", "USLAX", "DDOC", 20m);
			dstEntry.TI_OH_Supplier = TransportProvider2.PK;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "AUSYD", "USLAX", 1000);
			shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = TransportProvider1.MainAddress.PK;
			shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = TransportProvider2.MainAddress.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSSellAmt = 30m,
					CostAccountCode = ""
				},
				new AssertionCharge
				{
					ChargeCode = "OCART",
					JR_OSSellAmt = 10m,
					CostAccountCode = TransportProvider1.OH_Code
				},
				new AssertionCharge
				{
					ChargeCode = "DDOC",
					JR_OSSellAmt = 20m,
					CostAccountCode = ""
				}
			};

			AutorateAndAssert("Should not set the creditor for DDOC since provider is not a payable", expected, shipment, NewClient, autorateCosts: false);
		}

		#endregion

		#region Profit Share

		public void TestRegisteredAddedChargesAreIncludedInProfitShareCalculation_CreateAsAR()
		{
			TestRegisteredAddedChargesAreIncludedInProfitShareCalculation(true);
		}

		public void TestRegisteredAddedChargesAreIncludedInProfitShareCalculation_CreateAsAP()
		{
			TestRegisteredAddedChargesAreIncludedInProfitShareCalculation(false);
		}

		void TestRegisteredAddedChargesAreIncludedInProfitShareCalculation(bool createProfitShareAsAR)
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, createProfitShareAsAR);

			var consolChargeCodes = LoadChargeCodesForTesting();
			OrganisationRegistry.Instance.CustomProfitShareAgreementTypeChargeCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, $"{consolChargeCodes.FRT.PK},{consolChargeCodes.FSC.PK}");

			CreateInternalProfitSplitChargeCode();

			var parties = SetupNewSendingReceivingPartiesWithProfitSplitAgreement();
			var sendingParty = parties.Item1;
			var receivingParty = parties.Item2;

			Helper.NewClientRateWithSingleRateLine(receivingParty, RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US", consolChargeCodes.FRT.AC_Code, 100m);

			var shipment = CreateForwardingShipment(TransportModes.Air, sendingParty.PK, receivingParty.PK, "AUSYD", "USLAX", 100m);
			Factory.Save();

			CreateNewConsolWithSenderReceiverInParties(shipment, parties);

			var billingJob = CreateNewBillingJob(shipment);
			var expectedCharges = new List<AssertionCharge>
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 100m
				}
			};
			AutorateAndAssert("AutoRating should create the FRT charge", expectedCharges, shipment, sendingParty, autorateCosts: false, job: billingJob);

			var frtJobCharge1 = billingJob.Charges[0];
			frtJobCharge1.JR_AgentDeclaredCostAmt = 0;
			frtJobCharge1.JR_OSCostAmt = 0;
			Factory.Save();

			AssertJobTotalValues("Total values on Billing Job #1 - after autorating and charge modification", billingJob, 0m, 100m, 100m);

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, shipment, billingJob);
			profitShareShipmentChargeProcessor.Process();

			var expectedProfitShareCost1 = createProfitShareAsAR ? 0m : 50m;
			var expectedProfitShareRevenue1 = createProfitShareAsAR ? -50m : 0m;

			expectedCharges[0].JR_OSCostAmt = 0;
			expectedCharges.Add(new AssertionCharge
			{
				ChargeCode = "PS",
				JR_OSCostAmt = expectedProfitShareCost1,
				JR_OSSellAmt = expectedProfitShareRevenue1,
				JR_Desc = "Profit Share / Rebate - Receiving Agent - 50.00% of profit of 100.00"
			});

			AssertCharges("There should be a new created PS charge", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #2 - after manually creating new profit share charge", billingJob, expectedProfitShareCost1, 100m + expectedProfitShareRevenue1, 50m);

			AddChargeToBillingJob(billingJob, consolChargeCodes.FSC, 100m, 300m);

			Factory.Save();

			var expectedProfitShareCost2 = createProfitShareAsAR ? 0m : 100m;
			var expectedProfitShareRevenue2 = createProfitShareAsAR ? -100m : 0m;
			ZDecimal expectedPreviousProfit = createProfitShareAsAR ? expectedProfitShareRevenue1 : expectedProfitShareCost1;

			expectedCharges.AddRange(new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FSC",
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 300m
				},
				new AssertionCharge
				{
					ChargeCode = "INP",
					JR_OSCostAmt = expectedProfitShareCost2,
					JR_OSSellAmt = expectedProfitShareRevenue2,
					JR_Desc = $"Internal Profit Adjustment - Adjustment: less {expectedPreviousProfit.ToString(2)} - Receiving Agent - 50.00% of profit of 300.00"
				}
			});
			AssertCharges("New Profit Share Adjustment charge (INP) is added automatically when saving.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #3 - after adding FSC charge with profit share", billingJob,
				100m + expectedProfitShareCost1 + expectedProfitShareCost2, 400m + expectedProfitShareRevenue1 + expectedProfitShareRevenue2, 150m);

			AddChargeToBillingJob(billingJob, consolChargeCodes.FRT, 200m, 500m);
			AddChargeToBillingJob(billingJob, consolChargeCodes.FSC, 100m, 150m);

			Factory.Save();

			var expectedProfitShareCost3 = createProfitShareAsAR ? 0m : 175m;
			var expectedProfitShareRevenue3 = createProfitShareAsAR ? -175m : 0m;
			expectedPreviousProfit = createProfitShareAsAR
				? expectedProfitShareRevenue1 + expectedProfitShareRevenue2
				: expectedProfitShareCost1 + expectedProfitShareCost2;

			expectedCharges.AddRange(new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 200m,
					JR_OSSellAmt = 500m
				},
				new AssertionCharge
				{
					ChargeCode = "FSC",
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 150m
				},
				new AssertionCharge
				{
					ChargeCode = "INP",
					JR_OSCostAmt = expectedProfitShareCost3,
					JR_OSSellAmt = expectedProfitShareRevenue3,
					JR_Desc = $"Internal Profit Adjustment - Adjustment: less {expectedPreviousProfit.ToString(2)} - Receiving Agent - 50.00% of profit of 650.00"
				},
			});
			AssertCharges("New Profit Share Adjustment charge (INP) is added automatically when saving.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #4 - after adding 2 new charges FRT and FSC with profit share", billingJob, 400m + expectedProfitShareCost1 + expectedProfitShareCost2 + expectedProfitShareCost3,
				1050m + expectedProfitShareRevenue1 + expectedProfitShareRevenue2 + expectedProfitShareRevenue3, 325m);

			var bafChargeCodeNotInRegistry = LoadConsolChargeCode("BAF");
			var bafJobChargeNotForProfitShare = AddChargeToBillingJob(billingJob, bafChargeCodeNotInRegistry, 100m, 150m);
			AssertEquals("BAF charge should not be included in Profit Share", false, bafJobChargeNotForProfitShare.JR_IsIncludedInProfitShare);

			Factory.Save();

			expectedCharges.Add(new AssertionCharge
			{
				ChargeCode = "BAF",
				JR_OSCostAmt = 100m,
				JR_OSSellAmt = 150m
			});
			AssertCharges("No new Profit Share charge is added because no adjustment is needed.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #5 - after adding BAF - not included in profit share calculation", billingJob, 500m + expectedProfitShareCost1 + expectedProfitShareCost2 + expectedProfitShareCost3,
				1200m + expectedProfitShareRevenue1 + expectedProfitShareRevenue2 + expectedProfitShareRevenue3, 375m);
		}

		public void TestUpdateOrDeleteProfitShareChargesShouldGenerateProfitShareAdjustments_CreateAsAR()
		{
			TestUpdateOrDeleteProfitShareChargesShouldGenerateProfitShareAdjustments(true);
		}

		public void TestUpdateOrDeleteProfitShareChargesShouldGenerateProfitShareAdjustments_CreateAsAP()
		{
			TestUpdateOrDeleteProfitShareChargesShouldGenerateProfitShareAdjustments(false);
		}

		void TestUpdateOrDeleteProfitShareChargesShouldGenerateProfitShareAdjustments(bool createProfitShareAsAR)
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, createProfitShareAsAR);

			var consolChargeCodes = LoadChargeCodesForTesting();
			OrganisationRegistry.Instance.CustomProfitShareAgreementTypeChargeCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, $"{consolChargeCodes.FRT.PK},{consolChargeCodes.FSC.PK}");

			CreateInternalProfitSplitChargeCode();

			var parties = SetupNewSendingReceivingPartiesWithProfitSplitAgreement();
			var sendingParty = parties.Item1;
			var receivingParty = parties.Item2;

			Helper.NewClientRateWithSingleRateLine(receivingParty, RatingConstants.RateCategory.AIR, RateMode.LSE, "AU", "US", consolChargeCodes.FRT.AC_Code, 100m);

			var shipment = CreateForwardingShipment(TransportModes.Air, sendingParty.PK, receivingParty.PK, "AUSYD", "USLAX", 100m);
			Factory.Save();

			CreateNewConsolWithSenderReceiverInParties(shipment, parties);

			var expectedCharges = new List<AssertionCharge>
			{
				new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 100m }
			};

			var billingJob = CreateNewBillingJob(shipment);
			AutorateAndAssert("AutoRating should create the FRT charge", expectedCharges, shipment, sendingParty, autorateCosts: false, job: billingJob);

			var frtJobCharge = billingJob.Charges[0];
			frtJobCharge.JR_AgentDeclaredCostAmt = 0;
			frtJobCharge.JR_OSCostAmt = 0;
			Factory.Save();

			AssertJobTotalValues("Total values on Billing Job #1 - after autorating and charge modification", billingJob, 0m, 100m, 100m);

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, shipment, billingJob);
			profitShareShipmentChargeProcessor.Process();

			var expectedProfitShareCost1 = createProfitShareAsAR ? 0m : 50m;
			var expectedProfitShareRevenue1 = createProfitShareAsAR ? -50m : 0m;
			expectedCharges[0].JR_OSCostAmt = 0;
			expectedCharges.Add(new AssertionCharge
			{
				ChargeCode = "PS",
				JR_OSCostAmt = expectedProfitShareCost1,
				JR_OSSellAmt = expectedProfitShareRevenue1,
				JR_Desc = "Profit Share / Rebate - Receiving Agent - 50.00% of profit of 100.00"
			});

			AssertCharges("There should be a new created PS charge", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #2 - after manually creating new profit share charge", billingJob, expectedProfitShareCost1, 100m + expectedProfitShareRevenue1, 50m);

			var fscJobCharge = AddChargeToBillingJob(billingJob, consolChargeCodes.FSC, 100m, 300m);

			Factory.Save();

			var expectedProfitShareCost2 = createProfitShareAsAR ? 0m : 100m;
			var expectedProfitShareRevenue2 = createProfitShareAsAR ? -100m : 0m;
			ZDecimal expectedPreviousProfit = createProfitShareAsAR ? expectedProfitShareRevenue1 : expectedProfitShareCost1;
			expectedCharges.AddRange(new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FSC",
					JR_OSCostAmt = 100m,
					JR_OSSellAmt = 300m
				},
				new AssertionCharge
				{
					ChargeCode = "INP",
					JR_OSCostAmt = expectedProfitShareCost2,
					JR_OSSellAmt = expectedProfitShareRevenue2,
					JR_Desc = $"Internal Profit Adjustment - Adjustment: less {expectedPreviousProfit.ToString(2)} - Receiving Agent - 50.00% of profit of 300.00"
				}
			});

			AssertCharges("New Profit Share Adjustment charge (INP) is added automatically when saving.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #3 - after adding FSC charge with profit share", billingJob, 100m + expectedProfitShareCost1 + expectedProfitShareCost2, 400m + expectedProfitShareRevenue1 + expectedProfitShareRevenue2, 150m);

			fscJobCharge.JR_OSCostAmt = 150m;
			Factory.Save();

			var expectedProfitShareCost3 = createProfitShareAsAR ? 0m : -25m;
			var expectedProfitShareRevenue3 = createProfitShareAsAR ? 25m : 0m;
			expectedPreviousProfit = createProfitShareAsAR
				? expectedProfitShareRevenue1 + expectedProfitShareRevenue2
				: expectedProfitShareCost1 + expectedProfitShareCost2;
			expectedCharges[2].JR_OSCostAmt = 150; // FSC charge
			expectedCharges.Add(new AssertionCharge
			{
				ChargeCode = "INP",
				JR_OSCostAmt = expectedProfitShareCost3,
				JR_OSSellAmt = expectedProfitShareRevenue3,
				JR_Desc = $"Internal Profit Adjustment - Adjustment: less {expectedPreviousProfit.ToString(2)} - Receiving Agent - 50.00% of profit of 250.00"
			});

			AssertCharges("New Profit Share Adjustment charge (INP) is added automatically when saving.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #4 - after modifying FSC charge with profit share", billingJob, 150m + expectedProfitShareCost1 + expectedProfitShareCost2 + expectedProfitShareCost3,
				400m + expectedProfitShareRevenue1 + expectedProfitShareRevenue2 + expectedProfitShareRevenue3, 125m);

			billingJob.Charges.RemoveAndDelete(fscJobCharge);
			Factory.Save();

			var expectedProfitShareCost4 = createProfitShareAsAR ? 0m : -75m;
			var expectedProfitShareRevenue4 = createProfitShareAsAR ? 75m : 0m;
			expectedPreviousProfit = createProfitShareAsAR
				? expectedProfitShareRevenue1 + expectedProfitShareRevenue2 + expectedProfitShareRevenue3
				: expectedProfitShareCost1 + expectedProfitShareCost2 + expectedProfitShareCost3;
			expectedCharges.RemoveAt(2);
			expectedCharges.Add(new AssertionCharge
			{
				ChargeCode = "INP",
				JR_OSCostAmt = expectedProfitShareCost4,
				JR_OSSellAmt = expectedProfitShareRevenue4,
				JR_Desc = $"Internal Profit Adjustment - Adjustment: less {expectedPreviousProfit.ToString(2)} - Receiving Agent - 50.00% of profit of 100.00"
			});

			AssertCharges("New Profit Share Adjustment charge (INP) is added automatically when saving.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #5 - after deleting FSC charge with profit share", billingJob, 0m + expectedProfitShareCost1 + expectedProfitShareCost2 + expectedProfitShareCost3 + expectedProfitShareCost4,
				100m + expectedProfitShareRevenue1 + expectedProfitShareRevenue2 + expectedProfitShareRevenue3 + expectedProfitShareRevenue4, 50m);
		}

		public void TestUpdateChargeAmountToZeroShouldCreateANewAdjustmentShareCharge_CreateAsAR()
		{
			TestUpdateChargeAmountToZeroShouldCreateANewAdjustmentShareCharge(true);
		}

		public void TestUpdateChargeAmountToZeroShouldCreateANewAdjustmentShareCharge_CreateAsAP()
		{
			TestUpdateChargeAmountToZeroShouldCreateANewAdjustmentShareCharge(false);
		}

		void TestUpdateChargeAmountToZeroShouldCreateANewAdjustmentShareCharge(bool createProfitShareAsAR)
		{
			AccountingConfigurationRegistry.Instance.CreateProfitShareAsAR.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, createProfitShareAsAR);

			var consolChargeCodes = LoadChargeCodesForTesting();
			OrganisationRegistry.Instance.CustomProfitShareAgreementTypeChargeCodes.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, $"{consolChargeCodes.FRT.PK}");

			CreateInternalProfitSplitChargeCode();

			var parties = SetupNewSendingReceivingPartiesWithProfitSplitAgreement();
			var sendingParty = parties.Item1;
			var receivingParty = parties.Item2;

			var shipment = CreateForwardingShipment(TransportModes.Air, sendingParty.PK, receivingParty.PK, "AUSYD", "USLAX", 100m);

			CreateNewConsolWithSenderReceiverInParties(shipment, parties);

			var billingJob = CreateNewBillingJob(shipment);
			var frtJobCharge = AddChargeToBillingJob(billingJob, consolChargeCodes.FRT, 0m, 1200m);
			frtJobCharge.JR_OSCostAmt = 0m;
			frtJobCharge.JR_AgentDeclaredCostAmt = 0;
			frtJobCharge.JR_IsIncludedInProfitShare = true;

			Factory.Save();

			var profitShareShipmentChargeProcessor = new ProfitShareShipmentChargeProcessor(Factory, shipment, billingJob);
			profitShareShipmentChargeProcessor.Process();

			var expectedProfitShareCost1 = createProfitShareAsAR ? 0m : 600m;
			var expectedProfitShareRevenue1 = createProfitShareAsAR ? -600m : 0m;
			var expectedCharges = new List<AssertionCharge>
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 0m,
					JR_OSSellAmt = 1200m
				},
				new AssertionCharge
				{
					ChargeCode = "PS",
					JR_OSCostAmt = expectedProfitShareCost1,
					JR_OSSellAmt = expectedProfitShareRevenue1,
					JR_Desc = "Profit Share / Rebate - Receiving Agent - 50.00% of profit of 1200.00"
				}
			};
			AssertCharges("There should be a new created PS charge", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #1 - after manually creating new profit share charge", billingJob, expectedProfitShareCost1, 1200m + expectedProfitShareRevenue1, 600m);

			frtJobCharge.JR_OSSellAmt = 1000m;
			frtJobCharge.JR_OSCostAmt = 0m;
			Factory.Save();

			var expectedProfitShareCost2 = createProfitShareAsAR ? 0m : -100m;
			var expectedProfitShareRevenue2 = createProfitShareAsAR ? 100m : 0m;
			ZDecimal expectedPreviousProfit = createProfitShareAsAR ? expectedProfitShareRevenue1 : expectedProfitShareCost1;

			expectedCharges[0].JR_OSSellAmt = 1000;
			expectedCharges[0].JR_OSCostAmt = 0;
			expectedCharges.Add(new AssertionCharge
			{
				ChargeCode = "INP",
				JR_OSCostAmt = expectedProfitShareCost2,
				JR_OSSellAmt = expectedProfitShareRevenue2,
				JR_Desc = $"Internal Profit Adjustment - Adjustment: less {expectedPreviousProfit.ToString(2)} - Receiving Agent - 50.00% of profit of 1000.00"
			});
			AssertCharges("New Profit Share Adjustment charge (INP) #1 is added automatically when saving.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #2 - after modifying FRT charge to 1000", billingJob, expectedProfitShareCost1 + expectedProfitShareCost2, 1000m + expectedProfitShareRevenue1 + expectedProfitShareRevenue2, 500m);

			frtJobCharge.JR_OSSellAmt = 0m;
			frtJobCharge.JR_OSCostAmt = 0m;
			Factory.Save();

			var expectedProfitShareCost3 = createProfitShareAsAR ? 0m : -500m;
			var expectedProfitShareRevenue3 = createProfitShareAsAR ? 500m : 0m;
			expectedPreviousProfit = createProfitShareAsAR
				? expectedProfitShareRevenue1 + expectedProfitShareRevenue2
				: expectedProfitShareCost1 + expectedProfitShareCost2;
			expectedCharges[0].JR_OSCostAmt = 0;
			expectedCharges[0].JR_OSSellAmt = 0;
			expectedCharges.Add(new AssertionCharge
			{
				ChargeCode = "INP",
				JR_OSCostAmt = expectedProfitShareCost3,
				JR_OSSellAmt = expectedProfitShareRevenue3,
				JR_Desc = $"Internal Profit Adjustment - Adjustment: less {expectedPreviousProfit.ToString(2)} - Receiving Agent - 50.00% of profit of 0.00"
			});

			AssertCharges("New Profit Share Adjustment charge (INP) #2 is added automatically when saving.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #3 - after modifying FRT charge to ZERO", billingJob, expectedProfitShareCost1 + expectedProfitShareCost2 + expectedProfitShareCost3,
				expectedProfitShareRevenue1 + expectedProfitShareRevenue2 + expectedProfitShareRevenue3, 0m);

			frtJobCharge.JR_OSSellAmt = 100m;
			frtJobCharge.JR_OSCostAmt = 100m;
			Factory.Save();

			expectedCharges[0].JR_OSCostAmt = 100;
			expectedCharges[0].JR_OSSellAmt = 100;
			AssertCharges("No new Profit Share Adjustment charge (INP) is added when saving because of no difference in Cost/Revenue.", expectedCharges, billingJob);
			AssertJobTotalValues("Total values on Billing Job #4", billingJob, 100, 100, 0m);
		}

		#endregion

		#region Estimated Cost and Revenue

		#region Test method and test case struct

		struct UpdateEstimatedValuesTestCase
		{
			public ZDecimal ChargeCodeMarginPercentage { get; set; }
			public bool ShouldAutorateCost { get; set; }
			public bool ShouldAutorateRevenue { get; set; }

			// Price for a flat charge in costing
			public ZDecimal? SetupCostRate { get; set; }
			// Price for a flat charge in client rate
			public ZDecimal? SetupSellRate { get; set; }

			public struct DataForStep
			{
				public ZDecimal? ManuallyEnteredCostAmount { get; set; }
				public ZDecimal? ManuallyEnteredSellAmount { get; set; }

				public ZDecimal? ManuallyEnteredEstimatedCost { get; set; }
				public ZDecimal? ManuallyEnteredEstimatedRevenue { get; set; }

				public ZDecimal? ExpectedAutoRatedCostAmount { get; set; }
				public ZDecimal? ExpectedAutoRatedSellAmount { get; set; }

				public string ReasonForExpectedEstimatedCost { get; set; }
				public ZDecimal ExpectedEstimatedCost { get; set; }

				public string ReasonForExpectedEstimatedRevenue { get; set; }
				public ZDecimal ExpectedEstimatedRevenue { get; set; }
			}

			/// <summary>
			/// Manually enter a new charge or autorate to create the charge.
			/// If ManuallyEnteredCostAmount and ManuallyEnteredSellAmount present then manually add charge to job.
			/// Otherwise, ExpectedAutoRatedCostAmount and ExpectedAutoRatedSellAmount are expected from autorating.
			/// Expect to see results in ExpectedEstimatedCost and ExpectedEstimatedRevenue
			/// Note: A Save occurs after this step.
			/// </summary>
			public DataForStep FirstStep { get; set; }

			/// <summary>
			/// Manually update the charge amounts with ManuallyEnteredCostAmount & ManuallyEnteredSellAmount
			/// Expect to see results in ExpectedEstimatedCost and ExpectedEstimatedRevenue
			/// Note: A Save occurs after this step.
			/// </summary>
			public DataForStep SecondStep { get; set; }

			/// <summary>
			/// Manually update the charge amounts with ManuallyEnteredEstimatedCost and ManuallyEnteredEstimatedRevenue
			/// Autorate again. Expect to see results in
			/// - ExpectedAutoRatedCostAmount and ExpectedAutoRatedSellAmount
			/// - ExpectedEstimatedCost and ExpectedEstimatedRevenue
			/// Note: A Save occurs after this step.
			/// </summary>
			public DataForStep FinalStep { get; set; }
		}

		void TestUpdateEstimatedValues(UpdateEstimatedValuesTestCase testCase)
		{
			var chargeCode = Helper.ChargeCodes["FRT"];
			chargeCode.AC_MarginPercentage = testCase.ChargeCodeMarginPercentage;

			if (testCase.ShouldAutorateCost)
			{
				var costing = Helper.NewCosting(TransportProvider1);
				costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", testCase.SetupCostRate.Value, CurrencyCodes.Australia);
			}

			if (testCase.ShouldAutorateRevenue)
			{
				var clientRate = Helper.NewClientRate(NewClient);
				clientRate.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "AUSYD", "USLAX", "FRT", testCase.SetupSellRate.Value, CurrencyCodes.Australia);
			}

			// Save NewClient otherwise charge will have error "Enter a valid Debtor" and cannot be saved.
			var consignor = NewClient;

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, consignor.PK, Consignee.PK, "AUSYD", "USLAX", 1000, 1);
			CreateForwardingConsol(TransportModes.Air, "AUSYD", "USLAX", TransportProvider1, shipment);
			var job = CreateNewBillingJob(shipment);

			// ********** First step: create charge **********
			if (testCase.FirstStep.ManuallyEnteredCostAmount == null || testCase.FirstStep.ManuallyEnteredSellAmount == null)
			{
				var firstStepExpectedCharge = new[]
				{
					new AssertionCharge
					{
						JR_LocalCostAmt = testCase.FirstStep.ExpectedAutoRatedCostAmount.Value,
						JR_LocalSellAmt = testCase.FirstStep.ExpectedAutoRatedSellAmount.Value
					}
				};
				AutorateAndAssert("Autorate to create charge", firstStepExpectedCharge, shipment, NewClient,
					job: job, autorateRevenue: testCase.ShouldAutorateRevenue, autorateCosts: testCase.ShouldAutorateCost);
			}
			else
			{
				var manuallyEnteredCharge = job.Charges.AddNew();
				manuallyEnteredCharge.JR_AC = chargeCode.PK;
				manuallyEnteredCharge.JR_LocalCostAmt = testCase.FirstStep.ManuallyEnteredCostAmount.Value;
				manuallyEnteredCharge.JR_LocalSellAmt = testCase.FirstStep.ManuallyEnteredSellAmount.Value;
			}
			Factory.Save();

			var charge = job.Charges[0];
			AssertEquals(testCase.FirstStep.ReasonForExpectedEstimatedCost ?? string.Empty, testCase.FirstStep.ExpectedEstimatedCost, charge.JR_EstimatedCost);
			AssertEquals(testCase.FirstStep.ReasonForExpectedEstimatedRevenue ?? string.Empty, testCase.FirstStep.ExpectedEstimatedRevenue, charge.JR_EstimatedRevenue);

			// ********** Second step: manually update cost/sell amounts **********
			// Note: if one amount is 0 and the other is not, setting the other amount later will also automatically calculate new amount (by margin) for the former.
			// To avoid that, always set the one with non-zero value first, then override the calculated value after.
			var manualCostAmount = testCase.SecondStep.ManuallyEnteredCostAmount.Value;
			if (manualCostAmount != ZDecimal.Zero)
			{
				charge.JR_LocalCostAmt = manualCostAmount;
				charge.JR_LocalSellAmt = testCase.SecondStep.ManuallyEnteredSellAmount.Value;
			}
			else
			{
				charge.JR_LocalSellAmt = testCase.SecondStep.ManuallyEnteredSellAmount.Value;
				charge.JR_LocalCostAmt = manualCostAmount;
			}

			AssertEquals(testCase.SecondStep.ReasonForExpectedEstimatedRevenue ?? string.Empty, testCase.SecondStep.ExpectedEstimatedRevenue, charge.JR_EstimatedRevenue);
			AssertEquals(testCase.SecondStep.ReasonForExpectedEstimatedCost ?? string.Empty, testCase.SecondStep.ExpectedEstimatedCost, charge.JR_EstimatedCost);

			// ********** Final step: update estimated values and re-autorate **********
			charge.JR_EstimatedCost = testCase.FinalStep.ManuallyEnteredEstimatedCost.Value;
			charge.JR_Calc_CostRatingBehavior = RatingBehaviours.ReAutorateCharge;
			charge.JR_EstimatedRevenue = testCase.FinalStep.ManuallyEnteredEstimatedRevenue.Value;
			charge.JR_Calc_SellRatingBehavior = RatingBehaviours.ReAutorateCharge;

			Factory.Save();

			var expectedCharges = new[]
			{
				new AssertionCharge
				{
					JR_LocalCostAmt = testCase.FinalStep.ExpectedAutoRatedCostAmount.Value,
					JR_LocalSellAmt = testCase.FinalStep.ExpectedAutoRatedSellAmount.Value
				}
			};
			AutorateAndAssert("Autorate when charge exists", expectedCharges, shipment, NewClient,
				job: job, autorateRevenue: testCase.ShouldAutorateRevenue, autorateCosts: testCase.ShouldAutorateCost);
			charge = job.Charges[0];
			AssertEquals(testCase.FinalStep.ReasonForExpectedEstimatedCost, testCase.FinalStep.ExpectedEstimatedCost, charge.JR_EstimatedCost);
			AssertEquals(testCase.FinalStep.ReasonForExpectedEstimatedRevenue, testCase.FinalStep.ExpectedEstimatedRevenue, charge.JR_EstimatedRevenue);
		}

		#endregion Test method and test case struct

		#region Autorated charge test cases

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 0,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 200,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_ZeroMarginCharge_ZeroCostAndRevenue_ZeroEstimatedCost_NoneZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 0,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost with manual zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should not update when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 200,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_ZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostSameCost_NonZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 0,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be RevenueAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 100,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes with autorating cost",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 250,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_ZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostSameCost_NonZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 0,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue with manual zero RevenueAmount",
					ExpectedEstimatedRevenue = 0,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 100,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes with autorating cost",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should not update when SellAmount changes with autorating cost",
					ExpectedEstimatedRevenue = 200,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_ZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostDifferentCost_NonZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 0,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to be SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes with autorating cost.",
					ExpectedEstimatedCost = 150,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 250
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_ZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostDifferentCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 0,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost with manual zero SellAmount",
					ExpectedEstimatedRevenue = 0
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes with autorating cost.",
					ExpectedEstimatedCost = 150,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 250
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625, // <= $500 / 0.8 margin

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 625
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 200,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost",
					ExpectedEstimatedRevenue = 0
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_NonZeroMarginCharge_ZeroRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625, // <= $500 / 0.8 margin

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 625
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625, // <= should recalculate SellAmount based on CostAmount

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating cost",
					ExpectedEstimatedRevenue = 625
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_NonZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostSameCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625, // <= $500 / 0.8 margin

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 625
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 100,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes with autorating cost",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero sell amount",
					ExpectedEstimatedRevenue = 0
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_NonZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostSameCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625, // <= $500 / 0.8 margin

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 625
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 100,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes with autorating cost",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 625
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_NonZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostDifferentCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625, // <= $500 / 0.8 margin

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 625
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 300,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 200,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 300,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when autorating cost",
					ExpectedEstimatedCost = 200,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCost_NonZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostDifferentCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625, // <= $500 / 0.8 margin

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 625
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 200,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when autorating cost if it has been autorated before",
					ExpectedEstimatedCost = 200,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 625
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 0, // <= $625 * 0 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when it changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 100,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_ZeroMarginCharge_ZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 0, // <= $625 * 0 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost with manual zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 0,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost with zero CostAmount when autorating revenue",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 0, // <= $625 * 0 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 100,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue with non-zero CostAmount.",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating revenue.",
					ExpectedEstimatedRevenue = 200,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_ZeroMarginCharge_ZeroCost_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 0, // <= $625 * 0 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost with manual zero CostAmount.",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 0,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost with zero CostAmount when autorating revenue",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating revenue",
					ExpectedEstimatedRevenue = 200,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 0, // <= $625 * 0 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 100,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not change when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update autorating revenue",
					ExpectedEstimatedRevenue = 250,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_ZeroMarginCharge_ZeroCost_NonZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 0, // <= $625 * 0 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost with manual zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 100,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 0,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating revenue",
					ExpectedEstimatedRevenue = 250,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500, // <= $625 * 0.8 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 100,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_NonZeroMarginCharge_ZeroCost_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500, // <= $625 * 0.8 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be new CostAmount when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500, // <= $625 * 0.8 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount change manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 100,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost with zero CostAmount when autorating revenue",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating revenue",
					ExpectedEstimatedRevenue = 200,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_NonZeroMarginCharge_ZeroCost_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500, // <= $625 * 0.8 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount change manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 500, // <= should recalculate CostAmount based on SellAmount
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be new CostAmount when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating revenue",
					ExpectedEstimatedRevenue = 200,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500, // <= $625 * 0.8 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount change manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 100,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost with zero CostAmount when autorating revenue",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating revenue",
					ExpectedEstimatedRevenue = 250,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoRevenue_NonZeroMarginCharge_ZeroCost_ZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 500, // <= $625 * 0.8 margin
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount change manually",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 500, // <= should recalculate CostAmount based on SellAmount
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be new CostAmount when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating revenue",
					ExpectedEstimatedRevenue = 250,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCostAndRevenue_ZeroMarginCharge_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,  // <= $0 but then overridden by sell rate

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCostAndRevenue_ZeroMarginCharge_NonZeroEstimatedCostSameCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,  // <= $0 but then overridden by sell rate

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 100,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCostAndRevenue_ZeroMarginCharge_NonZeroEstimatedCostDifferentCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,  // <= $0 but then overridden by sell rate

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCostAndRevenue_NonZeroMarginCharge_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625, // <= $500 with cost rate but then overridden by sell rate

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCostAndRevenue_NonZeroMarginCharge_NonZeroEstimatedCostSameCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625, // <= $500 with cost rate but then overridden by sell rate

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 100,
					ManuallyEnteredEstimatedRevenue = 200,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeFromAutorate_WhenAutoCostAndRevenue_NonZeroMarginCharge_NonZeroEstimatedCostDifferentCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625, // <= $500 with cost rate but then overridden by sell rate

					ReasonForExpectedEstimatedCost = "EstimatedCost for new charge should be the same as CostAmount",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue for new charge should be the same as SellAmount",
					ExpectedEstimatedRevenue = 625,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "Non-zero EstimatedCost should not update when CostAmount change manually.",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "Non-zero EstimatedRevenue should not update when SellAmount change manually.",
					ExpectedEstimatedRevenue = 625,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		#endregion Autorated charge test cases

		#region Manual charge test cases

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 250,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_ZeroMarginCharge_ZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when SellAmount changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0, // <= should recalculate SellAmount based on CostAmount

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 250,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_ZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostSameCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_ZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostSameCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_ZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostDifferentCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 170,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_ZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostDifferentCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 170,
					ManuallyEnteredEstimatedRevenue = 270,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should not update when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 270,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 250,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500, // <= expected to be $500 from CostAmount
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_NonZeroMarginCharge_ZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_NonZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostSameCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_NonZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostSameCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be new SellAmount when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_NonZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostDifferentCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 170,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost and it has not been autorated before",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should not update when autorating cost with non-zero SellAmount",
					ExpectedEstimatedRevenue = 0
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCost_NonZeroMarginCharge_ZeroRevenue_NonZeroEstimatedCostDifferentCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 500,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 170,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should update to new CostAmount when autorating cost",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be new SellAmount when autorating cost with zero SellAmount",
					ExpectedEstimatedRevenue = 625
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 150,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_ZeroMarginCharge_ZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 0,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 150,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to new SellAmount when autorating revenue with non-zero SellAmount",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_ZeroMarginCharge_ZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 0,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 150,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to new SellAmount when autorating revenue with non-zero SellAmount",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_ZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 270,

					ExpectedAutoRatedCostAmount = 150,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_ZeroMarginCharge_ZeroCost_ZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 170,
					ManuallyEnteredEstimatedRevenue = 270,

					ExpectedAutoRatedCostAmount = 0,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 170,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 150,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue.",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_NonZeroMarginCharge_ZeroCostAndRevenue_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 0,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be new CostAmount when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue.",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 150,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue.",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_NonZeroMarginCharge_ZeroCost_ZeroEstimatedCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be new CostAmount when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue.",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_NonZeroMarginCharge_NonZeroCostAndRevenue_ZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 270,

					ExpectedAutoRatedCostAmount = 150,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should not update when autorating revenue with non-zero CostAmount",
					ExpectedEstimatedCost = 0,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoRevenue_NonZeroMarginCharge_ZeroCost_ZeroEstimatedCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 0,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 270,

					ExpectedAutoRatedCostAmount = 500,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "zero EstimatedCost should update to be new CostAmount when autorating revenue with zero CostAmount",
					ExpectedEstimatedCost = 500,
					ReasonForExpectedEstimatedRevenue = "non-zero EstimatedRevenue should update to be SellAmount when autorating revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCostAndRevenue_ZeroMarginCharge_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost should update to be new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should update to be new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCostAndRevenue_ZeroMarginCharge_NonZeroEstimatedCostSameCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost should update to be new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should update to be new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCostAndRevenue_ZeroMarginCharge_NonZeroEstimatedCostDifferentCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 0,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 170,
					ManuallyEnteredEstimatedRevenue = 270,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost should update to be new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should update to be new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCostAndRevenue_NonZeroMarginCharge_ZeroEstimatedCost_ZeroEstimatedRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 0,
					ManuallyEnteredEstimatedRevenue = 0,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost should update to be new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should update to be new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCostAndRevenue_NonZeroMarginCharge_NonZeroEstimatedCostSameCost_NonZeroEstimatedRevenueSameRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 150,
					ManuallyEnteredEstimatedRevenue = 250,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost should update to be new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should update to be new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		public void TestUpdateEstimatedValues_CreateChargeManually_WhenAutoCostAndRevenue_NonZeroMarginCharge_NonZeroCostAndRevenue_NonZeroEstimatedCostDifferentCost_NonZeroEstimatedRevenueDifferentRevenue()
		{
			TestUpdateEstimatedValues(new UpdateEstimatedValuesTestCase
			{
				ShouldAutorateCost = true,
				ShouldAutorateRevenue = true,
				ChargeCodeMarginPercentage = 80,
				SetupCostRate = 400,
				SetupSellRate = 625,
				FirstStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 100,
					ManuallyEnteredSellAmount = 200,

					ReasonForExpectedEstimatedCost = "EstimatedCost should be CostAmount when creating new charge",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should be SellAmount when creating new charge",
					ExpectedEstimatedRevenue = 200,
				},
				SecondStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredCostAmount = 150,
					ManuallyEnteredSellAmount = 250,

					ReasonForExpectedEstimatedCost = "non-zero EstimatedCost should not update when CostAmount changes manually",
					ExpectedEstimatedCost = 100,
					ReasonForExpectedEstimatedRevenue = "zero EstimatedCost should update to SellAmount when it changes manually",
					ExpectedEstimatedRevenue = 200,
				},
				FinalStep = new UpdateEstimatedValuesTestCase.DataForStep
				{
					ManuallyEnteredEstimatedCost = 170,
					ManuallyEnteredEstimatedRevenue = 270,

					ExpectedAutoRatedCostAmount = 400,
					ExpectedAutoRatedSellAmount = 625,

					ReasonForExpectedEstimatedCost = "EstimatedCost should update to be new CostAmount when autorating cost & revenue",
					ExpectedEstimatedCost = 400,
					ReasonForExpectedEstimatedRevenue = "EstimatedRevenue should update to be new SellAmount when autorating cost & revenue",
					ExpectedEstimatedRevenue = 625,
				}
			});
		}

		#endregion Manual charge test cases

		#endregion Estimated Cost and Revenue

		#region Implementations and helpers

		AccChargeCode LoadConsolChargeCode(string code)
		{
			var query = new ZQuery(AccChargeCodeSchema.AC_Code, code);
			query.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			var chargeCode = Factory.LoadTop1<AccChargeCode>(query);
			chargeCode.AC_IsGroupageCharge = true;

			return chargeCode;
		}

		(AccChargeCode FRT, AccChargeCode FSC) LoadChargeCodesForTesting()
		{
			return
			(
				LoadConsolChargeCode("FRT"),
				LoadConsolChargeCode("FSC")
			);
		}

		AccChargeCode CreateInternalProfitSplitChargeCode()
		{
			var inpCharge = Helper.ChargeCodes.NewConsolChargeCode("INP", "Internal Profit Adjustment", PercentageCalculator.Code);
			AccountingConfigurationRegistry.Instance.ProfitShareAdjustmentChargeCode.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(),
				Guid.Empty, Guid.Empty, inpCharge.PK.ToGuid());

			return inpCharge;
		}

		Tuple<OrgHeader, OrgHeader> SetupNewSendingReceivingPartiesWithProfitSplitAgreement()
		{
			var orgAProxy = Helper.NewOrgHeader();
			orgAProxy.OH_IsDebtor = true;
			orgAProxy.OH_IsCreditor = true;
			GlbCompany.CurrentCompany.GC_OH_OrgProxy = orgAProxy.PK;

			var orgBAgentWithProfitShareAgreements = Helper.NewOrgHeader();
			orgBAgentWithProfitShareAgreements.OH_IsForwarder = true;
			orgBAgentWithProfitShareAgreements.OH_RL_NKClosestPort = "USLAX";
			orgBAgentWithProfitShareAgreements.OH_IsCreditor = true;
			orgBAgentWithProfitShareAgreements.OH_IsDebtor = true;
			var orgBAddress = orgBAgentWithProfitShareAgreements.MainAddress;
			orgBAddress.OA_City = "Los Angeles";
			orgBAddress.OA_State = "CA";

			var agentRelationship = orgBAgentWithProfitShareAgreements.AgentRelationships.AddNew();
			agentRelationship.O3_OH_SendingAgent = orgAProxy.PK;
			agentRelationship.O3_OH_ReceivingAgent = orgBAgentWithProfitShareAgreements.PK;

			var profitShareDetailCollection = agentRelationship.GenericProfitShareDetails;
			var profitShareAgreement = profitShareDetailCollection.Count == 1
				? profitShareDetailCollection[0]
				: profitShareDetailCollection.AddNew();
			profitShareAgreement.O4_AgreementType = OrgProfitShareDetailsLookups.AgreementTypeCustom;
			profitShareAgreement.O4_StartDate = ZDateTime.Now.AddMonths(-1);
			profitShareAgreement.O4_FreightMode = "ALL";
			profitShareAgreement.O4_SendingPortOrCountry = "AU";
			profitShareAgreement.O4_ReceivingPortOrCountry = "US";

			OrgProfitShareParty sendingParty = profitShareAgreement.PartyDetails.AddNew();
			sendingParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.SendingAgent;
			sendingParty.PS_PartyProfitSharePercent = 50;

			OrgProfitShareParty receivingParty = profitShareAgreement.PartyDetails.AddNew();
			receivingParty.PS_PartyType = OrgProfitSharePartyLookups.PartyTypeCodes.ReceivingAgent;
			receivingParty.PS_PartyProfitSharePercent = 50;

			return new Tuple<OrgHeader, OrgHeader>(orgAProxy, orgBAgentWithProfitShareAgreements);
		}

		void CreateNewConsolWithSenderReceiverInParties(ForwardingShipment shipment, Tuple<OrgHeader, OrgHeader> parties)
		{
			var consol = shipment.Consols.AddNew();
			consol.SetDefaultSendingForwarderAddress(parties.Item1);
			consol.SetDefaultReceivingForwarderAddress(parties.Item2);
		}

		Job CreateNewBillingJob(ForwardingShipment shipment)
		{
			var job = new Job.Loader(Factory, shipment).TryCreateWithoutMutexForTestOnly();
			job.JH_GB = GlbBranch.CurrentBranch.PK;
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FEA")).PK;

			return job;
		}

		Charge AddChargeToBillingJob(Job billingJob, AccChargeCode chargeCode, ZDecimal costAmount, ZDecimal sellAmount)
		{
			var charge = billingJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_OSCostAmt = costAmount;
			charge.JR_OSSellAmt = sellAmount;

			return charge;
		}

		void AssertJobTotalValues(string message, Job billingJob, ZDecimal expectedCostAmount, ZDecimal expectedSellAmount, ZDecimal expectedProfitLossAmount)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Job Total Cost should be", expectedCostAmount, billingJob.JH_TotalCost);
				AssertEquals("Job Total Revenue should be", expectedSellAmount, billingJob.JH_TotalRevenue);
				AssertEquals("Job Total Profit/Loss should be", expectedProfitLossAmount, billingJob.JH_ProfitLoss);
			});
		}

		#endregion
	}
}
