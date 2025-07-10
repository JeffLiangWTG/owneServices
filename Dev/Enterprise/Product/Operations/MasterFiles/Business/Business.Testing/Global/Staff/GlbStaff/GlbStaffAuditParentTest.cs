using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaff))]
	sealed class GlbStaffAuditParentTest : AuditParentTest<GlbStaff>
	{
		protected override GlbStaff NewTestAuditParent()
		{
			return Factory.New<GlbStaff>();
		}
	}
}
