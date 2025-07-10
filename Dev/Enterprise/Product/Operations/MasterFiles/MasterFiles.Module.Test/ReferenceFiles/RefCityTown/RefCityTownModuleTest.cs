using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(RefCityTownModule))]
	class RefCityTownModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.CityTown, Module.SecurityCheckpoint);
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.RefCityTown;
		}

		protected virtual RefCityTownModule Module
		{
			get
			{
				if (module == null)
				{
					module = (RefCityTownModule)ZModuleFactory.Instance.Create(ModuleIDs.RefCityTown);
				}
				return module;
			}
		}
		RefCityTownModule module;

		protected override void TearDown()
		{
			base.TearDown();
			if (module != null)
			{
				module.Dispose();
			}
		}

		#endregion
	}
}
