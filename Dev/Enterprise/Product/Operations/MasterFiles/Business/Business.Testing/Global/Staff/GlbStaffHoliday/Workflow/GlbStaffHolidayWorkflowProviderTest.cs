using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffHoliday))]
	class GlbStaffHolidayWorkflowProviderTest : WorkflowProviderTest<GlbStaffHoliday, GlbStaffHolidayProcessTasksCollection>
	{
		protected override ZString ExpectedWorkflowType => WorkflowDescriptors.GlbStaffHolidayDescriptorCode;
	}
}
