using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DeniedPartyScreening.Common;
using Enterprise.DeniedPartyScreening.GUI;
using Enterprise.DeniedPartyScreening.GUI.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class OrderEntryFormTest : WhsDocketFormTestCase
	{
		#region TestConstructor

		[TestDate(2015, 10, 21)]
		public void TestConstructor()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Today.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			using (var form = new OrderEntryForm(Factory.New<WhsOrder>(), new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Posting not setup", true, form.SetupPostingCalledForTest);
				AssertNotNull("Billing not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
				AssertNotNull("EDocs not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertNotNull("DocumentUDF not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull("DocAddress plug in", form.PlugIns.GetPlugIn(ControllerIDs.DocAddresses));
				Assert(form.IsResizableByTabPageAllowed);
				AssertNotNull(form.ActionsMenuItemForTest.MenuItems.FindByText("Create all Work Orders by their Bills of Materials", false));
				AssertNotNull(form.ActionsMenuItemForTest.MenuItems.FindByText("Override Fulfillment Rule", false));

				var ifsMenuItem = form.ActionsMenuItemForTest.MenuItems.FindByText("Export Order to IFS", false);
				var ifsExportToFileMenuItem = ifsMenuItem.MenuItems.FindByText("Store to File");
				AssertEquals(typeof(WhsOrderExportToFileMenuItemIFS), ifsExportToFileMenuItem.GetType());

				var ifsExportToEmailMenuItem = ifsMenuItem.MenuItems.FindByText("Send Email");
				AssertEquals(typeof(WhsOrderExportToEmailMenuItemIFS), ifsExportToEmailMenuItem.GetType());

				var tabControl = (ZTabControl)form.EventTabPage.Controls[0].Controls[0];
				var screeningLogsTab = tabControl.TabPages.Cast<TabPage>().FirstOrDefault(t => t.Text == "Denied Party Screening Logs");
				var screeningLogControl = screeningLogsTab.Controls[0] as StmEntityScreeningLogControl;
				AssertNotNull(screeningLogControl);
				AssertEquals(typeof(StmEntityScreeningLogControl), screeningLogsTab.Controls[0].GetType());
				AssertNotNull(screeningLogControl);
				AssertEquals("RelatedOrgPartyScreeningStatusCollection", screeningLogControl.GetBindingMember());
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new OrderEntryForm(Factory.New<WhsOrder>(), null));
		}

		#endregion

		#region TestResynchronizeScreeningStatusOnFormLoadOrNot

		public void TestResynchronizeScreeningStatusOnFormLoad()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ABC";
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			order.DocAddresses.RemoveAndDeleteAll();
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, order.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Unknown, order.WD_ScreeningStatus);
			});

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
					AssertEquals(1, order.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Count());
					AssertContains("|NEW=CLR|OLD=UNK|TYP=SYNC", order.Logs.MostRecentLogByEventTime(ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated).SL_Reference);
				});
			}
		}

		public void TestResynchronizeScreeningStatusOnFormLoad_NoDeveloperNotificationExceptionThrown()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ABC";
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			order.DocAddresses.RemoveAndDeleteAll();
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
			Factory.Save();

			AssertNoExceptionThrown(() =>
			{
				using (TestingState.SuspendIsRunningTests())
				using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
				{
					form.Show();
					AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
					AssertEquals("Warehouse Order screening status CLR.", ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
				}
			});
		}

		public void TestNoNeedToResynchronizeScreeningStatusOnFormLoad()
		{
			var consignee = Factory.New<OrgHeader>();
			consignee.OH_Code = "ABC";
			consignee.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;

			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1.PK, data.Whs1.PK, consignee.PK, "TEST");
			order.DocAddresses.RemoveAndDeleteAll();
			data.Org1.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			data.Whs1.WarehouseAddress.Header.OH_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
			Factory.Save();

			CombineAssertions("Precondition: ", () =>
			{
				AssertEquals(false, order.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
				AssertEquals(ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
			});

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.Show();
				CombineAssertions(() =>
				{
					AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
					AssertEquals(ScreeningStatusesList.Codes.Clear, order.WD_ScreeningStatus);
					AssertEquals(false, order.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.AutoEvents.DeniedPartyStatusUpdated.Code).Any());
				});
			}
		}

		#endregion

		#region TestEDocsModifySetToOffNotDisablePlugin

		public void TestEDocsModifySetToOffNotDisablePlugin()
		{
			Env.Security.eDocs.IsAllowed = true;
			Env.Security.eDocsModify.IsAllowed = false;

			using (var form = new OrderEntryForm(Factory.New<WhsOrder>(), new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var eDocPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				Assert("Plugin should not be disabled.", eDocPlugin.SecurityCheckpoint.IsAllowed);
			}
		}

		#endregion

		#region GetNewDocketForm

		protected override ZForm GetNewDocketForm()
		{
			return new OrderEntryForm(Factory.New<WhsOrder>(), new NotificationSubscriberGuiHelper());
		}

		#endregion

		#region Client Consignee Link

		public void TestClientConsigneeLinkDialog()
		{
			var client = Helper.CreateClient();
			var warehouse = Helper.CreateWarehouse("TST WHS");
			var order = Helper.CreateWhsOrder(client, warehouse);
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				order.WD_OH_Client = Factory.NewWithValidTestData<OrgHeader>().PK;
				order.ConsigneePK = Factory.NewWithValidTestData<OrgHeader>().PK;
				var result = form.ShowPreSaveDialogsForTest();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals("Should show to Save Buyer-Supplier link dialog", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?",
					UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(ZDialogResult.No, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("Should not be linked", 0, order.Consignee.SupplierLinks.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				result = form.ShowPreSaveDialogsForTest();
				AssertEquals(ContinueWithSave.Yes, result);
				AssertEquals("Should show to Save Buyer-Supplier link dialog", "Do you wish to save this Supplier-Consignor/Buyer-Consignee relationship?",
					UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(ZDialogResult.Yes, UnitTestUserNotification.Instance.LastMessage.Answer);
				AssertEquals("Should be linked now", 1, order.Consignee.SupplierLinks.Count);
			}
		}

		#endregion

		#region Properties

		public void TestOrder()
		{
			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals(order, form.OrderForTest);
			}
		}

		public void TestFormCaption()
		{
			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				AssertEquals("Order", form.FormCaption.Trim());
				order.WD_DocketID = "W00000001";
				AssertEquals("Order W00000001", form.FormCaption);
			}
		}

		public void TestFormCaption_WhenBoundOrderIsNull()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.SetDataBinding(null, null);
				AssertEquals("the caption should be empty & no exception thrown", "Order", form.FormCaption.Trim());
			}
		}

		#endregion

		#region Show Invoicing Tab

		public void TestShowInvoicingTab()
		{
			var docket = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(docket, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				UserIdleWorker.Flush();
				var initial = form.TabControl.GetTabPage("EntryTabPage");
				var result = form.TabControl.GetTabPage("BillingTabPage");
				AssertNotNull("Precondition", result);
				AssertEquals(initial, form.TabControl.SelectedTab);
				form.ShowInvoicingTab();
				AssertEquals(result, form.TabControl.SelectedTab);
			}
		}

		#endregion

		#region Release Button

		public void TestReleaseButton()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.ReleaseButtonForTest.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(OrderEntryForm.OrderMustBePickedErrorMsg));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				var pick = Factory.New<WhsPick>();
				pick.Orders.Add(order);
				Factory.Save();

				form.ReleaseButtonForTest.PerformClick();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains(OrderEntryForm.OrderMustBePickedErrorMsg));

				var releaseForm = form.LastReleaseControllerForTest.LastShownForm as ReleaseEntryForm;
				AssertNotNull("Release Form not created", releaseForm);
				AssertEquals(true, releaseForm.Visible);
				AssertEquals(pick.PK, ((BusinessObject)releaseForm.BusinessEntity).PK);
				releaseForm.Dispose();
			}
		}

		public void TestReleaseButton_SetCorrectInitialOrderToSelectInGrid()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order1 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order1, data.Part1, 10m);
			var order2 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O2", data.Notify);
			Helper.CreateWhsOrderLine(order2, data.Part1, 4m);
			var order3 = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O3", data.Notify);
			Helper.CreateWhsOrderLine(order3, data.Part1, 2m);

			using (var form = new OrderEntryForm(order2, new NotificationSubscriberGuiHelper()))
			{
				var pick = Factory.New<WhsPick>();
				pick.Orders.Add(order1);
				pick.Orders.Add(order2);
				pick.Orders.Add(order3);
				Factory.Save();

				form.Show();
				form.ReleaseButtonForTest.PerformClick();
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains(OrderEntryForm.OrderMustBePickedErrorMsg));

				var releaseForm = form.LastReleaseControllerForTest.LastShownForm as ReleaseEntryForm;
				AssertEquals(order2.PK, releaseForm.CurrentOrder.PK);
				AssertEquals(true, releaseForm.Visible);
				AssertEquals(pick.PK, ((BusinessObject)releaseForm.BusinessEntity).PK);

				releaseForm.Dispose();
			}
		}

		public void TestReleaseButton_DoesNotHaveSecurityAccessToEdit()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			Factory.Save();

			Env.Security.WhsReleaseView.IsAllowed = false;

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertNoExceptionThrown(() => form.ReleaseButtonForTest.PerformClick());
				AssertEquals("No permission to view Release View should show no error", false, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("No error with security expected", false, UnitTestUserNotification.Instance.LastMessage.WasError);

				var releaseForm = form.LastReleaseControllerForTest.LastShownForm as ReleaseEntryForm;
				AssertEquals(order.PK, releaseForm.CurrentOrder.PK);
				AssertEquals(true, releaseForm.Visible);
				AssertEquals(pick.PK, ((BusinessObject)releaseForm.BusinessEntity).PK);
				releaseForm.Dispose();
			}
		}

		public void TestReleaseButton_DoesNotHaveSecurityAccessToViewOrEdit()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			var pick = Factory.New<WhsPick>();
			pick.Orders.Add(order);
			Factory.Save();

			Env.Security.WhsReleaseView.IsAllowed = false;
			Env.Security.WhsReleaseEdit.IsAllowed = false;

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertNoExceptionThrown(() => form.ReleaseButtonForTest.PerformClick());
				AssertEquals("No permission to view Release View", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertNull(form.LastReleaseControllerForTest.LastShownForm);
			}
		}

		#endregion

		#region TestReleasingPickWithNullWD_WP

		public void TestReleasingPickWithNullWD_WP()
		{
			var order = Factory.New<WhsOrder>();
			order.WD_WP = ZGuid.Empty;
			order.HasChanges = false;

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertNoExceptionThrown("No exception should be thrown", () => form.ReleaseButtonForTest.PerformClick());
				AssertEquals("This order must be picked before it can be Released.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Pick Button

		#region TestPickButtonAutoPick

		public void TestPickButtonAutoPick()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				form.Show();
				AssertEquals("Precondition", false, order.IsAttachedToPickButNotFinalised);
				AssertEquals("Precondition", false, order.IsInDatabase);

				form.PickButtonForTest.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Please save this Order before Picking it"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				order.WD_DocketStatus = DocketStatus.Codes.Held;
				Factory.Save();
				form.PickButtonForTest.PerformClick();
				AssertEquals("This Order is Held. Only Entered (Saved) Orders can be attached to a Pick.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);

				order.WD_DocketStatus = DocketStatus.Codes.Entered;
				Factory.Save();
				form.PickButtonForTest.PerformClick();
				AssertEquals("Order should be Picking", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals("Order should be in database", true, order.IsInDatabase);     // assert factory.save() was called
				AssertEquals("Pick should be in database", true, order.Pick.IsInDatabase);
				AssertEquals("Pick " + order.Pick.WP_PickNo + " has been created.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
			}
		}

		public void TestRARA()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			Factory.Save();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				order.WD_DocketStatus = DocketStatus.Codes.Entered;
				Factory.Save();
				form.PickButtonForTest.PerformClick();
				Assert(true);
			}
		}

		#endregion

		#region TestPickButtonWithJulianProducts

		public void TestPickButtonWithJulianProductsAndClient()
		{
			TestPickButtonWithJulianProductsEnabledClient(hasJulianProducts: true);
		}

		public void TestPickButtonWithJulianClientButNotProducts()
		{
			TestPickButtonWithJulianProductsEnabledClient(hasJulianProducts: false);
		}

		void TestPickButtonWithJulianProductsEnabledClient(bool hasJulianProducts)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", false, order.IsAttachedToPickButNotFinalised);

				Helper.SetClientAttributeType(data.Org1, Enterprise.Warehouse.Transactions.Business.Testing.AttributeNumber.One, PartAttributeTypeList.Codes.JulianBatchNumber);
				Helper.SetProductAttributeUse(data.Org1, data.Part1, Enterprise.Warehouse.Transactions.Business.Testing.AttributeNumber.One, hasJulianProducts);
				Helper.CreateProductParamsByWhsAndClient(data.Part1, data.Org1, data.Whs1);
				order.ConsigneePK = Helper.CreateClient().PK;
				Factory.Save();

				var docAddressControl = Enterprise.Warehouse.Transactions.GUI.Testing.GUITestHelper.FindControl<ZDocAddressControl>(form.OrderEntryUserControlForTest.Controls, "ConsigneeDocAddressControl");
				var docAddressGroupBox = Enterprise.Warehouse.Transactions.GUI.Testing.GUITestHelper.FindControl<ZOrganisationControl>(docAddressControl.Controls, "DefaultGroupBox");
				AssertEquals("Precondition", false, docAddressGroupBox.OrganisationFindBox.ReadOnly);
				form.PickButtonForTest.PerformClick();

				AssertEquals("Order should be Picking", true, order.IsAttachedToPickButNotFinalised);
				AssertEquals(hasJulianProducts, docAddressGroupBox.OrganisationFindBox.ReadOnly);
			}
		}

		#endregion

		#region TestPickButtonAbortsIfNoPickLineQuantity - NOT IMPLEMENTED

		//public void TestPickButtonAbortsIfNoPickLineQuantity()
		//{
		//   TestDataSimpleEnvironment Data = new TestDataSimpleEnvironment(Factory);
		//   WhsOrder Order = Helper.CreateWhsOrder(Data.Org1, Data.Whs1, "O1");
		//   WhsOrderLine OrderLine = Helper.CreateWhsOrderLine(Order, Data.Part1, 10m);

		//   using (OrderEntryForm Form = new OrderEntryForm(Order))
		//   {
		//      Form.Show();
		//      Form.PickButton.PerformClick();
		//      AssertEquals("Order should not be Picking", DocketStatus.Codes.Entered, Order.WD_DocketStatus);
		//      AssertEquals("Pick should be deleted", null, Order.Pick);
		//      AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("No stock could be found for this Order\n\nClick Ok to continue and create the pick anyway\nClick Cancel to abort and not create the pick"));
		//      UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

		//      Form.abortAutoPickCreationDefault = DialogResult.OK;
		//      Form.PickButton.PerformClick();
		//      AssertEquals("Order should be Picking", true, Order.IsPicking);
		//      AssertEquals("Pick should be valid", false, Order.Pick.IsCancelled);
		//      AssertEquals("Pick should not be deleted", false, Order.Pick.IsDeleted);
		//      AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("Pick P00000001 has been created"));
		//      AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
		//   }
		//}

		#endregion

		#region TestPickButtonManualPick

		public void TestPickButtonManualPick()
		{
			TestPickButtonManualPick("T1", WhsPickOption.Codes.Manual, 0m);
			TestPickButtonManualPick("T2", WhsPickOption.Codes.ManualWithAutoAllocate, 10m);
		}

		void TestPickButtonManualPick(ZString whsName, ZString pickOption, ZDecimal qtyPicked)
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory(whsName);
			data.Org1.OH_Code += pickOption;

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			var orderLine = Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			order.WD_PickOption = pickOption;
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				form.Show();
				AssertEquals("Precondition", false, order.IsAttachedToPickButNotFinalised);

				form.PickButtonForTest.PerformClick();
				form.PickManager.LastUsedPickControllerForTest.Factory.Save();

				var loadedOrder = form.PickManager.LastUsedPickControllerForTest.Factory.Load<WhsOrder>(order.PK);
				AssertEquals("Order should be Picking", true, loadedOrder.IsAttachedToPickButNotFinalised);
				AssertEquals("Order should not be saved", true, loadedOrder.IsInDatabase);
				AssertEquals("Order should be attached", true, loadedOrder.WD_WP.IsValid);

				// this test is crap because the controller uses its own factory
				AssertEquals("PickLineQuantity is incorrect", qtyPicked, loadedOrder.Pick.OrderedInventories[0].PickLineQuantity);
				AssertPickForm(order.Pick, form.PickManager.LastUsedPickControllerForTest, ODisplayMode.Browse);
			}
		}

		#endregion

		#region TestPickButtonForOrderAlreadyPicked

		public void TestPickButtonForOrderAlreadyPicked()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			var order = Helper.CreateWhsOrder(org, warehouse, "1");
			Helper.CreateWhsOrderLine(order, part, 10m);

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var pick = Factory.New<WhsPick>();
				pick.Orders.Add(order);
				Factory.Save();
				Assert("Precondition Order should be Picking", order.IsAttachedToPickButNotFinalised);

				form.PickButtonForTest.PerformClick();
				AssertPickForm(pick, form.PickManager.LastUsedPickControllerForTest, ODisplayMode.Browse);
			}
		}

		#endregion

		#region TestPickButtonToReleaseButtonAndBack

		public void TestPickButtonToReleaseButtonAndBack()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				form.Show();
				form.PickButtonForTest.PerformClick();
				form.ReleaseButtonForTest.PerformClick();

				var releaseForm = (ReleaseEntryForm)form.LastReleaseControllerForTest.LastShownForm;
				releaseForm.CancelPickButton.PerformClick();
				releaseForm.FireSaveButton();
				releaseForm.Close();
				form.PickButtonForTest.PerformClick();
				Assert("A save concurrency error should not be thrown", true);
			}
		}

		#endregion

		#region TestUserAbortedEmptyPick

		public void TestUserAbortedEmptyPick()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "1", Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				form.PickButtonForTest.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(WhsPick.NoStockWarningMessage));

				form.PostingButtonsUserControlForTest.SaveAndCloseButton.PerformClick(); // ensure no exception is thrown
			}
		}

		#endregion

		#region TestPickButtonClickForConcurrentUsers

		public void TestPickButtonClickForConcurrentUsers()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// create order and save it in one factory
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			// load the order in another factory
			var differentFactory = new BusinessObjectFactory();
			differentFactory.RefreshEnabled = false;
			var orderLoadedInDifferentFactory = differentFactory.Load<WhsOrder>(order.PK);

			// create the pick in one form
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				form.Show();
				AssertEquals("Precondition", false, order.IsAttachedToPickButNotFinalised);
				form.PickButtonForTest.PerformClick();
				AssertNotNull("Pick should be created.", order.Pick);
				AssertEquals("Pick should be saved to the database.", true, order.Pick.IsInDatabase);
			}

			// try to create the pick in another form
			using (var form = new OrderEntryForm(orderLoadedInDifferentFactory, new NotificationSubscriberGuiHelper()))
			using (ObjectFactory.Substitute<IAllocationEngineManager>(new AllocateFIFOLegacyMock()))
			{
				form.Show();
				AssertEquals("Precondition", false, orderLoadedInDifferentFactory.IsAttachedToPickButNotFinalised);
				AssertNoExceptionThrown("No exception should be thrown when pick button is clicked when another user has already created a pick while working on the order.", () => form.PickButtonForTest.PerformClick());
				AssertEquals("Loaded pick should not have any changes.", false, orderLoadedInDifferentFactory.Pick.HasChanges);
				AssertEquals("Picks should be same in both factories.", order.Pick.PK, orderLoadedInDifferentFactory.Pick.PK);
			}
		}

		#endregion

		#region TestCreatePick_OrderLocked

		public void TestCreatePick_OrderLocked()
		{
			var data = new TestDataForInventory(Factory);
			data.CreateSimpleInventory();

			// create order and save it in one factory
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1, "O1", data.Notify);
			Helper.CreateWhsOrderLine(order, data.Part1, 10m);
			Factory.Save();

			var mutex = new WhsPickCreationMutex(order);
			mutex.Lock();
			AssertEquals("Precondition", true, mutex.HasLock);

			try
			{
				using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
				{
					form.Show();
					AssertEquals("Precondition", false, order.IsAttachedToPickButNotFinalised);

					form.PickButtonForTest.PerformClick();
					AssertNull("Pick should not be created.", order.Pick);
					AssertEquals("Should have displayed a dialog", "Error " + GlbStaff.CurrentUser.GS_FullName + " is currently in the process of creating a new Pick for this Order. Try again later.", UnitTestUserNotification.Instance.LastMessage.ToString());
				}
			}
			finally
			{
				mutex.Unlock();
			}
		}

		#endregion

		#region TestPickButton_SetCorrectInitialOrderToSelectInGrid

		public void TestPickButton_SetCorrectInitialOrderToSelectInGrid()
		{
			var warehouse = Helper.CreateWarehouse("1");
			var org = Helper.CreateClient();
			var part = Helper.CreateProduct(org, "P1");
			var order1 = Helper.CreateWhsOrder(org, warehouse, "O1");
			Helper.CreateWhsOrderLine(order1, part, 10m);
			var order2 = Helper.CreateWhsOrder(org, warehouse, "O2");
			Helper.CreateWhsOrderLine(order2, part, 10m);
			var order3 = Helper.CreateWhsOrder(org, warehouse, "O3");
			Helper.CreateWhsOrderLine(order3, part, 10m);

			using (var form = new OrderEntryForm(order2, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var pick = Factory.New<WhsPick>();
				pick.Orders.Add(order1);
				pick.Orders.Add(order2);
				pick.Orders.Add(order3);
				Factory.Save();

				form.PickButtonForTest.PerformClick();

				var pickForm = form.PickManager.LastUsedPickControllerForTest.LastShownForm as PickEntryForm;
				AssertEquals(order2.PK, pickForm.SelectedOrder.PK);
				AssertEquals(true, pickForm.Visible);
				AssertEquals(pick.PK, ((BusinessObject)pickForm.BusinessEntity).PK);

				pickForm.Dispose();
			}
		}

		#endregion

		PickEntryForm AssertPickForm(WhsPick pick, ZController controller, ODisplayMode expectedDisplayMode)
		{
			var pickForm = controller.LastShownForm as PickEntryForm;

			AssertNotNull("Pick form not created", pickForm);
			AssertEquals("Form DisplayMode should be " + expectedDisplayMode, expectedDisplayMode, pickForm.DisplayMode);
			AssertEquals(true, pickForm.Visible);
			AssertEquals(pick.PK, ((BusinessObject)pickForm.BusinessEntity).PK);

			return pickForm;
		}

		#endregion

		#region Cancel Docket Handler

		public void TestOnCancelDocket()
		{
			var order = Factory.New<WhsOrder>();
			order.WD_DocketStatus = DocketStatus.Codes.Entered;

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				Env.Security.WhsOrderCancel.IsAllowed = false;
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);

				AssertEquals("No permission to Cancelation", true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals("Error with security expected", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Shouldn't have proceeded with Cancelation", false, order.IsCancelled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				Env.Security.WhsOrderCancel.IsAllowed = true;
				order.WD_DocketStatus = DocketStatus.Codes.Finalised;
				var allowToCancel = (ICancellable)order;
				string canCancelError = allowToCancel.CanCancel();
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);

				AssertEquals("No problems with permission", false, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));

				AssertEquals("'" + canCancelError + "' error was expected for order with status Finalised", true, UnitTestUserNotification.Instance.LastMessage.Contains(canCancelError));
				AssertEquals("Error was expected for order with status Finalised", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Order with status Finalised cannot be Canceled", false, order.IsCancelled);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				order.WD_DocketStatus = DocketStatus.Codes.Entered;
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);

				AssertEquals("No error was expected for order with status Entered (Saved)", false, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Orders with status Entered (Saved) should be Cancelled", true, order.IsCancelled);

				order.WD_DocketStatus = DocketStatus.Codes.Cancelled;
				form.OnCancelReactivateDocketForTest(null, EventArgs.Empty);
				AssertEquals("After reactivation Order status should be Entered", DocketStatus.Codes.Entered, order.WD_DocketStatus);
			}
		}

		#endregion

		#region OverrideFulfillmentRule

		public void TestOnOverrideFulfillmentRule()
		{
			var order = Factory.New<WhsOrder>();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				order.WD_DocketID = "WD1234";

				order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.None;
				form.OnOverrideFulfillmentRuleForTest(null, EventArgs.Empty);
				Assert(UnitTestUserNotification.Instance.LastMessage.Contains("The Fulfillment Rule is already set to 'None' for this Order: WD1234"));

				order.WD_WhsOrderFulfillmentRule = WhsOrderFulfillmentRuleList.Codes.All;
				UnitTestUserNotification.Instance.AddUserResponse("123456789");
				form.OnOverrideFulfillmentRuleForTest(null, EventArgs.Empty);
				AssertEquals(WhsOrderFulfillmentRuleList.Codes.None, order.WD_WhsOrderFulfillmentRule);
				AssertEquals("Fulfillment Rule Overridden - Ref: 123456789", order.Logs.MostRecentLog.SL_Reference);
			}
		}

		#endregion

		#region INotifications Members

		public void TestNotify()
		{
			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				var type = new Environment.Business.Testing.TestINotificationType("Message", "NotZErrorMessageBox");
				var e = new Environment.Business.Testing.TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				((INotifications)form).Add(e);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("TestMessageToDisplay"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				type = new Environment.Business.Testing.TestINotificationType("Pick", "ZErrorMessageBox");
				e = new Environment.Business.Testing.TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				((INotifications)form).Add(e);
				AssertEquals("There are errors that need to be corrected before this Warehouse Order can be Picked.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);

				type = new Environment.Business.Testing.TestINotificationType("Finalise", "ZErrorMessageBox");
				e = new Environment.Business.Testing.TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				((INotifications)form).Add(e);
				AssertEquals("There are errors that need to be corrected before this Warehouse Order can be Finalized.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			}
		}

		public void TestQueryUser()
		{
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(order, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
			}
		}

		#endregion

		#region DocketID Sequence

		public void TestDocketIDSequence()
		{
			AssertDocketIDSequence("W00000001");
			AssertDocketIDSequence("W00000002");
			AssertDocketIDSequence("W00000003");
		}

		void AssertDocketIDSequence(ZString docketID)
		{
			var docket = Helper.CreateWhsOrder(Helper.CreateClient(docketID), Helper.CreateWarehouse("1"));
			docket.WD_ExternalReference = docketID;
			docket.WD_DocketID = "";
			docket.ConsigneePK = docket.WD_OH_Client;
			docket.ConsigneeAddressPK = docket.Client.MainAddress.PK;
			docket.WD_RequiredDate = ZDateTimeOffset.Today;

			using (var form = new OrderEntryForm(docket, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Precondition", false, docket.IsInDatabase);
				Factory.Save();
			}

			AssertEquals("Precondition", true, docket.IsInDatabase);
			AssertEquals(docketID, docket.WD_DocketID);
		}

		#endregion

		#region TestDeniedPartyScreeningControls

		public void TestVisibleDeniedPartyScreeningControls()
		{
			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var dropEditControl = GUITestHelper.FindControl<DeniedPartyScreeningStatusDropEdit>(form.Controls, "ScreeningStatusDropEdit");
				Assert(GUITestHelper.FindControl<ZButton>(form.Controls, "ScreenButton").Visible);
				Assert(dropEditControl.Visible);
				AssertEquals("NOT", dropEditControl.Text);
			}
		}

		public void TestScreeningStatus_WhenStatusChanges_ExpectColorChangesBasedOnStatus()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();
			order.WD_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
			Factory.Save();

			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				var dropEditControl = GUITestHelper.FindControl<DeniedPartyScreeningStatusDropEdit>(form.Controls, "ScreeningStatusDropEdit");

				AssertCodeBoxAndDescriptionAreThisColor("Pre-condition", dropEditControl, DeniedPartyConstants.GridColor.NotScreened);

				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertCodeBoxAndDescriptionAreThisColor("Should be cleared (green)", dropEditControl, DeniedPartyConstants.GridColor.Clear);

				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Unknown;
				AssertCodeBoxAndDescriptionAreThisColor("Should be unknown (orange)", dropEditControl, DeniedPartyConstants.GridColor.Unknown);

				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertCodeBoxAndDescriptionAreThisColor("Should be matched (red)", dropEditControl, DeniedPartyConstants.GridColor.Matched);
			}
		}

		public void TestDPSMenuItemsPresent()
		{
			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				Assert(form.Menu.MenuItems.FindByText("View Compliance Status", true).Visible);
				Assert(form.Menu.MenuItems.FindByText("Resynchronize Screening Status", true).Visible);
			}
		}

		#endregion

		#region Test Mark as Job Clear

		public void TestMarkAsJobClearMenuItemExist()
		{
			AssertMarkAsJobClearMenuItemExist(true);
			AssertMarkAsJobClearMenuItemExist(false);
		}

		public void TestMarkAsJobClearWhenSecurityRightsIsDenied_ShouldShowErrorMessage()
		{
			var tmpSecurityCore = GetTemporarySecurityCore();

			using (Env.SetTemporarySecurityInstanceForTest(tmpSecurityCore))
			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrderEntryForm(Factory.NewWithValidTestData<WhsOrder>(), new NotificationSubscriberGuiHelper()))
			{
				Factory.Save();
				form.Show();

				tmpSecurityCore.OrgDeniedPartyScreeningAllowJobLevelClear.IsAllowed = false;
				new DpsMarkJobScreeningStatusClearTest().AssertSecurityRightsAccessibilityCheckpoint(form, tmpSecurityCore);
			}
		}

		public void TestMarkAsJobClearWhenScreeningStatusIsJCLorCLR_ShouldShowWarningMessage()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.JobCleared;
				AssertEquals("Precondition order screening status", "JCL", order.WD_ScreeningStatus);
				Factory.Save();

				var markJobClearTestHelper = new DpsMarkJobScreeningStatusClearTest();
				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);

				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Clear;
				AssertEquals("Precondition order screening status", "CLR", order.WD_ScreeningStatus);
				Factory.Save();

				markJobClearTestHelper.AssertStatusAlreadyClearOrJobClear(form);
			}
		}

		public void TestMarkAsJobClearWhenJobIsNotSaved_ShouldShowWarningMessage()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.NotScreened;
				AssertEquals("Precondition order screening status", "NOT", order.WD_ScreeningStatus);

				new DpsMarkJobScreeningStatusClearTest().AssertSaveBeforeMarkingClear(form);
			}
		}

		public void TestMarkAsJobClearSetScreeningStatusToJCL()
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				order.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
				AssertEquals("Precondition order screening status", "MAT", order.WD_ScreeningStatus);
				Factory.Save();

				new DpsMarkJobScreeningStatusClearTest().AssertScreeenigStatusToJCL(form, order);
			}
		}

		SecurityCore GetTemporarySecurityCore()
		{
			return new SecurityCore(null, EnvProxy.Instance.CurrentUser.PK, EnvProxy.Instance.CurrentBranch.PK, EnvProxy.Instance.CurrentDepartment.PK, EnvProxy.Instance.CurrentCompany.PK);
		}

		void AssertMarkAsJobClearMenuItemExist(bool registryValue)
		{
			var order = Factory.NewWithValidTestData<WhsOrder>();

			using (OrganisationsDataRegistry.Instance.DeniedpartyScreeningEnableJobClear.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new OrderEntryForm(order, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				new DpsMarkJobScreeningStatusClearTest().AssertMenuItemAccessibilityCheckpoint(form, registryValue);
			}
		}

		#endregion Test Mark as Job Clear

		#region eConversations

		public void TestEConversationsPlugIn_Visible()
		{
			AssertEConversationPlugInVisibility(true);
		}

		public void TestEConversationsPlugIn_Hidden()
		{
			AssertEConversationPlugInVisibility(false);
		}

		void AssertEConversationPlugInVisibility(bool registryValue)
		{
			using (GlowRegistry.Instance.NeoEnableConversations.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryValue))
			using (var form = new OrderEntryForm(Factory.New<WhsOrder>(), new NotificationSubscriberGuiHelper()))
			{
				var eConverationPlugIn = form.PlugIns.GetPlugIn(ControllerIDs.eConversationPlugIn);

				if (registryValue)
				{
					AssertNotNull(eConverationPlugIn);
				}
				else
				{
					AssertNull(eConverationPlugIn);
				}
			}
		}

		#endregion

		#region Implementation

		public void TestControllers()
		{
			using (var form = new OrderEntryForm(Factory.New<WhsOrder>(), new NotificationSubscriberGuiHelper()))
			{
				AssertNotEquals("ReleaseController should not be cached", form.ReleaseControllerForTest, form.ReleaseControllerForTest);
			}
		}

		void AssertCodeBoxAndDescriptionAreThisColor(string message, ZDropEdit status, Color color)
		{
			CombineAssertions(message, () =>
			{
				AssertEquals("Description box color should be:", color, status.DescriptionBox.BackColor);
				AssertEquals("Code box color should be:", color, status.DescriptionBox.BackColor);
			});
		}

		#endregion
	}
}
