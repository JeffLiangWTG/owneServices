using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SalesReminderBuilderTest : TestCaseWithFactory
	{
		public void TestAddAddress()
		{
			Env.Registry.SetOrgAllowMixedCase(true);
			var address = Factory.New<OrgAddress>();
			address.OA_Address1 = "Main Addr1";
			address.OA_Address2 = "Main Addr2";
			address.OA_City = "Sydney";
			address.OA_State = "NSW";
			address.OA_PostCode = "2002";
			address.OA_RL_NKRelatedPortCode = "AUSYD";
			address.OA_Email = "test@test.com";
			address.OA_Phone = "1234";

			ZStringBuilder builder = new ZStringBuilder();
			SalesReminderBuilder.AddAddress(builder, address, false);
			string expectedPlain =
@"Client Address:
Main Addr1
Main Addr2
Sydney NSW 2002
Office Email: test@test.com
Office Phone: 1234
";

			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddAddress(builder, null, false);
			AssertEquals("null address conversion", "", builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddAddress(builder, address, true);
			string expectedHtml =
@"<strong>Client Address</strong>:
Main Addr1
Main Addr2
Sydney NSW 2002
<strong>Office Email</strong>: test@test.com
<strong>Office Phone</strong>: 1234
";

			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddAddress(builder, null, true);
			AssertEquals("null address conversion", "", builder.ToString());
		}

		public void TestAddContact()
		{
			ZStringBuilder builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "Peon", "1234", "1@bob.com", "9999", "Worker", false);

			string expectedPlain = @"Contact: Bob
Title: Peon
Job Category: Worker
Email: 1@bob.com
Phone: 1234
Mobile: 9999

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			var contact = Factory.New<OrgContact>();
			contact.OC_ContactName = "Bob";
			contact.OC_Title = "Peon";
			contact.OC_Phone = "1234";
			contact.OC_Email = "1@bob.com";
			contact.OC_Mobile = "9999";
			contact.OC_JobCategory = "Worker";
			SalesReminderBuilder.AddContact(builder, contact, false);
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "Peon", "1234", "1@bob.com", "9999", "", false);
			expectedPlain = @"Contact: Bob
Title: Peon
Email: 1@bob.com
Phone: 1234
Mobile: 9999

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "1234", "1@bob.com", "9999", "", false);
			expectedPlain = @"Contact: Bob
Email: 1@bob.com
Phone: 1234
Mobile: 9999

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "", "1@bob.com", "9999", "", false);
			expectedPlain = @"Contact: Bob
Email: 1@bob.com
Mobile: 9999

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "", "", "9999", "", false);
			expectedPlain = @"Contact: Bob
Mobile: 9999

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "", "", "", "", false);
			expectedPlain = @"Contact: Bob

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			var contact2 = Factory.New<OrgContact>();
			contact2.OC_ContactName = "Bob";
			contact2.OC_Title = "Peon";
			contact2.OC_Phone = "1234";
			contact2.OC_Email = "1@bob.com";
			contact2.OC_Mobile = "9999";
			contact2.OC_JobCategory = "";
			SalesReminderBuilder.AddContact(builder, contact2, false);
			expectedPlain = @"Contact: Bob
Title: Peon
Email: 1@bob.com
Phone: 1234
Mobile: 9999

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, null, false);
			AssertEquals("null contact conversion", "", builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "Peon", "1234", "1@bob.com", "9999", "Worker", true);

			string expectedHtml = @"<strong>Contact</strong>: Bob
<strong>Title</strong>: Peon
<strong>Job Category</strong>: Worker
<strong>Email</strong>: 1@bob.com
<strong>Phone</strong>: 1234
<strong>Mobile</strong>: 9999

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, contact, true);
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "Peon", "1234", "1@bob.com", "9999", "", true);
			expectedHtml = @"<strong>Contact</strong>: Bob
<strong>Title</strong>: Peon
<strong>Email</strong>: 1@bob.com
<strong>Phone</strong>: 1234
<strong>Mobile</strong>: 9999

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "1234", "1@bob.com", "9999", "", true);
			expectedHtml = @"<strong>Contact</strong>: Bob
<strong>Email</strong>: 1@bob.com
<strong>Phone</strong>: 1234
<strong>Mobile</strong>: 9999

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "", "1@bob.com", "9999", "", true);
			expectedHtml = @"<strong>Contact</strong>: Bob
<strong>Email</strong>: 1@bob.com
<strong>Mobile</strong>: 9999

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "", "", "9999", "", true);
			expectedHtml = @"<strong>Contact</strong>: Bob
<strong>Mobile</strong>: 9999

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, "Bob", "", "", "", "", "", true);
			expectedHtml = @"<strong>Contact</strong>: Bob

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, contact2, true);
			expectedHtml = @"<strong>Contact</strong>: Bob
<strong>Title</strong>: Peon
<strong>Email</strong>: 1@bob.com
<strong>Phone</strong>: 1234
<strong>Mobile</strong>: 9999

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddContact(builder, null, true);
			AssertEquals("null contact conversion", "", builder.ToString());
		}

		public void TestAddOfficeEmailAndPhone()
		{
			var address = Factory.New<OrgAddress>();
			address.OA_Email = "test@test.com";
			address.OA_Phone = "1234";

			var builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address, false);
			string expectedPlain =
@"Office Email: test@test.com
Office Phone: 1234
";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, null, false);
			AssertEquals("null conversion", "", builder.ToString());

			address.OA_Email = "test@test.com";
			address.OA_Phone = "";
			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address, false);
			expectedPlain =
@"Office Email: test@test.com
";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			address.OA_Email = "";
			address.OA_Phone = "1234";
			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address, false);
			expectedPlain =
@"Office Phone: 1234
";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			var address2 = Factory.New<OrgAddress>();
			address2.OA_Email = "";
			address2.OA_Phone = "";
			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address2, false);
			AssertEquals(string.Empty, builder.ToString());

			address.OA_Email = "test@test.com";
			address.OA_Phone = "1234";
			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address, true);
			string expectedHtml =
@"<strong>Office Email</strong>: test@test.com
<strong>Office Phone</strong>: 1234
";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, null, true);
			AssertEquals("null conversion", "", builder.ToString());

			address.OA_Email = "test@test.com";
			address.OA_Phone = "";
			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address, true);
			expectedPlain =
@"<strong>Office Email</strong>: test@test.com
";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			address.OA_Email = "";
			address.OA_Phone = "1234";
			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address, true);
			expectedPlain =
@"<strong>Office Phone</strong>: 1234
";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOfficeEmailAndPhone(builder, address2, true);
			AssertEquals(string.Empty, builder.ToString());
		}

		[TestTimeZoneUNLOCO("GBAHE")] // UTC timezone
		public void TestAddOrganizationLastSalesCall()
		{
			var org = Factory.New<OrgHeader>();
			org.MiscServ.OM_CMLastCallDate = new ZDateTime(2016, 9, 26);

			var builder = new ZStringBuilder();
			SalesReminderBuilder.AddOrganizationLastSalesCall(builder, org, false);
			string expectedPlain = @"Last Communication Date: 26-Sep-16
";
			AssertEquals(expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOrganizationLastSalesCall(builder, org, true);
			string expectedHtml = @"<strong>Last Communication Date</strong>: 26-Sep-16
";
			AssertEquals(expectedHtml, builder.ToString());
		}

		public void TestAddOrganizationAndContactDetails()
		{
			var org = Factory.New<OrgHeader>();
			org.MainAddress.Address1 = "3a/72 O'Riordan St";
			org.MainAddress.Address2 = "";
			org.MainAddress.City = "Alexandria";
			org.MainAddress.State = "NSW";
			org.MainAddress.Postcode = "2015";
			org.MainAddress.OA_Email = "contact@wisetechglobal.com";
			org.MainAddress.OA_Phone = "02 98765432";

			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Jenny";
			contact.OC_Phone = "02 13245678";
			contact.OC_Email = "jenny@wisetechglobal.com";
			contact.OC_Mobile = "04 12345678";

			var inquiryOrg = Factory.New<SalesEnquiry>();
			inquiryOrg.OrgPk = org.PK;
			inquiryOrg.O1_OC_LinkedContact = contact.PK;

			var builder = new ZStringBuilder();
			SalesReminderBuilder.AddOrganizationAndContactDetails(builder, org, contact, false);
			string expectedPlain =
@"Contact: Jenny
Job Category: Employee (Undefined)
Email: jenny@wisetechglobal.com
Phone: 02 13245678
Mobile: 04 12345678

Client Address:
3A/72 O'RIORDAN ST
ALEXANDRIA NSW 2015
Office Email: contact@wisetechglobal.com
Office Phone: 02 98765432

";
			AssertMultilineASCIIEquals("Output", expectedPlain, builder.ToString());

			builder = new ZStringBuilder();
			SalesReminderBuilder.AddOrganizationAndContactDetails(builder, org, contact, true);
			string expectedHtml =
@"<strong>Contact</strong>: Jenny
<strong>Job Category</strong>: Employee (Undefined)
<strong>Email</strong>: jenny@wisetechglobal.com
<strong>Phone</strong>: 02 13245678
<strong>Mobile</strong>: 04 12345678

<strong>Client Address</strong>:
3A/72 O'RIORDAN ST
ALEXANDRIA NSW 2015
<strong>Office Email</strong>: contact@wisetechglobal.com
<strong>Office Phone</strong>: 02 98765432

";
			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());
		}
	}
}
