using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Enterprise.DeniedPartyScreening.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	public static class DpsSecurityRights
	{
		internal static (bool, string) IsGrantedUpdateStatus(string status, ScoreGrades matchScore)
		{
			(bool IsGranted, string CredentialDetails) isGrantedAndCredentialDetails = (IsGrantedScreeningWithShowError(), null);
			if (isGrantedAndCredentialDetails.IsGranted && status == ScreeningStatusesList.Codes.Matched)
			{
				isGrantedAndCredentialDetails = IsGrantedUpdateStatusToMatched(status);
			}
			else if (status == ScreeningStatusesList.Codes.PermanentClear)
			{
				isGrantedAndCredentialDetails = IsGrantedUpdateStatusToPermanentClear(status);
			}
			else if (status == ScreeningStatusesList.Codes.Clear)
			{
				isGrantedAndCredentialDetails = IsGrantedUpdateStatusToClear(status, matchScore);
			}

			return isGrantedAndCredentialDetails;
		}

		static (bool, string) IsGrantedUpdateStatusToMatched(string status)
		{
			(bool IsGranted, string CredentialDetails) isGrantedAndCredentialDetails = (true, null);
			if (!Env.Security.DpsAllowUpdateToMatched.IsAllowed)
			{
				isGrantedAndCredentialDetails = IsOverrideSecurityRestrictionAppliedWithShowConfirmation(s => s.DpsAllowUpdateToMatched, status, Env.Security.DpsAllowUpdateToMatched.DisplayTextPathToSecurityRight);
			}
			return isGrantedAndCredentialDetails;
		}

		static (bool, string) IsGrantedUpdateStatusToPermanentClear(string status)
		{
			(bool IsGranted, string CredentialDetails) isGrantedAndCredentialDetails = (true, null);
			if (!Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed)
			{
				isGrantedAndCredentialDetails = IsOverrideSecurityRestrictionAppliedWithShowConfirmation(s => s.DpsAllowOverrideScreeningStatusUpdateToClear, status, Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.DisplayTextPathToSecurityRight);
			}
			else if (Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed)
			{
				isGrantedAndCredentialDetails.IsGranted = IsPermanentClearAcknowledgeComplianceRiskWithShowConfirmation();
			}

			return isGrantedAndCredentialDetails;
		}

		static (bool, string) IsGrantedUpdateStatusToClear(string status, ScoreGrades matchScore)
		{
			(bool IsGranted, string CredentialDetails) isGrantedAndCredentialDetails = (true, null);
			if (matchScore == ScoreGrades.High && Env.Security.DpsAllowUpdateToClear.IsAllowed && !Env.Security.DpsAllowUpdateToClearFromHighRiskMatch.IsAllowed)
			{
				isGrantedAndCredentialDetails = IsOverrideSecurityRestrictionAppliedWithShowConfirmation(s => s.DpsAllowUpdateToClearFromHighRiskMatch, status, Env.Security.DpsAllowUpdateToClearFromHighRiskMatch.DisplayTextPathToSecurityRight);
			}
			else if (!Env.Security.DpsAllowUpdateToClear.IsAllowed)
			{
				isGrantedAndCredentialDetails = IsOverrideSecurityRestrictionAppliedWithShowConfirmation(s => s.DpsAllowUpdateToClear, status, Env.Security.DpsAllowUpdateToClear.DisplayTextPathToSecurityRight);
			}

			return isGrantedAndCredentialDetails;
		}

		public static bool IsGrantedScreeningWithShowError()
		{
			var isGranted = Env.Security.DpsAllowScreening.IsAllowed;
			if (!isGranted)
			{
				Env.Security.DpsAllowScreening.ShowError();
			}
			return isGranted;
		}

		public static bool IsGrantedUpdateRelatedJobWithShowError()
		{
			var isGranted = Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.IsAllowed;
			if (!isGranted)
			{
				Env.Security.OrgDeniedPartyScreeningAllowUpdateRelatedJobs.ShowError();
			}
			return isGranted;
		}

		public static bool IsGrantedScreeningFullListWithShowError()
		{
			var isGranted = IsGrantedScreeningWithShowError();
			if (isGranted && !Env.Security.DpsAllowFullList.IsAllowed)
			{
				Env.Security.DpsAllowFullList.ShowError();
				isGranted = false;
			}
			return isGranted;
		}

		public static bool IsGrantedScreeningForceReScreenWithShowError()
		{
			var isGranted = IsGrantedScreeningWithShowError();
			if (isGranted && !Env.Security.DpsAllowForceReScreen.IsAllowed)
			{
				Env.Security.DpsAllowForceReScreen.ShowError();
				isGranted = false;
			}
			return isGranted;
		}

		public static bool IsGrantedScreeningPermanentClearWithShowError(IEnumerable<DpsSourceWithParties> sourceParties)
		{
			var isGranted = true;
			if (sourceParties != null)
			{
				foreach (var bizO in sourceParties)
				{
					if (bizO.SourceBizO is IScreeningStatusProvider screeningStatusProvider && screeningStatusProvider.ScreeningStatus == ScreeningStatusesList.Codes.PermanentClear &&
						!Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed)
					{
						Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.ShowError();
						isGranted = false;
						break;
					}
				}
			}
			return isGranted;
		}

		public static bool IsGrantedScreeningPermanentClearWithShowError(IEnumerable<ScreeningParty> screeningParties)
		{
			var isGranted = true;
			if (screeningParties.All(party => party.CurrentScreeningStatus == ScreeningStatusesList.Codes.PermanentClear) &&
					!Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed)
			{
				Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.ShowError();
				isGranted = false;
			}
			return isGranted;
		}

		public static bool IsGrantedJobLevelClearanceWithShowError()
		{
			var isGranted = Env.Security.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed;
			if (!isGranted)
			{
				Env.Security.OrgDeniedPartyScreeningAllowJobLevelClear.ShowError();
			}
			return isGranted;
		}

		public static bool IsGrantedCountriesManageSanctionsWithShowError()
		{
			var isGranted = Env.Security.CountriesManageSanctions.IsAllowed;
			if (!isGranted)
			{
				Env.Security.CountriesManageSanctions.ShowError();
			}
			return isGranted;
		}

		public static bool IsGrantedOverrideScreeningStatusUpdateToUnknown()
		{
			var isGranted = true;
			if (!Env.Security.DpsAllowOverrideScreeningStatus.IsAllowed)
			{
				isGranted = IsOverrideComplianceSecurityRestrictionAppliedWithShowConfirmation(s => s.DpsAllowOverrideScreeningStatus, ScreeningStatusesList.Codes.Unknown, Env.Security.DpsAllowOverrideScreeningStatus.DisplayTextPathToSecurityRight);
			}
			return isGranted;
		}

		public static bool IsGrantedOverrideScreeningStatusUpdateToClear()
		{
			var isGranted = true;
			if (!Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.IsAllowed)
			{
				isGranted = IsOverrideComplianceSecurityRestrictionAppliedWithShowConfirmation(s => s.DpsAllowOverrideScreeningStatusUpdateToClear, ScreeningStatusesList.Codes.PermanentClear, Env.Security.DpsAllowOverrideScreeningStatusUpdateToClear.DisplayTextPathToSecurityRight);
			}
			return isGranted;
		}

		static bool IsOverrideComplianceSecurityRestrictionAppliedWithShowConfirmation(Func<SecurityCore, SecurityCheckpoint> securityFunction, string status, string securityPath)
		{
			var isGranted = false;
			var description = new ScreeningStatusesList().GetDescriptionFromCode(status);
			var message = Res.GetString("A2BD1BC1-D032-4412-A9F6-67696792ADD2", @"You do not have the security right to override screening status to '{0}'. The security right you require is '{1}'.

Contact the supervisor for further instructions or escalate the issue to a user who has security rights to override this restriction.

Do you wish to override this restriction by logging in as a user with this security right?", description, securityPath);

			var dialogResult = Globals.Message.Show(message, Res.GetString("B81452AE-3C84-4511-A089-5EEC636F8D08", "Security Restriction"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if (dialogResult == DialogResult.Yes)
			{
				isGranted = GetOverrideSecurityRestrictionResult(new SecurityChecker(securityFunction, securityPath, status)).IsGranted;
			}
			return isGranted;
		}

		static (bool, string) IsOverrideSecurityRestrictionAppliedWithShowConfirmation(Func<SecurityCore, SecurityCheckpoint> securityFunction, string status, string securityPath)
		{
			(bool IsGranted, string CredentialDetails) isGrantedAndCredentialDetails = (false, null);
			var description = new ScreeningStatusesList().GetDescriptionFromCode(status);
			var message = Res.GetString("2A73F526-5947-4C40-AB38-3BA2F437A0A3", @"You do not have the security right to update screening status to '{0}'. The security right you require is '{1}'.

Contact the supervisor for further instructions or escalate the issue to a user who has security rights to override this restriction.

Do you wish to override this restriction by logging in as a user with this security right?", description, securityPath);
			var caption = Res.GetString("2D1A7147-0771-4894-A4C9-DEE7F5FE064C", "Security Restriction");
			var dialogResult = Globals.Message.Show(message, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if (dialogResult == DialogResult.Yes)
			{
				isGrantedAndCredentialDetails = GetOverrideSecurityRestrictionResult(new SecurityChecker(securityFunction, securityPath, status));

				if (isGrantedAndCredentialDetails.IsGranted && status == ScreeningStatusesList.Codes.PermanentClear)
				{
					isGrantedAndCredentialDetails.IsGranted = IsPermanentClearAcknowledgeComplianceRiskWithShowConfirmation();
				}
			}
			return isGrantedAndCredentialDetails;
		}

		static bool IsPermanentClearAcknowledgeComplianceRiskWithShowConfirmation()
		{
			var message = Res.GetString("24A92273-1A10-47E1-A422-E18ADE293596", @"You are about to set status to 'Permanently Clear'.
Doing so will mark this record as permanently clear for denied party screening purposes. This records status will not be reset even when values are changed or added to denied party lists that could match to it.
This record status will not be reset when the data on the record is amended by your staff or integrations (This includes pertinent information such as changes to names, addresses and contact data).

This operation should only be done by staff that understand the impacts of this on your compliance processes.");

			var caption = Res.GetString("B24AE3E2-94B5-4812-8242-9B9B95A64CD2", "CRITICAL WARNING");
			var confirmation = Res.GetString("9621C7F3-EFBA-4288-A173-6FAEBB2156A2", "I understand the impact");
			var dialogResult = Globals.Message.ShowConfirmation(message, caption, confirmation, MessageBoxIcon.Warning);

			return dialogResult == DialogResult.OK;
		}

		static (bool IsGranted, string CredentialDetails) GetOverrideSecurityRestrictionResult(params SecurityChecker[] security)
		{
			var securityOverride = new DpsSecurityOverride(security.Select(s => s.CheckFunction).ToArray());
			var securityOverrideForm = new DeniedPartySecurityOverrideForm(securityOverride);
			ZFormModaliser.ShowDialogAndDispose(securityOverrideForm);

			return (!securityOverrideForm.IsCancelled && securityOverride.IsAllowed, securityOverrideForm.UserName);
		}
	}
}
