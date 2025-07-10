using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.NL.NCTS.Business;

public class CC044CPackagingProvider : PackagingProvider
{
	public CC044CPackagingProvider(NctsPackage package) : base(package, package?.B5_SequenceNumber ?? ZShort.Zero, package is null || package.B5_TypeOfDifference == NctsUnloadedStateList.Codes.NEW)
	{
	}
}
