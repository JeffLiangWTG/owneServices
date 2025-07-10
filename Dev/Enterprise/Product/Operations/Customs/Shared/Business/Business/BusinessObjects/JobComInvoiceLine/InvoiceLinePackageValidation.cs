using System;
using CargoWise.Common;

namespace Enterprise.Customs.Business
{
	public class InvoiceLinePackageValidation : BaseCusLinkPackageValidation
	{
		public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
			: base(package)
		{
			InvoiceLine = Argument.NotNull(invoiceLine, nameof(invoiceLine));
		}

		protected BaseJobComInvoiceLine InvoiceLine { get; }

		protected InvoiceLinePackagePivot Pivot
		{
			get
			{
				var package = Parent.Package;
				return package != null && !package.IsDeleted ? InvoiceLine.PackagesPivot.GetRelatedPivot(package) as InvoiceLinePackagePivot : null;
			}
		}

		protected override void CheckIsLinked()
		{
			var pivot = Pivot;

			if (pivot != null)
			{
				pivot.Validation.ValidateCHC_CW();
				InvoiceLine.Validation.ValidateJI_ContainerMode();

				Parent.IsLinkedInfo.AddAllNotificationsFrom(pivot.CHC_CWInfo);
			}
		}

		protected override void CheckPackQty()
		{
			var pivot = Pivot;

			if (pivot != null)
			{
				pivot.Validation.ValidateCHC_NumberOfPacks();
				Parent.PackQtyInfo.AddAllNotificationsFrom(pivot.CHC_NumberOfPacksInfo);
			}
		}

		public override Type AutoValidationType => typeof(InvoiceLinePackageValidation);
	}
}
