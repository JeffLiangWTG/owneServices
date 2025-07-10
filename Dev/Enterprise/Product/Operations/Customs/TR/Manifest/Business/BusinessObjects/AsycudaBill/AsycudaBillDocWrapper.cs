using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.TR.Manifest.Business
{
	public class AsycudaBillDocWrapper : DocumentWrapper
	{
		public AsycudaBillDocWrapper(AsycudaBill bill)
		{
			Bill = bill;
		}

		public AsycudaBill Bill { get; }

		List<AsycudaPack> Packs
		{
			get
			{
				if (packs == null)
				{
					packs = Bill.Cast<AsycudaBill>().SelectMany(bill => bill.Packs.Cast<AsycudaPack>()).ToList();
				}
				return packs;
			}
		}
		List<AsycudaPack> packs;

		public ZInt TotalBillPackQuantity => Packs.Select(pack => (int)pack.APA_PackQty).Sum();

		public ZString BillPackGoodsDescription
		{
			get
			{
				var packedItems = Bill.Cast<AsycudaBill>().SelectMany(bill => bill.Packs.Cast<AsycudaPack>().SelectMany(pack => pack.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => x.PackedItem))).FirstOrDefault();

				return packedItems?.API_GoodsDescription ?? ZString.Empty;
			}
		}

		public ZString TotalBillPackedItemsGrossWeight => string.Format(DefaultCulture.Instance.NumberFormat, "{0:#,0.00}", Bill.Cast<AsycudaBill>().SelectMany(bill => bill.Packs.Cast<AsycudaPack>().SelectMany(pack => pack.PackedItems.Cast<ManifestBase.AsycudaPackPackedItemPivot>().Select(x => x.PackedItem))).Sum(x => Core.Constants.Weight.ConvertSafe(x.API_GrossWeight, x.API_GrossWeightUQ, Core.Constants.Weight.Kilograms)));

		public ZString ShortGoodsLocationDescription => ((ZString)Bill.Lookups.GoodsLocationList.GetDescriptionFromCode(Bill.ABL_GoodsLocation)).SubstringSafe(0, 10);
	}
}
