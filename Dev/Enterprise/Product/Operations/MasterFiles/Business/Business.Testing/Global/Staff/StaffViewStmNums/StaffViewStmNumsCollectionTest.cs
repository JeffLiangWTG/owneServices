using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(StaffViewStmNumsCollection))]
	sealed class StaffViewStmNumsCollectionTest : ActiveBusinessObjectCollectionTestCase<StaffViewStmNumsCollection>
	{
		public void TestRelationshipFilter()
		{
			var creationFactory = new BusinessObjectFactory();
			var staff = creationFactory.NewWithValidTestData<GlbStaff>();
			var stmNum1 = creationFactory.New<StaffViewStmNums>();
			stmNum1.SN_Type = "AAA";
			stmNum1.SN_Owner = staff.PK;

			var stmNum2 = creationFactory.New<StaffViewStmNums>();
			stmNum2.SN_Type = "BBB";
			stmNum2.SN_Owner = staff.PK;

			var stmNum3 = creationFactory.New<StaffViewStmNums>();
			stmNum3.SN_Type = "XXX";
			stmNum3.SN_Owner = ZGuid.NewZGuid();

			creationFactory.Save();

			staff = Factory.Load<GlbStaff>(staff.PK);
			var collection = new StaffViewStmNumsCollection(staff);

			AssertContainsExactElementsInAnyOrder(new ZString[] { "AAA", "BBB" }, collection.Select(v => v.SN_Type));
		}

		public void TestTryGetStmNums()
		{
			var creationFactory = new BusinessObjectFactory();
			var staff = creationFactory.NewWithValidTestData<GlbStaff>();

			var stmNum1 = creationFactory.New<StaffViewStmNums>();
			stmNum1.SN_Type = "AAA";
			stmNum1.SN_Prefix = "123";
			stmNum1.SN_Owner = staff.PK;

			var stmNum2 = creationFactory.New<StaffViewStmNums>();
			stmNum2.SN_Type = "BBB";
			stmNum2.SN_Prefix = "456";
			stmNum2.SN_Owner = staff.PK;

			var stmNum3 = creationFactory.New<StaffViewStmNums>();
			stmNum3.SN_Type = "XXX";
			stmNum3.SN_Prefix = "789";
			stmNum3.SN_Owner = ZGuid.NewZGuid();

			creationFactory.Save();

			staff = Factory.Load<GlbStaff>(staff.PK);

			AssertStmAreEquals(stmNum1, staff.Fountains.TryGetStmNums("AAA", "123"));
			AssertStmAreEquals(stmNum2, staff.Fountains.TryGetStmNums("BBB", "456"));
			AssertNull(staff.Fountains.TryGetStmNums("XXX", "789"));
			AssertNull(staff.Fountains.TryGetStmNums("AAA", "456"));

			void AssertStmAreEquals(StaffViewStmNums expectedStmNum, StaffViewStmNums actualStmNum)
			{
				CombineAssertions(() =>
				{
					AssertEquals("SN_Type", expectedStmNum.SN_Type, actualStmNum.SN_Type);
					AssertEquals("SN_Owner", expectedStmNum.SN_Owner, actualStmNum.SN_Owner);
					AssertEquals("SN_Owner", expectedStmNum.SN_Prefix, actualStmNum.SN_Prefix);
				});
			}
		}
		public GlbStaff Staff => staff ??= Factory.NewWithValidTestData<GlbStaff>();
		GlbStaff staff;

		protected override StaffViewStmNumsCollection GetCollectionToTest() => new StaffViewStmNumsCollection(Staff);

		protected override BusinessObject GetNewElementToAddToTheCollection() => Factory.New<StaffViewStmNums>();
	}
}
