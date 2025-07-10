using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CustomsResponseModule))]
	sealed class CustomsResponseModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ZAModuleIDs.CustomsResponse;

		protected override string CountryCode => Core.Constants.CountryCodes.SouthAfrica;
	}
}
