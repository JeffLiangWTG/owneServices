using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.NCTS.Business.Testing;

[TestedType(typeof(ContactProvider))]
sealed class ContactProviderTest : Customs.Business.Testing.DataProviderTestCase<ContactProvider>
{
	public void TestNewOrNull_AddressNull()
	{
		AssertNull(ContactProvider.NewOrNull(null));
	}

	public void TestNewOrNull_AllPropertiesEmpty()
	{
		AssertNotNull(ContactProvider.NewOrNull(Factory.New<JobDocAddress>()));
	}

	public void TestName() => CombineAssertions(() =>
	{
		AssertEquals("Selected", "Name", provider.Name);
		jobDocAddress.ContactPK = ZGuid.Empty;
		AssertEquals("From organisation with allocation cus", "CusName", GetProvider().Name);
	});

	public void TestPhoneNumber() => CombineAssertions(() =>
	{
		AssertEquals("Selected", "PhoneNumber", provider.PhoneNumber);
		jobDocAddress.E2_Phone = "";
		AssertEquals("From organisation from selected contact", "PhoneNumber", GetProvider().PhoneNumber);
		jobDocAddress.ContactPK = ZGuid.Empty;
		AssertEquals("From organisation with allocation cus", "CusPhoneNumber", GetProvider().PhoneNumber);
	});

	public void TestEMailAddress() => CombineAssertions(() =>
	{
		AssertEquals("Selected", "EMailAddress", provider.EMailAddress);
		jobDocAddress.E2_Email = "";
		AssertEquals("From organisation from selected contact", "EMailAddress", GetProvider().EMailAddress);
		jobDocAddress.ContactPK = ZGuid.Empty;
		AssertEquals("From organisation with allocation cus", "CusEMailAddress", GetProvider().EMailAddress);
	});

	public void TestCommunications()
	{
		AssertEquals(0, provider.Communications.Count);
	}

	protected override void SetUp()
	{
		base.SetUp();
		jobDocAddress = Factory.CreateJobDocAddress(contactName: "Name", contactPhone: "PhoneNumber", contactEmail: "EMailAddress");
		var contact = jobDocAddress.Organisation.Contacts.AddNew();
		contact.OC_ContactName = "CusName";
		contact.OC_Email = "CusEMailAddress";
		contact.OC_Phone = "CusPhoneNumber";
		contact.Allocations.AddNew().PC_Type = "CUS";

		provider = GetProvider();
	}

	ContactProvider provider;
	JobDocAddress jobDocAddress;

	protected override ContactProvider GetProvider() => ContactProvider.NewOrNull(jobDocAddress);
}
