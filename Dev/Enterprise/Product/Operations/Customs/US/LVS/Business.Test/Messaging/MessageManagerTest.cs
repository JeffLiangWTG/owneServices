using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.US;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Customs.US.LVS.Business.MessageManager;

namespace Enterprise.Customs.US.LVS.Business.Testing
{
	public class MessageManagerTest : TestCaseWithFactory
	{
		public void TestSubmitToCustoms_OnlyUseASCServiceTaskWhenUpdateActionIsAdd()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			clearance.ULH_EntryFilerCode = "ABC";

			var consignment = clearance.CusUSLVConsignments.AddNew();
			consignment.CE_EntryStatus = CRLReleaseStatusList.Codes.REL;
			consignment.InitAction(UpdateActionCode.Update);

			Assert("Precondition : no StmProcessQueue", !(Factory.Load<StmProcessQueue>(new ZQuery(StmProcessQueueSchema.SW_ApplicationCode, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging)).Length > 0));

			var manager = new MessageManager(clearance);
			manager.SubmitToCustoms(clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>(), UpdateActionCode.Update);

			AssertEquals(ImportMessageStatusList.Codes.AwaitingACECargoReleaseUpdate, consignment.ULB_MessageStatus);
			Assert("would not add StmProcessQueue when update action is update", !(Factory.Load<StmProcessQueue>(new ZQuery(StmProcessQueueSchema.SW_ApplicationCode, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging)).Length > 0));

			consignment.InitAction(UpdateActionCode.Add);
			consignment.CE_EntryStatus = string.Empty;
			manager.SubmitToCustoms(clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>(), UpdateActionCode.Add);

			AssertEquals(ImportMessageStatusList.Codes.OriginalRequestPending, consignment.ULB_MessageStatus);
			Assert("would add StmProcessQueue when update action is add", Factory.Load<StmProcessQueue>(new ZQuery(StmProcessQueueSchema.SW_ApplicationCode, CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging)).Length > 0);
		}

		[TestDate(2021, 4, 7)]
		public void TestSubmitToCustomsViaASCServiceTask()
		{
			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();

			clearance.PrepareCusUSLVConsignmentsForUpdateAction(Messaging.Business.UpdateActionCode.Add);
			var consignmentsForMessaging = clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single();
			consignmentsForMessaging.SendToCustoms = true;

			new MessageManager(clearance).SubmitToCustoms(clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>(), Messaging.Business.UpdateActionCode.Add);

			AssertEquals(ImportMessageStatusList.Codes.OriginalRequestPending, consignment.ULB_MessageStatus);

			var stmProcessQueue = Factory.LoadTop1<StmProcessQueue>(new ZQuery(StmProcessQueueSchema.SW_ReferenceID, clearance.PK));
			var orpEvent = consignment.Logs.Find(l => l.SL_SE_NKEvent == "ORP").FirstOrDefault();

			CombineAssertions("new record in StmProcessQueue", () =>
			{
				AssertEquals(CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, stmProcessQueue.SW_ApplicationCode);
				AssertEquals(CustomsStmProcessQueueLoader.Constants.JobTypeCode, stmProcessQueue.SW_JobTypeCode);
				AssertEquals(CusUSLVClearanceSchema.Constants.Prefix, stmProcessQueue.SW_ReferenceTableCode);
				AssertEquals(WorkflowTriggerActionTypeConstants.Codes.SendReleaseMessage, stmProcessQueue.SW_ActionCode);
				AssertEquals(AutoEvents.MessagePendingProcessingCode, stmProcessQueue.SW_EventCode);
				AssertEquals(CustomsStmProcessQueueLoader.Constants.AutoSendCustomsMessaging, stmProcessQueue.SW_ApplicationCode);
				AssertEquals(new ZDateTime("07-4-21 00:00:00"), stmProcessQueue.SW_PostedTimeUtc);
				AssertNotNull(orpEvent);
				AssertEquals(consignment.ULB_HouseBill, orpEvent.SL_Reference);
				AssertEquals("Original Message Request Pending", orpEvent.SL_EventDescription);
				AssertEquals(new ZDateTime("07-4-21 00:00:00"), orpEvent.SL_EventTime);
			});
		}

		public void TestStmEventORPIsNotCustomizable()
		{
			var eventORP = Factory.LoadTop1<StmEvent>(new ZQuery(StmEventSchema.SE_Code, "ORP"));
			AssertEquals(false, eventORP.SE_IsCustomizable);
		}

		public void TestSubmitConsignmentToCustoms_UpdateMessageCreateUserWhenUpdateActionIsAdd()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_IsController = false;
			staff.GS_Code = "AA";
			staff.GS_EmailAddress = "blah@com.au";
			staff.GS_IsSystemAccount = true;

			var clearance = Factory.NewWithValidTestData<CusUSLVClearance>();
			var consignment = clearance.CusUSLVConsignments.AddNew();
			clearance.PrepareCusUSLVConsignmentsForUpdateAction(Messaging.Business.UpdateActionCode.Add);
			var messageNumberStrategy = new BulkAllocateMessageNumberStrategy(Factory, 1);
			var consignmentsForMessaging = clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single();
			consignmentsForMessaging.SendToCustoms = true;
			Factory.Save();

			using (Env.SetTemporaryUserContext(new UserContext(staff.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				new MessageManager(clearance).SubmitToCustoms(clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>(), Messaging.Business.UpdateActionCode.Add);
			}
			SubmitConsignmentToCustoms(clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single(), UpdateActionCode.Add, messageNumberStrategy);
			var message = consignment.Messages.Single() as MQEDIMessage;

			AssertNotNull(message);
			AssertEquals(staff.GS_Code, message.EM_SystemCreateUser);
			AssertEquals(false, message.EM_SendWithMessageErrors);

			new MessageManager(clearance).SubmitToCustoms(clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>(), Messaging.Business.UpdateActionCode.Update);
			SubmitConsignmentToCustoms(clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>().Single(), UpdateActionCode.Update, messageNumberStrategy, true);
			message = consignment.Messages.Last() as MQEDIMessage;

			AssertNotNull(message);
			AssertNullOrEmpty(message.EM_SystemCreateUser);
			AssertEquals(true, message.EM_SendWithMessageErrors);
		}

		public void TestSubmitToCustoms_WithConsignments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;

			clearance.PrepareCusUSLVConsignmentsForUpdateAction(Messaging.Business.UpdateActionCode.Replace);
			var consignments = new List<CusUSLVConsignment> { consignment1, consignment2, consignment3 };

			var consignmentsForMessaging = consignments.Select(consignment => new CusUSLVConsignmentForMessaging(consignment));
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment1.PK).SendToCustoms = true;
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment2.PK).SendToCustoms = false;
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment3.PK).SendToCustoms = false;
			new MessageManager(clearance).SubmitToCustoms(consignmentsForMessaging, Messaging.Business.UpdateActionCode.Replace);

			AssertEquals(1, consignment1.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, consignment1.Messages[0].EM_MessageSubType);
			AssertEquals(1, consignment2.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, consignment2.Messages[0].EM_MessageSubType);
			AssertEquals(1, consignment3.Messages.Count);
			AssertEquals(EM_MessageSubTypeList.Codes.ACECargoReleaseReplace, consignment3.Messages[0].EM_MessageSubType);
		}

		public void TestBulkLoadConsignmentMessageNumber_WithClearance()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;

			clearance.PrepareCusUSLVConsignmentsForUpdateAction(Messaging.Business.UpdateActionCode.Replace);
			var consignmentsForMessaging = clearance.CusUSLVConsignmentsToSend.OfType<CusUSLVConsignmentForMessaging>();
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment1.PK).SendToCustoms = true;
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment2.PK).SendToCustoms = true;
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment3.PK).SendToCustoms = true;

			Factory.Save();

			var count = NumberFountainProxy.NumberOfFountainCommands_ForTest.Value;
			AssertEquals("Count should be one as number is allocated for clearance", 1, count);

			var messageNumberStrategy = new BulkAllocateMessageNumberStrategy(Factory, consignmentsForMessaging.Count());
			foreach (CusUSLVConsignmentForMessaging consignmentForMessaging in consignmentsForMessaging)
			{
				SubmitConsignmentToCustoms(consignmentForMessaging, Messaging.Business.UpdateActionCode.Replace, messageNumberStrategy);
			}

			Factory.Save();

			count = NumberFountainProxy.NumberOfFountainCommands_ForTest.Value;
			AssertEquals("Bulk Allocation number should be one greater", 2, count);

			AssertConsignmentMessageNumbersExistAndAreUnique("all consignments have unique and existent messages", consignmentsForMessaging);
		}

		public void TestBulkLoadConsignmentMessageNumber_WithConsignments()
		{
			var clearance = Factory.New<CusUSLVClearance>();
			var consignment1 = clearance.CusUSLVConsignments.AddNew();
			consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment2 = clearance.CusUSLVConsignments.AddNew();
			consignment2.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			var consignment3 = clearance.CusUSLVConsignments.AddNew();
			consignment3.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;

			clearance.PrepareCusUSLVConsignmentsForUpdateAction(Messaging.Business.UpdateActionCode.Replace);
			var consignments = new List<CusUSLVConsignment> { consignment1, consignment2, consignment3 };

			var consignmentsForMessaging = consignments.Select(consignment => new CusUSLVConsignmentForMessaging(consignment));
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment1.PK).SendToCustoms = true;
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment2.PK).SendToCustoms = false;
			consignmentsForMessaging.Single(c => c.Consignment.PK == consignment3.PK).SendToCustoms = false;

			Factory.Save();

			var count = NumberFountainProxy.NumberOfFountainCommands_ForTest.Value;
			AssertEquals("Count should be one as number is allocated for clearance", 1, count);

			var messageNumberStrategy = new BulkAllocateMessageNumberStrategy(Factory, consignmentsForMessaging.Count());
			foreach (CusUSLVConsignmentForMessaging consignmentForMessaging in consignmentsForMessaging)
			{
				SubmitConsignmentToCustoms(consignmentForMessaging, Messaging.Business.UpdateActionCode.Replace, messageNumberStrategy);
			}

			Factory.Save();

			count = NumberFountainProxy.NumberOfFountainCommands_ForTest.Value;
			AssertEquals("Bulk Allocation number should be one greater", 2, count);

			AssertConsignmentMessageNumbersExistAndAreUnique("all consignments have unique and existent messages", consignmentsForMessaging);
		}

		public void TestSubmitConsignmentToCustoms_ConsignmentWillNotLock_WhenClearanceIsLocked()
		{
			using (CargoWise.Data.Db.Connection.TrackExecutedCommands())
			{
				var clearance = Factory.New<CusUSLVClearance>();
				var consignment1 = clearance.CusUSLVConsignments.AddNew();
				consignment1.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
				clearance.PrepareCusUSLVConsignmentsForUpdateAction(Messaging.Business.UpdateActionCode.Replace);
				var consignmentForMessaging = new CusUSLVConsignmentForMessaging(consignment1);

				try
				{
					if (clearance.LockSendCustomsMessageMutex())
					{
						var executedCommands = CargoWise.Data.Db.Connection.ExecutedCommands;
						var lockCommands = executedCommands.Where(commandString => commandString.StartsWith("InsertSemaphoreHandle"));

						CombineAssertions(() =>
						{
							AssertEquals("One lock command created by clearance locking", 1, lockCommands.Count());

							Assert(MessageManager.SubmitConsignmentToCustoms(consignmentForMessaging, Messaging.Business.UpdateActionCode.Replace, new BulkAllocateMessageNumberStrategy(Factory, 1)));
							executedCommands = CargoWise.Data.Db.Connection.ExecutedCommands;

							lockCommands = executedCommands.Where(commandString => commandString.StartsWith("InsertSemaphoreHandle"));
							AssertEquals("One lock command created by clearance locking", 1, lockCommands.Count());
						});
					}
				}
				finally
				{
					clearance.UnlockSendCustomsMessageMutex();
				}
			}
		}

		void AssertConsignmentMessageNumbersExistAndAreUnique(string message, IEnumerable<CusUSLVConsignmentForMessaging> consignmentsForMessaging)
		{
			CombineAssertions(message, () =>
			{
				Assert("Consignment.Messages should not be null", consignmentsForMessaging.All(c => c.Consignment.Messages != null));
				Assert("Consignment has no messages", consignmentsForMessaging.All(c => c.Consignment.Messages.Count > 0));
				var messageNumbers = consignmentsForMessaging.SelectMany(c => c.Consignment.Messages.Cast<EDIMessage>().Select(m => m.EM_MessageNum)).Distinct();
				Assert("Consignment has no message number", messageNumbers.All(number => !number.IsEmpty));
				AssertEquals("Message number is not unique", consignmentsForMessaging.Count(), messageNumbers.Count());
			});
		}
	}
}
