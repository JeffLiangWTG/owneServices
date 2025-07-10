using Enterprise.Freight.Forwarding.Business;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.FR
{
	public static class DemandeDeTracingHelper
	{
		public static bool IsTracingRequestApplicable(ForwardingConsol consol, DemandeDeTracingDirection direction)
		{
			if (consol == null
				|| !consol.TransportMode.EqualsIgnoringCase(Core.Constants.TransportModes.Sea))
			{
				return false;
			}

			switch (direction)
			{
				case DemandeDeTracingDirection.Export:
					return Core.Constants.CountryCodes.IsFranceOrTerritory(consol.JK_RL_NKLoadPort.Left(2));

				case DemandeDeTracingDirection.Import:
					return Core.Constants.CountryCodes.IsFranceOrTerritory(consol.JK_RL_NKDischargePort.Left(2));
			}

			return false;
		}
	}
}
