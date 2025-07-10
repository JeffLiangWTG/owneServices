using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Business.Testing
{
	public abstract class ViewStmNumsValidationTest : BusinessObjectValidationTestCase
	{
		public void TestCheckSN_IDIsNotEmpty()
		{
			AssertEquals("StmNums.SN_ID", ZInt.Zero, StmNums.SN_ID);
			AssertNoNotifications(StmNums.SN_IDInfo);
		}

		public void TestSN_MinimumValue()
		{
			StmNums.SN_MinimumValue = 1;
			AssertNoErrors(StmNums.SN_MinimumValueInfo);

			StmNums.SN_MinimumValue = ViewStmNums.Schema.DefaultFountainMaximumValue;
			AssertNoErrors(StmNums.SN_MinimumValueInfo);

			StmNums.SN_MinimumValue = 0;
			AssertHasErrors(StmNums.SN_MinimumValueInfo);

			StmNums.SN_MinimumValue = ViewStmNums.Schema.DefaultFountainMaximumValue + 1;
			AssertHasErrors(StmNums.SN_MinimumValueInfo);
		}

		public void TestSN_MaximumValue()
		{
			StmNums.SN_MinimumValue = 1000;
			StmNums.SN_MaximumValue = 2000;
			AssertNoErrors(StmNums.SN_MaximumValueInfo);

			StmNums.SN_MaximumValue = 1000;
			AssertNoErrors(StmNums.SN_MaximumValueInfo);

			StmNums.SN_MaximumValue = 999;

			var message = "Range End must be greater than or equal to the Range Start, please change the Range Start or Count.";
			AssertHasError(StmNums.SN_MaximumValueInfo, message);

			StmNums.SN_MaximumValue = 1500;
			AssertNoErrors(StmNums.SN_MaximumValueInfo);

			StmNums.SN_MaximumValue = StmNums.DefaultTypeRangeMax + 1;

			message = string.Format("Range End must be less than or equal to {0}, please change the Range Start or Count.", StmNums.DefaultTypeRangeMax);
			AssertHasError(StmNums.SN_MaximumValueInfo, message);

			StmNums.SN_MaximumValue = StmNums.DefaultTypeRangeMax;
			AssertNoErrors(StmNums.SN_MaximumValueInfo);

			StmNums.SN_MinimumValue = StmNums.DefaultTypeRangeMax;
			StmNums.SN_MaximumValue = StmNums.DefaultTypeRangeMax - 1;

			message = "Range End must be greater than or equal to the Range Start, please change the Range Start or Count.";
			AssertHasError(StmNums.SN_MaximumValueInfo, message);

			StmNums.SN_MaximumValue = StmNums.DefaultTypeRangeMax;
			AssertNoErrors(StmNums.SN_MaximumValueInfo);
		}

		protected ViewStmNums StmNums
		{
			get { return stmNums ?? (stmNums = GetNewViewStmNums()); }
		}
		ViewStmNums stmNums;

		protected abstract ViewStmNums GetNewViewStmNums();

		protected BusinessObject Owner
		{
			get { return owner ?? (owner = GetNewOwner()); }
		}
		BusinessObject owner;

		protected abstract BusinessObject GetNewOwner();
	}
}
