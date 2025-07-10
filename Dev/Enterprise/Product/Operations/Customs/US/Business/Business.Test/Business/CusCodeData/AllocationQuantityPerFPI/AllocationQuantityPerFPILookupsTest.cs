using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class AllocationQuantityPerFPILookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestForeignProducerIdentifiers()
		{
			var manufacturer = Factory.NewWithValidTestData<OrgHeader>();
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer, "B123456");
			manufacturer.MainAddress.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits, "S987654");
			var address1 = manufacturer.Addresses.AddNew();
			address1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine, "W11223344556677");
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var org1Wrapper = OrgHeaderWrapper.New(org);
			var orgQtyPerFPI = org1Wrapper.AllocationQuantityPerFPIs.AddNew();
			orgQtyPerFPI.US_OA_ManufacturerAddress = manufacturer.MainAddress.PK;
			AssertEquals(2, orgQtyPerFPI.Lookups.ForeignProducerIdentifiers.Count);
			AssertEquals("B123456, S987654", string.Join(", ", orgQtyPerFPI.Lookups.ForeignProducerIdentifiers.GetAllCodes()));
			orgQtyPerFPI.US_OA_ManufacturerAddress = address1.PK;
			AssertEquals(1, orgQtyPerFPI.Lookups.ForeignProducerIdentifiers.Count);
			AssertEquals("W1122334455667", string.Join(", ", orgQtyPerFPI.Lookups.ForeignProducerIdentifiers.GetAllCodes()));
			orgQtyPerFPI.US_OA_ManufacturerAddress = ZGuid.Empty;
			AssertEquals(0, orgQtyPerFPI.Lookups.ForeignProducerIdentifiers.Count);
		}
	}
}
