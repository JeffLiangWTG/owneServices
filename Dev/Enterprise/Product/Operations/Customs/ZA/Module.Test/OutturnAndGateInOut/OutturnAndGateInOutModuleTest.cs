using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(OutturnAndGateInOutModule))]
	sealed class OutturnAndGateInOutModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ZAModuleIDs.OutturnAndGateInOut;

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;
	}
}
