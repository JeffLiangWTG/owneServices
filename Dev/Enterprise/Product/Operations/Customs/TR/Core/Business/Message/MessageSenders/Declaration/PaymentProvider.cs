using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using CargoWise.Types;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class PaymentProvider : IPaymentTypes
	{
		public PaymentProvider(JobComInvoiceLine invoiceLine, ZDecimal totalPaymentAmount)
		{
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
			TotalPaymentAmount = totalPaymentAmount;
		}
		JobComInvoiceLine InvoiceLine { get; }
		ZDecimal TotalPaymentAmount { get; }

		public string PaymentTypeCode => InvoiceLine.ZG_CommercialPaymentCode;
		public decimal PaymentAmount => TotalPaymentAmount.Round(2);
		public string TBFID => !InvoiceLine.IsExport ? InvoiceLine.ZG_CommercialPaymentNumber : ZString.Empty;
	}
}
