using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(MultiDaysSelection))]
	sealed class MultiDaysSelectionTest : NonPersistentBusinessObjectTestCase
	{
		#region GenerateConsol for Flight Schedules

		public void TestGenerateConsolForFlightSchedules()
		{
			var sailings = new JobSailingCollection(Factory);
			var sailing = CreateSailing("SQ22");
			sailings.Add(sailing);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);

			AssertEquals(false, multiDaysSelection.IncludeWeeklyTimetable);
			AssertEquals(true, multiDaysSelection.ImportAndCreateMAWB);
			AssertEquals(false, multiDaysSelection.RecurrenceEnabled);
			AssertEquals(1, multiDaysSelection.SailingCollection.Count);
			AssertEquals(sailing.PK, multiDaysSelection.SailingCollection[0].PK);
			AssertEquals(1, multiDaysSelection.GetFlightNumber());
			AssertEquals(ZString.Empty, multiDaysSelection.ConsolDetails.AirlinePrefix);

			multiDaysSelection.ActiveTab = MultiDaysSelection.CreateNewConsolsTabName;
			multiDaysSelection.Generate();
			AssertEquals(1, multiDaysSelection.CreatedConsols.Count);
			AssertEquals("SQ22", multiDaysSelection.CreatedConsols[0].JK_JX_JV_VoyageFlight);
		}

		public void TestGenerateConsolForFlightSchedules_FromDifferentCarriers()
		{
			var sailings = new JobSailingCollection(Factory);
			var sailing1 = CreateSailing("SQ22");
			var sailing2 = CreateSailing("CX33");
			sailings.Add(sailing1);
			sailings.Add(sailing2);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			AssertEquals(ZString.Empty, multiDaysSelection.ConsolDetails.AirlinePrefix);

			multiDaysSelection.ActiveTab = MultiDaysSelection.CreateNewConsolsTabName;
			multiDaysSelection.Generate();

			AssertEquals(2, multiDaysSelection.CreatedConsols.Count);
			AssertEquals("SQ22", multiDaysSelection.CreatedConsols[0].JK_JX_JV_VoyageFlight);
			AssertEquals("CX33", multiDaysSelection.CreatedConsols[1].JK_JX_JV_VoyageFlight);
		}

		public void TestFirstCarrier()
		{
			var sailings = new JobSailingCollection(Factory);
			var sailing1 = CreateSailing("SQ22");
			var sailing2 = CreateSailing("CX33");
			sailings.Add(sailing1);
			sailings.Add(sailing2);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			AssertEquals("SQ", multiDaysSelection.FirstCarrier);
		}

		#endregion

		#region Implementation

		JobSailing CreateSailing(ZString voyageFlight)
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = voyageFlight;

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";
			destination.JB_E_ARV = ZDate.Today.AddDays(1);

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var sailings = new JobSailingCollection(Factory);
			sailings.Add(CreateSailing("SQ22"));

			return new MultiDaysSelection(sailings, Factory);
		}

		#endregion
	}
}
