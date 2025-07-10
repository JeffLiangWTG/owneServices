using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Recruiter.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Recruiter.GUI.Applicant.Testing
{
	class HRContactPhoneDiallerUserControlTest : TestCaseWithFactory
	{
		const string mobilePhone = "04 1111 9999";
		const string homePhone = "02 1111 9999";
		const string workPhone = "02 1111 8888";
		public void TestGetBoundApplicationWhenNoneExists()
		{
			using (var ctrl = new HRContactPhoneDialUserControl())
			{
				AssertNull(ctrl.Application);
			}
		}

		public void TestCallButton()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			VerifyExpectedDefaultResult(applicant, "Call", "No phone number available.", null);
			applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			applicant.HA_MobilePhone = mobilePhone;
			VerifyExpectedDefaultResult(applicant, "Mobile", null, "tel:0411119999");
			applicant.HA_MobilePhone = mobilePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDefaultResult(applicant, "Mobile", null, "tel:0411119999");
			applicant.HA_MobilePhone = string.Empty;
			applicant.HA_HomePhone = homePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDefaultResult(applicant, "Home", null, "tel:0211119999");
			applicant.HA_MobilePhone = string.Empty;
			applicant.HA_HomePhone = string.Empty;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDefaultResult(applicant, "Work", null, "tel:0211118888");
		}

		public void TestDropButton()
		{
			var applicant = Factory.NewWithValidTestData<HRJobApplicant>();
			VerifyExpectedDropdownResult(applicant, System.Array.Empty<string>());
			applicant.HA_MobilePhone = mobilePhone;
			VerifyExpectedDropdownResult(applicant, new[] { "Call Mobile (04 1111 9999)", "Call using Skype" });
			applicant.HA_MobilePhone = mobilePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDropdownResult(applicant, new[] { "Call Mobile (04 1111 9999)", "Call Work (02 1111 8888)", "Call using Skype" });
			applicant.HA_MobilePhone = mobilePhone;
			applicant.HA_HomePhone = homePhone;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDropdownResult(applicant, new[] { "Call Mobile (04 1111 9999)", "Call Home (02 1111 9999)", "Call Work (02 1111 8888)", "Call using Skype" });
			applicant.HA_MobilePhone = string.Empty;
			applicant.HA_HomePhone = string.Empty;
			applicant.HA_WorkPhone = workPhone;
			VerifyExpectedDropdownResult(applicant, new[] { "Call Work (02 1111 8888)", "Call using Skype" });
		}

		#region implementation
		void VerifyExpectedDropdownResult(HRJobApplicant applicant, string[] result)
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = applicant.PK;
			using (var form = new ZForm(application))
			using (var control = new HRContactPhoneDialUserControl())
			{
				var testPhoneDialler = new TestPhoneDialler();
				control.PhoneDiallerOverrideForTest = testPhoneDialler;
				form.Controls.Add(control);
				form.Show();
				AssertArrayEqualsByElements(result, control.DropButton.Items.Cast<ToolStripItem>().Select(item => item.Text).ToArray());
			}
		}

		void VerifyExpectedDefaultResult(HRJobApplicant applicant, string callButton, string notificationMessage, string uriDialled)
		{
			var campaign = Factory.NewWithValidTestData<HRRecruitmentJobCampaign>();
			HRJobApplication application = campaign.Applications.AddNew();
			application.HP_HA = applicant.PK;
			using (var form = new ZForm(application))
			using (var control = new HRContactPhoneDialUserControl())
			{
				var testPhoneDialler = new TestPhoneDialler();
				control.PhoneDiallerOverrideForTest = testPhoneDialler;
				form.Controls.Add(control);
				form.Show();
				AssertEquals(callButton, control.CallButton.TextIgnoringInternalPadding);
				UnitTestUserNotification.Instance.ClearMessages();
				control.CallButton.PerformClick();
				AssertEquals(notificationMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(uriDialled, testPhoneDialler.UriDialled);
			}
		}
		#endregion
	}
}
