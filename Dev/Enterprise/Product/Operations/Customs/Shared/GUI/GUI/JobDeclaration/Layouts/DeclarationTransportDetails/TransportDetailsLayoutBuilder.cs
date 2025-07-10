using Enterprise.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class TransportDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, TransportDetailsControlBag> where T : Business.BaseJobDeclaration
	{
		public override TransportDetailsControlBag CommonBag { get; } = TransportDetailsControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.OverrideValuesCheckBox, h => !h.IsStandAlone, h => h.JE_JSInfo);
			SetVisibility(CommonBag.OceanBillTextBox, h => h.IsSea || Env.Registry.IsExpress, h => h.JE_TransportModeInfo);
			SetVisibility(CommonBag.VoyageNumberTextBox, h => h.IsSea, h => h.JE_TransportModeInfo);
			SetVisibility(CommonBag.VesselCodeFindBox, h => h.IsSea, h => h.JE_TransportModeInfo);
			SetVisibility(CommonBag.MasterBillTextBox, h => h.IsAir && !Env.Registry.IsExpress, h => h.JE_TransportModeInfo);
			SetVisibility(CommonBag.FlightUserControl, h => h.IsAir, h => h.JE_TransportModeInfo);
			SetVisibility(CommonBag.VehicleRegistrationNumberTextBox, h => h.IsRoad, h => h.JE_TransportModeInfo);
		}
	}
}
