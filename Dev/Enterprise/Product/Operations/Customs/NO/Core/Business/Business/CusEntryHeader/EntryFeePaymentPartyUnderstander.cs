using System.Linq;
using Enterprise.Customs.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.NO.Business;

public static class EntryFeePaymentPartyUnderstander
{
	public static bool ShouldBrokerPayThisFee(this CusEntryHeader header, string feeCode, string methodOfPayment, ILogger logger)
	{
		var declarationPaymentMethod = header.CH_PaymentMethod;
		var entryLines = header.MergedLines.Cast<CusEntryLine>();
		var isLandedCostOnly = entryLines.SelectMany(x => x.Fees).Cast<CusEntryLineFee>().All(x => x.CF_IsLandedCostOnly);

		var response = declarationPaymentMethod == NOPaymentMethodCodeList.Codes.ForwardersDayCredit && !isLandedCostOnly;

		var logText = response
			? (NoResString)" is always paid by broker - included in rating"
			: (NoResString)" is never paid by broker - excluded from rating";

		var paymentMethodText = declarationPaymentMethod.IsEmpty ? (NoResString)"empty" : declarationPaymentMethod.ToString();
		logger?.Log(LogType.Information, (NoResString)"Fee " + feeCode + (NoResString)" with MoP=" + methodOfPayment +
					(NoResString)" with deferral payment party " + paymentMethodText + logText);

		return response;
	}
}
