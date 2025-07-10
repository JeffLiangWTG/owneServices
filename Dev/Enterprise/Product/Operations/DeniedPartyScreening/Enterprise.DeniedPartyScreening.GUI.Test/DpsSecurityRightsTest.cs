using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI.Test
{
	public class DpsSecurityRightsTest : TestCaseWithFactory
	{
		public void TestAllowCountriesManageSanctions_ShowAccessDeniedMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				tmpSecurityCore.CountriesManageSanctions.IsAllowed = false;
				AssertSecurityRightsAccessDenied(tmpSecurityCore.CountriesManageSanctions, DpsSecurityRights.IsGrantedCountriesManageSanctionsWithShowError());
			}
		}

		public void TestAllowCountriesManageSanctions_ShouldNotShowAccessDeniedMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				tmpSecurityCore.CountriesManageSanctions.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessages();

				Assert(DpsSecurityRights.IsGrantedCountriesManageSanctionsWithShowError());
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowJobLevelClearance_WhenAccessDenied_ShouldShowErrorMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;
				AssertSecurityRightsAccessDenied(tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear, DpsSecurityRights.IsGrantedJobLevelClearanceWithShowError());
			}
		}

		public void TestAllowJobLevelClearance_WhenGrantedAccess_ShouldNotShowErrorMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessages();

				Assert(DpsSecurityRights.IsGrantedJobLevelClearanceWithShowError());
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAllowScreening_ShowAccessDeniedMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowScreening.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				tmpSecurityCore.DpsAllowScreening.IsAllowed = false;
				AssertSecurityRightsAccessDenied(tmpSecurityCore.DpsAllowScreening, DpsSecurityRights.IsGrantedScreeningWithShowError());
			}
		}

		public void TestAllowUpdateRelatedJobs_ShowAccessDeniedMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertSecurityRightsAccessDenied(tmpSecurityCore.OrgDeniedPartyScreeningAllowUpdateRelatedJobs, DpsSecurityRights.IsGrantedUpdateRelatedJobWithShowError());
			}
		}

		public void TestAllowScreeningFullList_ShowAccessDeniedMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowScreening.IsAllowed = true;
			tmpSecurityCore.DpsAllowFullList.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertSecurityRightsAccessDenied(tmpSecurityCore.DpsAllowFullList, DpsSecurityRights.IsGrantedScreeningFullListWithShowError());
			}
		}

		public void TestAllowScreeningPermanentClear_ShowAccessDeniedMessage()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_ScreeningStatus = "CLP";
			Factory.Save();

			var dpsSourceWithParties = new List<DpsSourceWithParties>()
			{
				new DpsSourceWithParties(header, null)
			};
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowScreening.IsAllowed = true;
			tmpSecurityCore.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertSecurityRightsAccessDenied(Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear, DpsSecurityRights.IsGrantedScreeningPermanentClearWithShowError(dpsSourceWithParties));

				var granted = false;
				AssertNoExceptionThrown("No exception thrown when source bizO is not implement IScreeningStatusProvider", () =>
				{
					granted = DpsSecurityRights.IsGrantedScreeningPermanentClearWithShowError(new List<DpsSourceWithParties>
					{
						new DpsSourceWithParties(Factory.New<OrgAddress>(), null)
					});
				});

				AssertEquals(true, granted);
			}
		}

		public void TestAllowScreeningAllPermanentClear_ShowAccessDeniedMessage()
		{
			var header1 = Factory.NewWithValidTestData<OrgHeader>();
			var header2 = Factory.NewWithValidTestData<OrgHeader>();
			var header3 = Factory.NewWithValidTestData<OrgHeader>();

			header1.OH_ScreeningStatus = "CLP";
			header2.OH_ScreeningStatus = "CLP";
			header3.OH_ScreeningStatus = "CLP";

			Factory.Save();

			var parties = new ScreeningParty[]
			{
				new ScreeningParty(header1, string.Empty, header1),
				new ScreeningParty(header2, string.Empty, header2),
				new ScreeningParty(header3, string.Empty, header3),
			};
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowScreening.IsAllowed = true;
			tmpSecurityCore.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertSecurityRightsAccessDenied(tmpSecurityCore.DpsAllowOverrideScreeningStatusUpdateToClear, DpsSecurityRights.IsGrantedScreeningPermanentClearWithShowError(parties));
			}
		}

		public void TestUpdateStatustoMatched_WithOverrideSecurityRights()
		{
			var screeningStatus = ScreeningStatusesList.Codes.Matched;
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowUpdateToClear.IsAllowed = true;
			tmpSecurityCore.DpsAllowUpdateToMatched.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertUpdateStatus_WithOverrideSecurityRights(true, 90, tmpSecurityCore.DpsAllowUpdateToMatched, screeningStatus);
			}

			tmpSecurityCore.DpsAllowUpdateToMatched.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertUpdateStatus_WithOverrideSecurityRights(false, 90, tmpSecurityCore.DpsAllowUpdateToMatched, screeningStatus);
			}
		}

		public void TestUpdateStatustoClear_WithOverrideSecurityRights()
		{
			var screeningStatus = ScreeningStatusesList.Codes.Clear;
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowUpdateToClear.IsAllowed = true;
			tmpSecurityCore.DpsAllowUpdateToClear.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertUpdateStatus_WithOverrideSecurityRights(true, 80, tmpSecurityCore.DpsAllowUpdateToClear, screeningStatus);
			}

			tmpSecurityCore.DpsAllowUpdateToClear.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertUpdateStatus_WithOverrideSecurityRights(false, 80, tmpSecurityCore.DpsAllowUpdateToClear, screeningStatus);
			}
		}

		public void TestUpdateStatustoClearFromHighRiskMatch_WithOverrideSecurityRights()
		{
			var screeningStatus = ScreeningStatusesList.Codes.Clear;
			var tmpSecurityCore = GetTemporarySecurityCore();
			tmpSecurityCore.DpsAllowUpdateToClear.IsAllowed = true;
			tmpSecurityCore.DpsAllowUpdateToClearFromHighRiskMatch.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertUpdateStatus_WithOverrideSecurityRights(true, 90, tmpSecurityCore.DpsAllowUpdateToClearFromHighRiskMatch, screeningStatus);
			}

			tmpSecurityCore.DpsAllowUpdateToClearFromHighRiskMatch.IsAllowed = false;
			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			{
				AssertUpdateStatus_WithOverrideSecurityRights(false, 90, tmpSecurityCore.DpsAllowUpdateToClearFromHighRiskMatch, screeningStatus);
			}
		}

		#region generic assertion

		void AssertUpdateStatus_WithOverrideSecurityRights(bool expectedSaved, int nameScore, SecurityCheckpoint securityCheckpoint, ZString screeningStatus)
		{
			IntializeTestSecurityOverrideForm(securityCheckpoint, expectedSaved);

			var resultViewModel = GetTestResultViewModel(nameScore);
			var headerViewModel = resultViewModel.ScreenedParties.Single(x => x.PartyName == "TestOrgName");
			headerViewModel.ScreeningStatusWinModel.ScreeningStatus = screeningStatus;

			var expectedMessage = string.Format(@"You do not have the security right to update screening status to '{0}'. The security right you require is '{1}'.

Contact the supervisor for further instructions or escalate the issue to a user who has security rights to override this restriction.

Do you wish to override this restriction by logging in as a user with this security right?", headerViewModel.ScreeningStatusWinModel.ScreeningStatuses.GetDescriptionFromCode(screeningStatus), securityCheckpoint.DisplayTextPathToSecurityRight);

			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);

			if (expectedSaved)
			{
				AssertEquals(screeningStatus, headerViewModel.ScreeningStatusWinModel.ScreeningStatus);
				AssertEquals(true, headerViewModel.ScreeningStatusWinModel.SaveButtonEnabled);
			}
			else
			{
				AssertEquals(ZString.Empty, headerViewModel.ScreeningStatusWinModel.ScreeningStatus);
				AssertEquals(false, headerViewModel.ScreeningStatusWinModel.SaveButtonEnabled);
			}
		}

		void AssertSecurityRightsAccessDenied(SecurityCheckpoint checkpoint, bool securityResult)
		{
			var expectedMessage = string.Format(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

{0}", checkpoint.DisplayTextPathToSecurityRight);

			AssertEquals(false, securityResult);
			AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region implementation

		readonly string securityPassword = Guid.NewGuid().ToString().Substring(8);

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		GlbStaff GetTemporarySecurityStaff(string securityRight, bool securityIsAllowed)
		{
			var supervisor = Factory.NewWithValidTestData<GlbStaff>();
			supervisor.StaffPlainTextPassword = securityPassword;

			var glbSecurity = supervisor.StaffSecurityPermissionsCollection.AddNew();
			glbSecurity.GU_SecurityRight = securityRight;
			glbSecurity.GU_SecurityItemIsAllowed = securityIsAllowed;

			Factory.Save();

			return supervisor;
		}

		DpsResultWinModel GetTestResultViewModel(int nameScore)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.OH_FullName = "TestOrgName";

			var dpsProfileHeaderPk = Guid.NewGuid();
			var dpsProfileNamePk = Guid.NewGuid();
			var profileHeaderInfo = new ProfileHeaderInfo { SourceProfileID = dpsProfileHeaderPk, ProfileNotes = Array.Empty<byte>(), ProfileNames = new List<ProfileNameInfo>() { new ProfileNameInfo { ID = dpsProfileNamePk, FullName = "TestOrgName", Language = "", IsPrimaryName = true, SourceProfileID = dpsProfileHeaderPk } }, SourceListCodes = new List<string>() { "Source List" }, TypeOfEntity = "ORG" };
			var nameMatchInfos = new List<NameMatchInfo>() { new NameMatchInfo { RequestName = new DpsNameCandidate { NameType = "Primary Name", FullName = "TestOrgName" }, MatchingNameID = dpsProfileNamePk, MatchingNameScore = nameScore, SourceProfileID = dpsProfileHeaderPk } };
			var addressMatchInfos = new List<AddressMatchInfo>();
			var registrationCodeMatchInfos = new List<RegistrationCodeMatchInfo>();
			var responses = new List<DpsResponseWithScreeningParty>()
			{
				new DpsResponseWithScreeningParty(new ScreeningParty(orgHeader, string.Empty, orgHeader), new DpsResponse
				{
					ResponseCode = DpsResponseCode.Successful,
					ExtraMessage = "Test ORG",
					Profiles = new List<ProfileHeaderInfo>() { profileHeaderInfo },
					NameMatches = nameMatchInfos,
					AddressMatches = addressMatchInfos,
					RegistrationCodeMatches = registrationCodeMatchInfos
				}, new DpsRequestHeaderWithAddressMatching())
			};
			var resultModel = new DpsResultModel(responses, Factory);
			return new DpsResultWinModel(resultModel);
		}

		void IntializeTestSecurityOverrideForm(SecurityCheckpoint checkpoint, bool isAllowed)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			UnitTestUserNotification.Instance.AddAnswer(isAllowed ? DialogResult.Yes : DialogResult.No);
			UnitTestUserNotification.Instance.AddAnswer(isAllowed ? DialogResult.OK : DialogResult.Cancel);

			ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(overrideForm =>
			{
				var securityOverrideForm = (DeniedPartySecurityOverrideForm)overrideForm;
				var supervisor = GetTemporarySecurityStaff(checkpoint.Code, isAllowed);
				checkpoint.IsAllowed = isAllowed;

				securityOverrideForm.Shown += delegate
				{
					securityOverrideForm.usernameText.Text = supervisor.GS_LoginName;
					securityOverrideForm.passwordText.Text = securityPassword;
					securityOverrideForm.okButton_Click(null, null);
				};
			});
			ZFormModaliser.ShowDialogsInTest = true;
		}

		#endregion
	}
}
