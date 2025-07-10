using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Rating.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ProfitShareForwardingModuleConsolCollection))]
	sealed class ProfitShareForwardingModuleConsolCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new ProfitShareForwardingModuleConsolCollection(Factory);

		public void TestDefaultFilter()
		{
			var collection = GetCollectionToTest();

			CombineAssertions(() =>
			{
				AssertFilter("Consol # [C]", "Consol #:Property");
				AssertFilter("End Ports (First Load / Last Disch.)", "End Ports (First Load / Last Disch.):Property1");
				AssertFilter("ETD / Load Port", "ETD / Load Port:Property1");
				AssertFilter("ETA / Discharge Port", "ETA / Discharge Port:Property1");
				AssertFilter("Sending Agent Type", "Sending Agent Type:Property");
				AssertFilter("Receiving Agent Type", "Receiving Agent Type:Property");
				AssertFilter("Send / Receive Agents", "Send / Receive Agents:Property1");
			});

			void AssertFilter(string propertyName, string filterKey)
			{
				Assert($"should contain default prop {propertyName}", collection.FilterBusinessObjectDefaults.ContainsDefaultFor(filterKey));
				AssertEquals($"filterKey {filterKey} should have empty value", ZString.Empty, collection.FilterBusinessObjectDefaults[filterKey].Value);
			}
		}

		public void TestNotification()
		{
			var consol1 = Factory.NewWithValidTestData<ForwardingConsol>();

			var consol2 = Factory.New<ForwardingConsol>();
			consol2.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol2.JK_AgentType = Core.Constants.AgentType.Agent;
			consol2.JK_RL_NKLoadPort = "AUSYD";
			consol2.JK_RL_NKDischargePort = "USLAX";
			consol2.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol2.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
			var appointedPortsForSendingAgent = consol2.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			appointedPortsForSendingAgent.O5_OA_AgentOfficeAddress = consol2.JK_OA_SendingForwarderAddress;
			appointedPortsForSendingAgent.O5_PortOrCountry = "AUSYD";
			appointedPortsForSendingAgent.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appointedPortsForSendingAgent.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;

			Factory.Save();

			Assert("Consol 1 is not a gateway", !((IGateway)consol1).GatewayBillingSupporter.IsGatewayBillingEnabled());
			AssertEquals("Consol 2 is a gateway", true, ((IGateway)consol2).GatewayBillingSupporter.IsGatewayBillingEnabled());

			var collection = new ProfitShareForwardingModuleConsolCollection(Factory);

			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;
			var notification = notificationProvider.GetExtraNotification(consol1);

			AssertEquals(true, notification.Type == CargoWise.ComponentModel.NotificationType.Error);
			AssertEquals("Notification message", "Only Consol with Login Company as the Sending or Receiving Agent (GTT/GTA) is valid to be selected.", notification.Message);

			notification = notificationProvider.GetExtraNotification(consol2);
			AssertNull("Should not have any error", notification);

			var consolidationProfitShare = Factory.NewWithValidTestData<ConsolidationProfitShare>();
			consolidationProfitShare.CPS_JK = consol2.PK;
			Factory.Save();

			notification = notificationProvider.GetExtraNotification(consol2);
			AssertEquals("Notification message", "This consol is already processed.", notification.Message);
		}

		public void TestNotification_ProfitShareRedistributionIsAllowedPerLoginCompany()
		{
			var branch1 = CreateBranch("BSG", "CSG", "SG", "SGSIN");
			var branch2 = CreateBranch("BIN", "CIN", "IN", "INIXE");
			var branch1Address = branch1.OrgProxy.MainAddress;
			var branch2Address = branch2.OrgProxy.MainAddress;
			var consol1 = CreateGatewayConsol(branch1Address, branch2Address);
			var consol2 = CreateGatewayConsol(branch2Address, branch1Address);

			Factory.Save();

			var branch1PK = branch1.PK.ToGuid();
			var branch2PK = branch2.PK.ToGuid();
			ProcessConsol(branch1PK, consol1.PK);
			ProcessConsol(branch2PK, consol2.PK);

			var collection = new ProfitShareForwardingModuleConsolCollection(Factory);
			var notificationProvider = collection as IFilterModuleExtraNotificationProvider;

			AssertHasError(branch1PK, consol1.PK);
			AssertHasError(branch2PK, consol2.PK);

			AssertNoError(branch1PK, consol2.PK);
			AssertNoError(branch2PK, consol1.PK);

			ProcessConsol(branch1PK, consol2.PK);
			ProcessConsol(branch2PK, consol1.PK);

			AssertHasError(branch1PK, consol2.PK);
			AssertHasError(branch2PK, consol1.PK);

			GlbBranch CreateBranch(string branchCode, string companyCode, string countryCode, string portCode)
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				SetupPort(org, countryCode);
				org.MainAddress.OA_RL_NKRelatedPortCode = portCode;

				var branch = Factory.NewCompanyAndBranchWith(companyCode, branchCode, countryCode);
				branch.GB_OH_OrgProxy = org.PK;
				branch.Company.GC_OH_OrgProxy = org.PK;

				return branch;
			}

			void SetupPort(OrgHeader agent, string portOrCountry)
			{
				var appPort = agent.AppointedGatewayAgentPorts.AddNew();
				appPort.O5_OA_AgentOfficeAddress = agent.MainAddress.PK;
				appPort.O5_PortOrCountry = portOrCountry;
				appPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
				appPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				appPort.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				appPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
				appPort.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgentWithTariff;
			}

			ForwardingConsol CreateGatewayConsol(OrgAddress sendingAgentAddress, OrgAddress receivingAgentAddress)
			{
				var consol = Factory.New<ForwardingConsol>();
				consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
				consol.JK_AgentType = Core.Constants.AgentType.Agent;
				consol.JK_RL_NKLoadPort = sendingAgentAddress.ClosestPort;
				consol.JK_RL_NKDischargePort = receivingAgentAddress.ClosestPort;
				consol.JK_OA_SendingForwarderAddress = sendingAgentAddress.PK;
				consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;
				consol.JK_OA_ReceivingForwarderAddress = receivingAgentAddress.PK;
				consol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgentWithTariff;

				return consol;
			}

			void ProcessConsol(Guid branchPK, ZGuid consolPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
				{
					var consol = Factory.Load<ForwardingConsol>(consolPK);
					using (var consolJob = new JobHeader.Loader(Factory, consol).TryLoadOrCreateWithMutex())
					{
						AssertNotNull("Pre-Condition: consol job", consolJob);

						var profitShareRedistribution = Factory.NewWithValidTestData<ProfitShareRedistribution>();
						var consolidationProfitShare = Factory.New<ConsolidationProfitShare>();
						consolidationProfitShare.CPS_PSR = profitShareRedistribution.PK;
						consolidationProfitShare.CPS_RX_NKCurrency = CurrencyCodes.Australia;
						consolidationProfitShare.CPS_JK = consol.PK;
						consolidationProfitShare.CPS_JH_ConsolJob = consolJob.PK;

						Factory.Save();
					}
				}
			}

			void AssertHasError(Guid branchPK, ZGuid consolPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
				{
					var consol = Factory.Load<ForwardingConsol>(consolPK);
					var notification = notificationProvider.GetExtraNotification(consol);
					AssertEquals("Notification message", "This consol is already processed.", notification.Message);
				}
			}

			void AssertNoError(Guid branchPK, ZGuid consolPK)
			{
				using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, branchPK, Env.CurrentDepartment.PK))
				{
					var consol = Factory.Load<ForwardingConsol>(consolPK);
					var notification = notificationProvider.GetExtraNotification(consol);
					AssertNull("Should not have any error", notification);
				}
			}
		}
	}
}
