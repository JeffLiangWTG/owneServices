namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class EmailContactItemLookupsTest : ContactItemProxyLookupsTestCase<EmailContactItemLookups>
	{
		protected override EmailContactItemLookups GetNewLookups()
		{
			var contact = Factory.New<OrgContact>();
			var emailItem = new EmailContactItem(contact);
			return new EmailContactItemLookups(emailItem);
		}
	}
}
