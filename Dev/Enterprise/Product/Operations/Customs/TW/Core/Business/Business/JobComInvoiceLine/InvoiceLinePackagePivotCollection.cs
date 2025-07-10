using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLinePackagePivotCollection : Customs.Business.InvoiceLinePackagePivotCollection
	{
		public InvoiceLinePackagePivotCollection(JobComInvoiceLine line)
			: base(line)
		{
			jobComInvoiceLine = line;
		}

		readonly JobComInvoiceLine jobComInvoiceLine;

		protected override ZDecimal CalculateDefaultQuantity(BasePackage package)
		{
			return MathHelper.CalculateDefaultQuantity(jobComInvoiceLine.JI_InvoiceQuantity, jobComInvoiceLine.TotalQuantityForPackagesPivot);
		}
	}
}
