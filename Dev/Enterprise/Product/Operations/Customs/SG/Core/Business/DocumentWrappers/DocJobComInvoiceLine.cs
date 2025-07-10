using CargoWise.EntityFramework;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.SG.V4.Business
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
	{
		DocJobComInvoiceLine(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
			: base(jobComInvoiceLine, factoryToWrap)
		{
		}

		public static DocJobComInvoiceLine New(JobComInvoiceLine jobComInvoiceLine, BusinessObjectFactory factoryToWrap)
		{
			if (jobComInvoiceLine == null)
			{
				return null;
			}
			else
			{
				return new DocJobComInvoiceLine(jobComInvoiceLine, factoryToWrap);
			}
		}

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		public DocCusEntryLine EntryLine
		{
			get { return (DocCusEntryLine)CusEntryLineInternal; }
		}

		public DocJobComInvoiceHeader ComInvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}
	}
}
