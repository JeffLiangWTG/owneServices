using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.GUI.Testing
{
	sealed class VoyageRelatedJobsControlTest : TestCaseWithFactory
	{
		public void TestEditSelectedJob()
		{
			var voyage = Factory.NewWithValidTestData<JobVoyage>();
			var voyOrigin = voyage.Origins.AddNew();
			var voyDestination = voyage.Destinations.AddNew();

			voyOrigin.FillWithValidTestData();
			voyOrigin.JA_RL_NKPortOfLoading = "AUBNE";

			voyDestination.FillWithValidTestData();
			voyDestination.JB_RL_NKPortOfDischarge = "DEHAM";

			voyage.GenerateSailings();

			var sailing = voyage.Sailings[0];

			var anotherBranchPK = Factory.LoadTop1<GlbBranch>(new ZQuery(GlbBranchSchema.GB_GC, SQLComparisonOperator.NotEqual, GlbCompany.CurrentCompany.PK)).PK;

			var portTransportForAnotherCompany = (BusinessObject)Factory.New<LocalCartage.Integration.ICommonCartage>();
			portTransportForAnotherCompany.FillWithValidTestData();
			portTransportForAnotherCompany["JJ_JX_Sailing"] = sailing.PK;
			portTransportForAnotherCompany["JJ_GB"] = anotherBranchPK;

			Factory.Save();

			using (var dummyForm = new ZDummyForm())
			{
				var voyageRelatedJobsControl = new VoyageRelatedJobsControl();
				voyageRelatedJobsControl.SetDataBinding(voyage, "");
				dummyForm.Controls.Add(voyageRelatedJobsControl);
				dummyForm.Show();

				voyageRelatedJobsControl.OpenJobButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertContains("Please select an item in the grid.", UnitTestUserNotification.Instance.LastMessage.Text);

				voyageRelatedJobsControl.RelatedJobsGrid.Select(0);
				voyageRelatedJobsControl.OpenJobButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertContains("The selected job belongs to another Company so cannot be edited.", UnitTestUserNotification.Instance.LastMessage.Text);

				using (new Environment.TemporaryUserContext { BranchPK = anotherBranchPK.ToGuid() }.Set())
				{
					UnitTestUserNotification.Instance.ClearMessages();
					voyageRelatedJobsControl.OpenJobButton.PerformClick();
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}
	}
}
