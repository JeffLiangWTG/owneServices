using CargoWise.Types;
using Enterprise.Freight.Business;

namespace Enterprise.Freight.Agency.Business
{
	public abstract class AgencyShipmentJobDatesProvider : ShipmentJobDatesProvider<AgencyShipment>
	{
		protected AgencyShipmentJobDatesProvider(AgencyShipment agencyShipment)
			: base(agencyShipment) { }

		protected override ZDateTime GetArrivalDateCore()
		{
			var result = ZDateTime.Empty;

			if (Parent.JS_E_ARV.IsValid)
			{
				result = Parent.JS_E_ARV;
			}
			else
			{
				var sailing = Parent.Sailing;
				if (sailing != null && sailing.JX_JB_A_ARV.IsValid)
				{
					result = sailing.JX_JB_A_ARV;
				}
				else if (sailing != null && sailing.JX_JB_E_ARV.IsValid)
				{
					result = sailing.JX_JB_E_ARV;
				}
			}

			return result;
		}

		protected override ZDateTime GetDepartureDateCore()
		{
			ZDateTime result = ZDateTime.Empty;

			if (Parent.JS_E_DEP.IsValid)
			{
				result = Parent.JS_E_DEP;
			}
			else
			{
				var sailing = Parent.Sailing;
				if (sailing != null && sailing.JX_JA_A_DEP.IsValid)
				{
					result = sailing.JX_JA_A_DEP;
				}
				else if (sailing != null && sailing.JX_JA_E_DEP.IsValid)
				{
					result = sailing.JX_JA_E_DEP;
				}
			}

			return result;
		}
	}
}
