using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceWrapper : IInvoice
	{
		public InvoiceWrapper(ZString id, ZDate issueDate)
		{
			this.id = id;
			this.issueDate = issueDate;
		}

		readonly ZString id;
		readonly ZDate issueDate;

		ZString IInvoice.ID => id;

		ZDate IInvoice.IssueDateTime => issueDate;
	}
}
