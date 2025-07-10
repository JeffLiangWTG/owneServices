using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.US.Business;
using Enterprise.DocumentWrappers.Customs.Base;

namespace Enterprise.Customs.US.DocumentWrappers
{
	public class DocJobComInvoiceLine : DocBaseJobComInvoiceLine
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

		#region Overrides

		protected override DocBaseJobComInvoiceHeader CreateJobComInvoiceHeader(Customs.Business.BaseJobComInvoiceHeader invoiceToWrap)
		{
			return DocJobComInvoiceHeader.New((JobComInvoiceHeader)invoiceToWrap, Factory);
		}

		protected override DocBaseCusEntryLine CreateCusEntryLine(Customs.Business.CusEntryLine entryLineToWrap)
		{
			return DocCusEntryLine.New((CusEntryLine)entryLineToWrap, Factory);
		}

		protected override ZString CountryOfOriginCodeCore
		{
			get
			{
				var result = base.CountryOfOriginCodeCore;
				if (JobComInvoiceLine.IsImport)
				{
					result = JobComInvoiceLine.US_UC_NKCountryOfOrigin;
				}
				return result;
			}
		}

		#endregion

		#region Wrapper Fields

		public DocCusEntryLine EntryLine
		{
			get { return (DocCusEntryLine)CusEntryLineInternal; }
		}

		public DocJobComInvoiceHeader ComInvoiceHeader
		{
			get { return (DocJobComInvoiceHeader)InvoiceHeaderInternal; }
		}

		#endregion

		#region Implementation

		JobComInvoiceLine JobComInvoiceLine
		{
			get { return (JobComInvoiceLine)WrappedObject; }
		}

		#endregion
	}
}
