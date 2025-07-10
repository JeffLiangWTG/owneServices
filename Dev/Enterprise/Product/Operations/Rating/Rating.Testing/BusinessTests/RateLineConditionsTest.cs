using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Rating.Business.Testing;
using Enterprise.TransportBookings.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class RateLineConditionsTest : BaseRatingIntegrationTest
	{
		public void TestDeletedDocJobCharge()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var clientRateLine = clientRateEntry.AddRateLine("OCART", FlatCalculator.Code);
			clientRateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;
			clientRateLine.TL_Condition = RateLineConditions.UserDefined;
			clientRateLine.TL_ConditionalExpression = "(\"<JobHeader.JobChargesForLocalClient.ChargeCode.Code>\" == \"FRT\")";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);
			shipment.JS_UniqueConsignRef = "SHP0001";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = Consignee.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 10000;
			charge.JR_OH_CostAccount = TransportProvider1.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 5000;

			Factory.Save();

			AutorateAndAssert
			(
				new[]
				{
					new AssertionCharge { ChargeCode = "OCART" },
					new AssertionCharge { ChargeCode = "FRT" }
				},
				shipment,
				Consignee,
				deleteJobCharges: false
			);

			job.Charges.RemoveAndDeleteAll();

			// Given previously autorated charges are deleted, WHEN re-autorate, THEN should not throw deleted row exception
			AutorateAndAssert
			(
				Array.Empty<AssertionCharge>(),
				shipment,
				Consignee,
				deleteJobCharges: false
			);
		}

		public void TestAddedDocJobCharge()
		{
			var clientRate = Helper.NewClientRate(Consignor);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			clientRateEntry.RateLines.RemoveAndDeleteAll();

			var clientRateLine = clientRateEntry.AddRateLine("OCART", FlatCalculator.Code);
			clientRateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;
			clientRateLine.TL_Condition = RateLineConditions.UserDefined;
			clientRateLine.TL_ConditionalExpression = "(\"<JobHeader.JobChargesForLocalClient.ChargeCode.Code>\" == \"FRT\")";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);
			shipment.JS_UniqueConsignRef = "SHP0001";

			var job = CreateJob(shipment, shipment.JS_UniqueConsignRef);

			Factory.Save();

			// Given no charge, THEN autorate should not return any charge
			AutorateAndAssert
			(
				Array.Empty<AssertionCharge>(),
				shipment,
				Consignee,
				deleteJobCharges: false
			);

			var charge = job.Charges.AddNew();
			charge.JR_AC = Env.Registry.FreightChargeCode;
			charge.JR_OH_SellAccount = Consignee.PK;
			charge.JR_RX_NKSellCurrency = "USD";
			charge.JR_OSSellAmt = 10000;
			charge.JR_OH_CostAccount = TransportProvider1.PK;
			charge.JR_RX_NKCostCurrency = "USD";
			charge.JR_OSCostAmt = 5000;

			// Given matching charge, then autorate should return charge
			AutorateAndAssert
			(
				new[]
				{
					new AssertionCharge { ChargeCode = "OCART" },
					new AssertionCharge { ChargeCode = "FRT" }
				},
				shipment,
				Consignee,
				deleteJobCharges: false
			);
		}

		public void TestRateLineConditions()
		{
			var container1 = Helper.Containers["20GP"];
			var container2 = Helper.Containers["20RE"];
			container1.RC_HandlingRateClass = "20GN";
			container2.RC_HandlingRateClass = "20GN";

			var costing = Helper.NewCosting(TransportProvider1);

			var costEntry1 = costing.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX");
			costEntry1.TI_RC = container1.PK;
			costEntry1.TI_MatchContainerRateClass = true;
			var costLine1 = costEntry1.AddRateLine("ODOC", FlatCalculator.Code);
			costLine1.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var costEntry2 = costing.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX");
			costEntry2.TI_RC = container2.PK;
			var costLine2 = costEntry2.AddRateLine("ODOC", FlatCalculator.Code);
			costLine2.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariff = Helper.NewCompanyTariff();

			var orgTariffEntry = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX");
			orgTariffEntry.TI_RC = container1.PK;
			orgTariffEntry.TI_MatchContainerRateClass = true;
			var orgTariffline = orgTariffEntry.AddRateLine("ODOC", CompanyTariffOrCostBasedCalculator.CostBasedCode);
			orgTariffline.GetCalculator<CompanyTariffOrCostBasedCalculator>().Percent = 5m;

			var client = Helper.NewOrgHeader(1);
			var rate = Helper.NewClientRate(client);

			var frtEntry = rate.AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			frtEntry.TI_RX_NKCurrency = CurrencyCodes.Australia;
			frtEntry.TI_RC = container2.PK;
			frtEntry.RateLines.RemoveAndDeleteAll();

			var frtTariffLine = frtEntry.AddRateLine("FRT", FlatCalculator.Code);
			frtTariffLine.GetCalculator<FlatCalculator>().BaseRate = 2000m;
			frtTariffLine.TL_Condition = RateLineConditions.ForwardingAndBrokerage;

			var dstTariffEntry = rate.AddRateEntry("DST", "FCL", "AUSYD", "USLAX");
			dstTariffEntry.TI_RC = container2.PK;
			var dstTariffLine = dstTariffEntry.AddRateLine("DDOC", FlatCalculator.Code);
			dstTariffLine.GetCalculator<FlatCalculator>().BaseRate = 75m;
			dstTariffLine.TL_Condition = RateLineConditions.OwnBrokerage;

			tariff.Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OH_ExportBroker = TransportProvider1.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = Factory.LoadFromNaturalKey<RefContainer>(RefContainerSchema.RC_Code, "20RE").PK;
			container.JC_ContainerMode = ContainerModes.FCL;

			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 100, JR_OSSellAmt = 105 },
			};

			AutorateAndAssert(expected, shipment, client);

			costLine2.TL_Condition = RateLineConditions.UserDefined;
			costLine2.TL_ConditionalExpression = "\"<JobNumber>\"==\"SHIPMENT2\"";

			Factory.Save();

			expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 50, JR_OSSellAmt = 52.5 } };
			AutorateAndAssert(expected, shipment, client);

			shipment.Job.JH_JobNum = "SHIPMENT2";
			shipment.JS_UniqueConsignRef = "SHIPMENT2";

			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 100, JR_OSSellAmt = 105 },
			};
			AutorateAndAssert(expected, shipment, client);

			costLine1.TL_Condition = RateLineConditions.OwnCFS;
			costLine2.TL_Condition = RateLineConditions.OwnCFS;
			orgTariffline.TL_Condition = RateLineConditions.OwnCFS;
			tariff.Factory.Save();
			Factory.Save();

			shipment.JS_OH_ImportBroker = GlbBranch.CurrentBranch.OrgProxy.PK;

			expected = new[] { new AssertionCharge { ChargeCode = "DDOC", JR_OSSellAmt = 75 } };
			AutorateAndAssert(expected, shipment, client);

			shipment.JS_OH_ImportBroker = ZGuid.Empty;
			shipment.JS_OH_ExportBroker = GlbBranch.CurrentBranch.OrgProxy.PK;
			consol.JK_OA_SendingForwarderAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			expected = new[] { new AssertionCharge { ChargeCode = "FRT", JR_OSSellAmt = 2000 } };
			AutorateAndAssert(expected, shipment, client);

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			shipment.JS_OH_ExportBroker = TransportProvider1.PK;
			shipment.JS_OA_ExportReceivingDepot = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 100, JR_OSSellAmt = 105 },
			};
			AutorateAndAssert(expected, shipment, client);
		}

		#region Own Gateway

		public void TestRateLineConditions_OwnGateway()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OPCH");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OSEC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX");
			var costRate1 = costEntry.AddRateLine("OPCH", FlatCalculator.Code);
			costRate1.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var costRate2 = costEntry.AddRateLine("ODOC", FlatCalculator.Code);
			costRate2.TL_Condition = RateLineConditions.OwnGateway;
			costRate2.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariff = Helper.NewCompanyTariff();
			var tariffRate = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX").AddRateLine("OSEC", FlatCalculator.Code);
			tariffRate.TL_Condition = RateLineConditions.OwnGateway;
			tariffRate.GetCalculator<FlatCalculator>().BaseRate = 5m;

			var client = Helper.NewOrgHeader(1);
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(clientRateEntry.Currency);

			var clientRate = clientRateEntry.AddRateLine("FRT", FlatCalculator.Code);
			clientRate.TL_Condition = RateLineConditions.OwnGateway;
			clientRate.GetCalculator<FlatCalculator>().BaseRate = 2000m;

			tariff.Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OH_ExportBroker = TransportProvider1.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "OPCH", JR_OSCostAmt = 50m } };

			AutorateAndAssert("Only the charge code without the OwnGateway requirement should be matched", expected, shipment, client);

			var orgProxyAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress;
			consol.JK_OA_SendingForwarderAddress = orgProxyAddress.PK;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "OPCH", JR_OSCostAmt = 50m },
				new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 100m },
				new AssertionCharge { ChargeCode = "OSEC", JR_OSCostAmt = 5m },
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 2000m }
			};

			AutorateAndAssert("Expected to have matched all charges now that the Sending Agent is the Org Proxy", expected, shipment, client);
		}

		#endregion

		#region Own Controlling Agent

		public void TestRateLineConditions_OwnControllingAgent()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OSEC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("FRT");

			var costing = Helper.NewCosting(TransportProvider1);
			var costEntry = costing.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX");
			var costRate1 = costEntry.AddRateLine("OPCH", FlatCalculator.Code);
			costRate1.GetCalculator<FlatCalculator>().BaseRate = 50m;

			var costRate2 = costEntry.AddRateLine("ODOC", FlatCalculator.Code);
			costRate2.TL_Condition = RateLineConditions.OwnControllingAgent;
			costRate2.GetCalculator<FlatCalculator>().BaseRate = 100m;

			var tariff = Helper.NewCompanyTariff();
			var tariffRate = tariff.AddRateEntry("ORG", "FCL", "AUSYD", "USLAX").AddRateLine("OSEC", FlatCalculator.Code);
			tariffRate.TL_Condition = RateLineConditions.OwnControllingAgent;
			tariffRate.GetCalculator<FlatCalculator>().BaseRate = 5m;

			var client = Helper.NewOrgHeader(1);
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry("FCL", "SEA", "AUSYD", "USLAX");
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			AddParityExchangeRate(clientRateEntry.Currency);

			var clientRate = clientRateEntry.AddRateLine("FRT", FlatCalculator.Code);
			clientRate.TL_Condition = RateLineConditions.OwnControllingAgent;
			clientRate.GetCalculator<FlatCalculator>().BaseRate = 2000m;

			tariff.Factory.Save();

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.JS_OH_ExportBroker = TransportProvider1.PK;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.Transports[0].JW_IsLinked = false;

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "OPCH", JR_OSCostAmt = 50m } };

			AutorateAndAssert("Only the charge code without the OwnControllingAgent requirement should be matched", expected, shipment, client);

			var orgProxyAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress;
			shipment.ControllingAgentDocumentaryAddress.E2_OA_Address = orgProxyAddress.PK;
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "OPCH", JR_OSCostAmt = 50m },
				new AssertionCharge { ChargeCode = "ODOC", JR_OSCostAmt = 100m },
				new AssertionCharge { ChargeCode = "OSEC", JR_OSCostAmt = 5m },
				new AssertionCharge { ChargeCode = "FRT", JR_OSCostAmt = 2000m }
			};

			AutorateAndAssert("Expected to have matched all charges now that the Controlling Agent is the Org Proxy", expected, shipment, client);
		}

		#endregion

		#region Has Dangerous Goods

		public void TestRateLineConditions_HasDangerousGoods_AgencyShipment()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("ODOC");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OSEC");
			var refContainer = Helper.Containers["20GP"];

			var client = Helper.NewOrgHeader();
			var clientRateEntry = Helper.NewClientRate(client).AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AU", "US");
			clientRateEntry.TI_RC = refContainer.PK;
			clientRateEntry.RateLines.RemoveAndDeleteAll();
			var clientRate1 = clientRateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			clientRate1.GetCalculator<FlatCalculator>().BaseRate = 80m;

			var clientRate2 = clientRateEntry.AddRateLine("OSEC", FlatCalculator.Code);
			clientRate2.GetCalculator<FlatCalculator>().BaseRate = 200m;
			clientRate2.TL_Condition = RateLineConditions.DangerousGoods;

			var costEntry = Helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.SCO, RateMode.SEA, "AU", "US");
			costEntry.TI_RC = refContainer.PK;
			costEntry.RateLines.RemoveAndDeleteAll();
			var costRateLine = costEntry.AddRateLine("OSEC", FlatCalculator.Code);
			costRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			costRateLine.TL_Condition = RateLineConditions.DangerousGoods;
			AddParityExchangeRate(costRateLine.Currency);

			Factory.Save();

			var billOfLading = Factory.NewWithValidTestData<BillOfLading>();
			billOfLading.JS_TransportMode = TransportModes.Sea;
			billOfLading.JS_PackingMode = ContainerModes.FCL;
			billOfLading.JS_RL_NKOrigin = "AUSYD";
			billOfLading.JS_RL_NKDestination = "USLAX";
			billOfLading.JS_INCO = PaymentType.Prepaid;
			billOfLading.ConsigneeDocumentaryAddress.OrganisationPK = client.MainAddress.PK;

			var container = billOfLading.RealContainers.AddNew();
			container.JC_RC = refContainer.PK;

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 80m } };

			AutorateAndAssert("OSEC rate lines filtered out reason: RateLine condition DAG not met", expected, billOfLading, client);

			billOfLading.OuterPackLines.AddNew().UNDGs.AddNew();

			expected = new[]
				{
					new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 80m },
					new AssertionCharge { ChargeCode = "OSEC", CostCalculationDescription = "Base Rate USD 50.00", RevenueCalculationDescription = "Base Rate USD 200.00" }
				};

			AutorateAndAssert(expected, billOfLading, client);
		}

		public void TestRateLineConditions_HasDangerousGoods_ForwardingConsol()
		{
			GlbDepartment.GetCurrentDepartment(Factory).GE_Misc = false;
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CNS", "Consol Cost", "", ChargeCodeGroupList.Codes.Origin);
			var chargeCode2 = Helper.ChargeCodes.NewConsolChargeCode("DGS", "Dangerous Goods", "", ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("CNS");
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("DGS");

			var costEntry = Helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "US");
			var costRateLine1 = costEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			costRateLine1.GetCalculator<FlatCalculator>().BaseRate = 50m;
			var costRateLine2 = costEntry.AddRateLine(chargeCode2.AC_Code, FlatCalculator.Code);
			costRateLine2.GetCalculator<FlatCalculator>().BaseRate = 150m;
			costRateLine2.TL_Condition = RateLineConditions.DangerousGoods;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			var container = consol.Containers.AddNew();
			var shipment = consol.Shipments.AddNew();
			var line = shipment.OuterPackLines.AddNew();
			line.JL_JC = container.PK;
			consol.Shipments.AddNew();

			Factory.Save();

			var expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = chargeCode1.AC_Code, E6_OSCostAmount = 50m }
				};

			AutoCostAndAssert("OSEC rate lines filtered out reason: RateLine condition DAG not met", null, expectedCosts, consol, false);

			consol.Shipments[0].OuterPackLines.AddNew().UNDGs.AddNew();

			expectedCosts = new[]
				{
					new AssertionCost { ChargeCode = chargeCode1.AC_Code, E6_OSCostAmount = 50m },
					new AssertionCost { ChargeCode = chargeCode2.AC_Code, E6_OSCostAmount = 150m }
				};

			AutoCostAndAssert("RateLine condition should now be met", null, expectedCosts, consol, false);
		}

		public void TestRateLineConditions_HasDangerousGoods_ForwardingShipment()
		{
			Helper.ChargeCodes.SetTemporaryDepartmentValueOnChargeCode("OSEC");

			var localClient = Helper.NewOrgHeader();
			var clientRateEntry = Helper.NewClientRate(localClient).AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AUSYD", "USLAX");
			var clientRate1 = clientRateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			clientRate1.GetCalculator<FlatCalculator>().BaseRate = 80m;
			var clientRate2 = clientRateEntry.AddRateLine("OSEC", FlatCalculator.Code);
			clientRate2.GetCalculator<FlatCalculator>().BaseRate = 200m;
			clientRate2.TL_Condition = RateLineConditions.DangerousGoods;

			var costEntry = Helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.FCL, "AU", "US");
			var costRateLine = costEntry.AddRateLine("OSEC", FlatCalculator.Code);
			costRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			costRateLine.TL_Condition = RateLineConditions.DangerousGoods;

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = ContainerModes.FCL;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";
			shipment.OuterPackLines.AddNew();
			var consol = shipment.Consols.AddNew();
			consol.Containers.AddNew();

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 80m } };

			AutorateAndAssert("OSEC rate lines filtered out reason: RateLine condition DAG not met", expected, shipment, localClient);

			shipment.OuterPackLines.AddNew().UNDGs.AddNew();

			expected = new[]
				{
					new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 80m },
					new AssertionCharge { ChargeCode = "OSEC", JR_OSCostAmt = 50m, JR_OSSellAmt = 200m }
				};

			AutorateAndAssert(expected, shipment, localClient);

			shipment.OuterPackLines.RemoveAndDeleteAll();

			expected = new[] { new AssertionCharge { ChargeCode = "ODOC", JR_OSSellAmt = 80m } };

			AutorateAndAssert("Dangerous goods rate line condition is no longer applicable", expected, shipment, localClient);
		}

		public void TestRateLineConditions_HasDangerousGoods_TransportBooking()
		{
			var tbChargeCode = "TBCC";
			var dgChargeCode = "DGTBCC";
			Helper.ChargeCodes.New(tbChargeCode, "Transport Booking", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);
			Helper.ChargeCodes.New(dgChargeCode, "Dangerous Goods Transport", FlatCalculator.Code, ChargeCodeGroupList.Codes.TransportBooking);

			var localClient = Helper.NewOrgHeader();
			var clientRateEntry = Helper.NewClientRate(localClient).AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			var clientRate1 = clientRateEntry.AddRateLine(tbChargeCode, FlatCalculator.Code);
			clientRate1.GetCalculator<FlatCalculator>().BaseRate = 80m;
			var clientRate2 = clientRateEntry.AddRateLine(dgChargeCode, FlatCalculator.Code);
			clientRate2.GetCalculator<FlatCalculator>().BaseRate = 200m;
			clientRate2.TL_Condition = RateLineConditions.DangerousGoods;
			var costEntry = Helper.NewCosting(null).AddRateEntry(RatingConstants.RateCategory.TBC, RateMode.ALL, "AU", "");
			var costRateLine = costEntry.AddRateLine(dgChargeCode, FlatCalculator.Code);
			costRateLine.GetCalculator<FlatCalculator>().BaseRate = 50m;
			costRateLine.TL_Condition = RateLineConditions.DangerousGoods;

			Factory.Save();

			var supplier = Factory.NewWithValidTestData<OrgHeader>();
			supplier.OH_RL_NKClosestPort = "AUBNE";
			supplier.OH_IsCreditor = true;

			var bookingConsolidation = Factory.New<DtbBookingConsolidation>();
			var booking = bookingConsolidation.Bookings.AddNew();
			booking.KM_KT_NKBookingTemplate = "EFPU";
			booking.KM_RatingFreightMode = ContainerModes.Loose;
			booking.Address.OrganisationPK = localClient.PK;

			var fromInstruction = booking.Instructions.AddNew();
			fromInstruction.KN_InstructionType = InstructionTypes.Codes.PickUp;
			fromInstruction.KN_IsLooseRateable = true;
			fromInstruction.Address.OrganisationPK = supplier.PK;

			var toInstruction = booking.Instructions.AddNew();
			toInstruction.KN_InstructionType = InstructionTypes.Codes.Delivery;
			toInstruction.KN_IsLooseRateable = true;
			toInstruction.Address.OrganisationPK = localClient.PK;

			var package = CreatePackage(booking, 3, PkgUnit.Package, null, 30m, Weight.Kilograms, 1m, Volume.CubicMetres);

			CreateInstructionPkgDivots(fromInstruction, package);
			CreateInstructionPkgDivots(toInstruction, package);

			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = tbChargeCode, JR_OSSellAmt = 80m } };

			AutorateAndAssert("DGTB rate lines filtered out reason: RateLine condition DAG not met", expected, booking, localClient);

			package.UNDGs.AddNew();
			booking.KM_IsHazardous = true;

			expected = new[]
				{
					new AssertionCharge { ChargeCode = tbChargeCode, JR_OSSellAmt = 80m },
					new AssertionCharge { ChargeCode = dgChargeCode, JR_OSCostAmt = 50m, JR_OSSellAmt = 200m }
				};

			AutorateAndAssert(expected, booking, localClient);
		}

		#endregion

		#region Own Brokerage

		public void TestRateLineConditions_OwnBrokerage()
		{
			var localClient = Helper.NewOrgHeader();
			var consignee = Helper.NewOrgHeader(1);

			#region Create Rate

			var cclrChargeCode = Helper.ChargeCodes["CCLR"];
			cclrChargeCode.AC_ChargeGroup = "BRK";

			var rate = Helper.NewClientRate(localClient);

			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.DST, RateMode.FCL, "", "AU");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine(cclrChargeCode, UnitCalculator.Code, QuantityUnit.CN);
			rateLine.TL_Condition = RateLineConditions.OwnBrokerage;
			rateLine.GetCalculator<UnitCalculator>().PerUnit = 20m;

			#endregion

			#region Create shipment

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = "FCL";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var container = consol.Containers.AddNew();
			container.JC_ContainerMode = "FCL";
			container.JC_ContainerNum = "FAKE4100011";
			container.JC_RC = Helper.Containers["20GP"].PK;

			var shipment = consol.Shipments.AddNew();
			shipment.JS_TransportMode = TransportModes.Sea;
			shipment.JS_PackingMode = "FCL";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_INCO = "EXW";
			shipment.JS_ActualWeight = 3000;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.ConsigneeDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;
			shipment.ConsignorDocumentaryAddress.OrganisationPK = localClient.MainAddress.PK;

			#endregion

			#region Create declaration

			var declaration = (BaseJobDeclaration)Factory.New<Integration.Customs.IBaseJobDeclaration>();
			declaration.JE_TransportMode = TransportModes.Sea;
			declaration.JE_ContainerMode = ContainerModes.FCL;
			declaration.JE_MessageType = Customs.Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = "FRM";
			declaration.JE_RL_NKOrigin = "USLAX";
			declaration.JE_RL_NKFinalDestination = "AUSYD";
			declaration.JE_ShipmentIncoTerm = "EXW";
			declaration.JE_JS = shipment.PK;

			var cusContainer = declaration.CusContainers.AddNew();
			cusContainer.CO_JC = container.PK;
			cusContainer.CO_FCL_LCL_AIR = ContainerModes.FCL;

			#endregion

			Factory.Save();

			var expected = new[]
				{
					new AssertionCharge
						{
							ChargeCode = "CCLR",
							JR_OSSellAmt =  20m,
						}
				};

			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

			AutorateAndAssert(expected, shipment, localClient, autorateCosts: false);
		}

		#endregion

		#region User Defined

		public void TestRateLineConditions_UserDefined_AliasExpression()
		{
			var rate = Helper.NewClientRate(Consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;
			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "MOD=AIR";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);
			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "OCART" } };

			AutorateAndAssert(expected, shipment, Consignee);
		}

		public void TestRateLineConditions_UserDefined_ComparisonOperator()
		{
			var rate = Helper.NewClientRate(Consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;
			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "JS_ActualWeight>=300";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 400m);
			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "OCART" } };

			AutorateAndAssert(expected, shipment, Consignee);
		}

		public void TestRateLineConditions_UserDefined_DocEngineExpression()
		{
			var rate = Helper.NewClientRate(Consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OCART", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;
			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "\"<FreightShipment.JS_TransportMode>\"==\"AIR\"";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);
			Factory.Save();

			var expected = new[] { new AssertionCharge { ChargeCode = "OCART" } };

			AutorateAndAssert(expected, shipment, Consignee);
		}

		public void TestRateLineConditions_UserDefined_NonBooleanExpression()
		{
			var rate = Helper.NewClientRate(Consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine = rateEntry.AddRateLine("OAQF", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 100m;
			rateLine.TL_Condition = RateLineConditions.UserDefined;
			rateLine.TL_ConditionalExpression = "JS_ActualWeight";

			var rateLine3 = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 200m;
			rateLine3.TL_Condition = RateLineConditions.UserDefined;
			rateLine3.TL_ConditionalExpression = "\"<JS_ActualWeight>\"";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);
			Factory.Save();

			var expectedLogLines = new[]
			{
				"Information: RateLine Found OAQF-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1",
				"Information: RateLine Filtered OAQF-FLT-Client Rate CONSIGNOR1	reason:	RateLine condition JS_ActualWeight not met",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1	reason:	RateLine condition \"<JS_ActualWeight>\" cannot be evaluated as boolean value. Please rewrite it.",
			};

			AutorateAndAssert("Neither Condition is valid, should provide error message", null, shipment, Consignee);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestRateLineConditions_UserDefined_MetAndNotRemoved()
		{
			var rate = Helper.NewClientRate(Consignor);
			var rateEntryGeneral = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			var rateEntrySpecific = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AU", "");
			rateEntryGeneral.RateLines.RemoveAndDeleteAll();
			rateEntrySpecific.RateLines.RemoveAndDeleteAll();

			var lineWithMetCondition = rateEntryGeneral.AddRateLine("ODOC", FlatCalculator.Code);
			lineWithMetCondition.GetCalculator<FlatCalculator>().BaseRate = 10m;
			lineWithMetCondition.TL_Condition = RateLineConditions.UserDefined;
			lineWithMetCondition.TL_ConditionalExpression = "JS_ActualWeight > 0";

			var lineWithNoCondition = rateEntryGeneral.AddRateLine("OAQF", FlatCalculator.Code);
			lineWithNoCondition.GetCalculator<FlatCalculator>().BaseRate = 11m;

			var lineWithUnmetCondition = rateEntrySpecific.AddRateLine("ODOC", FlatCalculator.Code);
			lineWithUnmetCondition.GetCalculator<FlatCalculator>().BaseRate = 20m;
			lineWithUnmetCondition.TL_Condition = RateLineConditions.UserDefined;
			lineWithUnmetCondition.TL_ConditionalExpression = "JS_ActualWeight > 100";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);

			Factory.Save();

			var expectedLogLines = new[]
			{
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine NOT Filtered ODOC-FLT-Client Rate CONSIGNOR1	reason:	RateLine condition JS_ActualWeight > 0 met",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1	reason:	RateLine condition JS_ActualWeight > 100 not met",
			};
			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 10m },
				new AssertionCharge { ChargeCode = "OAQF", JR_LocalSellAmt = 11m },
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
			AssertAutoratingAuditLogNotContains(shipment, "Information: RateLine NOT Filtered OAQF-FLT-Client Rate CONSIGNOR1	reason:	RateLine condition  met");
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestRateLineConditions_UserDefined_MetAndRemoved()
		{
			var rate = Helper.NewClientRate(Consignor);
			var rateEntryGeneral = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			var rateEntrySpecific = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.AIR, "AU", "");
			rateEntryGeneral.RateLines.RemoveAndDeleteAll();
			rateEntrySpecific.RateLines.RemoveAndDeleteAll();

			var lineWithMetCondition = rateEntryGeneral.AddRateLine("ODOC", FlatCalculator.Code);
			lineWithMetCondition.GetCalculator<FlatCalculator>().BaseRate = 10m;
			lineWithMetCondition.TL_Condition = RateLineConditions.UserDefined;
			lineWithMetCondition.TL_ConditionalExpression = "JS_ActualWeight > 0";

			var lineWithoutCondition = rateEntrySpecific.AddRateLine("ODOC", FlatCalculator.Code);
			lineWithoutCondition.GetCalculator<FlatCalculator>().BaseRate = 20m;

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);

			Factory.Save();

			var expectedLogLines = new[]
			{
				"Information: RateLine Found ODOC-FLT-Client Rate CONSIGNOR1 (x2)",
				"Information: RateLine Filtered ODOC-FLT-Client Rate CONSIGNOR1	reason:	overridden by ODOC-FLT-Client Rate CONSIGNOR1 by Mode and Category comparer"
			};
			var expectedCharges = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 20m },
			};

			AutorateAndAssert(expectedCharges, shipment, Consignee);
			AssertAutoratingAuditLogNoteContainsLines(shipment, "Log should contain expected lines", expectedLogLines);
		}

		public void TestRateLineConditions_UserDefined_PropertiesOnJob()
		{
			var rate = Helper.NewClientRate(Consignor);
			var rateEntry = rate.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, "AU", "");
			rateEntry.RateLines.RemoveAndDeleteAll();

			var rateLine1 = rateEntry.AddRateLine("ODOC", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 110m;
			rateLine1.TL_Condition = RateLineConditions.UserDefined;
			rateLine1.TL_ConditionalExpression = "JS_INCO==\"DDP\"";

			var rateLine2 = rateEntry.AddRateLine("OAQF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 200m;
			rateLine2.TL_Condition = RateLineConditions.UserDefined;
			rateLine2.TL_ConditionalExpression = "\"<FreightShipment.Job.Company.GC_Code>\"==\"EDI\"";

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "AUSYD", "CNSHA", 1m);
			shipment.JS_INCO = IncoTerms.DeliveredDutyPaid;
			Factory.Save();

			var expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 110m },
				new AssertionCharge { ChargeCode = "OAQF", JR_LocalSellAmt = 200m }
			};

			AutorateAndAssert("Should have matched the DDP rates", expected, shipment, Consignee);

			var knownShipperDetails = Consignor.MainAddress.KnownShipperDetails.AddNew();
			knownShipperDetails.OV_OH_OrgHeader = Consignor.PK;
			knownShipperDetails.OV_EXApprovedOrMajorExporter = AviationSecuritySchemeMembership.Codes.KnownConsignor;
			knownShipperDetails.OV_EXApprovalExpiryDate = ZDate.Today.AddDays(1);
			knownShipperDetails.OV_EXApprovalNumber = "00001-03";

			rateLine1.TL_ConditionalExpression = "\"<Consignor.CountryDataCollectionForThisCompany.OV_EXApprovalNumber>\"==\"\"";
			rateLine2.TL_ConditionalExpression = "\"<Consignor.OH_Code>\"==\"CONSIGNOR1\"";
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "OAQF", JR_LocalSellAmt = 200m }
			};

			AutorateAndAssert("Should match with OAQF Only", expected, shipment, Consignee);

			rateLine1.TL_ConditionalExpression = "\"<Consignor.AviationSecurity.ApprovalNumber>\"==\"00001-03\"";
			rateLine2.TL_ConditionalExpression = "\"<Consignor.OH_Code>\"!=\"\"";
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 110m },
				new AssertionCharge { ChargeCode = "OAQF", JR_LocalSellAmt = 200m }
			};

			AutorateAndAssert("Both Charges are matched", expected, shipment, Consignee);

			rateLine1.TL_ConditionalExpression = "\"<Consignor.AviationSecurity.ApprovalNumber>\"==\"00001-01\"";
			rateLine2.TL_ConditionalExpression = "\"<Consignor.CountryDataCollectionForThisCompany.OV_EXApprovalNumber>\"==\"00001-01\"";
			Factory.Save();

			AutorateAndAssert("No charge should be found", null, shipment, Consignee);

			knownShipperDetails.OV_EXApprovedOrMajorExporter = "NO";
			knownShipperDetails.OV_EXApprovalNumber = "";

			rateLine1.TL_ConditionalExpression = "\"<Consignor.AviationSecurity.ApprovalNumber>\"!=\"00001-01\"&&\"<Consignor.OH_Code>\"==\"CONSIGNOR1\"";
			rateLine2.TL_ConditionalExpression = "\"<Consignor.CountryDataCollectionForThisCompany.OV_EXApprovalNumber>\"==\"00001-01\"||\"<Consignee.OH_Code>\"==\"CONSIGNEE1\"";
			Factory.Save();

			AutorateAndAssert("Both Charges are applicable", expected, shipment, Consignee);

			GlbBranch.CurrentBranch.GB_Code = "BNE";
			rateLine1.TL_ConditionalExpression = "\"<Consignor.AviationSecurity.ApprovalNumber>\"==\"\"&&\"<Consignor.OH_Code>\"==\"CONSIGNOR1\"";
			rateLine2.TL_ConditionalExpression = "\"<BranchCode>\"==\"BNE\"&&\"<Consignee.OH_Code>\"!=\"CONSIGNEE1\"";
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "ODOC", JR_LocalSellAmt = 110m }
			};

			AutorateAndAssert("Only ODOC is applicable", expected, shipment, Consignee);

			Consignee.MainAddress.OA_State = "QLD";

			rateLine1.TL_ConditionalExpression = "\"<Consignee.Addresses.State>\"==\"Queensland\"";
			rateLine2.TL_ConditionalExpression = "\"<Consignee.Addresses.State>\"==\"QLD\"";
			Factory.Save();

			expected = new[]
			{
				new AssertionCharge { ChargeCode = "OAQF", JR_LocalSellAmt = 200m }
			};

			AutorateAndAssert("Only OAQF is applicable", expected, shipment, Consignee);
		}

		public void TestRemoveCompanyTariffsIrrelevantLinesBasedOnClientRate_WhenClientRateWithEXLCalculatorButConditionIsNotMet_ThenCompanyTariffShouldNotBeFiltered()
		{
			var client = Helper.NewOrgHeader(1);

			var tariff = Helper.NewCompanyTariff();
			tariff.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE", "FRT", 200);

			var clientRate = Helper.NewClientRate(client);
			var clientRateEntry = clientRate.AddRateEntry(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUBNE", removeLines: true);
			var clientRateLine = clientRateEntry.AddFlatRateLine("FRT", 200);
			clientRateLine.TL_Condition = RateLineConditions.UserDefined;
			clientRateLine.TL_ConditionalExpression = "\"<Consignee.OH_Code>\"==\"VBCIMPHAM\"";

			tariff.Factory.Save();
			Factory.Save();

			var testObject = new AutoRatingObject("USLAX", "AUBNE", FreightMode.LSE, null, 0m, 0m, client);
			testObject.Consignor = Consignor;
			testObject.Consignee = Consignee;
			var autoRater = new FreightAutoRater(new RatingContext());
			var results = autoRater.AutoRate(new AutoRatingProxy(testObject), CostSell.Revenue);

			AssertRatingResults
			(
				"Given: Company Tariff has RateLine with charge code 'FRT'. Client Rate has RateLine with charge code 'FRT' and Exclude Company Tariff Calculator, but its condition is not met. When do autoRating, then the rate in Company Tariff should be applied.",
				new[]
				{
					new SimpleArInfo
					{
						InvoiceLineDesc = "International Freight",
						Amount = 200m,
						CalculationSingleLineDescription = "FRT: Base Rate USD 200.00",
					}
				},
				results
			);
		}

		#endregion
	}
}
