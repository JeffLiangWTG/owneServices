using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveProcessQueueLogCollectionView))]
	sealed class ActiveProcessQueueLogCollectionViewTest : BusinessObjectCollectionViewTestCase<ActiveProcessQueueLogCollectionView>
	{
		public void TestViewIsSortedByEventTimeInConstructor()
		{
			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AddCustomsProcessQueueLog("1", new ZDateTime(2005, 11, 1));
			AddCustomsProcessQueueLog("A", new ZDateTime(2005, 11, 2));
			AddCustomsProcessQueueLog("B", new ZDateTime(2005, 11, 3));
			AddCustomsProcessQueueLog("6", new ZDateTime(2005, 11, 4));
			AddCustomsProcessQueueLog("12", new ZDateTime(2005, 11, 5));

			ActiveProcessQueueLogCollectionView view = new ActiveProcessQueueLogCollectionView(CollectionToFilter);
			AssertEquals(5, view.Count);
			AssertEquals("Should be descendingly ordered by EventTime", "12", view[0].Queue);
			AssertEquals("Should be descendingly ordered by EventTime", "6", view[1].Queue);
			AssertEquals("Should be descendingly ordered by EventTime", "B", view[2].Queue);
			AssertEquals("Should be descendingly ordered by EventTime", "A", view[3].Queue);
			AssertEquals("Should be descendingly ordered by EventTime", "1", view[4].Queue);
		}

		public void TestIsThisPartOfTheCollection()
		{
			AssertEquals(0, Collection.Count);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AddCustomsProcessQueueLog("1", new ZDateTime(2005, 11, 1));
			AddCustomsProcessQueueLog("", new ZDateTime(2005, 11, 2));
			AddCustomsProcessQueueLog("BB", new ZDateTime(2005, 11, 3));
			AddCustomsProcessQueueLog("", new ZDateTime(2005, 11, 4));
			AddCustomsProcessQueueLog("120", new ZDateTime(2005, 11, 5));
			AddCustomsProcessQueueLog("121", new ZDateTime(2005, 11, 6));
			AddCustomsProcessQueueLog("", new ZDateTime(2005, 11, 8));
			AssertEquals(7, Collection.Count);

			ActiveProcessQueue.TestIncludeLogsWithEmptyQueueName = false;
			Collection.Rebuild();
			AssertEquals(4, Collection.Count);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("has not been rebuilt", 4, Collection.Count);

			Collection.Rebuild();
			AssertEquals(0, Collection.Count);
		}

		#region Implementation

		protected override ActiveProcessQueueLogCollectionView GetCollectionToTest()
		{
			return new ActiveProcessQueueLogCollectionView(CollectionToFilter);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return AddCustomsProcessQueueLog("1", ZDateTime.Today);
		}

		ActiveProcessQueueLogCollection CollectionToFilter
		{
			get
			{
				if (fCollectionToFilter == null)
				{
					fCollectionToFilter = new ActiveProcessQueueLogCollection(ActiveProcessQueue);
				}
				return fCollectionToFilter;
			}
		}

		ActiveProcessQueueForTest ActiveProcessQueue
		{
			get
			{
				if (fActiveProcessQueue == null)
				{
					fActiveProcessQueue = new ActiveProcessQueueForTest(ProcessQueue);
				}
				return fActiveProcessQueue;
			}
		}

		ProcessQueue ProcessQueue
		{
			get
			{
				if (fProcessQueue == null)
				{
					fProcessQueue = Factory.New<ProcessQueue>();
				}
				return fProcessQueue;
			}
		}

		new ActiveProcessQueueLogCollectionView Collection
		{
			get { return base.Collection; }
		}

		ProcessQueueLog AddCustomsProcessQueueLog(ZString queueName, ZDateTime eventTime)
		{
			ProcessQueueLog log = ProcessQueue.CustomsQueueLogs.AddNew(queueName, "", "", "", "");
			log.SL_EventTime = eventTime;
			return log;
		}

		ActiveProcessQueueLogCollection fCollectionToFilter;
		ActiveProcessQueueForTest fActiveProcessQueue;
		ProcessQueue fProcessQueue;

		#region ActiveProcessQueueForTest

		class ActiveProcessQueueForTest : ActiveProcessQueue
		{
			public ActiveProcessQueueForTest(ProcessQueue processQueue) : base(processQueue)
			{
				TestIncludeLogsWithEmptyQueueName = base.IncludeLogsWithEmptyQueueName;
			}

			public bool TestIncludeLogsWithEmptyQueueName;
			public override bool IncludeLogsWithEmptyQueueName
			{
				get { return TestIncludeLogsWithEmptyQueueName; }
			}
		}

		#endregion

		#endregion
	}
}
