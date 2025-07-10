using System.Net.Mail;
using CargoWise.EntityFramework;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class BounceBackEmailDetailsForTest : BounceBackEmailDetails
	{
		public BounceBackEmailDetailsForTest(BusinessObjectFactory factory, byte[] bodyEml, byte[] headerEml = null) : base(factory, bodyEml, headerEml)
		{
		}

		public override string[] GetRecipients()
		{
			return new[] { "invalid@recipient.test@com" };
		}

		public override MailAddressCollection GetBouncedRecipients()
		{
			return new MailAddressCollection();
		}
	}
}
