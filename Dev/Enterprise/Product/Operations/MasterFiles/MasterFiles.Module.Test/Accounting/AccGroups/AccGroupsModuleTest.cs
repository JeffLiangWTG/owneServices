using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(AccGroupsModule))]
	sealed class AccGroupsModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AccGroups;
		}

		protected override string CountryCode
		{
			get { return "AU"; }
		}

		public void TestCheckpoints()
		{
			using (AccGroupsModule module = new AccGroupsModule())
			{
				AssertEquals("SecurityCheckpoint", Env.Security.AccountingGroups, module.SecurityCheckpoint);
				AssertEquals("LicenseCheckPoint", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#region Properties

		[RequiresSTA]
		public void TestGetNewFilterControl()
		{
			using (AccGroupsModuleForTest module = new AccGroupsModuleForTest())
			{
				IFilterControl filterControl = module.NewFilterControl;
				Assert("Invalid type", filterControl is AccGroupsFilterControl);
				filterControl.Dispose();
			}
		}

		public void TestGetNewGridCollection()
		{
			using (AccGroupsModuleForTest module = new AccGroupsModuleForTest())
			{
				IBusinessObjectCollection zonesCollection = module.NewGridCollection;
				Assert("Invalid type", zonesCollection is AccGroupsCollection);
			}
		}

		public void TestGetNewFilterBusinessObject()
		{
			using (AccGroupsModuleForTest module = new AccGroupsModuleForTest())
			{
				FilterBusinessObject filterBusinessObject = module.NewFilterBusinessObject;
				Assert("Invalid type", filterBusinessObject is AccGroupsFilterBusinessObject);
			}
		}

		#endregion
	}
}
