namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PhoneContactItemLookupsTest : ContactItemProxyLookupsTestCase<PhoneContactItemLookups>
	{
		public void TestOrgContactColumnsByDescription()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.OrgContactColumnsByDescription);
		}

		public void TestOrgContactPhoneIsManuallyVerifiedColumnsByDescription()
		{
			var lookups = GetNewLookups();
			AssertNotNull(lookups.OrgContactPhoneIsManuallyVerifiedColumnsByDescription);
		}

		protected override PhoneContactItemLookups GetNewLookups()
		{
			var contact = Factory.New<OrgContact>();
			var phoneItem = new PhoneContactItem(contact);
			return new PhoneContactItemLookups(phoneItem);
		}
	}
}
