using Enterprise.ComplianceRisk.Integration;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.GUI
{
	public class ShipmentDetailsLayoutBuilder<T> : ColumnLayoutBuilder<T, ShipmentDetailsControlBag> where T : Business.BaseJobDeclaration
	{
		public override ShipmentDetailsControlBag CommonBag { get; } = ShipmentDetailsControlBag.Instance;

		protected override int MaxColumns => 1;

		protected override void SetDefaultVisibilities()
		{
			base.SetDefaultVisibilities();

			SetVisibility(CommonBag.ShipmentDetailsScreeningUserControl, h => h.Shipment == null && !((IComplianceItemRiskStatusProvider)h).IsEnabledComplianceWise, h => h.JE_JSInfo);
			SetVisibility(CommonBag.TotalNoOfPiecesCalcEdit, h => h.IsImport, h => h.JE_MessageTypeInfo);
			SetVisibility(CommonBag.ContainerCountCalcEdit, h => h.IsExport && h.IsSea, h => h.JE_TransportModeInfo);
		}
	}
}
