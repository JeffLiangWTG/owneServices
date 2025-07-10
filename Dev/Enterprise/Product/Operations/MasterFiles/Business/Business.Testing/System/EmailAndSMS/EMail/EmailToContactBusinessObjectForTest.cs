using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmailToContactBusinessObjectForTest : EmailToContactBusinessObject
	{
		public EmailToContactBusinessObjectForTest(BusinessObject businessobjectSendingEmail, EmailContentTypes emailContentTypeForTest)
			: base(businessobjectSendingEmail)
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
