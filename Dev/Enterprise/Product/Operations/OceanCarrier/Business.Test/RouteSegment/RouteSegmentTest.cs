using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(RouteSegment))]
	sealed class RouteSegmentTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var fromAddress = Factory.NewWithValidTestData<OrgAddress>();
			var toAddress = Factory.NewWithValidTestData<OrgAddress>();

			var routeSegment = Factory.NewWithValidTestData<RouteSegment>();
			routeSegment.RSG_OA_FromAddress = fromAddress.PK;
			routeSegment.RSG_OA_ToAddress = toAddress.PK;
			return routeSegment;
		}
	}
}
