using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EmailToContactUserControl : ZUserControl
	{
		public EmailToContactUserControl() : base()
		{
			InitializeComponent();
			AttachmentsChoosePanel.AllowOverlap(AttachmentsStatusPanel);
		}

		EmailToContactBusinessObject EmailToContact => FindForm() is IEmailAttachmentForm form ? form.Current : null;
		ISendEmailSource SendEmailSource => EmailToContact?.BusinessObjectSendingEmail as ISendEmailSource;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (EmailToContact != null)
			{
				EmailToContact.ShouldValidateFrom = true;
			}

			ToTextBox.Enabled = SendEmailSource == null;
			CcTextBox.Enabled = SendEmailSource == null;
		}

		#region Attachments

		void AddAttachmentButton_Click(object sender, EventArgs e)
		{
			if (EmailToContact != null)
			{
				AddAttachments(EmailToContact);
			}
			else
			{
				Globals.Message.ShowError(NoContactsMsg);
			}
		}

		void AddAttachments(EmailToContactBusinessObject emailObject)
		{
			string attachment = GetAttachmentFileName();
			if (attachment != null)
			{
				if (!emailObject.AttachmentList.ContainsCode(Path.GetFileName(attachment)))
				{
					emailObject.AddAttachment(attachment);
				}
				else
				{
					Globals.Message.ShowInformation(AttachmentWithSameFilenameMsg);
				}
			}
		}

		string GetAttachmentFileName()
		{
			string fileName = null;
			if (openFileDialog == null)
			{
				openFileDialog = new ZOpenFileDialog();
			}
			if (ZFormModaliser.ShowCommonDialogWithoutDispose(openFileDialog) == DialogResult.OK)
			{
				using (new ZWaitCursorChanger())
				{
					fileName = openFileDialog.ForceLocalFile();
				}
			}
			return fileName;
		}

		internal ZOpenFileDialog openFileDialog;

		void RemoveAttachmentButton_Click(object sender, EventArgs e)
		{
			if (EmailToContact != null)
			{
				RemoveAttachment(EmailToContact);
			}
			else
			{
				Globals.Message.ShowError(NoContactsMsg, Res.GetString("20f62ad4-4941-4024-93b8-cb64f361e730", "No contacts found"));
			}
		}

		void RemoveAttachment(EmailToContactBusinessObject emailObject)
		{
			if (emailObject.AttachmentList.Count > 0)
			{
				emailObject.RemoveAttachment();
			}
			else
			{
				Globals.Message.ShowInformation(NoAttachmentsMsg);
			}
		}

		void ToOrCcButton_Click(object sender, EventArgs e)
		{
			OpenRecipientSelectionForm();
		}

		void OpenRecipientSelectionForm()
		{
			if (SendEmailSource != null)
			{
				var form = new RecipientSelectionForm(new RecipientSelection(SendEmailSource.GetAddressBookSelection()));
				ZFormModaliser.Show(form, ParentForm);
				form.FormClosed += delegate
				{
					if (form.DialogResult == DialogResult.OK)
					{
						EmailToContact.ToEmailAddress = ((RecipientSelection)form.BusinessEntity).ToEmailAddress;
						EmailToContact.Cc = ((RecipientSelection)form.BusinessEntity).Cc;
					}
				};
			}
		}

		static string NoContactsMsg
		{
			get { return Res.GetString("579f1e44-025c-407a-a025-3992449c65fc", "There are no contacts to add or remove attachments on."); }
		}
		static string NoAttachmentsMsg
		{
			get { return Res.GetString("c30a17db-a419-4d0c-b260-7d7a400f9527", "There are no attachments to remove."); }
		}
		static string AttachmentWithSameFilenameMsg
		{
			get { return Res.GetString("d3394396-3b2c-4502-99a9-7d9b08998017", "An attachment with the same filename has already been added. You cannot add multiple attachments with the same filename."); }
		}

		#endregion

		protected override void Dispose(bool disposing)
		{
			if (openFileDialog != null)
			{
				openFileDialog.Dispose();
				openFileDialog = null;
			}
			base.Dispose(disposing);
		}
	}
}
