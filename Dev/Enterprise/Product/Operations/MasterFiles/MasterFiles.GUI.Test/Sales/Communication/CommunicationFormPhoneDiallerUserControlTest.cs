using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CommunicationFormPhoneDiallerUserControlTest : TestCaseWithFactory
	{
		public void TestCallButton()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Phone = "02 12345678";
			var contact = org.Contacts.AddNew();
			contact.OC_Phone = "02 98765432";

			var inquiryWithoutPhoneNumbers = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryWithPhoneNumbers = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiryWithPhoneNumbers.O1_Phone = "02 44448888";

			var communication = Factory.New<OrgSalesCall>();
			using (var form = new ZForm(communication))
			using (var control = new CommunicationFormPhoneDiallerUserControl())
			{
				var testPhoneDialler = new TestPhoneDialler();
				control.PhoneDiallerOverrideForTest = testPhoneDialler;
				form.Controls.Add(control);
				form.Show();

				AssertEquals("Call", control.CallButton.TextIgnoringInternalPadding);
				UnitTestUserNotification.Instance.ClearMessages();
				control.CallButton.PerformClick();
				AssertEquals("Contact does not have a Work or Office phone contact details.", UnitTestUserNotification.Instance.LastMessage.Text);

				communication.OQ_OH = org.PK;
				AssertEquals("Office", control.CallButton.TextIgnoringInternalPadding);
				control.CallButton.PerformClick();
				AssertEquals("tel:0212345678", testPhoneDialler.UriDialled);

				communication.OQ_OC = contact.PK;
				AssertEquals("Work", control.CallButton.TextIgnoringInternalPadding);
				control.CallButton.PerformClick();
				AssertEquals("tel:0298765432", testPhoneDialler.UriDialled);

				communication.LinkedInquiry = inquiryWithoutPhoneNumbers;
				AssertEquals("Call", control.CallButton.TextIgnoringInternalPadding);
				UnitTestUserNotification.Instance.ClearMessages();
				control.CallButton.PerformClick();
				AssertEquals("Contact does not have a Work or Office phone contact details.", UnitTestUserNotification.Instance.LastMessage.Text);

				communication.LinkedInquiry = inquiryWithPhoneNumbers;
				AssertEquals("Work", control.CallButton.TextIgnoringInternalPadding);
				control.CallButton.PerformClick();
				AssertEquals("tel:0244448888", testPhoneDialler.UriDialled);
			}
		}

		public void TestDropButton()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_Phone = "02 12345678";
			var contact = org.Contacts.AddNew();
			contact.OC_Phone = "02 98765432";

			var inquiryWithoutPhoneNumbers = Factory.NewWithValidTestData<SalesEnquiry>();
			var inquiryWithPhoneNumbers = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiryWithPhoneNumbers.O1_Phone = "02 44448888";
			inquiryWithPhoneNumbers.O1_Mobile = "04 44448888";

			var communication = Factory.New<OrgSalesCall>();
			using (var form = new ZForm(communication))
			using (var control = new CommunicationFormPhoneDiallerUserControl())
			{
				var testPhoneDialler = new TestPhoneDialler();
				control.PhoneDiallerOverrideForTest = testPhoneDialler;
				form.Controls.Add(control);
				form.Show();

				AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), control.DropButton.Items.Cast<ToolStripDropDownItem>().Select(item => item.Text));

				communication.OQ_OH = org.PK;
				AssertContainsExactElementsInAnyOrder(new[]
					{
						"Call Office (02 12345678)",
						"Call using Skype",
					},
					control.DropButton.Items.Cast<ToolStripDropDownItem>().Select(item => item.Text));

				communication.OQ_OC = contact.PK;
				AssertContainsExactElementsInAnyOrder(new[]
					{
						"Call Office (02 12345678)",
						"Call Work (02 98765432)",
						"Call using Skype",
					},
					control.DropButton.Items.Cast<ToolStripDropDownItem>().Select(item => item.Text));

				communication.LinkedInquiry = inquiryWithoutPhoneNumbers;
				AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), control.DropButton.Items.Cast<ToolStripDropDownItem>().Select(item => item.Text));

				communication.LinkedInquiry = inquiryWithPhoneNumbers;
				AssertContainsExactElementsInAnyOrder(new[]
					{
						"Call Work (02 44448888)",
						"Call Mobile (04 44448888)",
						"Call using Skype",
					},
					control.DropButton.Items.Cast<ToolStripDropDownItem>().Select(item => item.Text));
			}
		}
	}
}
