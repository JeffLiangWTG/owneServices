using System.Linq;

namespace Enterprise.Customs.Business
{
	public static class PackageSupporterExtension
	{
		public static ICusPackagePivot ToggleLinkageWithPackageCore(this ICusLinkPackageSupporter supporter, BasePackage package, bool value)
		{
			ICusPackagePivot pivot = null;

			var packagePivots = supporter != null && package != null ? supporter.CusPackPivots : null;

			if (packagePivots != null)
			{
				if (value)
				{
					pivot = packagePivots.AddPivotFor(package);
				}
				else
				{
					bool isParentOfExistingPackage = packagePivots.Cast<ICusPackagePivot>().Any(p => p.Package.CW_CW_Parent == package.PK);

					if (!isParentOfExistingPackage)
					{
						packagePivots.DeletePivotFor(package);
					}
				}

				supporter.ToggleLinkageWithPackage(package.ParentPackage, value);
			}

			return pivot;
		}
	}
}
