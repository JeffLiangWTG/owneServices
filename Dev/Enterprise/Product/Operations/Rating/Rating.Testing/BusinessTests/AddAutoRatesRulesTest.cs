using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class AddAutoRatesRulesTest : BaseRatingIntegrationTest
	{
		#region Rule 2: In case when rating cost and sell we attempt to clear both cost and sell sides (Rating Behavior is set to REA for both Cost and Sell).

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_ClearsAndRecreatesCharges()
		{
			var chargeCode = CreateTSTFRTCharge();
			chargeCode.AC_MarginPercentage = 90m;

			var rate = Helper.NewClientRate(NewClient);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 100;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignee.PK, NewClient.PK, "AUSYD", "USLAX", 50);
			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 7.1105m);

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 100m,
							JR_OSCostAmt = 90m,
							JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates
						}
				};

			AutorateAndAssert(expected, shipment, NewClient, job: shipmentJob);

			line.GetCalculator<FlatCalculator>().BaseRate = 200;
			Factory.Save();

			expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 200m,
							JR_OSCostAmt = 180m,
							JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates
						}
				};

			AutorateAndAssert(expected, shipment, NewClient, job: shipmentJob);
		}

		#endregion

		#region Rule 5:	Clear means recalculate margin from opposite amount

		public void TestAddAutoRates_GetCostingFromMarginChargeCode()
		{
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;

			var chargeCode = CreateTSTFRTCharge();
			chargeCode.AC_MarginPercentage = 90m;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(entry.Currency);
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 200.00m,
							JR_OSCostAmt = 180.00m,
							JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		public void TestAddAutoRates_GetRevenueForMarginChargeCode()
		{
			var chargeCode = CreateTSTFRTCharge();
			chargeCode.AC_MarginPercentage = 50m;

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 10;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 100;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 2000.00m,
							JR_OSCostAmt = 1000.00m,
							JR_RX_NKCostCurrency = GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency
						},
				};

			AutorateAndAssert(expected, shipment, client);
		}

		#endregion

		#region Rule 9: A new revenue charge is not added in case the same code charge exists on a job with revenue posted and for the same debtor / job reference if Sell Rating Behavoir is set to STP

		[TestDate(2016, 08, 29)]
		[DisableZeroExchangeRateOverriding]
		public void TestAddAutoRates_RevenueIsPosted_STP()
		{
			var chargeCode = CreateTSTFRTCharge();

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							JR_OSSellAmt = 200.00m,
							JR_OSCostAmt = 200.00m
						},
				};

			AutorateAndAssert(expected, shipment, client, expectedErrors: new[] { $@"Error {shipment.JS_UniqueConsignRef} has encountered the following errors while AutoRating:
	•  Local Cost Amount: Local amount cannot be zero when Overseas Cost Amount is non zero." });

			line.GetCalculator<FlatCalculator>().BaseRate = 190;

			var job = shipment.Job as Job;
			AssertEquals(2, job.ExchangeRates.Count);
			var debExRate = job.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(exRate => exRate.OrgType == ExchangeRateOrgTypeEnum.Debtor);
			var crdExRate = job.ExchangeRates.Cast<ExchangeRate>().FirstOrDefault(exRate => exRate.OrgType == ExchangeRateOrgTypeEnum.Creditor);
			AssertNotNull(debExRate);
			AssertNotNull(crdExRate);
			debExRate.JF_BaseRate = 0.98m; //debtor
			crdExRate.JF_BaseRate = 1.02m; //1.02m; //creditor
			AssertEquals(1, job.Charges.Count);

			var charge = job.Charges[0];

			var localCostAmt = charge.JR_LocalCostAmt;
			var localSellAmt = charge.JR_LocalSellAmt;

			charge.JR_LocalCostAmt = 0;
			charge.JR_LocalSellAmt = 0;

			Factory.Save();

			charge.JR_LocalCostAmt = localCostAmt;
			charge.JR_LocalSellAmt = localSellAmt;

			var tr = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);
			var th = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tl = Factory.NewWithValidTestData<AccTransactionLines>();

			tl.AL_AH = th.PK;
			tl.AL_LineType = TransactionLineTypes.Revenue;
			tl.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			tl.AL_RevRecognitionType = "IMM";

			charge.JR_AL_ARLine = tl.PK;
			charge.JR_AT_SellGSTRate = tr.PK;
			charge.JR_SellRatingOverride = false;

			Assert("Revenue is now posted", charge.IsRevenuePosted);

			AutorateAndAssert("There should be still one charge and unchanged", expected, shipment, client, null, job, autorateCosts: false);

			var expectedLogLine = @"Information: Skipping Rate 'TSTFRT: Base Rate USD 190.00' as an apportioned or posted charge with the same code already exists on the Job with 'Rating Behavior' set to STP.";

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain information about skipped rate", expectedLogLine);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_ManuallyAddedRevenueFromDifferentJobIsPosted_STP()
		{
			var chargeCode = CreateTSTFRTCharge();

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 1m);
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates;
			charge.JR_OSSellAmt = 100m;
			charge.JR_OSCostAmt = 100m;
			charge.JR_SellRatingOverride = false;
			charge.JR_SellReference = "Different Job";
			shipmentJob.LocalChargesPK = client.PK;

			var tr = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);

			var th = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tl = Factory.NewWithValidTestData<AccTransactionLines>();
			tl.AL_AH = th.PK;
			tl.AL_LineType = TransactionLineTypes.Revenue;
			tl.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			tl.AL_RevRecognitionType = "IMM";

			charge.JR_AL_ARLine = tl.PK;
			charge.JR_AT_SellGSTRate = tr.PK;

			Assert(charge.IsRevenuePosted);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200.00m,
					JR_OSCostAmt = 200.00m,
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 100.00m,
					JR_OSCostAmt = 100.00m,
				}
			};

			AutorateAndAssert("There should be two charges", expected, shipment, client, null, shipmentJob, autorateCosts: false);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_RevenuePosted_STP_WhenSellReferenceNotChanged_ShouldNotBringRevenueAgain()
		{
			var chargeCode = CreateTSTFRTCharge();

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 1m);
			shipmentJob.LocalChargesPK = client.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200.00m,
					JR_OSCostAmt = 200.00m,
				}
			};

			AutorateAndAssert("Should find one charge", expected, shipment, client, null, shipmentJob, autorateCosts: false);

			var charge1AfterRating = shipmentJob.Charges[0];
			PostRevenue(charge1AfterRating);

			AssertEquals("Correct Revenue", 200m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 200m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsRevenuePosted);

			AutorateAndAssert("There should be still one charge and unchanged", expected, shipment, client, null, shipmentJob, autorateRevenue: false);

			AssertEquals("Correct Revenue", 200m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 200m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsRevenuePosted);
		}

		public void TestAddAutoRates_RevenuePosted_STP_WhenSellReferenceChanged_ShouldBringRevenue()
		{
			var chargeCode = CreateTSTFRTCharge();

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 1m);
			shipmentJob.LocalChargesPK = client.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200.00m,
					JR_OSCostAmt = 200.00m,
				}
			};

			AutorateAndAssert("Should find one charge", expected, shipment, client, null, shipmentJob, autorateCosts: false);

			var charge1AfterRating = shipmentJob.Charges[0];
			charge1AfterRating.JR_SellReference = "Something new";

			PostRevenue(charge1AfterRating);

			AssertEquals("Correct Revenue", 200m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 200m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsRevenuePosted);

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200.00m,
					JR_OSCostAmt = 200.00m,
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 200.00m,
					JR_OSCostAmt = 200.00m,
				}
			};

			AutorateAndAssert("There should be two charges", expected, shipment, client, null, shipmentJob, autorateCosts: false);

			AssertEquals("Correct Revenue", 200m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 200m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed (Posted)", true, charge1AfterRating.JR_IsRevenuePosted);

			var charge2AfterRating = shipmentJob.Charges[1];

			AssertEquals("Correct Revenue", 200m, charge2AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 200m, charge2AfterRating.JR_LocalCostAmt);
			AssertEquals("Not Posted", false, charge2AfterRating.JR_IsCostPosted);
			AssertEquals("Not Posted", false, charge2AfterRating.JR_IsRevenuePosted);
		}

		#endregion

		#region Rule 10: A new cost charge is not added in case the same code charge exists on a job with cost posted/apportioned and for the same creditor / job reference and Cost Rating Behavior is set to STP

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_ManuallyAddedCostIsPosted_STP()
		{
			var chargeCode = CreateTSTFRTCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 8;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 7.1105m);
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates;
			charge.JR_LocalCostAmt = 592.93m;

			charge.JR_OSCostAmt = 4216m;
			charge.JR_CostRatingOverride = false;
			charge.JR_OH_CostAccount = carrier.PK;

			var tr = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);

			var th = Factory.NewWithValidTestData<AccTransactionHeader>();
			var tl = Factory.NewWithValidTestData<AccTransactionLines>();
			tl.AL_AH = th.PK;
			tl.AL_LineType = TransactionLineTypes.Cost;
			tl.AL_LineAmount = -200;
			tl.AL_OSAmount = -200;
			tl.AL_RX_NKTransactionCurrency = CurrencyCodes.Australia;
			tl.AL_RevRecognitionType = "IMM";

			charge.JR_AL_APLine = tl.PK;
			charge.JR_AT_SellGSTRate = tr.PK;

			Assert(charge.IsCostPosted);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 592.93m,
					JR_OSCostAmt = 4216m
				},
			};

			AutorateAndAssert("There should be still one charge and unchanged", expected, shipment, client, null, shipmentJob, autorateRevenue: false);

			var expectedLogLine = @"Information: Skipping Rate 'TSTFRT: 1500 Kilogram(s) @ AUD 8.00/KG' as an apportioned or posted charge with the same code already exists on the Job with 'Rating Behavior' set to STP.";

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Should indicate that posted line was skipped in Log", expectedLogLine);

			var chargeAfterRating = shipmentJob.Charges[0];

			AssertEquals("Same Charge", charge.PK, chargeAfterRating.PK);
			AssertEquals("Correct Revenue", 592.93m, chargeAfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 4216m, chargeAfterRating.JR_LocalCostAmt);
			AssertEquals("Creditor same", carrier.PK, chargeAfterRating.JR_OH_CostAccount);
			AssertEquals("Posting Not Changed", true, chargeAfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", false, chargeAfterRating.JR_IsRevenuePosted);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_CostIsApportioned_STP()
		{
			var chargeCodeFRT = CreateTSTFRTCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCodeFRT, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();

			var fRTConsolCost = Factory.New<JobConsolCost>();

			var fRTCharge = GetCharge(shipmentJob, chargeCodeFRT, false, 11, false, 22);
			fRTCharge.JR_E6 = fRTConsolCost.PK;
			var tr = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);
			fRTCharge.JR_AT_SellGSTRate = tr.PK;
			fRTCharge.JR_CostRatingOverride = false;

			AssertEquals("JR_IsApportioned", true, fRTCharge.JR_IsApportioned);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 22m,
					JR_OSCostAmt = 11m,
					JR_CostRatingOverride = false,
					JR_CostRated = false,
				},
			};

			AutorateAndAssert("The apportioned charge should not be affected", expected, shipment, client, null, shipmentJob, autorateRevenue: false);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_ManuallyAddedCostFromDifferentJobIsPosted_STP()
		{
			var chargeCode = CreateTSTFRTCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 4;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 1m);
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates;
			charge.JR_OSSellAmt = 200m;
			charge.JR_OSCostAmt = 200m;
			charge.JR_SellRatingOverride = false;
			charge.JR_CostRatingOverride = false;
			charge.JR_CostReference = "Different Job";
			shipmentJob.LocalChargesPK = client.PK;

			PostCost(charge);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 200m,
					JR_OSCostAmt = 200m
				},
				new AssertionCharge
				{
					JR_OSCostAmt = 6000m,
					JR_OSSellAmt = 6000m,
				}
			};

			AutorateAndAssert("There should be still one charge and unchanged", expected, shipment, client, null, shipmentJob, autorateRevenue: false);

			var charge1AfterRating = shipmentJob.Charges[0];

			AssertEquals("Same Charge", charge.PK, charge1AfterRating.PK);
			AssertEquals("Correct Revenue", 200m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 200m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsRevenuePosted);

			var charge2AfterRating = shipmentJob.Charges[1];
			AssertEquals("Cost not posted", false, charge2AfterRating.JR_IsCostPosted);
			AssertEquals("Sell not posted", false, charge2AfterRating.JR_IsRevenuePosted);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_CostPosted_STP_WhenCostReferenceNotChanged_ShouldNotBringCostAgain()
		{
			var chargeCode = CreateTSTFRTCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 4;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 1m);
			shipmentJob.LocalChargesPK = client.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 6000m,
					JR_OSSellAmt = 6000m,
				}
			};

			AutorateAndAssert("Should find one charge", expected, shipment, client, null, shipmentJob, autorateRevenue: false);

			var charge1AfterRating = shipmentJob.Charges[0];
			PostCost(charge1AfterRating);

			AssertEquals("Correct Revenue", 6000m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 6000m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsRevenuePosted);

			AutorateAndAssert("There should be still one charge and unchanged", expected, shipment, client, null, shipmentJob, autorateRevenue: false);

			AssertEquals("Correct Revenue", 6000m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 6000m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsRevenuePosted);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_CostPosted_STP_WhenCostReferenceChanged_ShouldBringCost()
		{
			var chargeCode = CreateTSTFRTCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 4;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 1m);
			shipmentJob.LocalChargesPK = client.PK;

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 6000m,
					JR_OSSellAmt = 6000m,
				}
			};

			AutorateAndAssert("Should find one charge", expected, shipment, client, null, shipmentJob, autorateRevenue: false);

			var charge1AfterRating = shipmentJob.Charges[0];
			charge1AfterRating.JR_CostReference = "Something new";

			PostCost(charge1AfterRating);

			AssertEquals("Correct Revenue", 6000m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 6000m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsRevenuePosted);

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 6000m,
					JR_OSSellAmt = 6000m,
				},
				new AssertionCharge
				{
					JR_OSCostAmt = 6000m,
					JR_OSSellAmt = 6000m,
				}
			};

			AutorateAndAssert("There should be two charges", expected, shipment, client, null, shipmentJob, autorateRevenue: false);

			AssertEquals("Correct Revenue", 6000m, charge1AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 6000m, charge1AfterRating.JR_LocalCostAmt);
			AssertEquals("Posting Not Changed", true, charge1AfterRating.JR_IsCostPosted);
			AssertEquals("Posting Not Changed", false, charge1AfterRating.JR_IsRevenuePosted);

			var charge2AfterRating = shipmentJob.Charges[1];

			AssertEquals("Correct Revenue", 6000m, charge2AfterRating.JR_LocalSellAmt);
			AssertEquals("Correct Cost", 6000m, charge2AfterRating.JR_LocalCostAmt);
			AssertEquals("Not Posted", false, charge2AfterRating.JR_IsCostPosted);
			AssertEquals("Not Posted", false, charge2AfterRating.JR_IsRevenuePosted);
		}

		#endregion

		#region Rule 11: A new revenue charge is added in case the revenue is posted/apportioned for the same debtor / job reference if Rating Behavior is set to NEW

		[TestDate(2016, 08, 29)]
		[DisableZeroExchangeRateOverriding]
		public void TestAddAutoRates_RevenueIsPosted_CreateNewCharge()
		{
			var chargeCode = CreateTSTFRTCharge();

			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;

			var rate = Helper.NewClientRate(client);
			var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AUSYD", "USLAX");
			entry.RateLines.RemoveAndDeleteAll();
			var line = entry.AddRateLine(chargeCode, FlatCalculator.Code);
			line.GetCalculator<FlatCalculator>().BaseRate = 200;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = "SEA";
			shipment.JS_PackingMode = "LCL";
			shipment.ConsigneePK = client.PK;
			shipment.JS_ActualWeight = 50m;
			shipment.JS_UnitOfWeight = "KG";

			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_INCO = "CIF";

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
					{
						JR_OSSellAmt = 200.00m,
						JR_OSCostAmt = 200.00m
					},
				};

			AutorateAndAssert(expected, shipment, client, expectedErrors: new[] { $@"Error {shipment.JS_UniqueConsignRef} has encountered the following errors while AutoRating:
	•  Local Cost Amount: Local amount cannot be zero when Overseas Cost Amount is non zero." });

			line.GetCalculator<FlatCalculator>().BaseRate = 190;

			var job = shipment.Job as Job;
			job.ExchangeRates[0].JF_BaseRate = 0.98m; //debtor
			job.ExchangeRates[1].JF_BaseRate = 1.02m; //1.02m; //creditor
			AssertEquals(1, job.Charges.Count);

			var charge = job.Charges[0];

			var localCostAmt = charge.JR_LocalCostAmt;
			var localSellAmt = charge.JR_LocalSellAmt;

			charge.JR_LocalCostAmt = 0;
			charge.JR_LocalSellAmt = 0;

			Factory.Save();

			charge.JR_LocalCostAmt = localCostAmt;
			charge.JR_LocalSellAmt = localSellAmt;

			PostRevenue(charge);

			charge.JR_SellRatingOverride = true;

			expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 200.00m,
					JR_OSCostAmt = 200.00m
				},
				new AssertionCharge
				{
					ChargeCode = chargeCode.AC_Code,
					JR_OSSellAmt = 190.00m,
					JR_OSCostAmt = 190.00m
				},
			};

			AutorateAndAssert("There should be two charges", expected, shipment, client, null, job, autorateCosts: false);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_AutoRatedRevenueIsPosted_CreateNewCharge()
		{
			var chargeCode = CreateTSTFRTCharge();

			var clientRate = Helper.NewClientRate(NewClient);
			var entry = clientRate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
			entry.RateLines.RemoveAndDeleteAll();
			var rateLine = entry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 4.5;

			var shipment = CreateForwardingShipment(TransportModes.Sea, NewClient.PK, Consignee.PK, "AUSYD", "USBOS", 80);

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 360m,
					JR_OSCostAmt = 360m,
					RevenueCalculationDescription = "TSTFRT: 80 Kilogram(s) @ AUD 4.50/KG"
				},
			};

			AutorateAndAssert("AutoRating should produce a single charge", expected, shipment, NewClient);

			shipment.JS_ActualWeight = 100m;

			var job = shipment.Job as Job;
			var charge = job.Charges[0];

			PostRevenue(charge);

			charge.JR_SellRatingOverride = true;

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 360m,
					JR_OSCostAmt = 360m,
					RevenueCalculationDescription = "TSTFRT: 80 Kilogram(s) @ AUD 4.50/KG"
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 450.00m,
					JR_OSCostAmt = 450.00m,
					RevenueCalculationDescription = "TSTFRT: 100 Kilogram(s) @ AUD 4.50/KG"
				},
			};

			AutorateAndAssert("There should be two charges", expected, shipment, NewClient, null, job, autorateCosts: false);

			var expectedLogLines = new[] { "Warning: Autorating has produced new charges for the same charge codes as pre-existing charges which resulted in following charges conflicting:",
			"  • TSTFRT",
			"Autorating deletes pre-existing charges unless one of the following conditions apply:",
" - the amount is Posted",
" - the amount is Apportioned",
" - the amount's 'Rating Behavior' is set to NEW",
" - the charge type of the charge code is 'Comment'.",
			"Pre-existing posted charge for the same debtor/creditor causes Autorating to discard matching newly autorated charges." };

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		[TestDate(2016, 08, 29)]
		public void TestAddAutoRates_CostIsApportioned_CreateNewCharge()
		{
			var chargeCodeFRT = CreateTSTFRTCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCodeFRT, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 5;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1000;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();

			var fRTConsolCost = Factory.New<JobConsolCost>();

			var fRTCharge = GetCharge(shipmentJob, chargeCodeFRT, false, 11, false, 22);
			fRTCharge.JR_E6 = fRTConsolCost.PK;
			var tr = AccTaxRate.LoadExistingOrCreateNewTaxRate(Factory, "TAX", "RAT", 0);
			fRTCharge.JR_AT_SellGSTRate = tr.PK;

			AssertEquals("JR_IsApportioned", true, fRTCharge.JR_IsApportioned);

			fRTCharge.JR_CostRatingOverride = true;

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 22m,
					JR_OSCostAmt = 11m,
					JR_CostRatingOverride = true,
					JR_CostRated = false,
				},
				new AssertionCharge
				{
					JR_OSCostAmt = 5000m,
					JR_CostRatingOverride = false,
				},
			};

			AutorateAndAssert("There should be two charges", expected, shipment, client, null, shipmentJob, autorateRevenue: false);
		}

		#endregion

		#region Rule 12: A new cost charge is added in case the same code charge exists on a job with cost posted and for the same creditor / job reference if Rating Behavior is set to NEW

		public void TestAddAutoRates_ManuallyAddedCostIsPosted_CreateNewCharge()
		{
			var chargeCode = CreateTSTFRTCharge();

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			var client = Helper.NewOrgHeader();
			client.CompanyData.OB_IsDebtor = true;
			var consignor = Helper.NewOrgHeader();

			var cost = Helper.NewCosting(carrier);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, "AUSYD", "USLAX", "", "20GP");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, "KG");
			costLine.GetCalculator<UnitCalculator>().PerUnit = 8;
			costLine.TL_RX_NKCurrency = CurrencyCodes.Australia;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsignorDocumentaryAddress.OrganisationPK = consignor.PK;
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = client.PK;
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_ActualWeight = 1500;
			shipment.JS_UnitOfWeight = "KG";
			shipment.JS_INCO = "FOB";

			Factory.Save();

			var shipmentJob = new Job.Loader(shipment).TryLoadOrCreate();
			shipmentJob.AddExRate(CurrencyCodes.UnitedStates, 7.1105m);
			var charge = shipmentJob.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_RX_NKSellCurrency = CurrencyCodes.UnitedStates;
			charge.JR_LocalCostAmt = 592.93m;

			charge.JR_OSCostAmt = 4216m;
			charge.JR_OH_CostAccount = carrier.PK;
			charge.JR_CostRatingOverride = true;

			PostCost(charge);

			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSSellAmt = 592.93m,
					JR_OSCostAmt = 4216m
				},
				new AssertionCharge
				{
					JR_OSSellAmt = 12000,
					JR_OSCostAmt = 12000,
					JR_CostRatingOverride = false,
					JR_CostRated = true,
				},
			};

			AutorateAndAssert("There should be another charge added", expected, shipment, client, null, shipmentJob, autorateRevenue: false);
		}

		public void TestAddAutoRates_AutoRatedCostIsPosted_CreateNewCharge()
		{
			var chargeCode = CreateTSTFRTCharge();
			TransportProvider1.OH_IsCreditor = true;

			var cost = Helper.NewCosting(null);
			var costEntry = cost.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "AU", "");
			costEntry.RateLines.RemoveAndDeleteAll();
			var costLine = costEntry.AddRateLine(chargeCode, UnitCalculator.Code, QuantityUnit.KG, CurrencyCodes.Australia);
			costLine.GetCalculator<UnitCalculator>().PerUnit = 4.5;

			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, NewClient.PK, "AUSYD", "USBOS", 800);
			var consol = CreateForwardingConsol(TransportModes.Sea, "AUSYD", "USBOS", TransportProvider1, shipment);
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "TEST1111117";
			container.JC_RC = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP")).PK;

			Factory.Save();

			var job = new Job.Loader(shipment).TryLoadOrCreate();
			var expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 3600m,
					JR_CostRatingOverride = false,
					JR_CostRated = true,
				},
			};

			AutorateAndAssert("AutoRating should create a charge on the job", expected, shipment, NewClient, job: job, autorateRevenue: false);

			shipment.JS_ActualWeight = 880;

			var charge = job.Charges[0];
			PostCost(charge);

			charge.JR_CostRatingOverride = true;

			expected = new[]
			{
				new AssertionCharge
				{
					JR_OSCostAmt = 3600m,
					JR_CostRatingOverride = true,
					JR_CostRated = true,
					CostCalculationDescription = "TSTFRT: 800 Kilogram(s) @ AUD 4.50/KG"
				},
				new AssertionCharge
				{
					JR_OSCostAmt = 3960m,
					JR_CostRatingOverride = false,
					JR_CostRated = true,
					CostCalculationDescription = "TSTFRT: 880 Kilogram(s) @ AUD 4.50/KG"
				},
			};

			AutorateAndAssert("Autorating the same job again should create a warning", expected, shipment, NewClient, job: job, autorateRevenue: false);

			var expectedLogLines = new[] { "Warning: Autorating has produced new charges for the same charge codes as pre-existing charges which resulted in following charges conflicting:",
			"  • TSTFRT",
			"Autorating deletes pre-existing charges unless one of the following conditions apply:",
" - the amount is Posted",
" - the amount is Apportioned",
" - the amount's 'Rating Behavior' is set to NEW",
" - the charge type of the charge code is 'Comment'.",
			"Pre-existing posted charge for the same debtor/creditor causes Autorating to discard matching newly autorated charges." };

			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		#endregion

		#region Rule 13: Costs Charges produced by accepting Group Company Charges are not cleared by Autorating

		[TestDate(2016, 08, 29)]
		[SuspendToTestReportJobIsChangedByDifferentCompany]/*Accounting objects, such as JobHeader, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		[SuspendToTestReportJobChargeIsChangedByDifferentCompany]/*Accounting objects, such as JobCharge, need to be processed under the correct company context. If you want to process many companies, you need to do that one by one*/
		public void TestAddAutoRates_AcceptedGroupCompanyCosts_STP()
		{
			var chargeCode = "GRPCHG";
			var globalChargeCode = Helper.ChargeCodes.CreateGlobalCharge(chargeCode);

			var testObjectCreator = new Accounting.Business.TestObjectCreator(Factory);
			var debtorCompany = testObjectCreator.CreateCompanyAndBranch("AUSYD");
			var creditorCompany = testObjectCreator.CreateCompanyAndBranch("NZAKL");
			var shipment = CreateForwardingShipment(TransportModes.Sea, Consignor.PK, Consignee.PK, "GBSUN", "AUSYD", 50);
			Factory.Save();

			var debtor = debtorCompany.OrgProxy;
			using (Env.SetTemporaryUserContext(testObjectCreator.GetUserContext(creditorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				debtor.OH_IsDebtor = true;
				job.LocalChargesPK = debtor.PK;

				var rate = Helper.NewClientRate(debtor);
				var entry = rate.AddRateEntry(RatingConstants.RateCategory.LCL, RateMode.LCL, "GBSUN", "AUSYD");
				entry.RateLines.RemoveAndDeleteAll();
				var line = entry.AddRateLine(chargeCode, FlatCalculator.Code, "", CurrencyCodes.Australia);
				line.GetCalculator<FlatCalculator>().BaseRate = 200;
				Factory.Save();

				Assert(shipment.IsCrossTrade());

				var expected = new[] { new AssertionCharge { ChargeCode = chargeCode, JR_OSSellAmt = 200m } };
				AutorateAndAssert(expected, shipment, debtor, job: job);

				AssertEquals(Guid.Empty, job.Charges[0].JR_OH_SellAccount);

				// if debtor.PK is not set here, next test will not have groupCompanyChargesForJob to run
				// because query will try to find the charges that have JR_OH_SellAccount of debtor.PK which is Guid.Empty now
				// Query can not be changed cause next test is not cross trade and changing query will out of scopes for cross trade

				job.Charges[0].JR_OH_SellAccount = debtor.PK;
			}

			Factory.Save();

			using (Env.SetTemporaryUserContext(testObjectCreator.GetUserContext(debtorCompany)))
			using (var job = new Job.Loader(Factory, shipment).TryCreateWithMutex())
			{
				debtor.OH_IsDebtor = true;

				AssertEquals("Pre-condition", 0, job.Charges.Count);

				var groupCompanyChargesForJob = new GroupCompanyChargesForJob(job);
				var groupCompanyCharges = groupCompanyChargesForJob.ChargesForDebtor.Cast<GroupCompanyCharge>().ToArray();
				job.GroupCompanyChargesForDebtor.AcceptSellChargesAsCosts(groupCompanyCharges);

				var acceptedCharge = job.Charges.Cast<Charge>().Single();
				AssertNotNull("Group Company Sell Charge should have been accepted as a cost", acceptedCharge);

				CombineAssertions(() =>
				{
					AssertEquals("acceptedCharge.JR_CostRatingOverride", false, acceptedCharge.JR_CostRatingOverride);
					AssertEquals("acceptedCharge.JR_SellRatingOverride", false, acceptedCharge.JR_SellRatingOverride);

					Assert("Should become readonly once accepted", acceptedCharge.JR_Calc_CostRatingBehaviorInfo.ReadOnly);
					AssertEquals(JobChargeLookups.StopFromAutorating, acceptedCharge.JR_Calc_CostRatingBehavior);
					AssertEquals(JobChargeLookups.ReAutorateCharge, acceptedCharge.JR_Calc_SellRatingBehavior);
				});

				var message = "No charges will be found but existing charges should remain.";
				var expected = new[]
				{
					new AssertionCharge { ChargeCode = chargeCode, JR_OSCostAmt = 200m, JR_OSSellAmt = 200m },
				};

				AutorateAndAssert(message, expected, shipment, debtor, job: job, autorateRevenue: false);
			}
		}

		#endregion

		#region Rating Override and Rating Behaviour Properties

		public void TestRatingOverride_AssertAutoRatedCharge()
		{
			var jobCharge = GetAutoRatedCharge(false);

			CombineAssertions("Pre-condition for all the tests using GetAutoRatedCharge", () =>
			{
				AssertEquals(JobChargeLookups.ReAutorateCharge, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals(true, jobCharge.JR_CostRated);
				AssertEquals(false, jobCharge.JR_CostRatingOverride);
				Assert("Should have produced cost payment basis", jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.ReAutorateCharge, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals("Sell Autorated", true, jobCharge.JR_SellRated);
				AssertEquals(false, jobCharge.JR_SellRatingOverride);
				Assert("Should have produced sell payment basis", jobCharge.CostPaymentBases.Count > 0);
			});
		}

		public void TestRatingOverride_LocalCurrency_IsPosted()
		{
			var jobCharge = GetAutoRatedCharge(false);
			Factory.Save();

			PostRevenue(jobCharge);
			PostCost(jobCharge);

			CombineAssertions("", () =>
			{
				AssertEquals(JobChargeLookups.StopFromAutorating, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals(80m, jobCharge.JR_OSCostAmt);
				AssertEquals(80m, jobCharge.JR_LocalCostAmt);
				AssertEquals("Cost Amount was originally set by Rating", true, jobCharge.JR_CostRated);
				AssertEquals("Should not be overriden just because local amount has changed due to currency", false, jobCharge.JR_CostRatingOverride);
				Assert("Should not have deleted cost payment basis", jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.StopFromAutorating, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals(100m, jobCharge.JR_OSSellAmt);
				AssertEquals(100m, jobCharge.JR_LocalSellAmt);
				AssertEquals("Sell Amount was originally set by Rating", true, jobCharge.JR_SellRated);
				AssertEquals("Should not be overriden just because local amount has changed due to currency", false, jobCharge.JR_SellRatingOverride);
				Assert("Should not have deleted sell payment basis", jobCharge.CostPaymentBases.Count > 0);
			});
		}

		public void TestRatingOverride_LocalCurrency_SetLocalAmount()
		{
			var jobCharge = GetAutoRatedCharge(false);

			CombineAssertions("Cost and Sell Currencies are the Local Currency for the Company", () =>
			{
				jobCharge.JR_LocalCostAmt = 88m;
				jobCharge.JR_LocalSellAmt = 111m;

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals("No exchange rate, OS should equal local", 88m, jobCharge.JR_OSCostAmt);
				AssertEquals("Cost Amount was originally set by Rating", true, jobCharge.JR_CostRated);
				AssertEquals("But should now be considered overriden", true, jobCharge.JR_CostRatingOverride);
				AssertEquals("Should have been deleted", false, jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals("No exchange rate, OS should equal local", 111m, jobCharge.JR_OSSellAmt);
				AssertEquals("Sell Amount was originally set by Rating", true, jobCharge.JR_SellRated);
				AssertEquals("But should now be considered overriden", true, jobCharge.JR_SellRatingOverride);
				AssertEquals("Should have been deleted", false, jobCharge.SellPaymentBases.Count > 0);
			});
		}

		public void TestRatingOverride_LocalCurrency_SetOverseasAmount()
		{
			var jobCharge = GetAutoRatedCharge(false);

			CombineAssertions("Cost and Sell Currencies are the Local Currency for the Company", () =>
			{
				jobCharge.JR_OSCostAmt = 88m;
				jobCharge.JR_OSSellAmt = 111m;

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals("No exchange rate, local should equal OS", 88m, jobCharge.JR_LocalCostAmt);
				AssertEquals("Cost Amount was originally set by Rating", true, jobCharge.JR_CostRated);
				AssertEquals("But should now be considered overriden", true, jobCharge.JR_CostRatingOverride);
				AssertEquals("Should have been deleted", false, jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals("No exchange rate, local should equal OS", 111m, jobCharge.JR_LocalSellAmt);
				AssertEquals("Sell Amount was originally set by Rating", true, jobCharge.JR_SellRated);
				AssertEquals("But should now be considered overriden", true, jobCharge.JR_SellRatingOverride);
				AssertEquals("Should have been deleted", false, jobCharge.SellPaymentBases.Count > 0);
			});
		}

		public void TestRatingOverride_OverseasCurrency_IsPosted()
		{
			var jobCharge = GetAutoRatedCharge(true);
			Factory.Save();

			PostRevenue(jobCharge);
			PostCost(jobCharge);

			CombineAssertions("", () =>
			{
				AssertEquals(JobChargeLookups.StopFromAutorating, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals(80m, jobCharge.JR_OSCostAmt);
				AssertEquals(45.71m, jobCharge.JR_LocalCostAmt);
				AssertEquals("Cost Amount was originally set by Rating", true, jobCharge.JR_CostRated);
				AssertEquals("Should not be overriden just because local amount has changed due to currency", false, jobCharge.JR_CostRatingOverride);
				Assert("Should not have deleted cost payment basis", jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.StopFromAutorating, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals(100m, jobCharge.JR_OSSellAmt);
				AssertEquals(57.14m, jobCharge.JR_LocalSellAmt);
				AssertEquals("Sell Amount was originally set by Rating", true, jobCharge.JR_SellRated);
				AssertEquals("Should not be overriden just because local amount has changed due to currency", false, jobCharge.JR_SellRatingOverride);
				Assert("Should not have deleted sell payment basis", jobCharge.CostPaymentBases.Count > 0);
			});
		}

		public void TestRatingOverride_OverseasCurrency_SetLocalAmount()
		{
			var jobCharge = GetAutoRatedCharge(true);

			CombineAssertions("", () =>
			{
				jobCharge.JR_LocalCostAmt = 88m;
				jobCharge.JR_LocalSellAmt = 111m;

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals("Local currency, same amount", 88m, jobCharge.JR_LocalCostAmt);
				AssertEquals("Cost Amount was originally set by Rating", true, jobCharge.JR_CostRated);
				AssertEquals(true, jobCharge.JR_CostRatingOverride);
				AssertEquals("Should have been deleted", false, jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals(true, jobCharge.JR_SellRatingOverride);
				AssertEquals("Local currency, same amount", 111m, jobCharge.JR_LocalSellAmt);
				AssertEquals("Sell Amount was originally set by Rating", true, jobCharge.JR_SellRated);
				AssertEquals("Should have been deleted", false, jobCharge.SellPaymentBases.Count > 0);
			});
		}

		public void TestRatingOverride_OverseasCurrency_SetOverseasAmount()
		{
			var jobCharge = GetAutoRatedCharge(true);

			CombineAssertions("", () =>
			{
				jobCharge.JR_OSCostAmt = 88m;
				jobCharge.JR_OSSellAmt = 111m;

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals(50.29m, jobCharge.JR_LocalCostAmt);
				AssertEquals("Cost Amount was originally set by Rating", true, jobCharge.JR_CostRated);
				AssertEquals(true, jobCharge.JR_CostRatingOverride);
				AssertEquals("Should have been deleted", false, jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.CreateNewCharge, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals(63.43m, jobCharge.JR_LocalSellAmt);
				AssertEquals("Sell Amount was originally set by Rating", true, jobCharge.JR_SellRated);
				AssertEquals(true, jobCharge.JR_SellRatingOverride);
				AssertEquals("Should have been deleted", false, jobCharge.SellPaymentBases.Count > 0);
			});
		}

		public void TestRatingOverride_OverseasCurrency_UpdateExchangeRate()
		{
			var jobCharge = GetAutoRatedCharge(true);

			CombineAssertions("", () =>
			{
				var costExchangeRate = jobCharge.CostExchangeRate;
				var sellExchangeRate = jobCharge.RevenueExchangeRate;
				costExchangeRate.SetBuyRate_ForTestOnly(1.65m);
				sellExchangeRate.SetBuyRate_ForTestOnly(1.65m);

				AssertEquals(JobChargeLookups.ReAutorateCharge, jobCharge.JR_Calc_CostRatingBehavior);
				AssertEquals("No reason for OS Amount to change", 80m, jobCharge.JR_OSCostAmt);
				AssertEquals(48.48m, jobCharge.JR_LocalCostAmt);
				AssertEquals("Cost Amount was originally set by Rating", true, jobCharge.JR_CostRated);
				AssertEquals("Should not be overriden just because local amount has changed due to currency", false, jobCharge.JR_CostRatingOverride);
				Assert("Should not have deleted cost payment basis", jobCharge.CostPaymentBases.Count > 0);

				AssertEquals(JobChargeLookups.ReAutorateCharge, jobCharge.JR_Calc_SellRatingBehavior);
				AssertEquals(100m, jobCharge.JR_OSSellAmt);
				AssertEquals(60.61m, jobCharge.JR_LocalSellAmt);
				AssertEquals("Sell Amount was originally set by Rating", true, jobCharge.JR_SellRated);
				AssertEquals("Should not be overriden just because local amount has changed due to currency", false, jobCharge.JR_SellRatingOverride);
				Assert("Should not have deleted sell payment basis", jobCharge.CostPaymentBases.Count > 0);
			});
		}

		#endregion

		#region Helper Methods

		AccChargeCode CreateTSTFRTCharge()
		{
			var glh = Factory.NewWithValidTestData<AccGLHeader>();

			var chargeCode = Factory.NewWithValidTestData<AccChargeCode>();
			chargeCode.AC_Code = "TSTFRT";
			chargeCode.AC_Desc = "Test Freight";
			chargeCode.AC_ChargeGroup = "FRT";
			chargeCode.AC_ChargeType = ChargeType.Margin;
			chargeCode.AC_AG_AccrualAccount = glh.PK;
			chargeCode.AC_AG_WIPAccount = glh.PK;
			chargeCode.AC_AT_GSTRate = Helper.ChargeCodes.CreateTaxRate(10).PK;

			return chargeCode;
		}

		Charge GetCharge(Job job, AccChargeCode chargeCode, ZBool costOverride, ZDecimal costAmount, ZBool sellOverride, ZDecimal sellAmount)
		{
			var charge = job.Charges.AddNew();
			charge.JR_AC = chargeCode.PK;
			charge.JR_LocalCostAmt = costAmount;
			charge.JR_CostRatingOverride = costOverride;
			charge.JR_LocalSellAmt = sellAmount;
			charge.JR_SellRatingOverride = sellOverride;

			return charge;
		}

		Charge GetAutoRatedCharge(bool useForeignCurrency)
		{
			var shipment = GetShipmentWithMatchingCostAndClientRate(Consignee, Consignor, TransportProvider1, "AUSYD", "GBSUN", "FRT", 100m, 80m);
			if (useForeignCurrency)
			{
				var rateLines = Factory.Load<RateLine>(new ZQuery());
				rateLines.ForEach(x => x.TL_RX_NKCurrency = CurrencyCodes.UnitedKingdom);
			}
			Factory.Save();

			var job = new Job.Loader(shipment).TryLoadOrCreateWithoutMutexForTestOnly();
			job.JH_GE = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "FES")).PK;

			if (useForeignCurrency)
			{
				var costExRate = job.AddExRate(CurrencyCodes.UnitedKingdom, 1.75m);
				costExRate.JF_OrgType = ExchangeRateOrgTypeEnum.Creditor.ToCode();

				var sellExRate = job.AddExRate(CurrencyCodes.UnitedKingdom, 1.75m);
				sellExRate.JF_OrgType = ExchangeRateOrgTypeEnum.Debtor.ToCode();
			}

			var expectedCurrency = useForeignCurrency ? CurrencyCodes.UnitedKingdom : CurrencyCodes.Australia;
			var expectedLocalCost = useForeignCurrency ? 45.71m : 80m;
			var expectedLocalSell = useForeignCurrency ? 57.14m : 100m;
			var expected = new[]
			{
				new AssertionCharge
				{
					ChargeCode = "FRT",
					JR_OSCostAmt = 80m,
					JR_LocalCostAmt = expectedLocalCost,
					JR_RX_NKCostCurrency = expectedCurrency,
					JR_OSSellAmt = 100m,
					JR_LocalSellAmt = expectedLocalSell,
					JR_RX_NKSellCurrency = expectedCurrency,
				},
			};

			AutorateAndAssert(expected, shipment, Consignee, job: job);

			return job.Charges[0];
		}

		#endregion
	}
}
