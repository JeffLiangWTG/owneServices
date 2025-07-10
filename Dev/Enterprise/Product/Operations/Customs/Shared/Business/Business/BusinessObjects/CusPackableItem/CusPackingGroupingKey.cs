using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Packing.Business;

namespace Enterprise.Customs.Business
{
	public class CusPackingGroupingKey : GroupingKey
	{
		CusPackingGroupingKey(ZGuid packableItemPK)
		{
			this.packableItemPK = packableItemPK;
		}

		readonly ZGuid packableItemPK;
		public static CusPackingGroupingKey New(CusPackableItem item)
		{
			Argument.NotNull(item, nameof(item));
			return new CusPackingGroupingKey(item.PK);
		}

		public override int GetHashCode() => packableItemPK.GetHashCode();

		protected override bool Equals(GroupingKey other)
		{
			var key = other as CusPackingGroupingKey;
			return key != null && packableItemPK == key.packableItemPK;
		}

		protected override bool IsSimilarItem_DoNotUseCore(GroupingKey otherKey)
		{
			var key = otherKey as CusPackingGroupingKey;
			return key != null && packableItemPK == key.packableItemPK;
		}
	}
}
