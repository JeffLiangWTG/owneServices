using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.SG.V4.Business.Testing
{
	public abstract class JobComInvoiceHeaderValidationTest : BusinessObjectValidationTestCase
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
		protected JobComInvoiceHeader InvoiceHeader
		{
			get
			{
				return invoiceHeader ?? (invoiceHeader = Declaration.Invoices.AddNew());
			}
		}

		JobComInvoiceHeader invoiceHeader;
		protected JobComInvoiceHeaderValidation Validation
		{
			get
			{
				return InvoiceHeader.Validation;
			}
		}
		#endregion
	}
}
