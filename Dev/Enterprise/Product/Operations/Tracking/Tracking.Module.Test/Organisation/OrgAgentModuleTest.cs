using Enterprise.ZArchitecture.Web.Modules;
using NUnit.Framework;

namespace Enterprise.Tracking.Module.Testing
{
	[TestedType(typeof(OrgAgentModule))]
	sealed class OrgAgentModuleTest : OrganisationModuleTest
	{
		#region Setup

		protected override void SetUp()
		{
			base.SetUp();
			Factory.Save();
		}

		#endregion Setup

		#region Overrides

		protected override WebModuleID TestID
		{
			get { return WebModuleIDs.OrgAgentTracking; }
		}

		#endregion
	}
}
