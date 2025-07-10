using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module.Testing
{
	sealed class JobAirSailingFilterControlForTest : JobSailingFilterControlForTest
	{
		public void TestJobAirSailingFilterControlChangedColumnsStyle()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(FilterControl);
				form.Show();

				AssertEquals("Column should not be available", true, FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_NKVessel).IsUnavailable);
				AssertEquals("The value should be Flight No.", "Flight No.", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_VoyageFlight).Caption);
				AssertEquals("The value should be Loose Cut Off", "Loose Cut Off", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotCutOff).Caption);
				AssertEquals("The value should be Loose Rec. Start", "Loose Rec. Start", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotReceivalCommences).Caption);
				AssertEquals("The value should be Loose Avail.", "Loose Avail.", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotAvailabilityDate).Caption);
				AssertEquals("The value should be Loose Stor.", "Loose Stor.", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_DepotStorageDate).Caption);
				AssertEquals("The value should be ULD Cut Off", "ULD Cut Off", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_CTOCutOff).Caption);
				AssertEquals("The value should be ULD Rec. Start", "ULD Rec. Start", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JA_CTOReceivalCommences).Caption);
				AssertEquals("The value should be ULD Avail.", "ULD Avail.", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_CTOAvailabilityDate).Caption);
				AssertEquals("The value should be ULD Stor.", "ULD Stor.", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JB_CTOStorageDate).Caption);
				AssertEquals("The value should be Rsrvd Master.", "Rsrvd. Master", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_ReservedMasterBill).Caption);

				var aircraftTypeColumn = FilterControl.FilteredGrid.GetColumnStyle("JX_JV_AircraftType");
				AssertNotNull(aircraftTypeColumn);
				Assert(!aircraftTypeColumn.IsVisible);
				AssertEquals("Aircraft Type", aircraftTypeColumn.CaptionResourceString.Caption);

				var flightStatusColumn = FilterControl.FilteredGrid.GetColumnStyle("OnlineScheduleStatusDescription");
				AssertNotNull(flightStatusColumn);
				Assert(!flightStatusColumn.IsVisible);
				AssertEquals("Flight Status", flightStatusColumn.CaptionResourceString.Caption);
			}
		}

		#region Implementation

		protected override IEnumerable<string> DateColumnStylesShouldHaveLongFormat
			=> new string[] {
				JobSailing.Schema.JX_DepotCutOff,
				JobSailing.Schema.JX_DepotReceivalCommences,
				JobSailing.Schema.JX_DepotAvailabilityDate,
				JobSailing.Schema.JX_DepotStorageDate,
				JobSailing.Schema.JX_JA_CTOCutOff,
				JobSailing.Schema.JX_JA_CTOReceivalCommences,
				JobSailing.Schema.JX_JB_CTOAvailabilityDate,
				JobSailing.Schema.JX_JB_CTOStorageDate,
				JobSailing.Schema.JX_JA_DocumentaryCutoff,
				JobSailing.Schema.JX_JA_VGMCutOff,
				JobSailing.Schema.JX_JA_DGFCLReceivalCommences,
				JobSailing.Schema.JX_JA_DGFCLCutOff,

				JobSailing.Schema.JX_JA_E_ARV,
				JobSailing.Schema.JX_JA_S_ARV,
				JobSailing.Schema.JX_JA_A_ARV,
				JobSailing.Schema.JX_JA_E_DEP,
				JobSailing.Schema.JX_JA_S_DEP,
				JobSailing.Schema.JX_JA_A_DEP,
				JobSailing.Schema.JX_JB_E_ARV,
				JobSailing.Schema.JX_JB_S_ARV,
				JobSailing.Schema.JX_JB_A_ARV,
			};

		protected override IEnumerable<string> DateColumnStylesShouldHaveShortFormat
			=> System.Array.Empty<string>();

		public override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobAirSailingFilterBusinessObject();

		public override JobSailingFilterControl GetNewJobSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			=> new JobAirSailingFilterControl(gridCollection, filterBusinessObject);

		#endregion
	}
}
