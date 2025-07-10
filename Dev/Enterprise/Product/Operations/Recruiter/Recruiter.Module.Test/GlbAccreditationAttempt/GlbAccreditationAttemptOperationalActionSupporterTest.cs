using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(GlbAccreditationAttemptOperationalActionSupporter))]
	internal class GlbAccreditationAttemptOperationalActionSupporterTest : OperationalActionSupporterTest<GlbAccreditationAttemptOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.GlbAccreditationAttempt;
	}
}
