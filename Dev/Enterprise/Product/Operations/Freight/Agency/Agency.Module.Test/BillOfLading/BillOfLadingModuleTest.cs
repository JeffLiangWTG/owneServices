using System;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DataTransfer.GUI;
using Enterprise.Environment;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BillOfLadingModule))]
	internal class BillOfLadingModuleTest : ZModuleBasherTest
	{
		public void TestUniversalDataTransferMenuItems()
		{
			using (var module = new BillOfLadingModuleForTest())
			{
				module.SetNewGridCollection(new AgencyShipmentCollection(Factory));
				AssertNoExceptionThrown("Module with AgencyShipment collection", () =>
				{
					var menu = module.FormActionMenu;
				});
			}
		}

		public void TestAllowUniversalCopy_ReturnsTrueWhenNotUserInteractive()
		{
			Globals.IsUserInteractive = false;
			using (var module = new BillOfLadingModuleForTest())
			{
				AssertEquals("When generating security reports, we only need to know if its ever true. Since it can be true sometimes this module must support copying, so we want the checkpoints to be made.", true, module.AllowUniversalCopy);
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestExportToXML()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export To XML");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Bill Of Lading To");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Bill Of Lading To", "Bill to Party(s)");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Bill Of Lading To", "Consignee");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Bill Of Lading To", "Consignor");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Bill Of Lading To", "Organization Proxy");
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		public void TestExportToXMLDoesNotIncludeBookings()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var billOfLading = Factory.New<BillOfLading>();
				billOfLading.JS_GoodsDescription = "Bill Of Lading Goods";
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				booking.JS_GoodsDescription = "Booking Goods";
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				var actionsMenuItem = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions");
				actionsMenuItem.OnPopup(EventArgs.Empty);
				var dataTransferMenuItem = MenuAssertion.AssertHasMenu(actionsMenuItem, "D&ata Transfer");
				dataTransferMenuItem.OnPopup(EventArgs.Empty);
				var exportToXML = MenuAssertion.AssertHasMenu(dataTransferMenuItem, "Export To XML");
				var outputFile = Env.GetTempFileName();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				ZFormModaliser.FileNameToSelectInShowCommonDialog = outputFile;
				exportToXML.PerformClick();
				Assert(File.Exists(outputFile));
				var xmlFile = File.ReadAllText(outputFile);
				AssertContains("Bill Of Lading Goods", xmlFile);
				AssertNotContains("Booking Goods", xmlFile);
				File.Delete(outputFile);
				tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
				{ EnabledUntil = ZDateTime.Empty };
				eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			}
		}

		public void TestIBulkSendUniversalDataSupportable()
		{
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var bulkSendUniversalDataSupportable = (IBulkSendUniversalDataSupportable)module;
				AssertEquals("Bill Of Lading", bulkSendUniversalDataSupportable.NameOfSingleObject);
				AssertEquals(typeof(BillOfLading), bulkSendUniversalDataSupportable.TypeOfSingleObject);
				AssertEquals(WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode, bulkSendUniversalDataSupportable.WorkflowDescriptorCode);
				AssertContainsExactElementsInAnyOrder(Array.Empty<IWorkflowProvider>(), bulkSendUniversalDataSupportable.GetElementsToSend());
				var billOfLading = Factory.New<BillOfLading>();
				Factory.Save();
				module.PerformSearch();
				AssertEquals("prerequisite - bills loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.Select(0);
				module.DisplayGrid.SelectSingleElement((BusinessObject)module.GridCollection[0]);
				AssertContainsExactElementsInAnyOrder(new[] { billOfLading.PK }, bulkSendUniversalDataSupportable.GetElementsToSend().Select(bizObj => bizObj.PK));
			}
		}

		public void TestIBulkSendUniversalDataSupportableNotAvailableForBookings()
		{
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var bulkSendUniversalDataSupportable = (IBulkSendUniversalDataSupportable)module;
				AssertEquals("Bill Of Lading", bulkSendUniversalDataSupportable.NameOfSingleObject);
				AssertEquals(typeof(BillOfLading), bulkSendUniversalDataSupportable.TypeOfSingleObject);
				AssertEquals(WorkflowDescriptors.BillOfLadingWorkflowDescriptorCode, bulkSendUniversalDataSupportable.WorkflowDescriptorCode);
				AssertContainsExactElementsInAnyOrder(Array.Empty<IWorkflowProvider>(), bulkSendUniversalDataSupportable.GetElementsToSend());
				var billOfLading = Factory.New<BillOfLading>();
				var booking = Factory.New<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				AssertEquals("Booking is not included in selected bizObjs", 1, module.GetSelectedBusinessObjects().Length);
				AssertContainsExactElementsInAnyOrder("Booking is not included in selected bizObjs", new[] { billOfLading.PK }, bulkSendUniversalDataSupportable.GetElementsToSend().Select(bizObj => bizObj.PK));
			}
		}

		public void TestExportBillOfLadingToNotAvailableForBookings()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var billOfLading = Factory.New<BillOfLading>();
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectSingleElement(booking);
				var actionsMenuItem = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions");
				actionsMenuItem.OnPopup(EventArgs.Empty);
				var dataTransferMenuItem = MenuAssertion.AssertHasMenu(actionsMenuItem, "D&ata Transfer");
				dataTransferMenuItem.OnPopup(EventArgs.Empty);
				var exportToBillOfLadingMenuItem = MenuAssertion.AssertHasMenu(dataTransferMenuItem, "Export Bill Of Lading To");
				exportToBillOfLadingMenuItem.OnPopup(EventArgs.Empty);
				var menuItems = exportToBillOfLadingMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				AssertEquals(12, menuItems.Length);
				foreach (var menuItem in menuItems)
				{
					menuItem.PerformClick();
					AssertEquals("Export Bill Of Lading To functionality is NOT available for Bookings", "Please select at least one Bill Of Lading.", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				module.DisplayGrid.SelectAllElements();
				actionsMenuItem.OnPopup(EventArgs.Empty);
				dataTransferMenuItem.OnPopup(EventArgs.Empty);
				exportToBillOfLadingMenuItem.OnPopup(EventArgs.Empty);
				menuItems = exportToBillOfLadingMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				AssertEquals(12, menuItems.Length);
				foreach (var menuItem in menuItems)
				{
					menuItem.PerformClick();
					AssertNotNull("Should have returned a form, as one bill of lading is selected", ZFormModaliser.LastFormShownDialogForTest);
					AssertEquals("Should have returned the correct form", typeof(ManualDataExportProgressForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				}
			}
		}

		public void TestImportXml()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var item = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From &XML");
				item.PerformClick();
				var form = ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(typeof(XmlDataImporterForm), form?.GetType());
			}
		}

		[TestDate(2015, 10, 21)]
		public void TestImportCSV_InterfaceConnectorOn()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var item = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Import From &CSV");
				item.PerformClick();
				var form = ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(typeof(DataImporterForm), form?.GetType());
				var importer = ((DataImporterForm)form).Importer;
				AssertEquals(ObjectFactory.GetType<Enterprise.Integration.Customs.AU.IShipnetToBillOfLadingImporter>(), importer == null ? null : importer.GetType());
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
		}

		[TestDate(2015, 10, 21)]
		public void TestImportCSV_InterfaceConnectorOff()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil()
			{ EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var cSVImportItem = module.FormActionMenu.FindByText("Actions").MenuItems.FindByText("Data Transfer").MenuItems.FindByText("Import From &CSV");
				AssertNotNull(cSVImportItem);
				// BG: For now does not matter what InterfaceConnector registry is set to.
			}
		}

		public void TestBusinessContexts()
		{
			using (var module = new BillOfLadingModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("AgencyDocumentation business context should be returned", BusinessContext.AgencyDocumentation, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestTransportBookingActionAdded()
		{
			using (var module = new BillOfLadingModule())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Transport Booking"));
			}
		}

		public void TestTransportBookingActionsMenuNotAvailableForBookings()
		{
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var billOfLading = Factory.New<BillOfLading>();
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectSingleElement(billOfLading);
				var actionsMenuItem = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions");
				actionsMenuItem.OnPopup(EventArgs.Empty);
				var transportBookingMenuItem = MenuAssertion.AssertHasMenu(actionsMenuItem, "Transport Booking");
				transportBookingMenuItem.OnPopup(EventArgs.Empty);
				var menuItems = transportBookingMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				AssertEquals(4, menuItems.Length);
				AssertEquals("Create Pickup Transport Booking", menuItems[0].Text);
				AssertEquals("Create Pickup Multi-Container Transport Booking", menuItems[1].Text);
				AssertEquals("Create Delivery Transport Booking", menuItems[2].Text);
				AssertEquals("Create Delivery Multi-Container Transport Booking", menuItems[3].Text);
			}
		}

		public void TestPostActionsNotAvailableForBookings()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var billOfLading = Factory.New<BillOfLading>();
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectSingleElement(booking);
				var actionsMenuItem = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions");
				actionsMenuItem.OnPopup(EventArgs.Empty);
				var postMenuItem = MenuAssertion.AssertHasMenu(actionsMenuItem, "&Post");
				postMenuItem.OnPopup(EventArgs.Empty);
				var menuItems = postMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem) && m.Text != "-").ToArray();
				AssertEquals(9, menuItems.Length);
				AssertEquals("Post All Charges and Costs", menuItems[0].Text);
				AssertEquals("Post Local Client Charges", menuItems[1].Text);
				AssertEquals("Post Overseas Agent Charges", menuItems[2].Text);
				AssertEquals("Post All Revenue Charges", menuItems[3].Text);
				AssertEquals("Post Charges for All Group Companies", menuItems[4].Text);
				AssertEquals("Post Charges for Group Companies in My Login Country/Region", menuItems[5].Text);
				AssertEquals("Post Disbursement Charges only", menuItems[6].Text);
				AssertEquals("Post Costs", menuItems[7].Text);
				AssertEquals("Print Job Profit Document", menuItems[8].Text);
				foreach (var menuItem in menuItems)
				{
					menuItem.PerformClick();
					AssertContains("Please select a Job", UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}

				module.DisplayGrid.SelectAllElements();
				actionsMenuItem.OnPopup(EventArgs.Empty);
				postMenuItem.OnPopup(EventArgs.Empty);
				menuItems = postMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem) && m.Text != "-").ToArray();
				AssertEquals(9, menuItems.Length);
				AssertEquals("Post All Charges and Costs", menuItems[0].Text);
				AssertEquals("Post Local Client Charges", menuItems[1].Text);
				AssertEquals("Post Overseas Agent Charges", menuItems[2].Text);
				AssertEquals("Post All Revenue Charges", menuItems[3].Text);
				AssertEquals("Post Charges for All Group Companies", menuItems[4].Text);
				AssertEquals("Post Charges for Group Companies in My Login Country/Region", menuItems[5].Text);
				AssertEquals("Post Disbursement Charges only", menuItems[6].Text);
				AssertEquals("Post Costs", menuItems[7].Text);
				AssertEquals("Print Job Profit Document", menuItems[8].Text);
				foreach (var menuItem in menuItems)
				{
					menuItem.PerformClick();
					AssertContains(string.Format("are you sure you want to {0}", menuItem.Text.ToLower()), UnitTestUserNotification.Instance.LastMessage.Text.ToLower());
					AssertContains(billOfLading.JS_UniqueConsignRef, UnitTestUserNotification.Instance.LastMessage.Text);
					AssertNotContains(booking.JS_UniqueConsignRef, UnitTestUserNotification.Instance.LastMessage.Text);
					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				}
			}
		}

		public void TestOperationalActionsNotAvailableForBookings()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Booking not loaded", 0, module.GridCollection.Count);
				module.DisplayGrid.SelectSingleElement(booking);
				var actionsMenuItem = MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions");
				actionsMenuItem.OnPopup(EventArgs.Empty);
				var operationalActionsMenuItem = MenuAssertion.AssertHasMenu(actionsMenuItem, "Operational Actions");
				operationalActionsMenuItem.OnPopup(EventArgs.Empty);
				var deliverDocumentsMenuItem = MenuAssertion.AssertHasMenu(operationalActionsMenuItem, "Deliver Documents");
				deliverDocumentsMenuItem.OnPopup(EventArgs.Empty);
				var deliverDocuments = deliverDocumentsMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				var eIDOMenuItem = MenuAssertion.AssertHasMenu(operationalActionsMenuItem, "E-IDO");
				eIDOMenuItem.OnPopup(EventArgs.Empty);
				var eIDO = eIDOMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				var exchangeRatesMenuItem = MenuAssertion.AssertHasMenu(operationalActionsMenuItem, "Update Exchange Rates");
				exchangeRatesMenuItem.OnPopup(EventArgs.Empty);
				var exchangeRates = exchangeRatesMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				AssertEquals(6, deliverDocuments.Length);
				AssertEquals(2, eIDO.Length);
				AssertEquals(2, exchangeRates.Length);
				var autoRateCostsMenuItem = MenuAssertion.AssertHasMenu(operationalActionsMenuItem, "Autorate Costs");
				var autoRateCostsRevenueMenuItem = MenuAssertion.AssertHasMenu(operationalActionsMenuItem, "Autorate Costs and Revenue");
				var autoRateRevenueMenuItem = MenuAssertion.AssertHasMenu(operationalActionsMenuItem, "Autorate Revenue");
				AssertOperationalActionMenuItems(deliverDocuments);
				AssertOperationalActionMenuItems(eIDO);
				AssertOperationalActionMenuItems(exchangeRates);
				AssertOperationalActionMenuItems(new[] { autoRateCostsMenuItem, autoRateCostsRevenueMenuItem, autoRateRevenueMenuItem });
				var billOfLading = Factory.New<BillOfLading>();
				Factory.Save();
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				actionsMenuItem.OnPopup(EventArgs.Empty);
				operationalActionsMenuItem.OnPopup(EventArgs.Empty);
				deliverDocumentsMenuItem.OnPopup(EventArgs.Empty);
				eIDOMenuItem.OnPopup(EventArgs.Empty);
				exchangeRatesMenuItem.OnPopup(EventArgs.Empty);
				deliverDocuments = deliverDocumentsMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				eIDO = eIDOMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				exchangeRates = exchangeRatesMenuItem.MenuItems.Cast<MenuItem>().Where((m) => m.GetType() == typeof(ZMenuItem)).ToArray();
				AssertOperationalActionMenuItems(deliverDocuments);
				AssertOperationalActionMenuItems(eIDO);
				AssertOperationalActionMenuItems(exchangeRates);
				AssertOperationalActionMenuItems(new[] { autoRateCostsMenuItem, autoRateCostsRevenueMenuItem, autoRateRevenueMenuItem });
			}
		}

		void AssertOperationalActionMenuItems(MenuItem[] menuItems)
		{
			foreach (var menuItem in menuItems)
			{
				menuItem.PerformClick();
				AssertNotNull(ZFormModaliser.LastFormShownDialogForTest);
				AssertContains("OperationalActionRunnerForm", ZFormModaliser.LastFormShownDialogForTest.Name);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestGetSelectedBusinessObjectsDoesNotIncludeBookings()
		{
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var billOfLading = Factory.New<BillOfLading>();
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectAllElements();
				var selectedBusinessObjects = module.GetSelectedBusinessObjects();
				AssertEquals("Booking is not included in selected bizObjs", 1, selectedBusinessObjects.Length);
				AssertEquals(billOfLading.PK, selectedBusinessObjects[0].PK);
			}
		}

		public void TestUniversalCopyMenuItemNotAvailableForBookings()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var billOfLading = Factory.New<BillOfLading>();
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only bill of lading loaded", 1, module.GridCollection.Count);
				var universalCopyMenuItem = MenuAssertion.AssertHasMenu(module.DisplayGrid.ContextMenu, "Universal Copy");
				module.DisplayGrid.SelectSingleElement(billOfLading);
				module.DisplayGrid.ContextMenu.DoPopup();
				AssertEquals(true, universalCopyMenuItem.Visible);
				AssertEquals(true, universalCopyMenuItem.Enabled);
				KeySender.PostKeyDown(module.DisplayGrid, Keys.Control | Keys.Shift | Keys.C);
				Application.DoEvents();
				AssertContains("There are no copy templates to use.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				module.DisplayGrid.SelectAllElements();
				module.DisplayGrid.ContextMenu.DoPopup();
				AssertEquals(true, universalCopyMenuItem.Visible);
				AssertEquals(true, universalCopyMenuItem.Enabled);
				KeySender.PostKeyDown(module.DisplayGrid, Keys.Control | Keys.Shift | Keys.C);
				Application.DoEvents();
				AssertContains("There are no copy templates to use.", UnitTestUserNotification.Instance.LastMessage.Text);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestDocumentsMenuItemNotAvailableForBookings()
		{
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				var filterControl = (BillOfLadingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var billOfLading = Factory.New<BillOfLading>();
				var booking = Factory.NewWithValidTestData<AgencyBooking>();
				Factory.Save();
				var filter = (ModuleTextFilter)module.FilterBusinessObject.ModuleFilters[AgencyShipmentFilterStrip.Descriptions.ShipmentStatus];
				filter.IsActive = true;
				filter.Property = "ALL";
				module.PerformSearch();
				AssertEquals("Only BillOfLading loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.SelectSingleElement(billOfLading);
				module.DisplayGrid.ContextMenu.DoPopup();
				MenuAssertion.AssertHasMenu(module.DisplayGrid.ContextMenu, "Documents");
				module.DisplayGrid.SelectSingleElement(booking);
				module.DisplayGrid.ContextMenu.DoPopup();
				AssertNull(module.DisplayGrid.ContextMenu.MenuItems.FindByText("Documents"));
			}
		}

		public void TestDeactiveBillOfLading()
		{
			var bill = Factory.NewWithValidTestData<BillOfLading>();
			Factory.Save();
			bill.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicShippingInstruction;
			Factory.Save();

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var module = new BillOfLadingModuleForTest())
			{
				form.Controls.Add(module.EmbeddedControl);
				form.Show();

				module.PerformSearch();
				AssertEquals(1, module.GridCollection.Count);

				module.DisplayGrid.Select(0);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var deactivateButton = module.ToolBarButtons.FirstOrDefault(t => t.Text == "Delete") as ZToolBarButton;
				AssertNotNull(deactivateButton);
				deactivateButton.PerformClick();

				var message = UnitTestUserNotification.Instance.LastMessage;
				AssertNotNull(message);
				AssertEquals("Bill Of Lading Rejection Message", message.Caption);
				AssertEquals("This Bill Of Lading was created electronically, would you like to send a Shipping Instruction Rejection message to the booking party?", message.Text);

				var openedBookingForms = ZApplication.GetOpenForms().OfType<BillOfLadingForm>();
				foreach (var openedBookingForm in openedBookingForms)
				{
					openedBookingForm.Dispose();
				}
			}
		}

		#region Implementation
		class BillOfLadingModuleForTest : BillOfLadingModule
		{
			public void PerformSearch()
			{
				base.PerformSearch();
			}

			protected override IBusinessObjectCollection GetNewGridCollection()
			{
				return gridCollection ?? base.GetNewGridCollection();
			}

			public void SetNewGridCollection(IBusinessObjectCollection collection)
			{
				gridCollection = collection;
			}

			IBusinessObjectCollection gridCollection;
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AgencyBillOfLading;
		}
		#endregion
	}
}
