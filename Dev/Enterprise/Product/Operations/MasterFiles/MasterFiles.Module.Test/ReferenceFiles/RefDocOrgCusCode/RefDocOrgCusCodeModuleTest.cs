using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefDocOrgCusCodeModule))]
	sealed class RefDocOrgCusCodeModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.RefDocOrgCusCode;

		public void TestAllowNew()
		{
			using (var module = new RefDocOrgCusCodeModuleForTest())
			{
				Assert("Document Organisation Mapping Module does not support adding new", !module.AllowNew);
			}
		}

		public void TestAllowEdit()
		{
			using (var module = new RefDocOrgCusCodeModuleForTest())
			{
				Assert("Document Organisation Mapping Module does not support editing", !module.AllowEdit);
			}
		}

		public void TestAllowDelete()
		{
			using (var module = new RefDocOrgCusCodeModuleForTest())
			{
				Assert("Document Organisation Mapping Module does not support deleting", !module.AllowDelete);
			}
		}

		public void TestSecurityCheckpoint()
		{
			using (var module = new RefDocOrgCusCodeModuleForTest())
			{
				AssertEquals("Document Organisation Mapping Module has no security checkpoint", module.SecurityCheckpoint, Env.Security.None);
			}
		}

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (var module = new RefDocOrgCusCodeModuleForTest())
			{
				var filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is RefDocOrgCusCodeFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new RefDocOrgCusCodeModuleForTest())
			{
				var countriesCollection = module.NewGridCollection;
				Assert("Invalid type", countriesCollection is RefDocOrgCusCodeCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (var module = new RefDocOrgCusCodeModuleForTest())
			{
				var filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is RefDocOrgCusCodeFilterBusinessObject);
			}
		}
	}
}
