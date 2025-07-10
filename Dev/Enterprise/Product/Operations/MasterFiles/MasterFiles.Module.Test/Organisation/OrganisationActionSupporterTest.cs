using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Organisation.Testing
{
	[TestedType(typeof(OrganisationActionSupporter))]
	sealed class OrganisationActionSupporterTest : OperationalActionSupporterTest<OrganisationActionSupporter>
	{
		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.Organisation; }
		}

		#endregion
	}
}
