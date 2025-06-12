using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CargoWise.eHub.Portal.Models.eHubTransactions;

namespace CargoWise.eHub.Portal.Models.Extensions
{
	public static class AirServiceProviderMappingExtensions
	{
		public static IQueryable<eHubAirServiceProviderMapping> GetAirlineRouting(this IeHubTransactionsContext _context, Guid clientId)
		{
			return from spm in _context.eHubAirServiceProviderMappings where spm.AM_CC_Client == clientId select spm;
		}

		public static eHubAirServiceProviderMapping GetAirServiceProviderMappings(this IeHubTransactionsContext _context, Guid clientId, Guid airlineId, Guid messageTypeId, string recipientAddress)
		{
			return (from spm in _context.eHubAirServiceProviderMappings where spm.AM_CC_Client == clientId 
			                                                                  && spm.AM_CC_Airline == airlineId 
			                                                                  && spm.AM_DT_MessageType == messageTypeId 
			                                                                  && (recipientAddress == null || spm.AM_RecipientAddress == recipientAddress)
																		select spm).FirstOrDefault();
		}
	}
}