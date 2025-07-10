using CargoWise.EntityFramework;
using Enterprise.Environment;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Packing.Module.Testing
{
	[TestedType(typeof(PackingModule))]
	internal class PackingModuleTest : ZModuleBasherTest
	{
		#region TestID

		public void TestID()
		{
			using (var module = (PackingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ModuleIDs.Packing, module.ID);
			}
		}

		#endregion

		#region TestAllowNew

		public void TestAllowNew()
		{
			using (var module = (PackingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		#endregion

		#region TestAllowDelete

		public void TestAllowDelete()
		{
			using (var module = (PackingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(false, module.AllowDelete);
			}
		}

		#endregion

		#region TestLicenseCheckPoint

		public void TestLicenseCheckPoint()
		{
			using (var module = (PackingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Licence.Packing, module.LicenceCheckPoint);
			}
		}

		#endregion

		#region TestSecurityCheckPoint

		public void TestSecurityCheckPoint()
		{
			using (var module = (PackingModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(Env.Security.Packing, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Packing;
		}

		protected override void AddTestObjects(IBusinessObjectCollection collection)
		{
			var packageJob = Factory.NewWithValidTestData<PkgPackageJob>();
			Factory.Save(); // collection is active, no need to do collection.Add(packageJob)

			base.AddTestObjects(collection);
		}

		#endregion
	}
}
