using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.Customs.US.Module.Testing
{
	abstract class USCFilterGridModuleTest : ZModuleBasherTest
	{
		public void TestLicenceCheckpoint()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals(ExpectedLicenceCheckpoint, module.LicenceCheckPoint);
			}
		}

		protected override bool HasController() => false;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;

		protected virtual LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.Core;
	}
}
