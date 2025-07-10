using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Documents.GUI.Actions;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.Documents.Testing.GUI.Actions
{
	sealed class CertificateOfOriginIndemnityTermsAgreementCheckerTests : TestCaseWithFactory
	{
		public void TestCheckTermAcknowledged()
		{
			var checker = new CertificateOfOriginIndemnityTermsAgreementChecker();

			using (var dummyForm = new ZForm(Factory.New<DummyBusinessObject>()))
			{
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;

				var result = checker.CheckTermAcknowledged(dummyForm).GetAwaiter().GetResult();
				AssertType<TermsAcknowledgementForm>(ZFormModaliser.LastFormShownDialogForTest);
			}
		}
	}
}
