using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.SG.V4.Module.Testing
{
	[TestedType(typeof(ClassificationModule))]
	sealed class ClassificationModuleTest : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.SG.SG4Classification;

		protected override string CountryCode => Core.Constants.CountryCodes.Singapore;
	}
}
