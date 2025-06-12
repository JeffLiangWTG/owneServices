using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class AirConnectionExtensions
	{
		public static IQueryable<eHubAirConnection> GetAirConnectionQuery(this IeHubTransactionsContext _context, Guid clientId)
		{
			return from ac in _context.eHubAirConnections where ac.AC_CC_Client == clientId select ac;
		}
	}
}