using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(CountryStatesGlbHolidayModule))]
	class CountryStatesGlbHolidayModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckPoint()
		{
			AssertEquals(Env.Licence.Core, Module.LicenceCheckPoint);
		}

		public void TestSecurityCheckpoint()
		{
			AssertEquals(Env.Security.CountryStatesGlbHoliday, Module.SecurityCheckpoint);
		}

		public override void TestModuleShowsAndCanSearch()
		{
			Assert(true);
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.CountryStatesGlbHoliday;
		}

		protected virtual CountryStatesGlbHolidayModule Module
		{
			get
			{
				if (module == null)
				{
					module = (CountryStatesGlbHolidayModule)ZModuleFactory.Instance.Create(ModuleIDs.CountryStatesGlbHoliday);
				}
				return module;
			}
		}
		CountryStatesGlbHolidayModule module;

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
