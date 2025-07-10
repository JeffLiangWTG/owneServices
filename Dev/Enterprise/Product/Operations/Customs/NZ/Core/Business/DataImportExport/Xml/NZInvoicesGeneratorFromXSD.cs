using Enterprise.Customs.Business;
using Enterprise.Customs.DataTransfer;
using Enterprise.DataTransfer.Integration;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Customs.NZ.Business.Data
{
	public class NZInvoicesGeneratorFromXSD : InvoicesGeneratorFromXSD
	{
		public NZInvoicesGeneratorFromXSD(BaseJobDeclaration declaration)
			: base(declaration)
		{
		}

		protected override void GenerateInvoiceGroupHeaders(Xsd.InvoiceHeaderCollection xmlInvoices, BaseJobDeclaration jobDec, IValueObjectImportContext context)
		{
			bool importedFirstInvoiceHeader = false;

			foreach (Xsd.InvoiceHeader invoiceHeaderValue in xmlInvoices)
			{
				if (invoiceHeaderValue.IsGroupInvoiceSpecified && (invoiceHeaderValue.IsGroupInvoice == Xsd.TrueFalse.@true))
				{
					BaseJobComInvoiceGroupHeader invoiceHeader = null;
					if (!importedFirstInvoiceHeader)
					{
						invoiceHeader = GetInvoiceGroupHeader(jobDec.JobComInvoiceGroupHeaders, invoiceHeaderValue.InvoiceNumber, false);
						importedFirstInvoiceHeader = true;
					}
					else
					{
						invoiceHeader = GetInvoiceGroupHeader(jobDec.JobComInvoiceGroupHeaders[0].JobComInvoiceGroupHeaders, invoiceHeaderValue.InvoiceNumber, true);
					}
					GroupInvoiceAdapter.ImportFromValueObject(invoiceHeader, invoiceHeaderValue, context);
				}
			}
		}

		protected override InvoiceValueObjectDataAdapter GetInvoiceAdapter()
		{
			return new NZInvoiceValueObjectDataAdapter(declaration);
		}
	}
}
