using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public static class IPackableItemParentExtensions
	{
		public static ZDecimal GetPackedQty(this IPackableItemParent packableItemParent, BusinessObjectFactory factory)
		{
			const IReadOnlyCollection<PkgPackage> NullPackages = null;

			return packableItemParent.GetPackedQtyCore(NullPackages, factory);
		}

		public static ZDecimal GetPackedQty(this IPackableItemParent packableItemParent, PkgPackage package)
		{
			Argument.NotNull(package, nameof(package));
			return packableItemParent.GetPackedQty(new[] { package });
		}

		public static ZDecimal GetPackedQty(this IPackableItemParent packableItemParent, IReadOnlyCollection<PkgPackage> packages)
		{
			Argument.NotNull(packages, nameof(packages));
			return packableItemParent.GetPackedQtyCore(packages);
		}

		static ZDecimal GetPackedQtyCore(this IPackableItemParent packableItemParent, IReadOnlyCollection<PkgPackage> packages, BusinessObjectFactory factory = null)
		{
			return packableItemParent.GetPackedDivotsCore(packages, factory).Sum(d => d.KI_PackedQty);
		}

		public static PkgPackageItemDivot[] GetPackedDivots(this IPackableItemParent packableItemParent, PkgPackage package)
		{
			Argument.NotNull(package, nameof(package));
			return packableItemParent.GetPackedDivots(new[] { package });
		}

		public static PkgPackageItemDivot[] GetPackedDivots(this IPackableItemParent packableItemParent, IReadOnlyCollection<PkgPackage> packages)
		{
			Argument.NotNull(packages, nameof(packages));
			return packableItemParent.GetPackedDivotsCore(packages);
		}

		static PkgPackageItemDivot[] GetPackedDivotsCore(this IPackableItemParent packableItemParent, IReadOnlyCollection<PkgPackage> packages, BusinessObjectFactory factory = null)
		{
			Argument.NotNull(packableItemParent, nameof(packableItemParent));
			PkgPackageItemDivot[] result;

			var packableItems = new Lazy<IEnumerable<(ZGuid PK, bool IsInDatabase)>>(() => packableItemParent.PackableItems.Select(i => (i.PK, i.IsInDatabase)));

			if (packages == null)
			{
				Argument.NotNull(factory, nameof(factory));

				var divots = new List<PkgPackageItemDivot>();

				const bool IsInDatabase = true;
				var packableItemsGrouped = packableItems.Value.ToLookup(p => p.IsInDatabase);
				var inMemoryItems = packableItemsGrouped[!IsInDatabase].ToArray();
				if (inMemoryItems.Length > 0)
				{
					var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, inMemoryItems.Select(p => p.PK));
					query.FetchOnlyFromLocalCache = true;
					divots.AddRange(factory.Load<PkgPackageItemDivot>(query));
				}

				var inDBItems = packableItemsGrouped[IsInDatabase].ToArray();
				if (inDBItems.Length > 0)
				{
					var query = new ZQuery { AllowTableValuedParameters = true };
					query.AddToFilter(PkgPackageItemDivotSchema.KI_ParentID, inDBItems.Select(p => p.PK).ToArray());
					divots.AddRange(factory.Load<PkgPackageItemDivot>(query));
				}

				result = divots.ToArray();
			}
			else if (packages.Count > 0)
			{
				var packableItemLookup = packableItems.Value.Select(p => p.PK).ToHashSet();
				result = packages.SelectMany(p => p.PackedItemDivots).Where(pd => packableItemLookup.Contains(pd.KI_ParentID)).ToArray();
			}
			else
			{
				result = Array.Empty<PkgPackageItemDivot>();
			}

			return result;
		}
	}
}
