using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaPackedItemCollection : ASYCUDA.Business.AsycudaPackedItemCollection<AsycudaPackedItem, AsycudaPack>
	{
		public AsycudaPackedItemCollection(AsycudaPack pack)
			: base(pack)
		{
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			var result = default(ZQuery);
			if (!pack.IsInDatabase)
			{
				result = pack.GetPackedItemsFromPivotQuery();
			}
			else
			{
				result = base.CreateRelationshipFilter();
			}

			return result;
		}
	}
}
