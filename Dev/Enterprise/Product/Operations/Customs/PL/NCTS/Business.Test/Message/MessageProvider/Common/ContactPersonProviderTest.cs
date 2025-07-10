using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.NCTS.Business.Testing;

sealed class ContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<ContactPersonProvider>
{
	public void TestNewOrNull_OnePerson()
	{
		CombineAssertions(() => ContactPersonTestHelper.TestNewOrNull_OnePerson(orgHeader,
			() => ContactPersonProvider.NewOrNull(orgHeader)));
	}

	public void TestNewOrNull_MultiplePersonsWithAllocations()
	{
		CombineAssertions(() => ContactPersonTestHelper.TestNewOrNull_MultiplePersonsWithAllocations(orgHeader,
			() => ContactPersonProvider.NewOrNull(orgHeader)));
	}

	public void TestName() => AssertEquals(ContactPersonTestHelper.TestContactName, Provider.Name);

	public void TestEMailAddress() => AssertEquals(ContactPersonTestHelper.TestContactEmail, Provider.EMailAddress);

	public void TestPhoneNumber() => AssertEquals(ContactPersonTestHelper.TestContactPhone, Provider.PhoneNumber);

	protected override ContactPersonProvider GetProvider() => ContactPersonProvider.NewOrNull(orgHeader);

	protected override void SetUp()
	{
		base.SetUp();

		orgHeader = Factory.New<OrgHeader>();
		contact = Factory.New<OrgContact>();
		ContactPersonTestHelper.FillWithValidData(contact);
		orgHeader.Contacts.Add(contact);
	}
	OrgHeader orgHeader;
	OrgContact contact;
}
