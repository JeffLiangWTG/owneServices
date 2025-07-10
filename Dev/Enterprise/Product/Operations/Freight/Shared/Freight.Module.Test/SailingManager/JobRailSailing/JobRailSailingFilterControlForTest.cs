using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Freight.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Module.Testing
{
	sealed class JobRailSailingFilterControlForTest : JobSailingFilterControlForTest
	{
		public void TestJobRailSailingFilterControlChangedColumnsStyle()
		{
			using (ZForm form = new ZForm())
			{
				form.Controls.Add(FilterControl);
				form.Show();

				AssertEquals("The value should be Journey Name", "Journey Name", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_NKVessel).Caption);
				AssertEquals("The value should be Journey No.", "Journey No.", FilterControl.FilteredGrid.GetColumnStyle(JobSailing.Schema.JX_JV_VoyageFlight).Caption);
			}
		}

		#region Implementation

		protected override IEnumerable<string> DateColumnStylesShouldHaveLongFormat
			=> new string[] {
			JobSailing.Schema.JX_JA_CTOCutOff,
			JobSailing.Schema.JX_JA_CTOReceivalCommences,
			JobSailing.Schema.JX_JB_CTOAvailabilityDate,
			JobSailing.Schema.JX_JB_CTOStorageDate,
			JobSailing.Schema.JX_JA_DocumentaryCutoff,
			JobSailing.Schema.JX_JA_VGMCutOff,
			JobSailing.Schema.JX_JA_DGFCLReceivalCommences,
			JobSailing.Schema.JX_JA_DGFCLCutOff,
			JobSailing.Schema.JX_DepotCutOff,
			JobSailing.Schema.JX_DepotReceivalCommences,
			JobSailing.Schema.JX_DepotAvailabilityDate,
			JobSailing.Schema.JX_DepotStorageDate,
		};

		protected override IEnumerable<string> DateColumnStylesShouldHaveShortFormat
			=> new string[] {
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

		public override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new JobRailSailingFilterBusinessObject();

		public override JobSailingFilterControl GetNewJobSailingFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			=> new JobRailSailingFilterControl(gridCollection, filterBusinessObject);

		#endregion
	}
}
