using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.Security;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Balloons;
using Enterprise.ZArchitecture.GUI.Controls.Extensions;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class ZDocAddressControlTest : TestCaseWithDummy
	{
		public void TestAddressFormatter()
		{
			using (var control = new ZDocAddressControlForTest())
			{
				AssertNull("Base DocAddressControl should not use any Address Formatter", control.AddressFormatter);
			}
		}

		[RequiresSTA]
		public void TestTriggerWebGetCityTownDetachedOnControlDisposed()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var docAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			docAddress.E2_AddressOverride = true;

			var num = 0;

			using (var form = new ZForm(orgAddress))
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var control = new ZDocAddressControl())
			{
				tabPage.Controls.Add(control);
				tabControl.TabPages.Add(tabPage);
				form.Controls.Add(tabControl);
				control.SetDataBinding(docAddress, "");
				form.Show();

				docAddress.TriggerWebGetCityTown += (sender, e) => { ++num; };
				CombineAssertions("Precondition: ", () =>
				{
					AssertEquals(0, num);
					AssertNotEquals("Canberra", docAddress.City);
				});
			}

			docAddress.E2_City = "Canberra";
			AssertEquals("The anonymous method to increase num by 1 is unsubscribed from TriggerWebGetCityTown event", 0, num);
		}

		[RequiresSTA]
		public void TestTriggerWebAddressValidationDetachedOnControlDisposed()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var docAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			docAddress.E2_AddressOverride = true;

			var num = 0;

			using (var form = new ZForm(orgAddress))
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var control = new ZDocAddressControl())
			{
				tabPage.Controls.Add(control);
				tabControl.TabPages.Add(tabPage);
				form.Controls.Add(tabControl);
				control.SetDataBinding(docAddress, "");
				form.Show();

				docAddress.TriggerWebAddressValidation += (sender, e) => { ++num; };
				CombineAssertions("Precondition: ", () =>
				{
					AssertEquals(0, num);
					AssertNotEquals("ABC DEF", docAddress.E2_Address1);
				});
			}

			docAddress.E2_Address1 = "ABC DEF";
			AssertEquals("The anonymous method to increase num by 1 is unsubscribed from TriggerWebAddressValidation event", 0, num);
		}

		[RequiresSTA]
		public void TestErrorIconShowsOnTabWhenAddressValidationStatusHasErrors()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var docAddress = Factory.New<JobDocAddress>();
			var orgAddress = Factory.New<OrgAddress>();
			docAddress.E2_OA_Address = orgAddress.PK;
			docAddress.E2_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			docAddress.E2_AddressOverride = true;
			using (var form = new ZForm(orgAddress))
			using (var tabControl = new ZTabControl())
			using (var tabPage = new ZTabPage())
			using (var control = new ZDocAddressControl())
			{
				tabPage.Controls.Add(control);
				tabControl.TabPages.Add(tabPage);
				form.Controls.Add(tabControl);
				control.SetDataBinding(docAddress, "");
				form.Show();
				Application.DoEvents();

				AssertEquals("No notifications on tab", -1, tabPage.ImageIndex);
				docAddress.E2_ValidationStatus = AddressValidationStatus.Invalid;
				form.FireValidateAllForTest();
				Application.DoEvents();
				AssertEquals("Error showing on tab", Icons.GetImageIndex(IconTypes.Error), tabPage.ImageIndex);

				docAddress.E2_ValidationStatus = AddressValidationStatus.ManuallyVerified;
				Application.DoEvents();
				AssertEquals("Error gone, no notifications again", -1, tabPage.ImageIndex);
			}
		}

		public void TestNotThrowExceptionWhenClickAddressValidationStatusButton()
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			var org = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			address.E2_OA_Address = orgAddress.PK;

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			using (var control = new ZDocAddressControl())
			{
				control.SetDataBinding(address, "");
				form.Controls.Add(control);
				form.Show();

				AssertNoExceptionThrown("Should not throw exception", control.AddressValidationStatusButton.PerformClick);

				using (var autoOpenedForm = OpenedFormCache.GetInstance().GetForm(org.PK.ToGuid(), ControllerIDs.Organisation.Name))
				{
					AssertNotNull("Should not throw exception", autoOpenedForm);

					autoOpenedForm.Close();
				}

				var controller = ZControllerFactory.Create(ControllerIDs.Organisation);

				using (var manualOpenedForm = ((IOrganisationController)controller).ShowForm(address.Organisation, OrganisationTabPages.Details, FormAction.Edit))
				{
					var manualOpenedOrgForm = manualOpenedForm as ZOrganisationsForm;

					AssertNotNull("Should display the OrgForm", manualOpenedOrgForm);
					AssertNoExceptionThrown("Should not throw exception", control.AddressValidationStatusButton.PerformClick);

					manualOpenedOrgForm.Close();
				}
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestAutoUpdateValidationStatusAfterDataItemChanged()
		{
			var collection = new ActiveBusinessObjectCollection<JobDocAddress>(Factory);
			var address1 = collection.AddNew();
			var address2 = collection.AddNew();
			var org1 = Factory.New<OrgHeader>();
			var org2 = Factory.New<OrgHeader>();

			var orgAddress1 = Factory.New<OrgAddress>();
			orgAddress1.ValidationStatus = AddressValidationStatus.Verified;
			var orgAddress2 = Factory.New<OrgAddress>();
			orgAddress2.ValidationStatus = AddressValidationStatus.ToBeVerified;
			orgAddress1.OA_OH = org1.PK;
			orgAddress2.OA_OH = org2.PK;
			address1.E2_OA_Address = orgAddress1.PK;
			address2.E2_OA_Address = orgAddress2.PK;
			address1.E2_RN_NKCountryCode = address2.E2_RN_NKCountryCode = "AU";
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm(collection))
			using (var control = new ZDocAddressControl())
			{
				control.SetDataBinding(collection, "");
				form.Controls.Add(control);
				form.Show();

				control.SetBindingManagerCurrent("", address1);
				Assert("Address valid", control.AddressValidationStatusButton.ToolTipCaption.Equals("This address is verified."));

				control.SetBindingManagerCurrent("", address2);
				Assert("Address invalid", control.AddressValidationStatusButton.ToolTipCaption.Equals("This address needs to be verified."));
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestAddressValidationButtonVisible()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			using (var form = new ZForm())
			using (var control = new ZDocAddressControl())
			{
				var address = Factory.NewWithValidTestData<JobDocAddress>();
				var org = Factory.NewWithValidTestData<OrgHeader>();

				var orgAddress = Factory.New<OrgAddress>();
				orgAddress.OA_OH = org.PK;
				address.E2_OA_Address = orgAddress.PK;
				address.E2_RN_NKCountryCode = "AU";
				address.E2_AddressOverride = false;
				control.SetDataBinding(address, "");
				form.Controls.Add(control);
				form.Show();
				var validateAddressButton = control.Controls.Find("ValidateAddressButton", true)[0] as ZButton;
				Assert("AddressValidationStatusButton is for non-overridden addresses, should be visible", control.AddressValidationStatusButton.Visible);
				Assert("ValidateAddressButton is for overridden addresses, hide it", !validateAddressButton.Visible);
				address.E2_AddressOverride = true;
				Assert("AddressValidationStatusButton is for non-overridden addresses, hide it", !control.AddressValidationStatusButton.Visible);
				Assert("ValidateAddressButton is for overridden addresses and display mode Default, should be visible", validateAddressButton.Visible);
				control.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;
				Assert("ValidateAddressButton is for overridden addresses and display mode CompactWithContactTab, should be visible", validateAddressButton.Visible);
				control.DisplayMode = ZDocAddressControlDisplayMode.CompactWithOverride;
				Assert("ValidateAddressButton is for overridden addresses and display mode CompactWithOverride, hide it", !validateAddressButton.Visible);
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestConvertToOrganizationButtonVisible()
		{
			using (var form = new ZForm())
			using (var control = new ZDocAddressControl())
			{
				var address = Factory.NewWithValidTestData<JobDocAddress>();
				var org = Factory.NewWithValidTestData<OrgHeader>();
				var orgAddress = Factory.New<OrgAddress>();
				orgAddress.OA_OH = org.PK;
				address.E2_OA_Address = orgAddress.PK;
				address.E2_RN_NKCountryCode = "AU";
				address.E2_AddressOverride = false;
				control.SetDataBinding(address, "");
				form.Controls.Add(control);
				form.Show();

				var convertToOrganizationButton = control.Controls.Find("convertToOrganizationButton", true)[0] as ZButton;
				Assert("convertToOrganizationButton is for overridden addresses, hide it", !convertToOrganizationButton.Visible);
				address.E2_AddressOverride = true;
				Assert("convertToOrganizationButton is for overridden addresses, should be visible", convertToOrganizationButton.Visible);
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestOverrideCheckBox()
		{
			using (var form = new ZForm())
			using (var control = new ZDocAddressControl())
			{
				var address = Factory.NewWithValidTestData<JobDocAddress>();
				address.E2_AddressOverride = false;
				Assert("Test row should not be detached", ((INeedRow)address).Row.RowState != DataRowState.Detached);

				control.SetDataBinding(address, "");
				form.Controls.Add(control);
				form.Show();
				var addressOverrideCheckBox = control.Controls.Find("OverrideAddressCheckbox", true)[0] as ZCheckBox;

				addressOverrideCheckBox.Checked = true;
				Assert("address should be overridden", address.E2_AddressOverride);
				addressOverrideCheckBox.Checked = false;
				Assert("address should not be overridden", !address.E2_AddressOverride);
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestValidateAddress_WhenAddressOverrideCheckBoxIsSelected_CancellationTokenIsNotNull()
		{
			var address = Factory.NewWithValidTestData<JobDocAddress>();
			var org = Factory.NewWithValidTestData<OrgHeader>();

			var orgAddress = Factory.New<OrgAddress>();
			orgAddress.OA_OH = org.PK;
			address.E2_OA_Address = orgAddress.PK;
			address.E2_RN_NKCountryCode = "AU";
			address.E2_AddressOverride = false;

			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			using (var control = new ZDocAddressControl())
			{
				control.SetDataBinding(address, "");
				form.Controls.Add(control);
				form.Show();

				var property = control.GetType().GetProperty("AddressValidationTokenSource");
				AssertNull("AddressValidationTokenSource should no exist", property);

				var addressOverrideCheckBox = control.Controls.Find("OverrideAddressCheckbox", true)[0] as ZCheckBox;

				Assert("Precondition: addressOverrideCheckBox is not checked", !addressOverrideCheckBox.Checked);

				address.E2_AddressOverride = true;
				Assert("addressOverrideCheckBox is checked", addressOverrideCheckBox.Checked);

				var validateAddressButton = control.Controls.Find("ValidateAddressButton", true)[0] as ZButton;
				Assert("validateAddressButton is visible", validateAddressButton.Visible);
				AssertNotNull("cancellationToken should not be null", control.cancellationToken);
			}
			ExceptionReporterTestListener.Instance.Clear();
		}

		public void TestAddressTypeDropBox()
		{
			var dummy = Factory.New<DummyWithDocAddress>();

			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				var dropEdit = form.DocAddressControl.AddressTypeDropEditForTest;
				AssertNotNull("Should render ZDropEditControl", form);
				AssertNotNull("Should render ZDropEdit", dropEdit);
				AssertEquals("AddressTypeDropEdit shouldn't be Visible", false, dropEdit.Visible);
				AssertEquals("AddressTypeDropEdit caption should be 'Type'", "Type", dropEdit.CaptionResourceString.Caption);
			}
		}

		public void TestAddressTypeDropBoxVisibility()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				var dropEdit = form.DocAddressControl.AddressTypeDropEditForTest;
				dummy.DocAddress.E2_AddressOverride = true;
				Application.DoEvents();
				AssertEquals(false, dropEdit.Visible);

				form.DocAddressControl.ShowResidentialAddressOnOverride = true;
				dummy.DocAddress.E2_AddressOverride = false;
				dummy.DocAddress.E2_AddressOverride = true;
				Application.DoEvents();
				AssertEquals(true, dropEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestSingleLineNoGroupBoxPanelWidth()
		{
			using (var control = new ZDocAddressControl())
			{
				control.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(296), control.Width);
				AssertEquals("Precondition", 30, control.CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(201), control.CutDownSingleLineNoGroupBoxAddressDropEdit.Width);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(296), control.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Width);
				AssertEquals("Precondition CodeBox", ControlDpiScalingHelper.ScaleToCurrentDpiX(180), control.CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width);
				AssertEquals("Precondition Address Drop Button", ControlDpiScalingHelper.ScaleToCurrentDpiX(201), control.CutDownSingleLineNoGroupBoxAddressDropEdit.AddressDropButton.Width);

				control.ShowCompanyName = true;
				control.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(296), control.Width);
				AssertEquals("Precondition", control.CutDownSingleLineNoGroupBoxOrgFindBox.Size, ControlDpiScalingHelper.NewScaledSize(200, 20));
				AssertEquals("PreCondition", 13, control.CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(119), control.CutDownSingleLineNoGroupBoxAddressDropEdit.Width);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(296), control.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Width);
				AssertEquals("Precondition CodeBox", ControlDpiScalingHelper.ScaleToCurrentDpiX(99), control.CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width);
				AssertEquals("Precondition Address Drop Button", ControlDpiScalingHelper.ScaleToCurrentDpiX(119), control.CutDownSingleLineNoGroupBoxAddressDropEdit.AddressDropButton.Width);

				control.ShowCompanyName = false;
				control.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				control.SingleLineNoGroupBoxPanelWidth = 266;
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(266), control.Width);
				AssertEquals(0, control.CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(88), control.CutDownSingleLineNoGroupBoxAddressDropEdit.Width);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(266), control.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Width);
				AssertEquals("CodeBox", ControlDpiScalingHelper.ScaleToCurrentDpiX(68), control.CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width);
				AssertEquals("Address Drop Button", ControlDpiScalingHelper.ScaleToCurrentDpiX(88), control.CutDownSingleLineNoGroupBoxAddressDropEdit.AddressDropButton.Width);
			}

			using (var control = new ZDocAddressControl())
			{
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(ZDocAddressControl.defaultControlWidth), control.Width);
				AssertEquals("Precondition", 30, control.CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(201), control.CutDownSingleLineNoGroupBoxAddressDropEdit.Width);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(296), control.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Width);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(180), control.CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width);
				AssertEquals("Precondition", ControlDpiScalingHelper.ScaleToCurrentDpiX(201), control.CutDownSingleLineNoGroupBoxAddressDropEdit.AddressDropButton.Width);

				// Should Only Change if mode is 'SingleLineNoOverrideNoGroupBox'
				control.SingleLineNoGroupBoxPanelWidth = 100;
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(ZDocAddressControl.defaultControlWidth), control.Width);
				AssertEquals(30, control.CutDownSingleLineNoGroupBoxAddressDropEdit.PreBoundMaxLength);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(201), control.CutDownSingleLineNoGroupBoxAddressDropEdit.Width);
				AssertEquals(ControlDpiScalingHelper.ScaleToCurrentDpiX(296), control.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Width);
				AssertEquals("CodeBox", ControlDpiScalingHelper.ScaleToCurrentDpiX(180), control.CutDownSingleLineNoGroupBoxAddressDropEdit.CodeBox.Width);
				AssertEquals("Address Drop Button", ControlDpiScalingHelper.ScaleToCurrentDpiX(201), control.CutDownSingleLineNoGroupBoxAddressDropEdit.AddressDropButton.Width);
			}
		}

		[RequiresSTA]
		public void TestSingleLineNoGroupBoxShowCompanyBehaviour()
		{
			using (var control = new ZDocAddressControl())
			{
				control.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				AssertEquals("Address DropEdit should have description box", false, control.CutDownSingleLineNoGroupBoxOrgFindBox.ShowDescriptionBox);
				AssertEquals("Precondition", 96, control.CutDownSingleLineNoGroupBoxAddressDropEdit.Left);

				control.ShowCompanyName = true;
				control.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				AssertEquals("Address DropEdit should have description box", true, control.CutDownSingleLineNoGroupBoxOrgFindBox.ShowDescriptionBox);
				AssertEquals("Precondition", control.CutDownSingleLineNoGroupBoxOrgFindBox.Width + ControlDpiScalingHelper.ScaleToCurrentDpiX(1), control.CutDownSingleLineNoGroupBoxAddressDropEdit.Left);

				control.ShowCompanyName = false;
				control.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				AssertEquals("Address DropEdit should have description box", false, control.CutDownSingleLineNoGroupBoxOrgFindBox.ShowDescriptionBox);
				AssertEquals("Precondition", 201, control.CutDownSingleLineNoGroupBoxAddressDropEdit.Left);
			}
		}

		public void TestSingleLineNoGroupBoxPanelWithOverride()
		{
			var dummy = Factory.New<DummyWithDocAddress>();

			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				var addyControl = form.DocAddressControl;
				addyControl.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				AssertEquals("Address DropEdit should be visible", true, addyControl.CutDownSingleLineNoGroupBoxAddressDropEdit.Visible);
				AssertEquals("Address Override should NOT be visible", false, addyControl.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Visible);
				AssertEquals("Address DropEdit should be positioned", 0, addyControl.CutDownSingleLineNoGroupBoxAddressDropEdit.Top);

				dummy.DocAddress.E2_AddressOverride = true;
				AssertEquals("AddressDropEdit should NOT be visible", false, addyControl.CutDownSingleLineNoGroupBoxAddressDropEdit.Visible);
				AssertEquals("Address Override should be visible", true, addyControl.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Visible);
				AssertEquals("Address Override should be positioned", 0, addyControl.CutDownSingleLineNoGroupBoxAddressDropEdit.Top);

				dummy.DocAddress.E2_AddressOverride = false;
				AssertEquals("Address DropEdit should be visible", true, addyControl.CutDownSingleLineNoGroupBoxAddressDropEdit.Visible);
				AssertEquals("Address Override should NOT be visible", false, addyControl.CutDownSingleLineNoGroupBoxOrgOverrideTextBox.Visible);
			}
		}

		public void TestCompactControlDimensions()
		{
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.Compact;
				form.Show();
				CombineAssertions(() =>
				{
					AssertEquals("Height", ControlDpiScalingHelper.ScaleToCurrentDpiY(130), form.DocAddressControl.Height);
					AssertEquals("Width", ControlDpiScalingHelper.ScaleToCurrentDpiX(261), form.DocAddressControl.Width);
				});
			}
		}

		[RequiresSTA]
		public void TestCompactFullNameLabelText()
		{
			string getCompactFullNameLabelText()
			{
				using (var form = new FormWithDocAddressControlForTest(DummyParent))
				{
					form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.Compact;
					form.Show();
					return form.DocAddressControl.CompactFullNameLabel.Text;
				}
			}
			var orgHeader = Factory.New<OrgHeader>();
			DummyParent.DocAddress.OrganisationPK = orgHeader.PK;
			orgHeader.OH_FullName = "Some Company Name";

			var address = orgHeader.MainAddress;
			address.OA_OH = orgHeader.PK;

			CombineAssertions(() =>
			{
				AssertEquals("Full Name Label text should be obtained from organisation", "Some Company Name", getCompactFullNameLabelText());
				address.OA_CompanyNameOverride = "Some Company Name Overriden";
				AssertEquals("Full Name Label text should be obtained from address", "Some Company Name Overriden", getCompactFullNameLabelText());
			});
		}

		public void TestCompactControlLabelsText()
		{
			PopulateDummyAddress();

			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				form.Show();

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.Compact;
				var expectedFullName = "Some Company Name";
				var expectedOrgAddressFormatted = string.Join(System.Environment.NewLine, "Bourton Hello noone", "26 Myrtle Street", "Prospect Blacktown NSW 2149", "Australia").ToUpper();

				CombineAssertions(() =>
				{
					AssertEquals("Full Name Label text should be set", expectedFullName, form.DocAddressControl.CompactFullNameLabel.Text);
					AssertMultilineASCIIEquals("Address Label text should be set", expectedOrgAddressFormatted, form.DocAddressControl.CompactAddressLabel.Text);
				});
			}
		}

		public void TestOverrideCheckBoxHidden_CompactDisplayMode()
		{
			PopulateDummyAddress();

			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				form.Show();

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.Compact;

				AssertEquals(false, form.DocAddressControl.OverrideAddressCheckbox.Visible);
			}
		}

		public void TestOverrideCheckBoxShown_CompactWithOverrideDisplayMode()
		{
			PopulateDummyAddress();

			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				form.Show();

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithOverride;

				AssertEquals(true, form.DocAddressControl.OverrideAddressCheckbox.Visible);
			}
		}

		public void TestOverrideTabControlShown_CompactWithOverrideDisplayModeWhenE2_AddressOverrideChanged()
		{
			PopulateDummyAddress();

			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithOverride;
				form.Show();

				DummyParent.DocAddress.E2_AddressOverride = true;

				CombineAssertions("E2_AddressOverride = true", () =>
				{
					AssertEquals(true, form.DocAddressControl.CompactOverrideTabControl.Visible);
					AssertEquals(true, form.DocAddressControl.CompactOverrideLayoutGroupBox.Visible);
					AssertEquals(false, form.DocAddressControl.CompactLayoutGroupBox.Visible);
					AssertEquals(form.DocAddressControl.CompactOverrideLayoutGroupBox, form.DocAddressControl.OverrideAddressCheckbox.Parent);
				});

				DummyParent.DocAddress.E2_AddressOverride = false;

				CombineAssertions("E2_AddressOverride = false", () =>
				{
					AssertEquals(false, form.DocAddressControl.CompactOverrideTabControl.Visible);
					AssertEquals(false, form.DocAddressControl.CompactOverrideLayoutGroupBox.Visible);
					AssertEquals(true, form.DocAddressControl.CompactLayoutGroupBox.Visible);
					AssertEquals(form.DocAddressControl.CompactLayoutGroupBox, form.DocAddressControl.OverrideAddressCheckbox.Parent);
				});
			}
		}

		[RequiresSTA]
		public void TestOverrideCompactLayout_AddressTab()
		{
			PopulateDummyAddress();

			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithOverride;
				form.Show();
				docAddressControl.OverrideAddressCheckbox.Checked = true;

				var addressTab = form.DocAddressControl.CompactAddressTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Short Caption", "Addr.", addressTab.CaptionResourceString.ShortCaption);
					AssertEquals("Full Caption", "Address", addressTab.CaptionResourceString.Caption);
					AssertEquals("Tab Visible", true, addressTab.TabVisible);
					AssertEquals("Use Visual Style Back Color", false, addressTab.UseVisualStyleBackColor);
				});

				var companyTextBox = docAddressControl.CompactOverriddenCompanyNameTextBox;

				CombineAssertions(() =>
				{
					AssertEquals("Short Caption", "Co.", companyTextBox.CaptionResourceString.ShortCaption);
					AssertEquals("Med Caption", "Company", companyTextBox.CaptionResourceString.MediumCaption);
					AssertEquals("Full Caption", "Company Name", companyTextBox.CaptionResourceString.Caption);
					AssertEquals("BindTo", nameof(JobDocAddress.E2_CompanyName), companyTextBox.BindTo);
					AssertEquals("Mixed Case Input", CharacterCasing.Normal, companyTextBox.CharacterCasing);
					AssertEquals("Visible", true, companyTextBox.Visible);
				});

				var addressTextBox = docAddressControl.CompactOverriddenAddressTextBox;

				CombineAssertions(() =>
				{
					AssertEquals("Short Caption", "Addr.", addressTextBox.CaptionResourceString.ShortCaption);
					AssertEquals("Full Caption", "Address", addressTextBox.CaptionResourceString.Caption);
					AssertEquals("BindTo", nameof(JobDocAddress.E2_Address1AndE2_Address2), addressTextBox.BindTo);
					AssertEquals("Mixed Case Input", CharacterCasing.Normal, addressTextBox.CharacterCasing);
					AssertEquals("Visible", true, addressTextBox.Visible);
				});

				var countryFindBox = docAddressControl.CompactOverriddenCountryFindBox;

				CombineAssertions(() =>
				{
					AssertEquals("Short Caption", "Ctry.", countryFindBox.CaptionResourceString.ShortCaption);
					AssertEquals("Full Caption", "Country", countryFindBox.CaptionResourceString.Caption);
					AssertEquals("BindTo", nameof(JobDocAddress.E2_RN_NKCountryCode), countryFindBox.BindTo);
					AssertEquals("CharacterCasing", CharacterCasing.Upper, countryFindBox.CodeBox.CharacterCasing);
					AssertEquals("Visible", true, countryFindBox.Visible);
				});

				var cityTextBox = docAddressControl.CompactOverriddenCityTextBox;

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "City", cityTextBox.CaptionResourceString.Caption);
					AssertEquals("BindTo", nameof(JobDocAddress.E2_City), cityTextBox.BindTo);
					AssertEquals("CharacterCasing", CharacterCasing.Normal, cityTextBox.CharacterCasing);
					AssertEquals("Visible", true, cityTextBox.Visible);
				});

				var postCodeTextBox = docAddressControl.CompactOverriddenPostCodeTextBox;

				CombineAssertions(() =>
				{
					AssertEquals("Short Caption", "Postc.", postCodeTextBox.CaptionResourceString.ShortCaption);
					AssertEquals("Full Caption", "Postcode", postCodeTextBox.CaptionResourceString.Caption);
					AssertEquals("BindTo", nameof(JobDocAddress.E2_Postcode), postCodeTextBox.BindTo);
					AssertEquals("Visible", true, postCodeTextBox.Visible);
				});
			}
		}

		public void TestOverrideCompactLayout_ContactTab()
		{
			PopulateDummyAddress();

			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithOverride;
				form.Show();

				docAddressControl.OverrideAddressCheckbox.Checked = true;

				var contactTabPage = form.DocAddressControl.CompactContactTabPage;
				form.DocAddressControl.CompactOverrideTabControl.SelectedTab = contactTabPage;

				CombineAssertions(() =>
				{
					AssertEquals("Short Caption", "Con.", contactTabPage.CaptionResourceString.ShortCaption);
					AssertEquals("Full Caption", "Contact", contactTabPage.CaptionResourceString.Caption);
					AssertEquals("Tab Visible", true, contactTabPage.TabVisible);
					AssertEquals("Use Visual Style Back Color", false, contactTabPage.UseVisualStyleBackColor);
				});

				var contactNameTextBox = docAddressControl.CompactOverriddenContactNameTextBox;

				CombineAssertions(() =>
				{
					AssertEquals("Caption", "Name", contactNameTextBox.CaptionResourceString.Caption);
					AssertEquals("BindTo", nameof(JobDocAddress.E2_Contact), contactNameTextBox.BindTo);
					AssertEquals("Mixed Case Input", CharacterCasing.Normal, contactNameTextBox.CharacterCasing);
					AssertEquals("Visible", true, contactNameTextBox.Visible);
				});

				var emailTextBox = docAddressControl.CompactOverriddenEmailTextBox;

				CombineAssertions(() =>
				{
					AssertEquals("Full Caption", "Email", emailTextBox.CaptionResourceString.Caption);
					AssertEquals("BindTo", nameof(JobDocAddress.E2_Email), emailTextBox.BindTo);
					AssertEquals("Visible", true, emailTextBox.Visible);
				});

				docAddressControl.CompactOverrideTabControl.SelectTab(docAddressControl.CompactContactTabPage);
				var phoneNumberTextBox = docAddressControl.CompactOverriddenPhoneNumberUserControl;

				CombineAssertions(() =>
				{
					AssertEquals("BindTo", nameof(JobDocAddress.PhoneNumber), phoneNumberTextBox.GetBindingMember());
					AssertEquals("Short Caption", "Phone", phoneNumberTextBox.CaptionResourceString.ShortCaption);
					AssertEquals("Full Caption", "Phone Number", phoneNumberTextBox.CaptionResourceString.Caption);
					AssertEquals("Visible", true, phoneNumberTextBox.Visible);
				});
			}
		}

		[RequiresSTA]
		public void TestCompactWithContactTabLayout_TabControl()
		{
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;

				var compactWithContactTabControl = docAddressControl.CompactWithContactTabControl;
				AssertContainsExactElementsInExactOrder(
					new[] { docAddressControl.CompactAddressTab, docAddressControl.CompactContactTab },
					compactWithContactTabControl.TabPages);

				var addressTab = docAddressControl.CompactAddressTab;
				AssertEquals("Use Visual Style Back Color", false, addressTab.UseVisualStyleBackColor);
				var addressTabCaption = addressTab.CaptionResourceString;
				AssertEquals("Addr.", addressTabCaption.ShortCaption);
				AssertEquals("Address", addressTabCaption.Caption);

				var contactTab = docAddressControl.CompactContactTab;
				AssertEquals("Use Visual Style Back Color", false, contactTab.UseVisualStyleBackColor);
				var contactTabCaption = contactTab.CaptionResourceString;
				AssertEquals("Con.", contactTabCaption.ShortCaption);
				AssertEquals("Contact", contactTabCaption.Caption);
			}
		}

		public void TestCompactWithContactTabLayout_AddressTabControls()
		{
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;

				form.Show();

				var addressTab = docAddressControl.CompactAddressTab;

				var nameAndAddressPanel = docAddressControl.CompactLayoutNameAndAddressPanel;

				AssertEquals(true, addressTab.Contains(nameAndAddressPanel));

				AssertEquals(true, nameAndAddressPanel.Contains(docAddressControl.CompactFullNameLabel));
				AssertEquals(true, nameAndAddressPanel.Contains(docAddressControl.CompactAddressLabel));
			}
		}

		public void TestCompactWithContactTabLayout_ContactTabControls()
		{
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;

				var addressTab = docAddressControl.CompactContactTab;

				var contactDropEdit = docAddressControl.CompactContactDropEdit;
				AssertEquals(true, addressTab.Contains(contactDropEdit));
				AssertEquals(true, addressTab.Contains(docAddressControl.CompactContactEmailAddressLabel));
				AssertEquals(true, addressTab.Contains(docAddressControl.CompactContactPhoneNumberLabel));
			}
		}

		public void TestCompactWithContactTabLayout_OverrideVisible()
		{
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;

				form.Show();

				AssertEquals(true, docAddressControl.OverrideAddressCheckbox.Visible);
			}
		}

		public void TestCompactWithContactTabLayout_ContactLabelText()
		{
			PopulateDummyAddress();
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;

				form.Show();

				docAddressControl.CompactWithContactTabControl.SelectTab(docAddressControl.CompactContactTab);
				var emailAddressLabel = docAddressControl.CompactContactEmailAddressLabel;
				var phoneLabel = docAddressControl.CompactContactPhoneNumberLabel;

				var contact = DummyParent.Contacts.AddNew();
				contact.OC_ContactName = "Wile E. Coyote";
				contact.OC_Email = "test@test.com";
				contact.OC_Phone = "+49 1234 1234";
				contact.OC_OH = DummyParent.DocAddress.Organisation.PK;

				Factory.Save();

				AssertEquals("No contact, label should be empty", ZString.Empty, emailAddressLabel.Text);
				AssertEquals("No contact, label should be empty", ZString.Empty, phoneLabel.Text);

				DummyParent.DocAddress.E2_Contact = contact.OC_ContactName;

				AssertMultilineASCIIEquals("Label should be formatted with Email of selected contact", "Em: test@test.com", emailAddressLabel.Text);
				AssertMultilineASCIIEquals("Contact label should be formatted with Phone of selected contact", "Ph: +49 1234 1234", phoneLabel.Text);
			}
		}

		public void TestCompactWithContactTabLayout_ContactLabelProperties()
		{
			PopulateDummyAddress();
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;

				docAddressControl.CompactWithContactTabControl.SelectTab(docAddressControl.CompactContactTab);

				var emailLabel = docAddressControl.CompactContactEmailAddressLabel;

				AssertEquals("Auto Ellipsis", true, emailLabel.AutoEllipsis);
				AssertEquals("Text Alignment", ContentAlignment.MiddleLeft, emailLabel.TextAlign);

				var phoneLabel = docAddressControl.CompactContactPhoneNumberLabel;

				AssertEquals("Auto Ellipsis", true, phoneLabel.AutoEllipsis);
				AssertEquals("Text Alignment", ContentAlignment.MiddleLeft, phoneLabel.TextAlign);
			}
		}

		public void TestCompactWithContactTabLayout_ContactDropEditProperties()
		{
			PopulateDummyAddress();
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				var docAddressControl = form.DocAddressControl;
				docAddressControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;

				docAddressControl.CompactWithContactTabControl.SelectTab(docAddressControl.CompactContactTab);

				var contactDropEdit = docAddressControl.CompactContactDropEdit;

				AssertEquals("Binding Member", nameof(JobDocAddress.E2_Contact), contactDropEdit.GetBindingMember());
				AssertEquals("Bind To List", "Organisation+ContactsActive", contactDropEdit.BindToList);
				AssertEquals("Show Description Box", false, contactDropEdit.ShowDescriptionBox);
			}
		}

		public void TestControlSize()
		{
			using (var form = new FormWithDocAddressControlForTest(DummyParent))
			{
				form.Show();

				var expectedControlWidth = ControlDpiScalingHelper.ScaleToCurrentDpiX(ZDocAddressControl.defaultControlWidth);
				var expectedControlHeight = ControlDpiScalingHelper.ScaleToCurrentDpiY(ZDocAddressControl.MaxControlHeight);

				Assert("Precondition - control height shouldn't be customized", !form.DocAddressControl.IsCustomHeight);
				AssertEquals("Precondition - initial control Height should be " + expectedControlHeight + ".", expectedControlHeight, form.DocAddressControl.Height);
				AssertEquals("Precondition - initial control Width should be " + expectedControlWidth + ".", expectedControlWidth, form.DocAddressControl.Width);

				ControlDpiScalingHelper.SetWidth<ZDocAddressControl>(form.DocAddressControl, 5, true);
				ControlDpiScalingHelper.SetHeight<ZDocAddressControl>(form.DocAddressControl, 10, true);

				AssertEquals("Control Height should be fixed at " + expectedControlHeight + ".", expectedControlHeight, form.DocAddressControl.Height);
				AssertEquals("Control Width should be fixed at " + expectedControlWidth + ".", expectedControlWidth, form.DocAddressControl.Width);

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideAndTabs;
				AssertEquals(ControlDpiScalingHelper.NewScaledSize(ZDocAddressControl.defaultControlWidth, 68), form.DocAddressControl.Size);
			}
		}

		[ExpectNoExceptions]
		public void TestDeleteBoundDocAddress()
		{
			DummyWithDocAddressCollection collection = new DummyWithDocAddressCollection(Factory);

			using (FormWithDocAddressControlForTest form = new FormWithDocAddressControlForTest(collection))
			{
				form.Size = new Size(600, 300);
				form.Show();

				DummyWithDocAddress dummy = collection.AddNew();

				dummy.Delete();
			}
		}

		public void TestBinding()
		{
			var dummy = Factory.New<DummyWithDocAddress>();

			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				form.DocAddressControl.ShowResidentialAddressOnOverride = true;
				dummy.DocAddress.E2_AddressOverride = true;
				AssertEquals("Control should be visible to test", true, form.DocAddressControl.PostcodeTextBoxForTest.Visible);

				form.DocAddressControl.PostcodeTextBoxForTest.Focus();
				KeySender.SendKeyPress(form.DocAddressControl.PostcodeTextBoxForTest, Keys.B);
				form.DocAddressControl.CityTextBoxForTest.Focus();
				Application.DoEvents();
				AssertEquals("Value of control text", "B", form.DocAddressControl.PostcodeTextBoxForTest.Text);
				Assert("Control should not have focus", !form.DocAddressControl.PostcodeTextBoxForTest.Focused);
				AssertEquals("Binding did not transmit B to business layer", "B", dummy.DocAddress.E2_Postcode);

				// residential address checkbox
				AssertEquals("Control should be visible to test", false, form.DocAddressControl.ResidentialAddressCheckBoxForTest.Visible);
				AssertEquals("Control should be visible to test", false, form.DocAddressControl.ResidentialAddressCheckBoxForTest.Checked);
			}
		}

		public void TestSetLabelCaptionVisible()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.DocAddressControl.SetLabelCaptionVisible(false);
				form.Show();
				Application.DoEvents();

				var provider = form.DocAddressControl.LabelCaptionRenderProvider_Exposed;
				AssertEquals("DefaultGroupBox LabelCaptionVisible", false, provider.GetLabelCaptionVisible(form.DocAddressControl.DefaultGroupBox));
				AssertEquals("DefaultGroupBox CutDownGroupBox", false, provider.GetLabelCaptionVisible(form.DocAddressControl.CutDownGroupBox));
				AssertEquals("DefaultGroupBox CutDownSingleLineGroupBox", false, provider.GetLabelCaptionVisible(form.DocAddressControl.CutDownSingleLineGroupBox));
				AssertEquals("DefaultGroupBox OverrideGroupBox", false, provider.GetLabelCaptionVisible(form.DocAddressControl.OverrideGroupBox));
			}
		}

		[RequiresSTA]
		public void TestGroupBoxLabelCaptionVisible()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				Application.DoEvents();

				Assert(!form.DocAddressControl.LabelCaptionRenderProvider_Exposed.GetLabelCaptionVisible(form.DocAddressControl.DefaultGroupBox));
			}
		}

		public void TestCaption_OverrideOptions()
		{
			const string docAddressCaption = "DocAddress Caption";
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				form.DocAddressControl.Text = docAddressCaption;

				CombineAssertions(() =>
				{
					AssertEquals("Initially", docAddressCaption, FindGroupBox(form).Text);

					form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideAndTabs;
					AssertEquals("DisplayMode = HideOverrideAndTabs", docAddressCaption, FindGroupBox(form).Text);

					form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.ShowOverrideAndTabs;
					AssertEquals("DisplayMode = ShowOverrideAndTabs", docAddressCaption, FindGroupBox(form).Text);

					((CheckBox)form.DocAddressControl.Controls["OverrideAddressCheckbox"]).Checked = true;
					AssertEquals("When Override ticked", docAddressCaption, FindGroupBox(form).Text);
				});
			}
		}

		public void TestResourceStringCaption()
		{
			const string caption = "DocAddress Caption";
			var dummy = Factory.New<DummyWithDocAddress>();

			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.DocAddressControl.CaptionResourceString = new ResourceStringData(ZGuid.NewZGuid().ToString(), caption);

				form.Show();
				Application.DoEvents();
				AssertEquals("Initially", caption, FindGroupBox(form).Text);

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideAndTabs;
				AssertEquals("DisplayMode = false", caption, FindGroupBox(form).Text);

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.ShowOverrideAndTabs;
				AssertEquals("DisplayMode = ShowOverrideAndTabs", caption, FindGroupBox(form).Text);

				((CheckBox)form.DocAddressControl.Controls["OverrideAddressCheckbox"]).Checked = true;
				AssertEquals("When Override ticked", caption, FindGroupBox(form).Text);
			}
		}

		public void TestEditControlsDoNotHaveTheirOwnResourceExtensions()
		{
			using (var form = new FormWithDocAddressControlForTest(Factory.New<DummyWithDocAddress>()))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull(form.DocAddressControl.GetExtension<IHintExtension>());
				AssertNotNull(form.DocAddressControl.GetExtension<ILabelCaptionRenderer>());
				AssertNotNull(form.DocAddressControl.GetExtension<IStatusbarExtension>());

				AssertEquals(1, form.DocAddressControl.CutDownOrganisationFindBox.Extensions.Count());
				AssertNotNull(form.DocAddressControl.CutDownOrganisationFindBox.GetExtension<INotificationExtension>());
				AssertEquals(1, form.DocAddressControl.DefaultGroupBox.OrganisationFindBox.Extensions.Count());
				AssertNotNull(form.DocAddressControl.DefaultGroupBox.OrganisationFindBox.GetExtension<INotificationExtension>());
				AssertEquals(1, form.DocAddressControl.CutDownSingleLineOrgFindBox.Extensions.Count());
				AssertNotNull(form.DocAddressControl.CutDownSingleLineOrgFindBox.GetExtension<INotificationExtension>());

				AssertEquals(1, form.DocAddressControl.CutDownAddressDropEdit.Extensions.Count());
				AssertEquals(1, form.DocAddressControl.CutDownSingleLineAddressDropEdit.Extensions.Count());
				AssertEquals(1, form.DocAddressControl.DefaultGroupBox.AddressEdit.Extensions.Count());
				AssertEquals(1, form.DocAddressControl.DefaultGroupBox.ContactEdit.Extensions.Count());

				AssertEquals(1, form.DocAddressControl.CompactAddressDropEdit.Extensions.Count());
				AssertEquals(1, form.DocAddressControl.CompactOrganizationFindBox.Extensions.Count());
			}
		}

		public void TestSelectFromPopup()
		{
			var dummy = Factory.New<DummyWithDocAddress>();

			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();

				AssertEquals("Popup not shown yet", false, form.DocAddressControl.PopupShown);

				dummy.DocAddress.E2_AddressOverride = false;
				form.DocAddressControl.SelectFromPopupForm();
				AssertEquals("Popup should have been shown", true, form.DocAddressControl.PopupShown);

				dummy.DocAddress.E2_AddressOverride = true;
				form.DocAddressControl.PopupShown = false;
				form.DocAddressControl.SelectFromPopupForm();
				AssertEquals("Popup should not have been shown", false, form.DocAddressControl.PopupShown);
			}
		}

		public void TestTabFromOrganisationSkipsAddressWhenOrganisationIsEmpty()
		{
			var dummy = Factory.New<DummyWithDocAddress>();

			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				TextBox textBox = new TextBox();
				textBox.Visible = true;
				form.Controls.Add(textBox);
				form.Show();

				AssertEquals("Popup not shown yet", false, form.DocAddressControl.PopupShown);

				dummy.DocAddress.E2_AddressOverride = false;
				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideAndTabs;
				textBox.Location = Point.Add(form.DocAddressControl.Location, form.DocAddressControl.Size);
				Application.DoEvents();
				var orgControl = form.DocAddressControl.CutDownGroupBox.Controls[1];
				orgControl.Focus();
				KeySender.PostKeyDown(orgControl, orgControl.Handle, Keys.Tab);
				Application.DoEvents();
				AssertEquals("After Tab", form.ActiveControl, textBox);
			}
		}

		public void TestGovernmentCodeVisibility()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				dummy.DocAddress.E2_AddressOverride = true;
				Application.DoEvents();
				AssertEquals(false, form.DocAddressControl.GovernmentRegistrationTabPageForTest.TabVisible);

				dummy.DocAddress.OverrideRequirement = new JobDocAddressRequirement();
				dummy.DocAddress.Requirement.GetRegistrationNumberResult = delegate
				{
					return new RegistrationNumberResult(dummy.DocAddress.Factory, false, () => new RegistrationNumber());
				};

				dummy.DocAddress.E2_AddressOverride = false;
				dummy.DocAddress.E2_AddressOverride = true;
				Application.DoEvents();
				AssertEquals(false, form.DocAddressControl.GovernmentRegistrationTabPageForTest.TabVisible);

				dummy.DocAddress.Requirement.GetRegistrationNumberResult = delegate
				{
					return new RegistrationNumberResult(dummy.DocAddress.Factory, true, () => new RegistrationNumber());
				};

				dummy.DocAddress.E2_AddressOverride = false;
				dummy.DocAddress.E2_AddressOverride = true;
				Application.DoEvents();
				AssertEquals(true, form.DocAddressControl.GovernmentRegistrationTabPageForTest.TabVisible);
			}
		}

		[RequiresSTA]
		public void TestResidentialCheckboxVisibility()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				dummy.DocAddress.E2_AddressOverride = true;
				Application.DoEvents();
				AssertEquals(false, form.DocAddressControl.ResidentialAddressCheckBoxForTest.Visible);

				form.DocAddressControl.ShowResidentialAddressOnOverride = true;
				dummy.DocAddress.E2_AddressOverride = false;
				dummy.DocAddress.E2_AddressOverride = true;
				Application.DoEvents();
				AssertEquals(false, form.DocAddressControl.ResidentialAddressCheckBoxForTest.Visible);
			}
		}

		public void TestAddressValidationStatusButtonVisibility_DisableAddressValidationWebService()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			dummy.DocAddress.E2_AddressOverride = false;
			Env.Instance.Registry.EnableAddressValidationWebService = false;
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				AssertEquals("OverrideAddressCheckBox should be visible by default", true, form.DocAddressControl.OverrideAddressCheckbox.Visible);
				AssertEquals("AddressValidationStatusButton should not be visible when not using address validation", false, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}
		}

		[RequiresSTA]
		public void TestAddressValidationStatusButtonVisibility_EnableAddressValidationWebService()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				AssertEquals("OverrideAddressCheckBox should be visible by default", true, form.DocAddressControl.OverrideAddressCheckbox.Visible);
				AssertEquals("AddressValidationStatusButton should not be visible if DocAddress is empty or null", false, form.DocAddressControl.AddressValidationStatusButton.Visible);

				dummy.DocAddress.E2_RN_NKCountryCode = null;
				form.DocAddressControl.DefaultGroupBox.AddressEdit.CodeBox.Text = "xyz";
				AssertEquals("AddressValidationStatusButton should not be visible when using address validation and DocAddress country/region is empty", false, form.DocAddressControl.AddressValidationStatusButton.Visible);

				dummy.DocAddress.E2_RN_NKCountryCode = "AU";
				form.DocAddressControl.DefaultGroupBox.AddressEdit.CodeBox.Text = "abc";
				AssertEquals("AddressValidationStatusButton should be visible when using address validation and DocAddress country/region is not empty", true, form.DocAddressControl.AddressValidationStatusButton.Visible);

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverride;
				AssertEquals("AddressValidationStatusButton should not be visible when in SingleLineNoOverride mode", false, form.DocAddressControl.AddressValidationStatusButton.Visible);

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverrideNoGroupBox;
				AssertEquals("AddressValidationStatusButton should not be visible when in SingleLineNoOverrideNoGroupBox mode", false, form.DocAddressControl.AddressValidationStatusButton.Visible);

				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideAndTabs;
				form.DocAddressControl.DefaultGroupBox.AddressEdit.CodeBox.Text = "bcd";
				AssertEquals("AddressValidationStatusButton should not be visible when DocAddressControl display mode is HideOverrideAndTabs", false, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}
		}

		public void TestAddressValidationStatusButtonVisibility_HideOverrideAndTabs()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				dummy.DocAddress.E2_RN_NKCountryCode = "AU";
				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideAndTabs;
				form.Show();
				AssertEquals("AddressValidationStatusButton should not be visible", false, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}
		}

		[RequiresSTA]
		public void TestAddressValidationStatusButtonVisibility_HideOverrideShowTabs()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideShowTabs;
				dummy.DocAddress.E2_AddressOverride = true;
				dummy.DocAddress.E2_RN_NKCountryCode = "AU";
				form.Show();
				AssertEquals("AddressValidationStatusButton should not be visible", false, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}

			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.HideOverrideShowTabs;
				dummy.DocAddress.E2_AddressOverride = false;
				dummy.DocAddress.E2_RN_NKCountryCode = "AU";
				form.Show();
				AssertEquals("AddressValidationStatusButton should be visible", true, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}
		}

		public void TestAddressValidationStatusButtonVisibility_SwitchingTab()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				dummy.DocAddress.E2_AddressOverride = false;
				dummy.DocAddress.E2_RN_NKCountryCode = "AU";
				form.DocAddressControl.DefaultGroupBox.AddressEdit.CodeBox.Text = "abc";
				AssertEquals("AddressValidationStatusButton should be visible", true, form.DocAddressControl.AddressValidationStatusButton.Visible);
				form.DocAddressControl.UpdateControlLayout();
				AssertEquals("Switching tab does not hide AddressValidationStatusButton", true, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}
		}

		public void TestAddressValidationStatusButtonVisibility_Compact()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.DocAddressControl.DisplayMode = ZDocAddressControlDisplayMode.Compact;
				dummy.DocAddress.E2_RN_NKCountryCode = "AU";
				form.Show();
				AssertEquals("AddressValidationStatusButton should be visible", true, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}
		}

		public void TestAddressValidationStatusButtonVisibility_UnmatchedOrganisation()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			var unMatchedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED"));
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				dummy.DocAddress.OrganisationPK = unMatchedOrg.PK;
				dummy.DocAddress.E2_AddressOverride = false;
				dummy.DocAddress.E2_RN_NKCountryCode = "AU";
				form.DocAddressControl.DefaultGroupBox.AddressEdit.CodeBox.Text = "abc";
				AssertEquals("AddressValidationStatusButton should NOT be visible", false, form.DocAddressControl.AddressValidationStatusButton.Visible);
			}
		}

		public void TestIZAddressParentParseCode()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();

				dummy.DocAddress.OrganisationPK = dummy.Organisations.Count > 0 ? dummy.Organisations[0].PK : dummy.Organisations.AddNew().PK;

				var address1 = dummy.DocAddress.Organisation.AddressesNoAutoCreate.AddNew();
				address1.OA_Code = "AAA";

				var address2 = dummy.DocAddress.Organisation.AddressesNoAutoCreate.AddNew();
				address2.OA_Code = "BBB";

				var iZAddressParent = (IZAddressParent)form.DocAddressControl;

				AssertEquals(address1.PK, iZAddressParent.ParseCode("AAA"));
				AssertEquals(address2.PK, iZAddressParent.ParseCode("BBB"));
				Assert(!iZAddressParent.ParseCode("CCC").IsValid);
			}
		}

		public void TestShowSimilarOrganizations()
		{
			var organization = TestOrganization;
			var dummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				dummy.DocAddress.E2_AddressOverride = true;
				var mainAddress = organization.MainAddress;
				dummy.DocAddress.E2_CompanyName = mainAddress.OA_CompanyNameOverride;
				dummy.DocAddress.E2_Address1 = mainAddress.OA_Address1;
				dummy.DocAddress.E2_Address2 = mainAddress.OA_Address2;
				dummy.DocAddress.E2_City = mainAddress.OA_City;
				dummy.DocAddress.E2_State = mainAddress.OA_State;
				dummy.DocAddress.E2_Email = mainAddress.OA_Email;
				dummy.DocAddress.E2_Phone = mainAddress.OA_Phone;
				dummy.DocAddress.E2_Fax = mainAddress.OA_Fax;
				form.DocAddressControl.ConvertToOrganizationButtonForTest.PerformClick();
				Assert(form.DocAddressControl.SimilarOrganizationShown);
			}
		}

		public void TestShowNewOrganization()
		{
			var organization = Factory.NewWithValidTestData<OrgHeader>();
			var dummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				dummy.DocAddress.E2_AddressOverride = true;
				var mainAddress = organization.MainAddress;
				mainAddress.OA_RN_NKCountryCode = "TS";

				dummy.DocAddress.E2_CompanyName = mainAddress.OA_CompanyNameOverride;
				dummy.DocAddress.E2_Address1 = mainAddress.OA_Address1;
				dummy.DocAddress.E2_Address2 = mainAddress.OA_Address2;
				dummy.DocAddress.E2_City = mainAddress.OA_City;
				dummy.DocAddress.E2_RN_NKCountryCode = mainAddress.OA_RN_NKCountryCode;
				dummy.DocAddress.E2_State = mainAddress.OA_State;
				dummy.DocAddress.E2_Email = mainAddress.OA_Email;
				dummy.DocAddress.E2_Phone = mainAddress.OA_Phone;
				dummy.DocAddress.E2_Fax = mainAddress.OA_Fax;
				form.DocAddressControl.ConvertToOrganizationButtonForTest.PerformClick();
				Assert(form.DocAddressControl.NewOrganizationShown);
				AssertEquals(form.DocAddressControl.NewOrganizationCountryCode, "TS");
			}
		}

		public void TestConvertToOrganization_WithoutOrganisationSecurity()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			bool originalOrganisationSecurity = Env.Security.Organisation.IsAllowed;

			try
			{
				Env.Security.Organisation.IsAllowed = false;

				var organization = TestOrganization;
				var dummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
				using (var form = new FormWithDocAddressControlForTest(dummy))
				{
					form.Show();
					dummy.DocAddress.E2_AddressOverride = true;
					var mainAddress = organization.MainAddress;
					dummy.DocAddress.E2_CompanyName = mainAddress.OA_CompanyNameOverride;
					dummy.DocAddress.E2_Address1 = mainAddress.OA_Address1;
					dummy.DocAddress.E2_Address2 = mainAddress.OA_Address2;
					dummy.DocAddress.E2_City = mainAddress.OA_City;
					dummy.DocAddress.E2_State = mainAddress.OA_State;
					dummy.DocAddress.E2_Email = mainAddress.OA_Email;
					dummy.DocAddress.E2_Phone = mainAddress.OA_Phone;
					dummy.DocAddress.E2_Fax = mainAddress.OA_Fax;
					form.DocAddressControl.ConvertToOrganizationButtonForTest.PerformClick();

					Assert("Security error message is shown", UnitTestUserNotification.Instance.LastMessage.WasError);
					Assert(!form.DocAddressControl.SimilarOrganizationShown);
				}
			}
			finally
			{
				Env.Security.Organisation.IsAllowed = originalOrganisationSecurity;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestAdditionalAddressInformation_CopiedWhenGenerateNewOrganization()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.MainAddress.OA_AdditionalAddressInformation = string.Empty;

			var dummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			dummy.DocAddress.AdditionalAddressInformation = "This is new additional address";
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				var newOrg = form.DocAddressControl.GenerateTemporaryOrganization_Exposed(org);
				AssertEquals(newOrg.MainAddress.OA_AdditionalAddressInformation, "This is new additional address");
			}
		}

		public void TestDataSourceType()
		{
			using (var control = new ZDocAddressControl())
			{
				AssertEquals(typeof(JobDocAddress), control.DataSourceType);
			}
		}

		public void TestReadonlyWhenControlIsReadOnly()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			using (var form = new ZForm(testHeader))
			using (var testControl = new ZDocAddressControlForTest())
			{
				form.Controls.Add(testControl);
				form.Show();

				var readOnlyToggleControl = testControl as IReadOnlyToggleControl;
				AssertNotNull("ZDocAddressControlTestClass should implement IReadOnlyToggleControl", readOnlyToggleControl);
				AssertEquals("Precondition: User control is not readonly", false, testControl.ReadOnly);

				readOnlyToggleControl.ReadOnly = true;
				AssertEquals("User control is readonly", true, testControl.ReadOnly);
				AssertEquals("AddressValidationStatusButton.ReadOnly", true, testControl.AddressValidationStatusButton_Exposed.ReadOnly);
				AssertEquals("ClearFieldsButton.ReadOnly", true, testControl.ClearAddressFieldsButton.ReadOnly);
			}
		}

		[ExpectNoExceptions]
		public void TestAddressValidationStatusButton_WhenUsingUNMATCHEDOrganisation()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			var unMatchedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED"));
			var docAddress = Factory.NewWithValidTestData<DummyWithDocAddress>();

			docAddress.DocAddress.OrganisationPK = unMatchedOrg.PK;

			using (var form = new FormWithDocAddressControlForTest(docAddress))
			{
				form.Show();

				try
				{
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
					form.DocAddressControl.AddressValidationStatusButtonClick();
				}
				finally
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					form.DocAddressControl = null;
				}
			}
		}

		[RequiresSTA]
		public void TestPhoneNumberControls()
		{
			using (var testControl = new ZDocAddressControlForTest())
			{
				Assert(testControl.PhoneNumberControl_Exposed.EnableValidStateColor);
				Assert(testControl.PhoneNumberControl_Exposed.ShowDiallerControl);
				Assert(!testControl.PhoneNumberControl_Exposed.ShowLocalNumberLabel);
				Assert(!testControl.PhoneNumberControl_Exposed.ShowPublishedCheckBox);
				Assert(testControl.PhoneNumberControl_Exposed.ShowToolTip);

				Assert(testControl.MobilePhoneNumberControl_Exposed.EnableValidStateColor);
				Assert(testControl.MobilePhoneNumberControl_Exposed.ShowDiallerControl);
				Assert(!testControl.MobilePhoneNumberControl_Exposed.ShowLocalNumberLabel);
				Assert(!testControl.MobilePhoneNumberControl_Exposed.ShowPublishedCheckBox);
				Assert(testControl.MobilePhoneNumberControl_Exposed.ShowToolTip);

				Assert(testControl.FaxNumberControl_Exposed.EnableValidStateColor);
				Assert(!testControl.FaxNumberControl_Exposed.ShowDiallerControl);
				Assert(!testControl.FaxNumberControl_Exposed.ShowLocalNumberLabel);
				Assert(!testControl.FaxNumberControl_Exposed.ShowPublishedCheckBox);
				Assert(testControl.FaxNumberControl_Exposed.ShowToolTip);
			}
		}

		public void TestAddAndRemoveFromMainForm()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());

			using (var form = new ZForm())
			{
				using (var docAddress = new ZDocAddressControl())
				{
					form.Controls.Add(docAddress);

					form.Show();

					Application.DoEvents();

					form.Controls.Remove(docAddress);
				}

				AssertNoExceptionThrown("It should not throw any exception since it's being disposed correctly", form.Close);
			}
		}

		[ExpectNoExceptions]
		public void TestValidationCompletesWithoutExceptionAfterFormIsClosed()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			bool originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var unMatchedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED"));
				var docAddress = Factory.NewWithValidTestData<DummyWithDocAddress>();
				docAddress.DocAddress.OrganisationPK = unMatchedOrg.PK;
				docAddress.DocAddress.E2_AddressOverride = ZBool.True;

				using (var form = new FormWithDocAddressControlForTest(docAddress))
				{
					form.Show();
				}

				Application.DoEvents(); //Allow the background address validation to complete after the form is closed
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestNoExceptionMessageShowWhenHookDocAddressAtTheFirstTime()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			var originalEnableAddressValidation = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				var unMatchedOrg = Factory.LoadTop1<OrgHeader>(new ZQuery(OrgHeaderSchema.OH_Code, "UNMATCHED"));
				var docAddress = Factory.NewWithValidTestData<DummyWithDocAddress>();
				docAddress.DocAddress.OrganisationPK = unMatchedOrg.PK;
				docAddress.DocAddress.E2_AddressOverride = ZBool.True;

				using (var form = new FormWithDocAddressControlForTest(docAddress))
				{
					AssertEquals("Precondition with unmatched org warning", 1, docAddress.DocAddress.Notifications.Count());
					AssertNullOrEmpty("Precondition", UnitTestUserNotification.Instance.LastMessage.Text);

					form.Show();
					AssertEquals(3, docAddress.DocAddress.Notifications.Count());
					AssertNullOrEmpty(UnitTestUserNotification.Instance.LastMessage.Text);

					docAddress = Factory.NewWithValidTestData<DummyWithDocAddress>();
					docAddress.DocAddress.OrganisationPK = unMatchedOrg.PK;
					docAddress.DocAddress.E2_AddressOverride = ZBool.True;

					AssertEquals("Precondition with unmatched org warning", 1, docAddress.DocAddress.Notifications.Count());

					form.DocAddressControl.SetDataBinding(docAddress, "DocAddress");
					AssertEquals(3, docAddress.DocAddress.Notifications.Count());
					AssertEquals("Please fix errors on address before running validation.", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = originalEnableAddressValidation;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestRefreshValidationStatus_AddressOverride()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			testDummy.DocAddress.E2_AddressOverride = true;

			using (var form = new FormWithDocAddressControlForTest(testDummy))
			{
				form.Show();

				var testControl = form.DocAddressControl;

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
				testDummy.DocAddress.E2_RN_NKCountryCode = "AU";
				testDummy.DocAddress.E2_ValidationStatus = AddressValidationStatus.Verified;
				testControl.ClearFieldsButtonEffectiveCore = false;
				testControl.RefreshValidationStatus();

				var validGreenColor = Color.FromArgb(198, 236, 198);

				AssertEquals(validGreenColor, testControl.Address1Control_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.Address2Control_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.CityControl_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.PostcodeControl_Exposed.BackColor);
				AssertEquals(validGreenColor, testControl.StateControl_Exposed.CodeBox.BackColor);

				AssertEquals(validGreenColor, testControl.CountryControl_Exposed.CodeBox.BackColor);
				AssertEquals(true, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(false, testControl.ClearAddressFieldsButton.Visible);

				testControl.ClearFieldsButtonEffectiveCore = true;
				testControl.RefreshValidationStatus();
				AssertEquals(true, testControl.ClearAddressFieldsButton.Visible);

				var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(us.PK, disabledForOverrideAddress: true));
				testDummy.DocAddress.E2_RN_NKCountryCode = "US";
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.Address1Control_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.Address2Control_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.CityControl_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.PostcodeControl_Exposed.BackColor);
				AssertEquals(Color.FromArgb(255, 215, 215), testControl.StateControl_Exposed.CodeBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CountryControl_Exposed.CodeBox.BackColor);
				AssertEquals(false, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(false, testControl.ClearAddressFieldsButton.Visible);

				Env.Instance.Registry.EnableAddressValidationWebService = false;
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.Address1Control_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.Address2Control_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.CityControl_Exposed.BackColor);
				AssertEquals(SystemColors.Window, testControl.PostcodeControl_Exposed.BackColor);
				AssertEquals(Color.FromArgb(255, 215, 215), testControl.StateControl_Exposed.CodeBox.BackColor);
				AssertEquals(SystemColors.Window, testControl.CountryControl_Exposed.CodeBox.BackColor);
				AssertEquals(false, testControl.ValidateAddressButton_Exposed.Visible);
				AssertEquals(false, testControl.ClearAddressFieldsButton.Visible);
			}
		}

		[RequiresSTA]
		public void TestTabNameChangeAndToolTip_AddressOverride_TabVisible()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			testDummy.DocAddress.E2_AddressOverride = true;

			using (var form = new FormWithDocAddressControlForTest(testDummy))
			{
				form.Show();

				var testControl = form.DocAddressControl;

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
				testDummy.DocAddress.E2_RN_NKCountryCode = "AU";
				testDummy.DocAddress.E2_State = null;

				testControl.GovernmentRegistrationTabPageForTest.TabVisible = true;
				testControl.Validate();

				AssertEquals("AddressTabPage's tooltip should be Address", "Address", testControl.AddressTabPageForTest.ToolTipText);
				AssertEquals("ContactTabPage's tooltip should be Contact", "Contact", testControl.ContactTabPageForTest.ToolTipText);

				AssertEquals("GovernmentRegistrationTabPage's tooltip should be Code", "Code", testControl.GovernmentRegistrationTabPageForTest.ToolTipText);

				AssertEquals("GovernmentRegistrationTabPage caption should be Code", "Code", testControl.GovernmentRegistrationTabPageForTest.CaptionResourceString.Caption);

				AssertEquals("AddressTabPage.Text when having validation errors should change to Addr.", "Addr.", testControl.AddressTabPageForTest.Text);
				AssertEquals("ContactTabPage.Text when having validation errors should change to Cont.", "Con.", testControl.ContactTabPageForTest.Text);
				AssertEquals("GovernmentRegistrationTabPage.Text when having validation errors should change to Cod.", "Cod.", testControl.GovernmentRegistrationTabPageForTest.Text);

				testDummy.DocAddress.E2_RN_NKCountryCode = "AU";
				testDummy.DocAddress.E2_State = "NSW";
				testDummy.DocAddress.CompanyName = "Test Company";
				testDummy.DocAddress.E2_ValidationStatus = AddressValidationStatus.Verified;
				testControl.RefreshValidationStatus();

				AssertEquals("AddressTabPage.Text should be changed back to Address after clearing error", "Address", testControl.AddressTabPageForTest.Text);
				AssertEquals("ContactTabPage.Text should be changed back to Contact clearing error", "Contact", testControl.ContactTabPageForTest.Text);
				AssertEquals("GovernmentRegistrationTabPage.Text should be changed back to Contact clearing error", "Code", testControl.GovernmentRegistrationTabPageForTest.Text);
			}
		}

		public void TestCompactLayoutWhenNotBoundAndCompactMode()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			testDummy.DocAddress.E2_AddressOverride = true;
			using (var form = new ZForm())
			{
				var testControl = new ZDocAddressControlForTest();
				testControl.DisplayMode = ZDocAddressControlDisplayMode.CompactWithContactTab;
				form.Controls.Add(testControl);
				form.Show();

				AssertEquals("CompactLayoutUsed", true, testControl.CompactLayoutGroupBox.Visible);
			}
		}

		public void TestTabNameChangeAndToolTip_AddressOverride_TabNotVisible()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			testDummy.DocAddress.E2_AddressOverride = true;

			using (var form = new FormWithDocAddressControlForTest(testDummy))
			{
				form.Show();

				var testControl = form.DocAddressControl;

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
				testDummy.DocAddress.E2_RN_NKCountryCode = "AU";
				testDummy.DocAddress.E2_State = null;

				testControl.GovernmentRegistrationTabPageForTest.TabVisible = false;
				testControl.Validate();

				AssertEquals("AddressTabPage.Text should not be changed since GovernmentRegistrationTabPage is not visible", "Addr.", testControl.AddressTabPageForTest.Text);
				AssertEquals("ContactTabPage.Text should not be changed since GovernmentRegistrationTabPage is not visible", "Con.", testControl.ContactTabPageForTest.Text);
				AssertEquals("GovernmentRegistrationTabPage.Text should be empty since GovernmentRegistrationTabPage is not visible", "", testControl.GovernmentRegistrationTabPageForTest.Text);
			}
		}

		public void TestRefreshValidationStatus_NoAddressOverride()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			var testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testDummy.DocAddress.E2_OA_Address = testAddress.PK;
			testDummy.DocAddress.E2_AddressOverride = false;

			using (var form = new FormWithDocAddressControlForTest(testDummy))
			{
				form.Show();

				var testControl = form.DocAddressControl;

				Env.Instance.Registry.EnableAddressValidationWebService = true;
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection());
				testAddress.OA_RN_NKCountryCode = "AU";
				testAddress.OA_ValidationStatus = AddressValidationStatus.Verified;
				testControl.RefreshValidationStatus();

				var validGreenColor = Color.FromArgb(198, 236, 198);
				AssertEquals(validGreenColor, testControl.AddressEditControl_Exposed.CodeBox.BackColor);

				var us = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, "US");
				OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(us.PK, disabledForOrgAddress: true));
				testAddress.OA_RN_NKCountryCode = "US";
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.AddressEditControl_Exposed.CodeBox.BackColor);

				Env.Instance.Registry.EnableAddressValidationWebService = false;
				testControl.RefreshValidationStatus();

				AssertEquals(SystemColors.Window, testControl.AddressEditControl_Exposed.CodeBox.BackColor);
			}
		}

		public void TestTabIndicesInRightOrder()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			using (var form = new FormWithDocAddressControlForTest(testDummy))
			{
				form.Show();
				var testControl = form.DocAddressControl;

				AssertEquals(testControl.StateControl_Exposed.TabIndex - testControl.CityTextBoxForTest.TabIndex, 1);
				AssertEquals(testControl.CityTextBoxForTest.TabIndex - testControl.PostcodeTextBoxForTest.TabIndex, 1);
			}
		}

		public void TestNotShowValidationStatusButtonWhenRefreshValidationStatus_SingleLineNoOverride()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			var testAddress = Factory.NewWithValidTestData<OrgAddress>();
			testDummy.DocAddress.E2_OA_Address = testAddress.PK;
			testDummy.DocAddress.E2_AddressOverride = false;
			testAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			testAddress.OA_ValidationStatus = AddressValidationStatus.Verified;

			var rawEnableSetting = Env.Instance.Registry.EnableAddressValidationWebService;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;
				using (var form = new ZForm(testDummy))
				{
					var control = new ZDocAddressControlForTest();
					control.BindTo = "DocAddress";
					control.BindToContacts = "Contacts";
					control.BindToOrganisations = "Organisations";
					control.DisplayMode = ZDocAddressControlDisplayMode.SingleLineNoOverride;
					form.Controls.Add(control);
					form.Show();
					Assert("Precondition", !control.AddressValidationStatusButton_Exposed.Visible);

					control.RefreshValidationStatus();
					Assert(!control.AddressValidationStatusButton_Exposed.Visible);
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawEnableSetting;
			}
		}

		[RequiresSTA]
		public void TestOverrideGroupBoxReadOnlyWhenDoNotHaveSecurityRight()
		{
			OverrideGroupBoxReadOnlyWhenDoNotHaveSecurityRight(ZDocAddressControlDisplayMode.HideOverrideShowTabs);
			OverrideGroupBoxReadOnlyWhenDoNotHaveSecurityRight(ZDocAddressControlDisplayMode.ShowOverrideAndTabs);
		}

		[RequiresSTA]
		public void TestShouldValidateAddressForTest()
		{
			var rawValue = Env.Instance.Registry.EnableAddressValidationWebService;
			var usPK = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "US")).PK.ToGuid();
			var dummy = Factory.New<DummyWithDocAddress>();
			dummy.DocAddress.E2_AddressOverride = false;

			try
			{
				Env.Instance.Registry.EnableAddressValidationWebService = true;

				using (var form = new FormWithDocAddressControlForTest(dummy))
				{
					form.Show();
					dummy.DocAddress.E2_RN_NKCountryCode = "US";

					using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
					{
						AssertEquals(true, form.DocAddressControl.ShouldValidateAddressForTest());
					}

					using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForOrgAddress: true)))
					{
						AssertEquals(false, form.DocAddressControl.ShouldValidateAddressForTest());

						dummy.DocAddress.E2_AddressOverride = true;
						dummy.DocAddress.E2_RN_NKCountryCode = "US";
						AssertEquals(true, form.DocAddressControl.ShouldValidateAddressForTest());
					}

					using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, DisabledCountryItemCollectionTestHelper.GetCollection(usPK, disabledForOverrideAddress: true)))
					{
						AssertEquals(false, form.DocAddressControl.ShouldValidateAddressForTest());
					}
				}
			}
			finally
			{
				Env.Instance.Registry.EnableAddressValidationWebService = rawValue;
			}
		}

		[RequiresSTA]
		public void TestTextBoxPlaceholderText()
		{
			var dummy = Factory.New<DummyWithDocAddress>();
			dummy.DocAddress.E2_AddressOverride = true;
			using (var form = new FormWithDocAddressControlForTest(dummy))
			{
				form.Show();
				CombineAssertions("Placeholder text inside of textboxes should be correct", () =>
				{
					AssertEquals("Company Name", form.DocAddressControl.CompanyTextBoxForTest.PlaceHolderText);
					AssertEquals("E.g. Leave at Reception", form.DocAddressControl.AdditionalAddressInformationTextBoxForTest.PlaceHolderText);
					AssertEquals("Address Line 1", form.DocAddressControl.AddressLine1TextBoxForTest.PlaceHolderText);
					AssertEquals("Address Line 2", form.DocAddressControl.AddressLine2TextBoxForTest.PlaceHolderText);
					AssertEquals("Postcode", form.DocAddressControl.PostcodeTextBoxForTest.PlaceHolderText);
				});
			}
		}

		public void TestOverrideGroupBoxReadOnlyWhenReadOnlyStatusChange()
		{
			var rawRegistryValue = Env.Security.GlobalChargeCodes.IsAllowed;

			try
			{
				var dummy = Factory.New<DummyWithDocAddress>();
				dummy.DocAddress.E2_AddressOverride = true;

				Env.Security.JobDocAddressOverride.IsAllowed = true;
				using (var form = new FormWithDocAddressControlForTest(dummy))
				{
					form.Show();
					form.DocAddressControl.CompanyTextBoxForTest.ReadOnly = false;
					Assert(!form.DocAddressControl.CompanyTextBoxForTest.ReadOnly);

					form.DocAddressControl.CompanyTextBoxForTest.ReadOnly = true;
					Assert(form.DocAddressControl.CompanyTextBoxForTest.ReadOnly);
				}

				Env.Security.JobDocAddressOverride.IsAllowed = false;
				using (var form = new FormWithDocAddressControlForTest(dummy))
				{
					form.Show();
					form.DocAddressControl.CompanyTextBoxForTest.ReadOnly = false;
					Assert(form.DocAddressControl.CompanyTextBoxForTest.ReadOnly);
				}
			}
			finally
			{
				Env.Security.GlobalChargeCodes.IsAllowed = rawRegistryValue;
			}
		}

		public void TestAdditionalAddressInformationTextBox()
		{
			var testDummy = Factory.NewWithValidTestData<DummyWithDocAddress>();
			testDummy.DocAddress.E2_AddressOverride = true;
			testDummy.DocAddress.E2_AdditionalAddressInformation = "xxxxx";
			Factory.Save();

			using (var form = new FormWithDocAddressControlForTest(testDummy))
			{
				form.Show();
				form.DocAddressControl.SetOverrideAddressVisibility(true);
				AssertEquals("Control should be visible to test", true, form.DocAddressControl.AdditionalAddressInformationTextBoxForTest.Visible);

				var testControl = form.DocAddressControl;

				AssertEquals("AdditionalAddressInformationTextBox has correct caption", "Note", testControl.AdditionalAddressInformationTextBoxForTest.CaptionResourceString.Caption);
				AssertEquals("AdditionalAddressInformationTextBox gets correct value from DB", "XXXXX", testControl.AdditionalAddressInformationTextBoxForTest.Text);
			}
		}

		[RequiresSTA]
		public void TestWarningNotificationShowsOnFormLoad_WhenJobDocAddressIsInvalid()
		{
			var inactiveOrg = Factory.NewWithValidTestData<OrgAddress>();
			inactiveOrg.OA_IsActive = false;
			inactiveOrg.OA_RN_NKCountryCode = "US";

			var docAddress = Factory.NewWithValidTestData<DummyWithDocAddress>();
			docAddress.DocAddress.E2_OA_Address = inactiveOrg.PK;
			docAddress.DocAddress.E2_AddressOverride = false;
			docAddress.DocAddress.E2_RN_NKCountryCode = "US";

			Factory.Save();

			using (Env.Registry.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationsDataRegistry.Instance.DisabledAddressValidationCountries.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, new AddressValidationDisabledCountryItemCollection()))
			using (var form = new FormWithDocAddressControlForTest(docAddress))
			{
				form.DocAddressControl.SetDataBinding(docAddress, "DocAddress");
				form.Show();
				Application.DoEvents();

				AssertHasWarning("inactive address", docAddress.DocAddress.E2_OA_AddressInfo, "This ?: Address is inactive.");
			}
		}

		public void TestNoExceptionWhenCloseSuggestionFormWithParentFormIsNull()
		{
			using (var control = new ZDocAddressControl())
			{
				var addressOverrideCheckBox = control.Controls.Find("OverrideAddressCheckbox", true)[0] as ZCheckBox;
				AssertNotNull(addressOverrideCheckBox);
				addressOverrideCheckBox.Checked = true;

				AssertNoExceptionThrown(() =>
				{
					addressOverrideCheckBox.Checked = false;
				});
			}
		}

		GroupBox FindGroupBox(Control control)
		{
			foreach (Control child in control.Controls)
			{
				GroupBox groupBox = child as GroupBox;
				if (groupBox != null && groupBox.Visible)
				{
					return groupBox;
				}
				groupBox = FindGroupBox(child);
				if (groupBox != null && groupBox.Visible)
				{
					return groupBox;
				}
			}
			return null;
		}

		OrgHeader TestOrganization
		{
			get
			{
				var result = Factory.NewWithValidTestData<OrgHeader>();
				var resultAddress = result.MainAddress;
				result.OH_FullName = "SKVISNVSDILXKCJ";
				resultAddress.OA_CompanyNameOverride = result.OH_FullName;
				resultAddress.OA_Address1 = "SKVISNVSDILXKCJ";
				resultAddress.OA_Address2 = "SKVISNVSDILXKCJ";
				resultAddress.OA_City = "SKVISNVSDILXKCJ";
				resultAddress.OA_State = "NSW";
				resultAddress.OA_Email = "test@test.com";
				resultAddress.OA_Phone = "56181271248";
				resultAddress.OA_Fax = "1241240128";
				Factory.Save();
				return result;
			}
		}

		void OverrideGroupBoxReadOnlyWhenDoNotHaveSecurityRight(ZDocAddressControlDisplayMode displayMode)
		{
			var rawRegistryValue = Env.Security.GlobalChargeCodes.IsAllowed;

			try
			{
				var dummy = Factory.New<DummyWithDocAddress>();
				dummy.DocAddress.E2_AddressOverride = true;
				using (var form = new FormWithDocAddressControlForTest(dummy))
				{
					Env.Security.JobDocAddressOverride.IsAllowed = true;
					form.DocAddressControl.DisplayMode = displayMode;
					form.Show();
					dummy.DocAddress.E2_RN_NKCountryCode = "AU";
					form.DocAddressControl.UpdateControlLayout();
					AssertEquals(false, form.DocAddressControl.CompanyTextBoxForTest.GetReadOnly());
					AssertEquals(true, form.DocAddressControl.OverrideAddressCheckbox.Enabled);
					AssertEquals(true, form.DocAddressControl.ClearAddressFieldsButton.Enabled);
					AssertEquals(true, form.DocAddressControl.ValidateAddressButtonForTest.Enabled);
					AssertEquals(true, form.DocAddressControl.ConvertToOrganizationButtonForTest.Enabled);

					Env.Security.JobDocAddressOverride.IsAllowed = false;
					form.DocAddressControl.UpdateControlLayout();
					Application.DoEvents();
					AssertEquals(true, form.DocAddressControl.CompanyTextBoxForTest.GetReadOnly());
					AssertEquals(false, form.DocAddressControl.OverrideAddressCheckbox.Enabled);
					AssertEquals(false, form.DocAddressControl.ClearAddressFieldsButton.Enabled);
					AssertEquals(false, form.DocAddressControl.ValidateAddressButtonForTest.Enabled);
					AssertEquals(false, form.DocAddressControl.ConvertToOrganizationButtonForTest.Enabled);
				}
			}
			finally
			{
				Env.Security.GlobalChargeCodes.IsAllowed = rawRegistryValue;
			}
		}

		void PopulateDummyAddress()
		{
			var org = Factory.New<OrgHeader>();
			DummyParent.DocAddress.OrganisationPK = org.PK;

			org.OH_RL_NKClosestPort = "AUSYD";
			org.OH_FullName = "Some Company Name";

			var address = org.MainAddress;
			address.OA_OH = org.PK;
			address.OA_Address1 = "Bourton  Hello  noone   ";
			address.OA_Address2 = "26 Myrtle   Street";
			address.OA_City = "Prospect    Blacktown";
			address.OA_State = "NSW";
			address.OA_PostCode = "2149";
		}

		protected override void TearDown()
		{
			Balloon.Instance.Hide();
			base.TearDown();
		}

		DummyWithDocAddress dummyParent;
		DummyWithDocAddress DummyParent => dummyParent ?? (dummyParent = Factory.New<DummyWithDocAddress>());

		sealed class FormWithDocAddressControlForTest : ZChildForm
		{
			public FormWithDocAddressControlForTest(DummyWithDocAddress dummy)
				: base(dummy)
			{
			}

			public FormWithDocAddressControlForTest(DummyWithDocAddressCollection dummyCollection)
				: base(dummyCollection)
			{
			}

			public ZDocAddressControlForTest DocAddressControl;

			protected override void InitializeComponent()
			{
				DocAddressControl = new ZDocAddressControlForTest();
				DocAddressControl.BindTo = "DocAddress";
				DocAddressControl.BindToContacts = "Contacts";
				DocAddressControl.BindToOrganisations = "Organisations";
				Controls.Add(DocAddressControl);

				base.InitializeComponent();
			}

			protected override void Dispose(bool disposing)
			{
				if (disposing)
				{
					if (DocAddressControl != null)
					{
						DocAddressControl.Dispose();
					}
				}
				base.Dispose(disposing);
			}
		}

		sealed class ZDocAddressControlForTest : ZDocAddressControl
		{
			internal ZDocAddressControlForTest() : base() { }

			internal ZTabPage AddressTabPageForTest => AddressTabPage;

			internal ZTabPage ContactTabPageForTest => ContactTabPage;

			internal ZTabPage GovernmentRegistrationTabPageForTest => GovernmentRegistrationTabPage;

			internal ZTextBox CityTextBoxForTest => CityTextBox;

			internal ZButton ConvertToOrganizationButtonForTest => convertToOrganizationButton;

			internal ZButton ValidateAddressButtonForTest => ValidateAddressButton;

			internal ZTextBox CompanyTextBoxForTest => CompanyTextBox;

			internal ZTextBox AdditionalAddressInformationTextBoxForTest => AdditionalAddressInformationTextBox;

			internal ZTextBox AddressLine1TextBoxForTest => AddressLine1TextBox;

			internal ZTextBox AddressLine2TextBoxForTest => AddressLine2TextBox;

			internal new ZGroupBox CutDownGroupBox => base.CutDownGroupBox;

			internal ZTextBox PostcodeTextBoxForTest => PostCodeTextBox;

			internal ZTextBox GovernmentRegistrationNumberTextBoxForTest => GovernmentRegistrationNumberTextBox;

			internal ZCheckBox ResidentialAddressCheckBoxForTest => ResidentialAddressCheckBox;

			internal ZDropEditWithFixedWidth AddressTypeDropEditForTest => AddressTypeDropEdit;

			internal LabelCaptionRenderProvider LabelCaptionRenderProvider_Exposed => LabelCaptionRenderProvider;

			internal ZButton AddressValidationStatusButton_Exposed => AddressValidationStatusButton;

			internal void AddressValidationStatusButtonClick() => AddressValidationStatusButton_Click(this, EventArgs.Empty);

			internal PhoneNumberUserControl PhoneNumberControl_Exposed => PhoneNumberControl;

			internal PhoneNumberUserControl MobilePhoneNumberControl_Exposed => MobilePhoneNumberControl;

			internal PhoneNumberUserControl FaxNumberControl_Exposed => FaxNumberControl;

			internal ZTextBox Address1Control_Exposed => Address1Control;

			internal ZTextBox Address2Control_Exposed => Address2Control;

			internal ZTextBox CityControl_Exposed => CityControl;

			internal ZTextBox PostcodeControl_Exposed => PostcodeControl;

			internal ZDropEdit StateControl_Exposed => StateControl;

			internal ZCodeFindBox CountryControl_Exposed => CountryControl;

			internal ZButton ValidateAddressButton_Exposed => ValidateAddressButton;

			internal ZDropEdit AddressEditControl_Exposed => DefaultGroupBox.AddressEdit;

			internal bool ShouldValidateAddressForTest() => ShouldValidateAddress();

			internal ZBool ClearFieldsButtonEffectiveCore { get; set; }

			internal OrgHeader GenerateTemporaryOrganization_Exposed(OrgHeader inputOrganization) => GenerateTemporaryOrganization(inputOrganization);

			internal Func<BusinessObjectFactory, OrgAddress, AddressFormatter> AddressFormatter => GetAddressFormatter();

			protected override ZBool ClearFieldsButtonEffective => base.ClearFieldsButtonEffective && ClearFieldsButtonEffectiveCore;
		}

		sealed class DummyWithDocAddress : DummyBusinessObject, IDocAddresses
		{
			public DummyWithDocAddress(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public JobDocAddress DocAddress
			{
				get
				{
					if (fDocAddress == null)
					{
						fDocAddress = JobDocAddress.New(this);
						DocAddresses.Add(fDocAddress);
					}
					return fDocAddress;
				}
			}

			public OrgHeaderCollection Organisations
			{
				get
				{
					if (fOrganisations == null)
					{
						ZQuery orgFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "O");
						fOrganisations = new OrgHeaderCollection(Factory, orgFilter);
					}
					return fOrganisations;
				}
			}

			public OrgContactCollection Contacts
			{
				get
				{
					if (fContacts == null)
					{
						ZQuery contactFilter = new ZQuery(OrgHeaderSchema.OH_FullName, SQLComparisonOperator.StartsWith, "C");
						fContacts = new OrgContactCollection(Factory, contactFilter);
					}
					return fContacts;
				}
			}

			public override void Delete()
			{
				DocAddress.Delete();
				base.Delete();
			}

			JobDocAddress fDocAddress;
			OrgHeaderCollection fOrganisations;
			OrgContactCollection fContacts;

			#region IDocAddresses Members

			public JobDocAddressDependentCollection DocAddresses
			{
				get
				{
					if (fDocAddresses == null)
					{
						fDocAddresses = new JobDocAddressDependentCollection(this);
					}

					return fDocAddresses;
				}
			}

			IReadOnlyList<DocAddressType> IDocAddresses.SupportedAddressTypes
			{
				get { return Array.Empty<DocAddressType>(); }
			}

			JobDocAddressRequirement IDocAddresses.GetDocAddressRequirement(DocAddressType addressType)
			{
				return null;
			}

			void IDocAddresses.DocAddressChanged(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OnBeforeDocAddressDeleted(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OrgAddressBeforeChange(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.AnyAddressFieldBeforeChange(JobDocAddress docAddress)
			{
			}

			void IDocAddresses.OrgHeaderAfterChange(JobDocAddress docAddress)
			{
			}

			JobDocAddressDependentCollection fDocAddresses;

			SecurityCheckpoint IDocAddresses.GetCanOverrideCheckpoint(JobDocAddress docAddress)
			{
				return null;
			}

			ZValidation IDocAddresses.PiggyBackedDocAddressValidation(JobDocAddress addressToValidate)
			{
				return null;
			}

			bool IDocAddresses.CanDeleteAddress(JobDocAddress docAddress)
			{
				return false;
			}

			OrgHeaderCollection IDocAddresses.GetOrgHeaderList(DocAddressType addressType)
			{
				return null;
			}

			#endregion
		}

		sealed class DummyWithDocAddressCollection : BusinessObjectCollection<DummyWithDocAddress>
		{
			public DummyWithDocAddressCollection(BusinessObjectFactory factory)
				: base(factory)
			{
			}
		}
	}
}
