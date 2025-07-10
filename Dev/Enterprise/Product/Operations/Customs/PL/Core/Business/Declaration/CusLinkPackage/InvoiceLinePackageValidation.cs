using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class InvoiceLinePackageValidation : Customs.Business.InvoiceLinePackageValidation
{
	public InvoiceLinePackageValidation(BaseCusLinkPackage package, BaseJobComInvoiceLine invoiceLine)
		: base(package, invoiceLine)
	{
	}

	protected new JobComInvoiceLine InvoiceLine => (JobComInvoiceLine)base.InvoiceLine;

	protected sealed override void CheckPackQty()
	{
		if (Parent is BaseCusLinkPackage package && package.IsLinked)
		{
			CheckPackQtyCore();
		}
	}

	protected virtual void CheckPackQtyCore()
	{
		var parent = Parent;
		var basePackage = parent.Package;

		var packQty = parent.PackQty;
		if (!packQty.IsEmpty)
		{
			if (packQty < 0)
			{
				parent.PackQtyInfo.AddMessageError(Res.GetString("5D49092D-A829-412A-9AA5-80B9FAA2F1A2", "Pack Quantity cannot be lower than 0."));
			}
		}
		else
		{
			var packType = basePackage?.CW_PackType ?? ZString.Empty;
			if (!PackageHelper.IsBulkCode(packType, InvoiceLine.Factory)
				&& IsSharedGoodsPackageAllowed)
			{
				CheckRuleR698(parent, basePackage);
			}
		}
	}

	static void CheckRuleR698(BaseCusLinkPackage package, BasePackage basePackage)
	{
		if (package != null)
		{
			if (basePackage?.CW_MarksAndNos.IsEmpty ?? true)
			{
				package.PackQtyInfo.AddMessageError(MessageErrorMarksAreEmptyOrPackQuantityNotEntered);
			}
			else
			{
				if (!PackageHelper.HasThePacksBeenDeclaredOnOtherInvoiceLines(basePackage))
				{
					package.PackQtyInfo.AddMessageError(MessageErrorMarksAreEmptyOrPackQuantityNotEntered);
				}
			}
		}
	}

	protected virtual ZBool IsSharedGoodsPackageAllowed => true;

	static string MessageErrorMarksAreEmptyOrPackQuantityNotEntered => Res.GetString("6D1533A7-E7EA-4B2F-A153-BE8B8D875809", "You have not entered a pack quantity or marks are empty in case of goods packed together.");
}
