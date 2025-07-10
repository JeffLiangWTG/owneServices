using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(OutturnAndGateInOutController))]
	sealed class OutturnAndGateInOutControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ZAControllerIDs.OutturnAndGateInOut;
	}
}
