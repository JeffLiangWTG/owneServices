using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.MasterFiles.Integration.Customs.PermitService
{
	public enum SuccessOrFailure { Failure, Success, FailureButCanBeFulfilledByMultiplePermits }
	public enum PermitType
	{
		FTZ
#if DEBUG
			, Unknown
#endif
	}

	public interface IPermitMatchingCriteria : IPermitMatchingDetail
	{
		IOrgAddress Owner { get; }
		IOrgAddress Manufacturer { get; }
		bool DetailedTrackingEnabled { get; }
		ZString Tariff { get; }
		ZString CountryOfOrigin { get; }
		ZString ProductCode { get; }
		ZString UQ { get; }
	}

	public interface IPermitWithdrawalRequestDetail : IPermitTransactionDetail
	{
		ZDecimal Qty { get; }
		ZDecimal ReceiveTotalQty { get; }
		ZDecimal ReceiveTotalCustomsValue { get; }
	}

	public interface IPermitTransactionDetail : IPermitMatchingDetail
	{
		/// <summary>
		/// A Concatenated value of Order number + Order Line number is expected to be passed in. This is based on the assumption that an order number is unique and an order line number in one order is unique.
		/// </summary>
		ZString PermitTransactionRefNumber { get; }
	}

	public interface IPermitMatchingDetail
	{
		ZString ZoneStatus { get; }
		IOrgAddress Warehouse { get; }
		PermitType PermitType { get; }
	}

	public interface IPermitWithdrawRequestResponseResult
	{
		bool Success { get; }
		IEnumerable<IPermitWithdrawRequestResponse> Responses { get; }
	}

	public interface IPermitWithdrawRequestResponse
	{
		IPermitWithdrawRequest Request { get; }

		SuccessOrFailure SuccessOrFailure { get; }

		ZString FailureReason { get; } // To be used to inform users of the failure reason
		ZDecimal? AvailableQty { get; }//To be used to inform users how many are available on the event of failure. 
		ZString? OutwardEntryNumber { get; }//this needs to be recorded in OrderLine on the event of success
	}

	public interface IPermitWithdrawRequest : IPermitMatchingCriteria, IPermitWithdrawalRequestDetail
	{
	}

	public interface IPermitService
	{
		/// <summary>
		/// This API will check if passed withdrawal requests are permitted in terms of Customs Permit records. 
		/// </summary>
		/// <param name="requests">An enumerable of withdrawal request</param>
		/// <returns></returns>
		IPermitWithdrawRequestResponseResult IsPermitAvailable(IEnumerable<IPermitWithdrawRequest> requests);

		/// <summary>
		/// This API will check if passed withdrawal requests are permitted in terms of Customs Permit records. If it is permitted the system will reduce the data as per the request 
		/// </summary>
		/// <param name="requests">An enumerable of withdrawal request</param>
		/// <returns></returns>
		IPermitWithdrawRequestResponseResult TryGetPermits(IEnumerable<IPermitWithdrawRequest> requests);

		/// <summary>
		/// This is to record finalised permit transactions. If a finalised quantity is different from the original requests, Customs permit records should record the fact.
		/// </summary>
		/// <param name="permitTransactions"></param>
		/// <returns></returns>
		bool ConfirmPermitTransactions(IEnumerable<IPermitWithdrawalRequestDetail> permitTransactions);

		/// <summary>
		/// This is to relinquish permit transactions before an order is finalised.
		/// </summary>
		/// <param name="permitTransactionRefNumbers"></param>
		/// <returns></returns>
		SuccessOrFailure RelinquishPermitTransactions(IEnumerable<IPermitTransactionDetail> permitTransactionRefNumbers);
	}
}
