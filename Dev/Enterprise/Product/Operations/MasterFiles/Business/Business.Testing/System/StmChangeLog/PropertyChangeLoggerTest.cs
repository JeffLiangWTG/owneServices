using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class PropertyChangeLoggerTest : TestCaseWithFactory
	{
		public void TestGetInstance_DoesntCauseMemoryLeak()
		{
			CreateFactoryLoggerRef(out WeakReference factoryRef, out WeakReference loggerRef);
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			AssertEquals("Factory should be collected", false, factoryRef.IsAlive);
			AssertEquals("Logger should be collected", false, loggerRef.IsAlive);
		}

		public void CreateFactoryLoggerRef(out WeakReference factoryRef, out WeakReference loggerRef)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			PropertyChangeLogger logger = PropertyChangeLogger.GetInstance(factory);
			factoryRef = new WeakReference(factory);
			loggerRef = new WeakReference(logger);

			factory = null;
			logger = null;
		}

		public void TestLogChange_DefaultValuesNotLogged()
		{
			AssertEquals("Default", Dummy.Z0_Description);
			Dummy.Factory.Save();
			Dummy.FieldChangeLogs.Load();
			AssertEquals("No log created when no changes since setting default value", 0, Dummy.FieldChangeLogs.Count);
		}

		public void TestLogChange_ForAddedRecord()
		{
			Dummy.Z0_VarCharMax = "ModifiedTextValue";
			Dummy.Z0_Description = "ModifiedDescriptionValue";
			Factory.Save();

			Dummy.FieldChangeLogs.Load();
			AssertEquals(1, Dummy.FieldChangeLogs.Count);
			AssertEquals(2, Dummy.FieldChangeLogs[0].FieldChanges.Count);
			Dummy.FieldChangeLogs[0].FieldChanges.Sort("PropertyName");

			AssertEquals(DummyBizoSchema.Z0_Description.Name, Dummy.FieldChangeLogs[0].FieldChanges[0].PropertyName);
			AssertEquals("Default", Dummy.FieldChangeLogs[0].FieldChanges[0].OldValue);
			AssertEquals("ModifiedDescriptionValue", Dummy.FieldChangeLogs[0].FieldChanges[0].NewValue);

			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, Dummy.FieldChangeLogs[0].FieldChanges[1].PropertyName);
			AssertEquals("", Dummy.FieldChangeLogs[0].FieldChanges[1].OldValue);
			AssertEquals("ModifiedTextValue", Dummy.FieldChangeLogs[0].FieldChanges[1].NewValue);
		}

		public void TestDontHitDBOnInit()
		{
			var db = ((IDbConnected)Factory).Connection;
			var initialDBHit = db.ExecutedCommandCount;
			PropertyChangeLogger.GetInstance(Factory);
			AssertEquals(initialDBHit, db.ExecutedCommandCount);
		}

		public void TestLogChange_ForModifiedRecord()
		{
			Dummy.Z0_VarCharMax = "OriginalTextValue";
			Dummy.Z0_Description = "OriginalDescriptionValue";
			Dummy.Factory.Save();
			Dummy.Z0_VarCharMax = "ModifiedTextValue";
			Dummy.Z0_Description = "ModifiedDescriptionValue";
			Factory.Save();

			Dummy.FieldChangeLogs.Load();
			AssertEquals(2, Dummy.FieldChangeLogs.Count);
			AssertEquals(2, Dummy.FieldChangeLogs[1].FieldChanges.Count);

			AssertEquals(DummyBizoSchema.Z0_VarCharMax.Name, Dummy.FieldChangeLogs[1].FieldChanges[0].PropertyName);
			AssertEquals("OriginalTextValue", Dummy.FieldChangeLogs[1].FieldChanges[0].OldValue);
			AssertEquals("ModifiedTextValue", Dummy.FieldChangeLogs[1].FieldChanges[0].NewValue);

			AssertEquals(DummyBizoSchema.Z0_Description.Name, Dummy.FieldChangeLogs[1].FieldChanges[1].PropertyName);
			AssertEquals("OriginalDescriptionValue", Dummy.FieldChangeLogs[1].FieldChanges[1].OldValue);
			AssertEquals("ModifiedDescriptionValue", Dummy.FieldChangeLogs[1].FieldChanges[1].NewValue);
		}

		public void TestLogChange_UserFieldPopulated()
		{
			Dummy.Z0_VarCharMax = "OriginalTextValue";
			Factory.Save();
			Dummy.FieldChangeLogs.Load();
			AssertEquals("1 change log", 1, Dummy.FieldChangeLogs.Count);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, Dummy.FieldChangeLogs.OfType<StmChangeLog>().First().SY_GS_NKUser);
		}

		public void TestLogChange_DontWhenPropertyValueHasntActuallyChanged()
		{
			Dummy.Z0_VarCharMax = "OriginalTextValue";
			Factory.Save();
			Dummy.FieldChangeLogs.Load();
			AssertEquals("1 change log", 1, Dummy.FieldChangeLogs.Count);

			Dummy.Z0_VarCharMax = "ModifiedTextValue";
			Dummy.Z0_VarCharMax = "OriginalTextValue";
			Factory.Save();
			Dummy.FieldChangeLogs.Load();
			AssertEquals("No new change logs for unchanged field", 1, Dummy.FieldChangeLogs.Count);

			Dummy.Z0_VarCharMax = "ModifiedTextValue";
			Factory.Save();
			Dummy.FieldChangeLogs.Load();
			AssertEquals("Change log created when field value has actually changed", 2, Dummy.FieldChangeLogs.Count);
			AssertEquals("Change log created when field value has actually changed", 1, Dummy.FieldChangeLogs[0].FieldChanges.Count);
		}

		public void TestLogChange_WhenFactorySaveFails()
		{
			Dummy.Z0_VarCharMax = "OriginalTextValue";
			PerformFailingFactorySave();
			Dummy.FieldChangeLogs.Load();
			AssertEquals("No change logs created when factory save fails", 0, Dummy.FieldChangeLogs.Count);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			PropertyChangeSubscriptionList.SubscribedProperties.Add(DummyBizoSchema.Z0_VarCharMax.Name);
			PropertyChangeSubscriptionList.SubscribedProperties.Add(DummyBizoSchema.Z0_Description.Name);
		}

		PropertyChangeSubscriptionListForTest PropertyChangeSubscriptionList
		{
			get { return PropertyChangeSubscriptionListForTest.GetInstance(Factory); }
		}

		void PerformFailingFactorySave()
		{
			OrgHeader org = Factory.New<OrgHeader>();
			org.OH_Code = "abc";
			org.OH_ScreeningStatus = "abc";
			try
			{
				Factory.Save();
				Fail("Should not reach this point because org headers has invalid column. That org has no relevance to this test other than to ensure saving will fail.");
			}
			catch (ZSaveException)
			{
			}
			org.Delete();
		}

		DummyWithChangeLogging Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithChangeLogging>()); }
		}
		DummyWithChangeLogging dummy;

		#endregion
	}
}
