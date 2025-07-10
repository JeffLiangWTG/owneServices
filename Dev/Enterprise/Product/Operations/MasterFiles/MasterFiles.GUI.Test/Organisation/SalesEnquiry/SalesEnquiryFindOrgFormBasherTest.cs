using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(SalesEnquiryFindOrgForm))]
	sealed class SalesEnquiryFindOrgFormBasherTest : ZFormBasherTest
	{
		public void TestNoExceptionsWhenAnotherFormIsOpenedAfterLinkingToOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			Factory.Save();

			var tempFactory = new BusinessObjectFactory();
			var orgForMatching = tempFactory.New<OrgHeaderForEnquiryMatching>();
			enquiry.PopulateOrg(orgForMatching);
			var finder = new EnquiryOrgFinder(tempFactory, orgForMatching, true, true);
			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldLinkOrganizationAddressToInquiry = true;
			finder.SingleOrgPk = org.PK;
			finder.SelectedAddressPk = org.MainAddress.PK;

			ZFormModaliser.ShowDialogsInTest = true;
			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(enquiry, finder, true))
			{
				findOrgForm.Show();
				findOrgForm.FormClosed += (s, e) => ZFormModaliser.ShowDialogAndDispose(new ZForm());

				AssertNoExceptionThrown(() =>
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					findOrgForm.OkButton_Click();
				});
			}
		}
		public void TestWithConcurrency()
		{
			Factory.RefreshEnabled = false;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TEST ORGANISATION";
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_CompanyName = "TEST COMPANY";
			enquiry.O1_ContactName = "Andrew";
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var orgForMatching = Factory.New<OrgHeaderForEnquiryMatching>();
			enquiry.PopulateOrg(orgForMatching);
			var finder = new EnquiryOrgFinder(Factory, orgForMatching, true, true);
			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldLinkOrganizationAddressToInquiry = true;
			finder.SingleOrgPk = org.PK;

			var enquiryInNewFactory = newFactory.Load<SalesEnquiry>(enquiry.PK);
			var orgInNewFactory = newFactory.NewWithValidTestData<OrgHeader>();
			orgInNewFactory.OH_Code = "TESTAA";
			enquiryInNewFactory.OrgPk = orgInNewFactory.PK;
			newFactory.Save();

			ZFormModaliser.ShowDialogsInTest = true;
			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(enquiry, finder, true))
			{
				findOrgForm.Show();
				findOrgForm.FormClosed += (s, e) => ZFormModaliser.ShowDialogAndDispose(new ZForm());
				finder.SelectedAddressPk = org.MainAddress.PK;

				AssertExceptionThrown<ZSaveConcurrencyException>(() =>
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					findOrgForm.OkButton_Click();
				});

				AssertEquals("Linked organization should be merged", orgInNewFactory.PK, enquiry.OrgPk);
			}
		}

		[RequiresSTA]
		public void TestLinkedOrganizationWithError()
		{
			Factory.RefreshEnabled = false;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TEST ORGANISATION";
			var contact = org.Contacts.AddNew();
			contact.OC_ContactName = "Andrew";
			contact.OC_IsActive = false;
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_CompanyName = "TEST COMPANY";
			enquiry.O1_ContactName = "Andrew";
			Factory.Save();

			var orgForMatching = Factory.New<OrgHeaderForEnquiryMatching>();
			enquiry.PopulateOrg(orgForMatching);
			var finder = new EnquiryOrgFinder(Factory, orgForMatching, true, true);
			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldLinkOrganizationAddressToInquiry = true;
			finder.SingleOrgPk = org.PK;

			ZFormModaliser.ShowDialogsInTest = true;
			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(enquiry, finder, true))
			{
				findOrgForm.Show();
				findOrgForm.FormClosed += (s, e) => ZFormModaliser.ShowDialogAndDispose(new ZForm());
				finder.SelectedAddressPk = org.MainAddress.PK;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				findOrgForm.OkButton_Click();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				Assert(enquiry.HasErrors);
				AssertEquals("Should not save enquiry when it has error!", "There are errors that need to be corrected before this Inquiry (I00001000) can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateNewOrganization_NoSecurityRight()
		{
			Env.Security.ClientIntelligenceModify.IsAllowed = false;

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_CompanyName = "TEST ORGANISATION";
			inquiry.O1_ContactName = "Andrew";
			Factory.Save();

			var tempFactory = new BusinessObjectFactory();
			var orgForMatching = tempFactory.New<OrgHeaderForEnquiryMatching>();
			inquiry.PopulateOrg(orgForMatching);

			var finder = new EnquiryOrgFinder(tempFactory, orgForMatching, true, true);
			finder.ShouldAllowWebAccess = true;

			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(inquiry, finder, true))
			{
				findOrgForm.Show();
				finder.ShouldCreateNewClientIntelligence = true;
				Application.DoEvents();

				findOrgForm.OkButton_Click();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(Env.Security.ClientIntelligenceModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.ClientIntelligenceModify.IsAllowed = true;
				Env.Security.OrgContactNew.IsAllowed = false;

				findOrgForm.OkButton_Click();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(Env.Security.OrgContactNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrgContactNew.IsAllowed = true;
				Env.Security.OrgAddressNew.IsAllowed = false;

				findOrgForm.OkButton_Click();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(Env.Security.OrgAddressNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrgAddressNew.IsAllowed = true;
				Env.Security.OrgAddressDetailsNonARAPNew.IsAllowed = false;

				findOrgForm.OkButton_Click();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals(Env.Security.OrgAddressDetailsNonARAPNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCreateNewOrganization_WebAccessSecurityRight()
		{
			Env.Security.ClientIntelligenceModify.IsAllowed = true;
			Env.Security.OrgDetailsNewWebSecurity.IsAllowed = false;

			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_CompanyName = "TEST ORGANISATION";
			inquiry.O1_ContactName = "Test Contact";
			Factory.Save();

			var factory = new BusinessObjectFactory();
			var orgForMatching = factory.New<OrgHeaderForEnquiryMatching>();
			inquiry.PopulateOrg(orgForMatching);

			var finder = new EnquiryOrgFinder(factory, orgForMatching, true, true);
			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(inquiry, finder, true))
			{
				findOrgForm.Show();
				finder.ShouldCreateNewClientIntelligence = true;
				Application.DoEvents();
				AssertEquals("Modify web access is not allowed", false, findOrgForm.ApproveWebAccessGroupBox_Exposed.Enabled);
			}

			Env.Security.OrgDetailsNewWebSecurity.IsAllowed = true;
			finder = new EnquiryOrgFinder(factory, orgForMatching, true, true);
			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(inquiry, finder, true))
			{
				findOrgForm.Show();
				finder.ShouldCreateNewClientIntelligence = true;
				Application.DoEvents();
				AssertEquals("Modify web access is allowed", true, findOrgForm.ApproveWebAccessGroupBox_Exposed.Enabled);
			}
		}

		[RequiresSTA]
		public void TestCreateNewOrganization()
		{
			var inquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			inquiry.O1_CompanyName = "TEST ORGANISATION";
			inquiry.O1_ContactName = "Andrew";
			Factory.Save();

			var tempFactory = new BusinessObjectFactory();
			var orgForMatching = tempFactory.New<OrgHeaderForEnquiryMatching>();
			inquiry.PopulateOrg(orgForMatching);

			var finder = new EnquiryOrgFinder(tempFactory, orgForMatching, true, true);
			finder.ShouldAllowWebAccess = true;

			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(inquiry, finder, true))
			{
				findOrgForm.Show();
				finder.ShouldCreateNewClientIntelligence = true;
				Application.DoEvents();

				findOrgForm.OkButton_Click();

				AssertNotNull(ZFormModaliser.LastFormShownForTest);
				try
				{
					AssertType(typeof(ZClientIntelligenceForm), ZFormModaliser.LastFormShownForTest);
					var org = (OrgHeader)((ZClientIntelligenceForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
					AssertEquals("TEST ORGANISATION", org.OH_FullName);
					AssertEquals(1, org.Contacts.Count);
					var contact = org.Contacts[0];
					AssertEquals("Andrew", contact.OC_ContactName);
					AssertEquals(true, contact.OC_WebAccessEnabled);
				}
				finally
				{
					ZFormModaliser.LastFormShownForTest.Dispose();
				}
			}
		}

		public void TestLinkToExistingClientIntelligence_NewContactWithNoSecurityRight()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var contact = Factory.New<OrgContact>();
			org.Contacts.Add(contact);
			contact.OC_ContactName = "Matching Contact";
			enquiry.O1_CompanyName = "TEST COMPANY";
			enquiry.O1_ContactName = "New Contact";
			enquiry.O1_Email = "test@123.com";
			Factory.Save();

			var tempFactory = new BusinessObjectFactory();
			var orgForMatching = tempFactory.New<OrgHeaderForEnquiryMatching>();
			enquiry.PopulateOrg(orgForMatching);
			var finder = new EnquiryOrgFinder(tempFactory, orgForMatching, true, true);
			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldAddInquiryAddressToOrganization = true;
			finder.SingleOrgPk = org.PK;
			finder.SelectedAddressPk = org.MainAddress.PK;

			ZFormModaliser.ShowDialogsInTest = true;
			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(enquiry, finder, false))
			{
				findOrgForm.Show();
				Application.DoEvents();

				Env.Security.OrgContactModify.IsAllowed = false;

				findOrgForm.OkButton_Click();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals("OrgContactModify is required for LinkToExistingClientIntelligence", Env.Security.OrgContactModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrgContactModify.IsAllowed = true;
				Env.Security.OrgContactModifyContactDetails.IsAllowed = false;

				findOrgForm.OkButton_Click();

				AssertNull(ZFormModaliser.LastFormShownForTest);
				AssertEquals("OrgContactModifyContactDetails is required for LinkToExistingClientIntelligence", Env.Security.OrgContactModifyContactDetails.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrgContactModifyContactDetails.IsAllowed = true;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				findOrgForm.OkButton_Click();

				var newContact = org.Contacts.Cast<OrgContact>().Any(c => c.OC_ContactName == "New Contact");
				Assert(newContact);
			}
		}
		[RequiresSTA]
		public void TestLinkToExistingClientIntelligence_UpdateContactWithNoSecurityRight()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var contact = Factory.New<OrgContact>();
			org.Contacts.Add(contact);
			contact.OC_ContactName = "Matching Contact";
			enquiry.O1_ContactName = "Matching Contact";
			enquiry.O1_Email = "test@123.com";
			Factory.Save();

			var tempFactory = new BusinessObjectFactory();
			var orgForMatching = tempFactory.New<OrgHeaderForEnquiryMatching>();
			enquiry.PopulateOrg(orgForMatching);
			var finder = new EnquiryOrgFinder(tempFactory, orgForMatching, true, true);
			finder.ShouldLinkToExistingClientIntelligence = true;
			finder.ShouldAddInquiryAddressToOrganization = true;
			finder.SingleOrgPk = org.PK;
			finder.SelectedAddressPk = org.MainAddress.PK;

			ZFormModaliser.ShowDialogsInTest = true;
			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(enquiry, finder, false))
			{
				findOrgForm.Show();
				Application.DoEvents();

				Env.Security.OrgContactModify.IsAllowed = false;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				findOrgForm.OkButton_Click();

				AssertEquals("Mail for matching contact should not be updated!", "", contact.OC_Email);
			}

			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(enquiry, finder, false))
			{
				findOrgForm.Show();
				Application.DoEvents();

				Env.Security.OrgContactModify.IsAllowed = true;
				Env.Security.OrgContactModifyContactDetails.IsAllowed = false;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				findOrgForm.OkButton_Click();

				AssertEquals("Mail for matching contact should not be updated!", "", contact.OC_Email);
			}

			using (var findOrgForm = new SalesEnquiryFindOrgFormForTesting(enquiry, finder, false))
			{
				findOrgForm.Show();
				Application.DoEvents();

				Env.Security.OrgContactModifyContactDetails.IsAllowed = true;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				findOrgForm.OkButton_Click();
				AssertEquals("Mail for matching contact should be updated!", "test@123.com", contact.OC_Email);
			}
		}

		#region Implementation

		class SalesEnquiryFindOrgFormForTesting : SalesEnquiryFindOrgForm
		{
			public SalesEnquiryFindOrgFormForTesting(SalesEnquiry enquiry, EnquiryOrgFinder finder, bool saveEnquiryOnClosed)
				: base(enquiry, finder, saveEnquiryOnClosed)
			{
			}

			public void OkButton_Click()
			{
				OkButton_Click(null, null);
			}

			public ZGroupBox ApproveWebAccessGroupBox_Exposed => ApproveWebAccessGroupBox;
		}

		protected override Form GetFormToBashCore()
		{
			SalesEnquiry enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			OrgHeaderForEnquiryMatching org = Factory.NewWithValidTestData<OrgHeaderForEnquiryMatching>();
			org.OH_FullName = "AAAAAAAAAAA";
			EnquiryOrgFinder finder = new EnquiryOrgFinder(enquiry.Factory, org, true, true);
			return new SalesEnquiryFindOrgForm(enquiry, finder, false);
		}

		protected override void SetUp()
		{
			base.SetUp();
			Env.Security.OrgContactNew.IsAllowed = true;
			Env.Security.OrgAddressModify.IsAllowed = true;
			Env.Security.OrgAddressListModify.IsAllowed = true;
			Env.Security.OrgAddressCapabilitiesNonARAP.IsAllowed = true;
			Env.Security.OrgAddressDetailsModify.IsAllowed = true;
			Env.Security.OrgAddressDetailsNonARAP.IsAllowed = true;
			Env.Security.OrganisationCRMSecurity.EditByStaffNotAssigned.IsAllowed = true;
		}
		#endregion
	}
}
