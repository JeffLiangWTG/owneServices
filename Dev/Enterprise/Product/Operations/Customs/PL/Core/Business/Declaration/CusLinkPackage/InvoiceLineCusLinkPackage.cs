using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class InvoiceLineCusLinkPackage : BaseCusLinkPackage
{
	public InvoiceLineCusLinkPackage(JobComInvoiceLine invoiceLine)
		: base(invoiceLine)
	{
		this.invoiceLine = invoiceLine;
	}

	readonly JobComInvoiceLine invoiceLine;

	protected override bool GetIsPackQty_ReadOnlyCore()
	{
		return PackageHelper.IsBulkCode(Package.CW_PackType, Factory) ||
				base.GetIsPackQty_ReadOnlyCore();
	}

	public override ZInt PackQty
	{
		get => base.PackQty;
		set
		{
			var oldValue = base.PackQty;
			base.PackQty = value;
			if (oldValue != PackQty
				&& !IsCopying
				&& !IsValidationSuspended
				&& invoiceLine.IsExport)
			{
				ValidateOtherPackageQty();
			}
		}
	}

	void ValidateOtherPackageQty()
	{
		var parentCollection = ParentCollections.FirstOrDefault(c => c is BaseCusLinkPackageCollection);
		parentCollection?.Cast<BaseCusLinkPackage>()
			.Where(c => c != this && c.IsLinked)
			.ForEach(x => x.Validation.ValidatePackQty());
	}
}
