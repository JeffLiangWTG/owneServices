using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NZ.Module.Declaration
{
	[TestedType(typeof(ECIWriteOffManifestingModule))]
	sealed class ECIWriteOffManifestingModuleTest : ZModuleBasherTest
	{
		protected override string CountryCode => Core.Constants.CountryCodes.NewZealand;

		protected override ModuleIdentifier GetModuleID()
		{
			return ModuleIDs.Customs.NZ.ECIWriteOffManifesting;
		}
	}
}
