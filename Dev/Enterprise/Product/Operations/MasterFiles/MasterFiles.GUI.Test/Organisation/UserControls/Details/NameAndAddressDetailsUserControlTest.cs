using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Tools.DuplicateDetector;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.Environment;
using Enterprise.MasterData.Business;
using Enterprise.MasterData.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class NameAndAddressDetailsUserControlTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestAllBranchesLinkCaptionNotEmpty()
		{
			using (var ctrl1 = new NameAndAddressControlForTesting())
			{
				AssertEquals("AllBranchesLink's caption is not empty, it's \"View all\"!!!", "View all", ctrl1.AllBranchesLinkExp.CaptionResourceString.Caption);
			}
		}

		public void TestViewBranchesForAllCompaniesIsSecure()
		{
			bool previousValue = Env.Security.OrganisationViewBranchesForAllCompanies.IsAllowed;

			try
			{
				Env.Security.OrganisationViewBranchesForAllCompanies.IsAllowed = false;

				OrgHeader testOrg = Factory.New<OrgHeader>();
				using (ZForm testForm = new ZChildForm(testOrg))
				{
					using (NameAndAddressControlForTesting ctrl = new NameAndAddressControlForTesting())
					{
						testForm.Controls.Add(ctrl);
						testForm.Show();

						AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
						ctrl.PerformAllBranchesLinkClick();
						AssertEquals(Env.Security.OrganisationViewBranchesForAllCompanies.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					}
				}

				Env.Security.OrganisationViewBranchesForAllCompanies.IsAllowed = true;

				using (ZForm testForm = new ZChildForm(testOrg))
				{
					using (NameAndAddressControlForTesting ctrl = new NameAndAddressControlForTesting())
					{
						testForm.Controls.Add(ctrl);
						testForm.Show();

						AssertNull(ZFormModaliser.LastFormShownForTest);
						ctrl.PerformAllBranchesLinkClick();
						AssertEquals(typeof(ControllingBranchesForm), ZFormModaliser.LastFormShownForTest.GetType());
					}
				}
			}
			finally
			{
				Env.Security.OrganisationViewBranchesForAllCompanies.IsAllowed = previousValue;
			}
		}

		public void TestAllBranchesLinkVisibility()
		{
			OrgHeader testOrg = Factory.New<OrgHeader>();
			AssertEquals(0, testOrg.CompanyDataCollection.Count);

			using (ZForm testForm = new ZChildForm(testOrg))
			{
				using (NameAndAddressControlForTesting ctrl = new NameAndAddressControlForTesting())
				{
					testForm.Controls.Add(ctrl);
					testForm.Show();
					Assert(!ctrl.AllBranchesLinkExp.Visible);
				}
			}

			testOrg = Factory.New<OrgHeader>();
			GlbCompanyCollection companies = new GlbCompanyCollection(Factory);

			foreach (GlbCompany company in companies)
			{
				GlbBranchCollection branches = new GlbBranchCollection(Factory, new ZQuery(GlbBranchSchema.GB_GC, company.PK));
				branches.Load();
				if (branches.Count > 0)
				{
					testOrg.GetCompanyDataForGlbCompany(company).OB_GB_ControllingBranch = branches[0].PK;
				}
			}

			Assert(testOrg.CompanyDataCollection.Count > 1);

			using (ZForm testForm = new ZChildForm(testOrg))
			{
				using (NameAndAddressControlForTesting ctrl = new NameAndAddressControlForTesting())
				{
					testForm.Controls.Add(ctrl);
					testForm.Show();
					Assert(ctrl.AllBranchesLinkExp.Visible);
				}
			}
		}

		public void TestNavigateToWeb()
		{
			OrgHeader testHeader = Factory.New<OrgHeader>();
			using (ZForm testForm = new ZChildForm(testHeader))
			{
				using (NameAndAddressControlForTesting ctrl = new NameAndAddressControlForTesting())
				{
					testForm.Controls.Add(ctrl);
					testForm.Show();

					testHeader.MainWebURL.PU_URL = "sdfjydskf";
					ctrl.NavigateToWeb_Exposed.PerformClick();
					Assert("Web Was Not Navigated - The URL is invalid", !ctrl.WebWasNavigated);

					testHeader.MainWebURL.PU_URL = "www.abc.com";
					ctrl.NavigateToWeb_Exposed.PerformClick();
					Assert(ctrl.WebWasNavigated);
				}
			}
		}

		[RequiresSTA]
		public void TestAllowedCharacterCasing()
		{
			Env.Registry.SetOrgAllowMixedCase(false);
			using (NameAndAddressControlForTesting ctrl = new NameAndAddressControlForTesting())
			{
				AssertEquals("OH_FullNameBoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("OH_FullNameBoundTextBox"));
				AssertEquals("Address1BoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("Address1BoundTextBox"));
				AssertEquals("Address2BoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("Address2BoundTextBox"));
				AssertEquals("CityBoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("CityBoundTextBox"));
				AssertEquals("EmailBoundTextBox.CharacterCasing (always normal)", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("EmailBoundTextBox"));
				AssertEquals("OH_WebBoundTextBox.CharacterCasing (always normal)", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OH_WebBoundTextBox"));
			}

			Env.Registry.SetOrgAllowMixedCase(true);
			using (NameAndAddressControlForTesting ctrl = new NameAndAddressControlForTesting())
			{
				AssertEquals("OH_FullNameBoundTextBox.CharacterCasing", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OH_FullNameBoundTextBox"));
				AssertEquals("Address1BoundTextBox.CharacterCasing", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("Address1BoundTextBox"));
				AssertEquals("Address2BoundTextBox.CharacterCasing", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("Address2BoundTextBox"));
				AssertEquals("CityBoundTextBox.CharacterCasing", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("CityBoundTextBox"));
				AssertEquals("EmailBoundTextBox.CharacterCasing (always normal)", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("EmailBoundTextBox"));
				AssertEquals("OH_WebBoundTextBox.CharacterCasing (always normal)", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OH_WebBoundTextBox"));
			}
		}

		[RequiresSTA]
		public void TestReadonlyWhenControlIsReadOnly()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var readOnlyToggleControl = testControl as IReadOnlyToggleControl;
				AssertNotNull("NameAndAddressControlForTesting should implement IReadOnlyToggleControl", readOnlyToggleControl);
				AssertEquals("Precondition: User control is not readonly", false, testControl.ReadOnly);

				readOnlyToggleControl.ReadOnly = true;
				AssertEquals("User control is readonly", true, testControl.ReadOnly);
				AssertEquals("ScreenButton.ReadOnly", true, testControl.ScreenButton_Exposed.ReadOnly);
				AssertEquals("ValidateAddressButton.ReadOnly", true, testControl.ValidateAddressButton_Exposed.ReadOnly);
				AssertEquals("ClearFieldsButton.ReadOnly", true, testControl.ClearAddressFieldsButton.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestCloseAddressSuggestionControlWhenTextChange()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var addressItems = new List<ValidationResultItem>();
				var topRecommendedAddress = AddressSuggestionControlTest.CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
				addressItems.Add(topRecommendedAddress);

				using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), addressItems, topRecommendedAddress, form, testControl, testControl.ValidateButton))
				{
					testControl.SuggestionWindowParentControl.Controls.Add(suggestionControl);
					suggestionControl.Show();
					var stateBoundDropEdit = ((ISupportWebAddressValidationControl)testControl).StateControl;
					stateBoundDropEdit.Text = "QLD";
					Application.DoEvents();
					Assert(suggestionControl.IsDisposed);
				}
			}
		}

		[ExpectNoExceptions]
		public void TestIsOnCurrentSelectedTab()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				testControl.IsOnCurrentSelectedTab();
			}
		}

		public void TestRefreshValidationStatus()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
				testHeader.MainAddress.OA_RN_NKCountryCode = "AU";
				testHeader.MainAddress.OA_State = "NSW";
				testHeader.MainAddress.OA_ValidationStatus = AddressValidationStatus.Verified;
				testControl.RefreshValidationStatus();

				var validGreenColor = Color.FromArgb(198, 236, 198);

				AssertEquals(validGreenColor, testControl.Address1TextBox.BackColor);
				AssertEquals(validGreenColor, testControl.Address2TextBox.BackColor);
				AssertEquals(validGreenColor, testControl.CityTextBox.BackColor);
				AssertEquals(validGreenColor, testControl.PostCodeTextBox.BackColor);
				AssertEquals(validGreenColor, testControl.StateDropEdit.CodeBox.BackColor);
				AssertEquals(validGreenColor, testControl.CountryControl.CodeBox.BackColor);
				AssertEquals(true, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(true, testControl.ClearAddressFieldsButton.Visible);

				var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(us.PK, disabledForOrgAddress: true));
				testHeader.MainAddress.OA_RN_NKCountryCode = "US";
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.Address1TextBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.Address2TextBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CityTextBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.PostCodeTextBox.BackColor);
				AssertEquals(Color.FromArgb(255, 215, 215), testControl.StateDropEdit.CodeBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CountryControl.CodeBox.BackColor);
				AssertEquals(false, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(false, testControl.ClearAddressFieldsButton.Visible);

				Env.Instance.Registry.EnableAddressValidationWebService = false;
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.Address1TextBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.Address2TextBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CityTextBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.PostCodeTextBox.BackColor);
				AssertEquals(Color.FromArgb(255, 215, 215), testControl.StateDropEdit.CodeBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CountryControl.CodeBox.BackColor);
				AssertEquals(false, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(false, testControl.ClearAddressFieldsButton.Visible);
			}
		}

		protected override void TearDown()
		{
			Balloon.Instance.Hide();
			base.TearDown();
		}

		public void TestAddressChanges_SetGuiChanged()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.MainAddress.OA_RN_NKCountryCode = "AU";
			testHeader.MainAddress.OA_State = "NSW";
			testHeader.MainAddress.Postcode = "2015";
			testHeader.MainAddress.Address1 = "Unit 73 O'Riden Street";
			testHeader.MainAddress.Address2 = "Above and under the food";
			testHeader.MainAddress.City = "Alexandria";

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testHeader.MainAddress.OA_RN_NKCountryCode = "US";
				AssertEquals("Changes to country code sets HasGUIChanges", true, testHeader.MainAddress.HasBeenChangedByUser);
				testHeader.MainAddress.HasBeenChangedByUser = false;

				testHeader.MainAddress.OA_State = "CA";
				AssertEquals("Changes to state value sets HasGUIChanges", true, testHeader.MainAddress.HasBeenChangedByUser);
				testHeader.MainAddress.HasBeenChangedByUser = false;

				testHeader.MainAddress.Postcode = "777";
				AssertEquals("Changes to postcode sets HasGUIChanges", true, testHeader.MainAddress.HasBeenChangedByUser);
				testHeader.MainAddress.HasBeenChangedByUser = false;

				testHeader.MainAddress.Address1 = "12 Flying Drive";
				AssertEquals("Changes to Address Line 1 sets HasGUIChanges", true, testHeader.MainAddress.HasBeenChangedByUser);
				testHeader.MainAddress.HasBeenChangedByUser = false;

				testHeader.MainAddress.Address2 = "Coding Stuff";
				AssertEquals("Changes to Address Line 2 sets HasGUIChanges", true, testHeader.MainAddress.HasBeenChangedByUser);
				testHeader.MainAddress.HasBeenChangedByUser = false;

				testHeader.MainAddress.City = "Silicon Valley";
				AssertEquals("Changes to City value sets HasGUIChanges", true, testHeader.MainAddress.HasBeenChangedByUser);
			}
		}

		[RequiresSTA]
		public void TestAddAndRemoveFromMainForm()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			{
				using (var docAddress = new NameAndAddressDetailsUserControl())
				{
					form.Controls.Add(docAddress);

					form.Show();

					Application.DoEvents();

					form.Controls.Remove(docAddress);
				}

				AssertNoExceptionThrown("It should not throw any exception since it's being disposed correctly", form.Close);
			}
		}

		[RequiresSTA]
		public void TestValidationAddressButton_IfDataIsNotInDatabase_AndNoSecurityGranted()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			bool originalNameAndAddressSecurity = Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;

				using (var form = new ZOrganisationsForm(testHeader))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					testControl.PerformValidateAddressClick();
					Assert("No messages are shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

					Factory.Save();
					testControl.PerformValidateAddressClick();
					Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
			finally
			{
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = originalNameAndAddressSecurity;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[ExpectNoExceptions]
		public void TestValidationAddressButton_ValidationCompletesWithoutExceptionAfterFormIsClosed()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			bool originalNameAndAddressSecurity = Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed;
			bool originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;

				var address = Factory.New<OrgAddressForTest>(); //Use this to replace original main address for faking real HTTP request
				address.OA_Address1 = "InvalidAddress";
				address.OA_RN_NKCountryCode = "AU";
				address.OA_State = "NSW";
				address.OA_PostCode = "2082";
				address.OA_OH = testHeader.PK;
				testHeader.Addresses.Add(address);

				address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
				address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

				using (var form = new ZOrganisationsForm(testHeader))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					testControl.SimulateIsOnSelectedTab = true;
					testControl.PerformValidateAddressClick();
				}

				Application.DoEvents(); //Allow the background address validation to complete after the form is closed
			}
			finally
			{
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = originalNameAndAddressSecurity;
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestNoCreatedChangesNotificationExceptionThrown_WhenOnLoad()
		{
			using (DataRegistry.Instance.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var originalNameAndAddressSecurity = Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed;
				var originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;
				try
				{
					var testHeader = Factory.NewWithValidTestData<OrgHeader>();
					var address = Factory.New<OrgAddressForTest>(); //Use this to replace original main address for faking real HTTP request
					address.OA_Address1 = "InvalidAddress";
					address.OA_RN_NKCountryCode = "AU";
					address.OA_State = "NSW";
					address.OA_PostCode = "2082";
					address.ValidationStatus = AddressValidationStatus.ToBeVerified;
					address.Address1 = "Test Address1";
					address.OA_OH = testHeader.PK;
					testHeader.Addresses.Add(address);

					address.AddressCapability.SetCapabilityEnabled(OrgAddressType.Office.Code);
					address.AddressCapability.SetIsMainAddress(OrgAddressType.Office.Code);

					Factory.Save();

					using (TestingState.SuspendIsRunningTests())
					using (var form = new ZOrganisationsForm(testHeader))
					{
						form.Show();
						Assert("Change should not happen on the OrgHeader when load", !testHeader.HasChanges);
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Validation status has been changed to INV.", AddressValidationStatus.Invalid, address.ValidationStatus);
					}
				}
				finally
				{
					Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = originalNameAndAddressSecurity;
					Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		[RequiresSTA]
		public void TestClearFieldsButton_IfDataIsNotInDatabase_AndNoSecurityGranted()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			bool originalNameAndAddressSecurity = Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed;

			try
			{
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;

				using (var form = new ZOrganisationsForm(testHeader))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					testControl.PerformClearFieldsClick();
					Assert("No messages are shown", UnitTestUserNotification.Instance.LastMessage.WasNone);

					Factory.Save();
					testControl.PerformClearFieldsClick();
					Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
				}
			}
			finally
			{
				Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = originalNameAndAddressSecurity;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[RequiresSTA]
		public void TestClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities()
		{
			AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(false, true, true, true, true, false, true);
			AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(true, false, true, true, true, true, false);
			AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(true, true, false, true, false, false, true);
			AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(true, true, true, false, false, true, false);
		}

		void AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(bool detailsARAPAllowed, bool detailsARAPNewAllowed, bool detailsNonARAPAllowed,
			bool detailsNonARAPNewAllowed, bool setARAPCapability, bool hasErrorMessageBeforeSave, bool hasErrorMessageAfterSave)
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.MainAddress.AddressCapability.DisableAllCapabilities();
			CombineAssertions("Precondition: Main Address always has Office capability.", () =>
			{
				AssertEquals(1, testHeader.MainAddress.AddressCapability.EnabledCapabilities.Count());
				AssertEquals(OrgConstants.AddressType.Office, testHeader.MainAddress.AddressCapability.EnabledCapabilities.First().Code);
			});

			if (setARAPCapability)
			{
				testHeader.MainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);
			}

			var originalOrgAddressDetailsARAP = Env.Security.OrgAddressDetailsARAP.IsAllowed;
			var originalOrgAddressDetailsARAPNew = Env.Security.OrgAddressDetailsARAPNew.IsAllowed;
			var originalOrgAddressDetailsNonARAP = Env.Security.OrgAddressDetailsNonARAP.IsAllowed;
			var originalOrgAddressDetailsNonARAPNew = Env.Security.OrgAddressDetailsNonARAPNew.IsAllowed;

			try
			{
				Env.Security.OrgAddressDetailsARAP.IsAllowed = detailsARAPAllowed;
				Env.Security.OrgAddressDetailsARAPNew.IsAllowed = detailsARAPNewAllowed;
				Env.Security.OrgAddressDetailsNonARAP.IsAllowed = detailsNonARAPAllowed;
				Env.Security.OrgAddressDetailsNonARAPNew.IsAllowed = detailsNonARAPNewAllowed;

				using (var form = new ZOrganisationsForm(testHeader))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testControl.PerformClearFieldsClick();
					if (hasErrorMessageBeforeSave)
					{
						Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					}
					else
					{
						Assert("No messages are shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testControl.PerformValidateAddressClick();
					if (hasErrorMessageBeforeSave)
					{
						Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					}
					else
					{
						Assert("No messages are shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					}

					Factory.Save();
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testControl.PerformClearFieldsClick();
					if (hasErrorMessageAfterSave)
					{
						Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					}
					else
					{
						Assert("No messages are shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testControl.PerformValidateAddressClick();
					if (hasErrorMessageAfterSave)
					{
						Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					}
					else
					{
						Assert("No messages are shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					}
				}
			}
			finally
			{
				Env.Security.OrgAddressDetailsARAP.IsAllowed = originalOrgAddressDetailsARAP;
				Env.Security.OrgAddressDetailsARAPNew.IsAllowed = originalOrgAddressDetailsARAPNew;
				Env.Security.OrgAddressDetailsNonARAP.IsAllowed = originalOrgAddressDetailsNonARAP;
				Env.Security.OrgAddressDetailsNonARAPNew.IsAllowed = originalOrgAddressDetailsNonARAPNew;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestOrgSecurityGroupFindBox()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "GG_DESC1";
			group.GG_Code = "GG_CODE1";
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MiscServ.OM_GG_OrgSecurityGroup = group.PK;

			using (var form = new ZOrganisationsForm(org))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();
				Application.DoEvents();

				var findbox = testControl.Controls.Find("OrgSecurityGroupFindBox", true)[0] as ZGuidFindBox;
				AssertEquals("Security Group", findbox.CaptionResourceString.Caption);
				AssertEquals("GG_CODE1", findbox.CodeBox.Text);

				for (var time = Stopwatch.StartNew(); time.Elapsed < TimeSpan.FromSeconds(30) && string.IsNullOrEmpty(findbox.DescriptionBox.Text);)
				{
					Thread.Yield();
					Application.DoEvents();
				}

				AssertEquals("GG_DESC1", findbox.DescriptionBox.Text);

				form.Close();
			}
		}

		public void TestTabIndicesInRightOrder()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals(testControl.StateDropEdit.TabIndex - testControl.CityTextBox.TabIndex, 1);
				AssertEquals(testControl.CityTextBox.TabIndex - testControl.PostCodeTextBox.TabIndex, 1);
			}
		}

		public void TestDeDupSpinnerNotShownAtStart()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals("Spinner should not be visible when form is created", false, testControl.DeduplicationStatusIconVisible);
			}
		}

		public void TestShowNoDuplicatesFoundDisplaysCorrectly()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var args = new DuplicationEventArgs(null, null, null, null, DuplicationStatus.OK, new DeduplicationExclusionManager<OrgHeader>());

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationStatus();
				Assert(testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Black));

				testControl.ShowNoDuplicatesFound(args);
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);

				var originalRegValueExcludingOtherCountries = OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.Value;
				var originalRegValueExcludingInactive = OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.Value;

				try
				{
					OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);
					OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);

					OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);
					OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);

					OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);
					OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
					testControl.ShowNoDuplicatesFound(args);
					AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);
				}
				finally
				{
					OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegValueExcludingOtherCountries);
					OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, originalRegValueExcludingInactive);
				}
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.DarkSeaGreen));
			}
		}

		[RequiresSTA]
		public void TestDeduplicationActionOccurred()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();
				var eventArgs = new DuplicationEventArgs(null);

				eventArgs.InvokedAction = DeduplicationAction.Merge;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.Ignore;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.Link;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.NotMatched;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.OpenMaster;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.OpenTarget;
				testControl.ShowDuplicatesFound(eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(!testControl.DeduplicationStatusVisible);

				eventArgs.InvokedAction = DeduplicationAction.None;
				testControl.ShowDuplicatesFound(eventArgs);
				testControl.DeduplicationActionOccurred(this, eventArgs);
				Assert(testControl.DeduplicationStatusVisible);
			}
		}

		public void TestShowDuplicationMessage()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = false;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = false;

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationStatus();
				Assert(testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Black));

				testControl.ShowDeduplicationTimeoutMessage();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("This process has stopped due to timeout"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.OrangeRed));

				testControl.ShowNotEnoughInformation();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Not enough information to detect duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.DarkSeaGreen));

				testControl.ShowExcludedDuplicationMessage();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Excluded from De-duplication"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.OrangeRed));
			}
		}

		public void TestShowDuplicationMessageWhenHaveExclusionInfo()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var exclusionManager = new DeduplicationExclusionManager<OrgHeader>()
				{
					DisplayInfo = "This Organization has too much addresses/contacts, The system excludes record with more than 100 addresses /100 contacts. Consider if this organisation needs to be split using management groups."
				};

				testControl.CurrentDuplicationEventArgs = ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", null, null, null, null, DuplicationStatus.OK, exclusionManager);
				testControl.ShowExcludedDuplicationMessage();
				CombineAssertions(() =>
				{
					AssertEquals(false, testControl.DeduplicationStatusIconVisible);
					Assert(testControl.DeduplicationStatusLabelReads("Excluded from De-duplication due to size limits"));
					Assert(testControl.DeduplicationStatusLabelColorIs(Color.OrangeRed));
					AssertEquals(exclusionManager.DisplayInfo, ToolTipService.GetToolTip(testControl.DuplicateDetectionStatusLabel));
				});
			}
		}

		public void TestShowDuplicationMessage_WithAdminAccess()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationStatus();
				Assert(testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Black));

				testControl.ShowDeduplicationTimeoutMessage();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("This process has stopped due to timeout"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Blue));

				testControl.ShowNotEnoughInformation();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Not enough information to detect duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.DarkSeaGreen));

				testControl.ShowExcludedDuplicationMessage();
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Excluded from De-duplication"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.OrangeRed));
			}
		}

		[RequiresSTA]
		public void TestTimeOutWarning_IsFormattedAsHyperlink()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;

			Factory.Save();

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationTimeoutMessage();
				CombineAssertions(() =>
				{
					AssertEquals("Dedupe icon hidden", false, testControl.DeduplicationStatusIconVisible);
					AssertEquals("Dedupe timeout message text", true, testControl.DeduplicationStatusLabelReads("This process has stopped due to timeout"));
					AssertEquals("Dedupe timeout message colour", true, testControl.DeduplicationStatusLabelColorIs(Color.Blue));
					AssertEquals("Tooltip set", true, testControl.HasTimeoutToolTip);
				});

				testControl.DuplicateDetectionLabelMouseEnter();

				CombineAssertions(() =>
				{
					AssertEquals("Link underlined", true, testControl.DeduplicationStatusLabelStyleIs(FontStyle.Underline));
					AssertEquals("Link cursor", true, testControl.DeduplicationStatusLabelCursorIs(Cursors.Hand));
				});

				testControl.DuplicateDetectionLabelMouseLeave();

				CombineAssertions(() =>
				{
					AssertEquals("Link not underlined", true, testControl.DeduplicationStatusLabelStyleIs(FontStyle.Regular));
					AssertEquals("normal cursor", true, testControl.DeduplicationStatusLabelCursorIs(Cursors.Default));
				});
			}
		}

		public void TestTimeOutWarning_WhenNotAdmin_IsNotFormattedAsHyperlink()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = false;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = false;

			Factory.Save();

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationTimeoutMessage();
				CombineAssertions(() =>
				{
					AssertEquals(false, testControl.DeduplicationStatusIconVisible);
					AssertEquals(true, testControl.DeduplicationStatusLabelReads("This process has stopped due to timeout"));
					AssertEquals(true, testControl.DeduplicationStatusLabelColorIs(Color.OrangeRed));
				});

				testControl.DuplicateDetectionLabelMouseEnter();

				CombineAssertions(() =>
				{
					AssertEquals(false, testControl.HasTimeoutToolTip);
					AssertEquals(true, testControl.DeduplicationStatusLabelStyleIs(FontStyle.Regular));
					AssertEquals(true, testControl.DeduplicationStatusLabelCursorIs(Cursors.Default));
				});
			}
		}

		[RequiresSTA]
		public void TestClickOutOfSizeLimitWarning_CorrectlyPopulatesFilter()
		{
			Env.Security.MdmAdministrationPanel.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.CurrentDuplicationEventArgs = ObjectFactory.Get<IDuplicationEventArgs>("IDuplicationEventArgs", null, null, null, null, DuplicationStatus.OK, new DeduplicationExclusionManager<OrgHeader>());
				testControl.ShowExcludedDuplicationMessage();
				testControl.DuplicateDetectionLabelMouseClick();
				Application.DoEvents();

				using (var adminPanel = Application.OpenForms.OfType<AdministrationPanelForm>().SingleOrDefault())
				{
					AssertNotNull(adminPanel);

					var dedupFilterControl = adminPanel.Controls.Find("DeduplicationOrganisationFilterControl", true)[0] as ZFilterStripControl;
					var filterBizo = dedupFilterControl.FilterBusinessObject;
					AssertEquals(1, filterBizo.ActiveModuleFilters.Count);

					var codeFilter = (ModuleTextFilter)filterBizo["Code"];
					AssertEquals(testHeader.OH_Code, codeFilter.Property);
				}

				AssertEquals(Color.Purple, (testControl.Controls.Find("DuplicateDetectionStatusLabel", true)[0] as ZLabel).ForeColor);
			}
		}

		public void TestClickTimeOutWarning_NavigatesToAdminPanel()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;

			Factory.Save();

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationTimeoutMessage();
				testControl.DuplicateDetectionLabelMouseClick();
				Application.DoEvents();

				using (var adminPanel = Application.OpenForms.OfType<AdministrationPanelForm>().SingleOrDefault())
				{
					AssertNotNull(adminPanel);

					var mainTabControl = adminPanel.Controls.Find("MainTabControl", true)[0] as ZTabControl;
					var duplicatesTabPage = mainTabControl.Controls.Find("DuplicatesTabPage", true)[0] as ZTabPage;
					AssertEquals(mainTabControl.SelectedTab, duplicatesTabPage);
				}
			}
		}

		[RequiresSTA]
		public void TestClickTimeOutWarning_CorrectlyPopulatesFilter()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;

			Factory.Save();

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationTimeoutMessage();
				testControl.DuplicateDetectionLabelMouseClick();
				Application.DoEvents();

				using (var adminPanel = Application.OpenForms.OfType<AdministrationPanelForm>().SingleOrDefault())
				{
					AssertNotNull(adminPanel);

					var dedupFilterControl = adminPanel.Controls.Find("DeduplicationOrganisationFilterControl", true)[0] as ZFilterStripControl;
					var filterBizo = dedupFilterControl.FilterBusinessObject;
					AssertEquals(1, filterBizo.ActiveModuleFilters.Count);

					var codeFilter = (ModuleTextFilter)filterBizo["Code"];
					AssertEquals(testHeader.OH_Code, codeFilter.Property);
				}
			}
		}

		public void TestClickTimeOutWarning_CorrectlyPopulatesFilter_WhenAdminPanelAlreadyOpen()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;

			Factory.Save();

			using (var adminForm = new AdministrationPanelForm(new AdministrationPanelManager(Factory)))
			{
				adminForm.Show();
				AssertNotNull("Precondition", adminForm);
				AssertEquals("Precondition", 1, Application.OpenForms.OfType<AdministrationPanelForm>().Count());

				using (var form = new ZOrganisationsForm(testHeader))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					testControl.ShowDeduplicationTimeoutMessage();
					testControl.DuplicateDetectionLabelMouseClick();
					Application.DoEvents();

					var openedAdminForm = Application.OpenForms.OfType<AdministrationPanelForm>().SingleOrDefault();
					AssertNotNull(openedAdminForm);

					var dedupFilterControl = openedAdminForm.Controls.Find("DeduplicationOrganisationFilterControl", true)[0] as ZFilterStripControl;
					var filterBizo = dedupFilterControl.FilterBusinessObject;
					AssertEquals(1, filterBizo.ActiveModuleFilters.Count);

					var codeFilter = (ModuleTextFilter)filterBizo["Code"];
					AssertEquals(testHeader.OH_Code, codeFilter.Property);
				}
			}
		}

		public void TestClickTimeOutWarning_CorrectlyPopulatesFilter_WhenAdminPanelAlreadyOpen_AndHasExistingFilters()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = true;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = true;

			Factory.Save();

			using (var adminForm = new AdministrationPanelForm(new AdministrationPanelManager(Factory)))
			{
				adminForm.Show();
				adminForm.ShowDuplicatesTab();
				Application.DoEvents();

				AssertNotNull("Precondition", adminForm);
				AssertEquals("Precondition", 1, Application.OpenForms.OfType<AdministrationPanelForm>().Count());

				var userControl = adminForm.Controls.Find("DeduplicationOrganizationsUserControl", true)[0] as IDeduplicationBusinessObjectUserControl;

				var filterBizo = userControl.DeduplicationBusinessObjectFilterBizo;

				var strip1 = filterBizo.FilterStrips.AddNew();
				strip1.FilterDescription = "Email";

				var strip2 = filterBizo.FilterStrips.AddNew();
				strip2.FilterDescription = "Web";

				var filterControl = userControl.DeduplicationBusinessObjectFilterControl;
				filterControl.AddFilterStrip(strip1);
				filterControl.AddFilterStrip(strip2);

				AssertEquals("Precondition", 2, filterBizo.ActiveModuleFilters.Count);

				using (var form = new ZOrganisationsForm(testHeader))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					testControl.ShowDeduplicationTimeoutMessage();
					testControl.DuplicateDetectionLabelMouseClick();
					Application.DoEvents();

					var openedAdminForm = Application.OpenForms.OfType<AdministrationPanelForm>().SingleOrDefault();
					AssertNotNull(openedAdminForm);

					AssertEquals("Should remove existing filters and then add the default", 1, filterBizo.ActiveModuleFilters.Count);

					var codeFilter = (ModuleTextFilter)filterBizo["Code"];
					AssertEquals(testHeader.OH_Code, codeFilter.Property);
				}
			}
		}

		[RequiresSTA]
		public void TestClickTimeOutWarning_WhenNotAdmin_DoesNothing()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			Env.Security.MdmAdministrationPanel.IsAllowed = false;
			Env.Security.MdmAdministrationPanelDuplicatesEdit.IsAllowed = false;

			Factory.Save();

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationTimeoutMessage();
				testControl.DuplicateDetectionLabelMouseClick();
				Application.DoEvents();

				using (var adminPanel = Application.OpenForms.OfType<AdministrationPanelForm>().SingleOrDefault())
				{
					AssertNull(adminPanel);
				}
			}
		}

		[RequiresSTA]
		public void TestRegistrationNumberShouldBeRefreshedWhenSelectedItemChanged()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var abnCode = orgHeader.CustomsCodes.AddNew();
			abnCode.OK_CodeType = "ABN";
			abnCode.OK_CustomsRegNo = "21 003 980 130";
			var gcnCode = orgHeader.CustomsCodes.AddNew();
			gcnCode.OK_CodeType = "GCR";
			gcnCode.OK_CustomsRegNo = "ABCD";

			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var numberTextBox = testControl.RegistrationNumberTextBox_Exposed;
				var dropEdit = testControl.RegistrationNumberTypeDropEdit_Exposed;
				AssertEquals("21 003 980 130", numberTextBox.Text);

				numberTextBox.Focus();
				dropEdit.SelectItem("GCR");
				dropEdit.OnItemSelected(dropEdit.LastSelectedItem, true);
				Application.DoEvents();
				dropEdit.Focus();
				AssertEquals("ABCD", numberTextBox.Text);
			}
		}

		public void TestRegistrationNumbersShouldBeCheckedForDuplicatesUponLaunch()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Turkey))
			{
				var otherOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
				otherOrgHeader.OH_Code = "TRORGNOTMINE";
				var vatCode = otherOrgHeader.CustomsCodes.AddNew();
				vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vatCode.OK_CustomsRegNo = "987654321";

				var myOrgHeader = Factory.NewWithValidTestData<OrgHeader>();
				myOrgHeader.OH_Code = "TRORGMYOWN";
				vatCode = myOrgHeader.CustomsCodes.AddNew();
				vatCode.OK_CodeType = OrgCusCode.CodeTypes.VATCode;
				vatCode.OK_CustomsRegNo = "987654321";

				Factory.Save();

				using (var form = new ZOrganisationsForm(myOrgHeader))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					Application.DoEvents();

					AssertEquals("Precondition: VAT number must be auto-selected as primary registration number.", vatCode.OK_CustomsRegNo, testControl.RegistrationNumberTextBox_Exposed.Text);
					AssertEquals("Precondition: VAT number must be auto-selected as primary registration number.", vatCode.OK_CodeType, testControl.RegistrationNumberTypeDropEdit_Exposed.CodeBox.Text);

					var numberInfo = myOrgHeader.PrimaryRegistrationNumber.NumberInfo;

					AssertEquals("There should be only one warning for registration number conflicts.", 1, numberInfo.Notifications.Count());
					AssertHasWarning("Conflicting registration number validation must be performed upon launch.", numberInfo, "This Registration Number is already in use by at least one organization. The organizations are: TRORGNOTMINE (Maximum of 10 organizations shown). Please check that the organizations are not the same.");
				}
			}
		}

		[RequiresSTA]
		public void TestRegistrationNumberShouldNotBeRefreshedWhenFirstLoaded()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals("The RegistrationNumberTypeDropEdit_SelectedIndexChanged should not be called if it is not focused", false, testControl.Focused);
			}
		}

		public void TestDeduplicationHyperlinkInvokesDeduplicationSearch()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var isSearchingForDuplicates = false;
			org.DeduplicationStarted += delegate
			{
				isSearchingForDuplicates = true;
			};
			using (var form = new ZOrganisationsForm(org))
			using (var control = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(control);
				form.Show();

				var testTarget = Factory.NewWithValidTestData<OrgHeader>();
				var testTargetList = new List<object> { testTarget };

				control.ShowDuplicatesFound(new DuplicationEventArgs(org, testTargetList, null, null));
				control.DuplicateDetectionLabelMouseClick();
				Assert(isSearchingForDuplicates);
			}
		}

		[RequiresSTA]
		public void TestShowDuplicatesFoundDisplaysCorrectly()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.ShowDeduplicationStatus();
				Assert(testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Detecting duplicates"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Black));

				var args = new DuplicationEventArgs(null, null, null, null);
				testControl.ShowDuplicatesFound(args);
				AssertEquals(false, testControl.DeduplicationStatusIconVisible);
				Assert(testControl.DeduplicationStatusLabelReads("Duplicates found"));
				Assert(testControl.DeduplicationStatusLabelColorIs(Color.Blue));
				AssertEquals(args, testControl.CurrentDuplicationEventArgs);
			}
		}

		public void TestShowDuplicatesFound_WithExclusions()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			var targetOrg = Factory.NewWithValidTestData<OrgHeader>();
			var manager = new DeduplicationExclusionManager<OrgHeader>();

			targetOrg.OH_Code = "VVREE";
			Factory.Save();

			manager.Builder.BuildExclusion(new ZQuery(OrgHeaderSchema.OH_Code, targetOrg.OH_Code), ResString.GetMultilingualString("d7b36162-e1af-40e1-a9e6-7c75a559df2b", "Excluded for weird reasons"));
			manager.BuildDisplay();

			using (var form = new ZOrganisationsForm(header))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var args = new DuplicationEventArgs(null, null, null, null, DuplicationStatus.OK, manager);
				testControl.ShowDuplicatesFound(args);
				Assert("Exclusion Label should be visible", testControl.DuplicateExclusionsLabel_Exposed.Visible);
				AssertEquals("Excluded records: (1)", testControl.DuplicateExclusionsLabel_Exposed.Text);
			}
		}

		[RequiresSTA]
		public void TestAlertFormClosedWhenDuplicationEliminated()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = org.PK.ToGuid(),
				MasterType = typeof(OrgHeader),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(OrgHeader)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new ZOrganisationsForm(org))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DeduplicationHelper.ShowDuplicateAlert(testControl, args);
				AssertNotNull("Alert window created", testControl.DeduplicationHelper.ExistingAlertControl);

				testControl.DuplicationEnded(null, new DuplicationEventArgs(null));
				AssertNull("Alert window removed", testControl.DeduplicationHelper.ExistingAlertControl);
			}
		}

		[RequiresSTA]
		public void TestDeDuplicationAlert()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = org.PK.ToGuid(),
				MasterType = typeof(OrgHeader),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(OrgHeader)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new ZOrganisationsForm(org))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DeduplicationHelper.ShowDuplicateAlert(testControl, args);
				AssertNotNull(testControl.DeduplicationHelper.ExistingAlertControl);
			}
		}

		[RequiresSTA]
		public void TestDeDuplicationAlertLocation()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var dummy = Factory.New<OrgHeader>();
			var scoringResult = new List<ScoringResult>();
			scoringResult.Add(new ScoringResult
			{
				MasterPK = org.PK.ToGuid(),
				MasterType = typeof(OrgHeader),
				Score = 1,
				TargetPK = Guid.NewGuid(),
				TargetType = typeof(OrgHeader)
			});

			var args = new DuplicationEventArgs(dummy, new object(), scoringResult, new List<PatternMatchingResultModel>());
			using (var form = new ZOrganisationsForm(org))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DeduplicationHelper.ShowDuplicateAlert(testControl, args);

				var marginX = ControlDpiScalingHelper.ScaleToCurrentDpiX(1);
				var alertControl = testControl.DeduplicationHelper.ExistingAlertControl;
				AssertEquals(testControl.OH_FullNameBoundTextBox.Location.X + testControl.OH_FullNameBoundTextBox.Width + marginX, alertControl.Location.X);
				AssertEquals(testControl.OH_FullNameBoundTextBox.Location.Y, alertControl.Location.Y);
			}
		}

		public void TestDuplicationEndedWhenDuplicatesFound()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>() { new ScoringResult() }, null, ZGuid.Empty));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("Duplicates found", testControl.DeduplicationStatusText);
			}
		}

		public void TestDuplicationEndedWhenExcludedFromDuplication()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("Excluded from De-duplication", testControl.DeduplicationStatusText);
			}
		}

		public void TestDuplicationEndedWithNoScoringResultsAndMinimumRequirementsNOTMet()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>(), null, ZGuid.Empty));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("Not enough information to detect duplicates", testControl.DeduplicationStatusText);
			}
		}

		[RequiresSTA]
		public void TestDeduplicationEndedWithNoScoringResultsAndMinimumRequirementsMet()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (OrganisationsDataRegistry.Instance.ExcludePotentialDuplicatesFromOtherCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (OrganisationsDataRegistry.Instance.ExcludeInactivePotentialDuplicates.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, new List<ScoringResult>(), new List<PatternMatchingResultModel>(), ZGuid.Empty));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("No duplicates found.", testControl.DeduplicationStatusText);
			}
		}

		[RequiresSTA]
		public void TestDeduplicationEndedWithTimeout()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null, DuplicationStatus.Timeout, new DeduplicationExclusionManager<OrgHeader>()));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("This process has stopped due to timeout", testControl.DeduplicationStatusText);
			}
		}

		[RequiresSTA]
		public void TestDeduplicationEndedWithErrorOccurred()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.DuplicationEnded(this, new DuplicationEventArgs(null, null, null, null, DuplicationStatus.ErrorOccurred, new DeduplicationExclusionManager<OrgHeader>()));
				Assert(testControl.DeduplicationStatusVisible);
				AssertEquals("An error occurred while detecting duplicates", testControl.DeduplicationStatusText);
			}
		}

		[RequiresSTA]
		public void TestAddressesUserControl_Resize()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var addressItems = new List<ValidationResultItem>();
				var topRecommendedAddress = AddressSuggestionControlTest.CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
				addressItems.Add(topRecommendedAddress);

				using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), addressItems, topRecommendedAddress, form, testControl, testControl.ValidateButton))
				{
					testControl.SuggestionWindowParentControl.Controls.Add(suggestionControl);
					testControl.Show();

					suggestionControl.Hide();
					testControl.FireNameAndAddressDetailsUserControl_Resize();
					AssertEquals(false, testControl.resizeAddressSuggestionControl);

					suggestionControl.Show();
					testControl.FireNameAndAddressDetailsUserControl_Resize();
					AssertEquals(true, testControl.resizeAddressSuggestionControl);
				}
			}
		}

		public void TestScreeningStatus_ColorChanges_BasedOnStatus()
		{
			var testHeader = Factory.New<OrgHeader>();
			testHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;

			using (var form = new ZForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();
				AssertCodeBoxAndDescriptionAreThisColor("Precondition, should be not screened (orange)", testControl.ScreeningStatusDropEditForTest, DeniedPartyConstants.GridColor.NotScreened);

				testHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertCodeBoxAndDescriptionAreThisColor("Should be cleared (green)", testControl.ScreeningStatusDropEditForTest, DeniedPartyConstants.GridColor.Clear);

				testHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertCodeBoxAndDescriptionAreThisColor("Should be matched (red)", testControl.ScreeningStatusDropEditForTest, DeniedPartyConstants.GridColor.Matched);

				testHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertCodeBoxAndDescriptionAreThisColor("Should be unknown (orange)", testControl.ScreeningStatusDropEditForTest, DeniedPartyConstants.GridColor.Unknown);

				testHeader.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
				AssertCodeBoxAndDescriptionAreThisColor("Should be permanent clear (blue)", testControl.ScreeningStatusDropEditForTest, DeniedPartyConstants.GridColor.PermanentClear);
			}

			void AssertCodeBoxAndDescriptionAreThisColor(string message, ZDropEdit status, Color color)
			{
				CombineAssertions(message, () =>
				{
					AssertEquals("Description box color should be:", color, status.DescriptionBox.BackColor);
					AssertEquals("Code box color should be:", color, status.DescriptionBox.BackColor);
				});
			}
		}

		public void TestDeduplicationStartedOnDuplicateDetectionStatusLabelClick()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var testTarget = Factory.NewWithValidTestData<OrgHeader>();
			var deduplicationOrgHeader = new DeduplicationOrgHeader(testTarget);
			var testTargetList = new List<DeduplicationOrgHeader>() { deduplicationOrgHeader };

			Factory.Save();

			var scoringResults = new List<ScoringResult>();
			var score = TargetScorerController.Score(
				new DeduplicationOrgHeader(testHeader),
				new DeduplicationOrgHeader(testTarget),
				true);
			scoringResults.Add(score);

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();
				testControl.ShowDuplicatesFound(new DuplicationEventArgs(testHeader, testTargetList, scoringResults, null));

				bool wasCalled = false;
				testHeader.DeduplicationStarted += (o, e) => wasCalled = true;
				testControl.DuplicateDetectionStatusLabelOnClick(null, null);

				AssertEquals("Deduplication Started", true, wasCalled);
			}
		}

		[RequiresSTA]
		public void TestDefaultLanguageOfOrgHeader()
		{
			var country1 = Factory.NewWithValidTestData<RefCountry>();
			country1.Code = "99";
			var country1Language = Constants.Languages.Afrikaans;
			var country2 = Factory.NewWithValidTestData<RefCountry>();
			country2.Code = "88";
			var country3 = Factory.NewWithValidTestData<RefCountry>();
			country3.Code = "77";
			var country3Language = Constants.Languages.Bangla;
			Factory.Save();

			var defaultLanguages = new CountryDefaultLanguageBusinessObjectCollection()
			{
				new CountryDefaultLanguageBusinessObject
				{
					CountryPk = country1.PK,
					DefaultLanguage = country1Language,
				},
				new CountryDefaultLanguageBusinessObject
				{
					CountryPk = country3.PK,
					DefaultLanguage = country3Language,
				}
			};

			var header = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(header))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals(Constants.Languages.English, header.OH_Language);
			}

			using (OrganisationsDataRegistry.Instance.CountryDefaultLanguage.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, defaultLanguages))
			{
				header.MainAddress.OA_RN_NKCountryCode = country2.Code;
				using (var form = new ZOrganisationsForm(header))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					AssertEquals(Constants.Languages.English, header.OH_Language);
				}

				header.MainAddress.OA_RN_NKCountryCode = country1.Code;
				using (var form = new ZOrganisationsForm(header))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					AssertEquals(country1Language, header.OH_Language);
				}

				Factory.Save();

				header.MainAddress.OA_RN_NKCountryCode = country2.Code;
				using (var form = new ZOrganisationsForm(header))
				using (var testControl = new NameAndAddressControlForTesting())
				{
					form.Controls.Add(testControl);
					form.Show();

					AssertEquals(country1Language, header.OH_Language);
				}
			}
		}

		[RequiresSTA]
		public void TestScreenButtonToolTip()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.OH_IsActive = false;
			using (var form = new ZForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();
				var button = form.Controls.Find("ScreenButton", true)[0] as ZButton;
				AssertEquals("ScreenButtonCoverLabel is invisible", false, button.Visible);
				var label = form.Controls.Find("ScreenButtonCoverLabel", true)[0] as ZLabel;
				AssertEquals("ScreenButtonCoverLabel is visible", true, label.Visible);
				var tip = ToolTipService.GetToolTip(label);
				AssertEquals("The Denied Party Screening service is not available for inactive organizations.", tip);
			}

			testHeader.OH_IsActive = true;
			using (var form = new ZForm(testHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();
				var button = form.Controls.Find("ScreenButton", true)[0] as ZButton;
				AssertEquals("ScreenButtonCoverLabel is visible", true, button.Visible);
				var label = form.Controls.Find("ScreenButtonCoverLabel", true)[0] as ZLabel;
				AssertEquals("ScreenButtonCoverLabel is invisible", false, label.Visible);
			}
		}

		[RequiresSTA]
		public void TestOverrideAdditionalAddressInformationCheckbox()
		{
			var header = Factory.NewWithValidTestData<OrgHeader>();
			header.OH_OverrideAdditionalAddressInformation = false;
			using (var form = new ZForm(header))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var overrideAdditionalInfo = form.Controls.Find("OverrideAdditionalAddressInformationCheckBox", true)[0] as ZCheckBox;
				AssertEquals("Override Additional Address Information should be unchecked", false, overrideAdditionalInfo.Checked);
			}

			header.OH_OverrideAdditionalAddressInformation = true;
			using (var form = new ZForm(header))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				var overrideAdditionalInfo = form.Controls.Find("OverrideAdditionalAddressInformationCheckBox", true)[0] as ZCheckBox;
				AssertEquals("Override Additional Address Information should be checked", true, overrideAdditionalInfo.Checked);
			}
		}

		public void TestRegistrationNumberTypeCanBeSet()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(orgHeader))
			using (var testControl = new NameAndAddressControlForTesting())
			{
				form.Controls.Add(testControl);
				form.Show();

				testControl.RegistrationNumberTypeDropEdit_Exposed.CodeBox.Focus();
				testControl.RegistrationNumberTypeDropEdit_Exposed.CodeBox.Text = "PAS";
				Application.DoEvents();
				AssertEquals("PAS", testControl.RegistrationNumberTypeDropEdit_Exposed.CodeBox.Text);
			}
		}
	}
}
