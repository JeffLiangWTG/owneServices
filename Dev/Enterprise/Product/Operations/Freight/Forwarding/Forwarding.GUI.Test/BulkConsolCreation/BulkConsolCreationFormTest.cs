using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Routing.S8.GUI.Testing
{
	[TestedType(typeof(BulkConsolCreationForm))]
	public class BulkConsolCreationFormTest : ZFormBasherTest
	{
		#region BulkConsolCreationForm RecurrenceDisabled

		public void TestBulkConsolCreationForm_RecurrenceDisabled()
		{
			var sailings = new JobSailingCollection(Factory);
			var sailing = CreateSailing();
			sailings.Add(sailing);

			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			using (var form = new BulkConsolCreationFormForTest(multiDaysSelection))
			{
				form.Show();

				AssertEquals(0, form.createConsolUserControl.Top);

				AssertEquals(true, form.MultiDaysSelectionData.ImportAndCreateMAWB);
				AssertEquals(false, form.MultiDaysSelectionData.IncludeWeeklyTimetable);
				AssertEquals(false, form.MultiDaysSelectionData.RecurrenceEnabled);

				foreach (var controlName in new[]
				{
					"MultipleFlightsLabel",
					"RecurrencePatternGroupBox",
					"RangeOfRecurrenceGroupBox",
					"DailyPanel",
					"WeeklyPanel",
					"MonthlyPanel"
				})
				{
					AssertEquals(false, form.Controls.Find(controlName, true)[0].Visible);
				}
			}
		}
		#endregion

		#region Implementation

		JobSailing CreateSailing()
		{
			var voyage = Factory.New<JobVoyage>();
			voyage.JV_AirSeaRoad = Core.Constants.TransportModes.Air;
			voyage.JV_VoyageFlight = "SQ22";

			var origin = voyage.Origins.AddNew();
			origin.JA_RL_NKPortOfLoading = "AUSYD";
			origin.JA_E_DEP = ZDateTime.Today;

			var destination = voyage.Destinations.AddNew();
			destination.JB_RL_NKPortOfDischarge = "HKHKG";

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];
			sailing.JX_IsPublished = true;

			return sailing;
		}

		protected override Form GetFormToBashCore()
		{
			var sailings = new JobSailingCollection(Factory);
			sailings.Add(CreateSailing());
			var multiDaysSelection = new MultiDaysSelection(sailings, Factory);
			return new BulkConsolCreationForm(multiDaysSelection);
		}

		protected override bool AllowHasChangesOnFormOpen => true;

		#endregion
	}
}
