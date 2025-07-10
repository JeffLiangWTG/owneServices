using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class ShipmentTypeLayoutBuilder<T> : ColumnLayoutBuilder<T, ShipmentTypeControlBag> where T : Business.BaseJobDeclaration
	{
		public override ShipmentTypeControlBag CommonBag { get; } = ShipmentTypeControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();
			SetVisibility(CommonBag.ContainerModeDropEdit, h => h.ContainerModeVisible, h => h.JE_TransportModeInfo);
		}
	}
}
