using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(RefShippingLineForm))]
	sealed class RefShippingLineFormTest : ZFormBasherTest
	{
		public void TestTextOfForm()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			using (var form = new RefShippingLineFormForTest(shippingLine))
			{
				form.Show();
				AssertEquals("New Shipping Line", form.Text);
			}
		}

		public void TestRSL_IsActiveCheckBox_Click()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsSystem = false;
			shippingLine.RSL_IsActive = true;

			using (var form = new RefShippingLineFormForTest(shippingLine))
			{
				form.Show();
				UnitTestUserNotification.Instance.ClearMessages();
				form.RSL_IsActiveCheckBox_ClickForTest();
				AssertEquals(false, shippingLine.RSL_IsActive);
				AssertEquals("All non-system created reference files will be marked as In-Active. If you wish to register a new carrier, please raise a CR8 service request.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSetTextToolTip()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();
				var topLevelTabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
				var mainTab = form.Controls.Find("MainTabPage", true).FirstOrDefault() as ZTabPage;
				topLevelTabControl.SelectTab(mainTab.Name);
				mainTab.Focus();
				var standardCarrierAlphaCodeTip = ToolTipService.GetToolTip(form.Controls.Find("RSL_StandardCarrierAlphaCodeTextBox", true).FirstOrDefault());
				var cargoWiseOneCodeTip = ToolTipService.GetToolTip(form.Controls.Find("RSL_CargoWiseOneCodeTextBox", true).FirstOrDefault());
				AssertEquals("Standard Carrier Alpha Code", standardCarrierAlphaCodeTip);
				AssertEquals("CargoWise Code", cargoWiseOneCodeTip);
			}
		}

		public void TestDetailHeaderTextBoxes()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CarrierName = "Fake Ocean Carrier Name";
			shippingLine.RSL_StandardCarrierAlphaCode = "SCA1";
			shippingLine.RSL_CargoWiseOneCode = "C1AR";

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals(false, shippingLine.IsInDatabase);
					AssertEquals("Fake Ocean Carrier Name", form.RSL_OceanCarrierNameTextBox.Text);
					AssertEquals("SCA1", form.RSL_StandardCarrierAlphaCodeTextBox.Text);
					AssertEquals("C1AR", form.RSL_CargoWiseOneCodeTextBox.Text);
				});

				form.RSL_IsActiveCheckBox.Checked = false;
				form.RSL_OceanCarrierNameTextBox.Text = "Ocean Carrier Name";
				form.RSL_StandardCarrierAlphaCodeTextBox.Text = "SCA2";
				form.RSL_CargoWiseOneCodeTextBox.Text = "C1SL";

				//swap tabs to commit changes (mimicing code that was taken out of ZForm.cs that made this unit test pass incidentally)
				var topLevelTabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
				foreach (var tab in topLevelTabControl.TabPages.OfType<ZBindingTabPage>())
				{
					if (tab.GetType().ToString() == "Enterprise.ZArchitecture.GUI.ZStmNoteTabPage")
					{
						if (!tab.IsBound)
						{
							var currentTab = topLevelTabControl.SelectedTab;
							topLevelTabControl.SelectTab(tab.Name);
							tab.Controls[0].Focus();
							topLevelTabControl.SelectTab(currentTab.Name);
							currentTab.Controls[0].Focus();
						}
						break;
					}
				}

				form.FireSaveButton();

				CombineAssertions(() =>
				{
					AssertEquals(true, shippingLine.IsInDatabase);
					AssertEquals("Ocean Carrier Name", shippingLine.RSL_CarrierName);
					AssertEquals("SCA2", shippingLine.RSL_StandardCarrierAlphaCode);
					AssertEquals("C1SL", shippingLine.RSL_CargoWiseOneCode);
				});
			}
		}

		public void TestDetailHeaderCheckBoxes()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_IsActive = true;
			shippingLine.RSL_IsSystem = true;
			shippingLine.RSL_IsNVO = false;

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();

				CombineAssertions("Form update correctly", () =>
				{
					AssertEquals("ShippingLine object shuould not exist in Database", false, shippingLine.IsInDatabase);
					AssertEquals("RSL_IsActiveCheckBox shuould be checked", true, form.RSL_IsActiveCheckBox.Checked);
					AssertEquals("RSL_IsSystemCheckBox shuould be checked", true, form.RSL_IsSystemCheckBox.Checked);
					AssertEquals("RSL_IsNVOCheckBox shuould be checked", false, form.RSL_IsNVOCheckBox.Checked);
				});

				form.RSL_IsActiveCheckBox.Checked = false;
				form.RSL_IsSystemCheckBox.Checked = false;
				form.RSL_IsNVOCheckBox.Checked = true;

				form.FireSaveButton();

				CombineAssertions("ShippingLine update correctly", () =>
				{
					AssertEquals("ShippingLine object shuould exist in Database", true, shippingLine.IsInDatabase);
					AssertEquals("RSL_IsActive of shippingLine should be false", false, shippingLine.RSL_IsActive);
					AssertEquals("RSL_IsSystem of shippingLine should be false", false, shippingLine.RSL_IsSystem);
					AssertEquals("RSL_IsNVO of shippingLine should be true", true, shippingLine.RSL_IsNVO);
				});
			}
		}

		public void TestIntegrations()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_ContainerAutomationAvailable = true;
			shippingLine.RSL_GlobalSailingScheduleAvailable = false;
			shippingLine.RSL_InvoiceAvailable = true;
			shippingLine.RSL_OceanCarrierMessagingAvailable = false;
			shippingLine.RSL_IsActive = false;

			using (var form = new RefShippingLineForm(shippingLine))
			{
				form.Show();

				CombineAssertions(() =>
				{
					AssertEquals(false, shippingLine.IsInDatabase);
					AssertEquals(true, form.IntegrationsControl.RSL_ContainerAutomationAvailableCheckBox.Checked);
					AssertEquals(false, form.IntegrationsControl.RSL_GlobalSailingScheduleAvailableCheckBox.Checked);
					AssertEquals(true, form.IntegrationsControl.RSL_InvoiceAvailableCheckBox.Checked);
					AssertEquals(false, form.IntegrationsControl.RSL_OceanCarrierMessagingAvailableCheckBox.Checked);
				});

				form.IntegrationsControl.RSL_ContainerAutomationAvailableCheckBox.Checked = false;
				form.IntegrationsControl.RSL_GlobalSailingScheduleAvailableCheckBox.Checked = true;
				form.IntegrationsControl.RSL_InvoiceAvailableCheckBox.Checked = false;
				form.IntegrationsControl.RSL_OceanCarrierMessagingAvailableCheckBox.Checked = true;

				form.FireSaveButton();

				CombineAssertions(() =>
				{
					AssertEquals(true, shippingLine.IsInDatabase);
					AssertEquals(false, shippingLine.RSL_ContainerAutomationAvailable);
					AssertEquals(true, shippingLine.RSL_GlobalSailingScheduleAvailable);
					AssertEquals(false, shippingLine.RSL_InvoiceAvailable);
					AssertEquals(true, shippingLine.RSL_OceanCarrierMessagingAvailable);
				});
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new RefShippingLineForm(Factory.New<RefShippingLine>());
		}
	}
}
