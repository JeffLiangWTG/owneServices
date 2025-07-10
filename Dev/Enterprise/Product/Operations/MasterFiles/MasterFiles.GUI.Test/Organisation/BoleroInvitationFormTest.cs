using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(BoleroInvitationForm))]
	sealed class BoleroInvitationFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			var org = Factory.New<OrgHeader>();
			var invitationDetails = new BoleroInvitationDetails(org);
			return new BoleroInvitationForm(invitationDetails);
		}

		public override void TestBoundListsAreNotLoadedOnAccess() => Assert(true);

		public override void TestFormIsFullyTranslatable()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_FullName = string.Empty;
			currentUser.GS_EmailAddress = string.Empty;
			Factory.Save();

			base.TestFormIsFullyTranslatable();
		}

		public void TestSendInviteShowValidationErrors()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_FullName = string.Empty;
			currentUser.GS_EmailAddress = string.Empty;

			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var invitationDetails = new BoleroInvitationDetails(org);
			using (var testForm = new BoleroInvitationForm(invitationDetails))
			{
				testForm.Show();

				invitationDetails.SelectedContactPK = ZGuid.Empty;

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var sendInviteButton = (ZButton)testForm.Controls.Find("sendInviteButton", true)[0];
				sendInviteButton.PerformClick();

				AssertEquals(4, invitationDetails.Notifications.Count());
				Assert(invitationDetails.Notifications.Any(n => n.Message == "Error - SelectedContactPK: Please enter a contact name."));
				Assert(invitationDetails.Notifications.Any(n => n.Message == "Error - SelectedContactEmail: Please enter a contact email address."));
				Assert(invitationDetails.Notifications.Any(n => n.Message == "Error - YourName: Please enter a login user's name."));
				Assert(invitationDetails.Notifications.Any(n => n.Message == "Error - YourEmail: Please enter a login user's email address."));
			}
		}

		public void TestSendInviteShowExtraErrors()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_FullName = "Andy Zhang";
			currentUser.GS_EmailAddress = "123456@163.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_RL_NKClosestPort = "AU";
			var mainAddress = org.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "AU";
			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "1234567";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "123456@163.com";
			contact.OC_ContactName = "John Smith";
			contact.OC_OH = org.PK;
			Factory.Save();

			var invitationDetails = new BoleroInvitationDetails(org);
			invitationDetails.SelectedContactPK = contact.PK;
			using (var testForm = new BoleroInvitationForm(invitationDetails))
			{
				testForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var sendInviteButton = (ZButton)testForm.Controls.Find("sendInviteButton", true)[0];
				sendInviteButton.PerformClick();

				AssertEquals(0, invitationDetails.Notifications.Count());
				var message = ((UnitTestUserNotification)Globals.Message).LastMessage.ToString();
				AssertEquals(@"Error LegalCompanyName, City, InviterMessage are required to Enroll for Electronic Bills of Lading.
Please ensure the organization type is either consignee or consignor.", message);
			}
		}

		public void TestSendInvite()
		{
			var currentUser = Factory.Load<GlbStaff>(Env.CurrentUserPK);
			currentUser.GS_FullName = "Test User";
			currentUser.GS_EmailAddress = "TestUser@123.com";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Test Org";
			org.OH_IsConsignee = true;
			var mainAddress = org.MainAddress;
			mainAddress.OA_RN_NKCountryCode = "AU";
			mainAddress.City = "Test City";

			var cusCode = org.CustomsCodes.AddNew();
			cusCode.OK_CodeType = OrgCusCode.AustraliaCodeTypes.AustralianBusinessNumber;
			cusCode.OK_RN_NKCodeCountry = "AU";
			cusCode.OK_CustomsRegNo = "1234567";

			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Email = "contact1@email.em";
			org.Contacts.Add(contact);
			Factory.Save();

			var invitationDetails = new BoleroInvitationDetails(org) { SelectedContactPK = contact.PK, YourMessage = "Test Message" };
			using (var testForm = new BoleroInvitationForm(invitationDetails))
			{
				testForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNull(testForm.OnboardingRequestDTO);

				var sendInviteButton = (ZButton)testForm.Controls.Find("sendInviteButton", true)[0];
				sendInviteButton.PerformClick();
				AssertNotNull(testForm.OnboardingRequestDTO);
				AssertEquals("Enrollment request sent.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(DialogResult.OK, testForm.DialogResult);
			}
		}
	}
}
