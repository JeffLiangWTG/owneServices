using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.SG.V4.Business
{
	public class JobComInvoiceHeaderValidation : Customs.Business.InvoiceHeaderValidation
	{
		public JobComInvoiceHeaderValidation(JobComInvoiceHeader invoiceHeader)
			: base(invoiceHeader)
		{
		}

		#region Implementation

		protected new JobComInvoiceHeader Parent => (JobComInvoiceHeader)base.Parent;

		protected ValidationHelper ValidationHelper
		{
			get { return validationHelper ?? (validationHelper = new ValidationHelper()); }
		}
		ValidationHelper validationHelper;

		protected override Customs.Business.ExternalMessageValidation GetNewExternalMessageValidation()
		{
			return new ExternalMessageValidation(Parent);
		}

		#endregion

		#region Check

		protected override void CheckJZ_JE()
		{
			base.CheckJZ_JE();

			if (Parent.JobDeclaration != null)
			{
				Parent.JobDeclaration.Validation.ValidateJE_Calc_InvoicesCount();
			}
		}

		protected override void CheckJZ_Calc_BalanceCore()
		{
			if (Parent.JobDeclaration != null)
			{
				if (!Parent.JobDeclaration.IsStandAloneCertificateOfOrigin)
				{
					if (Parent.JZ_Calc_Balance.Round(2) != 0.00m)
					{
						Parent.JZ_Calc_BalanceInfo.AddWarning("The total of all invoice lines does not equal the invoice total.");
					}
				}
			}
		}

		protected override void CheckJZ_OH_Supplier()
		{
			ListValidation.ErrorIfInvalidPK(Parent.JZ_OH_SupplierInfo, Parent.Lookups.SupplierList);
		}

		protected override void CheckJZ_IncoTerm()
		{
			ListValidation.MessageErrorIfInvalidCode(Parent.JZ_IncoTermInfo, Parent.Lookups.JZ_IncoTerm_List, (NoResString)"Please enter a valid Incoterm.");
		}

		protected override void CheckJZ_InvoiceNumber()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceNumberInfo, "Invoice Number");
		}

		protected override void CheckJZ_InvoiceDate()
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JZ_InvoiceDateInfo, "Invoice Date");
		}

		protected override void CheckJZ_InvoiceAmount()
		{
			base.CheckJZ_InvoiceAmount();
			CompareValidation.CheckNumberNotNegative(Parent.JZ_InvoiceAmountInfo);
		}

		protected override void CheckJZ_CU_RelatedHouseBill()
		{
		}

		protected override void CheckJZ_Calc_FOBAmount()
		{
		}

		protected override void CheckJZ_Calc_CIFAmount()
		{
		}

		protected override void CheckJZ_MessageType()
		{
			if (!Parent.IsAttachedToPersistentDeclaration)
			{
				ListValidation.ErrorIfInvalidCodeOrEmpty(Parent.JZ_MessageTypeInfo, Parent.Lookups.MessageTypes);
			}
		}

		#endregion
	}
}
