using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestsSubclassesOf(typeof(OrderTrackingDatesMap))]
	abstract class OrderTrackingDatesMapTest<T> : TestCaseWithFactory where T : OrderTrackingDatesMap
	{
		public abstract void TestDepartureActualDate();
		public abstract void TestArrivalActualDate();
		public abstract void TestCargoAvailableActualDate();
		public abstract void TestDeliveryCartageAdvisedActualDate();
		public abstract void TestDeliveryCartageCompleteFinalizedActualDate();
		public abstract void TestDepartureScheduledDate();
		public abstract void TestArrivalScheduledDate();
		public abstract void TestDeliveryCartageCompleteFinalizedScheduledDate();

		public void TestFindLatestEventLogTimes()
		{
			var dummy = Factory.New<DummyBizOWithAutoLogs>();

			var map = new OrderTrackingDatesMapForTesting(dummy);

			AssertEquals("no date found", ZDateTimeOffset.Empty, map.FindLatestEventLogTime(Events.ArrivalCode));

			var arvLog = dummy.Logs.AddNew();
			using (arvLog.LockForUpdatingKeyFieldsForTesting())
			{
				arvLog.SL_SE_NKEvent = Events.ArrivalCode;
				arvLog.SL_EventTime = new ZDateTime(2013, 1, 1, 9, 0, 0);
			}

			var clrLog = dummy.Logs.AddNew();
			using (clrLog.LockForUpdatingKeyFieldsForTesting())
			{
				clrLog.SL_SE_NKEvent = Events.CustomsClearedCode;
				clrLog.SL_EventTime = new ZDateTime(2013, 1, 1, 9, 1, 0);
			}

			var cccLog = dummy.Logs.AddNew();
			using (cccLog.LockForUpdatingKeyFieldsForTesting())
			{
				cccLog.SL_SE_NKEvent = Events.CustomsCommencedCode;
				cccLog.SL_EventTime = new ZDateTime(2013, 1, 1, 9, 2, 0);
			}

			var eccLog1 = dummy.Logs.AddNew();
			using (eccLog1.LockForUpdatingKeyFieldsForTesting())
			{
				eccLog1.SL_SE_NKEvent = Events.ExportCustomsClearedCode;
				eccLog1.SL_EventTime = new ZDateTime(2013, 1, 1, 9, 3, 0);
				eccLog1.SL_Reference = "USCHI";
			}

			var eccLog2 = dummy.Logs.AddNew();
			using (eccLog2.LockForUpdatingKeyFieldsForTesting())
			{
				eccLog2.SL_SE_NKEvent = Events.ExportCustomsClearedCode;
				eccLog2.SL_EventTime = new ZDateTime(2013, 1, 1, 9, 4, 0);
				eccLog2.SL_Reference = "AUSYD";
			}

			var ecmLog1 = dummy.Logs.AddNew();
			using (ecmLog1.LockForUpdatingKeyFieldsForTesting())
			{
				ecmLog1.SL_SE_NKEvent = Events.ExportCustomsCommencedCode;
				ecmLog1.SL_EventTime = new ZDateTime(2013, 1, 1, 9, 5, 0);
			}

			var ecmLog2 = dummy.Logs.AddNew();
			using (ecmLog2.LockForUpdatingKeyFieldsForTesting())
			{
				ecmLog2.SL_SE_NKEvent = Events.ExportCustomsCommencedCode;
				ecmLog2.SL_EventTime = new ZDateTime(2013, 1, 1, 9, 6, 0);
			}

			AssertEquals("no log found for AAA event", ZDateTimeOffset.Empty,
				map.FindLatestEventLogTime("AAA"));

			AssertEquals("log found for ARV event", arvLog.EventTimeOffset,
				map.FindLatestEventLogTime(Events.ArrivalCode));

			AssertEquals("log found for CLR event", clrLog.EventTimeOffset,
				map.FindLatestEventLogTime(Events.CustomsClearedCode));

			AssertEquals("log found for CCC event", cccLog.EventTimeOffset,
				map.FindLatestEventLogTime(Events.CustomsCommencedCode));

			AssertEquals("log found for ECC event (matching branch country)", eccLog2.EventTimeOffset,
				map.FindLatestEventLogTime(Events.ExportCustomsClearedCode));

			AssertEquals("no log found for ECM event (without country reference)", ecmLog2.EventTimeOffset,
				map.FindLatestEventLogTime(Events.ExportCustomsCommencedCode));
		}

		public class OrderTrackingDatesMapForTesting : OrderTrackingDatesMap
		{
			public OrderTrackingDatesMapForTesting(BusinessObject businessObject)
				: base(businessObject)
			{
			}

			public override ZDateTime DepartureActualDate
			{
				get { throw new NotImplementedException(); }
			}

			public override ZDateTime ArrivalActualDate
			{
				get { throw new NotImplementedException(); }
			}

			public override ZDateTime CargoAvailableActualDate
			{
				get { throw new NotImplementedException(); }
			}

			public override ZDateTime DeliveryCartageAdvisedActualDate
			{
				get { throw new NotImplementedException(); }
			}

			public override ZDateTime DeliveryCartageCompleteFinalizedActualDate
			{
				get { throw new NotImplementedException(); }
			}

			public override ZDateTime DepartureScheduledDate
			{
				get { throw new NotImplementedException(); }
			}

			public override ZDateTime ArrivalScheduledDate
			{
				get { throw new NotImplementedException(); }
			}

			public override ZDateTime DeliveryCartageCompleteFinalizedScheduledDate
			{
				get { throw new NotImplementedException(); }
			}
		}
	}
}
