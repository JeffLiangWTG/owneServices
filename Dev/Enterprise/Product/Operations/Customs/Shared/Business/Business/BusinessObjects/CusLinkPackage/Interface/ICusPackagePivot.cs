using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public interface ICusPackagePivot : IBusiness
	{
		ZGuid PackagePK { get; set; }

		ZGuid ParentPK { get; }

		ZGuid DeclarationPK { get; set; }

		ZInt NumberOfPacks { get; set; }

		BasePackage Package { get; }

		bool IsDeleted { get; }

		ICusLinkPackageSupporter PivotSupporter { get; }
	}
}
