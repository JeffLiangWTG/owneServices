using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business;

public static class PackageHelper
{
	public static bool IsBulkCode(ZString packType, BusinessObjectFactory factory) => Constants.ValidationLists.BulkPackageCodes(factory).Contains(packType);

	public static bool HasThePacksBeenDeclaredOnOtherInvoiceLines(BasePackage package) => package?.InvoiceLinePivotCollection?.Any(x => ((InvoiceLinePackagePivot)x).CHC_NumberOfPacks > 0) ?? false;
}
