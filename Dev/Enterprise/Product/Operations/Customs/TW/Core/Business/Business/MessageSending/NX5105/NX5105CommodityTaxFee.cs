using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Commodity_DutyTaxFee : ICommodityDutyTaxFee
	{
		public NX5105Commodity_DutyTaxFee(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine)
		{
			this.EntryLine = cusEntryLine;
			this.InvoiceLine = invoiceLine;
		}

		protected readonly CusEntryLine EntryLine;
		protected readonly JobComInvoiceLine InvoiceLine;

		public virtual ZDecimal AdValoremTaxBaseAmount => EntryLine?.CL_CustomsValue ?? ZDecimal.Zero;

		public virtual ZString DutyRegimeCode => InvoiceLine?.JI_TariffAdditionalCode ?? ZString.Empty;

		public virtual ZDecimal SpecificTaxBaseQuantity => GetEntryLineFeeByChargeType(UniversalReferenceConstants.RefCusRateCodes.DTS)?.CF_BaseValue ?? ZDecimal.Zero;

		public virtual ZDecimal PercentageNumeric => InvoiceLine?.JI_CusValueConvRatio ?? ZDecimal.Zero;

		protected CusEntryLineFee GetEntryLineFeeByChargeType(string chargeType)
		{
			return EntryLine?.Fees.Cast<CusEntryLineFee>().FirstOrDefault(x => x.CF_ChargeType == chargeType);
		}
	}
}
