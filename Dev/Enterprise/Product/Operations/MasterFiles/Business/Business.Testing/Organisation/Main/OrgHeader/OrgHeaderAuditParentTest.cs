using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(OrgHeader))]
	sealed class OrgHeaderAuditParentTest : AuditParentTest<OrgHeader>
	{
		protected override OrgHeader NewTestAuditParent()
		{
			return Factory.New<OrgHeader>();
		}
	}
}
