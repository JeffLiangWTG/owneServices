using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
    public static class AirConnectionPerBranchExtension
    {
        public static IQueryable<eHubAirConnectionPerBranch> GetAirConnectionPerBranchQuery(this IeHubTransactionsContext _context, Guid clientId, Guid serviceProviderId)
        {
            return (from ac in _context.eHubAirConnectionPerBranches where ac.AB_CC_Client == clientId && ac.AB_CC_AirServiceProvider == serviceProviderId select ac);
        }

        public static IQueryable<eHubAirConnectionPerBranch> GetAirConnectionPerBranchQuery(this IeHubTransactionsContext _context, Guid clientId, List<eHubAirConnection> airConnections)
        {
            var airServiceProviders = from asp in airConnections select asp.AC_CC_AirServiceProvider;
            return from ac in _context.eHubAirConnectionPerBranches 
                   where ac.AB_CC_Client == clientId && airServiceProviders.Contains(ac.AB_CC_AirServiceProvider)
                   select ac;
        }
    }
}