using Enterprise.Customs.ASYCUDA.GUI;
using Enterprise.Customs.NZ.Manifest.Business;

namespace Enterprise.Customs.NZ.Manifest.GUI
{
	public class NZBillLayoutBuilder : BillLayoutBuilder<AsycudaBill>
	{
		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.CustomsEntryNumberTextBox, b => b.IsOCR, b => b.CustomsEntryNumberInfo);
			SetVisibility(CommonBag.ShipmentTypeDropEdit, b => !b.IsOCR, b => b.ABL_ShipmentTypeInfo);
		}
	}
}
