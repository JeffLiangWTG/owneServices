using CargoWise.Common;
using CargoWise.Customs.TR.MessageContracts.Interfaces.Declaration;
using Enterprise.Customs.TR.Business.Declaration;

namespace Enterprise.Customs.TR.Business
{
	public class CusEntryLineFeeProvider : ITaxes
	{
		public CusEntryLineFeeProvider(CusEntryLineFee fee)
		{
			Fee = Argument.NotNull(fee, nameof(fee));
		}
		CusEntryLineFee Fee { get; }

		public int LineNo => Fee.EntryLine.CL_LineNumber;
		public string Code => Fee.NationalFeeTypeCode;
		public string Description => Fee.NationalFeeTypeCodeDescription;
		public decimal Quantity => Fee.CF_ChargeAmount.RoundAmount();
		public string Ratio => Fee.G4_RateSuspension.RoundRatio();
		public string PaymentType => Fee.G4_MethodOfPayment;
		public decimal TaxAssessment => Fee.G4_BaseAmount.RoundAmount();
	}
}

