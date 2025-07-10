using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaBillPackedItemCollection : ManifestBase.AsycudaBillPackedItemCollection<AsycudaPackedItem, AsycudaBill>
	{
		public AsycudaBillPackedItemCollection(AsycudaBill bill) : base(bill)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			if (child is AsycudaPackedItem packedItem)
			{
				packedItem.API_RX_NKGoodsValueCurrency = Master.ABL_RX_NKGoodsValueCurrency;
				packedItem.API_NetWeightUQ = Core.Constants.Weight.Kilograms;
			}
		}
	}
}
