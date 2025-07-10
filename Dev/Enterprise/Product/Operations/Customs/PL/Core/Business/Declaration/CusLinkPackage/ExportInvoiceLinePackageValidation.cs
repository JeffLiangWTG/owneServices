using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class ExportInvoiceLinePackageValidation : InvoiceLinePackageValidation
{
	public ExportInvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
		: base(package, invoiceLine)
	{
	}

	protected override void CheckPackQtyCore()
	{
		base.CheckPackQtyCore();

		CheckRuleR0219();
		CheckRuleRuleR0220();
	}

	void CheckRuleR0219()
	{
		var parent = Parent;
		if (UniversalValidationHelper.IsInAESTransitionPeriod
			|| IsBulkPackage(parent.Package))
		{
			return;
		}

		var packQtyEmpty = parent.PackQty.IsEmpty;
		if (InvoiceLine.PackagesPivot.Cast<InvoiceLinePackagePivot>()
			.Any(x => x.CHC_NumberOfPacks.IsEmpty != packQtyEmpty
					&& !IsBulkPackage(x.Package)))
		{
			parent.PackQtyInfo.AddMessageError(Res.GetString("ExportInvoiceLinePackageValidation|CheckRuleR0219"
				, "(R0219) Either all pack quantity should be equal to '0' Or all pack quantity should be greater than '0'."));
		}
	}

	bool IsBulkPackage(BasePackage basePackage) => basePackage is Package package && package.IsBulk;

	public void CheckRuleRuleR0220()
	{
		var parent = Parent;
		if (!UniversalValidationHelper.IsInAESTransitionPeriod
			&& parent.PackQty.IsEmpty
			&& !IsSharedGoodsPackageAllowed)
		{
			parent.PackQtyInfo.AddMessageError(Res.GetString("3D9CA2F3-C502-4D2E-82C4-64FBABB5FE9C", "[R0220] Pack Quantity ‘0’ is invalid for Pack Type NE, NF and NG"));
		}
	}

	protected override ZBool IsSharedGoodsPackageAllowed => UniversalValidationHelper.IsInAESTransitionPeriod
															|| (Parent?.Package is Package package && !package.IsBreakBulk);
}
