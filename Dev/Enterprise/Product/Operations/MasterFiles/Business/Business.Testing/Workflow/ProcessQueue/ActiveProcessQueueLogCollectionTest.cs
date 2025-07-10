using System;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveProcessQueueLogCollection))]
	sealed class ActiveProcessQueueLogCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestConstructor()
		{
			AssertEquals(ActiveProcessQueue, Collection.ActiveProcessQueue);
			AssertEquals("Should be loaded in the constructor", true, Collection.IsLoaded);
		}

		public void TestReadOnly()
		{
			AssertEquals("Should always be read-only", true, Collection.ReadOnly);
		}

		public new void TestLoad()
		{
			ProcessQueue.CustomsQueueLogs.AddNew("1", "", "", "", "");
			ProcessQueue.CustomsQueueLogs.AddNew("2", "", "", "", "");
			ProcessQueue.CommercialQueueLogs.AddNew("3", "", "", "", "");
			ProcessQueue.CommercialQueueLogs.AddNew("4", "", "", "", "");
			ProcessQueue.CommercialQueueLogs.AddNew("5", "", "", "", "");
			ProcessQueue.CommercialQueueLogs.AddNew("6", "", "", "", "");

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			Collection.Load();
			Collection.Sort(ProcessQueueLog.Schema.Queue, ListSortDirection.Ascending);
			AssertEquals(2, Collection.Count);
			AssertEquals("1", Collection[0].Queue);
			AssertEquals("2", Collection[1].Queue);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			Collection.Load();
			AssertEquals(4, Collection.Count);
			AssertEquals("3", Collection[0].Queue);
			AssertEquals("4", Collection[1].Queue);
			AssertEquals("5", Collection[2].Queue);
			AssertEquals("6", Collection[3].Queue);
		}

		public void TestLoad_RefreshedWhenOriginalQueueChanged()
		{
			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals("Pre-condition", 0, Collection.Count);

			ProcessQueue.CustomsQueueLogs.AddNew();
			AssertEquals("Should be added to this collection", 1, Collection.Count);

			ProcessQueue.CustomsQueueLogs.AddNew();
			AssertEquals("Should be added to this collection", 2, Collection.Count);

			ProcessQueue.CommercialQueueLogs.AddNew();
			AssertEquals("Commercial Queue Logs should not affect this collection", 2, Collection.Count);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			Collection.Load();
			AssertEquals(1, Collection.Count);

			ProcessQueue.CommercialQueueLogs.AddNew();
			AssertEquals("Should be added to this collection", 2, Collection.Count);
		}

		public new void TestAddNew()
		{
			try
			{
				Collection.AddNew();
				Fail("Exception should be thrown");
			}
			catch (NotSupportedException ex)
			{
				AssertEquals("Should not be adding ProcessQueueLog from here", ex.Message);
			}
		}

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			// This collection can never be added to via Binding.
			// The AddNew() method throws a NotSupportedException.
			Assert(true);
		}

		#region Implementation

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new ActiveProcessQueueLogCollection(ActiveProcessQueue);
		}

		ActiveProcessQueue ActiveProcessQueue
		{
			get
			{
				if (fActiveProcessQueue == null)
				{
					fActiveProcessQueue = ActiveProcessQueue.New(ProcessQueue);
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

		new ActiveProcessQueueLogCollection Collection
		{
			get { return (ActiveProcessQueueLogCollection)base.Collection; }
		}

		ActiveProcessQueue fActiveProcessQueue;
		ProcessQueue fProcessQueue;

		#endregion
	}
}
