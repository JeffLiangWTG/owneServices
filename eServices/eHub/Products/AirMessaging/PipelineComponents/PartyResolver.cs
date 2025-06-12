using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.DataAccess.Integration;

namespace CargoWise.eHub.Products.AirMessaging.PipelineComponents
{
	class PartyResolver
	{
		readonly string serviceProvider;

		public PartyResolver(string serviceProvider)
		{
			if (serviceProvider == null) throw new ArgumentNullException("serviceProvider");
			this.serviceProvider = serviceProvider;
		}

		public virtual string ResolveParty(string clientID)
		{
			if (!string.IsNullOrEmpty(clientID))
			{
				return PartyAccessor.GetClientIDFromAirPIMA(clientID, serviceProvider);
			}

			return null;
		}

		public virtual string ResolveRecipient(string clientPIMA, string clientAWB)
		{
			string clientId = ResolveClientAWB(clientAWB);
			return !string.IsNullOrEmpty(clientId) ? clientId : PartyAccessor.GetClientIDFromAirPIMA(clientPIMA, serviceProvider);
		}

		public virtual string ResolveAirline(string airlineCode)
		{
			if (!string.IsNullOrEmpty(airlineCode))
			{
				return PartyAccessor.GetClientIDFromAirlineCode(airlineCode, serviceProvider);
			}

			return null;
		}

		public virtual string ResolveClientPIMA(string clientPIMA)
		{
			if (!string.IsNullOrEmpty(clientPIMA))
			{
				return PartyAccessor.GetClientIDFromAirPIMA(clientPIMA, serviceProvider);
			}

			return null;
		}

		public virtual string ResolveClientAWB(string clientAWB)
        {
            if (!string.IsNullOrEmpty(clientAWB))
            {
				var clientID = PartyAccessor.GetClientIDFromClientAWB(clientAWB, serviceProvider);

				if (string.IsNullOrEmpty(clientID) && SubServiceProviders != null)
				{
					foreach (var serviceProviderId in SubServiceProviders)
					{
						if (!string.IsNullOrEmpty(serviceProviderId))
						{
							clientID = PartyAccessor.GetClientIDFromClientAWB(clientAWB, serviceProviderId.Trim());
							if (!string.IsNullOrEmpty(clientID)) break;
						}
					}
				}

				return clientID;
            }

            return null;
        }

        internal virtual IPartyAccessor PartyAccessor => DataAccessFactories.NewPartyAccessorInstance();
        internal IEnumerable<string> SubServiceProviders { get; set; }
	}
}
