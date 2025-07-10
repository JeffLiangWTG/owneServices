using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Customs.Module.Testing
{
	sealed class SendDiagnosticMessageControllerTest : TestCaseWithFactory
	{
		public void TestGetDisplayModeForNew()
		{
			var testSendDiagnosticMessageController = new SendDiagnosticMessageControllerForTest();
			var displayMode = testSendDiagnosticMessageController.GetDisplayModeForNewForTest();
			AssertEquals(ODisplayMode.Browse, displayMode);
		}

		sealed class SendDiagnosticMessageControllerForTest : SendDiagnosticMessageController
		{
			protected override DiagnosticConsol GetNewBusinessEntity(BusinessObjectFactory factory) => null;

			public override ModuleIdentifier ModuleID => null;

			public ODisplayMode GetDisplayModeForNewForTest() => GetDisplayModeForNew();
		}
	}
}
