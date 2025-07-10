using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.DocumentScanning.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(VASOrderEntryForm))]
	class VASOrderEntryFormBasherTest : ZFormBasherTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			AssertExceptionThrown<InvalidOperationException>("Exception should be thrown if parameterless constructor is called outside designer.",
				() => new VASOrderEntryForm());
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new VASOrderEntryForm(vasOrder, null));
		}

		#endregion

		#region TestButtonsReadOnlyIfVASOrderCancelled

		public void TestButtonsReadOnlyIfVASOrderCancelled()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			vasOrder.IsCancelled = true;
			Factory.Save();

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				AssertEquals(false, form.CreateInitialTransferButton.Enabled);
				AssertEquals(false, form.FinaliseButton.Enabled);
				AssertEquals(false, form.MarkVASOrderCompletedButton.Enabled);
			}
		}

		#endregion

		#region TestCreateInitialTransfer

		public void TestCreateInitialTransfer()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.CreateInitialTransferButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Cannot create Transfer as there are no Service Lines.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Button text is 'Create Transfer' by default", "Create Transfer In", form.CreateInitialTransferButton.CaptionResourceString.Caption);

				form.CreateInitialTransferButton.PerformClick();

				using (form.LastUsedTransferControllerForTesting.LastShownForm)
				{
					AssertEquals("Button text should change once Transfer is created.", "View Transfer In", form.CreateInitialTransferButton.CaptionResourceString.Caption);
					AssertNotNull(form.LastUsedTransferControllerForTesting);
					AssertEquals("Should have opened Transfer form.", ControllerIDs.WhsTransfer, form.LastUsedTransferControllerForTesting.ID);
					AssertNotNull(form.LastUsedTransferControllerForTesting.LastShownForm);
					AssertEquals("Form should be shown Modally.", form, form.LastUsedTransferControllerForTesting.ParentModalForm);
					AssertEquals("Should have saved all changes.", false, vasOrder.HasChanges);
				}

				form.LastUsedTransferControllerForTesting = null;
			}

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Button text should remain as View Transfer In once transfer has been created.", "View Transfer In", form.CreateInitialTransferButton.Text);

				var transfer = vasOrder.TransferIntoServiceArea;
				form.CreateInitialTransferButton.PerformClick();
				using (form.LastUsedTransferControllerForTesting.LastShownForm)
				{
					AssertEquals("Should open Transfer that was already created.", transfer.PK,
						form.LastUsedTransferControllerForTesting.LastShownForm.BusinessEntityForPersistingForm.Identifier);
				}
			}
		}

		#endregion

		#region TestFinaliseVASOrder

		public void TestFinaliseVASOrder()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			Factory.Save();
			AssertNotNull("Precondition: Initial Transfer is created.", initialTransfer);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FinaliseButton.PerformClick();
				AssertEquals("Should have an Error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Cannot Finalize VAS Order until the Work has been Completed.", UnitTestUserNotification.Instance.LastMessage.Text);

				initialTransfer.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(initialTransfer);
				Factory.Save();

				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FinaliseButton.PerformClick();
				AssertEquals("Should have an Error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Save all changes before Finalizing this VAS Order.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have attempted to save VAS Order.", true, vasOrder.HasChanges);

				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FinaliseButton.PerformClick();
				AssertEquals("VAS Order should be Finalised.", true, vasOrder.IsFinalised);
				AssertEquals("VAS Order should have been saved.", false, vasOrder.HasChanges);
			}
		}

		#endregion

		#region TestFinaliseVASOrder_ServiceJobNotCompleted

		public void TestFinaliseVASOrder_ServiceJobNotCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			var service = Factory.New<WhsJobService>();
			service.ES_ParentID = vasOrder.PK;
			service.ES_ParentTableCode = vasOrder.TablePrefix;
			service.ES_ServiceCode = "CHO";
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			Factory.Save();
			AssertNotNull("Precondition: Initial Transfer is created.", initialTransfer);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FinaliseButton.PerformClick();
				AssertEquals("Should have an Error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Cannot Finalize VAS Order until the Work has been Completed.", UnitTestUserNotification.Instance.LastMessage.Text);

				initialTransfer.FinaliseDocketWithoutUserConfirmation();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(initialTransfer);
				Factory.Save();

				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);

				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FinaliseButton.PerformClick();
				AssertEquals("Should have an Error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Cannot Finalize VAS Order until all Services have been Completed.", UnitTestUserNotification.Instance.LastMessage.Text);

				var servicesTab = GUITestHelper.FindControl<ZTabPage>(form.Controls, "ServicesTab");
				AssertEquals("Error icon is added to Service Tab. ", Icons.GetImageIndex(IconTypes.Error), servicesTab.ImageIndex);
			}
		}

		#endregion

		#region TestFinaliseVASOrder_TransferFinalizedInOtherFactory

		public void TestFinaliseVASOrder_TransferFinalizedInOtherFactory()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				form.CreateInitialTransferButton.PerformClick();
				AssertNotNull("Precondition: Transfer into service area is created.", vasOrder.TransferIntoServiceArea);

				var controller = ZControllerFactory.Create(ControllerIDs.WhsTransfer);
				var initialTransferFromControllerFactory = controller.Factory.Load<WhsTransfer>(vasOrder.TransferIntoServiceArea.PK);
				initialTransferFromControllerFactory.FinaliseDocketWithoutUserConfirmation();
				controller.Factory.Save();
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(initialTransferFromControllerFactory);
				WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(vasOrder.TransferIntoServiceArea);

				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Precondition: VAS Order is Complete.", true, vasOrder.WVO_WorkCompletedTimeUtc.IsValid);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				AssertNoExceptionThrown(() => form.FinaliseButton.PerformClick());
				AssertEquals("VAS Order should be Finalised.", true, vasOrder.IsFinalised);
				AssertEquals("VAS Order should have been saved.", false, vasOrder.HasChanges);
			}
		}

		#endregion

		#region TestCreateTransferOut_ServiceJobNotCompleted

		public void TestCreateTransferOut_ServiceJobNotCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			var service = Factory.New<WhsJobService>();
			service.ES_ParentID = vasOrder.PK;
			service.ES_ParentTableCode = vasOrder.TablePrefix;
			service.ES_ServiceCode = "CHO";
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			Factory.Save();
			AssertNotNull("Precondition: Initial Transfer is created.", initialTransfer);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Should have an Error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Before completing the VAS Order, the Transfer into the Service Area must first be finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(initialTransfer);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Button text is 'Mark as Completed' by default", "Mark as Completed", form.MarkVASOrderCompletedButton.CaptionResourceString.Caption);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Button text should change once VAS Order is completed.", "Create Transfer Out", form.MarkVASOrderCompletedButton.CaptionResourceString.Caption);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Successfully Completed the VAS Order.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Successfully Completed", UnitTestUserNotification.Instance.LastMessage.Caption);
				form.FireSaveButton();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Cannot create Transfer out of Service Area until all Services have been Completed.", UnitTestUserNotification.Instance.LastMessage.Text);

				var servicesTab = GUITestHelper.FindControl<ZTabPage>(form.Controls, "ServicesTab");
				AssertEquals("Error icon is added to Service Tab. ", Icons.GetImageIndex(IconTypes.Error), servicesTab.ImageIndex);
			}
		}

		#endregion

		#region TestMarkVASOrderAsCompleted

		public void TestMarkVASOrderAsCompleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m);
			Helper.CreateWhsVASOrderLine(vasOrder, data.Part1, 10m);
			Factory.Save();

			var initialTransfer = vasOrder.GetOrCreateInitialTransfer(new TestNotificationBuffer());
			Factory.Save();
			AssertNotNull("Precondition: Initial Transfer is created.", initialTransfer);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Should have an Error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Before completing the VAS Order, the Transfer into the Service Area must first be finalized.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			initialTransfer.FinaliseDocketWithoutUserConfirmation();
			Factory.Save();
			WhsTestCaseWithFactory.AssertIsFinalisedPrecondition(initialTransfer);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Button text is 'Mark as Completed' by default", "Mark as Completed", form.MarkVASOrderCompletedButton.CaptionResourceString.Caption);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Button text should change once VAS Order is completed.", "Create Transfer Out", form.MarkVASOrderCompletedButton.CaptionResourceString.Caption);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Successfully Completed the VAS Order.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Successfully Completed", UnitTestUserNotification.Instance.LastMessage.Caption);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Save all changes before creating the Transfer out of the Service Area.", UnitTestUserNotification.Instance.LastMessage.Text);

				form.FireSaveButton();
			}

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Button text should be as Create Transfer Out once VAS Order has been completed.", "Create Transfer Out", form.MarkVASOrderCompletedButton.Text);

				form.MarkVASOrderCompletedButton.PerformClick();
				AssertEquals("Button text should change once Return Transfer is created.", "View Transfer Out", form.MarkVASOrderCompletedButton.CaptionResourceString.Caption);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasInformation);
				AssertEquals("Save the VAS Order before viewing the Transfer Out.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Save VAS Order", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertNotNull("Return Transfer should have been created.", vasOrder.TransferOutOfServiceArea);

				form.FireSaveButton();
				form.MarkVASOrderCompletedButton.PerformClick();

				using (form.LastUsedTransferControllerForTesting.LastShownForm)
				{
					AssertNotNull(form.LastUsedTransferControllerForTesting);
					AssertEquals("Should have opened Transfer form.", ControllerIDs.WhsTransfer, form.LastUsedTransferControllerForTesting.ID);
					AssertNotNull(form.LastUsedTransferControllerForTesting.LastShownForm);
					AssertEquals("Form should be shown Modally.", form, form.LastUsedTransferControllerForTesting.ParentModalForm);
					AssertEquals("Should open Return Transfer that was created.", vasOrder.TransferOutOfServiceArea.PK,
						form.LastUsedTransferControllerForTesting.LastShownForm.BusinessEntityForPersistingForm.Identifier);
				}

				form.LastUsedTransferControllerForTesting = null;
			}

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("Button text should remain as View Transfer Out once transfer has been created.", "View Transfer Out", form.MarkVASOrderCompletedButton.Text);
			}
		}

		#endregion

		#region TestIsResizableByTabPageAllowed

		public void TestIsResizableByTabPageAllowed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals(true, form.IsResizableByTabPageAllowed);
			}
		}

		#endregion

		#region TestFormCaption

		public void TestFormCaption()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertEquals("FormCaption", "VAS Order", form.FormCaption);
				Factory.Save();
				AssertEquals("FormCaption", "VAS Order WV00000001", form.FormCaption);
			}
		}

		#endregion

		#region TestHandleSaveException_ShowsMessageWhenTriggersFail

		public void TestHandleSaveException_ShowsMessageWhenTriggersFail()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			GUITestHelper.AssertHandleSaveException_ShowsMessageWhenTriggersFail(() => new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()));
		}

		#endregion

		#region TestServicesTabVisiblity

		public void TestServicesTabVisiblity()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();

			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				var servicesTab = GUITestHelper.FindControl<ZTabPage>(form.Controls, "ServicesTab");
				AssertEquals("Service tab should be visible.", true, servicesTab.TabVisible);
			}
		}

		#endregion

		#region TestPlugin_JobInvoicing

		public void TestPlugin_JobInvoicing()
		{
			var vasOrder = Factory.New<WhsVASOrder>();
			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				form.Show();
				AssertNotNull("JobInvoicing should be plugged in.", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
		}

		#endregion

		// interfaces

		#region TestINotifications

		public void TestINotifications()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				INotifications notifications = form;
				notifications.AddError("Some Error");
				AssertEquals("Some Error", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
			}
		}

		#endregion

		#region TestINotificationSubscriberQueryUser

		public void TestINotificationSubscriberQueryUser()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			using (var form = new VASOrderEntryForm(vasOrder, new NotificationSubscriberGuiHelper()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				INotificationSubscriberQueryUser notifications = form;
				var eventArgs = new QueryUserYesNoEventArgs("Test", "Hello", false);
				notifications.QueryUser(eventArgs);
				AssertEquals("Should have been a Question.", true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
				AssertEquals("Caption should be correct.", "Test", UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals("Message should be correct.", "Hello", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Response should be true if user pressed yes.", true, eventArgs.Response);
			}
		}

		public void TestQueryUser()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			var helperMock = new Mock<INotificationSubscriberQueryUser>();
			using (var form = new VASOrderEntryForm(vasOrder, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
			}
		}

		#endregion

		#region Test eDocs ReadOnly with DisplyMode

		public void TestVASOrder_eDocs_EditMode()
		{
			TestVASOrder_eDocs_Core(ODisplayMode.Edit, false);
		}

		public void TestVASOrder_eDocs_ViewMode()
		{
			TestVASOrder_eDocs_Core(ODisplayMode.ReadOnly, true);
		}

		public void TestVASOrder_eDocs_DeleteMode()
		{
			TestVASOrder_eDocs_Core(ODisplayMode.Delete, true);
		}

		void TestVASOrder_eDocs_Core(ODisplayMode expectedDisplayMode, bool expectedReadOnly)
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var serviceArea = Helper.CreateServiceAreaForVASOrder(data.Whs1);
			var vASOrder = Helper.CreateWhsVASOrder(serviceArea, data.Org1);
			Helper.CreateWhsVASOrderLine(vASOrder, data.Part1, 10m);

			Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 10m, data.Whs1.FindLocation("A-1"), "");

			Factory.Save();
			Helper.Notify.Clear();

			if (expectedDisplayMode != ODisplayMode.Delete)
			{
				var transfer = vASOrder.GetOrCreateInitialTransfer(Helper.Notify);
				AssertNotNull(transfer);
				AssertNull("No Error Message should have been made.", Helper.Notify.LastEvent);
				AssertEquals("Transfer Into Service Area should be set on VAS Order.", transfer.PK, vASOrder.WVO_WD_TransferIntoServiceArea);

				transfer.FinaliseDocketWithoutUserConfirmation();
				Factory.Save();
			}

			var controller = ZControllerFactory.Create(ControllerIDs.WhsVASOrder);

			var form = null as VASOrderEntryForm;

			switch (expectedDisplayMode)
			{
				case ODisplayMode.Edit:
					form = controller.ShowEditForm(vASOrder) as VASOrderEntryForm;
					break;
				case ODisplayMode.ReadOnly:
					form = controller.ShowViewForm(vASOrder) as VASOrderEntryForm;
					break;
				case ODisplayMode.Delete:
					form = controller.ShowDeleteForm(vASOrder) as VASOrderEntryForm;
					break;
				default:
					break;
			}

			AssertNotNull(form);
			using (form)
			{
				vASOrder = form.BusinessEntity as WhsVASOrder;
				AssertNotNull(vASOrder);

				var eDocControl = form.PlugIns.GetPlugIn(ControllerIDs.eDocsPlugIn).UserControl as eDocsUserControl;
				AssertNotNull(eDocControl);

				var docManagerInfo = vASOrder.DocManagerInfo();
				AssertNotNull(docManagerInfo);

				AssertEquals($"DocManagerInfo ReadOnly should have been {expectedReadOnly}", expectedReadOnly, docManagerInfo.ReadOnly);
				AssertEquals($"eDocsUserControl ReadOnly should have been {expectedReadOnly}", expectedReadOnly, eDocControl.ReadOnly);
			}
		}
		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var vasOrder = Helper.CreateWhsVASOrder(data.Whs1.Areas[0], data.Org1);
			Factory.Save();
			return new VASOrderEntryForm(new BusinessObjectFactory().Load<WhsVASOrder>(vasOrder.PK), new NotificationSubscriberGuiHelper()) { ControllerID = ControllerIDs.WhsVASOrder };
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
