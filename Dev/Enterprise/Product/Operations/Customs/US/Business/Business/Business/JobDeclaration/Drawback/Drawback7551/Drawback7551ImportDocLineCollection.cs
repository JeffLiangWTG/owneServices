using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.US.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Interoperability", "CA1405:ComVisibleTypeBaseTypesShouldBeComVisible")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1035:ICollectionImplementationsHaveStronglyTypedMembers")]
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1039:ListsAreStronglyTyped")]
	public class Drawback7551ImportDocLineCollection : Drawback7551DocLinePageCollection
	{
		public Drawback7551ImportDocLineCollection(InvoiceLineCompleteCollection collection, BusinessObjectFactory factory)
			: base(collection, factory)
		{
		}

		protected override Drawback7551DocLine GetExistingDrawback7551DocLine(JobComInvoiceLine invoiceLine)
		{
			return this.OfType<Drawback7551DocLine>().FirstOrDefault(line => line.ImportLineEqualsToInvoiceLine(invoiceLine));
		}

		protected override void UpdateDocLineDetails(Drawback7551DocLine drawback7551DocLine, JobComInvoiceLine invoiceLine)
		{
			drawback7551DocLine.UpdateImportDocLineDetails(invoiceLine);
		}
	}
}
