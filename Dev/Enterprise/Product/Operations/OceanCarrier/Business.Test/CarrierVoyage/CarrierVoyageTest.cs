using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyage))]
	sealed class CarrierVoyageTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var carrierVoyage = Factory.New<CarrierVoyage>();
			var service = Factory.NewWithValidTestData<CarrierService>();
			carrierVoyage.CVO_VoyageNumber = "VOY00001";
			carrierVoyage.CVO_CSV_Service = service.PK;
			return carrierVoyage;
		}
	}
}
