using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Packing.Business
{
	public static class IPackableItemExtensions
	{
		public static bool IsUnpacked(this IPackableItem packableItem, BusinessObjectFactory factory)
		{
			Argument.NotNull(packableItem, nameof(packableItem));
			Argument.NotNull(factory, nameof(factory));

			return !GetDivot(packableItem, factory).Any();
		}

		public static ZDecimal GetPackedQty(this IPackableItem packableItem, BusinessObjectFactory factory)
		{
			var divot = GetDivot(packableItem, factory);
			return divot.Sum(d => d.KI_PackedQty);
		}

		static IEnumerable<PkgPackageItemDivot> GetDivot(IPackableItem packableItem, BusinessObjectFactory factory)
		{
			var query = new ZQuery(PkgPackageItemDivotSchema.KI_ParentID, packableItem.PK);
			query.FetchOnlyFromLocalCache = !packableItem.IsInDatabase;
			return factory.Load<PkgPackageItemDivot>(query);
		}
	}
}
