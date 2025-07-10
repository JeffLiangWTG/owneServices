using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	class ScreenedPartyUserControlTest : TestCaseWithFactory
	{
		int index;
		readonly int ScreenedPartiesCount = 2;

		public void TestShowAndHideNavigationBar()
		{
			var winModel = new DpsResultWinModel(DpsResultWinModelTest.CreateResultModel(Factory));

			using (var form = new ZForm())
			using (var userControl = new DpsResultUserControl())
			{
				var screenedParty = winModel.ScreenedParties;
				using (var userControl1 = new ScreenedPartyUserControl())
				{
					form.Controls.Add(userControl);
					userControl1.SetDataBinding(screenedParty.First(), "");
					form.Show();
					AssertEquals(userControl1.NavigationLabel.Visible, true);
				}
			}

			winModel = new DpsResultWinModel(DpsResultWinModelTest.CreateResultModel1(Factory));
			using (var form = new ZForm())
			using (var userControl = new DpsResultUserControl())
			{
				var screenedParty = winModel.ScreenedParties;
				using (var userControl1 = new ScreenedPartyUserControl())
				{
					form.Controls.Add(userControl);
					userControl1.SetDataBinding(screenedParty.First(), "");
					form.Show();
					AssertEquals(userControl1.NavigationPanel.Visible, false);
				}
			}
		}

		public void TestNavigationBar()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var screenedPartyModel = new ScreenedPartyModel(new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse(), new DpsRequestHeaderWithAddressMatching()), Factory);
			var winModel = new ScreenedPartyWinModel(screenedPartyModel, Navigate, null, true);

			using (var form = new ZForm())
			using (var userControl = new ScreenedPartyUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();
				userControl.NextButton_ClickForTest(null, null);
				AssertEquals(index, 1);
				userControl.NextButton_ClickForTest(null, null);
				AssertEquals(index, 2);
				userControl.PreviousButton_ClickForTest(null, null);
				AssertEquals(index, 1);
				userControl.PreviousButton_ClickForTest(null, null);
				AssertEquals(index, 0);
			}
		}

		public void TestShowAndHideSaveButton()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var screenedPartyModel = new ScreenedPartyModel(new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse(), new DpsRequestHeaderWithAddressMatching()), Factory);
			var winModel = new ScreenedPartyWinModel(screenedPartyModel, a => { }, null, true);

			using (var form = new ZForm())
			using (var userControl = new ScreenedPartyUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();
				AssertEquals(userControl.SaveButton.Visible, false);
			}

			winModel = new ScreenedPartyWinModel(screenedPartyModel, a => { }, null, false);

			using (var form = new ZForm())
			using (var userControl = new ScreenedPartyUserControl())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();
				AssertEquals(userControl.SaveButton.Visible, true);
			}
		}

		public void TestPotentialMatchBindingOnSelect()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For test1",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { IncludedSourceListCode }, TypeOfEntity = "PER" },
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { ExcludedSourceListCode }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = headerNamePk, MatchingNameScore = 100, SourceProfileID = headerPk },
				},
				AddressMatches = new List<AddressMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
			var model = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(model, a => { });

			using (var form = new ZForm())
			using (var userControl = new ScreenedPartyUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();
				using (var userControl1 = new PotentialMatchListItemUserControl())
				{
					var controls = userControl.PotentialMatchItemsPanel.Controls.Find("PotentialMatchListItemUserControl", true).OfType<PotentialMatchListItemUserControl>().ToArray();
					userControl.PotentialMatchListItemUserControl_ClickForTest(controls[1], null);
					AssertEquals(userControl.ScreenedPartyWinModel.SelectedPotentialMatchWinModel.ProfileName, "Primary Name");
				}
			}
		}

		public void TestPotentialMatchSelectedHighlight()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var headerPk = Guid.NewGuid();
			var headerNamePk = Guid.NewGuid();
			var responseWithParty = new DpsResponseWithScreeningParty(new ScreeningParty(header, string.Empty, header), new DpsResponse
			{
				ResponseCode = DpsResponseCode.Successful,
				ExtraMessage = "For test1",
				Profiles = new List<ProfileHeaderInfo>()
				{
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { IncludedSourceListCode }, TypeOfEntity = "PER" },
					new ProfileHeaderInfo { SourceProfileID = headerPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = headerNamePk, FullName = "Primary Name1", Language = "", IsPrimaryName = true, SourceProfileID = headerPk } }, SourceListCodes = new List<string>() { ExcludedSourceListCode }, TypeOfEntity = "PER" },
				},
				NameMatches = new List<NameMatchInfo>()
				{
					new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "Test name" }, MatchingNameID = headerNamePk, MatchingNameScore = 100, SourceProfileID = headerPk },
				},
				AddressMatches = new List<AddressMatchInfo>(),
				RegistrationCodeMatches = new List<RegistrationCodeMatchInfo>()
			}, new DpsRequestHeaderWithAddressMatching());
			var model = new ScreenedPartyModel(responseWithParty, Factory);
			var winModel = new ScreenedPartyWinModel(model, a => { });

			using (var form = new ZForm())
			using (var userControl = new ScreenedPartyUserControlForTest())
			{
				form.Controls.Add(userControl);
				userControl.SetDataBinding(winModel, "");
				form.Show();
				using (var userControl1 = new PotentialMatchListItemUserControl())
				{
					var controls = userControl.PotentialMatchItemsPanel.Controls.Find("PotentialMatchListItemUserControl", true).OfType<PotentialMatchListItemUserControl>().ToArray();
					userControl.PotentialMatchListItemUserControl_ClickForTest(controls[1], null);
					AssertEquals(userControl.ScreenedPartyWinModel.SelectedPotentialMatchWinModel.ProfileName, "Primary Name1");
					var potentialMatchListItemUserControl1 = userControl.GetSelectedPotentialMatchListItemControlForTest();
					AssertEquals(potentialMatchListItemUserControl1.BackColor, WinformConstants.BorderColor);

					userControl.PotentialMatchListItemUserControl_ClickForTest(controls[0], null);
					AssertEquals(userControl.ScreenedPartyWinModel.SelectedPotentialMatchWinModel.ProfileName, "Primary Name");
					var potentialMatchListItemUserControl = userControl.GetSelectedPotentialMatchListItemControlForTest();
					AssertEquals(potentialMatchListItemUserControl.BackColor, WinformConstants.BorderColor);
					AssertEquals(potentialMatchListItemUserControl1.BackColor, WinformConstants.UnselectedColor);
				}
			}
		}

		internal void Navigate(bool up)
		{
			if (up)
			{
				if (index < ScreenedPartiesCount)
				{
					index++;
				}
			}
			else if (index > 0)
			{
				index--;
			}
		}

		const string ExcludedSourceListCode = "SourceList1";
		const string IncludedSourceListCode = "SourceList2";
	}

	class ScreenedPartyUserControlForTest : ScreenedPartyUserControl
	{
		public void NextButton_ClickForTest(object sender, EventArgs e)
		{
			NextButton_Click(null, null);
		}

		public void PreviousButton_ClickForTest(object sender, EventArgs e)
		{
			PreviousButton_Click(null, null);
		}

		public void PotentialMatchListItemUserControl_ClickForTest(object sender, EventArgs e)
		{
			PotentialMatchListItemUserControl_Click(sender, e);
		}

		public PotentialMatchListItemUserControl GetSelectedPotentialMatchListItemControlForTest()
		{
			return SelectedPotentialMatchListItemUserControl;
		}
	}
}
