using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class WorkflowTriggerActionTrackerTest : TestCaseWithFactory
	{
		public void TestInitTriggerActionTracker()
		{
			using (WorkflowTriggerActionTracker.TrackTriggerActions(Factory))
			{
				var tracker = new TriggerActionTrackerForTest();
				AssertEquals(tracker, WorkflowTriggerActionTracker.GetOrCreateTracker(Factory, () => tracker));
				AssertEquals(tracker, WorkflowTriggerActionTracker.GetOrCreateTracker<TriggerActionTrackerForTest>(Factory, () => null));
			}
		}

		public void TestTryGetTriggerActionTracker()
		{
			AssertEquals(false, WorkflowTriggerActionTracker.TryGetTracker<TriggerActionTrackerForTest>(Factory, out var _));

			using (WorkflowTriggerActionTracker.TrackTriggerActions(Factory))
			{
				AssertEquals(false, WorkflowTriggerActionTracker.TryGetTracker<TriggerActionTrackerForTest>(Factory, out var _));
				var tracker = new TriggerActionTrackerForTest();
				AssertEquals(tracker, WorkflowTriggerActionTracker.GetOrCreateTracker(Factory, () => tracker));
				AssertEquals(true, WorkflowTriggerActionTracker.TryGetTracker<TriggerActionTrackerForTest>(Factory, out var _));
			}

			AssertEquals(false, WorkflowTriggerActionTracker.TryGetTracker<TriggerActionTrackerForTest>(Factory, out var _));
		}

		public void TestTriggerActionTrackerCalledOutOfScopeThrows()
		{
			AssertExceptionThrown<InvalidOperationException>(() => WorkflowTriggerActionTracker.GetOrCreateTracker(Factory, () => new TriggerActionTrackerForTest()));
			AssertExceptionThrown<InvalidOperationException>(() => WorkflowTriggerActionTracker.OnAllActionsRun(Factory));
		}

		public void TestNestedTriggerProcessing()
		{
			var outerTracker = new TriggerActionTrackerForTest();
			var innerTracker = new TriggerActionTrackerForTest();

			using (WorkflowTriggerActionTracker.TrackTriggerActions(Factory))
			{
				AssertEquals(outerTracker, WorkflowTriggerActionTracker.GetOrCreateTracker(Factory, () => outerTracker));
				using (WorkflowTriggerActionTracker.TrackTriggerActions(Factory))
				{
					AssertEquals("No tracker has been subscriber in the inner block", false, WorkflowTriggerActionTracker.TryGetTracker<TriggerActionTrackerForTest>(Factory, out var _));
					AssertEquals(innerTracker, WorkflowTriggerActionTracker.GetOrCreateTracker(Factory, () => innerTracker));
					WorkflowTriggerActionTracker.OnAllActionsRun(Factory);
					AssertEquals(true, innerTracker.HasOnAllActionsRun);
					AssertEquals(false, outerTracker.HasOnAllActionsRun);
				}

				AssertEquals(outerTracker, WorkflowTriggerActionTracker.GetOrCreateTracker(Factory, () => outerTracker));
				WorkflowTriggerActionTracker.OnAllActionsRun(Factory);
				AssertEquals(true, outerTracker.HasOnAllActionsRun);
			}

			AssertExceptionThrown<InvalidOperationException>("Out of scope", () => WorkflowTriggerActionTracker.GetOrCreateTracker(Factory, () => new TriggerActionTrackerForTest()));
		}

		sealed class TriggerActionTrackerForTest : ITriggerActionTracker
		{
			public void OnAllActionsRun()
			{
				HasOnAllActionsRun = true;
			}

			public bool HasOnAllActionsRun { get; private set; }
		}
	}
}
