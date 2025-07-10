using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.Registry.Business;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	public class LoadEntryFormTest : WhsGuiTestCaseWithFactory
	{
		#region TestFormCaption

		public void TestFormCaption()
		{
			var load = GetLoad();
			using (var form = new LoadEntryForm(load))
			{
				form.Show();
				AssertEquals("FormCaption", "Load Planning WL00000001", form.FormCaption);
			}

			load.WLO_JobID = "";
			using (var form = new LoadEntryForm(load))
			{
				form.Show();
				AssertEquals("FormCaption", "Load Planning", form.FormCaption);
			}
		}

		#endregion

		#region TestIsResizableByTabPageAllowed

		public void TestIsResizableByTabPageAllowed()
		{
			using (var form = new LoadEntryForm(GetLoad()))
			{
				form.Show();
				AssertEquals(true, form.IsResizableByTabPageAllowed);
			}
		}

		#endregion

		#region TestOnLoad

		public void TestOnLoad()
		{
			using (var form = new LoadEntryFormForTest(GetLoad()))
			{
				form.Show();

				var fileNewMenuItem = form.FileMenuItemForTest.MenuItems.FindByName(ZFormMenuStrategy.FileNewMenuItemName);
				Assert("New action should be disabled", fileNewMenuItem == null || !fileNewMenuItem.Enabled);
			}
		}

		#endregion

		#region TestWorkflowTabpage

		public void TestWorkflowTabpage()
		{
			using (var form = new LoadEntryFormForTest(GetLoad()))
			{
				form.Show();
				var tabControl = form.FindAll<ZTemplateTabControl>().First();
				AssertNotEquals("WorkflowTabPage visible", -1, tabControl.TabPages.IndexOf(tabControl.GetTabPage("WorkflowTabPage")));
			}
		}

		#endregion

		#region TestSupportEDocs

		public void TestSupportEDocs()
		{
			using (var form = new LoadEntryFormForTest(GetLoad()))
			{
				AssertEquals("EDocs should be enabled", true, form.SupportsEDocsForTest);
			}
		}

		#endregion

		#region TestDetailsTab

		public void TestDetailsTab()
		{
			using (var form = new LoadEntryForm(GetLoad()))
			{
				form.Show();

				var jobIdTextBox = form.Controls.Find("JobIdTextBox", true)[0] as ZTextBox;
				AssertEquals("Should be visible", true, jobIdTextBox.Visible);
				AssertEquals("Value of jobIdTextBox should be WL00000001", "WL00000001", jobIdTextBox.Text);
			}
		}

		#endregion

		#region TestGlowLinkLabel_LinkClicked

		public void TestGlowLinkLabel_LinkClicked()
		{
			var whsLoad = GetLoad();
			using (var form = new LoadEntryFormForTest(whsLoad))
			{
				form.Show();

				var label = form.FindSingle<ZLinkLabel>("WarehouseLoadGlowLinkLabel");
				AssertNotNull("Link Label should exist.", label);
				Assert("Link Label should be visible", label.Visible);

				GlowRegistry.Instance.GlowPortalsUri.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "https://address/");

				var staff = Helper.CreateGlbStaff("ABC", "ABC");
				Factory.Save();

				using (Env.SetTemporaryUserContext(staff.PK.ToGuid(), Env.CurrentBranchPK, Env.CurrentDepartmentPK))
				{
					label.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
					var launchedUrl = WebUrlLauncher.LastUrlLaunched;
					var uri = new Uri(launchedUrl, UriKind.Absolute);

					AssertEquals("Launched uri is correct.", "https", uri.Scheme);
					AssertEquals("Launched uri is correct.", "address", uri.Host);
					AssertEquals("Launched uri is correct.", "/goto/ViewProductWarehouseLoad", uri.AbsolutePath);
					Assert("Launched uri is correct.", uri.Query.Contains("entityPK=" + whsLoad.PK.ToString()));
				}
			}
		}

		public void TestGlowLinkLabel_LinkClicked_NoGlowConfig()
		{
			var whsLoad = GetLoad();
			using (var form = new LoadEntryFormForTest(whsLoad))
			{
				form.Show();

				var label = form.FindSingle<ZLinkLabel>("WarehouseLoadGlowLinkLabel");
				label.OnLinkClicked_Exposed(new LinkLabelLinkClickedEventArgs(null));
				Assert("Notification type is correct.", UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Notification message is correct.", @"This Load Planning WL00000001 cannot be opened in a browser as GLOW has not been configured for this client.
Registry: GLOW/Services/GLOW Portals Root URL", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		WhsLoad GetLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var dockDoorLocation = data.Whs1.DefaultOutboundDockDoorLocation;

			var transportCompany = Helper.CreateClient("TC1");
			var carrierServicelevel = transportCompany.MiscServ.CarrierServiceLevels.AddNew();
			carrierServicelevel.PL_Code = "RD";
			carrierServicelevel.PL_CarrierServiceLevelDescription = "Road";

			var truck = Helper.CreateEquipment("T001", 1m, Core.Constants.Weight.Kilograms, 1m, Core.Constants.Volume.CubicMetres);
			return Helper.CreateWhsLoad(transportCompany, dockDoorLocation, "WL00000001", carrierServicelevel.PL_Code, truck);
		}

		#endregion
	}

	public class LoadEntryFormForTest : LoadEntryForm
	{
		public LoadEntryFormForTest(WhsLoad whsLoad)
			: base(whsLoad)
		{
		}

		public MenuItem FileMenuItemForTest => FileMenuItem;
		public bool SupportsEDocsForTest => SupportsEDocs;
	}
}
