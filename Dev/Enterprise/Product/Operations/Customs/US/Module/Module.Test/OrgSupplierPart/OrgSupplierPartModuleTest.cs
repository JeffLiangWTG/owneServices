using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(OrgSupplierPartModule))]
	sealed class OrgSupplierPartModuleTest : Customs.Module.Testing.OrgSupplierPartModuleTest
	{
		public void TestLicenceCheckpoint()
		{
			using (var module = (ZFilterModule)ZModuleFactory.Instance.Create(GetModuleID()))
			{
				AssertEquals("This must be core so that both Broker And Warehouse Users Can Access it", Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedStates;
	}
}
