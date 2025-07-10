using System;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class ExtensionsForJobConsolWorkflow
	{
		public static bool IsAir(this ForwardingConsol consol)
		{
			return consol != null && consol.IsAir;
		}

		public static bool IsSea(this ForwardingConsol consol)
		{
			return consol != null && consol.IsSea;
		}

		public static bool IsRail(this ForwardingConsol consol)
		{
			return consol != null && consol.IsRail;
		}

		public static bool IsAir(this ProcessTaskTemplate template)
		{
			return template != null && template.P0_SubType1 == Constants.TransportModes.Air;
		}

		public static bool IsEmpty(this ProcessTaskTemplate template)
		{
			return template != null && template.P0_SubType1.IsEmpty;
		}

		public static bool IsSea(this ProcessTaskTemplate template)
		{
			return template != null && template.P0_SubType1 == Constants.TransportModes.Sea;
		}

		public static bool IsRail(this ProcessTaskTemplate template)
		{
			return template != null && template.P0_SubType1 == Constants.TransportModes.Rail;
		}

		public static bool IsImportTo(this ForwardingConsol consol, string countryCode)
		{
			return
				consol != null &&
				consol.JK_RL_NKDischargePort.StartsWith(countryCode, StringComparison.Ordinal) &&
				!consol.JK_RL_NKLoadPort.StartsWith(countryCode, StringComparison.Ordinal);
		}

		public static bool IsExportFrom(this ForwardingConsol consol, string countryCode)
		{
			return
				consol != null &&
				consol.JK_RL_NKLoadPort.StartsWith(countryCode, StringComparison.Ordinal) &&
				!consol.JK_RL_NKDischargePort.StartsWith(countryCode, StringComparison.Ordinal);
		}

		public static bool IsImportTo(this ProcessTaskTemplate template, string countryCode)
		{
			return
				template != null &&
				template.P0_DischargePortCountry.StartsWith(countryCode, StringComparison.Ordinal) &&
				!template.P0_LoadPortCountry.StartsWith(countryCode, StringComparison.Ordinal);
		}
	}
}
