using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Integration;
using Enterprise.Environment;
using Enterprise.Freight.Business;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.CFS.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.GUI.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class CFSLoadListConsolFormTest : BaseFreightTest
	{
		public void TestWorkflowTabPage()
		{
			var workflowTabPageField = typeof(CFSLoadListConsolForm).GetField("WorkflowTabPage", BindingFlags.NonPublic | BindingFlags.Instance);

			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.JK_IsForwarding = true;

			using (var form = new CFSLoadListConsolsForTesting(loadList))
			{
				var tabPage = (ZWorkflowTabPage)workflowTabPageField.GetValue(form);
				Assert(!tabPage.TabVisible);
				Assert(!((IWorkflowTabPage)tabPage).Initialized);
				AssertNull(form.ActionsMenuItem.MenuItems.FindByText("Send Universal XML"));
			}

			loadList.JK_IsForwarding = false;

			using (var form = new CFSLoadListConsolsForTesting(loadList))
			{
				var tabPage = (ZWorkflowTabPage)workflowTabPageField.GetValue(form);
				var universalMenuItem = form.ActionsMenuItem.MenuItems.FindByText("Send Universal XML");

				Assert(tabPage.TabVisible);
				Assert(((IWorkflowTabPage)tabPage).Initialized);
				Assert(universalMenuItem.Visible);

				loadList.JK_IsForwarding = true;
				Assert(!tabPage.TabVisible);
				Assert(!universalMenuItem.Visible);
			}
		}

		public void TestGuiFactoryServices()
		{
			CFSLoadListConsol loadListConsol = Factory.New<CFSLoadListConsol>();

			ICommonConsolDocumentSupporterQueryProvider consolDocSupporterQueryProvider = Factory.GetValue<ICommonConsolDocumentSupporterQueryProvider>();
			AssertNull("consol doc supporter", consolDocSupporterQueryProvider);

			ICommonShipmentDocumentSupporterQueryProvider shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
			AssertNull("shipment doc supporter", shipmentDocSupporterQueryProvider);

			IServicesSelectionProvider servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
			AssertNull("services selection provider", servicesSelectionProvider);

			using (new CFSLoadListConsolForm(loadListConsol))
			{
				consolDocSupporterQueryProvider = Factory.GetValue<ICommonConsolDocumentSupporterQueryProvider>();
				AssertNotNull("consol doc supporter", consolDocSupporterQueryProvider);
				Assert(consolDocSupporterQueryProvider is ConsolDocumentSupporterGuiQueryProvider);

				shipmentDocSupporterQueryProvider = Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				AssertNotNull("shipment doc supporter", shipmentDocSupporterQueryProvider);
				Assert(shipmentDocSupporterQueryProvider is ShipmentDocumentSupporterGuiQueryProvider);

				servicesSelectionProvider = Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("services selection provider", servicesSelectionProvider);
				Assert(servicesSelectionProvider is ServicesSelectionGuiProvider);
			}
		}

		#region TestAutomaticallyUpdatePackLineContainers

		public void TestAutomaticallyUpdatePackLineContainers()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			using (var form = new CFSLoadListConsolForm(loadList))
			{
				AssertEquals("Form should match registry setting", CFSDataRegistry.Instance.AutopackContainers.Value, loadList.AutomaticallyUpdatePackLineContainers);
			}
		}

		#endregion

		#region Selection From Grid

		public void TestSelectedShipmentsFromShipmentsGrid1Shipment()
		{
			var loadList = GetConsolToUnpack();
			using (var form = new CFSLoadListConsolForm(loadList))
			{
				form.Show();
				AssertEquals("If only 1 shipment, it should always be the selected shipment by default", loadList.Shipments[0], form.GetSelectedShipments()[0]);
				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.Select(0);
				AssertEquals("If only 1 shipment, it should always be the selected shipment if selected", loadList.Shipments[0], form.GetSelectedShipments()[0]);
				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.UnSelect(0);
				AssertEquals("If only 1 shipment, it should always be the selected shipment if in unselected", loadList.Shipments[0], form.GetSelectedShipments()[0]);
			}
		}

		public void TestSelectedShipmentsFromShipmentsGridMultipleShipments()
		{
			var loadList = GetConsolToUnpack();
			var shipment2 = loadList.Shipments.AddNew();
			using (var form = new CFSLoadListConsolForm(loadList))
			{
				form.Show();
				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.Select(0);
				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.Select(1);
				AssertEquals("2 shipments should be returned", 2, form.GetSelectedShipments().Length);
				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.UnSelect(0);
				AssertEquals("1 shipment should be returned", 1, form.GetSelectedShipments().Length);
				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.UnSelect(1);
				AssertEquals("0 shipment should be returned", 0, form.GetSelectedShipments().Length);
				form.LoadListDetailsUserControl.ShipmentReceivalModuleButtonGrid.InnerGrid.Select(0);
				AssertEquals("First Container Should Be Selected", loadList.Shipments[0], form.GetSelectedShipments()[0]);
			}
		}

		#endregion

		public void TestShipmentUniqueConsignementNumbers()
		{
			var loadList = GetConsolToPack();
			using (var form = new CFSLoadListConsolForm(loadList))
			{
				form.Show();
				loadList.JK_OH_Forwarder = GetNonRelatedCFSClient().PK;
				Factory.Save();
				AssertEquals("Shipment Number should start with H", 'H', loadList.Shipments[0].JS_UniqueConsignRef[0]);
			}
		}

		public void TestMasterBillLabelChangesWhenTransportModeChanges()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			using (var form = new CFSLoadListConsolForm(loadList))
			{
				form.Show();
				loadList.JK_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Master Bill", form.LoadListDetailsUserControl.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				loadList.JK_TransportMode = Constants.TransportModes.Sea;
				AssertEquals("Ocean Bill", form.LoadListDetailsUserControl.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
				loadList.JK_TransportMode = Constants.TransportModes.Air;
				AssertEquals("Master Bill", form.LoadListDetailsUserControl.JK_MasterBillNumTextBox.GetExtension<ILabelCaptionRenderer>().Caption);
			}
		}

		public void TestRNSMFLoadListPlugIn()
		{
			var loadList = Factory.New<CFSLoadListConsol>();

			using (var form = new CFSLoadListConsolForm(loadList))
			{
				form.Show();

				AssertNull("RNSMFLoadListPlugIn should not be plugged in", form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFLoadListPlugIn));
				AssertNull("RNS/MF menus should not be plugged in", form.Menu.MenuItems.FindByName("RNS/MF"));
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				loadList.JK_RL_NKLoadPort = HomePort;
				loadList.JK_RL_NKDischargePort = "";

				using (var form = new CFSLoadListConsolForm(loadList))
				{
					form.Show();

					var plugIn = form.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFLoadListPlugIn);
					AssertNotNull("RNSMFLoadListPlugIn should be plugged in", plugIn);
					Assert("RNSMFLoadListPlugIn should not be enabled", !plugIn.Enabled);

					var menu = form.Menu.MenuItems.FindByText("RNS/MF");
					AssertNotNull("RNS/MF menus should be plugged in", menu);
					Assert("RNS/MF menus should be invisible", !menu.Visible);

					loadList.JK_RL_NKLoadPort = "";
					loadList.JK_RL_NKDischargePort = HomePort;

					Assert("RNSMFLoadListPlugIn should be enabled", plugIn.Enabled);
					Assert("RNS/MF menus should be visible", menu.Visible);
				}
			}
		}

		class CFSLoadListConsolsForTesting : CFSLoadListConsolForm
		{
			public CFSLoadListConsolsForTesting(CFSLoadListConsol loadList)
				: base(loadList)
			{
			}
			public new MenuItem ActionsMenuItem
			{
				get { return base.ActionsMenuItem; }
			}

			protected override XmlDataTransferExporter GetNewXmlDataTransferExporter(IValueObjectDataAdapter adapter, bool checkForLicence)
			{
				return new XmlDataTransferExporter(adapter, false);
			}
		}

		[TestDate(2007, 6, 23, 12, 0, 0)]
		public void TestFileExportedIntoNominatedDirectory()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			loadList.JK_UniqueConsignRef = "L001000";
			Factory.Save();

			SystemDataRegistry.Instance.CFSLoadListConsolDirectory.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, EnvProxy.Instance.TempPath);
			var expectedFileName = Path.Combine(EnvProxy.Instance.TempPath, loadList.JK_UniqueConsignRef + "_20070623120000.xml");
			ZFormModaliser.FileNameToSelectInShowCommonDialog = expectedFileName;
			ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;

			try
			{
				using (var form = new CFSLoadListConsolsForTesting(loadList))
				{
					var item = form.ActionsMenuItem.MenuItems.FindByText("Export to XML (Verbose)");
					item.PerformClick();
					Assert("File should have been created", File.Exists(expectedFileName));
				}
			}
			finally
			{
				DeleteIfExists(expectedFileName);
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestWarningShownOnDeleteContainerWithPackLine()
		{
			var loadList = Factory.New<CFSLoadListConsol>();
			loadList.Shipments.AddNew().OuterPackLines.AddNew().Containers.Add(loadList.Containers.AddNew());
			using (new CFSLoadListConsolForm(loadList))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				loadList.Containers.RemoveAll();
				string warningMessage = "There are shipments packed into this container.\r\nDetaching the container will unpack the shipments from it.\r\nDo you want to proceed with Detach?";
				AssertEquals(warningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Container should be removed", 0, loadList.Containers.Count);
			}
		}

		public void TestJK_OH_ForwarderForBindingSet_ShouldRaiseWarningMessage()
		{
			var loadList = Factory.NewWithValidTestData<CFSLoadListConsol>();
			loadList.Shipments.AddNew().OuterPackLines.AddNew().Containers.Add(loadList.Containers.AddNew());

			var newClient = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new CFSLoadListConsolForm(loadList))
			{
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				form.Show();
				loadList.JK_OH_ForwarderForBinding = newClient.PK;
				string warningMessage = "The current client of all the attached containers will be overridden. Do you want to continue?";
				AssertEquals(warningMessage, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestActionsMenuItemsNotAvailableInViewMode()
		{
			ActionsMenuItemsHelperTest.AssertActionsMenuItemsNotAvailableInViewMode(new CFSLoadListConsolForm(Factory.New<CFSLoadListConsol>()));
		}

		public void TestDomesticConfirmationVisibilty()
		{
			var orgheader1 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader1.OH_RL_NKClosestPort = "AUSYD";

			var orgheader2 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader2.OH_RL_NKClosestPort = "AUPAL";

			var orgheader3 = Factory.NewWithValidTestData<OrgHeader>();
			orgheader3.OH_RL_NKClosestPort = "AUOOC";

			var loadlist = Factory.New<CFSLoadListConsol>();
			loadlist.JK_RL_NKLoadPort = "AUSYD";
			loadlist.JK_RL_NKDischargePort = "AUPAL";
			loadlist.JK_OA_DepotAddress = orgheader1.MainAddress.PK;

			Factory.Save();

			using (var form = new CFSLoadListConsolForm(loadlist))
			{
				form.Show();
				form.PickupConfirmControl.Parent.Visible = true;

				AssertEquals(true, form.PickupConfirmControl.Visible);
				AssertEquals(false, form.DeliveryConfirmControl.Visible);

				loadlist.JK_OA_DepotAddress = orgheader2.MainAddress.PK;
				AssertEquals(false, form.PickupConfirmControl.Visible);
				AssertEquals(true, form.DeliveryConfirmControl.Visible);

				loadlist.JK_OA_DepotAddress = orgheader3.MainAddress.PK;
				AssertEquals(false, form.PickupConfirmControl.Visible);
				AssertEquals(false, form.DeliveryConfirmControl.Visible);
			}
		}

		#region Implementation

		CFSLoadListConsol GetConsolToPack()
		{
			var result = Factory.New<CFSLoadListConsol>();
			result.JK_TransportMode = Constants.TransportModes.Sea;
			result.Transports[0].JW_JX = ExportSailing1.PK;
			result.AutomaticallyUpdatePackLineContainers = false;

			var container = result.Containers.AddNew();
			container.JC_ContainerNum = "JFDU8392839";
			container.JC_RC = RC_40GP_PK;

			CommonShipment shipment = result.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			AssertEquals("Load List 1 Shipment Unpacked", 1, shipment.OuterPackLines.Count);
			AssertEquals("Load List 1 Container Unpacked", 0, container.PackLines.Count);
			AssertEquals("Load List should have one unallocated pack line", 1, result.UnAllocatedPackLines.Count);
			return result;
		}

		CFSLoadListConsol GetConsolToUnpack()
		{
			var result = Factory.New<CFSLoadListConsol>();
			result.JK_TransportMode = Constants.TransportModes.Sea;
			result.Transports[0].JW_JX = ImportSailing1.PK;
			result.AutomaticallyUpdatePackLineContainers = true;

			var container = result.Containers.AddNew();
			container.JC_ContainerNum = "JFDU8392839";
			container.JC_RC = RC_40GP_PK;

			CommonShipment shipment = result.Shipments.AddNew();
			shipment.OuterPackLines.AddNew();
			shipment.JS_OuterPacks = 10;
			AssertEquals("Load List 1 Shipment packed", 1, shipment.OuterPackLines.Count);
			AssertEquals("Load List 1 Container packed", 1, container.PackLines.Count);
			AssertEquals("Load List should have one unallocated pack line", 0, result.UnAllocatedPackLines.Count);
			return result;
		}

		OrgHeader GetNonRelatedCFSClient()
		{
			var result = Factory.New<OrgHeader>();
			result.OH_FullName = "Non Related CFS Client";
			result.OH_RL_NKClosestPort = HomePort;
			result.MainAddress.OA_Address1 = "Non Related Address 1";
			return result;
		}

		#endregion
	}
}
