#if DEBUG
using CargoWise.EntityFramework;
using Enterprise.Customs._CustomsTemplate_.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs._CustomsTemplate_.DocumentWrappers
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine, Integration.Customs._CustomsTemplate_.IDocJobComInvoiceLine
	{
		DocJobComInvoiceLine(JobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
			: base(invoiceLine, factoryToWrap)
		{
		}

		public static DocJobComInvoiceLine New(JobComInvoiceLine invoiceLine, BusinessObjectFactory factoryToWrap)
		{
			if (invoiceLine == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceLine(invoiceLine, factoryToWrap);
			}
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New(invoiceToWrap as JobComInvoiceHeader, Factory);
		}

		#region Wrapper Fields

		public DocCusEntryLine EntryLine => (DocCusEntryLine)CusEntryLineInternal;

		public DocJobComInvoiceHeader ComInvoiceHeader => (DocJobComInvoiceHeader)InvoiceHeaderInternal;

		#endregion
	}
}
#endif
