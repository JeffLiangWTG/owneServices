using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StmChangeLog))]
	sealed class StmChangeLogTest : EnterpriseBusinessObjectTestCase
	{
		//[TestDate(2005, 1, 2)]
		public void TestSY_PostedTime()
		{
			Dummy.Factory.Save();
			Dummy.Z0_VarCharMax = "newValue";
			Dummy.Factory.Save();
			Dummy.FieldChangeLogs.Load();
			AssertEquals("SY_PostedTime set on insert", true, (Dummy.FieldChangeLogs[0].SY_PostedTimeUtc - ZDateTime.UtcNow).TotalSeconds < 2);
		}

		#region Related Business Objects

		public void TestParent_WhenTablePrefixUndefined()
		{
			BusinessObject parent = Factory.New(typeof(OrgHeader));
			StmChangeLogCollection changeLogs = new StmChangeLogCollection(parent);
			StmChangeLog changeLog = changeLogs.AddNew();
			changeLog.SY_ParentTableCode = "Z!";
			ErrorReporter.Clear();
			AssertNull("parent", changeLog.Parent);
			AssertEquals("Business object for TablePrefix 'Z!' is unknown", ErrorReporter.LastKeyReported);
			AssertEquals("Cannot determine the Busines object for TablePrefix 'Z!'", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestFieldChanges()
		{
			TestFieldChanges((ZString)"OldValue", (ZString)"NewValue");
			TestFieldChanges((ZInt)1, (ZInt)2);
			TestFieldChanges((ZDecimal)1, (ZDecimal)2);
			TestFieldChanges(ZDateTime.Empty, new ZDateTime(2005, 1, 2));
			TestFieldChanges(new ZDateTime(2005, 1, 2), ZDateTime.Empty);
			TestFieldChanges(ZGuid.Empty, new ZGuid("A9218F77-3B24-4ee4-815B-A775D7D08F1B"));
			TestFieldChanges(new ZGuid("16865BB2-DFF3-465f-9E92-531B14A85AD5"), ZGuid.Empty);
		}

		void TestFieldChanges(IZType newValue, IZType oldValue)
		{
			StmChangeLog changeLog = Dummy.FieldChangeLogs.AddNew();

			StmFieldChangeLog fieldChangeLog1 = changeLog.FieldChanges.AddNew();
			fieldChangeLog1.PropertyName = "PropertyName1";
			fieldChangeLog1.OldValue = oldValue;
			fieldChangeLog1.NewValue = newValue;

			StmFieldChangeLog fieldChangeLog2 = changeLog.FieldChanges.AddNew();
			fieldChangeLog2.PropertyName = "PropertyName2";
			fieldChangeLog2.OldValue = oldValue;
			fieldChangeLog2.NewValue = newValue;

			Factory.Save();
			StmChangeLog loadedChangeLog = Factory.Load<StmChangeLog>(changeLog.PK);
			StmFieldChangeLog loadedFieldChangeLog1 = loadedChangeLog.FieldChanges[0];
			StmFieldChangeLog loadedFieldChangeLog2 = loadedChangeLog.FieldChanges[1];

			AssertEquals("PropertyName", "PropertyName1", loadedFieldChangeLog1.PropertyName);
			AssertEquals("OldValue", oldValue, loadedFieldChangeLog1.OldValue);
			AssertEquals("NewValue", newValue, loadedFieldChangeLog1.NewValue);

			AssertEquals("PropertyName", "PropertyName2", loadedFieldChangeLog2.PropertyName);
			AssertEquals("OldValue", oldValue, loadedFieldChangeLog2.OldValue);
			AssertEquals("NewValue", newValue, loadedFieldChangeLog2.NewValue);
		}

		#endregion

		#region Implementation

		DummyWithChangeLogging Dummy
		{
			get { return dummy ?? (dummy = Factory.New<DummyWithChangeLogging>()); }
		}
		DummyWithChangeLogging dummy;

		PropertyChangeSubscriptionListForTest PropertyChangeSubscriptionList
		{
			get { return PropertyChangeSubscriptionListForTest.GetInstance(Factory); }
		}

		protected override void SetUp()
		{
			base.SetUp();
			PropertyChangeSubscriptionList.SubscribedProperties.Add(DummyBizoSchema.Z0_VarCharMax.Name);
			PropertyChangeSubscriptionList.SubscribedProperties.Add(DummyBizoSchema.Z0_Description.Name);
		}

		#endregion
	}
}
