namespace Enterprise.Freight.OnlineSailingSchedules.Testing
{
	using CargoWise.EntityFramework;
	using CargoWise.EntityFramework.Testing;
	using CargoWise.Types;
	using Enterprise.Freight.GUI;
	using Enterprise.MasterFiles.Business;
	using Enterprise.ZArchitecture.Modules;
	using NUnit.Framework;

	[TestedType(typeof(OnlineSchedulesVoyageVesselFilter))]
	public class OnlineSchedulesVoyageVesselFilterTest : NonPersistentBusinessObjectTestCase
	{
		public void TestOnlineSchedulesVoyageVesselFilter_IsEmpty()
		{
			var filter = (OnlineSchedulesVoyageVesselFilter)GetNewBusinessObject();

			filter.VoyageFlightNo = "CK2000";
			filter.Vessel = ZString.Empty;
			Assert("Filter is not empty when it has a voyage flight no.", !filter.IsEmpty);

			filter.VoyageFlightNo = ZString.Empty;
			filter.Vessel = "TITANIC";
			Assert("Filter is not empty when it has a vessel", !filter.IsEmpty);

			filter.VoyageFlightNo = ZString.Empty;
			filter.Vessel = ZString.Empty;
			Assert("Filter is empty when it has neither a voyage flight or vesse;", filter.IsEmpty);
		}

		public void TestOnlineSchedulesVoyageVesselFilter_HasNoComparisonOperator()
		{
			var filter = (OnlineSchedulesVoyageVesselFilter)GetNewBusinessObject();

			Assert("Filter has no comparison operator", !filter.HasComparisonOperator);
		}

		public void TestOnlineSchedulesVoyageVesselFilter_Clear()
		{
			var voyageFlightNo = "7986";
			var vessel = "BATTLESHIP";

			var filter = (OnlineSchedulesVoyageVesselFilter)GetNewBusinessObject();
			filter.VoyageFlightNo = voyageFlightNo;
			filter.Vessel = vessel;

			CombineAssertions("PRE: Filter properties are correctly set", () =>
			{
				AssertEquals(voyageFlightNo, filter.VoyageFlightNo);
				AssertEquals(vessel, filter.Vessel);
			});

			filter.Clear();

			CombineAssertions("Clearing filter properties sets both voyage and vessel to empty ZString", () =>
			{
				AssertEquals(ZString.Empty, filter.VoyageFlightNo);
				AssertEquals(ZString.Empty, filter.Vessel);
			});
		}

		public void TestOnlineSchedulesVoyageVesselFilter_LinksToRefVesselModule()
		{
			var filter = (OnlineSchedulesVoyageVesselFilter)GetNewBusinessObject();

			AssertEquals("Filter links to RefVessel Module", ModuleIDs.RefVessel, filter.ID);
		}

		protected override BusinessObject GetNewBusinessObject()
			=> new OnlineSchedulesVoyageVesselFilter("description", (a, b, c) => new ZQuery(), new RefVesselCollection(Factory));
	}
}
