using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Module.Testing
{
	[TestedType(typeof(JobDeclarationModule))]
	sealed class CusAddInfoModuleTests : ZModuleBasherTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.JobDeclaration;

		protected override string CountryCode => Core.Constants.CountryCodes.UnitedKingdom;
	}
}
