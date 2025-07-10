using System;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class EmailMultipleContactsForm : ZChildForm, IEmailAttachmentForm
	{
		public EmailMultipleContactsForm()
		{
		}

		public EmailMultipleContactsForm(MultipleEmailToContactSender sender) : base(sender)
		{
		}

		public new MultipleEmailToContactSender BusinessEntity
		{
			get { return (MultipleEmailToContactSender)base.BusinessEntity; }
		}

		void PreviewButton_Click(object sender, EventArgs e)
		{
			if (ContactListGrid.ListManager.Position > -1)
			{
				EmailToContactBusinessObject bizO = (EmailToContactBusinessObject)ContactListGrid.ListManager.GetCurrent();
				EmailPreviewFormHelper.ShowPreviewForm(bizO);
			}
		}

		#region Send Email

		void SendAllButton_Click(object sender, EventArgs e)
		{
			bool emailsSent = BusinessEntity.SendToAllIfReady();

			if (emailsSent)
			{
				Close();
			}
			else
			{
				Globals.Message.ShowError(ErrorOnEmailMsg);
			}
		}

		static string ErrorOnEmailMsg
		{
			get { return Res.GetString("510b81f5-2cad-4f23-907c-11d378d946e0", "Please correct all errors first before sending this Email."); }
		}

		#endregion

		#region Close

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		#endregion

		#region IEmailAttachmentForm Members

		EmailToContactBusinessObject IEmailAttachmentForm.Current
		{
			get
			{
				int currentIndex = ContactListGrid.ListManager.Position;
				return currentIndex != -1 ? (EmailToContactBusinessObject)ContactListGrid.ListManager.List[currentIndex] : null;
			}
		}

		#endregion
	}
}
