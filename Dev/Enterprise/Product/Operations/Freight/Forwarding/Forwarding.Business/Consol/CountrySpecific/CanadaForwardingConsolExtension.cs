using System;
using CargoWise.Types;

namespace Enterprise.Freight.Forwarding.Business
{
	public static class CanadaForwardingConsolExtension
	{
		public static ZBool IsDestinationToCanada(this ForwardingConsol consol)
		{
			return (consol != null && consol.JK_RL_NKDischargePort.StartsWith(Core.Constants.CountryCodes.Canada, StringComparison.Ordinal) && !consol.JK_RL_NKLoadPort.StartsWith(Core.Constants.CountryCodes.Canada, StringComparison.Ordinal));
		}
	}
}
