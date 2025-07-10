using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SalesEnquiryDetailsControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestNewRecord()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var testForm = new ZForm(enquiry))
			{
				using (var testControl = new SalesEnquiryDetailsControl())
				{
					testControl.Initialize(enquiry);
					testForm.Controls.Add(testControl);
					testForm.Show();

					AssertEquals("AdditionalEnquiryButtonsGroupBox.Visible", false, testControl.AdditionalEnquiryButtonsGroupBox.Visible);
					AssertEquals("StatusButtonsGroupBox.Visible", false, testControl.StatusButtonsGroupBox.Visible);
				}
			}
		}

		[RequiresSTA]
		public void TestReadOnlyCustomFieldValues()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControl())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", false, enquiry.ReadOnly);
				AssertEquals("Custom Fields should be not read-only", false, control.EnquiryCustomFieldsControl.GetReadOnly());
			}
			enquiry.DoClose();
			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControl())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", true, enquiry.ReadOnly);
				AssertEquals("Custom Fields should be read-only", true, control.EnquiryCustomFieldsControl.GetReadOnly());
			}
		}

		[RequiresSTA]
		public void TestSetOverallDispositionLabelColor()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControl())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();
				AssertEquals(System.Drawing.Color.LimeGreen, control.StatusDescriptionLabel.BackColor);
				enquiry.DoClose();
				AssertEquals(System.Drawing.Color.Red, control.StatusDescriptionLabel.BackColor);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestDoNotAccessPropertiesOfEnquiryIfDeleted()
		{
			var deletedEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var form = new ZForm(deletedEnquiry))
			using (var control = new SalesEnquiryDetailsControl())
			{
				control.Initialize(deletedEnquiry);
				form.Controls.Add(control);
				form.Show();
				deletedEnquiry.Delete();
				Factory.Save();
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestFormClosedIfEnquiryIsNull()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			var opp = Factory.NewWithValidTestData<OrgOpportunity>();
			opp.P8_O1_Enquiry = enquiry.PK;
			enquiry.O1_OH_ConvertedToQualifiedLead = header.PK;
			Factory.Save();

			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControlForTest())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to 'Convert Inquiry to Opportunity' popup

				control.shouldNullEnquiry = true;
				control.CreateSalesOpportunityButton.Parent.Visible = true;
				control.CreateSalesOpportunityButton.PerformClick();

				Application.DoEvents();
				ZFormModaliser.LastFormShownForTest.Close();
				form.Close();
			}
		}

		[RequiresSTA]
		public void TestFormClosedShouldRemoveDelegate()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = header.PK;
			Factory.Save();

			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControlForTest())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to 'Convert Inquiry to Opportunity' popup

				control.CreateSalesOpportunityButton.Parent.Visible = true;
				control.CreateSalesOpportunityButton.PerformClick();

				Application.DoEvents();
				AssertEquals(typeof(OpportunityForm), ZFormModaliser.LastFormShownForTest.GetType());

				var oppForm = (OpportunityForm)ZFormModaliser.LastFormShownForTest;
				var opp = oppForm.BusinessEntity;
				opp.FillWithValidTestData();
				opp.Factory.Save();
				opp.P8_O1_Enquiry = enquiry.PK;
				enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;

				oppForm.FormClosed += CallCloseAgainOnFormClosed;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				oppForm.Close();

				AssertEquals("Should show popup for calling OpportunityFormClosed only once", 2, UnitTestUserNotification.Instance.PreviousMessages.Length);
				AssertNull("Empty message", UnitTestUserNotification.Instance.PreviousMessages[1].Text);
				AssertEquals("Inquiry has been converted to an opportunity", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestInquiryFormIsClosedBeforeCloseOpportunityForm()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = header.PK;
			Factory.Save();

			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControlForTest())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes); // Yes to 'Convert Inquiry to Opportunity' popup

				control.CreateSalesOpportunityButton.Parent.Visible = true;
				control.CreateSalesOpportunityButton.PerformClick();

				Application.DoEvents();
				AssertEquals(typeof(OpportunityForm), ZFormModaliser.LastFormShownForTest.GetType());

				var opp = ((OpportunityForm)ZFormModaliser.LastFormShownForTest).BusinessEntity;
				opp.FillWithValidTestData();
				opp.Factory.Save();
				opp.P8_O1_Enquiry = enquiry.PK;
				enquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Converted;

				form.Close();
				ZFormModaliser.LastFormShownForTest.Close();
			}
		}

		[RequiresSTA]
		public void TestCloseInquiryRights()
		{
			var isCloseAllowed = Env.Security.InquiryManagerClose.IsAllowed;
			Env.Security.InquiryManagerClose.IsAllowed = false;
			var isReopenAllowed = Env.Security.InquiryManagerReopen.IsAllowed;
			Env.Security.InquiryManagerReopen.IsAllowed = false;

			try
			{
				var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
				Factory.Save();
				using (var form = new ZForm(enquiry))
				using (var control = new SalesEnquiryDetailsControl())
				{
					control.Initialize(enquiry);
					form.Controls.Add(control);
					form.Show();
					AssertEquals("Precondition", false, enquiry.IsNotOpen);
					control.CloseEnquiryButton.PerformClick();
					AssertEquals(Env.Security.InquiryManagerClose.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.InquiryManagerClose.IsAllowed = true;
					control.CloseEnquiryButton.PerformClick();
					AssertEquals("Accept rights", true, enquiry.IsNotOpen);

					control.CloseEnquiryButton.PerformClick();
					AssertEquals(Env.Security.InquiryManagerReopen.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

					Env.Security.InquiryManagerReopen.IsAllowed = true;
					control.CloseEnquiryButton.PerformClick();
					AssertEquals("Accept rights", false, enquiry.IsNotOpen);
				}
			}
			finally
			{
				Env.Security.InquiryManagerClose.IsAllowed = isCloseAllowed;
				Env.Security.InquiryManagerReopen.IsAllowed = isCloseAllowed;
			}
		}

		public void TestButtonEnablementViewClosedForm()
		{
			var closedEnquiryToView = Factory.NewWithValidTestData<SalesEnquiry>();
			ZController controller = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);
			closedEnquiryToView.DoClose();
			Factory.Save();
			using (var form = (ZForm)controller.ShowViewForm(closedEnquiryToView))
			{
				Application.DoEvents();
				var control = (SalesEnquiryDetailsControl)form.Controls.Find("enquiryDetailsControl", true)[0];
				AssertEquals("CloseEnquiryButton.Enabled", false, control.CloseEnquiryButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", false, control.CreateSalesOpportunityButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", false, control.LinkOrgButton.Enabled);
			}
		}

		public void TestButtonEnablementEditClosedForm()
		{
			var closedEnquiryToEdit = Factory.NewWithValidTestData<SalesEnquiry>();
			ZController controller = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);
			closedEnquiryToEdit.DoClose();
			Factory.Save();
			using (var form = (ZForm)controller.ShowEditForm(closedEnquiryToEdit))
			{
				Application.DoEvents();
				var control = (SalesEnquiryDetailsControl)form.Controls.Find("enquiryDetailsControl", true)[0];
				AssertEquals("CloseEnquiryButton.Enabled", true, control.CloseEnquiryButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", false, control.CreateSalesOpportunityButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", false, control.LinkOrgButton.Enabled);
			}
		}

		public void TestButtonEnablementViewOpenForm()
		{
			var openEnquiryToView = Factory.NewWithValidTestData<SalesEnquiry>();
			ZController controller = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);
			Factory.Save();
			using (var form = (ZForm)controller.ShowViewForm(openEnquiryToView))
			{
				Application.DoEvents();
				var control = (SalesEnquiryDetailsControl)form.Controls.Find("enquiryDetailsControl", true)[0];
				AssertEquals("CloseEnquiryButton.Enabled", false, control.CloseEnquiryButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", false, control.CreateSalesOpportunityButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", false, control.LinkOrgButton.Enabled);
			}
		}
		public void TestButtonEnablementEditOpenForm()
		{
			var openEnquiryToEdit = Factory.NewWithValidTestData<SalesEnquiry>();
			ZController controller = ZControllerFactory.Create(ControllerIDs.SalesEnquiry);
			Factory.Save();
			using (var form = (ZForm)controller.ShowEditForm(openEnquiryToEdit))
			{
				Application.DoEvents();
				var control = (SalesEnquiryDetailsControl)form.Controls.Find("enquiryDetailsControl", true)[0];
				AssertEquals("CloseEnquiryButton.Enabled", true, control.CloseEnquiryButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", true, control.CreateSalesOpportunityButton.Enabled);
				AssertEquals("CloseEnquiryButton.Enabled", true, control.LinkOrgButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestJobDropEditCodeBoxMaxLength()
		{
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			using (var form = new ZForm(salesEnquiry))
			using (var control = new SalesEnquiryDetailsControl())
			{
				control.Initialize(salesEnquiry);
				form.Controls.Add(control);
				form.Show();

				AssertEquals(AutoOrgColdCallRegister.Schema.O1_JobCategoryMaxLength, control.JobDropEdit.CodeBox.MaxLength);
				AssertEquals(AutoOrgColdCallRegister.Schema.O1_JobCategoryMaxLength, control.JobDropEdit.MaxLength);
			}
		}

		[RequiresSTA]
		public void TestCloseReasonDropEditVisibility()
		{
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			salesEnquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Closed;
			Factory.Save();

			using (var form = new ZForm(salesEnquiry))
			using (var control = new SalesEnquiryDetailsControl())
			{
				Assert("Should be hidden before setting enquiry", !control.CloseReasonDropEdit.Visible);

				control.Initialize(salesEnquiry);
				form.Controls.Add(control);
				form.Show();

				Assert("Should be visible as enquiry is closed", control.CloseReasonDropEdit.Visible);

				salesEnquiry.O1_LeadStatus = SalesEnquiryStatusCodeList.Codes.Open;
				Assert("Should be hidden as enquiry is open", !control.CloseReasonDropEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestScrollbarVisibility()
		{
			var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			using (var form = new ZForm(salesEnquiry))
			using (var control = new SalesEnquiryDetailsControl())
			{
				control.Initialize(salesEnquiry);
				form.Controls.Add(control);
				form.Show();

				ControlDpiScalingHelper.SetHeight(control, 200, true);
				ControlDpiScalingHelper.SetWidth(control, 200, true);
				Application.DoEvents();
				Assert("Vertical scroll is visible when height is less than the AutoScrollMinSize's height.", control.VerticalScroll.Visible);
				Assert("Horizontal scroll is visible when width is less than the AutoScrollMinSize's width.", control.HorizontalScroll.Visible);

				ControlDpiScalingHelper.SetHeight(control, 520, true);
				ControlDpiScalingHelper.SetWidth(control, 1200, true);
				Application.DoEvents();
				Assert("Vertical scroll is not visible when height is greater or equal to the AutoScrollMinSize's height.", !control.VerticalScroll.Visible);
				Assert("Vertical scroll is not visible when width is greater or equal to the the AutoScrollMinSize's width.", !control.HorizontalScroll.Visible);
			}
		}

		#region Address validation

		[RequiresSTA]
		public void TestNotRunAddressValidationWhenTopLevelFormDisplayModeIsDelete()
		{
			var rawValue = Env.Instance.Registry.EnableAddressValidationWebService;
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = ZGuid.Empty;
			enquiry.Address1 = "TEST ADDRESS";
			Factory.Save();

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				using (var form = new ZForm(enquiry))
				using (var control = new SalesEnquiryDetailsControlForTest())
				{
					control.Initialize(enquiry);
					form.Controls.Add(control);
					form.DisplayMode = ODisplayMode.Delete;

					CombineAssertions(() =>
					{
						AssertEquals("Precondition", false, control.ValidateAddressRun);
						AssertEquals("Precondition", false, control.ValidateAddressButtonForTest.ReadOnly);
						AssertEquals("Precondition", false, control.ClearFieldsButtonForTest.ReadOnly);
					});

					form.Show();

					CombineAssertions(() =>
					{
						AssertEquals(false, control.ValidateAddressRun);
						AssertEquals(true, control.ValidateAddressButtonForTest.ReadOnly);
						AssertEquals(true, control.ClearFieldsButtonForTest.ReadOnly);
					});

					form.Close();
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawValue;
			}
		}

		[RequiresSTA]
		public void TestNoCreatedChangesNotificationExceptionThrown_WhenOnLoad()
		{
			using (DataRegistry.Instance.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var salesEnquiry = Factory.NewWithValidTestData<SalesEnquiryForTest>();
				salesEnquiry.Address1 = "xxxx";
				salesEnquiry.O1_PortOrCountry = "AU";
				salesEnquiry.City = "Sydney";
				salesEnquiry.ValidationStatus = AddressValidationStatus.ToBeVerified;
				salesEnquiry.Address1 = "Test Address1";
				salesEnquiry.SetReadOnlyIncludingChildren(false);
				Factory.Save();

				AssertNoExceptionThrown(() =>
				{
					using (TestingState.SuspendIsRunningTests())
					using (var form = new SalesEnquiryForm(salesEnquiry))
					{
						form.Show();
						Assert("Change should not happen on the person when load", !salesEnquiry.HasChanges);
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Validation status has been changed to INV.", AddressValidationStatus.Invalid, salesEnquiry.ValidationStatus);
					}
				});
			}
		}

		#endregion

		#region Contact Controls

		[RequiresSTA]
		public void TestCreateCommunicationOnInquiryContactCall()
		{
			var contact = Factory.NewWithValidTestData<OrgContact>();
			contact.OC_Phone = "02 12345678";
			contact.OC_Mobile = "04 12345678";
			var inquiry = Factory.New<SalesEnquiry>();
			inquiry.O1_OC_LinkedContact = contact.PK;

			var phoneDiallerForTest = new TestPhoneDialler();
			using (var form = new ZForm(inquiry))
			using (var control = new SalesEnquiryDetailsControlForTest())
			{
				control.Initialize(inquiry);
				control.ContactPhoneNumberDialler_Exposed.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				control.ContactMobileNumberDialler_Exposed.PhoneDiallerOverrideForTest = phoneDiallerForTest;
				form.Controls.Add(control);
				form.Show();

				OrganisationsDataRegistry.Instance.CreateCommunicationOnInquiryContactCall.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				{
					control.ContactPhoneNumberDialler_Exposed.CallButton.PerformClick();
					AssertNull(control.ContactPhoneNumberDialler_Exposed.LastCommunicationControllerForTesting);

					control.ContactMobileNumberDialler_Exposed.CallButton.PerformClick();
					AssertNull(control.ContactMobileNumberDialler_Exposed.LastCommunicationControllerForTesting);
				}

				OrganisationsDataRegistry.Instance.CreateCommunicationOnInquiryContactCall.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				{
					control.ContactPhoneNumberDialler_Exposed.CallButton.PerformClick();
					AssertNotNull(control.ContactPhoneNumberDialler_Exposed.LastCommunicationControllerForTesting.LastShownForm);
					using (var communicationForm = (ZForm)control.ContactPhoneNumberDialler_Exposed.LastCommunicationControllerForTesting.LastShownForm)
					{
						var communication = (OrgSalesCall)communicationForm.BusinessEntity;
						AssertEquals(contact.PK, communication.OQ_OC);
					}

					control.ContactMobileNumberDialler_Exposed.CallButton.PerformClick();
					AssertNotNull(control.ContactMobileNumberDialler_Exposed.LastCommunicationControllerForTesting.LastShownForm);
					using (var communicationForm = (ZForm)control.ContactMobileNumberDialler_Exposed.LastCommunicationControllerForTesting.LastShownForm)
					{
						var communication = (OrgSalesCall)communicationForm.BusinessEntity;
						AssertEquals(contact.PK, communication.OQ_OC);
					}
				}
			}
		}

		#endregion

		#region Company Name Text Box

		[SuspendCriticalValidation]
		[RequiresSTA]
		public void TestCompanyNameTextBox_NewOrg()
		{
			var enquiry = Factory.New<SalesEnquiry>();

			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				var companyTextBox = testControl.Controls.Find("CompanyNameTextBox", true)[0] as ZTextBox;
				companyTextBox.Focus();
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3);

				AssertStartsWith("Should bring up client intelligence form", "New Client Intelligence", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				testControl.PressKeyOnCompanyNameTextBox(Keys.F3 | Keys.Alt);

				AssertStartsWith("Should bring up organisation form", "New Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				testControl.PressKeyOnCompanyNameTextBox(Keys.F4);

				AssertEquals("Should bring up search form", "Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		[SuspendCriticalValidation]
		[RequiresSTA]
		public void TestCompanyNameTextBox_LinkedOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			enquiry.O1_OH_ConvertedToQualifiedLead = org.PK;
			Factory.Save();

			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				var companyTextBox = testControl.Controls.Find("CompanyNameTextBox", true)[0] as ZTextBox;
				companyTextBox.Focus();
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3);

				AssertStartsWith("Should bring up client intelligence form", "Edit Client Intelligence", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				testControl.PressKeyOnCompanyNameTextBox(Keys.F3 | Keys.Alt);

				AssertStartsWith("Should bring up organisation form", "Edit Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		[SuspendCriticalValidation]
		[RequiresSTA]
		public void TestCompanyNameTextBox_NewOrgWithNoMatchedOrg()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				var companyTextBox = testControl.Controls.Find("CompanyNameTextBox", true)[0] as ZTextBox;
				companyTextBox.Focus();
				companyTextBox.Text = "Demo Company SCW Test Sydney";

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3);

				AssertEquals("The organization \"DEMO COMPANY SCW TEST SYDNEY\" does not exist. Would you like to create a new organization?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertStartsWith("Should bring up client intelligence form", "New Client Intelligence", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3 | Keys.Alt);

				AssertEquals("The organization \"DEMO COMPANY SCW TEST SYDNEY\" does not exist. Would you like to create a new organization?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertStartsWith("Should bring up organisation form", "New Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		[SuspendCriticalValidation]
		[RequiresSTA]
		public void TestCompanyNameTextBox_NewOrgWithOneMatchedOrg()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "Demo Company SCW Test Sydney";
			Factory.Save();

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				var companyTextBox = testControl.Controls.Find("CompanyNameTextBox", true)[0] as ZTextBox;
				companyTextBox.Focus();
				companyTextBox.Text = "Demo Company SCW Test Sydney";
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3);

				AssertStartsWith("Should bring up client intelligence form", "Edit Client Intelligence", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				testControl.PressKeyOnCompanyNameTextBox(Keys.F3 | Keys.Alt);

				AssertStartsWith("Should bring up organisation form", "Edit Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				testControl.PressKeyOnCompanyNameTextBox(Keys.F4);

				AssertEquals("Should bring up search form", "Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		[SuspendCriticalValidation]
		[RequiresSTA]
		public void TestCompanyNameTextBox_NewOrgWithMoreThanOneMatchedOrgs()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DDDSCWSYD1";
			org1.OH_FullName = "Demo Company SCW Test Sydney";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DDDSCWSYD2";
			org2.OH_FullName = "Demo Company SCW Test Sydney";
			Factory.Save();

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				var companyTextBox = testControl.Controls.Find("CompanyNameTextBox", true)[0] as ZTextBox;
				companyTextBox.Focus();
				companyTextBox.Text = "Demo Company SCW Test Sydney";
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3);

				AssertEquals("Should bring up search form", "Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				testControl.PressKeyOnCompanyNameTextBox(Keys.F3 | Keys.Alt);

				AssertEquals("Should bring up search form", "Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;

				testControl.PressKeyOnCompanyNameTextBox(Keys.F4);

				AssertEquals("Should bring up search form", "Organization", ZFormModaliser.LastFormShownForTest.Text);
				ZFormModaliser.LastFormShownForTest.Close();
				ZFormModaliser.LastFormShownForTest = null;
			}
		}

		[SuspendCriticalValidation]
		[RequiresSTA]
		public void TestCompanyNameTextBox_NewOrgWithoutSecurityRights()
		{
			var enquiry = Factory.New<SalesEnquiry>();

			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				Env.Security.OrgContactNew.IsAllowed = false;
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				var companyTextBox = testControl.Controls.Find("CompanyNameTextBox", true)[0] as ZTextBox;
				companyTextBox.Focus();
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3);

				AssertEquals("ErrorMessageForNotAllowed", Env.Security.OrgContactNew.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrgContactNew.IsAllowed = true;
				Env.Security.ClientIntelligenceModify.IsAllowed = false;
				testControl.PressKeyOnCompanyNameTextBox(Keys.F3);
				AssertEquals("ErrorMessageForNotAllowed", Env.Security.ClientIntelligenceModify.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[SuspendCriticalValidation]
		[RequiresSTA]
		public void TestCompanyNameTextBox_AutoComplete()
		{
			var org1 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DDDSCWSYD1";
			org1.OH_FullName = "Demo Company ABC";
			var org2 = Factory.NewWithValidTestData<OrgHeader>();
			org1.OH_Code = "DDDSCWSYD2";
			org2.OH_FullName = "Demo Company AAA";
			Factory.Save();

			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();
			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				var companyTextBox = testControl.Controls.Find("CompanyNameTextBox", true)[0] as ZTextBox;
				companyTextBox.Focus();
				companyTextBox.Text = "Demo Company A";
				testControl.PressKeyOnCompanyNameTextBox(Keys.Oemplus);

				AssertEquals("DEMO COMPANY AAA", companyTextBox.Text);
				AssertEquals(org2.PK, enquiry.O1_OH_ConvertedToQualifiedLead);

				companyTextBox.Text = "Demo Company AZZ";
				testControl.PressKeyOnCompanyNameTextBox(Keys.Oemplus);

				AssertEquals("DEMO COMPANY AZZ", companyTextBox.Text);
				AssertEquals(ZGuid.Empty, enquiry.O1_OH_ConvertedToQualifiedLead);
			}
		}

		[RequiresSTA]
		public void TestSetCharacterCasing_WhenOrgAllowMixedCaseIsTrue()
		{
			Env.Registry.SetOrgAllowMixedCase(true);

			using (var testControl = new SalesEnquiryDetailsControl())
			{
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, testControl.CompanyNameTextBox.CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, testControl.ContactNameTextBox.CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, testControl.ContactDropEdit.CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, testControl.ReferringContactDropEdit.CharacterCasing);
				AssertEquals("Character casing should be Normal", CharacterCasing.Normal, testControl.ReferToContactDropEdit.CharacterCasing);
			}
		}

		[RequiresSTA]
		public void TestSetCharacterCasing_WhenOrgAllowMixedCaseIsFalse()
		{
			Env.Registry.SetOrgAllowMixedCase(false);

			using (var testControl = new SalesEnquiryDetailsControl())
			{
				AssertEquals("Character casing should be Upper", CharacterCasing.Upper, testControl.CompanyNameTextBox.CharacterCasing);
				AssertEquals("Character casing should be Upper", CharacterCasing.Upper, testControl.ContactNameTextBox.CharacterCasing);
				AssertEquals("Character casing should be Upper", CharacterCasing.Upper, testControl.ContactDropEdit.CharacterCasing);
				AssertEquals("Character casing should be Upper", CharacterCasing.Upper, testControl.ReferringContactDropEdit.CharacterCasing);
				AssertEquals("Character casing should be Upper", CharacterCasing.Upper, testControl.ReferToContactDropEdit.CharacterCasing);
			}
		}

		#endregion

		#region Address Code Guid Drop Edit

		[RequiresSTA]
		public void TestCompanyAddressDropEdit_Visible()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			using (var testForm = new ZForm(enquiry))
			using (var testControl = new SalesEnquiryDetailsControlForTest())
			{
				testControl.Initialize(enquiry);
				testForm.Controls.Add(testControl);
				testForm.Show();

				AssertEquals(false, testControl.GetCompanyAddressDropEdit_ForTesting().Visible);

				enquiry.OrgPk = Factory.New<OrgHeader>().PK;

				AssertEquals(true, testControl.GetCompanyAddressDropEdit_ForTesting().Visible);
			}
		}

		#endregion

		#region ReadOnlyButtons

		[RequiresSTA]
		public void TestReadOnlyButtons()
		{
			var enquiry = Factory.NewWithValidTestData<SalesEnquiry>();

			enquiry.SetReadOnlyIncludingChildren(true);
			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControlForTest())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				AssertEquals(false, control.CloseEnquiryButton.Enabled);
				AssertEquals(false, control.CreateSalesOpportunityButton.Enabled);
				AssertEquals(false, control.LinkOrgButton.Enabled);
			}

			enquiry.SetReadOnlyIncludingChildren(false);
			using (var form = new ZForm(enquiry))
			using (var control = new SalesEnquiryDetailsControlForTest())
			{
				control.Initialize(enquiry);
				form.Controls.Add(control);
				form.Show();
				Application.DoEvents();
				AssertEquals(true, control.CloseEnquiryButton.Enabled);
				AssertEquals(true, control.CreateSalesOpportunityButton.Enabled);
				AssertEquals(true, control.LinkOrgButton.Enabled);
			}
		}

		#endregion

		#region Implementation

		class SalesEnquiryForTest : SalesEnquiry, ISupportWebAddressValidation
		{
			public SalesEnquiryForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
			{
				ValidationStatus = AddressValidationStatus.Invalid;
				return Task.FromResult(new WebAddressValidationResult());
			}
		}

		class SalesEnquiryDetailsControlForTest : SalesEnquiryDetailsControl
		{
			public PhoneDiallerUserControl ContactMobileNumberDialler_Exposed
			{
				get { return MobilePhoneNumberControl.Controls.Find("PhoneDiallerControl", false)[0] as PhoneDiallerUserControl; }
			}

			public PhoneDiallerUserControl ContactPhoneNumberDialler_Exposed
			{
				get { return PhoneNumberControl.Controls.Find("PhoneDiallerControl", false)[0] as PhoneDiallerUserControl; }
			}

			public void PressKeyOnCompanyNameTextBox(Keys keyPressed)
			{
				CompanyNameTextBox_KeyDown(null, new KeyEventArgs(keyPressed));
			}

			public ZGuidDropEdit GetCompanyAddressDropEdit_ForTesting()
			{
				return CompanyAddressDropEdit;
			}

			protected override void OpportunityFormClosed(object sender, EventArgs e)
			{
				if (shouldNullEnquiry)
				{
					Enquiry = null;
				}
				base.OpportunityFormClosed(sender, e);
			}

			public bool shouldNullEnquiry;

			public bool ValidateAddressRun { get; set; }

			protected override async Task ValidateAddress()
			{
				ValidateAddressRun = true;
				await base.ValidateAddress();
			}

			public ZButton ValidateAddressButtonForTest => ValidateAddressButton;

			public ZButton ClearFieldsButtonForTest => ClearFieldsButton;
		}

		void CallCloseAgainOnFormClosed(object sender, FormClosedEventArgs args)
		{
			var form = (ZForm)sender;
			form.FormClosed -= CallCloseAgainOnFormClosed;
			form.Close();
		}

		#endregion
	}
}
