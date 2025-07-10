using System;
using System.Threading;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AccPeriodManagement))]
	sealed class AccPeriodManagementTest : EnterpriseBusinessObjectTestCase
	{
		public override void TestSaveAndDeleteBusinessObject()
		{
			Assert("This should be implemented if a client has an issue with deleting", true);
		}

		public void TestReverseJournalPK()
		{
			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_StartDate = new ZDateTime(2009, 07, 01);
			period.AM_EndDate = new ZDateTime(2009, 07, 31);
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			period.AM_IsSubledgerClosedForAdjustments = true;

			var newID = Guid.NewGuid();
			period.ReverseJournalPK = newID;
			Factory.Save();

			AssertEquals(period.ReverseJournalPK, newID);
		}

		public void TestNoStmALogs()
		{
			var period = Factory.NewWithValidTestData<AccPeriodManagement>();
			Factory.Save();
			CombineAssertions(() =>
			{
				var query = new ZQuery(StmALogSchema.SL_Parent, period.PK);
				AssertEquals("Not expecting Add event.", 0, Factory.Load<StmALog>(query).Length);
				period.AM_Year = 2024;
				Factory.Save();
				AssertEquals("Not expecting Edit event", 0, Factory.Load<StmALog>(query).Length);
				period.Delete();
				Factory.Save();
				AssertEquals("Not expecting Delete event", 0, Factory.Load<StmALog>(query).Length);
			});
		}

		public void TestOnFactorySaving()
		{
			AccPeriodManagement period = Factory.NewWithValidTestData<AccPeriodManagement>();
			period.AM_StartDate = new ZDateTime(2009, 07, 01);
			period.AM_EndDate = new ZDateTime(2009, 07, 31);
			period.AM_IsGeneralLedgerClosed = true;
			period.AM_IsSubLedgerClosed = true;
			period.AM_IsSubledgerClosedForAdjustments = true;
			Factory.Save();
			var firstSaveTime = ZDateTime.Now;
			var allLogs = period.Logs.GetAllLogs();
			AssertEquals(3, allLogs.Count);
			AssertLog(allLogs, Events.PeriodClosed.Code, "General Ledger Closed");
			AssertLog(allLogs, Events.PeriodClosed.Code, "Sub Ledger Closed");
			AssertLog(allLogs, Events.PeriodClosed.Code, "Sub Ledger for Adjustments Closed");

			Thread.Sleep(1);

			period.AM_StartDate = new ZDateTime(2009, 07, 02);
			period.AM_EndDate = new ZDateTime(2009, 07, 30);
			period.AM_IsGeneralLedgerClosed = false;
			period.AM_IsSubLedgerClosed = false;
			period.AM_IsSubledgerClosedForAdjustments = false;
			Factory.Save();
			allLogs = period.Logs.GetAllLogs();
			AssertEquals(8, allLogs.Count);
			var newestLogs = new StmALogCollection(Factory);
			newestLogs.AddRange(allLogs.Find(new ZQuery(StmALogSchema.SL_EventTime, SQLComparisonOperator.GreaterThan, firstSaveTime)));
			AssertEquals(5, newestLogs.Count);
			AssertLog(newestLogs, Events.PeriodDateChanged.Code, "Start Date changed from 01-Jul-09 to 02-Jul-09");
			AssertLog(newestLogs, Events.PeriodDateChanged.Code, "End Date changed from 31-Jul-09 to 30-Jul-09");
			AssertLog(newestLogs, Events.PeriodReopened.Code, "General Ledger Reopened");
			AssertLog(newestLogs, Events.PeriodReopened.Code, "Sub Ledger Reopened");
			AssertLog(newestLogs, Events.PeriodReopened.Code, "General Ledger for Presentation Adjustments Reopened");
		}

		void AssertLog(BusinessObjectCollection logs, ZString eventCode, ZString reference)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, eventCode);
			query.AddToFilter(StmALogSchema.SL_Reference, reference);
			AssertCollectionContains(query, logs);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			return Factory.New(typeof(AccPeriodManagement));
		}
	}
}
