using System;
using System.Data;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaPack : ASYCUDA.Business.AsycudaPack
	{
		public AsycudaPack(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override Type GetPackedItemTypeCore() => typeof(AsycudaPackedItem);

		public new AsycudaPackedItem GetPackedItemFromCollection() => (AsycudaPackedItem)base.GetPackedItemFromCollection();

		public new AsycudaPackedItem PackedItem => (AsycudaPackedItem)base.PackedItem;
	}
}
