using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ProcessQueueLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestQueueList()
		{
			AssertEquals("Default should be an empty CodeDescriptionPairList", 0, Lookups.CustomsQueueList.Count);
			AssertEquals("Default should be an empty CodeDescriptionPairList", 0, Lookups.CommercialQueueList.Count);
		}

		public void TestCustomsQueueListCaching()
		{
			CodeDescriptionPairList initialQueueList = Lookups.CustomsQueueList;
			AssertEquals("By default should be cached", initialQueueList, Lookups.CustomsQueueList);

			Lookups.TestCustomsQueueListShouldBeCached = false;
			Assert("Should not be cached now", initialQueueList != Lookups.CustomsQueueList);
		}

		public void TestCommercialQueueListCaching()
		{
			CodeDescriptionPairList initialQueueList = Lookups.CommercialQueueList;
			AssertEquals("By default should be cached", initialQueueList, Lookups.CommercialQueueList);

			Lookups.TestCommercialQueueListShouldBeCached = false;
			Assert("Should not be cached now", initialQueueList != Lookups.CommercialQueueList);
		}

		public void TestStatusList()
		{
			AssertEquals("Default should be an empty CodeDescriptionPairList", 0, Lookups.CustomsStatusList.Count);
			AssertEquals("Default should be an empty CodeDescriptionPairList", 0, Lookups.CommercialStatusList.Count);
		}

		public void TestCustomsStatusListCaching()
		{
			CodeDescriptionPairList initialStatusList = Lookups.CustomsStatusList;
			AssertEquals("By default should be cached", initialStatusList, Lookups.CustomsStatusList);

			Lookups.TestCustomsStatusListShouldBeCached = false;
			Assert("Should not be cached now", initialStatusList != Lookups.CustomsStatusList);
		}

		public void TestCommercialStatusListCaching()
		{
			CodeDescriptionPairList initialStatusList = Lookups.CommercialStatusList;
			AssertEquals("By default should be cached", initialStatusList, Lookups.CommercialStatusList);

			Lookups.TestCommercialStatusListShouldBeCached = false;
			Assert("Should not be cached now", initialStatusList != Lookups.CommercialStatusList);
		}

		public void TestSubStatusList()
		{
			AssertEquals("Default should be an empty CodeDescriptionPairList", 0, Lookups.CustomsSubStatusList.Count);
			AssertEquals("Default should be an empty CodeDescriptionPairList", 0, Lookups.CommercialSubStatusList.Count);
		}

		public void TestCustomsSubStatusListCaching()
		{
			CodeDescriptionPairList initialSubStatusList = Lookups.CustomsSubStatusList;
			AssertEquals("By default should be cached", initialSubStatusList, Lookups.CustomsSubStatusList);

			Lookups.TestCustomsSubStatusListShouldBeCached = false;
			Assert("Should not be cached now", initialSubStatusList != Lookups.CustomsSubStatusList);
		}

		public void TestCommercialSubStatusListCaching()
		{
			CodeDescriptionPairList initialSubStatusList = Lookups.CommercialSubStatusList;
			AssertEquals("By default should be cached", initialSubStatusList, Lookups.CommercialSubStatusList);

			Lookups.TestCommercialSubStatusListShouldBeCached = false;
			Assert("Should not be cached now", initialSubStatusList != Lookups.CommercialSubStatusList);
		}

		#region Implementation

		ProcessQueueLookupsForTest Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new ProcessQueueLookupsForTest(ProcessQueue);
				}
				return fLookups;
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

		ProcessQueueLookupsForTest fLookups;
		ProcessQueue fProcessQueue;

		#region ProcessQueueLookupsForTest

		class ProcessQueueLookupsForTest : ProcessQueueLookups
		{
			public ProcessQueueLookupsForTest(AutoProcessQueue queue) : base(queue)
			{
				TestCustomsQueueListShouldBeCached = base.CustomsQueueListShouldBeCached;
				TestCustomsStatusListShouldBeCached = base.CustomsStatusListShouldBeCached;
				TestCustomsSubStatusListShouldBeCached = base.CustomsSubStatusListShouldBeCached;

				TestCommercialQueueListShouldBeCached = base.CommercialQueueListShouldBeCached;
				TestCommercialStatusListShouldBeCached = base.CommercialStatusListShouldBeCached;
				TestCommercialSubStatusListShouldBeCached = base.CommercialSubStatusListShouldBeCached;
			}

			public new BusinessObjectFactory Factory
			{
				get { return base.Factory; }
			}

			protected override bool CustomsQueueListShouldBeCached
			{
				get { return TestCustomsQueueListShouldBeCached; }
			}

			protected override bool CommercialQueueListShouldBeCached
			{
				get { return TestCommercialQueueListShouldBeCached; }
			}

			protected override bool CustomsStatusListShouldBeCached
			{
				get { return TestCustomsStatusListShouldBeCached; }
			}

			protected override bool CommercialStatusListShouldBeCached
			{
				get { return TestCommercialStatusListShouldBeCached; }
			}

			protected override bool CustomsSubStatusListShouldBeCached
			{
				get { return TestCustomsSubStatusListShouldBeCached; }
			}

			protected override bool CommercialSubStatusListShouldBeCached
			{
				get { return TestCommercialSubStatusListShouldBeCached; }
			}

			public bool TestCustomsQueueListShouldBeCached;
			public bool TestCommercialQueueListShouldBeCached;
			public bool TestCustomsStatusListShouldBeCached;
			public bool TestCommercialStatusListShouldBeCached;
			public bool TestCustomsSubStatusListShouldBeCached;
			public bool TestCommercialSubStatusListShouldBeCached;
		}

		#endregion

		#endregion
	}
}
