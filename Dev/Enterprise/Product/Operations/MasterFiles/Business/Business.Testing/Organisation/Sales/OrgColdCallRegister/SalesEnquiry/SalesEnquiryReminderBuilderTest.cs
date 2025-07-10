using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class SalesEnquiryReminderBuilderTest : TestCaseWithFactory
	{
		public void TestAddOrganizationAndContactDetails_Inquiry()
		{
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_ContactName = "Andrew";
			inquiry.O1_Phone = "02 13245678";
			inquiry.O1_Email = "andrew@wisetechglobal.com";
			inquiry.O1_Mobile = "04 12345678";
			inquiry.O1_JobCategory = "Developer";

			inquiry.O1_Address1 = "3a/72 O'Riordan St";
			inquiry.O1_Address2 = "";
			inquiry.O1_City = "Alexandria";
			inquiry.O1_State = "NSW";
			inquiry.O1_PostCode = "2015";

			ZStringBuilder builder = new ZStringBuilder();
			SalesEnquiryReminderBuilder.AddOrganizationAndContactDetails(builder, inquiry, false);
			string expected =
@"Contact: Andrew
Job Category: Developer
Email: andrew@wisetechglobal.com
Phone: 02 13245678
Mobile: 04 12345678

Client Address:
3A/72 O'RIORDAN ST
ALEXANDRIA NSW 2015
";

			AssertMultilineASCIIEquals("Output", expected, builder.ToString());

			builder = new ZStringBuilder();
			SalesEnquiryReminderBuilder.AddOrganizationAndContactDetails(builder, inquiry, true);
			string expectedHtml =
@"<strong>Contact</strong>: Andrew
<strong>Job Category</strong>: Developer
<strong>Email</strong>: andrew@wisetechglobal.com
<strong>Phone</strong>: 02 13245678
<strong>Mobile</strong>: 04 12345678

<strong>Client Address</strong>:
3A/72 O'RIORDAN ST
ALEXANDRIA NSW 2015
";

			AssertMultilineASCIIEquals("Output", expectedHtml, builder.ToString());
		}

		public void TestAddOrganizationAndContactDetails_Org()
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
			contact.OC_JobCategory = "Developer";

			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.OrgPk = org.PK;
			inquiry.O1_OC_LinkedContact = contact.PK;

			ZStringBuilder builder = new ZStringBuilder();
			SalesEnquiryReminderBuilder.AddOrganizationAndContactDetails(builder, inquiry, false);
			string expectedPlain =
@"Contact: Jenny
Job Category: Developer
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
			SalesEnquiryReminderBuilder.AddOrganizationAndContactDetails(builder, inquiry, true);

			string expectedHtml =
@"<strong>Contact</strong>: Jenny
<strong>Job Category</strong>: Developer
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
