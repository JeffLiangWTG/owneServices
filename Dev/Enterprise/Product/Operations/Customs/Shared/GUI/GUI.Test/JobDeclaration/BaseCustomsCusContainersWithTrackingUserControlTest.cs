using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;

namespace Enterprise.Customs.GUI.Testing
{
	public class BaseCustomsCusContainersWithTrackingUserControlTest : TestCaseWithFactory
	{
		public void TestTabFocus()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				BaseCustomsCusContainersWithTrackingUserControl testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				AssertEquals("Export Tab has focus", "ExportTabPage", testUserControl.containersUserControl1.DetailTabControl.SelectedTab.Name);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = DefaultImportMessageType;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				AssertEquals("Import Tab has focus", "ImportTabPage", testUserControl.containersUserControl1.DetailTabControl.SelectedTab.Name);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = DefaultExportMessageType;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				AssertEquals("Export Tab has focus", "ExportTabPage", testUserControl.containersUserControl1.DetailTabControl.SelectedTab.Name);
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.DeclarationTabPage;
				testForm.CustomsBrokerageUserControl.JobDeclaration.JE_MessageType = DefaultOtherMessageType;
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				AssertEquals("Import Tab has focus", "ImportTabPage", testUserControl.containersUserControl1.DetailTabControl.SelectedTab.Name);
			}
		}

		public void TestDoNotCheckSecurityRightOfContainerWhenContainersAreDeleted()
		{
			var declaration = BaseJobDeclaration.New(Factory);
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			var container = declaration.CusContainers.AddNew();
			Enterprise.Environment.Env.Security.ForwardingContainer.IsAllowed = false;
			using (BaseJobDeclarationForm decForm = new BaseJobDeclarationForm(declaration))
			{
				decForm.Show();
				decForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = decForm.CustomsBrokerageUserControl.ContainerTabPage;
				Application.DoEvents();
				var testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)decForm.CustomsBrokerageUserControl.ContainerUserControl;
				testUserControl.CusContainersBoundGrid.SelectFirstRowIfOnlyRowInGrid();
				Application.DoEvents();
				KeySender.PostKeyDown(testUserControl.CusContainersBoundGrid.InnerGrid, Keys.Delete);
				Application.DoEvents();
				Assert("container is deleted", container.IsDeleted);
			}
		}

		public void TestVisibilityOfAutoAssignContainerToInvoiceContextMenuItem()
		{
			var declaration = Factory.New<BaseJobDeclaration>();
			declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
			var container = declaration.CusContainers.AddNew();
			var inv = declaration.Invoices.AddNew();
			var line = inv.InvoiceLines.AddNew();
			string menuItemCaption = "&Assign container to invoice lines";
			using (var testForm = new BaseJobDeclarationForm(declaration))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				var testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				var grid = testUserControl.CusContainersBoundGrid;
				var mnu = grid.InnerGrid.ContextMenu;
				mnu.ShowPopupMenu();
				AssertEquals("Container menu item is not available when no containers selected", false, IsMenuItemVisible(mnu.MenuItems, menuItemCaption));
				grid.InnerGrid.SelectAllElements();
				mnu.ShowPopupMenu();
				AssertEquals("Container menu item is available when container selected", true, IsMenuItemVisible(mnu.MenuItems, menuItemCaption));
				var container2 = declaration.CusContainers.AddNew();
				grid.InnerGrid.SelectAllElements();
				mnu.ShowPopupMenu();
				AssertEquals("Container menu item is not available when multiple containers selected", false, IsMenuItemVisible(mnu.MenuItems, menuItemCaption));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Taiwan))
			{
				declaration = Factory.New<BaseJobDeclaration>();
				declaration.JE_TransportMode = declaration.TransportModeSeaCodeForTesting;
				container = declaration.CusContainers.AddNew();
				inv = declaration.Invoices.AddNew();
				line = inv.InvoiceLines.AddNew();
				using (var testTWForm = new BaseJobDeclarationForm(declaration))
				{
					testTWForm.Show();
					testTWForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testTWForm.CustomsBrokerageUserControl.ContainerTabPage;
					var testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testTWForm.CustomsBrokerageUserControl.ContainerUserControl;
					var grid = testUserControl.CusContainersBoundGrid;
					var mnu = grid.InnerGrid.ContextMenu;
					mnu.ShowPopupMenu();
					AssertEquals("Container menu item is not available when no containers selected", false, IsMenuItemVisible(mnu.MenuItems, menuItemCaption));
					grid.InnerGrid.SelectAllElements();
					mnu.ShowPopupMenu();
					AssertEquals("Container menu item is available when container selected", false, IsMenuItemVisible(mnu.MenuItems, menuItemCaption));
					var container2 = declaration.CusContainers.AddNew();
					grid.InnerGrid.SelectAllElements();
					mnu.ShowPopupMenu();
					AssertEquals("Container menu item is not available when multiple containers selected", false, IsMenuItemVisible(mnu.MenuItems, menuItemCaption));
				}
			}
		}

		public virtual void TestOutturnTabVisibility1()
		{
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				BaseCustomsCusContainersWithTrackingUserControl testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				AssertEquals("Outturn tab visibility", false, testUserControl.containersUserControl1.DetailTabControl.TabPages.Contains(testUserControl.containersUserControl1.OutturnTabPage));
			}
		}

		public virtual void TestOutturnTabVisibility2()
		{
			CommonShipment shipment = Factory.New<ForwardingShipment>();
			BaseJobDeclaration testDec = BaseJobDeclaration.New(Factory);
			testDec.JE_TransportMode = testDec.TransportModeSeaCodeForTesting;
			testDec.JE_JS = shipment.PK;
			using (BaseJobDeclarationForm testForm = new BaseJobDeclarationForm(testDec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				BaseCustomsCusContainersWithTrackingUserControl testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				AssertEquals("Outturn tab visibility", true, testUserControl.containersUserControl1.DetailTabControl.TabPages.Contains(testUserControl.containersUserControl1.OutturnTabPage));
			}
		}

		public void TestVGMTabPageVisibility()
		{
			var dec = BaseJobDeclaration.New(Factory);
			dec.JE_TransportMode = dec.TransportModeSeaCodeForTesting;
			using (var testForm = new BaseJobDeclarationForm(dec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				var testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				Assert("VGM tab is visible", testUserControl.containersUserControl1.DetailTabControl.TabPages.Contains(testUserControl.containersUserControl1.VGMTabPage));
			}

			var shipment = Factory.New<ForwardingShipment>();
			dec.JE_JS = shipment.PK;
			using (var testForm = new BaseJobDeclarationForm(dec))
			{
				testForm.Show();
				testForm.CustomsBrokerageUserControl.MainTabControl.SelectedTab = testForm.CustomsBrokerageUserControl.ContainerTabPage;
				var testUserControl = (BaseCustomsCusContainersWithTrackingUserControl)testForm.CustomsBrokerageUserControl.ContainerUserControl;
				Assert("VGM tab is not visible", !testUserControl.containersUserControl1.DetailTabControl.TabPages.Contains(testUserControl.containersUserControl1.VGMTabPage));
			}
		}

		public void TestGridId()
		{
			using (var control = new BaseCustomsCusContainersWithTrackingUserControl())
			{
				AssertEquals("GridLayoutV6/wYdjdj5Cz4Nc7hGpbKA==", control.CusContainersBoundGrid.InnerGrid.GridId);
			}
		}

		protected virtual ZString DefaultImportMessageType => JobMessageTypeList.Codes.Import;

		protected virtual ZString DefaultExportMessageType => JobMessageTypeList.Codes.Export;

		protected virtual ZString DefaultOtherMessageType => JobMessageTypeList.Codes.WarehousedByExternalAgent;

		bool IsMenuItemVisible(Menu.MenuItemCollection menuItems, string menuItemText)
		{
			foreach (MenuItem item in menuItems)
			{
				if (item.Text == menuItemText)
				{
					return item.Visible;
				}
			}

			return false;
		}
	}
}
