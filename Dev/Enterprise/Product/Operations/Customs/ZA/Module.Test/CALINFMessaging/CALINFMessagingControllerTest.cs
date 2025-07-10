using Enterprise.Customs.ZA.ModuleRegistration;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ZA.Module.Testing
{
	[TestedType(typeof(CALINFMessagingController))]
	sealed class CALINFMessagingControllerTest : ZControllerBasherTest
	{
		protected override ControllerID GetControllerID() => ZAControllerIDs.CALINFMessagingPlugin;
	}
}
