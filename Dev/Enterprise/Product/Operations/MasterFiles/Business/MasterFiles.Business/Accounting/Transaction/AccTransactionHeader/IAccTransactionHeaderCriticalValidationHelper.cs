using System.Diagnostics.CodeAnalysis;

namespace Enterprise.MasterFiles.Business.Accounting
{
	public interface IAccTransactionHeaderCriticalValidationHelper
	{
		CriticalValidationHelperResult CheckCancelledTransactionHeaderWithNoLineLinkedToCharge(AccTransactionHeader target);

		bool IsReversalOfOriginalTransaction(AccTransactionHeader target);

#if DEBUG
		void SetIsReversalOfOriginalTransaction_ForTestOnly(AccTransactionHeader target);
#endif
	}

	[SuppressMessage("Microsoft.Performance", "CA1815: Override equals and operator equals on value types", Justification = "It is not used for comparison.")]
	public struct CriticalValidationHelperResult
	{
		public CriticalValidationHelperResult(bool isValid, string errorMsg)
		{
			IsValid = isValid;
			ErrorMessage = errorMsg;
		}

		public bool IsValid { get; }
		public string ErrorMessage { get; }
	}
}