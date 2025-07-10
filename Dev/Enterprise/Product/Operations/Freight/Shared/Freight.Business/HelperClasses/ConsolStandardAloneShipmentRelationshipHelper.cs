using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Freight.Business
{
	public static class ConsolStandardAloneShipmentRelationshipHelper
	{
		public static void AddStandAloneShipmentToConsols(List<CommonConsol> consols, CommonShipment shipment)
		{
			if (shipment != null && shipment.IsStandAloneShipmentFromBooking && consols != null && consols.Any())
			{
				var firstConsol = consols.FirstOrDefault(c => c != null && !shipment.JS_RL_NKOrigin.IsEmpty && c.JK_RL_NKLoadPort == shipment.JS_RL_NKOrigin);
				if (firstConsol == null)
				{
					var sortedConsols = consols.ToArray();
					MovementLegComparer.SortMovementLegsByPorts(sortedConsols);
					firstConsol = sortedConsols.FirstOrDefault();
				}

				if (firstConsol != null)
				{
					var helper = new BuildConsolHelper();
					helper.AddStandAloneShipmentToConsol(firstConsol, shipment);
				}
			}
		}

		public static void AddStandAloneSubShipmentsToMasterShipment(List<CommonShipment> subShipments, CommonShipment parentShipment)
		{
			if (parentShipment != null && parentShipment.IsLeadOrMaster && subShipments != null && subShipments.Any())
			{
				var consols = parentShipment.Consols.OfType<CommonConsol>().ToList();
				if (consols.Any())
				{
					foreach (var subShipment in subShipments)
					{
						AddStandAloneShipmentToConsols(consols, subShipment);
					}
				}
			}
		}

		public static void AddStandAloneShipmentsToConsol(List<CommonShipment> shipments, CommonConsol consol)
		{
			if (consol != null && shipments != null && shipments.Any())
			{
				foreach (var shipment in shipments.Where(s => s != null && s.IsStandAloneShipmentFromBooking))
				{
					var helper = new BuildConsolHelper();
					helper.AddStandAloneShipmentToConsol(consol, shipment);
				}
			}
		}

		public static void MakeConsolFromStandaloneShipment(CommonConsol consol, CommonShipment shipment)
		{
			if (consol != null && shipment != null && shipment.IsStandAloneShipmentFromBooking)
			{
				var helper = new BuildConsolHelper();
				helper.MakeConsolFromBookingOrStandaloneShipment(consol, shipment.PK);
			}
		}
	}
}
