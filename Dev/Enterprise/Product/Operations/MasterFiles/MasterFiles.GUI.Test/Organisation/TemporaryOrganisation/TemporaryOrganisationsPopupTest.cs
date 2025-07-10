using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(TemporaryOrganisationsPopup))]
	sealed class TemporaryOrganisationsPopupTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestChangingTheBindingMember()
		{
			IOrgHeaderCollection orgCollection = CreateOrgCollection();
			IOrgHeader tempOrg = TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection);

			var popup = new TestPopupClass(orgCollection, tempOrg);
			popup.SetDataBinding(null, "");

			AssertNoExceptionThrown("Dispose method shouldnt care that we changed the events", popup.Dispose);
		}

		public void TestPositionInControlUnchangedWhenSearching()
		{
			IOrgHeaderCollection orgCollection = CreateOrgCollection();
			IOrgHeader tempOrg = TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection);

			using (TestPopupClass testPopup = new TestPopupClass(orgCollection, tempOrg))
			{
				testPopup.Show();
				testPopup.Address1ZTextBox.Focus();

				TestPopupText(testPopup, "TEST DATA ", 10, 0);
				TestPopupText(testPopup, "TEST DATA  ", 10, 0);
				TestPopupText(testPopup, "TEST DATA  ", 11, 0);
				TestPopupText(testPopup, "TEST DATA  ", 10, 1);
				TestPopupText(testPopup, "TEST DATA  ", 9, 2);
				TestPopupText(testPopup, "TEST DATA  ", 2, 2);
			}
		}

		public void TestHasContextMenuItemToShowOrg()
		{
			var orgCollection = CreateOrgCollection();
			var existingOrg = CreateOrgForMatching();
			var tempOrg = (OrgHeader)CreateMatchingTempOrg(existingOrg, orgCollection);

			tempOrg.SimilarOrgFinder.FindSimilarOrganisations();
			AssertEquals("PRE: Should find the org from above", 1, tempOrg.SimilarOrgMatches.Count);
			AssertEquals("PRE: Should find the org from above", tempOrg.SimilarOrgMatches[0].OS_OH, existingOrg.PK);

			using (var popup = new TestPopupClass(orgCollection, tempOrg))
			{
				popup.Show();

				var menuItems = popup.SimilarOrgMatchesBoundGrid_Exposed.ContextMenu.MenuItems;
				var openOrgMenuItem = menuItems.Cast<MenuItem>().FirstOrDefault(menuItem => menuItem.Text == "View Organization");

				AssertNotNull("Menu item should exist", openOrgMenuItem);

				openOrgMenuItem.PerformClick();
				AssertEquals("Should show our existing organisation", existingOrg.PK, popup.LastShownOrganisation);

				var organisationForm = ZFormModaliser.LastFormShownDialogForTest as ZOrganisationsForm;
				try
				{
					AssertNotNull("The form should have been shown", organisationForm);
					AssertEquals("The form should have the matched org", existingOrg.PK, organisationForm.Organisation.PK);
					AssertEquals("Should be in VIEW mode", ODisplayMode.ReadOnly, organisationForm.DisplayMode);
				}
				finally
				{
					organisationForm?.Dispose();
				}
			}
		}

		OrgHeader CreateOrgForMatching()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "SUMCOD";
			org.OH_FullName = "I wish I was creative";
			org.OH_RL_NKClosestPort = "AUSYD";

			org.MainAddress.OA_Address1 = "123 Fake St";
			org.MainAddress.OA_City = "Mount Druitt";
			org.MainAddress.OA_State = "NSW";
			org.MainAddress.OA_PostCode = "2770";

			Factory.Save();
			return org;
		}

		IOrgHeader CreateMatchingTempOrg(IOrgHeader source, IOrgHeaderCollection orgCollection)
		{
			var tempOrg = TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection);
			tempOrg.OH_FullName = source.OH_FullName;
			tempOrg.OH_RL_NKClosestPort = source.OH_RL_NKClosestPort;

			tempOrg.MainAddress.OA_Address1 = source.MainAddress.OA_Address1;
			tempOrg.MainAddress.OA_City = source.MainAddress.OA_City;
			tempOrg.MainAddress.OA_State = source.MainAddress.OA_State;
			tempOrg.MainAddress.OA_PostCode = source.MainAddress.OA_PostCode;

			return tempOrg;
		}

		[ExpectNoExceptions]
		public void TestSaveDoesNotThrowException()
		{
			IOrgHeaderCollection orgCollection = CreateOrgCollection();
			BusinessObject org = TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection) as BusinessObject;
			org["OH_FullName"] = "~for test~";
			org["OH_Code"] = "~for test~";

			using (TestPopupClass popup = new TestPopupClass(orgCollection, org as IOrgHeader))
			{
				popup.Show();
				popup.ShowPreSaveDialogs_Exposed();
			}
		}

		public void TestSaveDoesNotStopByValidationStatus()
		{
			var orgCollection = CreateOrgCollection();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = "TEST ORG";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.OA_Address1 = "ABCDEF";
			org.MainAddress.OA_PostCode = "12345";
			Assert("Precondition: Org not in database", !org.IsInDatabase);

			var originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				using (var popup = new TestPopupClass(orgCollection, org))
				{
					popup.Show();
					org.MainAddress.OA_ValidationStatus = AddressValidationStatus.Invalid;

					CombineAssertions(() =>
					{
						Assert("IsTemporaryOrgAddress should be true", org.MainAddress.IsTemporaryOrgAddress);
						AssertEquals(ContinueWithSave.Yes, popup.ValidateAndSave_Exposed());
						Assert("IsTemporaryOrgAddress should be false", !org.MainAddress.IsTemporaryOrgAddress);
						AssertNoErrors(org.MainAddress);
						AssertHasWarnings(org.MainAddress.OA_ValidationStatusInfo);
						Assert("Org should in database", org.IsInDatabase);
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
			}
		}

		public void TestCloseAddressSuggestionControlWhenTextChange()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCollection = CreateOrgCollection();

			using (var form = new TestPopupClass(orgCollection, testHeader))
			{
				form.Show();

				var topRecommendedAddress = AddressSuggestionControlTest.CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
				var addressItems = new List<ValidationResultItem>() { topRecommendedAddress };

				using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), addressItems, topRecommendedAddress, form, form, form.ValidateButton))
				{
					form.SuggestionWindowParentControl.Controls.Add(suggestionControl);
					suggestionControl.Show();
					var stateBoundDropEdit = form.StateControl;
					stateBoundDropEdit.Text = "QLD";
					Application.DoEvents();
					Assert("SuggestionControl should disposed", suggestionControl.IsDisposed);
				}
			}
		}

		public void TestRefreshValidationStatus()
		{
			var originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCollection = CreateOrgCollection();

			try
			{
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
				using (var form = new TestPopupClass(orgCollection, testHeader))
				{
					form.Show();

					Env.Instance.Registry.EnableAddressValidationWebService = true;
					OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
					testHeader.MainAddress.OA_RN_NKCountryCode = "AU";
					testHeader.MainAddress.OA_State = "NSW";
					testHeader.MainAddress.OA_ValidationStatus = AddressValidationStatus.Verified;
					form.RefreshValidationStatus();
					var validGreenColor = Color.FromArgb(198, 236, 198);

					CombineAssertions(() =>
					{
						AssertEquals(validGreenColor, form.Address1Control.BackColor);
						AssertEquals(validGreenColor, form.Address2Control.BackColor);
						AssertEquals(validGreenColor, form.CountryControl.CodeBox.BackColor);
						AssertEquals(validGreenColor, form.CityControl.BackColor);
						AssertEquals(validGreenColor, form.PostcodeControl.BackColor);
						AssertEquals(validGreenColor, form.StateControl.CodeBox.BackColor);
						AssertEquals(true, form.ValidateAddressButton_Exposed.Visible);
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
			}
		}

		public void TestRefreshValidationStatus_DisabledCountry()
		{
			var originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCollection = CreateOrgCollection();
			var countryWithValidationDisabled = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");

			try
			{
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(countryWithValidationDisabled.PK, disabledForOrgAddress: true)))
				using (var form = new TestPopupClass(orgCollection, testHeader))
				{
					form.Show();

					Env.Instance.Registry.EnableAddressValidationWebService = true;
					testHeader.MainAddress.OA_State = "NSW";
					testHeader.MainAddress.OA_RN_NKCountryCode = "US";
					testHeader.MainAddress.OA_ValidationStatus = AddressValidationStatus.Verified;
					form.RefreshValidationStatus();

					CombineAssertions(() =>
					{
						AssertEquals(SystemColors.Window, form.Address1Control.BackColor);
						AssertEquals(SystemColors.Window, form.Address2Control.BackColor);
						AssertEquals(SystemColors.Window, form.CountryControl.CodeBox.BackColor);
						AssertEquals(SystemColors.Window, form.CityControl.BackColor);
						AssertEquals(SystemColors.Window, form.PostcodeControl.BackColor);
						AssertEquals(Color.FromArgb(255, 215, 215), form.StateControl.CodeBox.BackColor);
						AssertEquals(false, form.ValidateAddressButton_Exposed.Visible);
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
			}
		}

		public void TestRefreshValidationStatus_DisabledValidationService()
		{
			var originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var orgCollection = CreateOrgCollection();

			try
			{
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
				using (var form = new TestPopupClass(orgCollection, testHeader))
				{
					form.Show();

					testHeader.MainAddress.OA_State = "NSW";
					testHeader.MainAddress.OA_RN_NKCountryCode = "AU";
					testHeader.MainAddress.OA_ValidationStatus = AddressValidationStatus.Verified;
					Env.Instance.Registry.EnableAddressValidationWebService = false;
					form.RefreshValidationStatus();

					CombineAssertions(() =>
					{
						AssertEquals(SystemColors.Window, form.Address1Control.BackColor);
						AssertEquals(SystemColors.Window, form.Address2Control.BackColor);
						AssertEquals(SystemColors.Window, form.CountryControl.CodeBox.BackColor);
						AssertEquals(SystemColors.Window, form.CityControl.BackColor);
						AssertEquals(SystemColors.Window, form.PostcodeControl.BackColor);
						AssertEquals(SystemColors.Window, form.StateControl.CodeBox.BackColor);
						AssertEquals(false, form.ValidateAddressButton_Exposed.Visible);
					});
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
			}
		}

		public void TestNotShowUnmatchedOrganisationWhenUseUnmatchedOrganisationForMatchingEnabled()
		{
			var orgCollection = CreateOrgCollection();
			var tempOrg = (OrgHeader)TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection);
			AssertEquals("Precondition", 0, tempOrg.SimilarOrgMatches.Count);

			using (OrganisationsDataRegistry.Instance.UseUnmatchedOrganisationForMatching.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new UnmatchedOrganisation(Factory) { IsEnabled = true }))
			using (var form = new TestPopupClass(orgCollection, tempOrg))
			{
				form.Show();
				form.PerformSearch();
				AssertEquals(0, tempOrg.SimilarOrgMatches.Count);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			IOrgHeaderCollection orgCollection = CreateOrgCollection();
			IOrgHeader tempOrg = TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection);

			return new TemporaryOrganisationsPopup(orgCollection, tempOrg);
		}

		void TestPopupText(TestPopupClass testPopup, string originalText, int originalSelectionStart, int originalSelectionLength)
		{
			testPopup.Address1ZTextBox.Text = originalText;
			testPopup.Address1ZTextBox.SelectionStart = originalSelectionStart;
			testPopup.Address1ZTextBox.SelectionLength = originalSelectionLength;

			testPopup.ResetTimerCalled = false;
			testPopup.PerformSearch();
			AssertEquals("Text", originalText, testPopup.Address1ZTextBox.Text);
			AssertEquals("Selection start", originalSelectionStart, testPopup.Address1ZTextBox.SelectionStart);
			AssertEquals("Selection length", originalSelectionLength, testPopup.Address1ZTextBox.SelectionLength);
			AssertEquals("TextChanged event should not occur as a result of performing the search", false, testPopup.ResetTimerCalled);
		}

		public void TestMixedCaseRespectedWhenSetInRegistryTrue()
		{
			IOrgHeaderCollection orgCollection = CreateOrgCollection();
			IOrgHeader tempOrg = TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection);
			Env.Registry.SetOrgAllowMixedCase(true);
			using (var popup = new TestPopupClass(orgCollection, tempOrg))
			{
				CombineAssertions("Mixed Case Respected When Set In Registry", () =>
				{
					AssertEquals("address1", CharacterCasing.Normal, popup.Address1Control.CharacterCasing);
					AssertEquals("address2", CharacterCasing.Normal, popup.Address2Control.CharacterCasing);
					AssertEquals("city", CharacterCasing.Normal, popup.CityControl.CharacterCasing);
					AssertEquals("full name", CharacterCasing.Normal, popup.NameControl.CharacterCasing);
					AssertEquals("email (always normal)", CharacterCasing.Normal, popup.EmailControl.CharacterCasing);
					AssertEquals("state", CharacterCasing.Normal, popup.StateControl.CharacterCasing);
				});
			}
		}
		public void TestMixedCaseRespectedWhenSetInRegistryFalse()
		{
			IOrgHeaderCollection orgCollection = CreateOrgCollection();
			IOrgHeader tempOrg = TemporaryOrganisationCreator.GetNewTemporaryOrganisation(Factory, orgCollection);
			Env.Registry.SetOrgAllowMixedCase(false);
			using (var popup = new TestPopupClass(orgCollection, tempOrg))
			{
				CombineAssertions("Mixed Case Respected When Set In Registry", () =>
				{
					AssertEquals("address1", CharacterCasing.Upper, popup.Address1Control.CharacterCasing);
					AssertEquals("address2", CharacterCasing.Upper, popup.Address2Control.CharacterCasing);
					AssertEquals("city", CharacterCasing.Upper, popup.CityControl.CharacterCasing);
					AssertEquals("full name", CharacterCasing.Upper, popup.NameControl.CharacterCasing);
					AssertEquals("email (always normal)", CharacterCasing.Normal, popup.EmailControl.CharacterCasing);
					AssertEquals("state", CharacterCasing.Upper, popup.StateControl.CharacterCasing);
				});
			}
		}

		IOrgHeaderCollection CreateOrgCollection()
		{
			return new OrgHeaderCollection(Factory);
		}

		class TestPopupClass : TemporaryOrganisationsPopup
		{
			public TestPopupClass(IOrgHeaderCollection collection, IOrgHeader businessEntity)
				: base(collection, businessEntity)
			{
			}

			public ZTextBox Address1ZTextBox
			{
				get { return base.Address1TextBox; }
			}

			public void PerformSearch()
			{
				base.SearchTimer_Tick(this, EventArgs.Empty);
			}

			public bool ResetTimerCalled;
			protected override void ResetTimer()
			{
				ResetTimerCalled = true;
			}

			public ContinueWithSave ShowPreSaveDialogs_Exposed()
			{
				return base.ShowPreSaveDialogs();
			}

			public ZGrid SimilarOrgMatchesBoundGrid_Exposed
			{
				get { return SimilarOrgMatchesBoundGrid; }
			}

			protected override void ShowOrganisation(IOrgPatternMatch matchingOrg)
			{
				LastShownOrganisation = matchingOrg?.OS_OH ?? ZGuid.Empty;

				base.ShowOrganisation(matchingOrg);
			}
			public ZGuid LastShownOrganisation { get; private set; }

			public ContinueWithSave ValidateAndSave_Exposed()
			{
				return ValidateAndSave();
			}

			protected override void CommitOrgToFindBox()
			{
			}

			public void PerformValidateAddressClick()
			{
				ValidateAddressButton_Click(this, null);
			}

			public ZButton ValidateAddressButton_Exposed => ValidateAddressButton;
		}

		#endregion
	}
}
