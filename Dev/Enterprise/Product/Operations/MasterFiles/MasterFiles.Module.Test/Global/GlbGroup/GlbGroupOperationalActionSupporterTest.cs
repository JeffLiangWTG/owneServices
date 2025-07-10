using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbGroupOperationalActionSupporter))]
	sealed class GlbGroupOperationalActionSupporterTest : OperationalActionSupporterTest<GlbGroupOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.GlbGroup;

		public override bool ShouldSupportDocuments => true;
	}
}
