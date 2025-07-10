using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.PL.Business.Declaration;

namespace Enterprise.Customs.PL.Business;

public class Package : EU.Business.Declaration.Package, Integration.Customs.PL.IPackage
{
	public Package(BusinessObjectFactory factory, DataRow row)
		: base(factory, row)
	{
	}

	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	[ReadOnlyMember(nameof(IsBulkCode))]
	public override ZInt CW_PackQty { get => base.CW_PackQty; set => base.CW_PackQty = value; }

	public override ZString CW_PackType
	{
		get => base.CW_PackType;
		set
		{
			var oldValue = base.CW_PackType;
			if (oldValue != value)
			{
				base.CW_PackType = value;
				UpdateInvoiceLinePivotQuantityForBulkCodes();
			}
		}
	}

	void UpdateInvoiceLinePivotQuantityForBulkCodes()
	{
		if (IsBulkCode)
		{
			CW_PackQty = 0;
			foreach (InvoiceLinePackagePivot invoiceLinePivot in InvoiceLinePivotCollection)
			{
				invoiceLinePivot.CHC_NumberOfPacks = 0;
			}
		}
	}

	bool IsBulkCode => PackageHelper.IsBulkCode(CW_PackType, Factory);

	protected override bool IsEmptyPackTypeAllowedCore => IsBulkCode
														|| (InvoiceLinePivotCollection?.Cast<InvoiceLinePackagePivot>()?.Any(x => x.CHC_NumberOfPacks > 0) ?? false);

	protected override CusDecHouseContainerPackValidation GetNewValidation() => Declaration?.IsExport ?? false
		? new ExportPackageValidation(this)
		: new PackageValidation(this);
}
