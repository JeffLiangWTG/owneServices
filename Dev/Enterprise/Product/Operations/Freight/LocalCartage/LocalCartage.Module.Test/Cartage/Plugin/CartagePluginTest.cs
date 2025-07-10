using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Modules;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Common.Business;
using Enterprise.Freight.Common.Business.Testing;
using Enterprise.Freight.LocalCartage.Business;
using Enterprise.Freight.LocalCartage.Business.Testing;
using Enterprise.Licensing;
using Enterprise.TransportCommon.Registry;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.PlugIn;
using Enterprise.ZArchitecture.PlugIn.Internal;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Module.Testing
{
	public class CartagePluginTest : TestCaseWithFactory
	{
		public void TestLicenceCheckpointForViewCartage()
		{
			AssertLicenceCheckpointForViewCartage(ModuleTree.Tree.FindByID(ModuleIDs.JobShipment.Name), Env.Licence.LocalTransport);
		}

		void AssertLicenceCheckpointForViewCartage(IMainFormModule mainFormModule, LicenceCheckpoint expected)
		{
			var dummyParent = new DummyCartageParent(Factory);
			using (var module = mainFormModule.CreateZModule())
			using (var form = ((IFilterModuleInternalsForTesting)module).ShowNewForm())
			using (var plugin = new CartagePlugin(dummyParent))
			{
				((IPlugInInternals)plugin).InitializePlugin(null, (ZForm)form);
				AssertEquals("LicenceCheckpointForViewCartage: " + mainFormModule.ID, expected, plugin.LicenceCheckpointForViewCartage);
			}
		}

		public void TestGetNewTopLevelMenu()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			using (CartagePlugin plugin = new CartagePlugin(dummyCartageParent))
			{
				MenuItem topLevelMenuItem = GetNewTopLevelMenu(plugin);
				AssertEquals("topLevelMenuItem should have the correct text", "Port Transport", topLevelMenuItem.Text);
				AssertEquals("Should have 1 cartage Type, so 1 submenu for it", 1, topLevelMenuItem.MenuItems.Count);
				AssertEquals("CartageType Menu should be Dummy Cartage", "Dummy Port Transport", topLevelMenuItem.MenuItems[0].Text);
				AssertEquals("CartageType Menu should have 4 submenus", 4, topLevelMenuItem.MenuItems[0].MenuItems.Count);
				AssertEquals("CartageType 1st Menu should be", "Create Port Transport", topLevelMenuItem.MenuItems[0].MenuItems[0].Text);
				AssertEquals("CartageType 2nd Menu should be", "-", topLevelMenuItem.MenuItems[0].MenuItems[1].Text);
				AssertEquals("CartageType 3rd Menu should be", "Export Port Transport Booking To XML", topLevelMenuItem.MenuItems[0].MenuItems[2].Text);
				AssertEquals("CartageType 4th Menu should be", "Export Port Transport Booking To XML: Store As File", topLevelMenuItem.MenuItems[0].MenuItems[3].Text);
			}
		}

		public void TestGetNewTopLevelMenu_ForNotSupportedCartageType()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			dummyParent.CartageTypesReturnsEmptyArray = true;
			ICartageParent dummyCartageParent = dummyParent;
			using (CartagePlugin plugin = new CartagePlugin(dummyCartageParent))
			{
				MenuItem topLevelMenuItem = GetNewTopLevelMenu(plugin);
				AssertEquals("topLevelMenuItem should have the correct text", "Port Transport", topLevelMenuItem.Text);
				AssertEquals("Should have the not supprted menu only", 1, topLevelMenuItem.MenuItems.Count);
				AssertEquals("CartageType Menu should be Dummy Cartage", "Not Supported", topLevelMenuItem.MenuItems[0].Text);
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				topLevelMenuItem.MenuItems[0].PerformClick();
				string text = UnitTestUserNotification.Instance.LastMessage.Text;
				AssertEquals("correct error message should be shown", true, text == "This " + ((ICartageParent)dummyParent).HumanReadableName + " doesn't support a Port Transport.");
			}
		}

		public void TestLocalCartage()
		{
			DummyCartageParent dummyParent = new DummyCartageParent(Factory);
			ICartageParent dummyCartageParent = dummyParent;
			DummyCartageType cartageType = new DummyCartageType(dummyParent, Core.Constants.CartageJobType.NEW_LCLImport, null);
			using (CartagePluginParentFormForTest form = new CartagePluginParentFormForTest(dummyParent))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				form.GetTopLevelMenu.MenuItems.FindByText("Port Transport").MenuItems[0].MenuItems[0].PerformClick();
				AssertEquals("Error: The Local Transport Organization entered must be set up as a Local Transport Organization (Organization -> Carrier -> Local Transport).", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[ExpectNoExceptions]
		public void TestEventsUnhookOnBizODelete()
		{
			var dummyCartageParent = new DummyCartageParent(Factory);
			using (var form = new CartagePluginParentFormForTest(dummyCartageParent))
			{
				form.Show();
				dummyCartageParent.Delete();
			}
		}

		public void TestCartageOrgChangeCreatesNewCartageJobAfterIfNoSaveInProgressOnceUserHasSaved() 
		{
			var (oldCartageJob, dummyCartageType, dummyCartageParent, orgProxyHeader) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory);
			var (_, anotherDummyCartageType, anotherDummyCartageParent, anotherOrgProxyHeader) = LocalCartageTestHelper.CreateTestCartageWithParentForwardingShipment(Factory, " Another Road", "XYZ0456", createCartage: false);
			using (TransportRegistry.Instance.PromptToCreateLocalTransportJob.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new CartagePluginShipmentFormForTest(dummyCartageParent as CommonShipment))
			{
				form.Show();
				var oldValue = dummyCartageType.CartageAddressInfo.Value;
				var otherFirstCartageType = anotherDummyCartageParent.CartageTypes.First();
				((UnitTestUserNotification)Globals.Message).ClearMessagesAndAnswers();
				var plugin = (CartagePlugin)form.PlugIns.GetPlugIn(ControllerIDs.CartagePlugin);
				plugin.ValidTransactionCount = TransactionedTestCase.InTransactionedTestCase ? 1 : 0;
				// set to always answer yes - except for..
				plugin.OverrideQueryUserResponse = DialogResult.Yes;
				// do not show form after InternalCartageManager.CreateCartageAndShow() is called, otherwise will not save cartage
				plugin.DoNotShowFormOnCreateCartageTestOnly = true;
				((BusinessObject)dummyCartageParent)[JobShipmentSchema.Constants.JS_TransportMode] = "SEA";
				// this will trigger the CartageOrganisationChanged() with the value in cartageType.LocalTransportProviderAddress.Header triggering the change.
				// in real situation this is triggered by CartageOrg changing on screen
				dummyCartageType.CartageAddressInfo.Value = anotherDummyCartageType.CartageAddressInfo.Value;

				Factory.Save();
				plugin.OnSaveCompletedOrAborted(true);
				plugin.OverrideQueryUserResponse = null;
				plugin.DoNotShowFormOnCreateCartageTestOnly = false;

				var otherFactory = new BusinessObjectFactory();
				var oldCartageJobCheck = otherFactory.Load<CommonCartage>(oldCartageJob.PK);
				AssertEquals("Old Cartage Job should be marked cancelled", true, oldCartageJobCheck.JJ_IsCancelled);
				var queryParentID = new ZQuery(JobCartageSchema.JJ_ParentID, ((BusinessObject)dummyCartageParent).PK);
				var queryNotCancelled = new ZQuery(JobCartageSchema.JJ_IsCancelled, false);
				var queryToCheckNewCartage = new ZQuery(queryParentID, queryNotCancelled);
				var newCartage = otherFactory.LoadTop1<CommonCartage>(queryToCheckNewCartage);
				AssertNotNull("New cartage should have been created on parent", newCartage);
			}
		}

		class CartagePluginParentFormForTest : ZForm
		{
			internal CartagePluginParentFormForTest(DummyCartageParent dummyCartageParent) : base(dummyCartageParent)
			{
				PlugIns.Add(ControllerIDs.CartagePlugin);
			}

			public MainMenu GetTopLevelMenu
			{
				get
				{
					return MainMenu;
				}
			}
		}

		class CartagePluginShipmentFormForTest : ZForm
		{
			internal CartagePluginShipmentFormForTest(CommonShipment shipmentParent) : base(shipmentParent)
			{
				PlugIns.Add(ControllerIDs.CartagePlugin);
			}

			public MainMenu GetTopLevelMenu
			{
				get
				{
					return MainMenu;
				}
			}
		}

		MenuItem GetNewTopLevelMenu(CartagePlugin plugin)
		{
			return (MenuItem)typeof(ZPlugIn).GetMethod("GetNewTopLevelMenu", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(plugin, Array.Empty<object>());
		}
	}
}
