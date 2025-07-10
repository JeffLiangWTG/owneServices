using CargoWise.EntityFramework;

namespace Enterprise.Customs.SG.Access.Business
{
	public class AsycudaPackCollection : ASYCUDA.Business.AsycudaPackCollection<AsycudaPack, AsycudaBill>
	{
		public AsycudaPackCollection(AsycudaBill master)
			: base(master)
		{ }

		protected override void OnRemoving(BusinessObject bizo)
		{
			base.OnRemoving(bizo);
			Master.MarkApportionmentDirty();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var pack = (AsycudaPack)child;
			pack.LinePriceCurrency = Master.ABL_RX_NKFreightValueCurrency;
		}
	}
}
