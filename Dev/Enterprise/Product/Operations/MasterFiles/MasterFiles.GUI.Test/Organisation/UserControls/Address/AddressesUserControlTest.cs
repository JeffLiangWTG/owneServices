using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Async;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AddressesUserControl))]
	sealed class AddressesUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		[RequiresSTA]
		public void TestDetailsARAPNewNotAllowed_ClearFieldsButtonAndValidateAddressButtonBlockedForNewOrg()
		{
			AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(false, true);
		}

		public void TestDetailsARAPNewAllowed_ClearFieldsButtonAndValidateAddressButtonNotBlockedForNewOrg()
		{
			AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(true, false);
		}

		void AssertClearFieldsButtonAndValidateAddressButtonSecurityCheckedByModifyAddressDetailsAsPerCapabilities(bool detailsARAPNewAllowed, bool hasErrorMessage)
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			address.OA_OH = testHeader.PK;
			address.OA_Code = "ABC";
			testHeader.Addresses.Add(address);

			address.AddressCapability.DisableAllCapabilities();
			AssertEquals("Precondition: ", 0, address.AddressCapability.EnabledCapabilities.Count());

			address.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

			var originalOrgAddressDetailsARAPNew = Env.Security.OrgAddressDetailsARAPNew.IsAllowed;

			try
			{
				Env.Security.OrgAddressDetailsARAPNew.IsAllowed = detailsARAPNewAllowed;

				using (var form = new ZOrganisationsForm(testHeader))
				using (var testControl = new TestAddressesControl())
				{
					form.Controls.Add(testControl);
					testControl.SetDataBinding(testHeader, "");
					form.Show();
					testControl.OrgAddressBoundGrid.SelectSingleElement(address);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testControl.PerformClearFieldsClick();
					if (hasErrorMessage)
					{
						Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					}
					else
					{
						Assert("No messages are shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
					}

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					testControl.PerformValidateAddressClick();
					if (hasErrorMessage)
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
				Env.Security.OrgAddressDetailsARAPNew.IsAllowed = originalOrgAddressDetailsARAPNew;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		[RequiresSTA]
		public void TestKnownShipperTabPage_ShownForCorrectCountry()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry("US"))
			{
				OrgHeader org = Factory.New<OrgHeader>();
				org.ActiveOrAllAddresses.AddNew();

				using (ZForm form = new ZForm(org))
				{
					TestAddressesControl ctrl = new TestAddressesControl();
					form.Controls.Add(ctrl);
					ctrl.SetDataBinding(org, "");
					form.Show();

					AssertEquals(true, ctrl.ExtraDetailsTabControl.TabPages.Contains(ctrl.USKnownShipperTabPage));
					AssertEquals(false, ctrl.ExtraDetailsTabControl.TabPages.Contains(ctrl.GenericKnownShipperTabPage));
				}
			}

			AssertTabPageShowForCorrectCountry("JP", FreightDataRegistry.Instance.EnableSupplyChainSecurity_JP, "JP");
			AssertTabPageShowForCorrectCountry("HK", FreightDataRegistry.Instance.EnableSupplyChainSecurity_HK, "HK");
			AssertTabPageShowForCorrectCountry("ES", FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU, "EU");
			AssertTabPageShowForCorrectCountry("CH", FreightDataRegistry.Instance.EnableSupplyChainSecurity_EU, "EU");

			foreach (var country in new[] { "JM", "SG" })
			{
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
				{
					OrgHeader org = Factory.New<OrgHeader>();
					org.ActiveOrAllAddresses.AddNew();

					using (ZForm form = new ZForm(org))
					{
						TestAddressesControl ctrl = new TestAddressesControl();
						form.Controls.Add(ctrl);
						ctrl.SetDataBinding(org, "");
						form.Show();

						AssertEquals(false, ctrl.ExtraDetailsTabControl.TabPages.Contains(ctrl.USKnownShipperTabPage));
						AssertEquals(false, ctrl.ExtraDetailsTabControl.TabPages.Contains(ctrl.GenericKnownShipperTabPage));
					}
				}
			}
		}

		public void TestAddNewTranslateAddressDefaultCursorOnAddressLine1()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.ActiveOrAllAddresses.AddNew();

			using (ZForm form = new ZForm(org))
			{
				TestAddressesControl ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				form.Show();
				ctrl.AddLocalAddressButton_Click(this, EventArgs.Empty);
				Assert("Should default cursor on address line 1", ctrl.OA_Address1BoundTextBox_Exposed.Focused);
			}
		}

		void AssertTabPageShowForCorrectCountry(string country, BooleanRegistryItem registryItemToEnable, string expectedCaptionCountryCode)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(country))
			{
				using (registryItemToEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					var org = Factory.New<OrgHeader>();
					org.ActiveOrAllAddresses.AddNew();

					using (ZForm form = new ZForm(org))
					{
						TestAddressesControl ctrl = new TestAddressesControl();
						form.Controls.Add(ctrl);
						ctrl.SetDataBinding(org, "");
						form.Show();

						AssertEquals("Enabled via registry for " + country, true, ctrl.ExtraDetailsTabControl.TabPages.Contains(ctrl.GenericKnownShipperTabPage));
						AssertEquals("Supply Chain Security (" + expectedCaptionCountryCode + ")", ctrl.GenericKnownShipperTabPage.Text);
					}
				}

				using (registryItemToEnable.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
				{
					var org = Factory.New<OrgHeader>();
					org.ActiveOrAllAddresses.AddNew();
					using (ZForm form = new ZForm(org))
					{
						TestAddressesControl ctrl = new TestAddressesControl();
						form.Controls.Add(ctrl);
						ctrl.SetDataBinding(org, "");
						form.Show();

						AssertEquals("Disabled via registry for " + country, false, ctrl.ExtraDetailsTabControl.TabPages.Contains(ctrl.GenericKnownShipperTabPage));
					}
				}
			}
		}

		public void TestAddressCapabilityGridHorizontallyResizable()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				addressesControl.SetDataBinding(org, "");
				form.Show();
				var addressCapabilityGrid = addressesControl.Controls.Find("AddressCapability", true)[0] as ZGrid;
				AssertEquals("AddressCapability Grid is in a GroupBox", typeof(ZGroupBox), addressCapabilityGrid.Parent.GetType());
				AssertEquals("The GroupBoxis in a SplitContainer", typeof(SplitterPanel), addressCapabilityGrid.Parent.Parent.GetType());
				var splitterPanel = addressCapabilityGrid.Parent.Parent;
				var width = addressCapabilityGrid.Width;
				((KSplitContainer)splitterPanel.Parent).SplitterDistance += 20;
				AssertEquals("addressCapabilityGrid Width Changed", width - 20, addressCapabilityGrid.Width);
			}
		}

		[RequiresSTA]
		public void TestKnownShipperTabPage_WithSecurity()
		{
			Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = true;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (ZForm form = new ZForm(org))
			{
				TestAddressesControl ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				form.Show();

				AssertEquals(0, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);

				ctrl.KnownShipperTabPage_Enter(this, EventArgs.Empty);
				AssertEquals(1, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);

				ctrl.KnownShipperTabPage_Leave(this, EventArgs.Empty);
				AssertEquals(0, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);

				ctrl.KnownShipperTabPage_Enter(this, EventArgs.Empty);
				AssertEquals(1, org.MainAddress.KnownShipperDetails.Count);
				org.Addresses[0].KnownShipperDetails[0].OV_EXApprovalNumber = "hello";
				AssertEquals(true, org.HasChanges);

				ctrl.KnownShipperTabPage_Leave(this, EventArgs.Empty);
				AssertEquals(1, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(true, org.HasChanges);

				Factory.Save();
				ctrl.KnownShipperTabPage_Enter(this, EventArgs.Empty);
				AssertEquals(1, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);

				ctrl.KnownShipperTabPage_Leave(this, EventArgs.Empty);
				AssertEquals(1, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);
			}
		}

		public void TestKnownShipperTabPage_NoSecurity()
		{
			Env.Security.OrgConsignorModifyExporterScheme.IsAllowed = false;

			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			using (ZForm rorm = new ZForm(org))
			{
				TestAddressesControl ctrl = new TestAddressesControl();
				rorm.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				rorm.Show();

				AssertEquals(0, org.Addresses[0].KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);

				ctrl.KnownShipperTabPage_Enter(this, EventArgs.Empty);
				AssertEquals(0, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);

				ctrl.KnownShipperTabPage_Leave(this, EventArgs.Empty);
				AssertEquals(0, org.MainAddress.KnownShipperDetails.Count);
				AssertEquals(false, org.HasChanges);
			}
		}

		public void TestReadonlyWhenControlIsReadOnly()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(testHeader))
			using (var testControl = new TestAddressesControl())
			{
				form.Controls.Add(testControl);
				form.Show();

				var readOnlyToggleControl = testControl as IReadOnlyToggleControl;
				AssertNotNull("TestAddressesControl should implement IReadOnlyToggleControl", readOnlyToggleControl);
				AssertEquals("Precondition: User control is not readonly", false, testControl.ReadOnly);

				readOnlyToggleControl.ReadOnly = true;
				AssertEquals("User control is readonly", true, testControl.ReadOnly);
				AssertEquals("ValidateAddressButton.ReadOnly", true, testControl.ValidateAddressButton_Exposed.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestSelectLocalAddressDropEdit_WhenOrganisationModifyIsGranted_ShouldBeEditable()
		{
			AssertSelectLocalAddressDropEditEditable(true);
		}

		public void TestSelectLocalAddressDropEdit_WhenOrganisationModifyIsNotGranted_ShouldBeEditable()
		{
			AssertSelectLocalAddressDropEditEditable(false);
		}

		void AssertSelectLocalAddressDropEditEditable(bool modifyIsGranted)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Address1 = "First Address";

			Factory.Save();

			var originalModifyAllowed = Env.Security.OrganisationModify.IsAllowed;

			try
			{
				Env.Security.OrganisationModify.IsAllowed = modifyIsGranted;
				orgAddress.ReadOnly = !modifyIsGranted;

				using (var form = new ZOrganisationsForm(orgHeader))
				using (var testAddressControl = new TestAddressesControl())
				{
					form.Controls.Add(testAddressControl);
					testAddressControl.SetDataBinding(orgHeader, "");
					form.Show();

					testAddressControl.OrgAddressBoundGrid.SelectSingleElement(orgAddress);

					AssertEquals("Should be editable", true, !testAddressControl.SelectLocalAddressDropEdit_Exposed.ReadOnly);
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = originalModifyAllowed;
			}
		}

		public void TestRemoveLocalAddressButton_WhenOrganisationModifyIsGranted_ShouldBeEnabled()
		{
			AssertRemoveLocalAddressButtonEnabled(true);
		}

		public void TestRemoveLocalAddressButton_WhenOrganisationModifyIsNotGranted_ShouldBeDisabled()
		{
			AssertRemoveLocalAddressButtonEnabled(false);
		}

		void AssertRemoveLocalAddressButtonEnabled(bool canModify)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress = orgHeader.MainAddress;
			orgAddress.OA_Address1 = "First Address";

			var translatedLanguage = orgAddress.AddNewTranslatedAddress();
			translatedLanguage.OTA_Address1 = "Translated Address";

			Factory.Save();

			var originalModifyAllowed = Env.Security.OrganisationModify.IsAllowed;

			try
			{
				Env.Security.OrganisationModify.IsAllowed = canModify;

				using (var form = new ZOrganisationsForm(orgHeader))
				using (var testAddressControl = new TestAddressesControl())
				{
					form.Controls.Add(testAddressControl);
					testAddressControl.SetDataBinding(orgHeader, "");
					form.Show();
					testAddressControl.OrgAddressBoundGrid.SelectSingleElement(orgAddress);

					AssertEquals(canModify, testAddressControl.RemoveLocalButton_Exposed.Enabled);
				}
			}
			finally
			{
				Env.Security.OrganisationModify.IsAllowed = originalModifyAllowed;
			}
		}

		#region TestAddressesControl

		public class TestAddressesControl : AddressesUserControl
		{
			public TestAddressesControl()
				: base()
			{
			}

			protected override void OnLoad(EventArgs e)
			{
				base.OnLoad(e);
				this.AdditionalAddressInfoTabPage.TabVisible = true;
			}

			public ZGrid AddressesGrid
			{
				get { return OrgAddressBoundGrid; }
			}

			public bool OA_StateDropDownVisible
			{
				get { return OA_StateBoundDropEdit.Visible; }
			}

			public ZButton ValidateAddressButton_Exposed
			{
				get { return this.ValidateAddressButton; }
			}

			public ZUserControl AddressAdditionalInfoControl_Exposed
			{
				get { return this.AddressAdditionalInfoUserControl; }
			}

			public ZUserControl TranslatedAddressAdditionalInfoControl_Exposed
			{
				get { return this.TranslatedAddressAdditionalInfoUserControl; }
			}

			public ZTextBox OA_Address1BoundTextBox_Exposed
			{
				get { return this.OA_Address1BoundTextBox; }
			}

			public ZTextBox OA_Address2BoundTextBox_Exposed
			{
				get { return this.OA_Address2BoundTextBox; }
			}

			public ZTextBox OA_CityBoundTextBox_Exposed
			{
				get { return this.OA_CityBoundTextBox; }
			}

			public ZDropEdit OA_StateBoundDropEdit_Exposed
			{
				get { return this.OA_StateBoundDropEdit; }
			}

			public ZTextBox OA_PostCodeBoundTextBox_Exposed
			{
				get { return this.OA_PostCodeBoundTextBox; }
			}

			public ZTextBox OA_CompanyNameOverrideBoundTextBox_Exposed => OA_CompanyNameOverrideBoundTextBox;

			public ZDropEdit OA_LanguageBoundDropEdit_Exposed => OA_LanguageBoundDropEdit;

			public ZButton RemoveLocalButton_Exposed
			{
				get { return RemoveLocalAddressButton_Exposed; }
			}

			public ZCodeFindBox RelatedPortBox
			{
				get { return RelatedPortFindBox; }
			}

			public CharacterCasing AllowedCharacterCasingForControl(string controlName)
			{
				CharacterCasing casing = CharacterCasing.Lower;
				foreach (Control ctrl in AddressDetailsGroupBox.Controls)
				{
					if (ctrl.Name == controlName && ctrl is ZTextBox textBox)
					{
						casing = textBox.CharacterCasing;
					}
				}
				return casing;
			}

			public ZLabel CustomsAddressSecurityLabelForTest
			{
				get { return CustomsAddressSecurityLabel; }
			}

			public void FireAddressesUserControl_Resize()
			{
				AddressesUserControl_Resize(null, null);
			}

			protected override void AddressesUserControl_Resize(object sender, EventArgs e)
			{
				var suggestionControl = SupportWebAddressValidationControlHelper.FindAddressSuggestionControl(this);
				if (suggestionControl != null && suggestionControl.Visible)
				{
					resizeAddressSuggestionControl = true;
				}
			}
			public bool resizeAddressSuggestionControl;

			public async Task GetCityTownAsyncForTest()
			{
				await GetCityTownAsync();
			}

			public void PerformValidateAddressClick()
			{
				ValidateAddressButton_Click(this, null);
			}

			public void PerformClearFieldsClick()
			{
				ClearFieldsButton_Click(this, null);
			}

			public ZGrid TimetableGridForTest => TimetableGrid;
			public ZRadioButton DefaultRadioButtonForTest => DefaultRadioButton;
			public ZRadioButton WeekdayRadioButtonForTest => WeekdayRadioButton;
			public ZRadioButton AdvancedRadioButtonForTest => AdvancedRadioButton;
			public ZRadioButton NotApplicableRadioButtonForTest => NotApplicableRadioButton;
			public ZDropEdit SelectLocalAddressDropEdit_Exposed => SelectLocalAddressDropEdit;
		}

		#endregion

		public void TestOA_StateVisibility()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.ActiveOrAllAddresses.AddNew();

			using (ZForm form = new ZForm(org))
			{
				TestAddressesControl ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				form.Show();

				org.OH_RL_NKClosestPort = "SGSIN";
				Assert("OA_StateBoundDropEdit.Visible = true", ctrl.OA_StateDropDownVisible);

				org.OH_RL_NKClosestPort = "AUSYD";
				Assert("OA_StateBoundDropEdit.Visible = true", ctrl.OA_StateDropDownVisible);
			}
		}

		[RequiresSTA]
		public void TestDefaultTimeTable_ShouldNoError()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.MainAddress;
			address.Address1 = "Test 1";
			Factory.Save();

			var orgPK = org.PK;
			var sql =
				@$"UPDATE OrgHeader SET OH_IsValid = 0 WHERE OH_PK = '{orgPK}'";
			TestConnection.ExecuteNonQuery(sql);

			var reloadOrg = Factory.Load<OrgHeader>(orgPK);
			using (ZForm form = new ZForm(reloadOrg))
			{
				var ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(reloadOrg, "");
				form.Show();

				var address1TextBox = ctrl.OA_Address1BoundTextBox_Exposed;
				address1TextBox.Focus();
				address1TextBox.Text = "Test 2";

				form.ValidateAndSave();
				Assert("default timeTable should have no error", !reloadOrg.MainAddress.Timetables.HasErrors());
			}
		}

		public void TestOA_StateVisibility_AdditionalAddresses()
		{
			OrgHeader org = Factory.New<OrgHeader>();

			OrgAddress addressMain = org.ActiveOrAllAddresses.AddNew();
			addressMain.AddressCapability.SetIsMainAddress(OrgAddressType.Postal);
			addressMain.OA_RL_NKRelatedPortCode = "SGSIN";

			OrgAddress addressAdditional = org.ActiveOrAllAddresses.AddNew();
			addressAdditional.OA_RL_NKRelatedPortCode = "AUSYD";

			using (ZForm form = new ZForm(org))
			{
				TestAddressesControl ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				form.Show();

				//Select a cell from the main address for edit
				ctrl.AddressesGrid.CurrentCell = new DataGridCell(1, 0);
				AssertEquals("SGSIN", ((OrgAddress)ctrl.AddressesGrid.ListManager.GetCurrent()).OA_RL_NKRelatedPortCode);
				Assert("OA_StateBoundDropEdit.Visible = true", ctrl.OA_StateDropDownVisible);

				//Select a cell from the additional address edit
				ctrl.AddressesGrid.CurrentCell = new DataGridCell(2, 0);
				AssertEquals("AUSYD", ((OrgAddress)ctrl.AddressesGrid.ListManager.GetCurrent()).OA_RL_NKRelatedPortCode);
				Assert("OA_StateBoundDropEdit.Visible = true", ctrl.OA_StateDropDownVisible);
			}
		}

		public void TestAllowedCharacterCasing()
		{
			Env.Registry.SetOrgAllowMixedCase(false);
			using (TestAddressesControl ctrl = new TestAddressesControl())
			{
				AssertEquals("OA_Address1BoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("OA_Address1BoundTextBox"));
				AssertEquals("OA_Address2BoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("OA_Address2BoundTextBox"));
				AssertEquals("OA_CityBoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("OA_CityBoundTextBox"));
				AssertEquals("OA_CompanyNameOverrideBoundTextBox.CharacterCasing", CharacterCasing.Upper, ctrl.AllowedCharacterCasingForControl("OA_CompanyNameOverrideBoundTextBox"));
				AssertEquals("OA_EmailTextBox.CharacterCasing (always normal)", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OA_EmailTextBox"));
			}

			Env.Registry.SetOrgAllowMixedCase(true);
			using (TestAddressesControl ctrl = new TestAddressesControl())
			{
				AssertEquals("OA_Address2BoundTextBox.CharacterCasing", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OA_Address2BoundTextBox"));
				AssertEquals("OA_CityBoundTextBox.CharacterCasing", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OA_CityBoundTextBox"));
				AssertEquals("OA_CompanyNameOverrideBoundTextBox.CharacterCasing", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OA_CompanyNameOverrideBoundTextBox"));
				AssertEquals("OA_EmailTextBox.CharacterCasing (always normal)", CharacterCasing.Normal, ctrl.AllowedCharacterCasingForControl("OA_EmailTextBox"));
			}
		}

		public void TestCustomsAddressSecurity()
		{
			var euCustomsAddressExistingSecurity = Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed;
			Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed = false;

			var customsAddressExistingSecurity = Env.Security.OrgAddressCustomsAddressModify.IsAllowed;
			Env.Security.OrgAddressCustomsAddressModify.IsAllowed = false;

			try
			{
				var organization = Factory.NewWithValidTestData<OrgHeader>();
				organization.MainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.EUCustomsAddress);
				organization.MainAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.CustomsAddressOfRecord);
				Factory.Save();

				using (var form = new ZForm(organization))
				{
					var addressControl = new TestAddressesControl();
					form.Controls.Add(addressControl);
					addressControl.SetDataBinding(organization, "");
					form.Show();

					addressControl.AddressesGrid.ListManager.Position = 0;
					Application.DoEvents();
					AssertEquals("CustomsAddress Security Label should be visible if not have security access to modify Customs Address", true, addressControl.CustomsAddressSecurityLabelForTest.Visible);
					AssertEquals("Do not have security access to modify Customs Address.", "You do not have security access to modify Customs Address.", addressControl.CustomsAddressSecurityLabelForTest.CaptionResourceString.Caption);
				}

				Env.Security.OrgAddressCustomsAddressModify.IsAllowed = true;
				using (var form = new ZForm(organization))
				{
					var addressControl = new TestAddressesControl();
					form.Controls.Add(addressControl);
					addressControl.SetDataBinding(organization, "");
					form.Show();

					addressControl.AddressesGrid.ListManager.Position = 0;
					Application.DoEvents();
					AssertEquals("CustomsAddress Security Label should be visible if not have security access to modify EU Customs Address", true, addressControl.CustomsAddressSecurityLabelForTest.Visible);
					AssertEquals("Do not have security access to modify EU Customs Address.", "You do not have security access to modify EU Customs Address.", addressControl.CustomsAddressSecurityLabelForTest.CaptionResourceString.Caption);
				}

				Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed = true;
				using (var form = new ZForm(organization))
				{
					var addressControl = new TestAddressesControl();
					form.Controls.Add(addressControl);
					addressControl.SetDataBinding(organization, "");
					form.Show();

					addressControl.AddressesGrid.ListManager.Position = 0;
					Application.DoEvents();
					AssertEquals("CustomsAddress Security Label should be invisible", false, addressControl.CustomsAddressSecurityLabelForTest.Visible);
				}
			}
			finally
			{
				Env.Security.OrgAddressEUCustomsAddressModify.IsAllowed = euCustomsAddressExistingSecurity;
				Env.Security.OrgAddressCustomsAddressModify.IsAllowed = customsAddressExistingSecurity;
			}
		}

		public void TestTSAKnownAddressSecurity()
		{
			var existingSecurity = Env.Security.OrgAddressTSAKnownAddressModify.IsAllowed;

			try
			{
				Env.Security.OrgAddressTSAKnownAddressModify.IsAllowed = false;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "ABC";
				org.OH_FullName = "Test Org";
				org.MainAddress.OA_Address1 = "Address 1";
				org.MainAddress.OA_RN_NKCountryCode = "AU";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.MainAddress.Postcode = "2000";
				org.MainAddress.State = "NSW";
				org.MainAddress.City = "Sydney";
				org.OH_IsConsignee = true;

				var mainAddress = org.MainAddress;
				var orgCountryDataTSARecord = Factory.New<OrgCountryData>();
				orgCountryDataTSARecord.OV_OA_ApprovedLocation = mainAddress.PK;
				orgCountryDataTSARecord.OV_EXApprovedOrMajorExporter = "Yes";
				orgCountryDataTSARecord.OV_EXApprovalNumber = "1234";
				orgCountryDataTSARecord.OV_RN_NKClientCountryRelation = Core.Constants.CountryCodes.UnitedStates;
				orgCountryDataTSARecord.OV_OH_OrgHeader = org.PK;

				Factory.Save();

				using (var form = new ZForm(org))
				{
					var addressControl = new TestAddressesControl();
					form.Controls.Add(addressControl);
					addressControl.SetDataBinding(org, "");
					form.Show();

					Application.DoEvents();
					AssertEquals("Address 1 Control should be readonly", true, addressControl.Address1Control.ReadOnly);
					AssertEquals("Address 2 Control should be readonly", true, addressControl.Address2Control.ReadOnly);
					AssertEquals("City Control should be readonly", true, addressControl.CityControl.ReadOnly);
					AssertEquals("State Control should be readonly", true, addressControl.StateControl.ReadOnly);
					AssertEquals("Postcode Control should be readonly", true, addressControl.PostcodeControl.ReadOnly);
					AssertEquals("Country Control should be readonly", true, addressControl.CountryControl.ReadOnly);
				}
			}
			finally
			{
				Env.Security.OrgAddressTSAKnownAddressModify.IsAllowed = existingSecurity;
			}
		}

		public void TestTimeTables_AreInitialised_OnLoad()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_Code = "ABC";
			org.OH_FullName = "Test Org";
			org.MainAddress.OA_Address1 = "Address 1";
			org.MainAddress.OA_RN_NKCountryCode = "AU";
			org.OH_RL_NKClosestPort = "AUSYD";
			org.MainAddress.Postcode = "2000";
			org.MainAddress.State = "NSW";
			org.MainAddress.City = "Sydney";
			org.OH_IsConsignee = true;

			Factory.Save();

			using (var form = new ZForm(org))
			{
				var addressControl = new TestAddressesControl();
				form.Controls.Add(addressControl);
				addressControl.SetDataBinding(org, "");
				form.Show();

				Application.DoEvents();
				AssertEquals(1, addressControl.AddressesGrid.ListManager.Count);

				CombineAssertions(() =>
				{
					AssertEquals(false, org.HasChanges);
				});
			}
		}

		[RequiresSTA]
		public void TestMIDAddressSecurity()
		{
			var existingSecurity = Env.Security.OrgAddressMIDAddressModify.IsAllowed;

			try
			{
				Env.Security.OrgAddressMIDAddressModify.IsAllowed = false;

				var org = Factory.NewWithValidTestData<OrgHeader>();
				org.OH_Code = "ABC";
				org.OH_FullName = "Test Org";
				org.MainAddress.OA_Address1 = "Address 1";
				org.MainAddress.OA_RN_NKCountryCode = "AU";
				org.OH_RL_NKClosestPort = "AUSYD";
				org.MainAddress.Postcode = "2000";
				org.MainAddress.State = "NSW";
				org.MainAddress.City = "Sydney";
				org.OH_IsConsignee = true;

				var mainAddress = org.MainAddress;
				var orgCusCodeMIDRecord = Factory.New<OrgCusCode>();
				orgCusCodeMIDRecord.OK_OA_PremisesAddress = mainAddress.PK;
				orgCusCodeMIDRecord.OK_CodeType = "MID";
				orgCusCodeMIDRecord.OK_CustomsRegNo = "abcd1234";
				orgCusCodeMIDRecord.OK_RN_NKCodeCountry = Core.Constants.CountryCodes.UnitedStates;
				orgCusCodeMIDRecord.OK_OH = org.PK;

				Factory.Save();

				using (var form = new ZForm(org))
				{
					var addressControl = new TestAddressesControl();
					form.Controls.Add(addressControl);
					addressControl.SetDataBinding(org, "");
					form.Show();

					Application.DoEvents();
					AssertEquals("Address 1 Control should be readonly", true, addressControl.Address1Control.ReadOnly);
					AssertEquals("Address 2 Control should be readonly", true, addressControl.Address2Control.ReadOnly);
					AssertEquals("City Control should be readonly", true, addressControl.CityControl.ReadOnly);
					AssertEquals("State Control should be readonly", true, addressControl.StateControl.ReadOnly);
					AssertEquals("Postcode Control should be readonly", true, addressControl.PostcodeControl.ReadOnly);
					AssertEquals("Country Control should be readonly", true, addressControl.CountryControl.ReadOnly);
				}
			}
			finally
			{
				Env.Security.OrgAddressMIDAddressModify.IsAllowed = existingSecurity;
			}
		}

		public void TestDeleteAddressAssociatedWithMID()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();

			var address1 = orgHeader.Addresses.AddNew();
			address1.Address1 = "TestAddress1";
			address1.OA_Code = "TestAddressCode1";

			var address2 = orgHeader.Addresses.AddNew();
			address2.Address1 = "TestAddress2";
			address2.OA_Code = "TestAddressCode2";

			var orgCusCode = orgHeader.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ManufacturerID, "TESTCusRegNo", Core.Constants.CountryCodes.UnitedStates);
			orgCusCode.OK_OA_PremisesAddress = address1.PK;
			Factory.Save();

			var orgHeaderDb = Factory.Load<OrgHeader>(orgHeader.PK);
			AssertEquals(3, orgHeaderDb.Addresses.Count);
			AssertEquals(1, orgHeaderDb.CustomsCodes.Count);

			using (var form = new ZOrganisationsForm(orgHeader))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				form.Show();
				form.OrganisationsTabControl.SelectedTab = form.AddressesTabPage;
				var grid = form.AddressesPageControl2.OrgAddressBoundGrid;
				var index = ((OrgHeader)grid.DataSource).ActiveOrAllAddresses.Cast<OrgAddress>().IndexOf(t => t.PK == address1.PK);
				typeof(ZGrid).GetMethod("HandleDelete", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(grid, new object[] { index });

				AssertEquals(string.Format("The address(es): {0} are associated with Registration Numbers / Codes. If you delete the address(es), the associated premises address will be set to empty. Are you sure to delete the address(es)?", "TestAddress1"), UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertNoExceptionThrown(Factory.Save);

				var orgHeaderDbNew = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(2, orgHeaderDbNew.Addresses.Count);
				AssertEquals(1, orgHeaderDbNew.CustomsCodes.Count);
				AssertEquals(ZGuid.Empty, orgHeaderDbNew.CustomsCodes[0].OK_OA_PremisesAddress);

				var index2 = ((OrgHeader)grid.DataSource).ActiveOrAllAddresses.Cast<OrgAddress>().IndexOf(t => t.PK == address2.PK);
				typeof(ZGrid).GetMethod("HandleDelete", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(grid, new object[] { index2 });

				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNoExceptionThrown(Factory.Save);

				var orgHeaderNew = Factory.Load<OrgHeader>(orgHeader.PK);
				AssertEquals(1, orgHeaderNew.Addresses.Count);
				AssertEquals(1, orgHeaderNew.CustomsCodes.Count);
			}
		}

		public void TestCheckShortCodeUniquenessCorrectly_WhenCurrentlyEditedNewAddressIsNotYesInAddressesList()
		{
			var org = Factory.New<OrgHeader>();

			using (var form = new ZForm(org))
			{
				var oa_address1Text = new ZString("Somewhere only we know");
				var notUniqueShortCodeError = "The Address Short Code must be unique for each Address (whether active or inactive) within an organization.";

				var ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				form.Show();

				//Default Main Address
				ctrl.AddressesGrid.CurrentCell = new DataGridCell(0, 0);
				ctrl.AddressesGrid[ctrl.AddressesGrid.CurrentCell] = oa_address1Text;

				AssertEquals("Addresses list shoud contain 1 address", 1, org.Addresses.Count);
				AssertEquals("Addresses and ActiveOrAllAddresses lists shoud contain the same number of addresses", org.Addresses.Count, org.ActiveOrAllAddresses.Count);

				//Edit a new address, should not hit Addresses list straight away
				ctrl.AddressesGrid.CurrentCell = new DataGridCell(1, 0);
				ctrl.AddressesGrid[ctrl.AddressesGrid.CurrentCell] = oa_address1Text;

				AssertEquals("Addresses list shoud still contains 1 address", 1, org.Addresses.Count);
				AssertEquals("ActiveOrAllAddresses should contain 2 addresses", 2, org.ActiveOrAllAddresses.Count);

				AssertEquals("OA_Code should have been made unique", oa_address1Text + "1", org.ActiveOrAllAddresses[1].OA_Code);
				AssertNoError(org.ActiveOrAllAddresses[1].OA_CodeInfo, notUniqueShortCodeError);

				ctrl.AddressesGrid.CurrentCell = new DataGridCell(1, 1);
				ctrl.AddressesGrid[ctrl.AddressesGrid.CurrentCell] = oa_address1Text;
				AssertHasError(org.ActiveOrAllAddresses[1].OA_CodeInfo, notUniqueShortCodeError);

				ctrl.AddressesGrid[new DataGridCell(0, 1)] = (ZString)"Another place";
				AssertNoError(org.ActiveOrAllAddresses[1].OA_CodeInfo, notUniqueShortCodeError);
			}
		}

		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new AddressesUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyAddress", "IsModifyConsignorExporterScheme", "IsModifyAddressCapabilities", "IsModifyAddressCapabilitiesARAP", "IsModifyAddressCapabilitiesNonARAP" }; }
		}

		[RequiresSTA]
		public void TestAddAndRemoveFromMainForm()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			{
				using (var docAddress = new AddressesUserControl())
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
		public void TestRefreshValidationStatus()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm(testHeader))
			using (var testControl = new TestAddressesControl())
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

				AssertEquals(validGreenColor, testControl.OA_Address1BoundTextBox_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.OA_Address2BoundTextBox_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.OA_CityBoundTextBox_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.OA_PostCodeBoundTextBox_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.OA_StateBoundDropEdit_Exposed.CodeBox.BackColor);
				AssertEquals(validGreenColor, testControl.CountryControl.CodeBox.BackColor);
				AssertEquals(true, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(true, testControl.ClearAddressFieldsButton.Visible);

				var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(us.PK, disabledForOrgAddress: true));
				testHeader.MainAddress.OA_RN_NKCountryCode = "US";
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.OA_Address1BoundTextBox_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.OA_Address2BoundTextBox_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.OA_CityBoundTextBox_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.OA_PostCodeBoundTextBox_Exposed.BackColor);
				AssertEquals(Color.FromArgb(255, 215, 215), testControl.OA_StateBoundDropEdit_Exposed.CodeBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CountryControl.CodeBox.BackColor);
				AssertEquals(false, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(false, testControl.ClearAddressFieldsButton.Visible);

				Env.Instance.Registry.EnableAddressValidationWebService = false;
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.OA_Address1BoundTextBox_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.OA_Address2BoundTextBox_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.OA_CityBoundTextBox_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.OA_PostCodeBoundTextBox_Exposed.BackColor);
				AssertEquals(Color.FromArgb(255, 215, 215), testControl.OA_StateBoundDropEdit_Exposed.CodeBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CountryControl.CodeBox.BackColor);
				AssertEquals(false, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(false, testControl.ClearAddressFieldsButton.Visible);
			}
		}

		public void TestAdditionalAddressInfoTabUserControlVisibility()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = Factory.NewWithValidTestData<OrgAddress>();
			org.ActiveOrAllAddresses.Add(address);

			using (var form = new ZForm(org))
			{
				var ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, string.Empty);
				form.Show();

				var tabPage = (ZTabPage)form.Controls.Find("AdditionalAddressInfoTabPage", true).FirstOrDefault();
				tabPage.Show();

				Assert(ctrl.AddressAdditionalInfoControl_Exposed.Visible);
				Assert(!ctrl.TranslatedAddressAdditionalInfoControl_Exposed.Visible);

				ctrl.AddLocalAddressButton_Click(this, EventArgs.Empty);

				Assert(!ctrl.AddressAdditionalInfoControl_Exposed.Visible);
				Assert(ctrl.TranslatedAddressAdditionalInfoControl_Exposed.Visible);
			}
		}

		[RequiresSTA]
		public void TestTabIndicesInRightOrder()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(testHeader))
			using (var testControl = new TestAddressesControl())
			{
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals(testControl.OA_StateBoundDropEdit_Exposed.TabIndex - testControl.OA_CityBoundTextBox_Exposed.TabIndex, 1);
				AssertEquals(testControl.OA_CityBoundTextBox_Exposed.TabIndex - testControl.OA_PostCodeBoundTextBox_Exposed.TabIndex, 1);
			}
		}

		public void TestColumnsCanBeDisplayed()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new ZForm(testHeader))
			using (var testControl = new TestAddressesControl())
			{
				form.Controls.Add(testControl);
				form.Show();

				var columns = testControl.AddressesGrid.ColumnStyles;

				CombineAssertions(() =>
				{
					AssertHasColumn(columns, "OA_Code");
					AssertHasColumn(columns, "OA_Address1");
					AssertHasColumn(columns, "OA_Address2");
					AssertHasColumn(columns, "OA_City");
					AssertHasColumn(columns, "OA_State");
					AssertHasColumn(columns, "OA_PostCode");
					AssertHasColumn(columns, "OA_Language");
					AssertHasColumn(columns, "OA_IsActive");
					AssertHasColumn(columns, "OA_CompanyNameOverride");
				});
			}
		}

		void AssertHasColumn(ArrayList columns, string nameOfColumn)
		{
			var anyColumnHasGivenName = columns.Cast<ZGridColumnInfo>().Any(column => column.ColumnName == nameOfColumn);

			Assert("Should have the column - " + nameOfColumn, anyColumnHasGivenName);
		}

		public void TestUpdateControlsEnabledStatusWithNullCandidates()
		{
			using (var testControl = new TestAddressesControl())
			{
				testControl.OrgAddressBoundGrid.IsListManagerNotNull = false;
				AssertNoExceptionThrown(testControl.UpdateControlsEnabledStatus);
				AssertEquals(testControl.RemoveLocalButton_Exposed.Enabled, false);
				AssertEquals(testControl.CountryControl.Enabled, true);
				AssertEquals(testControl.RelatedPortBox.Enabled, true);
			}
		}

		public void TestExtraDetailsTabControl_WithProductivityWiseEnabled_ShouldBeHidden()
		{
			void AssertExtraDetailsTabControlVisibility(bool expected)
			{
				using (var form = new Form { Size = ControlDpiScalingHelper.NewScaledSize(1000, 1000) })
				using (var addressControl = new TestAddressesControl())
				{
					form.Controls.Add(addressControl);
					form.Show();
					Application.DoEvents();

					AssertEquals(expected, addressControl.ExtraDetailsTabControl.Visible);
				}
			}

			AssertExtraDetailsTabControlVisibility(true);

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertExtraDetailsTabControlVisibility(false);
		}

		[RequiresSTA]
		public void TestProductivityWiseMode_ShouldNotLeaveEmptySpaceWhereExtraDetailsTabControlWas()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			int originalGroupBoxWidth;
			int tabControlWidth;

			using (var form = new Form { Size = ControlDpiScalingHelper.NewScaledSize(1000, 1000) })
			using (var addressControl = new TestAddressesControl())
			{
				form.Controls.Add(addressControl);
				form.Show();
				Application.DoEvents();

				var groupBox = addressControl.FindSingle<GroupBox>("AddressDetailsGroupBox");
				originalGroupBoxWidth = groupBox.Width;
				tabControlWidth = addressControl.ExtraDetailsTabControl.Width;
			}

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;

			using (var form = new Form { Size = ControlDpiScalingHelper.NewScaledSize(1000, 1000) })
			using (var addressControl = new TestAddressesControl())
			{
				form.Controls.Add(addressControl);
				form.Show();
				Application.DoEvents();

				var groupBox = addressControl.FindSingle<GroupBox>("AddressDetailsGroupBox");

				AssertEquals("The group box should have grown to consume the area previously occupied by the tab control. SAD!", originalGroupBoxWidth + tabControlWidth, groupBox.Width);
			}
		}

		public void TestContextMenuShowsCombineWhenTwoAddressesAreSelected()
		{
			const string menuItemName = "Combine Addresses";
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = org.Addresses[0];
			var addressB = Factory.NewWithValidTestData<OrgAddress>();
			var addressC = Factory.NewWithValidTestData<OrgAddress>();
			addressB.OA_Code = "#Test1";
			addressB.OA_OH = org.PK;
			addressC.OA_Code = "#Test2";
			addressC.OA_OH = org.PK;
			org.Addresses.Add(addressB);
			org.Addresses.Add(addressC);

			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				Application.DoEvents();
				AssertEquals(3, addressesControl.AddressesGrid.ListManager.Count);
				var addressesGrid = addressesControl.AddressesGrid;
				AssertEquals(0, addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().Count(m => m.Text == menuItemName));
				addressesGrid.Select(0);
				addressesGrid.Select(1);
				addressesGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(2, addressesGrid.SelectedElements.Length);
				AssertEquals("Combine menu item should not load when addresses not in DB", 0, addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().Count(m => m.Text == menuItemName));
				Factory.Save();
				addressesGrid.Select(0);
				addressesGrid.Select(1);
				addressesGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Combine menu item should not load when addresses are the same language", 0, addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().Count(m => m.Text == menuItemName));
				addressA.Language = Core.Constants.Languages.ChineseSimplified;
				addressesGrid.Select(0);
				addressesGrid.Select(1);
				addressesGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Combine menu item should load when addresses are different languages and in DB", 1, addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().Count(m => m.Text == menuItemName));
				addressesGrid.PerformMouseDownForTest(0, 1);
				AssertEquals("Combine menu item should still only show once after subsequent clicks", 1, addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().Count(m => m.Text == menuItemName));
				addressesGrid.SelectAllElements();
				addressesGrid.PerformMouseDownForTest(0, 1);
				AssertEquals(3, addressesGrid.SelectedElements.Length);
				AssertEquals(0, addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().Count(m => m.Caption == menuItemName));
			}
		}

		public void TestCombineAddressesMenuShowErrorMessageWhenUserHaveNoSecurity()
		{
			const string menuItemName = "Combine Addresses";
			var rawSecurityValue = Env.Security.OrgAddressCombineAddresses.IsAllowed;
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = org.Addresses[0];
			addressA.Language = Core.Constants.Languages.ChineseSimplified;
			var addressB = Factory.NewWithValidTestData<OrgAddress>();
			addressB.OA_Code = "#Test1";
			addressB.OA_OH = org.PK;
			org.Addresses.Add(addressB);
			Factory.Save();

			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				Application.DoEvents();
				AssertEquals(2, addressesControl.AddressesGrid.ListManager.Count);
				var addressesGrid = addressesControl.AddressesGrid;
				AssertEquals(0, addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().Count(m => m.Text == menuItemName));

				try
				{
					Env.Security.OrgAddressCombineAddresses.IsAllowed = false;
					UnitTestUserNotification.Instance.ClearMessages();
					addressesGrid.Select(0);
					addressesGrid.Select(1);
					addressesGrid.PerformMouseDownForTest(0, 1);
					var menuCombine = addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == menuItemName);
					AssertEquals(2, addressesGrid.SelectedElements.Length);
					AssertNotNull(menuCombine);

					menuCombine.PerformClick();
					var addressCombinerForm = (CombineTranslatedAddressForm)ZFormModaliser.ActiveForm;
					AssertEquals("Should contain error message", Env.Security.OrgAddressCombineAddresses.ErrorMessageForNotAllowed, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNull("Should not show CombineTranslatedAddressForm", addressCombinerForm);

					UnitTestUserNotification.Instance.ClearMessages();
					Env.Security.OrgAddressCombineAddresses.IsAllowed = true;
					addressesGrid.PerformMouseDownForTest(0, 1);
					menuCombine = addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == menuItemName);
					AssertNotNull(menuCombine);

					menuCombine.PerformClick();
					addressCombinerForm = (CombineTranslatedAddressForm)ZFormModaliser.ActiveForm;
					AssertNull("Should not contain error message", UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotNull("Should show CombineTranslatedAddressForm", addressCombinerForm);

					addressCombinerForm.Close();
				}
				finally
				{
					Env.Security.OrgAddressCombineAddresses.IsAllowed = rawSecurityValue;
				}
			}
		}

		public void TestCombineAddressFormLoadsFromContextMenu()
		{
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			var addressA = org.Addresses[0];
			var addressB = Factory.NewWithValidTestData<OrgAddress>();
			var addressC = Factory.NewWithValidTestData<OrgAddress>();
			addressA.Language = Core.Constants.Languages.ChineseSimplified;
			addressB.OA_Code = "#Test1";
			addressB.OA_OH = org.PK;
			addressC.OA_Code = "#Test2";
			addressC.OA_OH = org.PK;
			org.Addresses.Add(addressB);
			org.Addresses.Add(addressC);
			Factory.Save();
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				Application.DoEvents();
				AssertEquals(3, addressesControl.AddressesGrid.ListManager.Count);
				var addressesGrid = addressesControl.AddressesGrid;
				addressesGrid.Select(0);
				addressesGrid.Select(1);
				addressesGrid.PerformMouseDownForTest(0, 1);
				var combineMenuItem = addressesGrid.ContextMenu.MenuItems.Cast<ZMenuItem>().FirstOrDefault(m => m.Text == "Combine Addresses");
				combineMenuItem.PerformClick();
				AssertType(typeof(CombineTranslatedAddressForm), ZFormModaliser.LastFormShownForTest);
			}
		}

		[RequiresSTA]
		public void TestTextBox_WhenLanguageIsChinese_ShouldBeYaheiFont()
		{
			var org = Factory.New<OrgHeader>();
			var toBeFixedControls = new string[] { "OrgAddressBoundGrid", "OA_CompanyNameOverrideBoundTextBox", "OA_Address1BoundTextBox", "OA_Address2BoundTextBox", "OA_CityBoundTextBox",
				"OA_LoadingUnloadingConstraintsTextBox", "OA_OtherWarehouseFacilitiesTextBox", "AddressAdditionalInfoUserControl", "TranslatedAddressAdditionalInfoUserControl" };
			using (var form = new ZForm(org))
			{
				var ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				form.Show();

				toBeFixedControls.ForEach(control =>
				{
					var font = ((Control)typeof(AddressesUserControl).GetField(control, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ctrl)).Font;
					AssertNotEquals(AddressesUserControl.YaheiFontName, font.Name);
					AssertEquals(ctrl.OA_LanguageBoundDropEdit_Exposed.Font, font);
				});

				org.MainAddress.Language = Core.SharedConstants.Languages.ChineseSimplified;
				UserIdleWorker.Flush();

				toBeFixedControls.ForEach(control =>
				{
					var font = ((Control)typeof(AddressesUserControl).GetField(control, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ctrl)).Font;
					AssertEquals(AddressesUserControl.YaheiFontName, font.Name);
				});

				org.MainAddress.Language = Core.SharedConstants.Languages.English;
				UserIdleWorker.Flush();

				toBeFixedControls.ForEach(control =>
				{
					var font = ((Control)typeof(AddressesUserControl).GetField(control, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ctrl)).Font;
					AssertNotEquals(AddressesUserControl.YaheiFontName, font.Name);
					AssertEquals(ctrl.OA_LanguageBoundDropEdit_Exposed.Font, font);
				});
			}
		}

		[DeveloperOnlyTest]
		public void TestCopyAddressForAnalysis()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.ActiveOrAllAddresses.RemoveAndDeleteAll();
			var orgAddress1 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress1.OA_Address1 = "Address 1";
			orgAddress1.OA_Address2 = "Address 2";
			orgAddress1.OA_City = "Sydney";
			orgAddress1.OA_PostCode = "botany";
			orgAddress1.OA_State = "NSW";
			orgAddress1.OA_RN_NKCountryCode = "AU";
			org.ActiveOrAllAddresses.Add(orgAddress1);
			var orgAddress2 = Factory.NewWithValidTestData<OrgAddress>();
			orgAddress2.OA_Address1 = "Address 3";
			orgAddress2.OA_Address2 = "Address 4";
			orgAddress2.OA_City = "Murburne";
			orgAddress2.OA_PostCode = "M1234";
			orgAddress2.OA_State = "cwa";
			orgAddress2.OA_RN_NKCountryCode = "AU";
			org.ActiveOrAllAddresses.Add(orgAddress2);
			Factory.Save();

			var expectContents = new string[2]
			{
				@"InputAddress1: Address 1
InputAddress2: Address 2
InputCity: Sydney
InputPostcode: botany
InputState: NSW
InputCountryCode: AU",
				@"InputAddress1: Address 3
InputAddress2: Address 4
InputCity: Murburne
InputPostcode: M1234
InputState: cwa
InputCountryCode: AU"
			};

			using (var form = new ZForm(org))
			{
				var ctrl = new TestAddressesControl();
				form.Controls.Add(ctrl);
				ctrl.SetDataBinding(org, "");
				form.Show();

				var menuitem = ctrl.OrgAddressBoundGrid.ContextMenu.MenuItems.FindByName("Copy Address for Analysis");
				AssertNotNull(menuitem);

				var addressGrid = ctrl.OrgAddressBoundGrid;
				addressGrid.SelectSingleElementByPK(orgAddress1.PK);
				SafeClipboard.Clear();
				menuitem.PerformClick();
				var content = SafeClipboard.GetText();
				AssertMultilineASCIIEquals(expectContents[0], content);

				addressGrid.SelectSingleElementByPK(orgAddress2.PK);
				SafeClipboard.Clear();
				menuitem.PerformClick();
				content = SafeClipboard.GetText();
				AssertMultilineASCIIEquals(expectContents[1], content);
			}
		}

		public void TestCopyAddressForAnalysisMenuItemIndexAndMultiLanguage()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Enterprise.Core.Constants.CountryCodes.China))
			{
				using (var ctrl = new TestAddressesControl())
				{
					var menuitem = ctrl.OrgAddressBoundGrid.ContextMenu.MenuItems.FindByName("Copy Address for Analysis");
					var deleteMenuItem = ctrl.OrgAddressBoundGrid.DeleteMenuItem;

					AssertEquals(deleteMenuItem.Index + 1, menuitem.Index);
				}
			}
		}

		#region TimeTables

		public void TestAdvancedRadioButtonIsCheckedWhenLoaded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				Assert(addressesControl.AdvancedRadioButtonForTest.Checked);
				Assert(!addressesControl.WeekdayRadioButtonForTest.Checked);
				Assert(!addressesControl.DefaultRadioButtonForTest.Checked);
				Assert(!addressesControl.NotApplicableRadioButtonForTest.Checked);
				Assert(!addressesControl.TimetableGridForTest.ReadOnly);
				AssertEquals("Column 'DayOfWeek' is available.", 1, addressesControl.TimetableGridForTest.Columns.Count(x => x.ColumnName == "DayOfWeek"));
			}
		}

		public void TestWeekdayRadioButtonIsCheckedWhenLoaded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateWeekdayTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				Assert(addressesControl.WeekdayRadioButtonForTest.Checked);
				Assert(!addressesControl.AdvancedRadioButtonForTest.Checked);
				Assert(!addressesControl.DefaultRadioButtonForTest.Checked);
				Assert(!addressesControl.NotApplicableRadioButtonForTest.Checked);
				Assert(!addressesControl.TimetableGridForTest.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestDefaultRadioButtonIsCheckedWhenLoaded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				Assert(!addressesControl.AdvancedRadioButtonForTest.Checked);
				Assert(!addressesControl.NotApplicableRadioButtonForTest.Checked);
				Assert(addressesControl.DefaultRadioButtonForTest.Checked);
				Assert(!addressesControl.WeekdayRadioButtonForTest.Checked);
				Assert(addressesControl.TimetableGridForTest.ReadOnly);
				AssertEquals("Column 'DayOfWeek' is available.", 1, addressesControl.TimetableGridForTest.Columns.Count(x => x.ColumnName == "DayOfWeek"));
			}
		}

		public void TestNotApplicableRadioButtonIsCheckedWhenLoaded()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateNotApplicableTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				Assert(!addressesControl.AdvancedRadioButtonForTest.Checked);
				Assert(!addressesControl.DefaultRadioButtonForTest.Checked);
				Assert(!addressesControl.WeekdayRadioButtonForTest.Checked);
				Assert(addressesControl.NotApplicableRadioButtonForTest.Checked);
				Assert(addressesControl.TimetableGridForTest.ReadOnly);
			}
		}

		public void TestDayOfWeekColumnIsHiddenWhenWeekdayRadioButtonIsChecked()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				AssertEquals("Precondition", 1, addressesControl.TimetableGridForTest.Columns.Count(x => x.ColumnName == "DayOfWeek"));
				addressesControl.WeekdayRadioButtonForTest.Checked = true;
				AssertEquals("Column 'DayOfWeek' is hidden.", 0, addressesControl.TimetableGridForTest.Columns.Count(x => x.ColumnName == "DayOfWeek"));
			}
		}

		public void TestDontShowWarningDialogWhenSwitchRadioButtonWithNoChanges()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");

				Assert("Precondition", !addressesControl.DefaultRadioButtonForTest.Checked);
				Assert("Precondition", addressesControl.AdvancedRadioButtonForTest.Checked);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				addressesControl.DefaultRadioButtonForTest.Checked = true;
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDontShowWarningDialogWhenSwitchRadioButtonWithNoChangesWhenChangingFromAdvancedToWeekday()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");

				Assert("Precondition", !addressesControl.WeekdayRadioButtonForTest.Checked);
				Assert("Precondition", addressesControl.AdvancedRadioButtonForTest.Checked);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				addressesControl.WeekdayRadioButtonForTest.Checked = true;
				AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Changed the radio button since there were no changes", addressesControl.WeekdayRadioButtonForTest.Checked);
				Assert("Changed the radio button since there were no changes", !addressesControl.AdvancedRadioButtonForTest.Checked);
			}
		}

		public void TestShowWarningDialogWithAnswerYes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				orgTimeTableCollection[0].OTT_Type = OrgTimetableType.Codes.Deliver;

				Assert("Precondition", !addressesControl.DefaultRadioButtonForTest.Checked);
				Assert("Precondition", addressesControl.AdvancedRadioButtonForTest.Checked);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				addressesControl.DefaultRadioButtonForTest.Checked = true;

				AssertEquals(@"By resetting to Default Pickup and Delivery Times all Pickup and Delivery Times entered against other settings will be lost.
Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(addressesControl.DefaultRadioButtonForTest.Checked);
			}
		}

		[RequiresSTA]
		public void TestShowWarningDialogIfAttemptingToChangeFromAdvancedToWeekday_PerformTheChangeIfTheUserAnswersYes()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				orgTimeTableCollection[0].OTT_Type = OrgTimetableType.Codes.Deliver;

				Assert("Precondition", !addressesControl.WeekdayRadioButtonForTest.Checked);
				Assert("Precondition", addressesControl.AdvancedRadioButtonForTest.Checked);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);

				addressesControl.WeekdayRadioButtonForTest.Checked = true;

				AssertEquals(@"By resetting to Weekday Pickup and Delivery Times all Pickup and Delivery Times entered against other settings will be lost.
Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert("Switched to weekday because the user was happy to lose changes", !addressesControl.AdvancedRadioButtonForTest.Checked);
				Assert("Switched to weekday because the user was happy to lose changes", addressesControl.WeekdayRadioButtonForTest.Checked);
			}
		}

		public void TestCanNotDeleteLastPickupOrDeliveryTime()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");

				AssertEquals("Precondition", 12, orgTimeTableCollection.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				address.Timetables.DeleteAll();

				AssertEquals(1, address.Timetables.Count);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Delete Pickup and Delivery Times", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("The last Pickup or Delivery Time can not be deleted!", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestShowWarningDialogWithAnswerNo()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses[0];
			var orgTimeTableCollection = new OrgTimetableCollection(address);
			CreateAdvancedTimeTable(address, orgTimeTableCollection);
			using (var form = new ZForm(org))
			using (var addressesControl = new TestAddressesControl())
			{
				form.Controls.Add(addressesControl);
				form.Show();
				addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
				orgTimeTableCollection[0].OTT_Type = OrgTimetableType.Codes.Deliver;

				Assert("Precondition", !addressesControl.DefaultRadioButtonForTest.Checked);
				Assert("Precondition", addressesControl.AdvancedRadioButtonForTest.Checked);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				addressesControl.DefaultRadioButtonForTest.Checked = true;

				AssertEquals(@"By resetting to Default Pickup and Delivery Times all Pickup and Delivery Times entered against other settings will be lost.
Are you sure you want to continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				Assert(!addressesControl.DefaultRadioButtonForTest.Checked);
				Assert(addressesControl.AdvancedRadioButtonForTest.Checked);
			}
		}

		public void TestSetRadioButtonEnabledCorrectlyAfterAddressItemChanged()
		{
			AssertSetRadioButtonEnabledCorrectlyAfterAddressItemChanged(Env.Security.OrgAddressAdditionalDetailsNew);
			AssertSetRadioButtonEnabledCorrectlyAfterAddressItemChanged(Env.Security.OrgAddressAdditionalDetailsModify);
		}

		public void TestSetRadioButtonEnabledCorrectlyAfterTimetableGridBind()
		{
			AssertSetRadioButtonEnabledCorrectlyAfterTimetableGridBind(Env.Security.OrgAddressAdditionalDetailsNew);
			AssertSetRadioButtonEnabledCorrectlyAfterTimetableGridBind(Env.Security.OrgAddressAdditionalDetailsModify);
		}

		public void TestAddressesUserControl_Resize()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new TestAddressesControl())
			{
				form.Controls.Add(testControl);
				form.Show();

				var addressItems = new List<ValidationResultItem>();
				var topRecommendedAddress = AddressSuggestionControlTest.CreateAddressItem("Add10", "Add20", "City0", "AU", "", "P0", "NSW");
				addressItems.Add(topRecommendedAddress);

				using (var suggestionControl = new AddressSuggestionControl(Factory.NewWithValidTestData<OrgAddress>(), addressItems, topRecommendedAddress, form, testControl, testControl.ValidateButton))
				{
					testControl.Controls.Add(suggestionControl);
					testControl.Show();

					suggestionControl.Hide();
					testControl.FireAddressesUserControl_Resize();
					AssertEquals(false, testControl.resizeAddressSuggestionControl);

					suggestionControl.Show();
					testControl.FireAddressesUserControl_Resize();
					AssertEquals(true, testControl.resizeAddressSuggestionControl);
				}
			}
		}

		public void TestCityTownSuggestionControlWillHideWhenSwitchTab()
		{
			ObjectFactory.Substitute<TaskScheduler>(new SynchronousTaskSchedulerForTest());

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			testHeader.MainAddress.City = "TestCity";
			testHeader.MainAddress.Postcode = "21000";

			var cityTown = new CandidateCityTown
			{
				City = "Sydney",
				Postcode = "2000",
				State = "NSW",
			};

			CityTownSuggestionControlHelper.FakeResult = new CandidateCityTown[] { cityTown, cityTown };

			using (var form = new ZOrganisationsForm(testHeader))
			using (var testControl = new TestAddressesControl())
			{
				form.OrganisationsTabControl.TabPages[2].Controls.Add(testControl);
				form.OrganisationsTabControl.SelectedIndex = 2;
				form.Show();

				testControl.GetCityTownAsyncForTest().GetAwaiter().GetResult();
				var suggestionControl = form.Controls.Find(CityTownSuggestionControlHelper.CityTownSuggestionControlName, true).FirstOrDefault();

				AssertNotNull(suggestionControl);
				Assert(suggestionControl.Visible);

				form.OrganisationsTabControl.SelectedIndex = 1;
				Assert("Hide when parent control lost focus.", !suggestionControl.Visible);
			}
		}

		#region Implementation

		void AssertSetRadioButtonEnabledCorrectlyAfterAddressItemChanged(SecurityCheckpoint checkpoint)
		{
			var oldCheckPointValue = checkpoint.IsAllowed;

			try
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var address1 = org.Addresses[0];
				address1.OA_Address1 = "TEST1";
				var orgTimeTableCollection1 = new OrgTimetableCollection(address1);

				var address2 = org.Addresses.AddNew();
				address2.OA_Address1 = "TEST2";
				var orgTimeTableCollection2 = new OrgTimetableCollection(address2);
				CreateAdvancedTimeTable(address2, orgTimeTableCollection2, false);

				if (checkpoint.Equals(Env.Security.OrgAddressAdditionalDetailsModify))
				{
					Factory.Save();
				}

				checkpoint.IsAllowed = false;
				using (var form = new ZForm(org))
				using (var addressesControl = new TestAddressesControl())
				{
					form.Controls.Add(addressesControl);
					form.Show();

					addressesControl.AddressesGrid.SelectSingleElementByPK(address2.PK);
					addressesControl.AddressesGrid.SelectSingleElementByPK(address1.PK);
					Assert(!addressesControl.DefaultRadioButtonForTest.Enabled);
					Assert(!addressesControl.AdvancedRadioButtonForTest.Enabled);
					Assert(!addressesControl.NotApplicableRadioButtonForTest.Enabled);
					Assert(!addressesControl.WeekdayRadioButtonForTest.Enabled);
					Assert(addressesControl.DefaultRadioButtonForTest.Checked);

					addressesControl.AddressesGrid.SelectSingleElementByPK(address2.PK);
					Assert(!addressesControl.DefaultRadioButtonForTest.Enabled);
					Assert(!addressesControl.AdvancedRadioButtonForTest.Enabled);
					Assert(!addressesControl.NotApplicableRadioButtonForTest.Enabled);
					Assert(!addressesControl.WeekdayRadioButtonForTest.Enabled);
					Assert(addressesControl.AdvancedRadioButtonForTest.Checked);
				}

				checkpoint.IsAllowed = true;
				using (var form = new ZForm(org))
				using (var addressesControl = new TestAddressesControl())
				{
					form.Controls.Add(addressesControl);
					form.Show();

					addressesControl.AddressesGrid.SelectSingleElementByPK(address2.PK);
					addressesControl.AddressesGrid.SelectSingleElementByPK(address1.PK);
					Assert(addressesControl.DefaultRadioButtonForTest.Enabled);
					Assert(addressesControl.AdvancedRadioButtonForTest.Enabled);
					Assert(addressesControl.NotApplicableRadioButtonForTest.Enabled);
					Assert(addressesControl.WeekdayRadioButtonForTest.Enabled);
					Assert(addressesControl.DefaultRadioButtonForTest.Checked);

					addressesControl.AddressesGrid.SelectSingleElementByPK(address2.PK);
					Assert(addressesControl.DefaultRadioButtonForTest.Enabled);
					Assert(addressesControl.AdvancedRadioButtonForTest.Enabled);
					Assert(addressesControl.NotApplicableRadioButtonForTest.Enabled);
					Assert(addressesControl.WeekdayRadioButtonForTest.Enabled);
					Assert(addressesControl.AdvancedRadioButtonForTest.Checked);
				}
			}
			finally
			{
				checkpoint.IsAllowed = oldCheckPointValue;
			}
		}

		void AssertSetRadioButtonEnabledCorrectlyAfterTimetableGridBind(SecurityCheckpoint checkpoint)
		{
			var oldCheckPointValue = checkpoint.IsAllowed;

			try
			{
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var address = org.Addresses[0];
				var orgTimeTableCollection = new OrgTimetableCollection(address);

				if (checkpoint.Equals(Env.Security.OrgAddressAdditionalDetailsModify))
				{
					Factory.Save();
				}

				checkpoint.IsAllowed = false;
				using (var form = new ZForm(org))
				using (var addressesControl = new TestAddressesControl())
				{
					form.Controls.Add(addressesControl);
					form.Show();

					addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
					Assert(!addressesControl.DefaultRadioButtonForTest.Enabled);
					Assert(!addressesControl.WeekdayRadioButtonForTest.Enabled);
					Assert(!addressesControl.AdvancedRadioButtonForTest.Enabled);
					Assert(!addressesControl.NotApplicableRadioButtonForTest.Enabled);
				}

				checkpoint.IsAllowed = true;
				using (var form = new ZForm(org))
				using (var addressesControl = new TestAddressesControl())
				{
					form.Controls.Add(addressesControl);
					form.Show();

					addressesControl.TimetableGridForTest.SetDataBinding(orgTimeTableCollection, "");
					Assert(addressesControl.DefaultRadioButtonForTest.Enabled);
					Assert(addressesControl.WeekdayRadioButtonForTest.Enabled);
					Assert(addressesControl.AdvancedRadioButtonForTest.Enabled);
					Assert(addressesControl.NotApplicableRadioButtonForTest.Enabled);
				}
			}
			finally
			{
				checkpoint.IsAllowed = oldCheckPointValue;
			}
		}

		void CreateAdvancedTimeTable(OrgAddress address, OrgTimetableCollection orgTimeTableCollection, bool factorySave = true)
		{
			address.SetTimetablesRangeType(OrgTimeTableRangeType.Advanced);
			var t1 = Factory.New<OrgTimetable>();
			t1.OTT_OA = address.PK;
			t1.OTT_Type = OrgTimetableType.Codes.Pickup;
			t1.OTT_Monday = true;
			t1.OTT_Tuesday = false;
			t1.OTT_Wednesday = false;
			t1.OTT_Thursday = false;
			t1.OTT_Friday = false;
			t1.OTT_Saturday = false;
			t1.OTT_Sunday = false;
			t1.OTT_TimeFrom = new DateTime(2015, 1, 1, 8, 0, 0);
			t1.OTT_TimeTo = new DateTime(2015, 1, 1, 9, 0, 0);
			orgTimeTableCollection.Add(t1);

			if (factorySave)
			{
				Factory.Save();
			}
		}

		void CreateWeekdayTimeTable(OrgAddress address, OrgTimetableCollection orgTimeTableCollection)
		{
			address.SetTimetablesRangeType(OrgTimeTableRangeType.Weekday);
			var tt = Factory.New<OrgTimetable>();
			tt.OTT_OA = address.PK;
			tt.OTT_Type = OrgTimetableType.Codes.Pickup;
			tt.OTT_IsForAllWeekDays = true;
			tt.OTT_TimeFrom = new DateTime(2015, 1, 1, 8, 0, 0);
			tt.OTT_TimeTo = new DateTime(2015, 1, 1, 9, 0, 0);
			orgTimeTableCollection.Add(tt);
			Factory.Save();
		}

		void CreateNotApplicableTimeTable(OrgAddress address, OrgTimetableCollection orgTimeTableCollection, bool factorySave = true)
		{
			address.SetTimetablesRangeType(OrgTimeTableRangeType.NotApplicable);
			var t1 = Factory.New<OrgTimetable>();
			using (t1.GetValidationSuspender())
			{
				t1.OTT_OA = address.PK;
				t1.OTT_Type = OrgTimetableType.Codes.Pickup;
				t1.OTT_Monday = false;
				t1.OTT_Tuesday = false;
				t1.OTT_Wednesday = false;
				t1.OTT_Thursday = false;
				t1.OTT_Friday = false;
				t1.OTT_Saturday = false;
				t1.OTT_Sunday = false;
			}
			orgTimeTableCollection.Add(t1);

			var t2 = Factory.New<OrgTimetable>();
			using (t2.GetValidationSuspender())
			{
				t2.OTT_OA = address.PK;
				t2.OTT_Type = OrgTimetableType.Codes.Pickup;
				t2.OTT_Monday = false;
				t2.OTT_Tuesday = false;
				t2.OTT_Wednesday = false;
				t2.OTT_Thursday = false;
				t2.OTT_Friday = false;
				t2.OTT_Saturday = false;
				t2.OTT_Sunday = false;
			}
			orgTimeTableCollection.Add(t2);

			if (factorySave)
			{
				Factory.Save();
			}
		}

		#endregion

		#endregion
	}
}
