using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierPreferredRoute))]
	sealed class CarrierPreferredRouteTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var fromAddress = Factory.NewWithValidTestData<OrgAddress>();
			var toAddress = Factory.NewWithValidTestData<OrgAddress>();

			var carrierPreferredRoute = Factory.New<CarrierPreferredRoute>();
			carrierPreferredRoute.CPU_IsActive = true;
			carrierPreferredRoute.CPU_IsExclusive = true;
			carrierPreferredRoute.CPU_OA_FromAddress = fromAddress.PK;
			carrierPreferredRoute.CPU_OA_ToAddress = toAddress.PK;
			return carrierPreferredRoute;
		}
	}
}
