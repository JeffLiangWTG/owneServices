using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.View;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class ClientExtensions
	{
		public static IQueryable<eHubClient> GetClientQuery(this IeHubTransactionsContext _context)
		{
			return from c in _context.eHubClients select c;
		}

		public static eHubClient GetClient(this IeHubTransactionsContext _context, Guid? id)
		{
			return (from c in _context.eHubClients where c.CC_PK == id select c).ToList().FirstOrDefault();
		}

		public static SelectValueView[] FindClientViewList(this IeHubTransactionsContext _context, string text)
		{
			return (from c in _context.eHubClients
					where c.CC_FriendlyName.StartsWith(text) || c.CC_ID.StartsWith(text)
					orderby c.CC_ID
					select new SelectValueView { Id = c.CC_PK, Code = c.CC_ID, Name = c.CC_FriendlyName }).Distinct().ToArray();
		}

		public static eHubClient CreateClient()
		{
			return new eHubClient() { CC_PK = Guid.NewGuid(), CC_DistributionZone = new Guid("75419F4C-C522-4890-BD5D-BCA5E12268F6"), CC_EmailAddress = "", CC_Password = "" };
		}

		public static IQueryable<eHubClient> GetAllAirServiceProvider(this IeHubTransactionsContext _context)
		{
			return from asp in _context.eHubClients where asp.CC_IsAirServiceProvider == true select asp;
		}

		public static IQueryable<eHubClient> GetAllAirline(this IeHubTransactionsContext _context)
		{
			return from asp in _context.eHubClients where asp.CC_AirlineCode != null orderby asp.CC_ID select asp;
		}

		public static Guid? GetClientRouting(this IeHubTransactionsContext _context, Guid clientId)
		{
			return (from c in _context.eHubClients where c.CC_PK == clientId select c.CC_AirServiceProvider).FirstOrDefault();
		}

		public static ClientView GetClientView(this IeHubTransactionsContext context, Guid id)
		{
			var client = context.GetClient(id);
			var clientView = new ClientView(client);
			clientView.DistributionZone = context.GetZoneQuery();
			clientView.AirServiceProvider = context.GetAllAirServiceProvider();

			var zone = context.GetZone(client.CC_DistributionZone);
			if (zone != null) clientView.DistributionZoneName = zone.ZZ_ID;
			var airServiceProvider = context.GetClient(client.CC_AirServiceProvider);
			if (airServiceProvider != null)
				clientView.AirServiceProviderName = String.Format("{0} ({1})", airServiceProvider.CC_FriendlyName,
																  airServiceProvider.CC_ID);
			return clientView;
		}
	}
}	