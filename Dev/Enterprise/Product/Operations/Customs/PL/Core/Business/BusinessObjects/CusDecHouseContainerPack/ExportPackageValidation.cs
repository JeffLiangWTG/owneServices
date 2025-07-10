using System.Linq;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public class ExportPackageValidation : PackageValidation
{
	public ExportPackageValidation(Package parent) : base(parent)
	{
	}
	protected override void CheckCW_PackQty()
	{
		base.CheckCW_PackQty();
		if (CheckRuleB1964())
		{
			CheckRuleR0364();
		}
	}

	bool CheckRuleB1964() => !UniversalValidationHelper.IsInAESTransitionPeriod;

	void CheckRuleR0364()
	{
		var parent = Package;
		if (!IsEmptyPackType() && HasAllEmptyPackLines())
		{
			parent.CW_PackQtyInfo.AddMessageError(Res.GetString("D6B31C88-0A94-49CD-A970-F0B8154BA1B2",
				"At least one Package line under Invoice line must have Package Quantity greater than 0 for same Package Type (non BULK+BREAKBULK) and Shipping Marks."));
		}

		bool IsEmptyPackType() => parent.IsBulk || parent.IsBreakBulk;

		bool HasAllEmptyPackLines() => parent.InvoiceLinePivotCollection.Cast<InvoiceLinePackagePivot>().All(x => x.CHC_NumberOfPacks.IsEmpty);
	}
}
