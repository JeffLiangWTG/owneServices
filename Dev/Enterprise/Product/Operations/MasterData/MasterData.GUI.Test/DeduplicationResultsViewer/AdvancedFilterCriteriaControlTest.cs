using CargoWise.Windows.UI;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterData.GUI.Test
{
	public class AdvancedFilterCriteriaControlTest : TestCase
	{
		public void TestExpandAndCollapseFilterArea()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var expandCollapseButton = potentialDuplicatesUserControl.FindSingle<ZButton>("ExpandCollapseButton");
				var tableLayoutPanel = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.FindSingle<KTableLayoutPanel>("MainTableLayoutPanel");
				CombineAssertions(() =>
				{
					AssertEquals(2, tableLayoutPanel.Controls.Count);
					AssertEquals("⯅ ", expandCollapseButton.Text);
				});

				expandCollapseButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, tableLayoutPanel.Controls.Count);
					AssertEquals("⯆ ", expandCollapseButton.Text);
				});

				expandCollapseButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(2, tableLayoutPanel.Controls.Count);
					AssertEquals("⯅ ", expandCollapseButton.Text);
				});
			}
		}

		public void TestSetupDataContextWithOrganisationResultDetail()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				var deduplicationOrganisationResultDetail = new DeduplicationOrganisationResultDetailForTest();
				deduplicationOrganisationResultDetail.SetIsEmptyOrNotForTesting(false);

				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var tableLayoutPanel = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.FindSingle<KTableLayoutPanel>("MainTableLayoutPanel");
				var contentPanel = tableLayoutPanel.FindSingle<KTableLayoutPanel>("ContentPanel");
				var orgFilterContent = contentPanel.FindSingleOrDefault<OrganisationFilterContentControl>("OrganisationFilterContentControl");
				var personFilterContent = contentPanel.FindSingleOrDefault<PersonFilterContentControl>("PersonFilterContentControl");

				CombineAssertions(() =>
				{
					AssertNull(orgFilterContent);
					AssertNull(personFilterContent);
				});

				potentialDuplicatesUserControl.SetupDataContext(deduplicationOrganisationResultDetail, true);
				orgFilterContent = contentPanel.FindSingleOrDefault<OrganisationFilterContentControl>("OrganisationFilterContentControl");
				personFilterContent = contentPanel.FindSingleOrDefault<PersonFilterContentControl>("PersonFilterContentControl");
				CombineAssertions(() =>
				{
					AssertNotNull(orgFilterContent);
					AssertNull(personFilterContent);
				});
			}
		}

		public void TestSetupDataContextWithPersonResultDetail()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				var deduplicationPersonResultDetailForTest = new DeduplicationPersonResultDetailForTest();
				deduplicationPersonResultDetailForTest.SetIsEmptyOrNotForTesting(false);

				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var tableLayoutPanel = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.FindSingle<KTableLayoutPanel>("MainTableLayoutPanel");
				var contentPanel = tableLayoutPanel.FindSingle<KTableLayoutPanel>("ContentPanel");
				var orgFilterContent = contentPanel.FindSingleOrDefault<OrganisationFilterContentControl>("OrganisationFilterContentControl");
				var personFilterContent = contentPanel.FindSingleOrDefault<PersonFilterContentControl>("PersonFilterContentControl");

				CombineAssertions(() =>
				{
					AssertNull(orgFilterContent);
					AssertNull(personFilterContent);
				});

				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetailForTest, true);
				orgFilterContent = contentPanel.FindSingleOrDefault<OrganisationFilterContentControl>("OrganisationFilterContentControl");
				personFilterContent = contentPanel.FindSingleOrDefault<PersonFilterContentControl>("PersonFilterContentControl");
				CombineAssertions(() =>
				{
					AssertNull(orgFilterContent);
					AssertNotNull(personFilterContent);
				});
			}
		}
	}
}
