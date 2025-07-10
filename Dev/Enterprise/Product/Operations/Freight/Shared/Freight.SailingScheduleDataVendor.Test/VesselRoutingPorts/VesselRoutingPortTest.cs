using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.SailingDataVendor.Business.Testing
{
	[TestedType(typeof(VesselRoutingPort))]
	sealed class VesselRoutingPortTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("Cannot save or delete rows in a view", true);
		}

		[DeveloperOnlyTest]
		public override void TestCalcPropertiesWithDbHitsUseFetchHints()
		{
			Assert("Cannot save the factory for a view.", true);
		}

		#region Implementation

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			var result = Factory.New<JobVesselSchedule>();
			result.EV_RL_NKPortCode = "AUSYD";
			result.EV_ETA = new ZDateTime(2014, 12, 1);
			result.EV_ETD = new ZDateTime(2014, 12, 2);
			result.EV_IMOLloydsNumber = "Lloyds";
			result.EV_ShipOperatorVoyageIn = "VoyageIn";
			result.EV_ShipOperatorVoyageOut = "VoyageOut";
			result.EV_DataProvider = FreightConstants.VesselDataProviders.OneStop;
			result.EV_LineOperator = "MOL";
			result.EV_DataProviderReference = "XXX_12";

			var refVessel = Factory.New<RefVessel>();
			refVessel.RV_Name = "Vessel";
			refVessel.RV_LloydsNumber = "Lloyds";

			var voyage = Factory.New<JobVoyage>();
			voyage.Origins.AddNew();
			voyage.Origins[0].JA_RL_NKPortOfLoading = "CNCAN";

			voyage.Destinations.AddNew();
			voyage.Destinations[0].JB_RL_NKPortOfDischarge = "AUSYD";

			voyage.JV_RV_NKVessel = refVessel.RV_FK;
			voyage.JV_VoyageFlight = "VoyageOut";
			refVessel.Factory.Save();

			var loader = new VesselRoutingPort.Loader(Factory);
			var loadedPort = loader.Load(voyage.Destinations[0], "MOL");

			return loadedPort;
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var schedule = factory.New<JobVesselSchedule>();
			schedule.EV_RL_NKPortCode = "AUSYD";
			schedule.EV_ETA = new ZDateTime(2014, 12, 1);
			schedule.EV_ETD = new ZDateTime(2014, 12, 2);
			schedule.EV_IMOLloydsNumber = "Lloyds";
			schedule.EV_ShipOperatorVoyageIn = "VoyageIn";
			schedule.EV_ShipOperatorVoyageOut = "VoyageOut";
			schedule.EV_DataProvider = FreightConstants.VesselDataProviders.OneStop;
			schedule.EV_LineOperator = "MOL";
			schedule.EV_DataProviderReference = "XXX_12";

			var refVessel = factory.New<RefVessel>();
			refVessel.RV_Name = "Vessel";
			refVessel.RV_LloydsNumber = "Lloyds";

			var voyage = factory.New<JobVoyage>();
			voyage.Origins.AddNew();
			voyage.Origins[0].JA_RL_NKPortOfLoading = "CNCAN";

			voyage.Destinations.AddNew();
			voyage.Destinations[0].JB_RL_NKPortOfDischarge = "AUSYD";

			voyage.JV_RV_NKVessel = refVessel.RV_FK;
			voyage.JV_VoyageFlight = "VoyageOut";
			refVessel.Factory.Save();

			var loader = new VesselRoutingPort.Loader(factory);
			var loadedPort = loader.Load(voyage.Destinations[0], "MOL");

			return loadedPort;
		}

		#endregion
	}
}
