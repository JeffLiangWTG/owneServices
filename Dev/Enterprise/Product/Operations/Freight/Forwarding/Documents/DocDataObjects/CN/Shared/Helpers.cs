using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.CN
{
	static class Helpers
	{
		#region GetBookingNumberWithFallback

		public static ZString GetBookingNumberWithFallback(PackLine packLine)
		{
			return string.IsNullOrWhiteSpace(packLine?.JL_ExportRefNumber)
				? (packLine?.Shipment
					?.Numbers
					.OfType<CusEntryNumber>()
					.FirstOrDefault(n => n.CE_EntryType == ChinaAdditionalReferenceNumberTypes.Codes.ShippingOrderNumber)?.CE_EntryNum ?? string.Empty)
				: packLine.JL_ExportRefNumber;
		}

		#endregion

		#region GetOperationalPort

		public static Unloco GetOperationalPort(this ForwardingConsol consol, IContext context)
		{
			return context != null
				? Unloco.Create(context, consol?.LoadPort)
				: null;
		}

		#endregion

		#region GetExportLeftFromChinaPort

		public static Freight.Business.Transport GetExportLegFromChina(this ForwardingConsol consol)
		{
			return consol
				?.Transports
				.OfType<Freight.Business.Transport>()
				.Where(t => t.JW_TransportMode == Core.Constants.TransportModes.Sea)
				.FirstOrDefault(t => t.JW_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase)
					&& !t.JW_RL_NKDiscPort.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase));
		}

		#endregion

		#region IsLoadingInNingboPort

		public static bool IsLoadingInNingboPort(this ITransport transport)
		{
			return transport?.PortOfLoading?.Code.IsNingboPort() ?? false;
		}

		#endregion

		#region IsNingboPort

		public static bool IsNingboPort(this ZString code) => CheckCodeInUnlocos(code, "CNNBO", "CNNGB", "CNNBG");

		#endregion

		#region IsShanghaiPort

		public static bool IsShanghaiPort(this ZString code) => CheckCodeInUnlocos(code, "CNSHA", "CNSGH", "CNSHG");

		#endregion

		#region IsLoadingInChina

		public static bool IsLoadingInChina(this ZString unloco)
		{
			return unloco.StartsWith(Core.Constants.CountryCodes.China, StringComparison.OrdinalIgnoreCase);
		}

		#endregion

		#region Implementation

		static bool CheckCodeInUnlocos(ZString code, params string[] unlocos)
		{
			const int unlocoLength = 5;

			if (code.IsEmpty
				|| code.Length != unlocoLength)
			{
				return false;
			}

			return unlocos.Any(unloco => string.CompareOrdinal(code, unloco) == 0);
		}

		#endregion
	}
}
