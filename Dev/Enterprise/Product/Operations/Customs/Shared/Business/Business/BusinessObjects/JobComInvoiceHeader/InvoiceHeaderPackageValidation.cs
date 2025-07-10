using System;
using CargoWise.Common;

namespace Enterprise.Customs.Business
{
	public class InvoiceHeaderPackageValidation : BaseCusLinkPackageValidation
	{
		public InvoiceHeaderPackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceHeader invoiceHeader)
			: base(package)
		{
			InvoiceHeader = Argument.NotNull(invoiceHeader, nameof(invoiceHeader));
		}

		BaseJobComInvoiceHeader InvoiceHeader { get; }

		InvoiceHeaderPackagePivot Pivot
		{
			get
			{
				var package = Parent.Package;
				return package != null && !package.IsDeleted ? InvoiceHeader.PackagesPivot.GetRelatedPivot(package) as InvoiceHeaderPackagePivot : null;
			}
		}

		protected override void CheckIsLinked()
		{
			var pivot = Pivot;

			if (pivot != null)
			{
				pivot.Validation.ValidateCHZ_CW();
				Parent.IsLinkedInfo.AddAllNotificationsFrom(pivot.CHZ_CWInfo);
			}
		}

		protected override void CheckPackQty()
		{
			var pivot = Pivot;

			if (pivot != null)
			{
				pivot.Validation.ValidateCHZ_NumberOfPacks();
				Parent.PackQtyInfo.AddAllNotificationsFrom(pivot.CHZ_NumberOfPacksInfo);
			}
		}

		public override Type AutoValidationType => typeof(InvoiceHeaderPackageValidation);
	}
}
