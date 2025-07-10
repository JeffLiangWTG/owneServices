using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector.Standard.Common;
using CargoWise.Windows.UI;
using Enterprise.MasterData.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterData.GUI.Test
{
	internal class PersonFilterContentControlTest : TestCaseWithFactory
	{
		public void TestControlBindingSource()
		{
			using (var personFilterContentControl = new PersonFilterContentControl())
			{
				AssertEquals(nameof(PersonFilterDataSource), personFilterContentControl.BindingSource.DataSourceType.Name);

				var dropEditActiveStatus = personFilterContentControl.FindSingle<ZDropEdit>("DropEditActiveStatus");
				AssertEquals(dropEditActiveStatus.GetBindingMember(), "PersonFilterActiveDescription");

				var dropEditEmail = personFilterContentControl.FindSingle<ZDropEdit>("DropEditEmail");
				AssertEquals(dropEditEmail.GetBindingMember(), "PersonFilterEmailDescription");

				var textBoxEmail = personFilterContentControl.FindSingle<ZTextBox>("TextBoxEmail");
				AssertEquals(textBoxEmail.GetBindingMember(), "PersonFilterEmailKeyword");

				var dropEditName = personFilterContentControl.FindSingle<ZDropEdit>("DropEditName");
				AssertEquals(dropEditName.GetBindingMember(), "PersonFilterNameDescription");

				var textBoxName = personFilterContentControl.FindSingle<ZTextBox>("TextBoxName");
				AssertEquals(textBoxName.GetBindingMember(), "PersonFilterNameKeyword");

				var dropEditPhone = personFilterContentControl.FindSingle<ZDropEdit>("DropEditPhone");
				AssertEquals(dropEditPhone.GetBindingMember(), "PersonFilterPhoneDescription");

				var textBoxPhone = personFilterContentControl.FindSingle<ZTextBox>("TextBoxPhone");
				AssertEquals(textBoxPhone.GetBindingMember(), "PersonFilterPhoneKeyword");
			}
		}

		public void TestFilterActiveStatus()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();

				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var deduplicationPersonResultDetail = GetDeduplicationPersonResultDetail();
				deduplicationPersonResultDetail.IsExcludingInactiveResults = false;
				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetail, true);

				var personFilterContentControl = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.PersonFilterContentControl;
				var dataSource = (PersonFilterDataSource)personFilterContentControl.BindingSource.DataSource;
				var findButton = personFilterContentControl.FindSingle<ZButton>("FindButton");

				dataSource.PersonFilterActive = PersonFilterActivesList.Descriptions.All.GetUnresolvedString();
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(3, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[2].Name);
				});

				dataSource.PersonFilterActive = PersonFilterActivesList.Descriptions.Active.GetUnresolvedString();
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(2, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
				});

				dataSource.PersonFilterActive = PersonFilterActivesList.Descriptions.Inactive.GetUnresolvedString();
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});

				deduplicationPersonResultDetail = GetDeduplicationPersonResultDetail();
				deduplicationPersonResultDetail.DuplicationCandidates.Remove(deduplicationPersonResultDetail.DuplicationCandidates[2]);
				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetail, true);
				dataSource.PersonFilterActive = "not exists word";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(0, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				});
			}
		}

		public void TestMutilConfidenceFilter()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var deduplicationPersonResultDetail = GetDeduplicationPersonResultDetail();
				deduplicationPersonResultDetail.IsExcludingInactiveResults = false;
				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetail, true);

				var personFilterContentControl = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.PersonFilterContentControl;
				var checkBoxHigh = personFilterContentControl.FindSingle<ZCheckBox>("CheckBoxHigh");
				var checkBoxMedium = personFilterContentControl.FindSingle<ZCheckBox>("CheckBoxMedium");
				var checkBoxLow = personFilterContentControl.FindSingle<ZCheckBox>("CheckBoxLow");
				var findButton = personFilterContentControl.FindSingle<ZButton>("FindButton");

				checkBoxHigh.Checked = true;
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});

				checkBoxMedium.Checked = true;
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(2, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
				});

				checkBoxLow.Checked = true;
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(3, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[2].Name);
				});

				checkBoxHigh.Checked = false;
				checkBoxMedium.Checked = false;
				checkBoxLow.Checked = false;
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(3, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[2].Name);
				});

				checkBoxHigh.Checked = true;
				checkBoxMedium.Checked = true;
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(2, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
				});

				checkBoxHigh.Checked = true;
				checkBoxMedium.Checked = false;
				checkBoxLow.Checked = true;
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(2, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
				});
			}
		}

		public void TestFilterEmail()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				var deduplicationPersonResultDetail = GetDeduplicationPersonResultDetail();
				deduplicationPersonResultDetail.IsExcludingInactiveResults = false;
				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetail, true);
				var personFilterContentControl = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.PersonFilterContentControl;
				var dataSource = (PersonFilterDataSource)personFilterContentControl.BindingSource.DataSource;
				var findButton = personFilterContentControl.FindSingle<ZButton>("FindButton");

				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				dataSource.PersonFilterEmail = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();
				dataSource.PersonFilterEmailKeyword = "outlook";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(3, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[2].Name);
				});

				dataSource.PersonFilterEmailKeyword = "test1.email";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});

				dataSource.PersonFilterEmail = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(0, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				dataSource.PersonFilterEmail = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				dataSource.PersonFilterEmailKeyword = "test1.email@outlook.com";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});
			}
		}

		public void TestFilterName()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				var deduplicationPersonResultDetail = GetDeduplicationPersonResultDetail();
				deduplicationPersonResultDetail.IsExcludingInactiveResults = false;
				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetail, true);
				var personFilterContentControl = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.PersonFilterContentControl;
				var dataSource = (PersonFilterDataSource)personFilterContentControl.BindingSource.DataSource;
				var findButton = personFilterContentControl.FindSingle<ZButton>("FindButton");

				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				dataSource.PersonFilterName = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();
				dataSource.PersonFilterNameKeyword = "Test";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(3, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[2].Name);
				});

				dataSource.PersonFilterNameKeyword = "A";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});

				dataSource.PersonFilterName = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(0, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				dataSource.PersonFilterName = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				dataSource.PersonFilterNameKeyword = "Test A";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});
			}
		}

		public void TestFilterPhone()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				var deduplicationPersonResultDetail = GetDeduplicationPersonResultDetail();
				deduplicationPersonResultDetail.IsExcludingInactiveResults = false;
				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetail, true);
				var personFilterContentControl = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.PersonFilterContentControl;
				var dataSource = (PersonFilterDataSource)personFilterContentControl.BindingSource.DataSource;
				var findButton = personFilterContentControl.FindSingle<ZButton>("FindButton");

				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				dataSource.PersonFilterPhone = PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString();
				dataSource.PersonFilterPhoneKeyword = "0";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(3, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[1].Name);
					AssertEquals("Test C", deduplicationPersonResultDetail.DuplicationCandidates[2].Name);
				});

				dataSource.PersonFilterPhoneKeyword = "011";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test A", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});

				dataSource.PersonFilterPhone = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				dataSource.PersonFilterPhoneKeyword = "0";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(0, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("There are no records that match your search.", UnitTestUserNotification.Instance.LastMessage.Text);
				});

				dataSource.PersonFilterPhone = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				dataSource.PersonFilterPhoneKeyword = "0222222";
				findButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(1, deduplicationPersonResultDetail.DuplicationCandidates.Count);
					AssertEquals("Test B", deduplicationPersonResultDetail.DuplicationCandidates[0].Name);
				});
			}
		}

		public void TestClearFilter()
		{
			using (var form = new ZForm())
			{
				var potentialDuplicatesUserControl = new PotentialDuplicatesUserControl();
				form.Controls.Add(potentialDuplicatesUserControl);
				form.Show();

				var deduplicationPersonResultDetail = GetDeduplicationPersonResultDetail();
				potentialDuplicatesUserControl.SetupDataContext(deduplicationPersonResultDetail, true);
				var personFilterContentControl = potentialDuplicatesUserControl.AdvancedFilterCriteriaControl.PersonFilterContentControl;

				var dataSource = (PersonFilterDataSource)personFilterContentControl.BindingSource.DataSource;
				var clearButton = personFilterContentControl.FindSingle<ZButton>("ClearButton");
				var checkBoxHigh = personFilterContentControl.FindSingle<ZCheckBox>("CheckBoxHigh");
				var checkBoxMedium = personFilterContentControl.FindSingle<ZCheckBox>("CheckBoxMedium");
				var checkBoxLow = personFilterContentControl.FindSingle<ZCheckBox>("CheckBoxLow");

				dataSource.PersonFilterActive = PersonFilterActivesList.Descriptions.Inactive.GetUnresolvedString();
				dataSource.PersonFilterEmail = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				dataSource.PersonFilterName = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				dataSource.PersonFilterPhone = PersonFilterOptionsList.Descriptions.ExactMatch.GetUnresolvedString();
				dataSource.PersonFilterEmailKeyword = "abcd";
				dataSource.PersonFilterNameKeyword = "abcd";
				dataSource.PersonFilterPhoneKeyword = "111";
				checkBoxHigh.Checked = true;
				checkBoxMedium.Checked = true;
				checkBoxLow.Checked = true;

				clearButton.PerformClick();
				CombineAssertions(() =>
				{
					AssertEquals(PersonFilterActivesList.Descriptions.All.GetUnresolvedString(), dataSource.PersonFilterActive);
					AssertEquals(PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString(), dataSource.PersonFilterEmail);
					AssertEquals(PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString(), dataSource.PersonFilterName);
					AssertEquals(PersonFilterOptionsList.Descriptions.Contains.GetUnresolvedString(), dataSource.PersonFilterPhone);
					AssertEquals(string.Empty, dataSource.PersonFilterEmailKeyword);
					AssertEquals(string.Empty, dataSource.PersonFilterNameKeyword);
					AssertEquals(string.Empty, dataSource.PersonFilterPhoneKeyword);
					Assert(!checkBoxHigh.Checked);
					Assert(!checkBoxMedium.Checked);
					Assert(!checkBoxLow.Checked);
				});
			}
		}

		DeduplicationPersonResultDetail GetDeduplicationPersonResultDetail()
		{
			var deduplicationPersonResultDetail = new DeduplicationPersonResultDetailForTest();
			deduplicationPersonResultDetail.SetIsEmptyOrNotForTesting(false);

			var glbPerson1 = Factory.NewWithValidTestData<GlbPerson>();
			glbPerson1.PER_IsActive = true;
			glbPerson1.PER_FullName = "Test A";
			glbPerson1.PER_EmailAddress = "test1.email@outlook.com";
			glbPerson1.PER_EmailAddress2 = "test2.email@outlook.com";
			glbPerson1.PER_MobilePhone = "011111111";
			glbPerson1.PER_MobilePhone2 = "0111111";
			glbPerson1.PER_HomePhone = "01111";
			glbPerson1.PER_FaxNumber = "011";

			var glbPerson2 = Factory.NewWithValidTestData<GlbPerson>();
			glbPerson2.PER_IsActive = true;
			glbPerson2.PER_FullName = "Test B";
			glbPerson2.PER_EmailAddress = "test3.email@outlook.com";
			glbPerson2.PER_EmailAddress2 = "test4.email@outlook.com";
			glbPerson2.PER_MobilePhone = "022222222";
			glbPerson2.PER_MobilePhone2 = "0222222";
			glbPerson2.PER_HomePhone = "02222";
			glbPerson2.PER_FaxNumber = "022";

			var glbPerson3 = Factory.NewWithValidTestData<GlbPerson>();
			glbPerson3.PER_IsActive = false;
			glbPerson3.PER_FullName = "Test C";
			glbPerson3.PER_EmailAddress = "test3.email@outlook.com";
			glbPerson3.PER_EmailAddress2 = "test4.email@outlook.com";
			glbPerson3.PER_MobilePhone = "033333333";
			glbPerson3.PER_MobilePhone2 = "03333333";
			glbPerson3.PER_HomePhone = "03333";
			glbPerson3.PER_FaxNumber = "033";

			var model1 = new DeduplicationPresenterModel() { Confidence = ConfidenceRating.High };
			var model2 = new DeduplicationPresenterModel() { Confidence = ConfidenceRating.Medium };
			var model3 = new DeduplicationPresenterModel() { Confidence = ConfidenceRating.Low };

			var duplicationPersonCandidate1 = new DuplicationPersonCandidate(model1, new DeduplicationGlbPerson(glbPerson1));
			duplicationPersonCandidate1.TargetPK = Guid.NewGuid();
			var duplicationPersonCandidate2 = new DuplicationPersonCandidate(model2, new DeduplicationGlbPerson(glbPerson2));
			duplicationPersonCandidate2.TargetPK = Guid.NewGuid();
			var duplicationPersonCandidate3 = new DuplicationPersonCandidate(model3, new DeduplicationGlbPerson(glbPerson3));
			duplicationPersonCandidate2.TargetPK = Guid.NewGuid();

			deduplicationPersonResultDetail.DuplicationCandidatesForTest = new[] { duplicationPersonCandidate1, duplicationPersonCandidate2, duplicationPersonCandidate3 };
			deduplicationPersonResultDetail.UnfilteredResults = deduplicationPersonResultDetail.DuplicationCandidatesForTest;

			return deduplicationPersonResultDetail;
		}
	}
}
