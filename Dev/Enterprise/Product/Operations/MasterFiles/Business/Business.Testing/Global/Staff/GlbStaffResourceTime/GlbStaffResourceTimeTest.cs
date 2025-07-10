using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffResourceTime))]
	public abstract class GlbStaffResourceTimeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestTypeDescription()
		{
			GlbStaffResourceTime bizo = (GlbStaffResourceTime)GetNewBusinessObject();
			bizo.GA_WorkHolidayType = bizo.Lookups.Types[0].Code;
			AssertEquals(bizo.Lookups.Types[0].Description, bizo.TypeDescription);
		}
	}
}
