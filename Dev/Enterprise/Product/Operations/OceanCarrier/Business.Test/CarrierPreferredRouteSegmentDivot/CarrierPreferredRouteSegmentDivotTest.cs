using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierPreferredRouteSegmentDivot))]
	sealed class CarrierPreferredRouteSegmentDivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var fromAddress = Factory.NewWithValidTestData<OrgAddress>();
			var toAddress = Factory.NewWithValidTestData<OrgAddress>();
			var routeSegment = Factory.NewWithValidTestData<RouteSegment>();
			var preferredRoute = Factory.NewWithValidTestData<CarrierPreferredRoute>();
			routeSegment.RSG_OA_FromAddress = fromAddress.PK;
			routeSegment.RSG_OA_ToAddress = toAddress.PK;
			preferredRoute.CPU_OA_FromAddress = fromAddress.PK;
			preferredRoute.CPU_OA_ToAddress = toAddress.PK;

			var carrierPreferredRouteSegmentDivot = Factory.New<CarrierPreferredRouteSegmentDivot>();
			carrierPreferredRouteSegmentDivot.CPG_RSG_RouteSegment = routeSegment.PK;
			carrierPreferredRouteSegmentDivot.CPG_CPU_CarrierPreferredRoute = preferredRoute.PK;
			carrierPreferredRouteSegmentDivot.CPG_LegNo = 1;

			return carrierPreferredRouteSegmentDivot;
		}
	}
}
