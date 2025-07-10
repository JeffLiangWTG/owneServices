using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MarketingManager.Module.Testing
{
	[TestedType(typeof(SalesProductModule))]
	public class SalesProductModuleTest : ZModuleBasherTest
	{
		#region ID

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.SalesProduct;
		}

		#endregion

		#region Allowed Actions

		public void TestAllowNew()
		{
			using (var module = new SalesProductModule())
			{
				AssertEquals(false, module.AllowNew);
			}
		}

		#endregion

		#region Security

		public void TestSecurityCheckpoint()
		{
			using (var module = new SalesProductModule())
			{
				AssertEquals(Env.Security.SalesProducts, module.SecurityCheckpoint);
			}
		}

		#endregion

		#region Licence

		public void LicenceCheckpoint()
		{
			using (var module = new SalesProductModule())
			{
				AssertEquals(Env.Licence.Core, module.LicenceCheckPoint);
			}
		}

		#endregion
	}
}
