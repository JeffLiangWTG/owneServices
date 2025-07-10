using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public static class PkgPackageHandlingUnitDivotHelper
	{
		public static void ReplaceParentPackageWithDivotForLabelledPackages(BusinessObjectFactory factory, PkgPackage inner, bool ignoreInnerPackageID = false)
		{
			var currentParent = inner.ParentPackage;
			if (currentParent != null &&
				currentParent.KP_KPH_PackageHeader.IsValid &&
				(inner.KP_KPH_PackageHeader.IsValid || ignoreInnerPackageID))
			{
				var packageHandlingUnitDivot = GetExistingPackageHandlingUnitDivot(factory, inner) ?? factory.New<PkgPackageHandlingUnitDivot>();
				packageHandlingUnitDivot.KPD_KP_HandlingUnit = currentParent.PK;
				packageHandlingUnitDivot.KPD_KP_Package = inner.PK;
				packageHandlingUnitDivot.KPD_PackedTime = ZDateTimeOffset.Now;
				packageHandlingUnitDivot.KPD_GS_NKPackedUser = Env.CurrentUser.Initials;

				var topHandlingUnit = GetTopLevelPackageViaDivots(inner);
				if (topHandlingUnit != inner)
				{
					// must be set to satisfy DB constraint
					inner.KP_KP_TopHandlingUnitPackage = topHandlingUnit.PK;
				}
			}
		}

		public static PkgPackage GetTopLevelPackageViaDivots(PkgPackage package)
		{
			var topLevelPackage = package;
			var parentHU = package.PackageHandlingUnitPackageDivots.FirstOrDefault(d => d.KPD_UnpackedTime.IsEmpty)?.HandlingUnit;
			while (parentHU != null)
			{
				topLevelPackage = parentHU;
				parentHU = topLevelPackage.PackageHandlingUnitPackageDivots.FirstOrDefault(d => d.KPD_UnpackedTime.IsEmpty)?.HandlingUnit;
			}

			return topLevelPackage;
		}

		public static List<PkgPackage> GetAllInnerPackagesViaDivots(PkgPackage package)
		{
			return GetAllInnerPackagesViaDivots(package?.PackageHandlingUnitHandlingUnitDivots);
		}

		public static List<PkgPackage> GetAllInnerPackagesViaDivots(IEnumerable<PkgPackageHandlingUnitDivot> packageHandlingUnitDivots)
		{
			var inners = new List<PkgPackage>();

			if (packageHandlingUnitDivots != null)
			{
				var divotsToCheck = new Queue<PkgPackageHandlingUnitDivot>(packageHandlingUnitDivots.Where(d => d.KPD_UnpackedTime.IsEmpty));

				while (divotsToCheck.Count > 0)
				{
					var nextPackage = divotsToCheck.Dequeue()?.Package;
					if (nextPackage != null)
					{
						inners.Add(nextPackage);
						nextPackage.PackageHandlingUnitHandlingUnitDivots.Where(d => d.KPD_UnpackedTime.IsEmpty).ForEach(d => divotsToCheck.Enqueue(d));
					}
				}
			}

			return inners;
		}

		static PkgPackageHandlingUnitDivot GetExistingPackageHandlingUnitDivot(BusinessObjectFactory factory, PkgPackage package)
		{
			var query = new ZQuery(PkgPackageHandlingUnitDivotSchema.KPD_KP_Package, package.PK);

			return factory.LoadTop1<PkgPackageHandlingUnitDivot>(query);
		}
	}
}
