using System;
using System.Data;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Interop;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.MasterFiles.Business;
using Enterprise.Recruiter.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(PhoneNumberUserControlFormForTest))]
	sealed class PhoneNumberUserControlTest : ZFormBasherTest
	{
		#region Overrides / Methods

		readonly Color ValidColour = Color.FromArgb(198, 236, 198);

		public void TestBackgroundColourIfEnableValidStateColor()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			var number = new PhoneNumber(dummy.Z0_NVarCharInfo, null, null);
			using (dummy.SuspendValidationTesting())
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.PhoneNumberControl.NumberTextBox_Exposed.EnableValidStateColor = true;
				dummy.Z0_NVarChar = "9876 5432";
				form.Show();
				AssertEquals("Valid colour", ValidColour, form.PhoneNumberControl.NumberTextBox_Exposed.BackColor);
				dummy.Z0_NVarChar = string.Empty; // trigger colour change
				AssertEquals("Default colour", SystemColors.Info, form.PhoneNumberControl.NumberTextBox_Exposed.BackColor);
				dummy.Z0_NVarChar = "9876 5432"; // trigger colour change
				AssertEquals("Valid colour", ValidColour, form.PhoneNumberControl.NumberTextBox_Exposed.BackColor);
			}
			using (var form = new PhoneNumberUserControlFormForTest(number, false))
			{
				form.PhoneNumberControl.NumberTextBox_Exposed.EnableValidStateColor = true;
				dummy.Z0_NVarChar = "9876 5432";
				form.Show();
				AssertEquals("Disabled colour", SystemColors.Window, form.PhoneNumberControl.NumberTextBox_Exposed.BackColor);
				dummy.Z0_NVarChar = string.Empty; // trigger colour change
				AssertEquals("Disabled colour", SystemColors.Window, form.PhoneNumberControl.NumberTextBox_Exposed.BackColor);
				dummy.Z0_NVarChar = "9876 5432"; // trigger colour change
				AssertEquals("Disabled colour", SystemColors.Window, form.PhoneNumberControl.NumberTextBox_Exposed.BackColor);
			}
		}

		public void TestLocalNumberLinkLabel_LinkClicked()
		{
			SetupPhoneDialingUriProtocols(true);

			var staff = Factory.New<GlbStaff>();

			var number = new PhoneNumber(staff.GS_MobilePhoneInfo, null, staff.GS_HomePhoneInfo);
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.Show();

				staff.GS_MobilePhone = string.Empty;
				InvokeLinkLabelClick(form.PhoneNumberControl.LocalNumberLinkLabel_Exposed);
				AssertNull(form.PhoneNumberControl.PhoneDiallerForTest.UriDialled);

				staff.GS_MobilePhone = "0412 345 678";
				staff.GS_HomePhone = "345 678"; // pretend this binded property calculates the local number if needed
				InvokeLinkLabelClick(form.PhoneNumberControl.LocalNumberLinkLabel_Exposed);
				AssertEquals("callto:0412345678", form.PhoneNumberControl.PhoneDiallerForTest.UriDialled);

				SetupPhoneDialingUriProtocols(false);
				InvokeLinkLabelClick(form.PhoneNumberControl.LocalNumberLinkLabel_Exposed);
				AssertEquals("tel:0412345678", form.PhoneNumberControl.PhoneDiallerForTest.UriDialled);
			}
		}

		public void TestOnCurrentDataItemChanged_PhoneNumberProperty()
		{
			var staff = Factory.New<GlbStaff>();
			staff.GS_MobilePhone = "+61 4 1234 5678";
			var number = new PhoneNumber(staff.GS_MobilePhoneInfo, null, null);
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.Show();
				AssertEquals("PhoneNumberProperty", staff.GS_MobilePhoneInfo, form.PhoneNumberControl.NumberTextBox_Exposed.PhoneNumberProperty);
				AssertNull("PhoneNumberTooltipProperty", form.PhoneNumberControl.NumberTextBox_Exposed.PhoneNumberTooltipProperty);

				form.PhoneNumberControl.SetDataBinding(null, "");
				AssertEquals("NumberTextBox Text should be blank", true, string.IsNullOrEmpty(form.PhoneNumberControl.NumberTextBox_Exposed.Text));
				AssertEquals("NumberTextBox BackColor should be default", SystemColors.Info, form.PhoneNumberControl.NumberTextBox_Exposed.BackColor);
			}

			var address = Factory.New<OrgAddress>();
			number = new PhoneNumber(address.OA_Fax_FormattedInfo, null, address.OA_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo);
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.PhoneNumberControl.ShowToolTip = true;
				form.Show();
				AssertEquals("PhoneNumberProperty", address.OA_Fax_FormattedInfo, form.PhoneNumberControl.NumberTextBox_Exposed.PhoneNumberProperty);
				AssertEquals("PhoneNumberTooltipProperty", address.OA_Fax_FormattedLocalNumberIfLoggedInSameCountryInfo, form.PhoneNumberControl.NumberTextBox_Exposed.PhoneNumberTooltipProperty);
			}

			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.PhoneNumberControl.ShowToolTip = false;
				form.Show();
				AssertEquals("PhoneNumberProperty", address.OA_Fax_FormattedInfo, form.PhoneNumberControl.NumberTextBox_Exposed.PhoneNumberProperty);
				AssertNull("PhoneNumberTooltipProperty", form.PhoneNumberControl.NumberTextBox_Exposed.PhoneNumberTooltipProperty);
			}
		}

		public void TestOnLoad_SetLocationsForControlsThatCanToggle()
		{
			int paddingBetweenControls = ControlDpiScalingHelper.ScaleToCurrentDpiX(6);

			// hidden dialler only
			using (var form = new ZForm())
			using (var control = new PhoneNumberUserControlForTest())
			{
				control.ShowDiallerControl = false;
				form.Controls.Add(control);
				var originalLeft = control.LocalNumberLinkLabel_Exposed.Left;
				form.Show();

				var expectedLeft = originalLeft - (control.PhoneDiallerControl_Exposed.Width + paddingBetweenControls);
				AssertEquals(expectedLeft, control.LocalNumberLinkLabel_Exposed.Left);
			}

			// hidden local number label only
			using (var form = new ZForm())
			using (var control = new PhoneNumberUserControlForTest())
			{
				control.ShowLocalNumberLabel = false;
				form.Controls.Add(control);
				var originalLeft = control.PublishCheckEdit_Exposed.Left;
				form.Show();

				var expectedLeft = originalLeft - (control.LocalNumberLinkLabel_Exposed.Width + paddingBetweenControls);
				AssertEquals(expectedLeft, control.PublishCheckEdit_Exposed.Left);
			}

			// adjusted padding only
			AssertOnLoad_SetLocationsForControlsThatCanToggle(10);
			AssertOnLoad_SetLocationsForControlsThatCanToggle(-20);

			// hidden dialler and local number label (see GlbStaffForm)
			using (var form = new ZForm())
			using (var control = new PhoneNumberUserControlForTest())
			{
				control.ShowDiallerControl = false;
				control.ShowLocalNumberLabel = false;
				form.Controls.Add(control);
				var originalLeft = control.PublishCheckEdit_Exposed.Left;
				form.Show();

				var expectedLeft = control.PhoneDiallerControl_Exposed.Left;
				AssertEquals(expectedLeft, control.PublishCheckEdit_Exposed.Left);
			}

			// hidden dialler and local number label and adjusted padding (see GlbStaffForm)
			using (var form = new ZForm())
			using (var control = new PhoneNumberUserControlForTest())
			{
				control.ShowDiallerControl = false;
				control.ShowLocalNumberLabel = false;
				control.UnscaledLeftPadding = 15;
				form.Controls.Add(control);
				var originalLeft_NumberTextBox = control.NumberTextBox_Exposed.Left;
				var originalLeft_PhoneDiallerControl = control.PhoneDiallerControl_Exposed.Left;
				var scaledLeftPadding = ControlDpiScalingHelper.ScaleToCurrentDpiX(15);
				form.Show();

				var expectedLeft_NumberTextBox = scaledLeftPadding + originalLeft_NumberTextBox;
				AssertEquals(expectedLeft_NumberTextBox, control.NumberTextBox_Exposed.Left);

				var expectedLeft_PublishCheckEdit = scaledLeftPadding + originalLeft_PhoneDiallerControl;
				AssertEquals(expectedLeft_PublishCheckEdit, control.PublishCheckEdit_Exposed.Left);
			}
		}

		void AssertOnLoad_SetLocationsForControlsThatCanToggle(int unscaledLeftPadding)
		{
			using (var form = new ZForm())
			using (var control = new PhoneNumberUserControlForTest())
			{
				control.UnscaledLeftPadding = unscaledLeftPadding;
				form.Controls.Add(control);
				var originalLeft_NumberTextBox = control.NumberTextBox_Exposed.Left;
				var originalLeft_PhoneDiallerControl = control.PhoneDiallerControl_Exposed.Left;
				var originalLeft_LocalNumberLinkLabel = control.LocalNumberLinkLabel_Exposed.Left;
				var originalLeft_PublishCheckEdit = control.PublishCheckEdit_Exposed.Left;
				var scaledLeftPadding = ControlDpiScalingHelper.ScaleToCurrentDpiX(unscaledLeftPadding);
				form.Show();

				var expectedLeft_NumberTextBox = scaledLeftPadding + originalLeft_NumberTextBox;
				AssertEquals(expectedLeft_NumberTextBox, control.NumberTextBox_Exposed.Left);

				var expectedLeft_PhoneDiallerControl = scaledLeftPadding + originalLeft_PhoneDiallerControl;
				AssertEquals(expectedLeft_PhoneDiallerControl, control.PhoneDiallerControl_Exposed.Left);

				var expectedLeft_LocalNumberLinkLabel = scaledLeftPadding + originalLeft_LocalNumberLinkLabel;
				AssertEquals(expectedLeft_LocalNumberLinkLabel, control.LocalNumberLinkLabel_Exposed.Left);

				var expectedLeft_PublishCheckEdit = scaledLeftPadding + originalLeft_PublishCheckEdit;
				AssertEquals(expectedLeft_PublishCheckEdit, control.PublishCheckEdit_Exposed.Left);
			}
		}

		public void TestOnLoad_NumberTextCaption()
		{
			using (var form = new ZForm())
			using (var control = new PhoneNumberUserControlForTest())
			{
				form.Controls.Add(control);
				var caption = new ResourceStringData(ZGuid.NewZGuid().ToString(), "Number");
				control.CaptionResourceString = caption;
				control.UseNumberTypeCaption = true;
				form.Show();
				AssertEquals("NumberTextBox uses control caption", caption.Key, control.NumberTextBox_Exposed.CaptionResourceString.Key);
			}
		}

		public void TestSetReadOnly()
		{
			using (var control = new PhoneNumberUserControlForTest())
			{
				AssertEquals("Precondition: NumberTextBox.ReadOnly", false, control.NumberTextBox_Exposed.ReadOnly);
				AssertEquals("Precondition: PublishCheckEdit.ReadOnly", false, control.PublishCheckEdit_Exposed.ReadOnly);

				control.SetReadOnly(true);
				AssertEquals("NumberTextBox.ReadOnly", true, control.NumberTextBox_Exposed.ReadOnly);
				AssertEquals("PublishCheckEdit.ReadOnly", true, control.PublishCheckEdit_Exposed.ReadOnly);
			}
		}

		public void TestNumberTextBox_Enter_TextChanged()
		{
			var dummy = new DummyBusinessObjectForPhoneValidation();
			var number = new PhoneNumber(dummy.PhoneNumberInfo, null, null, dummy.PhoneNumber_IsManuallyVerifiedInfo);
			using (dummy.SuspendValidationTesting())
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.PhoneNumberControl.NumberTextBox_Exposed.EnableValidStateColor = true;
				dummy.PhoneNumber = "XXXX";
				form.Show();
				form.PhoneNumberControl.NumberTextBox_Exposed.Focus();
				AssertNotNull(PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls));

				form.PhoneNumberControl.NumberTextBox_Exposed.Text = "XXXXY";
				AssertNull(PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls));

				DataRegistry.Instance.SetNumericValuesOnlyForPhoneNumberFields(true);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "AAAABBBCCC", false);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "12121", true);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "+121122 233223", true);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "+12121 - 232323", true);

				DataRegistry.Instance.SetNumericValuesOnlyForPhoneNumberFields(false);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "12121", true);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "+121122 233223", true);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "+12121 - 232323", true);
				AssertConfirmFormShouldAcceptPhoneNumberOrNot(form, "AAAABBBCCC", true);
			}
		}

		public void TestNumberTextBox_Enter_PositionsBottomEqualNumberTextBoxBottom()
		{
			var dummy = new DummyBusinessObjectForPhoneValidation();
			var number = new PhoneNumber(dummy.PhoneNumberInfo, null, null, dummy.PhoneNumber_IsManuallyVerifiedInfo);
			using (dummy.SuspendValidationTesting())
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.Height = ControlDpiScalingHelper.ScaleToCurrentDpiY(211);  // Sets form height just one pixel less than PhoneNumberValidationOverrideControl
				form.PhoneNumberControl.NumberTextBox_Exposed.EnableValidStateColor = true;
				dummy.PhoneNumber = "XXXX";
				form.Show();
				form.PhoneNumberControl.NumberTextBox_Exposed.Focus();

				var overrideControl = PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls);

				AssertNotNull(overrideControl);
				AssertEquals("Bottom of Override Control should match bottom of NumberTextBox", overrideControl.Bottom, form.PhoneNumberControl.NumberTextBox_Exposed.Bottom);
			}
		}

		void AssertConfirmFormShouldAcceptPhoneNumberOrNot(PhoneNumberUserControlFormForTest form, string phoneNumber, bool expectToBeAccepted)
		{
			form.PhoneNumberControl.NumberTextBox_Exposed.Text = phoneNumber;
			form.PhoneNumberControl.PhoneDiallerControl_Exposed.Focus();
			form.PhoneNumberControl.NumberTextBox_Exposed.Focus();

			var confirmControl = PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls);
			AssertNotNull(confirmControl);

			if (expectToBeAccepted)
			{
				AssertNull(confirmControl.NotAcceptedReason);
			}
			else
			{
				AssertEquals(confirmControl.NotAcceptedReason, "This number contains non-numeric characters which are not accepted. System Administrators have disallowed non-numeric values to be manually verified.");
			}

			var manualVerifyButtons = confirmControl.Controls.Find("ManualVerifyButton", true);
			AssertEquals(manualVerifyButtons[0].Visible, expectToBeAccepted);
		}

		public void TestNumberTextBox_Enter_Leave()
		{
			var dummy = new DummyBusinessObjectForPhoneValidation();
			var number = new PhoneNumber(dummy.PhoneNumberInfo, null, null, dummy.PhoneNumber_IsManuallyVerifiedInfo);
			using (dummy.SuspendValidationTesting())
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.PhoneNumberControl.NumberTextBox_Exposed.EnableValidStateColor = true;
				dummy.PhoneNumber = "XXXX";
				form.Show();
				form.PhoneNumberControl.NumberTextBox_Exposed.Focus();
				AssertNotNull(PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls));

				form.PhoneNumberControl.LocalNumberLinkLabel_Exposed.Focus();
				AssertNull(PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls));
			}
		}

		public void TestNumberTextBox_Enter_DoesNotDisplayValidationOverrideControlForValidPhoneNumber()
		{
			var dummy = new DummyBusinessObjectForPhoneValidation();
			var number = new PhoneNumber(dummy.PhoneNumberInfo, null, null, dummy.PhoneNumber_IsManuallyVerifiedInfo);
			using (dummy.SuspendValidationTesting())
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.PhoneNumberControl.NumberTextBox_Exposed.EnableValidStateColor = true;
				dummy.PhoneNumber = "+61 2 0202 0202";
				form.Show();
				form.PhoneNumberControl.NumberTextBox_Exposed.Focus();
				AssertNull(PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls));
			}
		}

		public void TestNumberTextBox_Validated_CheckRelatedMobiles()
		{
			var initialMobile = "000000000";
			var person = Factory.NewWithValidTestData<GlbPerson>();
			person.PER_MobilePhone = initialMobile;

			var staff = person.StaffCollection.AddNew();
			staff.GS_PER = person.PK;
			staff.GS_MobilePhone = initialMobile;

			var contact = person.ContactCollection.AddNew();
			contact.OC_PER = person.PK;
			contact.OC_OH = Factory.NewWithValidTestData<OrgHeader>().PK;
			contact.OC_Mobile = initialMobile;

			var applicant = (HRJobApplicant)person.ApplicantCollection.AddNew();
			applicant.HA_PER = person.PK;
			applicant.HA_MobilePhone = initialMobile;

			Factory.Save();

			using (var control = new PhoneNumberUserControlForTest())
			{
				person.ShouldUpdateMobileOnRelatedRecords = false;
				var newMobile = "111111111";

				person.PER_MobilePhone = newMobile;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.CheckRelatedMobiles(person.PER_MobilePhoneInfo, person);
				AssertEquals(person.ShouldUpdateMobileOnRelatedRecords, true);
				AssertEquals(control.NumberTextBox.Modified, false);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.CheckRelatedMobiles(person.PER_MobilePhoneInfo, person);
				AssertEquals(person.ShouldUpdateMobileOnRelatedRecords, false);
				person.PER_MobilePhone = initialMobile;

				contact.OC_Mobile = newMobile;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.CheckRelatedMobiles(contact.OC_MobileInfo, person);
				AssertEquals(person.ShouldUpdateMobileOnRelatedRecords, true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.CheckRelatedMobiles(contact.OC_MobileInfo, person);
				AssertEquals(person.ShouldUpdateMobileOnRelatedRecords, false);

				applicant.HA_MobilePhone = newMobile;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				control.CheckRelatedMobiles(person.PER_MobilePhoneInfo, person);
				AssertEquals(person.ShouldUpdateMobileOnRelatedRecords, true);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				control.CheckRelatedMobiles(person.PER_MobilePhoneInfo, person);
				AssertEquals(person.ShouldUpdateMobileOnRelatedRecords, false);
			}
		}

		public void TestNumberTextBox_ValidationOverride_DoesNotAccessDisposedObject()
		{
			var dummy = new DummyBusinessObjectForPhoneValidation();
			var lowerCaseValueInBizLayer = "xxxx";
			dummy.PhoneNumber = lowerCaseValueInBizLayer;
			var number = new PhoneNumber(dummy.PhoneNumberInfo, null, null, dummy.PhoneNumber_IsManuallyVerifiedInfo);
			using (var form = new PhoneNumberUserControlFormForTest(number))
			{
				form.Show();
				form.PhoneNumberControl.NumberTextBox_Exposed.Focus();
				AssertNotEquals("Precondition: business value is lower case and UI value is upper case", lowerCaseValueInBizLayer, form.PhoneNumberControl.NumberTextBox_Exposed.Text);
				var confirmationControl = PhoneNumberValidationOverrideControlHelper.FindConfirmationForm(form.Controls);
				AssertNotNull(confirmationControl);

				MouseSender.SendMessage(confirmationControl.ManualVerifyButton, confirmationControl.ManualVerifyButton.Handle, WindowsMessage.WM_LBUTTONDOWN, 0, 0);
			}
		}

		#endregion

		#region Properties

		public void TestDontSetHasChangesOnOpen()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithPhoneNumber>();
			dummy.Z0_NVarChar = "0412345678";
			Factory.Save();

			using (var form = new ZChildForm())
			using (var phoneControl = new PhoneNumberUserControl())
			{
				phoneControl.SetBindingMember("PhoneNumber");

				phoneControl.Dock = DockStyle.Fill;
				form.Controls.Add(phoneControl);

				form.SetDataBinding(dummy, "");
				form.Show();

				Application.DoEvents();
				Application.DoEvents();

				Assert("There should be no changes here", !dummy.HasChanges);
			}
		}

		public void TestDontSetHasChangesOnOpenNullPhone()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithPhoneNumber>();
			dummy.Z0_NVarChar = null;
			Factory.Save();

			using (var form = new ZChildForm())
			using (var phoneControl = new PhoneNumberUserControl())
			{
				phoneControl.SetBindingMember("PhoneNumber");

				phoneControl.Dock = DockStyle.Fill;
				form.Controls.Add(phoneControl);

				form.SetDataBinding(dummy, "");
				form.Show();

				Application.DoEvents();
				Application.DoEvents();

				Assert("There should be no changes here", !dummy.HasChanges);
			}
		}

		public void TestDontSetHasChangesOnOpenEmptyPhone()
		{
			var dummy = Factory.NewWithValidTestData<DummyWithPhoneNumber>();
			dummy.Z0_NVarChar = "";
			Factory.Save();

			using (var form = new ZChildForm())
			using (var phoneControl = new PhoneNumberUserControl())
			{
				phoneControl.SetBindingMember("PhoneNumber");

				phoneControl.Dock = DockStyle.Fill;
				form.Controls.Add(phoneControl);

				form.SetDataBinding(dummy, "");
				form.Show();

				Application.DoEvents();
				Application.DoEvents();

				Assert("There should be no changes here", !dummy.HasChanges);
			}
		}

		class DummyWithPhoneNumber : DummyBusinessObject
		{
			readonly PhoneNumberPropertyHelper helper = new PhoneNumberPropertyHelper(() => "AU");

			public DummyWithPhoneNumber(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
				PhoneNumber = new PhoneNumber(Z0_PhoneFormattedInfo, null, null, null);
			}

			public ZString Z0_PhoneFormatted
			{
				get => helper.GetPhoneNumber(Z0_NVarCharInfo);
				set => helper.SetPhoneNumber(Z0_NVarCharInfo, Z0_PhoneFormattedInfo, value, () => { }, null);
			}

			ZPropertyInfo Z0_PhoneFormattedInfo => GetWrappedZPropertyInfo(nameof(Z0_PhoneFormatted), x => Z0_NVarCharInfo);

			[ChildEditable]
			public PhoneNumber PhoneNumber { get; }
		}

		public void TestUseNumberTypeCaption()
		{
			using (var control = new PhoneNumberUserControlForTest())
			{
				AssertEquals("Default", true, control.UseNumberTypeCaption);
				AssertEquals("NumberTextBox.Label.Visible", true, control.NumberTextBox_Exposed.GetExtension<ILabelCaptionRenderer>().Visible);
				control.UseNumberTypeCaption = false;
				AssertEquals("NumberTextBox.Label.Visible", false, control.NumberTextBox_Exposed.GetExtension<ILabelCaptionRenderer>().Visible);
			}
		}

		public void TestShowDiallerControl()
		{
			using (var control = new PhoneNumberUserControlForTest())
			{
				AssertEquals("Default", true, control.ShowDiallerControl);
				AssertEquals("PhoneDialler.Visible", true, control.PhoneDiallerControl_Exposed.Visible);
				control.ShowDiallerControl = false;
				AssertEquals("PhoneDialler.Visible", false, control.PhoneDiallerControl_Exposed.Visible);
			}
		}

		public void TestShowPublishedCheckBox()
		{
			using (var control = new PhoneNumberUserControlForTest())
			{
				AssertEquals("Ensure caption is visible by disabling autosize", false, control.LocalNumberLinkLabel_Exposed.AutoSize);
				Assert("Ensure width is wide enough for translations of Publish (e.g. Veröffentlichen in GRM)", ControlDpiScalingHelper.ScaleToCurrentDpiX(100) <= control.Width);

				AssertEquals("Default", true, control.ShowPublishedCheckBox);
				AssertEquals("PublishCheckEdit.Visible", true, control.PublishCheckEdit_Exposed.Visible);
				control.ShowPublishedCheckBox = false;
				AssertEquals("PublishCheckEdit.Visible", false, control.PublishCheckEdit_Exposed.Visible);
			}
		}

		public void TestShowLocalNumberDisplay()
		{
			using (var control = new PhoneNumberUserControlForTest())
			{
				AssertEquals("Default", true, control.ShowLocalNumberLabel);
				AssertEquals("LocalNumberLinkLabel.Visible", true, control.LocalNumberLinkLabel_Exposed.Visible);
				control.ShowLocalNumberLabel = false;
				AssertEquals("LocalNumberLinkLabel.Visible", false, control.LocalNumberLinkLabel_Exposed.Visible);
			}
		}

		public void TestUnscaledLeftPadding()
		{
			using (var control = new PhoneNumberUserControlForTest())
			{
				AssertEquals("Default", 0, control.UnscaledLeftPadding);
				control.UnscaledLeftPadding = 30;
				AssertEquals(30, control.UnscaledLeftPadding);
				control.UnscaledLeftPadding = -20;
				AssertEquals(-20, control.UnscaledLeftPadding);
			}
		}

		#endregion

		#region Implementation

		PhoneNumber GetNewPhoneNumber()
		{
			var staff = Factory.New<GlbStaff>();
			return new PhoneNumber(staff.GS_MobilePhoneInfo, null, null);
		}

		protected override Form GetFormToBashCore()
		{
			var number = GetNewPhoneNumber();
			return new PhoneNumberUserControlFormForTest(number);
		}

		void InvokeLinkLabelClick(LinkLabel linkLabel)
		{
			linkLabel.GetType().InvokeMember("OnLinkClicked", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod, null, linkLabel, new object[] { new LinkLabelLinkClickedEventArgs(null) });
		}

		void SetupPhoneDialingUriProtocols(bool skypeIsDefault)
		{
			var dialingProtocols = new CodeDescriptionWithEnabledAndDefaultCollection(256);
			var skypeProtocol = dialingProtocols.AddNew();
			skypeProtocol.Code = "callto";
			skypeProtocol.Description = (NoResString)"Skype";
			skypeProtocol.IsDefault = skypeIsDefault;
			skypeProtocol.IsEnabled = true;

			var lyncProtocol = dialingProtocols.AddNew();
			lyncProtocol.Code = "tel";
			lyncProtocol.Description = (NoResString)"Lync";
			lyncProtocol.IsDefault = !skypeIsDefault;
			lyncProtocol.IsEnabled = true;
			SystemDataRegistry.Instance.PhoneDialingUriProtocols.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, dialingProtocols);
		}

		#endregion

		#region Classes

		public class DummyBusinessObjectForPhoneValidation : NonPersistentBusinessObject, IObsoleteValidation
		{
			PhoneNumberFormatterAndValidator PhoneNumberFormatterAndValidator
			{
				get { return phoneNumberFormatterAndValidator ?? (phoneNumberFormatterAndValidator = new PhoneNumberFormatterAndValidator()); }
			}
			PhoneNumberFormatterAndValidator phoneNumberFormatterAndValidator;

			public ZString PhoneNumber
			{
				private get { return phoneNumber; }
				set
				{
					phoneNumber = value;
					ValidatePhoneNumber();
				}
			}
			ZString phoneNumber;

			public ZPropertyInfo PhoneNumberInfo
			{
				get { return GetZPropertyInfo(nameof(PhoneNumber)); }
			}

			public ZBool PhoneNumber_IsManuallyVerified { get; set; }

			public ZPropertyInfo PhoneNumber_IsManuallyVerifiedInfo
			{
				get { return GetZPropertyInfo(nameof(PhoneNumber_IsManuallyVerified)); }
			}

			void ValidatePhoneNumber()
			{
				PhoneNumberInfo.ClearAllNotifications();
				PhoneNumberFormatterAndValidator.Validate(PhoneNumberInfo, null, PhoneNumber_IsManuallyVerifiedInfo, ZString.Empty);
			}
		}

		public class PhoneNumberUserControlFormForTest : ZChildForm
		{
			public PhoneNumberUserControlFormForTest(PhoneNumber phoneNumber, bool isEnabled = true)
				: base(phoneNumber)
			{
				PhoneNumberControl.Enabled = isEnabled;
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = ControlDpiScalingHelper.NewScaledSize(1024, 768);
				PhoneNumberControl = new PhoneNumberUserControlForTest();
				Controls.Add(PhoneNumberControl);
				CaptionRenderingEnabled = true;
				PhoneNumberControl.UseNumberTypeCaption = false;
			}

			public PhoneNumberUserControlForTest PhoneNumberControl;
		}

		public class PhoneNumberUserControlForTest : PhoneNumberUserControl
		{
			internal TestPhoneDialler PhoneDiallerForTest = new TestPhoneDialler();

			protected override PhoneDialler GetNewPhoneDialler()
			{
				return PhoneDiallerForTest;
			}

			internal PhoneNumberTextBox NumberTextBox_Exposed
			{
				get { return (PhoneNumberTextBox)Controls.Find("NumberTextBox", false)[0]; }
			}

			internal PhoneDiallerUserControl PhoneDiallerControl_Exposed
			{
				get { return PhoneDiallerControl; }
			}

			internal ZCheckBox PublishCheckEdit_Exposed
			{
				get { return PublishCheckEdit; }
			}

			internal ZLinkLabel LocalNumberLinkLabel_Exposed
			{
				get { return LocalNumberLinkLabel; }
			}
		}

		#endregion
	}
}
