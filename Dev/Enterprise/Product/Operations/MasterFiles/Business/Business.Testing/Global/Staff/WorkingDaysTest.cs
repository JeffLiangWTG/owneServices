using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Environment;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class WorkingDaysTest : TestCaseWithFactory
	{
		public void TestCache()
		{
			var calendarArithmetic1 = WorkingDays.GetInstance(Factory, Env.CurrentDepartment.PK, Env.CurrentBranch.PK, Env.CurrentUser.PK);
			var calendarArithmetic2 = WorkingDays.GetInstance(Factory, Env.CurrentDepartment.PK, Env.CurrentBranch.PK, Env.CurrentUser.PK);

			Assert(object.ReferenceEquals(calendarArithmetic1, calendarArithmetic2));

			var calendarArithmetic3 = WorkingDays.GetInstance(Factory, ZGuid.NewZGuid(), ZGuid.Empty);

			Assert(!object.ReferenceEquals(calendarArithmetic2, calendarArithmetic3));
		}

		public void TestCache_WhenUsingCreatorFunction_ShouldResetOnFactorySave()
		{
			var departmentPK = new ZGuid(Env.CurrentDepartmentPK);
			var branchPK = new ZGuid(Env.CurrentBranchPK);

			var creatorFunctionCalledCount = 0;

			WorkTimeArithmetic GetWorkTimeArithmetic()
			{
				creatorFunctionCalledCount++;

				return null;
			}

			void GetWorkingDaysInstance() => WorkingDays.GetInstance(Factory, departmentPK, branchPK, workingDaysCreator: GetWorkTimeArithmetic);

			AssertEquals(0, creatorFunctionCalledCount);

			GetWorkingDaysInstance();
			GetWorkingDaysInstance();

			AssertEquals(1, creatorFunctionCalledCount);

			Factory.Save();

			GetWorkingDaysInstance();
			GetWorkingDaysInstance();

			AssertEquals(2, creatorFunctionCalledCount);
		}
	}
}
