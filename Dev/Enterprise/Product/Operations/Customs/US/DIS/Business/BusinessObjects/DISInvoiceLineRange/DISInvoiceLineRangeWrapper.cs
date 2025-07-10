using System.Collections.Generic;
using Enterprise.MasterFiles.Business.DIS;

namespace Enterprise.Customs.US.DIS.Business
{
	class DISInvoiceLineRangeWrapper
	{
		public DISInvoiceLineRangeWrapper(DISInvoiceLineRange lineRange, DISHostWrapper hostWrapper)
		{
			this.lineRange = lineRange;
			this.hostWrapper = hostWrapper;
		}

		readonly DISInvoiceLineRange lineRange;
		readonly DISHostWrapper hostWrapper;

		public IEnumerable<IDISInvoiceLine> InvoiceLines
		{
			get { return hostWrapper.GetInvoiceLines(lineRange.InvoiceNumber, lineRange.InvoiceLineFrom, lineRange.InvoiceLineTo); }
		}
	}
}
