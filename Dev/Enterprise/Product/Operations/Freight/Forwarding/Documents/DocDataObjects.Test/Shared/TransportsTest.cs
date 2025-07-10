using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Freight.Forwarding.Documents.Testing
{
	[TestedType(typeof(Transports))]
	sealed class TransportsTest : NonPersistentBusinessObjectTestCase
	{
		#region TestMain

		public void TestMain()
		{
			var transports = CreateTransports();

			AssertEquals("AUSYD", transports.Main.PortOfLoading.Code);
			AssertEquals("NZAKL", transports.Main.PortOfDischarge.Code);
		}

		public void TestMain_DirectShipmentWithPreCarriageAGTConsol()
		{
			var directShipment = Factory.New<ForwardingShipment>();

			var directConsol = directShipment.Consols.AddNew();
			directConsol.JK_RL_NKLoadPort = "USLAX";
			directConsol.JK_RL_NKDischargePort = "SGSIN";
			directConsol.JK_TransportMode = TransportModes.Sea;
			directConsol.JK_AgentType = AgentType.Direct;

			var agentConsol = directShipment.Consols.AddNew();
			agentConsol.JK_RL_NKLoadPort = "USCHI";
			agentConsol.JK_RL_NKDischargePort = "USLAX";
			agentConsol.JK_TransportMode = TransportModes.Sea;
			agentConsol.JK_AgentType = AgentType.Agent;

			AssertEquals("Pre-condition", 1, directConsol.Transports.Count);
			AssertEquals("Pre-condition", 1, agentConsol.Transports.Count);

			var transports = Transports.Create(Context, new[]
			{
				agentConsol.Transports[0],
				directConsol.Transports[0]
			}, directShipment);

			AssertEquals("USLAX", transports.Main.PortOfLoading.Code);
			AssertEquals("SGSIN", transports.Main.PortOfDischarge.Code);
		}

		#endregion

		#region TestCollection

		public void TestCollection()
		{
			var transports = CreateTransports();

			AssertArrayEqualsByElements(new[]
			{
				"AUBNE|AUSYD",
				"AUSYD|NZAKL",
				"NZAKL|NZCHC"
			},
			transports.Select(t => $"{t.PortOfLoading.Code}|{t.PortOfDischarge.Code}").ToArray());
		}

		#endregion

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return Transports.Create(new CommonContext(Factory), new List<Freight.Business.Transport>());
		}

		Transports CreateTransports()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_RL_NKLoadPort = "AUBNE";
			consol.JK_RL_NKDischargePort = "NZCHC";

			var transport1 = (Freight.Business.Transport)consol.Transports.First();
			transport1.JW_TransportMode = TransportModes.Road;
			transport1.JW_TransportType = TransportPlanningType.PreCarriage;
			transport1.JW_RL_NKLoadPort = "AUBNE";
			transport1.JW_RL_NKDiscPort = "AUSYD";

			var transport2 = consol.Transports.AddNew();
			transport2.JW_TransportMode = TransportModes.Sea;
			transport2.JW_TransportType = TransportPlanningType.MainVessel;
			transport2.JW_RL_NKLoadPort = "AUSYD";
			transport2.JW_RL_NKDiscPort = "NZAKL";

			var transport3 = consol.Transports.AddNew();
			transport3.JW_TransportMode = TransportModes.Rail;
			transport3.JW_TransportType = TransportPlanningType.OnForwarding;
			transport3.JW_RL_NKLoadPort = "NZAKL";
			transport3.JW_RL_NKDiscPort = "NZCHC";

			return Transports.Create(Context, new[]
			{
				transport1,
				transport2,
				transport3
			});
		}

		IContext Context => context ?? (context = new CommonContext(Factory.GetCachedReadOnlyFactory()));
		IContext context;

		#endregion
	}
}
