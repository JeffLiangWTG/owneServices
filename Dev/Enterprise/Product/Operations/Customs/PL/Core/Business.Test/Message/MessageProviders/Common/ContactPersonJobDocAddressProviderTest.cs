using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

class ContactPersonJobDocAddressProviderTest : Customs.Business.Testing.DataProviderTestCase<ContactPersonJobDocAddressProvider>
{
	public void TestName()
	{
		AssertEquals("Wise Tech", GetProvider().Name);
	}

	public void TestPhoneNumber()
	{
		AssertEquals("1234567890", GetProvider().PhoneNumber);
	}

	public void TestEMailAddress()
	{
		AssertEquals("pl@Wisetechglobal.com", GetProvider().EMailAddress);
	}

	public void TestInvalidContact()
	{
		CombineAssertions(() =>
		{
			address.E2_Contact = "";
			address.E2_Phone = "";
			var invalidContact = ContactPersonJobDocAddressProvider.NewOrNull(address);
			AssertNull("E2_Contact and E2_Phone are empty", invalidContact);

			address.E2_Contact = "";
			address.E2_Phone = "12345678";
			invalidContact = ContactPersonJobDocAddressProvider.NewOrNull(address);
			AssertNull("E2_Contact is empty", invalidContact);

			address.E2_Contact = "Wise Tech";
			address.E2_Phone = "";
			invalidContact = ContactPersonJobDocAddressProvider.NewOrNull(address);
			AssertNull("E2_Phone is empty", invalidContact);
		});
	}

	protected override ContactPersonJobDocAddressProvider GetProvider() => ContactPersonJobDocAddressProvider.NewOrNull(address);

	protected override void SetUp()
	{
		base.SetUp();
		address = Factory.New<JobDocAddress>();
		address.E2_Contact = "Wise Tech";
		address.E2_Phone = "1234567890";
		address.E2_Email = "pl@Wisetechglobal.com";
	}

	JobDocAddress address;
}
