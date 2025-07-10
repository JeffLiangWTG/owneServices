using CargoWise.EntityFramework.Testing;

namespace Enterprise.MarketingManager.Business.Testing
{
	sealed class ContactsAndUniqueOrganisationsTest : TestCaseWithFactory
	{
		public void TestLoadData()
		{
			var campaign = Factory.NewWithValidTestData<GlbCompanyCampaign>();
			Factory.Save();

			var data = ContactsAndUniqueOrganisations.LoadEmailsAndOrgsCount(campaign);
			AssertData(data);
		}

		static void AssertData(ContactsAndUniqueOrganisations.ContactsAndUniqueOrganisationsData data)
		{
			AssertNotNull(data);
			AssertNotNull(data.DeliveryData);
			AssertEquals(0, data.DeliveryData.Count);

			AssertNotNull(data.TotalData);
			AssertEquals(0D, data.TotalData.ClientsCount);
			AssertEquals(0D, data.TotalData.EmailsCount);

			AssertNotNull(data.UnsubscribedData);
			AssertEquals(0D, data.UnsubscribedData.ClientsCount);
			AssertEquals(0D, data.UnsubscribedData.EmailsCount);
		}

		public void TestLoadEmptyData()
		{
			var data = ContactsAndUniqueOrganisations.LoadEmailsAndOrgsCount(null);
			AssertData(data);
		}
	}
}
