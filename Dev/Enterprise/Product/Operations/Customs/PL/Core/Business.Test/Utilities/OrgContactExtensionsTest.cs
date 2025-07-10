using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.PL.Business.Testing;

public sealed class OrgContactExtensionsTest : TestCaseWithFactory
{
	const string TestPhone = "123";
	const string TestMobilePhone = "456";
	const string TestHomePhone = "3462";
	const string TestOtherPhone = "15688";

	public void TestHasNonEmptyPhone()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Null contact", false, ((IOrgContact)null).HasNonEmptyPhone());

			AssertEquals("All phones empty", false, contact.HasNonEmptyPhone());

			contact.OC_Phone = TestPhone;
			AssertEquals("OC_Phone non-empty", true, contact.HasNonEmptyPhone());
			contact.OC_Phone = ZString.Empty;

			contact.OC_Mobile = TestMobilePhone;
			AssertEquals("OC_Mobile non-empty", true, contact.HasNonEmptyPhone());
			contact.OC_Mobile = ZString.Empty;

			contact.OC_HomePhone = TestHomePhone;
			AssertEquals("OC_HomePhone non-empty", true, contact.HasNonEmptyPhone());
			contact.OC_HomePhone = ZString.Empty;

			contact.OC_OtherPhone = TestOtherPhone;
			AssertEquals("OC_OtherPhone non-empty", true, contact.HasNonEmptyPhone());
		});
	}

	public void TestGetFirstNonEmptyPhone()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Null contact", ZString.Empty, ((IOrgContact)null).GetFirstNonEmptyPhone());

			FillAllPhonesOfContact();
			AssertEquals("All phones non-empty - found non empty phone", TestPhone, contact.GetFirstNonEmptyPhone());

			contact.OC_Phone = ZString.Empty;
			AssertEquals("OC_Phone is empty - found non empty phone", TestMobilePhone, contact.GetFirstNonEmptyPhone());

			contact.OC_Mobile = ZString.Empty;
			AssertEquals("OC_Phone, OC_Mobile are empty - found non empty phone", TestHomePhone, contact.GetFirstNonEmptyPhone());

			contact.OC_HomePhone = ZString.Empty;
			AssertEquals("OC_Phone, OC_Mobile, OC_HomePhone are empty - found non empty phone", TestOtherPhone, contact.GetFirstNonEmptyPhone());

			contact.OC_OtherPhone = ZString.Empty;
			AssertEquals("All phones are empty - non empty phone not found - found non empty phone", ZString.Empty, contact.GetFirstNonEmptyPhone());
		});
	}
	protected override void SetUp()
	{
		base.SetUp();

		orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		contact = orgHeader.Contacts.AddNew();
	}
	OrgHeader orgHeader;
	OrgContact contact;

	void FillAllPhonesOfContact()
	{
		contact.OC_Phone = TestPhone;
		contact.OC_Mobile = TestMobilePhone;
		contact.OC_HomePhone = TestHomePhone;
		contact.OC_OtherPhone = TestOtherPhone;
	}
}
