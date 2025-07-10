using System;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Moq;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	[TestedType(typeof(AdHocServiceJobEntryForm))]
	class AdHocServiceJobEntryFormBasherTest : ZFormBasherTest
	{
		#region TestConstructor

		public void TestConstructor()
		{
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdHocServiceJobEntryForm(Factory.New<WhsAdHocServiceJob>(), helper))
			{
				AssertNotNull("Billing should be plugged in", form.PlugIns.GetPlugIn(ControllerIDs.JobInvoicing));
			}
			AssertExceptionThrown<InvalidOperationException>("Exception should be thrown if parameterless constructor is called outside designer.",
				() => new AdHocServiceJobEntryForm());
			AssertExceptionThrown<ArgumentNullException>("Exception should be thrown if null INotificationSubscriberQueryUser is passed in.",
				() => new AdHocServiceJobEntryForm(Factory.New<WhsAdHocServiceJob>(), null));
		}

		#endregion

		#region TestIsResizableByTabPageAllowed

		public void TestIsResizableByTabPageAllowed()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdHocServiceJobEntryForm(adhocServiceJob, helper))
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
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdHocServiceJobEntryForm(adhocServiceJob, helper))
			{
				form.Show();
				AssertEquals("FormCaption", "Ad Hoc Service Job", form.FormCaption);
				Factory.Save();
				AssertEquals("FormCaption", "Ad Hoc Service Job WI00000001", form.FormCaption);
			}
		}

		#endregion

		#region TestFinaliseAdHocServiceJob

		public void TestFinaliseAdHocServiceJob()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			adhocServiceJob.HasChanges = true;
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdHocServiceJobEntryForm(adhocServiceJob, helper))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.FinaliseButton.PerformClick();
				AssertEquals("Should have an Error Message.", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Save all changes before Finalizing this Ad Hoc Service Job.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should not have attempted to save Ad Hoc Service Job.", true, adhocServiceJob.HasChanges);

				form.FireSaveButton();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.FinaliseButton.PerformClick();
				AssertEquals("Ad Hoc Service Job should be Finalised.", true, adhocServiceJob.IsFinalised);
				AssertEquals("Should not have attempted to save Ad Hoc Service Job.", true, adhocServiceJob.HasChanges);
			}
		}

		#endregion

		// interfaces

		#region TestINotifications

		public void TestINotifications()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			using (var form = new AdHocServiceJobEntryForm(adhocServiceJob, new NotificationSubscriberGuiHelper()))
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
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdHocServiceJobEntryForm(adhocServiceJob, helper))
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
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var helperMock = new Mock<INotificationSubscriberQueryUser>();

			using (var form = new AdHocServiceJobEntryForm(adhocServiceJob, helperMock.Object))
			{
				GUITestHelper.VerifyQueryUserMethodInNotificationSubscriberGuiHelperIsBeingCalled(form, helperMock);
			}
		}

		#endregion

		#region TestCancel_NonSavedForm

		public void TestCancel_NonSavedForm()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var helper = new NotificationSubscriberGuiHelper();
			using (var form = new AdHocServiceJobEntryForm(adhocServiceJob, helper))
			{
				AssertNoExceptionThrown(() => form.CancelButton.PerformClick());
			}
		}

		#endregion

		#region TestDispose_AdHocServiceJobDeleted

		public void TestDispose_AdHocServiceJobDeleted()
		{
			var data = new TestDataSimpleEnvironment(Factory, 2, 1);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			var helper = new NotificationSubscriberGuiHelper();
			var form = new AdHocServiceJobEntryForm(adhocServiceJob, helper);
			adhocServiceJob.Delete();

			Assert("Precondition: AdHocServiceJob is deleted.", adhocServiceJob.IsDeleted);
			AssertNoExceptionThrown(form.Dispose);
		}

		#endregion

		#region TestErrorOnSavingWhenBranchIsEmpty

		public void TestPerformValidation_EmptyBranchError()
		{
			var jobBranchDefaultOrderRule = new JobBranchDefaultOrderRule()
			{
				DefaultToBlank = 1,
				DefaultToBranchRelatedToPortOrWarehouseBranch = 0,
				DefaultToBranchOfOrganisation = 0,
				DefaultToLoginUserDefault = 0
			};

			var client = Helper.CreateClient();
			var whs = Helper.CreateWarehouse("Warehouse");
			var adHocServiceJob = Factory.New<WhsAdHocServiceJob>();
			adHocServiceJob.WSJ_OH_Client = client.PK;
			adHocServiceJob.WSJ_WW_Whs = whs.PK;
			var continueWithSave = ContinueWithSave.No;
			var helper = new NotificationSubscriberGuiHelper();
			using (AccountingMasterFilesRegistry.Instance.MaxOccurrencesBeforeReportJHBranchIsNull.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 0))
			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobBranchDefaultOrderRule))
			using (var form = new AdHocServiceJobEntryForm(adHocServiceJob, helper))
			{
				form.Show();
				AssertNoExceptionThrown(() => continueWithSave = form.FireSaveButton());
				AssertEquals(ContinueWithSave.No, continueWithSave);

				// : LastMassage.Text is not used here because in this test scenario it will not have access to its children error
				//please refer to ZForm.cs line 1532
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);

				var error = adHocServiceJob.GetErrors();
				Assert(error.Count() == 1);
				AssertEquals("Error - JH_GB: Please enter a Branch.", error.GetFirst().Message);
			}

			jobBranchDefaultOrderRule = new JobBranchDefaultOrderRule()
			{
				DefaultToBlank = 0,
				DefaultToBranchRelatedToPortOrWarehouseBranch = 1,
				DefaultToBranchOfOrganisation = 2,
				DefaultToLoginUserDefault = 3
			};

			using (AccountingConfigurationRegistry.Instance.JobBranchDefaultOrderRule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, jobBranchDefaultOrderRule))
			using (var form = new AdHocServiceJobEntryForm(adHocServiceJob, helper))
			{
				form.Show();
				AssertNoExceptionThrown(() => continueWithSave = form.FireSaveButton());
				AssertEquals(ContinueWithSave.Yes, continueWithSave);
				Assert(adHocServiceJob.GetErrors().Count() == 0);
			}
		}

		#endregion

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var adhocServiceJob = Helper.CreateWhsAdHocServiceJob(data.Whs1, data.Org1, ZDateTime.Today);
			Factory.Save();
			var helper = new NotificationSubscriberGuiHelper();
			return new AdHocServiceJobEntryForm(new BusinessObjectFactory().Load<WhsAdHocServiceJob>(adhocServiceJob.PK), helper) { ControllerID = ControllerIDs.WhsAdHocServiceJob };
		}

		WhsTestHelperFunctions Helper => helper ?? (helper = new WhsTestHelperFunctions(Factory));
		WhsTestHelperFunctions helper;

		#endregion
	}
}
