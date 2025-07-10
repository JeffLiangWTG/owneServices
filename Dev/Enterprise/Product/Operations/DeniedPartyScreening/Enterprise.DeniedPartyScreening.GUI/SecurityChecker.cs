using System;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.MasterFiles.Integration;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DeniedPartyScreening.GUI
{
	class SecurityChecker
	{
		internal SecurityChecker(Func<SecurityCore, SecurityCheckpoint> checkFunction, string securityPath, string screeningStatus)
		{
			CheckFunction = Argument.NotNull(checkFunction, "checkFunction");
			SecurityPath = Argument.NotNull(securityPath, "securityPath");
			ScreeningStatus = Argument.NotNull(screeningStatus, "screeningStatus");
		}

		internal Func<SecurityCore, SecurityCheckpoint> CheckFunction { get; }
		internal string SecurityPath { get; }
		internal string ScreeningStatus { get; }

		#region Security Rights Check

		static bool WarnAndEscalate(bool isCheckAllowOverride, params SecurityChecker[] securityCheckers)
		{
			var message = (isCheckAllowOverride) ? ComposeNotAllowToOverrideMessage(securityCheckers[0]) : ComposeNotAllowToAcceptMessage(securityCheckers);
			var dialogResult = Globals.Message.Show(message, Res.GetString("38a7a9c6-92c3-4fc8-a6a9-d0f3dcf799bb", "Security restriction"), MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
			if (dialogResult == DialogResult.No)
			{
				return false;
			}

			var checkFunctions = securityCheckers.Select(s => s.CheckFunction).ToArray();
			var dpsSecurityOverride = new DpsSecurityOverride(checkFunctions);
			var deniedPartySecurityOverrideForm = new DeniedPartySecurityOverrideForm(dpsSecurityOverride);
			ZFormModaliser.ShowDialogAndDispose(deniedPartySecurityOverrideForm);

			if (deniedPartySecurityOverrideForm.IsCancelled)
			{
				return false;
			}

			if (dpsSecurityOverride.IsAllowed)
			{
				return true;
			}

			var remainedSecurityCheckers = securityCheckers.Where(s => dpsSecurityOverride.NotAllowedSecurityCheckFunctions.Contains(s.CheckFunction)).ToArray();
			Argument.GreaterThanZero(remainedSecurityCheckers.Length, "remainedSecurityCheckers.Length");
			return WarnAndEscalate(isCheckAllowOverride, remainedSecurityCheckers);
		}

		static string ComposeNotAllowToOverrideMessage(SecurityChecker securityChecker)
		{
			var statusDescription = new ScreeningStatusesList().GetDescriptionFromCode(securityChecker.ScreeningStatus);
			return Res.GetString("26423313-d086-40f0-95c0-1b988f115c5c", "You do not have the security right to override the system recommended screening status '{0}'. The security right you require is '{1}'.\r\n\r\nContact the supervisor for further instructions or escalate the issue to a user who has security rights to override this restriction.\r\n\r\nDo you wish to override this restriction by logging in as a user with this security right?", statusDescription, securityChecker.SecurityPath);
		}

		static string ComposeNotAllowToAcceptMessage(SecurityChecker[] securityCheckers)
		{
			var errorMessageBuilder = new StringBuilder();
			var statusList = new ScreeningStatusesList();
			errorMessageBuilder.AppendLine(Res.GetString("6f7dc61b-8df1-4b5c-b8ba-24937e16cbc2", "You do not have security right(s) to accept screening status(es):"));

			foreach (var securityChecker in securityCheckers)
			{
				errorMessageBuilder.AppendLine(Res.GetString("374814c4-b47b-4afd-915b-f39947ef257e", "{0}- To accept '{1}', require security right '{2}' .", "\t", statusList.GetDescriptionFromCode(securityChecker.ScreeningStatus), securityChecker.SecurityPath));
			}

			return errorMessageBuilder.ToString().Trim() + "\r\n\r\n" + Res.GetString("0d2fda7d-108a-4699-9b66-f318a162dcee", "Contact the supervisor for further instructions or escalate the issue to a user who has security rights to override this restriction.\r\n\r\nDo you wish to override this restriction by logging in as a user with these security right(s)?");
		}

		internal static bool PermanentlyClearOrganizationIsAccepted(ZString[] fullNames)
		{
			var message = GetWarningMessage(fullNames);
			var caption = Res.GetString("1d42d1e1-71c1-4366-9803-83bb057f015c", "CRITICAL WARNING");
			var confirmMessage = Res.GetString("ad42d1e1-72c2-4366-9803-83bb057f015c", "I understand the impact");
			var confirmationResult = Globals.Message.ShowConfirmation(message, caption, confirmMessage, MessageBoxIcon.Warning);
			return confirmationResult == DialogResult.OK;
		}

		static string GetWarningMessage(ZString[] fullNames)
		{
			var codes = string.Join(",", fullNames);
			var warningMessage = Res.GetString("d77e8bfe-d1c3-3a16-9621-a25684b9224a", @"You are about to permanently clear organization '{0}'.
Doing so will mark this record as permanently clear for denied party screening purposes. This records status will not be reset even when values are changed or added to denied party lists that could match to it. This record status will not be reset when the data on the record is amended by your staff or integrations (This includes pertinent information such as changes to names, addresses and contact data).

This operation should only be done by staff that understand the impacts of this on your compliance processes.", codes);
			return warningMessage;
		}

		#endregion
	}
}
