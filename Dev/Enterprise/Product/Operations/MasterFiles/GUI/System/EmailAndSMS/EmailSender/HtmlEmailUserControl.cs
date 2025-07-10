using System;
using System.IO;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
#if NETFRAMEWORK
using Enterprise.ZArchitecture.Core;
#endif
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class HtmlEmailUserControl : ZUserControl
	{
		public HtmlEmailUserControl() : base()
		{
			InitializeComponent();

			InitializeHotKey();
		}

		#region Hot Key

		void InitializeHotKey()
		{
			var keys = Keys.Control | Keys.E;
			var descriptionString = Res.GetString("BDF770C7-737F-4424-BE49-2D58362C60E2", "Add the current user's email");

			ToTextBox.Hotkeys.RegisterHotKey(keys, InsertCurrentUserEmailIntoTextBox, descriptionString);
			CcTextBox.Hotkeys.RegisterHotKey(keys, InsertCurrentUserEmailIntoTextBox, descriptionString);
			BccTextBox.Hotkeys.RegisterHotKey(keys, InsertCurrentUserEmailIntoTextBox, descriptionString);
		}

		bool InsertCurrentUserEmailIntoTextBox(object sender, Keys keyData)
		{
			var textBox = (ZTextBox)sender;
			var originalAddressList = textBox.Text;

			if (!originalAddressList.Contains(GlbStaff.CurrentUser.GS_EmailAddress, StringComparison.Ordinal))
			{
				textBox.Text = string.IsNullOrEmpty(originalAddressList) ? GlbStaff.CurrentUser.GS_EmailAddress.ToString() : originalAddressList + ";" + GlbStaff.CurrentUser.GS_EmailAddress;
			}

			return true;
		}

		#endregion

		public HtmlEmailWithAttachment EmailMessage
		{
			get { return (HtmlEmailWithAttachment)CurrentDataItem; }
		}

		#region Attachments

		void AddAttachmentButton_Click(object sender, EventArgs e)
		{
			if (EmailMessage != null)
			{
				AddAttachments(EmailMessage);
			}
			else
			{
				Globals.Message.ShowError(NoContactsMsg);
			}
		}

		void AddAttachments(HtmlEmailWithAttachment emailMessage)
		{
			string attachment = GetAttachmentFileName();
			if (attachment != null)
			{
				if (!emailMessage.AttachmentList.ContainsCode(Path.GetFileName(attachment)))
				{
					emailMessage.AddAttachment(attachment);
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

		void RemoveAttachmentButton_Click(object sender, EventArgs e)
		{
			if (EmailMessage != null)
			{
				RemoveAttachment(EmailMessage);
			}
			else
			{
				Globals.Message.ShowError(NoContactsMsg, Res.GetString("20f62ad4-4941-4024-93b8-cb64f361e730", "No contacts found"));
			}
		}

		void RemoveAttachment(HtmlEmailWithAttachment emailMessage)
		{
			if (emailMessage.AttachmentList.Count > 0)
			{
				emailMessage.RemoveAttachment();
			}
			else
			{
				Globals.Message.ShowInformation(NoAttachmentsMsg);
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

		void ToButton_Click(object sender, EventArgs e)
		{
			OpenRecipientSelectionForm();
		}

		void CcButton_Click(object sender, EventArgs e)
		{
			OpenRecipientSelectionForm();
		}

		void BccButton_Click(object sender, EventArgs e)
		{
			OpenRecipientSelectionForm();
		}

		void OpenRecipientSelectionForm()
		{
			var form = new RecipientSelectionForm(EmailMessage.RecipientSelection);
			ZFormModaliser.Show(form, ParentForm);
			form.FormClosed += delegate
			{
				if (form.DialogResult == DialogResult.OK)
				{
					EmailMessage.Update((RecipientSelection)form.BusinessEntity);
				}
			};
		}
	}
}
