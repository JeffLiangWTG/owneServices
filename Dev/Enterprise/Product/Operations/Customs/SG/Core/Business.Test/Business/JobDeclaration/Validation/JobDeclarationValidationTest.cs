using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class JobDeclarationValidationTest : BusinessObjectValidationTestCase
	{
		#region Implementation
		#region Declaration
		protected JobDeclaration Declaration
		{
			get
			{
				if (declaration == null)
				{
					declaration = Factory.New<JobDeclaration>();
					declaration.JE_MessageType = MessageType;
				}

				return declaration;
			}
		}

		JobDeclaration declaration;
		protected virtual string MessageType
		{
			get
			{
				return "";
			}
		}

		#endregion
		#region InvoiceHeader
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		#endregion
		#region InvoiceLine
		protected JobComInvoiceLine InvoiceLine
		{
			get
			{
				return invoiceLine ?? (invoiceLine = InvoiceHeader.JobComInvoiceLines.AddNew());
			}
		}

		JobComInvoiceLine invoiceLine;
		#endregion
		protected JobDeclarationValidation Validation
		{
			get
			{
				return Declaration.Validation;
			}
		}
		#endregion
	}
}
