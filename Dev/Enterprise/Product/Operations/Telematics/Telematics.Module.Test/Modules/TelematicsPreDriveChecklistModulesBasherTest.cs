using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test.Modules
{
	[TestedType(typeof(TelematicsPreDriveChecklistModule))]
	class TelematicsPreDriveChecklistModuleBasherTest : ZModuleBasherTest
	{
		#region ZModuleBasherTest

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.TelematicsPreDriveChecklists;

		#endregion
	}
}
