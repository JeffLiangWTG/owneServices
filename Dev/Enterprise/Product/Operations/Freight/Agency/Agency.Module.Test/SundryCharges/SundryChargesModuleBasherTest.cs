using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Agency.Module.Testing
{
	[TestedType(typeof(SundryChargesModule))]
	internal class SundryChargesModuleBasherTest : ZModuleBasherTest
	{
		#region Implementation
		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.AgencySundryCharges;
		}
		#endregion
	}
}
