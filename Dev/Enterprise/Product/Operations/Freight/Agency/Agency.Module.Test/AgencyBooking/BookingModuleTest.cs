using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Freight.Agency.Business;
using Enterprise.Freight.Agency.GUI;
using Enterprise.Freight.Business;
using Enterprise.Freight.GUI;
using Enterprise.Freight.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(BookingModule))]
	internal class BookingModuleTest : ZModuleBasherTest
	{
		public void TestUniversalDataTransferMenuItems()
		{
			using (var module = new BookingModuleForTest())
			{
				module.SetNewGridCollection(new AgencyShipmentCollection(Factory));
				AssertNoExceptionThrown("Module with AgencyShipment collection", () =>
				{
					var menu = module.FormActionMenu;
				});
			}
		}

		public void TestExportToXMLMenuItems()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Booking To");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Booking To", "Bill to Party(s)");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Booking To", "Consignee");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Booking To", "Consignor");
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "D&ata Transfer", "Export Booking To", "Organization Proxy");
			}
		}

		public void TestUniversalMenuItemsDoNotThrowExceptionWhenDataTransferMenuIsNotPresent()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				var moduleDecissionProvider = new Mock<IModuleDecisionProvider>();
				moduleDecissionProvider.Setup(m => m.AllowExcelExport).Returns(false);
				module.OverrideModuleDecisionProvider(moduleDecissionProvider.Object);
				AssertEquals("prerequisite - doesn't have import menu items", false, module.HasImportMenuItems);
				AssertEquals("prerequisite - doesn't have export menu items", false, module.HasExportMenuItems);
				AssertNoExceptionThrown("NullReferenceException is not thrown when there's no ImportMenuItems and ExportMenuItems", () =>
				{
					var menu = module.FormActionMenu;
				});
			}
		}

		public void TestIBulkSendUniversalDataSupportable()
		{
			using (var form = new ZForm())
			using (var module = new BookingModuleForTest())
			{
				var filterControl = (AgencyBookingFilterControl)module.EmbeddedControl;
				form.Controls.Add(filterControl);
				form.Show();
				var bulkSendUniversalDataSupportable = (IBulkSendUniversalDataSupportable)module;
				AssertEquals("Booking", bulkSendUniversalDataSupportable.NameOfSingleObject);
				AssertEquals(typeof(AgencyBooking), bulkSendUniversalDataSupportable.TypeOfSingleObject);
				AssertEquals(WorkflowDescriptors.AgencyBookingWorkflowDescriptorCode, bulkSendUniversalDataSupportable.WorkflowDescriptorCode);
				AssertContainsExactElementsInAnyOrder(Array.Empty<IWorkflowProvider>(), bulkSendUniversalDataSupportable.GetElementsToSend());
				var booking = Factory.New<AgencyBooking>();
				Factory.Save();
				module.PerformSearch();
				AssertEquals("prerequisite - bookings loaded", 1, module.GridCollection.Count);
				module.DisplayGrid.Select(0);
				module.DisplayGrid.SelectSingleElement((BusinessObject)module.GridCollection[0]);
				AssertContainsExactElementsInAnyOrder(new[] { booking.PK }, bulkSendUniversalDataSupportable.GetElementsToSend().Select(bizObj => bizObj.PK));
			}
		}

		public void TestModuleIDAndSupportsWorkflow()
		{
			using (BookingModule module = new BookingModule())
			{
				AssertEquals(ModuleIDs.AgencyBooking, module.ID);
				AssertEquals(true, module.SupportsWorkflow);
			}
		}

		public void TestBusinessContexts()
		{
			using (BookingModule module = new BookingModule())
			{
				AssertEquals("Only one business context should be returned", 1, module.BusinessContexts.Length);
				AssertEquals("AgencyBooking business context should be returned", BusinessContext.AgencyBooking, module.BusinessContexts[0]);
				Assert("Business context array should be returned", module.BusinessContexts is BusinessContext[]);
			}
		}

		public void TestMergeIntoSelectedBooking()
		{
			using (BookingModule module = new BookingModule())
			{
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "Merge Into Selected Booking");
			}
		}

		public void TestSplitBooking()
		{
			using (BookingModule module = new BookingModule())
			{
				MenuAssertion.AssertHasMenu(module.FormActionMenu, "&Actions", "Split Booking");
			}
		}

		public void TestSetGuiProviders()
		{
			using (BookingModuleForTest module = new BookingModuleForTest())
			{
				module.PerformSearch();
				var shipmentDocumentSupporterQueryProvider = module.Factory.GetValue<ICommonShipmentDocumentSupporterQueryProvider>();
				var servicesSelectionProvider = module.Factory.GetValue<IServicesSelectionProvider>();
				AssertNotNull("ShipmentDocumentSupporterGuiQueryProvider query provider initialized", shipmentDocumentSupporterQueryProvider as ShipmentDocumentSupporterGuiQueryProvider);
				AssertNotNull("ServicesSelectionGuiProvider query provider initialized", servicesSelectionProvider as ServicesSelectionGuiProvider);
			}
		}

		public void TestTransportBookingActionAdded()
		{
			using (var module = new BookingModule())
			{
				AssertNotNull(module.ToolBarButtons.FindByText("Actions").DropDownMenu.MenuItems.FindByText("Transport Booking"));
			}
		}

		#region TestDeactivateAndShowBookingRejectionMessage

		public void TestDeactivateAndShowBookingRejectionMessage()
		{
			var booking = Factory.NewWithValidTestData<AgencyBooking>();
			booking.JS_ShipmentStatus = ShipmentStatusList.Codes.ElectronicBooking;
			Factory.Save();

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			using (var module = new BookingModuleForTest())
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
				AssertNotEquals("Booking Rejection Message", message?.Caption ?? string.Empty);
				AssertNotEquals("This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", message?.Text ?? string.Empty);

				var openedBookingForms = ZApplication.GetOpenForms().OfType<AgencyBookingForm>();
				foreach (var openedBookingForm in openedBookingForms)
				{
					openedBookingForm.Dispose();
				}
			}

			using (AgencyRegistry.Instance.ElectronicBookingAndShippingInstructions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			using (var module = new BookingModuleForTest())
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
				AssertEquals("Booking Rejection Message", message.Caption);
				AssertEquals("This Booking was created electronically, would you like to send Booking Rejection message to the Booking Party?", message.Text);

				var openedBookingForms = ZApplication.GetOpenForms().OfType<AgencyBookingForm>();
				foreach (var openedBookingForm in openedBookingForms)
				{
					openedBookingForm.Dispose();
				}
			}
		}

		#endregion

		#region Implementation
		protected override ModuleIdentifier GetModuleID()
		{
			return Enterprise.ZArchitecture.Modules.ModuleIDs.AgencyBooking;
		}

		class BookingModuleForTest : BookingModule
		{
			public new BusinessObjectFactory Factory
			{
				get
				{
					return base.Factory;
				}
			}

			public new IBusinessObjectCollection GetNewGridCollection()
			{
				return gridCollection ?? base.GetNewGridCollection();
			}

			public void SetNewGridCollection(IBusinessObjectCollection collection)
			{
				gridCollection = collection;
			}

			IBusinessObjectCollection gridCollection;
			public void PerformSearch()
			{
				base.PerformSearch();
			}
		}
		#endregion
	}
}
