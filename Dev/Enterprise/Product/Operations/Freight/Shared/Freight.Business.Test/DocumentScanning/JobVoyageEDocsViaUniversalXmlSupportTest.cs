using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Freight.Business.Test.DocumentScanning
{
	internal class JobVoyageEDocsViaUniversalXmlSupportTest : TestCaseWithFactory
	{
		public void TestLoadJobVoyage(ZString airSeaRoad, ZString voyageFlight)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = airSeaRoad;
			voyage.JV_VoyageFlight = voyageFlight;
			Factory.Save();

			var loader = new JobVoyageEDocsViaUniversalXmlSupport(airSeaRoad);
			AssertEquals(voyage.PK, loader.LoadBusinessObjectFromCode(Factory, voyage.JV_SendersMessageReference)?.PK);
		}

		public void TestTryLoadJobVoyageNotInDb(ZString airSeaRoad, ZString voyageFlight)
		{
			var loader = new JobVoyageEDocsViaUniversalXmlSupport(airSeaRoad);
			AssertEquals(null, loader.LoadBusinessObjectFromCode(Factory, voyageFlight));
		}

		public void TestLoadFlightSchedule() => TestLoadJobVoyage(Core.Constants.TransportModes.Air, "J00000801");

		public void TestTryLoadFlightScheduleNotInDb() => TestTryLoadJobVoyageNotInDb(Core.Constants.TransportModes.Air, "J99999999");

		public void TestLoadRailSchedule() => TestLoadJobVoyage(Core.Constants.TransportModes.Rail, "123R");

		public void TestTryLoadRailScheduleNotInDb() => TestTryLoadJobVoyageNotInDb(Core.Constants.TransportModes.Rail, "J99999999");

		public void TestLoadSailingSchedule() => TestLoadJobVoyage(Core.Constants.TransportModes.Sea, "J00000803");

		public void TestTryLoadSailingScheduleNotInDb() => TestTryLoadJobVoyageNotInDb(Core.Constants.TransportModes.Sea, "J99999999");

		public void TestLoadTruckingSchedule() => TestLoadJobVoyage(Core.Constants.TransportModes.Road, "123T");

		public void TestTryLoadTruckingScheduleNotInDb() => TestTryLoadJobVoyageNotInDb(Core.Constants.TransportModes.Road, "J99999999");
	}
}
