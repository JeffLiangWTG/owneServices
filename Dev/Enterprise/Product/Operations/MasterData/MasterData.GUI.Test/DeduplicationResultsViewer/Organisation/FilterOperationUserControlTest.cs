using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterData.GUI.Test
{
	public class FilterOperationUserControlTest : TestCaseWithFactory
	{
		public void TestAddFilter()
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
			}
		}

		public void TestSaveFilterWithEmptyName()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var resultDetail = new DeduplicationOrganisationResultDetailForTest();
				resultDetail.SetIsEmptyOrNotForTesting(false);
				potentialDuplicatesUserControl.SetupDataContext(resultDetail, true);

				var savedFilter = form.Controls.Find("SavedFilterComboBox", true)[0] as KComboBox;
				AssertEquals(0, savedFilter.Items.Count);

				var filterName = form.Controls.Find("FilterNameTextBox", true)[0] as ZTextBox;
				filterName.Text = string.Empty;
				UnitTestUserNotification.Instance.ClearMessages();

				form.GetControl<ZButton>("SaveButton").PerformClick();
				AssertEquals("Please specify a name.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestLoadFilterWithEmptyName()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var resultDetail = new DeduplicationOrganisationResultDetailForTest();
				resultDetail.SetIsEmptyOrNotForTesting(false);
				potentialDuplicatesUserControl.SetupDataContext(resultDetail, true);

				var savedFilter = form.Controls.Find("SavedFilterComboBox", true)[0];
				savedFilter.Text = string.Empty;
				UnitTestUserNotification.Instance.ClearMessages();

				form.GetControl<ZButton>("LoadButton").PerformClick();
				AssertEquals("Please select a saved Dynamic Filter to load.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSaveAndLoadFilter_Integration()
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

				var savedFilter = form.Controls.Find("SavedFilterComboBox", true)[0] as KComboBox;
				AssertEquals(0, savedFilter.Items.Count);

				var addButton = form.GetControl<ZButton>("AddFilterButton");
				addButton.PerformClick();
				addButton.PerformClick();
				addButton.PerformClick();

				var filterItemUserControls = form.Controls.Find("FilterItemUserControl", true).Cast<FilterItemUserControl>().ToList();
				AssertEquals(4, filterItemUserControls.Count);
				filterItemUserControls[0].OrgFilterItemDataSource.OrgFilterType = OrgFilterTypeList.Descriptions.MainUNLOCO;
				filterItemUserControls[0].OrgFilterItemDataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.Contains;
				filterItemUserControls[0].OrgFilterItemDataSource.OrgFilterKeyword = "测试";

				filterItemUserControls[1].OrgFilterItemDataSource.OrgFilterType = OrgFilterTypeList.Descriptions.OrgTypes;
				GetSpecifiedCheckBox(filterItemUserControls[1], "Consignee").Checked = true;
				GetSpecifiedCheckBox(filterItemUserControls[1], "Payables").Checked = true;

				filterItemUserControls[2].OrgFilterItemDataSource.OrgFilterType = OrgFilterTypeList.Descriptions.Name;
				filterItemUserControls[2].OrgFilterItemDataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.StartsWith;
				filterItemUserControls[2].OrgFilterItemDataSource.OrgFilterKeyword = "A";

				filterItemUserControls[3].OrgFilterItemDataSource.OrgFilterType = OrgFilterTypeList.Descriptions.Email;
				filterItemUserControls[3].OrgFilterItemDataSource.OrgFilterOption = OrgFilterOptionList.Descriptions.NotEqual;
				filterItemUserControls[3].OrgFilterItemDataSource.OrgFilterKeyword = "B";

				var filterName = form.Controls.Find("FilterNameTextBox", true)[0] as ZTextBox;
				filterName.Text = "All 4 filters";
				var existingFilters = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, FilterOperationUserControl.OrgDynamicFilterKey));
				AssertEquals(false, existingFilters.Any());
				var savedFilterComboBox = form.GetControl<KComboBox>("SavedFilterComboBox");
				AssertEquals(0, savedFilterComboBox.Items.Count);
				UnitTestUserNotification.Instance.ClearMessages();

				form.GetControl<ZButton>("SaveButton").PerformClick();
				AssertEquals("The filter is saved as 'All 4 filters' successfully.", UnitTestUserNotification.Instance.LastMessage.Text);
				existingFilters = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, FilterOperationUserControl.OrgDynamicFilterKey));
				AssertEquals(1, existingFilters.Length);
				AssertContains(FilterOperationUserControl.OrgDynamicFilterKey + "All 4 filters", existingFilters[0].SD_Name);

				savedFilterComboBox = form.GetControl<KComboBox>("SavedFilterComboBox");
				AssertEquals(1, savedFilterComboBox.Items.Count);
				AssertEquals("All 4 filters", savedFilterComboBox.GetItemText(savedFilterComboBox.Items[0]));

				for (var i = filterItemUserControls.Count - 1; i >= 0; i--)
				{
					filterItemUserControls[i].Dispose();
				}

				filterItemUserControls = form.Controls.Find("FilterItemUserControl", true).Cast<FilterItemUserControl>().ToList();
				AssertEquals(false, filterItemUserControls.Any());

				savedFilterComboBox.SelectedIndex = 0;
				form.GetControl<ZButton>("LoadButton").PerformClick();
				filterItemUserControls = form.Controls.Find("FilterItemUserControl", true).Cast<FilterItemUserControl>().ToList();
				AssertEquals(4, filterItemUserControls.Count);

				var dataSource_0 = filterItemUserControls[0].OrgFilterItemDataSource;
				var dataSource_1 = filterItemUserControls[1].OrgFilterItemDataSource;
				var dataSource_2 = filterItemUserControls[2].OrgFilterItemDataSource;
				var dataSource_3 = filterItemUserControls[3].OrgFilterItemDataSource;
				CombineAssertions(() =>
				{
					AssertEquals(OrgFilterTypeList.Descriptions.MainUNLOCO, dataSource_0.OrgFilterTypeDescription);
					AssertEquals(OrgFilterOptionList.Descriptions.Contains, dataSource_0.OrgFilterOptionDescription);
					AssertEquals("测试", dataSource_0.OrgFilterKeyword);

					AssertEquals(OrgFilterTypeList.Descriptions.OrgTypes, dataSource_1.OrgFilterTypeDescription);
					GetSpecifiedCheckBox(filterItemUserControls[1], "Consignee").Checked = true;
					GetSpecifiedCheckBox(filterItemUserControls[1], "Payables").Checked = true;

					AssertEquals(OrgFilterTypeList.Descriptions.Name, dataSource_2.OrgFilterTypeDescription);
					AssertEquals(OrgFilterOptionList.Descriptions.StartsWith, dataSource_2.OrgFilterOptionDescription);
					AssertEquals("A", dataSource_2.OrgFilterKeyword);

					AssertEquals(OrgFilterTypeList.Descriptions.Email, dataSource_3.OrgFilterTypeDescription);
					AssertEquals(OrgFilterOptionList.Descriptions.NotEqual, dataSource_3.OrgFilterOptionDescription);
					AssertEquals("B", dataSource_3.OrgFilterKeyword);
				});

				UnitTestUserNotification.Instance.ClearMessages();

				form.GetControl<ZButton>("SaveButton").PerformClick();
				AssertEquals("The filter can't be saved as 'All 4 filters' because another saved filter has already used the same name.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteFilter_ConfirmationMessage()
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

				var filterName = form.Controls.Find("FilterNameTextBox", true)[0] as ZTextBox;
				filterName.Text = "Test Filter";

				UnitTestUserNotification.Instance.ClearMessages();

				form.GetControl<ZButton>("SaveButton").PerformClick();
				AssertEquals("The filter is saved as 'Test Filter' successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

				var savedFilterComboBox = form.GetControl<KComboBox>("SavedFilterComboBox");
				AssertEquals(1, savedFilterComboBox.Items.Count);

				savedFilterComboBox.SelectedIndex = 0;
				var deleteButton = form.GetControl<ZButton>("DeleteButton");
				deleteButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessages();

				deleteButton.PerformClick();
				AssertEquals("Are you sure you want to delete the Dynamic Filter 'Test Filter'?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteFilter_Successful()
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

				var filterName = form.Controls.Find("FilterNameTextBox", true)[0] as ZTextBox;
				filterName.Text = "Test Filter";

				UnitTestUserNotification.Instance.ClearMessages();

				form.GetControl<ZButton>("SaveButton").PerformClick();
				AssertEquals("The filter is saved as 'Test Filter' successfully.", UnitTestUserNotification.Instance.LastMessage.Text);

				var savedFilterComboBox = form.GetControl<KComboBox>("SavedFilterComboBox");
				AssertEquals(1, savedFilterComboBox.Items.Count);
				var existingFilters = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, FilterOperationUserControl.OrgDynamicFilterKey + "Test Filter"));
				AssertEquals(1, existingFilters.Length);

				savedFilterComboBox.SelectedIndex = 0;
				var deleteButton = form.GetControl<ZButton>("DeleteButton");
				deleteButton.PerformClick();

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				deleteButton.PerformClick();
				savedFilterComboBox = form.GetControl<KComboBox>("SavedFilterComboBox");
				AssertEquals(0, savedFilterComboBox.Items.Count);
				existingFilters = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, SQLComparisonOperator.StartsWith, FilterOperationUserControl.OrgDynamicFilterKey + "Test Filter"));
				AssertEquals(false, existingFilters.Any());
				AssertEquals("The Dynamic Filter 'Test Filter' has been deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteFilterWithEmptyName()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var resultDetail = new DeduplicationOrganisationResultDetailForTest();
				resultDetail.SetIsEmptyOrNotForTesting(false);
				potentialDuplicatesUserControl.SetupDataContext(resultDetail, true);

				var savedFilterComboBox = form.GetControl<KComboBox>("SavedFilterComboBox");
				AssertEquals(0, savedFilterComboBox.Items.Count);

				var deleteButton = form.GetControl<ZButton>("DeleteButton");
				UnitTestUserNotification.Instance.ClearMessages();

				deleteButton.PerformClick();
				AssertEquals("Please select a saved Dynamic Filter to delete.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestFindFilteredRecords_UNLOCO_Name_EmialFilterWithDifferentFilterOptions()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_RL_NKClosestPort = "AUSYD";
			header.OH_FullName = "TEST ABC";
			header.Addresses[0].OA_Email = "AAA@BBB.com";
			Factory.Save();
			var candidate = new DuplicationOrganisationCandidate(null, new DeduplicationOrgHeader(header));

			using (var form = new ZForm())
			{
				var detailUserControl = new DeduplicationResultsViewerDetailsUserControl();
				form.Controls.Add(detailUserControl);
				form.Show();

				var duplicationResultDetail = new DeduplicationOrganisationResultDetailForTest { CandidatesCount = 1 };
				duplicationResultDetail.SetIsEmptyOrNotForTesting(false);
				duplicationResultDetail.DuplicationCandidates.Add(candidate);

				var unfilteredCandidates = new List<DuplicationOrganisationCandidate> { candidate };
				duplicationResultDetail.UnfilteredResults = unfilteredCandidates;

				var potentialDuplicatesUserControl = form.GetControl<PotentialDuplicatesUserControl>("PotentialDuplicatesUserControl");
				potentialDuplicatesUserControl.SetupDataContext(duplicationResultDetail, true);

				var operationUserControl = form.GetControl<FilterOperationUserControl>("FilterOperationUserControl");
				operationUserControl.AdvancedFilterCriteriaControl = form.GetControl<AdvancedFilterCriteriaControl>("AdvancedFilterCriteriaControl");
				operationUserControl.DuplicationResultDetail = duplicationResultDetail;
				operationUserControl.PotentialDuplicatesUserControl = form.GetControl<PotentialDuplicatesUserControl>("PotentialDuplicatesUserControl");
				duplicationResultDetail.GetDynamicFilterItemUserControls = () => operationUserControl.AdvancedFilterCriteriaControl.OrganisationFilterContentControl.DynamicFilterPanel.Controls.OfType<FilterItemUserControl>().ToList();

				var filterItemUserControls = form.Controls.Find("FilterItemUserControl", true).Cast<FilterItemUserControl>().ToList();
				AssertEquals(1, filterItemUserControls.Count);
				var filterItemSource = filterItemUserControls[0].OrgFilterItemDataSource;
				filterItemSource.OrgFilterTypeDescription = OrgFilterTypeList.Descriptions.MainUNLOCO;
				filterItemSource.OrgFilterOptionDescription = OrgFilterOptionList.Descriptions.Contains;
				filterItemSource.OrgFilterKeyword = "SY";

				var findButton = form.GetControl<ZButton>("FindButton");
				AssertEquals("Precondition: ", 1, duplicationResultDetail.DuplicationCandidates.Count);

				findButton.PerformClick();
				AssertEquals(1, duplicationResultDetail.DuplicationCandidates.Count);

				filterItemSource.OrgFilterOptionDescription = OrgFilterOptionList.Descriptions.NotContain;
				findButton.PerformClick();
				AssertEquals(false, duplicationResultDetail.DuplicationCandidates.Any());

				filterItemSource.OrgFilterTypeDescription = OrgFilterTypeList.Descriptions.Name;
				filterItemSource.OrgFilterOptionDescription = OrgFilterOptionList.Descriptions.StartsWith;
				filterItemSource.OrgFilterKeyword = "TES";
				findButton.PerformClick();
				AssertEquals(1, duplicationResultDetail.DuplicationCandidates.Count);

				filterItemSource.OrgFilterOptionDescription = OrgFilterOptionList.Descriptions.NotStartWith;
				findButton.PerformClick();
				AssertEquals(false, duplicationResultDetail.DuplicationCandidates.Any());

				filterItemSource.OrgFilterTypeDescription = OrgFilterTypeList.Descriptions.Email;
				filterItemSource.OrgFilterOptionDescription = OrgFilterOptionList.Descriptions.ExactMatch;
				filterItemSource.OrgFilterKeyword = "AAA@BBB.com";
				findButton.PerformClick();
				AssertEquals(1, duplicationResultDetail.DuplicationCandidates.Count);

				filterItemSource.OrgFilterOptionDescription = OrgFilterOptionList.Descriptions.NotEqual;
				findButton.PerformClick();
				AssertEquals(false, duplicationResultDetail.DuplicationCandidates.Any());
			}
		}
		public void TestFindFilteredRecords_SingleCheckBoxFilter()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_IsConsignee = true;
			header.OH_IsConsignor = true;
			header.OH_IsBroker = false;
			Factory.Save();

			var candidate = new DuplicationOrganisationCandidate(null, new DeduplicationOrgHeader(header));

			using (var form = new ZForm())
			{
				var detailUserControl = new DeduplicationResultsViewerDetailsUserControl();
				form.Controls.Add(detailUserControl);
				form.Controls.Add(detailUserControl);
				form.Show();

				var duplicationResultDetail = new DeduplicationOrganisationResultDetailForTest { CandidatesCount = 1 };
				duplicationResultDetail.SetIsEmptyOrNotForTesting(false);
				duplicationResultDetail.DuplicationCandidates.Add(candidate);

				var unfilteredCandidates = new List<DuplicationOrganisationCandidate> { candidate };
				duplicationResultDetail.UnfilteredResults = unfilteredCandidates;

				var potentialDuplicatesUserControl = form.GetControl<PotentialDuplicatesUserControl>("PotentialDuplicatesUserControl");
				potentialDuplicatesUserControl.SetupDataContext(duplicationResultDetail, true);

				var operationUserControl = form.GetControl<FilterOperationUserControl>("FilterOperationUserControl");
				operationUserControl.AdvancedFilterCriteriaControl = form.GetControl<AdvancedFilterCriteriaControl>("AdvancedFilterCriteriaControl");
				operationUserControl.DuplicationResultDetail = duplicationResultDetail;
				operationUserControl.PotentialDuplicatesUserControl = form.GetControl<PotentialDuplicatesUserControl>("PotentialDuplicatesUserControl");
				duplicationResultDetail.GetDynamicFilterItemUserControls = () => operationUserControl.AdvancedFilterCriteriaControl.OrganisationFilterContentControl.DynamicFilterPanel.Controls.OfType<FilterItemUserControl>().ToList();

				var filterItemUserControls = form.Controls.Find("FilterItemUserControl", true).Cast<FilterItemUserControl>().ToList();
				AssertEquals(1, filterItemUserControls.Count);
				filterItemUserControls[0].OrgFilterItemDataSource.OrgFilterTypeDescription = OrgFilterTypeList.Descriptions.OrgTypes;
				GetSpecifiedCheckBox(filterItemUserControls[0], "Consignee").Checked = true;

				var findButton = form.GetControl<ZButton>("FindButton");
				AssertEquals("Precondition: ", 1, duplicationResultDetail.DuplicationCandidates.Count);

				findButton.PerformClick();
				AssertEquals(1, duplicationResultDetail.DuplicationCandidates.Count);

				GetSpecifiedCheckBox(filterItemUserControls[0], "Consignor").Checked = true;
				findButton.PerformClick();
				AssertEquals(1, duplicationResultDetail.DuplicationCandidates.Count);

				GetSpecifiedCheckBox(filterItemUserControls[0], "Broker").Checked = true;
				findButton.PerformClick();
				AssertEquals(false, duplicationResultDetail.DuplicationCandidates.Any());
			}
		}

		public void TestFindFilteredRecords_MultipleFilters()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			header1.OH_FullName = "TEST ABC";
			header1.OH_IsConsignee = true;
			header1.OH_IsCompetitor = true;
			header1.OH_IsBroker = false;

			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			header2.OH_FullName = "TEST DEF";
			header2.OH_IsConsignee = true;
			header2.OH_IsCompetitor = false;
			header2.OH_IsBroker = true;
			Factory.Save();

			var candidate1 = new DuplicationOrganisationCandidate(null, new DeduplicationOrgHeader(header1));
			var candidate2 = new DuplicationOrganisationCandidate(null, new DeduplicationOrgHeader(header2));

			using (var form = new ZForm())
			{
				var detailUserControl = new DeduplicationResultsViewerDetailsUserControl();
				form.Controls.Add(detailUserControl);

				var duplicationResultDetail = new DeduplicationOrganisationResultDetailForTest { CandidatesCount = 1 };
				duplicationResultDetail.SetIsEmptyOrNotForTesting(false);
				duplicationResultDetail.DuplicationCandidates.Add(candidate1);
				duplicationResultDetail.DuplicationCandidates.Add(candidate2);

				var unfilteredCandidates = new List<DuplicationOrganisationCandidate> { candidate1, candidate2 };
				duplicationResultDetail.UnfilteredResults = unfilteredCandidates;
				form.Controls.Add(detailUserControl);
				form.Show();

				var potentialDuplicatesUserControl = form.GetControl<PotentialDuplicatesUserControl>("PotentialDuplicatesUserControl");
				potentialDuplicatesUserControl.SetupDataContext(duplicationResultDetail, true);

				var operationUserControl = form.GetControl<FilterOperationUserControl>("FilterOperationUserControl");
				operationUserControl.AdvancedFilterCriteriaControl = form.GetControl<AdvancedFilterCriteriaControl>("AdvancedFilterCriteriaControl");
				operationUserControl.DuplicationResultDetail = duplicationResultDetail;
				operationUserControl.PotentialDuplicatesUserControl = form.GetControl<PotentialDuplicatesUserControl>("PotentialDuplicatesUserControl");
				duplicationResultDetail.GetDynamicFilterItemUserControls = () => operationUserControl.AdvancedFilterCriteriaControl.OrganisationFilterContentControl.DynamicFilterPanel.Controls.OfType<FilterItemUserControl>().ToList();

				var addButton = form.GetControl<ZButton>("AddFilterButton");
				addButton.PerformClick();

				var filterItemUserControls = form.Controls.Find("FilterItemUserControl", true).Cast<FilterItemUserControl>().ToList();
				AssertEquals(2, filterItemUserControls.Count);

				filterItemUserControls[0].OrgFilterItemDataSource.OrgFilterTypeDescription = OrgFilterTypeList.Descriptions.OrgTypes;
				GetSpecifiedCheckBox(filterItemUserControls[0], "Consignee").Checked = true;

				var nameFilterItemSource = filterItemUserControls[1].OrgFilterItemDataSource;
				nameFilterItemSource.OrgFilterTypeDescription = OrgFilterTypeList.Descriptions.Name;
				nameFilterItemSource.OrgFilterOptionDescription = OrgFilterOptionList.Descriptions.StartsWith;
				nameFilterItemSource.OrgFilterKeyword = "TEST";

				var findButton = form.GetControl<ZButton>("FindButton");
				AssertEquals("Precondition: ", 2, duplicationResultDetail.DuplicationCandidates.Count);

				findButton.PerformClick();
				AssertEquals(2, duplicationResultDetail.DuplicationCandidates.Count);

				nameFilterItemSource.OrgFilterKeyword = "A";
				findButton.PerformClick();
				AssertEquals(false, duplicationResultDetail.DuplicationCandidates.Any());

				nameFilterItemSource.OrgFilterKeyword = "TEST";
				GetSpecifiedCheckBox(filterItemUserControls[0], "Competitor").Checked = true;
				findButton.PerformClick();
				AssertEquals(1, duplicationResultDetail.DuplicationCandidates.Count);
				AssertEquals(candidate1, duplicationResultDetail.DuplicationCandidates[0]);

				GetSpecifiedCheckBox(filterItemUserControls[0], "Competitor").Checked = false;
				GetSpecifiedCheckBox(filterItemUserControls[0], "Broker").Checked = true;

				findButton.PerformClick();
				AssertEquals(1, duplicationResultDetail.DuplicationCandidates.Count);
				AssertEquals(candidate2, duplicationResultDetail.DuplicationCandidates[0]);
			}
		}

		public void TestFilterFactoryDoesNotSaveDataInOtherFactory()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var testOrg = Factory.NewWithValidTestData<OrgHeader>();
				var masterOrg = new DeduplicationOrgHeader(testOrg);
				var resultDetail = new DeduplicationOrganisationResultDetailForTest(masterOrg);
				potentialDuplicatesUserControl.SetupDataContext(resultDetail, false);

				var operationUserControl = form.GetControl<FilterOperationUserControl>("FilterOperationUserControl");
				var filterName = form.Controls.Find("FilterNameTextBox", true)[0] as ZTextBox;
				filterName.Text = "ToBeSaved";

				var orgNotToBeSaved = operationUserControl.DuplicationResultDetail.Factory.NewWithValidTestData<OrgHeader>();
				orgNotToBeSaved.OH_Code = "NotToBeSaved";

				form.GetControl<ZButton>("SaveButton").PerformClick();
				var filters = Factory.Load<StmData>(new ZQuery(StmDataSchema.SD_Name, "MDMPanelDynamicOrganizationFiltersKey_ToBeSaved"));
				AssertEquals(1, filters.Length);
				AssertEquals(true, filters[0].IsInDatabase);

				var org = Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, "NotToBeSaved");
				AssertEquals(false, org.IsInDatabase);
			}
		}

		#region Implementation

		ZCheckBox GetSpecifiedCheckBox(FilterItemUserControl control, string name)
		{
			return control.FindSingle<ZCheckBox>(u => u.Tag.ToString() == name);
		}

		#endregion
	}
}
