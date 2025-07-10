using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Agency.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Agency.DataTransfer
{
	internal class AgencyShipmentLocator<TShipment> where TShipment : AgencyShipment
	{
		readonly BusinessObjectFactory factory;
		readonly ZString houseBill;
		readonly ZString dischargePort;
		readonly ZString loadPort;
		readonly ZString vessel;
		readonly ZString voyageFlight;
		readonly ZGuid shippingLinePK;

		public AgencyShipmentLocator(BusinessObjectFactory factory, ZString houseBill, ZString loadPort, ZString dischargePort, ZString vessel, ZString voyageFlight, ZGuid shippingLinePK)
		{
			this.factory = factory;
			this.houseBill = houseBill;
			this.loadPort = loadPort;
			this.dischargePort = dischargePort;
			this.vessel = vessel;
			this.voyageFlight = voyageFlight;
			this.shippingLinePK = shippingLinePK;
		}

		public TShipment Find()
		{
			if (houseBill.IsEmpty)
			{
				return null;
			}

			ZQuery agencyShipmentQuery = new ZQuery(JobShipmentSchema.JS_HouseBill, houseBill);
			var agencyShipments = factory.Load<TShipment>(agencyShipmentQuery);

			foreach (var agencyShipment in agencyShipments)
			{
				if (!vessel.IsEmpty
					&& (agencyShipment.Sailing == null
					|| agencyShipment.Sailing.Voyage == null
					|| !AreEqualStringsIgnoringCase(agencyShipment.Sailing.Voyage.JV_RV_NKVessel, vessel)))
				{
					continue;
				}

				if (!voyageFlight.IsEmpty
					&& (agencyShipment.Sailing == null
					|| agencyShipment.Sailing.Voyage == null
					|| !AreEqualStringsIgnoringCase(agencyShipment.Sailing.Voyage.JV_VoyageFlight, voyageFlight)))
				{
					continue;
				}

				if (!shippingLinePK.IsEmpty
					&& (agencyShipment.Sailing == null
					|| agencyShipment.Sailing.Voyage == null
					|| shippingLinePK != agencyShipment.Sailing.Voyage.JV_OH_Line))
				{
					continue;
				}

				if (!loadPort.IsEmpty
					&& (agencyShipment.CalcLoadPort == null
					|| !AreEqualStringsIgnoringCase(agencyShipment.CalcLoadPort.RL_Code, loadPort)))
				{
					continue;
				}

				if (!dischargePort.IsEmpty
					&& (agencyShipment.CalcDischargePort == null
					|| !AreEqualStringsIgnoringCase(agencyShipment.CalcDischargePort.RL_Code, dischargePort)))
				{
					continue;
				}

				return agencyShipment;
			}

			return null;
		}

		bool AreEqualStringsIgnoringCase(ZString a, ZString b)
		{
			return (string.Compare(a, b, StringComparison.OrdinalIgnoreCase) == 0);
		}
	}
}
