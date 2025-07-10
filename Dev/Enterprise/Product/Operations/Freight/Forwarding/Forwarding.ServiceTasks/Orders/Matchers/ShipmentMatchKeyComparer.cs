using System.Collections.Generic;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.ServiceTasks.Orders
{
	class ShipmentMatchKeyComparer : IEqualityComparer<ShipmentMatchKey>
	{
		bool IEqualityComparer<ShipmentMatchKey>.Equals(ShipmentMatchKey x, ShipmentMatchKey y)
		{
			if (x == y)
			{
				return true;
			}
			else if (x == null || y == null)
			{
				return false;
			}

			if (x.PlannedShipment != null && y.PlannedShipment != null)
			{
				if (x.PlannedShipment.PK == y.PlannedShipment.PK)
				{
					return x.PackedConsol.PK == y.PackedConsol.PK;
				}
				else
				{
					return false;
				}
			}

			return x.PackedConsol?.PK == y.PackedConsol?.PK
				&& x.SupplierBooking?.PK == y.SupplierBooking?.PK
				&& x.LoadMode == y.LoadMode
				&& x.ConsigneeAddress?.PK == y.ConsigneeAddress?.PK;
		}

		int IEqualityComparer<ShipmentMatchKey>.GetHashCode(ShipmentMatchKey obj)
		{
			if (obj == null)
			{
				return 0;
			}

			if (obj.PlannedShipment != null)
			{
				return obj.PlannedShipment.PK.GetHashCode();
			}

			unchecked
			{
				var hashCode = -1792165137;
				hashCode = hashCode * -1521134295 + obj.LoadMode.GetHashCode();
				hashCode = hashCode * -1521134295 + (obj.ConsigneeAddress?.PK ?? ZGuid.Empty).GetHashCode();
				hashCode = hashCode * -1521134295 + (obj.PackedConsol?.PK ?? ZGuid.Empty).GetHashCode();
				hashCode = hashCode * -1521134295 + (obj.SupplierBooking?.PK ?? ZGuid.Empty).GetHashCode();
				return hashCode;
			}
		}
	}
}
