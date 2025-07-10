using Enterprise.Environment;
using Enterprise.Licensing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.Module.Testing
{
	[TestedType(typeof(USCTeamSpecialistModule))]
	sealed class USCTeamSpecialistModuleTest : USCFilterGridModuleTest
	{
		protected override ModuleIdentifier GetModuleID() => ModuleIDs.Customs.US.TeamSpecialist;

		protected override LicenceCheckpoint ExpectedLicenceCheckpoint => Env.Licence.ImportBroker;
	}
}
