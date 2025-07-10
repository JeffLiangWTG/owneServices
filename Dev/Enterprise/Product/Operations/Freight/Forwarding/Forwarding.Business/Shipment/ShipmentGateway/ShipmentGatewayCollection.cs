using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Business
{
	public class ShipmentGatewayCollection : ActiveBusinessObjectCollection<ShipmentGateway>
	{
		public ShipmentGatewayCollection(ForwardingShipment shipment)
			: base(shipment.Factory, new DependentRelationship(shipment, typeof(ShipmentGateway), new ZQuery(JobShipmentGatewaySchema.JSG_JS_Shipment, shipment.PK), JobShipmentGatewaySchema.JSG_JS_Shipment))
		{
		}

		#region Adding

		protected override void SetDefaultsForNewElementCore(ShipmentGateway newElement)
		{
			base.SetDefaultsForNewElementCore(newElement);
			newElement.JSG_Sequence = this.Any() ? (byte)(this.Max(x => x.JSG_Sequence) + 1) : (byte)1;
		}

		protected override bool AllowNew => !this.Any() || this.Max(x => x.JSG_Sequence) < byte.MaxValue;

		#endregion

		#region Deleting

		public override void Delete(ShipmentGateway elementToDelete)
		{
			var deletedElementSequence = elementToDelete.JSG_Sequence;

			base.Delete(elementToDelete);

			foreach (var gateway in this.Where(x => x.JSG_Sequence > deletedElementSequence))
			{
				gateway.JSG_Sequence--;
			}
		}

		#endregion

		#region Re-ordering

		public bool SwapGateways(byte sequence1, byte sequence2)
		{
			var gateway1 = this.FirstOrDefault(x => x.JSG_Sequence == sequence1);
			var gateway2 = this.FirstOrDefault(x => x.JSG_Sequence == sequence2);

			if (gateway1 != null && gateway2 != null)
			{
				var address1 = gateway1.JSG_OA_ForwarderAddress;
				gateway1.JSG_OA_ForwarderAddress = gateway2.JSG_OA_ForwarderAddress;
				gateway2.JSG_OA_ForwarderAddress = address1;

				foreach (var gateway in this)
				{
					gateway.Validation.ValidateJSG_OA_ForwarderAddress();
				}

				return true;
			}

			return false;
		}

		#endregion
	}
}
