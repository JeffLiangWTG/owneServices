using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Tracking.Business.Testing
{
	public abstract class BusinessObjectModifiedEventHelperTest : TestCaseWithFactory
	{
		public void TestWMREventIsAdded()
		{
			var dummyBizO = GetNewTestEventReferenceProvider();
			new BusinessObjectModifiedEventHelper(dummyBizO);
			dummyBizO.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.ModifiedOnWebCode);
			AssertEquals("WMR Event is added", 1, dummyBizO.GetLogs().Find(query).Length);
		}

		public void TestWMREventIsAdded_OncePerSave()
		{
			var dummyBizO = GetNewTestEventReferenceProvider();
			new BusinessObjectModifiedEventHelper(dummyBizO);
			dummyBizO.Factory.Save();

			dummyBizO.HasChanges = true;
			dummyBizO.Factory.Save();

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "WMR");
			var changelogs = dummyBizO.GetLogs().Find(query);

			AssertEquals("Should be 2 WMR logs", 2, changelogs.Length);
		}

		public void TestOnlyAddsLogWhenThereAreChanges()
		{
			var dummy = GetNewTestEventReferenceProvider();
			Factory.Save();

			// Forces db to actually do a save
			Factory.NewWithValidTestData<DummyBusinessObject>();

			var logsCount = dummy.GetLogs().DatabaseCount;

			Assert("PRE: No changes", !dummy.HasChanges);
			new BusinessObjectModifiedEventHelper(dummy);
			Factory.Save();

			AssertEquals("Shouldnt add WMR log when HasChanges is false. Last log: " + dummy.GetLogs().MostRecentLog?.SL_SE_NKEvent, logsCount, dummy.GetLogs().DatabaseCount);
		}

		#region Implementation

		protected abstract BusinessObject GetNewTestEventReferenceProvider();

		protected TestHelper Helper
		{
			get { return helper ?? (helper = new TestHelper(Factory)); }
		}
		TestHelper helper;

		#endregion
	}
}
