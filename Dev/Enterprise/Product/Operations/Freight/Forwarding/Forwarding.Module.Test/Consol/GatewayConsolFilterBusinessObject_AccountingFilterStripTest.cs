using CargoWise.Types;
using Enterprise.Accounting.Integration.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	public class GatewayConsolFilterBusinessObject_AccountingFilterStripTest : AccountingFilterStripTest<ForwardingConsol>
	{
		protected override ForwardingConsol GetNewBusinessObjectForFilterCollection()
		{
			var sendingGatewayCompany = GlbCompany.CurrentCompany;
			var sendingAgent = Factory.Load<OrgHeader>(sendingGatewayCompany.OrgProxy.PK);

			var appPort = sendingAgent.AppointedGatewayAgentPorts.AddNew();
			appPort.O5_OA_AgentOfficeAddress = sendingAgent.MainAddress.PK;
			appPort.O5_PortOrCountry = "AUSYD";
			appPort.O5_AgentDirection = AgentDirectionList.Codes.Both;
			appPort.O5_AirAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort.O5_RailAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort.O5_SeaAgentStatus = AgentStatusList.Codes.GatewayAgent;
			appPort.O5_RoadAgentStatus = AgentStatusList.Codes.GatewayAgent;

			Factory.Save();

			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			consol.JK_AgentType = Constants.AgentType.Agent;
			consol.JK_TransportMode = "AIR";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "SGSIN";

			var transport = consol.Transports[0];
			transport.JW_RL_NKLoadPort = "AUSYD";
			transport.JW_RL_NKDiscPort = "SGSIN";
			transport.JW_ETD = ZDateTime.Now.AddDays(10);
			transport.JW_ETA = ZDateTime.Now.AddDays(13);
			transport.JW_VoyageFlight = "QF105";

			consol.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			consol.JK_SendingForwarderHandlingType = AgentStatusList.Codes.GatewayAgent;

			return consol;
		}

		protected override ModuleIdentifier FilterStripModuleID
		{
			get { return ModuleIDs.JobConsol; }
		}

		public new void TestControlForAmountFilters()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobProfitFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobMarginPercentFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobRevenueAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobWIPAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobWIPAmountExcludingDeferredChargesFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobWIPAmountDeferredChargesOnlyFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobCostAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestJobAccrualAmountFilter()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestAmountFiltersWhenJobsDoesntHaveTransactionLines()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestAmountFiltersTogether()
		{
			Assert("There are not Amount filters in this module.", true);
		}

		public new void TestLocalJobReferenceFilter()
		{
			Assert("There are not Reference filters in this module.", true);
		}

		public new void TestAddJobManagementFiltersOnlyIfSecurityIsAllowed()
		{
			Assert("This condition not sutible here.", true);
		}

		public new void TestJobManagementFiltersWithMaxAmounts()
		{
			Assert(true);
		}
	}
}
