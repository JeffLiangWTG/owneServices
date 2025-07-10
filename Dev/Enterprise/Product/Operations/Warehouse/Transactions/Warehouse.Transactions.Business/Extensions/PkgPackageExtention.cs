using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Packing.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Warehouse.Transactions.Business
{
	public static class PkgPackageExtensions
	{
		#region IsTote

		public static bool GetIsTote(this PkgPackage package)
		{
			return package.GetSystemDefinedValue<ZBool>(TotePackage);
		}

		public static void SetIsTote(this PkgPackage package, bool isTote)
		{
			package.SetSystemDefinedValue(TotePackage, (ZBool)isTote);
			package.ClearActionStrategyCacheIncludingChildren();
		}

		public const string TotePackage = "TotePackage";

		public static bool IsToteOrParentIsTote(this PkgPackage package)
		{
			return package.GetIsTote() || (package.ParentPackage?.IsToteOrParentIsTote() ?? false);
		}

		#endregion

		#region MostRecentAudit

		public static WhsPackageAudit GetMostRecentAudit(this PkgPackage package)
		{
			WhsPackageAudit audit = null;
			var parentJobPK = package?.PackageJob?.ParentJob?.PK ?? ZGuid.Empty;
			if (!parentJobPK.IsEmpty)
			{
				var auditQuery = new ZQuery(WhsPackageAuditSchema.WPA_PackageID, package.KP_PackageID);
				auditQuery.AddToFilter(WhsPackageAuditSchema.WPA_WD_Order, parentJobPK);
				auditQuery.OrderBy = string.Format(CultureInfo.InvariantCulture, "{0} {1}", WhsPackageAuditSchema.Constants.WPA_AuditCompleteTime, OrderByClause.Descending);
				audit = package.Factory.LoadTop1<WhsPackageAudit>(auditQuery);
			}

			return audit;
		}

		#endregion

		#region GetPickLines

		public static IEnumerable<WhsPickLine> GetPickLines(this PkgPackage package)
		{
			var packedItems = package.PackedItemDivots.Select(d => d.PackedItem);
			foreach (var packedItem in packedItems.WhereNotNull())
			{
				yield return (WhsPickLine)packedItem;
			}
		}

		#endregion

		#region CannotDeleteAndUnpackPackage

		public static bool CannotDeleteAndPackUnpackPackage(this PkgPackage package)
		{
			Argument.NotNull(package, nameof(package));

			return (package.PackedItemDivots.Count > 0 || package.Packages.Count > 0)
				&& package.GetPickLines().Any(line => !line.IsPickedFromPutawayLocation);
		}

		#endregion

		#region HasLoadedPivot

		public static bool HasLoadedPivot(this PkgPackage package)
		{
			Argument.NotNull(package, nameof(package));

			var pivotQuery = new ZQuery(WhsLoadPkgPackagePivotSchema.WLP_KP_Package, package.PK);
			pivotQuery.AddToFilter(WhsLoadPkgPackagePivotSchema.WLP_UnloadedTime, SQLComparisonOperator.Equal, null);
			var pivot = package.Factory.LoadTop1<WhsLoadPkgPackagePivot>(pivotQuery);
			return pivot != null && pivot.IsLoaded;
		}

		#endregion
	}
}
