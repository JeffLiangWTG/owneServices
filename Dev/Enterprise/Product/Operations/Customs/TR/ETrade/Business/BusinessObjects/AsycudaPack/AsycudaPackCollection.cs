using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaPackCollection : ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>
	{
		public AsycudaPackCollection(AsycudaBill master)
			: base(master)
		{
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);
			var bill = Master;
			bill?.ReCalcStatisticalValue();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var packedItem = (child as AsycudaPack)?.PackedItem;
			if (packedItem != null)
			{
				packedItem.API_RX_NKGoodsValueCurrency = Master.ABL_RX_NKGoodsValueCurrency;
				var balance = Master.ABL_GoodsValue - this.Cast<AsycudaPack>().Sum(x => x.PackedItem?.API_GoodsValue ?? ZDecimal.Zero);
				if (balance > 0)
				{
					packedItem.API_GoodsValue = balance;
				}
			}
		}
	}
}
