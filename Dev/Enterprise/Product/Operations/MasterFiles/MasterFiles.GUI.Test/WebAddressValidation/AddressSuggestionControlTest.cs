using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressSuggestionControlTest : TestCaseWithFactory
	{
#if !WINZOR // WI00914184 - [BetterListView] CS - ListView-SelectedItems refactor
		public void TestUseSelectedButtonEnabledWhenTopSuggestedAddressSelected()
		{
			var addressItems = new List<ValidationResultItem>();
			var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
			addressItems.Add(topRecommendedAddress);
			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), addressItems, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				Assert("Top suggested address auto selected, \"Use Selected\" button should be enabled", suggestionControl.Find(c => c.Name == "ValidateAddressButton").First().Enabled);
			}
		}

		public void TestMessageShowWhenSelectSuggestedAddressClicked()
		{
			var addressItems = new List<ValidationResultItem>();
			var currentOrgAddress = Factory.NewWithValidTestData<OrgAddress>();

			var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
			topRecommendedAddress.UnparsedAddressInformation = new string('0', 10);
			currentOrgAddress.UnrestrictedAdditionalAddressInformation = new string('A', currentOrgAddress.OA_AdditionalAddressInformationInfo.MaxLength);

			addressItems.Add(topRecommendedAddress);

			using (var applicationForm = new ZForm())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControl(currentOrgAddress, addressItems, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				suggestionControl.SelectAddressAndClose();
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains($"The validated address' additional address information details could not be added to the existing additional address information field as the value is longer than {OrgAddressAdditionalInfoSchema.OAI_AdditionalInfo.MaxLength} characters"));
			}
		}

		public void TestConstructList()
		{
			var addressItems = new List<ValidationResultItem>();
			var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
			var address1 = CreateAddressItem("Add11", "Add21", "City2", "AU", "Group1", "P1", "NSW");
			var address2 = CreateAddressItem("Add12", "Add22", "City3", "AU", "Group2", "P2", "NSW");
			var address3 = CreateAddressItem("Add13", "Add23", "City4", "AU", "Group2", "P3", "NSW");

			addressItems.Add(topRecommendedAddress);
			addressItems.Add(address1);
			addressItems.Add(address2);
			addressItems.Add(address3);

			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), addressItems, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				Assert(suggestionControl.HasTopRecommendedItem);
#if !WINZOR
				Assert(suggestionControl.AddressListViewExposedForTesting.ShowGroups);
				AssertEquals("Top recommended item has no group", null, suggestionControl.AddressListViewExposedForTesting.SelectedItems[0].Group);
				AssertEquals("No group created if address it has no group or there is only one item in group", 1, suggestionControl.AddressListViewExposedForTesting.Groups.Count);
				AssertEquals(2, suggestionControl.AddressListViewExposedForTesting.Groups[0].Items.Count);
				AssertEquals(address2, suggestionControl.AddressListViewExposedForTesting.Groups[0].Items[0].Tag);
				AssertEquals(address3, suggestionControl.AddressListViewExposedForTesting.Groups[0].Items[1].Tag);
#endif
				AssertEquals("Top recommended item is selected by default", 1, suggestionControl.AddressListViewExposedForTesting.SelectedItems.Count);
				var selectedItem = suggestionControl.AddressListViewExposedForTesting.SelectedItems[0].Tag as ValidationResultItem;
				AssertEquals("Top recommended item is selected by default", topRecommendedAddress.ToString(), selectedItem.ToString());
				AssertEquals("Text for display is constructed correctly", "Add10, Add20, City0, NSW, AU, P0", suggestionControl.AddressListViewExposedForTesting.SelectedItems[0].Text);
			}
		}

		public void TestConstructListWhenStateProvinceValidationRuleIsMustNotBeEnteredOrNot()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_RN_NKCountryCode = "AU";

			var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;

			var addressItems = new List<ValidationResultItem>();
			var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
			var address1 = CreateAddressItem("Add11", "Add21", "City1", "AU", "Group1", "P1", "NSW");
			var address2 = CreateAddressItem("Add12", "Add22", "City2", "AU", "Group1", "P2", "NSW");

			addressItems.Add(topRecommendedAddress);
			addressItems.Add(address1);
			addressItems.Add(address2);

			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControl(orgAddress, addressItems, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				AssertEquals("TopRecommendedAddress should not contain state.", "Add10, Add20, City0, AU, P0", suggestionControl.AddressListViewExposedForTesting.SelectedItems[0].Text);
#if !WINZOR
				AssertEquals("Should only have 1 group.", 1, suggestionControl.AddressListViewExposedForTesting.Groups.Count);
				AssertEquals("Should have 2 group items.", 2, suggestionControl.AddressListViewExposedForTesting.Groups[0].Items.Count);
				AssertEquals("Group's header should not contain state.", " City1 P1", suggestionControl.AddressListViewExposedForTesting.Groups[0].Header);
				AssertEquals("Group's item1 should not contain state.", "Add11, Add21, City1, AU, P1", suggestionControl.AddressListViewExposedForTesting.Groups[0].Items[0].Text);
				AssertEquals("Group's item2 should not contain state.", "Add12, Add22, City2, AU, P2", suggestionControl.AddressListViewExposedForTesting.Groups[0].Items[1].Text);
#endif
			}

			au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.NoValidationRule;
			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControl(orgAddress, addressItems, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				AssertEquals("TopRecommendedAddress should contain state.", "Add10, Add20, City0, NSW, AU, P0", suggestionControl.AddressListViewExposedForTesting.SelectedItems[0].Text);
#if !WINZOR
				AssertEquals("Should only have 1 group.", 1, suggestionControl.AddressListViewExposedForTesting.Groups.Count);
				AssertEquals("Should have 2 group items.", 2, suggestionControl.AddressListViewExposedForTesting.Groups[0].Items.Count);
				AssertEquals("Group's header should contain state.", " City1 NSW P1", suggestionControl.AddressListViewExposedForTesting.Groups[0].Header);
				AssertEquals("Group's item1 should contain state.", "Add11, Add21, City1, NSW, AU, P1", suggestionControl.AddressListViewExposedForTesting.Groups[0].Items[0].Text);
				AssertEquals("Group's item2 should contain state.", "Add12, Add22, City2, NSW, AU, P2", suggestionControl.AddressListViewExposedForTesting.Groups[0].Items[1].Text);
#endif
			}
		}
#endif

		public void TestManuallyVerifyAddress()
		{
			var oldValue = Env.Security.OrgAddressesAllowedManualVerification.IsAllowed;
			try
			{
				Env.Security.OrgAddressesAllowedManualVerification.IsAllowed = true;
				var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");

				using (var applicationForm = new Form())
				using (var parentControl = new Control())
				using (var validationButton = new Button())
				using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), null, topRecommendedAddress, applicationForm, parentControl, validationButton))
				{
					suggestionControl.Close();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					suggestionControl.AddressSelected += (_, x_) => { };
					AssertNoExceptionThrown(() => suggestionControl.ManualVerifyButton_Click(this, null));
				}
			}
			finally
			{
				Env.Security.OrgAddressesAllowedManualVerification.IsAllowed = oldValue;
			}
		}

		public void TestManuallyVerifyAddressWithoutSecurityRights()
		{
			var oldValue = Env.Security.OrgAddressesAllowedManualVerification.IsAllowed;
			try
			{
				Env.Security.OrgAddressesAllowedManualVerification.IsAllowed = false;
				var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");

				using (var applicationForm = new Form())
				using (var parentControl = new Control())
				using (var validationButton = new Button())
				using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), null, topRecommendedAddress, applicationForm, parentControl, validationButton))
				{
					suggestionControl.Close();
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
					suggestionControl.AddressSelected += (_, x_) => { };
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					suggestionControl.ManualVerifyButton_Click(this, null);
					AssertEquals("Should show error message because user does not have security rights", @"You do not have the appropriate security rights to manually verify an address.

If you require access to manually verify an address, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> Addresses -> Allow Manual Verification", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Security.OrgAddressesAllowedManualVerification.IsAllowed = oldValue;
			}
		}

		public static ValidationResultItem CreateAddressItem(string address1, string address2, string city, string countryCode, string group, string postcode, string state)
		{
			var addressItem = new ValidationResultItem
			{
				Address1 = address1,
				Address2 = address2,
				City = city,
				Country = countryCode,
				Group = @group,
				Postcode = postcode,
				State = state
			};
			return addressItem;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1827:Do not use Count() or LongCount() when Any() can be used", Justification = "Test Method")]
		public void TestWithEmptyAddressesItems()
		{
			var addressItems = new List<ValidationResultItem>();
			ValidationResultItem topRecommendedAddress = null;

			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), addressItems, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				Assert(suggestionControl.AddressListViewExposedForTesting.Items.Count == 0);
			}

			Assert(ErrorReporter.LastMessageReported.Contains("AddressesListView is empty"));

			ErrorReporter.Clear();
		}

		public void TestConstructor_WhenGettingEnoughSpace_ShouldOpenToRightByDefault()
		{
			// Arrange.

			var topRecommendation = CreateAddressItem(
				"[_MOCK_ADDRESS_1_]",
				"[_MOCK_ADDRESS_2_]",
				"[_MOCK_CITY_]",
				"[_MOCK_COUNTRY_]",
				"[_MOCK_GROUP_]",
				"[_MOCK_POSTCODE_]",
				"[_MOCK_STATE_]");

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var buttonLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var buttonSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			// Act.

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var validationButton = new Button { Location = buttonLocation, Size = buttonSize })
			{
				parentControl.Controls.Add(validationButton);
				applicationForm.Controls.Add(parentControl);

				var suggestionControl = new AddressSuggestionControl(
					address,
					null,
					topRecommendation,
					applicationForm,
					parentControl,
					validationButton);

				using (suggestionControl)
				{
					// Assert.

					AssertEquals(
						100 + 1,
						ControlDpiScalingHelper.UnscaleFromCurrentDpiX(suggestionControl.Location.X));

					AssertEquals(
						0,
						ControlDpiScalingHelper.UnscaleFromCurrentDpiY(suggestionControl.Location.Y));
				}
			}
		}

		public void TestHeightShouldBeDefaultValue()
		{
			// Arrange.

			var topRecommendation = CreateAddressItem(
				"[_MOCK_ADDRESS_1_]",
				"[_MOCK_ADDRESS_2_]",
				"[_MOCK_CITY_]",
				"[_MOCK_COUNTRY_]",
				"[_MOCK_GROUP_]",
				"[_MOCK_POSTCODE_]",
				"[_MOCK_STATE_]");

			var address = Factory.NewWithValidTestData<OrgAddress>();

			var formSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var parentLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var parentSize = ControlDpiScalingHelper.NewScaledSize(2000, 2000);

			var buttonLocation = ControlDpiScalingHelper.NewScaledPoint(0, 0);
			var buttonSize = ControlDpiScalingHelper.NewScaledSize(100, 100);

			// Act.

			using (var applicationForm = new Form { Size = formSize })
			using (var parentControl = new Control { Location = parentLocation, Size = parentSize })
			using (var validationButton = new Button { Location = buttonLocation, Size = buttonSize })
			{
				parentControl.Controls.Add(validationButton);
				applicationForm.Controls.Add(parentControl);

				var suggestionControl = new AddressSuggestionControl(
					address,
					null,
					topRecommendation,
					applicationForm,
					parentControl,
					validationButton);

				using (suggestionControl)
				{
					// Assert.

					AssertEquals(
						320,
						ControlDpiScalingHelper.UnscaleFromCurrentDpiX(suggestionControl.Height));
				}
			}
		}

		public void TestHideListView()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");

			orgAddress.OA_Address1 = "Address Line 1";
			orgAddress.OA_ValidationStatus = "MAN";

			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControlForTest(Factory.NewWithValidTestData<OrgAddress>(), null, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				suggestionControl.HideListView(orgAddress);
				AssertEquals("No suggestions provided as this has been manually verified. Press the Validation Icon to call the service.", suggestionControl.InfoLabelExposed.Text);

				orgAddress.ValidationStatus = "VLD";
				suggestionControl.HideListView(orgAddress);
				AssertEquals("No suggestions have been found for the address that you entered. Please confirm as original or amend the address to receive suggestions.", suggestionControl.InfoLabelExposed.Text);
			}
		}

		public void TestUpdateSize()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var topRecommendedAddress = CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");

			orgAddress.OA_Address1 = "Address Line 1";
			orgAddress.OA_ValidationStatus = "MAN";

			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControlForTest(Factory.NewWithValidTestData<OrgAddress>(), null, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				suggestionControl.IsFixedControl = false;
				suggestionControl.Show();
				AssertEquals(280, suggestionControl.Width);
				AssertEquals("Preconditon", 125, suggestionControl.ManuallyVerifyButton.Width);
				AssertEquals("Preconditon", "Accept as Entered", suggestionControl.ManuallyVerifyButton.CaptionResourceString.Caption);
				AssertEquals("Preconditon", "Accept as Entered", suggestionControl.ManuallyVerifyButton.ToolTipCaption);

				suggestionControl.UpdateSizeAndLocation(applicationForm, parentControl, validationButton, 200);

				var margin = ControlDpiScalingHelper.ScaleToCurrentDpiX(8);
				AssertEquals(suggestionControl.Width - suggestionControl.ValidateButton.Width - margin, suggestionControl.ManuallyVerifyButton.Width);
				AssertEquals("Accept as ...", suggestionControl.ManuallyVerifyButton.CaptionResourceString.Caption);
				AssertEquals("Accept as Entered", suggestionControl.ManuallyVerifyButton.ToolTipCaption);
			}
		}

		public void TestUpdateSizeWithImage()
		{
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			var topRecommendedAddress = CreateAddressItem("Add10000 11111 22222", "Add20000", "City0", "AU", "", "P0", "NSW");

			orgAddress.OA_Address1 = "Address Line 1";
			orgAddress.OA_ValidationStatus = "MAN";

			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControlForTest(Factory.NewWithValidTestData<OrgAddress>(), null, topRecommendedAddress, applicationForm, parentControl, validationButton))
			{
				suggestionControl.IsFixedControl = false;
				suggestionControl.Show();
#if WINZOR
				AssertEquals(318, suggestionControl.AddressListViewExposedForTesting.Columns[0].Width);
#else
				AssertEquals(336, suggestionControl.AddressListViewExposedForTesting.Columns[0].Width);
#endif
				suggestionControl.UpdateSizeAndLocation(applicationForm, parentControl, validationButton, 200);
				AssertEquals(178, suggestionControl.AddressListViewExposedForTesting.Columns[0].Width);
			}
		}

		public void TestAddressSelectedHandlerBeenUnregisteredAfterDisposed()
		{
			var addressItems = new List<ValidationResultItem> { new ValidationResultItem() };

			using (var form = new ZForm())
			using (var parentControl = new Control())
			using (var control = new AddressSuggestionControl(Factory.New<OrgAddress>(), addressItems, addressItems.Single(), form, parentControl, null))
			{
				var fieldInfo = typeof(AddressSuggestionControl).GetField("AddressSelected", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
				var addressSelected = fieldInfo.GetValue(control);

				var addressSelectedMethods = (addressSelected as Delegate).GetInvocationList().Select(x => x.Method.Name).ToArray();
				AssertEquals(1, addressSelectedMethods.Length);
				Assert(addressSelectedMethods.Any(x => x.Equals("HandleAddressSelected")));

				control.Dispose();
				addressSelected = fieldInfo.GetValue(control);
				AssertNull(addressSelected);
			}
		}

		public void TestAddressListViewViewType()
		{
			using (var applicationForm = new Form())
			using (var parentControl = new Control())
			using (var validationButton = new Button())
			using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), null, null, applicationForm, parentControl, validationButton))
			{
				AssertNotEquals(suggestionControl.AddressListViewExposedForTesting.View, View.LargeIcon);
			}
		}

		#region Implementation

		class AddressSuggestionControlForTest : AddressSuggestionControl
		{
			public AddressSuggestionControlForTest(ISupportWebAddressValidation address, List<ValidationResultItem> addressesItems, ValidationResultItem topRecommendedAddress, Form applicationForm, Control parentControl, Control referenceControl, int maxWidth = 0)
				: base(address, addressesItems, topRecommendedAddress, applicationForm, parentControl, referenceControl, maxWidth)
			{
			}

			public ZLabel InfoLabelExposed => base.InfoLabel;

			public ZButton ValidateButton => base.ValidateAddressButton;
		}

		#endregion
	}
}
