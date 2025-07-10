using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentParsing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	// Unable to create a manifest resource name if the filenames of this file, SenderForm.Designer.cs and SenderForm.resx are changed
	// http://crikey.corporate.cargowise.com/BuildFailureDetailsForm.aspx?IsUserTest=true&PK=eedd1d1a-4891-41a7-b3bd-517a4b992ba9
	// http://crikey.corporate.cargowise.com/BuildFailureDetailsForm.aspx?IsUserTest=true&PK=eb10c657-8ae6-467b-9368-90f7bf596473
	public partial class EmailSenderForm : ZChildForm
	{
		#region Construction

		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public EmailSenderForm()
		{
			InitializeComponent();
		}

		public EmailSenderForm(EmailSenderConfiguration senderConfiguration)
			: base(senderConfiguration)
		{
			InitializeComponent();

			SaveToEDocsDocType.Enabled = senderConfiguration.CanSaveToEDocs;

			ToolStripEMailButton.PerformClick();
			this.EMailViaMailClientLabel.Text = Res.GetString("EmailSenderForm|Instructions", "After selecting your recipients, click the \"Preview E-mail\" button to start your mail client (Outlook etc.) where you can type your E-mail.");

			eDocsGroupBox.AllowOverlap(CommunicationGroupBox);

			senderConfiguration.HtmlEmail.ShouldValidateFrom = true;

			senderConfiguration.HtmlEmail.ShouldCheckFormatOfEmailBody = true;
		}

		#endregion

		public new EmailSenderConfiguration BusinessEntity
		{
			get { return (EmailSenderConfiguration)base.BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		#region Selecting a Message Type

		void ToolStripButton_CheckStateChanged(object sender, EventArgs e)
		{
			var button = (ToolStripButton)sender;
			var userPressedAnAlreadyPressedButton = (button.Pressed && !button.Checked);
			if (button.Checked)
			{
				SelectToolStripItem(button);
			}
			else if (button.Pressed)
			{
				// user pressed an already pressed button
				button.Checked = true;
			}
		}

		void SelectToolStripItem(ToolStripButton checkedButton)
		{
			UpdateToolStripItemVisualCheckedStates(checkedButton);
			UpdateControls(checkedButton);

			BusinessEntity.DeliveryMethod = (MessageDeliveryMethod)checkedButton.Tag;
		}

		void UpdateToolStripItemVisualCheckedStates(ToolStripButton checkedButton)
		{
			foreach (ToolStripItem item in CommunicationToolStrip.Items)
			{
				ToolStripButton button = item as ToolStripButton;
				if (button != null && button != checkedButton)
				{
					button.CheckState = CheckState.Unchecked;
				}
			}
		}

		void UpdateControls(ToolStripButton checkedButton)
		{
			SendButton.Visible = IsEmail;
			EMailViaMailClientLabel.Visible = IsEmailViaMailClient;
		}

		bool IsEmail
		{
			get { return ToolStripEMailButton.Checked; }
		}

		bool IsEmailViaMailClient
		{
			get { return ToolStripEMailViaMailClientButton.Checked; }
		}

		#endregion

		#region Cancel Button

		void CancelSendButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#region Send Button

		void PreviewButton_Click(object sender, EventArgs e)
		{
			if (IsEmailViaMailClient)
			{
				HandleSendOrPreviewViaMailClient();
			}
			else
			{
				try
				{
					EmailPreviewFormHelper.ShowPreviewForm(BusinessEntity.HtmlEmail);
				}
				catch (IOException ex)
				{
					Globals.Message.ShowError(Res.GetString("CCFF6E85-6BA9-44C8-8CCB-CC773B623B69", @"An error occurred while previewing the e-mail:

{0}

Please check that all attachments are not currently being used by other applications.", ex.Message),
						Res.GetString("EB5FD6DB-8A41-4819-9F61-6E337E3CCD4D", "Error Previewing E-mail"));
				}
				catch (DocumentParsingFailedException ex)
				{
					Globals.Message.ShowError(ex.Message);
				}
			}
		}

		void SendButton_Click(object sender, EventArgs e)
		{
			HandleSendOrPreviewViaMailClient();
		}

		protected void HandleSendOrPreviewViaMailClient()
		{
			BusinessEntityForValidation.RunPreSaveValidation();
			if (BusinessEntityForValidation.HasErrors())
			{
				ShowErrorsDialog();
			}
			else
			{
				var checker = ObjectFactory.Get<IEmailPreSendChecker>();
				if (checker.PromptUserIfSendingToNdrRecipients(BusinessEntity.HtmlEmail.AllRecipients.Select(x => (ZString)x)))
				{
					DialogResult = DialogResult.OK;
					Close();
				}
			}
		}

		protected override ZMessageBox CreateErrorMessageBox(CargoWise.EntityFramework.IBusiness businessEntityForValidation, bool includeIgnoreOption)
		{
			return new ZErrorMessageBox(businessEntityForValidation, Res.GetString("487DC92C-D8A6-4B75-83CD-98C6EE7DE3E8", "There are errors that need to be corrected before the E-mail can be sent."), Res.GetString("8732E5F5-67AD-4CB8-98EA-D459E62F9D27", "Unable to Send E-mail"), includeIgnoreOption);
		}

		#region Ctrl-Enter Clicking the Send Button

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			bool result;

			if (keyData == (Keys.Control | Keys.Enter))
			{
				this.SendButton.PerformClick();
				result = true;
			}
			else
			{
				result = base.ProcessCmdKey(ref msg, keyData);
			}

			return result;
		}

		#endregion

		#endregion
	}
}
