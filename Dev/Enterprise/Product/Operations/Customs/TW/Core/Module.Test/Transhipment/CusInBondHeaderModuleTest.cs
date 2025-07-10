using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Transhipment.Module.Test
{
	[TestedType(typeof(CusInBondHeaderModule))]
	internal sealed class CusInBondHeaderModuleTest : ZModuleBasherTest
	{
		public void TestLicenceAndSecurityCheckPoint()
		{
			using (CusInBondHeaderModule module = new CusInBondHeaderModule())
			{
				AssertEquals("Licence Checkpoint", Env.Licence.ImportBroker, module.LicenceCheckPoint);
				AssertEquals("SecurityCheckpoint", Env.Security.TWTranshipment, module.SecurityCheckpoint);
			}
		}

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.TW.Transhipment;
		}

		protected override string CountryCode => Core.Constants.CountryCodes.Taiwan;

		public void TestCusInBondHeaderModuleAllows()
		{
			using (CusInBondHeaderModule module = new CusInBondHeaderModule())
			{
				AssertEquals("module.AllowNew", true, module.AllowNew);
				AssertEquals("module.AllowEdit", true, module.AllowEdit);
				AssertEquals("module.AllowDelete", true, module.AllowDelete);
			}
		}

		protected override bool HasController()
		{
			return true;
		}
	}
}
