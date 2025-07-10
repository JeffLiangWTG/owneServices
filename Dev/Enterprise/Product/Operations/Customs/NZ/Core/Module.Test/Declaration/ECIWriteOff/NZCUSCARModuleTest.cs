using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration.ECIWriteOff.Testing
{
	[TestedType(typeof(NZCUSCARModule))]
	sealed class NZCUSCARModuleTest : ZModuleBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.NewZealand;

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.NZ.CUSCAR;
	}
}
