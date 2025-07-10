using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessQueue))]
	sealed class ProcessQueueTest : EnterpriseBusinessObjectTestCase
	{
		#region Business Object Overrides

		public void TestOnSaving_CustomsQueueLogs()
		{
			AssertEquals("Pre-condition", 0, Queue.CustomsQueueLogs.Count);

			Queue.P4_ParentTableCode = "OO";
			Factory.Save();
			AssertEquals("Queue is not changed, should not create log", 0, Queue.CustomsQueueLogs.Count);

			Queue.P4_CustomsQueue = "123";
			Factory.Save();
			AssertEquals(1, Queue.CustomsQueueLogs.Count);
			string expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "123", "", "", "", "");
			AssertEquals(expectedReference, Queue.CustomsQueueLogs[0].SL_Reference);

			Queue.P4_CustomsStatus = "AA";
			Factory.Save();
			AssertEquals(2, Queue.CustomsQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "123", "AA", "", "", "");
			AssertEquals(expectedReference, Queue.CustomsQueueLogs[1].SL_Reference);

			Queue.P4_CustomsSubStatus = "TT";
			Factory.Save();
			AssertEquals(3, Queue.CustomsQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "123", "AA", "TT", "", "");
			AssertEquals(expectedReference, Queue.CustomsQueueLogs[2].SL_Reference);

			Queue.P4_CustomsReason = "Reason";
			Factory.Save();
			AssertEquals(4, Queue.CustomsQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "123", "AA", "TT", "Reason", "");
			AssertEquals(expectedReference, Queue.CustomsQueueLogs[3].SL_Reference);

			Queue.P4_GS_NKCustomsTaskAssignedTo = "AAA";
			Factory.Save();
			AssertEquals(5, Queue.CustomsQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "123", "AA", "TT", "Reason", "AAA");
			AssertEquals(expectedReference, Queue.CustomsQueueLogs[4].SL_Reference);

			Queue.P4_CustomAttrib1 = "XXX";
			Factory.Save();
			AssertEquals("Should not add log history. Queue details were not changed", 5, Queue.CustomsQueueLogs.Count);

			Queue.P4_CustomsSubStatus = "T2";
			Factory.Save();
			AssertEquals(6, Queue.CustomsQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "123", "AA", "T2", "Reason", "AAA");
			AssertEquals(expectedReference, Queue.CustomsQueueLogs[5].SL_Reference);
		}

		public void TestOnSaving_CommercialQueueLogs()
		{
			AssertEquals("Pre-condition", 0, Queue.CommercialQueueLogs.Count);

			Queue.P4_ParentTableCode = "OO";
			Factory.Save();
			AssertEquals("Queue is not changed, should not create log", 0, Queue.CommercialQueueLogs.Count);

			Queue.P4_QueueName = "123";
			Factory.Save();
			AssertEquals(1, Queue.CommercialQueueLogs.Count);
			string expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "123", "", "", "", "");
			AssertEquals(expectedReference, Queue.CommercialQueueLogs[0].SL_Reference);

			Queue.P4_Status = "AA";
			Factory.Save();
			AssertEquals(2, Queue.CommercialQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "123", "AA", "", "", "");
			AssertEquals(expectedReference, Queue.CommercialQueueLogs[1].SL_Reference);

			Queue.P4_SubStatus = "TT";
			Factory.Save();
			AssertEquals(3, Queue.CommercialQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "123", "AA", "TT", "", "");
			AssertEquals(expectedReference, Queue.CommercialQueueLogs[2].SL_Reference);

			Queue.P4_Reason = "Reason";
			Factory.Save();
			AssertEquals(4, Queue.CommercialQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "123", "AA", "TT", "Reason", "");
			AssertEquals(expectedReference, Queue.CommercialQueueLogs[3].SL_Reference);

			Queue.P4_GS_NKTaskAssignedTo = "AAA";
			Factory.Save();
			AssertEquals(5, Queue.CommercialQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "123", "AA", "TT", "Reason", "AAA");
			AssertEquals(expectedReference, Queue.CommercialQueueLogs[4].SL_Reference);

			Queue.P4_CustomAttrib2 = "XXX";
			Factory.Save();
			AssertEquals("Should not add log history. Queue details were not changed", 5, Queue.CommercialQueueLogs.Count);

			Queue.P4_Reason = "SSS";
			Factory.Save();
			AssertEquals(6, Queue.CommercialQueueLogs.Count);
			expectedReference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "123", "AA", "TT", "SSS", "AAA");
			AssertEquals(expectedReference, Queue.CommercialQueueLogs[5].SL_Reference);
		}

		public void TestOnSaving_BothQueueChanged()
		{
			Factory.Save();
			AssertEquals("Pre-condition", 0, Queue.CommercialQueueLogs.Count);
			AssertEquals("Pre-condition", 0, Queue.CustomsQueueLogs.Count);

			Queue.P4_QueueName = "Q01";
			Queue.P4_CustomsQueue = "Q02";
			Factory.Save();
			AssertEquals(1, Queue.CommercialQueueLogs.Count);
			AssertEquals(1, Queue.CustomsQueueLogs.Count);
		}

		#endregion

		#region Property Overrides

		public void TestP4_CustomsQueue()
		{
			Queue.P4_CustomsQueue = "001";
			Queue.P4_CustomsStatus = "S01";
			Queue.P4_CustomsSubStatus = "TTT";
			Queue.P4_CustomsReason = "Reason1";
			Queue.P4_GS_NKCustomsTaskAssignedTo = ">o<";

			AssertEquals("Pre-condition", "001", Queue.P4_CustomsQueue);
			AssertEquals("Pre-condition", "S01", Queue.P4_CustomsStatus);
			AssertEquals("Pre-condition", "TTT", Queue.P4_CustomsSubStatus);
			AssertEquals("Pre-condition", "Reason1", Queue.P4_CustomsReason);
			AssertEquals("Pre-condition", ">o<", Queue.P4_GS_NKCustomsTaskAssignedTo);
			Factory.Save();

			Queue.P4_CustomsQueue = "001";
			AssertEquals("Should not change", "001", Queue.P4_CustomsQueue);
			AssertEquals("Should not change", "S01", Queue.P4_CustomsStatus);
			AssertEquals("Should not change", "TTT", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_CustomsReason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKCustomsTaskAssignedTo);

			Queue.P4_CustomsQueue = "002";
			AssertEquals("002", Queue.P4_CustomsQueue);
			AssertEquals("Should be emptied", "", Queue.P4_CustomsStatus);
			AssertEquals("Should be emptied", "", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_CustomsReason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKCustomsTaskAssignedTo);
		}

		public void TestP4_CustomsQueue_DependentPropertiesAreResetWhenOriginalValueIsAssigned()
		{
			Queue.P4_CustomsQueue = "001";
			Queue.P4_CustomsStatus = "01";
			Queue.P4_CustomsSubStatus = "S1";
			Queue.P4_CustomsReason = "Test";
			Queue.P4_GS_NKCustomsTaskAssignedTo = "HOW";
			Factory.Save();

			Queue.P4_CustomsQueue = "002";
			AssertEquals("Should be empty", "", Queue.P4_CustomsStatus);
			AssertEquals("Should be empty", "", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_CustomsReason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKCustomsTaskAssignedTo);

			Queue.P4_CustomsQueue = "001";
			AssertEquals("Should be reset", "01", Queue.P4_CustomsStatus);
			AssertEquals("Should be reset", "S1", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_CustomsReason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKCustomsTaskAssignedTo);
		}

		public void TestP4_CustomsStatus()
		{
			Queue.P4_CustomsQueue = "001";
			Queue.P4_CustomsStatus = "S01";
			Queue.P4_CustomsSubStatus = "TTT";
			Queue.P4_CustomsReason = "Reason1";
			Queue.P4_GS_NKCustomsTaskAssignedTo = ">o<";

			AssertEquals("Pre-condition", "001", Queue.P4_CustomsQueue);
			AssertEquals("Pre-condition", "S01", Queue.P4_CustomsStatus);
			AssertEquals("Pre-condition", "TTT", Queue.P4_CustomsSubStatus);
			AssertEquals("Pre-condition", "Reason1", Queue.P4_CustomsReason);
			AssertEquals("Pre-condition", ">o<", Queue.P4_GS_NKCustomsTaskAssignedTo);
			Factory.Save();

			Queue.P4_CustomsStatus = "S01";
			AssertEquals("Should not change", "001", Queue.P4_CustomsQueue);
			AssertEquals("Should not change", "S01", Queue.P4_CustomsStatus);
			AssertEquals("Should not change", "TTT", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_CustomsReason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKCustomsTaskAssignedTo);

			Queue.P4_CustomsStatus = "S02";
			AssertEquals("Should not change", "001", Queue.P4_CustomsQueue);
			AssertEquals("S02", Queue.P4_CustomsStatus);
			AssertEquals("Should be emptied", "", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_CustomsReason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKCustomsTaskAssignedTo);
		}

		public void TestP4_CustomsStatus_DependentPropertiesAreResetWhenOriginalValueIsAssigned()
		{
			Queue.P4_CustomsQueue = "001";
			Queue.P4_CustomsStatus = "01";
			Queue.P4_CustomsSubStatus = "S1";
			Queue.P4_CustomsReason = "Test";
			Queue.P4_GS_NKCustomsTaskAssignedTo = "HOW";
			Factory.Save();

			Queue.P4_CustomsStatus = "02";
			AssertEquals("Should not change", "001", Queue.P4_CustomsQueue);
			AssertEquals("Should be empty", "", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_CustomsReason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKCustomsTaskAssignedTo);

			Queue.P4_CustomsStatus = "01";
			AssertEquals("Should not change", "001", Queue.P4_CustomsQueue);
			AssertEquals("Should be reset", "S1", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_CustomsReason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKCustomsTaskAssignedTo);
		}

		public void TestP4_CustomsStatus_SubStatusIsClearedWhenStatusIsSet()
		{
			Queue.P4_CustomsQueue = "001";
			Queue.P4_CustomsSubStatus = "S1";
			Queue.P4_CustomsStatus = "01";

			AssertEquals("Should not change", "001", Queue.P4_CustomsQueue);
			AssertEquals("Should be empty", "", Queue.P4_CustomsSubStatus);
			AssertEquals("Should not change", "01", Queue.P4_CustomsStatus);
		}

		public void TestP4_CustomsStatusDefaultedIfOnly1StatusSelectable()
		{
			ProcessQueue queue = (ProcessQueue)Factory.New(typeof(ProcessQueueForTest));
			queue.P4_CustomsQueue = "XXX";
			AssertEquals("Status should not be auto-populated if there is more than 1", "", queue.P4_CustomsStatus);
			queue.P4_CustomsQueue = "ON1";
			AssertEquals("Status should be defaulted if there is only 1 possible", "XXX", queue.P4_CustomsStatus);
		}

		public void TestP4_CustomsSubStatusDefaultedIfOnly1SubStatusSelectable()
		{
			ProcessQueue queue = (ProcessQueue)Factory.New(typeof(ProcessQueueForTest));
			queue.P4_CustomsStatus = "XXX";
			AssertEquals("Sub-status should not be auto-populated if there is more than 1", "", queue.P4_CustomsSubStatus);
			queue.P4_CustomsStatus = "S1";
			AssertEquals("Sub-status should be defaulted if there is only 1 possible", "YYY", queue.P4_CustomsSubStatus);
		}

		public void TestP4_QueueName()
		{
			Queue.P4_QueueName = "001";
			Queue.P4_Status = "S01";
			Queue.P4_SubStatus = "TTT";
			Queue.P4_Reason = "Reason1";
			Queue.P4_GS_NKTaskAssignedTo = ">o<";

			AssertEquals("Pre-condition", "001", Queue.P4_QueueName);
			AssertEquals("Pre-condition", "S01", Queue.P4_Status);
			AssertEquals("Pre-condition", "TTT", Queue.P4_SubStatus);
			AssertEquals("Pre-condition", "Reason1", Queue.P4_Reason);
			AssertEquals("Pre-condition", ">o<", Queue.P4_GS_NKTaskAssignedTo);
			Factory.Save();

			Queue.P4_QueueName = "001";
			AssertEquals("Should not change", "001", Queue.P4_QueueName);
			AssertEquals("Should not change", "S01", Queue.P4_Status);
			AssertEquals("Should not change", "TTT", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_Reason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKTaskAssignedTo);

			Queue.P4_QueueName = "002";
			AssertEquals("002", Queue.P4_QueueName);
			AssertEquals("Should be emptied", "", Queue.P4_Status);
			AssertEquals("Should be emptied", "", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_Reason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKTaskAssignedTo);
		}

		public void TestP4_QueueName_DependentPropertiesAreResetWhenOriginalValueIsAssigned()
		{
			Queue.P4_QueueName = "001";
			Queue.P4_Status = "01";
			Queue.P4_SubStatus = "S1";
			Queue.P4_Reason = "Test";
			Queue.P4_GS_NKTaskAssignedTo = "HOW";
			Factory.Save();

			Queue.P4_QueueName = "002";
			AssertEquals("Should be empty", "", Queue.P4_Status);
			AssertEquals("Should be empty", "", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_Reason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKTaskAssignedTo);

			Queue.P4_QueueName = "001";
			AssertEquals("Should be reset", "01", Queue.P4_Status);
			AssertEquals("Should be reset", "S1", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_Reason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKTaskAssignedTo);
		}

		public void TestP4_Status()
		{
			Queue.P4_QueueName = "001";
			Queue.P4_Status = "S01";
			Queue.P4_SubStatus = "TTT";
			Queue.P4_Reason = "Reason1";
			Queue.P4_GS_NKTaskAssignedTo = ">o<";

			AssertEquals("Pre-condition", "001", Queue.P4_QueueName);
			AssertEquals("Pre-condition", "S01", Queue.P4_Status);
			AssertEquals("Pre-condition", "TTT", Queue.P4_SubStatus);
			AssertEquals("Pre-condition", "Reason1", Queue.P4_Reason);
			AssertEquals("Pre-condition", ">o<", Queue.P4_GS_NKTaskAssignedTo);
			Factory.Save();

			Queue.P4_Status = "S01";
			AssertEquals("Should not change", "001", Queue.P4_QueueName);
			AssertEquals("Should not change", "S01", Queue.P4_Status);
			AssertEquals("Should not change", "TTT", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_Reason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKTaskAssignedTo);

			Queue.P4_Status = "S02";
			AssertEquals("Should not change", "001", Queue.P4_QueueName);
			AssertEquals("S02", Queue.P4_Status);
			AssertEquals("Should be emptied", "", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Reason1", Queue.P4_Reason);
			AssertEquals("Should not change", ">o<", Queue.P4_GS_NKTaskAssignedTo);
		}

		public void TestP4_Status_DependentPropertiesAreResetWhenOriginalValueIsAssigned()
		{
			Queue.P4_QueueName = "001";
			Queue.P4_Status = "01";
			Queue.P4_SubStatus = "S1";
			Queue.P4_Reason = "Test";
			Queue.P4_GS_NKTaskAssignedTo = "HOW";
			Factory.Save();

			Queue.P4_Status = "02";
			AssertEquals("Should not change", "001", Queue.P4_QueueName);
			AssertEquals("Should be empty", "", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_Reason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKTaskAssignedTo);

			Queue.P4_Status = "01";
			AssertEquals("Should not change", "001", Queue.P4_QueueName);
			AssertEquals("Should be reset", "S1", Queue.P4_SubStatus);
			AssertEquals("Should not change", "Test", Queue.P4_Reason);
			AssertEquals("Should not change", "HOW", Queue.P4_GS_NKTaskAssignedTo);
		}

		public void TestP4_Status_SubStatusIsClearedWhenStatusIsSet()
		{
			Queue.P4_QueueName = "001";
			Queue.P4_SubStatus = "S1";
			Queue.P4_Status = "01";

			AssertEquals("Should not change", "001", Queue.P4_QueueName);
			AssertEquals("Should be empty", "", Queue.P4_SubStatus);
			AssertEquals("Should not change", "01", Queue.P4_Status);
		}

		public void TestP4_StatusDefaultedIfOnly1StatusSelectable()
		{
			ProcessQueue queue = (ProcessQueue)Factory.New(typeof(ProcessQueueForTest));
			queue.P4_QueueName = "XXX";
			AssertEquals("Status should not be auto-populated if there is more than 1", "", queue.P4_Status);
			queue.P4_QueueName = "ON2";
			AssertEquals("Status should be defaulted if there is only 1 possible", "XXX", queue.P4_Status);
		}

		public void TestP4_SubStatusDefaultedIfOnly1SubStatusSelectable()
		{
			ProcessQueue queue = (ProcessQueue)Factory.New(typeof(ProcessQueueForTest));
			queue.P4_Status = "XXX";
			AssertEquals("Sub-status should not be auto-populated if there is more than 1", "", queue.P4_SubStatus);
			queue.P4_Status = "S2";
			AssertEquals("Sub-status should be defaulted if there is only 1 possible", "YYY", queue.P4_SubStatus);
		}

		public void TestCustomsQueueShouldNotAffectCommercialQueueAndViceVersa()
		{
			Queue.P4_QueueName = "Q01";
			Queue.P4_CustomsQueue = "Q02";
			Queue.P4_CustomsStatus = "S1";
			Queue.P4_Status = "s2";
			Queue.P4_SubStatus = "ss1";
			Queue.P4_CustomsSubStatus = "ss2";
			Queue.P4_CustomsReason = "Reason1";
			Queue.P4_Reason = "Reason2";
			Queue.P4_GS_NKCustomsTaskAssignedTo = "AAA";
			Queue.P4_GS_NKTaskAssignedTo = "BBB";

			AssertEquals("Q01", Queue.P4_QueueName);
			AssertEquals("s2", Queue.P4_Status);
			AssertEquals("ss1", Queue.P4_SubStatus);
			AssertEquals("Reason2", Queue.P4_Reason);
			AssertEquals("BBB", Queue.P4_GS_NKTaskAssignedTo);

			AssertEquals("Q02", Queue.P4_CustomsQueue);
			AssertEquals("S1", Queue.P4_CustomsStatus);
			AssertEquals("ss2", Queue.P4_CustomsSubStatus);
			AssertEquals("Reason1", Queue.P4_CustomsReason);
			AssertEquals("AAA", Queue.P4_GS_NKCustomsTaskAssignedTo);
		}

		#endregion

		#region QueueLogs

		public void TestCustomsQueueLogs()
		{
			AssertNotNull(Queue.CustomsQueueLogs);
			AssertEquals("Should be loaded in the getter", true, Queue.CustomsQueueLogs.IsLoaded);
			AssertEquals(0, Queue.CustomsQueueLogs.Count);

			AddProcessQueueLog(ProcessQueueType.Customs);
			AddProcessQueueLog(ProcessQueueType.Customs);
			AddProcessQueueLog(ProcessQueueType.Customs);
			AddProcessQueueLog(ProcessQueueType.Commercial);
			Queue.CustomsQueueLogs.Load();
			AssertEquals(3, Queue.CustomsQueueLogs.Count);
		}

		public void TestCommercialQueueLogs()
		{
			AssertNotNull(Queue.CommercialQueueLogs);
			AssertEquals("Should be loaded in the getter", true, Queue.CommercialQueueLogs.IsLoaded);
			AssertEquals(0, Queue.CommercialQueueLogs.Count);

			AddProcessQueueLog(ProcessQueueType.Customs);
			AddProcessQueueLog(ProcessQueueType.Customs);
			AddProcessQueueLog(ProcessQueueType.Customs);
			AddProcessQueueLog(ProcessQueueType.Commercial);
			Queue.CommercialQueueLogs.Load();
			AssertEquals(1, Queue.CommercialQueueLogs.Count);
		}

		public void TestIsCustomsQueueLogsLoaded()
		{
			AssertEquals("not yet lazy loaded", false, Queue.IsCustomsQueueLogsLoaded);
			AssertEquals("ensure that calling this property does not lazy load the logs", false, Queue.IsCustomsQueueLogsLoaded);

			object forceLazyLoading = Queue.CustomsQueueLogs;
			AssertEquals(true, Queue.IsCustomsQueueLogsLoaded);
		}

		public void TestIsCommercialQueueLogsLoaded()
		{
			AssertEquals("not yet lazy loaded", false, Queue.IsCommercialQueueLogsLoaded);
			AssertEquals("ensure that calling this property does not lazy load the logs", false, Queue.IsCommercialQueueLogsLoaded);

			object forceLazyLoading = Queue.CommercialQueueLogs;
			AssertEquals(true, Queue.IsCommercialQueueLogsLoaded);
		}

		void AddProcessQueueLog(ZString reference)
		{
			ProcessQueueLog log = Factory.New<ProcessQueueLog>();
			log.SL_Reference = reference;
			log.SL_Parent = Queue.PK;
			log.SL_Table = Queue.TableName;
		}

		#endregion

		#region QueueHasChanges

		public void TestCustomsQueueHasChanges()
		{
			Queue.P4_CustomsQueue = "Q1";
			Queue.P4_CustomsStatus = "S1";
			Queue.P4_CustomsSubStatus = "SS1";
			Queue.P4_CustomsReason = "R1";
			Queue.P4_GS_NKCustomsTaskAssignedTo = "A1";
			AssertEquals(true, Queue.CustomsQueueHasChanges);
			AssertEquals(false, Queue.CommercialQueueHasChanges);

			Factory.Save();
			AssertEquals(false, Queue.CustomsQueueHasChanges);

			Queue.P4_CustomsReason = "r2";
			AssertEquals(true, Queue.CustomsQueueHasChanges);

			Queue.P4_CustomsReason = "R1";
			AssertEquals(false, Queue.CustomsQueueHasChanges);
		}

		public void TestCommercialQueueHasChanges()
		{
			Queue.P4_QueueName = "Q1";
			Queue.P4_Status = "S1";
			Queue.P4_SubStatus = "SS1";
			Queue.P4_Reason = "R1";
			Queue.P4_GS_NKTaskAssignedTo = "A1";
			AssertEquals(false, Queue.CustomsQueueHasChanges);
			AssertEquals(true, Queue.CommercialQueueHasChanges);

			Factory.Save();
			AssertEquals(false, Queue.CommercialQueueHasChanges);

			Queue.P4_SubStatus = "SS2";
			AssertEquals(true, Queue.CommercialQueueHasChanges);

			Queue.P4_SubStatus = "SS1";
			AssertEquals(false, Queue.CommercialQueueHasChanges);
		}

		#endregion

		public void TestParent()
		{
			AssertNull("Not set yet, should be null", Queue.Parent);
			AssertEquals(ZGuid.Empty, Queue.P4_ParentID);
			AssertEquals("", Queue.P4_ParentTableCode);

			ProcessQueueParentForTest queueParent = Factory.New<ProcessQueueParentForTest>();
			Queue.Parent = queueParent;
			AssertEquals(queueParent, Queue.Parent);
			AssertEquals("_1", Queue.P4_ParentTableCode);
			AssertEquals(queueParent.PK, Queue.P4_ParentID);
		}

		[ExpectNoExceptions]
		public void TestParent_AssignedToNull()
		{
			Queue.Parent = null;
		}

		#region Implementation

		ProcessQueue Queue
		{
			get
			{
				if (fQueue == null)
				{
					fQueue = Factory.New<ProcessQueue>();
				}
				return fQueue;
			}
		}

		ProcessQueue fQueue;

		#endregion
	}
}
