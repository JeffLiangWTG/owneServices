using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierShipmentRouteLeg))]
	sealed class CarrierShipmentRouteLegTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var carrierShipmentHeader = Factory.NewWithValidTestData<CarrierShipmentHeader>();
			var fromAddress = Factory.NewWithValidTestData<OrgAddress>();
			var toAddress = Factory.NewWithValidTestData<OrgAddress>();
			var leg = Factory.New<CarrierShipmentRouteLeg>();
			leg.CRG_Sequence = 1;
			leg.CRG_CSH_CarrierShipment = carrierShipmentHeader.PK;
			leg.CRG_OA_FromAddress = fromAddress.PK;
			leg.CRG_OA_ToAddress = toAddress.PK;

			return leg;
		}
	}
}
