using Enterprise.Environment;
using Enterprise.Security;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(GlbPersonOperationalActionSupporter))]
	sealed class GlbPersonOperationalActionSupporterTest : OperationalActionSupporterTest<GlbPersonOperationalActionSupporter>
	{
		protected override SecurityCheckpoint ExpectedCustomizationSecurityCheckpoint
		{
			get { return Env.Security.PersonIntelligenceCustomiseActions; }
		}
		protected override SecurityCheckpoint ExpectedRunSecurityCheckpoint
		{
			get { return Env.Security.PersonIntelligenceRunActions; }
		}
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.GlbPerson; }
		}
	}
}
