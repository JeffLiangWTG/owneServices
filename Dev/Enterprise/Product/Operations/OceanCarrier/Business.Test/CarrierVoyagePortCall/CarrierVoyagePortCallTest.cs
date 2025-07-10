using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.OceanCarrier.Business.Testing
{
	[TestedType(typeof(CarrierVoyagePortCall))]
	sealed class CarrierVoyagePortCallTest : EnterpriseBusinessObjectTestCase
	{
		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

		protected override BusinessObject GetNewBusinessObject()
		{
			var portAddress = Factory.NewWithValidTestData<OrgAddress>();

			var vessel = Factory.NewWithValidTestData<RefVessel>();

			var carrierVoyagePortCall = Factory.New<CarrierVoyagePortCall>();
			carrierVoyagePortCall.CPO_EstimatedArrivalTime = new ZDateTimeOffset(2024, 01, 01);
			carrierVoyagePortCall.CPO_EstimatedDepartureTime = new ZDateTimeOffset(2024, 01, 05);
			carrierVoyagePortCall.CPO_OA_Port = portAddress.PK;
			carrierVoyagePortCall.CPO_RV_Vessel = vessel.PK;
			carrierVoyagePortCall.CPO_VesselName = vessel.RV_Code;

			return carrierVoyagePortCall;
		}
	}
}
