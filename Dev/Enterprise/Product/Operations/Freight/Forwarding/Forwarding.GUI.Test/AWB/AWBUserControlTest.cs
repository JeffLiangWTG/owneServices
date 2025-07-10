using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.DocumentEngine;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Business.AWB;
using Enterprise.Freight.Forwarding.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.GUI.AWB.Testing
{
	public class AWBUserControlTest : TestCaseWithFactory
	{
		public void TestNoExceptionThrownWhenCreateAWBUserControl()
		{
			using (var stream = typeof(AWBUserControl).Assembly.GetManifestResourceStream("Enterprise.Freight.Forwarding.GUI.AWB.AWBBackgroundImage.png"))
			{
				AssertNotNull(stream);
				AssertNotNull(Image.FromStream(stream));
			}

			AssertNoExceptionThrown(() =>
			{
				using (var control = new AWBUserControl())
				{
					AssertNotNull(control.AWBImagelabel.Image);
				}
			});
		}

		public void TestIsThereSpaceForNextCharacter()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (Form form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				form.Controls.Add(testControl);
				testControl.SetDataBinding(shipment, "");

				String str = "Mohsen";

				int width = (new TextSizeCalculator(new Font("Arial", 9))).GetLengthInMillimeter(str);

				AssertEquals(true, testControl.IsThereSpaceForNextCharacter("", str.Length, width));
				AssertEquals(true, testControl.IsThereSpaceForNextCharacter(" ", str.Length, width));
				AssertEquals(true, testControl.IsThereSpaceForNextCharacter(str, str.Length, width));
				AssertEquals(false, testControl.IsThereSpaceForNextCharacter(str, str.Length, width - 1));
				AssertEquals(false, testControl.IsThereSpaceForNextCharacter(str, str.Length - 1, width));
				AssertEquals(false, testControl.IsThereSpaceForNextCharacter(str, str.Length - 1, width - 1));
			}
		}

		public void TestStress()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (Form form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				form.Controls.Add(testControl);
				testControl.SetDataBinding(shipment, "");
				String str = "Mohsen";

				int width = (new TextSizeCalculator(new Font("Arial", 9))).GetLengthInMillimeter(str);

				long number = 1000000;
				string longString = new String('W', 10000);
				int counter;
				bool flag = true;
				for (counter = 0; counter < number; counter++)
				{
					flag |= testControl.IsThereSpaceForNextCharacter("", 100, 1000);
				}
				AssertEquals(counter, number);
			}
		}

		public void TestShipperNameTextBox_TextChanged()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (Form form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				form.Controls.Add(testControl);
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);
				testControl.ShipperNameTextBox.SelectionStart = 0;
				testControl.ShipperNameTextBox.Text = new ZString('W', 100);
				AssertEquals("The Text must me trimed to fit", new ZString('W', 48), testControl.ShipperNameTextBox.Text);
			}
		}

		public void TestAddressOverrideTextBox_TextChange()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (Form form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);
				testControl.ConsigneeAddressTextBox.Text = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ";
				AssertEquals("Text must be trimmed to fit", "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUV", testControl.ConsigneeAddressTextBox.Text);
			}
		}

		public void TestOverrideValuesCheckBox_WithForwardingConsolAndPermission_Succeeds()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAWBOverride.IsAllowed = true;

			using (var form = new ZForm())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var awbControl = new AWBUserControl();
				awbControl.SetDataBinding(forwardingConsol, "");

				form.Controls.Add(awbControl);

				var overrideCheckbox = (ZCheckBox)awbControl.Controls.Find("OverrideValuesCheckBox", true)[0];
				var clickEventHandler = awbControl.GetType().GetMethod("OverrideValuesCheckBox_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				clickEventHandler.Invoke(awbControl, new object[] { overrideCheckbox, null });

				Assert("Clicking OverrideValuesCheckBox on Forwarding Consolidations (with permission) should not display an error", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOverrideValuesCheckBox_WithForwardingConsolAndWithoutPermission_DisplaysError()
		{
			var forwardingConsol = Factory.New<ForwardingConsol>();
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainConsolAWBOverride.IsAllowed = false;

			using (var form = new ZForm())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var awbControl = new AWBUserControl();
				awbControl.SetDataBinding(forwardingConsol, "");

				form.Controls.Add(awbControl);

				var overrideCheckbox = (ZCheckBox)awbControl.Controls.Find("OverrideValuesCheckBox", true)[0];
				var clickEventHandler = awbControl.GetType().GetMethod("OverrideValuesCheckBox_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				clickEventHandler.Invoke(awbControl, new object[] { overrideCheckbox, null });

				var expectedErrorMessage = Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainConsolAWBOverride);

				CombineAssertions("Clicking OverrideValuesCheckBox on Forwarding Consolidations (without permission) should fail with error", () =>
				{
					AssertEquals("Error message should explain lack of MaintainConsolAWBOverride permission", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Error message should have displayed", UnitTestUserNotification.Instance.LastMessage.WasError);
				});
			}
		}

		public void TestOverrideValuesCheckBox_WithForwardingShipmentAndPermission_Succeeds()
		{
			var forwardingShipment = Factory.New<ForwardingShipment>();
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainShipmentAWBOverride.IsAllowed = true;

			using (var form = new ZForm())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var awbControl = new AWBUserControl();
				awbControl.SetDataBinding(forwardingShipment, "");

				form.Controls.Add(awbControl);

				var overrideCheckbox = (ZCheckBox)awbControl.Controls.Find("OverrideValuesCheckBox", true)[0];
				var clickEventHandler = awbControl.GetType().GetMethod("OverrideValuesCheckBox_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				clickEventHandler.Invoke(awbControl, new object[] { overrideCheckbox, null });

				Assert("Clicking OverrideValuesCheckBox on Forwarding Shipments (with permission) should not display an error", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestOverrideValuesCheckBox_WithForwardingShipmentAndWithoutPermission_DisplaysError()
		{
			var forwardingShipment = Factory.New<ForwardingShipment>();
			var securityInstance = SecurityTestHelper.CreateSecurityInstance(Factory);
			securityInstance.MaintainShipmentAWBOverride.IsAllowed = false;

			using (var form = new ZForm())
			using (Env.SetTemporarySecurityInstanceForTest(securityInstance))
			{
				var awbControl = new AWBUserControl();
				awbControl.SetDataBinding(forwardingShipment, "");

				form.Controls.Add(awbControl);

				var overrideCheckbox = (ZCheckBox)awbControl.Controls.Find("OverrideValuesCheckBox", true)[0];
				var clickEventHandler = awbControl.GetType().GetMethod("OverrideValuesCheckBox_Click", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

				clickEventHandler.Invoke(awbControl, new object[] { overrideCheckbox, null });

				var expectedErrorMessage = Env.Security.GetErrorMessageForNotAllowed(Env.Security.MaintainShipmentAWBOverride);

				CombineAssertions("Clicking OverrideValuesCheckBox on Forwarding Shipments (without permission) should fail with error", () =>
				{
					AssertEquals("Error message should explain lack of MaintainShipmentAWBOverride permission", expectedErrorMessage, UnitTestUserNotification.Instance.LastMessage.Text);
					Assert("Error message should have displayed", UnitTestUserNotification.Instance.LastMessage.WasError);
				});
			}
		}

		public void TestAWBDescription2()
		{
			ForwardingShipment shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (Form form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);
				AssertEquals("Desciption2 on AWBUserControl",
					"It is agreed that the goods described herein are accepted in apparent good order and condition (except as noted) for carriage SUBJECT TO THE CONDITIONS OF CONTRACT ON THE REVERSE HEREOF. ALL GOODS MAY BE CARRIED BY ANY OTHER MEANS INCLUDING ROAD OR ANY SHIPPER, AND SHIPPER AGREES THAT THE SHIPMENT MAY BE CARRIED VIA INTERMEDIATE STOPPING PLACES WHICH THE CARRIER DEEMS APPROPRIATE. THE SHIPPER'S ATTENTION IS DRAWN TO THE NOTICE CONCERNING CARRIER'S LIMITATION OF LIABILITY. Shipper may increase such limitation of liability by declaring a higher value for carriage and paying a supplemental charge if required.",
					testControl.ZLabel88.Text);
			}
		}

		public void TestOverrideTabsVisibility()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			using (var form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);

				Assert("Pre-condition: Tab should not be visible when first loaded as override defaults to false", !testControl.ShipperNameAndAddressOverrideTabPage.TabVisible);
				Assert("Pre-condition: Tab should not be visible when first loaded as override defaults to false", !testControl.ConsigneeOverrideTabPage.TabVisible);
				Assert("Pre-condition: Tab should not be visible when first loaded as override defaults to false", !testControl.NotifyAddressOverrideTabPage.TabVisible);

				testControl.OverrideConsigneeAddressCheckBox.Checked = true;
				Assert("Should still not be visible", !testControl.ShipperNameAndAddressOverrideTabPage.TabVisible);
				Assert("Should now be visible", testControl.ConsigneeOverrideTabPage.TabVisible);
				Assert("Should still not be visible", !testControl.NotifyAddressOverrideTabPage.TabVisible);

				testControl.OverrideShipperAddressCheckBox.Checked = true;
				testControl.OverrideConsigneeAddressCheckBox.Checked = false;
				testControl.EH_IsNotifyOverridenCheckBox.Checked = true;
				Assert("Should be visible", testControl.ShipperNameAndAddressOverrideTabPage.TabVisible);
				Assert("Should not be visible", !testControl.ConsigneeOverrideTabPage.TabVisible);
				Assert("Should be visible", testControl.NotifyAddressOverrideTabPage.TabVisible);
			}
		}

		public void TestDataBindingResetWhenAWBHeaderDeletedByUniqueIndexHandler()
		{
			var factoryEdi1 = new BusinessObjectFactory();
			factoryEdi1.RefreshEnabled = false;

			var consol1 = factoryEdi1.New<ForwardingConsol>();
			consol1.JK_TransportMode = Constants.TransportModes.Air;

			factoryEdi1.Save();

			var factoryEdi2 = new BusinessObjectFactory();
			factoryEdi2.RefreshEnabled = false;

			var consol2 = factoryEdi2.Load<ForwardingConsol>(consol1.PK);
			consol2.JK_TransportMode = Constants.TransportModes.Air;

			using (var form = new ZForm())
			{
				var awbControl = new AWBUserControl();
				awbControl.SetDataBinding(consol2, "");
				form.Controls.Add(awbControl);
				form.Show();

				var awbHeader1 = consol1.AWBHeader;
				consol1.IsAWBValuesOverriddenProperty = true;

				var awbHeader2 = consol2.AWBHeader;
				consol2.IsAWBValuesOverriddenProperty = true;

				factoryEdi1.Save();

				try
				{
					factoryEdi2.Save();
					Fail("Save should fail due to another AWBHeader already being saved by another edi instance for the same parent consol.");
				}
				catch (ZSaveException ex)
				{
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					ZExceptionReporting.HandleSaveException(ex);
				}

				AssertEquals("Control should be bound to the AWBHeader saved in another edi instance.", consol1.AWBHeader.PK, ((IAWBParent)awbControl.CurrentDataItem).AWBHeader.PK);
				AssertNoExceptionThrown("No exception expected, as binding context was reset and control is bound to correct AWBHeader.", factoryEdi2.Save);
			}
		}

		[RequiresSTA]
		public void TestAWBAddressSaveButton_CheckOrgAddressCapabilities()
		{
			var originalValue = Env.Security.OrgAddressCapabilitiesModify.IsAllowed;
			Env.Security.OrgAddressCapabilitiesModify.IsAllowed = false;

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			using (var form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);
				shipment.AWBHeader.Populate();
				shipment.JS_OverrideWaybillDefaults = true;
				form.Show();

				testControl.SaveConsigneeAddress();
				AssertEquals("You do not have the security right to modify addresses of an organization. Please check with your system administrator if you require access to this function.", UnitTestUserNotification.Instance.LastMessage.Text);

				Env.Security.OrgAddressCapabilitiesModify.IsAllowed = originalValue;
			}
		}

		public void TestAWBAddressSaveButton_OrgAddressIsNull()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;

			Factory.Save();

			using (var form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);
				shipment.AWBHeader.Populate();
				shipment.JS_OverrideWaybillDefaults = true;
				form.Show();

				testControl.SaveConsigneeAddress();
				AssertEquals("Cannot save this address as there is no real organization for this address.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShowOrganisationFormForSavingShipperAWBAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var consignor = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsignorPK = consignor.PK;

			Factory.Save();

			using (var form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);
				shipment.AWBHeader.Populate();
				shipment.JS_OverrideWaybillDefaults = true;

				form.Show();

				testControl.SaveShipperAddress();
				var organisationForm = testControl.LastOrganisationControllerForTesting.LastShownForm as ZOrganisationsForm;

				AssertNotNull(organisationForm);
				organisationForm.Dispose();
			}
		}

		public void TestShowOrganisationFormForSavingConsigneeAWBAddress()
		{
			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Constants.TransportModes.Air;
			var consignee = Factory.NewWithValidTestData<OrgHeader>();
			shipment.ConsigneePK = consignee.PK;

			Factory.Save();

			using (var form = new Form())
			{
				var testControl = new AWBUserControlTestClass();
				testControl.SetDataBinding(shipment, "");
				form.Controls.Add(testControl);
				shipment.AWBHeader.Populate();
				shipment.JS_OverrideWaybillDefaults = true;
				form.Show();

				testControl.SaveConsigneeAddress();
				var organisationForm = testControl.LastOrganisationControllerForTesting.LastShownForm as ZOrganisationsForm;

				AssertNotNull(organisationForm);
				organisationForm.Dispose();
			}
		}

		public void TestSecurityStatusVisibility()
		{
			var shipment = Factory.New<ForwardingShipment>();

			using (var testControl = new AWBUserControlTestClass())
			{
				var securityStatusTextBox = testControl.Controls.Find("EH_SecurityStatusTextBox", true)[0] as ZTextBox;

				CombineAssertions(delegate
				{
					AssertNoExceptionThrown("Should not throw an exception even without AWB Header", () => testControl.SetDataBinding(shipment, ""));
					AssertNull("Should have no export header", testControl.ExportAWB);
					Assert("Should be invisible if no ExportAWB", !securityStatusTextBox.IsVisibleForBinding);
				});

				var mock = Factory.New<ExportAWBHeaderForTest>();
				testControl.ExportAWB = mock;
				testControl.SetDataBinding(shipment, "");

				CombineAssertions(delegate
				{
					Assert("Should be true", securityStatusTextBox.IsVisibleForBinding);
					AssertEquals("Should use SecurityStatusAWBVisibility from the exportAWB rather than defaulting",
						securityStatusTextBox.IsVisibleForBinding, testControl.ExportAWB.SecurityStatusAWBVisibility);
				});
			}
		}

		class AWBUserControlTestClass : AWBUserControl
		{
			public ExportAWBHeader ExportAWB
			{
				get { return base.exportAWB; }
				set { base.exportAWB = value; }
			}

			public new ZController LastOrganisationControllerForTesting
			{
				get { return base.LastOrganisationControllerForTesting; }
				set { base.LastOrganisationControllerForTesting = value; }
			}

			public ZTextBox ShipperNameTextBox => this.Controls.Find("ShipperNameTextBox", true)[0] as ZTextBox;

			public ZTextBox ConsigneeAddressTextBox => this.Controls.Find("ConsigneeAddressTextBox", true)[0] as ZTextBox;

			public ZTabPage ShipperNameAndAddressOverrideTabPage => FindTabPage(ShipperAddressTabControl, "ShipperNameAndAddressOverrideTabPage");

			public ZTemplateTabControl ShipperAddressTabControl => this.Controls.Find("ShipperAddressTabControl", true)[0] as ZTemplateTabControl;

			public ZTabPage ConsigneeOverrideTabPage => FindTabPage(ConsigneeNameAndAddressTabControl, "ConsigneeOverrideTabPage");

			public ZTemplateTabControl ConsigneeNameAndAddressTabControl => this.Controls.Find("ConsigneeNameAndAddressTabControl", true)[0] as ZTemplateTabControl;

			public ZTabPage NotifyAddressOverrideTabPage => FindTabPage(AccountingInformationTabControl, "NotifyAddressOverrideTabPage");

			public new ZTemplateTabControl AccountingInformationTabControl => base.AccountingInformationTabControl;

			public ZCheckBox EH_IsNotifyOverridenCheckBox => this.Controls.Find("EH_IsNotifyOverridenCheckBox", true)[0] as ZCheckBox;

			public ZCheckBox OverrideShipperAddressCheckBox => this.Controls.Find("OverrideShipperAddressCheckBox", true)[0] as ZCheckBox;

			public ZCheckBox OverrideConsigneeAddressCheckBox => this.Controls.Find("OverrideConsigneeAddressCheckBox", true)[0] as ZCheckBox;

			public ZLabel ZLabel88 => this.Controls.Find("zLabel88", true)[0] as ZLabel;

			public new bool IsThereSpaceForNextCharacter(string text, int maxFieldWidthInDB, int maxTextWidthInMillimeters)
			{
				return base.IsThereSpaceForNextCharacter(text, maxFieldWidthInDB, maxTextWidthInMillimeters);
			}

			public void SaveShipperAddress()
			{
				var button = this.Controls.Find("ShipperAddressSaveButton", true)[0] as ZButton;
				button.PerformClick();
			}

			public void SaveConsigneeAddress()
			{
				var button = this.Controls.Find("ConsigneeAddressSaveButton", true)[0] as ZButton;
				button.PerformClick();
			}

			public void SaveAlsoNotifyAddress()
			{
				var button = this.Controls.Find("AlsoNotifyAddressSaveButton", true)[0] as ZButton;
				button.PerformClick();
			}

			ZTabPage FindTabPage(ZTemplateTabControl control, string tabPageName)
			{
				ZTabPage foundTabPage = null;
				foreach (ZTabPage tabPage in control.AllTabPages)
				{
					if (tabPage.Name == tabPageName)
					{
						foundTabPage = tabPage;
						break;
					}
				}
				return foundTabPage;
			}
		}

		class ExportAWBHeaderForTest : ShipmentExportAWBHeader
		{
			public ExportAWBHeaderForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public override bool SecurityStatusAWBVisibility => true;
		}
	}
}
