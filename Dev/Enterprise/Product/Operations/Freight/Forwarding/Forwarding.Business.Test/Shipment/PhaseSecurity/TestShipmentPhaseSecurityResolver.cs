using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class TestShipmentPhaseSecurityResolver : TestCaseWithFactory
	{
		public void TestPhaseCode()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_Phase = "AAA";

			ShipmentPhaseSecurityResolverForTest resolver = new ShipmentPhaseSecurityResolverForTest(shipment);
			AssertEquals("AAA", resolver.PhaseCode);

			shipment.JS_Phase = "XXX";
			AssertEquals("XXX", resolver.PhaseCode);
		}

		public void TestIsPhaseSecurityApplicable_ForwardingModuleShipment()
		{
			var moduleShipment = Factory.New<ForwardingModuleShipment>();
			var resolver = new ShipmentPhaseSecurityResolverForTest(moduleShipment);
			resolver.GetPhaseSecurityDelegate = () => { throw new Exception("Should not access phase security"); };

			AssertEquals("PhaseSecurity should not be applicable to ForwardingModuleShipment", true, resolver.IsEditingAllowed);
		}

		public void TestGetPhaseSecurity()
		{
			PhaseSecurity security = new PhaseSecurity(PhaseConstants.GetShipmentLocationsList());

			Phase phase = security.Phases.AddNew();
			phase.Code = "AAA";
			phase.Description = (NoResString)"Hello";

			PhaseRule rule = phase.Rules.AddNew();
			rule.Location = PhaseConstants.Locations.AnyLocation;
			rule.DepartmentPK = GlbDepartment.CurrentDepartment.PK;

			ForwardingConfigurationRegistry.Instance.ShipmentPhaseSecurity.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, security);

			var shipment = Factory.New<ForwardingShipment>();
			var resolver = new ShipmentPhaseSecurityResolverForTest(shipment);

			var phaseSecurity = resolver.PhaseSecurity_Exposed;
			AssertNotNull("Phase security comes from registry", phaseSecurity);
			AssertEquals("AAA", phaseSecurity.Phases.First().Code);
		}

		public void TestResolveUNLOCOs()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			ShipmentPhaseSecurityResolverForTest resolver = new ShipmentPhaseSecurityResolverForTest(shipment);
			CombineAssertions(delegate
			{
				AssertContainsExactElementsInAnyOrder(new ZString[] { "AUSYD" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.OriginPort));
				AssertContainsExactElementsInAnyOrder(new ZString[] { "AU" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.OriginCountry));

				AssertContainsExactElementsInAnyOrder(new ZString[] { "USLAX" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DestinationPort));
				AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DestinationCountry));

				AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadPort).Any());
				AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadCountry).Any());
				AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargePort).Any());
				AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeCountry).Any());
				AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.TransitCountry).Any());
			});

			// Adding this consol first to ensure that consol collection will be sorted by ports
			ForwardingConsol consol3 = shipment.Consols.AddNew();
			consol3.JK_RL_NKLoadPort = "DEFRA";
			consol3.JK_RL_NKDischargePort = "USSFO";

			ForwardingConsol consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			ForwardingConsol consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "DEFRA";

			Transport transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "AUMEL";
			transport.JW_RL_NKDiscPort = "NZAKL";

			transport = shipment.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "NZAKL";
			transport.JW_RL_NKDiscPort = "SGSIN";

			transport = consol1.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "SGSIN";
			transport.JW_RL_NKDiscPort = "DEBRE";

			transport = consol1.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEBRE";
			transport.JW_RL_NKDiscPort = "DEFRA";

			transport = consol2.Transports.AddNew();
			transport.JW_RL_NKLoadPort = "DEFRA";
			transport.JW_RL_NKDiscPort = "USSFO";

			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUSYD" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.OriginPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.OriginCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "USLAX" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DestinationPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DestinationCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUMEL" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "USSFO" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargePort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeCountry));

			AssertContainsExactElementsInAnyOrder(new ZString[] { "NZ", "SG", "DE" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.TransitCountry));
		}

		public void TestResolvingPhaseDoesNotLoadConsolCollectionOnShipment()
		{
			BusinessObjectFactory creationFactory = new BusinessObjectFactory();
			ForwardingShipment someShipment = creationFactory.New<ForwardingShipment>();
			someShipment.Consols.AddNew();
			someShipment.Consols.AddNew();

			AssertEquals("Precondition: shipment has consols", 2, someShipment.Consols.Count);
			creationFactory.Save();

			ForwardingShipmentForRegisterEditableTest shipmentReloaded = new BusinessObjectFactory().Load<ForwardingShipmentForRegisterEditableTest>(someShipment.PK);
			AssertEquals("Precondition", false, shipmentReloaded.ConsolPivotCollectionRegisteredAsEditableChild);
			AssertEquals("Touching consols collection to ensure that consol-shipment pivot collection will be registered as editable child", 2, shipmentReloaded.Consols.Count);
			AssertEquals("Pivot collection was registered as editable child", true, shipmentReloaded.ConsolPivotCollectionRegisteredAsEditableChild);

			ForwardingShipmentForRegisterEditableTest shipment = Factory.Load<ForwardingShipmentForRegisterEditableTest>(someShipment.PK);
			AssertEquals("Precondition", false, shipment.ConsolPivotCollectionRegisteredAsEditableChild);

			// Resolve those locations that will require consols to be loaded
			ShipmentPhaseSecurityResolverForTest resolver = new ShipmentPhaseSecurityResolverForTest(shipment);
			resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadPort);
			resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadCountry);
			resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargePort);
			resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeCountry);
			resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.TransitCountry);

			string extremelyImportantMessage =
				"<br/><br/><b>IMPORTANT:</b> Consol collection on the shipment should <b>NOT</b> be loaded during phase resolving<br/>" +
				"Failure of this test means that stack overflow exception will happen in runtime<br/>" +
				"Please contact International Logistics team if you don't know how to <b>PROPERLY</b> fix this test.<br/>";

			HtmlAssertEquals(extremelyImportantMessage, false, shipment.ConsolPivotCollectionRegisteredAsEditableChild);
		}

		public void TestResolvingPhase_DBOnlyQueriesAreNotUsedToLoadConsols_FactoryCacheIsUsed()
		{
			var creationFactory = new BusinessObjectFactory();

			var consol = creationFactory.NewWithValidTestData<ForwardingConsol>();
			var shipment1 = consol.Shipments.AddNew();
			var shipment2 = consol.Shipments.AddNew();
			var shipment3 = consol.Shipments.AddNew();

			creationFactory.Save();

			var consolReloaded = Factory.Load<ForwardingConsol>(consol.PK);
			var consolTableHitCountBefore = Factory.GetTableHitCount(JobConsolSchema.Constants.TableName);

			foreach (var shipment in new[] { shipment1, shipment2, shipment3 })
			{
				var shipmentReloaded = Factory.Load<ForwardingShipment>(shipment.PK);

				var resolver = new ShipmentPhaseSecurityResolverForTest(shipmentReloaded);
				resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadPort);

				var consolTableHitCountAfter = Factory.GetTableHitCount(JobConsolSchema.Constants.TableName);
				AssertEquals("Should not hit database, but use already loaded objects from factory", consolTableHitCountAfter, consolTableHitCountBefore);
			}
		}

		#region ResolveUNLOCOs with Own Sending/Receiving Agent

		public void TestResolveUNLOCOs_WithOwnSendingAgent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var resolver = new ShipmentPhaseSecurityResolverForTest(shipment);
			AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort).Any());
			AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry).Any());

			var sendingAgent = Factory.New<OrgHeader>();
			sendingAgent.OH_Code = "TEST100";
			sendingAgent.MainAddress.OA_RL_NKRelatedPortCode = "FRPAR";
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "FR";

			// Adding this consol first to ensure that consol collection will be sorted by ports
			var consol3 = shipment.Consols.AddNew();
			consol3.JK_RL_NKLoadPort = "DEFRA";
			consol3.JK_RL_NKDischargePort = "USSFO";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "DEFRA";

			Factory.Save();

			AssertNull(consol1.SendingForwarder);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUMEL" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry));

			consol1.JK_OA_SendingForwarderAddress = sendingAgent.MainAddress.PK;
			Assert(!consol1.SendingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUMEL" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry));

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = sendingAgent.PK;
			Assert(consol1.SendingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUMEL", "FRPAR" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "FR" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry));

			sendingAgent.MainAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			sendingAgent.MainAddress.OA_RN_NKCountryCode = "HK";
			Assert(consol1.SendingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AUMEL" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "AU", "HK" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.LoadOrOwnSendingAgentCountry));
		}

		public void TestResolveUNLOCOs_WithOwnReceivingAgent()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			shipment.JS_RL_NKOrigin = "AUSYD";
			shipment.JS_RL_NKDestination = "USLAX";

			var resolver = new ShipmentPhaseSecurityResolverForTest(shipment);
			AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort).Any());
			AssertEquals(false, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry).Any());

			var receivingAgent = Factory.New<OrgHeader>();
			receivingAgent.OH_Code = "TEST100";
			receivingAgent.MainAddress.OA_RL_NKRelatedPortCode = "AUSYD";
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "AU";

			// Adding this consol first to ensure that consol collection will be sorted by ports
			var consol3 = shipment.Consols.AddNew();
			consol3.JK_RL_NKLoadPort = "DEFRA";
			consol3.JK_RL_NKDischargePort = "USSFO";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_RL_NKLoadPort = "AUMEL";
			consol1.JK_RL_NKDischargePort = "SGSIN";

			var consol2 = shipment.Consols.AddNew();
			consol2.JK_RL_NKLoadPort = "SGSIN";
			consol2.JK_RL_NKDischargePort = "DEFRA";

			Factory.Save();

			AssertNull(consol3.ReceivingForwarder);
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USSFO" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry));

			consol3.JK_OA_ReceivingForwarderAddress = receivingAgent.MainAddress.PK;
			Assert(!consol3.ReceivingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USSFO" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry));

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = receivingAgent.PK;
			Assert(consol3.ReceivingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USSFO", "AUSYD" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US", "AU" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry));

			receivingAgent.MainAddress.OA_RL_NKRelatedPortCode = ZString.Empty;
			receivingAgent.MainAddress.OA_RN_NKCountryCode = "FR";
			Assert(consol3.ReceivingForwarder.IsProxyOrg(GlbCompany.CurrentCompany));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "USSFO" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentPort));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "US", "FR" }, resolver.ResolveUNLOCOs(PhaseConstants.Locations.Shipment.DischargeOrOwnReceivingAgentCountry));
		}

		#endregion
	}
}
