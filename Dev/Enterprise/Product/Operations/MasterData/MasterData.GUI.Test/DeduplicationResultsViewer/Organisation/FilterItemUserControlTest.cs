using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	public class FilterItemUserControlTest : TestCase
	{
		public void TestRemoveFilter()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var resultDetail = new DeduplicationOrganisationResultDetailForTest();
				resultDetail.SetIsEmptyOrNotForTesting(false);
				potentialDuplicatesUserControl.SetupDataContext(resultDetail, true);

				var operationUserControl = form.GetControl<FilterOperationUserControl>("FilterOperationUserControl");
				operationUserControl.AdvancedFilterCriteriaControl = form.GetControl<AdvancedFilterCriteriaControl>("AdvancedFilterCriteriaControl");

				var removeFilterButtons = form.Controls.Find("RemoveFilterButton", true);
				CombineAssertions(() =>
				{
					AssertEquals(1, removeFilterButtons.Length);
					AssertEquals(false, removeFilterButtons[0].Enabled);
				});

				form.GetControl<ZButton>("AddFilterButton").PerformClick();
				removeFilterButtons = form.Controls.Find("RemoveFilterButton", true);
				CombineAssertions(() =>
				{
					AssertEquals(2, removeFilterButtons.Length);
					AssertEquals(true, removeFilterButtons[0].Enabled);
					AssertEquals(true, removeFilterButtons[1].Enabled);
				});

				(removeFilterButtons[0] as ZButton).PerformClick();
				removeFilterButtons = form.Controls.Find("RemoveFilterButton", true);
				CombineAssertions(() =>
				{
					AssertEquals(1, removeFilterButtons.Length);
					AssertEquals(false, removeFilterButtons[0].Enabled);
				});

				form.Close();
			}
		}

		public void TestFilterItemDefaultVisibility()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var orgResultDetail = new DeduplicationOrganisationResultDetailForTest();
				orgResultDetail.SetIsEmptyOrNotForTesting(false);
				potentialDuplicatesUserControl.SetupDataContext(orgResultDetail, true);

				var filterItemUserControl = form.Controls.Find("FilterItemUserControl", true)[0] as FilterItemUserControl;
				CombineAssertions(() =>
				{
					AssertEquals(false, filterItemUserControl.FilterConditionDropEdit.Visible);
					AssertEquals(false, filterItemUserControl.FilterKeywordTextBox.Visible);
					AssertEquals(false, filterItemUserControl.CheckBoxLayoutPanel.Visible);
				});
			}
		}

		public void TestOrgFilterTypeDescriptionValueChanged()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var orgResultDetail = new DeduplicationOrganisationResultDetailForTest();
				orgResultDetail.SetIsEmptyOrNotForTesting(false);
				potentialDuplicatesUserControl.SetupDataContext(orgResultDetail, true);

				var filterItemUserControl = form.Controls.Find("FilterItemUserControl", true)[0] as FilterItemUserControl;
				filterItemUserControl.OrgFilterItemDataSource.OrgFilterTypeDescription = TextConstant.OrganizationTypes.GetUnresolvedString();
				CombineAssertions(() =>
				{
					AssertEquals(false, filterItemUserControl.FilterConditionDropEdit.Visible);
					AssertEquals(false, filterItemUserControl.FilterKeywordTextBox.Visible);
					AssertEquals(true, filterItemUserControl.CheckBoxLayoutPanel.Visible);
				});

				filterItemUserControl.OrgFilterItemDataSource.OrgFilterTypeDescription = TextConstant.Email.GetUnresolvedString();
				CombineAssertions(() =>
				{
					AssertEquals(true, filterItemUserControl.FilterConditionDropEdit.Visible);
					AssertEquals(true, filterItemUserControl.FilterKeywordTextBox.Visible);
					AssertEquals(false, filterItemUserControl.CheckBoxLayoutPanel.Visible);
				});
			}
		}
	}
}
