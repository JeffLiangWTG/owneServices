using CargoWise.Definitions;
using Enterprise.Environment;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Organisation.Testing
{
	[TestedType(typeof(OrgContactsActionSupporter))]
	sealed class OrgContactsActionSupporterTest : OperationalActionSupporterTest<OrgContactsActionSupporter>
	{
		public void TestBusinessContext()
		{
			var supporter = new OrgContactsActionSupporter();

			AssertEquals(BusinessContext.OrganisationContacts, supporter.BusinessContext);
			AssertEquals(Env.Security.FindOrCreateOperationalActionsRunCheckpoint(Env.Security.OrgContact), supporter.RunSecurityCheckpoint);
			AssertEquals(Env.Security.FindOrCreateOperationalActionsCustomiseCheckpoint(Env.Security.OrgContact), supporter.CustomizationSecurityCheckpoint);
		}

		#region Implementation
		protected override ModuleIdentifier ModuleID
		{
			get { return ModuleIDs.OrgContacts; }
		}

		public override bool ShouldSupportDocuments
		{
			get { return false; }
		}

		#endregion
	}
}
