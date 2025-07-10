using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(GlbStaffWrapper))]
	sealed class GlbStaffWrapperTest : MasterFiles.Business.Testing.GlbStaffWrapperTest<GlbStaffWrapper>
	{
		protected override GlbStaffWrapper CreateNewWrapper(GlbStaff staff) => GlbStaffWrapper.Get(staff);
	}
}
