using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	class ImportNonCondensedDeclarationDutyTaxFee : NX5105Commodity_DutyTaxFee
	{
		public ImportNonCondensedDeclarationDutyTaxFee(CusEntryLine cusEntryLine, JobComInvoiceLine invoiceLine) : base(cusEntryLine, invoiceLine)
		{
		}

		public override ZDecimal AdValoremTaxBaseAmount => InvoiceLine.JI_CVAfterRecon;

		public override ZDecimal SpecificTaxBaseQuantity
		{
			get
			{
				var result = ZDecimal.Zero;
				var unit = GetEntryLineFeeByChargeType(UniversalReferenceConstants.RefCusRateCodes.DTS)?.CF_MethodOfCalculation ?? ZString.Empty;
				if (InvoiceLine.JI_CustomsUnitQty == unit)
				{
					result = InvoiceLine.JI_CustomsQuantity;
				}
				else if (InvoiceLine.JI_CustomsSecondUnitQty == unit)
				{
					result = InvoiceLine.JI_CustomsSecondQuantity;
				}
				return result;
			}
		}
	}
}
