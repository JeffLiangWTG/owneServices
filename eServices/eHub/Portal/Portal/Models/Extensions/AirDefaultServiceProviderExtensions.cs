using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class AirDefaultServiceProviderExtensions
	{
		public static IQueryable<eHubAirDefaultServiceProvider> GetMessageTypeRouting(this IeHubTransactionsContext _context, Guid clientId)
		{
			return from dfp in _context.eHubAirDefaultServiceProviders where dfp.AD_CC_Client == clientId select dfp;
		}
	}
}