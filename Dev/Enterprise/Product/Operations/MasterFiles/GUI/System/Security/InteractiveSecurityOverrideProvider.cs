using System.Windows.Forms;
using Enterprise.Security;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class InteractiveSecurityOverrideProvider : SecurityOverrideProvider, Enterprise.Integration.Security.IInteractiveSecurityOverrideProvider
	{
		protected override SecurityCore RequestLoginCredentials(SecurityCheckpoint checkPoint)
		{
			SecurityCore result = null;
			using (var form = CreateNewLoginForm())
			{
				form.Message = GetSecurityOverrideMessage(checkPoint);

				while (true)
				{
					LastLoginFormResult = ZFormModaliser.ShowDialogWithoutDispose(form);

					if (LastLoginFormResult != DialogResult.OK)
					{
						break;
					}
					else if (form.Credentials == null || form.Credentials.UserSecurity == null)
					{
						if (form.Credentials != null && form.Credentials.LoginAuthentication.State == ZArchitecture.Core.LoginAuthenticationInfo.Status.UserInactive)
						{
							Globals.Message.Show(form.Credentials.LoginAuthentication.FailureMessage);
						}
						else
						{
							Globals.Message.Show(InvalidLoginErrorMsg);
						}
					}

					result = form.Credentials?.UserSecurity;

					if (Globals.IsTest || result != null)
					{
						break;
					}
				}
			}

			return result;
		}

		protected override SecurityCertificate RequestGrantedConfirmation(SecurityCheckpoint checkPoint)
		{
			SecurityCertificate result = SecurityCertificate.Granted;

			string message = GetSecurityGrantedMessage(checkPoint);
			if (!string.IsNullOrEmpty(message) && Globals.Message.Show(message, checkPoint.Code, MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.Cancel)
			{
				result = SecurityCertificate.Denied;
			}

			return result;
		}

		protected virtual LoginForm CreateNewLoginForm()
		{
			return new LoginForm();
		}

		protected DialogResult LastLoginFormResult;
	}
}
