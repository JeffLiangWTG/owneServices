using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ContractManagement.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CarbonEmissions.Business;
using Enterprise.Freight.CarbonEmissions.Business.Testing;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Forwarding.AWB.Business;
using Enterprise.Freight.Forwarding.Business.AWB.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.Freight.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class ForwardingConsolValidationTest : BaseFreightTest
	{
		#region JK Fields

		string ReceivingAgentGatewayMessage => "Receiving Agent on this Consol is flagged as \"Gateway\", the Consol type must be either AGT, CLA, CLD, CLM, or DRT.";
		string SendingAgentGatewayMessage => "Sending Agent on this Consol is flagged as \"Gateway\", the Consol type must be either AGT, CLA, CLD, CLM, or DRT.";

		#region JK_SendingForwarderHandlingType

		public void TestValidateJK_SendingForwarderHandlingType()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";

			var orgProxyAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertHasError("SendingForwarderAddress is not a valid Gateway Agent", consol.JK_SendingForwarderHandlingTypeInfo, @"Sending Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

			var sendingForwarderAppointedPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			sendingForwarderAppointedPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			sendingForwarderAppointedPort.O5_PortOrCountry = "AUBNE";
			sendingForwarderAppointedPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			sendingForwarderAppointedPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			sendingForwarderAppointedPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.Validation.ValidateAll();

			AssertNoError("SendingForwarderAddress is now a valid Gateway Agent", consol.JK_SendingForwarderHandlingTypeInfo, @"Sending Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertHasError("No address selected", consol.JK_SendingForwarderHandlingTypeInfo, "No Address has been selected to flag as \"Gateway\".");

			consol.JK_OA_SendingForwarderAddress = orgProxyAddressPK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertNoError("Address selected", consol.JK_SendingForwarderHandlingTypeInfo, "No Address has been selected to flag as \"Gateway\".");
			AssertHasError("Invalid override", consol.JK_SendingForwarderHandlingTypeInfo, @"This Agent Organization is flagged as GTT - Gateway Agent with Tariff. GTA - Gateway Agent type is not valid.
You can verify the organization's Gateway Agent Details on the Fwd/Agent tab.");

			sendingForwarderAppointedPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertHasError("Invalid override", consol.JK_SendingForwarderHandlingTypeInfo, @"This Agent Organization is flagged as GTA - Gateway Agent. GTT - Gateway Agent with Tariff type is not valid.
You can verify the organization's Gateway Agent Details on the Fwd/Agent tab.");
		}

		public void TestValidateJK_SendingForwarderHandlingType_OrgProxyOfActiveCompanies()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var originalOrgProxy = otherCompany.OrgProxy;

			try
			{
				var otherOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				otherOrgProxy.OH_Code = "AAA";
				var address = otherOrgProxy.MainAddress.PK;

				otherCompany.GC_OH_OrgProxy = otherOrgProxy.PK;

				Factory.Save();

				consol.JK_OA_SendingForwarderAddress = address;

				var sendingForwarderAppointedPort1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				sendingForwarderAppointedPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				sendingForwarderAppointedPort1.O5_PortOrCountry = "AUBNE";
				sendingForwarderAppointedPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
				sendingForwarderAppointedPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				sendingForwarderAppointedPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertNoError("Sending Forwarder is org proxy of some active company", consol.JK_SendingForwarderHandlingTypeInfo, @"Sending Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

				var notOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = notOrgProxy.MainAddress.PK;

				var sendingForwarderAppointedPort2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				sendingForwarderAppointedPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				sendingForwarderAppointedPort2.O5_PortOrCountry = "AUBNE";
				sendingForwarderAppointedPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				sendingForwarderAppointedPort2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				sendingForwarderAppointedPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertHasError("Sending Forwarder is not org proxy of some active company", consol.JK_SendingForwarderHandlingTypeInfo, @"Sending Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");
			}
			finally
			{
				otherCompany.GC_OH_OrgProxy = originalOrgProxy.PK;
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_OrgProxyOfActiveBranch()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";

			var query = new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			query.AddToFilter(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var otherBranch = Factory.LoadTop1<GlbBranch>(query);
			var originalOrgProxy = otherBranch.OrgProxy;

			try
			{
				var otherOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				otherOrgProxy.OH_Code = "AAA";
				var address = otherOrgProxy.MainAddress.PK;

				otherBranch.GB_OH_OrgProxy = otherOrgProxy.PK;

				Factory.Save();

				consol.JK_OA_SendingForwarderAddress = address;

				var sendingForwarderAppointedPort1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				sendingForwarderAppointedPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				sendingForwarderAppointedPort1.O5_PortOrCountry = "AUBNE";
				sendingForwarderAppointedPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
				sendingForwarderAppointedPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				sendingForwarderAppointedPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertNoError("Sending Forwarder is org proxy of some active company", consol.JK_SendingForwarderHandlingTypeInfo, @"Sending Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

				var notOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_SendingForwarderAddress = notOrgProxy.MainAddress.PK;

				var sendingForwarderAppointedPort2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
				sendingForwarderAppointedPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
				sendingForwarderAppointedPort2.O5_PortOrCountry = "AUBNE";
				sendingForwarderAppointedPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				sendingForwarderAppointedPort2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				sendingForwarderAppointedPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertHasError("Sending Forwarder is not org proxy of some active company", consol.JK_SendingForwarderHandlingTypeInfo, @"Sending Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Load Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");
			}
			finally
			{
				otherBranch.GB_OH_OrgProxy = originalOrgProxy?.PK ?? ZGuid.Empty;
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_GatewayBillingJobExists_OtherHandlingTypeSet()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPort1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPort1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			consol.JK_OA_ReceivingForwarderAddress = otherCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var orgAppointedAgentPort2 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPort2.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			Assert("Precondition", consol.IsGateway());

			using (new JobHeader.Loader(consol).TryCreate())
			{
				Factory.Save();
			}

			var errorMessage = ZString.Format(
				"Un-flagging the Gateway Flag for Sending Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
				GlbCompany.CurrentCompany.GC_Code);
			AssertNoError("Precon: No errors yet", consol.JK_SendingForwarderHandlingTypeInfo, errorMessage);
			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			AssertEquals("Precon: Receiving Forwarder Handling type still set", "GTT", consol.JK_ReceivingForwarderHandlingType);
			AssertHasError("Gateway billing job found - error even when receiving type is non-empty.", consol.JK_SendingForwarderHandlingTypeInfo, errorMessage);
		}

		public void TestValidateJK_SendingForwarderHandlingType_WhenSetToBlankAndThereIsAJobInAnotherCompany()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPorts = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			Assert("pre-condition", consol.IsGateway());

			using (new JobHeader.Loader(consol).TryCreate())
			{
				Factory.Save();
			}

			var company = GlbCompany.CurrentCompany;

			var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var sharedCompany = Factory.LoadTop1<GlbCompany>(query);
			AssertNotNull(sharedCompany);
			AssertNotEquals(sharedCompany.PK, GlbCompany.CurrentCompany.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sharedCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);

				loadedConsol.JK_SendingForwarderHandlingType = ZString.Empty;
				var errorMessage = ZString.Format(
						"Un-flagging the Gateway Flag for Sending Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
						company.GC_Code);
				AssertHasError(loadedConsol.JK_SendingForwarderHandlingTypeInfo, errorMessage);
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_AgentType()
		{
			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, SendingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.AWBCoload;
			AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, SendingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, SendingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, SendingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, SendingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.Courier;
			AssertHasError(consol.JK_SendingForwarderHandlingTypeInfo, SendingAgentGatewayMessage);
		}

		#endregion

		#region JK_ReceivingForwarderHandlingType

		public void TestValidateJK_ReceivingForwarderHandlingType()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = "AUBNE";

			var orgProxyAddressPK = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			consol.JK_OA_ReceivingForwarderAddress = orgProxyAddressPK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertHasError("ReceivingForwarderAddress is not a valid Gateway Agent", consol.JK_ReceivingForwarderHandlingTypeInfo, @"Receiving Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Discharge Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

			var receivingForwarderAppointedPort = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			receivingForwarderAppointedPort.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			receivingForwarderAppointedPort.O5_PortOrCountry = "AUBNE";
			receivingForwarderAppointedPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			receivingForwarderAppointedPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			receivingForwarderAppointedPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			consol.Validation.ValidateAll();

			AssertNoError("ReceivingForwarderAddress is now a valid Gateway Agent", consol.JK_ReceivingForwarderHandlingTypeInfo, @"Receiving Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Discharge Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertHasError("No address selected", consol.JK_ReceivingForwarderHandlingTypeInfo, "No Address has been selected to flag as \"Gateway\".");

			consol.JK_OA_ReceivingForwarderAddress = orgProxyAddressPK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertNoError("Address selected", consol.JK_ReceivingForwarderHandlingTypeInfo, "No Address has been selected to flag as \"Gateway\".");
			AssertHasError("Invalid override", consol.JK_ReceivingForwarderHandlingTypeInfo, @"This Agent Organization is flagged as GTT - Gateway Agent with Tariff. GTA - Gateway Agent type is not valid.
You can verify the organization's Gateway Agent Details on the Fwd/Agent tab.");

			receivingForwarderAppointedPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			AssertHasError("Invalid override", consol.JK_ReceivingForwarderHandlingTypeInfo, @"This Agent Organization is flagged as GTA - Gateway Agent. GTT - Gateway Agent with Tariff type is not valid.
You can verify the organization's Gateway Agent Details on the Fwd/Agent tab.");
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_OrgProxyOfActiveCompanies()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = "AUBNE";

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			var originalOrgProxy = otherCompany.OrgProxy;

			try
			{
				var otherOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				otherOrgProxy.OH_Code = "AAA";
				var address = otherOrgProxy.MainAddress.PK;

				otherCompany.GC_OH_OrgProxy = otherOrgProxy.PK;

				Factory.Save();

				consol.JK_OA_ReceivingForwarderAddress = address;

				var receivingForwarderAppointedPort = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				receivingForwarderAppointedPort.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
				receivingForwarderAppointedPort.O5_PortOrCountry = "AUBNE";
				receivingForwarderAppointedPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				receivingForwarderAppointedPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				receivingForwarderAppointedPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertNoError("Receiving Forwarder is org proxy of some active company", consol.JK_ReceivingForwarderHandlingTypeInfo, @"Receiving Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Discharge Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

				var notOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_ReceivingForwarderAddress = notOrgProxy.MainAddress.PK;

				var receivingForwarderAppointedPort2 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				receivingForwarderAppointedPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
				receivingForwarderAppointedPort2.O5_PortOrCountry = "AUBNE";
				receivingForwarderAppointedPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				receivingForwarderAppointedPort2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				receivingForwarderAppointedPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertHasError("Receiving Forwarder is not org proxy of some active company", consol.JK_ReceivingForwarderHandlingTypeInfo, @"Receiving Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Discharge Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");
			}
			finally
			{
				otherCompany.GC_OH_OrgProxy = originalOrgProxy?.PK ?? ZGuid.Empty;
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_OrgProxyOfActiveBranch()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = "AUBNE";

			var query = new ZQuery(GlbBranchSchema.PK, SQLComparisonOperator.NotEqual, GlbBranch.CurrentBranch.PK);
			query.AddToFilter(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));

			var otherBranch = Factory.LoadTop1<GlbBranch>(query);
			var originalOrgProxy = otherBranch.OrgProxy;

			try
			{
				var otherOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				otherOrgProxy.OH_Code = "AAA";
				var address = otherOrgProxy.MainAddress.PK;

				otherBranch.GB_OH_OrgProxy = otherOrgProxy.PK;

				Factory.Save();

				consol.JK_OA_ReceivingForwarderAddress = address;

				var receivingForwarderAppointedPort1 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				receivingForwarderAppointedPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
				receivingForwarderAppointedPort1.O5_PortOrCountry = "AUBNE";
				receivingForwarderAppointedPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
				receivingForwarderAppointedPort1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				receivingForwarderAppointedPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertNoError("Receiving Forwarder is org proxy of some active branch", consol.JK_ReceivingForwarderHandlingTypeInfo, @"Receiving Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Discharge Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");

				var notOrgProxy = Factory.NewWithValidTestData<OrgHeader>();
				consol.JK_OA_ReceivingForwarderAddress = notOrgProxy.MainAddress.PK;

				var receivingForwarderAppointedPort2 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
				receivingForwarderAppointedPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
				receivingForwarderAppointedPort2.O5_PortOrCountry = "AUBNE";
				receivingForwarderAppointedPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
				receivingForwarderAppointedPort2.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
				receivingForwarderAppointedPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertHasError("Receiving Forwarder is not Org Proxy for any active branch", consol.JK_ReceivingForwarderHandlingTypeInfo, @"Receiving Agent can be set as a Gateway Agent with Tariff on a Consol if this Agent is:
- configured as a Gateway Agent with Tariff for the Discharge Port with the corresponding Transport Mode in Organization>Forwarder/Agent>Gateway Agent
- an Organization Proxy");
			}
			finally
			{
				otherBranch.GB_OH_OrgProxy = originalOrgProxy?.PK ?? ZGuid.Empty;
			}
		}

		public void TestValidateJK_OA_ReceivingForwarderAddress_GatewayBillingJobExists()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPort1 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPort1.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			consol.JK_OA_SendingForwarderAddress = otherCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var orgAppointedAgentPort2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPort2.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			Assert("Precondition", consol.IsGateway());

			var nonProxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var orgAppointedAgentPort3 = nonProxyOrg.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort3.O5_OA_AgentOfficeAddress = nonProxyOrg.MainAddress.PK;
			orgAppointedAgentPort3.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort3.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort3.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var branchProxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = branchProxyOrg.PK;
			var orgAppointedAgentPort4 = branchProxyOrg.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort4.O5_OA_AgentOfficeAddress = branchProxyOrg.MainAddress.PK;
			orgAppointedAgentPort4.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort4.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort4.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPort5 = branchProxyOrg.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort5.O5_OA_AgentOfficeAddress = branchProxyOrg.MainAddress.PK;
			orgAppointedAgentPort5.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort5.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort5.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			using (new JobHeader.Loader(consol).TryCreate())
			{
				Factory.Save();
			}

			var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(query);
			AssertNotEquals(anotherCompany.PK, GlbCompany.CurrentCompany.PK);
			var jobCompany = GlbCompany.CurrentCompany;
			var jobCompanyCode = jobCompany.GC_Code;
			GlbCompany[] companies = { jobCompany, anotherCompany };

			foreach (var company in companies)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var newFactory = new BusinessObjectFactory();
					consol = newFactory.Load<ForwardingConsol>(consol.PK);

					var errorMessage = ZString.Format(
						"Unable to change the Receiving Agent. Gateway Billing Job exists in the Gateway Agent's login Company {0}", jobCompanyCode);
					consol.Validation.ValidateJK_OA_ReceivingForwarderAddress();
					AssertNoError("Precon: No errors yet", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessage);
					consol.JK_OA_ReceivingForwarderAddress = nonProxyOrg.MainAddress.PK;
					AssertHasErrorContaining(consol.JK_OA_ReceivingForwarderAddressInfo, errorMessage);

					consol.JK_OA_SendingForwarderAddress = branchProxyOrg.MainAddress.PK;
					consol.Validation.ValidateJK_OA_ReceivingForwarderAddress();
					AssertNoErrors("Precon: No errors yet", consol.JK_OA_ReceivingForwarderAddressInfo);

					consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
					consol.JK_OA_ReceivingForwarderAddress = branchProxyOrg.MainAddress.PK;
					AssertNoErrors("Precon: No errors yet", consol.JK_OA_ReceivingForwarderAddressInfo);

					consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
					AssertHasErrorContaining(consol.JK_OA_ReceivingForwarderAddressInfo, errorMessage);

					consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
					consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
					AssertHasErrorContaining(consol.JK_OA_ReceivingForwarderAddressInfo, errorMessage);

					consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
					consol.JK_OA_ReceivingForwarderAddress = jobCompany.OrgProxy.MainAddress.PK;
					AssertNoErrors("Precon: No errors yet", consol.JK_OA_ReceivingForwarderAddressInfo);
				}
			}
		}

		public void TestValidateJK_OA_SendingForwarderAddress_GatewayBillingJobExists()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPort1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPort1.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			consol.JK_OA_ReceivingForwarderAddress = otherCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var orgAppointedAgentPort2 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPort2.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			Assert("Precondition", consol.IsGateway());

			var nonProxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var orgAppointedAgentPort3 = nonProxyOrg.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort3.O5_OA_AgentOfficeAddress = nonProxyOrg.MainAddress.PK;
			orgAppointedAgentPort3.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort3.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort3.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var branchProxyOrg = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = branchProxyOrg.PK;
			var orgAppointedAgentPort4 = branchProxyOrg.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort4.O5_OA_AgentOfficeAddress = branchProxyOrg.MainAddress.PK;
			orgAppointedAgentPort4.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort4.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort4.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var orgAppointedAgentPort5 = branchProxyOrg.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort5.O5_OA_AgentOfficeAddress = branchProxyOrg.MainAddress.PK;
			orgAppointedAgentPort5.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort5.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort5.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			using (new JobHeader.Loader(consol).TryCreate())
			{
				Factory.Save();
			}

			var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(query);
			AssertNotEquals(anotherCompany.PK, GlbCompany.CurrentCompany.PK);
			var jobCompany = GlbCompany.CurrentCompany;
			var jobCompanyCode = jobCompany.GC_Code;
			GlbCompany[] companies = { jobCompany, anotherCompany };

			foreach (var company in companies)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, company.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var newFactory = new BusinessObjectFactory();
					consol = newFactory.Load<ForwardingConsol>(consol.PK);

					var errorMessage = ZString.Format(
						"Unable to change the Sending Agent. Gateway Billing Job exists in the Gateway Agent's login Company {0}", jobCompanyCode);
					consol.Validation.ValidateJK_OA_SendingForwarderAddress();
					AssertNoError("Precon: No errors yet", consol.JK_SendingForwarderHandlingTypeInfo, errorMessage);
					consol.JK_OA_SendingForwarderAddress = nonProxyOrg.MainAddress.PK;
					AssertHasErrorContaining(consol.JK_OA_SendingForwarderAddressInfo, errorMessage);

					consol.JK_OA_ReceivingForwarderAddress = branchProxyOrg.MainAddress.PK;
					consol.Validation.ValidateJK_OA_SendingForwarderAddress();
					AssertNoErrors("Precon: No errors yet", consol.JK_OA_SendingForwarderAddressInfo);

					consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
					consol.JK_OA_SendingForwarderAddress = branchProxyOrg.MainAddress.PK;
					AssertNoErrors("Precon: No errors yet", consol.JK_OA_SendingForwarderAddressInfo);

					consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
					AssertHasErrorContaining(consol.JK_OA_SendingForwarderAddressInfo, errorMessage);

					consol.JK_SendingForwarderHandlingType = ZString.Empty;
					consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
					AssertHasErrorContaining(consol.JK_OA_SendingForwarderAddressInfo, errorMessage);

					consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
					consol.JK_OA_SendingForwarderAddress = jobCompany.OrgProxy.MainAddress.PK;
					AssertNoErrors("Precon: No errors yet", consol.JK_OA_SendingForwarderAddressInfo);
				}
			}
		}

		public void TestValidateJK_OA_SendingForwarderAddress_WhenCFSJobExists()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_IsCFS = true;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var cfsJob = Factory.NewJobForTesting<JobHeader>();
			cfsJob.JH_JobNum = ((IJobNumber)consol).JobNumber;
			cfsJob.JH_ParentID = consol.PK;
			cfsJob.JH_ParentTableCode = consol.Prefix;
			cfsJob.JH_GB = GlbBranch.CurrentBranch.PK;
			cfsJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			cfsJob.JH_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var orgAppointedAgentPort1 = org1.AppointedAgentPorts.AddNew();
			orgAppointedAgentPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPort1.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort1.O5_SeaAgentStatus = AgentStatusList.Codes.Appointed;

			var orgAppointedAgentPort3 = org3.AppointedAgentPorts.AddNew();
			orgAppointedAgentPort3.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPort3.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort3.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort3.O5_SeaAgentStatus = AgentStatusList.Codes.Appointed;

			Factory.Save();

			consol.JK_OA_SendingForwarderAddress = org3.MainAddress.PK;
			AssertNoErrors("should not have any errors", consol.JK_OA_SendingForwarderAddressInfo);
		}

		public void TestValidateJK_OA_ReceivingForwarderAddress_WhenCFSJobExists()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			var org3 = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_IsCFS = true;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "NZAKL";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = org1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = org2.MainAddress.PK;

			var cfsJob = Factory.NewJobForTesting<JobHeader>();
			cfsJob.JH_JobNum = ((IJobNumber)consol).JobNumber;
			cfsJob.JH_ParentID = consol.PK;
			cfsJob.JH_ParentTableCode = consol.Prefix;
			cfsJob.JH_GB = GlbBranch.CurrentBranch.PK;
			cfsJob.JH_GE = GlbDepartment.CurrentDepartment.PK;
			cfsJob.JH_GC = GlbCompany.CurrentCompany.PK;

			Factory.Save();

			var orgAppointedAgentPort2 = org2.AppointedAgentPorts.AddNew();
			orgAppointedAgentPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPort2.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort2.O5_SeaAgentStatus = AgentStatusList.Codes.Appointed;

			var orgAppointedAgentPort3 = org3.AppointedAgentPorts.AddNew();
			orgAppointedAgentPort3.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPort3.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort3.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort3.O5_SeaAgentStatus = AgentStatusList.Codes.Appointed;

			Factory.Save();

			consol.JK_OA_ReceivingForwarderAddress = org3.MainAddress.PK;
			AssertNoErrors("should not have any errors", consol.JK_OA_ReceivingForwarderAddressInfo);
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_GatewayBillingJobExists_OtherHandlingTypeSet()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPort1 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPort1.O5_PortOrCountry = "NZAKL";
			orgAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			var otherCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			consol.JK_OA_SendingForwarderAddress = otherCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var orgAppointedAgentPort2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPort2.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			Assert("Precondition", consol.IsGateway());

			using (new JobHeader.Loader(consol).TryCreate())
			{
				Factory.Save();
			}

			var errorMessage = ZString.Format(
				"Un-flagging the Gateway Flag for Receiving Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
				GlbCompany.CurrentCompany.GC_Code);
			consol.Validation.ValidateJK_ReceivingForwarderHandlingType();
			AssertNoError("Precon: No errors yet", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessage);
			consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			AssertEquals("Precon: Sending handling type still set", "GTT", consol.JK_SendingForwarderHandlingType);
			AssertHasError("Gateway billing job found - error even when sending type is non-empty.", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessage);
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_WhenSetToBlankAndThereIsAJobInAnotherCompany()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var orgAppointedAgentPorts = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPorts.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			consol.JK_AgentType = Constants.AgentType.Agent;

			Assert("pre-condition", consol.IsGateway());

			using (new JobHeader.Loader(consol).TryCreate())
			{
				Factory.Save();
			}

			var company = GlbCompany.CurrentCompany;

			var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var sharedCompany = Factory.LoadTop1<GlbCompany>(query);
			AssertNotNull(sharedCompany);
			AssertNotEquals(sharedCompany.PK, GlbCompany.CurrentCompany.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sharedCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
			{
				var newFactory = new BusinessObjectFactory();
				var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);

				loadedConsol.JK_ReceivingForwarderHandlingType = ZString.Empty;
				var errorMessage = ZString.Format(
						"Un-flagging the Gateway Flag for Receiving Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
						company.GC_Code);
				AssertHasError(loadedConsol.JK_ReceivingForwarderHandlingTypeInfo, errorMessage);
				loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_AgentType()
		{
			var consol = Factory.New<ForwardingConsol>();

			Factory.Save();

			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, ReceivingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.AWBCoload;
			AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, ReceivingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, ReceivingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.AWBMaster;
			AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, ReceivingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.Direct;
			AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, ReceivingAgentGatewayMessage);

			consol.JK_AgentType = Constants.AgentType.Courier;
			AssertHasError(consol.JK_ReceivingForwarderHandlingTypeInfo, ReceivingAgentGatewayMessage);
		}

		#endregion

		#region JK_AgentType

		public void TestValidateJK_AgentType()
		{
			Env.Security.MaintainConsolTypeAgent.IsAllowed = true;
			Env.Security.MaintainConsolTypeDirect.IsAllowed = false;

			Consol.JK_AgentType = Constants.AgentType.Direct;
			Assert("Consol is not in database, AgentType is not allowed. Error expected.", Consol.JK_AgentTypeInfo.HasErrors());

			Consol.JK_AgentType = Constants.AgentType.Agent;
			Assert("Consol is not in database, AgentType is allowed. No error expected.", !Consol.JK_AgentTypeInfo.HasErrors());

			Factory.Save();

			Env.Security.MaintainConsolTypeAgent.IsAllowed = false;

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			ForwardingConsol dbConsol = factory2.Load<ForwardingConsol>(Consol.PK);
			dbConsol.Validation.ValidateJK_AgentType();
			Assert("AgentType is NOT allowed, but consol has no errors because it's in database AND agentType has no changes.", !dbConsol.JK_AgentTypeInfo.HasErrors());

			dbConsol.JK_AgentType = Constants.AgentType.Direct;
			Assert("Consol is in database and AgentType has change, AgentType is not allowed. Error expected.", dbConsol.JK_AgentTypeInfo.HasErrors());
		}

		public void TestValidateJK_SendingForwarderHandlingType_GatewayJobInAnotherCompany()
		{
			var currentCompany = Env.CurrentCompany;
			var sharedCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, currentCompany.PK));
			AssertNotNull("Pre-condition", sharedCompany);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00002222";
			consol.JK_RL_NKLoadPort = "CNSHA";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var appointedPortsForSendingAgent = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			appointedPortsForSendingAgent.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			appointedPortsForSendingAgent.O5_PortOrCountry = "CNSHA";
			appointedPortsForSendingAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedPortsForSendingAgent.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			using (var job = new JobHeader.Loader(Factory, consol).TryCreate())
			{
				job.JH_JobNum = ((IJobNumber)consol).JobNumber;
				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = consol.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = currentCompany.PK;

				Factory.Save();

				Assert("Precondition", !job.IsGatewayLegacyJob);
				Assert("Precondition", consol.IsGateway());

				consol.JK_SendingForwarderHandlingType = ZString.Empty;
				var errorMessage = ZString.Format(
			"Un-flagging the Gateway Flag for Sending Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
			job.Company.GC_Code);
				AssertHasError(consol.JK_SendingForwarderHandlingTypeInfo, errorMessage);
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sharedCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var newFactory = new BusinessObjectFactory();
					var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);

					loadedConsol.JK_SendingForwarderHandlingType = ZString.Empty;
					errorMessage = ZString.Format(
				"Un-flagging the Gateway Flag for Sending Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
				job.Company.GC_Code);
					AssertHasError("Expected same error as the other company has both gateway agent and a job", loadedConsol.JK_SendingForwarderHandlingTypeInfo, errorMessage);
					loadedConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				}
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_GatewayJobInAnotherCompany()
		{
			var currentCompany = Env.CurrentCompany;
			var sharedCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, currentCompany.PK));
			AssertNotNull("Pre-condition", sharedCompany);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00002222";
			consol.JK_RL_NKDischargePort = "CNSHA";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var appointedPortsForSendingAgent = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			appointedPortsForSendingAgent.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			appointedPortsForSendingAgent.O5_PortOrCountry = "CNSHA";
			appointedPortsForSendingAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedPortsForSendingAgent.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			using (var job = new JobHeader.Loader(Factory, consol).TryCreate())
			{
				job.JH_JobNum = ((IJobNumber)consol).JobNumber;
				job.JH_ParentID = consol.PK;
				job.JH_ParentTableCode = consol.Prefix;
				job.JH_GB = GlbBranch.CurrentBranch.PK;
				job.JH_GE = GlbDepartment.CurrentDepartment.PK;
				job.JH_GC = currentCompany.PK;

				Factory.Save();

				Assert("Precondition", !job.IsGatewayLegacyJob);
				Assert("Precondition", consol.IsGateway());

				consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
				var errorMessage = ZString.Format(
			"Un-flagging the Gateway Flag for Receiving Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
			job.Company.GC_Code);
				AssertHasError(consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessage);
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sharedCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartmentPK))
				{
					var newFactory = new BusinessObjectFactory();
					var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);

					loadedConsol.JK_ReceivingForwarderHandlingType = ZString.Empty;
					errorMessage = ZString.Format(
				"Un-flagging the Gateway Flag for Receiving Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
				job.Company.GC_Code);
					AssertHasError("Expected same error as the other company has both gateway agent and a job", loadedConsol.JK_ReceivingForwarderHandlingTypeInfo, errorMessage);
					loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				}
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_WhenGatewayAgentHasCostsAndSharedCompanyLogedIn()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNotNull("Pre-condition", consol.SendingForwarder);

			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			AssertNotEquals(((IOrgHeader)null, (IOrgHeader)null), ((IGateway)consol).GatewayBillingSupporter.GatewayAgent());

			var consolCostType = ObjectFactory.GetType<IJobConsolCost>();
			var consolCost = Factory.NewWithValidTestData(consolCostType);
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			Factory.Save();

			var sharedCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			AssertNotNull(sharedCompany);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sharedCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_AgentType = Constants.AgentType.Agent;
				loadedConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertNoErrors(loadedConsol.JK_SendingForwarderHandlingTypeInfo);
				loadedConsol.JK_SendingForwarderHandlingType = ZString.Empty;
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_WhenGatewayAgentHasCostsAndSharedCompanyLogedIn()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "DEFRA";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNotNull("Pre-condition", consol.ReceivingForwarder);

			var orgAppointedAgentPorts1 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;

			AssertNotEquals(((IOrgHeader)null, (IOrgHeader)null), ((IGateway)consol).GatewayBillingSupporter.GatewayAgent());

			var consolCostType = ObjectFactory.GetType<IJobConsolCost>();
			var consolCost = Factory.NewWithValidTestData(consolCostType);
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.E6_ParentTableCode] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			Factory.Save();

			var sharedCompany = Factory.LoadTop1<GlbCompany>(new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK));
			AssertNotNull(sharedCompany);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, sharedCompany.Branches[0].PK.ToGuid(), GlbDepartment.CurrentDepartment.PK.ToGuid()))
			{
				var newFactory = new BusinessObjectFactory();
				var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_AgentType = Constants.AgentType.Agent;
				loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertNoErrors(loadedConsol.JK_ReceivingForwarderHandlingTypeInfo);
				loadedConsol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_WhenConvertingFromGTWtoNonGTWAndNotSavedJobExist()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNotNull("Pre-condition", consol.SendingForwarder);

			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.JK_AgentType = Constants.AgentType.Agent;

			Assert(consol.IsGateway());

			Factory.Save();

			using (var job = new JobHeader.Loader(consol).TryCreate())
			{
				consol.JK_SendingForwarderHandlingType = ZString.Empty;
				AssertHasErrors(consol.JK_SendingForwarderHandlingTypeInfo);
				AssertHasError(consol.JK_SendingForwarderHandlingTypeInfo,
				string.Format("Un-flagging the Gateway Flag for Sending Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.", job.Company.GC_Code));
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_WhenConvertingFromGTWtoNonGTWAndNotSavedJobExist()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_ConsolMode = "LSE";
			consol.JK_MasterBillNum = "12345678905";
			consol.Transports[0].JW_ETD = DateTime.Today;
			consol.Transports[0].JW_VoyageFlight = "AA123";

			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			AssertNotNull("Pre-condition", consol.ReceivingForwarder);

			var orgAppointedAgentPorts1 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "DEFRA";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.JK_AgentType = Constants.AgentType.Agent;

			Assert(consol.IsGateway());

			Factory.Save();

			using (var job = new JobHeader.Loader(consol).TryCreate())
			{
				consol.JK_ReceivingForwarderHandlingType = ZString.Empty;

				AssertHasErrors(consol.JK_ReceivingForwarderHandlingTypeInfo);
				AssertHasError(consol.JK_ReceivingForwarderHandlingTypeInfo,
					string.Format("Un-flagging the Gateway Flag for Receiving Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.", job.Company.GC_Code));
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			}
		}

		public void TestValidateJK_SendingReceivingForwarderHandlingTypes_TwoGatewayAgentsOnDomesticConsol()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "AUMEL";
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_AgentType = Constants.AgentType.Agent;

			var orgAppointedAgentPort1 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort1.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPort1.O5_PortOrCountry = "AUMEL";
			orgAppointedAgentPort1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_GC = GlbCompany.CurrentCompany.PK;
			branch.GB_OH_OrgProxy = branchProxy.PK;

			consol.JK_OA_SendingForwarderAddress = branchProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

			var orgAppointedAgentPort2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPort2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPort2.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPort2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPort2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			Assert("Precondition: Consol is gateway", consol.IsGateway());
			using (new JobHeader.Loader(consol).TryCreate())
			{
				Factory.Save();
			}

			var errorMessageSending = ZString.Format(
				"Un-flagging the Gateway Flag for Sending Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
				GlbCompany.CurrentCompany.GC_Code);
			var errorMessageReceiving = ZString.Format(
					"Un-flagging the Gateway Flag for Receiving Forwarder is only permitted if no Gateway Billing Job exists in the Gateway Agent's login Company {0}.",
					GlbCompany.CurrentCompany.GC_Code);

			consol.Validation.ValidateJK_SendingForwarderHandlingType();
			consol.Validation.ValidateJK_ReceivingForwarderHandlingType();
			AssertNoError("At least one Agent for Country is still present", consol.JK_SendingForwarderHandlingTypeInfo, errorMessageSending);
			AssertNoError("At least one Agent for Country is still present", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessageReceiving);

			consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			consol.Validation.ValidateJK_SendingForwarderHandlingType();
			consol.Validation.ValidateJK_ReceivingForwarderHandlingType();
			AssertNoError("At least one Agent for Country is still present", consol.JK_SendingForwarderHandlingTypeInfo, errorMessageSending);
			AssertNoError("At least one Agent for Country is still present", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessageReceiving);

			consol.JK_SendingForwarderHandlingType = ZString.Empty;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.Validation.ValidateJK_SendingForwarderHandlingType();
			consol.Validation.ValidateJK_ReceivingForwarderHandlingType();
			AssertNoError("At least one Agent for Country is still present", consol.JK_SendingForwarderHandlingTypeInfo, errorMessageSending);
			AssertNoError("At least one Agent for Country is still present", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessageReceiving);

			consol.JK_ReceivingForwarderHandlingType = ZString.Empty;
			consol.Validation.ValidateJK_SendingForwarderHandlingType();
			consol.Validation.ValidateJK_ReceivingForwarderHandlingType();
			AssertHasError("No valid Gateway Agents for Country are left", consol.JK_SendingForwarderHandlingTypeInfo, errorMessageSending);
			AssertHasError("No valid Gateway Agents for Country are left", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessageReceiving);

			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.Validation.ValidateJK_ReceivingForwarderHandlingType();
			AssertHasError("No valid Gateway Agents for Country are left", consol.JK_ReceivingForwarderHandlingTypeInfo, errorMessageReceiving);
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.Validation.ValidateJK_SendingForwarderHandlingType();
			AssertHasError("No valid Gateway Agents for Country are left", consol.JK_SendingForwarderHandlingTypeInfo, errorMessageSending);
			consol.JK_OA_SendingForwarderAddress = branchProxy.MainAddress.PK;

			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
		}

		public void TestValidateJK_SendingForwarderHandlingTypeValidationWhenGatewayConsolWasSavedWithNonGatewayCosts()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUBNE";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var freightChargeCode = newFactory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var consolCost = (BusinessObject)newFactory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.Constants.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.Constants.E6_ParentTableCode] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.Constants.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.Constants.E6_AC_ChargeCode] = freightChargeCode.PK;

			newFactory.Save();

			consol.Validation.ValidateJK_SendingForwarderHandlingType();
			AssertNoErrors(consol.JK_SendingForwarderHandlingTypeInfo);
		}

		public void TestValidateJK_ReceivingForwarderHandlingTypeValidationWhenGatewayConsolWasSavedWithNonGatewayCosts()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			var gatewayAgentPort = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUBNE";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var freightChargeCode = newFactory.LoadTop1<AccChargeCode>(new ZQuery(AccChargeCodeSchema.AC_Code, "FRT"));
			var consolCost = (BusinessObject)newFactory.New<IJobConsolCost>();
			consolCost.SetContext(BusinessContext.EnableDirectSettingConsolCostParent);
			try
			{
				consolCost[JobConsolCostSchema.Constants.E6_ParentID] = consol.PK;
				consolCost[JobConsolCostSchema.Constants.E6_ParentTableCode] = consol.TablePrefix;
			}
			finally
			{
				consolCost.RemoveContext(BusinessContext.EnableDirectSettingConsolCostParent);
			}
			consolCost[JobConsolCostSchema.Constants.E6_GC] = GlbCompany.CurrentCompany.PK;
			consolCost[JobConsolCostSchema.Constants.E6_AC_ChargeCode] = freightChargeCode.PK;

			newFactory.Save();

			consol.Validation.ValidateJK_ReceivingForwarderHandlingType();
			AssertNoErrors(consol.JK_ReceivingForwarderHandlingTypeInfo);
		}

		#endregion

		#region JK_Phase

		public void TestValidateJK_Phase()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_Phase = "XXX";
			AssertHasErrors(consol.JK_PhaseInfo);

			consol.JK_Phase = "FOO";
			AssertHasErrors(consol.JK_PhaseInfo);

			consol.JK_Phase = PhaseConstants.Phase.ALL;
			AssertNoErrors(consol.JK_PhaseInfo);
		}

		#endregion

		#region JK_OH_Creditor

		public void TestValidateJK_OH_Creditor()
		{
			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			Consol.JK_OA_CreditorAddress = ZGuid.Invalid;
			AssertHasError(Consol.JK_OA_CreditorAddressInfo, "Enter a valid Co-Loader.");
			Consol.JK_OA_CreditorAddress = ZGuid.Empty;

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			Consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
			Consol.JK_OA_CreditorAddress = ZGuid.Invalid;
			AssertHasError(Consol.JK_OA_CreditorAddressInfo, "Enter a valid Gateway Co-Loader.");

			Consol.JK_AgentType = Constants.AgentType.Agent;
			Consol.JK_SendingForwarderHandlingType = ZString.Empty;
			Consol.JK_OA_CreditorAddress = ZGuid.Invalid;
			AssertHasError(Consol.JK_OA_CreditorAddressInfo, "Enter a valid Creditor.");
		}

		#endregion

		#region JK_BookingReference

		public void TestValidateBookingRefErrorsWithDelimiterCharacters()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_BookingReference = "123";
			AssertNoError(Consol.JK_BookingReferenceInfo, "Booking Reference cannot contain multiple numbers. Please split these numbers and add them to the Reference Numbers grid under the numbers tab.");

			Consol.JK_BookingReference = "123, 123";
			AssertHasError(Consol.JK_BookingReferenceInfo, "Booking Reference cannot contain multiple numbers. Please split these numbers and add them to the Reference Numbers grid under the numbers tab.");

			Consol.JK_BookingReference = "123,123";
			AssertHasError(Consol.JK_BookingReferenceInfo, "Booking Reference cannot contain multiple numbers. Please split these numbers and add them to the Reference Numbers grid under the numbers tab.");

			Consol.JK_BookingReference = "123:123";
			AssertHasError(Consol.JK_BookingReferenceInfo, "Booking Reference cannot contain multiple numbers. Please split these numbers and add them to the Reference Numbers grid under the numbers tab.");

			Consol.JK_BookingReference = "123;123";
			AssertHasError(Consol.JK_BookingReferenceInfo, "Booking Reference cannot contain multiple numbers. Please split these numbers and add them to the Reference Numbers grid under the numbers tab.");

			Consol.JK_BookingReference = "123 123";
			AssertHasError(Consol.JK_BookingReferenceInfo, "Booking Reference cannot contain multiple numbers. Please split these numbers and add them to the Reference Numbers grid under the numbers tab.");
		}

		public void TestValidateBookingRefErrorsWithLeadingSpaces()
		{
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_BookingReference = " REF-02398J";
			AssertHasMessageError(Consol.JK_BookingReferenceInfo, "Booking Reference should not contain leading spaces.");

			Consol.JK_BookingReference = "REF-02398J";
			AssertNoMessageError(Consol.JK_BookingReferenceInfo, "Booking Reference should not contain leading spaces.");
		}

		#endregion

		#endregion

		#region Load / Discharge

		public void TestValidateJK_RL_NKLoadPort_ProhibitedRouting()
		{
			var expectedWarning = "The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through, Syria, Yemen, and Somalia. You are required to meet with government requirements and/or consider a change of transport mode.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "YEAAY";
			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_TransportMode = "AIR";
			AssertHasWarning("Prohibited routing", consol.JK_RL_NKLoadPortInfo, expectedWarning);

			consol.JK_TransportMode = "SEA";
			AssertNoWarning("No warning for sea", consol.JK_RL_NKLoadPortInfo, expectedWarning);

			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKDischargePort = "NZAKL";
			AssertNoWarning("No warning for for non-AU discharge", consol.JK_RL_NKLoadPortInfo, expectedWarning);

			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.JK_RL_NKLoadPort = "FRPAR";
			AssertNoWarning("No warning for non-prohibited load port", consol.JK_RL_NKLoadPortInfo, expectedWarning);
		}

		public void TestValidateJK_RL_NKLoadPort_TranshipmentWithProhibitedRouting()
		{
			var expectedWarning = "The Australian Government has imposed prohibitions on the carriage of air cargo that has originated from, or transited through, Syria, Yemen, and Somalia. You are required to meet with government requirements and/or consider a change of transport mode.";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "YEAAY";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_TransportMode = "AIR";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = "AIR";
			transport1.JW_RL_NKLoadPort = "YEAAY";
			transport1.JW_RL_NKDiscPort = "NZAKL";

			AssertNoWarning("No warning for non AU discharge", consol.JK_RL_NKLoadPortInfo, expectedWarning);

			transport1.JW_RL_NKDiscPort = "AUBNE";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = "AIR";
			transport2.JW_RL_NKLoadPort = "AUBNE";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			AssertHasWarning("Has warning for prohibited routing transhipping through AU", consol.JK_RL_NKLoadPortInfo, expectedWarning);
		}

		public void TestValidateLoadPortDiscPort_ShouldBeSameAsOriginDest()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = "A";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "HKHKG";

			var transport1 = consol.Transports[0];
			transport1.JW_RL_NKLoadPort = "AUMEL";
			transport1.JW_RL_NKDiscPort = "SGSIN";
			transport1.JW_ETD = ZDateTime.Now.AddDays(2);
			transport1.JW_ETA = ZDateTime.Now.AddDays(3);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "AUMEL";
			transport2.JW_ETD = ZDateTime.Now.AddDays(-3);
			transport2.JW_ETA = ZDateTime.Now.AddDays(-2);

			consol.TemplateRecord = templateRecord;
			consol.IsTemplateRecord = false;

			consol.Validation.ValidateJK_RL_NKLoadPort();
			AssertHasWarning(consol.JK_RL_NKLoadPortInfo, "Consol templates must be the same origin as the selected flight schedule.");

			consol.Validation.ValidateJK_RL_NKDischargePort();
			AssertHasWarning(consol.JK_RL_NKDischargePortInfo, "Consol templates must be the same destination as the selected flight schedule.");
		}

		#endregion

		#region JK_JX_JA_E_DEP / JK_JX_JB_E_LastARV

		public void TestJK_JX_JA_E_DEP()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = "A";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETD = ZDateTime.Now.AddDays(-3);
			transport1.JW_ETA = ZDateTime.Now.AddDays(-2);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ETD = ZDateTime.Now.AddDays(12);
			transport2.JW_ETA = ZDateTime.Now.AddDays(13);

			consol.TemplateRecord = templateRecord;
			consol.IsTemplateRecord = false;

			var warningMessage = "Flight dates must be today or later.";
			consol.Validation.ValidateJK_JX_JA_E_DEP();
			AssertHasWarning(consol.JK_JX_JA_E_DEPInfo, warningMessage);

			transport1.JW_ETD = ZDateTime.Now.AddDays(2);
			transport1.JW_ETA = ZDateTime.Now.AddDays(4);
			consol.Validation.ValidateJK_JX_JA_E_DEP();
			AssertNoWarning(consol.JK_JX_JA_E_DEPInfo, warningMessage);
		}

		public void TestJK_JX_JB_E_LastARV()
		{
			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = "A";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var transport1 = consol.Transports[0];
			transport1.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport1.JW_RL_NKLoadPort = "AUSYD";
			transport1.JW_RL_NKDiscPort = "AUMEL";
			transport1.JW_ETD = ZDateTime.Now.AddDays(-13);
			transport1.JW_ETA = ZDateTime.Now.AddDays(-12);

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = Core.Constants.TransportModes.Air;
			transport2.JW_RL_NKLoadPort = "AUMEL";
			transport2.JW_RL_NKDiscPort = "SGSIN";
			transport2.JW_ETD = ZDateTime.Now.AddDays(-5);
			transport2.JW_ETA = ZDateTime.Now.AddDays(-4);

			consol.TemplateRecord = templateRecord;
			consol.IsTemplateRecord = false;

			var warningMessage = "Flight dates must be today or later.";
			consol.Validation.ValidateJK_JX_JB_E_LastARV();
			AssertHasWarning(consol.JK_JX_JB_E_LastARVInfo, warningMessage);

			transport2.JW_ETD = ZDateTime.Today.AddDays(-1);
			transport2.JW_ETA = ZDateTime.Today;
			consol.Validation.ValidateJK_JX_JB_E_LastARV();
			AssertNoWarning(consol.JK_JX_JB_E_LastARVInfo, warningMessage);
		}

		#endregion

		#region JK_OA_ShippingLineAddress

		public void TestJK_OA_ShippingLineAddress()
		{
			var sailing = CreateSailing();
			var templateRecord = CreateTemplateRecord(sailing);

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			var transport = consol.Transports[0];
			transport.JW_IsLinked = true;
			transport.JW_JX = sailing.PK;
			AssertNotNull(transport.Carrier);
			AssertEquals("CARRIER1", transport.Carrier.OH_Code);

			var provider = consol as ITemplateRecordProvider;
			provider.TemplateRecord = templateRecord;
			provider.IsTemplateRecord = false;

			var message = "If the carrier is populated on the template, it may only be selected for that carrier.";
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertNoWarnings(message, consol.JK_OA_ShippingLineAddressInfo);

			var carrier2 = Factory.NewWithValidTestData<OrgHeader>();
			carrier2.OH_IsShippingLine = true;
			carrier2.OH_Code = "CARRIER2";
			carrier2.OH_FullName = "Carrier 2";

			var carrierAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			carrierAddress2.OA_Address1 = "Carrier Address 2";
			carrierAddress2.OA_Code = "CRADDR2";
			carrierAddress2.OA_OH = carrier2.PK;

			transport.JW_OA_CarrierAddress = carrierAddress2.PK;
			consol.Validation.ValidateJK_OA_ShippingLineAddress();
			AssertHasWarnings(message, consol.JK_OA_ShippingLineAddressInfo);
		}

		StmTemplateRecord CreateTemplateRecord(JobSailing sailing)
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.Transports[0].JW_IsLinked = true;
			consol.Transports[0].JW_JX = sailing.PK;

			Factory.Save();

			var templateRecord = Factory.New<StmTemplateRecord>();
			templateRecord.STR_TemplateName = "A";

			var provider = consol as ITemplateRecordProvider;
			provider.TemplateRecord = templateRecord;

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			{
				provider.SaveToTemplateRecord();
			}

			return templateRecord;
		}

		JobSailing CreateSailing()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_IsShippingLine = true;
			carrier.OH_Code = "CARRIER1";
			carrier.OH_FullName = "Carrier Full Name";

			var carrierAddress = Factory.NewWithValidTestData<OrgAddress>();
			carrierAddress.OA_Address1 = "Carrier Address";
			carrierAddress.OA_Code = "CRADDR1";
			carrierAddress.OA_OH = carrier.PK;

			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "SQ22";
			voyage.JV_OH_Line = carrier.PK;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today.AddDays(1);

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			destination.JB_E_ARV = ZDate.Today.AddDays(3);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		#endregion

		public void TestConsolTypeSecurityCheckpoint()
		{
			Consol.JK_AgentType = Constants.AgentType.Direct;
			AssertEquals("Direct", "MaintainConsolTypeDirect", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = Constants.AgentType.Agent;
			AssertEquals("Agent", "MaintainConsolTypeAgent", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = Constants.AgentType.CoLoad;
			AssertEquals("CoLoad", "MaintainConsolTypeCoLoad", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = Constants.AgentType.Charter;
			AssertEquals("Charter", "MaintainConsolTypeCharter", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = Constants.AgentType.Other;
			AssertEquals("Other", "MaintainConsolTypeOther", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = Constants.AgentType.AWBCoload;
			AssertEquals("Other", "MaintainConsolTypeAWBCoload", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = Constants.AgentType.AWBMaster;
			AssertEquals("Other", "MaintainConsolTypeAWBMaster", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = Constants.AgentType.Courier;
			AssertEquals("MaintainConsolTypeCourier", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);

			Consol.JK_AgentType = "XXX";
			AssertEquals("Invalid agent type", "None", Consol.Validation.GetConsolTypeSecurityCheckpoint().Code);
		}

		#region JK_HBLAWBChargesDisplay

		public void TestJK_HBLAWBChargesValidation()
		{
			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUSYD";
			const string errorMessage = "Enter a valid selection.";

			var consol = Factory.New<ForwardingConsol>();

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NON;
			AssertNoErrors(consol.JK_MBLAWBChargesDisplayInfo);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.APP;
			AssertNoErrors(consol.JK_MBLAWBChargesDisplayInfo);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.CPD;
			AssertNoErrors(consol.JK_MBLAWBChargesDisplayInfo);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NPP;
			AssertNoErrors(consol.JK_MBLAWBChargesDisplayInfo);

			consol.JK_MBLAWBChargesDisplay = "ABC";
			AssertHasError(consol.JK_MBLAWBChargesDisplayInfo, errorMessage);
		}

		public void TestJK_HBLAWBChargesValidationBrazil()
		{
			var currentBranch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			currentBranch.GB_RL_NKHomePort = "AUSYD";
			const string errorMessage = "Enter a valid selection.";
			const string brazilErrorMessage = "\"As Agreed\" option cannot be used for imports to Brazil";
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Direct;

			consol.JK_RL_NKDischargePort = "BR6MO";
			consol.JK_RL_NKLoadPort = "AUSYD";

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NON;
			AssertNoErrors(consol.JK_MBLAWBChargesDisplayInfo);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.APP;

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.CPD;
			AssertHasError(consol.JK_MBLAWBChargesDisplayInfo, brazilErrorMessage);

			consol.JK_MBLAWBChargesDisplay = ChargesApplyHelper.ChargesApplyConstants.NPP;
			AssertHasError(consol.JK_MBLAWBChargesDisplayInfo, brazilErrorMessage);

			consol.JK_MBLAWBChargesDisplay = "ABC";
			AssertHasError(consol.JK_MBLAWBChargesDisplayInfo, errorMessage);
		}

		#endregion

		#region MasterBillNum

		public void TestValidateJK_MasterBillNum()
		{
			Consol.JK_IsNeutralMaster = true;
			SetConsolAirLoadingMasterBill();
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = "";
			Consol.Validation.ValidateJK_MasterBillNum();
			Assert(Consol.JK_MasterBillNumInfo.HasWarnings());
			Consol.JK_MasterBillNum = "Something";
			Consol.Validation.ValidateJK_MasterBillNum();
			Assert(!Consol.JK_MasterBillNumInfo.HasWarnings());
		}

		public void TestValidateMasterBillNum()
		{
			JobMawb jobMawb = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "ALL");
			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_RL_NKLoadPort = AUSYDLoco;
			consol.JK_RL_NKDischargePort = USLAXLoco;
			consol.MasterBillAirlinePrefix = "176";
			Factory.Save();

			ForwardingConsol newConsol = Factory.New<ForwardingConsol>();
			newConsol.JK_TransportMode = Core.Constants.TransportModes.Air;
			newConsol.JK_RL_NKLoadPort = AUSYDLoco;
			newConsol.JK_RL_NKDischargePort = USLAXLoco;

			ZInt savedRecyclePeriod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			try
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

				newConsol.JK_MasterBillNum = "17610000001";
				AssertEquals("This Master Bill Number already exists on another Consol, Shipment or Booking.\r\nPlease select another number.", newConsol.MasterBillMAWBInfo.GetErrors().GetFirstMessage());

				newConsol.Delete();
				Factory.Save();

				newConsol = Factory.New<ForwardingConsol>();
				newConsol.JK_MasterBillNum = "17610000001";
				newConsol.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value - 1);
				Factory.Save();

				consol.Validation.ValidateMasterBillMAWB();
				AssertNoErrors(Consol.MasterBillMAWBInfo);
			}
			finally
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, savedRecyclePeriod);
			}

			newConsol.Delete();
			Factory.Save();

			consol.JK_IsNeutralMaster = false;
			consol.JK_MasterBillNum = "083101010";
			consol.Validation.ValidateMasterBillMAWB();
			Assert(consol.MasterBillMAWBInfo.HasWarnings());

			consol.JK_MasterBillNum = "08310000000";
			consol.Validation.ValidateMasterBillMAWB();
			Assert(consol.MasterBillMAWBInfo.HasWarnings());

			consol.JK_MasterBillNum = "08310000000";
			consol.Validation.ValidateMasterBillMAWB();
			Assert(consol.MasterBillMAWBInfo.HasWarnings());

			consol.JK_MasterBillNum = "17610000001";
			consol.Validation.ValidateMasterBillMAWB();
			AssertHasError(consol.MasterBillMAWBInfo, "This Master Bill Number is already in stock.\r\nIt is flagged as a Neutral Number.\r\nPlease enter another number.");

			jobMawb.JM_OH_AllocatedTo = ZGuid.NewZGuid();
			consol.JK_MasterBillNum = "17610000001";
			consol.Validation.ValidateMasterBillMAWB();
			AssertHasError(consol.MasterBillMAWBInfo, "MAWB has been \'borrowed out\' to a customer. Please enter another number.");

			consol.JK_IsNeutralMaster = true;
			jobMawb.JM_OH_AllocatedTo = ZGuid.Empty;
			consol.Factory.Save();
			consol.Validation.ValidateMasterBillNeutralMAWB();
			AssertHasWarning(consol.MasterBillNeutralMAWBInfo, "There are no MAWBs left for this Airline. Please add more numbers to your stock.");
		}

		public void TestValidateMasterBillNumberForFWANumberRange()
		{
			var shipper = Factory.NewWithValidTestData<OrgHeader>();
			shipper.OH_IsShippingLine = true;
			shipper.CustomsCodes.AddNew("HID", "FWA", "US");

			var stmNum = shipper.OrgFountains.AddNew();
			stmNum.SN_Type = OrgConstants.NumberFountains.Code.ForwardAirBillNumbers;
			stmNum.SN_Prefix = string.Empty;
			stmNum.SN_MinimumValue = 10;
			stmNum.SN_MaximumValue = 11;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			consol.JK_OA_ShippingLineAddress = shipper.MainAddress.PK;
			consol.JK_RL_NKLoadPort = "USCHI";
			consol.JK_MasterBillNum = "MAS345555";

			var warningMessage = "The BOL number range allocated by Forward Air is about to expire. You have 1 numbers left. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges";
			AssertNoWarning(consol.JK_MasterBillNumInfo, warningMessage);

			using (((IDbConnected)Factory).Connection.BeginTransactionWithManager())
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.UnitedStates))
			{
				var fountain = stmNum.TryGetNumberFountain();
				fountain.GetNextFormatted(Factory);

				consol.Validation.ValidateJK_MasterBillNum();
				AssertHasWarning(consol.JK_MasterBillNumInfo, warningMessage);

				fountain.GetNextFormatted(Factory);
				warningMessage = "The BOL number range allocated by Forward Air is to expire. Please contact Forward Air to obtain a new set of numbers, which can be added to the Forward Air Carrier organization under Details > Config > Number Ranges";

				consol.Validation.ValidateJK_MasterBillNum();
				AssertHasWarning(consol.JK_MasterBillNumInfo, warningMessage);
			}
		}

		public void TestValidateMasterBillNumErrorsWithLeadingSpaces()
		{
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = " OB032938";
			Consol.Validation.ValidateJK_MasterBillNum();
			AssertHasMessageError(Consol.JK_MasterBillNumInfo, "Ocean Bill should not contain leading spaces.");

			Consol.JK_MasterBillNum = "OB032938";
			Consol.Validation.ValidateJK_MasterBillNum();
			AssertNoMessageError(Consol.JK_MasterBillNumInfo, "Ocean Bill should not contain leading spaces.");
		}

		public void TestValidateMasterBIllNumErrorsWithInvalidCharacters()
		{
			Consol.JK_TransportMode = Constants.TransportModes.Sea;
			Consol.JK_MasterBillNum = "123";
			AssertNoError(Consol.JK_MasterBillNumInfo, "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.");

			Consol.JK_MasterBillNum = "123, 123";
			AssertHasError(Consol.JK_MasterBillNumInfo, "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.");

			Consol.JK_MasterBillNum = "123,123";
			AssertHasError(Consol.JK_MasterBillNumInfo, "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.");

			Consol.JK_MasterBillNum = "123:123";
			AssertHasError(Consol.JK_MasterBillNumInfo, "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.");

			Consol.JK_MasterBillNum = "123;123";
			AssertHasError(Consol.JK_MasterBillNumInfo, "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.");

			Consol.JK_MasterBillNum = "123 123";
			AssertHasError(Consol.JK_MasterBillNumInfo, "Commas, spaces, colons, and semicolons are not valid characters on a Bill of Lading.");
		}

		public void TestMawbInStockCount()
		{
			JobMawb mawb1 = AddMawb("176", "10000001", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb2 = AddMawb("176", "10000002", GlbBranch.CurrentBranch, "STD");
			JobMawb mawb3 = AddMawb("176", "10000003", GlbBranch.CurrentBranch, "ALL");

			JobMawb mawb4 = AddMawb("176", "10000004", GlbBranch.CurrentBranch, "STD");
			mawb4.JM_ParentID = ZGuid.NewZGuid();
			mawb4.JM_ParentTableCode = "JS";

			JobMawb mawb5 = AddMawb("176", "10000005", GlbBranch.CurrentBranch, "ALL");
			mawb5.JM_ParentID = ZGuid.NewZGuid();
			mawb5.JM_ParentTableCode = "JS";

			Factory.Save();

			AssertContainsExactElementsInAnyOrder("prerequisite", new[] { mawb1, mawb2, mawb3, mawb4, mawb5 }, Factory.Load<JobMawb>(new ZQuery()));

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.JK_IsNeutralMaster = true;
			consol.JK_MasterBillNum = "17610000001";
			consol.Validation.ValidateMasterBillNeutralMAWB();

			AssertHasWarning(consol.MasterBillNeutralMAWBInfo, "There are only 2 MAWBs left for this Airline. Please add more numbers to your stock.");
		}

		public void TestValidateMasterBillNumIgnoredOldConsols()
		{
			ZInt savedRecyclePeriod = Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.Value;
			try
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, 12);

				const string Error = "This Master Bill Number already exists on another Consol, Shipment or Booking.\r\nPlease select another number.";
				const string BillNumber1 = "xxxsnth";
				const string BillNumber2 = "xxxaoeu";

				ForwardingConsol oldConsol1 = Factory.New<ForwardingConsol>();
				oldConsol1.JK_TransportMode = Constants.TransportModes.Air;
				oldConsol1.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-11);
				oldConsol1.JK_MasterBillNum = BillNumber1;

				ForwardingConsol oldConsol2 = Factory.New<ForwardingConsol>();
				oldConsol2.JK_TransportMode = Constants.TransportModes.Air;
				oldConsol2.JK_SystemCreateTimeUtc = ZDateTime.UtcNow.AddMonths(-13);
				oldConsol2.JK_MasterBillNum = BillNumber2;

				Factory.Save();

				ForwardingConsol newConsol = Factory.New<ForwardingConsol>();
				newConsol.JK_TransportMode = Constants.TransportModes.Air;
				newConsol.JK_RL_NKLoadPort = HomePort;
				newConsol.JK_RL_NKDischargePort = OverseasPort;
				newConsol.JK_MasterBillNum = BillNumber1;
				newConsol.Validation.ValidateMasterBillMAWB();
				AssertHasError(newConsol.MasterBillMAWBInfo, Error);

				newConsol.JK_TransportMode = Constants.TransportModes.Sea;
				newConsol.Validation.ValidateMasterBillMAWB();
				AssertNoErrors(newConsol.MasterBillMAWBInfo);

				newConsol.JK_MasterBillNum = BillNumber2;
				newConsol.JK_TransportMode = Constants.TransportModes.Air;
				newConsol.Validation.ValidateMasterBillMAWB();
				AssertNoError(newConsol.MasterBillMAWBInfo, Error);

				newConsol.JK_TransportMode = Constants.TransportModes.Sea;
				newConsol.Validation.ValidateMasterBillMAWB();
				AssertNoError(newConsol.MasterBillMAWBInfo, Error);
			}
			finally
			{
				Enterprise.Registry.Business.FreightDataRegistry.Instance.MAWBRecyclePeriod.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, savedRecyclePeriod);
			}
		}

		public void TestValidateMasterBillAirlinePrefix()
		{
			SetConsolAirLoadingMasterBill();
			Consol.MasterBillMAWB = ZString.Empty;
			Consol.JK_IsNeutralMaster = true;

			Transport transport = Consol.Transports[0];
			transport.JW_VoyageFlight = "QF123";

			Consol.Validation.ValidateMasterBillAirlinePrefix();
			AssertNoWarnings(Consol.MasterBillAirlinePrefixInfo);

			Consol.MasterBillAirlinePrefix = "083";
			Consol.Validation.ValidateMasterBillAirlinePrefix();
			AssertHasWarning(Consol.MasterBillAirlinePrefixInfo, "The Airline Prefix does not match the Airline 2 Letter Code in the Flight Number.");
		}

		public void TestMasterBillMAWBWhenChangingTransportModeFromAIR()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			consol.MasterBillAirlinePrefix = "081";

			consol.JK_IsNeutralMaster = true;
			var transport = consol.Transports[0];
			transport.JW_VoyageFlight = "QF1234";

			consol.RunPreSaveValidation();
			AssertHasNotifications("Should have air specific notifications", consol.MasterBillMAWBInfo);

			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.RunPreSaveValidation();
			AssertNoNotifications("Should have cleared all notifications as this is air specific", consol.MasterBillMAWBInfo);
		}

		public void TestValidateJK_AWBServiceLevel()
		{
			Consol.JK_IsNeutralMaster = true;
			SetConsolAirLoadingMasterBill();

			Consol.JK_AWBServiceLevel = "STD";
			AssertEquals(false, Consol.JK_AWBServiceLevelInfo.HasErrors());
			Consol.JK_AWBServiceLevel = "";
			AssertEquals(false, Consol.JK_AWBServiceLevelInfo.HasErrors());

			Consol.JK_AWBServiceLevel = "ABC";
			AssertEquals(true, Consol.JK_AWBServiceLevelInfo.HasErrors());
		}

		void SetConsolAirLoadingMasterBill()
		{
			Consol.JK_AgentType = "AGT";
			Consol.JK_TransportMode = Constants.TransportModes.Air;

			Transport transport = Consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";

			Consol.JK_MasterBillNum = "17610000001";
		}

		#endregion

		#region CIMEDIMessages

		public void TestValidateCurrentStatus_NoError()
		{
			ForwardingConsol consol = Factory.NewWithValidTestData<ForwardingConsol>();

			CIMEDIMessage message = consol.CIMEDIMessages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FMA;
			Factory.Save();

			consol.Validation.ValidateAWBCurrentStatus();
			AssertEquals(false, consol.AWBCurrentStatusInfo.HasMessageErrors());

			message = consol.CIMEDIMessages.AddNew();
			message.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message.EM_MessageType = CargoIMPMessageTypeList.Codes.FNA;
			Factory.Save();
			consol.Validation.ValidateAWBCurrentStatus();
			AssertEquals(true, consol.AWBCurrentStatusInfo.HasMessageErrors());
		}

		#endregion

		#region JK_RequiresTemperatureControl

		public void TestValidateJK_RequiresTemperatureControl_ErrorWhenNotSetWithSpecifiedRange()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RequiredTemperatureMinimum = 0;
			consol.JK_RequiredTemperatureMaximum = 0;
			consol.JK_RequiresTemperatureControl = false;

			AssertNoError(consol.JK_RequiresTemperatureControlInfo, "Is Temperature Control flag should be set when a temperature range is specified.");

			consol.JK_RequiredTemperatureMaximum = 1;
			consol.JK_RequiresTemperatureControl = false;
			AssertHasError(consol.JK_RequiresTemperatureControlInfo, "Is Temperature Control flag should be set when a temperature range is specified.");
		}

		public void TestValidateJK_RequiresTemperatureControl_ErrorOnTempControlledShipments()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipmentA = consol.Shipments.AddNew();
			shipmentA.JS_UniqueConsignRef = "SHIPMENTA";
			var packlineA = shipmentA.OuterPackLines.AddNew();
			packlineA.JL_RequiresTemperatureControl = false;

			var shipmentB = consol.Shipments.AddNew();
			shipmentB.JS_UniqueConsignRef = "SHIPMENTB";
			var packlineB = shipmentB.OuterPackLines.AddNew();
			packlineB.JL_RequiresTemperatureControl = true;

			var shipmentC = consol.Shipments.AddNew();
			shipmentC.JS_UniqueConsignRef = "SHIPMENTC";
			var packlineC = shipmentC.OuterPackLines.AddNew();
			packlineC.JL_RequiresTemperatureControl = true;

			consol.JK_RequiresTemperatureControl = false;

			consol.Validation.ValidateJK_RequiresTemperatureControl();

			var unsupportedShipments = new string[] { "Shipment SHIPMENTB", "Shipment SHIPMENTC" };
			var expectedError = "The following Shipment(s) attached to this consol have temperature controlled cargo that is not supported by this consol: " + string.Join(System.Environment.NewLine, unsupportedShipments);

			AssertHasError(consol.JK_RequiresTemperatureControlInfo, expectedError);
		}

		public void TestValidateJK_RequiresTemperatureControl_ErrorOnShipmentsOutsideRange()
		{
			var consol = Factory.New<ForwardingConsol>();

			var shipmentA = consol.Shipments.AddNew();
			shipmentA.JS_UniqueConsignRef = "SHIPMENTA";

			var packlineWithoutTempControl = shipmentA.OuterPackLines.AddNew();
			packlineWithoutTempControl.JL_RequiresTemperatureControl = false;

			var shipmentB = consol.Shipments.AddNew();
			shipmentB.JS_UniqueConsignRef = "SHIPMENTB";

			var packlineTempInRangeInFahrenheit = shipmentB.OuterPackLines.AddNew();
			packlineTempInRangeInFahrenheit.JL_RequiresTemperatureControl = true;
			packlineTempInRangeInFahrenheit.JL_RequiredTemperatureUnit = Constants.Temperature.Fahrenheit;
			packlineTempInRangeInFahrenheit.JL_RequiredTemperatureMinimum = Constants.Temperature.Convert(-50, Constants.Temperature.Centigrade, Constants.Temperature.Fahrenheit);
			packlineTempInRangeInFahrenheit.JL_RequiredTemperatureMaximum = Constants.Temperature.Convert(-10, Constants.Temperature.Centigrade, Constants.Temperature.Fahrenheit);

			var shipmentC = consol.Shipments.AddNew();
			shipmentC.JS_UniqueConsignRef = "SHIPMENTC";

			var packline1WithLowestMin = shipmentC.OuterPackLines.AddNew();
			packline1WithLowestMin.JL_RequiresTemperatureControl = true;
			packline1WithLowestMin.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			packline1WithLowestMin.JL_RequiredTemperatureMinimum = -51;
			packline1WithLowestMin.JL_RequiredTemperatureMaximum = -20;

			var packline2WithHighestMax = shipmentC.OuterPackLines.AddNew();
			packline2WithHighestMax.JL_RequiresTemperatureControl = true;
			packline2WithHighestMax.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			packline2WithHighestMax.JL_RequiredTemperatureMinimum = -30;
			packline2WithHighestMax.JL_RequiredTemperatureMaximum = -9;

			var shipmentD = consol.Shipments.AddNew();
			shipmentD.JS_UniqueConsignRef = "SHIPMENTD";
			var packlineWithTempExactlyOnRange = shipmentD.OuterPackLines.AddNew();
			packlineWithTempExactlyOnRange.JL_RequiresTemperatureControl = true;
			packlineWithTempExactlyOnRange.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			packlineWithTempExactlyOnRange.JL_RequiredTemperatureMinimum = -50;
			packlineWithTempExactlyOnRange.JL_RequiredTemperatureMaximum = -10;

			var shipmentE = consol.Shipments.AddNew();
			shipmentE.JS_UniqueConsignRef = "SHIPMENTE";
			var packline3WithLowestMin = shipmentE.OuterPackLines.AddNew();
			packline3WithLowestMin.JL_RequiresTemperatureControl = true;
			packline3WithLowestMin.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			packline3WithLowestMin.JL_RequiredTemperatureMinimum = -21;
			packline3WithLowestMin.JL_RequiredTemperatureMaximum = 0;

			var shipmentF = consol.Shipments.AddNew();
			shipmentF.JS_UniqueConsignRef = "SHIPMENTF";
			var packline4WithHighestMax = shipmentF.OuterPackLines.AddNew();
			packline4WithHighestMax.JL_RequiresTemperatureControl = true;
			packline4WithHighestMax.JL_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			packline4WithHighestMax.JL_RequiredTemperatureMinimum = -60;
			packline4WithHighestMax.JL_RequiredTemperatureMaximum = -20;

			consol.JK_RequiredTemperatureUnit = Constants.Temperature.Centigrade;
			consol.JK_RequiredTemperatureMinimum = -50;
			consol.JK_RequiredTemperatureMaximum = -10;
			consol.JK_RequiresTemperatureControl = true;

			var expectedError = "The following Shipment(s) attached to this consol have temperature controlled cargo that is not supported by this consol: Shipment SHIPMENTC" + System.Environment.NewLine + "Shipment SHIPMENTE" + System.Environment.NewLine + "Shipment SHIPMENTF";

			AssertHasError(consol.JK_RequiresTemperatureControlInfo, expectedError);
		}

		public void TestValidateJK_RequiresTemperatureControl_WarningWhenConsolRequiresTempControlButPackLineDoesNot()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var shipment1 = consol.Shipments.AddNew();
			shipment1.JS_UniqueConsignRef = "SHIPA";

			var shipment2 = consol.Shipments.AddNew();
			shipment2.JS_UniqueConsignRef = "SHIPB";

			var packLine1 = shipment1.OuterPackLines.AddNew();
			var packLine2 = shipment2.OuterPackLines.AddNew();

			var warningPrefix = "The following Shipment(s) contain non temperature controlled cargo. Please verify the temperature range of this Consol is suitable for this cargo: ";

			packLine1.JL_RequiresTemperatureControl = false;
			packLine2.JL_RequiresTemperatureControl = true;
			consol.JK_RequiresTemperatureControl = true;

			consol.Validation.ValidateJK_RequiresTemperatureControl();

			AssertHasWarning("Temperature Controlled Consol with one shipment packline that does not require temperature control displays a warning about that shipment",
				consol.RequiresTemperatureControlInfo, warningPrefix + "Shipment SHIPA");

			var packLine3 = shipment2.OuterPackLines.AddNew();
			packLine3.JL_RequiresTemperatureControl = false;

			consol.Validation.ValidateJK_RequiresTemperatureControl();

			AssertHasWarning("Temperature Controlled Consol with two shipments with any packline that does not require temperature control displays a warning listing those shipments",
				consol.RequiresTemperatureControlInfo, warningPrefix + "Shipment SHIPA" + System.Environment.NewLine + "Shipment SHIPB");

			packLine2.JL_RequiresTemperatureControl = false;
			consol.RequiresTemperatureControl = false;

			consol.Validation.ValidateJK_RequiresTemperatureControl();

			AssertNoWarning("Non Temperature Controlled Consol with non temperature controlled cargo displays no warning",
				consol.RequiresTemperatureControlInfo, warningPrefix + "Shipment SHIPA" + System.Environment.NewLine + "Shipment SHIPB");
		}

		#endregion

		#region JK_RequiredTemperatureUnit

		public void TestValidateJK_RequiredTemperatureUnit_AcceptedValues()
		{
			var consol = Factory.New<ForwardingConsol>();
			var errorNotification = "Temperature must be set to C (Celsius) or F (Fahrenheit)";

			var validTemps = new string[] { Constants.Temperature.Centigrade, Constants.Temperature.Fahrenheit };
			var invalidTemps = new string[] { Constants.Temperature.Kelvin, "A", "Z", ".", "*", "Y" };

			CombineAssertions("No error should display for accepted temperature units", () =>
			{
				foreach (var validTemp in validTemps)
				{
					consol.JK_RequiredTemperatureUnit = validTemp;
					AssertNoError(consol.JK_RequiredTemperatureUnitInfo, errorNotification);
				}
			});

			CombineAssertions("Error should display for invalid temperature units", () =>
			{
				foreach (var invalidTemp in invalidTemps)
				{
					consol.JK_RequiredTemperatureUnit = invalidTemp;
					AssertHasError(consol.JK_RequiredTemperatureUnitInfo, errorNotification);
				}
			});
		}

		#endregion

		#region JK_RequiredTemperatureMinimum

		public void TestValidateJK_RequiredTemperatureMinimum_Centigrade()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RequiredTemperatureUnit = "C";

			var validDecimals = new ZDecimal[] { -273.1, -273.0, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimals = new ZDecimal[] { -999.9, -500, -459.6, -273.2 };
			var errorSuffix = "°C is below the minimum possible temperature of absolute zero (-273.15°C)";

			foreach (var validDecimal in validDecimals)
			{
				consol.JK_RequiredTemperatureMinimum = validDecimal;

				AssertNoErrorContaining(consol.JK_RequiredTemperatureMinimumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimals)
			{
				consol.JK_RequiredTemperatureMinimum = invalidDecimal;

				AssertHasErrorContaining(consol.JK_RequiredTemperatureMinimumInfo, errorSuffix);
			}
		}

		public void TestValidateJK_RequiredTemperatureMinimum_Fahrenheit()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RequiredTemperatureUnit = "F";

			var validDecimals = new ZDecimal[] { -459.6, -459.5, -333, -273.2, -273.1, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimalsBelowMinimum = new ZDecimal[] { -999.9, -500, -459.7 };
			var errorSuffix = "°F is below the minimum possible temperature of absolute zero (-459.67°F)";

			foreach (var validDecimal in validDecimals)
			{
				consol.JK_RequiredTemperatureMinimum = validDecimal;

				AssertNoErrorContaining(consol.JK_RequiredTemperatureMinimumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimalsBelowMinimum)
			{
				consol.JK_RequiredTemperatureMinimum = invalidDecimal;

				AssertHasErrorContaining(consol.JK_RequiredTemperatureMinimumInfo, errorSuffix);
			}
		}

		#endregion

		#region JK_RequiredTemperature Min & Max Comparison

		public void TestValidateJK_RequiredTemperatureMinimumAndMaximum_Comparison()
		{
			var consol = Factory.New<ForwardingConsol>();

			var minGreaterThanMaxWarning = "Minimum temperature cannot be higher than maximum temperature";
			var maxGreaterThanMinWarning = "Maximum temperature cannot be lower than minimum temperature";

			consol.JK_RequiredTemperatureMinimum = 0;
			consol.JK_RequiredTemperatureMaximum = 0;
			consol.Validation.ValidateAll();

			AssertNoError(consol.JK_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertNoError(consol.JK_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);

			consol.JK_RequiredTemperatureMinimum = 1;
			consol.JK_RequiredTemperatureMaximum = 0;
			consol.Validation.ValidateAll();

			AssertHasError(consol.JK_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertHasError(consol.JK_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);

			consol.JK_RequiredTemperatureMinimum = 0;
			consol.JK_RequiredTemperatureMaximum = 1;
			consol.Validation.ValidateAll();

			AssertNoError(consol.JK_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertNoError(consol.JK_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);

			consol.JK_RequiredTemperatureMinimum = -1.0;
			consol.JK_RequiredTemperatureMaximum = -1.1;
			consol.Validation.ValidateAll();

			AssertHasError(consol.JK_RequiredTemperatureMinimumInfo, minGreaterThanMaxWarning);
			AssertHasError(consol.JK_RequiredTemperatureMaximumInfo, maxGreaterThanMinWarning);
		}

		#endregion

		#region JK_RequiredTemperatureMaximum

		public void TestValidateJK_RequiredTemperatureMaximum_Centigrade()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RequiredTemperatureUnit = "C";

			var validDecimals = new ZDecimal[] { -273.1, -273.0, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimals = new ZDecimal[] { -999.9, -500, -459.6, -273.2 };
			var errorSuffix = "°C is below the minimum possible temperature of absolute zero (-273.15°C)";

			foreach (var validDecimal in validDecimals)
			{
				consol.JK_RequiredTemperatureMaximum = validDecimal;

				AssertNoErrorContaining(consol.JK_RequiredTemperatureMaximumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimals)
			{
				consol.JK_RequiredTemperatureMaximum = invalidDecimal;

				AssertHasErrorContaining(consol.JK_RequiredTemperatureMaximumInfo, errorSuffix);
			}
		}

		public void TestValidateJK_RequiredTemperatureMaximum_Fahrenheit()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RequiredTemperatureUnit = "F";

			var validDecimals = new ZDecimal[] { -459.6, -459.5, -333, -273.2, -273.1, -100, -1.9, 0, 0.1, 123, 999, 999.9 };
			var invalidDecimals = new ZDecimal[] { -999.9, -500, -459.7 };
			var errorSuffix = "°F is below the minimum possible temperature of absolute zero (-459.67°F)";

			foreach (var validDecimal in validDecimals)
			{
				consol.JK_RequiredTemperatureMaximum = validDecimal;

				AssertNoErrorContaining(consol.JK_RequiredTemperatureMaximumInfo, errorSuffix);
			}

			foreach (var invalidDecimal in invalidDecimals)
			{
				consol.JK_RequiredTemperatureMaximum = invalidDecimal;

				AssertHasErrorContaining(consol.JK_RequiredTemperatureMaximumInfo, errorSuffix);
			}
		}

		#endregion

		#region JK_MaximumPackageDimensions

		public void TestValidateJK_MaximumPackageDimensions_NotFilled()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_MaximumAllowablePackageLength = 0;
			consol.JK_MaximumAllowablePackageWidth = 0;
			consol.JK_MaximumAllowablePackageHeight = 0;
			consol.JK_MaximumAllowablePackageUnit = ZString.Empty;

			var errorMessage = "Please enter the dimensions into all fields.";

			AssertNoError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
			AssertNoError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
			AssertNoError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);
			AssertNoError(consol.JK_MaximumAllowablePackageUnitInfo, errorMessage);

			consol.JK_MaximumAllowablePackageLength = 5;

			AssertNoError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
			AssertHasError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
			AssertHasError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);
			AssertHasError(consol.JK_MaximumAllowablePackageUnitInfo, errorMessage);

			consol.JK_MaximumAllowablePackageWidth = 5;
			consol.JK_MaximumAllowablePackageHeight = 5;
			consol.JK_MaximumAllowablePackageUnit = Constants.Length.Metres;

			AssertNoError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
			AssertNoError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
			AssertNoError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);
			AssertNoError(consol.JK_MaximumAllowablePackageUnitInfo, errorMessage);
		}

		public void TestValidateJK_MaximumPackageDimensions_Packlines_Errors()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Dimensions.Action = PreAllocationCheck.Actions.Restriction;
			checks.Dimensions.Percentage = 100m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_MaximumAllowablePackageLength = 7;
				consol.JK_MaximumAllowablePackageWidth = 6;
				consol.JK_MaximumAllowablePackageHeight = 5;
				consol.JK_MaximumAllowablePackageUnit = Constants.Length.Metres;

				var shipment = consol.Shipments.AddNew();

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_Length = 7;
				packLine.JL_Width = 6;
				packLine.JL_Height = 5;
				packLine.JL_UnitOfDimension = Constants.Length.Metres;

				AssertGreaterThanOrEqualTo("Precondition: length is less than allowed length", consol.JK_MaximumAllowablePackageLength, packLine.JL_Length);
				AssertGreaterThanOrEqualTo("Precondition: width is less than allowed width", consol.JK_MaximumAllowablePackageWidth, packLine.JL_Width);
				AssertGreaterThanOrEqualTo("Precondition: height is less than allowed height", consol.JK_MaximumAllowablePackageHeight, packLine.JL_Height);

				var errorMessage = MaximumPackageDimensionsPacklinesErrorMessage;

				AssertNoError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);

				consol.JK_MaximumAllowablePackageLength = 4;
				consol.JK_MaximumAllowablePackageWidth = 4;
				consol.JK_MaximumAllowablePackageHeight = 4;

				AssertHasError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
				AssertHasError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
				AssertHasError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);

				consol.JK_MaximumAllowablePackageLength = 6;
				consol.JK_MaximumAllowablePackageWidth = 7;
				consol.JK_MaximumAllowablePackageHeight = 5;

				AssertNoError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);
			}
		}

		public void TestValidateJK_MaximumPackageDimensions_PacklinesConvertedUnits_Errors()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Dimensions.Action = PreAllocationCheck.Actions.Restriction;
			checks.Dimensions.Percentage = 100m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_MaximumAllowablePackageLength = 5;
				consol.JK_MaximumAllowablePackageWidth = 5;
				consol.JK_MaximumAllowablePackageHeight = 5;
				consol.JK_MaximumAllowablePackageUnit = Constants.Length.Metres;

				var shipment = consol.Shipments.AddNew();

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_Length = 10;
				packLine.JL_Width = 10;
				packLine.JL_Height = 10;
				packLine.JL_UnitOfDimension = Constants.Length.Feet;

				AssertGreaterThan<ZDecimal>("Precondition: length is less than allowed length", consol.JK_MaximumAllowablePackageLength, Constants.Length.Convert(packLine.JL_Length, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertGreaterThan<ZDecimal>("Precondition: width is less than allowed width", consol.JK_MaximumAllowablePackageWidth, Constants.Length.Convert(packLine.JL_Width, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertGreaterThan<ZDecimal>("Precondition: height is less than allowed height", consol.JK_MaximumAllowablePackageHeight, Constants.Length.Convert(packLine.JL_Height, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));

				var errorMessage = MaximumPackageDimensionsPacklinesErrorMessage;

				AssertNoError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);

				consol.JK_MaximumAllowablePackageLength = 2;
				consol.JK_MaximumAllowablePackageWidth = 2;
				consol.JK_MaximumAllowablePackageHeight = 2;

				AssertLessThan<ZDecimal>("Precondition: length is greater than allowed length", consol.JK_MaximumAllowablePackageLength, Constants.Length.Convert(packLine.JL_Length, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertLessThan<ZDecimal>("Precondition: width is greater than allowed width", consol.JK_MaximumAllowablePackageWidth, Constants.Length.Convert(packLine.JL_Width, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertLessThan<ZDecimal>("Precondition: height is greater than allowed height", consol.JK_MaximumAllowablePackageHeight, Constants.Length.Convert(packLine.JL_Height, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));

				AssertHasError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
				AssertHasError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
				AssertHasError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);
			}
		}

		public void TestValidateJK_MaximumPackageDimensions_ConsolDimensionFieldsNotFilled()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.Dimensions.Action = PreAllocationCheck.Actions.Restriction;
			checks.Dimensions.Percentage = 100m;

			using (ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, checks))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_MaximumAllowablePackageLength = 0;
				consol.JK_MaximumAllowablePackageWidth = 0;
				consol.JK_MaximumAllowablePackageHeight = 0;
				consol.JK_MaximumAllowablePackageUnit = ZString.Empty;

				var shipment = consol.Shipments.AddNew();

				var packLine = shipment.OuterPackLines.AddNew();
				packLine.JL_Length = 10;
				packLine.JL_Width = 10;
				packLine.JL_Height = 10;
				packLine.JL_UnitOfDimension = Constants.Length.Feet;

				var errorMessage = MaximumPackageDimensionsPacklinesErrorMessage;

				AssertNoError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
				AssertNoError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);

				consol.JK_MaximumAllowablePackageLength = 2;
				consol.JK_MaximumAllowablePackageWidth = 2;
				consol.JK_MaximumAllowablePackageHeight = 2;
				consol.JK_MaximumAllowablePackageUnit = Constants.Length.Metres;

				AssertLessThan<ZDecimal>("Precondition: length is greater than allowed length", consol.JK_MaximumAllowablePackageLength, Constants.Length.Convert(packLine.JL_Length, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertLessThan<ZDecimal>("Precondition: width is greater than allowed width", consol.JK_MaximumAllowablePackageWidth, Constants.Length.Convert(packLine.JL_Width, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));
				AssertLessThan<ZDecimal>("Precondition: height is greater than allowed height", consol.JK_MaximumAllowablePackageHeight, Constants.Length.Convert(packLine.JL_Height, Constants.Length.Feet, consol.JK_MaximumAllowablePackageUnit));

				AssertHasError(consol.JK_MaximumAllowablePackageLengthInfo, errorMessage);
				AssertHasError(consol.JK_MaximumAllowablePackageWidthInfo, errorMessage);
				AssertHasError(consol.JK_MaximumAllowablePackageHeightInfo, errorMessage);
			}
		}

		#region ValidateOrganisationPayables

		public void TestValidateExportImportCreditorOrganisationPayables_IsNotPayable()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = false;

			consol.CarrierExportCreditorAddress.OrganisationPK = org.PK;
			consol.CarrierImportCreditorAddress.OrganisationPK = org.PK;

			//check WI00532393 - Remove Validation for Carrier Import/ Export addresses to see why we decided to remove validations for Carrier Export Creditor Address
			AssertNoErrors("We should not validate Carrier Export Creditor Address", consol.CarrierExportCreditorAddress.OrganisationPKInfo);
			AssertNoErrors("We should not validate Carrier Import Creditor Address", consol.CarrierImportCreditorAddress.OrganisationPKInfo);

			org.OH_IsCreditor = true;
			Factory.Save();

			consol.CarrierExportCreditorAddress.Validation.ValidateOrganisationPK();
			consol.CarrierImportCreditorAddress.Validation.ValidateOrganisationPK();

			AssertNoErrors(consol.CarrierExportCreditorAddress.OrganisationPKInfo);
			AssertNoErrors(consol.CarrierImportCreditorAddress.OrganisationPKInfo);
		}

		#endregion

		const string MaximumPackageDimensionsPacklinesErrorMessage = "The attached packlines have dimensions that exceeds the maximum dimensions.";

		#endregion

		public void TestJK_OA_PackDepotAddress()
		{
			void TestJK_OA_PackDepotAddress(bool enableAdvOrmFeature, Action<ZPropertyInfo, string> assertDifference)
			{
				var warningMessage = "The consolidation departure CFS address does not match the CFS address on the linked container load plan.";

				AdvOrmFeatureHelper.RunTestWith(enableAdvOrmFeature, action: () =>
				{
					var org1 = Factory.NewWithValidTestData<OrgHeader>();
					var org2 = Factory.NewWithValidTestData<OrgHeader>();

					var containerLoadPlan = Factory.NewWithValidTestData<CFSContainerLoadList>();
					containerLoadPlan.CLH_OA_CFSAddress = org1.MainAddress.PK;

					var consol = Factory.NewWithValidTestData<ForwardingConsol>();
					var container = consol.Containers.AddNew();
					container.JC_CLH_LoadListPlan = containerLoadPlan.PK;

					consol.RunPreSaveValidation();
					assertDifference(consol.JK_OA_PackDepotAddressInfo, warningMessage);

					consol.JK_OA_PackDepotAddress = org1.MainAddress.PK;
					consol.RunPreSaveValidation();
					AssertNoWarning(consol.JK_OA_PackDepotAddressInfo, warningMessage);

					consol.JK_OA_PackDepotAddress = org2.MainAddress.PK;
					consol.RunPreSaveValidation();
					assertDifference(consol.JK_OA_PackDepotAddressInfo, warningMessage);

					containerLoadPlan.CLH_OA_CFSAddress = org2.MainAddress.PK;
					consol.RunPreSaveValidation();
					AssertNoWarning(consol.JK_OA_PackDepotAddressInfo, warningMessage);

					containerLoadPlan.CLH_OA_CFSAddress = org1.MainAddress.PK;
					consol.RunPreSaveValidation();
					assertDifference(consol.JK_OA_PackDepotAddressInfo, warningMessage);
				});
			}

			TestJK_OA_PackDepotAddress(false, AssertNoWarning);
			TestJK_OA_PackDepotAddress(true, AssertHasWarning);
		}

		#region JK_PackageGrouping

		public void TestJK_PackageGrouping()
		{
			var consol = Factory.New<ForwardingConsol>();

			AssertEquals(Constants.PackageGrouping.Codes.DoNotGroup, consol.JK_PackageGrouping);
			AssertNoErrors(consol.JK_PackageGroupingInfo);

			consol.JK_PackageGrouping = ZString.Empty;
			AssertHasErrors(consol.JK_PackageGroupingInfo);

			consol.JK_PackageGrouping = "AAA";
			AssertHasErrors(consol.JK_PackageGroupingInfo);

			consol.JK_PackageGrouping = Constants.PackageGrouping.Codes.GroupByShipment;
			AssertNoErrors(consol.JK_PackageGroupingInfo);
		}

		#endregion

		#region Contract Allocation

		public void TestValidateJK_CarrierContractNumber_InvalidDatesWhenAllocatedToContract()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_GS_NKContractOwner = "FOW";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(5);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_MasterBillNum = "A";
			consol.JK_BookingReference = "A";

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today;
			Factory.Save();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasError(consol.JK_CarrierContractNumberInfo,
					$"None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date ({contract.RCT_StartDate.ToShortDateString()}) and the Expiry Date ({contract.RCT_EndDate.ToShortDateString()}) of Contract {contract.RCT_ContractNumber}.");
			}
		}

		public void TestValidateJK_CarrierContractNumber_ValidationWhenInvalidDatesNoAllocationChanges()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(5);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			contract.Allocations.Add(allocationRoute);
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate;
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(8);
			Factory.Save();
			transport.JW_ETD = ZDate.Today;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasErrors(consol.JK_CarrierContractNumberInfo);
			}
		}

		public void TestValidateJK_CarrierContractNumber_ValidationWhenInvalidDatesAndMBLExists()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(5);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			contract.Allocations.Add(allocationRoute);
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate;
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.JK_MasterBillNum = "A";

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(8);
			Factory.Save();
			transport.JW_ETD = ZDate.Today;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasErrors(consol.JK_CarrierContractNumberInfo);
			}
		}

		public void TestValidateJK_CarrierContractNumber_ValidationWhenInvalidDatesAndBookingExists()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(5);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			contract.Allocations.Add(allocationRoute);
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate;
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.JK_BookingReference = "A";

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(8);
			Factory.Save();
			transport.JW_ETD = ZDate.Today;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasErrors(consol.JK_CarrierContractNumberInfo);
			}
		}

		public void TestJK_CarrierContractNumber()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "SHREK1ONVHS";

			var carrierContract1 = Factory.New<RatingContract>();
			carrierContract1.RCT_ContractNumber = "SHREK1234";
			carrierContract1.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract1.RCT_StartDate = ZDate.Today.Add(new TimeSpan(-5, 12, 0, 0)).Date;
			carrierContract1.RCT_EndDate = ZDate.Today.Add(new TimeSpan(5, 12, 0, 0)).Date;
			carrierContract1.RCT_TransportMode = Constants.TransportModes.Sea;
			carrierContract1.RCT_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var carrierContract2 = Factory.New<RatingContract>();
			carrierContract2.RCT_ContractNumber = carrierContract1.RCT_ContractNumber;
			carrierContract2.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract2.RCT_OH = Factory.NewWithValidTestData<OrgHeader>().PK;

			var allocationLine = carrierContract1.Allocations.AddNew();

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Sea;

				var voyageTransport = consol.Transports.MostInterestingTransport;
				voyageTransport.JW_ETD = ZDate.Today;

				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoWarnings(consol.JK_CarrierContractNumberInfo);

				consol.JK_CarrierContractNumber = carrierContract1.RCT_ContractNumber;
				consol.JK_OA_ShippingLineAddress = ZGuid.NewZGuid();
				consol.JK_RCA_AllocationLine = ZGuid.Empty;
				consol.Validation.ValidateJK_CarrierContractNumber();
				var expectedMessage = "Carrier of Consol does not match Service Provider(s) of any Carrier Contract(s) with this number.";
				AssertHasWarning(consol.JK_CarrierContractNumberInfo, expectedMessage);

				consol.JK_RCA_AllocationLine = allocationLine.PK;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasError(consol.JK_CarrierContractNumberInfo, expectedMessage);

				consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
				consol.JK_RCA_AllocationLine = ZGuid.Empty;
				consol.Validation.ValidateJK_CarrierContractNumber();
				expectedMessage = $"Carrier '{carrier.OH_Code}' of Consol does not match Service Provider(s) of any Carrier Contract(s) with this number.";
				AssertHasWarning(consol.JK_CarrierContractNumberInfo, expectedMessage);

				consol.JK_RCA_AllocationLine = allocationLine.PK;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasError(consol.JK_CarrierContractNumberInfo, expectedMessage);

				carrierContract1.RCT_OH = carrier.PK;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoWarnings(consol.JK_CarrierContractNumberInfo);

				var times = new[]
				{
					new TimeSpan(0, 0, 0),
					new TimeSpan(11, 0, 0),
					new TimeSpan(13, 0, 0),
					new TimeSpan(23, 59, 59),
				};

				foreach (var time in times)
				{
					voyageTransport.JW_ETD = carrierContract1.RCT_StartDate.Add(time);
					consol.Validation.ValidateJK_CarrierContractNumber();
					AssertNoErrors(consol.JK_CarrierContractNumberInfo);

					voyageTransport.JW_ETD = carrierContract1.RCT_EndDate.Add(-time);
					consol.Validation.ValidateJK_CarrierContractNumber();
					AssertNoErrors(consol.JK_CarrierContractNumberInfo);
				}

				voyageTransport.JW_ETD = carrierContract1.RCT_StartDate.AddDays(-1);
				consol.Validation.ValidateJK_CarrierContractNumber();
				expectedMessage = $"None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period between the Start Date ({carrierContract1.RCT_StartDate.ToShortDateString()}) and the Expiry Date ({carrierContract1.RCT_EndDate.ToShortDateString()}) of Contract {carrierContract1.RCT_ContractNumber}.";
				AssertHasError(consol.JK_CarrierContractNumberInfo, expectedMessage);

				voyageTransport.JW_ETD = carrierContract1.RCT_EndDate.AddDays(1);
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasError(consol.JK_CarrierContractNumberInfo, expectedMessage);

				carrierContract1.RCT_EndDate = ZDate.Empty;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoErrors(consol.JK_CarrierContractNumberInfo);

				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.Validation.ValidateJK_CarrierContractNumber();
				expectedMessage = $"Transport Mode '{consol.JK_TransportMode}' of Consol is NOT the same as the Transport Mode '{carrierContract1.RCT_TransportMode}' of Carrier Contract for allocation.";
				AssertHasError(consol.JK_CarrierContractNumberInfo, expectedMessage);
			}
		}

		public void TestJK_CarrierContractNumber_WarningWhenNoMatchingContracts()
		{
			var clientContractNumber = "CLIENT";
			var carrierContractNumber = "CARRIER";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var message = "This number does not have a corresponding Carrier Contract & Allocations record.";

			var clientContract = (IRatingContract)Factory.NewWithValidTestData(ObjectFactory.GetType<IRatingContract>());
			clientContract.RCT_ContractType = Constants.RatingContractTypes.Client;
			clientContract.RCT_ContractNumber = clientContractNumber;

			var carrierContract = (IRatingContract)Factory.NewWithValidTestData(ObjectFactory.GetType<IRatingContract>());
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_ContractNumber = carrierContractNumber;

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_CarrierContractNumber = "";
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoWarning("empty contract number should have no warning", consol.JK_CarrierContractNumberInfo, message);

				consol.JK_CarrierContractNumber = clientContractNumber;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasWarning("should have warning since it is a client number", consol.JK_CarrierContractNumberInfo, message);

				consol.JK_CarrierContractNumber = carrierContractNumber;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoWarning("should have no warning", consol.JK_CarrierContractNumberInfo, message);
			}
		}

		public void TestJK_CarrierContractNumber_WarningWhenNoMatchingNamedAccounts()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "EvilInc01";

			var carrierContract1 = Factory.New<RatingContract>();
			carrierContract1.RCT_ContractNumber = "INCONSPICUOUSCONTRACT73";
			carrierContract1.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract1.RCT_StartDate = ZDate.Today.AddDays(-5);
			carrierContract1.RCT_EndDate = ZDate.Today.AddDays(5);
			carrierContract1.RCT_TransportMode = Constants.TransportModes.Sea;
			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			carrierContract1.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_CarrierContractNumber = carrierContract1.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			carrierContract1.RCT_OH = carrier.PK;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ControllingCustomerNameOrPK = Factory.NewWithValidTestData<OrgHeader>().PK.ToString();

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				var message = $"All of the Consol's Shipments should have at least one client that matches a Named Account of Carrier Contract {carrierContract1.RCT_ContractNumber}" +
					" (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers for a Consol Shipment).";
				AssertHasWarning("should have warning since there is no matching named account", consol.JK_CarrierContractNumberInfo, message);

				shipment.ConsigneePK = namedAccount.PK;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoWarning("should have no warning", consol.JK_CarrierContractNumberInfo, message);
			}
		}

		public void TestJK_CarrierContractNumber_NoWarningIfEnableCarrierAndClientContractModulesFalse()
		{
			var clientContractNumber = "CLIENT";
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var message = "This number does not have a corresponding Carrier Contract & Allocations record.";

			var clientContract = (IRatingContract)Factory.NewWithValidTestData(ObjectFactory.GetType<IRatingContract>());
			clientContract.RCT_ContractType = Constants.RatingContractTypes.Client;
			clientContract.RCT_ContractNumber = clientContractNumber;

			consol.JK_CarrierContractNumber = clientContractNumber;

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasWarning("should have warning since it is a client number", consol.JK_CarrierContractNumberInfo, message);
			}

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoWarning("should have no warning since registry was disabled", consol.JK_CarrierContractNumberInfo, message);
			}
		}

		public void TestJK_CarrierContractNumber_ErrorWhenContainerTypeNotMatchContractContainerType()
		{
			var message = "NOT all Containers on Consol GOODRIDDANCE share the same Container Type (DRY) of the selected Carrier Contract RCT69420. Consol Container(s) and selected Carrier Contract should share the same Container Type.";
			var contractNumber = "RCT69420";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "GOODRIDDANCE";

			var contract = Factory.NewWithValidTestData<RatingContract>();
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_ContractNumber = contractNumber;
			contract.RCT_ContainerType = "DRY";

			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_ContainerType = "OTH";

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "GOBLINJR";
			container1.JC_RC = refContainer1.PK;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "GONNACRY";
			container2.JC_RC = refContainer1.PK;

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.JK_CarrierContractNumber = "";
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoError("empty contract number should have no error", consol.JK_CarrierContractNumberInfo, message);

				consol.JK_OA_ShippingLineAddress = contract.ServiceProvider.MainAddress.PK;
				consol.JK_CarrierContractNumber = contractNumber;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasError("should have error since no container type matches contract's", consol.JK_CarrierContractNumberInfo, message);

				refContainer1.RC_ContainerType = "DRY";
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoError("should have no error since container type matches contract's", consol.JK_CarrierContractNumberInfo, message);
			}
		}

		public void TestJK_CarrierContractNumber_ErrorWhenConsolHazardousAndContractNot()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "EvilInc01";

			var contract = Factory.New<IRatingContract>();
			contract.RCT_ContractNumber = "FIONAAA";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_AllowHazardousCommodities = true;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_IsHazardous = true;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var hazardousCommodity = Factory.New<RefCommodityCode>();
			hazardousCommodity.RH_Code = "SHRK";
			hazardousCommodity.RH_IsHazardous = true;

			var safeCommodity = Factory.New<RefCommodityCode>();
			safeCommodity.RH_Code = "DNKY";
			safeCommodity.RH_IsHazardous = false;

			var container1 = consol.Containers.AddNew();
			container1.JC_RH_NKContainerCommodityCode = safeCommodity.RH_Code;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "DRAGON";
			container2.JC_RH_NKContainerCommodityCode = hazardousCommodity.RH_Code;

			var container3 = consol.Containers.AddNew();
			container3.JC_RH_NKContainerCommodityCode = hazardousCommodity.RH_Code;

			var consolHazardousMessage = $"Hazardous Commodities are not allowed for Carrier Contract {contract.RCT_ContractNumber} but Consol {consol.JK_UniqueConsignRef} has Pre-Allocation > Is Hazardous checked. Only Consol(s) with 'Is Hazardous' not checked can be allocated.";
			var containerWithNumHazardousMessage = $"Hazardous Commodities are not allowed for Carrier Contract {contract.RCT_ContractNumber} but Container {container2.JC_ContainerNum} has Commodity {hazardousCommodity.RH_Code} with 'Is this Commodity Hazardous' checked. Only Consol(s) and Container(s) without Hazardous Commodities can be allocated.";

			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertNoError(consol.JK_CarrierContractNumberInfo, consolHazardousMessage);
				AssertNoError(consol.JK_CarrierContractNumberInfo, containerWithNumHazardousMessage);

				contract.RCT_AllowHazardousCommodities = false;
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasError(consol.JK_CarrierContractNumberInfo, consolHazardousMessage);
				AssertHasError(consol.JK_CarrierContractNumberInfo, containerWithNumHazardousMessage);

				var containerWithoutNumHazardousMessage = $"Hazardous Commodities are not allowed for Carrier Contract {contract.RCT_ContractNumber} but Container has Commodity {hazardousCommodity.RH_Code} with 'Is this Commodity Hazardous' checked. Only Consol(s) and Container(s) without Hazardous Commodities can be allocated.";

				consol.Containers.Remove(container2);
				consol.Validation.ValidateJK_CarrierContractNumber();
				AssertHasError(consol.JK_CarrierContractNumberInfo, containerWithoutNumHazardousMessage);
			}
		}

		public void TestJK_RCA_AllocationLine_WarningWhenNoMatchingNamedAccounts()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			carrier.OH_Code = "RivalEvilCo";

			var carrierContract = Factory.New<RatingContract>();
			carrierContract.RCT_ContractNumber = "WorldDominationAttempt03";
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_StartDate = ZDate.Today.AddDays(-5);
			carrierContract.RCT_EndDate = ZDate.Today.AddDays(5);
			carrierContract.RCT_TransportMode = Constants.TransportModes.Sea;
			carrierContract.RCT_OH = carrier.PK;

			var allocationLine = carrierContract.Allocations.AddNew();
			var namedAccount = Factory.NewWithValidTestData<OrgHeader>();
			allocationLine.NamedAccountPivots.AddRelatedIfNotExist(namedAccount);
			allocationLine.RCA_LoadLocation = "AUSYD";
			allocationLine.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationLine.PK;
			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;

			var voyageTransport = consol.Transports.MostInterestingTransport;
			voyageTransport.JW_ETD = ZDate.Today;
			voyageTransport.JW_RL_NKLoadPort = allocationLine.RCA_LoadLocation;
			voyageTransport.JW_RL_NKDiscPort = allocationLine.RCA_DischargeLocation;

			var shipment = consol.Shipments.AddNew();
			shipment.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ConsignorPK = Factory.NewWithValidTestData<OrgHeader>().PK;
			shipment.ControllingCustomerNameOrPK = Factory.NewWithValidTestData<OrgHeader>().PK.ToString();

			consol.Validation.ValidateJK_RCA_AllocationLine();
			var message = $"All of the Consol's Shipments should have at least one client that matches a Named Account of Carrier Contract {carrierContract.RCT_ContractNumber} and Allocation Route {allocationLine.RCA_AllocationLineID}" +
				" (i.e. no matching Clients, Consignors, Consignees, or Controlling Customers for a Consol Shipment).";
			AssertHasWarning("should have warning since there is no matching named account", consol.JK_RCA_AllocationLineInfo, message);

			shipment.ConsigneePK = namedAccount.PK;
			AssertConsolAllocationLineHasNoErrors(consol);
		}

		public void TestJK_RCA_AllocationLine()
		{
			var nz = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "NZ");
			var ausyd = Factory.LoadFromNaturalKey<RefUNLOCO>(RefUNLOCOSchema.RL_Code, "AUSYD");

			var intZone = Factory.New<RefZoneHeader>();
			intZone.FZ_ZoneType = RefZoneHeaderLookups.ZoneTypeCodes.Contract;
			intZone.FZ_Code = "SHRK";
			intZone.UNLOCOs.Add(ausyd);
			intZone.Countries.Add(nz);

			var vessel = Factory.New<RefVessel>();
			vessel.RV_Code = "SHREKS SHIP";

			var refContainer1 = Factory.New<RefContainer>();
			refContainer1.RC_ContainerType = "WOW";
			refContainer1.RC_Code = "FIONA";
			refContainer1.RC_FreightRateClass = "XXX";
			refContainer1.RC_StorageClass = "YYY";
			refContainer1.RC_TEU = 1;

			var refContainer2 = Factory.New<RefContainer>();
			refContainer2.RC_ContainerType = "TOP";
			refContainer2.RC_Code = "PUSS";
			refContainer2.RC_FreightRateClass = "XXX";
			refContainer2.RC_StorageClass = "YYY";
			refContainer2.RC_TEU = 1;

			var carrier = Factory.New<OrgHeader>();

			var carrierContract1 = Factory.New<RatingContract>();
			carrierContract1.RCT_ContractNumber = "SHREKTRACT";
			carrierContract1.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract1.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract1.RCT_OH = carrier.PK;

			var allocationRoute1 = carrierContract1.Allocations.AddNew();
			allocationRoute1.RCA_StartDate = ZDate.Today.Add(new TimeSpan(-5, 12, 0, 0)).Date;
			allocationRoute1.RCA_ExpiryDate = ZDate.Today.Add(new TimeSpan(5, 12, 0, 0)).Date;
			allocationRoute1.RCA_LoadLocation = "AUSYD";
			allocationRoute1.RCA_DischargeLocation = "NZAKL";
			allocationRoute1.RCA_VoyageNumber = "SHREK123";
			allocationRoute1.RCA_RV_NKVessel = vessel.RV_Code;
			allocationRoute1.RCA_RC_ContainerType = refContainer1.PK;
			allocationRoute1.RCA_AllocatedQuantity = 15;
			allocationRoute1.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.TwentyFootUnits;
			allocationRoute1.RCA_BookingVariance = 10;
			allocationRoute1.RCA_AllocationLineID = "0001";
			allocationRoute1.RCA_HasBookingLimit = true;

			var allocationRoute2 = carrierContract1.Allocations.AddNew();

			var carrierContract2 = Factory.New<RatingContract>();
			carrierContract2.RCT_ContractNumber = "FARQUAAD";
			carrierContract2.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract2.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract2.RCT_OH = carrier.PK;

			carrierContract2.Allocations.AddNew();

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = carrierContract1.RCT_ContractNumber;
			consol.JK_UniqueConsignRef = "WOOP WOOP";

			var voyageTransport = consol.Transports[0];
			voyageTransport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			voyageTransport.JW_ETD = ZDate.Today;
			voyageTransport.JW_RL_NKLoadPort = allocationRoute1.RCA_LoadLocation;
			voyageTransport.JW_RL_NKDiscPort = allocationRoute1.RCA_DischargeLocation;
			voyageTransport.JW_VoyageFlight = allocationRoute1.RCA_VoyageNumber;
			voyageTransport.JW_Vessel = allocationRoute1.RCA_RV_NKVessel;

			var container1 = consol.Containers.AddNew();
			container1.JC_ContainerNum = "DONKEY6969";
			container1.JC_RC = refContainer1.PK;
			container1.JC_ContainerCount = 7;

			var container2 = consol.Containers.AddNew();
			container2.JC_ContainerNum = "DRAGON420";
			container2.JC_RC = refContainer1.PK;
			container2.JC_ContainerCount = 4;

			container1.JC_RCA_AllocationLine = ZGuid.Empty;
			consol.JK_RCA_AllocationLine = allocationRoute1.PK;
			AssertConsolAllocationLineHasNoErrors(consol);

			container2.JC_RC = refContainer2.PK;
			AssertConsolAllocationLineHasError(consol, $"NOT all Containers on this Consol share the same Container Code ({allocationRoute1.ContainerType?.RC_Code}) of the selected Allocation Route {allocationRoute1.RCA_AllocationLineID}. Consol Container Codes should match the selected Allocation Route's Container Code.");

			allocationRoute1.RCA_RC_ContainerType = ZGuid.Empty;
			AssertConsolAllocationLineHasNoErrors(consol);

			allocationRoute1.RCA_StorageOrFreightRateClass = "XXX";
			AssertConsolAllocationLineHasNoErrors(consol);

			refContainer1.RC_FreightRateClass = "AAA";
			AssertConsolAllocationLineHasError(consol, $"NOT all Containers on this Consol share the same Container Class ({allocationRoute1.RCA_StorageOrFreightRateClass}) of the selected Allocation Route {allocationRoute1.RCA_AllocationLineID}. Container Class of Consol Containers should match the selected Allocation Route's Container Class.");

			allocationRoute1.RCA_StorageOrFreightRateClass = string.Empty;
			AssertConsolAllocationLineHasNoErrors(consol);

			allocationRoute1.RCA_StorageOrFreightRateClass = "YYY";
			refContainer1.RC_StorageClass = "XXX";
			AssertConsolAllocationLineHasError(consol, $"NOT all Containers on this Consol share the same Container Class ({allocationRoute1.RCA_StorageOrFreightRateClass}) of the selected Allocation Route {allocationRoute1.RCA_AllocationLineID}. Container Class of Consol Containers should match the selected Allocation Route's Container Class.");

			refContainer1.RC_FreightRateClass = "YYY";
			AssertConsolAllocationLineHasNoErrors(consol);

			allocationRoute1.RCA_LoadLocation = string.Empty;
			AssertConsolAllocationLineHasNoErrors(consol);

			allocationRoute1.RCA_DischargeLocation = string.Empty;
			AssertConsolAllocationLineHasNoErrors(consol);

			consol.JK_CarrierContractNumber = ZString.Empty;
			allocationRoute1.RCA_RCT_RatingContract = ZGuid.Empty;
			consol.JK_RCA_AllocationLine = allocationRoute1.PK;
			allocationRoute1.RCA_RCT_RatingContract = carrierContract1.PK;
			AssertConsolAllocationLineHasError(consol, "This Consol doesn't have any Carrier Contracts allocated. Allocation Route ID selected for the Consol and/or its Containers should be from the same Carrier Contract that this Consol is allocated to.");

			consol.JK_CarrierContractNumber = carrierContract1.RCT_ContractNumber;
			var nonExistentAllocationLineGuid = ZGuid.NewZGuid();
			consol.JK_RCA_AllocationLine = nonExistentAllocationLineGuid;
			AssertConsolAllocationLineHasError(consol, $"Allocation Route ID selected for the Consol and/or its Containers should be valid and from the same Carrier Contract ({carrierContract1.RCT_ContractNumber}) that this Consol is allocated to.");

			consol.JK_RCA_AllocationLine = allocationRoute1.PK;
			allocationRoute1.RCA_RCT_RatingContract = carrierContract2.PK;
			AssertConsolAllocationLineHasError(consol, $"Allocation Route ID selected for the Consol and/or its Containers should be from the same Carrier Contract ({carrierContract1.RCT_ContractNumber}) that this Consol is allocated to.");

			allocationRoute1.RCA_RCT_RatingContract = carrierContract1.PK;
			allocationRoute1.RCA_AllocatedQuantity = 5;
			var exceededAmount = -ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationRoute1);
			AssertConsolAllocationLineHasError(consol, $"Number of TEUs to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute1.RCA_AllocationLineID} by {exceededAmount} TEUs. Number of TEUs should be within the Booking Limit of the selected Allocation Route.");

			allocationRoute1.RCA_AllocatedUQ = Constants.AllocationQuantityUnits.Containers;
			exceededAmount = -ContractAllocationHelper.CalculateOutstandingUtilisation(Factory, allocationRoute1);
			AssertConsolAllocationLineHasError(consol, $"Number of Containers to be allocated exceeds available capacity of the selected Allocation Route {allocationRoute1.RCA_AllocationLineID} by {exceededAmount}. Number of Containers should be within the Booking Limit of the selected Allocation Route.");
		}

		public void TestJK_RCA_AllocationLine_LoadPort()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol first load matches", consol.JK_RCA_AllocationLineInfo, "The First Load Port or the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.");

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol first load does not match", consol.JK_RCA_AllocationLineInfo, "The First Load Port or the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.");

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has leg that matches load", consol.JK_RCA_AllocationLineInfo, "The First Load Port or the Load Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Load Port.");
		}

		public void TestJK_RCA_AllocationLine_LoadPortWithValidETD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today;

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();

			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period");

			transport.JW_ETD = ZDateTime.Today.AddDays(6);
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period");

			transport.JW_RL_NKLoadPort = "AUBNE";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period");

			transport.JW_ETD = ZDateTime.Today;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period");

			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period");

			transport.JW_RL_NKLoadPort = "AUSYD";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period");
		}

		public void TestJK_RCA_AllocationLine_DischargePort()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol final discharge matches", consol.JK_RCA_AllocationLineInfo, "The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.");

			consol.JK_RL_NKDischargePort = "AUBNE";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol final discharge does not match", consol.JK_RCA_AllocationLineInfo, "The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.");

			var transport = consol.Transports[0];
			transport.JW_RL_NKDiscPort = "NZAKL";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has leg that matches discharge", consol.JK_RCA_AllocationLineInfo, "The Last Discharge Port or the Discharge Port of one of the Consol Routing Leg(s) should match the selected Allocation Route's Discharge Port.");
		}

		public void TestJK_RCA_AllocationLine_DischargePortWithValidETD()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today;

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();

			const string etdDateRangeMessage = "None of the ETDs under the relevant Consol Routing Leg(s) fall within the validity period";

			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_ETD = ZDateTime.Today.AddDays(6);
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_RL_NKDiscPort = "NZWEL";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_ETD = ZDateTime.Today;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			consol.JK_RL_NKDischargePort = "NZWEL";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no valid leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);

			transport.JW_RL_NKDiscPort = "NZAKL";
			var precedingTransport = consol.Transports.AddNew();
			precedingTransport.JW_TransportMode = Constants.TransportModes.Sea;
			precedingTransport.JW_ETD = transport.JW_ETD.AddDays(-1);
			precedingTransport.JW_RL_NKLoadPort = "AUPER";
			precedingTransport.JW_RL_NKDiscPort = "NZWEL";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has valid disc leg that falls within valid ETD dates", consol.JK_RCA_AllocationLineInfo, etdDateRangeMessage);
		}

		public void TestJK_RCA_AllocationLine_TransportLegScheduleDetails()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "DORYA";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_ExpiryDate = ZDate.Today.AddDays(5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_VoyageNumber = "SORYA";
			allocationRoute.RCA_RV_NKVessel = "DUH";
			allocationRoute.RCA_ServiceLoop = "CD2";

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "DORYA";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_TransportMode = Core.Constants.TransportModes.Sea;
			transport.JW_ETD = ZDateTime.Today;
			transport.JW_VoyageFlight = "DF2";

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Schedule details are not fully matching", consol.JK_RCA_AllocationLineInfo, "that match the Voyage, Vessel, and Service String of Allocation Route");

			transport.JW_VoyageFlight = "SORYA";
			transport.JW_Vessel = "DUH";
			transport.JW_ServiceString = "CD2";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Schedule details are fully matching", consol.JK_RCA_AllocationLineInfo, "that match the Voyage, Vessel, and Service String of Allocation Route");
		}

		public void TestJK_RCA_AllocationLine_LinkedAllocation()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.New<RatingContract>();
			carrierContract.RCT_ContractNumber = "BLAHAJ";
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var refVessel = Factory.NewWithValidTestData<RefVessel>();

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "KRAMIG";
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_VoyageFlight = "JATTELIK";
			voyage.JV_RV_NKVessel = refVessel.RV_Code;
			voyage.JV_OH_Line = carrier.PK;

			var origin = voyage.Origins.AddNew();
			var destination = voyage.Destinations.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			destination.JB_RL_NKPortOfDischarge = "NZAKL";

			var schedule = Factory.New<JobSailing>();
			schedule.JX_JA = origin.PK;
			schedule.JX_JB = destination.PK;
			schedule.JX_ServiceString = "AFTONSPARV";
			allocationRoute.RCA_JX_SailingSchedule = schedule.PK;

			var today = ZDateTime.Today;
			origin.JA_E_DEP = today;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = "BLAHAJ";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			var transportLeg = consol.Transports[0];
			transportLeg.JW_IsLinked = false;
			transportLeg.JW_ETD = today;
			transportLeg.JW_RL_NKLoadPort = "AUSYD";
			transportLeg.JW_RL_NKDiscPort = "NZAKL";
			transportLeg.JW_VoyageFlight = "JATTELIK";
			transportLeg.JW_Vessel = refVessel.RV_Code;
			transportLeg.JW_ServiceString = "AFTONSPARV";
			transportLeg.JW_OA_CarrierAddress = carrier.MainAddress.PK;

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();

			AssertNoError("Transport leg should match schedule", consol.JK_RCA_AllocationLineInfo, $"Allocation Route KRAMIG is linked to Schedule {schedule.JX_UniqueReference}. However, this Consol has no legs matching Schedule details.");

			transportLeg.JW_ETD = today.AddDays(1);
			consol.Validation.ValidateJK_RCA_AllocationLine();

			AssertHasError("Transport leg details do not match", consol.JK_RCA_AllocationLineInfo, $"Allocation Route KRAMIG is linked to Schedule {schedule.JX_UniqueReference}. However, this Consol has no legs matching Schedule details.");
		}

		public void TestJK_RCA_AllocationLine_GatewayConsol()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "TIRAMISU";
			carrierContract.RCT_TransportMode = Core.Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "PUDDING";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";
			allocationRoute.RCA_AllowGatewayConsolOnly = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "TIRAMISU";
			consol.JK_SendingForwarderHandlingType = "YAH";
			consol.JK_ReceivingForwarderHandlingType = "AAH";

			const string errorMessage = "Allocation Route PUDDING is restricted to Gateway Consols only, but this Consol has no assigned Gateway Agent. Either the Sending or Receiving Agent of this Consol must be assigned as a Gateway Agent.";

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Consol has no Gateway Agent assigned.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_SendingForwarderHandlingType = "GTT";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has Gateway Agent assigned.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_SendingForwarderHandlingType = "YAH";
			consol.JK_ReceivingForwarderHandlingType = "GTA";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("Consol has Gateway Agent assigned.", consol.JK_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestJK_RCA_AllocationLine_GroupageContainerMode()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "AQ";
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "AQ";
			allocationRoute.RCA_AllowGroupageOnly = true;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_CarrierContractNumber = "AQ";
			consol.JK_ConsolMode = "FCL";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";

			const string errorMessage = "Allocation Route AQ is restricted to Consols with Groupage container mode. The Consol's current container mode is FCL.";

			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Error when Consol's container mode is not GRP.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_ConsolMode = "GRP";
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's container mode is GRP.", consol.JK_RCA_AllocationLineInfo, errorMessage);
		}
		
		public void TestValidateJK_RCA_AllocationLine_ValidationWhenInvalidDatesNoAllocationChanges()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(1);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			contract.Allocations.Add(allocationRoute);
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate.AddDays(2);
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(8);
			Factory.Save();
			transport.JW_ETD = contract.RCT_StartDate;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertHasErrors(consol.JK_RCA_AllocationLineInfo);
			}
		}

		public void TestValidateJK_RCA_AllocationLine_NoValidationWhenInvalidDatesAndMBLExists()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(1);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			contract.Allocations.Add(allocationRoute);
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate.AddDays(2);
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.JK_MasterBillNum = "A";

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(8);
			Factory.Save();
			transport.JW_ETD = contract.RCT_StartDate;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertNoErrors(consol.JK_RCA_AllocationLineInfo);
				AssertHasWarning(consol.JK_RCA_AllocationLineInfo,
					$"Consol's departure date ({transport.JW_ETD.ToShortDateString()}) is outside the Allocation Route {allocationRoute.RCA_AllocationLineID} period from {allocationRoute.StartDateWithContractFallback.ToShortDateString()} to {allocationRoute.ExpiryDateWithContractFallback.ToShortDateString()}.");
			}
		}

		public void TestValidateJK_RCA_AllocationLine_VoyageVesselServiceNotEmpty_NoValidationWhenInvalidDatesAndMBLExists()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(1);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			contract.Allocations.Add(allocationRoute);
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate.AddDays(2);
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;
			allocationRoute.RCA_VoyageNumber = "123";
			allocationRoute.RCA_RV_NKVessel = "456";
			allocationRoute.RCA_ServiceLoop = "789";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.JK_MasterBillNum = "A";

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(8);
			transport.JW_VoyageFlight = "123";
			transport.JW_Vessel = "456";
			transport.JW_ServiceString = "789";
			Factory.Save();
			transport.JW_ETD = contract.RCT_StartDate;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertNoErrors(consol.JK_RCA_AllocationLineInfo);
				AssertHasWarning(consol.JK_RCA_AllocationLineInfo,
					$"Consol's departure date ({transport.JW_ETD.ToShortDateString()}) is outside the Allocation Route {allocationRoute.RCA_AllocationLineID} period from {allocationRoute.StartDateWithContractFallback.ToShortDateString()} to {allocationRoute.ExpiryDateWithContractFallback.ToShortDateString()}.");
			}
		}

		public void TestValidateJK_RCA_AllocationLine_NoValidationWhenInvalidDatesAndBookingExists()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var contract = Factory.New<RatingContract>();
			contract.RCT_ContractNumber = "TEST123";
			contract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			contract.RCT_OH = carrier.PK;
			contract.RCT_StartDate = ZDate.Today.AddDays(1);
			contract.RCT_EndDate = ZDate.Today.AddDays(10);
			contract.RCT_GS_NKContractOwner = "FOW";

			var allocationRoute = Factory.NewWithValidTestData<RatingContractAllocationLine>();
			contract.Allocations.Add(allocationRoute);
			allocationRoute.RCA_AllocatedQuantity = 1;
			allocationRoute.RCA_StartDate = contract.RCT_StartDate.AddDays(2);
			allocationRoute.RCA_ExpiryDate = contract.RCT_EndDate;

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_CarrierContractNumber = contract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.JK_BookingReference = "A";

			var transport = consol.Transports[0];
			transport.JW_ETD = ZDate.Today.AddDays(8);
			Factory.Save();
			transport.JW_ETD = contract.RCT_StartDate;
			using (FreightDataRegistry.Instance.EnableCarrierAndClientContractModules.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertNoErrors(consol.JK_RCA_AllocationLineInfo);
				AssertHasWarnings(consol.JK_RCA_AllocationLineInfo);
			}
		}

		public void TestJK_RCA_AllocationLine_Agents()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "CHICKENJOCKEY";
			carrierContract.RCT_TransportMode = Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "CHICKEN";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_AllocatedQuantity = 5;
			allocationRoute.RCA_AllocatedUQ = "TU";
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var agent = Factory.NewWithValidTestData<OrgHeader>();
			agent.OH_Code = "DENNIS";
			var randomOrg1 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg1.OH_Code = "STEVE";
			var randomOrg2 = Factory.NewWithValidTestData<OrgHeader>();
			randomOrg2.OH_Code = "GARRETT";
			allocationRoute.AgentPivots.AddRelatedIfNotExist(agent);

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = randomOrg2.MainAddress.PK;

			const string errorMessage = "Neither Sending Agent STEVE nor Receiving Agent GARRETT of the Consol matches with the Agents specified on Allocation Route CHICKEN under Carrier Contract CHICKENJOCKEY.";

			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasErrorContaining("Error when Consol's Sending/Receiving Agent does not contain Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = randomOrg1.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = agent.MainAddress.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branchProxy = Factory.NewWithValidTestData<OrgHeader>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.GC_OH_OrgProxy = agent.PK;
			branch.GB_GC = company.PK;
			branch.GB_OH_OrgProxy = branchProxy.PK;

			Factory.Save();

			consol.JK_OA_ReceivingForwarderAddress = branchProxy.MainAddress.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);

			consol.JK_OA_SendingForwarderAddress = agent.MainAddress.PK;
			consol.JK_OA_ReceivingForwarderAddress = branchProxy.MainAddress.PK;
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrorContaining("No errors when Consol's Sending/Receiving Agent contains Allocation Route's Agent.", consol.JK_RCA_AllocationLineInfo, errorMessage);
		}

		public void TestJK_RCA_AllocationLine_PlaceOfReceiptDelivery()
		{
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var carrierContract = Factory.NewWithValidTestData<RatingContract>();
			carrierContract.RCT_ContractNumber = "CHICKENJOCKEY";
			carrierContract.RCT_TransportMode = Constants.TransportModes.Sea;
			carrierContract.RCT_ContractType = Constants.RatingContractTypes.Provider;
			carrierContract.RCT_OH = carrier.PK;

			var allocationRoute = carrierContract.Allocations.AddNew();
			allocationRoute.RCA_AllocationLineID = "CHICKEN";
			allocationRoute.RCA_StartDate = ZDate.Today.AddDays(-5);
			allocationRoute.RCA_AllocatedQuantity = 5;
			allocationRoute.RCA_AllocatedUQ = "TU";
			allocationRoute.RCA_LoadLocation = "AUSYD";
			allocationRoute.RCA_DischargeLocation = "NZAKL";

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_CarrierContractNumber = carrierContract.RCT_ContractNumber;
			consol.JK_OA_ShippingLineAddress = carrier.MainAddress.PK;
			consol.JK_RCA_AllocationLine = allocationRoute.PK;
			consol.JK_RL_NKLoadPort = "AUMEL";
			consol.JK_RL_NKDischargePort = "HKHKG";
			consol.JK_UniqueConsignRef = "S00001";

			var expectedReceiptMessage = "Mismatch: ‘Place of receipt’ (AUSYD) must match ‘1st Load’ (AUMEL) in ‘Allocated consol’ (S00001).";
			var expectedDeliveryMessage = "Mismatch: ‘Place of delivery’ (CNSHA) must match ‘Last  discharge’ (HKHKG) in ‘Allocated consol’ (S00001).";

			using (FreightConfigurationRegistry.Instance.EnablePlaceOfReceiptAndDeliverySupportOnAllocationRoutes.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				allocationRoute.RCA_PlaceOfReceipt = "AUSYD";
				allocationRoute.RCA_PlaceOfDelivery = "CNSHA";
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertHasErrorContaining(consol.JK_RCA_AllocationLineInfo, expectedReceiptMessage);

				allocationRoute.RCA_PlaceOfReceipt = "AUMEL";
				allocationRoute.RCA_PlaceOfDelivery = "CNSHA";
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertHasErrorContaining(consol.JK_RCA_AllocationLineInfo, expectedDeliveryMessage);

				allocationRoute.RCA_PlaceOfReceipt = ZString.Empty;
				allocationRoute.RCA_PlaceOfDelivery = ZString.Empty;
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertNoErrorContaining(consol.JK_RCA_AllocationLineInfo, expectedReceiptMessage);
				AssertNoErrorContaining(consol.JK_RCA_AllocationLineInfo, expectedDeliveryMessage);

				allocationRoute.RCA_PlaceOfReceipt = "AUMEL";
				allocationRoute.RCA_PlaceOfDelivery = "HKHKG";
				consol.Validation.ValidateJK_RCA_AllocationLine();
				AssertNoErrorContaining(consol.JK_RCA_AllocationLineInfo, expectedReceiptMessage);
				AssertNoErrorContaining(consol.JK_RCA_AllocationLineInfo, expectedDeliveryMessage);
			}
		}

		void AssertConsolAllocationLineHasError(ForwardingConsol consol, string expectedMessage)
		{
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertHasError(consol.JK_RCA_AllocationLineInfo, expectedMessage);
		}

		void AssertConsolAllocationLineHasNoErrors(ForwardingConsol consol)
		{
			consol.Validation.ValidateJK_RCA_AllocationLine();
			AssertNoErrors(consol.JK_RCA_AllocationLineInfo);
		}

		#endregion

		#region Verification / Pre-Allocation Details

		public void TestVerificationWeightVolumeUnitValidation()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertNoErrors(consol.WeightVerificationUnitInfo);
			AssertNoErrors(consol.VolumeVerificationUnitInfo);

			consol.JK_TotalShipmentActWeightCheck = 300m;
			AssertHasErrors(consol.WeightVerificationUnitInfo);
			AssertNoErrors(consol.VolumeVerificationUnitInfo);

			consol.JK_TotalShipmentActVolumeCheck = 2m;
			AssertHasErrors(consol.WeightVerificationUnitInfo);
			AssertHasErrors(consol.VolumeVerificationUnitInfo);

			consol.WeightVerificationUnit = "KG";
			consol.VolumeVerificationUnit = "M3";
			AssertNoErrors(consol.WeightVerificationUnitInfo);
			AssertNoErrors(consol.VolumeVerificationUnitInfo);

			consol.WeightVerificationUnit = "XX";
			consol.VolumeVerificationUnit = "ZZ";
			AssertHasErrors(consol.WeightVerificationUnitInfo);
			AssertHasErrors(consol.VolumeVerificationUnitInfo);
		}

		public void TestTotalWeightCheck()
		{
			AssertPreAllocationWarnings(JobConsolSchema.JK_TotalShipmentActWeightCheck.Name, JobShipmentSchema.JS_ActualWeight.Name, PreAllocationCheck.Measures.Weight);
		}

		public void TestTotalVolumeCheck()
		{
			AssertPreAllocationWarnings(JobConsolSchema.JK_TotalShipmentActVolumeCheck.Name, JobShipmentSchema.JS_ActualVolume.Name, PreAllocationCheck.Measures.Volume);
		}

		public void TestTotalChargeableCheck()
		{
			AssertPreAllocationWarnings(JobConsolSchema.JK_TotalShipmentChargableCheck.Name, JobShipmentSchema.JS_ActualChargeable.Name, PreAllocationCheck.Measures.Chargeable);
		}

		void AssertPreAllocationWarnings(string consolPreAllocationPropertyName, string shipmentMeasurePropertyName, string registryMeasureName)
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ZPropertyInfo<ZDecimal> consolPreAllocationInfo = (ZPropertyInfo<ZDecimal>)consol.FindPropertyInfo(consolPreAllocationPropertyName);
			AssertNoWarnings("Precondition: no warnings", consolPreAllocationInfo);

			ForwardingShipment shipment = consol.Shipments.AddNew();
			ZPropertyInfo<ZDecimal> shipmentMeasureInfo = (ZPropertyInfo<ZDecimal>)shipment.FindPropertyInfo(shipmentMeasurePropertyName);
			shipmentMeasureInfo.Value = 70m;

			consolPreAllocationInfo.Value = 100m;
			AssertNoWarnings("Precondition: no warnings as registry setup is 'None'", consolPreAllocationInfo);

			PreAllocationCheckCollection preAllocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			PreAllocationCheck check = preAllocationChecks.Cast<PreAllocationCheck>().FirstOrDefault(x => x.Measure == registryMeasureName);
			check.Action = PreAllocationCheck.Actions.Warning;
			check.Percentage = 50m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, preAllocationChecks);

			consolPreAllocationInfo.Value = 101m;
			AssertHasWarnings("Has warning: registry setup is 'Warning' and pre-allocation percentage is exceeded", consolPreAllocationInfo);

			consolPreAllocationInfo.Value = 200m;
			AssertNoWarnings("No warnings as pre-allocation percentage not exceeded", consolPreAllocationInfo);

			preAllocationChecks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			check = preAllocationChecks.Cast<PreAllocationCheck>().FirstOrDefault(x => x.Measure == registryMeasureName);
			check.Action = PreAllocationCheck.Actions.Restriction;
			check.Percentage = 50m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, preAllocationChecks);

			consolPreAllocationInfo.Value = 102m;
			AssertHasWarnings("Has warning: registry setup is 'Restriction' and pre-allocation percentage is exceeded", consolPreAllocationInfo);

			consolPreAllocationInfo.Value = 200m;
			AssertNoWarnings("No warnings as pre-allocation percentage not exceeded", consolPreAllocationInfo);
		}

		public void TestTotalShipmentCountCheck()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			AssertNoWarnings("Precondition: no warnings", consol.JK_TotalShipmentCountCheckInfo);

			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			consol.Shipments.AddNew();
			AssertEquals("Precondition: has 3 shipments", 3, consol.Shipments.Count);

			consol.JK_TotalShipmentCountCheck = 5;
			AssertNoWarnings("Precondition: no warnings as registry setup is 'None'", consol.JK_TotalShipmentCountCheckInfo);

			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.ShipmentCount.Action = PreAllocationCheck.Actions.Warning;
			checks.ShipmentCount.Percentage = 50m;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			consol.JK_TotalShipmentCountCheck = 4;
			AssertHasWarnings("Has warning: registry setup is 'Warning' and pre-allocation percentage is exceeded", consol.JK_TotalShipmentCountCheckInfo);

			checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			checks.ShipmentCount.Action = PreAllocationCheck.Actions.Restriction;
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			consol.JK_TotalShipmentCountCheck = 5;
			AssertHasWarnings("Has warning: registry setup is 'Restriction' and pre-allocation percentage is exceeded", consol.JK_TotalShipmentCountCheckInfo);

			consol.JK_TotalShipmentCountCheck = 10;
			AssertNoWarnings("No warnings as pre-allocation percentage not exceeded", consol.JK_TotalShipmentCountCheckInfo);
		}

		public void TestValidatePreAllocationValues()
		{
			PreAllocationCheckCollection checks = ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.Value;
			foreach (PreAllocationCheck check in checks)
			{
				if (check.Measure != PreAllocationCheck.Measures.Dimensions)
				{
					check.Action = PreAllocationCheck.Actions.Warning;
					check.Percentage = 50m;
				}
			}
			ForwardingConfigurationRegistry.Instance.ConsolPreAllocationCheck.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, checks);

			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TotalShipmentActWeightCheck = 100m;
			consol.JK_TotalShipmentActVolumeCheck = 10m;
			consol.JK_TotalShipmentChargableCheck = 100m;
			consol.JK_TotalShipmentCountCheck = 1;

			ForwardingShipment shipment = consol.Shipments.AddNew();
			shipment.JS_ActualWeight = 80m;
			shipment.JS_ActualVolume = 8m;
			shipment.JS_ActualChargeable = 80m;

			consol.Validation.ValidatePreAllocationValues();

			AssertHasWarnings(consol.JK_TotalShipmentActWeightCheckInfo);
			AssertHasWarnings(consol.JK_TotalShipmentActVolumeCheckInfo);
			AssertHasWarnings(consol.JK_TotalShipmentChargableCheckInfo);
			AssertHasWarnings(consol.JK_TotalShipmentCountCheckInfo);
		}

		#region Is Hazardous Flags

		public void TestIsHazardousFlagNotSet()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsHazardous = false;
				AssertEquals("Precondition: JK_IsHazardous is false", false, consol.JK_IsHazardous);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var restriction = consol.ConsolDGRestrictionCollection.AddNew();
				restriction.JKD_Class = "1";
				consol.JK_IsHazardous = false;
				consol.Validation.ValidateAll();
				AssertHasError("Expected warning.", consol.JK_IsHazardousInfo, "Is Hazardous Flag should be set when the Consol accepts DG Classes or DG Substances.");

				consol.ConsolDGRestrictionCollection.DeleteAll();
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S1";
				var packline = shipment.OuterPackLines.AddNew();
				var dangerousGood = packline.UNDGs.AddNew();
				dangerousGood.DI_IMOClass = "Cls1";

				consol.Validation.ValidateAll();
				AssertHasError("Expected warning.", consol.JK_IsHazardousInfo, @"Is Hazardous Flag should be set when there are shipments with dangerous goods attached.
The following Shipment(s) attached to this consol have dangerous cargo: Shipment S1");
			}
		}

		public void TestJK_IsHazardousShouldNotBeFalse_WhenConsolIsAttachedToHazardousShipment()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var shipment = Factory.New<ForwardingShipment>();
				shipment.JS_TransportMode = Constants.TransportModes.Air;
				shipment.JS_UniqueConsignRef = "S1";
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_RH_NKCommodityCode = "HAZ";

				var consol = shipment.Consols.AddNew();
				consol.JK_TransportMode = Constants.TransportModes.Air;
				consol.Validation.ValidateAll();
				AssertHasError("Expected warning.", consol.JK_IsHazardousInfo, @"Is Hazardous Flag should be set when there are shipments with dangerous goods attached.
The following Shipment(s) attached to this consol have dangerous cargo: Shipment S1");

				consol.JK_IsHazardous = true;
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var shipment2 = Factory.New<ForwardingShipment>();
				shipment2.JS_TransportMode = Constants.TransportModes.Air;
				shipment2.JS_UniqueConsignRef = "S2";
				var packline2 = shipment2.OuterPackLines.AddNew();

				var hazardousCommodity = Factory.New<RefCommodityCode>();
				hazardousCommodity.RH_Code = "DED";
				hazardousCommodity.RH_IsHazardous = true;
				packline2.JL_RH_NKCommodityCode = hazardousCommodity.RH_Code;

				var consol2 = shipment2.Consols.AddNew();
				consol2.JK_TransportMode = Constants.TransportModes.Air;
				consol2.Validation.ValidateAll();
				AssertHasError("Expected warning.", consol2.JK_IsHazardousInfo, @"Is Hazardous Flag should be set when there are shipments with dangerous goods attached.
The following Shipment(s) attached to this consol have dangerous cargo: Shipment S2");

				consol2.JK_IsHazardous = true;
				consol2.Validation.ValidateAll();
				AssertNoErrors(consol2.JK_IsHazardousInfo);
			}
		}

		public void TestIsHazardousFlagSetOnly()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsHazardous = true;
				AssertEquals("Precondition: JK_IsHazardous is true", true, consol.JK_IsHazardous);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var shipment1 = consol.Shipments.AddNew();
				shipment1.JS_UniqueConsignRef = "S1";
				var packline1 = shipment1.OuterPackLines.AddNew();
				var dangerousGood1 = packline1.UNDGs.AddNew();
				dangerousGood1.DI_IMOClass = "Cls1";
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "S2";
				var packline2 = shipment2.OuterPackLines.AddNew();
				var dangerousGood2 = packline2.UNDGs.AddNew();
				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "Sub";
				subs.DG_Variant = "1";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				dangerousGood2.LinkDefault(subs);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);
			}
		}

		public void TestIsHazardousFlagSetWithSubstances()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsHazardous = true;
				AssertEquals("Precondition: JK_IsHazardous is true", true, consol.JK_IsHazardous);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var restriction = consol.ConsolDGRestrictionCollection.AddNew();
				restriction.JKD_Calc_Substance = "sub1";
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "sub";
				subs.DG_Variant = "2";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S1";
				var packline = shipment.OuterPackLines.AddNew();
				packline.JL_RH_NKCommodityCode = "HAZ";
				var dangerousGood = packline.UNDGs.AddNew();
				dangerousGood.DI_DG = subs.PK;
				dangerousGood.LinkDefault(subs);
				consol.Validation.ValidateAll();
				AssertHasError("Expected warning.", consol.JK_IsHazardousInfo, "The following Shipment(s) attached to this consol have dangerous cargo that is not accepted: Shipment S1");

				var restriction2 = consol.ConsolDGRestrictionCollection.AddNew();
				restriction2.JKD_Calc_Substance = "sub2";
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);
			}
		}

		public void TestIsHazardousFlagSetWithClasses()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsHazardous = true;
				AssertEquals("Precondition: JK_IsHazardous is true", true, consol.JK_IsHazardous);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var restriction = consol.ConsolDGRestrictionCollection.AddNew();
				restriction.JKD_Class = "Cls1";
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S1";
				var packline = shipment.OuterPackLines.AddNew();
				var dangerousGood = packline.UNDGs.AddNew();
				dangerousGood.DI_IMOClass = "Cls2";
				consol.Validation.ValidateAll();
				AssertHasError("Expected warning.", consol.JK_IsHazardousInfo, "The following Shipment(s) attached to this consol have dangerous cargo that is not accepted: Shipment S1");

				var restriction2 = consol.ConsolDGRestrictionCollection.AddNew();
				restriction2.JKD_Class = "Cls2";
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);
			}
		}

		public void TestIsHazardousFlagSetWithBothClassesAndSubstances()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsHazardous = true;
				AssertEquals("Precondition: JK_IsHazardous is true", true, consol.JK_IsHazardous);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var restriction = consol.ConsolDGRestrictionCollection.AddNew();
				restriction.JKD_Class = "Cls1";
				restriction.JKD_Calc_Substance = "Sub1";
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "Sub";
				subs.DG_Variant = "1";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S1";
				var packline = shipment.OuterPackLines.AddNew();
				var dangerousGood = packline.UNDGs.AddNew();
				dangerousGood.DI_IMOClass = "Cls1";
				dangerousGood.DI_DG = subs.PK;
				dangerousGood.LinkDefault(subs);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var packline2 = shipment.OuterPackLines.AddNew();
				var dangerousGood2 = packline2.UNDGs.AddNew();
				dangerousGood2.DI_DG = subs.PK;
				dangerousGood2.LinkDefault(subs);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var packline3 = shipment.OuterPackLines.AddNew();
				packline3.JL_RH_NKCommodityCode = "HAZ";
				var dangerousGood3 = packline3.UNDGs.AddNew();
				var subs2 = Factory.New<UNDGSubstance>();
				subs2.DG_UNNO = "Sub";
				subs2.DG_Variant = "2";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				dangerousGood3.DI_DG = subs2.PK;
				dangerousGood3.LinkDefault(subs2);
				consol.Validation.ValidateAll();
				AssertHasError("Expected Error.", consol.JK_IsHazardousInfo, "The following Shipment(s) attached to this consol have dangerous cargo that is not accepted: Shipment S1");
			}
		}

		public void TestIsHazardousFlagMultipleShipments()
		{
			using (FreightConfigurationRegistry.Instance.IsConsolDangerousGoodsValidationEnabled.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_IsHazardous = true;
				AssertEquals("Precondition: JK_IsHazardous is true", true, consol.JK_IsHazardous);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var restriction = consol.ConsolDGRestrictionCollection.AddNew();
				restriction.JKD_Class = "Cls1";
				restriction.JKD_Calc_Substance = "Sub1";
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var subs = Factory.New<UNDGSubstance>();
				subs.DG_UNNO = "Sub";
				subs.DG_Variant = "1";
				subs.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				var shipment = consol.Shipments.AddNew();
				shipment.JS_UniqueConsignRef = "S1";
				var packline = shipment.OuterPackLines.AddNew();
				var dangerousGood = packline.UNDGs.AddNew();
				dangerousGood.DI_IMOClass = "Cls1";
				dangerousGood.DI_DG = subs.PK;
				dangerousGood.LinkDefault(subs);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var shipment2 = consol.Shipments.AddNew();
				shipment2.JS_UniqueConsignRef = "S2";
				var packline2 = shipment2.OuterPackLines.AddNew();
				var dangerousGood2 = packline2.UNDGs.AddNew();
				dangerousGood2.DI_DG = subs.PK;
				dangerousGood2.LinkDefault(subs);
				consol.Validation.ValidateAll();
				AssertNoErrors(consol.JK_IsHazardousInfo);

				var shipment3 = consol.Shipments.AddNew();
				shipment3.JS_UniqueConsignRef = "S3";
				var packline3 = shipment3.OuterPackLines.AddNew();
				packline3.JL_RH_NKCommodityCode = "HAZ";
				var dangerousGood3 = packline3.UNDGs.AddNew();
				var subs2 = Factory.New<UNDGSubstance>();
				subs2.DG_UNNO = "Sub";
				subs2.DG_Variant = "2";
				subs2.DG_Standard = UNDGSubstanceLookups.UNDGSubstanceStandardTypes.IMO;
				dangerousGood3.DI_DG = subs2.PK;
				dangerousGood3.LinkDefault(subs2);
				consol.Validation.ValidateAll();
				AssertHasError("Expected warning.", consol.JK_IsHazardousInfo, "The following Shipment(s) attached to this consol have dangerous cargo that is not accepted: Shipment S3");
			}
		}

		#endregion

		#endregion

		#region CutOffDate

		public void TestConsolCutOffDateLocal_ShipmentsAttachedOrDetached()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();

			consol.Validation.ValidateJK_ConsolCutOffDate();
			AssertEquals(false, consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());

			ForwardingShipment shipment1 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment2 = Factory.New<ForwardingShipment>();
			ForwardingShipment shipment3 = Factory.New<ForwardingShipment>();
			shipment1.JS_UniqueConsignRef = "SHIP1";

			consol.ShipmentsAttachedThisSession.Add(shipment1.PK, ZDateTime.UtcNow);
			consol.ShipmentsAttachedThisSession.Add(shipment2.PK, ZDateTime.UtcNow);
			consol.ShipmentsAttachedThisSession.Add(ZGuid.NewZGuid(), ZDateTime.UtcNow);

			consol.ShipmentsDetachedThisSession.Add(shipment3.PK, ZDateTime.UtcNow);
			consol.ShipmentsDetachedThisSession.Add(ZGuid.NewZGuid(), ZDateTime.UtcNow);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
			consol.Validation.ValidateJK_ConsolCutOffDate();
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasErrors());

			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddDays(-1);
			consol.Validation.ValidateJK_ConsolCutOffDate();
			AssertHasWarningContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Shipment(s) were removed from the Consol after the Cut Off Date:\r\nNew Shipment\r\n");
			AssertHasWarningContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Shipment(s) were added to the Consol after the Cut Off Date:\r\nSHIP1\r\n");
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasErrors());

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
			consol.Validation.ValidateJK_ConsolCutOffDate();
			AssertHasErrorContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Shipment(s) were removed from the Consol after the Cut Off Date:\r\nNew Shipment\r\n");
			AssertHasErrorContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Shipment(s) were added to the Consol after the Cut Off Date:\r\nSHIP1\r\n");
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());

			consol.ShipmentsAttachedThisSession.Clear();
			consol.ShipmentsDetachedThisSession.Clear();

			consol.Validation.ValidateJK_ConsolCutOffDate();
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());
		}

		public void TestConsolCutOffDateLocal_ConsolsAttachedOrDetached()
		{
			var consol = Factory.New<ForwardingConsol>();

			consol.Validation.ValidateJK_ConsolCutOffDate();
			AssertEquals(false, consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());

			var consol1 = Factory.New<ForwardingConsol>();
			var consol2 = Factory.New<ForwardingConsol>();
			var consol3 = Factory.New<ForwardingConsol>();
			consol1.JK_UniqueConsignRef = "CONSOL1";

			consol.ConsolsAttachedThisSession.Add(consol1.PK, ZDateTime.UtcNow);
			consol.ConsolsAttachedThisSession.Add(consol2.PK, ZDateTime.UtcNow);
			consol.ConsolsAttachedThisSession.Add(ZGuid.NewZGuid(), ZDateTime.UtcNow);

			consol.ConsolsDetachedThisSession.Add(consol3.PK, ZDateTime.UtcNow);
			consol.ConsolsDetachedThisSession.Add(ZGuid.NewZGuid(), ZDateTime.UtcNow);

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = true;
			consol.Validation.ValidateJK_ConsolCutOffDate();
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasErrors());

			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddDays(-1);
			consol.Validation.ValidateJK_ConsolCutOffDate();
			AssertHasWarningContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Consol(s) were removed from the Consol after the Cut Off Date:\r\nNew Consol\r\n");
			AssertHasWarningContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Consol(s) were added to the Consol after the Cut Off Date:\r\nCONSOL1\r\nNew Consol\r\n");
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasErrors());

			Env.Security.ConsolAttachDetachShipmentAfterCutOffDate.IsAllowed = false;
			consol.Validation.ValidateJK_ConsolCutOffDate();
			AssertHasErrorContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Consol(s) were removed from the Consol after the Cut Off Date:\r\nNew Consol\r\n");
			AssertHasErrorContaining(consol.JK_ConsolCutOffDateLocalInfo, "The following Consol(s) were added to the Consol after the Cut Off Date:\r\nCONSOL1\r\nNew Consol\r\n");
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());

			consol.ConsolsAttachedThisSession.Clear();
			consol.ConsolsDetachedThisSession.Clear();

			consol.Validation.ValidateJK_ConsolCutOffDate();
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasWarnings());
			Assert(!consol.JK_ConsolCutOffDateLocalInfo.HasErrors());
		}

		public void TestShipmentWeightOrVolumeChangedAfterConsolCutOffDate()
		{
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var shipment = consol.Shipments.AddNew();

			shipment.JS_UnitOfWeight = "KT";
			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddMinutes(-1);

			var expectedWarning = "You have modified Shipment's weight or volume after the Consol Cut Off Date.";
			var expectedError = @"You have modified Shipment's weight or volume after the Consol Cut Off Date. Supervisor access is required to save the changes at this time.

You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Operate -> Forwarding -> Consolidations -> Allow changing Shipments Weight / Volume after Consol Cut Off Date.";

			AssertNoWarning("No warning if consol is not saved", consol.JK_ConsolCutOffDateInfo, expectedWarning);

			Factory.Save();

			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddMinutes(-2);

			AssertNoWarning("Modifications made before the consol was first saved do not trigger a warning", consol.JK_ConsolCutOffDateInfo, expectedWarning);

			shipment.JS_UnitOfVolume = "CC";
			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddMinutes(1);

			AssertNoWarning("Modification was made before cut off date", consol.JK_ConsolCutOffDateInfo, expectedWarning);

			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddMinutes(-1);

			AssertHasWarning(consol.JK_ConsolCutOffDateInfo, expectedWarning);

			Env.Security.ConsolChangeWeightOrVolumeAfterCutOffDate.IsAllowed = false;

			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddMinutes(-2);

			AssertHasError(consol.JK_ConsolCutOffDateInfo, expectedError);

			consol.JK_ConsolCutOffDate = ZDateTime.UtcNow.AddMinutes(1);

			AssertNoError(consol.JK_ConsolCutOffDateInfo, expectedError);
		}

		public void TestValidateJK_SendingForwarderHandlingType_SecurityRights_NewConsol_SettingValue()
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_SendingForwarderHandlingType();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				AssertHasError(consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_SendingForwarderHandlingType();

				AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				AssertNoError(consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_SecurityRights_NewConsol_SystemDefault_UnsettingDefault()
		{
			var forwarder = GetForwarder();
			var address = AddForwarderAgentAddress(forwarder, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.GatewayAgent);
			AddForwarderAgentAddress(forwarder, address, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.Published);

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.GB_OH_OrgProxy = address.Header.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_SendingForwarderHandlingType();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				AssertEquals("JK_SendingForwarderHandlingType has been set", "GTA", consol.JK_SendingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_SendingForwarderHandlingType = ZString.Empty;

				AssertHasError("When user overrides default: error", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_SendingForwarderHandlingType();

				AssertEquals("JK_SendingForwarderHandlingType has been set", "GTA", consol.JK_SendingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_SendingForwarderHandlingType = ZString.Empty;

				AssertNoError("Security rights granted: no error", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_SecurityRights_NewConsol_SystemDefault_SetDifferentValue()
		{
			var forwarder = GetForwarder();
			var address = AddForwarderAgentAddress(forwarder, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.GatewayAgent);
			AddForwarderAgentAddress(forwarder, address, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.Published);

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GES"));
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.GB_OH_OrgProxy = address.Header.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_SendingForwarderHandlingType();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				AssertEquals("JK_SendingForwarderHandlingType has been set", "GTA", consol.JK_SendingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertHasError("When user overrides default: error", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_SendingForwarderHandlingType();

				AssertEquals("JK_SendingForwarderHandlingType has been set", "GTA", consol.JK_SendingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertNoError("Security rights granted: no error", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_SecurityRights_ExistingConsol_UnsettingValue()
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				Factory.Save();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				var loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_SendingForwarderHandlingType = ZString.Empty;
				AssertHasError(loadedConsol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_SendingForwarderHandlingType = ZString.Empty;
				AssertNoError(loadedConsol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_SecurityRights_ExistingConsol_SettingValue()
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				Factory.Save();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				var loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertHasError(loadedConsol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertNoError(loadedConsol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_SecurityRights_NewConsol_SettingValue()
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_ReceivingForwarderHandlingType();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				AssertHasError(consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_ReceivingForwarderHandlingType();

				AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

				AssertNoError(consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_SecurityRights_NewConsol_SystemDefault_UnsettingDefault()
		{
			var forwarder = GetForwarder();
			var address = AddForwarderAgentAddress(forwarder, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.GatewayAgent);
			AddForwarderAgentAddress(forwarder, address, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.Published);

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GIS"));
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.GB_OH_OrgProxy = address.Header.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_ReceivingForwarderHandlingType();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				AssertEquals("JK_ReceivingForwarderHandlingType has been set", "GTA", consol.JK_ReceivingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_ReceivingForwarderHandlingType = ZString.Empty;

				AssertHasError("When user overrides default: error", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_ReceivingForwarderHandlingType();

				AssertEquals("JK_ReceivingForwarderHandlingType has been set", "GTA", consol.JK_ReceivingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_ReceivingForwarderHandlingType = ZString.Empty;

				AssertNoError("Security rights granted: no error", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_SecurityRights_NewConsol_SystemDefault_SetDifferentValue()
		{
			var forwarder = GetForwarder();
			var address = AddForwarderAgentAddress(forwarder, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.GatewayAgent);
			AddForwarderAgentAddress(forwarder, address, "AUSYD", Constants.TransportModes.Sea, AgentDirectionList.Codes.Both, AgentStatusList.Codes.Published);

			var department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "GIS"));
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_RL_NKHomePort = "AUSYD";
			branch.GB_OH_OrgProxy = address.Header.PK;

			Factory.Save();

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branch.PK.ToGuid(), department.PK.ToGuid()))
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_ReceivingForwarderHandlingType();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				AssertEquals("JK_ReceivingForwarderHandlingType has been set", "GTA", consol.JK_ReceivingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertHasError("When user overrides default: error", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				consol = Factory.New<ForwardingConsol>();
				consol.Validation.ValidateJK_SendingForwarderHandlingType();

				AssertEquals("JK_ReceivingForwarderHandlingType has been set", "GTA", consol.JK_ReceivingForwarderHandlingType);
				AssertNoError("Precondition: No error: system has defaulted value", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				AssertNoError("Security rights granted: no error", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_SecurityRights_ExistingConsol_UnsettingValue()
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				Factory.Save();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				var loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertHasError(loadedConsol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertNoError(loadedConsol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_SecurityRights_ExistingConsol_SettingValue()
		{
			using (ForwardingConfigurationRegistry.Instance.ConsolTypeDefaultForConsol.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, Constants.AgentType.Agent))
			{
				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

				var consol = Factory.New<ForwardingConsol>();
				Factory.Save();

				string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

				var loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertHasError(loadedConsol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

				Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;

				loadedConsol = new BusinessObjectFactory().Load<ForwardingConsol>(consol.PK);
				loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;
				AssertNoError(loadedConsol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);
			}
		}

		public void TestValidateJK_SendingForwarderHandlingType_SecurityRights_DoesNotThrowErrorWhenLoadedAndUnchanged()
		{
			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "GBSUN";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

			AssertHasError("Security rights for adding/removing gateway handling type granted", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

			var otherFactory = new BusinessObjectFactory();
			var consolLoaded = otherFactory.Load<ForwardingConsol>(consol.PK);
			consolLoaded.Validation.ValidateJK_SendingForwarderHandlingType();

			AssertNoError("Should not throw error when value is unchanged", consolLoaded.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_SecurityRights_DoesNotThrowErrorWhenLoadedAndUnchanged()
		{
			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();

			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_UniqueConsignRef = "C00001111";
			consol.JK_RL_NKLoadPort = "GBSUN";
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_OA_ReceivingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

			AssertHasError("Security rights for adding/removing gateway handling type granted", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;

			var otherFactory = new BusinessObjectFactory();
			var consolLoaded = otherFactory.Load<ForwardingConsol>(consol.PK);
			consolLoaded.Validation.ValidateJK_ReceivingForwarderHandlingType();

			AssertNoError("Should not throw error when value is unchanged", consolLoaded.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);
		}

		public void TestValidateJK_ReceivingForwarderHandlingType_SecurityRights()
		{
			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

			AssertNoError("Security rights for adding/removing gateway handling type granted", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);

			Factory.Save();

			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;
			consol.JK_ReceivingForwarderHandlingType = ZString.Empty;

			AssertHasError("Security rights for adding/removing gateway handling type not granted", consol.JK_ReceivingForwarderHandlingTypeInfo, securityErrorMessage);
		}

		public void TestValidateJK_SendingForwarderHandlingType_SecurityRights()
		{
			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = true;
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			string securityErrorMessage = "You do not have the security rights to add or remove the Gateway Handling Type.";

			AssertNoError("Security rights for adding/removing gateway handling type granted", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);

			Factory.Save();

			Env.Security.ConsolAdditionRemovalOfSendingReceivingAgentGatewayType.IsAllowed = false;
			consol.JK_SendingForwarderHandlingType = ZString.Empty;

			AssertHasError("Security rights for adding/removing gateway handling type not granted", consol.JK_SendingForwarderHandlingTypeInfo, securityErrorMessage);
		}

		#endregion

		#region SecurityStatusCode

		public void ValidateCheckSecurityStatusCode()
		{
			Consol.SecurityStatusCode = "~";
			AssertHasErrorContaining(Consol.SecurityStatusCodeInfo, ListValidation.InvalidCodeError);

			Consol.SecurityStatusCode = "SCO";
			AssertNoErrorContaining(Consol.SecurityStatusCodeInfo, ListValidation.InvalidCodeError);
		}

		public void ValidateCheckSecurityStatusCode_SecurityStatusDefaulting()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("AU"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Constants.TransportModes.Air;

				var transport = consol.Transports[0];
				transport.JW_TransportMode = Constants.TransportModes.Air;
				transport.JW_RL_NKLoadPort = "AUSYD";
				transport.JW_RL_NKDiscPort = "GBLON";

				var shipment = consol.Shipments.AddNew();
				shipment.JS_InspectionTypeCode = "APP";

				consol.SecurityStatusCode = "SCO";

				AssertHasWarning("Warning should show as Security Status has a default value, but is currently unset.", consol.SecurityStatusCodeInfo, "This status has been overridden and does not match attached Shipments' overall Inspection status of 'SPX'.");

				consol.SecurityStatusCode = "SPX";
				AssertNoNotifications(consol.SecurityStatusCodeInfo);
			}
		}

		public void TestValidateSecurityStatusCode_UK_UncertifiedUser()
		{
			const string expectedError = "The Person Screening the cargo must have a valid training certification applicable to handling secured air cargo (either \"CO\" (Cargo Operative), \"COS\" (Cargo Operative Screening), \"CS\" (Cargo Supervisor) or \"CM\" (Cargo Manager)) saved against the staff profile's Human Resources > Certificates & ID Numbers grid.";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("DE"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.Transports[0].JW_TransportMode = "AIR";
				consol.Transports[0].JW_RL_NKLoadPort = "DEFRA";
				consol.Transports[0].JW_RL_NKDiscPort = "AUBNE";
				consol.SecurityStatusCode = "SPX";

				AssertNoError("Not applicable for DE", consol.SecurityStatusCodeInfo, expectedError);
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry("GB"))
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = "AIR";
				consol.JK_RL_NKLoadPort = Core.Constants.CountryCodes.UnitedKingdom;
				consol.Transports[0].JW_TransportMode = "AIR";
				consol.Transports[0].JW_RL_NKLoadPort = "GBLHR";
				consol.Transports[0].JW_RL_NKDiscPort = "AUBNE";
				consol.SecurityStatusCode = "NSC";

				AssertNoError("Uncertified user can choose NSC", consol.SecurityStatusCodeInfo, expectedError);

				Factory.SetValue<ISecuredFreightVerificationChecker, FreightVerifiedStub>();
				consol.SecurityStatusCode = "SPX";

				AssertHasError("Uncertified user cannot chose 'SPX'", consol.SecurityStatusCodeInfo, expectedError);

				var certificate = GlbStaff.CurrentUser.Certificates.AddNew();
				certificate.XZ_Type = "CS2";
				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(1);
				consol.Validation.ValidateSecurityStatusCode();

				AssertNoError("Valid Certificate", consol.SecurityStatusCodeInfo, expectedError);

				certificate.XZ_ExpiryOrDueDate = ZDate.Today.AddDays(-1);
				consol.Validation.ValidateSecurityStatusCode();

				AssertHasError("Expired Certificate", consol.SecurityStatusCodeInfo, expectedError);

				Factory.Save();

				var newFactory = new BusinessObjectFactory();
				var reloadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);
				AssertEquals("Precondition: SPX", "SPX", consol.SecurityStatusCode);

				reloadedConsol.Validation.ValidateSecurityStatusCode();
				AssertNoError("No error as Security Status has no changes", reloadedConsol.SecurityStatusCodeInfo, expectedError);
			}
		}

		#endregion

		#region JK_ElectronicBillOfLadingTerms

		public void TestJK_ElectronicBillOfLadingTerms()
		{
			Consol.JK_ElectronicBillOfLadingTerms = ZString.Empty;
			AssertNoErrors(Consol.JK_ElectronicBillOfLadingTermsInfo);

			Consol.JK_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.Transferable;
			AssertNoErrors(Consol.JK_ElectronicBillOfLadingTermsInfo);

			Consol.JK_ElectronicBillOfLadingTerms = Constants.BillOfLadingBillTerms.Codes.NonTransferable;
			AssertNoErrors(Consol.JK_ElectronicBillOfLadingTermsInfo);

			Consol.JK_ElectronicBillOfLadingTerms = "INV";
			AssertHasErrors(Consol.JK_ElectronicBillOfLadingTermsInfo);
		}

		#endregion

		#region JK_ElectronicBillOfLadingType

		public void TestJK_ElectronicBillOfLadingType()
		{
			Consol.JK_ElectronicBillOfLadingType = ZString.Empty;
			AssertNoErrors(Consol.JK_ElectronicBillOfLadingTypeInfo);

			Consol.JK_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.ToOrder;
			AssertNoErrors(Consol.JK_ElectronicBillOfLadingTypeInfo);

			Consol.JK_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.Straight;
			AssertNoErrors(Consol.JK_ElectronicBillOfLadingTypeInfo);

			Consol.JK_ElectronicBillOfLadingType = Constants.BillOfLadingBillType.Codes.BlankEndorse;
			AssertNoErrors(Consol.JK_ElectronicBillOfLadingTypeInfo);

			Consol.JK_ElectronicBillOfLadingType = "INV";
			AssertHasErrors(Consol.JK_ElectronicBillOfLadingTypeInfo);
		}

		#endregion

		#region JK_RS_NKGatewayServiceLevel

		public void TestValidateJK_RS_NKGatewayServiceLevel()
		{
			SetupGatewayServiceLevels();
			var consol = GetConsolToTestGatewayServiceLevelValidation();

			consol.Validation.ValidateJK_RS_NKGatewayServiceLevel();
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo); // pre-defined supporting shipment's service level

			consol.JK_RS_NKGatewayServiceLevel = "GW3";
			AssertHasError(
				consol.JK_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'GW1' to a consol with Gateway Service Level 'GW3'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.SendingForwarder.OH_Code}' or give you permission for this operation."); // different service levels and doesn't have permission

			consol.JK_RS_NKGatewayServiceLevel = "GW1";
			AssertHasError(
				consol.JK_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'GW1' to a consol with Gateway Service Level 'GW1'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.SendingForwarder.OH_Code}' or give you permission for this operation."); // same as shipment's service level and doesn't have permission
		}

		public void TestValidateJK_RS_NKGatewayServiceLevel_CallFrom_JK_PrepaidCollect()
		{
			SetupGatewayServiceLevels();
			var consol = GetConsolToTestGatewayServiceLevelValidation();

			consol.Validation.ValidateJK_RS_NKGatewayServiceLevel();
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);

			consol.JK_PrepaidCollect = "CCX";
			AssertHasError(
				consol.JK_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'GW1' to a consol with Gateway Service Level 'GW2'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.ReceivingForwarder.OH_Code}' or give you permission for this operation."); // different service levels and doesn't have permission

			consol.JK_PrepaidCollect = "PPD";
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);
		}

		public void TestValidateJK_RS_NKGatewayServiceLevel_CallFrom_JK_SendingForwarderHandlingType()
		{
			SetupGatewayServiceLevels();
			var consol = GetConsolToTestGatewayServiceLevelValidation();
			consol.JK_SendingForwarderHandlingType = "";
			consol.JK_RS_NKGatewayServiceLevel = "GW3";

			consol.Validation.ValidateJK_RS_NKGatewayServiceLevel();
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);

			consol.JK_SendingForwarderHandlingType = "GTA";
			AssertHasError(
				consol.JK_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'GW1' to a consol with Gateway Service Level 'GW3'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.SendingForwarder.OH_Code}' or give you permission for this operation."); // different service levels and doesn't have permission

			consol.JK_SendingForwarderHandlingType = "";
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);
		}

		public void TestValidateJK_RS_NKGatewayServiceLevel_CallFrom_JK_OA_SendingForwarderAddress()
		{
			SetupGatewayServiceLevels();
			var consol = GetConsolToTestGatewayServiceLevelValidation();
			consol.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			consol.JK_RS_NKGatewayServiceLevel = "GW3";

			consol.Validation.ValidateJK_RS_NKGatewayServiceLevel();
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);

			var newSendingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var sendingAgentPorts = newSendingAgent.AppointedGatewayAgentPorts.AddNew();
			sendingAgentPorts.O5_OA_AgentOfficeAddress = newSendingAgent.MainAddress.PK;
			sendingAgentPorts.O5_PortOrCountry = consol.JK_RL_NKLoadPort;
			sendingAgentPorts.O5_AgentDirection = "BTH";
			sendingAgentPorts.O5_SeaAgentStatus = "GTA";
			sendingAgentPorts.O5_AirAgentStatus = "GTA";
			sendingAgentPorts.O5_RailAgentStatus = "GTA";
			sendingAgentPorts.O5_RoadAgentStatus = "GTA";

			var sendingGatewayService = sendingAgentPorts.ExclusiveGatewayServices.AddNew();
			sendingGatewayService.O7_RS_NKGatewayService = "GW2";
			sendingGatewayService.O7_RS_NKShipmentServiceLevel = "GW2";

			consol.JK_OA_SendingForwarderAddress = newSendingAgent.MainAddress.PK;
			AssertHasError(
				consol.JK_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'GW1' to a consol with Gateway Service Level 'GW3'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.SendingForwarder.OH_Code}' or give you permission for this operation."); // different service levels and doesn't have permission
		}

		public void TestValidateJK_RS_NKGatewayServiceLevel_CallFrom_JK_ReceivingForwarderHandlingType()
		{
			SetupGatewayServiceLevels();
			var consol = GetConsolToTestGatewayServiceLevelValidation();
			consol.JK_ReceivingForwarderHandlingType = "";
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_RS_NKGatewayServiceLevel = "GW2";

			consol.Validation.ValidateJK_RS_NKGatewayServiceLevel();
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);

			consol.JK_ReceivingForwarderHandlingType = "GTA";
			AssertHasError(
				consol.JK_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'GW1' to a consol with Gateway Service Level 'GW2'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.ReceivingForwarder.OH_Code}' or give you permission for this operation."); // different service levels and doesn't have permission

			consol.JK_ReceivingForwarderHandlingType = "";
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);
		}

		public void TestValidateJK_RS_NKGatewayServiceLevel_CallFrom_JK_OA_ReceivingForwarderAddress()
		{
			SetupGatewayServiceLevels();
			var consol = GetConsolToTestGatewayServiceLevelValidation();
			consol.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			consol.JK_PrepaidCollect = "CCX";
			consol.JK_RS_NKGatewayServiceLevel = "GW2";

			consol.Validation.ValidateJK_RS_NKGatewayServiceLevel();
			AssertNoErrors(consol.JK_RS_NKGatewayServiceLevelInfo);

			var newReceivingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var receivingAgentPorts = newReceivingAgent.AppointedGatewayAgentPorts.AddNew();
			receivingAgentPorts.O5_OA_AgentOfficeAddress = newReceivingAgent.MainAddress.PK;
			receivingAgentPorts.O5_PortOrCountry = consol.JK_RL_NKDischargePort;
			receivingAgentPorts.O5_AgentDirection = "BTH";
			receivingAgentPorts.O5_SeaAgentStatus = "GTA";
			receivingAgentPorts.O5_AirAgentStatus = "GTA";
			receivingAgentPorts.O5_RailAgentStatus = "GTA";
			receivingAgentPorts.O5_RoadAgentStatus = "GTA";

			var receivingGatewayService = receivingAgentPorts.ExclusiveGatewayServices.AddNew();
			receivingGatewayService.O7_RS_NKGatewayService = "GW3";
			receivingGatewayService.O7_RS_NKShipmentServiceLevel = "GW3";

			consol.JK_OA_ReceivingForwarderAddress = newReceivingAgent.MainAddress.PK;
			AssertHasError(
				consol.JK_RS_NKGatewayServiceLevelInfo,
				"You don't have permission to attach a shipment with Gateway Service Level 'GW1' to a consol with Gateway Service Level 'GW2'." +
				$" Please contact the administrator to either set up this relation for the organization '{consol.ReceivingForwarder.OH_Code}' or give you permission for this operation."); // different service levels and doesn't have permission
		}

		void SetupGatewayServiceLevels()
		{
			var sl1 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl1.RS_Code = "GW1";
			sl1.RS_IsGateway = true;

			var sl2 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl2.RS_Code = "GW2";
			sl2.RS_IsGateway = true;

			var sl3 = Factory.NewWithValidTestData<RefServiceLevel>();
			sl3.RS_Code = "GW3";
			sl3.RS_IsGateway = true;

			Factory.Save();
		}

		ForwardingConsol GetConsolToTestGatewayServiceLevelValidation()
		{
			Env.Security.ConsolAttachShipmentWithDifferentGatewayServiceLevel.IsAllowed = false;

			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "CONSOL1";
			consol.JK_RL_NKLoadPort = "USLAX";
			consol.JK_RL_NKDischargePort = "AUSYD";
			consol.JK_TransportMode = "SEA";
			consol.JK_AgentType = "AGT";
			consol.JK_SendingForwarderHandlingType = "GTA";
			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_ReceivingForwarderHandlingType = "GTA";
			consol.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			consol.JK_PrepaidCollect = "PPD";
			consol.JK_RS_NKGatewayServiceLevel = "GW2";
			consol.Transports[0].JW_ETD = ZDateTime.Today;
			consol.Transports[0].JW_ETA = ZDateTime.Today.AddDays(5);
			consol.RequestPermissionByImpersonation = null;

			var sendingAgentPorts = sendingAgent.AppointedGatewayAgentPorts.AddNew();
			sendingAgentPorts.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			sendingAgentPorts.O5_PortOrCountry = consol.JK_RL_NKLoadPort;
			sendingAgentPorts.O5_AgentDirection = "BTH";
			sendingAgentPorts.O5_SeaAgentStatus = "GTA";
			sendingAgentPorts.O5_AirAgentStatus = "GTA";
			sendingAgentPorts.O5_RailAgentStatus = "GTA";
			sendingAgentPorts.O5_RoadAgentStatus = "GTA";

			var sendingGatewayService = sendingAgentPorts.ExclusiveGatewayServices.AddNew();
			sendingGatewayService.O7_RS_NKGatewayService = "GW2";
			sendingGatewayService.O7_RS_NKShipmentServiceLevel = "GW1";

			var receivingAgentPorts = receivingAgent.AppointedGatewayAgentPorts.AddNew();
			receivingAgentPorts.O5_OA_AgentOfficeAddress = receivingAgent.MainAddress.PK;
			receivingAgentPorts.O5_PortOrCountry = consol.JK_RL_NKDischargePort;
			receivingAgentPorts.O5_AgentDirection = "BTH";
			receivingAgentPorts.O5_SeaAgentStatus = "GTA";
			receivingAgentPorts.O5_AirAgentStatus = "GTA";
			receivingAgentPorts.O5_RailAgentStatus = "GTA";
			receivingAgentPorts.O5_RoadAgentStatus = "GTA";

			var receivingGatewayService = receivingAgentPorts.ExclusiveGatewayServices.AddNew();
			receivingGatewayService.O7_RS_NKGatewayService = "GW3";
			receivingGatewayService.O7_RS_NKShipmentServiceLevel = "GW1";

			var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
			shipment.RequestPermissionByImpersonation = null;
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_ShipmentType = "STD";
			shipment.JS_UniqueConsignRef = "SHIPMENT1";
			shipment.JS_TransportMode = "SEA";
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_RS_NKGatewayServiceLevel = "GW1";
			shipment.Consols.Add(consol);

			return consol;
		}

		#endregion

		#region CO2e

		public void TestCheckTotalCO2eForBinding()
		{
			var shipment1 = Factory.New<ForwardingShipment>();
			shipment1.JS_TransportMode = Constants.TransportModes.Air;
			shipment1.JS_ActualWeight = 2;
			shipment1.JS_UnitOfWeight = "T";
			shipment1.SetCO2ePerTonneInKg(50000m);

			var shipment2 = Factory.New<ForwardingShipment>();
			shipment2.JS_TransportMode = Constants.TransportModes.Air;
			shipment2.JS_ActualWeight = 2;
			shipment2.JS_UnitOfWeight = "T";
			shipment2.SetCO2ePerTonneInKg(50000m);

			shipment1.Validation.ValidateTotalCO2eForBinding();
			shipment2.Validation.ValidateTotalCO2eForBinding();
			Assert("Precondition", !shipment1.TotalCO2eForBindingInfo.HasErrors());
			Assert("Precondition", !shipment2.TotalCO2eForBindingInfo.HasErrors());

			var consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.SetCO2eStatus(CO2eStatusList.Codes.Current);
			AssertNoWarning(consol.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			consol.SetCO2eStatus(CO2eStatusList.Codes.NotCurrent);
			AssertHasWarning(consol.TotalCO2eForBindingInfo, CO2eTestHelper.CO2eStaleWarning);

			consol.Shipments.Add(shipment1);
			consol.Shipments.Add(shipment2);
			AssertEquals("Precondition", "KG", consol.JK_TotalShipmentWeightUnit);
			AssertEquals("Precondition", (ZDecimal)4000, consol.JK_TotalShipmentWeight);
			Assert("Precondition", !(consol as ICO2eCalculationSupporter).RequireTEU);

			consol.SetCO2eStatus(CO2eStatusList.Codes.Rejected);
			consol.Validation.ValidateTotalCO2eForBinding();
			AssertHasWarning(consol.TotalCO2eForBindingInfo, "The greenhouse gas emissions value could not be calculated.");
		}

		#endregion

		#region Implementation

		ForwardingConsolForTest Consol;
		const string AUSYDLoco = "AUSYD";
		const string USLAXLoco = "USLAX";
		ZString BranchUNLOCO;

		protected override void SetUp()
		{
			base.SetUp();

			Consol = Factory.New<ForwardingConsolForTest>();
			Consol.JK_IsNeutralMaster = true;
			Consol.JK_RL_NKLoadPort = AUSYDLoco;
			Consol.JK_RL_NKDischargePort = USLAXLoco;

			BranchUNLOCO = GlbBranch.CurrentBranch.GB_RL_NKHomePort;
		}

		protected override void TearDown()
		{
			GlbBranch.CurrentBranch.GB_RL_NKHomePort = BranchUNLOCO;
			base.TearDown();
		}

		JobMawb AddMawb(string prefix, string mawbNo, GlbBranch branch, string serviceLevel)
		{
			JobMawb mawb = Factory.NewWithValidTestData<JobMawb>();
			mawb.JM_Airline3DigitPrefix = prefix;
			mawb.JM_MAWB = mawbNo;
			mawb.JM_GB = branch.PK;
			mawb.JM_ServiceLevel = serviceLevel;

			return mawb;
		}

		#endregion
	}
}
