using System.Collections.Generic;
using System.Linq;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Rating.Web.Model.Conversion
{
	/// <summary>
	/// This utility class provides Creditor related functionality for RateQueryBusinessObject.
	/// </summary>
	public static class RateQueryCreditorPrioritizer
	{
		/// <summary>
		/// This method converts a given RateQueryBusinessObject to a hypothetical shipment, and returns its Creditors.
		/// </summary>
		/// <param name="rateQuery"></param>
		/// <returns></returns>
		public static Creditors GetCreditors(this RateQueryBusinessObject rateQuery)
		{
			var shipment = RateQueryToShipmentConverter.CreateSingleConsolidatedShipment(rateQuery, false);
			return shipment.GetCreditors();
		}

		internal static Dictionary<string, OrgHeader> GetResolvedRateParties(this RateQueryBusinessObject rateQuery)
		{
			var result = new Dictionary<string, OrgHeader>();

			if (rateQuery.RateQuery.RateParties != null)
			{
				foreach (var org in rateQuery.RateQuery.RateParties.Where(o => o != null))
				{
					var orgHeader = rateQuery.Factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, org.Code);
					if (orgHeader != null)
					{
						result[org.Role.ToUpperInvariant()] = orgHeader;
					}
				}
			}

			return result;
		}
	}
}
