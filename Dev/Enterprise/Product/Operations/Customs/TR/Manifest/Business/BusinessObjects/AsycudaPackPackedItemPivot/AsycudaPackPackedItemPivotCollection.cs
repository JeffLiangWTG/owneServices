namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaPackPackedItemPivotCollection : ASYCUDA.Business.AsycudaPackPackedItemPivotCollection
	{
		public AsycudaPackPackedItemPivotCollection(AsycudaPackedItem packedItem)
			: base(packedItem)
		{
		}

		public AsycudaPackPackedItemPivotCollection(AsycudaPack pack)
			: base(pack)
		{
		}
	}
}
