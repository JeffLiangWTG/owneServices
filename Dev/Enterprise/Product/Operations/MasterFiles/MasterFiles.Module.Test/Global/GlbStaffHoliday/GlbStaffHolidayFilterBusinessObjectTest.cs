using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffHolidayFilterBusinessObject))]
	sealed class GlbStaffHolidayFilterBusinessObjectTest : FilterStripBusinessObjectTestCase
	{
		protected override FilterStripBusinessObject GetNewFilterStripBusinessObject() => new GlbStaffHolidayFilterBusinessObject();
	}
}
