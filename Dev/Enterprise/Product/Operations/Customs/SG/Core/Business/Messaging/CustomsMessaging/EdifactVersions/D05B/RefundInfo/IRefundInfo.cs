using CargoWise.Types;
using Enterprise.Customs.SG.V4.Business.PermitPrinting;

namespace Enterprise.Customs.SG.V4.Business.CustomsMessaging.RefundInfo
{
	public interface IRefundInfo
	{
		ZString PermitNumber { get; }
		ZString ReplacementNumber { get; }
		ZString NameOfCompany { get; }
		ZString EntityIdentifier { get; }
		ZString DeclarantName { get; }
		ZString DeclarantCode { get; }
		ZString TelNo { get; }
		ZDate DateOfApproval { get; }
		ZString UniqueRef { get; }
		ICConditions[] ReasonForRefund { get; }
		ICConditions[] RefundMessage { get; }
		IRefundInfoConsignment[] ConsignmentDetails { get; }
		ZDecimal TotalGoodsAndServicesTaxRefundAmount { get; }
		ZDecimal TotalExciseDutyRefundAmount { get; }
		ZDecimal TotalCustomsDutyRefundAmount { get; }
		ZDecimal TotalOtherTaxRefundAmount { get; }
	}

	public interface IRefundInfoConsignment
	{
		ZString SerialNb { get; }
		ZString HSCode { get; }
		ZDecimal DutyAmountPayable { get; }
		ZDecimal ExciseAmountPayable { get; }
		ZDecimal GstAmount { get; }
	}
}
