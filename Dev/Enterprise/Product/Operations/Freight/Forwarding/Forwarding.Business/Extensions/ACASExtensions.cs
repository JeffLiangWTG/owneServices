using System;
using System.Linq;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ACASExtensions
	{
		public static bool ShouldApplyACAS(this ExportAWBHeader exportAwbHeader)
		{
			if (exportAwbHeader is ConsolExportAWBHeader consolExport)
			{
				return consolExport.Consol.ShouldApplyACAS();
			}

			if (exportAwbHeader is ShipmentExportAWBHeader shipmentExport)
			{
				return shipmentExport.Shipment.ShouldApplyACAS();
			}

			return false;
		}

		public static bool ShouldApplyACAS(this ForwardingConsol consol)
		{
			if (consol == null)
			{
				return false;
			}

			var originateFromUS = Core.Constants.CountryCodes.UsaAndTerritoriesList.Any(x => consol.JK_RL_NKLoadPort.StartsWith(x, StringComparison.Ordinal));
			if (originateFromUS)
			{
				return false;
			}

			var arriveOrTransportViaUS = Core.Constants.CountryCodes.UsaAndTerritoriesList.Any(x =>
				consol.JK_RL_NKDischargePort.StartsWith(x, StringComparison.Ordinal) ||
				consol.Transports.IsAnyDischargeInCountry(x));
			return arriveOrTransportViaUS;
		}

		public static bool ShouldApplyACAS(this ForwardingShipment shipment)
		{
			if (shipment == null || !shipment.IsFHLShipment())
			{
				return false;
			}

			var originateFromUS = Core.Constants.CountryCodes.UsaAndTerritoriesList.Any(x =>
				shipment.JS_RL_NKOrigin.StartsWith(x, StringComparison.Ordinal) ||
				shipment.JS_RL_NKLoadPort.StartsWith(x, StringComparison.Ordinal));
			if (originateFromUS)
			{
				return false;
			}

			var arriveOrTransportViaUS = Core.Constants.CountryCodes.UsaAndTerritoriesList.Any(x =>
				shipment.JS_RL_NKDestination.StartsWith(x, StringComparison.Ordinal) ||
				shipment.JS_RL_NKDischargePort.StartsWith(x, StringComparison.Ordinal) ||
				shipment.TransportsIncludingRelated.IsAnyDischargeInCountry(x));
			if (arriveOrTransportViaUS)
			{
				return true;
			}

			return shipment.Consols?.OfType<ForwardingConsol>().Any(ShouldApplyACAS) == true;
		}
	}
}
