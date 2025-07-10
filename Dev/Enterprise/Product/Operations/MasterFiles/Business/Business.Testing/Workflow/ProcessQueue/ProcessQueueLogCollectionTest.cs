using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ProcessQueueLogCollection))]
	sealed class ProcessQueueLogCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestChild()
		{
			ProcessQueueLog log = Collection.AddNew();
			AssertEquals(log, Collection[0]);
		}

		public void TestDefaultsForNewChild()
		{
			QueueType = ProcessQueueType.Enum.Commercial;
			ProcessQueueLog log = Collection.AddNew();
			AssertEquals("Invalid Queue Type", ProcessQueueType.Commercial, log.SL_Reference);

			QueueType = ProcessQueueType.Enum.Customs;
			ResetCollection();
			log = Collection.AddNew();
			AssertEquals("Invalid Queue Type", ProcessQueueType.Customs, log.SL_Reference);
		}

		public new void TestLoad()
		{
			QueueType = ProcessQueueType.Enum.Commercial;
			Collection.Load();
			AssertEquals("Pre-condition", 0, Collection.Count);

			AddNewProcessQueueLog(ProcessQueueType.Commercial, "AA");
			AddNewProcessQueueLog("ARG", "DD");
			AddNewProcessQueueLog(ProcessQueueType.Commercial, "CC");
			AddNewProcessQueueLog(ProcessQueueType.Customs, "DD");
			AddNewProcessQueueLog("JKT", "EE");
			AddNewProcessQueueLog(ProcessQueueType.Customs, "FF");
			AddNewProcessQueueLog("LAx", "ZZ");
			Collection.Load();
			Collection.Sort(StmALogSchema.Constants.SL_GS_NKUser, ListSortDirection.Ascending);
			AssertEquals(2, Collection.Count);
			AssertEquals("AA", Collection[0].SL_GS_NKUser);
			AssertEquals("CC", Collection[1].SL_GS_NKUser);

			ResetCollection();
			QueueType = ProcessQueueType.Enum.Customs;
			Collection.Load();
			Collection.Sort(StmALogSchema.Constants.SL_GS_NKUser, ListSortDirection.Ascending);
			AssertEquals(2, Collection.Count);
			AssertEquals("DD", Collection[0].SL_GS_NKUser);
			AssertEquals("FF", Collection[1].SL_GS_NKUser);
		}

		public void TestContainsQueue()
		{
			QueueType = ProcessQueueType.Enum.Commercial;
			Assert("Should return false, nothing in the collection", !Collection.ContainsQueue("BAD"));

			Queue.P4_QueueName = "AA";
			Queue.P4_CustomsQueue = "BAD";
			Factory.Save();
			Collection.Load();
			Assert("Should return false, Commercial Queue does not contain BAD", !Collection.ContainsQueue("BAD"));

			Queue.P4_QueueName = "BAD";
			Factory.Save();
			Collection.Load();
			Assert("Should contain BAD", Collection.ContainsQueue("BAD"));

			Queue.P4_QueueName = "HLD";
			Factory.Save();
			Collection.Load();
			Assert("Should still return true", Collection.ContainsQueue("BAD"));
		}

		public void TestContainsQueueWithExtraParameters()
		{
			Collection.AddNew("XXX", "S1", "SS", "MEh Reason", "XIX");
			Collection.AddNew("XXX", "S2", "SS", "MEh Reason", "XIX");
			Collection.AddNew("XXX", "S3", "SS", "MEh Reason", "XIX");

			AssertEquals("Should be found", true, Collection.ContainsQueue("XXX", "S1", "SS"));
			AssertEquals("Should be found", true, Collection.ContainsQueue("XXX", "S2", "SS"));
			AssertEquals("Should be found", true, Collection.ContainsQueue("XXX", "S3", "SS"));
			AssertEquals("Should not be found", false, Collection.ContainsQueue("XXX", "S4", "SS"));
		}

		public void TestGetLastQueueNameChangedEventDate()
		{
			AssertEquals("No queue history, should return empty ZDateTime", ZDateTime.Empty, Collection.GetLastQueueNameChangedEventDate());

			ZDateTime date1 = new ZDateTime(2005, 11, 1);
			AddNewProcessQueueLog("Q01", "", "", "", "", date1);
			AssertEquals(date1, Collection.GetLastQueueNameChangedEventDate());

			ZDateTime date2 = new ZDateTime(2005, 11, 2);
			AddNewProcessQueueLog("Q01", "X2", "LL", "", "", date2);
			AssertEquals("Queue name is not changed", date1, Collection.GetLastQueueNameChangedEventDate());

			AddNewProcessQueueLog("Q01", "", "", "SSasdfsdf", "LL", date2);
			AssertEquals("Queue name is not changed", date1, Collection.GetLastQueueNameChangedEventDate());

			AddNewProcessQueueLog("RAR", "", "", "SSasdfsdf", "LL", date2);
			AssertEquals(date2, Collection.GetLastQueueNameChangedEventDate());

			ZDateTime date3 = new ZDateTime(2005, 11, 3);
			AddNewProcessQueueLog("", "", "", "ASDF", "", date3);
			AssertEquals(date3, Collection.GetLastQueueNameChangedEventDate());
		}

		public void TestGetLastQueueNameChangedEventDate_WithParameter()
		{
			AssertEquals("No queue history, should return empty ZDateTime", ZDateTime.Empty, Collection.GetLastQueueNameChangedEventDate());

			AddNewProcessQueueLog("Q01", "", "", "", "", new ZDateTime(2005, 11, 1));
			AddNewProcessQueueLog("Q03", "", "", "", "", new ZDateTime(2005, 11, 2));
			AddNewProcessQueueLog("Q02", "", "", "", "", new ZDateTime(2005, 11, 3));
			AddNewProcessQueueLog("Q05", "", "", "", "", new ZDateTime(2005, 11, 4));
			AddNewProcessQueueLog("Q06", "", "", "", "", new ZDateTime(2005, 11, 5));
			AddNewProcessQueueLog("Q03", "", "", "", "", new ZDateTime(2005, 11, 6));
			AddNewProcessQueueLog("Q02", "", "", "", "", new ZDateTime(2005, 11, 7));
			AddNewProcessQueueLog("Q02", "", "", "23", "", new ZDateTime(2005, 11, 8));
			AddNewProcessQueueLog("Q01", "", "", "", "", new ZDateTime(2005, 11, 9));
			AddNewProcessQueueLog("Q01", "34", "33", "2", "", new ZDateTime(2005, 11, 10));
			AddNewProcessQueueLog("Q03", "", "", "", "", new ZDateTime(2005, 11, 11));
			AddNewProcessQueueLog("Q04", "", "", "", "", new ZDateTime(2005, 11, 12));
			AddNewProcessQueueLog("Q06", "", "", "", "", new ZDateTime(2005, 11, 13));
			AddNewProcessQueueLog("Q06", "123", "", "", "", new ZDateTime(2005, 11, 14));

			AssertEquals(new ZDateTime(2005, 11, 9), Collection.GetLastQueueNameChangedEventDate("Q01"));
			AssertEquals(new ZDateTime(2005, 11, 7), Collection.GetLastQueueNameChangedEventDate("Q02"));
			AssertEquals(new ZDateTime(2005, 11, 11), Collection.GetLastQueueNameChangedEventDate("Q03"));
			AssertEquals(new ZDateTime(2005, 11, 12), Collection.GetLastQueueNameChangedEventDate("Q04"));
			AssertEquals(new ZDateTime(2005, 11, 4), Collection.GetLastQueueNameChangedEventDate("Q05"));
			AssertEquals(new ZDateTime(2005, 11, 13), Collection.GetLastQueueNameChangedEventDate("Q06"));
		}

		public void TestGetLastQueueNameAndStatusesChangedEventDate()
		{
			AssertEquals("No queue history, should return empty ZDateTime", ZDateTime.Empty, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			ZDateTime date1 = new ZDateTime(2005, 11, 1);
			AddNewProcessQueueLog("Q01", "", "", "", "", date1);
			AssertEquals(date1, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			ZDateTime date2 = new ZDateTime(2005, 11, 2);
			AddNewProcessQueueLog("Q01", "", "", "adfkj", "", date2);
			AssertEquals("Queue name or statuses are not changed", date1, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			AddNewProcessQueueLog("Q01", "", "", "SSasdfsdf", "LL", date2);
			AssertEquals("Queue name or statuses are not changed", date1, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			AddNewProcessQueueLog("Q01", "", "LL", "SSasdfsdf", "LL", date2);
			AssertEquals(date2, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			ZDateTime date3 = new ZDateTime(2005, 11, 3);
			AddNewProcessQueueLog("Q01", "S1", "LL", "SSasdfsdf", "LL", date3);
			AssertEquals(date3, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			ZDateTime date4 = new ZDateTime(2005, 11, 4);
			AddNewProcessQueueLog("Q02", "S1", "LL", "SSasdfsdf", "LL", date4);
			AssertEquals(date4, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			ZDateTime date5 = new ZDateTime(2005, 11, 5);
			AddNewProcessQueueLog("Q02", "S1", "LL", "SSasdfsdf111", "LL", date5);
			AssertEquals("Queue name or statuses are not changed", date4, Collection.GetLastQueueNameAndStatusesChangedEventDate());

			AddNewProcessQueueLog("Q02", "S2", "LL", "SSasdfsdf111", "LL", date5);
			AssertEquals(date5, Collection.GetLastQueueNameAndStatusesChangedEventDate());
		}

		public void TestGetLastQueueChangedEventDate()
		{
			QueueType = ProcessQueueType.Enum.Customs;
			AddNewProcessQueueLog("Q01", "", "", "", "", new ZDateTime(2005, 11, 1));
			AddNewProcessQueueLog("Q02", "", "", "", "", new ZDateTime(2005, 11, 2));
			AssertEquals(new ZDateTime(2005, 11, 2), Collection.GetLastQueueChangedEventDate());

			AddNewProcessQueueLog("Q02", "", "", "", "", new ZDateTime(2005, 11, 4));
			AssertEquals(new ZDateTime(2005, 11, 4), Collection.GetLastQueueChangedEventDate());

			AddNewProcessQueueLog("Q01", "", "", "", "", new ZDateTime(2005, 11, 5));
			AssertEquals(new ZDateTime(2005, 11, 5), Collection.GetLastQueueChangedEventDate());

			AddNewProcessQueueLog("Q01", "", "", "", "qwe", new ZDateTime(2005, 11, 6));
			AssertEquals(new ZDateTime(2005, 11, 6), Collection.GetLastQueueChangedEventDate());
		}

		public void TestAddNewWithParameters()
		{
			ProcessQueueLog addedLog = Collection.AddNew("XXX", "S1", "SS", "MEh Reason", "XIX");
			Factory.Save();
			AssertEquals("XXX", addedLog.Queue);
			AssertEquals("S1", addedLog.Status);
			AssertEquals("SS", addedLog.SubStatus);
			AssertEquals("MEh Reason", addedLog.Reason);
			AssertEquals("XIX", addedLog.AssignedTo);
		}

		public void TestGetLogsDescendinglySortedByEventTime()
		{
			AddNewProcessQueueLog("Q01", "", "", "", "", new ZDateTime(2005, 11, 1));
			AddNewProcessQueueLog("Q02", "", "", "", "", new ZDateTime(2005, 11, 5));
			AddNewProcessQueueLog("Q03", "", "", "", "", new ZDateTime(2005, 11, 2));
			AddNewProcessQueueLog("Q04", "", "", "", "", new ZDateTime(2005, 11, 6));
			AddNewProcessQueueLog("Q05", "", "", "", "", new ZDateTime(2005, 11, 3));
			AddNewProcessQueueLog("Q06", "", "", "", "", new ZDateTime(2005, 11, 4));

			ProcessQueueLog[] sortedLogs = Collection.GetLogsDescendinglySortedByEventTime();
			AssertEquals("Incorrect number of logs returned", 6, sortedLogs.Length);
			AssertEquals("Q04", sortedLogs[0].Queue);
			AssertEquals("Q02", sortedLogs[1].Queue);
			AssertEquals("Q06", sortedLogs[2].Queue);
			AssertEquals("Q05", sortedLogs[3].Queue);
			AssertEquals("Q03", sortedLogs[4].Queue);
			AssertEquals("Q01", sortedLogs[5].Queue);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ProcessQueueLogCollection(Queue, QueueType);
		}

		new ProcessQueueLogCollectionForTest Collection
		{
			get
			{
				if (fCollection == null)
				{
					fCollection = new ProcessQueueLogCollectionForTest(Queue, QueueType);
				}
				return fCollection;
			}
		}

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

		void ResetCollection()
		{
			fCollection = null;
		}

		void AddNewProcessQueueLog(ZString reference, ZString user)
		{
			ProcessQueueLog log = Factory.New<ProcessQueueLog>();
			log.SL_Parent = Queue.PK;
			log.SL_Table = Queue.TableName;
			log.SL_Reference = reference;
			log.SL_GS_NKUser = user;
		}

		void AddNewProcessQueueLog(ZString queueName, ZString status, ZString subStatus, ZString reason, ZString assignedTo, ZDateTime eventTime)
		{
			ProcessQueueLog log = Collection.AddNew(queueName, status, subStatus, reason, assignedTo);
			log.SL_EventTime = eventTime;
		}

		ProcessQueueType.Enum QueueType;
		ProcessQueueLogCollectionForTest fCollection;
		ProcessQueue fQueue;

		#region ProcessQueueLogCollectionForTest

		class ProcessQueueLogCollectionForTest : ProcessQueueLogCollection
		{
			public ProcessQueueLogCollectionForTest(ProcessQueue processQueue, ProcessQueueType.Enum queueType) : base(processQueue, queueType)
			{
			}

			public new ZQuery AdditionalFilter
			{
				get { return base.CreateAdditionalFilter(); }
			}
		}

		#endregion

		#endregion
	}
}
