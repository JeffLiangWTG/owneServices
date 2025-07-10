using CargoWise.Types;

namespace Enterprise.MasterFiles.Business
{
	/// <summary>
	/// E-Invoicing specific details about an AccTransactionHeader.
	/// </summary>
	public interface IEInvoicingTransaction
	{
		ZGuid PK { get; }

		ZString UniqueIdentifier { get; }

		ZString CurrentStatus { get; }

		ZString CurrentError { get; }

		ZDateTime LastResponseReceivedUtc { get; }

		ZDateTime LastSentTimeUtc { get; }

		ZString BatchNumber { get; }

		ZString GovernmentAllocatedNumber { get; }

		ZString eHubAllocatedNumber { get; }

		/// <summary>
		/// Evaluates the transaction for e-Invoicing eligibility and creates / removes business objects as required.
		/// </summary>
		/// <remarks>
		/// If eligible, the transaction will be queued (or re-queued) for processing.
		/// If ineligible, the transaction will be de-queued for processing.
		/// If in an invalid status (eg: Sent or Successful), no action will be taken (modifying something while in-flight or after submission is dangerous).
		/// </remarks>
		void EvaluateEligibilityAndQueue();
	}
}
