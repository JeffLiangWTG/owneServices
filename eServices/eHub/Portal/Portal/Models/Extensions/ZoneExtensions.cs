using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class ZoneExtensions
	{
		public static IQueryable<eHubZone> GetZoneQuery(this IeHubTransactionsContext _context)
		{
			return from z in _context.eHubZones select z;
		}

		public static eHubZone GetZone(this IeHubTransactionsContext _context, Guid? id)
		{
			return (from z in _context.eHubZones where z.ZZ_PK == id select z).ToList().FirstOrDefault();
		}
	}
}	