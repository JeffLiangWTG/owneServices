using System.Linq;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.NZ.TradeSingleWindow.MessageBuilders.Testing
{
	class DeliveryNotificationPartiesWrapperTest : TestCaseWithFactory
	{
		public void TestWrapper()
		{
			var wrapper = new DeliveryNotificationPartiesWrapper("name", "code", "1@1.com") as IOrganisationSimple;
			AssertEquals("name", wrapper.Name);
			AssertEquals("code", wrapper.CustomsClientCode);
			AssertEquals("", wrapper.CustomsSupplierCode);
			AssertEquals(1, wrapper.Contacts.Count());
			Assert(wrapper.Contacts.Any(x => x.Communications.Any(y => y.ContactDetail == "1@1.com")));
		}
	}
}
