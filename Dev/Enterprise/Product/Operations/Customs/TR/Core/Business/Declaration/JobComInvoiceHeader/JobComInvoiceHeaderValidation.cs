using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.Business.Declaration
{
	public class JobComInvoiceHeaderValidation : EU.Business.Declaration.JobComInvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader) : base(invoiceHeader)
		{
		}

		public override void ValidateAll()
		{
			Parent.ClearRowNotifications();
			base.ValidateAll();
			ValidateSingleInvoice();
		}

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected override TypeOfValidationForMissingMandatoryChargesForIncoterm ValidationForMissingMandatoryCharges
		{
			get { return TypeOfValidationForMissingMandatoryChargesForIncoterm.Warning; }
		}

		protected override void CheckJZ_RelatedIndicator()
		{
			base.CheckJZ_RelatedIndicator();

			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_RelatedIndicatorInfo, Parent.Lookups.RelationCodeList);

			var targetInfo = Parent.JZ_RelatedIndicatorInfo;
			var relatedIndicator = Parent.JZ_RelatedIndicator;

			if (!relatedIndicator.IsEmpty)
			{
				if (Parent.JobDeclaration.Invoices.OfType<JobComInvoiceHeader>().Any(x => x.JZ_RelatedIndicator != relatedIndicator))
				{
					targetInfo.AddMessageError(Res.GetString("DE28404C-E01B-4C3E-99C5-5CD594306426", "Seller and Buyer relations are different on invoices"));
				}
			}
		}

		protected override void CheckJZ_PaymentNo()
		{
			base.CheckJZ_PaymentNo();

			var mandatoryProcedureList = new ZString[] { "6121", "6123", "6323", "6771", "5100", "5121", "5171", "5191", "5300", "5321", "5353", "5358", "5371", "5391", "5800" };
			if (Parent.JZ_PaymentNo.IsEmpty && (Parent.JobComInvoiceLines.OfType<JobComInvoiceLine>().Any(x => x.ProcedureCode.StartsWith("4", StringComparison.Ordinal) || x.ProcedureCode.StartsWith("71", StringComparison.Ordinal) || mandatoryProcedureList.Contains(x.ProcedureCode))))
			{
				Parent.JZ_PaymentNoInfo.AddMessageError(Res.GetString("59D14C37-8C3A-47D0-A15A-021508088F33", "Payment Code field is mandatory for procedure codes starting with 4 and 71, and for the procedure codes 6121, 6123, 6323, 6771, 5100, 5121, 5171, 5191, 5300, 5321, 5353, 5358, 5371, 5391, 5800."));
			}
		}

		protected override void CheckJZ_PaymentAmount()
		{
			base.CheckJZ_PaymentAmount();

			if (Parent.JZ_PaymentAmount == 0 && !Parent.ZG_CommercialPaymentCode.IsEmpty)
			{
				Parent.JZ_PaymentAmountInfo.AddMessageError(Res.GetString("034F7143-015E-40B0-B2E1-31345271BF30", "Payment amount is required if payment code is filled."));
			}
		}

		protected override void CheckJZ_RX_NKInvoice_Currency()
		{
			base.CheckJZ_RX_NKInvoice_Currency();

			var currency = Parent.JZ_RX_NKInvoice_Currency;
			if (!currency.IsEmpty && Parent.JobDeclaration.Invoices.OfType<JobComInvoiceHeader>().Any(invoice => !invoice.Equals(Parent) && !invoice.JZ_RX_NKInvoice_Currency.Equals(currency)))
			{
				Parent.JZ_RX_NKInvoice_CurrencyInfo.AddMessageError(Res.GetString("E64787B7-8BE0-41B7-B6EF-A671587E9D97", "All Invoices must have the same Currency."));
			}
		}

		void ValidateSingleInvoice()
		{
			if (Parent.JobDeclaration.Invoices.Count > 1)
			{
				 Parent.AddRowMessageError(Res.GetString("5909E291-6564-4A92-B4B0-867B74F446E3", "You can enter only one invoice for a declaration."));
			}
		}
	}
}
