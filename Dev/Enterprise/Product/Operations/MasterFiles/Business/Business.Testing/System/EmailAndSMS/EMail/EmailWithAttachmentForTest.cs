using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmailWithAttachmentForTest : EmailWithAttachment
	{
		public EmailWithAttachmentForTest(EmailContentTypes emailContentTypeForTest)
			: base()
		{
			this.emailContentTypeForTest = emailContentTypeForTest;
		}

		public readonly EmailContentTypes emailContentTypeForTest;

		protected override EmailDef GetEmailCore()
		{
			EmailDef emailDef = base.GetEmailCore();
			emailDef.ContentType = emailContentTypeForTest;
			return emailDef;
		}
	}
}
