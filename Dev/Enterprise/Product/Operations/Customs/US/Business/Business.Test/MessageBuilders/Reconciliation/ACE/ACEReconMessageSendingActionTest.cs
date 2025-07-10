using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.US.Messaging.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.MessageBuilders.Testing
{
	[TestedType(typeof(ACEReconMessageSendingAction))]
	sealed class ACEReconMessageSendingActionTest : NonPersistentBusinessObjectTestCase
	{
		public void TestIsActionForReplaceOrAdd()
		{
			var recon = new ACEReconMessageSendingAction(ReconDecIReconciliation, Messaging.Business.UpdateActionCode.Replace);
			Assert(recon.IsActionForReplaceOrAdd);

			recon = new ACEReconMessageSendingAction(ReconDecIReconciliation, Messaging.Business.UpdateActionCode.Add);
			Assert(recon.IsActionForReplaceOrAdd);

			recon = new ACEReconMessageSendingAction(ReconDecIReconciliation, Messaging.Business.UpdateActionCode.Delete);
			Assert(!recon.IsActionForReplaceOrAdd);
		}

		public void TestValidateCertificationSignature()
		{
			var action = new ACEReconMessageSendingAction(ReconDecIReconciliation, UpdateActionCode.Add);
			action.ValidateCertificationSignature();
			var messageText = string.Format(ACEReconMessageSendingAction.ShouldBeSigned, "");
			AssertHasMessageErrorContaining(action.CertificationSignatureInfo, messageText);

			action.CertificationSignature = true;
			AssertNoMessageErrorContaining(action.CertificationSignatureInfo, messageText);
		}

		public void TestValidatePaymentFinalized()
		{
			ReconDecIReconciliation.ReconDeclaration.ReconEntry.CH_Status = ReconMessageStatusList.Codes.ClearReconOriginal;
			var action = new ACEReconMessageSendingAction(ReconDecIReconciliation, UpdateActionCode.Add);
			action.PaymentFinalized = "";
			AssertHasMessageErrorContaining(action.PaymentFinalizedInfo, $"{MandatoryValidation.YouHaveNotEntered} a Suppress Payment Info.");

			action.PaymentFinalized = "G";
			AssertHasMessageError(action.PaymentFinalizedInfo, "The code you have selected is not in the list.");

			action.PaymentFinalized = "Y";
			AssertNoMessageError(action.PaymentFinalizedInfo, "The code you have selected is not in the list.");

			action.ReconWrappedJobDeclaration.US_PaymentDueDate = new ZDateTime(2013, 12, 05);
			AssertNoWarning(action.PaymentFinalizedInfo, "Payment Due Date has passed, but this is indicated as not paid yet.");

			action.PaymentFinalized = "N";
			AssertHasWarning(action.PaymentFinalizedInfo, "Payment Due Date has passed, but this is indicated as not paid yet.");
		}

		public void TestMessageManager()
		{
			var action = new ACEReconMessageSendingAction(ReconDecIReconciliation, UpdateActionCode.Add);
			AssertNotNull(action.MessageManager);
		}

		protected override BusinessObject GetNewBusinessObject() => new ACEReconMessageSendingAction(ReconDecIReconciliation, UpdateActionCode.Add);

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
