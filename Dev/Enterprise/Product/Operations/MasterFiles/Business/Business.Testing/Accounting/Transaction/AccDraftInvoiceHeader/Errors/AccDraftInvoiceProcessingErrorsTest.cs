using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	public class AccDraftInvoiceProcessingErrorsTest : TestCase
	{
		public void TestConvertToLogableError()
		{
			var errorKeys = typeof(AccDraftInvoiceProcessingErrors.Keys).GetAllPublicConstantValues();
			foreach (var errorKey in errorKeys)
			{
				var logableError = AccDraftInvoiceProcessingErrors.ConvertToLogableError(errorKey
					, AccDraftInvoiceProcessingErrors.Context.SavingDraftInvoiceAfterParsing);

				var expectedMessage = GetExpectedLogMessage(errorKey);
				AssertEquals(errorKey, logableError.Code);
				AssertEquals(expectedMessage, logableError.Message);
				AssertEquals(AccDraftInvoiceProcessingErrors.Context.SavingDraftInvoiceAfterParsing, logableError.ErrorContext);
				AssertEquals(expectedMessage, logableError.ErrorDetails);
			}
		}

		public void TestGetErrorMessageForDisplay()
		{
			var errorKeys = typeof(AccDraftInvoiceProcessingErrors.Keys).GetAllPublicConstantValues();
			foreach (var errorKey in errorKeys)
			{
				var errorMessage = errorKey == AccDraftInvoiceProcessingErrors.Keys.DisabledNegativeAccrualBehaviour
					? AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(errorKey, "DINV-001", "EnableNegativeAccrualBehaviors")
					: AccDraftInvoiceProcessingErrors.GetErrorMessageForDisplay(errorKey, "CLUSTER_01");

				var expectedMessage = GetExpectedUserFriendlyMessage(errorKey);
				AssertEquals(expectedMessage, errorMessage);
			}
		}

		string GetExpectedLogMessage(string errorKey)
		{
			switch (errorKey)
			{
				case AccDraftInvoiceProcessingErrors.Keys.ForeignCurrency:
					return "[{ClusterKey}]: Cannot reconcile foreign currency accruals unless the invoice is in the local currency";
				case AccDraftInvoiceProcessingErrors.Keys.ReconciliationTimeout:
					return "[{ClusterKey}]: Reconciliation aborted due to time out";
				case AccDraftInvoiceProcessingErrors.Keys.DisabledNegativeAccrualBehaviour:
					return "Reconciliation for {transactionNumber} is aborted as {EnableNegativeAccrualBehaviors} registry is disabled";
				case AccDraftInvoiceProcessingErrors.Keys.NoSuitableCombinationOfAccrualsFound:
					return "[{ClusterKey}]: No matching accruals found";
				case AccDraftInvoiceProcessingErrors.Keys.MultipleCombinationsOfAccrualsFound:
					return "[{ClusterKey}]: Multiple matching accruals found";
				case AccDraftInvoiceProcessingErrors.Keys.NoAccrual:
					return "[{ClusterKey}]: No accruals found";
				default:
					Assert("Missing message template", false);
					return string.Empty;
			}
		}

		string GetExpectedUserFriendlyMessage(string errorKey)
		{
			switch (errorKey)
			{
				case AccDraftInvoiceProcessingErrors.Keys.ForeignCurrency:
					return "[CLUSTER_01]: Cannot reconcile foreign currency accruals unless the invoice is in the local currency";
				case AccDraftInvoiceProcessingErrors.Keys.ReconciliationTimeout:
					return "[CLUSTER_01]: Reconciliation aborted due to time out";
				case AccDraftInvoiceProcessingErrors.Keys.DisabledNegativeAccrualBehaviour:
					return "Reconciliation for DINV-001 is aborted as EnableNegativeAccrualBehaviors registry is disabled";
				case AccDraftInvoiceProcessingErrors.Keys.NoSuitableCombinationOfAccrualsFound:
					return "[CLUSTER_01]: No matching accruals found";
				case AccDraftInvoiceProcessingErrors.Keys.MultipleCombinationsOfAccrualsFound:
					return "[CLUSTER_01]: Multiple matching accruals found";
				case AccDraftInvoiceProcessingErrors.Keys.NoAccrual:
					return "[CLUSTER_01]: No accruals found";
				default:
					Assert("Missing message template", false);
					return string.Empty;
			}
		}
	}
}
