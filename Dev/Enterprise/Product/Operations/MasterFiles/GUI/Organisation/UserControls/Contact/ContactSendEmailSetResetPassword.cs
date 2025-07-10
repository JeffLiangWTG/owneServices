using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Security;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public class ContactSendEmailSetResetPassword
	{
		public ContactSendEmailSetResetPassword()
		{
		}

		public virtual bool SendPasswordInstructions(OrgContact selectedContact, bool displayMsg = true, PasswordInstructionUrlType type = PasswordInstructionUrlType.Default)
		{
			if (selectedContact != null)
			{
				if (selectedContact.CheckContactHasValidEmail())
				{
					if (selectedContact.OC_WebAccessEnabled)
					{
						return SendEmailToContact(selectedContact, displayMsg, type);
					}
					else
					{
						if (displayMsg)
						{
							Globals.Message.Show(ResString.GetMultilingualString("83948310-fc2c-481c-88a3-2a700753a588", "Instructions can only be sent to users with web access enabled."));
						}
					}
				}
				else
				{
					if (displayMsg)
					{
						Globals.Message.Show(ResString.GetMultilingualString("f6748d89-4cfa-47f8-b771-3cf5c917e8f1", "Please specify a valid email address for this contact before sending the password instruction."));
					}
				}
			}
			return false;
		}

		public bool SendEmailToContact(OrgContact selectedContact, bool displayMsg = true, PasswordInstructionUrlType type = PasswordInstructionUrlType.Default)
		{
			if (displayMsg)
			{
				using (var selectEmailChildForm = new SendPasswordResetEmailForm())
				{
					if (ZFormModaliser.ShowDialogWithoutDispose(selectEmailChildForm) == DialogResult.OK)
					{
						return SendEmailCore(selectedContact, displayMsg, selectEmailChildForm.useCurrentEmailCheckBox.Checked, type);
					}
					return false;
				}
			}
			else
			{
				return SendEmailCore(selectedContact, displayMsg);
			}
		}

		public bool SendEmailToContacts(BusinessObject[] selectedContacts)
		{
			using (var selectEmailChildForm = new SendPasswordResetEmailForm())
			{
				if (ZFormModaliser.ShowDialogWithoutDispose(selectEmailChildForm) != DialogResult.OK)
				{
					return false;
				}
				foreach (var selectedContact in selectedContacts.Cast<OrgContact>())
				{
					if (!SendEmailCore(selectedContact, displayMsg: false, selectEmailChildForm.useCurrentEmailCheckBox.Checked, type: PasswordInstructionUrlType.Default))
					{
						return false;
					}
				}
				return true;
			}
		}

		bool SendEmailCore(OrgContact selectedContact, bool displayMsg, bool isFromCurrentUser = false, PasswordInstructionUrlType type = PasswordInstructionUrlType.Default)
		{
			try
			{
				var passwordInstructionType = IsClearingPasswordOverride ? PasswordInstructionType.Set : selectedContact.GetPasswordInstructionType();
				var passwordResetInfo = CreatePasswordResetInfo(passwordInstructionType, selectedContact);
				var isPasswordEmailSent = PasswordInstructionEmailSender.SendPasswordInstructionEmail(selectedContact, passwordInstructionType, passwordResetInfo, type, isFromCurrentUser);

				if (selectedContact != null && isPasswordEmailSent)
				{
					if (displayMsg)
					{
						Globals.Message.Show(ResString.GetMultilingualString("2bce0d9d-86a9-4942-856c-1ade5d08a6d3", "An email was sent to this contact containing the password Instruction and URL."));
					}

					return true;
				}

				if (displayMsg)
				{
					Globals.Message.ShowError(ResString.GetMultilingualString("b5cb7c6f-649d-44b9-a574-d031281d8031", "An error was encountered while sending the password instruction email."));
				}
			}
			catch (WebSiteUrlNotSetException e)
			{
				if (displayMsg)
				{
					Globals.Message.Show(e.Message);
				}
			}

			return false;
		}

		protected PasswordResetInfo CreatePasswordResetInfo(PasswordInstructionType passwordInstructionType, OrgContact contact)
		{
			return new PasswordResetInfo()
			{
				ContactEmail = contact.Email,
				OrgCode = contact.OrgCode,
				EmailTemplateCompanyPk = GlbCompany.CurrentCompany.PK.ToString()
			};
		}

		protected bool IsClearingPasswordOverride { get; set; }
	}
}
