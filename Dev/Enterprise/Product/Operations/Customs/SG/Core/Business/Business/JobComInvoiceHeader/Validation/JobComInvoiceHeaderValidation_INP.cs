
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceHeaderValidation_INP : JobComInvoiceHeaderValidation_Inward
	{
		public JobComInvoiceHeaderValidation_INP(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		protected override void CheckJZ_IncoTerm()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_IncoTermInfo, Parent.Lookups.JZ_IncoTerm_List, (NoResString)"Please enter a valid Incoterm.");
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
			if (Parent.JZ_Calc_Balance.Round(2) != 0.00m)
			{
				Parent.JZ_Calc_BalanceInfo.AddWarning("The total of all invoice lines does not equal the invoice total.");
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			base.CheckJZ_OH_Supplier();

			if (Parent.HasPreferentialDuty)
			{
				MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_OH_SupplierInfo, "Supplier");
			}
		}
	}
}
