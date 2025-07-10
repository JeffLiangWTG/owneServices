#if DEBUG

using System;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.MasterFiles.Business
{
	public class DepartmentScheduleTestHelper : IDepartmentScheduleTestHelper
	{
		public IDisposable GetTestingUserContextWithDepartmentSchedule(string workingHours)
		{
			var factory = new BusinessObjectFactory();
			var staff = factory.NewWithValidTestData<GlbStaff>();
			var branch = factory.NewWithValidTestData<GlbBranch>();
			var department = factory.NewWithValidTestData<GlbDepartment>();
			factory.Save();

			var context = EnvProxy.Instance.SetTemporaryUserContext(staff.GS_LoginName, branch.PK.ToGuid(), department.PK.ToGuid());

			department.WorkTimes.MondayWorkingHours = workingHours;
			department.WorkTimes.TuesdayWorkingHours = workingHours;
			department.WorkTimes.WednesdayWorkingHours = workingHours;
			department.WorkTimes.ThursdayWorkingHours = workingHours;
			department.WorkTimes.FridayWorkingHours = workingHours;
			factory.Save();

			return context;
		}
	}
}

#endif