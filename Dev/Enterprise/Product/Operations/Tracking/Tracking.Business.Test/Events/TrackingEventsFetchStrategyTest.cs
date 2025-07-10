using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	sealed class TrackingEventsFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchHints_Description()
		{
			TestFetchHints(new TableColumn("Description", "Event.SE_Desc"));
		}

		public void TestFetchHints_EventDetails()
		{
			TestFetchHints(new TableColumn("Event Details", BaseStmALog.Schema.DisplayEventReference));
		}

		void TestFetchHints(TableColumn column)
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var event1 = dummy.Logs.AddNew();
			using (event1.LockForUpdatingKeyFieldsForTesting())
			{
				event1.SL_SE_NKEvent = AutoEvents.SecurityModifiedCode;
				event1.SL_EventTime = ZDateTime.Now.AddDays(-10);
			}

			var event2 = dummy.Logs.AddNew();
			using (event2.LockForUpdatingKeyFieldsForTesting())
			{
				event2.SL_SE_NKEvent = AutoEvents.EmailSentCode;
				event2.SL_EventTime = ZDateTime.Now.AddDays(-9);
			}

			var event3 = dummy.Logs.AddNew();
			using (event3.LockForUpdatingKeyFieldsForTesting())
			{
				event3.SL_SE_NKEvent = AutoEvents.ArrivalCode;
				event3.SL_EventTime = ZDateTime.Now;
			}

			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var events = new TrackingEventsCollection(newFactory);
			events.Load(new ZQuery(StmALogSchema.SL_Parent, dummy.PK));

			var strategy = new TrackingEventsFetchStrategy(events);
			strategy.FetchForView(events.ToArray(), new TableColumn[] { column });

			foreach (StmALog trackingEvent in events)
			{
				_ = trackingEvent.Event;
				_ = trackingEvent.DisplayEventReference;
			}

			var expectedDBHits = new Dictionary<string, int>()
			{
				{ AutoDummyBizo.Schema.TableName, 1 },
				{ AutoStmALog.Schema.TableName, 1 },
				{ AutoStmEvent.Schema.TableName, 1 },
			};
			AssertDbHits(expectedDBHits, newFactory);
		}

		public void TestFetchHInts_NoEventColumns()
		{
			var dummy = Factory.NewWithValidTestData<DummyEnterpriseBusinessObject>();
			var event1 = dummy.Logs.AddNew();
			using (event1.LockForUpdatingKeyFieldsForTesting())
			{
				event1.SL_SE_NKEvent = AutoEvents.SecurityModifiedCode;
				event1.SL_EventTime = ZDateTime.Now.AddDays(-10);
			}

			var newFactory = new BusinessObjectFactory();

			var events = new TrackingEventsCollection(newFactory);
			events.Load(new ZQuery(StmALogSchema.SL_Parent, dummy.PK));

			var initialFetchHintCount = newFactory.ActiveTableFetchHints;

			var strategy = new TrackingEventsFetchStrategy(events);
			strategy.FetchForView(events.ToArray(), System.Array.Empty<TableColumn>());

			AssertEquals(initialFetchHintCount, newFactory.ActiveTableFetchHints);
		}
	}
}
