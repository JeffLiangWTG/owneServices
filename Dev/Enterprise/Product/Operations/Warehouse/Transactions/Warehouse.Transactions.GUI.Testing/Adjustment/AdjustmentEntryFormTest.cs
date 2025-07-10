using System;
using CargoWise.ComponentModel;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.GUI;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.Warehouse.Transactions.GUI;
using Enterprise.Warehouse.Transactions.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Warehouse.Transactions.GUI.Testing
{
	public class AdjustmentEntryFormTest : WhsDocketFormTestCase
	{
		#region Constructors

		[TestDate(2015, 10, 21)]
		public void TestConstructor()
		{
			var tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Now.Date.AddDays(1) };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);

			var adjust = Factory.New<WhsAdjustment>();
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				AssertEquals("Posting not setup", true, form.GetSetupPostingCalled());
				AssertNotNull("DocData not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
				AssertNotNull("EDocs not plugged in", form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn));
				AssertBinding(form.GetFinaliseButton(), "ReadOnly", "ManualFinaliseReadonly");

				var menuItem = form.GetActionsMenuItem().MenuItems.FindByText("Export Adjustment Confirmation Job To XML");
				AssertNotNull("XML Export Item not added", menuItem);
				AssertNotNull("Export XML to Email Item not added", menuItem.MenuItems.FindByText("Send Email"));
				AssertNotNull("Export XML to File Item not added", menuItem.MenuItems.FindByText("Store to File"));
			}

			tomorrow = new InterfaceConnectorTemporarilyEnabledUntil() { EnabledUntil = ZDateTime.Empty };
			eHubMessagingRegistry.Instance.InterfaceConnectorTemporarilyEnabledUntilItem.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, tomorrow);
			AssertExceptionThrown<InvalidOperationException>("Exception should be thrown if parameterless constructor is called outside designer.",
				() => new AdjustmentEntryForm());
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new AdjustmentEntryForm(adjust, null));
		}

		#endregion

		#region TestEDocsModifySetToOffNotDisablePlugin

		public void TestEDocsModifySetToOffNotDisablePlugin()
		{
			Env.Security.eDocs.IsAllowed = true;
			Env.Security.eDocsModify.IsAllowed = false;
			var helper = new NotificationSubscriberGuiHelper();
			var adjust = Factory.New<WhsAdjustment>();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				form.Show();
				var eDocPlugin = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn);
				Assert("Plugin should not be disabled.", eDocPlugin.SecurityCheckpoint.IsAllowed);
			}
		}

		#endregion

		public void TestSetupInventoryFilterStripUserControl()
		{
			Assert("incomplete test", true);
		}

		#region GetNewDocketForm

		protected override ZForm GetNewDocketForm()
		{
			var helper = new NotificationSubscriberGuiHelper();
			return new AdjustmentEntryForm(Factory.New<WhsAdjustment>(), helper);
		}

		#endregion

		#region ZForm Overloads

		public void TestOnLoad()
		{
			var helper = new NotificationSubscriberGuiHelper();
			var adjust = Factory.New<WhsAdjustment>();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				form.Show();
				form.EndInvoke(form.GetDeferredAsyncResult());
				AssertEquals("Focus is not on OrgWhsFindControl", form.GetOrgWhsFindControl(), form.ActiveControl);
			}
		}

		#endregion

		#region Properties

		#region TestDocket

		public void TestDocket()
		{
			var helper = new NotificationSubscriberGuiHelper();
			var adjust = Factory.New<WhsAdjustment>();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				AssertEquals(adjust, form.GetDocket());
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			var helper = new NotificationSubscriberGuiHelper();
			var adjust = Factory.New<WhsAdjustment>();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				AssertEquals("Adjustment", form.FormCaption.Trim());
				adjust.WD_DocketID = "W00000001";
				AssertEquals("Adjustment W00000001", form.FormCaption);
			}
		}

		public void TestFormCaptionWhenDocketNull()
		{
			var helper = new NotificationSubscriberGuiHelper();
			var adjust = Factory.New<WhsAdjustment>();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				form.SetDataBinding(null, "");
				AssertEquals("Adjustment ", form.FormCaption);
			}
		}

		#endregion

		#region TestShowBottomPanel

		public void TestShowBottomPanel()
		{
			var adjustment = Factory.New<WhsAdjustment>();
			var newOwnershipAdjustmentChild = Factory.New<WhsAdjustment>();
			var newOwnershipAdjustmentParent = Factory.New<WhsAdjustment>();
			newOwnershipAdjustmentChild.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			newOwnershipAdjustmentParent.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			newOwnershipAdjustmentChild.WD_WD_ParentDocket = newOwnershipAdjustmentParent.PK;
			var whsNotificationSubscriberGuiHelper = new NotificationSubscriberGuiHelper();
			// Entered adjustment
			using (var form = new AdjustmentEntryForm(adjustment, whsNotificationSubscriberGuiHelper))
			{
				form.Show();
				AssertEquals(true, form.GetBottomPanel().Visible);
				AssertEquals(true, form.GetSplitter().Visible);

				form.ShowBottomPanel = false;
				AssertEquals(false, form.GetBottomPanel().Visible);
				AssertEquals(false, form.GetSplitter().Visible);
			}

			// Finalised adjustment
			adjustment.WD_FinalisedDate = ZDateTimeOffset.Now;
			using (var form = new AdjustmentEntryForm(adjustment, whsNotificationSubscriberGuiHelper))
			{
				form.Show();
				AssertEquals(false, form.GetBottomPanel().Visible);
				AssertEquals(false, form.GetSplitter().Visible);
			}
			// Entered new ownership adjustment child
			using (var form = new AdjustmentEntryForm(newOwnershipAdjustmentChild, whsNotificationSubscriberGuiHelper))
			{
				form.Show();
				AssertEquals(false, form.GetBottomPanel().Visible);
				AssertEquals(false, form.GetSplitter().Visible);
			}

			// Finalised new ownership adjustment child
			newOwnershipAdjustmentChild.WD_FinalisedDate = ZDateTimeOffset.Now;
			using (var form = new AdjustmentEntryForm(newOwnershipAdjustmentChild, whsNotificationSubscriberGuiHelper))
			{
				form.Show();
				AssertEquals(false, form.GetBottomPanel().Visible);
				AssertEquals(false, form.GetSplitter().Visible);
			}

			// Entered new ownership adjustment parent
			using (var form = new AdjustmentEntryForm(newOwnershipAdjustmentParent, whsNotificationSubscriberGuiHelper))
			{
				form.Show();
				AssertEquals(true, form.GetBottomPanel().Visible);
				AssertEquals(true, form.GetSplitter().Visible);

				form.ShowBottomPanel = false;
				AssertEquals(false, form.GetBottomPanel().Visible);
				AssertEquals(false, form.GetSplitter().Visible);
			}

			// Finalised new ownership adjustment parent
			newOwnershipAdjustmentParent.WD_FinalisedDate = ZDateTimeOffset.Now;
			using (var form = new AdjustmentEntryForm(newOwnershipAdjustmentParent, whsNotificationSubscriberGuiHelper))
			{
				form.Show();
				AssertEquals(false, form.GetBottomPanel().Visible);
				AssertEquals(false, form.GetSplitter().Visible);
			}
		}

		#endregion

		#endregion

		#region Finalise Button

		public void TestFinaliseButton()
		{
			// setup an adjustment docket to use with the form
			var warehouse = Helper.CreateWarehouse("1", "A", 1, 1);
			var client = Helper.CreateClient();
			var prod = Helper.CreateProduct(client, "P1");
			Factory.Save();
			var helper = new NotificationSubscriberGuiHelper();
			var adjust = Helper.CreateWhsAdjustment(client, warehouse, "1");
			Helper.CreateWhsAdjustmentLine(adjust, prod, 10, "A");

			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				form.Show();

				// test user has no access
				Env.Security.WhsAdjustmentFinalise.IsAllowed = false;
				form.GetFinaliseButton().PerformClick();

				// check for security error
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				// ensure adjustment wasnt processed
				AssertEquals(false, adjust.IsFinalised);

				// test user has access
				Env.Security.WhsAdjustmentFinalise.IsAllowed = true;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				AssertEquals(false, form.BusinessEntity.HasErrors());

				// test finalisation worked and notification subscriber was popped
				form.GetFinaliseButton().PerformClick();
				AssertEquals(true, adjust.IsFinalised);
				AssertEquals(form.GetNotificationBufferForTest(), adjust.NotificationManager.LastPopped);

				// ensure no security error occurred
				AssertEquals(false, UnitTestUserNotification.Instance.LastMessage.Contains(SecurityCore.SecurityErrorMessage));
			}
		}

		public void TestFinaliseButtonValidatesAndSaves()
		{
			// setup an adjustment docket to use with the form
			var warehouse = Helper.CreateWarehouse("1", "A", 1, 1);
			var client = Helper.CreateClient();
			var prod = Helper.CreateProduct(client, "P1");
			Factory.Save();

			var adjust = Helper.CreateWhsAdjustment(client, warehouse, "1");
			var adjustLine = Helper.CreateWhsAdjustmentLine(adjust, prod, 0, "A");
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				form.Show();

				form.GetFinaliseButton().PerformClick();
				AssertEquals(false, adjust.IsInDatabase);
				AssertEquals(false, adjust.IsFinalised);
				AssertEquals(true, adjust.HasErrors);

				adjustLine.WE_TransactionQuantity = 10;
				form.GetFinaliseButton().PerformClick();
				AssertEquals(true, adjust.IsInDatabase);
				AssertEquals(true, adjust.IsFinalised);
				AssertEquals(false, adjust.HasErrors);
			}
		}

		#endregion

		#region INotifications Members

		public void TestNotify()
		{
			var adjust = Factory.New<WhsAdjustment>();
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdjustmentEntryForm(adjust, helper))
			{
				var type = new Enterprise.Warehouse.Environment.Business.Testing.TestINotificationType("Message", "NotZErrorMessageBox");
				var e = new Enterprise.Warehouse.Environment.Business.Testing.TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				form.Notify(e);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.Contains("TestMessageToDisplay"));
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);

				type = new Enterprise.Warehouse.Environment.Business.Testing.TestINotificationType("Finalize", "ZErrorMessageBox");
				e = new Enterprise.Warehouse.Environment.Business.Testing.TestINotificationSubscriberNotification("TestMessageToDisplay", type);
				form.Notify(e);
				AssertEquals("There are errors that need to be corrected before this Warehouse Adjustment can be Finalized.", UnitTestUserNotification.Instance.PreviousMessages[0].Text);
			}
		}

		public void TestQueryUser()
		{
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			var adjust = Factory.New<WhsAdjustment>();
			using (var form = new AdjustmentEntryForm(adjust, helperMock.Object))
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
			var docket = Factory.NewWithValidTestData<WhsAdjustment>();
			docket.WD_ExternalReference = docketID;
			docket.WD_DocketID = "";
			docket.WD_BookingDate = ZDateTimeOffset.Now;
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdjustmentEntryForm(docket, helper))
			{
				form.Show();
				AssertEquals("Precondition", false, docket.IsInDatabase);
				form.GetPostingButtonsUserControl().SaveAndCloseButton.PerformClick();
			}

			AssertEquals("Precondition", true, docket.IsInDatabase);
			AssertEquals(docketID, docket.WD_DocketID);
		}

		#endregion

		#region TestNewClientUserControl

		public void TestNewClientUserControl()
		{
			var helper = new NotificationSubscriberGuiHelper();
			var adjustment = Factory.New<WhsAdjustment>();
			var parentAdjustment = Factory.New<WhsAdjustment>();
			adjustment.WD_DocketSubType = AdjustmentType.Codes.Adjustment;
			using (var form = new AdjustmentEntryForm(adjustment, helper))
			{
				form.Show();
				AssertEquals(false, form.GetNewOrgFindControl().Visible);
			}

			adjustment.WD_DocketSubType = AdjustmentType.Codes.InternalWarehouseAdjustment;
			using (var form = new AdjustmentEntryForm(adjustment, helper))
			{
				form.Show();
				AssertEquals(false, form.GetNewOrgFindControl().Visible);
			}

			adjustment.WD_DocketSubType = AdjustmentType.Codes.OwnershipAdjustment;
			using (var form = new AdjustmentEntryForm(adjustment, helper))
			{
				form.Show();
				AssertEquals(true, form.GetNewOrgFindControl().Visible);
			}

			adjustment.WD_WD_ParentDocket = parentAdjustment.PK;
			using (var form = new AdjustmentEntryForm(adjustment, helper))
			{
				form.Show();
				AssertEquals(false, form.GetNewOrgFindControl().Visible);
			}
		}

		#endregion
	}
}
