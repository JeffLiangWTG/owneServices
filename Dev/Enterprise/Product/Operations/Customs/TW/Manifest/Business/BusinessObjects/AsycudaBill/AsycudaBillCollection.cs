using CargoWise.EntityFramework;
using Enterprise.Customs.TW.Business;

namespace Enterprise.Customs.TW.Manifest.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master)
		{ }

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var bill = (AsycudaBill)child;
			bill.ABL_MessageStatus = TWMessageStatusCodeList.Codes.NotSent;
			bill.ABL_GoodsLocation = Master.AMA_GoodsLocationFromMasterBill;
		}

		public override void Add(BusinessObject businessObject)
		{
			base.Add(businessObject);

			var bill = (AsycudaBill)businessObject;
			if (bill.ABL_GoodsLocation.IsEmpty)
			{
				bill.ABL_GoodsLocation = Master.AMA_GoodsLocationFromMasterBill;
			}
		}
	}
}
