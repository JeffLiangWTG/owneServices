using Enterprise.MasterFiles.Business.Accounting.ProcessLogging;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business
{
	public static partial class AccDraftInvoiceProcessingErrors
	{
		///<summary>
		/// returns an ILogableError instance for the provided business rule violation error code.
		/// </summary>
		public static ILogableError ConvertToLogableError(string errorKey, string errorContext, params object[] messageParameters)
		{
			return new LogableBusinessError(errorCode: errorKey
							, context: errorContext
							, messageTemplate: GetErrorDetailsForLogging(errorKey)
							, messageParameters);
		}

		///<summary>
		/// returns an error message to display for the provided business rule violation error code.
		/// </summary>
		public static string GetErrorMessageForDisplay(string errorCode, params string[] messageParameters) =>
			GetErrorMessageCore(errorCode, false, messageParameters);

		static string GetErrorDetailsForLogging(string errorCode, params object[] messageParameters) =>
			GetErrorMessageCore(errorCode, true, messageParameters);

		static string GetErrorMessageCore(string errorCode, bool forLogging, params object[] messageParameters)
		{
			var message = string.Empty;
			switch (errorCode)
			{
				case Keys.NoAccrual: message = GetNoAccrualErrorMessage(forLogging, messageParameters); break;
				case Keys.NoSuitableCombinationOfAccrualsFound: message = GetNoSuitableCombinationOfAccrualsFoundErrorMessage(forLogging, messageParameters); break;
				case Keys.MultipleCombinationsOfAccrualsFound: message = GetMultipleCombinationsOfAccrualsFoundErrorMessage(forLogging, messageParameters);break;
				case Keys.ReconciliationTimeout: message = GetReconciliationTimeoutErrorMessage(forLogging, messageParameters); break;
				case Keys.ForeignCurrency: message = GetForeignCurrencyErrorMessage(forLogging, messageParameters); break;
				case Keys.DisabledNegativeAccrualBehaviour: message = GetDisabledNegativeAccrualBehaviourErrorMessage(forLogging, messageParameters); break;
				default: break;
			}
			return message;
		}

		static string GetNoAccrualErrorMessage(bool forLogging, params object[] messageParameters) =>
			forLogging
				? (NoResString)"[{ClusterKey}]: No accruals found"
				: Res.GetString("370d8db0-324d-4559-af1a-4907d944eed7", "[{0}]: No accruals found", messageParameters);

		static string GetNoSuitableCombinationOfAccrualsFoundErrorMessage(bool forLogging, params object[] messageParameters) =>
			forLogging
				? (NoResString)"[{ClusterKey}]: No matching accruals found"
				: Res.GetString("05b5d262-887b-43e7-b0ee-6be2b07075f0", "[{0}]: No matching accruals found", messageParameters);

		static string GetMultipleCombinationsOfAccrualsFoundErrorMessage(bool forLogging, params object[] messageParameters) =>
			forLogging
				? (NoResString)"[{ClusterKey}]: Multiple matching accruals found"
				: Res.GetString("773574c2-310b-45be-8edb-bd82c53b6ab7", "[{0}]: Multiple matching accruals found", messageParameters);

		static string GetReconciliationTimeoutErrorMessage(bool forLogging, params object[] messageParameters) =>
			forLogging
				? (NoResString)"[{ClusterKey}]: Reconciliation aborted due to time out"
				: Res.GetString("9f626240-b26c-44fd-a264-7bd8623a455d", "[{0}]: Reconciliation aborted due to time out", messageParameters);

		static string GetForeignCurrencyErrorMessage(bool forLogging, params object[] messageParameters) =>
			forLogging
				? (NoResString)"[{ClusterKey}]: Cannot reconcile foreign currency accruals unless the invoice is in the local currency"
				: Res.GetString("43735f03-9102-4b0f-b5c2-cafdf281451e", "[{0}]: Cannot reconcile foreign currency accruals unless the invoice is in the local currency", messageParameters);

		static string GetDisabledNegativeAccrualBehaviourErrorMessage(bool forLogging, params object[] messageParameters) =>
			forLogging
				? (NoResString)"Reconciliation for {transactionNumber} is aborted as {EnableNegativeAccrualBehaviors} registry is disabled"
				: Res.GetString("1c41df40-2488-4540-9152-4d7bcb59a633", "Reconciliation for {0} is aborted as {1} registry is disabled", messageParameters);
	}
} 
