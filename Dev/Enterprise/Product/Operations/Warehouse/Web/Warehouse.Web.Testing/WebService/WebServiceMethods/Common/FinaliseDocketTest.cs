using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.Data;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Warehouse;
using Enterprise.Warehouse.Environment.Business.Testing;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Warehouse.Web.WebService.Testing
{
	class FinaliseDocketTest : WhsSecureServiceTestCase
	{
		#region Receives

		#region TestFinaliseDocket

		public void TestFinaliseDocket()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive1 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			var receive2 = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R2", Notify);
			Helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			Helper.CreateWhsReceiveInventoryLine(receive2, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();

			Helper.Factory.Save();

			AssertEquals("Precondition - ensure Receive is not finalised", false, receive1.IsFinalised);
			AssertEquals("Precondition - ensure Receive is not finalised", false, receive2.IsFinalised);

			var webService1 = GetNewWebService(data.Whs1);
			webService1.FinaliseDocket(receive1.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals("Receive should be finalised. No errors expected.", true, receive1.IsFinalised);

			var webService2 = GetNewWebService(data.Whs1);
			webService2.FinaliseDocket(receive2.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals("Receive shouldn't be finalised since it's inventory wasn't allocated.", false, receive2.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_WrongDocketType

		public void TestFinaliseDocket_WrongDocketType()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory);

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // Error in not having sufficient receives for cross dock orders
			var order = Helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0], 10m);

			Helper.Factory.Save();

			AssertEquals("Precondition - ensure Order is not finalised", false, order.IsFinalised);

			var webService = GetNewWebService(data.Whs1);
			AssertBusinessValidationError(webService, "Exception should add log if docket type is not supported.",
				"Docket type '999' is not supported.",
				webService.FinaliseDocket(receive.PK.ToGuid(), (RFDocketType)999));
		}

		#endregion

		#region TestFinaliseDocket_WrongWarehouse

		public void TestFinaliseDocket_WrongWarehouse()
		{
			var data = new TestDataForInventory(Helper.Factory);
			data.CreateSimpleInventory(false); // making sure that there is receive that could be finalised.
			var otherWarehouse = Helper.CreateWarehouse("WHS2");
			Helper.Factory.Save();

			var webService = GetNewWebService(otherWarehouse);
			AssertBusinessValidationError(webService,
				"Can't find un-finalized Receive.",
				webService.FinaliseDocket(data.Receive11.PK.ToGuid(), RFDocketType.WhsReceive));
		}

		#endregion

		#region TestFinaliseDocket_FinaliseAlreadyFinalisedDocket

		public void TestFinaliseDocket_FinaliseAlreadyFinalisedDocket()
		{
			var data = new TestDataForInventory(Helper.Factory);
			data.CreateSimpleInventory();
			AssertIsFinalisedPrecondition(data.Receive11);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1);
			AssertBusinessValidationError(webService,
				"Can't find un-finalized Receive.",
				webService.FinaliseDocket(data.Receive11.PK.ToGuid(), RFDocketType.WhsReceive));
		}

		#endregion

		#region TestFinaliseDocket_SendFinaliseFailedEmail

		#region TestFinaliseDocket_SendFinaliseFailedEmail_NoRecipients

		public void TestFinaliseDocket_SendFinaliseFailedEmail_NoRecipients()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");

			var data = new TestDataSimpleEnvironment(webService.Factory);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // Error in having no arrival date
			receive1.WD_ArrivalDate = ZDateTimeOffset.Empty;

			var emptyGroup = helper.Factory.NewWithValidTestData<GlbGroup>();

			helper.Factory.Save();

			AssertEquals("Precondition - ensure that no other emails were sent previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, Guid.Empty);
			AssertEquals(false, receive1.IsFinalised);
			AssertEquals(ZGuid.Empty, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);
			AssertEquals("No new emails should be created since there is no one to send email to.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, emptyGroup.PK.ToGuid());
			webService.FinaliseDocket(receive1.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals(false, receive1.IsFinalised);
			AssertEquals("No new emails should be created since there is no one to send email to.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion

		#region TestFinaliseDocket_SendFinaliseFailedEmail_SingleRecipient

		public void TestFinaliseDocket_SendFinaliseFailedEmail_SingleRecipient()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");

			var data = new TestDataSimpleEnvironment(webService.Factory);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // Error in having no arrival date
			receive1.WD_ArrivalDate = ZDateTimeOffset.Empty;

			var bossGroup = helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");

			helper.Factory.Save();

			AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bossGroup.PK.ToGuid());
			webService.FinaliseDocket(receive1.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals(false, receive1.IsFinalised);
			AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail11 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(ZString.Format("Receive Job {0} RF finalization Failure", receive1.WD_DocketID), errorEmail11.Subject);
			AssertEquals(1, errorEmail11.Recipients.Count);
			AssertEquals(true, ContainEmail(errorEmail11.Recipients, "BOSS@AAAA.AA"));
			AssertEquals(GetCorrectRFFinaliseEmailErrorMessage(receive1), errorEmail11.Body);
			Assert(!errorEmail11.Body.Contains("\r\nIf there are no errors, then there might be active tasks set to working for the Transfer Job."));
		}

		#endregion

		#region TestFinaliseDocket_SendFinaliseFailedEmail_SeveralRecipients

		public void TestFinaliseDocket_SendFinaliseFailedEmail_SeveralRecipients()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);

			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");

			var data = new TestDataSimpleEnvironment(webService.Factory);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // Error in having no arrival date
			receive1.WD_ArrivalDate = ZDateTimeOffset.Empty;

			var postMastersGroup = helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(helper, postMastersGroup, "GS2", "AAA", "AAA@AAAA.AA");
			CreateStaff(helper, postMastersGroup, "GS3", "BBB", "BBB@AAAA.AA");
			CreateStaff(helper, postMastersGroup, "GS4", "CCC", "");

			helper.Factory.Save();

			AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, postMastersGroup.PK.ToGuid());
			webService.FinaliseDocket(receive1.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals(false, receive1.IsFinalised);
			AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail12 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(ZString.Format("Receive Job {0} RF finalization Failure", receive1.WD_DocketID), errorEmail12.Subject);
			AssertEquals(2, errorEmail12.Recipients.Count);
			AssertEquals(true, ContainEmail(errorEmail12.Recipients, "AAA@AAAA.AA"));
			AssertEquals(true, ContainEmail(errorEmail12.Recipients, "BBB@AAAA.AA"));
			AssertEquals(GetCorrectRFFinaliseEmailErrorMessage(receive1), errorEmail12.Body);
		}

		#endregion

		#region TestFinaliseDocket_SendFinaliseFailedEmail_NoLines

		public void TestFinaliseDocket_SendFinaliseFailedEmail_NoLines()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // Error in having no Lines		
			var bossGroup = helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");

			helper.Factory.Save();

			AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bossGroup.PK.ToGuid());
			webService.FinaliseDocket(receive1.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals(false, receive1.IsFinalised);
			AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail2 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(ZString.Format("Receive Job {0} RF finalization Failure", receive1.WD_DocketID), errorEmail2.Subject);
			AssertEquals(GetCorrectRFFinaliseEmailErrorMessage(receive1), errorEmail2.Body);
		}

		#endregion

		#region TestFinaliseDocket_SendFinaliseFailedEmail_UnallocatedLocations

		public void TestFinaliseDocket_SendFinaliseFailedEmail_UnallocatedLocations()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // Error in having not allocated all lines
			helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			var bossGroup = helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");

			helper.Factory.Save();

			AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			webService.FinaliseDocket(receive1.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals(false, receive1.IsFinalised);
			AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail3 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(ZString.Format("Receive Job {0} RF finalization Failure", receive1.WD_DocketID), errorEmail3.Subject);
			AssertEquals(GetCorrectRFFinaliseEmailErrorMessage(receive1), errorEmail3.Body);
		}

		#endregion

		#region TestFinaliseDocket_SendFinaliseFailedEmail_MultipleUsersAtOnce

		public void TestFinaliseDocket_SendFinaliseFailedEmail_MultipleUsersAtOnce()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			webService.Factory.RefreshEnabled = false;

			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");

			var data = new TestDataSimpleEnvironment(webService.Factory);
			var receive1 = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // No errors, will have line added during finalise docket
			helper.CreateWhsReceiveInventoryLine(receive1, data.Part1, 10m);
			receive1.AllocateLocationsWithMock();

			var bossGroup = helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");

			helper.Factory.Save();

			AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;

			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			var localFactory = new BusinessObjectFactory();
			localFactory.RefreshEnabled = false;

			var loadedReceive = localFactory.Load<WhsReceive>(receive1.PK);
			loadedReceive.WD_BookingDate = DateTime.Now;
			localFactory.Save();

			receive1.WD_BookingDate = DateTime.Now;

			webService.FinaliseDocket(receive1.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertEquals("Receive should pass finalise validation", true, receive1.IsFinalised);
			AssertEquals("New email should be created since error occured during factory save.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

			var errorEmail4 = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(ZString.Format("Receive Job {0} RF finalization Failure", receive1.WD_DocketID), errorEmail4.Subject);
			AssertContains(GetCorrectRFFinaliseEmailErrorMessageConcurrent(receive1), errorEmail4.Body);

			var receive4InDb = new BusinessObjectFactory().Load<WhsReceive>(receive1.PK);
			AssertEquals("Should not have been saved as finalised", false, receive4InDb.IsFinalised);
		}

		#endregion

		#region TestFinaliseDocket_SendFinaliseFailedEmail_CrossDockShortage

		public void TestFinaliseDocket_SendFinaliseFailedEmail_CrossDockShortage()
		{
			var webService = GetNewWebService();
			var helper = new WhsTestHelperFunctions(webService.Factory);
			var data = new TestDataSimpleEnvironment(webService.Factory);
			var bossGroup = helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");

			var receive = helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify); // Error in not having sufficient receives for cross dock orders
			var receiveLine = helper.CreateWhsReceiveLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			helper.Factory.Save();

			var order1 = helper.CreateWhsOrderWithOrderLine(data.Org1, data.Whs1, data.Part1, 10m);
			order1.Lines[0].ReserveStockIfAbleTo(receive.Inventory[0], 10m);
			receiveLine.WE_TransactionQuantity = 5m;

			AssertEquals("Precondition: There should be 10 stock reserved", 10m, order1.Lines[0].ReservedQuantity);
			AssertEquals("Precondition: There should be 10 stock expected", 10m, receiveLine.WE_ClientOrderedUnits);
			AssertEquals("Precondition: There should be 5 stock received", 5m, receiveLine.WE_TransactionQuantity);
			AssertEquals("Precondition: Ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			webService.SecurityHeader.WarehouseCode = data.Whs1.WW_WarehouseCode;
			WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, bossGroup.PK.ToGuid());

			webService.FinaliseDocket(receive.PK.ToGuid(), RFDocketType.WhsReceive);
			var errorEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("Receive with unfulfilled Cross Dock orders cannot be finalised.", false, receive.IsFinalised);
			AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Email subject should be correct.", ZString.Format("Receive Job {0} RF finalization Failure", receive.WD_DocketID), errorEmail.Subject);
			AssertEquals("Cross Dock Shortage Error Message should be included in email.", true, GetCorrectRFFinaliseEmailErrorMessage(receive).Contains("This Receive Line has 10 units reserved, you cannot finalize while the received quantity is less than this."));
			AssertEquals("Cross Dock shortage error message should be included in email.", GetCorrectRFFinaliseEmailErrorMessage(receive), errorEmail.Body);
		}

		#endregion

		#endregion

		public void TestFinaliseDocket_MatchedDPS()
		{
			try
			{
				Globals.IsUserInteractive = false;

				Env.Security.DpsAllowUpdateToMatched.IsAllowed = false;
				using (OrganisationsDataRegistry.Instance.DPSFreightMovementRestrictions.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, "ALL"))
				{
					var data = new TestDataSimpleEnvironment(Helper.Factory, 3, 1);
					var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");
					var bossGroup = Helper.Factory.NewWithValidTestData<GlbGroup>();
					CreateStaff(Helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");
					var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1");
					Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 50);
					receive.AllocateLocationsWithMock();
					receive.WD_ScreeningStatus = ScreeningStatusesList.Codes.Matched;
					Helper.Factory.Save();

					AssertEquals("Precondition", false, Globals.CanShowDialogs);
					AssertEquals("Precondition - ensure Receive is not finalised", false, receive.IsFinalised);
					AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
					AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);
					var webService = GetNewWebService(data.Whs1);
					webService.FinaliseDocket(receive.PK.ToGuid(), RFDocketType.WhsReceive);
					AssertEquals(false, receive.IsFinalised);
					AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);

					var errorEmail = Env.OutgoingMailManager.EmailsCreated[0];
					AssertEquals(ZString.Format("Receive Job {0} RF finalization Failure", receive.WD_DocketID), errorEmail.Subject);
					AssertEquals(true, errorEmail.Body.Contains("Cannot finalize when Denied Party Screening is matched."));
				}
			}
			finally
			{
				Globals.IsWeb = false;
				Globals.IsUserInteractive = true;
			}
		}

		public void TestFinaliseDocket_PlannedReceiveWithTasks_WorkingTask()
			=> TestFinaliseDocket_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestFinaliseDocket_NotPlannedReceiveWithTasks_WorkingTask()
			=> TestFinaliseDocket_WithTaskManagement(isPlannedReceive: false, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestFinaliseDocket_PlannedReceiveWithTasks_NotWorkingTask()
			=> TestFinaliseDocket_WithTaskManagement(isPlannedReceive: true, isWorkingTask: false, isUnloadTask: true, isTaskForOtherUser: true);

		public void TestFinaliseDocket_PlannedReceiveWithTasks_WorkingTask_NotUnloadingTask()
			=> TestFinaliseDocket_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: false, isTaskForOtherUser: true);

		public void TestFinaliseDocket_PlannedReceiveWithTasks_WorkingTask_TaskForCurrentUser()
			=> TestFinaliseDocket_WithTaskManagement(isPlannedReceive: true, isWorkingTask: true, isUnloadTask: true, isTaskForOtherUser: false);

		void TestFinaliseDocket_WithTaskManagement(bool isPlannedReceive, bool isWorkingTask, bool isUnloadTask, bool isTaskForOtherUser)
		{
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var otherStaff = Helper.CreateGlbStaff("S3", "S3");
			Helper.Factory.Save();

			var data = new TestDataSimpleEnvironment(Helper.Factory);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;

			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");
			var bossGroup = Helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(Helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceive(data.Org1, data.Whs1, "R1", Notify);
			if (isPlannedReceive)
			{
				receive.WD_TaskPlanningStatus = TaskPlanningStatus.Codes.Planned;
			}
			Helper.CreateWhsReceiveInventoryLine(receive, data.Part1, 10m);
			receive.AllocateLocationsWithMock();
			Helper.Factory.Save();

			var task = Helper.CreateProcessTaskForReceive(receive, isTaskForOtherUser ? otherStaff : (GlbStaff)Env.CurrentUser);
			if (isWorkingTask)
			{
				task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;
			}
			if (!isUnloadTask)
			{
				task.P9_FormFlowType = string.Empty;
			}
			Helper.Factory.Save();

			AssertEquals("Precondition", isPlannedReceive, receive.WD_TaskPlanningStatus.EqualsIgnoringCase(TaskPlanningStatus.Codes.Planned));
			AssertEquals("Precondition", isUnloadTask, task.P9_FormFlowType.EqualsIgnoringCase(WarehouseTaskFormFlowTypes.UnloadJob));
			AssertEquals("Precondition", isWorkingTask, task.P9_Status.EqualsIgnoringCase(ProcessTaskStatusCodeList.Codes.Working));
			AssertEquals("Precondition - ensure Receive is not finalised", false, receive.IsFinalised);
			AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			var webService = GetNewWebService(data.Whs1);
			var response = webService.FinaliseDocket(receive.PK.ToGuid(), RFDocketType.WhsReceive);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			var receiveIsNotFinalised = isPlannedReceive && isUnloadTask && isWorkingTask && isTaskForOtherUser;
			AssertEquals(!receiveIsNotFinalised, receive.IsFinalised);

			if (receiveIsNotFinalised)
			{
				AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
				var errorEmail = Env.OutgoingMailManager.EmailsCreated[0];
				AssertEquals(ZString.Format("Receive Job {0} RF finalization Failure", receive.WD_DocketID), errorEmail.Subject);
				Assert(errorEmail.Body.Contains("\r\nIf there are no errors, then there might be active tasks set to working assigned to other users for the Receive."));
			}

			var newFactory = new BusinessObjectFactory();
			var receiveInNewFactory = newFactory.Load<WhsReceive>(receive.PK);
			AssertEquals(!receiveIsNotFinalised, receiveInNewFactory.IsFinalised);
		}

		#endregion

		#region Transfers

		public void TestTestFinaliseDocket_Transfer()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);

			AssertEquals("Precondition - Transfer is not finalised", false, transfer.IsFinalised);
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.FinaliseDocket(transfer.PK.ToGuid(), RFDocketType.WhsTransfer);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals("No new email should be created.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var newFactory = new BusinessObjectFactory();
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals(true, transferInNewFactory.IsFinalised);
		}

		public void TestTestFinaliseDocket_Transfer_NotAllLinesAreFinalised()
			=> TestTestFinaliseDocket_Transfer_NotAllLinesAreFinalisedCore(unfinalisedLineHasAssignedDestLocation: false);

		public void TestTestFinaliseDocket_Transfer_NotAllLinesAreFinalised_UnfinalisedLineHasDestLocation()
			=> TestTestFinaliseDocket_Transfer_NotAllLinesAreFinalisedCore(unfinalisedLineHasAssignedDestLocation: true);

		void TestTestFinaliseDocket_Transfer_NotAllLinesAreFinalisedCore(bool unfinalisedLineHasAssignedDestLocation)
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", unfinalisedLineHasAssignedDestLocation ? "A-2" : "");

			AssertEquals("Precondition - Transfer line is not finalised", false, transferLine2.IsFinalised);
			AssertEquals("Precondition - Transfer is not finalised", false, transfer.IsFinalised);
			transfer.RunPreSaveValidation();
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.FinaliseDocket(transfer.PK.ToGuid(), RFDocketType.WhsTransfer);
			AssertSuccessfulResponseWithNoErrors(response, webService);
			AssertEquals("No new email should be created - it should not have run finalise.", 0, Env.OutgoingMailManager.EmailsCreated.Count);

			var newFactory = new BusinessObjectFactory();
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals(false, transferInNewFactory.IsFinalised);
		}

		public void TestTestFinaliseDocket_Transfer_ErrorOnSave()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var releaseGroup = Helper.CreateReleaseGroup("RG1", "RG1");
			data.Whs1.WW_GG_ReleaseGroup = releaseGroup.PK;
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");
			var bossGroup = Helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(Helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");
			Helper.Factory.Save();

			var receive = Helper.CreateWhsReceiveWithInventory(data.Org1, data.Whs1, "R1", data.Part1, 50m, data.Whs1.FindLocation("A-1"), "");
			Helper.Factory.Save();

			var transfer = Helper.CreateWhsTransfer(data.Org1, data.Whs1, "TR1", Notify);
			var transferLine1 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine1.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine1);

			var transferLine2 = Helper.CreateWhsTransferLine(transfer, data.Part1, 10m, "A-1", "A-2");
			transferLine2.FinaliseDocketLine();
			AssertIsFinalisedPrecondition(transferLine2);

			AssertEquals("Precondition - Transfer is not finalised", false, transfer.IsFinalised);
			Helper.Factory.Save();

			AssertEquals("Precondition - ensure that no other emails were send previously.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
			AssertEquals("Precondition - default group should be ALL", allGroupPK, WarehouseDataRegistry.Instance.WarehouseRFJobFinalizationFailureNotificationGroup.Value);

			var webService = GetNewWebService(data.Whs1, staff);
			var innerException = new Exception();
			var concurrencyException = new ZDataConcurrencyException(innerException, ((IBusinessObjectInternals)transfer).Row, Db.Connection);
			webService.Factory.Saving += f => throw new ZSaveConcurrencyException(concurrencyException, Helper.Factory);

			var response = webService.FinaliseDocket(transfer.PK.ToGuid(), RFDocketType.WhsTransfer);
			AssertSuccessfulResponseWithNoErrors(response, webService);

			AssertEquals("New email should be created since error occured during finalise.", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var errorEmail = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals(ZString.Format("Transfer Job {0} RF finalization Failure", transfer.WD_DocketID), errorEmail.Subject);

			var newFactory = new BusinessObjectFactory();
			var transferInNewFactory = newFactory.Load<WhsTransfer>(transfer.PK);
			AssertEquals(false, transferInNewFactory.IsFinalised);
		}

		public void TestTestFinaliseDocket_Transfer_TransferLoadReturnsNull()
		{
			var data = new TestDataSimpleEnvironment(Helper.Factory, 2, 1);
			var staff = Helper.CreateGlbStaff("S2", "S2");
			var allGroupPK = RegistryFactory.Instance.GetGroupPK("ALL");
			var bossGroup = Helper.Factory.NewWithValidTestData<GlbGroup>();
			CreateStaff(Helper, bossGroup, "GS1", "BOSS", "BOSS@AAAA.AA");
			Helper.Factory.Save();

			var webService = GetNewWebService(data.Whs1, staff);
			var response = webService.FinaliseDocket(Guid.Empty, RFDocketType.WhsTransfer);
			AssertBusinessValidationError(webService, "Can't find un-finalized Transfer.", response);
			AssertEquals("No new email should be created as finalise is not invoked.", 0, Env.OutgoingMailManager.EmailsCreated.Count);
		}

		#endregion

		#region Helpers

		static void CreateStaff(WhsTestHelperFunctionsEnv helper, GlbGroup group, ZString code, ZString name, ZString email)
		{
			var staff = helper.CreateGlbStaff(code, name);
			staff.GS_EmailAddress = email;
			group.Staff.Add(staff);
		}

		static bool ContainEmail(RecipientDefReadonlyCollection recipients, ZString emailToFind)
		{
			return recipients.Cast<RecipientDef>().Any(x => x.Email == emailToFind);
		}

		static ZString GetCorrectRFFinaliseEmailErrorMessage(AutoWhsDocket receive, string errorMessage = "")
		{
			return ZString.Format("Job ID: {0} \r\n", receive.WD_DocketID) +
				ZString.Format("Reference: {0}\r\n\r\n", receive.WD_ExternalReference) +
				ZString.Format("Receive Job {0} (entered via RF) could not be finalized due to following errors:\r\n\r\n", receive.WD_DocketID) +
				receive.NotificationsIncludingChildren.ToUniqueMessageListString() + errorMessage +
				//automaticaly added part.
				"\r\n" +
				"\r\n" +
				"You have received this email because you are a member of the staff group defined at System Registry: Warehouse -> Scanning -> Warehouse RF Job Finalization Failure Notification Group.";
		}

		static ZString GetCorrectRFFinaliseEmailErrorMessageConcurrent(AutoWhsDocket receive)
		{
			return ZString.Format("Job ID: {0} \r\n", receive.WD_DocketID) +
				ZString.Format("Reference: {0}\r\n\r\n", receive.WD_ExternalReference) +
				ZString.Format("Receive Job {0} (entered via RF) could not be finalized due to following errors:\r\n\r\n", receive.WD_DocketID) +
				receive.NotificationsIncludingChildren.ToUniqueMessageListString() + "\r\n**CONCURRENCY Error Saving Record **";
		}

		#endregion
	}
}
