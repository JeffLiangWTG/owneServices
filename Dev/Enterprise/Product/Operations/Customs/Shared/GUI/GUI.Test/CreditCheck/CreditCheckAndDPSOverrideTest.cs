using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.CreditControl.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GUI.Testing
{
	sealed class CreditCheckAndDPSOverrideTest : TestCaseWithFactory
	{
		public void TestProcessOverride()
		{
			var declaration = Factory.NewWithValidTestData<MessageManageableJobDeclaration>();
			declaration.DPSFreightMovementRestricted = true;
			declaration.JE_OH_Importer = Factory.NewWithValidTestData<OrgHeader>().PK;

			var creditManager = new DocumentDeliveryCreditControlManager();
			_ = creditManager.GetDocumentDeliveryStatusForCreditManagement(declaration, "DPS Gui", ZGuid.Empty, false);
			var args = creditManager.GetSecurityLoginEventArgs();

			args.IsCustomsSubmission = true;

			var overrider = new CreditCheckAndDPSOverride();

			overrider.ProcessOverride(args, declaration, "DPS Caption");

			CombineAssertions(() =>
			{
				AssertContains("Message", "Organization which may be on the Denied Party Screening list", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertContains("Caption", "Denied Party Screening", UnitTestUserNotification.Instance.LastMessage.Caption);
			});
		}
	}
}
