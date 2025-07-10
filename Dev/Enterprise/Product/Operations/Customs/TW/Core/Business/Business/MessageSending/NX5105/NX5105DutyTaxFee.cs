using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	class NX5105Declaration_DutyTaxFee : IDutyTaxFee
	{
		readonly JobDeclaration declaration;
		readonly CusEntryInstruction entryInstruction;
		readonly CusEntryHeader entryHeader;

		public NX5105Declaration_DutyTaxFee(CusEntryHeader entryHeader)
		{
			this.entryHeader = Argument.NotNull(entryHeader, "entryHeader");
			this.declaration = Argument.NotNull(entryHeader.Declaration, "declaration");
			this.entryInstruction = Argument.NotNull(entryHeader.EntryInstruction, "entryInstruction");
		}

		ZString IDutyTaxFee.DutyExemptionWaiverNote => entryInstruction.CEI_WaiverOfExemption.ConvertBoolToString(YesNoList.Codes.Yes);

		ZString IDutyTaxFee.DutyMemoPrinted => entryInstruction.CEI_PrintDutyMemo.ConvertBoolToString(YesNoList.Codes.Yes);

		ZString IDutyTaxFee.DutyMethodCode => declaration.JE_PaymentMethod;

		ZDecimal IDutyTaxFee.TotalDutyTaxFeeAmount => entryHeader.TotalTaxAmount;

		ZString IDutyTaxFee.PaymentObligationGuaranteeReferenceID => declaration.JE_DefermentAccountNumber;

		ZDecimal IDutyTaxFee.TotalCashDutyTaxFeeAmount => entryHeader.TotalCashTaxAmount;

		ZDecimal IDutyTaxFee.TotalNonCashDutyTaxFeeAmount => entryHeader.TotalNonCashTaxAmount;
	}
}
