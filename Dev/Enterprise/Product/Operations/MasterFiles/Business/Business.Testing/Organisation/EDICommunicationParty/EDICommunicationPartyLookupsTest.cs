using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class EDICommunicationPartyLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTechnicalContactsAdditionalFilter()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var contact1 = header.Contacts.AddNew();
			var contact2 = header.Contacts.AddNew();

			contact1.OC_ContactName = "Test Name 1";
			contact1.OC_Email = "test@test.mail.com";

			contact2.OC_ContactName = "Test Name 2";
			contact2.OC_Email = ZString.Empty;

			Factory.Save();
			var expectedError = "An Organization Contact selected from here must have an email.";
			var contacts = new EDICommunicationPartyLookups.EDICommunicationPartyOrgContactCollection(Factory);

			Assert(contact1.MatchesFilter(contacts.CompleteFilter));
			Assert(!contact2.MatchesFilter(contacts.CompleteFilter));
			AssertContains(expectedError, contacts.GetAllNotificationsWhenAdditionalFilterNotMet(contact2));
		}
	}
}
