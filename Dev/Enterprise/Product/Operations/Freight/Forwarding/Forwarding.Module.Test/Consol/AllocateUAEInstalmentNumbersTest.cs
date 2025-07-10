using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Module.Testing
{
	class AllocateUAEInstalmentNumbersTest : TestCaseWithFactory
	{
		public void TestAllocateUAEInstallment()
		{
			ForwardingConsol[] consols = new ForwardingConsol[8];
			consols[0] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[1] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[2] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[3] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[4] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[5] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[6] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[7] = Factory.NewWithValidTestData<ForwardingConsol>();

			consols[0].JK_AgentType = Constants.AgentType.Agent;
			consols[1].JK_AgentType = Constants.AgentType.Agent;
			consols[2].JK_AgentType = Constants.AgentType.Agent;
			consols[3].JK_AgentType = Constants.AgentType.Agent;
			consols[4].JK_AgentType = Constants.AgentType.Agent;
			consols[5].JK_AgentType = Constants.AgentType.Agent;
			consols[6].JK_AgentType = Constants.AgentType.Agent;
			consols[7].JK_AgentType = Constants.AgentType.Agent;

			var legInbound0 = consols[0].Transports.AddNew();
			legInbound0.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legInbound0.JW_LegOrder = 1;
			legInbound0.JW_RL_NKLoadPort = "AUSYD";
			legInbound0.JW_RL_NKDiscPort = "AEAUH";
			legInbound0.JW_Vessel = "NED ROCHESTER";
			legInbound0.JW_VoyageFlight = "123";

			var legOutbound0 = consols[0].Transports.AddNew();
			legOutbound0.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legOutbound0.JW_LegOrder = 2;
			legOutbound0.JW_RL_NKLoadPort = "AEAUH";
			legOutbound0.JW_RL_NKDiscPort = "UKLON";
			legOutbound0.JW_Vessel = "NED ROCHESTER";
			legOutbound0.JW_VoyageFlight = "234";

			var legInbound1 = consols[1].Transports.AddNew();
			legInbound1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legInbound1.JW_LegOrder = 1;
			legInbound1.JW_RL_NKLoadPort = "NZAKL";
			legInbound1.JW_RL_NKDiscPort = "AEAUH";
			legInbound1.JW_Vessel = "NED ROCHESTER";
			legInbound1.JW_VoyageFlight = "123";

			var legOutbound1 = consols[1].Transports.AddNew();
			legOutbound1.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legOutbound1.JW_LegOrder = 2;
			legOutbound1.JW_RL_NKLoadPort = "AEAUH";
			legOutbound1.JW_RL_NKDiscPort = "UKLON";
			legOutbound1.JW_Vessel = "NED ROCHESTER";
			legOutbound1.JW_VoyageFlight = "234";

			var legInbound2 = consols[2].Transports.AddNew();
			legInbound2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legInbound2.JW_LegOrder = 1;
			legInbound2.JW_RL_NKLoadPort = "NZAKL";
			legInbound2.JW_RL_NKDiscPort = "AEAUH";
			legInbound2.JW_Vessel = "ADDU MOON";
			legInbound2.JW_VoyageFlight = "123";

			var legOutbound2 = consols[2].Transports.AddNew();
			legOutbound2.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legOutbound2.JW_LegOrder = 2;
			legOutbound2.JW_RL_NKLoadPort = "AEAUH";
			legOutbound2.JW_RL_NKDiscPort = "UKLON";
			legOutbound2.JW_Vessel = "NED ROCHESTER";
			legOutbound2.JW_VoyageFlight = "234";

			var legImportOnly3 = consols[3].Transports.AddNew();
			legImportOnly3.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legImportOnly3.JW_LegOrder = 1;
			legImportOnly3.JW_RL_NKLoadPort = "AUSYD";
			legImportOnly3.JW_RL_NKDiscPort = "AEAUH";
			legImportOnly3.JW_Vessel = "NED ROCHESTER";
			legImportOnly3.JW_VoyageFlight = "123";

			var legExportOnly4 = consols[4].Transports.AddNew();
			legExportOnly4.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legExportOnly4.JW_LegOrder = 1;
			legExportOnly4.JW_RL_NKLoadPort = "AEAUH";
			legExportOnly4.JW_RL_NKDiscPort = "AUSYD";
			legExportOnly4.JW_Vessel = "NED ROCHESTER";
			legExportOnly4.JW_VoyageFlight = "345";

			var legDomesticOnly5 = consols[5].Transports.AddNew();
			legDomesticOnly5.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legDomesticOnly5.JW_LegOrder = 1;
			legDomesticOnly5.JW_RL_NKLoadPort = "AEAUH";
			legDomesticOnly5.JW_RL_NKDiscPort = "AEDXB";
			legDomesticOnly5.JW_Vessel = "NED ROCHESTER";
			legDomesticOnly5.JW_VoyageFlight = "456";

			var legConsol1Inbound = consols[6].Transports.AddNew();
			legConsol1Inbound.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legConsol1Inbound.JW_LegOrder = 1;
			legConsol1Inbound.JW_RL_NKLoadPort = "AUSYD";
			legConsol1Inbound.JW_RL_NKDiscPort = "AEAUH";
			legConsol1Inbound.JW_Vessel = "NED ROCHESTER";
			legConsol1Inbound.JW_VoyageFlight = "789";

			var legConsol1Domestic = consols[6].Transports.AddNew();
			legConsol1Domestic.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legConsol1Domestic.JW_LegOrder = 2;
			legConsol1Domestic.JW_RL_NKLoadPort = "AEAUH";
			legConsol1Domestic.JW_RL_NKDiscPort = "AEDXB";
			legConsol1Domestic.JW_Vessel = "NED ROCHESTER";
			legConsol1Domestic.JW_VoyageFlight = "321";

			var legConsol2DomesticOnly = consols[7].Transports.AddNew();
			legConsol2DomesticOnly.JW_TransportMode = Core.Constants.TransportModes.Sea;
			legConsol2DomesticOnly.JW_LegOrder = 1;
			legConsol2DomesticOnly.JW_RL_NKLoadPort = "AEAUH";
			legConsol2DomesticOnly.JW_RL_NKDiscPort = "AEDXB";
			legConsol2DomesticOnly.JW_Vessel = "NED ROCHESTER";
			legConsol2DomesticOnly.JW_VoyageFlight = "321";

			Factory.Save();

			var alloc = new AllocateUAEInstalmentNumbers();
			alloc.Allocate(consols);

			AssertEquals("First Load Port AU/Final Discharge Port UK", "1", consols[0].UAEInstalmentNumber);
			AssertEquals("First Load Port NZ/Final Discharge Port UK", "2", consols[1].UAEInstalmentNumber);
			AssertEquals("Different Vessel", "1", consols[2].UAEInstalmentNumber);
			AssertEquals("Import Only", "3", consols[3].UAEInstalmentNumber);
			AssertEquals("Export Only", 0, consols[4].Numbers.Count);
			AssertEquals("Domestic Only", "1", consols[5].UAEInstalmentNumber);
			AssertEquals("First Load Port AU/Final Discharge Port Domestic", "1", consols[6].UAEInstalmentNumber);
			AssertEquals("Domestic Only", "1", consols[7].UAEInstalmentNumber);
		}

		public void TestAllocateUAEInstallment_WhenTransportModeIsIWT()
		{
			ForwardingConsol[] consols = new ForwardingConsol[8];
			consols[0] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[1] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[2] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[3] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[4] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[5] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[6] = Factory.NewWithValidTestData<ForwardingConsol>();
			consols[7] = Factory.NewWithValidTestData<ForwardingConsol>();

			consols[0].JK_AgentType = Constants.AgentType.Agent;
			consols[1].JK_AgentType = Constants.AgentType.Agent;
			consols[2].JK_AgentType = Constants.AgentType.Agent;
			consols[3].JK_AgentType = Constants.AgentType.Agent;
			consols[4].JK_AgentType = Constants.AgentType.Agent;
			consols[5].JK_AgentType = Constants.AgentType.Agent;
			consols[6].JK_AgentType = Constants.AgentType.Agent;
			consols[7].JK_AgentType = Constants.AgentType.Agent;

			var legInbound0 = consols[0].Transports.AddNew();
			legInbound0.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legInbound0.JW_LegOrder = 1;
			legInbound0.JW_RL_NKLoadPort = "AUSYD";
			legInbound0.JW_RL_NKDiscPort = "AEAUH";
			legInbound0.JW_Vessel = "NED ROCHESTER";
			legInbound0.JW_VoyageFlight = "123";

			var legOutbound0 = consols[0].Transports.AddNew();
			legOutbound0.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legOutbound0.JW_LegOrder = 2;
			legOutbound0.JW_RL_NKLoadPort = "AEAUH";
			legOutbound0.JW_RL_NKDiscPort = "UKLON";
			legOutbound0.JW_Vessel = "NED ROCHESTER";
			legOutbound0.JW_VoyageFlight = "234";

			var legInbound1 = consols[1].Transports.AddNew();
			legInbound1.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legInbound1.JW_LegOrder = 1;
			legInbound1.JW_RL_NKLoadPort = "NZAKL";
			legInbound1.JW_RL_NKDiscPort = "AEAUH";
			legInbound1.JW_Vessel = "NED ROCHESTER";
			legInbound1.JW_VoyageFlight = "123";

			var legOutbound1 = consols[1].Transports.AddNew();
			legOutbound1.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legOutbound1.JW_LegOrder = 2;
			legOutbound1.JW_RL_NKLoadPort = "AEAUH";
			legOutbound1.JW_RL_NKDiscPort = "UKLON";
			legOutbound1.JW_Vessel = "NED ROCHESTER";
			legOutbound1.JW_VoyageFlight = "234";

			var legInbound2 = consols[2].Transports.AddNew();
			legInbound2.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legInbound2.JW_LegOrder = 1;
			legInbound2.JW_RL_NKLoadPort = "NZAKL";
			legInbound2.JW_RL_NKDiscPort = "AEAUH";
			legInbound2.JW_Vessel = "ADDU MOON";
			legInbound2.JW_VoyageFlight = "123";

			var legOutbound2 = consols[2].Transports.AddNew();
			legOutbound2.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legOutbound2.JW_LegOrder = 2;
			legOutbound2.JW_RL_NKLoadPort = "AEAUH";
			legOutbound2.JW_RL_NKDiscPort = "UKLON";
			legOutbound2.JW_Vessel = "NED ROCHESTER";
			legOutbound2.JW_VoyageFlight = "234";

			var legImportOnly3 = consols[3].Transports.AddNew();
			legImportOnly3.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legImportOnly3.JW_LegOrder = 1;
			legImportOnly3.JW_RL_NKLoadPort = "AUSYD";
			legImportOnly3.JW_RL_NKDiscPort = "AEAUH";
			legImportOnly3.JW_Vessel = "NED ROCHESTER";
			legImportOnly3.JW_VoyageFlight = "123";

			var legExportOnly4 = consols[4].Transports.AddNew();
			legExportOnly4.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legExportOnly4.JW_LegOrder = 1;
			legExportOnly4.JW_RL_NKLoadPort = "AEAUH";
			legExportOnly4.JW_RL_NKDiscPort = "AUSYD";
			legExportOnly4.JW_Vessel = "NED ROCHESTER";
			legExportOnly4.JW_VoyageFlight = "345";

			var legDomesticOnly5 = consols[5].Transports.AddNew();
			legDomesticOnly5.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legDomesticOnly5.JW_LegOrder = 1;
			legDomesticOnly5.JW_RL_NKLoadPort = "AEAUH";
			legDomesticOnly5.JW_RL_NKDiscPort = "AEDXB";
			legDomesticOnly5.JW_Vessel = "NED ROCHESTER";
			legDomesticOnly5.JW_VoyageFlight = "456";

			var legConsol1Inbound = consols[6].Transports.AddNew();
			legConsol1Inbound.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legConsol1Inbound.JW_LegOrder = 1;
			legConsol1Inbound.JW_RL_NKLoadPort = "AUSYD";
			legConsol1Inbound.JW_RL_NKDiscPort = "AEAUH";
			legConsol1Inbound.JW_Vessel = "NED ROCHESTER";
			legConsol1Inbound.JW_VoyageFlight = "789";

			var legConsol1Domestic = consols[6].Transports.AddNew();
			legConsol1Domestic.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legConsol1Domestic.JW_LegOrder = 2;
			legConsol1Domestic.JW_RL_NKLoadPort = "AEAUH";
			legConsol1Domestic.JW_RL_NKDiscPort = "AEDXB";
			legConsol1Domestic.JW_Vessel = "NED ROCHESTER";
			legConsol1Domestic.JW_VoyageFlight = "321";

			var legConsol2DomesticOnly = consols[7].Transports.AddNew();
			legConsol2DomesticOnly.JW_TransportMode = Core.Constants.TransportModes.InlandWaterwayTransport;
			legConsol2DomesticOnly.JW_LegOrder = 1;
			legConsol2DomesticOnly.JW_RL_NKLoadPort = "AEAUH";
			legConsol2DomesticOnly.JW_RL_NKDiscPort = "AEDXB";
			legConsol2DomesticOnly.JW_Vessel = "NED ROCHESTER";
			legConsol2DomesticOnly.JW_VoyageFlight = "321";

			Factory.Save();

			var alloc = new AllocateUAEInstalmentNumbers();
			alloc.Allocate(consols);

			AssertEquals("First Load Port AU/Final Discharge Port UK", "1", consols[0].UAEInstalmentNumber);
			AssertEquals("First Load Port NZ/Final Discharge Port UK", "2", consols[1].UAEInstalmentNumber);
			AssertEquals("Different Vessel", "1", consols[2].UAEInstalmentNumber);
			AssertEquals("Import Only", "3", consols[3].UAEInstalmentNumber);
			AssertEquals("Export Only", 0, consols[4].Numbers.Count);
			AssertEquals("Domestic Only", "1", consols[5].UAEInstalmentNumber);
			AssertEquals("First Load Port AU/Final Discharge Port Domestic", "1", consols[6].UAEInstalmentNumber);
			AssertEquals("Domestic Only", "1", consols[7].UAEInstalmentNumber);
		}
	}
}
