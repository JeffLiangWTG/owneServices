using System.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ActiveProcessQueueLookupsTest : TestCaseWithFactory
	{
		public void TestQueueList()
		{
			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals(ProcessQueue.Lookups.CustomsQueueList, Lookups.QueueList);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals(ProcessQueue.Lookups.CommercialQueueList, Lookups.QueueList);
		}

		public void TestStatusList()
		{
			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals(ProcessQueue.Lookups.CustomsStatusList, Lookups.StatusList);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals(ProcessQueue.Lookups.CommercialStatusList, Lookups.StatusList);
		}

		public void TestSubStatusList()
		{
			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals(ProcessQueue.Lookups.CustomsSubStatusList, Lookups.SubStatusList);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals(ProcessQueue.Lookups.CommercialSubStatusList, Lookups.SubStatusList);
		}

		public void TestTaskAssignedToList()
		{
			fProcessQueue = (ProcessQueue)Factory.New(typeof(ProcessQueueForTest));
			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals(ProcessQueue.Lookups.CustomsTaskAssignedTos, Lookups.TaskAssignedToList);

			ActiveProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals(ProcessQueue.Lookups.TaskAssignedTos, Lookups.TaskAssignedToList);
		}

		ActiveProcessQueueLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new ActiveProcessQueueLookups(ActiveProcessQueue);
				}
				return fLookups;
			}
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

		ActiveProcessQueueLookups fLookups;
		ActiveProcessQueue fActiveProcessQueue;
		ProcessQueue fProcessQueue;

		#region ProcessQueueForTest

		class ProcessQueueForTest : ProcessQueue
		{
			public ProcessQueueForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			protected override ProcessQueueLookups GetNewLookups()
			{
				return new ProcessQueueLookupsForTest(this);
			}
		}

		class ProcessQueueLookupsForTest : ProcessQueueLookups
		{
			public ProcessQueueLookupsForTest(ProcessQueueForTest parent) : base(parent)
			{
			}

			public override GlbStaffCollection TaskAssignedTos
			{
				get
				{
					if (fTaskAssignedTos == null)
					{
						fTaskAssignedTos = base.TaskAssignedTos;
					}
					return fTaskAssignedTos;
				}
			}

			public override GlbStaffCollection CustomsTaskAssignedTos
			{
				get
				{
					if (fCustomsTaskAssignedTos == null)
					{
						fCustomsTaskAssignedTos = base.CustomsTaskAssignedTos;
					}
					return fCustomsTaskAssignedTos;
				}
			}

			GlbStaffCollection fTaskAssignedTos;
			GlbStaffCollection fCustomsTaskAssignedTos;
		}

		#endregion
	}
}
