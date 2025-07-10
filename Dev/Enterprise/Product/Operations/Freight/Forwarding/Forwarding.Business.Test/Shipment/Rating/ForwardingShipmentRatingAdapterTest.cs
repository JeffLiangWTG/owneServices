using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.Business.Testing;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Rating.Integration;
using Enterprise.Rating.Rateable;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Schema;
using FluentAssertions;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingShipmentRatingAdapterTest : TestCaseWithFactory
	{
		public void TestUpdateClientContractNumber()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();

			// blank + BBB = BBB
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "",
				newNumbers: new[] { "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// AAA + BBB = AAA
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "AAA",
				newNumbers: new[] { "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// AAA + AAA = AAA
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "AAA",
				newNumbers: new[] { "AAA" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + empty => AAA
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "AAA",
				newNumbers: Array.Empty<string>(),
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + blank => AAA
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "AAA",
				newNumbers: new[] { "" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// AAA + many = AAA
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "AAA",
				newNumbers: new[] { "", "BBB", "CCC" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "AAA");

			// blank + BBB x 3 = BBB
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "",
				newNumbers: new[] { "BBB", "BBB", "BBB" },
				expectedResult: DataUpdateResult.Updated,
				expectedNumber: "BBB");

			// blank + multiple = error
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "",
				newNumbers: new[] { "", "BBB", "CCC" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");

			// blank + blank = blank
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "",
				newNumbers: Array.Empty<string>(),
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");

			// blank + blank = blank
			AssertUpdateClientContractNumber(
				shipment,
				existingNumber: "",
				newNumbers: new[] { "" },
				expectedResult: DataUpdateResult.NoAction,
				expectedNumber: "");
		}

		static void AssertUpdateClientContractNumber(
			ForwardingShipment shipment,
			string existingNumber,
			IEnumerable<string> newNumbers,
			DataUpdateResult expectedResult,
			string expectedNumber)
		{
			var jobHeader = new JobHeader.Loader(shipment).TryLoadOrCreate();
			if (existingNumber != null)
			{
				jobHeader.JH_ClientContractNumber = existingNumber;
			}
			var jobDataUpdater = (IJobDataUpdater)shipment.RatingAdapter;

			ErrorReporter.Clear();
			var result = jobDataUpdater.UpdateClientContractNumber(newNumbers);

			var actualHasError = ErrorReporter.TotalErrorCount > 0;

			CombineAssertions(() =>
			{
				AssertEquals("Should update successfully", expectedResult, result);
				AssertEquals("Number", expectedNumber, jobHeader.JH_ClientContractNumber);
			});
		}

		public void TestRateOriginRateDestination()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKFreightRateOrigin = "NZAKL";
			shipment.JS_RL_NKFreightRateDestination = "NZAWT";

			var adapterToTest = shipment.RatingAdapter;

			AssertEquals("NZAKL", adapterToTest.RateOrigin.Code);
			AssertEquals("NZAWT", adapterToTest.RateDestination.Code);
		}

		public void TestPlannedLoadPlannedDischarge()
		{
			var costSell = new CostSell();
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKLoadPort = "CNAJG";
			shipment.JS_RL_NKDischargePort = "CNANI";

			var adapterToTest = shipment.RatingAdapter;

			AssertEquals("CNAJG", adapterToTest.PlannedLoad(costSell).Code);
			AssertEquals("CNANI", adapterToTest.PlannedDischarge(costSell).Code);
		}

		#region IGateway

		public void TestGatewayAgents()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			var orgHeader1 = Factory.NewWithValidTestData<OrgHeader>();
			var orgHeader2 = Factory.NewWithValidTestData<OrgHeader>();

			var gateway2 = shipment.Gateways.AddNew();
			gateway2.JSG_OA_ForwarderAddress = orgHeader2.MainAddress.PK;
			gateway2.JSG_Sequence = 2;

			var gateway1 = shipment.Gateways.AddNew();
			gateway1.JSG_OA_ForwarderAddress = orgHeader1.MainAddress.PK;
			gateway1.JSG_Sequence = 1;

			var adapterToTest = (IGateway)shipment.RatingAdapter;

			AssertNotNull(adapterToTest);
			AssertEquals("We should have two gateways on shipment", 2, adapterToTest.SortedGatewayAgentPKs.Count);
			AssertEquals("gateway with smallest sequence number should be the first one", gateway1.ForwarderPK, adapterToTest.SortedGatewayAgentPKs[0]);
		}

		public void TestGetNonGatewayOrgPKs_WhenLocationIsInvalid_NoExceptionIsThrown()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "OLAUD"; // invalid location
			shipment.JS_RL_NKDestination = "USLAX";

			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var gateway = shipment.Gateways.AddNew();
			gateway.JSG_OA_ForwarderAddress = orgHeader.MainAddress.PK;
			gateway.JSG_Sequence = 1;

			var adapter = (IGateway)shipment.RatingAdapter;

			AssertNoExceptionThrown(() => _ = adapter.LoginGatewayAgentRoles);
		}

		public void TestIGateway_ContinueWithDefaultCosting_Export()
		{
			#region Logic Reference
			/*
			GIVEN
				Shipment is Export (User Login Company > Country = Shipment > Origin > Country) AND
				Shipment > Consol > Sending Agent and Receiving Agent and User Login Company under same Country AND
				Shipment > Consol > Sending Agent and Receiving Agent under different Companies AND
				Shipment > Consol > Sending Agent is NOT G/W AND
				Shipment > Consol > Receiving Agent is G/W AND
				User Login Company is under the same Company of Shipment > Consol > Receiving Agent
			WHEN
				Shipment > Job Invoicing > Cost Autoration 
				(multiple menu option can trigger Autoration of Costing)
			THEN
				No cost from Costing or Intercompany Tariffs expected

			Ref: PRJ00034557 to describe the Autorating behavior of Shipment engaging G/W services.
			*/
			#endregion

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, branch2) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, _) = CreateCompanyAndBranchProxy("USCHS");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var consol1 = CreateForwardingConsolWithGatewayAgents(
				"CHZUR", "CHBSL",
				agent1, "",
				agent2, AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol2 = CreateForwardingConsolWithGatewayAgents(
				"CHBSL", "USCHS",
				agent2, AgentStatusList.Codes.GatewayAgentWithTariff,
				agent3, AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol3 = CreateForwardingConsolWithGatewayAgents(
				"USCHS", "USHOU",
				agent3, AgentStatusList.Codes.GatewayAgentWithTariff,
				agent4, "");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CHZUR";
			shipment.JS_RL_NKDestination = "USHOU";
			shipment.Gateways.AddNew().ForwarderPK = agent2.PK;
			shipment.Gateways.AddNew().ForwarderPK = agent3.PK;

			var nonMiscDepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA")).PK.ToGuid();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch2.PK.ToGuid(), nonMiscDepartmentPK))
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Assumptions", () =>
				{
					AssertEquals("shipment.JobDirection", Directions.Export, shipment.JobDirection);
					AssertGreaterThan("Gateways count", shipment.Gateways.Count, 0);
				});

				var gateway = (IGateway)shipment.RatingAdapter;

				AssertEquals("There is no Consol attached to the Shipment yet", true, gateway.ContinueWithDefaultCosting(BillingType.Invoicing));

				shipment.Consols.AddRange(consol2, consol3);
				AssertEquals("Attached Consols (consol2 & consol3) don't meet the criteria", true, gateway.ContinueWithDefaultCosting(BillingType.Invoicing));

				shipment.Consols.AddRange(consol1);
				AssertEquals("Consol1 attached to the Shipment, which meet the criteria", false, gateway.ContinueWithDefaultCosting(BillingType.Invoicing));
			}
		}

		public void TestIGateway_ContinueWithDefaultCosting_Import()
		{
			#region Logic Reference
			/*
			GIVEN
				Shipment is Import (User Login Company > Country = Shipment > Destination > Country) AND
				Shipment > Consol > Sending Agent and Receiving Agent and User Login Company under same Country AND
				Shipment > Consol > Sending Agent and Receiving Agent under different Companies AND
				Shipment > Consol > Sending Agent is G/W AND
				Shipment > Consol > Receiving Agent is NOT G/W AND
				User Login Company is under the same Company of Shipment > Consol > Sending Agent
			WHEN
				Shipment > Job Invoicing > Cost Autoration 
				(multiple menu option can trigger Autoration of Costing)
			THEN
				No cost from Costing or Intercompany Tariffs expected

			Ref: PRJ00034557 to describe the Autorating behavior of Shipment engaging G/W services.
			*/
			#endregion

			var (agent1, _) = CreateCompanyAndBranchProxy("CHZUR");
			var (agent2, _) = CreateCompanyAndBranchProxy("CHBSL");
			var (agent3, branch3) = CreateCompanyAndBranchProxy("USCHS");
			var (agent4, _) = CreateCompanyAndBranchProxy("USHOU");

			var consol1 = CreateForwardingConsolWithGatewayAgents(
				"CHZUR", "CHBSL",
				agent1, "",
				agent2, AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol2 = CreateForwardingConsolWithGatewayAgents(
				"CHBSL", "USCHS",
				agent2, AgentStatusList.Codes.GatewayAgentWithTariff,
				agent3, AgentStatusList.Codes.GatewayAgentWithTariff);

			var consol3 = CreateForwardingConsolWithGatewayAgents(
				"USCHS", "USHOU",
				agent3, AgentStatusList.Codes.GatewayAgentWithTariff,
				agent4, "");

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CHZUR";
			shipment.JS_RL_NKDestination = "USHOU";
			shipment.Gateways.AddNew().ForwarderPK = agent2.PK;
			shipment.Gateways.AddNew().ForwarderPK = agent3.PK;

			var nonMiscDepartmentPK = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GEA")).PK.ToGuid();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, branch3.PK.ToGuid(), nonMiscDepartmentPK))
			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				CombineAssertions("Assumptions", () =>
				{
					AssertEquals("shipment.JobDirection", Directions.Import, shipment.JobDirection);
					AssertGreaterThan("Gateways count", shipment.Gateways.Count, 0);
				});

				var gateway = (IGateway)shipment.RatingAdapter;

				AssertEquals("There is no Consol attached to the Shipment yet", true, gateway.ContinueWithDefaultCosting(BillingType.Invoicing));

				shipment.Consols.AddRange(consol1, consol2);
				AssertEquals("Attached Consols (consol1 & consol2) don't meet the criteria", true, gateway.ContinueWithDefaultCosting(BillingType.Invoicing));

				shipment.Consols.AddRange(consol3);
				AssertEquals("Consol3 attached to the Shipment, which meet the criteria", false, gateway.ContinueWithDefaultCosting(BillingType.Invoicing));
			}
		}

		(OrgHeader proxy, GlbBranch branch) CreateCompanyAndBranchProxy(string unloco)
		{
			var countryCode = unloco.Substring(0, 2);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Name = countryCode + " Company";
			company.GC_RN_NKCountryCode = countryCode;

			var newBranch = company.Branches.AddNew();
			newBranch.GB_Code = company.GC_Code;
			newBranch.GB_BranchName = unloco + " Branch";
			newBranch.GB_RL_NKHomePort = unloco;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUserPK, newBranch.PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var proxy = Factory.NewWithValidTestData<OrgHeader>();
				proxy.OH_Code = "PROXY" + unloco;
				proxy.OH_IsCreditor = true;
				proxy.CompanyData.SetAPTaxApplicable(false);
				proxy.OH_IsDebtor = true;
				proxy.MainAddress.OA_RL_NKRelatedPortCode = unloco;
				newBranch.GB_OH_OrgProxy = proxy.PK;

				return (proxy, newBranch);
			}
		}

		ForwardingConsol CreateForwardingConsolWithGatewayAgents(
			string origin,
			string destination,
			OrgHeader sendingAgent,
			string sendingAgentType,
			OrgHeader receivingAgent,
			string receivingAgentType)
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Air;
			consol.JK_ConsolMode = ContainerModes.Loose;
			consol.JK_RL_NKLoadPort = origin;
			consol.JK_RL_NKDischargePort = destination;
			consol.JK_PrepaidCollect = PaymentType.Collect;

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = origin;
			transport.JW_RL_NKDiscPort = destination;
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_VoyageFlight = "QF105";

			if (sendingAgent != null)
			{
				consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;

				if (!string.IsNullOrWhiteSpace(sendingAgentType))
				{
					consol.JK_SendingForwarderHandlingType = sendingAgentType;
					SetUpAppointedGatewayAgentPorts(consol.SendingForwarderAddress, origin, sendingAgentType);
				}
			}

			if (receivingAgent != null)
			{
				consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;

				if (!string.IsNullOrWhiteSpace(receivingAgentType))
				{
					consol.JK_ReceivingForwarderHandlingType = receivingAgentType;
					SetUpAppointedGatewayAgentPorts(consol.ReceivingForwarderAddress, destination, receivingAgentType);
				}
			}

			consol.JK_AgentType = AgentType.Agent;
			return consol;
		}

		void SetUpAppointedGatewayAgentPorts(OrgAddress gatewayAgentAddress, ZString location, string handlingType)
		{
			var gatewayAgent = gatewayAgentAddress.Header;
			var agentPorts =
				gatewayAgent.AppointedGatewayAgentPorts.Cast<OrgAppointedAgentPorts>().FirstOrDefault(x => x.O5_PortOrCountry == location)
				?? gatewayAgent.AppointedGatewayAgentPorts.AddNew();
			agentPorts.O5_OA_AgentOfficeAddress = gatewayAgentAddress.PK;
			agentPorts.O5_PortOrCountry = location;
			agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			agentPorts.O5_SeaAgentStatus = handlingType;
			agentPorts.O5_AirAgentStatus = handlingType;
			agentPorts.O5_RailAgentStatus = handlingType;
			agentPorts.O5_RoadAgentStatus = handlingType;
		}

		public void TestIGateway_ShouldRemoveNonIntercompanyTariffFRTEntries()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_RL_NKOrigin = "CHZUR";
			shipment.JS_RL_NKDestination = "USHOU";

			var gateway = (IGateway)shipment.RatingAdapter;

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Apportionment));
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Invoicing));
			}

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.Switzerland))
			{
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Apportionment));
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Invoicing));

				shipment.Gateways.AddNew();
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Apportionment));
				AssertEquals(true, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Invoicing));

				shipment.Gateways.DeleteAll();

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = TransportModes.Air;
				consol.JK_RL_NKLoadPort = shipment.JS_RL_NKOrigin;
				consol.JK_RL_NKDischargePort = shipment.JS_RL_NKDestination;
				consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var agentPorts = consol.SendingForwarderAddress.Header.AppointedGatewayAgentPorts.AddNew();
				agentPorts.O5_OA_AgentOfficeAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
				agentPorts.O5_PortOrCountry = CountryCodes.Switzerland;
				agentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
				agentPorts.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

				Factory.Save();

				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Invoicing));

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				AssertEquals(true, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Invoicing));
			}
		}

		public void TestIGateway_GatewayAgentTypeFilteredReason_forSendingForwarder()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gateway = (IGateway)shipment.RatingAdapter;

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Apportionment));

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var sendingForwarder = Factory.NewWithValidTestData<OrgHeader>();

				consol.JK_OA_SendingForwarderAddress = sendingForwarder.MainAddress.PK;
				AssertNotEquals("Pre-condition: consol.SendingForwarderPK", consol.SendingForwarderPK, Guid.Empty);

				shipment.Consols.Add(consol);

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				consol.JK_PrepaidCollect = PaymentType.Prepaid;
				AssertEquals(string.Empty, gateway.GatewayAgentTypeFilteredReason(GatewayAgentType.Codes.SendingAgent, sendingForwarder.PK, BillingType.Invoicing, CostSell.Cost));
				AssertEquals("Gateway agent type RAG cannot be used for shipment autorating because of sending agent of attached Consol 26FYS0PM3GCY3VHFAX26 with following payment PPD",
					gateway.GatewayAgentTypeFilteredReason(GatewayAgentType.Codes.ReceivingAgent, sendingForwarder.PK, BillingType.Invoicing, CostSell.Cost));

				consol.JK_PrepaidCollect = PaymentType.Collect;
				AssertEquals("Gateway agent type SAG cannot be used for shipment autorating because of sending agent of attached Consol 26FYS0PM3GCY3VHFAX26 with following payment CCX",
					gateway.GatewayAgentTypeFilteredReason(GatewayAgentType.Codes.SendingAgent, sendingForwarder.PK, BillingType.Invoicing, CostSell.Cost));
				AssertEquals("Gateway agent type RAG cannot be used for shipment autorating because of sending agent of attached Consol 26FYS0PM3GCY3VHFAX26 with following payment CCX",
					gateway.GatewayAgentTypeFilteredReason(GatewayAgentType.Codes.ReceivingAgent, sendingForwarder.PK, BillingType.Invoicing, CostSell.Cost));
			}
		}

		public void TestIGateway_GatewayAgentTypeFilteredReason_ForReceivingForwarder()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gateway = (IGateway)shipment.RatingAdapter;

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Apportionment));

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var receivingForwarder = Factory.NewWithValidTestData<OrgHeader>();

				consol.JK_OA_ReceivingForwarderAddress = receivingForwarder.MainAddress.PK;
				AssertNotEquals("Pre-condition: consol.ReceivingForwarderPK", consol.ReceivingForwarderPK, Guid.Empty);

				shipment.Consols.Add(consol);

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				consol.JK_PrepaidCollect = PaymentType.Prepaid;
				AssertEquals("Gateway agent type RAG cannot be used for shipment autorating because of receiving agent of attached Consol 26FYS0PM3GCY3VHFAX26 with following payment PPD",
					gateway.GatewayAgentTypeFilteredReason(GatewayAgentType.Codes.ReceivingAgent, receivingForwarder.PK, BillingType.Invoicing, CostSell.Cost));

				consol.JK_PrepaidCollect = PaymentType.Collect;
				AssertEquals(string.Empty, gateway.GatewayAgentTypeFilteredReason(GatewayAgentType.Codes.ReceivingAgent, receivingForwarder.PK, BillingType.Invoicing, CostSell.Cost));
			}
		}

		public void TestIGateway_GatewayAgentTypeFilteredReason()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var gateway = (IGateway)shipment.RatingAdapter;
			var agentType = "test";
			var gatewayAgentPk = new ZGuid();

			AssertEquals(string.Empty, gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, BillingType.Invoicing, CostSell.Revenue));

			AssertContains
			(
				$"Gateway agent type {agentType} cannot be used for shipment autorating",
				gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, BillingType.Invoicing, CostSell.Cost)
			);

			agentType = GatewayAgentType.Codes.SendingAgent;
			AssertEquals(string.Empty, gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, BillingType.Invoicing, CostSell.Cost));

			agentType = string.Empty;
			AssertEquals(string.Empty, gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgentPk, BillingType.Invoicing, CostSell.Cost));

			using (RatingDataRegistry.Instance.UseIntercompanyTariffsToAutorateGatewayBilling.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertEquals(false, gateway.ShouldRemoveNonIntercompanyTariffFRTEntries(BillingType.Apportionment));

				var consol = Factory.NewWithValidTestData<ForwardingConsol>();
				var gatewayAgent = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = gatewayAgent.MainAddress.PK;

				AssertNotEquals("Pre-condition: consol.SendingForwarderPK", consol.SendingForwarderPK, Guid.Empty);

				shipment.Consols.Add(consol);
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();
				consol1.JK_OA_ReceivingForwarderAddress = gatewayAgent.MainAddress.PK;
				AssertNotEquals("Pre-condition: consol.ReceivingForwarderPK", consol1.ReceivingForwarderPK, Guid.Empty);

				shipment.Consols.Add(consol1);
				consol1.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				agentType = GatewayAgentType.Codes.ReceivingAgent;
				AssertEquals("Gateway agent type RAG cannot be used for shipment autorating because of receiving agent of attached Consol LH368W538T2QSF50OJ3K with following payment ",
					gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgent.PK, BillingType.Invoicing, CostSell.Cost));

				agentType = GatewayAgentType.Codes.SendingAgent;
				AssertEquals(string.Empty, gateway.GatewayAgentTypeFilteredReason(agentType, gatewayAgent.PK, BillingType.Invoicing, CostSell.Cost));
			}
		}

		public void TestIGateway()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var adapter = shipment.RatingAdapter;
			var adapterAsGateway = (IGateway)adapter;

			AssertNull(adapterAsGateway.GatewayBillingSupporter);
			AssertContainsExactElementsInAnyOrder(adapterAsGateway.GatewayAgentPKsForIntercompanyTariff, adapterAsGateway.SortedGatewayAgentPKs);

			AssertContainsExactElementsInAnyOrder(new List<LocationWithSource>(), adapterAsGateway.SortedOverridenPlannedLoad);
			AssertContainsExactElementsInAnyOrder(new List<LocationWithSource>(), adapterAsGateway.SortedOverridenPlannedDischarge);

			Assert("should be true for shipment adapter", adapterAsGateway.IsContainerNegotiatedCostApplicable(CostSell.Cost));
			Assert("should be true for shipment adapter", adapterAsGateway.IsContainerNegotiatedCostApplicable(CostSell.Revenue));
			Assert("should be true for shipment adapter", adapterAsGateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Cost));
			Assert("should be true for shipment adapter", adapterAsGateway.IsGatewaySellApplicableToGatewayConsol(CostSell.Revenue));
		}

		#endregion

		public void TestGetContractNumberConfiguration_ShouldAddContractNumberQueryFilter()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(configuration.ShouldAddContractNumberQueryFilter);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(configuration.ShouldAddContractNumberQueryFilter);
		}

		public void TestGetContractNumberConfiguration_ShouldApplySpecificAdapterContractNumberFilterFlag_ShipmentNoConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(!configuration.ShouldApplySpecificAdapterContractNumberFilter);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);
		}

		public void TestGetContractNumberConfiguration_ShouldApplySpecificAdapterContractNumberFilterFlag_ShipmentWithConsol()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.Consols.AddNew();
			var ratingAdapter = shipment.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(configuration.ShouldApplySpecificAdapterContractNumberFilter);
		}

		public void TestGetContractNumberConfiguration_ShouldIgnoreJobCarrierContractNumbers()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(!configuration.ShouldIgnoreJobCarrierContractNumbers);
		}

		public void TestGetContractNumberConfiguration_ShouldIgnoreJobClientContractNumbersFlag()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(!configuration.ShouldIgnoreJobClientContractNumbers);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(!configuration.ShouldIgnoreJobClientContractNumbers);
		}

		public void TestGetContractNumberConfiguration_ShouldMatchJobBlankContractNumberFlag()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(!configuration.ShouldMatchJobBlankContractNumber);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(!configuration.ShouldMatchJobBlankContractNumber);
		}

		public void TestGetContractNumberConfiguration_ShouldUseCarrierContractDateFilterFlag()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var ratingAdapter = shipment.RatingAdapter;

			var configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Cost);
			Assert(!configuration.ShouldUseCarrierContractDateFilter);

			configuration = ratingAdapter.GetContractNumberConfiguration(CostSell.Revenue);
			Assert(!configuration.ShouldUseCarrierContractDateFilter);
		}

		public void TestAircraftType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var adapterToTest = shipment.RatingAdapter as ForwardingShipmentRatingAdapter;

			AssertEquals(ZString.Empty, adapterToTest.AircraftType);

			var shipmentLeg1 = shipment.Transports.AddNew("AUSYD", "AUPER");
			shipmentLeg1.JW_TransportMode = TransportModes.Air;
			shipmentLeg1.JW_IsCargoOnly = true;

			var shipmentLeg2 = shipment.Transports.AddNew("AUPER", "USLAX");
			shipmentLeg2.JW_TransportMode = TransportModes.Air;
			shipmentLeg2.JW_IsCargoOnly = true;

			AssertEquals(AircraftType.CAO, adapterToTest.AircraftType);

			shipmentLeg2.JW_IsCargoOnly = false;

			AssertEquals(AircraftType.PAX, adapterToTest.AircraftType);
		}

		public void TestMeasures_WeightAndVolumeCommodity_WithContainerCommodity_NoOverrides()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var gp20 = Factory.LoadTop1<RefContainer>(new ZQuery(RefContainerSchema.RC_Code, "20GP"));
			var container = consol.Containers.AddNew();
			container.JC_RC = gp20.PK;
			container.JC_ContainerCount = 5;
			container.JC_IsGrossWeightOverridden = true;
			container.JC_TareWeight = 2000;
			container.JC_DunnageWeight = 200;
			container.JC_GrossWeight = 1000;
			container.JC_ContainerMode = ContainerModes.FCL;
			container.JC_RH_NKContainerCommodityCode = "COAL";

			var shipment = consol.Shipments.AddNew();
			shipment.AddPackLine(commodity: "SHIP");
			// packline is autopacked to the 1 container.

			var adapter = shipment.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			measures
				.GetPartList(MeasureType.Weight)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "It is the packline commodity");
			measures
				.GetPartList(MeasureType.Volume)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "It is the packline commodity");
			measures
				.GetPartList(MeasureType.ContainerCount)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "COAL" }, because: "It is the container commodity");
			measures.GetCommodities().Should().BeEquivalentTo(new[] { "SHIP", "COAL" }, because: "Both packline and container commodity applies");
			Assert("Fluent assertion in use", true);
		}

		public void TestMeasures_WeightAndVolumeCommodity_WithNoContainerCommodity_NoOverride()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = TransportModes.Sea;
			consol.JK_ConsolMode = ContainerModes.LCL;
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";

			var shipment = consol.Shipments.AddNew();

			consol.AddContainer(count: 5, commodity: null, packLines: new[]
			{
				shipment.AddPackLine(commodity: "SHIP")
			});

			var adapter = shipment.RatingAdapter;
			var measures = (RateableMeasureSet)adapter.RateableMeasures;

			measures
				.GetPartList(MeasureType.Weight)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "It is the packline commodity");
			measures
				.GetPartList(MeasureType.Volume)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "It is the packline commodity");
			measures
				.GetPartList(MeasureType.ContainerCount)
				.Select(x => x.CommodityCode)
				.Should().BeEquivalentTo(new[] { "SHIP" }, because: "The container inherits the packline commodity");
			measures.GetCommodities().Should().BeEquivalentTo(new[] { "SHIP" }, because: "Packline commodity applies");
			Assert("Fluent assertion in use", true);
		}

		public void TestHBLDeliveryMode()
		{
			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.JS_HBLContainerPackModeOverride = "DOOR/DOOR";

			var adapterToTest = shipment.RatingAdapter;

			AssertEquals("DOOR/DOOR", adapterToTest.HBLDeliveryMode);
		}
	}
}
