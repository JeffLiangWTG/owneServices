using CargoWise.Types;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLineCusLinkPackage : Customs.Business.BaseCusLinkPackage
	{
		public InvoiceLineCusLinkPackage(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		protected override ZString GetPackageNumberDesc()
		{
			var package = Package;
			var result = string.Empty;

			if (package != null)
			{
				if (!package.CW_MarksAndNos.IsEmpty)
				{
					result = ZString.Format("{0}: ", package.CW_MarksAndNos);
				}
				result += ZString.Format("{0} {1}", package.CW_PackQty, package.CW_PackType);
			}

			return result;
		}
	}
}
