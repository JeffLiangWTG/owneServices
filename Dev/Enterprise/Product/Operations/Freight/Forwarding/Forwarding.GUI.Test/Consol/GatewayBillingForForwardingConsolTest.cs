using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	class GatewayBillingForForwardingConsolTest : TestCaseWithFactory
	{
		public void TestJobInvoicingSecurityConcreteAndBillingApportionmentTabs_GatewayAgent()
		{
			TestJobInvoicingSecurityConcreteAndBillingApportionmentTabs(Constants.AgentType.Agent);
		}

		public void TestJobInvoicingSecurityConcreteAndBillingApportionmentTabs_GatewayCoLoad()
		{
			TestJobInvoicingSecurityConcreteAndBillingApportionmentTabs(Constants.AgentType.CoLoad);
		}

		///<summary>
		///6. Access to the Gateway Billing, hence "GB", tab is restricted by security settings. 
		///		- GB related tabs are shown when Consol is a legacy gateway consol or is GB enabled for the current company.
		///</summary>
		void TestJobInvoicingSecurityConcreteAndBillingApportionmentTabs(string gatewayType)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUSYD";
			var invoicingSupporter = new ForwardingConsolInvoicingSupporter(consol);

			AssertEquals(false, consol.IsGateway());
			AssertEquals("Default Consol JobInvoicingSecurity", Env.Security.MaintainConsolJobInvoicing, invoicingSupporter.JobInvoicingSecurity);
			AssertNotNull("Default Consol invoicing", Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.ReverseBilling));
			AssertNotNull("Default Consol invoicing", Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.PrintGovtTax));
			AssertNotNull("Default Consol invoicing", Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.MaintainConsolJobInvoicing, SecurityCore.PrintSelfBillingInvoiceDocument));

			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;

			var gatewayAgentPort = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			gatewayAgentPort.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			gatewayAgentPort.O5_PortOrCountry = "AUSYD";
			gatewayAgentPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			gatewayAgentPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.JK_AgentType = gatewayType;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertEquals(true, consol.IsGateway());
			AssertEquals("Gateway Consol JobInvoicingSecurity", Env.Security.GatewayConsolJobInvoicing, invoicingSupporter.JobInvoicingSecurity);
			AssertNotNull("Gateway Consol invoicing", Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.GatewayConsolJobInvoicing, SecurityCore.ReverseBilling));
			AssertNotNull("Gateway Consol invoicing", Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.GatewayConsolJobInvoicing, SecurityCore.PrintGovtTax));
			AssertNotNull("Gateway Consol invoicing", Env.Security.GetInvoicingSecurityCheckPoint(Env.Security.GatewayConsolJobInvoicing, SecurityCore.PrintSelfBillingInvoiceDocument));

			consol = Factory.NewWithValidTestData<ForwardingConsol>();

			using (var consolForm = new ConsolForm(consol))
			{
				Assert(!consol.IsGateway());
				AssertPlugInShownForNonGWConsol(consolForm);
			}

			consol.JK_AgentType = gatewayType;
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertNotNull("Precondition", consol.SendingForwarder);

			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;

			using (var consolForm = new ConsolForm(consol))
			{
				Assert(consol.IsGateway());
				AssertPlugInShownForGWConsol(consolForm);
			}

			var companyQuery = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var company = Factory.LoadTop1<GlbCompany>(companyQuery);
			AssertNotNull(company);

			var orgAppointedAgentPorts2 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts2.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts2.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.JK_OA_SendingForwarderAddress = company.OrgProxy.MainAddress.PK;
			consol.JK_AgentType = Constants.AgentType.Agent;

			using (var consolForm = new ConsolForm(consol))
			{
				Assert(!consol.IsGateway());
				AssertPlugInShownForNonGWConsol(consolForm);
			}

			consol.JK_OA_SendingForwarderAddress = GlbBranch.CurrentBranch.OrgProxy.MainAddress.PK;
			var orgAppointedAgentPorts3 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts3.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts3.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts3.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts3.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			consol.JK_AgentType = gatewayType;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			using (var consolForm = new ConsolForm(consol))
			{
				Assert(consol.IsGateway());
				AssertPlugInShownForGWConsol(consolForm);

				consol.JK_AgentType = Constants.AgentType.Courier;

				Assert(!consol.IsGateway());
				AssertPlugInShownForNonGWConsol(consolForm);
			}
			consol.JK_AgentType = gatewayType;
		}

		public void TestGatewayNotShownWhenSharedCompanyConsolHasConsolCosts_GatewayAgent()
		{
			TestGatewayNotShownWhenSharedCompanyConsolHasConsolCosts(Constants.AgentType.Agent);
		}

		public void TestGatewayNotShownWhenSharedCompanyConsolHasConsolCosts_GatewayCoLoad()
		{
			TestGatewayNotShownWhenSharedCompanyConsolHasConsolCosts(Constants.AgentType.CoLoad);
		}

		/// <summary>
		/// 7. Gateway Billing tab/job should not be accessible from another company 
		/// </summary>
		void TestGatewayNotShownWhenSharedCompanyConsolHasConsolCosts(string gatewayType)
		{
			AutoJRJRegistryStatusHelper.SetAutoJRJEnabled_ForTestOnly();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Sea;
			consol.JK_AgentType = Constants.AgentType.Direct;
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "DEFRA";
			consol.JK_OA_SendingForwarderAddress = GlbCompany.CurrentCompany.OrgProxy.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			AssertNotNull("Pre-condition", consol.SendingForwarder);

			var orgAppointedAgentPorts1 = consol.SendingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts1.O5_OA_AgentOfficeAddress = consol.JK_OA_SendingForwarderAddress;
			orgAppointedAgentPorts1.O5_PortOrCountry = "AUBNE";
			orgAppointedAgentPorts1.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts1.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var query = new ZQuery(GlbCompanySchema.PK, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK);
			var anotherCompany = Factory.LoadTop1<GlbCompany>(query);
			consol.JK_OA_ReceivingForwarderAddress = anotherCompany.OrgProxy.MainAddress.PK;

			AssertNotNull("Pre-condition", consol.ReceivingForwarder);

			var orgAppointedAgentPorts2 = consol.ReceivingForwarder.AppointedGatewayAgentPorts.AddNew();
			orgAppointedAgentPorts2.O5_OA_AgentOfficeAddress = consol.JK_OA_ReceivingForwarderAddress;
			orgAppointedAgentPorts2.O5_PortOrCountry = "DEFRA";
			orgAppointedAgentPorts2.O5_AgentDirection = AgentDirectionList.Codes.Both;
			orgAppointedAgentPorts2.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			var consolCost = CreateConsolCost(consol);

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var loadedConsol = newFactory.Load<ForwardingConsol>(consol.PK);

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, anotherCompany.Branches[0].PK.ToGuid(), Env.CurrentDepartment.PK))
			{
				using (var consolForm = new ConsolForm(loadedConsol))
				{
					Assert("Currently Agent mode with no gateway job", !loadedConsol.IsGateway());
					AssertPlugInShownForNonGWConsol(consolForm);

					loadedConsol.JK_AgentType = gatewayType;
					loadedConsol.JK_ReceivingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

					Assert("Should become gateway once consol costs are deleted", loadedConsol.IsGateway());
					AssertNoErrors(loadedConsol.JK_ReceivingForwarderHandlingTypeInfo);
					AssertPlugInShownForGWConsol(consolForm, "however there are no consol costs for this company so we should see it");

					loadedConsol.JK_AgentType = Constants.AgentType.Agent;
					consolCost.Delete();
					Factory.Save();
					loadedConsol.JK_AgentType = gatewayType;
					loadedConsol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

					Assert("Should become gateway once consol costs are deleted", loadedConsol.IsGateway());
					AssertPlugInShownForGWConsol(consolForm);
				}
				consolCost = CreateConsolCost(consol);
				loadedConsol.JK_ReceivingForwarderHandlingType = ZString.Empty;
				Factory.Save();
			}
		}

		BusinessObject CreateConsolCost(ForwardingConsol consol)
		{
			var consolCost = Factory.NewWithValidTestData(ObjectFactory.GetType<IJobConsolCost>());
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

			return consolCost;
		}

		static void AssertPlugInShownForNonGWConsol(ConsolForm consolForm, string message = "")
		{
			var apportionmentPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment);
			var invoicingPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			var sellApportionmentForGWPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.SellApportionmentForGateway);

			Assert(message, !invoicingPlugIn.Enabled);
			AssertEquals("Billing", invoicingPlugIn.TabPage.Text);
			AssertEquals("Consol Costing", apportionmentPlugIn.TabPage.Text);

			Assert(apportionmentPlugIn.TopLevelMenu.Visible);
			AssertEquals("&Job Invoicing", apportionmentPlugIn.TopLevelMenu.Text);

			Assert(message, !sellApportionmentForGWPlugIn.Enabled);
		}

		static void AssertPlugInShownForGWConsol(ConsolForm consolForm, string message = "")
		{
			var apportionmentPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.Apportionment);
			var invoicingPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing);
			var sellApportionmentForGWPlugIn = consolForm.PlugIns.GetPlugIn(ControllerIDs.SellApportionmentForGateway);

			Assert(message, invoicingPlugIn.Enabled);
			AssertEquals("Gateway Billing", invoicingPlugIn.TabPage.Text);
			AssertEquals("Gateway Invoicing", invoicingPlugIn.TopLevelMenu.Text);

			AssertEquals("Consol Costing", apportionmentPlugIn.TabPage.Text);
			Assert(apportionmentPlugIn.TopLevelMenu.Visible);
			AssertEquals("&Job Invoicing", apportionmentPlugIn.TopLevelMenu.Text);

			Assert(message, sellApportionmentForGWPlugIn.Enabled);
			AssertNull(sellApportionmentForGWPlugIn.TopLevelMenu);
		}
	}
}
