using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class DeliveryNotificationPartiesContactsWrapperTest : TestCaseWithFactory
	{
		public void TestCommunications()
		{
			var contact = new DeliveryNotificationPartiesContactsWrapper("1@1.com") as IContact;
			AssertEquals(1, contact.Communications.Count());
			Assert(contact.Communications.Any(x => x.ContactDetail == "1@1.com"));
		}
	}
}
