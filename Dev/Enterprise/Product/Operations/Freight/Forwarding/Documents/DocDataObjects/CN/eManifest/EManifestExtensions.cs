using System;
using System.Linq;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects
{
	static class EManifestExtensions
	{
		public static bool IsLoadingInNingboPort(this EManifest eManifest)
		{
			return eManifest.OperationalPort?.Code.IsNingboPort() ?? false;
		}

		public static bool IsLoadingInShanghaiPort(this EManifest eManifest)
		{
			return eManifest.OperationalPort?.Code.IsShanghaiPort() ?? false;
		}

		public static bool IsLoadingInChina(this EManifest eManifest)
		{
			var firstSeaTransportLeg = GetFirstSeaTransport(eManifest);
			return firstSeaTransportLeg?.PortOfLoading?.Code.IsLoadingInChina() ?? false;
		}

		public static ITransport GetFirstSeaTransport(this EManifest eManifest)
		{
			return eManifest
				?.Transports
				?.FirstOrDefault(t => string.CompareOrdinal(t.Mode.Code, Core.Constants.TransportModes.Sea) == 0);
		}

		public static ForwardingConsol GetExportConsolFromChina(this ForwardingShipment shipment)
		{
			return shipment?.Consols.Cast<ForwardingConsol>().FirstOrDefault(c => c.Transports.Cast<Freight.Business.Transport>()
				.Any(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea
					&& t.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase)
					&& !t.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase)));
		}
	}
}
