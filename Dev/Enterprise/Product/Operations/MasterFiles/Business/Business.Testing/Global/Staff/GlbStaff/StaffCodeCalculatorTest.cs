using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	class StaffCodeCalculatorTest : TestCaseWithFactory
	{
		public void TestSimple()
		{
			var calculator = new StaffCodeCalculator(Factory);
			AssertEquals("", calculator.GetMostAppropriateStaffInitial("Brett"));
			AssertEquals("BS", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			AssertEquals("BAS", calculator.GetMostAppropriateStaffInitial("Brett Anthony Shearer"));
			AssertEquals("BAS", calculator.GetMostAppropriateStaffInitial("Brett Anthony Shearer BLAH"));
		}

		public void TestMultiSpace()
		{
			var calculator = new StaffCodeCalculator(Factory);
			AssertEquals("BS", calculator.GetMostAppropriateStaffInitial("brett  shearer"));
		}

		public void TestMultiByte()
		{
			var calculator = new StaffCodeCalculator(Factory);

			AssertEquals("", calculator.GetMostAppropriateStaffInitial("\u96C5\u60E0"));
		}

		public void TestPartialMultiByte()
		{
			var calculator = new StaffCodeCalculator(Factory);
			AssertEquals("BS", calculator.GetMostAppropriateStaffInitial("\u96C5 Bob Smith"));
			AssertEquals("", calculator.GetMostAppropriateStaffInitial("\u96C5 Bob"));
		}

		public void TestCollisionWithMiddleInitial()
		{
			var calculator = new StaffCodeCalculator(Factory);
			AssertEquals("BAS", calculator.GetMostAppropriateStaffInitial("Brett Anthony Shearer"));
			AddStaff("BAS");
			AssertEquals("BS", calculator.GetMostAppropriateStaffInitial("Brett Anthony Shearer"));
		}

		public void TestCollision()
		{
			var calculator = new StaffCodeCalculator(Factory);
			AddStaff("BS");
			AssertEquals("BSH", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			AddStaff("BSH");
			AssertEquals("BSR", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			AddStaff("BSR");
			AssertEquals("BRS", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			AddStaff("BRS");
			AssertEquals("BTS", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			AddStaff("BTS");
			AssertEquals("BS1", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			AddStaff("BS1", "BS2", "BS3", "BS4", "BS5", "BS6", "BS7", "BS8", "BS9");
			AssertEquals("Jumps to numeric", "BS0", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			AddStaff("BS0");
			AssertEquals("Jumps to single character plus two digits", "B01", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
			for (int i = 1; i <= 100; i++)
			{
				AddStaff("B" + (i % 100).ToString().PadLeft(2, '0'));
			}
			AssertEquals("No codes left to allocate", "", calculator.GetMostAppropriateStaffInitial("Brett Shearer"));
		}

		public void TestGetNextAvailableUniqueCode()
		{
			AddStaff("AAA", "AAB", "AAC");
			Factory.Save();
			AddStaff("AAD"); // Object in local factory only

			StaffCodeCalculator calculator = new StaffCodeCalculator(Factory);
			AssertEquals("AAE", calculator.GetNextAvailableUniqueCode());
			AssertEquals("BDA", calculator.GetNextAvailableUniqueCode("BCZ"));
			AssertEquals("ZZ0", calculator.GetNextAvailableUniqueCode("ZZZ"));
			AssertEquals("AA1", calculator.GetNextAvailableUniqueCode("AA0"));
		}

		#region Implementation

		void AddStaff(params string[] staffCodes)
		{
			foreach (var staffCode in staffCodes)
			{
				var staff = Factory.New<GlbStaff>();
				staff.GS_Code = staffCode;
				staff.GS_LoginName = staffCode;
			}
		}

		#endregion
	}
}
