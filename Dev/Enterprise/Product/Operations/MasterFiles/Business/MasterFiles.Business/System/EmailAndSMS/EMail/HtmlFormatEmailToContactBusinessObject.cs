using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class HtmlFormatEmailToContactBusinessObject : EmailToContactBusinessObject
	{
		public HtmlFormatEmailToContactBusinessObject(BusinessObject businessObjectSendingEmail)
			: base(businessObjectSendingEmail)
		{
		}

		public HtmlFormatEmailToContactBusinessObject(BusinessObject businessObjectSendingEmail, string overridingDefaultFromEmailAddress, string defaultFromDisplayName)
			: base(businessObjectSendingEmail, overridingDefaultFromEmailAddress, defaultFromDisplayName)
		{
		}

		protected override EmailDef GetNewEmailDef()
		{
			return new HtmlEmailDef();
		}

		protected override EmailDef GetEmailCore()
		{
			HtmlEmailDef result = (HtmlEmailDef)base.GetEmailCore();
			LoadHtmlUsingTemplate(result);
			return result;
		}

		protected virtual void LoadHtmlUsingTemplate(HtmlEmailDef emailDef)
		{
			emailDef.LoadHtmlUsingTemplate(Body);
		}
	}
}
