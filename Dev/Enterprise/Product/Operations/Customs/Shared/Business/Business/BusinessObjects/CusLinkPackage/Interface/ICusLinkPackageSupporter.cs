using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.Business
{
	public enum PivotLevel
	{
		Invoice,
		InvoiceLine
	}

	public interface ICusLinkPackageSupporter : IBusiness
	{
		PivotLevel PivotLevel { get; }

		ZBool IsSupportPivot { get; }

		BaseJobDeclaration Declaration { get; }

		IBasePackagePivotCollection CusPackPivots { get; }

		BaseCusLinkPackageValidation GetNewLinkPackValidation(BaseCusLinkPackage linkPackage);

		ICusPackagePivot ToggleLinkageWithPackage(BasePackage package, bool value);

		ZBool IsSupportEmptyPackType(BasePackage package);

		HashSet<ZString> DistinctPackageTypes { get; }

		public int GetChildPackageUsage(BasePackage parentPackage);

		public void SyncParentPivotPackNum(BasePackage parentPackage);
	}
}
