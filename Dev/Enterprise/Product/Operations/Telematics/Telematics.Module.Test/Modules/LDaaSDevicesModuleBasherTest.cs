using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Telematics.Module.Test
{
	[TestedType(typeof(LDaaSDevicesModule))]
	public class LDaaSDevicesModuleBasherTest : ZModuleBasherTest
	{
		#region ZModuleBasherTest

		protected override ModuleIdentifier GetModuleID() => ModuleIDs.LDaaSDevices;

		#endregion
	}
}
