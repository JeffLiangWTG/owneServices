using CargoWise.EntityFramework;

namespace Enterprise.Customs.TR.ETrade.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master)
			: base(master)
		{ }

		protected override void OnCountChanged(CollectionCountChangedEventArgs e)
		{
			base.OnCountChanged(e);
			var header = Master;
			header?.ReCalcNumberOfBills();
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			base.RemoveAndDelete(elementToDelete);
			var header = Master;
			header?.ReCalcTotalBoxQty();
			header?.ReCalcTotalCustomsValue();
			header?.ReCalcFreightValue();
			header?.ReCalcInsuranceValue();
			header?.ReCalcOtherValue();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var bill = child as AsycudaBill;
			if (bill.Lookups.Containers.Count == 1)
			{
				bill.ContainerNumber = bill.Lookups.Containers[0].ACN_ContainerNumber;
			}

			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_ManifestUQ = AsycudaPack.PackTypes.Bin;
		}
	}
}
