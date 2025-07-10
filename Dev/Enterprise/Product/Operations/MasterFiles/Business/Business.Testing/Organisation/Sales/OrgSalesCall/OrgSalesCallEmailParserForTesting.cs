using MimeKit;
using MsgReader.Outlook;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class OrgSalesCallEmailParserForTesting : OrgSalesCallEmailParser
	{
		public new OrgSalesCall Parent
		{
			get { return base.Parent; }
		}

		public OrgSalesCallEmailParserForTesting(OrgSalesCall salesCall, IGUIProvider gUIProvider)
			: base(salesCall, gUIProvider)
		{
		}

		public new Result ParseMailItem(MimeMessage message)
		{
			return base.ParseMailItem(message);
		}

		public new Result ParseMailItem(Storage.Message message)
		{
			return base.ParseMailItem(message);
		}

		public new string GetPlainTextFromHtml(string html)
		{
			return base.GetPlainTextFromHtml(html);
		}

		public new OrgContact GetContactFromEmailAddresses(string[] emails)
		{
			return base.GetContactFromEmailAddresses(emails);
		}
	}
}
