using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.GUI.Testing
{
	sealed class ReconHeaderUserControlTest : TestCaseWithFactory
	{
		public void TestControlsVisibility()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			using (var form = new ReconDeclarationForm(reconDeclaration))
			{
				form.Show();
				var userControl = form.HeaderDetailsReconHeaderUserControl;
				AssertEquals("Should not be visible for non-Aggregate Recon", false, userControl.NoChangeAggregateCheckBox.Visible);
				foreach (var control in userControl.ImporterZDocAddressControl.Controls)
				{
					if (control.GetType() == typeof(ZCheckBox))
					{
						AssertEquals("'OverrideAddress' control should be invisible", false, ((ZCheckBox)control).Visible);
					}
				}

				AssertNotNull(userControl.CurrentDataItem);
				reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACS;
				reconDeclaration.US_IsAggregate = true;
				AssertEquals("Should be visible for Aggregate Recon", true, userControl.NoChangeAggregateCheckBox.Visible);
				AssertEquals("Should be visible for Aggregate Recon", true, userControl.AggregateFeesGroupBox.Visible);
				AssertEquals("Should be visible for Aggregate Recon", true, userControl.WaiveCheckBox.Visible);
				reconDeclaration.US_IsAggregate = false;
				Assert("PreCondition", !reconDeclaration.US_R_IsNoChangeAgg);
				AssertEquals("Should be NOT visible for Aggregate Recon", false, userControl.NoChangeAggregateCheckBox.Visible);
				AssertEquals("Should be NOT visible for Aggregate Recon", false, userControl.AggregateFeesGroupBox.Visible);
				AssertEquals("Should be NOT visible for Aggregate Recon", false, userControl.WaiveCheckBox.Visible);
				reconDeclaration.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
				reconDeclaration.US_IsAggregate = true;
				AssertEquals("Should be visible for Aggregate Recon", true, userControl.NoChangeAggregateCheckBox.Visible);
				AssertEquals("Should be NOT visible for Aggregate Recon", false, userControl.AggregateFeesGroupBox.Visible);
				AssertEquals("Should be visible for Aggregate Recon", true, userControl.WaiveCheckBox.Visible);
				reconDeclaration.US_IsAggregate = false;
				Assert("PreCondition", !reconDeclaration.US_R_IsNoChangeAgg);
				AssertEquals("Should be NOT visible for Aggregate Recon", false, userControl.NoChangeAggregateCheckBox.Visible);
				AssertEquals("Should be NOT visible for Aggregate Recon", false, userControl.AggregateFeesGroupBox.Visible);
				AssertEquals("Should be NOT visible for Aggregate Recon", false, userControl.WaiveCheckBox.Visible);
			}
		}

		public void TestQualifyingGoodsFTADecLabelVisibility()
		{
			var reconDeclaration = new ReconDeclaration(Factory.New<JobDeclaration>());
			using (var form = new ReconDeclarationForm(reconDeclaration))
			{
				form.Show();
				var userControl = form.HeaderDetailsReconHeaderUserControl;
				AssertEquals(true, userControl.Visible);
				AssertEquals("Should not be visible for US_IssureCode != FTA", false, userControl.QualifyingGoodsFTADecLabel.Visible);
				reconDeclaration.JE_ApplicationCode = "ACE";
				reconDeclaration.US_IssueCode = ReconIssueCodeList.Codes.FTA;
				AssertEquals("Should be visible for US_IssureCode == FTA", true, userControl.QualifyingGoodsFTADecLabel.Visible);
			}
		}
	}
}
