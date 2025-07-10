using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class TestConsolPhaseSecurityResolver : TestCaseWithFactory
	{
		public void TestPhaseCode()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_Phase = "AAA";

			ConsolPhaseSecurityResolverForTest resolver = new ConsolPhaseSecurityResolverForTest(consol);
			AssertEquals("AAA", resolver.PhaseCode);

			consol.JK_Phase = "XXX";
			AssertEquals("XXX", resolver.PhaseCode);
		}

		public void TestIsPhaseSecurityApplicable_ForwardingModuleConsol()
		{
			var moduleConsol = Factory.New<ForwardingModuleConsol>();
			var resolver = new ConsolPhaseSecurityResolverForTest(moduleConsol);
			resolver.GetPhaseSecurityDelegate = () => { throw new Exception("Should not access phase security"); };

			AssertEquals("PhaseSecurity should not be applicable to ForwardingModuleConsol", true, resolver.IsEditingAllowed);
		}

		public void TestGetPhaseSecurity()
		{
			PhaseSecurity security = new PhaseSecurity(PhaseConstants.GetConsolLocationsList());

			Phase phase = security.Phases.AddNew();
			phase.Code = "AAA";
			phase.Description = (NoResString)"Hello";

			PhaseRule rule = phase.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			ForwardingConfigurationRegistry.Instance.ConsolPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

			var consol = Factory.New<ForwardingConsol>();
			var resolver = new ConsolPhaseSecurityResolverForTest(consol);

			var phaseSecurity = resolver.PhaseSecurity_Exposed;
			AssertNotNull("Phase security comes from registry", phaseSecurity);
			AssertEquals("AAA", phaseSecurity.Phases.First().Code);
		}

		public void TestResolveUNLOCOs()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.JK_TransportMode = Constants.TransportModes.Air;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";

			Transport transport = consol.Transports[0];
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_TransportType = Core.Constants.TransportPlanningType.Flight1;
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "CATOR";

			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "DEFRA";

			transport = consol.Transports.AddNew();
			transport.JW_TransportMode = Constants.TransportModes.Air;
			transport.JW_RL_NKLoadPort = "DEFRA";
			transport.JW_RL_NKDiscPort = "USSFO";

			ConsolPhaseSecurityResolverForTest resolver = new ConsolPhaseSecurityResolverForTest(consol);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUSYD" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "USLAX" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargePort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZAKL" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LoadPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LoadCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "CATOR" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.DischargePort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CA" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.DischargeCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ", "CA", "SG", "DE" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Consol.TransitCountry));
		}

		#region ResolveUNLOCOs with Own Sending/Receiving Agent

		public void TestResolveUNLOCOs_WithOwnSendingAgent()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "USLAX";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_OA_SendingForwarderAddress = ZGuid.Empty;
			AssertNull(consol1.ReceivingForwarder);

			var resolver1 = new ConsolPhaseSecurityResolverForTest(consol1);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USLAX" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentCountry));

			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_Code = "TEST100";
			sendingAgent.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "AU";
			consol1.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			Assert(!consol1.SendingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USLAX" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentCountry));

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = sendingAgent.PK;
			Assert(consol1.SendingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USLAX", "AUSYD" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US", "AU" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentCountry));

			sendingAgent.MainAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "FR";
			Assert(consol1.SendingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USLAX" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US", "FR" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.FirstLoadOrOwnSendingAgentCountry));
		}

		public void TestResolveUNLOCOs_WithOwnReceivingAgent()
		{
			var consol1 = Factory.New<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;
			consol1.JK_RL_NKLoadPort = "USLAX";
			consol1.JK_RL_NKDischargePort = "NZAKL";
			consol1.JK_OA_ReceivingForwarderAddress = ZGuid.Empty;
			AssertNull(consol1.ReceivingForwarder);

			var resolver1 = new ConsolPhaseSecurityResolverForTest(consol1);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZAKL" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentCountry));

			var receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_Code = "TEST100";
			receivingAgent.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "AU";
			consol1.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			Assert(!consol1.ReceivingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZAKL" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentCountry));

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = receivingAgent.PK;
			Assert(consol1.ReceivingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZAKL", "AUSYD" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ", "AU" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentCountry));

			receivingAgent.MainAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "FR";
			Assert(consol1.ReceivingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZAKL" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ", "FR" }, resolver1.ResolveUNLOCOs(PhaseConstants.Locations.Consol.LastDischargeOrOwnReceivingAgentCountry));
		}

		#endregion
	}
}
