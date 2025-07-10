using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;
using static Enterprise.Rating.Business.RatingConstants;
using HandlingType = Enterprise.MasterFiles.Business.AgentStatusList.Codes;
using RateMode = Enterprise.Core.Constants.RateMode;

namespace Enterprise.Rating.Business.Gateway
{
	public class GatewayScenarioGenerator
	{
		public GatewayScenarioGenerator(BaseGatewayBillingIntegrationTest parentTestCase, BusinessObjectFactory factory)
		{
			ParentTestCase = parentTestCase;
			Factory = factory;
		}

		#region Scenario S1

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                                        | THBKK      | SGSIN      | NLRTM     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S1.1 (C1.1, C1.2)(LCL-DAP)                   | THBKK      |            | NLRTM     |
		  | S1.2 (C1.1, C1.2)(LCL-EXW)                   | THBKK      |            | NLRTM     |
		  | S1.3 (C1.1      )(LCL-CFR)                   | THBKK      | SGSIN      |           |
		  | S1.4 (C1.1      )(LCL-EXW)                   | THBKK      | SGSIN      |           |
		  | S1.5 (      C1.2)(LCL-EXW)                   |            | SGSIN      | NLRTM     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C1.1 (S1.1, S1.2, S1.3, S1.4      )(SEA-CCX) | THBKK  (-) | SGSIN  GTT |           |
		  | C1.2 (S1.1, S1.2,             S1.5)(SEA-PPD) |            | SGSIN  GTT | NLRTM (-) |
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS1NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS1(fromCache);
			SetupScenarioS1Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS1PlanedLoadRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS1(fromCache);
			SetupScenarioS1PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS1(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS1)}";
				return Factory.GetCachedValue(key, SetupScenarioS1Core);
			}

			return SetupScenarioS1Core();
		}

		GatewayScenario SetupScenarioS1Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S1");

			scenario.RoutePorts.AddRange(new[] { "THBKK", "SGSIN", "NLRTM" });

			scenario.Agents.Add("THBKK", Helper.CreateNonProxyBranch("THBKK")); //Non Gateway agent
			scenario.Agents.Add("SGSIN", Helper.CreateCompanyAndBranchProxy("SGSIN"));
			scenario.Agents.Add("NLRTM", Helper.CreateCompanyAndBranchProxy("NLRTM"));

			scenario.ChargeCodes.Add("GWPRV", Helper.ChargeCodes.CreateGlobalCharge("GWPRV"));
			scenario.ChargeCodes["GWPRV"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C1.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "THBKK",
					destination: "SGSIN",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Collect)
				.SetSendingAgent(scenario.Agents["THBKK"].proxy)
				.SetReceivingAgent(scenario.Agents["SGSIN"].proxy, HandlingType.GatewayAgentWithTariff, "SGSIN"));

			scenario.Consols.Add("C1.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "SGSIN",
					destination: "NLRTM",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["SGSIN"].proxy, HandlingType.GatewayAgentWithTariff, "SGSIN")
				.SetReceivingAgent(scenario.Agents["NLRTM"].proxy));

			scenario.Shipments.Add("S1.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "THBKK",
					destination: "NLRTM",
					incoterm: IncoTerms.DeliveredAtPlace,
					scenario.Consols["C1.1"],
					scenario.Consols["C1.2"]));

			scenario.Shipments.Add("S1.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "THBKK",
					destination: "NLRTM",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C1.1"],
					scenario.Consols["C1.2"]));

			scenario.Shipments.Add("S1.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "THBKK",
					destination: "SGSIN",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C1.1"]));

			scenario.Shipments.Add("S1.4",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "THBKK",
					destination: "SGSIN",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C1.1"]));

			scenario.Shipments.Add("S1.5",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "SGSIN",
					destination: "NLRTM",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C1.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS1Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("SGSIN", Helper.NewIntercompanyTariff(scenario.Agents["SGSIN"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWPRV",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "TH",
				plannedDischarge: "SG",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWFWD",
				4m,
				origin: "",
				destination: "NL",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWIMP",
				5m,
				origin: "",
				destination: "SG",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS1PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("SGSIN", Helper.NewIntercompanyTariff(scenario.Agents["SGSIN"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWPRV",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "TH",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "NL",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWIMP",
				5m,
				origin: "",
				destination: "SG",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S2

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                               | CNSZX  | HKHKG      | BEANR      | BRSSZ     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S2.1 (C2.1, C2.2)(LCL-DAP)          | CNSZX  |            |            | BRSSZ     |
		  | S2.2 (C2.1, ....)(LCL-EXW)          | CNSZX  |            | BEANR      |           |
		  | S2.3 (C2.1, C2.2)(LCL-EXW)          |        | HKHKG      |            | BRSSZ     |
		  | S2.4 (C2.1      )(LCL-CFR)          |        | HKHKG      | BEANR      |           |
		  | S2.5 (      C2.2)(LCL-DAP)          |        |            | BEANR      | BRSSZ     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C2.1 (S2.1, S2.2, S2.3, S2.4      )(SEA-CCX) | HKHKG  GTT | BEANR  GTT |           |
		  | C2.1 (S2.1, S2.3, S2.5            )(SEA-PPD) |            | BEANR  GTT | BRSSZ (-) |
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS2NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS2(fromCache);
			SetupScenarioS2Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS2PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS2(fromCache);
			SetupScenarioS2PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS2(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS2)}";
				return Factory.GetCachedValue(key, SetupScenarioS2Core);
			}

			return SetupScenarioS2Core();
		}

		GatewayScenario SetupScenarioS2Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S2");

			scenario.RoutePorts.AddRange(new[] { "CNSZX", "HKHKG", "BEANR", "BRSSZ" });

			scenario.Agents.Add("CNSZX", Helper.CreateNonProxyBranch("CNSZX")); //Non Gateway agent
			scenario.Agents.Add("HKHKG", Helper.CreateCompanyAndBranchProxy("HKHKG"));
			scenario.Agents.Add("BEANR", Helper.CreateCompanyAndBranchProxy("BEANR"));
			scenario.Agents.Add("BRSSZ", Helper.CreateNonProxyBranch("BRSSZ")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C2.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "HKHKG",
					destination: "BEANR",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["HKHKG"].proxy, HandlingType.GatewayAgentWithTariff, "HKHKG")
				.SetReceivingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff, "BEANR"));

			scenario.Consols.Add("C2.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "BEANR",
					destination: "BRSSZ",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff, "BEANR"));

			scenario.Shipments.Add("S2.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "CNSZX",
					destination: "BRSSZ",
					incoterm: IncoTerms.DeliveredAtPlace,
					scenario.Consols["C2.1"],
					scenario.Consols["C2.2"]));

			scenario.Shipments.Add("S2.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "CNSZX",
					destination: "BEANR",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C2.1"]));

			scenario.Shipments.Add("S2.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "HKHKG",
					destination: "BRSSZ",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C2.1"],
					scenario.Consols["C2.2"]));

			scenario.Shipments.Add("S2.4",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "HKHKG",
					destination: "BEANR",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C2.1"]));

			scenario.Shipments.Add("S2.5",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "BEANR",
					destination: "BRSSZ",
					incoterm: IncoTerms.DeliveredAtPlace,
					scenario.Consols["C2.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS2Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("HKHKG", Helper.NewIntercompanyTariff(scenario.Agents["HKHKG"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				3m,
				origin: "",
				destination: "BR",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				4m,
				origin: "",
				destination: "BE",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWIMP",
				5m,
				origin: "",
				destination: "HK",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("BEANR", Helper.NewIntercompanyTariff(scenario.Agents["BEANR"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				6m,
				origin: "",
				destination: "BR",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWIMP",
				7m,
				origin: "",
				destination: "BE",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS2PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("HKHKG", Helper.NewIntercompanyTariff(scenario.Agents["HKHKG"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "BR",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "BE",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWIMP",
				5m,
				origin: "",
				destination: "HK",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("BEANR", Helper.NewIntercompanyTariff(scenario.Agents["BEANR"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				6m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "BR",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWIMP",
				7m,
				origin: "",
				destination: "BE",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S3

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                               | THBKK  | SGSIN      | SIKOP      | HUBUD     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S3.1 (C3.1, C3.2)(LCL-DAP)          | THBKK  |            |            | HUBUD     |
		  | S3.2 (C3.2, ....)(LCL-EXW)          |        | SGSIN      |            | HUBUD     |
		  | S3.3 (C3.1,     )(LCL-EXW)          | THBKK  | SGSIN      |            |           |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C3.1 (S3.1, S3.3,                 )(SEA-CCX) | THBKK  (-) | SGSIN  GTT |           |
		  | C3.2 (S3.1, S3.2,                 )(SEA-PPD) |            | SGSIN  GTT | HUBUD (-) |
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS3NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS3(fromCache);
			SetupScenarioS3Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS3PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS3(fromCache);
			SetupScenarioS3PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS3(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS3)}";
				return Factory.GetCachedValue(key, SetupScenarioS3Core);
			}

			return SetupScenarioS3Core();
		}

		GatewayScenario SetupScenarioS3Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S3");

			scenario.RoutePorts.AddRange(new[] { "THBKK", "SGSIN", "SIKOP", "HUBUD" });

			scenario.Agents.Add("THBKK", Helper.CreateNonProxyBranch("THBKK")); //Non Gateway agent
			scenario.Agents.Add("SGSIN", Helper.CreateCompanyAndBranchProxy("SGSIN"));
			scenario.Agents.Add("SIKOP", Helper.CreateNonProxyBranch("SIKOP")); //Non Gateway agent
			scenario.Agents.Add("HUBUD", Helper.CreateNonProxyBranch("HUBUD")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWPRV", Helper.ChargeCodes.CreateGlobalCharge("GWPRV"));
			scenario.ChargeCodes["GWPRV"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C3.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "THBKK",
					destination: "SGSIN",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Collect)
				.SetSendingAgent(scenario.Agents["THBKK"].proxy)
				.SetReceivingAgent(scenario.Agents["SGSIN"].proxy, HandlingType.GatewayAgentWithTariff, "SGSIN"));

			scenario.Consols.Add("C3.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "SGSIN",
					destination: "HUBUD",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["SGSIN"].proxy, HandlingType.GatewayAgentWithTariff, "SGSIN")
				.SetReceivingAgent(scenario.Agents["HUBUD"].proxy));

			scenario.Shipments.Add("S3.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "THBKK",
					destination: "HUBUD",
					incoterm: IncoTerms.DeliveredAtPlace,
					scenario.Consols["C3.1"],
					scenario.Consols["C3.2"]));

			scenario.Shipments.Add("S3.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "SGSIN",
					destination: "HUBUD",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C3.2"]));

			scenario.Shipments.Add("S3.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "THBKK",
					destination: "SGSIN",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C3.1"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS3Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("SGSIN", Helper.NewIntercompanyTariff(scenario.Agents["SGSIN"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWPRV",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "TH",
				plannedDischarge: "SG",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWFWD",
				4m,
				origin: "",
				destination: "HU",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWIMP",
				5m,
				origin: "",
				destination: "SG",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS3PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("SGSIN", Helper.NewIntercompanyTariff(scenario.Agents["SGSIN"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWPRV",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "TH",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "HU",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SGSIN"],
				"GWIMP",
				5m,
				origin: "",
				destination: "SG",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S5

		/*+----------------------------------------------------+-------------+-------------+-----------+-----------+
		  | Route                                  | DKCPH     | DEBRE       | BEANR       | CRSJO     | HNSAP     |
		  +------------------------------------------------------------------+-------------+-----------+-----------+
		  | Shipments                              |           |             |             |           |           |
		  | S5.1 (C5.1, C5.2, C5.3, C5.4)(LCL-DAP) | DKCPH     |             |             |           | HNSAP     |
		  | S5.2 (      C5.2, C5.3      )(LCL-FOB) |           | DEBRE       |             | CRSJO     |           |
		  | S5.3 (            C5.3, C5.4)(LCL-DDP) |           |             | BEANR       |           | HNASP     |
		  +----------------------------------------+-----------+-------------+-------------+-----------+-----------+
		  | Consols                                |           |             |             |           |           |
		  | C5.1 (S5.1,           )(ROA-PPD)       | DKCPH (-) | DEBRE (GTT) |             |           |           |
		  | C5.2 (S5.1, S5.2,     )(SEA-PPD)       |           | DEBRE (GTT) | BEANR (GTT) |           |           |
		  | C5.3 (S5.1, S5.2, S5.3)(SEA-PPD)       |           |             | BEANR (GTT) | CRSJO (-) |           |
		  | C5.4 (S5.1,       S5.3)(SEA-PPD)       |           |             |             | CRSJO (-) | HNSAP (-) |
		  +----------------------------------------+-----------+-------------+-------------+-----------+-----------+*/

		public GatewayScenario SetupScenarioS5NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5(fromCache);
			SetupScenarioS5Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5(fromCache);
			SetupScenarioS5PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS5(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS5)}";
				return Factory.GetCachedValue(key, SetupScenarioS5Core);
			}

			return SetupScenarioS5Core();
		}

		GatewayScenario SetupScenarioS5Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S5");

			scenario.RoutePorts.AddRange(new[] { "DKCPH", "DEBRE", "BEANR", "CRSJO", "HNSAP" });

			scenario.Agents.Add("DKCPH", Helper.CreateNonProxyBranch("DKCPH")); //Non Gateway agent
			scenario.Agents.Add("DEBRE", Helper.CreateCompanyAndBranchProxy("DEBRE"));
			scenario.Agents.Add("BEANR", Helper.CreateCompanyAndBranchProxy("BEANR"));
			scenario.Agents.Add("CRSJO", Helper.CreateNonProxyBranch("CRSJO")); //Non Gateway agent
			scenario.Agents.Add("HNSAP", Helper.CreateNonProxyBranch("HNSAP")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C5.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DKCPH",
					destination: "DEBRE",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DKCPH"].proxy)
				.SetReceivingAgent(scenario.Agents["DEBRE"].proxy, HandlingType.GatewayAgentWithTariff));

			scenario.Consols.Add("C5.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DEBRE",
					destination: "BEANR",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DEBRE"].proxy, HandlingType.GatewayAgentWithTariff)
				.SetReceivingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff));

			scenario.Consols.Add("C5.3", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "BEANR",
					destination: "CRSJO",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff)
				.SetReceivingAgent(scenario.Agents["CRSJO"].proxy));

			scenario.Consols.Add("C5.4", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "CRSJO",
					destination: "HNSAP",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["CRSJO"].proxy)
				.SetReceivingAgent(scenario.Agents["HNSAP"].proxy));

			scenario.Shipments.Add("S5.1", Helper
				.CreateGatewayShipment(
					jobNumber: "S5.1",
					transportMode: TransportModes.Sea,
					origin: "DKCPH",
					destination: "HNSAP",
					incoterm: IncoTerms.DeliveredAtPlace)
				.AttachConsol(
					scenario.Consols["C5.1"],
					scenario.Consols["C5.2"],
					scenario.Consols["C5.3"],
					scenario.Consols["C5.4"]));

			scenario.Shipments.Add("S5.2", Helper
				.CreateGatewayShipment(
					jobNumber: "S5.2",
					transportMode: TransportModes.Sea,
					origin: "DEBRE",
					destination: "CRSJO",
					incoterm: IncoTerms.FreeOnBoard)
				.AttachConsol(
					scenario.Consols["C5.2"],
					scenario.Consols["C5.3"]));

			scenario.Shipments.Add("S5.3", Helper
				.CreateGatewayShipment(
					jobNumber: "S5.3",
					transportMode: TransportModes.Sea,
					origin: "BEANR",
					destination: "HNSAP",
					incoterm: IncoTerms.DeliveredDutyPaid)
				.AttachConsol(
					scenario.Consols["C5.3"],
					scenario.Consols["C5.4"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS5Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper
				.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy)
				.AddFlatRate(
					chargeCode: "GWFWD",
					baseRate: 1m,
					origin: "",
					destination: "HN",
					plannedLoad: "",
					plannedDischarge: "",
					gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
					rateCategory: RateCategory.LCL,
					rateMode: RateMode.LCL)
				.AddFlatRate(
					chargeCode: "GWFWD",
					baseRate: 2m,
					origin: "",
					destination: "CR",
					plannedLoad: "",
					plannedDischarge: "",
					gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
					rateCategory: RateCategory.LCL,
					rateMode: RateMode.LCL));

			scenario.IntercompanyTariffs.Add("BEANR", Helper
				.NewIntercompanyTariff(scenario.Agents["BEANR"].proxy)
				.AddFlatRate(
					chargeCode: "GWFWD",
					baseRate: 11m,
					origin: "",
					destination: "HN",
					plannedLoad: "",
					plannedDischarge: "",
					gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
					rateCategory: RateCategory.LCL,
					rateMode: RateMode.LCL)
				.AddFlatRate(
					chargeCode: "GWFWD",
					baseRate: 12m,
					origin: "",
					destination: "CR",
					plannedLoad: "",
					plannedDischarge: "",
					gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
					rateCategory: RateCategory.LCL,
					rateMode: RateMode.LCL));

			Factory.Save();
		}

		void SetupScenarioS5PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "HN",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "CR",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("BEANR", Helper.NewIntercompanyTariff(scenario.Agents["BEANR"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "HN",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				6m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "CR",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S5-Planned Load

		/*+----------------------------------------------------+-------------+-------------+------------+-----------+
		  | Route                                  | DKCPH     | DEBRE       | BEANR       | CRSJO      | HNSAP     |
		  +------------------------------------------------------------------+-------------+------------+-----------+
		  | Shipments                              |           |             |             |            |           |
		  | S5.1 (C5.1, C5.2, C5.3, C5.4)(LCL-DAP) | DKCPH     |             |             |            | HNSAP     |
		  | S5.2 (C5.2, C5.3)(LCL-FOB)             |           | DEBRE       |             | CRSJO      |           |
		  | S5.3 (C5.3, C5.4)(LCL-DDP)             |           |             | BEANR       |            | HNASP     |
		  +------------------------------------------------+-----------------+-------------+------------+-----------+
		  | Consols                                            |             |             |            |           |
		  | C5.1 (S5.1,                 )(ROA-PPD) | DKCPH (-) | DEBRE (GTT) |             |            |           |
		  | C5.2 (S5.1, S5.2,           )(SEA-PPD) |           | DEBRE (GTT) | BEANR (-)   |            |           |
		  | C5.3 (S5.2, S5.3,           )(SEA-PPD) |           |             | BEANR (GTT) | CRSJO (-)  |           |
		  | C5.4 (S5.3                  )(SEA-PPD) |           |             |             | CRSJO (-)  | HNSAP (-) |
		  +------------------------------------------------+-----------------+-------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS5PLRatesAlternative(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateNew(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForC1(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForC1(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForC2(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForC2(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForC3(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForC3(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForC4(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForC4(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForC1C2(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForC1C2(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForC2C4(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForC2C4(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForAll(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForAll(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForAllGTT(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateForAllGTT(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PLRatesForAllGTTAlternative(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5_Alternative(fromCache);
			SetupScenarioS5PLRateNewAllGTTAlternative(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS5_Alternative(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS5_Alternative)}";
				return Factory.GetCachedValue(key, SetupScenarioS5CoreAlternative);
			}

			return SetupScenarioS5CoreAlternative();
		}

		GatewayScenario SetupScenarioS5CoreAlternative()
		{
			var scenario = new GatewayScenario("WI00317849 - S5");

			scenario.RoutePorts.AddRange(new[] { "THBKK", "SGSIN", "SIKOP", "HUBUD" });

			scenario.Agents.Add("DKCPH", Helper.CreateNonProxyBranch("DKCPH")); //Non Gateway agent
			scenario.Agents.Add("DEBRE", Helper.CreateCompanyAndBranchProxy("DEBRE"));
			scenario.Agents.Add("BEANR", Helper.CreateCompanyAndBranchProxy("BEANR"));
			scenario.Agents.Add("CRSJO", Helper.CreateNonProxyBranch("CRSJO")); //Non Gateway agent
			scenario.Agents.Add("HNSAP", Helper.CreateNonProxyBranch("HNSAP")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C5.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DKCPH",
					destination: "DEBRE",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DKCPH"].proxy)
				.SetReceivingAgent(scenario.Agents["DEBRE"].proxy, HandlingType.GatewayAgentWithTariff, "DEBRE"));

			scenario.Consols.Add("C5.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DEBRE",
					destination: "BEANR",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DEBRE"].proxy, HandlingType.GatewayAgentWithTariff, "DEBRE")
				.SetReceivingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff, "BEANR"));

			scenario.Consols.Add("C5.3", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "BEANR",
					destination: "CRSJO",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff, "BEANR")
				.SetReceivingAgent(scenario.Agents["CRSJO"].proxy));

			scenario.Consols.Add("C5.4", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "CRSJO",
					destination: "HNSAP",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["CRSJO"].proxy)
				.SetReceivingAgent(scenario.Agents["HNSAP"].proxy));

			scenario.Shipments.Add("SSS",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "DEBRE",
					destination: "CRSJO",
					incoterm: IncoTerms.FreeOnBoard,
					scenario.Consols["C5.1"],
					scenario.Consols["C5.2"],
					scenario.Consols["C5.3"],
					scenario.Consols["C5.4"]));

			scenario.Shipments["SSS"].JS_RL_NKLoadPort = "DEHAM";

			scenario.Shipments.Add("S5.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "BEANR",
					destination: "HNSAP",
					incoterm: IncoTerms.FreeOnBoard,
					scenario.Consols["C5.3"],
					scenario.Consols["C5.4"]));

			scenario.Shipments["S5.3"].JS_RL_NKLoadPort = "DEHAM";

			Factory.Save();

			return scenario;
		}

		void SetupScenarioS5PLRateNew(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "DEHAM",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForC1(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.1"].LoadPort.Code,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForC2(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.2"].JK_RL_NKLoadPort,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForC3(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				6m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.3"].JK_RL_NKLoadPort,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForC4(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				7m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.4"].JK_RL_NKLoadPort,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForC1C2(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.1"].LoadPort.Code,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.2"].JK_RL_NKLoadPort,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForC2C4(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.2"].LoadPort.Code,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				7m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.4"].JK_RL_NKLoadPort,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForAll(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "DEHAM",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.1"].LoadPort.Code,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.2"].JK_RL_NKLoadPort,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				6m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.3"].LoadPort.Code,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				7m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.4"].JK_RL_NKLoadPort,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateForAllGTT(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("BEANR", Helper.NewIntercompanyTariff(scenario.Agents["BEANR"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				8m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.3"].SendingForwarderAddress.RelatedPortCode.Code,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PLRateNewAllGTTAlternative(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("BEANR", Helper.NewIntercompanyTariff(scenario.Agents["BEANR"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "DEHAM",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				8m,
				origin: "",
				destination: "",
				plannedLoad: scenario.Consols["C5.3"].SendingForwarderAddress.RelatedPortCode.Code,
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S5-Planned Discharge

		/*+----------------------------------------------------+-------------+-------------+------------+-----------+
		  | Route                                  | DKCPH     | DEBRE       | BEANR       | CRSJO      | HNSAP     |
		  +------------------------------------------------------------------+-------------+------------+-----------+
		  | Shipments                              |           |             |             |            |           |
		  | S5.1 (C5.1, C5.2, C5.3, C5.4)(LCL-DAP) | DKCPH     |             |             |            | HNSAP     |
		  | S5.2 (C5.2, C5.3)(LCL-FOB)             |           | DEBRE       |             | CRSJO      |           |
		  | S5.3 (C5.3, C5.4)(LCL-DDP)             |           |             | BEANR       |            | HNASP     |
		  +------------------------------------------------+-----------------+-------------+------------+-----------+
		  | Consols                                            |             |             |            |           |
		  | C5.1 (S5.1,                 )(ROA-PPD) | DKCPH (-) | DEBRE (GTT) |             |            |           |
		  | C5.2 (S5.1, S5.2,           )(SEA-PPD) |           | DEBRE (GTT) | BEANR (-)   |            |           |
		  | C5.3 (S5.2, S5.3,           )(SEA-PPD) |           |             | BEANR (GTT) | CRSJO (-)  |           |
		  | C5.4 (S5.3                  )(SEA-PPD) |           |             |             | CRSJO (-)  | HNSAP (-) |
		  +------------------------------------------------+-----------------+-------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS5PDRatesAlternative(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateAlternative(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForC1(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForC1(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForC2(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForC2(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForC3(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForC3(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForC4(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForC4(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForC1C2(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForC1C2(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForC2C4(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForC2C4(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForAll(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForAll(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForAllGTT(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForAllGTT(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS5PDRatesForAllGTTAlternative(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS5ForPD(fromCache);
			SetupScenarioS5PDRateForAllGTTAlternative(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS5ForPD(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS5ForPD)}";
				return Factory.GetCachedValue(key, SetupScenarioS5CoreForPD);
			}

			return SetupScenarioS5CoreForPD();
		}

		GatewayScenario SetupScenarioS5CoreForPD()
		{
			var scenario = new GatewayScenario("WI00317849 - S5");

			scenario.RoutePorts.AddRange(new[] { "THBKK", "SGSIN", "SIKOP", "HUBUD" });

			scenario.Agents.Add("DKCPH", Helper.CreateNonProxyBranch("DKCPH")); //Non Gateway agent
			scenario.Agents.Add("DEBRE", Helper.CreateCompanyAndBranchProxy("DEBRE"));
			scenario.Agents.Add("BEANR", Helper.CreateCompanyAndBranchProxy("BEANR"));
			scenario.Agents.Add("CRSJO", Helper.CreateNonProxyBranch("CRSJO")); //Non Gateway agent
			scenario.Agents.Add("HNSAP", Helper.CreateNonProxyBranch("HNSAP")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C5.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DKCPH",
					destination: "DEBRE",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DKCPH"].proxy)
				.SetReceivingAgent(scenario.Agents["DEBRE"].proxy, HandlingType.GatewayAgentWithTariff, "DEBRE"));

			scenario.Consols.Add("C5.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DEBRE",
					destination: "BEANR",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DEBRE"].proxy, HandlingType.GatewayAgentWithTariff, "DEBRE")
				.SetReceivingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff, "BEANR"));

			scenario.Consols.Add("C5.3", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "BEANR",
					destination: "CRSJO",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["BEANR"].proxy, HandlingType.GatewayAgentWithTariff, "BEANR")
				.SetReceivingAgent(scenario.Agents["CRSJO"].proxy));

			scenario.Consols.Add("C5.4", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "CRSJO",
					destination: "HNSAP",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["CRSJO"].proxy)
				.SetReceivingAgent(scenario.Agents["HNSAP"].proxy));

			scenario.Shipments.Add("SSS",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "DEBRE",
					destination: "CRSJO",
					incoterm: IncoTerms.FreeOnBoard,
					scenario.Consols["C5.1"],
					scenario.Consols["C5.2"],
					scenario.Consols["C5.3"],
					scenario.Consols["C5.4"]));

			scenario.Shipments["SSS"].JS_RL_NKDischargePort = "DEHAM";

			scenario.Shipments.Add("S5.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "BEANR",
					destination: "HNSAP",
					incoterm: IncoTerms.FreeOnBoard,
					scenario.Consols["C5.3"],
					scenario.Consols["C5.4"]));

			scenario.Shipments["S5.3"].JS_RL_NKDischargePort = "DEHAM";

			Factory.Save();

			return scenario;
		}

		void SetupScenarioS5PDRateAlternative(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "DEHAM",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForC1(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.1"].DischargePort.Code,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForC2(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.2"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForC3(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				6m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.3"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForC4(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				7m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.4"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForC1C2(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.1"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.2"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForC2C4(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.2"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				7m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.4"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForAll(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "DEHAM",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.1"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.2"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				6m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.3"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				7m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.4"].JK_RL_NKDischargePort,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForAllGTT(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEBRE", Helper.NewIntercompanyTariff(scenario.Agents["DEBRE"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEBRE"],
				"GWFWD",
				8m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.2"].ReceivingForwarderAddress.RelatedPortCode.Code,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS5PDRateForAllGTTAlternative(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("BEANR", Helper.NewIntercompanyTariff(scenario.Agents["BEANR"].proxy));

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "DEHAM",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEANR"],
				"GWFWD",
				8m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: scenario.Consols["C5.2"].ReceivingForwarderAddress.RelatedPortCode.Code,
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S6

		/*+----------------------------+--------------+-------------+-----------+
		  | Route                      | HKHKG        | USLAX       | USELP     |
		  +----------------------------+--------------+-------------+-----------+
		  | Shipments                  |              |             |           |
		  | S6.1 (C6.1, C6.2)(LCL-EXW) | HKHKG        |             | USELP     |
		  | S6.2 (C6.1      )(LCL-DAP) | HKHKG        | USLAX       |           |
		  +----------------------------+--------------+-------------+-----------+
		  | Consols                    |              |             |           |
		  | C6.1 (S6.1, S6.2)(SEA-PPD) | HKHKGK (GTT) | USLAX (GTA) |           |
		  | C6.2 (S6.1      )(ROA-PPD) |              | USLAX (GTA) | USELP (-) |
		  +----------------------------+--------------+-------------+-----------+*/

		public GatewayScenario SetupScenarioS6NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS6(fromCache);
			SetupScenarioS6Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS6PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS6(fromCache);
			SetupScenarioS6PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS6(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS6)}";
				return Factory.GetCachedValue(key, SetupScenarioS6Core);
			}

			return SetupScenarioS6Core();
		}

		GatewayScenario SetupScenarioS6Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S6");

			scenario.RoutePorts.AddRange(new[] { "HKHKG", "USLAX", "USELP" });

			scenario.Agents.Add("HKIFF", Helper.CreateCompanyAndBranchProxy("HKIFF"));
			scenario.Agents.Add("HKHKG", Helper.CreateCompanyAndBranchProxy("HKHKG"));
			scenario.Agents.Add("USLAX", Helper.CreateCompanyAndBranchProxy("USLAX"));
			scenario.Agents.Add("USELP", Helper.CreateNonProxyBranch("USELP")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C6.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "HKHKG",
					destination: "USLAX",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["HKHKG"].proxy, HandlingType.GatewayAgentWithTariff)
				.SetReceivingAgent(scenario.Agents["USLAX"].proxy, HandlingType.GatewayAgent));

			scenario.Consols.Add("C6.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "USLAX",
					destination: "USELP",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["USLAX"].proxy, HandlingType.GatewayAgent)
				.SetReceivingAgent(scenario.Agents["USELP"].proxy));

			scenario.Shipments.Add("S6.1", Helper
				.CreateGatewayShipment(
					jobNumber: "S6.1",
					transportMode: TransportModes.Sea,
					origin: "HKHKG",
					destination: "USELP",
					incoterm: IncoTerms.ExWorks)
				.AttachConsol(
					scenario.Consols["C6.1"],
					scenario.Consols["C6.2"]));

			scenario.Shipments.Add("S6.2", Helper
				.CreateGatewayShipment(
					jobNumber: "S6.2",
					transportMode: TransportModes.Sea,
					origin: "HKHKG",
					destination: "USLAX",
					incoterm: IncoTerms.DeliveredDutyPaid)
				.AttachConsol(
					scenario.Consols["C6.1"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS6Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("HKHKG", Helper
				.NewIntercompanyTariff(scenario.Agents["HKHKG"].proxy)
				.AddFlatRate(
					chargeCode: "GWFWD",
					baseRate: 1m,
					origin: "",
					destination: "US",
					plannedLoad: "",
					plannedDischarge: "",
					gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
					rateCategory: RateCategory.LCL,
					rateMode: RateMode.LCL));

			scenario.IntercompanyTariffs.Add("USLAX", Helper
				.NewIntercompanyTariff(scenario.Agents["USLAX"].proxy)
				.AddFlatRate(
					"GWFWD",
					11m,
					origin: "",
					destination: "US",
					plannedLoad: "",
					plannedDischarge: "",
					gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
					rateCategory: RateCategory.LCL,
					rateMode: RateMode.LCL)
				.AddFlatRate(
					"GWIMP",
					12m,
					origin: "",
					destination: "US",
					plannedLoad: "",
					plannedDischarge: "",
					gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport,
					rateCategory: RateCategory.LCL,
					rateMode: RateMode.LCL));

			Factory.Save();
		}

		void SetupScenarioS6PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("HKHKG", Helper.NewIntercompanyTariff(scenario.Agents["HKHKG"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "US",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("USLAX", Helper.NewIntercompanyTariff(scenario.Agents["USLAX"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["USLAX"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "US",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["USLAX"],
				"GWIMP",
				5m,
				origin: "",
				destination: "US",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S7

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                               | SESTO  | DEFRA      | ZAJNB      | ZADUR     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S7.1 (C7.1, C7.2, C7.3)(LCL-EXW)    | SESTO  |            |            | ZADUR     |
		  | S7.2 (C7.1, C7.2)(LCL-CFR)          | SESTO  |            | ZAJNB      |           |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C7.1 (S7.1, S7.2,                 )(ROA-PPD) | SESTO  (-) | DEFRA  GTT |           |
		  | C7.2 (S7.1, S7.2,                 )(AIR-PPD) |            | DEFRA  GTT | ZAJNB GTT |
		  | C7.3 (S7.1,                       )(ROA-PPD) |            | ZAJNB  GTT | ZADUR (-) |
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS7NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS7(fromCache);
			SetupScenarioS7Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS7PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS7(fromCache);
			SetupScenarioS7PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS7(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS7)}";
				return Factory.GetCachedValue(key, SetupScenarioS7Core);
			}

			return SetupScenarioS7Core();
		}

		GatewayScenario SetupScenarioS7Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S7");

			scenario.RoutePorts.AddRange(new[] { "SESTO", "DEFRA", "ZAJNB", "ZADUR" });

			scenario.Agents.Add("SESTO", Helper.CreateNonProxyBranch("SESTO")); //Non Gateway agent
			scenario.Agents.Add("DEFRA", Helper.CreateCompanyAndBranchProxy("DEFRA"));
			scenario.Agents.Add("ZAJNB", Helper.CreateCompanyAndBranchProxy("ZAJNB"));
			scenario.Agents.Add("ZADUR", Helper.CreateNonProxyBranch("ZADUR")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C7.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "SESTO",
					destination: "DEFRA",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["SESTO"].proxy)
				.SetReceivingAgent(scenario.Agents["DEFRA"].proxy, HandlingType.GatewayAgentWithTariff, "DEFRA"));

			scenario.Consols.Add("C7.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DEFRA",
					destination: "ZAJNB",
					transportMode: TransportModes.Air,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DEFRA"].proxy, HandlingType.GatewayAgentWithTariff, "DEFRA")
				.SetReceivingAgent(scenario.Agents["ZAJNB"].proxy, HandlingType.GatewayAgentWithTariff, "ZAJNB"));

			scenario.Consols.Add("C7.3", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "ZAJNB",
					destination: "ZADUR",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["ZAJNB"].proxy, HandlingType.GatewayAgentWithTariff, "ZAJNB")
				.SetReceivingAgent(scenario.Agents["ZADUR"].proxy));

			scenario.Shipments.Add("S7.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "SESTO",
					destination: "ZADUR",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C7.1"],
					scenario.Consols["C7.2"],
					scenario.Consols["C7.3"]));

			scenario.Shipments.Add("S7.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "SESTO",
					destination: "ZAJNB",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C7.1"],
					scenario.Consols["C7.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS7Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEFRA", Helper.NewIntercompanyTariff(scenario.Agents["DEFRA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				3m,
				origin: "",
				destination: "ZA",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("ZAJNB", Helper.NewIntercompanyTariff(scenario.Agents["ZAJNB"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["ZAJNB"],
				"GWFWD",
				4m,
				origin: "",
				destination: "ZA",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["ZAJNB"],
				"GWIMP",
				5m,
				origin: "",
				destination: "ZA",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS7PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEFRA", Helper.NewIntercompanyTariff(scenario.Agents["DEFRA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "ZA",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("ZAJNB", Helper.NewIntercompanyTariff(scenario.Agents["ZAJNB"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["ZAJNB"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "ZA",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["ZAJNB"],
				"GWIMP",
				5m,
				origin: "",
				destination: "ZA",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S8

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                                        | CHZRH      | DEFRA      | USJFK     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S8.1 (C8.1      )(LCL-CFR)                   | CHZRH      | DEFRA      | .....     |
		  | S8.2 (C8.1, C8.2)(LCL-EXW)                   | CHZRH      |            | USJFK     |
		  | S8.3 (C8.1      )(LCL-EXW)                   |            | DEFRA      | USJFK     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C8.1 (S8.1, S8.2,                 )(ROA-PPD) | CHZRH  (-) | DEFRA  GTT |           |
		  | C8.2 (S8.2, S8.3,                 )(AIR-PPD) |            | DEFRA  GTT | USJFK (-) |
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS8NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS8(fromCache);
			SetupScenarioS8Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS8PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS8(fromCache);
			SetupScenarioS8PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS8(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS8)}";
				return Factory.GetCachedValue(key, SetupScenarioS8Core);
			}

			return SetupScenarioS8Core();
		}

		GatewayScenario SetupScenarioS8Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S8");

			scenario.RoutePorts.AddRange(new[] { "CHZRH", "DEFRA", "USJFK" });

			scenario.Agents.Add("CHZRH", Helper.CreateNonProxyBranch("CHZRH")); //Non Gateway agent
			scenario.Agents.Add("DEFRA", Helper.CreateCompanyAndBranchProxy("DEFRA"));
			scenario.Agents.Add("USJFK", Helper.CreateCompanyAndBranchProxy("USJFK"));

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C8.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "CHZRH",
					destination: "DEFRA",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["CHZRH"].proxy)
				.SetReceivingAgent(scenario.Agents["DEFRA"].proxy, HandlingType.GatewayAgentWithTariff, "DEFRA"));

			scenario.Consols.Add("C8.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DEFRA",
					destination: "USJFK",
					transportMode: TransportModes.Air,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DEFRA"].proxy, HandlingType.GatewayAgentWithTariff, "DEFRA")
				.SetReceivingAgent(scenario.Agents["USJFK"].proxy));

			scenario.Shipments.Add("S8.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "CHZRH",
					destination: "DEFRA",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C8.1"]));

			scenario.Shipments.Add("S8.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "CHZRH",
					destination: "USJFK",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C8.1"],
					scenario.Consols["C8.2"]));

			scenario.Shipments.Add("S8.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "DEFRA",
					destination: "USJFK",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C8.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS8Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEFRA", Helper.NewIntercompanyTariff(scenario.Agents["DEFRA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				3m,
				origin: "",
				destination: "US",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWIMP",
				5m,
				origin: "",
				destination: "DE",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS8PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEFRA", Helper.NewIntercompanyTariff(scenario.Agents["DEFRA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "US",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWIMP",
				5m,
				origin: "",
				destination: "DE",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S9

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                               | DESTR  | DEFRA      | HKHKG      | CNSZX     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S9.1 (C9.1, C9.2)(LCL-EXW)          | DESTR  |            | HKHKG      |           |
		  | S9.2 (C9.2, ....)(LCL-DDP)          |        | DEFRA      |            | CNSZX     |
		  | S9.3 (C9.2,     )(LCL-CFR)          |        | DEFRA      | HKHKG      |           |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C9.1 (S9.1,                       )(ROA-PPD) | DESTR  (-) | DEFRA  GTT |           |
		  | C9.2 (S9.1, S9.2, S9.3            )(AIR-PPD) |            | DEFRA  GTT | HKHKG (-) |
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS9NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS9(fromCache);
			SetupScenarioS9Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS9PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS9(fromCache);
			SetupScenarioS9PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS9(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS9)}";
				return Factory.GetCachedValue(key, SetupScenarioS9Core);
			}

			return SetupScenarioS9Core();
		}

		GatewayScenario SetupScenarioS9Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S9");

			scenario.RoutePorts.AddRange(new[] { "DESTR", "DEFRA", "HKHKG", "CNSZX" });

			scenario.Agents.Add("DESTR", Helper.CreateNonProxyBranch("DESTR")); //Non Gateway agent
			scenario.Agents.Add("DEFRA", Helper.CreateCompanyAndBranchProxy("DEFRA"));
			scenario.Agents.Add("HKHKG", Helper.CreateNonProxyBranch("HKHKG")); //Non Gateway agent
			scenario.Agents.Add("CNSZX", Helper.CreateNonProxyBranch("CNSZX")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C9.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DESTR",
					destination: "DEFRA",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DESTR"].proxy)
				.SetReceivingAgent(scenario.Agents["DEFRA"].proxy, HandlingType.GatewayAgentWithTariff, "DEFRA"));

			scenario.Consols.Add("C9.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "DEFRA",
					destination: "HKHKG",
					transportMode: TransportModes.Sea,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["DEFRA"].proxy, HandlingType.GatewayAgentWithTariff, "DEFRA")
				.SetReceivingAgent(scenario.Agents["HKHKG"].proxy));

			scenario.Shipments.Add("S9.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "DESTR",
					destination: "HKHKG",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C9.1"],
					scenario.Consols["C9.2"]));

			scenario.Shipments.Add("S9.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "DEFRA",
					destination: "CNSZX",
					incoterm: IncoTerms.DeliveredDutyPaid,
					scenario.Consols["C9.2"]));

			scenario.Shipments.Add("S9.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "DEFRA",
					destination: "HKHKG",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C9.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS9Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEFRA", Helper.NewIntercompanyTariff(scenario.Agents["DEFRA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				3m,
				origin: "",
				destination: "HK",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				4m,
				origin: "",
				destination: "CN",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS9PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("DEFRA", Helper.NewIntercompanyTariff(scenario.Agents["DEFRA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "HK",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["DEFRA"],
				"GWFWD",
				4m,
				origin: "",
				destination: "CN",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S11

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                                        | SESTO      | NLAMS      | CLSCL     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S11.1 (C11.1, C11.2)(LCL-EXW)                | SESTO      |            | CLSCL     |
		  | S11.2 (C11.1       )(LCL-DAP)                |            | NLAMS      | CLSCL     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C11.1 (S11.1,                     )(ROA-PPD) | SESTO  GTT | NLAMS  GTT |           |
		  | C11.2 (S11.1 , S11.2              )(AIR-PPD) |            | NLAMS  GTT | CLSCL  GTA|
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS11NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS11(fromCache);
			SetupScenarioS11Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS11PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS11(fromCache);
			SetupScenarioS11PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS11(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS11)}";
				return Factory.GetCachedValue(key, SetupScenarioS11Core);
			}

			return SetupScenarioS11Core();
		}

		GatewayScenario SetupScenarioS11Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S11");

			scenario.RoutePorts.AddRange(new[] { "SESTO", "NLAMS", "CLSCL" });

			scenario.Agents.Add("SESTO", Helper.CreateCompanyAndBranchProxy("SESTO"));
			scenario.Agents.Add("NLAMS", Helper.CreateCompanyAndBranchProxy("NLAMS"));
			scenario.Agents.Add("CLSCL", Helper.CreateCompanyAndBranchProxy("CLSCL"));

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C11.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "SESTO",
					destination: "NLAMS",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["SESTO"].proxy, HandlingType.GatewayAgentWithTariff, "SESTO")
				.SetReceivingAgent(scenario.Agents["NLAMS"].proxy, HandlingType.GatewayAgentWithTariff, "NLAMS"));

			scenario.Consols.Add("C11.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "NLAMS",
					destination: "CLSCL",
					transportMode: TransportModes.Air,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["NLAMS"].proxy, HandlingType.GatewayAgentWithTariff, "NLAMS")
				.SetReceivingAgent(scenario.Agents["CLSCL"].proxy, HandlingType.GatewayAgent, "CLSCL"));

			scenario.Shipments.Add("S11.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "SESTO",
					destination: "CLSCL",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C11.1"],
					scenario.Consols["C11.2"]));

			scenario.Shipments.Add("S11.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "NLAMS",
					destination: "CLSCL",
					incoterm: IncoTerms.DeliveredAtPlace,
					scenario.Consols["C11.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS11Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("SESTO", Helper.NewIntercompanyTariff(scenario.Agents["SESTO"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SESTO"],
				"GWFWD",
				3m,
				origin: "",
				destination: "CL",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("NLAMS", Helper.NewIntercompanyTariff(scenario.Agents["NLAMS"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				4m,
				origin: "",
				destination: "CL",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("CLSCL", Helper.NewIntercompanyTariff(scenario.Agents["CLSCL"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["CLSCL"],
				"GWIMP",
				5m,
				origin: "",
				destination: "CL",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS11PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("SESTO", Helper.NewIntercompanyTariff(scenario.Agents["SESTO"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["SESTO"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "CL",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("NLAMS", Helper.NewIntercompanyTariff(scenario.Agents["NLAMS"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "CL",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("CLSCL", Helper.NewIntercompanyTariff(scenario.Agents["CLSCL"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["CLSCL"],
				"GWIMP",
				5m,
				origin: "",
				destination: "NL",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S12

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                               | NLAMS  | BEBRU      | DEFRA      | RUMOW     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S12.1 (C12.1)(LCL-EXW)              | NLAMS  |            | DEFRA      |           |
		  | S12.2 (C12.2......)(LCL-DDP)        |        | BEBRU      |            | RUMOW     |
		  | S12.3 (C12.1, C12.2    )(LCL-CFR)   | NLAMS  |            |            | RUMOW     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C12.1 (S12.1, S12.3               )(ROA-PPD) | NLAMS  GTT | BEBRU  GTT |           |
		  | C12.2 (S12.1, S12.3               )(AIR-PPD) |            | BEBRU  GTT | RUMOW (-) |
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS12NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS12(fromCache);
			SetupScenarioS12Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS12PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS12(fromCache);
			SetupScenarioS12PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS12(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS12)}";
				return Factory.GetCachedValue(key, SetupScenarioS12Core);
			}

			return SetupScenarioS12Core();
		}

		GatewayScenario SetupScenarioS12Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S12");

			scenario.RoutePorts.AddRange(new[] { "NLAMS", "BEBRU", "DEFRA", "RUMOW" });

			scenario.Agents.Add("NLAMS", Helper.CreateCompanyAndBranchProxy("NLAMS"));
			scenario.Agents.Add("BEBRU", Helper.CreateCompanyAndBranchProxy("BEBRU"));
			scenario.Agents.Add("DEFRA", Helper.CreateCompanyAndBranchProxy("DEFRA"));
			scenario.Agents.Add("RUMOW", Helper.CreateCompanyAndBranchProxy("RUMOW"));

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C12.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "NLAMS",
					destination: "BEBRU",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["NLAMS"].proxy, HandlingType.GatewayAgentWithTariff, "NLAMS")
				.SetReceivingAgent(scenario.Agents["BEBRU"].proxy, HandlingType.GatewayAgentWithTariff, "BEBRU"));

			scenario.Consols.Add("C12.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "BEBRU",
					destination: "RUMOW",
					transportMode: TransportModes.Air,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["BEBRU"].proxy, HandlingType.GatewayAgentWithTariff, "BEBRU")
				.SetReceivingAgent(scenario.Agents["RUMOW"].proxy, HandlingType.GatewayAgentWithTariff, "RUMOW"));

			scenario.Shipments.Add("S12.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "NLAMS",
					destination: "DEFRA",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C12.1"],
					scenario.Consols["C12.2"]));

			scenario.Shipments.Add("S12.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "BEBRU",
					destination: "RUMOW",
					incoterm: IncoTerms.DeliveredDutyPaid,
					scenario.Consols["C12.2"]));

			scenario.Shipments.Add("S12.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "NLAMS",
					destination: "RUMOW",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C12.1"],
					scenario.Consols["C12.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS12Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("NLAMS", Helper.NewIntercompanyTariff(scenario.Agents["NLAMS"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				3m,
				origin: "",
				destination: "DE",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				4m,
				origin: "",
				destination: "RU",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("BEBRU", Helper.NewIntercompanyTariff(scenario.Agents["BEBRU"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEBRU"],
				"GWFWD",
				5m,
				origin: "",
				destination: "DE",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEBRU"],
				"GWFWD",
				6m,
				origin: "",
				destination: "RU",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("RUMOW", Helper.NewIntercompanyTariff(scenario.Agents["RUMOW"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["RUMOW"],
				"GWIMP",
				7m,
				origin: "",
				destination: "RU",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS12PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("NLAMS", Helper.NewIntercompanyTariff(scenario.Agents["NLAMS"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "DE",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "RU",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("BEBRU", Helper.NewIntercompanyTariff(scenario.Agents["BEBRU"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEBRU"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "DE",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["BEBRU"],
				"GWFWD",
				6m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "RU",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("RUMOW", Helper.NewIntercompanyTariff(scenario.Agents["RUMOW"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["RUMOW"],
				"GWIMP",
				7m,
				origin: "",
				destination: "RU",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S16

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                                        | NLAMS      | SGSIN      | IDBTH     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S16.1 (C16.1       )(LCL-EXW)                | NLAMS      | SGSIN      |           |
		  | S16.2 (C16.1, C16.2)(LCL-DAP)                | NLAMS      |            | IDBTH     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C16.1 (S16.1, S16.2               )(AIR-PPD) | NLAMS  GTT | SGSIN  (-) |           |
		  | C16.2 (S16.2                      )(ROA-PPD) |            | SGSIN  (-) | IDBTH  (-)|
		  +----------------------------------------------+------------+------------+-----------+*/

		public GatewayScenario SetupScenarioS16NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS16(fromCache);
			SetupScenarioS16Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS16PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS16(fromCache);
			SetupScenarioS16PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS16(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS16)}";
				return Factory.GetCachedValue(key, SetupScenarioS16Core);
			}

			return SetupScenarioS16Core();
		}

		GatewayScenario SetupScenarioS16Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S16");

			scenario.RoutePorts.AddRange(new[] { "NLAMS", "SGSIN", "IDBTH" });

			scenario.Agents.Add("NLAMS", Helper.CreateCompanyAndBranchProxy("NLAMS"));
			scenario.Agents.Add("SGSIN", Helper.CreateNonProxyBranch("SGSIN")); //Non Gateway agent
			scenario.Agents.Add("IDBTH", Helper.CreateNonProxyBranch("IDBTH")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C16.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "NLAMS",
					destination: "SGSIN",
					transportMode: TransportModes.Air,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["NLAMS"].proxy, HandlingType.GatewayAgentWithTariff, "NLAMS")
				.SetReceivingAgent(scenario.Agents["SGSIN"].proxy));

			scenario.Consols.Add("C16.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "SGSIN",
					destination: "IDBTH",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["SGSIN"].proxy)
				.SetReceivingAgent(scenario.Agents["IDBTH"].proxy));

			scenario.Shipments.Add("S16.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "NLAMS",
					destination: "SGSIN",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C16.1"]));

			scenario.Shipments.Add("S16.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "NLAMS",
					destination: "IDBTH",
					incoterm: IncoTerms.DeliveredAtPlace,
					scenario.Consols["C16.1"],
					scenario.Consols["C16.2"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS16Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("NLAMS", Helper.NewIntercompanyTariff(scenario.Agents["NLAMS"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				3m,
				origin: "",
				destination: "SG",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				4m,
				origin: "",
				destination: "ID",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS16PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("NLAMS", Helper.NewIntercompanyTariff(scenario.Agents["NLAMS"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "SG",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["NLAMS"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "ID",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		#region Scenario S18

		/*+----------------------------------------------+------------+------------+-----------+
		  | Route                               | CNSZX  | HKHKG      | USMIA      | ARBUE     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Shipments                                    |            |            |           |
		  | S18.1 (C18.1, C18.2, C18.3)(LCL-EXW)| CNSZX  |            |            | ARBUE     |
		  | S18.2 (C18.1, C18.2)       (LCL-DDP)| CNSZX  |            | USMIA      |           |
		  | S18.3 (C18.2       )       (LCL-CFR)|        | HKHKG      | USMIA      |           |
		  | S18.4 (C18.3       )       (LCL-CFR)|        |            | USMIA      | ARBUE     |
		  +----------------------------------------------+------------+------------+-----------+
		  | Consols                                      |            |            |           |
		  | C18.1 (S18.1, S18.2               )(ROA-PPD) | CNSZX  (-) | HKHKG  GTA |           |
		  | C18.2 (S18.1, S18.2, S18.3        )(AIR-PPD) | HKHKG  GTA | USMIA  GTA |           |
		  | C18.3 (S18.1, S.18.4              )(AIR-PPD) |            | USMIA  GTA | ARBUE (-) |
		  +----------------------------------------------+------------+------------+-----------+*/
		public GatewayScenario SetupScenarioS18NormalRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS18(fromCache);
			SetupScenarioS18Rate(gatewayScenario);
			return gatewayScenario;
		}

		public GatewayScenario SetupScenarioS18PLPDRates(bool fromCache = true)
		{
			var gatewayScenario = SetupScenarioS18(fromCache);
			SetupScenarioS18PLPDRate(gatewayScenario);
			return gatewayScenario;
		}

		GatewayScenario SetupScenarioS18(bool fromCache)
		{
			if (fromCache)
			{
				var key = $"{GetType().FullName}.{nameof(SetupScenarioS18)}";
				return Factory.GetCachedValue(key, SetupScenarioS18Core);
			}

			return SetupScenarioS18Core();
		}

		GatewayScenario SetupScenarioS18Core()
		{
			var scenario = new GatewayScenario("WI00317849 - S18");

			scenario.RoutePorts.AddRange(new[] { "CNSZX", "HKHKG", "USMIA", "ARBUE" });

			scenario.Agents.Add("CNSZX", Helper.CreateNonProxyBranch("CNSZX")); //Non Gateway agent
			scenario.Agents.Add("HKHKG", Helper.CreateCompanyAndBranchProxy("HKHKG"));
			scenario.Agents.Add("USMIA", Helper.CreateCompanyAndBranchProxy("USMIA"));
			scenario.Agents.Add("ARBUE", Helper.CreateNonProxyBranch("ARBUE")); //Non Gateway agent

			scenario.ChargeCodes.Add("GWFWD", Helper.ChargeCodes.CreateGlobalCharge("GWFWD"));
			scenario.ChargeCodes["GWFWD"].AC_IsGroupageCharge = false; // Non-Consol Level Charge

			scenario.ChargeCodes.Add("GWIMP", Helper.ChargeCodes.CreateGlobalCharge("GWIMP"));
			scenario.ChargeCodes["GWIMP"].AC_IsGroupageCharge = true; // Consol Level Charge

			Factory.Save();

			scenario.Consols.Add("C18.1", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "CNSZX",
					destination: "HKHKG",
					transportMode: TransportModes.Road,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["CNSZX"].proxy)
				.SetReceivingAgent(scenario.Agents["HKHKG"].proxy, HandlingType.GatewayAgent, "HKHKG"));

			scenario.Consols.Add("C18.2", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "HKHKG",
					destination: "USMIA",
					transportMode: TransportModes.Air,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(sendingAgent: scenario.Agents["HKHKG"].proxy, HandlingType.GatewayAgent, "HKHKG")
				.SetReceivingAgent(scenario.Agents["USMIA"].proxy, HandlingType.GatewayAgent, "USMIA"));

			scenario.Consols.Add("C18.3", Helper
				.CreateForwardingConsolWithGatewayAgents(
					origin: "USMIA",
					destination: "ARBUE",
					transportMode: TransportModes.Air,
					consolMode: ContainerModes.FCL,
					paymentType: PaymentType.Prepaid)
				.SetSendingAgent(scenario.Agents["USMIA"].proxy, HandlingType.GatewayAgent, "USMIA")
				.SetReceivingAgent(scenario.Agents["ARBUE"].proxy));

			scenario.Shipments.Add("S18.1",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "CNSZX",
					destination: "ARBUE",
					incoterm: IncoTerms.ExWorks,
					scenario.Consols["C18.1"],
					scenario.Consols["C18.2"],
					scenario.Consols["C18.3"]));

			scenario.Shipments.Add("S18.2",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "CNSZX",
					destination: "USMIA",
					incoterm: IncoTerms.DeliveredDutyPaid,
					scenario.Consols["C18.1"],
					scenario.Consols["C18.2"]));

			scenario.Shipments.Add("S18.3",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "HKHKG",
					destination: "USMIA",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C18.2"]));

			scenario.Shipments.Add("S18.4",
				Helper.CreateGatewayShipmentWithMultipleGatewayConsols(
					transportMode: TransportModes.Sea,
					origin: "USMIA",
					destination: "ARBUE",
					incoterm: IncoTerms.CostAndFreight,
					scenario.Consols["C18.3"]));

			Factory.Save();

			CreateJobsForEachShipmentsInEveryBranches(scenario);

			return scenario;
		}

		void SetupScenarioS18Rate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("HKHKG", Helper.NewIntercompanyTariff(scenario.Agents["HKHKG"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				3m,
				origin: "",
				destination: "AR",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				4m,
				origin: "",
				destination: "US",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("USMIA", Helper.NewIntercompanyTariff(scenario.Agents["USMIA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["USMIA"],
				"GWFWD",
				5m,
				origin: "",
				destination: "AR",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["USMIA"],
				"GWIMP",
				6m,
				origin: "",
				destination: "US",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgentForImport,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		void SetupScenarioS18PLPDRate(GatewayScenario scenario)
		{
			scenario.IntercompanyTariffs.Add("HKHKG", Helper.NewIntercompanyTariff(scenario.Agents["HKHKG"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				3m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "AR",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["HKHKG"],
				"GWFWD",
				4m,
				origin: "",
				destination: "",
				plannedLoad: "",
				plannedDischarge: "US",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			scenario.IntercompanyTariffs.Add("USMIA", Helper.NewIntercompanyTariff(scenario.Agents["USMIA"].proxy));
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["USMIA"],
				"GWFWD",
				5m,
				origin: "",
				destination: "",
				plannedLoad: "AR",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.SendingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);
			Helper.CreateFlatRate(
				scenario.IntercompanyTariffs["USMIA"],
				"GWIMP",
				6m,
				origin: "",
				destination: "US",
				plannedLoad: "",
				plannedDischarge: "",
				gatewayAgentType: GatewayAgentType.Codes.ReceivingAgent,
				rateCategory: RateCategory.LCL,
				rateMode: RateMode.LCL);

			Factory.Save();
		}

		#endregion

		void CreateJobsForEachShipmentsInEveryBranches(GatewayScenario scenario)
		{
			var shipments = scenario.Shipments.Values.ToArray();
			var branches = scenario.Agents.Select(x => x.Value.branch).Distinct();

			foreach (var branch in branches)
			{
				using (EnvProxy.Instance.SetTemporaryUserContext(EnvProxy.Instance.CurrentUser.LoginName, branch.PK.ToGuid(), EnvProxy.Instance.CurrentDepartment.PK))
				{
					foreach (var shipment in shipments)
					{
						new JobHeader.Loader(shipment).TryCreateWithMutex(branch);
					}
					Factory.Save();
				}
			}
		}

		public class GatewayScenario
		{
			internal GatewayScenario(string name)
			{
				Name = name;

				RoutePorts = new List<string>();
				Agents = new Dictionary<string, (OrgHeader proxy, GlbBranch branch)>();
				Consols = new Dictionary<string, ForwardingConsol>();
				Shipments = new Dictionary<string, ForwardingShipment>();
				ChargeCodes = new Dictionary<string, AccChargeCode>();
				IntercompanyTariffs = new Dictionary<string, IntercompanyTariff>();
				Costings = new Dictionary<string, Costing>();
				ClientRates = new Dictionary<string, ClientRate>();
			}

			public string Name { get; }

			public List<string> RoutePorts { get; }
			public Dictionary<string, (OrgHeader proxy, GlbBranch branch)> Agents { get; }

			public Dictionary<string, AccChargeCode> ChargeCodes { get; }

			public Dictionary<string, IntercompanyTariff> IntercompanyTariffs { get; }
			public Dictionary<string, Costing> Costings { get; }
			public Dictionary<string, ClientRate> ClientRates { get; }

			public Dictionary<string, ForwardingConsol> Consols { get; }
			public Dictionary<string, ForwardingShipment> Shipments { get; }
		}

		public GatewayTestHelper Helper => helper ?? (helper = new GatewayTestHelper(ParentTestCase, Factory));
		GatewayTestHelper helper;

		BaseGatewayBillingIntegrationTest ParentTestCase { get; }
		BusinessObjectFactory Factory { get; }
	}
}
