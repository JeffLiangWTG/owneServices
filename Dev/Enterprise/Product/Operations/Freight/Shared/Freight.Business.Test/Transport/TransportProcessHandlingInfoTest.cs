using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business.Testing
{
	sealed class TransportProcessHandlingInfoTest : TestCaseWithFactory
	{
		public void TestArvEventShouldNotPropagateToConsolLevelGivenThatFirstTransportLegReceiveArrivalEventAndNotAllTransportLegHaveArrivalEvent()
		{
			//Given : all transport do not have ARV event
			var consol = Factory.New<CommonConsol>();
			var firstTransport = consol.Transports[0];
			firstTransport.JW_LegOrder = 1;
			firstTransport.JW_RL_NKDiscPort = "AUMEL";
			firstTransport.JW_RL_NKLoadPort = "AUSYD";
			firstTransport.JW_TransportMode = Constants.TransportModes.Air;

			var secondTransport = consol.Transports.AddNew();
			secondTransport.JW_LegOrder = 2;
			secondTransport.JW_RL_NKLoadPort = "AUMEL";
			secondTransport.JW_RL_NKDiscPort = "DEFRA";
			secondTransport.JW_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			Assert("Precondition: Consol should not have Arrival event", !consol.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode));

			//When: ARV event is linked to first transport
			firstTransport.Logs.AddNew(Events.Arrival, ZDateTimeOffset.Empty, false);

			//Expect: ARV event should not propagate to consol level
			Assert("ARV event should not propagate to consol level", !consol.Logs.HasLogWith(StmALogSchema.SL_SE_NKEvent, Events.ArrivalCode));
		}

		public void TestPreviousJW_ETAForLinkedConsolLeg()
		{
			var consol = Factory.New<CommonConsol>();
			var consolTransport = consol.Transports[0];
			consolTransport.JW_TransportMode = Constants.TransportModes.Sea;
			consolTransport.JW_Vessel = "BUNGA DELIMA";
			consolTransport.JW_VoyageFlight = "4544";
			consolTransport.JW_RL_NKLoadPort = "AUSYD";
			consolTransport.JW_ETD = new ZDateTime(2012, 3, 4);
			consolTransport.JW_RL_NKDiscPort = "NZAKL";
			consolTransport.JW_ETA = new ZDateTime(2012, 4, 5);
			consolTransport.JW_IsLinked = true;
			consol.Factory.Save();
			AssertEquals(ZDateTime.Empty, consolTransport.JW_ATA);
			AssertEquals(ZDateTime.Empty, consolTransport.PreviousJW_ATA);
			var newDate = ZDateTime.Now;
			consolTransport.JW_ATA = newDate;
			consol.Factory.Save();
			AssertEquals(newDate, consolTransport.JW_ATA);
			AssertEquals(newDate, consolTransport.PreviousJW_ATA);
		}

		public void TestFireWorkflow_ShouldNotThrowAnException()
		{
			var consol = Factory.New<CommonConsol>();
			consol.JK_TransportMode = "SEA";
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "NZAKL";

			var consolTransport = consol.Transports[0];
			consolTransport.JW_TransportMode = Constants.TransportModes.Sea;
			consolTransport.JW_Vessel = "BUNGA DELIMA";
			consolTransport.JW_VoyageFlight = "4544";
			consolTransport.JW_RL_NKLoadPort = "AUSYD";
			consolTransport.JW_ETD = new ZDateTime(2012, 3, 4);
			consolTransport.JW_RL_NKDiscPort = "NZAKL";
			consolTransport.JW_ETA = new ZDateTime(2012, 4, 5);
			consolTransport.JW_IsLinked = false;

			var log = consolTransport.Logs.AddNew(Events.Authorised, ZDateTimeOffset.Empty, false);

			Factory.Save();

			var factory = new BusinessObjectFactory();

			var log2 = (StmALog)factory.ImportFromAnotherFactory(log);
			var transport2 = factory.Load<Transport>(consolTransport.PK); // LWK service task loads log parent directly, so there's nothing that can set the parent type

			Assert("Transport does not have own workflow", !(transport2 is IWorkflowProvider));

			AssertNoExceptionThrown("there's no exceptions during workflow run", () =>
				new WorkflowGun(log2, FireWorkflowMode.UpdateAll).TriggerWorkflow(transport2));
		}
	}
}
