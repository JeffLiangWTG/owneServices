using Enterprise.Environment;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(VoyageAccountingModule))]
	internal class VoyageAccountingModuleBasherTest : ZModuleBasherTest
	{
		public void TestLicense()
		{
			using (ZModule module = ZModuleFactory.Instance.Create(ModuleIDs.AgencyVoyageAccounting))
			{
				AssertEquals(Env.Licence.ShippingManagerVoyageAccounting, module.LicenceCheckPoint);
			}
		}

		#region Implementation
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AgencyVoyageAccounting;
		}
		#endregion
	}
}
