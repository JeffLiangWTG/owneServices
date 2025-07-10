using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test
{
	[TestedType(typeof(TelematicsPreDriveChecklistTemplatesModule))]
	class TelematicsPreDriveChecklistTemplatesModuleBasherTest : ZModuleBasherTest
	{
		#region ZModuleBasherTest

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.TelematicsPreDriveChecklistTemplates;

		#endregion
	}
}
