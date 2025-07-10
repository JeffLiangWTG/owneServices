using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(OrgAgentRelationshipModule))]
	sealed class OrgAgentRelationshipModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.ProfitShare;
		}

		public void TestCheckpoints()
		{
			using (OrgAgentRelationshipModule module = new OrgAgentRelationshipModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.ProfitShare, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		public void TestModuleID()
		{
			using (var module = new OrgAgentRelationshipModuleForTest())
			{
				AssertEquals(ModuleIDs.ProfitShare, module.ID);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new OrgAgentRelationshipModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is OrgAgentRelationshipFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new OrgAgentRelationshipModuleForTest())
			{
				Assert("Invalid type", module.NewGridCollection is OrgAgentRelationshipCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new OrgAgentRelationshipModuleForTest())
			{
				Assert("Invalid type", module.NewFilterBusinessObject is OrgAgentRelationshipFilterBusinessObject);
			}
		}

		public void TestGetNewController()
		{
			using (var module = new OrgAgentRelationshipModuleForTest())
			{
				Assert("Invalid type", module.NewController is OrgAgentRelationshipController);
			}
		}
	}
}
