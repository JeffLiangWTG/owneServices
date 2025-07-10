using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public static class WiseRatesGUIHelper
	{
#if DEBUG
		[ThreadStatic]
		public static Action ViewJSONWithDeveloperAuthenticationAction_ForTestOnly;
#endif

		public static void ViewJSONWithDeveloperAuthentication(string rawDataAsString)
		{
			if (DeveloperLoginForm.TryAuthenticate())
			{
				ViewJSON(rawDataAsString);
			}
		}

		public static void ViewJSON(string rawDataAsString)
		{
			using (var tempFile = TempFileWithDelayedDelete.NewWithExtension("json"))
			{
				using (var writer = new StreamWriter(tempFile.Filename))
				{
					writer.WriteLine(rawDataAsString);
				}

				try
				{
#if DEBUG
					ViewJSONWithDeveloperAuthenticationAction_ForTestOnly?.Invoke();
					ViewJSONWithDeveloperAuthenticationAction_ForTestOnly = null;
#endif

					FileOpener.Open(tempFile.Filename);
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					Globals.Message.ShowError(Res.GetString("1BC64841-8EDC-437B-9F45-A6A7B3879C83", "There was a problem showing raw data: {0}", ex.Message));
				}
			}
		}

		public static bool EnhanceUserPermissionToRunAction(
			string permissionName,
			Func<SecurityCore, SecurityCheckpoint> securityCheckpointSelector,
			bool needsConfirmation,
			string confirmationCaption,
			Func<bool> action)
		{
			var security = securityCheckpointSelector(Env.Security);

			if (!security.IsAllowed)
			{
				if (needsConfirmation)
				{
					var confirmationMsg = Res.GetString("B40497C4-A8D7-43B6-803A-DBBF063BC727",
						@"You do not have the following security right to edit {0}:

{1}

Do you wish to have a user with this right enter their credentials?", permissionName, security.DisplayTextPathToSecurityRight);

					if (DialogResult.Yes != Globals.Message.Show(confirmationMsg, confirmationCaption, MessageBoxButtons.YesNo, MessageBoxIcon.Hand))
					{
						return false;
					}
				}

				var overrideLogin = new SecurityOverridenLogin();
				if (ZFormModaliser.ShowDialogAndDispose(new LoginForm(overrideLogin)) != DialogResult.OK)
				{
					return false;
				}

				var validationMsg = string.Empty;
				if (overrideLogin.UserSecurity == null)
				{
					validationMsg = Res.GetString("EE3E360A-3DDB-4DA8-9631-449D8B919290", "The login details entered are incorrect or password is expired.");
				}
				else if (!securityCheckpointSelector(overrideLogin.UserSecurity).IsAllowed)
				{
					validationMsg = Res.GetString("D54B2CEC-05B0-4938-8B0C-7127372F924E", "The login details entered do not have security rights for '{0}'.", security.HumanReadableName);
				}

				if (validationMsg.Length != 0)
				{
					Globals.Message.Show(validationMsg);
					return false;
				}

				using (Env.SetTemporaryUserContext(overrideLogin.UserSecurity.UserPK, overrideLogin.UserSecurity.BranchPK, overrideLogin.UserSecurity.DepartmentPK))
				{
					return action();
				}
			}
			else
			{
				return action();
			}
		}
	}
}
