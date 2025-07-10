using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyagePortCallDivot))]
	sealed class CarrierVoyagePortCallDivotTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var voyage = Factory.NewWithValidTestData<CarrierVoyage>();

			var portCall = Factory.NewWithValidTestData<CarrierVoyagePortCall>();

			var carrierVoyagePortCallDivot = Factory.New<CarrierVoyagePortCallDivot>();
			carrierVoyagePortCallDivot.CVP_CVO_Voyage = voyage.PK;
			carrierVoyagePortCallDivot.CVP_CPO_PortCall = portCall.PK;

			return carrierVoyagePortCallDivot;
		}
	}
}
