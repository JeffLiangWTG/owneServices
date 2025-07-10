
namespace Enterprise.Freight.Business.Extensions
{
	using System.Collections.Generic;
	using CargoWise.EntityFramework;
	using CargoWise.Types;
	using Enterprise.MasterFiles.Business;

	public static class TransportExtensions
	{
		public static Transport Linked(this Transport transport, bool isLinked = true)
		{
			transport.JW_IsLinked = isLinked;

			return transport;
		}

		public static Transport WithSailing(this Transport transport, ZGuid sailingPK)
		{
			transport.JW_JX = sailingPK;

			return transport;
		}

		public static IEnumerable<ScreeningParty> GetScreeningPartiesFromVessel(this Transport transport, BusinessObject parentToAdd, bool allowInlandWaterwayTransport = false)
		{
			var partiesFromVessel = System.Array.Empty<ScreeningParty>();

			if ((transport.JW_TransportMode == Core.Constants.TransportModes.Sea || allowInlandWaterwayTransport && transport.JW_TransportMode == Core.Constants.TransportModes.InlandWaterwayTransport)
				&& !string.IsNullOrWhiteSpace(transport.JW_Vessel))
			{
				if (transport.Vessel != null)
				{
					partiesFromVessel = ((IScreeningPartyProvider)transport.Vessel).ScreeningParties;
				}
				else
				{
					partiesFromVessel = ((IScreeningPartyProvider)transport).ScreeningParties;
				}

				foreach (ScreeningParty party in partiesFromVessel)
				{
					party.AddParent(parentToAdd);
					party.LinkEntityToAssociatedJobIfApplicable(parentToAdd);
				}
			}

			return partiesFromVessel;
		}
	}
}
