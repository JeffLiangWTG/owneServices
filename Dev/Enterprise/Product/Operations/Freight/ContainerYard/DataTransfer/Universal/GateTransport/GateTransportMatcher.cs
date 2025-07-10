using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.ContainerYard.Business
{
	sealed class GateTransportMatcher : CombinationKeyMatcher<GateTransport, GateReference>
	{
		internal GateTransportMatcher(BusinessObjectFactory factory, GateReference references, IXmlImportLogger logger)
			: base(factory, references, logger)
		{
		}

		protected override bool CheckLatestParent(GateTransport gateTransport, GateTransport gateTransportToCompareTo)
		{
			return gateTransport.GTT_TimeIn > gateTransportToCompareTo.GTT_TimeIn;
		}

		protected override ZQuery GetFullQuery(ZQuery initialMatchingQuery, GateReference gateReference)
		{
			return initialMatchingQuery;
		}

		protected override void BuildMatchingQueryAndMatchDelegates(GateReference gateReference)
		{
			var query = new ZQuery(GateTransportSchema.GTT_TimeIn, SQLComparisonOperator.NotEqual, ZDateTime.Empty);
			query.AddToFilter(GateTransportSchema.GTT_TimeOut, ZDateTime.Empty);

			if (!string.IsNullOrEmpty(gateReference?.VehicleRegistrationNumber))
			{
				query.AddToFilter(GateTransportSchema.GTT_VehicleRegistration, gateReference.VehicleRegistrationNumber);
			}

			AddPossibleMatch(query, gate => GetMatchCount(gate?.GTT_TimeOut, ZDateTime.Empty));
		}

		protected override void BuildFallbackMatchDelegates(GateReference referencesParent)
		{
			if (!string.IsNullOrEmpty(referencesParent?.VehicleRegistrationNumber))
			{
				AddFallbackMatch(referencesParent.VehicleRegistrationNumber, gate => GetMatchCount(gate?.GTT_VehicleRegistration, referencesParent.VehicleRegistrationNumber));
			}
			else if (referencesParent.ContainerNumbers.Count > 0)
			{
				AddFallbackMatch(referencesParent.ContainerNumbers, gate => GetContainersMatchCount(GetCfsDetailUnitNumbers(gate), referencesParent.ContainerNumbers));
			}
		}

		int GetContainersMatchCount(List<ZString> actualValues, List<ZString> valuesToMatch)
		{
			var result = actualValues.Intersect(valuesToMatch);
			return result.Count();
		}

		List<ZString> GetCfsDetailUnitNumbers(GateTransport gate)
		{
			var result = new List<ZString>();

			foreach (var cfsDetail in gate?.GateTransportCFSDetails)
			{
				var containerNumber = cfsDetail.GateBookingDetail?.YardUnit?.GTY_UnitNumber ?? ZString.Empty;
				if (!string.IsNullOrEmpty(containerNumber))
				{
					result.Add(containerNumber);
				}
			}
			return result;
		}
	}
}
