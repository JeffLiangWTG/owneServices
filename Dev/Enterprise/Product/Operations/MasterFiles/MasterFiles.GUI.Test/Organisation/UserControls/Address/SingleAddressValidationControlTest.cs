using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.AddressCleansing.Common;
using ISupportWebAddressValidation = Enterprise.MasterFiles.Business.ISupportWebAddressValidation;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class SingleAddressValidationControlTest : TestCaseWithFactory
	{
		protected override void TearDown()
		{
			Balloon.Instance.Hide();
			base.TearDown();
		}

		MDMAdminPanelAddressView GetMDMAddress(bool addressEntityIsJobDocAddress = false, bool isMainAddress = false, string status = AddressValidationStatus.Invalid)
		{
			var collection = new MDMAdminPanelAddressCollection(Factory);
			var org = Factory.NewWithValidTestData<OrgHeader>();
			ZQuery query = null;
			if (addressEntityIsJobDocAddress)
			{
				var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
				docAddress.E2_ParentID = org.PK;
				docAddress.E2_ParentTableCode = "OH";
				docAddress.E2_Address1 = "DOCADDRESS1_ADDRESS1";
				docAddress.E2_Address2 = "DOCADDRESS1_ADDRESS2";
				docAddress.E2_Postcode = "210036";
				docAddress.E2_City = "CITY";
				docAddress.E2_State = "NSW";
				docAddress.E2_RN_NKCountryCode = "AU";
				docAddress.E2_ValidationStatus = status;
				Factory.Save();
				query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, docAddress.PK);
			}
			else
			{
				var orgheader = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = isMainAddress ? orgheader.MainAddress : orgheader.Addresses.AddNew();
				orgAddress.OA_CompanyNameOverride = "Wise Tech";
				orgAddress.OA_Address1 = "ORGADDRESS1_ADDRESS1";
				orgAddress.OA_Address2 = "ORGADDRESS1_ADDRESS2";
				orgAddress.OA_PostCode = "210036";
				orgAddress.OA_City = "CITY";
				orgAddress.OA_State = "NSW";
				orgAddress.OA_RN_NKCountryCode = "AU";
				orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
				orgAddress.OA_ValidationStatus = status;
				Factory.Save();
				query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, orgAddress.PK);
			}
			collection.Load(query);
			return collection[0];
		}

		public void TestBindingToAddress()
		{
			var mdm = GetMDMAddress();
			using (var form = new ZForm(mdm))
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(singleAddressControl);
				form.Show();

				var address = mdm.AddressEntity as OrgAddress;
				AssertEquals(address.Address1, singleAddressControl.Address1Control.Text);
			}
		}

#if !WINZOR //WI00914184 - [BetterListView] CS - ListView-SelectedItems refactor
		public void TestAddressValidationWorks()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				singleAddressControl.SetDataBinding(GetMDMAddress(), "");
				singleAddressControl.PerformValidateAddressClickForTest(ValidationResultStatusCode.StreetExact);
				singleAddressControl.AddressSuggestionControl.SelectAddressAndClose();
				AssertEquals("Address verified", AddressValidationStatus.Verified, (singleAddressControl.CurrentAddressEntity as OrgAddress).ValidationStatus);
			}
		}
#endif

		[RequiresSTA]
		public void TestCityTownSuggestions()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				form.Controls.Add(singleAddressControl);
				form.Show();
				singleAddressControl.SetDataBinding(GetMDMAddress(), "");
				var address = singleAddressControl.CurrentAddressEntity as OrgAddress;
				address.OA_RN_NKCountryCode = "US";
				address.OA_PostCode = "6006";

				AssertEquals("Address began validation", AddressValidationStatus.ToBeVerified, address.OA_ValidationStatus);
			}
		}

#if !WINZOR //WI00914184 - [BetterListView] CS - ListView-SelectedItems refactor
		public void TestSetSelectedAddress()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				var selectedAddress = new ValidationResultItem()
				{
					Address1 = "1234567890-1234567890-1234567890-1234567890-1234567890", // 54 character
					Address2 = "1234567890-1234567890-1234567890-1234567890-1234567890",
					City = "VeryLongCityNameForTestingAddressValidationAssigningTheRightValue",
					Postcode = "VeryLongPostCodeForAddressValidation"
				};

				singleAddressControl.SetDataBinding(GetMDMAddress(), "");
				SetupSelectedAddress(singleAddressControl.AddressSuggestionControl.AddressListViewExposedForTesting, selectedAddress);
				singleAddressControl.AddressSuggestionControl.SelectAddressAndClose();
				var address = singleAddressControl.CurrentAddressEntity as OrgAddress;
				AssertNotNull(address);
				AssertEquals("Address verified", AddressValidationStatus.Verified, address.ValidationStatus);
				AssertEquals(address.Address1, selectedAddress.Address1.Substring(0, OrgAddressSchema.OA_Address1.MaxLength));
				AssertEquals(address.Address2, selectedAddress.Address2.Substring(0, OrgAddressSchema.OA_Address2.MaxLength));
				AssertEquals(address.City, selectedAddress.City.Substring(0, OrgAddressSchema.OA_City.MaxLength));
				AssertEquals(address.Postcode, selectedAddress.Postcode.Substring(0, OrgAddressSchema.OA_PostCode.MaxLength));
			}
		}

		public void TestMessageShowWorks_WhenSetSelectedAddressLengthExceed()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				var selectedAddress = new ValidationResultItem()
				{
					Address1 = "address1",
					Address2 = "address2",
					UnparsedAddressInformation = new string('0', OrgAddressAdditionalInfoSchema.OAI_AdditionalInfo.MaxLength + 5),
				};

				singleAddressControl.SetDataBinding(GetMDMAddress(), "");
				SetupSelectedAddress(singleAddressControl.AddressSuggestionControl.AddressListViewExposedForTesting, selectedAddress);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				singleAddressControl.AddressSuggestionControl.SelectAddressAndClose();
				var address = singleAddressControl.CurrentAddressEntity as OrgAddress;
				AssertEquals("ADDITIONAL ADDRESS", address.UnrestrictedAdditionalAddressInformation);
				Assert(UnitTestUserNotification.Instance.LastMessage.Text.Contains($"The validated address' additional address information details could not be added to the existing additional address information field as the value is longer than {OrgAddressAdditionalInfoSchema.OAI_AdditionalInfo.MaxLength} characters"));
			}
		}

		public void TestSetSelectedAddressWhenStateIsMustNotBeEntered()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				var selectedAddress = new ValidationResultItem()
				{
					Address1 = "address1",
					Address2 = "address2",
					State = "NSW",
					Country = "AU"
				};

				var au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
				au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustNotBeEntered;

				singleAddressControl.SetDataBinding(GetMDMAddress(), "");
				var address = singleAddressControl.CurrentAddressEntity as OrgAddress;
				AssertNotNullOrEmpty("Precondition", address.State);

				SetupSelectedAddress(singleAddressControl.AddressSuggestionControl.AddressListViewExposedForTesting, selectedAddress);
				singleAddressControl.AddressSuggestionControl.SelectAddressAndClose();
				AssertNotNull(address);
				Assert("The state is must not be entered.", address.Country.IsStateMustNotBeEntered);
				AssertNullOrEmpty(address.State);
			}
		}

		public void TestCoordinatesInfo()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				singleAddressControl.SetDataBinding(GetMDMAddress(), "");
				var address = singleAddressControl.CurrentAddressEntity as OrgAddress;
				AssertNullOrEmpty("Precondition", address.AddressMap);
				AssertNotEquals("Precondition", 30m, address.Longitude);
				AssertNotEquals("Precondition", 30m, address.Latitude);

				var selectedAddress = new ValidationResultItem()
				{
					Address1 = "Unit 3A",
					Address2 = "72 O'Riordan ST",
					Apartment = "Unit 3A",
					StreetNumber = "72",
					Street = "O'Riordan ST",
					Longitude = 30,
					Latitude = 30
				};

				SetupSelectedAddress(singleAddressControl.AddressSuggestionControl.AddressListViewExposedForTesting, selectedAddress);
				singleAddressControl.AddressSuggestionControl.SelectAddressAndClose();
				AssertEquals("AA1[0-6]SNA2[0-1]SA2[3-14]", address.AddressMap);
				AssertEquals(30m, address.Longitude);
				AssertEquals(30m, address.Latitude);
			}
		}
#endif
		public void TestTextBoxReadOnlyWhenAddressNotOrgAddress()
		{
			var rawEnableAddressValidationWebServiceValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
				using (var form = new ZForm())
				using (var control = new SingleAddressValidationControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					control.SimulateIsOnSelectedTab = true;
					var mdmAddress1 = GetMDMAddress(true);
					var mdmAddress2 = GetMDMAddress();

					control.SetDataBinding(mdmAddress1, "");
					Assert(control.AdditionalAddressInformationControl.ReadOnly);
					Assert(control.AddressCodeControl.ReadOnly);

					control.SetDataBinding(mdmAddress2, "");
					Assert(!control.AdditionalAddressInformationControl.ReadOnly);
					Assert(!control.AddressCodeControl.ReadOnly);
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawEnableAddressValidationWebServiceValue;
			}
		}

		public void TestShouldSetTextToEmptyWhenBindNullObject()
		{
			var rawEnableAddressValidationWebServiceValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
				using (var form = new ZForm())
				using (var control = new SingleAddressValidationControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					control.SimulateIsOnSelectedTab = true;
					var mdmAddress1 = GetMDMAddress();

					control.SetDataBinding(mdmAddress1, "");
					AssertNotNullOrEmpty(control.OrganisationControl.CurrentCode);
					AssertNotNullOrEmpty(control.OrganisationNameControl.Text);
					AssertNotNullOrEmpty(control.AddressTypeControl.Text);

					control.SetDataBinding(null, "");
					AssertNullOrEmpty(control.OrganisationControl.CurrentCode);
					AssertNullOrEmpty(control.OrganisationControl.DescriptionBox.Text);
					AssertNullOrEmpty(control.OrganisationNameControl.Text);
					AssertNullOrEmpty(control.AddressTypeControl.Text);
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawEnableAddressValidationWebServiceValue;
			}
		}

		public void TestShouldValidateAddressWhenDataItemChanged_IfAddressValidationWebServiceIsEnabled()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SimulateIsOnSelectedTab = true;
				var mdmAddress1 = GetMDMAddress();
				var mdmAddress2 = GetMDMAddress();

				control.SetDataBinding(mdmAddress1, "");
				Assert("Auto Validating started", control.WebAddressValidationStarted);

				control.SetDataBinding(mdmAddress2, "");
				Assert("Auto Validating started again", control.WebAddressValidationStarted);
			}
		}

		public void TestShouldNotValidateAddressWhenDataItemChanged_IfAddressValidationWebServiceIsDisabled()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SimulateIsOnSelectedTab = true;
				var mdmAddress1 = GetMDMAddress();
				var mdmAddress2 = GetMDMAddress();

				control.SetDataBinding(mdmAddress1, "");
				Assert("Auto Validating is not started", !control.WebAddressValidationStarted);

				control.SetDataBinding(mdmAddress2, "");
				Assert("Auto Validating is not started again", !control.WebAddressValidationStarted);
			}
		}

		public void TestValidationWillNotRunWhenControlIsNotBindingCompleted()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SimulateIsOnSelectedTab = true;
				var mdmAddress1 = GetMDMAddress();
				var mdmAddress2 = GetMDMAddress();

				control.BindingCompleted = false;
				control.SetDataBinding(mdmAddress1, "");
				Assert("Validating didn't start", !control.WebAddressValidationStarted);

				control.BindingCompleted = true;
				control.SetDataBinding(mdmAddress2, "");
				Assert("Validating started", control.WebAddressValidationStarted);
			}
		}

		public void TestDoNotAutoValidateManuallyVerifiedAddressWhenDataItemChanged()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				form.Controls.Add(control);
				form.Show();
				control.SimulateIsOnSelectedTab = true;
				var mdmAddress1 = GetMDMAddress(false, false, AddressValidationStatus.ManuallyVerified);
				var mdmAddress2 = GetMDMAddress(false, false, AddressValidationStatus.ManuallyVerified);

				control.SetDataBinding(mdmAddress1, "");
				Assert("Auto Validating should not start", !control.WebAddressValidationStarted);

				control.ValidateButton.PerformClick();
				Assert("Auto Validating started", control.WebAddressValidationStarted);

				control.RunAddressValidation = false;
				control.WebAddressValidationStarted = false;

				control.SetDataBinding(mdmAddress2, "");
				Assert("Auto Validating should not start", !control.WebAddressValidationStarted);

				control.ValidateButton.PerformClick();
				Assert("Auto Validating started", control.WebAddressValidationStarted);
			}
		}

		public void TestAddressSuggestionControl()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				AssertEquals("The SuggestionControl should be shown", true, singleAddressControl.AddressSuggestionControl.Visible);
				AssertEquals("The SuggestionControl should be disabled", false, singleAddressControl.AddressSuggestionControl.Enabled);

				AssertAddressSuggestionControl(singleAddressControl, AddressValidationStatus.Invalid, true);
				AssertAddressSuggestionControl(singleAddressControl, AddressValidationStatus.Verified, false);
				AssertAddressSuggestionControl(singleAddressControl, AddressValidationStatus.ManuallyVerified, true);
				AssertAddressSuggestionControl(singleAddressControl, AddressValidationStatus.CountryNotAvailable, false);
				AssertAddressSuggestionControl(singleAddressControl, AddressValidationStatus.Unverifiable, true);
				AssertAddressSuggestionControl(singleAddressControl, AddressValidationStatus.VerifiedToStreet, false);
			}
		}

		void AssertAddressSuggestionControl(SingleAddressValidationControlForTest singleAddressControl, string status, bool controlEnabled)
		{
			singleAddressControl.ValidationStatusForTest = status;
			singleAddressControl.SetDataBinding(GetMDMAddress(false, false, status), "");
			AssertEquals("The SuggestionControl's visible should be true", true, singleAddressControl.AddressSuggestionControl.Visible);
			AssertEquals("The SuggestionControl's enabled should be " + controlEnabled, controlEnabled, singleAddressControl.AddressSuggestionControl.Enabled);
		}

		[RequiresSTA]
		public void TestEncodeURLParamsWhenSearchOnline()
		{
			var preEnableAddressValidationValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
				using (var form = new ZForm())
				using (var singleAddressControl = new SingleAddressValidationControlForTest())
				{
					singleAddressControl.SimulateIsOnSelectedTab = true;
					form.Controls.Add(singleAddressControl);
					form.Show();

					var mdmAddress = GetMDMAddress();
					mdmAddress.MDM_CompanyName = "& \"#%+,/:;<=>?@\\|";
					singleAddressControl.SetDataBinding(mdmAddress, "");

					WebUrlLauncher.ClearLastUrlLaunched();
					AssertEquals("Precondition", string.Empty, WebUrlLauncher.LastUrlLaunched);

					singleAddressControl.SearchCompanyOnline();
					AssertEquals("Special characters should be encoded", "http://google.com/search?q=%26+%22%23%25%2B%2C%2F%3A%3B%3C%3D%3E%3F%40%5C%7C+Australia", WebUrlLauncher.LastUrlLaunched);
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = preEnableAddressValidationValue;
			}
		}

		public void TestHotKeys()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				form.Controls.Add(control);
				form.Show();
				control.SimulateIsOnSelectedTab = true;
				var mdmAddress1 = GetMDMAddress();
				control.SetDataBinding(mdmAddress1, "");
				control.FireProcessCmdKey(Keys.Control | Keys.M);

				var address = control.CurrentAddressEntity as OrgAddress;
				AssertEquals("The address should be manually verified", AddressValidationStatus.ManuallyVerified, address.OA_ValidationStatus);
			}
		}

		public void TestModifiedAddressInfoShouldBeRetainedWhenManuallyVerifyByHotKey()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			{
				using (var form = new ZForm())
				using (var control = new SingleAddressValidationControlForTest())
				using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
				{
					form.Controls.Add(control);
					form.Show();
					control.SimulateIsOnSelectedTab = true;
					var mdmAddress1 = GetMDMAddress();
					control.SetDataBinding(mdmAddress1, "");
					var address = control.CurrentAddressEntity as OrgAddress;

					control.AddressCodeControl.Focus();
					control.AddressCodeControl.Text = "ORGADDRESS1_ADDRESS1_TEST";
					AssertEquals("Precondition", "ORGADDRESS1_ADDRESS1", address.AddressCode);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("ORGADDRESS1_ADDRESS1_TEST", address.AddressCode);

					control.Address1Control.Focus();
					control.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
					AssertEquals("Precondition", "ORGADDRESS1_ADDRESS1", address.Address1);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("ORGADDRESS1_ADDRESS1_TEST", address.Address1);

					control.Address2Control.Focus();
					control.Address2Control.Text = "ORGADDRESS1_ADDRESS2_TEST";
					AssertEquals("Precondition", "ORGADDRESS1_ADDRESS2", address.Address2);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("ORGADDRESS1_ADDRESS2_TEST", address.Address2);

					control.AdditionalAddressInformationControl.Focus();
					control.AdditionalAddressInformationControl.Text = "ADDITIONAL ADDRESS_TEST";
					AssertEquals("Precondition", "ADDITIONAL ADDRESS", address.UnrestrictedAdditionalAddressInformation);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("ADDITIONAL ADDRESS_TEST", address.UnrestrictedAdditionalAddressInformation);

					control.CountryControl.CodeBox.Focus();
					control.CountryControl.CodeBox.Text = "CN";
					AssertEquals("Precondition", "AU", address.OA_RN_NKCountryCode);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("CN", address.OA_RN_NKCountryCode);

					control.PostcodeControl.Focus();
					control.PostcodeControl.Text = "210000";
					AssertEquals("Precondition", "210036", address.Postcode);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("210000", address.Postcode);

					control.CityControl.Focus();
					control.CityControl.Text = "CITY_TEST";
					AssertEquals("Precondition", "CITY", address.City);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("CITY_TEST", address.City);

					control.StateControl.Focus();
					control.StateControl.Text = "ACT";
					AssertEquals("Precondition", "NSW", address.StateCode);
					control.FireProcessCmdKey(Keys.Control | Keys.M);
					AssertEquals("ACT", address.StateCode);
				}
			}
		}

		public void TestUserManualVerifiedAddressWillNotChangeValidationStatusWhenModifyAddressInfo()
		{
			var rawRegistryValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var mdmAddressOrg = GetMDMAddress();
				var mdmAddressDoc = GetMDMAddress(true);
				AssertValidationStatusChangeResult(mdmAddressOrg, "ORGADDRESS1_ADDRESS1", "ORGADDRESS1_ADDRESS2");
				AssertValidationStatusChangeResult(mdmAddressDoc, "DOCADDRESS1_ADDRESS1", "DOCADDRESS1_ADDRESS2");

				void AssertValidationStatusChangeResult(MDMAdminPanelAddressView addressView, string address1, string address2)
				{
					using (var form = new ZForm())
					using (var control = new SingleAddressValidationControlForTest())
					using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
					{
						form.Controls.Add(control);
						form.Show();
						control.SimulateIsOnSelectedTab = true;
						control.ValidationStatusForTest = AddressValidationStatus.Invalid;

						control.SetDataBinding(addressView, "");
						var address = control.CurrentAddressEntity as ISupportWebAddressValidation;

						control.Address1Control.Focus();
						AssertEquals(AddressValidationStatus.Invalid, address.ValidationStatus);
						AssertEquals(address1, address.Address1);
						AssertEquals(address2, address.Address2);

						control.FireProcessCmdKey(Keys.Control | Keys.M);
						AssertEquals(AddressValidationStatus.ManuallyVerified, address.ValidationStatus);

						control.Address1Control.Text = "NEW_ADDRESS_1";
						control.Address2Control.Focus();
						AssertEquals("NEW_ADDRESS_1", address.Address1);
						AssertEquals(AddressValidationStatus.ManuallyVerified, address.ValidationStatus);

						control.Address2Control.Text = "NEW_ADDRESS_2";
						control.Address1Control.Focus();
						AssertEquals("NEW_ADDRESS_2", address.Address2);
						AssertEquals(AddressValidationStatus.ManuallyVerified, address.ValidationStatus);

						control.ValidateButton.PerformClick();
						AssertEquals(AddressValidationStatus.Invalid, address.ValidationStatus);
					}
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawRegistryValue;
			}
		}

#if !WINZOR //WI00914184 - [BetterListView] CS - ListView-SelectedItems refactor
		public void TestAdditionalAddressInfoShouldBeRetained()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;

			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				var selectedAddress = new ValidationResultItem()
				{
					UnparsedAddressInformation = "AddressInfomation"
				};

				singleAddressControl.SetDataBinding(GetMDMAddress(), "");

				var address = singleAddressControl.CurrentAddressEntity as OrgAddress;
				AssertEquals("Precondition", "ADDITIONAL ADDRESS", address.OA_AdditionalAddressInformation);

				SetupSelectedAddress(singleAddressControl.AddressSuggestionControl.AddressListViewExposedForTesting, selectedAddress);
				singleAddressControl.AddressSuggestionControl.SelectAddressAndClose();
				AssertEquals("Additional addressInfo should be retained", "ADDITIONAL ADDRESS, AddressInfomation", address.OA_AdditionalAddressInformation);
			}
		}
#endif
		public void TestRefreshSuggestionControl()
		{
			using (var form = new ZForm())
			using (var singleAddressControl = new SingleAddressValidationControlForTest())
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				singleAddressControl.SimulateIsOnSelectedTab = true;
				form.Controls.Add(singleAddressControl);
				form.Show();

				var validationResult = new WebAddressValidationResult();
				validationResult.ResultAddress = new ValidationResultItem { Address1 = "Myrtle Street", City = "Prospect", ResultStatusCode = ValidationResultStatusCode.PointClose, AvailableData = AvailableData.StreetNumber };

				singleAddressControl.SetDataBinding(GetMDMAddress(), "");
				Env.Instance.Registry.EnableAddressValidationWebService = false;
				singleAddressControl.RefreshSuggestionControl(SingleAddressValidationControl.SuggestionControlStatus.HideListView);
				Assert(!singleAddressControl.AddressSuggestionControl.Enabled);

				singleAddressControl.RefreshSuggestionControl(SingleAddressValidationControl.SuggestionControlStatus.Disabled);
				Assert(!singleAddressControl.AddressSuggestionControl.Enabled);

				singleAddressControl.RefreshSuggestionControl(SingleAddressValidationControl.SuggestionControlStatus.SetUpListView);
				Assert(!singleAddressControl.AddressSuggestionControl.Enabled);

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				singleAddressControl.RefreshSuggestionControl(SingleAddressValidationControl.SuggestionControlStatus.HideListView);
				Assert(singleAddressControl.AddressSuggestionControl.Enabled);

				singleAddressControl.RefreshSuggestionControl(SingleAddressValidationControl.SuggestionControlStatus.Disabled);
				Assert(!singleAddressControl.AddressSuggestionControl.Enabled);

				singleAddressControl.RefreshSuggestionControl(SingleAddressValidationControl.SuggestionControlStatus.SetUpListView, validationResult);
				Assert(singleAddressControl.AddressSuggestionControl.Enabled);
			}
		}

		public void TestAllowModifyCurrentAddress()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			{
				using (var form = new ZForm())
				using (var control = new SingleAddressValidationControlForTest())
				{
					form.Controls.Add(control);
					form.Show();
					control.SimulateIsOnSelectedTab = true;

					var jobDocAddress = GetMDMAddress(true);
					var mainddress = GetMDMAddress(false, true);
					var address = GetMDMAddress();

					Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
					Env.Security.OrgAddressDetailsModify.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					control.SetDataBinding(jobDocAddress, "");
					AssertStatusForAllowModifyCurrentAddress(control, true, null);

					Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
					Env.Security.OrgAddressDetailsModify.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					control.SetDataBinding(mainddress, "");
					AssertStatusForAllowModifyCurrentAddress(control, true, null);

					Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
					Env.Security.OrgAddressDetailsModify.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessages();
					control.SetDataBinding(address, "");
					AssertStatusForAllowModifyCurrentAddress(control, true, null);

					Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = false;
					Env.Security.OrgAddressDetailsModify.IsAllowed = true;
					UnitTestUserNotification.Instance.ClearMessages();
					control.SetDataBinding(mainddress, "");
					AssertStatusForAllowModifyCurrentAddress(control, false, Env.Security.OrgDetailsModifyNameAndAddress.ErrorMessageForNotAllowed);

					Env.Security.OrgDetailsModifyNameAndAddress.IsAllowed = true;
					Env.Security.OrgAddressDetailsModify.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					control.SetDataBinding(address, "");
					AssertStatusForAllowModifyCurrentAddress(control, false, Env.Security.OrgAddressDetailsModify.ErrorMessageForNotAllowed);
				}
			}
		}

		void AssertStatusForAllowModifyCurrentAddress(SingleAddressValidationControlForTest control, bool enabled, string text)
		{
			AssertEquals(enabled, control.ValidateButton.Enabled);
			AssertEquals(enabled, control.ClearButton.Enabled);
			AssertEquals(enabled, control.AddressSuggestionControl.Enabled);
			AssertEquals(text, UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestOpenRecordButton()
		{
			AssertOpenRecordButton();
		}

		public void TestOpenRecordButtonForExtendedClient()
		{
			using (ZArchitecture.Modules.ClientHookLoader.Instance.OverrideClientAssemblyForTest(Clients.JAS))
			{
				AssertOpenRecordButton();
			}
		}

		public void TestSaveButton()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var orgAddress = GetMDMAddress();
				control.SetDataBinding(orgAddress, "");

				control.Address1Control.Focus();
				control.Address1Control.Text = "ORGADDRESS1_ADDRESS1_TEST";
				control.Address2Control.Focus();
				control.Address2Control.Text = "ORGADDRESS1_ADDRESS2_TEST";
				control.PostcodeControl.Focus();
				control.PostcodeControl.Text = "210037";
				control.CityControl.Focus();
				control.CityControl.Text = "LONDON";
				control.StateControl.Focus();
				control.StateControl.Text = "ABE";
				control.CountryControl.Focus();
				control.CountryControl.CurrentCode = "GB";
				control.AdditionalAddressInformationControl.Focus();
				control.AdditionalAddressInformationControl.Text = "ADDITIONAL ADDRESS_TEST";
				control.AddressCodeControl.Focus();
				control.AddressCodeControl.Text = "TESTCODE2";
				control.SaveButton.Focus();
				control.SaveButton.PerformClick();

				var savedOrgAddress1 = Factory.Load<OrgAddress>(orgAddress.PK);
				AssertEquals("The address1 should be 'ORGADDRESS1_ADDRESS1_TEST'", "ORGADDRESS1_ADDRESS1_TEST", savedOrgAddress1.OA_Address1);
				AssertEquals("The address2 should be 'ORGADDRESS1_ADDRESS2_TEST'", "ORGADDRESS1_ADDRESS2_TEST", savedOrgAddress1.OA_Address2);
				AssertEquals("The postcode should be '210037'", "210037", savedOrgAddress1.OA_PostCode);
				AssertEquals("The city should be 'LONDON'", "LONDON", savedOrgAddress1.OA_City);
				AssertEquals("The state should be 'ABE'", "ABE", savedOrgAddress1.OA_State);
				AssertEquals("The country should be 'GB'", "GB", savedOrgAddress1.OA_RN_NKCountryCode);
				AssertEquals("The additionaladdressinformation should be 'ADDITIONAL ADDRESS_TEST'", "ADDITIONAL ADDRESS_TEST", savedOrgAddress1.OA_AdditionalAddressInformation);
				AssertEquals("The address code should be 'TESTCODE2'", "TESTCODE2", savedOrgAddress1.OA_Code);
			}
		}

		public void TestSaveAddressWithChange_LogUpdated()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var addressView = GetMDMAddress(true);
				control.SetDataBinding(addressView, "");

				var log = Factory.Load(addressView.MDM_ParentTableCode, addressView.MDM_ParentID).GetLogs();
				var logItemCount = log.DatabaseCount;

				var newAddress = control.CurrentAddressEntity as ISupportWebAddressValidation;
				newAddress.Address1 = "changed";

				control.SaveButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(logItemCount + 1, log.DatabaseCount);
					AssertContains("from 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference.ToString());
					AssertContains("to 'changed DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference.ToString());
				});
			}
		}

		public void TestSaveAddressWithNoChange_LogUpdated()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var addressView = GetMDMAddress(true);
				control.SetDataBinding(addressView, "");

				var log = Factory.Load(addressView.MDM_ParentTableCode, addressView.MDM_ParentID).GetLogs();
				var logItemCount = log.DatabaseCount;

				var newAddress = control.CurrentAddressEntity as ISupportWebAddressValidation;
				newAddress.Address1 = "changed";
				newAddress.Address2 = "changed";

				newAddress.Address1 = "DOCADDRESS1_ADDRESS1";
				newAddress.Address2 = "DOCADDRESS1_ADDRESS2";

				control.SaveButton.PerformClick();

				CombineAssertions(() =>
				{
					AssertEquals(logItemCount + 1, log.DatabaseCount);
					AssertContains("from 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference.ToString());
					AssertContains("to 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference.ToString());
				});
			}
		}

		public void TestVerifyAddress_LogUpdated()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var addressView = GetMDMAddress(true);
				control.SetDataBinding(addressView, "");

				var log = Factory.Load(addressView.MDM_ParentTableCode, addressView.MDM_ParentID).GetLogs();
				var logItemCount = log.DatabaseCount;

				var verifiedAddress = control.CurrentAddressEntity as ISupportWebAddressValidation;
				verifiedAddress.Address1 = "changed";

				control.AcceptAddress(verifiedAddress);

				AssertEquals(logItemCount + 1, log.DatabaseCount);
				AssertContains("from 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference.ToString());
				AssertContains("to 'changed DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference.ToString());
			}
		}
		public void TestVerifyAddress_ValidationStatusChange_LogUpdated()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var addressView = GetMDMAddress(addressEntityIsJobDocAddress: true);
				control.SetDataBinding(addressView, "");

				var log = Factory.Load(addressView.MDM_ParentTableCode, addressView.MDM_ParentID).GetLogs();
				var logItemCount = log.DatabaseCount;

				var verifiedAddress = control.CurrentAddressEntity as ISupportWebAddressValidation;
				verifiedAddress.Address1 = "changed";
				verifiedAddress.ValidationStatus = AddressValidationStatus.ToBeVerified;
				control.AcceptAddress(verifiedAddress);

				CombineAssertions(() =>
				{
					AssertEquals("There should be one new log", logItemCount + 1, log.DatabaseCount);
					AssertContains("from 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference);
					AssertContains("to 'changed DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU'", log.MostRecentLog.DisplayEventReference);
					AssertContains("Validation status changed from INV to NYV", log.MostRecentLog.DisplayEventReference);
				});
			}
		}
		public void TestVerifyAddress_CancelBtnClicked_NoLogsUpdated()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var verifiedJobAddress = VerifiedJobDocAddress;
				var jobAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "JobDocAddress1")).FirstOrDefault();
				var query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, jobAddress.PK);
				var collection = new MDMAdminPanelAddressCollection(Factory);
				collection.Load(query);
				control.SetDataBinding(collection[0], "");

				var log = Factory.Load(collection[0].MDM_ParentTableCode, collection[0].MDM_ParentID).GetLogs();
				var logItemCount = log.DatabaseCount;

				control.UpdateChoice = UpdateChoice.Cancel;
				control.AcceptAddress(jobAddress);

				AssertEquals(logItemCount, log.DatabaseCount);
			}
		}

		public void TestSaveMultipleRecords_AllLogsShouldBeUpdated()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 7))
			{
				form.Show();

				var addressControl = form.UserControl;
				var addressDetailControl = addressControl.AddressDetailControl;
				var verifiedAddressView = GetMDMAddress(true);

				for (var i = 0; i < 5; i++)
				{
					GetMDMAddress(true);
				}

				var filterControl = addressControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
				filter.Property = "DOCADDRESS1_ADDRESS1";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;
				filterControl.FirePerformSearch();

				var jobAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "DOCADDRESS1_ADDRESS1"));
				var logItemCountList = new List<int>();
				for (var i = 0; i < 5; i++)
				{
					logItemCountList.Add(Factory.Load(jobAddresses[i].E2_ParentTableCode, jobAddresses[i].E2_ParentID).GetLogs().DatabaseCount);
				}

				addressDetailControl.SaveMultipleRecords((ISupportWebAddressValidation)verifiedAddressView.AddressEntity, jobAddresses.ToList());

				for (var i = 0; i < 5; i++)
				{
					var logItemCount = Factory.Load(jobAddresses[i].E2_ParentTableCode, jobAddresses[i].E2_ParentID).GetLogs().DatabaseCount;
					AssertEquals(logItemCountList[i] + 1, logItemCount);
				}
			}
		}

		public void TestSaveMultipleRecords_AddressChanged_AllLogsShouldBeUpdated()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				form.Show();

				var addressControl = form.UserControl;
				var addressDetailControl = addressControl.AddressDetailControl;
				var verifiedAddressView = GetMDMAddress(addressEntityIsJobDocAddress: true);
				var addressEntity = (JobDocAddress)verifiedAddressView.AddressEntity;

				for (var i = 0; i < 3; i++)
				{
					GetMDMAddress(addressEntityIsJobDocAddress: true);
				}

				PerformAddress1FilterSearch(addressControl.FilterControl, "DOCADDRESS1_ADDRESS1");

				var jobAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "DOCADDRESS1_ADDRESS1"));
				addressDetailControl.SaveMultipleRecords(addressEntity, jobAddresses.ToList());

				addressEntity.E2_City = "NEWCITY";
				addressDetailControl.SaveMultipleRecords(addressEntity, jobAddresses.ToList());

				AssertEquals("Precondition:", true, jobAddresses.All(a => a.E2_ParentTableCode == "OH"));
				var parentOrgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, jobAddresses.Select(a => a.E2_ParentID)));

				CombineAssertions(() =>
				{
					foreach (var parentOrg in parentOrgHeaders)
					{
						var logs = parentOrg.GetLogs();
						AssertContains("from 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU' to 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 NEWCITY New South Wales 210036 AU'", logs.MostRecentLog.DisplayEventReference.ToString());
					}
				});
			}
		}

		public void TestSaveMultipleRecords_ValidationChanged_AllLogsShouldBeUpdated()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				form.Show();

				var addressControl = form.UserControl;
				var addressDetailControl = addressControl.AddressDetailControl;
				var verifiedAddressView = GetMDMAddress(addressEntityIsJobDocAddress: true);
				var addressEntity = (JobDocAddress)verifiedAddressView.AddressEntity;

				for (var i = 0; i < 3; i++)
				{
					GetMDMAddress(addressEntityIsJobDocAddress: true);
				}

				PerformAddress1FilterSearch(addressControl.FilterControl, "DOCADDRESS1_ADDRESS1");

				var jobAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "DOCADDRESS1_ADDRESS1"));    //jobAddresses holds 4 of the addresses loaded
				addressDetailControl.SaveMultipleRecords(addressEntity, jobAddresses.ToList());

				addressEntity.ValidationStatus = AddressValidationStatus.ToBeVerified;
				addressDetailControl.SaveMultipleRecords(addressEntity, jobAddresses.ToList());

				AssertEquals("Precondition:", true, jobAddresses.All(a => a.E2_ParentTableCode == "OH"));
				var parentOrgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, jobAddresses.Select(a => a.E2_ParentID)));

				CombineAssertions(() =>
				{
					foreach (var parentOrg in parentOrgHeaders)
					{
						var logs = parentOrg.GetLogs();
						AssertContains("Validation status changed from INV to NYV", logs.MostRecentLog.DisplayEventReference.ToString());
					}
				});
			}
		}

		public void TestSaveMultipleRecords_ValidationAndAddressChanged_AllLogsShouldBeUpdated()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 5))
			{
				form.Show();

				var addressControl = form.UserControl;
				var addressDetailControl = addressControl.AddressDetailControl;
				var verifiedAddressView = GetMDMAddress(addressEntityIsJobDocAddress: true);
				var addressEntity = (JobDocAddress)verifiedAddressView.AddressEntity;

				for (var i = 0; i < 3; i++)
				{
					GetMDMAddress(addressEntityIsJobDocAddress: true);
				}

				PerformAddress1FilterSearch(addressControl.FilterControl, "DOCADDRESS1_ADDRESS1");

				var jobAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "DOCADDRESS1_ADDRESS1"));    //jobAddresses holds 4 of the addresses loaded
				addressDetailControl.SaveMultipleRecords(addressEntity, jobAddresses.ToList());

				addressEntity.ValidationStatus = AddressValidationStatus.ManuallyVerified;
				addressEntity.E2_City = "NEWNEWCITY";
				addressDetailControl.SaveMultipleRecords(addressEntity, jobAddresses.ToList());

				AssertEquals("Precondition:", true, jobAddresses.All(a => a.E2_ParentTableCode == "OH"));
				var parentOrgHeaders = Factory.Load<OrgHeader>(new ZQuery(OrgHeaderSchema.PK, jobAddresses.Select(a => a.E2_ParentID)));

				CombineAssertions(() =>
				{
					foreach (var parentOrg in parentOrgHeaders)
					{
						var logs = parentOrg.GetLogs();
						AssertContains("Validation status changed from INV to MAN", logs.MostRecentLog.DisplayEventReference.ToString());
						AssertContains("from 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 CITY New South Wales 210036 AU' to 'DOCADDRESS1_ADDRESS1 DOCADDRESS1_ADDRESS2 NEWNEWCITY New South Wales 210036 AU'", logs.MostRecentLog.DisplayEventReference.ToString());
					}
				});
			}
		}

		void PerformAddress1FilterSearch(AddressesFilterControl filterControl, string property)
		{
			var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
			filter.Property = property;
			filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
			filter.IsActive = true;
			filterControl.FirePerformSearch();
		}

		public void TestValidatePartialPropertiesOfOrgAddress()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
				orgAddress.OA_Address1 = "Address1";
				orgAddress.OA_Address2 = "Français";
				orgAddress.OA_Language = Core.Constants.Languages.English;
				orgAddress.OA_PostCode = "";
				orgAddress.OA_City = "Français";
				orgAddress.OA_State = "";
				orgAddress.OA_RN_NKCountryCode = "AU";
				orgAddress.OA_Phone = "1";
				orgAddress.OA_ValidationStatus = "INV";
				Factory.Save();

				var query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, orgAddress.PK);
				var collection = new MDMAdminPanelAddressCollection(Factory);
				collection.Load(query);
				control.SetDataBinding(collection[0], "");

				var header = orgAddress.Header;
				header.OH_RL_NKClosestPort = "AUSYD";
				RefCountry au = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "AU");
				au.RN_PostcodeValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
				au.RN_StateProvinceValidationRule = CountryAddressValidationRuleList.Codes.MustBeEntered;
				Factory.Save();

				var address = control.CurrentAddressEntity as OrgAddress;
				address.OA_Address1 = "";
				address.ValidationStatus = "INV";
				address.OA_Code = "";
				control.SaveButton.PerformClick();

				Assert("This property should be validated in the Admin Panel.", address.OA_Address1Info.HasErrors());
				Assert("This property should be validated in the Admin Panel.", address.OA_Address2Info.HasErrors());
				Assert("This property should be validated in the Admin Panel.", address.OA_StateInfo.HasErrors());
				Assert("This property should be validated in the Admin Panel.", address.OA_PostCodeInfo.HasErrors());
				Assert("This property should be validated in the Admin Panel.", address.OA_CityInfo.HasErrors());
				Assert("This property should be validated in the Admin Panel.", address.OA_CodeInfo.HasErrors());

				address.OA_RN_NKCountryCode = "";
				control.SaveButton.PerformClick();
				Assert("This property should be validated in the Admin Panel.", address.OA_RN_NKCountryCodeInfo.HasErrors());

				Assert("Phone number will not be validated in the Admin Panel. Only the above properties of orgaddress will be validated.", !address.OA_PhoneInfo.HasErrors());
				Assert("Validation status will not be validated in the Admin Panel.", !address.OA_ValidationStatusInfo.HasErrors());
			}
		}

		public void TestValidatePartialPropertiesOfJobDocAddress()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			using (RawDataRegistry.Instance.JobAddressValidation_CityMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (RawDataRegistry.Instance.JobAddressValidation_CountryMandatory.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				form.Controls.Add(control);
				form.Show();

				var docAddress = Factory.NewWithValidTestData<JobDocAddress>();
				docAddress.E2_AddressOverride = true;
				docAddress.E2_Address1 = "Address1";
				docAddress.E2_Address2 = "Address2";
				docAddress.E2_Postcode = "";
				docAddress.E2_City = "";
				docAddress.E2_State = "";
				docAddress.E2_RN_NKCountryCode = "AU";
				docAddress.E2_Phone = "1";
				docAddress.E2_ValidationStatus = "INV";
				Factory.Save();

				var query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, docAddress.PK);
				var collection = new MDMAdminPanelAddressCollection(Factory);
				collection.Load(query);
				control.SetDataBinding(collection[0], "");

				var address = control.CurrentAddressEntity as JobDocAddress;
				address.E2_Address1 = "";
				address.ValidationStatus = "INV";
				control.SaveButton.PerformClick();

				Assert("This property should be validated in the Admin Panel.", address.E2_Address1Info.HasErrors());
				Assert("This property should be validated in the Admin Panel.", address.E2_StateInfo.HasErrors());

				address.E2_RN_NKCountryCode = "";
				control.SaveButton.PerformClick();
				Assert("This property should be validated in the Admin Panel.", address.E2_RN_NKCountryCodeInfo.HasErrors());

				Assert("Phone number will not be validated in the Admin Panel. Only the above properties of jobdocaddress will be validated.", !address.E2_PhoneInfo.HasErrors());
				Assert("Validation status will not be validated in the Admin Panel.", !address.E2_ValidationStatusInfo.HasErrors());
			}
		}

		public void TestSaveWhenHasErrors()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var orgAddress = GetMDMAddress();
				control.SetDataBinding(orgAddress, "");

				var address = control.CurrentAddressEntity as OrgAddress;
				address.Address1 = "";
				address.ValidationStatus = "INV";
				control.SaveButton.PerformClick();

				Assert("The address has errors.", address.HasErrors);
				Assert("The address1 has errors.", address.OA_Address1Info.HasErrors());
				Assert("The save button should be enabled.", control.SaveButton.Enabled);
				AssertEquals("There are errors that need to be corrected before this Address (ORGADDRESS1_ADDRESS1) can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				address.OA_Address1 = "ORGADDRESS1_ADDRESS1_TEST";
				address.ValidationStatus = "INV";
				control.SaveButton.PerformClick();

				Assert("The address has no errors.", !address.HasErrors);
				AssertNull("The validation status is invalid but the address can be saved.", UnitTestUserNotification.Instance.LastMessage.Text);

				var savedOrgAddress1 = Factory.Load<OrgAddress>(orgAddress.PK);
				AssertEquals("ORGADDRESS1_ADDRESS1_TEST", savedOrgAddress1.Address1);
				AssertEquals("INV", savedOrgAddress1.ValidationStatus);
			}
		}

		public void TestUpdateMultipleJobDocAddressesForm()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var verifiedJobAddress = VerifiedJobDocAddress;
				var jobAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "JobDocAddress1")).FirstOrDefault();
				var query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, jobAddress.PK);
				var collection = new MDMAdminPanelAddressCollection(Factory);
				collection.Load(query);
				control.SetDataBinding(collection[0], "");
				control.AcceptAddress(jobAddress);

				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(lastShownForm);
					AssertEquals(lastShownForm.GetType(), typeof(UpdateMultipleJobDocAddressesForm));
				}
			}
		}

		public void TestUIStatusWhenCurrentDataItemChange()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);

				var mdmAddress1 = GetMDMAddress();
				var mdmAddress2 = GetMDMAddress();

				mdmAddress1.AutoVerifyState = AddressAutoVerifyState.Waiting;
				control.SetDataBinding(mdmAddress1, "");
				Assert(control.AddressBoundPanel.Enabled);

				mdmAddress2.AutoVerifyState = AddressAutoVerifyState.Verifying;
				control.SetDataBinding(mdmAddress2, "");
				Assert(!control.AddressBoundPanel.Enabled);
				AssertContains("This address is being auto-verified", control.AddressDetailGroupBox.Text);

				mdmAddress1.AutoVerifyState = AddressAutoVerifyState.Verified;
				control.SetDataBinding(mdmAddress1, "");
				Assert(control.AddressBoundPanel.Enabled);
				AssertContains("Address Details(This address has been auto-verified", control.AddressDetailGroupBox.Text);
			}
		}

		[RequiresSTA]
		public void TestAddressHasChanges()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				var mdmAddress1 = GetMDMAddress();
				var mdmAddress2 = GetMDMAddress();

				Assert(!mdmAddress1.HasAddressInfoChanges);
				control.SetDataBinding(mdmAddress1, "");
				var orgAddress = mdmAddress1.AddressEntity as OrgAddress;
				orgAddress.OA_Address1 = Guid.NewGuid().ToString();
				Application.DoEvents();
				Assert(mdmAddress1.HasAddressInfoChanges);

				Assert(!mdmAddress2.HasAddressInfoChanges);
				control.SetDataBinding(mdmAddress2, "");
				orgAddress = mdmAddress2.AddressEntity as OrgAddress;
				orgAddress.PrimaryOrgAddressAdditionalInfoDetail = Guid.NewGuid().ToString();
				Application.DoEvents();
				Assert(mdmAddress2.HasAddressInfoChanges);
			}
		}

		public void TestNotAutoValidateAddressWhenDataItemChangedButHasAutoVerifiedMark()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();
				control.SimulateIsOnSelectedTab = true;
				var mdmAddress1 = GetMDMAddress();
				var mdmAddress2 = GetMDMAddress();

				control.WebAddressValidationStarted = false;
				control.SetDataBinding(mdmAddress1, "");
				Assert("Auto Validating started", control.WebAddressValidationStarted);

				control.WebAddressValidationStarted = false;
				mdmAddress2.AutoVerifyState = AddressAutoVerifyState.Verified;
				control.SetDataBinding(mdmAddress2, "");
				Assert("Auto Validating should not start", !control.WebAddressValidationStarted);
			}
		}

		JobDocAddress VerifiedJobDocAddress
		{
			get
			{
				if (verifiedJobDocAddress == null)
				{
					var org = Factory.NewWithValidTestData<OrgHeader>();

					for (int i = 0; i < 10; i++)
					{
						var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
						jobDocAddress.E2_ParentID = org.PK;
						jobDocAddress.E2_ParentTableCode = "OH";
						jobDocAddress.E2_AddressOverride = true;
						jobDocAddress.E2_Address1 = "JobDocAddress1";
						jobDocAddress.E2_Address2 = "JobDocAddress2";
						jobDocAddress.E2_Postcode = "0001";
						jobDocAddress.E2_City = "ALEXANDRIA";
						jobDocAddress.E2_State = "NSW";
						jobDocAddress.E2_RN_NKCountryCode = "AU";
						jobDocAddress.GeoLocation = ZGeography.CreatePoint(1.0, 1.0);
						jobDocAddress.AddressMap = "AddressMap";
						jobDocAddress.E2_ValidationStatus = "INV";
					}

					verifiedJobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
					verifiedJobDocAddress.E2_ParentID = org.PK;
					verifiedJobDocAddress.E2_ParentTableCode = "OH";
					verifiedJobDocAddress.E2_AddressOverride = true;
					verifiedJobDocAddress.E2_Address1 = "VerifiedJobDocAddress1";
					verifiedJobDocAddress.E2_Address2 = "VerifiedJobDocAddress2";
					verifiedJobDocAddress.E2_Postcode = "0002";
					verifiedJobDocAddress.E2_City = "NOBLE PARK";
					verifiedJobDocAddress.E2_State = "VIC";
					verifiedJobDocAddress.E2_RN_NKCountryCode = "AU";
					verifiedJobDocAddress.GeoLocation = ZGeography.CreatePoint(2.0, 2.0);
					verifiedJobDocAddress.AddressMap = "VerifiedAddressMap";
					verifiedJobDocAddress.E2_ValidationStatus = "VAD";

					Factory.Save();
				}

				return verifiedJobDocAddress;
			}
		}

		JobDocAddress verifiedJobDocAddress;

		public void TestTitleOfUpdateMultipleJobDocAddressesForm()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var verifiedJobAddress = VerifiedJobDocAddress;
				var jobAddress = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "JobDocAddress1")).FirstOrDefault();
				var actualValue = control.GetFormTitle(jobAddress, verifiedJobAddress, 10, TitleType.VerifyAll);
				var expectedValue = "The address 'JobDocAddress1, JobDocAddress2, AU, ALEXANDRIA, 0001, NSW' is in the system 10 times. Do you want to update all records to 'VerifiedJobDocAddress1, VerifiedJobDocAddress2, AU, NOBLE PARK, 0002, VIC' ?";
				AssertEquals(expectedValue, actualValue);

				actualValue = control.GetFormTitle(jobAddress, verifiedJobAddress, 10, TitleType.ManuallyVerifyAll);
				expectedValue = "The address 'JobDocAddress1, JobDocAddress2, AU, ALEXANDRIA, 0001, NSW' is in the system 10 times. Do you want to update all records to 'VerifiedJobDocAddress1, VerifiedJobDocAddress2, AU, NOBLE PARK, 0002, VIC' and manually verify these addresses?";
				AssertEquals(expectedValue, actualValue);

				actualValue = control.GetFormTitle(jobAddress, verifiedJobAddress, 10, TitleType.ManuallyVerifyAllInDataBase);
				expectedValue = "The address 'JobDocAddress1, JobDocAddress2, AU, ALEXANDRIA, 0001, NSW' is in the system 10 times. Do you want to manually verify all records with the same address 'VerifiedJobDocAddress1, VerifiedJobDocAddress2, AU, NOBLE PARK, 0002, VIC'?";
				AssertEquals(expectedValue, actualValue);
			}
		}

		public void TestShouldShowUpdateMultipleJobDocAddressesFormWhenThereArMoreSameAddressesInDataBase()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var jobDocAddress = Factory.NewWithValidTestData<JobDocAddress>();
				var verifiedJobAddress = VerifiedJobDocAddress;
				verifiedJobAddress.E2_Address1 = "JobDocAddress1";
				verifiedJobAddress.E2_Address2 = "JobDocAddress2";
				verifiedJobAddress.E2_Postcode = "0001";
				verifiedJobAddress.E2_City = "ALEXANDRIA";
				verifiedJobAddress.E2_State = "NSW";
				verifiedJobAddress.E2_RN_NKCountryCode = "AU";
				Factory.Save();

				control.ManuallyVerifyAllInDataBase(verifiedJobAddress, new List<JobDocAddress>() { jobDocAddress });
				using (var lastShownForm = (ZForm)ZFormModaliser.LastFormShownDialogForTest)
				{
					AssertNotNull(lastShownForm);
					AssertEquals(lastShownForm.GetType(), typeof(UpdateMultipleJobDocAddressesForm));
				}
			}
		}

		public void TestUpdateAllJobDocAddresses()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 7))
			{
				form.Show();

				var verifiedJobAddress = VerifiedJobDocAddress;
				var addressControl = form.UserControl;
				var addressDetailControl = addressControl.AddressDetailControl;
				var filterControl = addressControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
				filter.Property = "JobDocAddress1";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;
				filterControl.FirePerformSearch();

				var jobAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "JobDocAddress1"));
				AssertEquals("Precondition", 10, jobAddresses.Length);
				AssertEquals("Precondition: 7 records are in the filter grid, 3 records are in the database.", 7, addressControl.Manager.AdminPanelAddressCollection.Count);
				AssertEquals("Precondition: No records are in the processed grid.", 0, addressControl.Manager.AdminPanelProcessedAddressCollection.Count);
				foreach (var address in jobAddresses)
				{
					AssertNotEquals("Precondition", address.Address1, VerifiedJobDocAddress.Address1);
					AssertNotEquals("Precondition", address.Address2, VerifiedJobDocAddress.Address2);
					AssertNotEquals("Precondition", address.City, VerifiedJobDocAddress.City);
					AssertNotEquals("Precondition", address.Postcode, VerifiedJobDocAddress.Postcode);
					AssertNotEquals("Precondition", address.State, VerifiedJobDocAddress.State);
					AssertNotEquals("Precondition", address.AddressMap, VerifiedJobDocAddress.AddressMap);
					AssertNotEquals("Precondition", address.GeoLocation.Latitude, VerifiedJobDocAddress.GeoLocation.Latitude);
					AssertNotEquals("Precondition", address.GeoLocation.Longitude, VerifiedJobDocAddress.GeoLocation.Longitude);
				}

				addressDetailControl.SaveMultipleRecords(verifiedJobAddress, jobAddresses.ToList());

				jobAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "VerifiedJobDocAddress1"));
				AssertEquals("10 records have been updated, including on screen and in the database.", 10, jobAddresses.Length - 1);
				AssertEquals("No records are in the filter grid.", 0, addressControl.Manager.AdminPanelAddressCollection.Count);
				AssertEquals("7 records were in the filter gird and have been moved to processed grid.", 7, addressControl.Manager.AdminPanelProcessedAddressCollection.Count);
				foreach (var address in jobAddresses)
				{
					AssertEquals(address.Address1, VerifiedJobDocAddress.Address1);
					AssertEquals(address.Address2, VerifiedJobDocAddress.Address2);
					AssertEquals(address.City, VerifiedJobDocAddress.City);
					AssertEquals(address.Postcode, VerifiedJobDocAddress.Postcode);
					AssertEquals(address.State, VerifiedJobDocAddress.State);
					AssertEquals(address.AddressMap, VerifiedJobDocAddress.AddressMap);
					AssertEquals(address.GeoLocation.Latitude, VerifiedJobDocAddress.GeoLocation.Latitude);
					AssertEquals(address.GeoLocation.Longitude, VerifiedJobDocAddress.GeoLocation.Longitude);
				}
			}
		}

		public void TestShouldNotThrowException_ManuallyVerifyLastRecordInTheUnProcessedGrid()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.RefreshGrid += delegate
				{
					control.SetDataBinding(null, "");
				};

				GetMDMAddress(true);
				var addressView = GetMDMAddress(true);
				control.SetDataBinding(addressView, "");

				var verifiedAddress = control.CurrentAddressEntity as ISupportWebAddressValidation;
				verifiedAddress.Address1 = "changed";
				verifiedAddress.ValidationStatus = AddressValidationStatus.ManuallyVerified;
				control.UpdateChoice = UpdateChoice.ThisRecord;

				AssertNoExceptionThrown(() => control.AcceptAddress(verifiedAddress));
			}
		}

		public void TestSaveMultipleRecords_WhenCurrentItemIsNull()
		{
			using (var form = new AdministrationPanelFormForTest(Factory))
			using (SystemDataRegistry.Instance.MaximumNumberOfAddressesForMDMAddressGrid.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 7))
			{
				form.Show();

				var verifiedJobAddress = VerifiedJobDocAddress;
				var addressControl = form.UserControl;
				var addressDetailControl = addressControl.AddressDetailControl;
				var filterControl = addressControl.FilterControl;
				var filter = filterControl.FilterBusinessObject[AddressesFilterBusinessObject.AddressFilterConstants.Address1] as ModuleTextFilter;
				filter.Property = "JobDocAddress1";
				filter.SqlComparisonOperator = SQLComparisonOperator.Equal;
				filter.IsActive = true;
				filterControl.FirePerformSearch();

				var jobAddresses = Factory.Load<JobDocAddress>(new ZQuery(JobDocAddressSchema.E2_Address1, "JobDocAddress1"));
				addressDetailControl.SetDataBinding(null, "");

				AssertNoExceptionThrown(() => addressDetailControl.SaveMultipleRecords(verifiedJobAddress, jobAddresses.ToList()));
			}
		}

		[RequiresSTA]
		public void TestShouldClearDataBindings_WhenResetControl()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				var addressView = GetMDMAddress(true);
				control.SetDataBinding(addressView, "");

				Assert("Precondition", control.CityControl.DataBindings.Count > 0);
				Assert("Precondition", control.AddressTypeControl.DataBindings.Count > 0);
				Assert("Precondition", control.AddressCodeControl.DataBindings.Count > 0);
				Assert("Precondition", control.Address1Control.DataBindings.Count > 0);
				Assert("Precondition", control.Address2Control.DataBindings.Count > 0);
				Assert("Precondition", control.PostcodeControl.DataBindings.Count > 0);
				Assert("Precondition", control.CountryControl.CodeBox.DataBindings.Count > 0);
				Assert("Precondition", control.OrganisationNameControl.DataBindings.Count > 0);
				Assert("Precondition", control.StateControl.CodeBox.DataBindings.Count > 0);
				Assert("Precondition", control.OrganisationControl.CodeBox.DataBindings.Count > 0);
				Assert("Precondition", control.AdditionalAddressInformationControl.DataBindings.Count > 0);

				control.SetUIStatus(true, false);

				Assert(control.CityControl.DataBindings.Count == 0);
				Assert(control.AddressTypeControl.DataBindings.Count == 0);
				Assert(control.AddressCodeControl.DataBindings.Count == 0);
				Assert(control.Address1Control.DataBindings.Count == 0);
				Assert(control.Address2Control.DataBindings.Count == 0);
				Assert(control.PostcodeControl.DataBindings.Count == 0);
				Assert(control.CountryControl.CodeBox.DataBindings.Count == 0);
				Assert(control.OrganisationNameControl.DataBindings.Count == 0);
				Assert(control.StateControl.CodeBox.DataBindings.Count == 0);
				Assert(control.OrganisationControl.CodeBox.DataBindings.Count == 0);
				Assert(control.AdditionalAddressInformationControl.DataBindings.Count == 0);
			}
		}

		[ExpectNoExceptions]
		public void TestShouldNotThrowNullReferenceExceptionAfterCallValidationAndCurrentAddressEntityIsNull()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				var mdmAddress1 = GetMDMAddress();
				control.BindingCompleted = true;
				control.SimulateIsOnSelectedTab = true;
				control.AddressValidationSuspended = true;
				control.SetDataBinding(mdmAddress1, "");

				form.Controls.Add(control);
				form.Show();

				control.AddressValidationSuspended = false;
				control.ClearDataBindingWhenGetWebAddressValidationResult = true;
				control.ValidateAddressAsync().GetAwaiter().GetResult();
			}
		}

		public void TestShouldNotAddNotificationWhenPhoneOrFaxIsInvalid()
		{
			var collection = new MDMAdminPanelAddressCollection(Factory);
			var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress.OA_CompanyNameOverride = "Wise Tech";
			orgAddress.OA_Address1 = "ORGADDRESS1_ADDRESS1";
			orgAddress.OA_PostCode = "210036";
			orgAddress.OA_City = "CITY";
			orgAddress.OA_State = "NSW";
			orgAddress.OA_RN_NKCountryCode = "AU";
			orgAddress.OA_Phone = "123";
			orgAddress.OA_Fax = "123";
			Factory.Save();

			var query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, orgAddress.PK);
			collection.Load(query);
			var view = collection[0];

			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.SetDataBinding(view, "");
				(control.CurrentAddressEntity as OrgAddress).OA_RN_NKCountryCode = "CN";
				control.SaveButton.PerformClick();

				AssertEquals(false, control.CurrentAddressEntity.Notifications.Any());
			}
		}

		public void TestShouldValidate()
		{
			var rawValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				using (var form = new ZForm())
				using (var control = new SingleAddressValidationControlForTest())
				{
					var auPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU")).PK;
					var mdmAddress1 = GetMDMAddress();
					var orgAddress = (OrgAddress)mdmAddress1.AddressEntity;
					orgAddress.OA_RN_NKCountryCode = "AU";
					control.BindingCompleted = true;
					control.SimulateIsOnSelectedTab = true;
					control.AddressValidationSuspended = true;
					control.SetDataBinding(mdmAddress1, "");

					form.Controls.Add(control);
					form.Show();

					using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
					{
						AssertEquals(true, control.ShouldValidateForTest);
					}

					using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(auPK, disabledForAdminPanel: true)))
					{
						AssertEquals(false, control.ShouldValidateForTest);
					}

					using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(auPK, disabledForOrgAddress: true)))
					{
						AssertEquals(true, control.ShouldValidateForTest);
					}
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawValue;
			}
		}

		public void TestSetCurrentEntityValidationSection()
		{
			var rawValue = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				using (var form = new ZForm())
				using (var control = new SingleAddressValidationControlForTest())
				{
					var mdmAddress1 = GetMDMAddress();
					var orgAddress = (OrgAddress)mdmAddress1.AddressEntity;
					AssertEquals("Precondition", false, orgAddress.IsInAdminPanel);
					AssertEquals("Precondition", AddressValidationSection.OrganizationAddress, orgAddress.ValidationSection);

					control.BindingCompleted = true;
					control.SimulateIsOnSelectedTab = true;
					control.AddressValidationSuspended = true;
					control.SetDataBinding(mdmAddress1, "");

					form.Controls.Add(control);
					form.Show();
					AssertEquals(true, orgAddress.IsInAdminPanel);
					AssertEquals(AddressValidationSection.AdminPanel, orgAddress.ValidationSection);

					var mdmAddress2 = GetMDMAddress(true);
					var docAddress = (JobDocAddress)mdmAddress2.AddressEntity;
					AssertEquals("Precondition", false, docAddress.IsInAdminPanel);
					AssertEquals("Precondition", AddressValidationSection.OverrideAddress, docAddress.ValidationSection);

					control.SetDataBinding(mdmAddress2, "");
					AssertEquals(true, docAddress.IsInAdminPanel);
					AssertEquals(AddressValidationSection.AdminPanel, docAddress.ValidationSection);
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawValue;
			}
		}

#if !WINZOR
		void SetupSelectedAddress(ListView addressesListView, ValidationResultItem selectedAddress)
		{
			addressesListView.Items.Clear();
			addressesListView.SelectedItems.Clear();
			addressesListView.Items.Add(new ListViewItem(string.Empty) { Tag = selectedAddress });
			addressesListView.Items[0].Selected = true;
		}
#endif

		void AssertOpenRecordButton()
		{
			using (var form = new ZForm())
			using (var control = new SingleAddressValidationControlForTest())
			{
				form.Controls.Add(control);
				form.Show();

				control.SimulateIsOnSelectedTab = true;
				var addressView = GetMDMAddress();
				var header = Factory.Load<OrgAddress>(addressView.PK).Header;
				control.SetDataBinding(addressView, "");

				control.OpenRecordButton.Enabled = true;
				control.OpenRecordButton.PerformClick();
				var newForm = Application.OpenForms["ZOrganisationsForm"] as ZForm;
				var org1 = newForm.BusinessEntity as OrgHeader;
				AssertEquals("org form opened", org1.PK, header.PK);
				newForm.Close();
			}
		}
	}
}
