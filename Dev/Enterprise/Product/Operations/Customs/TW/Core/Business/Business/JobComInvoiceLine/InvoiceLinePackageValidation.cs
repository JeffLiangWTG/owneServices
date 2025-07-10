using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.TW.Business
{
	public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
	{
		public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
			: base(package, invoiceLine)
		{
			this.invoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine)) as JobComInvoiceLine;
		}
		readonly JobComInvoiceLine invoiceLine;

		protected override void CheckQuantity()
		{
			base.CheckQuantity();
			if (Parent.IsLinked)
			{
				var quantityInfo = Parent.QuantityInfo;
				var totalQuantityForPackagesPivot = invoiceLine.TotalQuantityForPackagesPivot;
				if (totalQuantityForPackagesPivot != invoiceLine.JI_InvoiceQuantity)
				{
					quantityInfo.AddWarning(ValidationConstants.InvoiceLine.TotalQuantityForPackagesPivotNotEqualToInvoiceQuantity(totalQuantityForPackagesPivot, invoiceLine.JI_InvoiceQuantity));
				}
				CompareValidation.MessageErrorIfLessThanOrEqualToZero(Parent.QuantityInfo);
				TypeValidation.CheckValidDecimal(quantityInfo, 9, 3);
			}
		}
	}
}
