using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineCusLinkPackageCollection : BaseCusLinkPackageCollection
	{
		public InvoiceLineCusLinkPackageCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public new InvoiceLineCusLinkPackage this[int index]
		{
			get { return (InvoiceLineCusLinkPackage)base[index]; }
		}

		public new InvoiceLineCusLinkPackage AddNew()
		{
			return (InvoiceLineCusLinkPackage)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceLineCusLinkPackage((JobComInvoiceLine)Supporter);
		}

		protected override ZBool ShouldAddItem(BasePackage package)
		{
			return !package.InvoiceLinePivotCollection.Any() || package.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().Any(x => x.InvoiceLine.InvoiceHeader == ((JobComInvoiceLine)Supporter).InvoiceHeader);
		}
	}
}
