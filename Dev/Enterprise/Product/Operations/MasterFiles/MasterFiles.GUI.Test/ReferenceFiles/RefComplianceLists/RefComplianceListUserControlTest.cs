using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class RefComplianceListUserControlTest : TestCaseWithFactory
	{
		public void TestBinding()
		{
			var complianceListItem = Factory.NewWithValidTestData<RefComplianceList>();

			complianceListItem.RCL_ListCode = "XXX-YYY-ZZZ";
			complianceListItem.RCL_ListType = "Type";
			complianceListItem.RCL_PublisherJurisdiction = "Publisher Jurisdiction";

			complianceListItem.RCL_ListName = "List Name";
			complianceListItem.RCL_ListPublisher = "Publisher Name";
			complianceListItem.RCL_MainSourceURL = "Main Source";
			complianceListItem.RCL_SecondarySourceURL = "Secondary Source";

			complianceListItem.RCL_IntegrationDate = new ZDate(2020, 5, 5);
			complianceListItem.RCL_LastUpdatedDate = new ZDate(2020, 1, 1);

			complianceListItem.RCL_IsSystem = false;
			complianceListItem.RCL_IsActive = true;

			complianceListItem.RCL_ListDescription = "List Description";
			complianceListItem.RCL_PublisherDescription = "Publisher Description";

			complianceListItem.RCL_IsExcluded = true;

			Factory.Save();

			using (var form = new ZForm())
			using (var control = new RefComplianceListUserControlForTest())
			{
				form.SetDataBinding(complianceListItem, ".");
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Type is RefComplianceList", typeof(RefComplianceList), control.DataSourceType);
				CombineAssertions("Control elements display correct information", () =>
				{
					AssertEquals("XXX-YYY-ZZZ", control.ListCodeTextBox_Exposed.Text);
					AssertEquals("Type", control.ListTypeTextBox_Exposed.Text);
					AssertEquals("Publisher Jurisdiction", control.PublisherJurisdictionTextBox_Exposed.Text);

					AssertEquals("List Name", control.ListNameTextBox_Exposed.Text);
					AssertEquals("Publisher Name", control.PublisherNameTextBox_Exposed.Text);
					AssertEquals("Main Source", control.MainSourceTextBox_Exposed.Text);
					AssertEquals("Secondary Source", control.SecondarySourceTextBox_Exposed.Text);

					AssertEquals("5/05/2020", control.IntegrationDateTextBox_Exposed.Text);
					AssertEquals("1/01/2020", control.ModificationDateTextBox_Exposed.Text);

					AssertEquals(false, control.IsSystemCheckBox_Exposed.Checked);
					AssertEquals(true, control.IsActiveCheckBox_Exposed.Checked);

					AssertEquals("List Description", control.ListDescriptionTextBox_Exposed.Text);
					AssertEquals("Publisher Description", control.PublisherDetailsTextBox_Exposed.Text);

					AssertEquals(true, control.IsExcludedCheckBox_Exposed.Checked);
				});
			}
		}
	}
}
