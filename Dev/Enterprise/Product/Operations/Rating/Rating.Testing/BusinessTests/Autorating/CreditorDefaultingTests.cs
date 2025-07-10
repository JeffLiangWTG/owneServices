using System;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Rating.Business;
using Enterprise.Registry.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.RatingTests.Testing.GUI
{
	public class CreditorDefaultingTests : BaseRatingIntegrationTest
	{
		#region Default Properties

		protected static ZString OverseasPort
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "SGSIN")
				{
					return "SGSIN";
				}

				return "USLAX";
			}
		}

		protected static ZString OverseasPort2
		{
			get
			{
				if (GlbBranch.CurrentBranch.GB_RL_NKHomePort != "HKHKG")
				{
					return "HKHKG";
				}

				return "THBKK";
			}
		}

		#endregion

		#region Charge Creditor Defaulting

		public void TestChargeCreditorDefaulting_AutorateAgentConsol_WhenCarrierAndCreditorProvided_ChargeCreditorSetToCarrierImportCreditor()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());
			AssertEquals("We should set import creditor when we update creditor for import consol", creditor.PK, consol.CarrierImportCreditorAddress.OrganisationPK);

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK },
				new AssertionCost { ChargeCode = "CAF", E6_LocalCostAmount = 201m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 101m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK }
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateAgentConsol_WhenCarrierAndCreditorProvided_NoFallbackToAgency()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			_ = NewAgencyWithCost("AGENCY1", "AUSYD", creditor);
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));
			_ = NewAgencyWithCost("AGENCY2", "AUSYD", carrier);

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			AssertEquals("We should set import creditor when we update creditor for import consol", creditor.PK, consol.CarrierImportCreditorAddress.OrganisationPK);

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK }, // based on CostsProviderComparer, Consol>Creditor takes priority over Consol>Carrier
				new AssertionCost { ChargeCode = "CAF", E6_LocalCostAmount = 201m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 101m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = consol.CarrierImportCreditorAddress.OrganisationPK }
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateAgentConsol_WhenCarrierProvided()
		{
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100));

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m, E6_OH_Creditor = carrier.PK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = carrier.PK },
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateAgentConsol_WhenCarrierProvided_NoFallbackToAgencyIfAvailable()
		{
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100));
			_ = NewAgencyWithCost("AGENCY", "AUSYD", carrier);

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			Factory.Save();

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.CreditorPK = ZGuid.Empty;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m, E6_OH_Creditor = carrier.PK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = carrier.PK }
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadConsol_WhenCarrierAndCreditorProvided()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m, E6_OH_Creditor = creditor.PK }, // based on CostsProviderComparer, Consol>Creditor takes priority over Consol>Carrier
				new AssertionCost { ChargeCode = "CAF", E6_LocalCostAmount = 201m, E6_OH_Creditor = creditor.PK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 101m, E6_OH_Creditor = carrier.PK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = creditor.PK },
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadConsol_WhenCarrierAndCreditorProvided_NoFallbackToAgencyIfAvailable()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			_ = NewAgencyWithCost("AGENCY1", "AUSYD", creditor);
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));
			_ = NewAgencyWithCost("AGENCY2", "AUSYD", carrier);

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m, E6_OH_Creditor = creditor.PK }, // based on CostsProviderComparer, Consol>Creditor takes priority over Consol>Carrier
				new AssertionCost { ChargeCode = "CAF", E6_LocalCostAmount = 201m, E6_OH_Creditor = creditor.PK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 101m, E6_OH_Creditor = carrier.PK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = creditor.PK },
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadConsol_WhenCarrierProvided()
		{
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100));

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m, E6_OH_Creditor = carrier.PK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = carrier.PK },
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadConsol_WhenCarrierProvided_NoFallbackToAgencyIfAvailable()
		{
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100));
			_ = NewAgencyWithCost("AGENCY", "AUSYD", carrier);

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_PrepaidCollect = PaymentType.Collect;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 100m, E6_OH_Creditor = carrier.PK },
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = carrier.PK },
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadPrepaidConsol_WhenCarrierAndCreditorProvided_SendingAgentAsAgency()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			var sendingAgent = NewAgencyWithCost("SENDINGAGENT", "AUSYD", creditor);
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m, E6_OH_Creditor = creditor.PK }, //based on WI00489878 - !CR7 DHL: Autorating Cost for Co - Load Export / Import Consol, creditor takes priority over carrier
				new AssertionCost { ChargeCode = "CAF", E6_LocalCostAmount = 201m, E6_OH_Creditor = creditor.PK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 101m, E6_OH_Creditor = carrier.PK },  //based on WI00489878, NO Carrier’s Organisation > Details > Related Parties, should fallback to carrier
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = creditor.PK }
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadPrepaidConsol_WhenCarrierAndCreditorProvided_SendingAgentNotAgency()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));
			var sendingAgent = NewCreditorWithCost("SENDINGAGENT");

			Helper.NewCosting(null).AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "USLAX", "AUSYD", "WAR", 500);

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, "USLAX", "AUSYD", 1000m);
			var consol = shipment.Consols.AddNew();
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 200m, E6_OH_Creditor = creditor.PK }, //based on WI00489878 - !CR7 DHL: Autorating Cost for Co - Load Export / Import Consol, creditor takes priority over carrier
				new AssertionCost { ChargeCode = "CAF", E6_LocalCostAmount = 201m, E6_OH_Creditor = creditor.PK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 101m, E6_OH_Creditor = carrier.PK }, //based on WI00489878, NO Carrier’s Organisation > Details > Related Parties, should fallback to carrier
				new AssertionCost { ChargeCode = "WAR", E6_LocalCostAmount = 500m, E6_OH_Creditor = creditor.PK },
			};

			AutoCostAndAssert("", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AgentExortConsol_ManualSetting()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			AssertEquals("We should set export creditor when we update creditor for import consol", creditor.PK, consol.CarrierExportCreditorAddress.OrganisationPK);
			var costCollection = consol.GetApportionments().CostsCollection;
			Factory.Save();

			var cost = costCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Helper.ChargeCodes["FRT"].PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			AssertEquals("Should be export creditor address", cost.E6_OH_Creditor, consol.CarrierExportCreditorAddress.OrganisationPK);

			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			var cost1 = costCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost1.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_OSCostAmount = 10m;
			cost1.E6_LocalCostAmount = 10m;

			AssertEquals("No export address, should fallback to carrier", cost1.E6_OH_Creditor, carrier.PK);
		}

		public void TestChargeCreditorDefaulting_AgentImportConsol_ManualSetting()
		{
			var creditor = NewCreditorWithCost("CREDITOR", ("FRT", 200), ("CAF", 201));
			var carrier = NewCreditorWithCost("CARRIER", ("FRT", 100), ("BAF", 101));

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

			Factory.Save();

			AssertEquals("We should set import creditor when we update creditor for import consol", creditor.PK, consol.CarrierImportCreditorAddress.OrganisationPK);
			var costCollection = consol.GetApportionments().CostsCollection;
			Factory.Save();

			var cost = costCollection.TryAddNew();
			cost.E6_AC_ChargeCode = Helper.ChargeCodes["FRT"].PK;
			cost.E6_GC = GlbCompany.CurrentCompany.PK;
			cost.E6_OSCostAmount = 10m;
			cost.E6_LocalCostAmount = 10m;

			AssertEquals("Should be import creditor address", cost.E6_OH_Creditor, consol.CarrierImportCreditorAddress.OrganisationPK);

			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			var cost1 = costCollection.TryAddNew();
			cost1.E6_AC_ChargeCode = Env.Registry.FreightChargeCode;
			cost1.E6_GC = GlbCompany.CurrentCompany.PK;
			cost1.E6_OSCostAmount = 10m;
			cost1.E6_LocalCostAmount = 10m;

			AssertEquals("No import address, should fallback to carrier", cost1.E6_OH_Creditor, carrier.PK);
		}

		#region Cross Trade Autorating Common Test Methods

		ForwardingConsol GetCrossTradeConsolWithShipment(bool isCoload)
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var routerCarrier = Factory.NewWithValidTestData<OrgHeader>();
			routerCarrier.OH_IsCreditor = true;

			var routerCreditor = Factory.NewWithValidTestData<OrgHeader>();
			routerCreditor.OH_IsCreditor = true;

			var shipment = CreateForwardingShipment(TransportModes.Sea, creditor.PK, Consignee.PK, OverseasPort, OverseasPort2, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, OverseasPort, OverseasPort2, creditor, shipment, PaymentType.Prepaid);

			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = isCoload ? AgentType.CoLoad : AgentType.Agent;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.CarrierImportCreditorAddress.E2_OA_Address = creditor.MainAddress.PK;
			consol.Transports[0].JW_TransportMode = TransportModes.Sea;
			consol.Transports[0].JW_OA_CreditorAddress = routerCreditor.MainAddress.PK;
			consol.Transports[0].JW_OA_CarrierAddress = routerCarrier.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: Cross Trade Consol expected.", true, consol.IsCrossTrade());
			AssertEquals("Preconditions: Cross Trade Consol is mode", isCoload, consol.IsCoLoad);

			return consol;
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol(ForwardingConsol consol, bool isMultiRouteAutoCostingEnabled, OrgHeader rateProvider, ZGuid expectedChargeCreditor)
		{
			var costing = Helper.NewCosting(rateProvider);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, OverseasPort, OverseasPort2);
			rateEntry.RateLines.RemoveAndDeleteAll();

			var chargeCode = Helper.ChargeCodes.NewConsolChargeCode("CC1", "CrossTradeChargeCode", FlatCalculator.Code);
			var rateLine = rateEntry.AddRateLine(chargeCode.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			Factory.Save();

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, isMultiRouteAutoCostingEnabled))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = expectedChargeCreditor
					}
				};

				AutoCostAndAssert("Charge Creditor Defaulting Test When Autorate For Cross Trade Consol", null, expectedCosts, consol, false);
			}
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);

			AssertEquals("Consol Creditor is same as Creditor Import Creditor", consol.JK_OA_CreditorAddress, consol.CarrierImportCreditorAddress.E2_OA_Address);

			OrgHeader rateProvider = consol.CarrierImportCreditorAddress.Organisation;
			ZGuid expectedChargeCreditor = consol.CarrierImportCreditorAddress.OrganisationPK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditorAndItIsDifferentFromCarrier_ThenChargeCreditorIsCarrierImportCreditor(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);

			var carrierImport = Factory.NewWithValidTestData<OrgHeader>();
			carrierImport.OH_IsCreditor = true;
			consol.CarrierImportCreditorAddress.E2_OA_Address = carrierImport.MainAddress.PK;

			Factory.Save();

			AssertNotEquals("Consol Creditor is not the same as Creditor Import Creditor", consol.JK_OA_CreditorAddress, consol.CarrierImportCreditorAddress.E2_OA_Address);

			OrgHeader rateProvider = consol.CarrierImportCreditorAddress.Organisation;
			ZGuid expectedChargeCreditor = consol.CarrierImportCreditorAddress.OrganisationPK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromConsolCarrier_ThenChargeCreditorIsConsolCarrier(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);

			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			Factory.Save();

			AssertEquals("Creditor Import Creditor is blank", consol.CarrierImportCreditorAddress.E2_OA_Address, ZGuid.Empty);

			OrgHeader rateProvider = isCoload ? consol.Creditor : consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader;
			ZGuid expectedChargeCreditor = isCoload ? consol.Creditor.PK : consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromConsolCarrier_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(false);

			AssertNotSame("Creditor Import Creditor is not blank", consol.CarrierImportCreditorAddress.E2_OA_Address, ZGuid.Empty);

			OrgHeader rateProvider = isCoload ? consol.Creditor : consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader;
			ZGuid expectedChargeCreditor = consol.CarrierImportCreditorAddress.OrganisationPK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromColoadWith_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsColoadWith(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(false);

			AssertNotSame("Creditor Import Creditor is not blank", consol.CarrierImportCreditorAddress.E2_OA_Address, ZGuid.Empty);

			OrgHeader rateProvider = consol.Creditor;
			ZGuid expectedChargeCreditor = consol.Creditor.PK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCreditor_ThenChargeCreditorIsRoutingCreditor(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);

			OrgHeader rateProvider = consol.Transports[0].CreditorAddress.Header;
			ZGuid expectedChargeCreditor = consol.Transports[0].CreditorAddress.Header.PK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_ThenChargeCreditorIsRoutingCarrier(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);

			consol.Transports[0].JW_OA_CreditorAddress = ZGuid.Empty;
			Factory.Save();

			AssertEquals("Routing Creditor is blank", consol.Transports[0].CreditorPK, ZGuid.Empty);

			OrgHeader rateProvider = consol.Transports[0].CarrierAddress.Header;
			ZGuid expectedChargeCreditor = consol.Transports[0].CarrierAddress.Header.PK;

			if (isMultiRouteAutoCostingEnabled)
			{
				TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
			}
			else
			{
				using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AutoCostAndAssert("When MultiModalRatingCost Disabled, No costs were found due to current RateHeader search limitation when autorating.", null, null, consol, false);
				}
			}
		}

		void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_AndWhenRoutingCreditorNotBlank_ThenChargeCreditorIsRoutingCreditor(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);

			AssertNotSame("Routing Creditor is not blank", consol.Transports[0].CreditorPK, ZGuid.Empty);

			OrgHeader rateProvider = consol.Transports[0].CarrierAddress.Header;
			ZGuid expectedChargeCreditor = consol.Transports[0].CreditorPK;

			if (isMultiRouteAutoCostingEnabled)
			{
				TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
			}
			else
			{
				using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					AutoCostAndAssert("When MultiModalRatingCost Disabled, No costs were found due to current RateHeader search limitation when autorating.", null, null, consol, false);
				}
			}
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_ThenChargeCreditorIsConsolCarrier(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);
			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			Factory.Save();

			AssertEquals("Creditor Import Creditor is blank", consol.CarrierImportCreditorAddress.E2_OA_Address, ZGuid.Empty);

			OrgHeader rateProvider = null;
			ZGuid expectedChargeCreditor = isCoload ? consol.Creditor.PK : consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader.PK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(bool isCoload, bool isMultiRouteAutoCostingEnabled)
		{
			var consol = GetCrossTradeConsolWithShipment(isCoload);

			AssertNotSame("Creditor Import Creditor is not blank", consol.CarrierImportCreditorAddress.E2_OA_Address, ZGuid.Empty);

			OrgHeader rateProvider = null;
			ZGuid expectedChargeCreditor = isCoload ? consol.Creditor.PK : consol.CarrierImportCreditorAddress.OrganisationPK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, isMultiRouteAutoCostingEnabled, rateProvider, expectedChargeCreditor);
		}

		#endregion

		#region Cross-Trade Non-Coload Autorating Tests Registry No

		public void TestChargeCreditorDefaulting_AutorateCrossTradeCoload_MultiRouteDisabled_WhenRateFromCarrierImportCreditorAndItIsDifferentFromCarrier_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditorAndItIsDifferentFromCarrier_ThenChargeCreditorIsCarrierImportCreditor(true, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeCoload_MultiRouteEnabled_WhenRateFromCarrierImportCreditorAndItIsDifferentFromCarrier_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditorAndItIsDifferentFromCarrier_ThenChargeCreditorIsCarrierImportCreditor(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor(false, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromConsolCarrier_ThenChargeCreditorIsConsolCarrier()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromConsolCarrier_ThenChargeCreditorIsConsolCarrier(false, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromConsolCarrier_AndWhenCreditorImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromConsolCarrier_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(false, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromRoutingCreditor_ThenChargeCreditorIsRoutingCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCreditor_ThenChargeCreditorIsRoutingCreditor(false, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromRoutingCarrier_ThenChargeCreditorIsRoutingCarrier()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_ThenChargeCreditorIsRoutingCarrier(false, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromRoutingCarrier_AndWhenRoutingCreditorNotBlank_ThenChargeCreditorIsRoutingCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_AndWhenRoutingCreditorNotBlank_ThenChargeCreditorIsRoutingCreditor(false, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromGeneralCosting_ThenChargeCreditorIsConsolCarrier()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_ThenChargeCreditorIsConsolCarrier(false, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteDisabled_WhenRateFromGeneralCosting_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(false, false);
		}

		#endregion

		#region Cross-Trade Non-Coload Autorating Tests Registry Yes

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteEnabled_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor(false, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteEnabled_WhenRateFromRoutingCreditor_ThenChargeCreditorIsRoutingCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCreditor_ThenChargeCreditorIsRoutingCreditor(false, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteEnabled_WhenRateFromRoutingCarrier_ThenChargeCreditorIsRoutingCarrier()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_ThenChargeCreditorIsRoutingCarrier(false, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteEnabled_WhenRateFromRoutingCarrier_AndWhenRoutingCreditorNotBlank_ThenChargeCreditorIsRoutingCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_AndWhenRoutingCreditorNotBlank_ThenChargeCreditorIsRoutingCreditor(false, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteEnabled_WhenRateFromGeneralCosting_ThenChargeCreditorIsConsolCarrier()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_ThenChargeCreditorIsConsolCarrier(false, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeNonCoload_MultiRouteEnabled_WhenRateFromGeneralCosting_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(false, true);
		}

		#endregion

		#region Cross-Trade Coload Autorating Tests Registry No

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteDisabled_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor(true, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteDisabled_WhenRateFromCoLoadWith_ChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromConsolCarrier_ThenChargeCreditorIsConsolCarrier(true, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteDisabled_WhenRateFromCoLoadWith_AndWhenCreditorImportCreditorNotBlank_ThenChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromColoadWith_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsColoadWith(true, false);
		}

		void AddRelatedPartyRecord(string transportMode, string containerMode, OrgRelatedPartyCompanySpecificCollection relatedParties, OrgHeader creditor, ZString freightDirection, ZString location)
		{
			var partyRecord = relatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = freightDirection;
			partyRecord.PR_OH_RelatedParty = creditor.PK;
			partyRecord.PR_FreightTransportMode = transportMode;
			partyRecord.PR_FreightContainerMode = containerMode;
			partyRecord.PR_Location = location;
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteDisabled_WhenRateFromConsolCarrier_ChargeCreditorIsRelatedPartyFallbackCarrier()
		{
			var consol = GetCrossTradeConsolWithShipment(true);
			var carrier = consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader;

			var relatedParties = carrier.AllRelatedParties;

			var creditorForPad = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPad.OH_IsCreditor = true;

			AddRelatedPartyRecord(consol.TransportMode, consol.JK_ConsolMode, relatedParties, creditorForPad, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_RL_NKDischargePort);

			Factory.Save();

			OrgHeader rateProvider = consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader;
			ZGuid expectedChargeCreditor = creditorForPad.PK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, false, rateProvider, expectedChargeCreditor);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteDisabled_WhenRateFromConsolCarrier_ChargeCreditorIsRelatedPartyFallbackDLVCarrier()
		{
			var consol = GetCrossTradeConsolWithShipment(true);
			var carrier = consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader;

			var relatedParties = carrier.AllRelatedParties;

			var creditorForDlv = Factory.NewWithValidTestData<OrgHeader>();
			creditorForDlv.OH_IsCreditor = true;

			var creditorForPad = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPad.OH_IsCreditor = true;

			var creditorForPickup = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPickup.OH_IsCreditor = true;

			AddRelatedPartyRecord(consol.TransportMode, consol.JK_ConsolMode, relatedParties, creditorForDlv, RelatedPartyDirectionList.Codes.Delivery, consol.JK_RL_NKDischargePort);
			AddRelatedPartyRecord(consol.TransportMode, consol.JK_ConsolMode, relatedParties, creditorForPad, RelatedPartyDirectionList.Codes.PickupAndDelivery, consol.JK_RL_NKDischargePort);
			AddRelatedPartyRecord(consol.TransportMode, consol.JK_ConsolMode, relatedParties, creditorForPickup, RelatedPartyDirectionList.Codes.Pickup, consol.JK_RL_NKDischargePort);

			Factory.Save();

			OrgHeader rateProvider = consol.JK_OA_ShippingLineAddress_ZAddress.OrgHeader as OrgHeader;
			ZGuid expectedChargeCreditor = creditorForDlv.PK;

			TestChargeCreditorDefaulting_AutorateCrossTradeConsol(consol, false, rateProvider, expectedChargeCreditor);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteDisabled_WhenRateFromGeneralCosting_ThenChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_ThenChargeCreditorIsConsolCarrier(true, false);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteDisabled_WhenRateFromGeneralCosting_AndWhenCreditorImportCreditorNotBlank_ThenChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(true, false);
		}

		#endregion

		#region Cross-Trade Coload Autorating Tests Registry Yes

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromCarrierImportCreditor_ThenChargeCreditorIsCarrierImportCreditor(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromCoLoadWith_ChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromConsolCarrier_ThenChargeCreditorIsConsolCarrier(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromCoLoadWith_AndWhenCreditorImportCreditorNotBlank_ThenChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromConsolCarrier_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromRoutingCreditor_ThenChargeCreditorIsRoutingCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCreditor_ThenChargeCreditorIsRoutingCreditor(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromRoutingCarrier_ThenChargeCreditorIsRoutingCarrier()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_ThenChargeCreditorIsRoutingCarrier(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromRoutingCarrier_AndWhenRoutingCreditorNotBlank_ThenChargeCreditorIsRoutingCreditor()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromRoutingCarrier_AndWhenRoutingCreditorNotBlank_ThenChargeCreditorIsRoutingCreditor(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromGeneralCosting_ThenChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_ThenChargeCreditorIsConsolCarrier(true, true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrossTradeColoadConsol_MultiRouteEnabled_WhenRateFromGeneralCosting_AndWhenCreditorImportCreditorNotBlank_ThenChargeCreditorIsCoLoadWith()
		{
			TestChargeCreditorDefaulting_AutorateCrossTradeConsol_WhenRateFromGeneralCosting_AndWhenCarrierImportCreditorNotBlank_ThenChargeCreditorIsCarrierImportCreditor(true, true);
		}

		#endregion

		#region export departure

		public void TestChargeCreditorDefaulting_AutorateExportConsol_WhenDepartureCtoIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Factory.Save();

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctoOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(ctoOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_DepartureCTOAddress = ctoOrg.MainAddress.PK;

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = ctoOrg.PK }
			};

			AutoCostAndAssert("No related party, creditor should be CTO (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(ctoOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPIC = AddNewRelatedPartyToOrganisation(ctoOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPIC.PK }
			};

			AutoCostAndAssert("PIC related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateExportConsol_WhenDepartureCfsIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Factory.Save();

			var cfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			cfsOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(cfsOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_PackDepotAddress = cfsOrg.MainAddress.PK;

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = cfsOrg.PK }
			};

			AutoCostAndAssert("No related party, creditor should be CFS (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(cfsOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPIC = AddNewRelatedPartyToOrganisation(cfsOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPIC.PK }
			};

			AutoCostAndAssert("PIC related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateExportConsol_WhenDepartureTransportIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Factory.Save();

			var portTransportOrg = Factory.NewWithValidTestData<OrgHeader>();
			portTransportOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(portTransportOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_DeparturePackCFSTransportAddress = portTransportOrg.MainAddress.PK;

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = portTransportOrg.PK }
			};

			AutoCostAndAssert("No related party, creditor should be Port Transport (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(portTransportOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPIC = AddNewRelatedPartyToOrganisation(portTransportOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPIC.PK }
			};

			AutoCostAndAssert("PIC related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		#region import Arrival

		public void TestChargeCreditorDefaulting_AutorateImportConsol_WhenArrivalCtoIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			var ctoOrg = Factory.NewWithValidTestData<OrgHeader>();
			ctoOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(ctoOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_ArrivalCTOAddress = ctoOrg.MainAddress.PK;

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = ctoOrg.PK }
			};

			AutoCostAndAssert("No related party, creditor should be CTO (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(ctoOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyDLV = AddNewRelatedPartyToOrganisation(ctoOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyDLV.PK }
			};

			AutoCostAndAssert("DLV related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateImportConsol_WhenArrivalCfsIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			var cfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			cfsOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(cfsOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_UnpackDepotAddress = cfsOrg.MainAddress.PK;

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = cfsOrg.PK }
			};

			AutoCostAndAssert("No related party, creditor should be CFS (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(cfsOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyDLV = AddNewRelatedPartyToOrganisation(cfsOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyDLV.PK }
			};

			AutoCostAndAssert("DLV related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateImportConsol_WhenArrivalTransportIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			var portTransportOrg = Factory.NewWithValidTestData<OrgHeader>();
			portTransportOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(portTransportOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = portTransportOrg.MainAddress.PK;

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = portTransportOrg.PK }
			};

			AutoCostAndAssert("No related party, creditor should be Transport (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(portTransportOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyDLV = AddNewRelatedPartyToOrganisation(portTransportOrg, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyDLV.PK }
			};

			AutoCostAndAssert("DLV related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		#region Domestic Consol Departure/Arriavl

		public void TestChargeCreditorDefaulting_AutorateDomesticConsol_WhenDepartureArrivalCtoAreProviders_ShouldBeProvider()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			Factory.Save();

			var departureCtoOrg = Factory.NewWithValidTestData<OrgHeader>();
			departureCtoOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(departureCtoOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			var arrivalCtoOrg = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCtoOrg.OH_IsCreditor = true;
			var costingArrival = Helper.NewCosting(arrivalCtoOrg);
			costingArrival.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_DepartureCTOAddress = departureCtoOrg.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = arrivalCtoOrg.MainAddress.PK;

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = departureCtoOrg.PK },
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = arrivalCtoOrg.PK }
			};

			AutoCostAndAssert("creditor should be CTO (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateDomesticConsol_WhenDepartureArrivalCfsAreProviders_ShouldBeProvider()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			Factory.Save();

			var departureCfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			departureCfsOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(departureCfsOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			var arrivalCfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCfsOrg.OH_IsCreditor = true;
			var costingArrival = Helper.NewCosting(arrivalCfsOrg);
			costingArrival.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_PackDepotAddress = departureCfsOrg.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = arrivalCfsOrg.MainAddress.PK;

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = departureCfsOrg.PK },
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = arrivalCfsOrg.PK }
			};

			AutoCostAndAssert("creditor should be CFS (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateDomesticConsol_WhenDepartureArrivalTransportAreProviders_ShouldBeProvider()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			Factory.Save();

			var transport = Factory.NewWithValidTestData<OrgHeader>();
			transport.OH_IsCreditor = true;
			var costing = Helper.NewCosting(transport);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			var arrivalTransport = Factory.NewWithValidTestData<OrgHeader>();
			arrivalTransport.OH_IsCreditor = true;
			var costingArrival = Helper.NewCosting(arrivalTransport);
			costingArrival.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_DeparturePackCFSTransportAddress = transport.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransport.MainAddress.PK;

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = transport.PK },
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = arrivalTransport.PK }
			};

			AutoCostAndAssert("creditor should be Port Transport (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		#region Crosstrade Consol Departure/Arriavl

		public void TestChargeCreditorDefaulting_AutorateCrosstradeConsol_WhenDepartureArrivalCtoAreProviders_ShouldBeProvider()
		{
			var origin = "HKHKG";
			var destination = "USLAX";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			Factory.Save();

			var departureCtoOrg = Factory.NewWithValidTestData<OrgHeader>();
			departureCtoOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(departureCtoOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			var arrivalCtoOrg = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCtoOrg.OH_IsCreditor = true;
			var costingArrival = Helper.NewCosting(arrivalCtoOrg);
			costingArrival.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_DepartureCTOAddress = departureCtoOrg.MainAddress.PK;
			consol.JK_OA_ArrivalCTOAddress = arrivalCtoOrg.MainAddress.PK;

			AssertEquals("Preconditions: Crosstrade consol expected.", true, consol.IsCrossTrade());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = departureCtoOrg.PK },
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = arrivalCtoOrg.PK }
			};

			AutoCostAndAssert("creditor should be CTO (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrosstradeConsol_WhenDepartureArrivalCfsAreProviders_ShouldBeProvider()
		{
			var origin = "HKHKG";
			var destination = "USLAX";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			Factory.Save();

			var departureCfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			departureCfsOrg.OH_IsCreditor = true;
			var costing = Helper.NewCosting(departureCfsOrg);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			var arrivalCfsOrg = Factory.NewWithValidTestData<OrgHeader>();
			arrivalCfsOrg.OH_IsCreditor = true;
			var costingArrival = Helper.NewCosting(arrivalCfsOrg);
			costingArrival.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_PackDepotAddress = departureCfsOrg.MainAddress.PK;
			consol.JK_OA_UnpackDepotAddress = arrivalCfsOrg.MainAddress.PK;

			AssertEquals("Preconditions: Crosstrade consol expected.", true, consol.IsCrossTrade());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = departureCfsOrg.PK },
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = arrivalCfsOrg.PK }
			};

			AutoCostAndAssert("creditor should be CFS (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrosstradeConsol_WhenDepartureArrivalTransportAreProvider_ShouldBeProvider()
		{
			var origin = "HKHKG";
			var destination = "USLAX";

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);

			Factory.Save();

			var transport = Factory.NewWithValidTestData<OrgHeader>();
			transport.OH_IsCreditor = true;
			var costing = Helper.NewCosting(transport);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.AIR, origin, "", "CLORG", 210);

			var arrivalTransport = Factory.NewWithValidTestData<OrgHeader>();
			arrivalTransport.OH_IsCreditor = true;
			var costingArrival = Helper.NewCosting(arrivalTransport);
			costingArrival.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.AIR, "", destination, "CLDST", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_DeparturePackCFSTransportAddress = transport.MainAddress.PK;
			consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrivalTransport.MainAddress.PK;

			AssertEquals("Preconditions: Crosstrade consol expected.", true, consol.IsCrossTrade());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = transport.PK },
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 210m, E6_OH_Creditor = arrivalTransport.PK }
			};

			AutoCostAndAssert("creditor should be Port Transport (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		#region sending/receiving agent

		public void TestChargeCreditorDefaulting_AutorateExportConsol_WhenReceivingAgentIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "AUSYD";
			var destination = "USLAX";

			var receivinggAgent = Helper.NewOrgHeader();
			receivinggAgent.OH_IsCreditor = true;
			var costing = Helper.NewCosting(receivinggAgent);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "", destination, "FRT", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_ReceivingForwarderAddress = receivinggAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals("Preconditions: export consol expected.", true, consol.IsExport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 210m, E6_OH_Creditor = receivinggAgent.PK }
			};

			AutoCostAndAssert("No related party, creditor should be receiving agent (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(receivinggAgent, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPIC = AddNewRelatedPartyToOrganisation(receivinggAgent, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPIC.PK }
			};

			AutoCostAndAssert("PIC related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateImportConsol_WhenSendingAgentIsProvider_ShouldBringSPCRelatedPartyFallBackToProvider()
		{
			var origin = "USLAX";
			var destination = "AUSYD";

			var sendingAgent = Helper.NewOrgHeader();
			sendingAgent.OH_IsCreditor = true;
			var costing = Helper.NewCosting(sendingAgent);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "", destination, "FRT", 210);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 210m, E6_OH_Creditor = sendingAgent.PK }
			};

			AutoCostAndAssert("No related party, creditor should be sending agent (rate provider)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(sendingAgent, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyPAD.PK }
			};

			AutoCostAndAssert("PAD related party, creditor should be PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			var relatedPartyDLV = AddNewRelatedPartyToOrganisation(sendingAgent, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Delivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);

			expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 210m, E6_OH_Creditor = relatedPartyDLV.PK }
			};

			AutoCostAndAssert("DLV related party, creditor should overtake PAD", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateCrosstradeConsol_WhenSendingReceivingAgentsAreProvider_ShouldBeProviders()
		{
			var origin = "HKHKG";
			var destination = "USLAX";

			var sendingAgent = Helper.NewOrgHeader();
			sendingAgent.OH_IsCreditor = true;
			var costing = Helper.NewCosting(sendingAgent);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "", destination, "FRT", 210);

			var receivingAgent = Helper.NewOrgHeader();
			receivingAgent.OH_IsCreditor = true;
			var costingRecevinh = Helper.NewCosting(receivingAgent);
			costingRecevinh.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.AIR, RateMode.LSE, "", destination, "BAF", 310);

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals("Preconditions: Crosstrade consol expected.", true, consol.IsCrossTrade());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "FRT", E6_LocalCostAmount = 210m, E6_OH_Creditor = sendingAgent.PK },
				new AssertionCost { ChargeCode = "BAF", E6_LocalCostAmount = 310m, E6_OH_Creditor = receivingAgent.PK }
			};

			AutoCostAndAssert("creditor should be sending/receiving agent (rate providers)", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
		}

		#endregion

		(ForwardingConsol forwardingConsol, ForwardingShipment ForwardingShipment) CreateForwardingConsolWithShipment(string origin, string destination)
		{
			var creditor = NewCarrierCreditor("CREDITOR");
			var carrier = NewCarrierCreditor("CARRIER");

			var shipment = CreateForwardingShipment(TransportModes.Air, Consignor.PK, Consignee.PK, origin, destination, 100m);
			var consol = CreateForwardingConsol(TransportModes.Air, origin, destination, TransportProvider1, shipment);
			consol.JK_AgentType = AgentType.Agent;
			consol.JK_PrepaidCollect = PaymentType.Prepaid;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			Factory.Save();

			return (consol, shipment);
		}

		OrgHeader AddNewRelatedPartyToOrganisation(OrgHeader organisation, string partyType, string freightDirection, string transportMode, string containerMode)
		{
			var relatedParty = Factory.NewWithValidTestData<OrgHeader>();
			relatedParty.OH_IsCreditor = true;

			var relatedParties = organisation.AllRelatedParties;
			var partyRecord = relatedParties.AddNew();
			partyRecord.PR_PartyType = partyType;
			partyRecord.PR_FreightDirection = freightDirection;
			partyRecord.PR_OH_RelatedParty = relatedParty.PK;
			partyRecord.PR_FreightTransportMode = transportMode;
			partyRecord.PR_FreightContainerMode = containerMode;

			Factory.Save();

			return relatedParty;
		}

		OrgHeader NewCreditorWithCost(string orgCode, params (string chargeCode, decimal amount)[] costs)
		{
			var orgHeader = Helper.NewOrgHeader(orgCode);
			orgHeader.OH_IsCreditor = true;

			if (!costs.IsNullOrEmpty())
			{
				var cost = Helper.NewCosting(orgHeader);
				var entry = cost.AddRateEntry(
					category: RatingConstants.RateCategory.AIR,
					mode: RateMode.LSE,
					origin: "USLAX",
					destination: "AUSYD");
				entry.RateLines.RemoveAndDeleteAll();

				foreach (var (chatgeCode, amount) in costs)
				{
					entry.AddFlatRateLine(chatgeCode, amount);
				}
			}

			return orgHeader;
		}

		OrgHeader NewAgencyWithCost(string agencyCode, string portOrCountry, OrgHeader parent, params (string chargeCode, decimal amount)[] costs)
		{
			var agency = NewCreditorWithCost(agencyCode, costs);

			var appointedAgentPorted = parent.CarrierAppointedAgentPorts_Agency.AddNew();
			appointedAgentPorted.O5_PortOrCountry = portOrCountry;
			appointedAgentPorted.O5_OA_AgentOfficeAddress = agency.MainAddress.PK;

			return agency;
		}

		#endregion

		#region Cross Trade
		public void TestAutoRatingCost_ForCoLoadImportConsol_ShouldBringCarrierImportCharges()
		{
			var origin = "NZAKL";
			var destination = "AUSYD";

			var importCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;

			importCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;

			Factory.Save();

			var costing = Helper.NewCosting(importCreditorAddress);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("FRT", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierImportCreditorAddress.E2_OA_Address = importCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: import consol expected.", true, consol.IsImport());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Import Creditor has value", importCreditorAddress.PK, consol.CarrierImportCreditor.PK);

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m
				}
			};

			AutoCostAndAssert("Should Bring Import Creditor charges", null, expectedCosts, consol, false);

			var costing1 = Helper.NewCosting(coLoadWith);
			var rateEntry1 = costing1.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry1.RateLines.RemoveAndDeleteAll();
			var rateLine1 = rateEntry1.AddRateLine("FRT", FlatCalculator.Code);
			rateLine1.GetCalculator<FlatCalculator>().BaseRate = 300m;

			var rateLine2 = rateEntry1.AddRateLine("CAF", FlatCalculator.Code);
			rateLine2.GetCalculator<FlatCalculator>().BaseRate = 400m;

			var costing2 = Helper.NewCosting(carrier);
			var rateEntry2 = costing2.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry2.RateLines.RemoveAndDeleteAll();
			var rateLine3 = rateEntry2.AddRateLine("BAF", FlatCalculator.Code);
			rateLine3.GetCalculator<FlatCalculator>().BaseRate = 500m;

			var relatedPartyForCarrier = Factory.NewWithValidTestData<OrgHeader>();
			relatedPartyForCarrier.OH_IsCreditor = true;

			var carrierRelatedParties = carrier.AllRelatedParties;
			var partyRecord = carrierRelatedParties.AddNew();
			partyRecord.PR_PartyType = RelatedPartyTypeList.Codes.ServiceProviderCreditor;
			partyRecord.PR_FreightDirection = RelatedPartyDirectionList.Codes.Delivery;
			partyRecord.PR_OH_RelatedParty = relatedPartyForCarrier.PK;
			partyRecord.PR_FreightTransportMode = consol.JK_TransportMode;
			partyRecord.PR_FreightContainerMode = consol.JK_ConsolMode;
			partyRecord.PR_Location = destination;

			var costing3 = Helper.NewCosting(null);
			var rateEntry3 = costing3.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry3.RateLines.RemoveAndDeleteAll();
			var rateLine4 = rateEntry3.AddRateLine("WAR", FlatCalculator.Code);
			rateLine4.GetCalculator<FlatCalculator>().BaseRate = 600m;

			Factory.Save();

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 200m,
					E6_OH_Creditor = importCreditorAddress.PK
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_OSCostAmount = 400m,
					E6_OH_Creditor = importCreditorAddress.PK //There is Carrier Import Creditor, so we should use it as creditor
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 500m,
					E6_OH_Creditor = relatedPartyForCarrier.PK // this is from carrier, creditor should be carrier’s Organisation > Details > Related Parties
				},
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = importCreditorAddress.PK //There is Carrier Import Creditor, so we should use it as creditor for general costing
				},
			};

			AutoCostAndAssert("Import Creditor takes priority over co-load with", null, expectedCosts, consol, false);
			AssertAutoratingAuditLogNoteContainsLines(consol, "Import Creditor should take priority over Co-load with", "Information: RateLine Filtered FRT-FLT-Costing TRASPROV2\treason:\toverridden by FRT-FLT-Costing TRASPROV1 by Creditors comparer");

			carrier.AllRelatedParties.RemoveAndDeleteAll();
			consol.CarrierImportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "FRT",
					E6_OSCostAmount = 300m,
					E6_OH_Creditor = coLoadWith.PK // no creditor import address, we should fallback to Co-Load With
				},
				new AssertionCost
				{
					ChargeCode = "CAF",
					E6_OSCostAmount = 400m,
					E6_OH_Creditor = coLoadWith.PK // no creditor import address, we should fallback to Co-Load With
				},
				new AssertionCost
				{
					ChargeCode = "BAF",
					E6_OSCostAmount = 500m,
					E6_OH_Creditor = carrier.PK // NO Carrier’s Organisation > Details > Related Parties with Direction = DLY/PAD, should fallback to carrier
				},
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = coLoadWith.PK //There is NO Carrier Import Creditor, so we should fall back to Co-Load With
				},
			};

			Factory.Save();

			AutoCostAndAssert("Defaulting creditor when there is no import creditor", null, expectedCosts, consol, false);
		}

		#endregion

		#region Test Charge Creditor Defaulting Autorate Domestic Not Coload Consol

		public void TestChargeCreditorDefaulting_AutorateDomesticNotCLDConsol_WhenCarrierExportCreditorIsProvider()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			exportCreditorAddress.OH_IsCreditor = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var costing = Helper.NewCosting(exportCreditorAddress);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, "", "CLORG", 210);

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, creditor, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.CreditorPK = creditor.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = exportCreditorAddress.PK },
			};

			AssertEquals("Preconditions: MultiRouteAutoCosting registry expected.", false, RatingDataRegistry.Instance.MultiModalRatingCost.Value);
			AutoCostAndAssert("Charge creditor should be Carrier Export Creditor when Rates’ Service Provider is Carrier Export Creditor.", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Preconditions: MultiRouteAutoCosting registry expected.", true, RatingDataRegistry.Instance.MultiModalRatingCost.Value);
				AutoCostAndAssert("Charge creditor should be Carrier Export Creditor when Rates’ Service Provider is Carrier Export Creditor.", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
			}
		}

		public void TestChargeCreditorDefaulting_AutorateDomesticNotCLDConsol_WhenCarrierIsProvider_WithMultiRouteAutoCostingRegistryIsFalse()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			exportCreditorAddress.OH_IsCreditor = true;
			var carrier = Consignor;
			carrier.OH_IsCreditor = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);

			var costing = Helper.NewCosting(carrier);
			costing.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, "", "CLORG", 210);

			Factory.Save();

			var (consol, _, container) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, creditor, PaymentType.Prepaid);
			consol.CreditorPK = creditor.PK;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = carrier.PK },
			};
			AssertEquals("Preconditions: MultiRouteAutoCosting registry expected.", false, RatingDataRegistry.Instance.MultiModalRatingCost.Value);
			AutoCostAndAssert("Charge creditor should be Carrier Export Creditor when Rates’ Service Provider is Carrier and MultiRouteAutoCosting registry is false if Carrier Export Creditor is empty.", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			Factory.Save();

			var expectedResult2 = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = exportCreditorAddress.PK },
			};

			AssertEquals("Preconditions: MultiRouteAutoCosting registry expected.", false, RatingDataRegistry.Instance.MultiModalRatingCost.Value);
			AutoCostAndAssert("Charge creditor should be Carrier Export Creditor when Rates’ Service Provider is Carrier Export Creditor if Carrier Export Creditor is not empty.", null, expectedResult2, consol, autorateRevenue: false, autorateCosts: true);
		}

		public void TestChargeCreditorDefaulting_AutorateDomesticNotCLDConsol_WhenRoutingCarrierIsProviderAndRoutingCreditorIsEmpty_WithMultiRouteAutoCostingRegistryIsYes()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var routingCarrier = Helper.NewOrgHeader("RCRD");
			routingCarrier.OH_IsCreditor = true;

			var routingCreditor = Helper.NewOrgHeader("TESTORG2");
			routingCreditor.OH_IsCreditor = true;

			var carrier = Consignor;
			carrier.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.ChargeCodes.NewConsolChargeCode("CLDST", "Consol Level Destination", FlatCalculator.Code, ChargeCodeGroupList.Codes.Destination);
			Factory.Save();

			Helper.NewCosting(routingCarrier)
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, destination, "CLORG", 210);
			Helper.NewCosting(routingCreditor)
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.DST, RateMode.ALL, origin, destination, "CLDST", 200);
			var (consol, _, _) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, creditor, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;

			consol.CreditorPK = ZGuid.Empty;
			var transport1 = consol.Transports[0];
			transport1.CarrierPK = routingCarrier.PK;
			transport1.JW_TransportMode = TransportModes.Air;

			Factory.Save();

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = routingCarrier.PK }
			};

			var expectedResult2 = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = routingCreditor.PK },
				new AssertionCost { ChargeCode = "CLDST", E6_LocalCostAmount = 200m, E6_OH_Creditor = routingCreditor.PK }
			};

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Preconditions: MultiRouteAutoCosting registry expected.", true, RatingDataRegistry.Instance.MultiModalRatingCost.Value);
				AutoCostAndAssert("Charge creditor should be Routing > Carrier when Rates’ Service Provider is Routing > Carrier and Routing > Creditor is empty.", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

				transport1.CreditorPK = routingCreditor.PK;
				Factory.Save();

				AutoCostAndAssert("Charge creditor should be Routing > Creditor when Rates’ Service Provider is Routing > Carrier and Routing > Creditor is not empty.", null, expectedResult2, consol, autorateRevenue: false, autorateCosts: true);
			}
		}

		public void TestChargeCreditorDefaulting_AutorateDomesticNotCLDConsol_WhenRoutingCreditorIsProvider()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var routeCreditor = Helper.NewOrgHeader("RCRD");
			routeCreditor.OH_IsCreditor = true;

			var carrier = Consignor;
			carrier.OH_IsCreditor = true;

			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			Helper.ChargeCodes.NewConsolChargeCode("CLORG", "Consol Level Origin", FlatCalculator.Code, ChargeCodeGroupList.Codes.Origin);
			Helper.NewCosting(routeCreditor)
				.AddRateEntryWithFlatRateLine(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, destination, "CLORG", 210);
			Factory.Save();

			var (consol, _, _) = CreateForwardingConsolWithShipmentAndContainer(origin, destination, creditor, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;

			consol.CreditorPK = ZGuid.Empty;
			var transport1 = consol.Transports[0];
			transport1.CreditorPK = routeCreditor.PK;
			transport1.JW_TransportMode = TransportModes.Air;

			Factory.Save();

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedResult = new[]
			{
				new AssertionCost { ChargeCode = "CLORG", E6_LocalCostAmount = 210m, E6_OH_Creditor = routeCreditor.PK },
			};

			AssertEquals("Preconditions: MultiRouteAutoCosting registry expected.", false, RatingDataRegistry.Instance.MultiModalRatingCost.Value);
			AutoCostAndAssert("Charge creditor should be Routing > Creditor when Rates’ Service Provider is Routing > Creditor.", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals("Preconditions: MultiRouteAutoCosting registry expected.", true, RatingDataRegistry.Instance.MultiModalRatingCost.Value);
				AutoCostAndAssert("Charge creditor should be Routing > Creditor when Rates’ Service Provider is Routing > Creditor.", null, expectedResult, consol, autorateRevenue: false, autorateCosts: true);
			}
		}

		//when provider is ZGuid.Empty, it is a manually entered charge or a charge in Standard Costing, otherwise it was created due to autorating.
		public void TestChargeCreditorDefaulting_AutorateDomesticNotCLDConsol_WhenProviderIsEmpty()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			exportCreditorAddress.OH_IsCreditor = true;
			var carrier = Consignor;
			carrier.OH_IsCreditor = true;
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			creditor.OH_IsCreditor = true;

			var costing = Helper.NewCosting(null);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.ORG, RateMode.ALL, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine("WAR", FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 600m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.CreditorPK = TransportProvider1.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;

			AssertEquals("Preconditions: Domestic consol expected.", true, consol.IsDomestic());

			var expectedCosts = new[]
			{
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = carrier.PK //There is Carrier Export Creditor, so we should use it as creditor for general costing
				},
			};
			Factory.Save();

			AutoCostAndAssert("Charge creditor should be Carrier when there is no export creditor", null, expectedCosts, consol, false);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert("Charge creditor should be Carrier when there is no export creditor", null, expectedCosts, consol, false);
			}

			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			var expectedCosts2 = new[]
			{
				new AssertionCost
				{
					ChargeCode = "WAR",
					E6_OSCostAmount = 600m,
					E6_OH_Creditor = exportCreditorAddress.PK //There is Carrier Export Creditor, so we should use it as creditor for general costing
				},
			};
			Factory.Save();

			AutoCostAndAssert("Charge creditor should be Carrier Export Creditor when there is an export creditor", null, expectedCosts2, consol, false);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AutoCostAndAssert("Charge creditor should be Carrier Export Creditor when there is an export creditor", null, expectedCosts2, consol, false);
			}
		}

		#endregion

		#region Test Charge Default Creditor After Autorating With Domestic And CoLoad Consol

		public void TestChargeCreditorDefaulting_AutorateCoLoadDomesticConsol_WhenCarrierExportCreditorIsProvider_ChargeCreditorSetToCarrierExportCreditor()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;

			exportCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Domestic", FlatCalculator.Code);

			Factory.Save();

			var costing = Helper.NewCosting(exportCreditorAddress);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Export Creditor has value", exportCreditorAddress.PK, consol.CarrierExportCreditor.PK);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = exportCreditorAddress.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Carrier Export Creditor, Charges Default Creditor become Carrier Export Creditor", null, expectedCosts, consol, false);
			}
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = exportCreditorAddress.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Carrier Export Creditor, Charges Default Creditor become Carrier Export Creditor", null, expectedCosts, consol, false);
			}
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadDomesticConsol_WhenCoLoadWithIsProvided_ChargeCreditorSetToCarrierExportCreditorFallbackToCoLoadWith()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;

			exportCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Domestic", FlatCalculator.Code);

			Factory.Save();

			var costing = Helper.NewCosting(coLoadWith);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Export Creditor has value", exportCreditorAddress.PK, consol.CarrierExportCreditor.PK);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = exportCreditorAddress.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is 'Co-Load With', Charges Default Creditor become Carrier Export Creditor", null, expectedCosts, consol, false);
			}
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = exportCreditorAddress.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is 'Co-Load With', Charges Default Creditor become Carrier Export Creditor", null, expectedCosts, consol, false);
			}
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = coLoadWith.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is 'Co-Load With', If Carrier Export Creditor is blank, Charges Default Creditor fallback to Co-Load With", null, expectedCosts, consol, false);
			}
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = coLoadWith.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is 'Co-Load With', If Carrier Export Creditor is blank, Charges Default Creditor fallback to Co-Load With", null, expectedCosts, consol, false);
			}
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadDomesticConsol_WhenCarrierIsProvider_ChargeCreditorSetToRelatedPartiesFallbackToCarrier()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;

			exportCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Domestic", FlatCalculator.Code);
			Factory.Save();

			var costing = Helper.NewCosting(carrier);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Export Creditor has value", exportCreditorAddress.PK, consol.CarrierExportCreditor.PK);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = carrier.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Carrier, And there is no party which party type = SPC with Direction = PIC/PAD in Carrier's related parties, Charges Default Creditor become Carrier", null, expectedCosts, consol, false);
			}

			var relatedPartyPAD = AddNewRelatedPartyToOrganisation(carrier, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.PickupAndDelivery,
				consol.JK_TransportMode, consol.JK_ConsolMode);
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = relatedPartyPAD.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Carrier, And there is party which party type = SPC with Direction = PAD in Carrier's related parties, Charges Default Creditor become related party which party direction = PAD", null, expectedCosts, consol, false);
			}

			var relatedPartyPIC = AddNewRelatedPartyToOrganisation(carrier, RelatedPartyTypeList.Codes.ServiceProviderCreditor, RelatedPartyDirectionList.Codes.Pickup,
				consol.JK_TransportMode, consol.JK_ConsolMode);
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = relatedPartyPIC.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Carrier, And there is party which party type = SPC with Direction = PIC in Carrier's related parties, Charges Default Creditor become related party which party direction = PIC", null, expectedCosts, consol, false);
			}
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadDomesticConsol_WhenStandardCostingIsProvider_ChargeCreditorSetToCarrierExportCreditorFallbackToCoLoadWith()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;

			exportCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Domestic", FlatCalculator.Code);

			Factory.Save();

			var costing = Helper.NewCosting(null);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;

			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Export Creditor has value", exportCreditorAddress.PK, consol.CarrierExportCreditor.PK);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = exportCreditorAddress.PK
					}
				};

				AutoCostAndAssert("When rate is standard costing, Charges Default Creditor become Carrier Export Creditor", null, expectedCosts, consol, false);
			}
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = exportCreditorAddress.PK
					}
				};

				AutoCostAndAssert("When rate is standard costing, Charges Default Creditor become Carrier Export Creditor", null, expectedCosts, consol, false);
			}
			consol.CarrierExportCreditorAddress.E2_OA_Address = ZGuid.Empty;
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = coLoadWith.PK
					}
				};

				AutoCostAndAssert("When rate is standard costing, If Carrier Export Creditor is blank, Charges Default Creditor fallback to Co-Load With", null, expectedCosts, consol, false);
			}
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = coLoadWith.PK
					}
				};

				AutoCostAndAssert("When rate is standard costing, If Carrier Export Creditor is blank, Charges Default Creditor fallback to Co-Load With", null, expectedCosts, consol, false);
			}
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadDomesticConsol_WhenRoutingCreditorIsProvider_ChargeCreditorSetToRoutingCreditor()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;
			var routingCreditor = NewClient;
			var receivingAgent = Helper.NewOrgHeader();

			exportCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;
			routingCreditor.OH_IsCreditor = true;
			receivingAgent.OH_IsCreditor = true;

			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Domestic", FlatCalculator.Code);
			Factory.Save();

			var costing = Helper.NewCosting(routingCreditor);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;
			var transport1 = consol.Transports[0];
			transport1.CreditorPK = routingCreditor.PK;
			transport1.JW_TransportMode = TransportModes.Sea;
			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Export Creditor has value", exportCreditorAddress.PK, consol.CarrierExportCreditor.PK);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = routingCreditor.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Routing Creditor, the charge defaulting creditor should become Routing Credtior", null, expectedCosts, consol, false);
			}

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = routingCreditor.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Routing Creditor, the charge defaulting creditor should become Routing Credtior", null, expectedCosts, consol, false);
			}
		}

		public void TestChargeCreditorDefaulting_AutorateCoLoadDomesticConsol_WhenRoutingCarrierIsProvider_ChargeCreditorSetToRoutingCreditorFallbackToRoutingCarrier()
		{
			var origin = "AUSYD";
			var destination = "AUMEL";

			var exportCreditorAddress = TransportProvider1;
			var coLoadWith = TransportProvider2;
			var carrier = Consignor;
			var routingCarrier = NewClient;
			var routingCreditor = Helper.NewOrgHeader("TESTORG");

			exportCreditorAddress.OH_IsCreditor = true;
			coLoadWith.OH_IsCreditor = true;
			carrier.OH_IsCreditor = true;
			routingCreditor.OH_IsCreditor = true;
			routingCarrier.OH_IsCreditor = true;
			var chargeCode1 = Helper.ChargeCodes.NewConsolChargeCode("CC1", "Domestic", FlatCalculator.Code);

			Factory.Save();

			var costing = Helper.NewCosting(routingCarrier);
			var rateEntry = costing.AddRateEntry(RatingConstants.RateCategory.FCL, RateMode.SEA, origin, destination);
			rateEntry.RateLines.RemoveAndDeleteAll();
			var rateLine = rateEntry.AddRateLine(chargeCode1.AC_Code, FlatCalculator.Code);
			rateLine.GetCalculator<FlatCalculator>().BaseRate = 200m;

			var shipment = CreateForwardingShipment(TransportModes.Sea, carrier.PK, Consignee.PK, origin, destination, 550m);
			var consol = CreateForwardingConsol(TransportModes.Sea, origin, destination, carrier, shipment, PaymentType.Prepaid);
			consol.JK_ConsolMode = ContainerModes.FCL;
			consol.JK_AgentType = AgentType.CoLoad;

			consol.JK_OA_CreditorAddress = coLoadWith.MainAddress.PK;
			consol.CarrierExportCreditorAddress.E2_OA_Address = exportCreditorAddress.MainAddress.PK;
			var transport1 = consol.Transports[0];
			transport1.CarrierPK = routingCarrier.PK;
			transport1.CreditorPK = routingCreditor.PK;
			transport1.JW_TransportMode = TransportModes.Sea;
			Factory.Save();

			AssertEquals("Preconditions: domestic consol expected.", true, consol.IsDomestic());
			AssertEquals("Preconditions: Coload with has value", coLoadWith.PK, consol.Creditor.PK);
			AssertEquals("Preconditions: Carrier Export Creditor has value", exportCreditorAddress.PK, consol.CarrierExportCreditor.PK);

			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = routingCreditor.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Routing Carrier, the charge defaulting creditor should become Routing Credtior", null, expectedCosts, consol, false);
			}

			transport1.CreditorPK = ZGuid.Empty;
			using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var expectedCosts = new[]
				{
					new AssertionCost
					{
						ChargeCode = "CC1",
						E6_OSCostAmount = 200m,
						E6_OH_Creditor = routingCarrier.PK
					}
				};

				AutoCostAndAssert("When rate's service provider is Routing Carrier, if Routing Creditor is Empty, the charge defaulting creditor should become Routing Carrier", null, expectedCosts, consol, false);
			}
		}

		#endregion

		#region  Sending /Receiving Agents
		public void TestCreditorDefaultingFromSendingForwardingAgen()
		{
			CreditorDefaultingFromForwardingAgent(isSendingAgent: true);
		}

		public void TestCreditorDefaultingFromReceivingForwardingAgen()
		{
			CreditorDefaultingFromForwardingAgent(isSendingAgent: false);
		}

		void CreditorDefaultingFromForwardingAgent(bool isSendingAgent)
		{
			var origin = "USLAX";
			var destination = "AUSYD";
			var agentName = "sending";

			if (!isSendingAgent)
			{
				origin = "AUSYD";
				destination = "USLAX";
				agentName = "receiving";
			}

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_IsCreditor = true;

			(var consol, _) = CreateForwardingConsolWithShipment(origin, destination);
			consol.JK_PrepaidCollect = isSendingAgent ? PaymentType.Prepaid : PaymentType.Collect;
			consol.JK_OA_DepartureCTOAddress = agent.MainAddress.PK;

			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			consol.JK_OA_ShippingLineAddress = ZGuid.Empty;

			if (isSendingAgent)
			{
				AssertEquals("Preconditions: Import Consol expected.", true, consol.IsImport());
			}
			else
			{
				AssertEquals("Preconditions: Export Consol expected.", true, consol.IsExport());
			}

			SetForwarderAddress(consol, agent.MainAddress.PK, isSendingAgent);

			AssertEquals($"Consol creditor is same as {agentName} agent (this is legacy logic)", agent.MainAddress.PK, consol.JK_OA_CreditorAddress);

			var someRandomOrg = Factory.NewWithValidTestData<OrgHeader>();
			someRandomOrg.OH_IsCreditor = true;

			SetForwarderAddress(consol, ZGuid.Empty, isSendingAgent);
			consol.JK_OA_CreditorAddress = someRandomOrg.MainAddress.PK;

			Factory.Save();

			SetForwarderAddress(consol, agent.MainAddress.PK, isSendingAgent);

			AssertEquals($"When Consol creditor has value, setting {agentName} agent should NOT change it", someRandomOrg.MainAddress.PK, consol.JK_OA_CreditorAddress);

			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsCreditor = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			SetForwarderAddress(consol, ZGuid.Empty, isSendingAgent);

			Factory.Save();

			SetForwarderAddress(consol, agent.MainAddress.PK, isSendingAgent);

			AssertEquals($"When Consol creditor is empty and carrier has value, setting {agentName} agent should set creditor based on carrier", carrier.MainAddress.PK, consol.JK_OA_CreditorAddress);

			var relatedParties = carrier.AllRelatedParties;

			var creditorForPad = Factory.NewWithValidTestData<OrgHeader>();
			creditorForPad.OH_IsCreditor = true;

			AddRelatedPartyRecord(consol.TransportMode, consol.JK_ConsolMode, relatedParties, creditorForPad, RelatedPartyDirectionList.Codes.PickupAndDelivery, isSendingAgent ? destination : origin);

			SetForwarderAddress(consol, ZGuid.Empty, isSendingAgent);
			consol.JK_OA_CreditorAddress = ZGuid.Empty;

			Factory.Save();

			SetForwarderAddress(consol, agent.MainAddress.PK, isSendingAgent);
			AssertEquals($"When Consol creditor is empty and carrier has SPC, setting {agentName} agent should set creditor based on carrier's SPC", creditorForPad.MainAddress.PK, consol.JK_OA_CreditorAddress);

			var address = agent.Addresses.AddNew();
			consol.JK_OA_CreditorAddress = ZGuid.Empty;
			SetForwarderAddress(consol, address.PK, isSendingAgent);

			AssertEquals($"When we select another {agentName} address in same organiaztion, we should not default credtior", ZGuid.Empty, consol.JK_OA_CreditorAddress);
		}

		void SetForwarderAddress(ForwardingConsol consol, ZGuid forwarderAddress, bool isSendingAgent)
		{
			if (isSendingAgent)
			{
				consol.JK_OA_SendingForwarderAddress = forwarderAddress;
			}
			else
			{
				consol.JK_OA_ReceivingForwarderAddress = forwarderAddress;
			}
		}

		#endregion
	}
}
