using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessQueueLog))]
	sealed class ProcessQueueLogTest : BaseStmALogTest
	{
		public void TestQueueLogType()
		{
			Log.SL_Reference = ProcessQueueType.Commercial;
			AssertEquals(false, Log.IsCustomsQueueLog);
			AssertEquals(true, Log.IsCommercialQueueLog);

			Log.SL_Reference = ProcessQueueType.Customs;
			AssertEquals(true, Log.IsCustomsQueueLog);
			AssertEquals(false, Log.IsCommercialQueueLog);
		}

		public void TestReadOnly()
		{
			AssertEquals("Should be read-only", true, Log.ReadOnly);
		}

		public void TestDefaultEventCode()
		{
			AssertEquals("Event code should be set to QueueChanged event by default", Events.QueueChanged.Code, Log.SL_SE_NKEvent);
		}

		public void TestMaster()
		{
			AssertEquals(ProcessQueue, Log.Master);
		}

		public void TestSetQueueDetails_QueueTypeSpecified()
		{
			Log.SetQueueDetails(ProcessQueueType.Enum.Customs, "Q1", "S0", "S1", "R1", "A1");
			AssertEquals(true, Log.IsCustomsQueueLog);
			AssertEquals(false, Log.IsCommercialQueueLog);
			AssertEquals("Q1", Log.Queue);
			AssertEquals("S0", Log.Status);
			AssertEquals("S1", Log.SubStatus);
			AssertEquals("R1", Log.Reason);
			AssertEquals("A1", Log.AssignedTo);

			Log.SetQueueDetails(ProcessQueueType.Enum.Commercial, "Q1", "S0", "S1", "R1", "A1");
			AssertEquals(false, Log.IsCustomsQueueLog);
			AssertEquals(true, Log.IsCommercialQueueLog);
			AssertEquals("Q1", Log.Queue);
			AssertEquals("S0", Log.Status);
			AssertEquals("S1", Log.SubStatus);
			AssertEquals("R1", Log.Reason);
			AssertEquals("A1", Log.AssignedTo);
		}

		public void TestSetQueueDetails()
		{
			Log.SetQueueDetails("Q1", "S0", "S1", "R1", "A1");
			Factory.Save();
			AssertEquals("Q1", Log.Queue);
			AssertEquals("S0", Log.Status);
			AssertEquals("S1", Log.SubStatus);
			AssertEquals("R1", Log.Reason);
			AssertEquals("A1", Log.AssignedTo);
		}

		public void TestLogReference_EmptyProcessQueue()
		{
			Log.OnSaving();
			AssertEquals("Should not serialise empty ProcessQueue", "", Log.SL_Reference);
		}

		public void TestLogReference_QueueChanged()
		{
			SetCustomsProcessQueue("123", "AA", "XF", "Reason1", "AOI");
			SetCommercialProcessQueue("234", "BB", "XG", "Reason2", "TTT");
			Log.OnSaving();
			AssertEquals("Should not be serialised, QueueType is not specified", "", Log.SL_Reference);

			Log.SL_Reference = ProcessQueueType.Customs;
			AssertEquals("Should not be serialised, QueueDetailsLine is only populated OnSaving", ProcessQueueType.Customs, Log.SL_Reference);

			Log.OnSaving();
			AssertEquals(ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "123", "AA", "XF", "Reason1", "AOI"), Log.SL_Reference);

			Log.SL_Reference = ProcessQueueType.Commercial;
			ResetQueueDetails();
			Log.OnSaving();
			AssertEquals(ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Commercial, "234", "BB", "XG", "Reason2", "TTT"), Log.SL_Reference);
		}

		public void TestLogReference_QueueChanged_EmptyDetails()
		{
			Log.SL_Reference = ProcessQueueType.Customs;
			Log.OnSaving();
			AssertEquals(ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "", "", "", "", ""), Log.SL_Reference);
			AssertEquals("", Log.Queue);
			AssertEquals("", Log.Status);
			AssertEquals("", Log.SubStatus);
			AssertEquals("", Log.Reason);
			AssertEquals("", Log.AssignedTo);
		}

		public void TestLogReference_OnSaving_ShouldRespectMaxLength()
		{
			Log.SL_Reference = ProcessQueueType.Commercial;
			Log.SetQueueDetails("Something", "Blah", "".PadLeft(1024, '-'), "No reason", "Me");
			Log.OnSaving();
			AssertEquals("COM\"Something\",\"Blah\",\"" + "".PadLeft(1001, '-'), Log.SL_Reference);
		}

		public void TestLogReference_NotOverwritingSavedSL_Reference()
		{
			Log.SL_Reference = ProcessQueueType.Customs;
			SetCustomsProcessQueue("345", "BB", "AA", "Reason2", "XXX");
			Factory.Save();
			string expectedString = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "345", "BB", "AA", "Reason2", "XXX");
			AssertEquals(expectedString, Log.SL_Reference);

			Log.SetQueueDetails("456", "BC", "DD", "Re\",\"ason2", "XX2");
			Factory.Save();
			AssertEquals("Should not change", expectedString, Log.SL_Reference);
		}

		public void TestNoExceptionsThrownWhenReferenceIsInvalid()
		{
			Log.SL_Reference = "INVALID REFERENCE FORMAT HERE";
			AssertEquals("", Log.Queue);
			AssertEquals("", Log.Status);
			AssertEquals("", Log.SubStatus);
			AssertEquals("", Log.Reason);
			AssertEquals("", Log.AssignedTo);
		}

		public void TestLogReference_PersistedAndCanBeDeserialisedByNewLog()
		{
			var log = ProcessQueue.Logs.AddNew();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.AccreditationAttemptCommencedCode;
				log.SL_Reference = ProcessQueueLog.GetEncodedLogReference(ProcessQueueType.Enum.Customs, "234", "AB", "TT", "Re\",\"ason1", "ZOE");
			}
			Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_Parent, ProcessQueue.PK);
			query.AddToFilter(StmALogSchema.SL_SE_NKEvent, log.SL_SE_NKEvent);
			var newLog = new BusinessObjectFactory().LoadTop1<ProcessQueueLog>(query);

			AssertEquals("234", newLog.Queue);
			AssertEquals("AB", newLog.Status);
			AssertEquals("TT", newLog.SubStatus);
			AssertEquals("Re\",\"ason1", newLog.Reason);
			AssertEquals("ZOE", newLog.AssignedTo);
		}

		#region Implementation

		void SetCustomsProcessQueue(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString taskUser)
		{
			ResetQueueDetails();
			ProcessQueue.P4_CustomsQueue = queueName;
			ProcessQueue.P4_CustomsStatus = status;
			ProcessQueue.P4_CustomsSubStatus = subStatus;
			ProcessQueue.P4_CustomsReason = reason;
			ProcessQueue.P4_GS_NKCustomsTaskAssignedTo = taskUser;
		}

		void SetCommercialProcessQueue(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString taskUser)
		{
			ResetQueueDetails();
			ProcessQueue.P4_QueueName = queueName;
			ProcessQueue.P4_Status = status;
			ProcessQueue.P4_SubStatus = subStatus;
			ProcessQueue.P4_Reason = reason;
			ProcessQueue.P4_GS_NKTaskAssignedTo = taskUser;
		}

		void ResetQueueDetails()
		{
			typeof(ProcessQueueLog).GetProperty("QueueDetailsLine", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(Log, null, null);
		}

		ProcessQueueLog Log
		{
			get
			{
				if (log == null)
				{
					log = Factory.New<ProcessQueueLog>();
					Log.Master = ProcessQueue;
				}
				return log;
			}
		}

		ProcessQueue ProcessQueue
		{
			get
			{
				return processQueue ?? (processQueue = Factory.New<ProcessQueue>());
			}
		}

		ProcessQueueLog log;
		ProcessQueue processQueue;

		#endregion
	}
}
