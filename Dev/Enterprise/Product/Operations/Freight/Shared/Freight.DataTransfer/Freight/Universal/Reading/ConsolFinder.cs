using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;

namespace Enterprise.Freight.DataTransfer.Universal
{
	public class ConsolFinder<TConsol> where TConsol : CommonConsol
	{
		public ConsolFinder(CommonShipment shipmentBO)
		{
			this.shipmentBO = Argument.NotNull(shipmentBO, "CommonShipment shipmentBO");
		}

		readonly CommonShipment shipmentBO;

		public TConsol GetBestMatchingConsolForShipmentIfExistsOtherwiseAddNew(Shipment shipmentDataObject, bool addConsolIfNoConsolsExists)
		{
			if (shipmentBO.Consols.Count > 0)
			{
				if (shipmentBO.Consols.Count == 1)
				{
					return (TConsol)shipmentBO.Consols[0];
				}
				else
				{
					var portOfLoading = shipmentDataObject.PortOfLoading.GetUNLOCOAsUpperCase(shipmentBO.Factory);
					var portOfDischarge = shipmentDataObject.PortOfDischarge.GetUNLOCOAsUpperCase(shipmentBO.Factory);
					var result = GetBestMatchingConsol(portOfLoading, portOfDischarge);

					return result ?? (TConsol)shipmentBO.Consols.GetLatestConsol();
				}
			}

			if (addConsolIfNoConsolsExists)
			{
				return (TConsol)shipmentBO.Consols.AddNew();
			}
			else
			{
				return null;
			}
		}

		public ITransportParent GetBestMatchingParentForTransport(TransportLeg transportLegDataObject)
		{
			var portOfLoading = transportLegDataObject.PortOfLoading.GetUNLOCOAsUpperCase(shipmentBO.Factory);
			var portOfDischarge = transportLegDataObject.PortOfDischarge.GetUNLOCOAsUpperCase(shipmentBO.Factory);
			var result = GetBestMatchingConsol(portOfLoading, portOfDischarge);

			if (result == null)
			{
				var finder = new TransportLegBusinessObjectFinder(transportLegDataObject, shipmentBO);
				var transport = finder.Find(shipmentBO.TransportsIncludingRelated.Cast<Transport>());

				if (transport != null && transport.Parent is CommonConsol)
				{
					result = (TConsol)transport.Parent;
				}
			}

			return (ITransportParent)result ?? shipmentBO;
		}

		TConsol GetBestMatchingConsol(ZString portOfLoading, ZString portOfDischarge)
		{
			TConsol result = null;
			var resultScore = 0;

			foreach (TConsol consol in shipmentBO.Consols)
			{
				var loadPortMatches = !portOfLoading.IsEmpty && consol.JK_RL_NKLoadPort == portOfLoading;
				var dischargePortMatches = !portOfDischarge.IsEmpty && consol.JK_RL_NKDischargePort == portOfDischarge;

				var score = 0;
				if (loadPortMatches && dischargePortMatches)
				{
					return consol;
				}
				else if (dischargePortMatches)
				{
					score = 2;
				}
				else if (loadPortMatches)
				{
					score = 1;
				}

				if (score > resultScore)
				{
					result = consol;
					resultScore = score;
				}
			}

			return result;
		}
	}
}
