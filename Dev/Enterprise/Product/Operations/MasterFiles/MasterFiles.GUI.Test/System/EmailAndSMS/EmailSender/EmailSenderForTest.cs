using System.IO;
using CargoWise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.GUI.Testing
{
	class EmailSenderForTest : EmailSender
	{
		public EmailSenderForTest() : base(null, null)
		{
		}

		public EmailSenderForTest(ISendEmailSource source) : base(null, source)
		{
		}

		protected override void DisplayEmailAndSave(EmailSenderConfiguration senderConfiguration)
		{
		}

		public void SendEmail_Exposed(EmailSenderConfigurationForTest senderConfiguration)
		{
			base.SendEmail(senderConfiguration);
		}

		protected override void AddEDoc(EmailDef email, EmailSenderConfiguration senderConfiguration)
		{
			if (DeleteAttachmentFileForTest)
			{
				foreach (ICodeDescription attachment in senderConfiguration.HtmlEmail.AttachmentList)
				{
					var fullPathAndFileName = senderConfiguration.HtmlEmail.GetFileNameFromAttachmentListDescription(attachment.Description);
					File.Delete(fullPathAndFileName);
				}
			}
			base.AddEDoc(email, senderConfiguration);
		}

		public bool DeleteAttachmentFileForTest { get; set; }
	}
}
