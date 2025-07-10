using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.PL.Business.Testing;

sealed class ContactPersonProviderTest : Customs.Business.Testing.DataProviderTestCase<ContactPersonProvider>
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Null GlbStaff", "Value cannot be null.\r\nParameter name: glbStaff",
			() => new ContactPersonProvider(null));
	}

	public void TestCreateOrGetNull()
	{
		CombineAssertions(() =>
		{
			var glbStaff = Factory.New<GlbStaff>();
			glbStaff.GS_FullName = "AgentName";
			glbStaff.GS_PublishWorkPhone = true;
			glbStaff.GS_WorkPhone = "12345";

			var provider = ContactPersonProvider.NewOrNull(glbStaff);
			AssertNotNull("Not Null if name and phone is not empty", provider);

			glbStaff.GS_FullName = string.Empty;
			provider = ContactPersonProvider.NewOrNull(glbStaff);
			AssertNull("Null if name is empty", provider);

			glbStaff.GS_FullName = "AgentName";
			glbStaff.GS_WorkPhone = string.Empty;
			provider = ContactPersonProvider.NewOrNull(glbStaff);
			AssertNull("Null if phone is empty", provider);
		});
	}

	public void TestName()
	{
		AssertEquals("FName", Provider.Name);
	}

	public void TestPhoneNumber()
	{
		(string, bool, string, bool, string, bool) phoneData(
			string work = null, bool publishWork = false,
			string mobile = null, bool publishMobile = false,
			string home = null, bool publishHome = false)
		{
			return (work, publishWork, mobile, publishMobile, home, publishHome);
		}

		const string workPhone = "12345";
		const string mobilePhone = "54321";
		const string homePhone = "12321";

		var testCases = new (string, (string, bool, string, bool, string, bool), string)[] {
			("All phones is empty", phoneData(), null),

			("Work phone is not publihed", phoneData(work: workPhone), null),
			("Work phone is publihed", phoneData(work: workPhone, publishWork: true), workPhone),
			("Work phone is publihed and empty", phoneData(publishWork: true), null),

			("Mobile phone is not publihed", phoneData(mobile: mobilePhone), null),
			("Mobile phone is publihed", phoneData(mobile: mobilePhone, publishMobile: true), mobilePhone),
			("Mobile phone is publihed and empty", phoneData(publishMobile: true), null),

			("Home phone is not publihed", phoneData(home: homePhone), null),
			("Home phone is publihed", phoneData(home: homePhone, publishHome: true), homePhone),
			("Home phone is publihed and empty", phoneData(publishHome: true), null),

			("Work phone is not publihed, mobile phone is published", phoneData(work: workPhone, mobile: mobilePhone, publishMobile: true ), mobilePhone),
			("Work phone is not publihed, home phone is published", phoneData(work: workPhone, home: homePhone, publishHome: true ), homePhone),
			("Work mobile is not publihed, home phone is published", phoneData(mobile: mobilePhone, home: homePhone, publishHome: true ), homePhone),

			("Only work phone is published", phoneData(work: workPhone, mobile: mobilePhone, home: homePhone, publishWork: true ), workPhone),
			("Only mobile phone is published", phoneData(work: workPhone, mobile: mobilePhone, home: homePhone, publishMobile: true ), mobilePhone),
			("Only home phone is published", phoneData(work: workPhone, mobile: mobilePhone, home: homePhone, publishHome: true ), homePhone),

			("Only work phone is not published", phoneData(work: workPhone, mobile: mobilePhone, home: homePhone, publishMobile: true, publishHome: true), mobilePhone),
			("Only mobile phone is not published", phoneData(work: workPhone, mobile: mobilePhone, home: homePhone, publishWork: true, publishHome: true), workPhone),
			("Only home phone is not published", phoneData(work: workPhone, mobile: mobilePhone, home: homePhone, publishWork: true, publishMobile: true), workPhone),

			("All phones are published", phoneData(work: workPhone, mobile: mobilePhone, home: homePhone, publishWork: true, publishMobile: true, publishHome: true), workPhone),
		};

		CombineAssertions(() =>
		{
			foreach (var (description, data, expectedResult) in testCases)
			{
				(glbStaff.GS_WorkPhone, glbStaff.GS_PublishWorkPhone, glbStaff.GS_MobilePhone, glbStaff.GS_PublishMobilePhone,
					glbStaff.GS_HomePhone, glbStaff.GS_PublishHomePhone) = data;
				AssertEquals(description, expectedResult, GetProvider().PhoneNumber);
			}
		});
	}

	public void TestEMailAddress()
	{
		const string testEmail = "test@email.com";

		var testCases = new (string, string, bool, string)[] {
			("Email is not empty and published", testEmail, true, testEmail),
			("Email is not empty and not published", testEmail, false, null),
			("Email is empty and published", null, true, null),
			("Email is empty and not published", null, false, null)
		};

		CombineAssertions(() =>
		{
			foreach (var (descriptipn, email, isPublished, expected) in testCases)
			{
				glbStaff.GS_EmailAddress = email;
				glbStaff.GS_PublishEmailAddress = isPublished;
				AssertEquals(descriptipn, expected, GetProvider().EMailAddress);
			}
		});
	}

	protected override ContactPersonProvider GetProvider() => new ContactPersonProvider(glbStaff);

	protected override void SetUp()
	{
		base.SetUp();
		glbStaff = Factory.New<GlbStaff>();
		glbStaff.GS_FullName = "FName";
		glbStaff.GS_WorkPhone = "12345";
		glbStaff.GS_EmailAddress = "glbemail@abc.com";
	}

	GlbStaff glbStaff;
}
