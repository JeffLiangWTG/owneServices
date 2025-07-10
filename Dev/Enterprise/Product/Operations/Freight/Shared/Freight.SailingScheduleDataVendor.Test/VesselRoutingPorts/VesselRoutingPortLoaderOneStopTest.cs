using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingPort.Loader))]
	sealed class VesselRoutingPortLoaderOneStopTest : VesselRoutingPortLoaderTestBase
	{
		public void TestLoad_LineOperator()
		{
			var port1 = NewJobVesselSchedule("AUSYD", new ZDateTime(1999, 1, 1), new ZDateTime(1999, 1, 2), "Lloyds", "VoyageIn", "VoyageOut");
			port1.EV_LineOperator = "AAA";
			port1.EV_DataProviderReference = "XXX_12";
			var port2 = NewJobVesselSchedule("AUSYD", new ZDateTime(1999, 1, 1), new ZDateTime(1999, 1, 2), "Lloyds", "VoyageIn", "VoyageOut");
			port2.EV_LineOperator = "BBB";
			port2.EV_DataProviderReference = "ZZZ_11";
			RefVessel.Factory.Save();

			Voyage.JV_RV_NKVessel = RefVessel.RV_FK;
			Voyage.JV_VoyageFlight = "VoyageOut";
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			var loadedPort = Loader.Load(Destination, "ZZZ");
			AssertNull("Should not load port if carrier code is not found", loadedPort);

			loadedPort = Loader.Load(Destination, "BBB");
			AssertEquals("ZZZ_11", loadedPort.E7_DataProviderReference);

			loadedPort = Loader.Load(Destination, new List<ZString> { "AAA", "BBB" });
			AssertEquals("XXX_12", loadedPort.E7_DataProviderReference);
		}

		public override void TestLoad()
		{
			JobVesselSchedule port = NewJobVesselSchedule("AUSYD", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Lloyds", "VoyageIn", "VoyageOut");
			JobVesselSchedule decoyPort1 = NewJobVesselSchedule("AUSYD", new ZDateTime(1999, 1, 1), new ZDateTime(1999, 1, 2), "Lloyds", "VoyageIn", "VoyageOut");
			JobVesselSchedule decoyPort2 = NewJobVesselSchedule("AUMEL", new ZDateTime(2000, 1, 1), new ZDateTime(2000, 1, 2), "Lloyds", "VoyageIn", "VoyageOut");
			RefVessel.Factory.Save();

			Voyage.JV_RV_NKVessel = RefVessel.RV_FK;
			Voyage.JV_VoyageFlight = "VoyageIn";
			Origin.JA_RL_NKPortOfLoading = "AUSYD";
			VesselRoutingPort loadedPort = Loader.Load(Origin, ZString.Empty);
			AssertEquals("Matching the in voyage, using an origin port", port.EV_RL_NKPortCode, loadedPort.E7_RL_NKPortCode);
			AssertEquals("Matching the in voyage, using an origin port", port.EV_ETA, loadedPort.E7_ETA);

			Voyage.JV_RV_NKVessel = RefVessel.RV_FK;
			Voyage.JV_VoyageFlight = "VoyageOut";
			Destination.JB_RL_NKPortOfDischarge = "AUSYD";
			loadedPort = Loader.Load(Destination, ZString.Empty);
			AssertEquals("Matching the out voyage, using a destination port", loadedPort.E7_RL_NKPortCode, port.EV_RL_NKPortCode);
			AssertEquals("Matching the out voyage, using a destination port", loadedPort.E7_ETA, port.EV_ETA);
		}

		protected override string DataProvider { get { return FreightConstants.VesselDataProviders.OneStop; } }
	}
}
