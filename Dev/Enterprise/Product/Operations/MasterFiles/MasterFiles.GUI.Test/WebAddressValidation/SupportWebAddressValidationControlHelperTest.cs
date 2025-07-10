using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.WebAddressValidation.Testing
{
	sealed class SupportWebAddressValidationControlHelperTest : TestCaseWithFactory
	{
		[RequiresSTA]
		public void TestClearFields()
		{
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.GeoLocation = ZGeography.CreatePoint(12, 21);
			AssertNotEquals("Percondition", 0d, address.GeoLocation.Latitude);
			AssertNotEquals("Percondition", 0d, address.GeoLocation.Longitude);

			using (var form = new ZForm())
			using (var addressesControl = new AddressesUserControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();

				SupportWebAddressValidationControlHelper.ClearFields(address, addressesControl);
				AssertNull(address.GeoLocation.Latitude);
				AssertNull(address.GeoLocation.Longitude);
			}
		}

		public void TestShowSuggestionControlsUponGotFocus()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new AddressesUserControl())
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
					var preLocation = suggestionControl.Location;
					var preWitdth = suggestionControl.Width;

					suggestionControl.Hide();
					SupportWebAddressValidationControlHelper.ShowSuggestionControlsUponGotFocus(null, testHeader.MainAddress, testControl, 100, 20);

					AssertEquals("There are no changes of X location.", preLocation.X, suggestionControl.Location.X);
					AssertNotEquals("There are changes of Y location.", preLocation.Y, suggestionControl.Location.Y);
					AssertNotEquals("There are changes of wdith.", preWitdth, suggestionControl.Width);
				}
			}
		}

		public void TestForceCommitCurrentChanges()
		{
			var oldValue = Env.Security.OrgAddressesAllowedManualVerification.IsAllowed;
			try
			{
				Env.Security.OrgAddressesAllowedManualVerification.IsAllowed = true;
				var keyData = Keys.Control | Keys.M;
				using (var form = new ZForm())
				using (var parentControl = new AddressUserControl(new AdministrationPanelManager(new BusinessObjectFactory())))
				using (var control = new SingleAddressValidationControl(parentControl.Manager))
				{
					control.ParentControl = parentControl;
					form.Controls.Add(control);
					form.Show();

					var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
					orgAddress.OA_Address1 = "ORGADDRESS1_ADDRESS1";
					orgAddress.OA_Address2 = "ORGADDRESS1_ADDRESS2";
					orgAddress.OA_PostCode = "210036";
					orgAddress.OA_City = "CITY";
					orgAddress.OA_State = "NSW";
					orgAddress.OA_RN_NKCountryCode = "AU";
					orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
					orgAddress.OA_ValidationStatus = AddressValidationStatus.Invalid;
					Factory.Save();

					var collection = new MDMAdminPanelAddressCollection(Factory);
					var query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, orgAddress.PK);
					collection.Load(query);
					control.BindingCompleted = true;
					control.SetDataBinding(collection[0], "");
					var address = ((control.CurrentDataItem as MDMAdminPanelAddressView).AddressEntity) as OrgAddress;

					control.Enabled = true;
					control.AddressCodeControl.Focus();
					control.AddressCodeControl.Text = "ORGADDRESS1_ADDRESS1_TEST";
					AssertEquals("Precondition", "ORGADDRESS1_ADDRESS1", address.AddressCode);
					SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, false, control, address);
					AssertEquals("ORGADDRESS1_ADDRESS1_TEST", address.AddressCode);

					control.AdditionalAddressInformationControl.Focus();
					control.AdditionalAddressInformationControl.Text = "ADDITIONAL ADDRESS_TEST";
					AssertEquals("Precondition", "ADDITIONAL ADDRESS", address.UnrestrictedAdditionalAddressInformation);
					SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, false, control, address);
					AssertEquals("ADDITIONAL ADDRESS_TEST", address.UnrestrictedAdditionalAddressInformation);

					control.CountryControl.CodeBox.Focus();
					control.CountryControl.CodeBox.Text = "CN";
					AssertEquals("Precondition", "AU", address.OA_RN_NKCountryCode);
					SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, false, control, address);
					AssertEquals("CN", address.OA_RN_NKCountryCode);
				}
			}
			finally
			{
				Env.Security.OrgAddressesAllowedManualVerification.IsAllowed = oldValue;
			}
		}

		public void TestProcessCommandKeyManualAddressValidationNoSecurity()
		{
			var oldValue = Env.Security.OrgAddressesAllowedManualVerification.IsAllowed;
			try
			{
				Env.Security.OrgAddressesAllowedManualVerification.IsAllowed = false;
				var keyData = Keys.Control | Keys.M;
				using (var form = new ZForm())
				using (var parentControl = new AddressUserControl(new AdministrationPanelManager(new BusinessObjectFactory())))
				using (var control = new SingleAddressValidationControl(parentControl.Manager))
				{
					control.ParentControl = parentControl;
					form.Controls.Add(control);
					form.Show();

					var orgAddress = Factory.NewWithValidTestData<OrgAddress>();
					orgAddress.OA_Address1 = "ORGADDRESS1_ADDRESS1";
					orgAddress.OA_Address2 = "ORGADDRESS1_ADDRESS2";
					orgAddress.OA_PostCode = "210036";
					orgAddress.OA_City = "CITY";
					orgAddress.OA_State = "NSW";
					orgAddress.OA_RN_NKCountryCode = "AU";
					orgAddress.PrimaryOrgAddressAdditionalInfoDetail = "ADDITIONAL ADDRESS";
					orgAddress.OA_ValidationStatus = AddressValidationStatus.Invalid;
					Factory.Save();

					var collection = new MDMAdminPanelAddressCollection(Factory);
					var query = new ZQuery(MDMAdminPanelAddressViewSchema.PK, orgAddress.PK);
					collection.Load(query);
					control.BindingCompleted = true;
					control.SetDataBinding(collection[0], "");
					var address = ((control.CurrentDataItem as MDMAdminPanelAddressView).AddressEntity) as OrgAddress;

					control.Enabled = true;
					control.AddressCodeControl.Focus();
					control.AddressCodeControl.Text = "ORGADDRESS1_ADDRESS1_TEST";
					AssertEquals("Precondition", "ORGADDRESS1_ADDRESS1", address.AddressCode);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

					SupportWebAddressValidationControlHelper.ProcessCommandKey(keyData, false, control, address);
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
	}
}
