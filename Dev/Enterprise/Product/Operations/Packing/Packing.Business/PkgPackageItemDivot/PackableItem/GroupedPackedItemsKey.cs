using CargoWise.Types;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Packing.Business
{
	[Immutable]
	public sealed class GroupedPackedItemsKey : GroupingKey
	{
		public GroupedPackedItemsKey(PkgPackageItemDivotsWrapper packedItem)
		{
			Key = packedItem.Key;
			PackagePK = packedItem.PackagePK;
		}

		public GroupedPackedItemsKey(GroupingKey packedItemKey, ZGuid packagePK)
		{
			Key = packedItemKey;
			PackagePK = packagePK;
		}

		readonly GroupingKey Key;
		readonly ZGuid PackagePK;

		public override int GetHashCode()
		{
			return Key.GetHashCode() ^ PackagePK.GetHashCode();
		}

		protected override bool Equals(GroupingKey other)
		{
			var otherPackedItemsKey = other as GroupedPackedItemsKey;
			return
				otherPackedItemsKey != null
				&& PackagePK == otherPackedItemsKey.PackagePK;
		}

		protected override bool IsSimilarItem_DoNotUseCore(GroupingKey otherKey)
		{
			return Equals(otherKey);
		}
	}
}
