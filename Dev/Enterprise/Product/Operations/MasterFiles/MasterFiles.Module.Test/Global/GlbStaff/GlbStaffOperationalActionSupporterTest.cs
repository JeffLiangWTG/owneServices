using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbStaffOperationalActionSupporter))]
	sealed class GlbStaffOperationalActionSupporterTest : OperationalActionSupporterTest<GlbStaffOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbStaff; }
		}
	}
}
