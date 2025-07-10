using System;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(UNDGSubstanceModule))]
	sealed class UNDGSubstanceModuleTest : ZModuleBasherTest
	{
		public override void TestBusinessObjectHasIndexedPKIfExcelExportIsEnabled()
		{
			Assert("This should be implemented if a module is represented by a view", true);
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.UNDGSubstance;
		}

		#region Data Transfer Menu Items

		[RequiresSTA]
		public void TestDataTransfer_ExcelMenuItems_ShowError()
		{
			using (var form = new ZForm())
			using (var module = new UNDGSubstanceModule())
			{
				var filterControl = module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();

				module.DataTransferMenuItem.OnPopup(EventArgs.Empty);

				AssertMenuItemClick_ShowsError("Export All Columns To Excel");
				AssertMenuItemClick_ShowsError("Export Visible Columns To Excel");

				void AssertMenuItemClick_ShowsError(string menuItemName)
				{
					var targetMenuItem = module.DataTransferMenuItem.MenuItems.FindByText(menuItemName);
					AssertNotNull("Pre-condition", targetMenuItem);

					targetMenuItem.PerformClick();

					var lastMessage = UnitTestUserNotification.Instance.LastMessage;

					CombineAssertions(() =>
					{
						AssertEquals("Not supported", lastMessage.Caption);
						AssertEquals("Exporting data from the Dangerous Goods module is not permitted under current agreements with our data providers.", lastMessage.Text);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestDataTransfer_NativeXMLMenuItems_AreHidden()
		{
			AssertDataTransfer_NativeXMLMenuItems_AreHidden();
		}

		[RequiresSTA]
		public void TestDataTransfer_NativeXMLMenuItems_AreHidden_RegardlessOfRegistry()
		{
			var today = ZDateTime.UtcNow;
			DataRegistry.Instance.NativeXMLSupportTillDate = today.AddDays(-2).ToDateTime();

			Assert("Native XML not supported", !DataRegistry.Instance.IsNativeXMLSupported);
			AssertDataTransfer_NativeXMLMenuItems_AreHidden();

			DataRegistry.Instance.NativeXMLSupportTillDate = today.AddDays(2).ToDateTime();

			Assert("Native XML supported", DataRegistry.Instance.IsNativeXMLSupported);
			AssertDataTransfer_NativeXMLMenuItems_AreHidden();
		}

		void AssertDataTransfer_NativeXMLMenuItems_AreHidden()
		{
			using (var form = new ZForm())
			using (var module = new UNDGSubstanceModule())
			{
				var filterControl = module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();

				module.DataTransferMenuItem.OnPopup(EventArgs.Empty);

				var menuItems = module.DataTransferMenuItem.MenuItems;

				CombineAssertions("Should not be available as menu items", () =>
				{
					AssertNull(menuItems.FindByText("Import Native XML"));
					AssertNull(menuItems.FindByText("Export Native XML"));
				});
			}
		}

		#endregion

		#region GetIsSystemDefinedDefaultProperty

		protected override string GetIsSystemDefinedDefaultProperty()
		{
			return "All";
		}

		#endregion

		#region Licence/Security

		public void TestLicenceSecurity()
		{
			using (var module = new UNDGSubstanceModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
				AssertEquals(Env.Security.UNDGSubstance, module.SecurityCheckpoint);
			}
		}

		public void TestUNDGSubstanceModuleAllowNew()
		{
			using (var module = new UNDGSubstanceModule())
			{
				AssertEquals("module.AllowNew", false, module.AllowNew);
			}
		}

		#endregion
	}
}
