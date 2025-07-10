using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Business
{
	internal static class VoyageChildHelper
	{
		public static IEnumerable<CommonConsol> GetConsolsWithShipmentsNotApprovedForPassengerFlights(this IEnumerable<JobSailing> sailings)
		{
			return GetConsolsRequiringPassengerFlightValidation(sailings)
				.Where(consol => consol.Shipments.Cast<CommonShipment>()
					.Any(shipment => !shipment.AviationSecurity.IsAllowedOnPassengerFlights()));
		}

		public static IEnumerable<CommonConsol> GetConsolsWithOrganisationsNotApprovedForShippingOnPassengerFlights(this IEnumerable<JobSailing> sailings)
		{
			return GetConsolsRequiringPassengerFlightValidation(sailings)
				.Where(x => x.Shipments.Cast<CommonShipment>()
					.Any(shipment => shipment.JS_InspectionTypeCode == BaseJobShipmentLookups.InspectionType_Approved
						&& !shipment.AviationSecurity.RelevantOrganisationsAreApprovedForShippingOnPassengerFlights));
		}

		static IEnumerable<CommonConsol> GetConsolsRequiringPassengerFlightValidation(IEnumerable<JobSailing> sailings)
		{
			var result = new List<CommonConsol>();

			if (sailings.Any())
			{
				var factory = sailings.First().Factory;

				var linkedConsolTransportsQuery = new ZQuery();
				linkedConsolTransportsQuery.AddToFilter(JobConsolTransportSchema.JW_ParentType, Constants.TransportParentTypes.Consol);
				linkedConsolTransportsQuery.AddToFilter(JobConsolTransportSchema.JW_JX, sailings.Select(sailing => sailing.PK));

				var consolPKs = factory.Load<Transport>(linkedConsolTransportsQuery).Select(transport => transport.JW_ParentGUID);
				if (consolPKs.Any())
				{
					var consolQuery = new ZQuery(JobConsolSchema.PK, consolPKs);

					result = factory
						.Load<CommonConsol>(consolQuery)
						.Where(FreightUtilities.SupplyChainSecurityConfiguration.IsExportForAviationSecurityPurposes)
						.ToList();
				}
			}

			return result;
		}

		public static string GetNamesString(this IEnumerable<CommonConsol> consols)
		{
			var consolNames = consols.Select(consol => consol.JK_UniqueConsignRef).OrderBy(x => x);
			var fixedConsolNames = consolNames.Select(name => name.IsEmpty ? (ZString)Res.GetString("5e4b857d-917f-4208-87b9-f376cb4f06d8", "New Consol") : name);
			var consolsAsString = string.Join(", ", fixedConsolNames);

			return consolsAsString;
		}
	}
}
