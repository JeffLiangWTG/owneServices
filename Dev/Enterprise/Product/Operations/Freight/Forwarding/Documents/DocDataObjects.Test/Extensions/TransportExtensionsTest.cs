using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	sealed class TransportExtensionsTest : TestCaseWithFactory
	{
		public void TestIsLegFromNonDirectConsolAttachedToDirectShipment()
		{
			var today = ZDateTime.Today;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_ShipmentType = Constants.ShipmentTypes.StandardHouse;
			shipment.JS_RL_NKOrigin = "USLAX";
			shipment.JS_RL_NKDestination = "SGSIN";

			var directConsol = shipment.Consols.AddNew();
			directConsol.JK_RL_NKLoadPort = "USLAX";
			directConsol.JK_RL_NKLoadPort = "SGSIN";
			directConsol.JK_AgentType = Constants.AgentType.Direct;

			var agentConsol = shipment.Consols.AddNew();
			agentConsol.JK_RL_NKLoadPort = "USCHI";
			agentConsol.JK_RL_NKLoadPort = "USLAX";
			agentConsol.JK_AgentType = Constants.AgentType.Agent;

			AssertEquals("Pre-condition", 1, directConsol.Transports.Count);
			AssertEquals("Pre-condition", 1, agentConsol.Transports.Count);

			Assert(!directConsol.Transports[0].IsLegFromNonDirectConsolAttachedToDirectShipment(shipment));
			Assert(agentConsol.Transports[0].IsLegFromNonDirectConsolAttachedToDirectShipment(shipment));
		}
	}
}
