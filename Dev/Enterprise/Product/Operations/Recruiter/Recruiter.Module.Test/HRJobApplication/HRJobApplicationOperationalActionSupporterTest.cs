using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Recruiter.Module.Testing
{
	[TestedType(typeof(HRJobApplicationOperationalActionSupporter))]
	internal class HRJobApplicationOperationalActionSupporterTest : OperationalActionSupporterTest<HRJobApplicationOperationalActionSupporter>
	{
		protected override ModuleIdentifier ModuleID => ModuleIDs.HRJobApplication;
		public override bool ShouldSupportDocuments => false;
	}
}
