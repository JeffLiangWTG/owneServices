using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	public class ShipmentContainerPenaltyCollection : ActiveBusinessObjectCollection<ShipmentContainerPenalty>
	{
		public ShipmentContainerPenaltyCollection(CommonShipment shipment, string processType)
			: base(shipment.Factory, new DependentRelationship(shipment, typeof(ContainerPenalty), new ZQuery(JobContainerPenaltySchema.CPY_JS_Shipment, shipment.PK), JobContainerPenaltySchema.CPY_JS_Shipment))
		{
			this.processType = processType;
			this.shipment = shipment;
			AdditionalFilter = new ZQuery(JobContainerPenaltySchema.CPY_ProcessType, processType);
		}

		protected readonly CommonShipment shipment;
		protected readonly string processType;

		protected override void OnAdded(ShipmentContainerPenalty businessObject)
		{
			base.OnAdded(businessObject);
			businessObject.CPY_ProcessType = processType;
		}
	}
}
