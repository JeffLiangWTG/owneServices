using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ActiveProcessQueue))]
	sealed class ActiveProcessQueueTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstructor()
		{
			ActiveProcessQueue activeProcessQueue = ActiveProcessQueue.New(ProcessQueue);
			AssertEquals("Default Queue Type should be initialised in the constructor", ProcessQueueType.Enum.Customs, activeProcessQueue.QueueType);
			AssertEquals(ProcessQueue, activeProcessQueue.ProcessQueue);
		}

		public void TestQueueName()
		{
			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			ProcessQueue.P4_QueueName = "ABC";
			ProcessQueue.P4_CustomsQueue = "CED";
			AssertEquals("CED", AProcessQueue.QueueName);
			AssertEquals(ProcessQueue.P4_CustomsQueueInfo, ((ZWrappedPropertyInfo)AProcessQueue.QueueNameInfo).InnerInfo);

			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("ABC", AProcessQueue.QueueName);
			AssertEquals(ProcessQueue.P4_QueueNameInfo, ((ZWrappedPropertyInfo)AProcessQueue.QueueNameInfo).InnerInfo);

			AssertEquals("Queue", AProcessQueue.QueueNameCaption);
		}

		public void TestStatus()
		{
			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			ProcessQueue.P4_Status = "S1";
			ProcessQueue.P4_CustomsStatus = "S2";
			AssertEquals("S2", AProcessQueue.Status);
			AssertEquals(ProcessQueue.P4_CustomsStatusInfo, ((ZWrappedPropertyInfo)AProcessQueue.StatusInfo).InnerInfo);

			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("S1", AProcessQueue.Status);
			AssertEquals(ProcessQueue.P4_StatusInfo, ((ZWrappedPropertyInfo)AProcessQueue.StatusInfo).InnerInfo);

			AssertEquals("Status", AProcessQueue.StatusCaption);
		}

		public void TestSubStatus()
		{
			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			ProcessQueue.P4_SubStatus = "X1";
			ProcessQueue.P4_CustomsSubStatus = "X2";
			AssertEquals("X2", AProcessQueue.SubStatus);
			AssertEquals(ProcessQueue.P4_CustomsSubStatusInfo, ((ZWrappedPropertyInfo)AProcessQueue.SubStatusInfo).InnerInfo);

			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("X1", AProcessQueue.SubStatus);
			AssertEquals(ProcessQueue.P4_SubStatusInfo, ((ZWrappedPropertyInfo)AProcessQueue.SubStatusInfo).InnerInfo);

			AssertEquals("Sub Status", AProcessQueue.SubStatusCaption);
		}

		public void TestHasSubStatuses()
		{
			AssertEquals(false, AProcessQueue.HasSubStatuses);
			ProcessQueue.Lookups.CustomsSubStatusList.AddPair("XXX");
			AssertEquals(true, AProcessQueue.HasSubStatuses);
		}

		public void TestAssignedTo()
		{
			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			ProcessQueue.P4_GS_NKTaskAssignedTo = "A1";
			ProcessQueue.P4_GS_NKCustomsTaskAssignedTo = "A2";
			AssertEquals("A2", AProcessQueue.AssignedTo);
			AssertEquals(ProcessQueue.P4_GS_NKCustomsTaskAssignedToInfo, ((ZWrappedPropertyInfo)AProcessQueue.AssignedToInfo).InnerInfo);

			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("A1", AProcessQueue.AssignedTo);
			AssertEquals(ProcessQueue.P4_GS_NKTaskAssignedToInfo, ((ZWrappedPropertyInfo)AProcessQueue.AssignedToInfo).InnerInfo);

			AssertEquals("Assigned To", AProcessQueue.AssignedToCaption);
		}

		public void TestReason()
		{
			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			ProcessQueue.P4_Reason = "R1";
			ProcessQueue.P4_CustomsReason = "R2";
			AssertEquals("R2", AProcessQueue.Reason);
			AssertEquals(ProcessQueue.P4_CustomsReasonInfo, ((ZWrappedPropertyInfo)AProcessQueue.ReasonInfo).InnerInfo);

			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("R1", AProcessQueue.Reason);
			AssertEquals(ProcessQueue.P4_ReasonInfo, ((ZWrappedPropertyInfo)AProcessQueue.ReasonInfo).InnerInfo);

			AssertEquals("Reason", AProcessQueue.ReasonCaption);
		}

		public void TestLookups()
		{
			AssertEquals(typeof(ActiveProcessQueueLookups), AProcessQueue.Lookups.GetType());
		}

		public void TestQueueLogs()
		{
			AssertEquals("Should not be loaded in constructor", false, AProcessQueue.IsQueueLogsLoaded);
			AssertEquals(AProcessQueue, ((ActiveProcessQueueLogCollection)AProcessQueue.QueueLogs.CollectionToFilter).ActiveProcessQueue);
			AssertEquals("Should be loaded now", true, AProcessQueue.IsQueueLogsLoaded);
		}

		public void TestIncludeLogsWithEmptyQueueName()
		{
			AssertEquals("Default should be true", true, AProcessQueue.IncludeLogsWithEmptyQueueName);
		}

		public void TestIsCustomsQueue()
		{
			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals(true, AProcessQueue.IsCustomsQueue);
			AssertEquals(false, AProcessQueue.IsCommercialQueue);
		}

		public void TestIsCommercialQueue()
		{
			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals(false, AProcessQueue.IsCustomsQueue);
			AssertEquals(true, AProcessQueue.IsCommercialQueue);
		}

		public void TestQueueType()
		{
			ProcessQueue.CustomsQueueLogs.AddNew("1", "", "", "", "");
			ProcessQueue.CustomsQueueLogs.AddNew("2", "", "", "", "");
			ProcessQueue.CommercialQueueLogs.AddNew("2", "", "", "", "");
			AProcessQueue.RefreshBindingCalled = false;
			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("Should be rebuilt when QueueType is changed", 1, AProcessQueue.QueueLogs.Count);
			AssertEquals("RefreshBinding() should be called", true, AProcessQueue.RefreshBindingCalled);

			AProcessQueue.RefreshBindingCalled = false;
			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals("Should be rebuilt when QueueType is changed", 2, AProcessQueue.QueueLogs.Count);
			AssertEquals("RefreshBinding() should be called", true, AProcessQueue.RefreshBindingCalled);
		}

		public void TestQueueType_QueueLogsDoNotGetRebuiltWhenQueueTypeChangedIfNotAlreadyLoaded()
		{
			AssertEquals(false, AProcessQueue.IsQueueLogsLoaded);

			AProcessQueue.QueueType = ProcessQueueType.Enum.Commercial;
			AssertEquals("Should not be rebuilt if not already loaded", false, AProcessQueue.IsQueueLogsLoaded);

			AProcessQueue.QueueType = ProcessQueueType.Enum.Customs;
			AssertEquals("Should not be rebuilt if not already loaded", false, AProcessQueue.IsQueueLogsLoaded);
		}

		public void TestDelegatedConstruction()
		{
			AssertEquals(typeof(ActiveProcessQueue), ActiveProcessQueue.New(ProcessQueue).GetType());

			ActiveProcessQueueForTest.RegisterThisSubTypeOverride();
			AssertEquals(typeof(ActiveProcessQueueForTest), ActiveProcessQueue.New(ProcessQueue).GetType());
		}

		#region Implementation

		public override void TestBizObjectFields()
		{
			// Overridden as the custom behaviour of the setter is not appropriate for BizObjectFields Test
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return ActiveProcessQueue.New(ProcessQueue);
		}

		ActiveProcessQueueForTest AProcessQueue
		{
			get
			{
				if (fAProcessQueue == null)
				{
					fAProcessQueue = new ActiveProcessQueueForTest(ProcessQueue);
				}
				return fAProcessQueue;
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

		ActiveProcessQueueForTest fAProcessQueue;
		ProcessQueue fProcessQueue;

		#region ActiveProcessQueueForTest

		class ActiveProcessQueueForTest : ActiveProcessQueue
		{
			public ActiveProcessQueueForTest(ProcessQueue processQueue) : base(processQueue)
			{
			}

			public new static ActiveProcessQueue New(ProcessQueue processQueue)
			{
				return new ActiveProcessQueueForTest(processQueue);
			}

			public static void RegisterThisSubTypeOverride()
			{
				OverridableNewDelegate.Value = new NewDelegate(New);
			}

			public ProcessQueueType.Enum TestDefaultQueueType = ProcessQueueType.Enum.Customs;
			protected override ProcessQueueType.Enum DefaultQueueType
			{
				get { return TestDefaultQueueType; }
			}

			protected override void OnElementChanged()
			{
				base.OnElementChanged();
				RefreshBindingCalled = true;
			}

			public bool RefreshBindingCalled;
		}

		#endregion

		#endregion
	}
}
