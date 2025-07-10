using Enterprise.Customs.US.InBond.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.InBond.Module.Testing
{
	[TestedType(typeof(ThreeLetterRefAirlineModule))]
	sealed class ThreeLetterRefAirlineModuleTest : ZModuleBasherTest
	{
		public void TestGetNewController()
		{
			using (var module = new ThreeLetterRefAirlineModuleForTest())
			{
				Assert("Invalid Controller type", module.Controller is ThreeLetterRefAirlineController);
			}
		}

		public void TestGetNewGridCollection()
		{
			using (var module = new ThreeLetterRefAirlineModule())
			{
				Assert("Invalid GridCollection type", module.GridCollection is ThreeLetterRefAirlineCollection);
			}
		}

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.ThreeLetterRefAirline;
		sealed class ThreeLetterRefAirlineModuleForTest : ThreeLetterRefAirlineModule
		{
			public ThreeLetterRefAirlineModuleForTest()
			{
			}

			public ZController Controller => GetNewController(null);
		}
	}
}
