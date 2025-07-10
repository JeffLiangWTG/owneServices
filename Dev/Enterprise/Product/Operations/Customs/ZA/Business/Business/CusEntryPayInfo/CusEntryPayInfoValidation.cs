
using CargoWise.Types;

namespace Enterprise.Customs.ZA.Business
{
	public class CusEntryPayInfoValidation : Customs.Business.CusEntryPayInfoValidation
	{
		public CusEntryPayInfoValidation(CusEntryPayInfo parent) : base(parent)
		{
		}

		protected override void CheckC9_PaymentReference()
		{
			base.CheckC9_PaymentReference();
			if (Parent.C9_TransactionType == UniversalReferenceConstants.TaxOrFeeTypeCode.VAT)
			{
				if (Parent.C9_PaymentReference.IsEmpty && !Parent.C9_ReceiptDate.IsEmpty)
				{
					Parent.C9_PaymentReferenceInfo.AddError(ValidationConstants.CusEntryPayInfo.ReceiptNumberRequiredWithDate);
				}
			}
		}

		protected override void CheckC9_ReceiptDate()
		{
			base.CheckC9_ReceiptDate();
			if (Parent.C9_TransactionType == UniversalReferenceConstants.TaxOrFeeTypeCode.VAT)
			{
				if (!Parent.C9_ReceiptDate.IsEmpty)
				{
					if (Parent.C9_ReceiptDate > ZDate.Today)
					{
						Parent.C9_ReceiptDateInfo.AddError(ValidationConstants.CusEntryPayInfo.ReceiptDateCannotBeGreaterThantoday);
					}
				}
				else if (!Parent.C9_PaymentReference.IsEmpty)
				{
					Parent.C9_ReceiptDateInfo.AddError(ValidationConstants.CusEntryPayInfo.ReceiptDateRequiredWithNumber);
				}
			}
		}
	}
}
