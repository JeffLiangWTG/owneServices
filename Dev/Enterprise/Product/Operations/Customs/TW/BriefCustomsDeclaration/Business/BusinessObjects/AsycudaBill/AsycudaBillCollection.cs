using CargoWise.EntityFramework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business
{
	public class AsycudaBillCollection : ASYCUDA.Business.AsycudaBillCollection<AsycudaBill, AsycudaManifestHeader>
	{
		public AsycudaBillCollection(AsycudaManifestHeader master) : base(master)
		{
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);

			var bill = (AsycudaBill)child;
			bill.ABL_GrossWeightUQ = Core.Constants.Weight.Kilograms;
			bill.ABL_RX_NKCustomsValueCurrency = Core.Constants.CurrencyCodes.Taiwan;
		}
	}
}
