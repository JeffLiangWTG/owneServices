using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportProviderPrioritiserTest : TestCaseWithFactory
	{
		public void TestNoBranchOrCompanyOrgProxyDoesNotThrowException()
		{
			var consol = Factory.New<CommonConsol>();

			GlbBranch.CurrentBranch.GB_OH_OrgProxy = ZGuid.Empty;
			AssertNoExceptionThrown(() => consol.GetCreditors());

			GlbCompany.CurrentCompany.GC_OH_OrgProxy = ZGuid.Empty;
			AssertNoExceptionThrown(() => consol.GetCreditors());
		}

		public void TestTransportProvidersOnShipment()
		{
			var creditorOnRouteC1 = Factory.NewWithValidTestData<OrgHeader>();
			var creditorC1 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoOnRouteC1 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoC1 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsC1 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsTransportC1 = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoOnRouteC1 = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoC1 = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsC1 = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsTransportC1 = Factory.NewWithValidTestData<OrgHeader>();
			var carrierC1 = Factory.NewWithValidTestData<OrgHeader>();
			var sendingAgentC1 = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgentC1 = Factory.NewWithValidTestData<OrgHeader>();

			var creditorOnRouteC2 = Factory.NewWithValidTestData<OrgHeader>();
			var creditorC2 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoOnRouteC2 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoC2 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsC2 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsTransportC2 = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoOnRouteC2 = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoC2 = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsC2 = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsTransportC2 = Factory.NewWithValidTestData<OrgHeader>();
			var carrierC2 = Factory.NewWithValidTestData<OrgHeader>();
			var sendingAgentC2 = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgentC2 = Factory.NewWithValidTestData<OrgHeader>();

			var creditorOnRouteC3 = Factory.NewWithValidTestData<OrgHeader>();
			var creditorC3 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoOnRouteC3 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoC3 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsC3 = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsTransportC3 = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoOnRouteC3 = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoC3 = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsC3 = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsTransportC3 = Factory.NewWithValidTestData<OrgHeader>();
			var carrierC3 = Factory.NewWithValidTestData<OrgHeader>();
			var sendingAgentC3 = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgentC3 = Factory.NewWithValidTestData<OrgHeader>();

			var importBroker = Factory.NewWithValidTestData<OrgHeader>();
			var exportBroker = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryAgent = Factory.NewWithValidTestData<OrgHeader>();
			var pickupAgent = Factory.NewWithValidTestData<OrgHeader>();
			var controllingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var deliveryCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			var pickupCartageCo = Factory.NewWithValidTestData<OrgHeader>();
			var importCFS = Factory.NewWithValidTestData<OrgHeader>();
			var exportCFS = Factory.NewWithValidTestData<OrgHeader>();

			var shipment = Factory.NewWithValidTestData<CommonShipment>();
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "AUSYD";
			shipment.JS_TransportMode = "SEA";

			var consol1 = shipment.Consols.AddNew();
			consol1.JK_AgentType = Constants.AgentType.CoLoad;
			var consol2 = shipment.Consols.AddNew();
			consol2.JK_AgentType = Constants.AgentType.CoLoad;
			var consol3 = shipment.Consols.AddNew();
			consol3.JK_AgentType = Constants.AgentType.CoLoad;

			var ass = new TPAsserter();

			ass.SetC1Orgs(consol1, creditorOnRouteC1, creditorC1, carrierC1, sendingAgentC1, receivingAgentC1, depCtoOnRouteC1, depCtoC1, depCfsC1, depCfsTransportC1, arrCtoOnRouteC1, arrCtoC1, arrCfsC1, arrCfsTransportC1);
			ass.SetC2Orgs(consol2, creditorOnRouteC2, creditorC2, carrierC2, sendingAgentC2, receivingAgentC2, depCtoOnRouteC2, depCtoC2, depCfsC2, depCfsTransportC2, arrCtoOnRouteC2, arrCtoC2, arrCfsC2, arrCfsTransportC2);
			ass.SetC3Orgs(consol3, creditorOnRouteC3, creditorC3, carrierC3, sendingAgentC3, receivingAgentC3, depCtoOnRouteC3, depCtoC3, depCfsC3, depCfsTransportC3, arrCtoOnRouteC3, arrCtoC3, arrCfsC3, arrCfsTransportC3);

			ass.SetShipmentOrgs(shipment, importBroker, exportBroker, deliveryAgent, pickupAgent, deliveryCartageCo, pickupCartageCo, importCFS, exportCFS, controllingAgent);

			#region FRT

			ass.Assert("FRT", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.receivingAgentC2),
					   new IntOrg(4, Orgs.receivingAgentC3)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3)
				);

			ass.Assert("FRT", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(4, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.sendingAgentC2),
					   new IntOrg(4, Orgs.sendingAgentC3)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.receivingAgentC2),
					   new IntOrg(4, Orgs.receivingAgentC3)
				);

			#endregion

			#region ORG

			ass.Assert("ORG", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCtoC3),

					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsC3),

					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.depCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.receivingAgentC1),
					   new IntOrg(6, Orgs.receivingAgentC2),
					   new IntOrg(6, Orgs.receivingAgentC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("ORG", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC2),
					   new IntOrg(5, Orgs.depCtoOnRouteC3),

					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCtoC2),
					   new IntOrg(6, Orgs.depCtoC3),

					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsC2),
					   new IntOrg(6, Orgs.depCfsC3),

					   new IntOrg(6, Orgs.depCfsTransportC1),
					   new IntOrg(6, Orgs.depCfsTransportC2),
					   new IntOrg(6, Orgs.depCfsTransportC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("ORG", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC2),
					   new IntOrg(5, Orgs.depCtoOnRouteC3),

					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCtoC2),
					   new IntOrg(6, Orgs.depCtoC3),

					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsC2),
					   new IntOrg(6, Orgs.depCfsC3),

					   new IntOrg(6, Orgs.depCfsTransportC1),
					   new IntOrg(6, Orgs.depCfsTransportC2),
					   new IntOrg(6, Orgs.depCfsTransportC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("ORG", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCtoC3),

					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsC3),

					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.depCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.sendingAgentC1),
					   new IntOrg(6, Orgs.sendingAgentC2),
					   new IntOrg(6, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("ORG", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCtoC3),

					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsC3),

					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.depCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("ORG", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(3, Orgs.depCtoC1),
					   new IntOrg(3, Orgs.depCtoC2),
					   new IntOrg(3, Orgs.depCtoC3),

					   new IntOrg(3, Orgs.depCfsC1),
					   new IntOrg(3, Orgs.depCfsC2),
					   new IntOrg(3, Orgs.depCfsC3),

					   new IntOrg(3, Orgs.depCfsTransportC1),
					   new IntOrg(3, Orgs.depCfsTransportC2),
					   new IntOrg(3, Orgs.depCfsTransportC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("ORG", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.depCtoOnRouteC3),

					   new IntOrg(6, Orgs.sendingAgentC1),
					   new IntOrg(6, Orgs.sendingAgentC2),

					   new IntOrg(6, Orgs.depCtoC3),
					   new IntOrg(6, Orgs.depCfsC3),
					   new IntOrg(6, Orgs.depCfsTransportC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			#endregion

			#region DST

			ass.Assert("DST", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC2),
					   new IntOrg(5, Orgs.arrCtoOnRouteC3),

					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCtoC2),
					   new IntOrg(6, Orgs.arrCtoC3),

					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsC2),
					   new IntOrg(6, Orgs.arrCfsC3),

					   new IntOrg(6, Orgs.arrCfsTransportC1),
					   new IntOrg(6, Orgs.arrCfsTransportC2),
					   new IntOrg(6, Orgs.arrCfsTransportC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("DST", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(4, Orgs.arrCtoC3),

					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(4, Orgs.arrCfsC3),

					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(4, Orgs.arrCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.receivingAgentC1),
					   new IntOrg(6, Orgs.receivingAgentC2),
					   new IntOrg(6, Orgs.receivingAgentC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("DST", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(4, Orgs.arrCtoC3),

					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(4, Orgs.arrCfsC3),

					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(4, Orgs.arrCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.sendingAgentC1),
					   new IntOrg(6, Orgs.sendingAgentC2),
					   new IntOrg(6, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("DST", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC2),
					   new IntOrg(5, Orgs.arrCtoOnRouteC3),

					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCtoC2),
					   new IntOrg(6, Orgs.arrCtoC3),

					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsC2),
					   new IntOrg(6, Orgs.arrCfsC3),

					   new IntOrg(6, Orgs.arrCfsTransportC1),
					   new IntOrg(6, Orgs.arrCfsTransportC2),
					   new IntOrg(6, Orgs.arrCfsTransportC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("DST", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(4, Orgs.arrCtoC3),

					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(4, Orgs.arrCfsC3),

					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(4, Orgs.arrCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("DST", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(3, Orgs.arrCtoC1),
					   new IntOrg(3, Orgs.arrCtoC2),
					   new IntOrg(3, Orgs.arrCtoC3),

					   new IntOrg(3, Orgs.arrCfsC1),
					   new IntOrg(3, Orgs.arrCfsC2),
					   new IntOrg(3, Orgs.arrCfsC3),

					   new IntOrg(3, Orgs.arrCfsTransportC1),
					   new IntOrg(3, Orgs.arrCfsTransportC2),
					   new IntOrg(3, Orgs.arrCfsTransportC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("DST", Constants.PaymentType.Prepaid, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(6, Orgs.receivingAgentC2),
					   new IntOrg(6, Orgs.receivingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(4, Orgs.arrCtoC3),

					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(4, Orgs.arrCfsC3),

					   new IntOrg(6, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(4, Orgs.arrCfsTransportC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			#endregion

			#region UNL

			ass.Assert("UNL", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC2),
					   new IntOrg(5, Orgs.arrCtoOnRouteC3),

					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCtoC2),
					   new IntOrg(6, Orgs.arrCtoC3),

					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsC2),
					   new IntOrg(6, Orgs.arrCfsC3),

					   new IntOrg(6, Orgs.arrCfsTransportC1),
					   new IntOrg(6, Orgs.arrCfsTransportC2),
					   new IntOrg(6, Orgs.arrCfsTransportC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("UNL", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(4, Orgs.arrCtoC3),

					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(4, Orgs.arrCfsC3),

					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(4, Orgs.arrCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.receivingAgentC1),
					   new IntOrg(6, Orgs.receivingAgentC2),
					   new IntOrg(6, Orgs.receivingAgentC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("UNL", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(4, Orgs.arrCtoC3),

					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(4, Orgs.arrCfsC3),

					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(4, Orgs.arrCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.sendingAgentC1),
					   new IntOrg(6, Orgs.sendingAgentC2),
					   new IntOrg(6, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("UNL", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC2),
					   new IntOrg(5, Orgs.arrCtoOnRouteC3),

					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCtoC2),
					   new IntOrg(6, Orgs.arrCtoC3),

					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsC2),
					   new IntOrg(6, Orgs.arrCfsC3),

					   new IntOrg(6, Orgs.arrCfsTransportC1),
					   new IntOrg(6, Orgs.arrCfsTransportC2),
					   new IntOrg(6, Orgs.arrCfsTransportC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("UNL", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(4, Orgs.arrCtoC3),

					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(4, Orgs.arrCfsC3),

					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(4, Orgs.arrCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("UNL", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(3, Orgs.arrCtoOnRouteC3),

					   new IntOrg(3, Orgs.arrCtoC1),
					   new IntOrg(3, Orgs.arrCtoC2),
					   new IntOrg(3, Orgs.arrCtoC3),

					   new IntOrg(3, Orgs.arrCfsC1),
					   new IntOrg(3, Orgs.arrCfsC2),
					   new IntOrg(3, Orgs.arrCfsC3),

					   new IntOrg(3, Orgs.arrCfsTransportC1),
					   new IntOrg(3, Orgs.arrCfsTransportC2),
					   new IntOrg(3, Orgs.arrCfsTransportC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("UNL", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC2),
					   new IntOrg(5, Orgs.arrCtoOnRouteC3),

					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCtoC2),
					   new IntOrg(6, Orgs.arrCtoC3),

					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsC2),
					   new IntOrg(6, Orgs.arrCfsC3),

					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.arrCfsTransportC2),
					   new IntOrg(6, Orgs.arrCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(6, Orgs.sendingAgentC1),
					   new IntOrg(6, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.importBroker),
					   new IntOrg(2, Orgs.importBroker),
					   new IntOrg(3, Orgs.importBroker),
					   new IntOrg(4, Orgs.importBroker),
					   new IntOrg(5, Orgs.importBroker),
					   new IntOrg(6, Orgs.importBroker),

					   new IntOrg(1, Orgs.deliveryAgent),
					   new IntOrg(2, Orgs.deliveryAgent),
					   new IntOrg(3, Orgs.deliveryAgent),
					   new IntOrg(4, Orgs.deliveryAgent),
					   new IntOrg(5, Orgs.deliveryAgent),
					   new IntOrg(6, Orgs.deliveryAgent),

					   new IntOrg(1, Orgs.deliveryCartageCo),
					   new IntOrg(2, Orgs.deliveryCartageCo),
					   new IntOrg(3, Orgs.deliveryCartageCo),
					   new IntOrg(4, Orgs.deliveryCartageCo),
					   new IntOrg(5, Orgs.deliveryCartageCo),
					   new IntOrg(6, Orgs.deliveryCartageCo),

					   new IntOrg(1, Orgs.importCFS),
					   new IntOrg(2, Orgs.importCFS),
					   new IntOrg(3, Orgs.importCFS),
					   new IntOrg(4, Orgs.importCFS),
					   new IntOrg(5, Orgs.importCFS),
					   new IntOrg(6, Orgs.importCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			#endregion

			#region LOD

			ass.Assert("LOD", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCtoC3),

					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsC3),

					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.depCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.receivingAgentC1),
					   new IntOrg(6, Orgs.receivingAgentC2),
					   new IntOrg(6, Orgs.receivingAgentC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("LOD", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC2),
					   new IntOrg(5, Orgs.depCtoOnRouteC3),

					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCtoC2),
					   new IntOrg(6, Orgs.depCtoC3),

					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsC2),
					   new IntOrg(6, Orgs.depCfsC3),

					   new IntOrg(6, Orgs.depCfsTransportC1),
					   new IntOrg(6, Orgs.depCfsTransportC2),
					   new IntOrg(6, Orgs.depCfsTransportC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("LOD", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3),

					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC2),
					   new IntOrg(5, Orgs.depCtoOnRouteC3),

					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCtoC2),
					   new IntOrg(6, Orgs.depCtoC3),

					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsC2),
					   new IntOrg(6, Orgs.depCfsC3),

					   new IntOrg(6, Orgs.depCfsTransportC1),
					   new IntOrg(6, Orgs.depCfsTransportC2),
					   new IntOrg(6, Orgs.depCfsTransportC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("LOD", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCtoC3),

					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsC3),

					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.depCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.sendingAgentC1),
					   new IntOrg(6, Orgs.sendingAgentC2),
					   new IntOrg(6, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("LOD", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCtoC3),

					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsC3),

					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.depCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(5, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("LOD", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(3, Orgs.depCtoC1),
					   new IntOrg(3, Orgs.depCtoC2),
					   new IntOrg(3, Orgs.depCtoC3),

					   new IntOrg(3, Orgs.depCfsC1),
					   new IntOrg(3, Orgs.depCfsC2),
					   new IntOrg(3, Orgs.depCfsC3),

					   new IntOrg(3, Orgs.depCfsTransportC1),
					   new IntOrg(3, Orgs.depCfsTransportC2),
					   new IntOrg(3, Orgs.depCfsTransportC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			ass.Assert("LOD", Constants.PaymentType.Collect, Constants.PaymentType.Prepaid, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC2),
					   new IntOrg(3, Orgs.depCtoOnRouteC3),

					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCtoC2),
					   new IntOrg(4, Orgs.depCtoC3),

					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsC2),
					   new IntOrg(4, Orgs.depCfsC3),

					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(6, Orgs.depCfsTransportC2),
					   new IntOrg(4, Orgs.depCfsTransportC3),

					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(5, Orgs.carrierC3),

					   new IntOrg(6, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(6, Orgs.sendingAgentC3),

					   new IntOrg(1, Orgs.exportBroker),
					   new IntOrg(2, Orgs.exportBroker),
					   new IntOrg(3, Orgs.exportBroker),
					   new IntOrg(4, Orgs.exportBroker),
					   new IntOrg(5, Orgs.exportBroker),
					   new IntOrg(6, Orgs.exportBroker),

					   new IntOrg(1, Orgs.pickupAgent),
					   new IntOrg(2, Orgs.pickupAgent),
					   new IntOrg(3, Orgs.pickupAgent),
					   new IntOrg(4, Orgs.pickupAgent),
					   new IntOrg(5, Orgs.pickupAgent),
					   new IntOrg(6, Orgs.pickupAgent),

					   new IntOrg(1, Orgs.pickupCartageCo),
					   new IntOrg(2, Orgs.pickupCartageCo),
					   new IntOrg(3, Orgs.pickupCartageCo),
					   new IntOrg(4, Orgs.pickupCartageCo),
					   new IntOrg(5, Orgs.pickupCartageCo),
					   new IntOrg(6, Orgs.pickupCartageCo),

					   new IntOrg(1, Orgs.exportCFS),
					   new IntOrg(2, Orgs.exportCFS),
					   new IntOrg(3, Orgs.exportCFS),
					   new IntOrg(4, Orgs.exportCFS),
					   new IntOrg(5, Orgs.exportCFS),
					   new IntOrg(6, Orgs.exportCFS),

					   new IntOrg(1, Orgs.controllingAgent),
					   new IntOrg(2, Orgs.controllingAgent),
					   new IntOrg(3, Orgs.controllingAgent),
					   new IntOrg(4, Orgs.controllingAgent),
					   new IntOrg(5, Orgs.controllingAgent),
					   new IntOrg(6, Orgs.controllingAgent)
				);

			#endregion

			#region INS

			ass.Assert("INS", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.receivingAgentC2),
					   new IntOrg(4, Orgs.receivingAgentC3)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3)
				);

			ass.Assert("INS", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(4, Orgs.carrierC3)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3),

					   new IntOrg(4, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.sendingAgentC2),
					   new IntOrg(4, Orgs.sendingAgentC3)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(3, Orgs.sendingAgentC3),

					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC2),
					   new IntOrg(3, Orgs.receivingAgentC3),

					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3)
				);

			ass.Assert("INS", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(1, Orgs.creditorOnRouteC2),
					   new IntOrg(1, Orgs.creditorOnRouteC3),

					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(2, Orgs.creditorC2),
					   new IntOrg(2, Orgs.creditorC3),

					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC2),
					   new IntOrg(4, Orgs.sendingAgentC3),

					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.carrierC2),
					   new IntOrg(3, Orgs.carrierC3)
				);

			#endregion

			#region CDS

			ass.Assert("CDS", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export, new IntOrg(1, Orgs.exportBroker));
			ass.Assert("CDS", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export, new IntOrg(1, Orgs.exportBroker));
			ass.Assert("CDS", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import, new IntOrg(1, Orgs.importBroker));
			ass.Assert("CDS", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import, new IntOrg(1, Orgs.importBroker));
			ass.Assert("CDS", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Domestic, Array.Empty<IntOrg>());
			ass.Assert("CDS", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.CrossTrade, Array.Empty<IntOrg>());

			#endregion

			#region BRK

			ass.Assert("BRK", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Import, new IntOrg(1, Orgs.importBroker));
			ass.Assert("BRK", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Import, new IntOrg(1, Orgs.importBroker));
			ass.Assert("BRK", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Domestic, Array.Empty<IntOrg>());
			ass.Assert("BRK", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.CrossTrade, Array.Empty<IntOrg>());

			#endregion

			#region OBR

			ass.Assert("OBR", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.Export, new IntOrg(1, Orgs.exportBroker));
			ass.Assert("OBR", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Export, new IntOrg(1, Orgs.exportBroker));
			ass.Assert("OBR", Constants.PaymentType.Collect, Constants.PaymentType.Collect, Constants.PaymentType.Collect, Directions.Domestic, Array.Empty<IntOrg>());
			ass.Assert("OBR", Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Constants.PaymentType.Prepaid, Directions.CrossTrade, Array.Empty<IntOrg>());

			#endregion
		}

		public void TestConsolTransportProviders()
		{
			var creditorOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			var arrCto = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfs = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsTransport = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			var depCto = Factory.NewWithValidTestData<OrgHeader>();
			var depCfs = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsTransport = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var ass = new TPAsserter();
			ass.SetC1Orgs(consol, creditorOnRoute, creditor, carrier, sendingAgent, receivingAgent, depCtoOnRoute, depCto, depCfs, depCfsTransport, arrCtoOnRoute, arrCto, arrCfs, arrCfsTransport);

			#region FRT

			ass.Assert("FRT", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.Assert("FRT", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1)
				);

			ass.Assert("FRT", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.carrierC1)
				);

			#endregion

			#region ORG

			ass.Assert("ORG", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.Assert("ORG", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.Assert("ORG", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.Assert("ORG", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.Assert("ORG", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.Assert("ORG", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoC1),
					   new IntOrg(3, Orgs.depCfsC1),
					   new IntOrg(3, Orgs.depCfsTransportC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC1)
				);

			#endregion

			#region DST

			ass.Assert("DST", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.Assert("DST", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.Assert("DST", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.Assert("DST", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.Assert("DST", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.Assert("DST", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoC1),
					   new IntOrg(3, Orgs.arrCfsC1),
					   new IntOrg(3, Orgs.arrCfsTransportC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC1)
				);

			#endregion

			#region UNL

			ass.Assert("UNL", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.Assert("UNL", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.Assert("UNL", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.Assert("UNL", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.Assert("UNL", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.Assert("UNL", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoC1),
					   new IntOrg(3, Orgs.arrCfsC1),
					   new IntOrg(3, Orgs.arrCfsTransportC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC1)
				);

			#endregion

			#region LOD

			ass.Assert("LOD", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.Assert("LOD", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.Assert("LOD", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.Assert("LOD", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.Assert("LOD", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.Assert("LOD", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(3, Orgs.depCtoC1),
					   new IntOrg(3, Orgs.depCfsC1),
					   new IntOrg(3, Orgs.depCfsTransportC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.sendingAgentC1)
				);

			#endregion

			#region INS

			ass.Assert("INS", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.Assert("INS", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1)
				);

			ass.Assert("INS", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(3, Orgs.carrierC1)
				);

			#endregion
		}

		public void TestRatingRouteTransportProviders()
		{
			var creditorOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			var creditor = Factory.NewWithValidTestData<OrgHeader>();
			var arrCtoOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			var arrCto = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfs = Factory.NewWithValidTestData<OrgHeader>();
			var arrCfsTransport = Factory.NewWithValidTestData<OrgHeader>();
			var depCtoOnRoute = Factory.NewWithValidTestData<OrgHeader>();
			var depCto = Factory.NewWithValidTestData<OrgHeader>();
			var depCfs = Factory.NewWithValidTestData<OrgHeader>();
			var depCfsTransport = Factory.NewWithValidTestData<OrgHeader>();
			var carrier = Factory.NewWithValidTestData<OrgHeader>();
			var sendingAgent = Factory.NewWithValidTestData<OrgHeader>();
			var receivingAgent = Factory.NewWithValidTestData<OrgHeader>();

			var consol = Factory.New<CommonConsol>();
			consol.JK_AgentType = Constants.AgentType.CoLoad;

			var ass = new RatingRouteTPAsserter();
			ass.SetC1Orgs(consol, creditorOnRoute, creditor, carrier, sendingAgent, receivingAgent, depCtoOnRoute, depCto, depCfs, depCfsTransport, arrCtoOnRoute, arrCto, arrCfs, arrCfsTransport);

			#region FRT

			ass.AssertSingleRoute("FRT", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1)
				);

			ass.AssertSingleRoute("FRT", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("FRT", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("FRT", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			ass.AssertSingleRoute("FRT", Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("FRT", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion

			#region ORG

			ass.AssertSingleRoute("ORG", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.AssertSingleRoute("ORG", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.AssertSingleRoute("ORG", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.AssertSingleRoute("ORG", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.AssertSingleRoute("ORG", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.depCtoC1),
					   new IntOrg(5, Orgs.depCfsC1),
					   new IntOrg(5, Orgs.depCfsTransportC1),
					   new IntOrg(6, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("ORG", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			#endregion

			#region DST

			ass.AssertSingleRoute("DST", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.AssertSingleRoute("DST", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.AssertSingleRoute("DST", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.AssertSingleRoute("DST", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.AssertSingleRoute("DST", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.arrCtoC1),
					   new IntOrg(5, Orgs.arrCfsC1),
					   new IntOrg(5, Orgs.arrCfsTransportC1),
					   new IntOrg(6, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("DST", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			#endregion

			#region UNL

			ass.AssertSingleRoute("UNL", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.AssertSingleRoute("UNL", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.AssertSingleRoute("UNL", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.AssertSingleRoute("UNL", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1),
					   new IntOrg(6, Orgs.arrCtoC1),
					   new IntOrg(6, Orgs.arrCfsC1),
					   new IntOrg(6, Orgs.arrCfsTransportC1)
				);

			ass.AssertSingleRoute("UNL", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.arrCtoC1),
					   new IntOrg(5, Orgs.arrCfsC1),
					   new IntOrg(5, Orgs.arrCfsTransportC1),
					   new IntOrg(6, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("UNL", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoC1),
					   new IntOrg(4, Orgs.arrCfsC1),
					   new IntOrg(4, Orgs.arrCfsTransportC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			#endregion

			#region LOD

			ass.AssertSingleRoute("LOD", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.receivingAgentC1)
				);

			ass.AssertSingleRoute("LOD", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.AssertSingleRoute("LOD", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1),
					   new IntOrg(6, Orgs.depCtoC1),
					   new IntOrg(6, Orgs.depCfsC1),
					   new IntOrg(6, Orgs.depCfsTransportC1)
				);

			ass.AssertSingleRoute("LOD", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(5, Orgs.carrierC1),
					   new IntOrg(6, Orgs.sendingAgentC1)
				);

			ass.AssertSingleRoute("LOD", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.depCtoC1),
					   new IntOrg(5, Orgs.depCfsC1),
					   new IntOrg(5, Orgs.depCfsTransportC1),
					   new IntOrg(6, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("LOD", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.depCtoC1),
					   new IntOrg(4, Orgs.depCfsC1),
					   new IntOrg(4, Orgs.depCfsTransportC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			#endregion

			#region INS

			ass.AssertSingleRoute("INS", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.receivingAgentC1)
				);

			ass.AssertSingleRoute("INS", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("INS", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("INS", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(2, Orgs.creditorC1),
					   new IntOrg(3, Orgs.carrierC1),
					   new IntOrg(4, Orgs.sendingAgentC1)
				);

			ass.AssertSingleRoute("INS", Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertSingleRoute("INS", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.creditorC1),
					   new IntOrg(4, Orgs.sendingAgentC1),
					   new IntOrg(4, Orgs.receivingAgentC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion

			consol.Transports.AddNew();
			consol.Transports.AddNew();

			#region FRT - Multiple Routes

			ass.AssertMultipleRoutes("FRT", Constants.PaymentType.Prepaid, Directions.Export,
				   new IntOrg(1, Orgs.creditorOnRouteC1),
				   new IntOrg(3, Orgs.carrierC1)
			);

			ass.AssertMultipleRoutes("FRT", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("FRT", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("FRT", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("FRT", Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("FRT", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion

			#region ORG - Multiple Routes

			ass.AssertMultipleRoutes("ORG", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("ORG", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("ORG", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("ORG", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("ORG", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("ORG", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion

			#region DST - Multiple Routes

			ass.AssertMultipleRoutes("DST", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("DST", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("DST", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("DST", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("DST", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("DST", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion

			#region UNL - Multple Routes

			ass.AssertMultipleRoutes("UNL", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("UNL", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("UNL", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("UNL", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.arrCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("UNL", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("UNL", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.arrCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion

			#region LOD - Multiple Routes

			ass.AssertMultipleRoutes("LOD", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("LOD", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("LOD", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1),
					   new IntOrg(5, Orgs.depCtoOnRouteC1)
				);

			ass.AssertMultipleRoutes("LOD", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("LOD", Constants.PaymentType.Prepaid, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(5, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("LOD", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.depCtoOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion

			#region INS - Multiple Routes

			ass.AssertMultipleRoutes("INS", Constants.PaymentType.Prepaid, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("INS", Constants.PaymentType.Collect, Directions.Export,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("INS", Constants.PaymentType.Prepaid, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("INS", Constants.PaymentType.Collect, Directions.Import,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(3, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("INS", Constants.PaymentType.Collect, Directions.Domestic,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			ass.AssertMultipleRoutes("INS", Constants.PaymentType.Collect, Directions.CrossTrade,
					   new IntOrg(1, Orgs.creditorOnRouteC1),
					   new IntOrg(4, Orgs.carrierC1)
				);

			#endregion
		}

		enum Orgs
		{
			importBroker,
			exportBroker,
			deliveryAgent,
			pickupAgent,
			controllingAgent,
			deliveryCartageCo,
			pickupCartageCo,
			importCFS,
			exportCFS,

			creditorOnRouteC1,
			creditorC1,
			arrCtoOnRouteC1,
			arrCtoC1,
			arrCfsC1,
			arrCfsTransportC1,
			depCtoOnRouteC1,
			depCtoC1,
			depCfsC1,
			depCfsTransportC1,
			carrierC1,
			sendingAgentC1,
			receivingAgentC1,
			controllingAgentC1,

			creditorOnRouteC2,
			creditorC2,
			arrCtoOnRouteC2,
			arrCtoC2,
			arrCfsC2,
			arrCfsTransportC2,
			depCtoOnRouteC2,
			depCtoC2,
			depCfsC2,
			depCfsTransportC2,
			carrierC2,
			sendingAgentC2,
			receivingAgentC2,
			controllingAgentC2,

			creditorOnRouteC3,
			creditorC3,
			arrCtoOnRouteC3,
			arrCtoC3,
			arrCfsC3,
			arrCfsTransportC3,
			depCtoOnRouteC3,
			depCtoC3,
			depCfsC3,
			depCfsTransportC3,
			carrierC3,
			sendingAgentC3,
			receivingAgentC3,
			controllingAgentC3
		}

		class IntOrg
		{
			public IntOrg(int priority, Orgs org)
			{
				Int = priority;
				Org = org;
			}

			public int Int { get; private set; }
			public Orgs Org { get; private set; }
		}

		class TPAsserter
		{
			public void Assert(string group, string ppdcltC1, Directions direction, params IntOrg[] tuples)
			{
				consol1.JK_PrepaidCollect = ppdcltC1;

				switch (direction)
				{
					case Directions.Import:
						consol1.JK_RL_NKLoadPort = "USLAX";
						consol1.JK_RL_NKDischargePort = "AUSYD";
						break;
					case Directions.Export:
						consol1.JK_RL_NKLoadPort = "AUSYD";
						consol1.JK_RL_NKDischargePort = "USLAX";
						break;
					case Directions.Domestic:
						consol1.JK_RL_NKLoadPort = "AUBNE";
						consol1.JK_RL_NKDischargePort = "AUSYD";
						break;
					case Directions.CrossTrade:
						consol1.JK_RL_NKLoadPort = "USLAX";
						consol1.JK_RL_NKDischargePort = "NLAMS";
						break;
				}

				SetConsolOrgs(consol1, creditorOnRouteC1, creditorC1, carrierC1, sendingAgentC1, receivingAgentC1, depCtoOnRouteC1, depCtoC1, depCfsC1, depCfsTransportC1, arrCtoOnRouteC1, arrCtoC1, arrCfsC1, arrCfsTransportC1);
				Assert(string.Format("payment type: {0}, direction: {1}", ppdcltC1, direction), group, tuples);
			}

			public void Assert(string group, string ppdcltC1, string ppdcltC2, string ppdcltC3, Directions direction, params IntOrg[] tuples)
			{
				consol1.JK_PrepaidCollect = ppdcltC1;
				consol2.JK_PrepaidCollect = ppdcltC2;
				consol3.JK_PrepaidCollect = ppdcltC3;

				switch (direction)
				{
					case Directions.Import:
						shipment.JS_RL_NKOrigin = "USLAX";
						shipment.JS_RL_NKDestination = "AUSYD";
						break;
					case Directions.Export:
						shipment.JS_RL_NKOrigin = "AUSYD";
						shipment.JS_RL_NKDestination = "USLAX";
						break;
					case Directions.Domestic:
						shipment.JS_RL_NKOrigin = "AUBNE";
						shipment.JS_RL_NKDestination = "AUSYD";
						break;
					case Directions.CrossTrade:
						shipment.JS_RL_NKOrigin = "USLAX";
						shipment.JS_RL_NKDestination = "NLAMS";
						break;
				}

				SetConsolOrgs(consol1, creditorOnRouteC1, creditorC1, carrierC1, sendingAgentC1, receivingAgentC1, depCtoOnRouteC1, depCtoC1, depCfsC1, depCfsTransportC1, arrCtoOnRouteC1, arrCtoC1, arrCfsC1, arrCfsTransportC1);
				SetConsolOrgs(consol2, creditorOnRouteC2, creditorC2, carrierC2, sendingAgentC2, receivingAgentC2, depCtoOnRouteC2, depCtoC2, depCfsC2, depCfsTransportC2, arrCtoOnRouteC2, arrCtoC2, arrCfsC2, arrCfsTransportC2);
				SetConsolOrgs(consol3, creditorOnRouteC3, creditorC3, carrierC3, sendingAgentC3, receivingAgentC3, depCtoOnRouteC3, depCtoC3, depCfsC3, depCfsTransportC3, arrCtoOnRouteC3, arrCtoC3, arrCfsC3, arrCfsTransportC3);

				Assert(string.Format("consol1: {0}, consol2: {1}, consol3: {2}, direction: {3}", ppdcltC1, ppdcltC2, ppdcltC3, direction), group, tuples);
			}

			void Assert(string message, string group, params IntOrg[] tuples)
			{
				var dic = new Dictionary<OrgHeader, List<int>>();

				foreach (var tuple in tuples)
				{
					List<int> list;
					OrgHeader orgByType = GetOrgByType(tuple.Org);

					if (dic.TryGetValue(orgByType, out list))
					{
						list.Add(tuple.Int);
					}
					else
					{
						dic[orgByType] = new List<int> { tuple.Int };
					}
				}

				var providers = shipment != null ? shipment.GetCreditors() : consol1.GetCreditors();

				CombineAssertions(string.Format("{0} group, {1}", group, message),
								  () =>
								  {
									  foreach (Orgs orgType in Enum.GetValues(typeof(Orgs)))
									  {
										  OrgHeader org = GetOrgByType(orgType);
										  List<int> expected;
										  if (org == null || !dic.TryGetValue(org, out expected))
										  {
											  expected = new List<int>(0);
										  }

										  AssertContainsExactElementsInAnyOrder(orgType.ToString(), expected, providers.GetRank(group, OrgWithSource.New(org, new List<string>() { "Provider" })));
									  }
								  });
			}

			public void SetShipmentOrgs(CommonShipment shipment, OrgHeader importBroker, OrgHeader exportBroker, OrgHeader deliveryAgent, OrgHeader pickupAgent, OrgHeader deliveryCartageCo, OrgHeader pickupCartageCo, OrgHeader importCFS, OrgHeader exportCFS, OrgHeader controllingAgent)
			{
				this.exportBroker = exportBroker;
				this.importBroker = importBroker;
				this.deliveryAgent = deliveryAgent;
				this.pickupAgent = pickupAgent;
				this.controllingAgent = controllingAgent;
				this.deliveryCartageCo = deliveryCartageCo;
				this.pickupCartageCo = pickupCartageCo;
				this.importCFS = importCFS;
				this.exportCFS = exportCFS;

				this.shipment = shipment;

				shipment.JS_OH_ExportBroker = this.exportBroker.PK;
				shipment.JS_OH_ImportBroker = this.importBroker.PK;
				shipment.DocsAndCartage.JP_OA_PickupCartageCoAddr = this.pickupCartageCo.MainAddress.PK;
				shipment.DocsAndCartage.JP_OA_DeliveryCartageCoAddr = this.deliveryCartageCo.MainAddress.PK;
				shipment.JS_OH_DeliveryAgent = this.deliveryAgent.PK;
				shipment.PickupAgentDocumentaryAddress.OrganisationPK = this.pickupAgent.PK;
				shipment.JS_OA_ImportReleaseDepot = importCFS.MainAddress.PK;
				shipment.JS_OA_ExportReceivingDepot = exportCFS.MainAddress.PK;
				var controllingAgentAddress = shipment.DocAddresses.AddNew(DocAddressType.ControllingAgent);
				controllingAgentAddress.OrganisationPK = controllingAgent.PK;
			}

			public void SetC1Orgs(CommonConsol c1, OrgHeader creditorOnRoute, OrgHeader creditor, OrgHeader carrier, OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader depCtoOnRoute, OrgHeader depCto, OrgHeader depCfs, OrgHeader depCfsTransport, OrgHeader arrCtoOnRoute, OrgHeader arrCto, OrgHeader arrCfs, OrgHeader arrCfsTransport)
			{
				consol1 = c1;

				creditorOnRouteC1 = creditorOnRoute;
				creditorC1 = creditor;
				arrCtoOnRouteC1 = arrCtoOnRoute;
				arrCtoC1 = arrCto;
				arrCfsC1 = arrCfs;
				arrCfsTransportC1 = arrCfsTransport;
				depCtoOnRouteC1 = depCtoOnRoute;
				depCtoC1 = depCto;
				depCfsC1 = depCfs;
				depCfsTransportC1 = depCfsTransport;
				carrierC1 = carrier;
				sendingAgentC1 = sendingAgent;
				receivingAgentC1 = receivingAgent;
			}

			public void SetC2Orgs(CommonConsol c2, OrgHeader creditorOnRoute, OrgHeader creditor, OrgHeader carrier, OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader depCtoOnRoute, OrgHeader depCto, OrgHeader depCfs, OrgHeader depCfsTransport, OrgHeader arrCtoOnRoute, OrgHeader arrCto, OrgHeader arrCfs, OrgHeader arrCfsTransport)
			{
				consol2 = c2;

				creditorOnRouteC2 = creditorOnRoute;
				creditorC2 = creditor;
				arrCtoOnRouteC2 = arrCtoOnRoute;
				arrCtoC2 = arrCto;
				arrCfsC2 = arrCfs;
				arrCfsTransportC2 = arrCfsTransport;
				depCtoOnRouteC2 = depCtoOnRoute;
				depCtoC2 = depCto;
				depCfsC2 = depCfs;
				depCfsTransportC2 = depCfsTransport;
				carrierC2 = carrier;
				sendingAgentC2 = sendingAgent;
				receivingAgentC2 = receivingAgent;
			}

			public void SetC3Orgs(CommonConsol c3, OrgHeader creditorOnRoute, OrgHeader creditor, OrgHeader carrier, OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader depCtoOnRoute, OrgHeader depCto, OrgHeader depCfs, OrgHeader depCfsTransport, OrgHeader arrCtoOnRoute, OrgHeader arrCto, OrgHeader arrCfs, OrgHeader arrCfsTransport)
			{
				consol3 = c3;

				creditorOnRouteC3 = creditorOnRoute;
				creditorC3 = creditor;
				arrCtoOnRouteC3 = arrCtoOnRoute;
				arrCtoC3 = arrCto;
				arrCfsC3 = arrCfs;
				arrCfsTransportC3 = arrCfsTransport;
				depCtoOnRouteC3 = depCtoOnRoute;
				depCtoC3 = depCto;
				depCfsC3 = depCfs;
				depCfsTransportC3 = depCfsTransport;
				carrierC3 = carrier;
				sendingAgentC3 = sendingAgent;
				receivingAgentC3 = receivingAgent;
			}

			static void SetConsolOrgs(CommonConsol consol, OrgHeader creditorOnRoute, OrgHeader creditor, OrgHeader carrier, OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader depCtoOnRoute, OrgHeader depCto, OrgHeader depCfs, OrgHeader depCfsTransport, OrgHeader arrCtoOnRoute, OrgHeader arrCto, OrgHeader arrCfs, OrgHeader arrCfsTransport)
			{
				consol.SetDefaultShippingLineAddress(carrier);
				consol.SetDefaultSendingForwarderAddress(sendingAgent);
				consol.SetDefaultReceivingForwarderAddress(receivingAgent);
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;

				consol.Transports.MostInterestingTransport.JW_OA_ArrivalLocation = arrCtoOnRoute.MainAddress.PK;
				consol.JK_OA_ArrivalCTOAddress = arrCto.MainAddress.PK;
				consol.JK_OA_UnpackDepotAddress = arrCfs.MainAddress.PK;
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrCfsTransport.MainAddress.PK;

				consol.Transports.MostInterestingTransport.JW_OA_DepartureLocation = depCtoOnRoute.MainAddress.PK;
				consol.JK_OA_DepartureCTOAddress = depCto.MainAddress.PK;
				consol.JK_OA_PackDepotAddress = depCfs.MainAddress.PK;
				consol.JK_OA_DeparturePackCFSTransportAddress = depCfsTransport.MainAddress.PK;

				consol.Transports.MostInterestingTransport.CreditorPK = creditorOnRoute.PK;
				consol.Transports.MostInterestingTransport.JW_OA_DepartureLocation = depCtoOnRoute.MainAddress.PK;
			}

			OrgHeader GetOrgByType(Orgs orgType)
			{
				switch (orgType)
				{
					case Orgs.importBroker:
						return importBroker;
					case Orgs.exportBroker:
						return exportBroker;
					case Orgs.deliveryAgent:
						return deliveryAgent;
					case Orgs.pickupAgent:
						return pickupAgent;
					case Orgs.controllingAgent:
						return controllingAgent;
					case Orgs.deliveryCartageCo:
						return deliveryCartageCo;
					case Orgs.pickupCartageCo:
						return pickupCartageCo;
					case Orgs.importCFS:
						return importCFS;
					case Orgs.exportCFS:
						return exportCFS;

					case Orgs.creditorOnRouteC1:
						return creditorOnRouteC1;
					case Orgs.creditorC1:
						return creditorC1;
					case Orgs.arrCtoOnRouteC1:
						return arrCtoOnRouteC1;
					case Orgs.arrCtoC1:
						return arrCtoC1;
					case Orgs.arrCfsC1:
						return arrCfsC1;
					case Orgs.arrCfsTransportC1:
						return arrCfsTransportC1;
					case Orgs.depCtoOnRouteC1:
						return depCtoOnRouteC1;
					case Orgs.depCtoC1:
						return depCtoC1;
					case Orgs.depCfsC1:
						return depCfsC1;
					case Orgs.depCfsTransportC1:
						return depCfsTransportC1;
					case Orgs.carrierC1:
						return carrierC1;
					case Orgs.sendingAgentC1:
						return sendingAgentC1;
					case Orgs.receivingAgentC1:
						return receivingAgentC1;

					case Orgs.creditorOnRouteC2:
						return creditorOnRouteC2;
					case Orgs.creditorC2:
						return creditorC2;
					case Orgs.arrCtoOnRouteC2:
						return arrCtoOnRouteC2;
					case Orgs.arrCtoC2:
						return arrCtoC2;
					case Orgs.arrCfsC2:
						return arrCfsC2;
					case Orgs.arrCfsTransportC2:
						return arrCfsTransportC2;
					case Orgs.depCtoOnRouteC2:
						return depCtoOnRouteC2;
					case Orgs.depCtoC2:
						return depCtoC2;
					case Orgs.depCfsC2:
						return depCfsC2;
					case Orgs.depCfsTransportC2:
						return depCfsTransportC2;
					case Orgs.carrierC2:
						return carrierC2;
					case Orgs.sendingAgentC2:
						return sendingAgentC2;
					case Orgs.receivingAgentC2:
						return receivingAgentC2;

					case Orgs.creditorOnRouteC3:
						return creditorOnRouteC3;
					case Orgs.creditorC3:
						return creditorC3;
					case Orgs.arrCtoOnRouteC3:
						return arrCtoOnRouteC3;
					case Orgs.arrCtoC3:
						return arrCtoC3;
					case Orgs.arrCfsC3:
						return arrCfsC3;
					case Orgs.arrCfsTransportC3:
						return arrCfsTransportC3;
					case Orgs.depCtoOnRouteC3:
						return depCtoOnRouteC3;
					case Orgs.depCtoC3:
						return depCtoC3;
					case Orgs.depCfsC3:
						return depCfsC3;
					case Orgs.depCfsTransportC3:
						return depCfsTransportC3;
					case Orgs.carrierC3:
						return carrierC3;
					case Orgs.sendingAgentC3:
						return sendingAgentC3;
					case Orgs.receivingAgentC3:
						return receivingAgentC3;
				}

				return null;
			}

			CommonConsol consol1;
			CommonConsol consol2;
			CommonConsol consol3;

			CommonShipment shipment;

			OrgHeader creditorOnRouteC1;
			OrgHeader creditorC1;
			OrgHeader arrCtoOnRouteC1;
			OrgHeader arrCtoC1;
			OrgHeader arrCfsC1;
			OrgHeader arrCfsTransportC1;
			OrgHeader depCtoOnRouteC1;
			OrgHeader depCtoC1;
			OrgHeader depCfsC1;
			OrgHeader depCfsTransportC1;
			OrgHeader carrierC1;
			OrgHeader sendingAgentC1;
			OrgHeader receivingAgentC1;

			OrgHeader creditorOnRouteC2;
			OrgHeader creditorC2;
			OrgHeader arrCtoOnRouteC2;
			OrgHeader arrCtoC2;
			OrgHeader arrCfsC2;
			OrgHeader arrCfsTransportC2;
			OrgHeader depCtoOnRouteC2;
			OrgHeader depCtoC2;
			OrgHeader depCfsC2;
			OrgHeader depCfsTransportC2;
			OrgHeader carrierC2;
			OrgHeader sendingAgentC2;
			OrgHeader receivingAgentC2;

			OrgHeader creditorOnRouteC3;
			OrgHeader creditorC3;
			OrgHeader arrCtoOnRouteC3;
			OrgHeader arrCtoC3;
			OrgHeader arrCfsC3;
			OrgHeader arrCfsTransportC3;
			OrgHeader depCtoOnRouteC3;
			OrgHeader depCtoC3;
			OrgHeader depCfsC3;
			OrgHeader depCfsTransportC3;
			OrgHeader carrierC3;
			OrgHeader sendingAgentC3;
			OrgHeader receivingAgentC3;

			OrgHeader importBroker;
			OrgHeader exportBroker;
			OrgHeader deliveryAgent;
			OrgHeader pickupAgent;
			OrgHeader controllingAgent;
			OrgHeader deliveryCartageCo;
			OrgHeader pickupCartageCo;
			OrgHeader importCFS;
			OrgHeader exportCFS;
		}

		class RatingRouteTPAsserter
		{
			public void AssertSingleRoute(string group, string ppdcltC1, Directions direction, params IntOrg[] tuples)
			{
				consol1.JK_PrepaidCollect = ppdcltC1;

				switch (direction)
				{
					case Directions.Import:
						consol1.JK_RL_NKLoadPort = "USLAX";
						consol1.JK_RL_NKDischargePort = "AUSYD";
						break;
					case Directions.Export:
						consol1.JK_RL_NKLoadPort = "AUSYD";
						consol1.JK_RL_NKDischargePort = "USLAX";
						break;
					case Directions.Domestic:
						consol1.JK_RL_NKLoadPort = "AUBNE";
						consol1.JK_RL_NKDischargePort = "AUSYD";
						break;
					case Directions.CrossTrade:
						consol1.JK_RL_NKLoadPort = "USLAX";
						consol1.JK_RL_NKDischargePort = "NLAMS";
						break;
				}

				SetConsolAndRouteOrgs(consol1, creditorOnRouteC1, creditorC1, carrierC1, sendingAgentC1, receivingAgentC1, depCtoOnRouteC1, depCtoC1, depCfsC1, depCfsTransportC1, arrCtoOnRouteC1, arrCtoC1, arrCfsC1, arrCfsTransportC1);
				Assert(string.Format("payment type: {0}, direction: {1}", ppdcltC1, direction), group, tuples);
			}

			public void AssertMultipleRoutes(string group, string ppdcltC1, Directions direction, params IntOrg[] tuples)
			{
				consol1.JK_PrepaidCollect = ppdcltC1;
				var t1 = consol1.Transports[0];
				var t2 = consol1.Transports.Count > 1 ? consol1.Transports[1] : consol1.Transports.AddNew();
				var t3 = consol1.Transports.Count > 2 ? consol1.Transports[2] : consol1.Transports.AddNew();

				switch (direction)
				{
					case Directions.Import:
						consol1.JK_RL_NKLoadPort = "USLAX";
						consol1.JK_RL_NKDischargePort = "AUSYD";
						t1.JW_VoyageFlight = "123R";
						t1.JW_RL_NKLoadPort = "USLAX";
						t1.JW_RL_NKDiscPort = "USCHI";
						t1.JW_TransportMode = Constants.TransportModes.Road;
						t1.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
						t2.JW_VoyageFlight = "123S";
						t2.JW_RL_NKLoadPort = "USCHI";
						t2.JW_RL_NKDiscPort = "AUMEL";
						t2.JW_TransportMode = Constants.TransportModes.Sea;
						t2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
						t3.JW_VoyageFlight = "123S";
						t3.JW_RL_NKLoadPort = "AUMEL";
						t3.JW_RL_NKDiscPort = "AUSYD";
						t3.JW_TransportMode = Constants.TransportModes.Sea;
						t3.JW_TransportType = Constants.TransportPlanningType.Other;
						break;
					case Directions.Export:
						consol1.JK_RL_NKLoadPort = "AUSYD";
						consol1.JK_RL_NKDischargePort = "USLAX";
						t1.JW_VoyageFlight = "123R";
						t1.JW_RL_NKLoadPort = "AUSYD";
						t1.JW_RL_NKDiscPort = "AUMEL";
						t1.JW_TransportMode = Constants.TransportModes.Road;
						t1.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
						t2.JW_VoyageFlight = "123S";
						t2.JW_RL_NKLoadPort = "AUMEL";
						t2.JW_RL_NKDiscPort = "USCHI";
						t2.JW_TransportMode = Constants.TransportModes.Sea;
						t2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
						t3.JW_VoyageFlight = "123S";
						t3.JW_RL_NKLoadPort = "USCHI";
						t3.JW_RL_NKDiscPort = "USLAX";
						t3.JW_TransportMode = Constants.TransportModes.Sea;
						t3.JW_TransportType = Constants.TransportPlanningType.Other;
						break;
					case Directions.Domestic:
						consol1.JK_RL_NKLoadPort = "AUBNE";
						consol1.JK_RL_NKDischargePort = "AUSYD";
						t1.JW_VoyageFlight = "123R";
						t1.JW_RL_NKLoadPort = "AUBNE";
						t1.JW_RL_NKDiscPort = "AUMEL";
						t1.JW_TransportMode = Constants.TransportModes.Road;
						t1.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
						t2.JW_VoyageFlight = "123S";
						t2.JW_RL_NKLoadPort = "AUMEL";
						t2.JW_RL_NKDiscPort = "AUSYD";
						t2.JW_TransportMode = Constants.TransportModes.Sea;
						t2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
						t3.JW_VoyageFlight = "123S";
						t3.JW_RL_NKLoadPort = "AUSYD";
						t3.JW_RL_NKDiscPort = "AUFRE";
						t3.JW_TransportMode = Constants.TransportModes.Sea;
						t3.JW_TransportType = Constants.TransportPlanningType.Other;
						break;
					case Directions.CrossTrade:
						consol1.JK_RL_NKLoadPort = "USLAX";
						consol1.JK_RL_NKDischargePort = "NLAMS";
						t1.JW_VoyageFlight = "123R";
						t1.JW_RL_NKLoadPort = "USLAX";
						t1.JW_RL_NKDiscPort = "USCHI";
						t1.JW_TransportMode = Constants.TransportModes.Road;
						t1.JW_TransportType = Constants.TransportPlanningType.PreCarriage;
						t2.JW_VoyageFlight = "123S";
						t2.JW_RL_NKLoadPort = "USCHI";
						t2.JW_RL_NKDiscPort = "DEBRE";
						t2.JW_TransportMode = Constants.TransportModes.Sea;
						t2.JW_TransportType = Constants.TransportPlanningType.MainVessel;
						t3.JW_VoyageFlight = "123S";
						t3.JW_RL_NKLoadPort = "DEBRE";
						t3.JW_RL_NKDiscPort = "NLAMS";
						t3.JW_TransportMode = Constants.TransportModes.Sea;
						t3.JW_TransportType = Constants.TransportPlanningType.Other;
						break;
				}

				SetConsolAndMultipleRoutes(consol1, creditorOnRouteC1, creditorC1, carrierC1, sendingAgentC1, receivingAgentC1, depCtoC1, depCfsC1, depCfsTransportC1, arrCtoC1, arrCfsC1, arrCfsTransportC1);

				t2.JW_OA_ArrivalLocation = arrCtoOnRouteC1.MainAddress.PK;
				t2.JW_OA_DepartureLocation = depCtoOnRouteC1.MainAddress.PK;
				t2.JW_OA_CarrierAddress = carrierC1.MainAddress.PK;
				t3.JW_OA_CarrierAddress = carrierC1.MainAddress.PK;
				t2.CreditorPK = creditorOnRouteC1.PK;
				t3.CreditorPK = creditorOnRouteC1.PK;
				t1.JW_CarrierBookingReference = "";
				t2.JW_CarrierBookingReference = "0001";
				t3.JW_CarrierBookingReference = "0001";

				Assert(string.Format("payment type: {0}, direction: {1}", ppdcltC1, direction), group, tuples);
			}

			void Assert(string message, string group, params IntOrg[] tuples)
			{
				var dic = new Dictionary<OrgHeader, List<int>>();

				foreach (var tuple in tuples)
				{
					List<int> list;
					OrgHeader orgByType = GetOrgByType(tuple.Org);

					if (dic.TryGetValue(orgByType, out list))
					{
						list.Add(tuple.Int);
					}
					else
					{
						dic[orgByType] = new List<int> { tuple.Int };
					}
				}

				using (RatingDataRegistry.Instance.MultiModalRatingCost.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var providers = consol1.GetRatingRoutes(CostSell.Cost).Last().GetCreditors();

					CombineAssertions(string.Format("{0} group, {1}", group, message),
									  () =>
									  {
										  foreach (Orgs orgType in Enum.GetValues(typeof(Orgs)))
										  {
											  OrgHeader org = GetOrgByType(orgType);
											  List<int> expected;
											  if (org == null || !dic.TryGetValue(org, out expected))
											  {
												  expected = new List<int>(0);
											  }

											  AssertContainsExactElementsInAnyOrder(orgType.ToString(), expected, providers.GetRank(group, OrgWithSource.New(org, new List<string>() { "Provider" })));
										  }
									  });
				}
			}

			public void SetC1Orgs(CommonConsol c1, OrgHeader creditorOnRoute, OrgHeader creditor, OrgHeader carrier, OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader depCtoOnRoute, OrgHeader depCto, OrgHeader depCfs, OrgHeader depCfsTransport, OrgHeader arrCtoOnRoute, OrgHeader arrCto, OrgHeader arrCfs, OrgHeader arrCfsTransport)
			{
				consol1 = c1;

				creditorOnRouteC1 = creditorOnRoute;
				creditorC1 = creditor;
				arrCtoOnRouteC1 = arrCtoOnRoute;
				arrCtoC1 = arrCto;
				arrCfsC1 = arrCfs;
				arrCfsTransportC1 = arrCfsTransport;
				depCtoOnRouteC1 = depCtoOnRoute;
				depCtoC1 = depCto;
				depCfsC1 = depCfs;
				depCfsTransportC1 = depCfsTransport;
				carrierC1 = carrier;
				sendingAgentC1 = sendingAgent;
				receivingAgentC1 = receivingAgent;
			}

			static void SetConsolAndRouteOrgs(CommonConsol consol, OrgHeader creditorOnRoute, OrgHeader creditor, OrgHeader carrier, OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader depCtoOnRoute, OrgHeader depCto, OrgHeader depCfs, OrgHeader depCfsTransport, OrgHeader arrCtoOnRoute, OrgHeader arrCto, OrgHeader arrCfs, OrgHeader arrCfsTransport)
			{
				consol.SetDefaultShippingLineAddress(carrier);
				consol.SetDefaultSendingForwarderAddress(sendingAgent);
				consol.SetDefaultReceivingForwarderAddress(receivingAgent);

				var leg = consol.Transports[0];

				leg.JW_OA_ArrivalLocation = arrCtoOnRoute.MainAddress.PK;
				consol.JK_OA_ArrivalCTOAddress = arrCto.MainAddress.PK;
				consol.JK_OA_UnpackDepotAddress = arrCfs.MainAddress.PK;
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrCfsTransport.MainAddress.PK;

				leg.JW_OA_DepartureLocation = depCtoOnRoute.MainAddress.PK;
				consol.JK_OA_DepartureCTOAddress = depCto.MainAddress.PK;
				consol.JK_OA_PackDepotAddress = depCfs.MainAddress.PK;
				consol.JK_OA_DeparturePackCFSTransportAddress = depCfsTransport.MainAddress.PK;

				leg.JW_OA_CarrierAddress = carrier.MainAddress.PK;
				leg.CreditorPK = creditorOnRoute.PK;

				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			}

			static void SetConsolAndMultipleRoutes(CommonConsol consol, OrgHeader creditorOnRoute, OrgHeader creditor, OrgHeader carrier, OrgHeader sendingAgent, OrgHeader receivingAgent, OrgHeader depCto, OrgHeader depCfs, OrgHeader depCfsTransport, OrgHeader arrCto, OrgHeader arrCfs, OrgHeader arrCfsTransport)
			{
				consol.SetDefaultShippingLineAddress(carrier);
				consol.SetDefaultSendingForwarderAddress(sendingAgent);
				consol.SetDefaultReceivingForwarderAddress(receivingAgent);

				consol.JK_OA_ArrivalCTOAddress = arrCto.MainAddress.PK;
				consol.JK_OA_UnpackDepotAddress = arrCfs.MainAddress.PK;
				consol.JK_OA_ArrivalUnpackCFSTransportAddress = arrCfsTransport.MainAddress.PK;
				consol.JK_OA_DepartureCTOAddress = depCto.MainAddress.PK;
				consol.JK_OA_PackDepotAddress = depCfs.MainAddress.PK;
				consol.JK_OA_DeparturePackCFSTransportAddress = depCfsTransport.MainAddress.PK;
				consol.JK_OA_CreditorAddress = creditor.MainAddress.PK;
			}

			OrgHeader GetOrgByType(Orgs orgType)
			{
				switch (orgType)
				{
					case Orgs.creditorOnRouteC1:
						return creditorOnRouteC1;
					case Orgs.creditorC1:
						return creditorC1;
					case Orgs.arrCtoOnRouteC1:
						return arrCtoOnRouteC1;
					case Orgs.arrCtoC1:
						return arrCtoC1;
					case Orgs.arrCfsC1:
						return arrCfsC1;
					case Orgs.arrCfsTransportC1:
						return arrCfsTransportC1;
					case Orgs.depCtoOnRouteC1:
						return depCtoOnRouteC1;
					case Orgs.depCtoC1:
						return depCtoC1;
					case Orgs.depCfsC1:
						return depCfsC1;
					case Orgs.depCfsTransportC1:
						return depCfsTransportC1;
					case Orgs.carrierC1:
						return carrierC1;
					case Orgs.sendingAgentC1:
						return sendingAgentC1;
					case Orgs.receivingAgentC1:
						return receivingAgentC1;
				}

				return null;
			}

			CommonConsol consol1;

			OrgHeader creditorOnRouteC1;
			OrgHeader creditorC1;
			OrgHeader arrCtoOnRouteC1;
			OrgHeader arrCtoC1;
			OrgHeader arrCfsC1;
			OrgHeader arrCfsTransportC1;
			OrgHeader depCtoOnRouteC1;
			OrgHeader depCtoC1;
			OrgHeader depCfsC1;
			OrgHeader depCfsTransportC1;
			OrgHeader carrierC1;
			OrgHeader sendingAgentC1;
			OrgHeader receivingAgentC1;
		}
	}
}
