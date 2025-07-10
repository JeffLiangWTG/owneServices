using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public interface IEmailAttachmentForm
	{
		EmailToContactBusinessObject Current { get; }
	}

	public partial class EmailContactForm : ZChildForm, IEmailAttachmentForm
	{
		public EmailContactForm(EmailToContactBusinessObject emailToContactBusinessObject)
			: base(emailToContactBusinessObject)
		{
			CloseButton.Text = CloseButtonText;
		}

		public EmailContactForm()
		{
		}

		public new EmailToContactBusinessObject BusinessEntity
		{
			get { return (EmailToContactBusinessObject)base.BusinessEntity; }
		}

		protected virtual string CloseButtonText { get; }

		#region Send Email

		void SendButton_Click(object sender, EventArgs e)
		{
			SendEmailAndClose();
		}

		void SendEmailAndClose()
		{
			if (!BusinessEntity.CheckIsReadyToSendEmail())
			{
				Globals.Message.ShowError(Res.GetString("aa835b9e-3ec7-4301-8cab-77cefdb72246", "Please correct all errors first before sending this Email."));
			}
			else
			{
				try
				{
					BusinessEntity.SendEmail();
					this.DialogResult = DialogResult.OK;
					Close();
				}
				catch (IOException ex)
				{
					Globals.Message.ShowError(Res.GetString("a493916a-ac07-4797-bbba-ce913fafd71a", @"An error occurred while sending the email:

{0}

Please check that all attachments are not currently being used by other applications.", ex.Message),
						Res.GetString("4101c622-d89c-4a3e-ae4e-fc2bc940d22a", "Error Sending Email"));
				}
				catch (ZSaveException ex)
				{
					HandleSaveException(ex);
					Close();
				}
			}
		}

		#endregion

		#region Close Form

		void CloseButton_Click(object sender, EventArgs e)
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}

		#endregion

		#region IEmailAttachmentForm Members

		EmailToContactBusinessObject IEmailAttachmentForm.Current
		{
			get { return BusinessEntity; }
		}

		#endregion

		void PreviewButton_Click(object sender, EventArgs e)
		{
			try
			{
				EmailPreviewFormHelper.ShowPreviewForm(BusinessEntity);
			}
			catch (IOException ex)
			{
				Globals.Message.ShowError(Res.GetString("d8fd17d6-93a5-4dc2-aa76-d553c79e1819", @"An error occurred while previewing the email:

{0}

Please check that all attachments are not currently being used by other applications.", ex.Message),
					Res.GetString("a4210e4b-e4ac-4ea3-8197-8432f37a58c2", "Error Previewing Email"));
			}
		}
	}
}
