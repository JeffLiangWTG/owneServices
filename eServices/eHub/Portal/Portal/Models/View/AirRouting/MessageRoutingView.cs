using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.eHub.Portal.Models.eHubTransactions;
using CargoWise.eHub.Portal.Models.Extensions;

namespace CargoWise.eHub.Portal.Models.View
{
	public class MessageRoutingView
	{
		public eHubClient Client { get; set; }
		public ClientProviderView ClientProvider { get; set; }
		public List<AirlineProviderView> AirlineProvider { get; set; }

		public MessageRoutingView(IeHubTransactionsContext context, Guid clientId) 
		{
			Client = context.GetClient(clientId);

			var serviceProviderId = context.GetClientRouting(clientId);
			ClientProvider = new ClientProviderView(context, serviceProviderId);

			AirlineProvider = new List<AirlineProviderView>();
			var airlineRouting = context.GetAirlineRouting(clientId).ToList();

			foreach( var airlineProvider in airlineRouting)
			{
				var airlineProviderView = new AirlineProviderView(context, airlineProvider.AM_CC_AirServiceProvider, airlineProvider.AM_DT_MessageType, airlineProvider.AM_CC_Airline, airlineProvider.AM_RecipientAddress, airlineProvider.AM_ClientPIMA, airlineProvider.AM_MessagePriority, airlineProvider.AM_DoubleSignatureCode, airlineProvider.AM_ShipmentOrigin);
				AirlineProvider.Add(airlineProviderView);
			}

		}
	}
}