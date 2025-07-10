using System;

namespace Enterprise.MasterFiles.Integration
{
	public interface IDepartmentScheduleTestHelper
	{
		IDisposable GetTestingUserContextWithDepartmentSchedule(string workingHours);
	}
}
