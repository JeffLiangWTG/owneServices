using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.Business.MessageBuilders;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.US.GUI.Testing
{
	[TestedType(typeof(ReconMessageSendingForm))]
	sealed class ReconMessageSendingFormTest : ZFormBasherTest
	{
		public void TestChangePaymentPanelVisibility()
		{
			using (var reconFormToDelete = new ReconMessageSendingForm(new ACEReconMessageSendingAction(ReconDecIReconciliation, Messaging.Business.UpdateActionCode.Delete)))
			{
				reconFormToDelete.Show();
				AssertEquals("PaymentPanel is NOT visible", false, reconFormToDelete.PaymentPanel.Visible);
			}

			ReconDecIReconciliation.ReconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.AwaitingReconOriginal;
			using (var reconFormToAdd = new ReconMessageSendingForm(new ACEReconMessageSendingAction(ReconDecIReconciliation, Messaging.Business.UpdateActionCode.Replace)))
			{
				reconFormToAdd.Show();
				AssertEquals("PaymentPanel is visible", true, reconFormToAdd.PaymentPanel.Visible);
			}

			ReconDecIReconciliation.ReconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			using (var reconFormToAdd = new ReconMessageSendingForm(new ACEReconMessageSendingAction(ReconDecIReconciliation, Messaging.Business.UpdateActionCode.Add)))
			{
				reconFormToAdd.Show();
				AssertEquals("PaymentPanel is visible", true, reconFormToAdd.PaymentPanel.Visible);
			}
		}

		protected override Form GetFormToBashCore() => new ReconMessageSendingForm(new ACEReconMessageSendingAction(ReconDecIReconciliation, Messaging.Business.UpdateActionCode.Add));

		ReconDeclarationIReconciliation reconDecIReconciliation;
		ReconDeclarationIReconciliation ReconDecIReconciliation
		{
			get
			{
				if (reconDecIReconciliation == null)
				{
					var dec = Factory.New<JobDeclaration>();
					dec.JE_MessageType = JobMessageTypeList.Codes.Recon;
					dec.JE_ApplicationCode = JobApplicationCodeList.Codes.ACE;
					var reconDec = new ReconDeclaration(dec);
					reconDecIReconciliation = new ReconDeclarationIReconciliation(reconDec);
				}

				return reconDecIReconciliation;
			}
		}
	}
}
