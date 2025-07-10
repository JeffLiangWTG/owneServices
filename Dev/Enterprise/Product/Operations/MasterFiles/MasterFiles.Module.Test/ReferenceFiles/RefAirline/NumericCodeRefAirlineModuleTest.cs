using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Module.Testing
{
	[TestedType(typeof(NumericCodeRefAirlineModule))]
	sealed class NumericCodeRefAirlineModuleTest : ZModuleBasherTest
	{
		public void TestGetNewController()
		{
			using (var module = new NumericCodeRefAirlineModuleForTest())
			{
				Assert("Invalid Controller type", module.Controller is NumericCodeRefAirlineController);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new NumericCodeRefAirlineModule())
			{
				Assert("Invalid GridCollection type", module.GridCollection is NumericCodeRefAirlineCollection);
			}
		}

		#region Implementation

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.NumericCodeRefAirline;

		#endregion
	}
}
