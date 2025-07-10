using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.PL.Business.Declaration;

public class PackingGroup : EU.Business.Declaration.PackingGroup,
	Integration.Customs.PL.IPackingGroup
{
	public PackingGroup(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	protected override bool ShouldCopyDeclarationTotalNoOfPacksCore => Packages != null
																		&& TotalPackageCount() == 0
																		&& !PackageHelper.IsBulkCode(Packages.Cast<BasePackage>().FirstOrDefault()?.CW_PackType ?? ZString.Empty, Factory);
}
