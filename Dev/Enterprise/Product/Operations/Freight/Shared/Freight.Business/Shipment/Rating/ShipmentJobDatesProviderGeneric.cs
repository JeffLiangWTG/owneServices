using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Business
{
	public abstract class ShipmentJobDatesProvider<T> : JobDatesProvider<CommonShipment>
		where T : CommonShipment
	{
		protected ShipmentJobDatesProvider(T shipment)
			: base(shipment) { }

		protected override ZDateTime GetVesselArrivalDateCore()
		{
			var result = ZDateTime.Empty;

			if (Parent.Sailing != null && Parent.Sailing.Destination != null)
			{
				if (Parent.Sailing.Destination.JB_A_ARV.IsValid)
				{
					result = Parent.Sailing.Destination.JB_A_ARV;
				}

				if (result == ZDateTime.Empty && Parent.Sailing.Destination.JB_E_ARV.IsValid)
				{
					result = Parent.Sailing.Destination.JB_E_ARV;
				}
			}

			if (result == ZDateTime.Empty && Parent.JS_E_ARV.IsValid)
			{
				result = Parent.JS_E_ARV;
			}

			return result;
		}

		protected override ZDateTime GetVesselDepartureDateCore()
		{
			var result = ZDateTime.Empty;

			if (Parent.Sailing != null && Parent.Sailing.Origin != null)
			{
				if (Parent.Sailing.Origin.JA_A_DEP.IsValid)
				{
					result = Parent.Sailing.Origin.JA_A_DEP;
				}

				if (result == ZDateTime.Empty && Parent.Sailing.Origin.JA_E_DEP.IsValid)
				{
					result = Parent.Sailing.Origin.JA_E_DEP;
				}
			}

			if (result == ZDateTime.Empty && Parent.JS_E_DEP.IsValid)
			{
				result = Parent.JS_E_DEP;
			}

			return result;
		}
	}
}
