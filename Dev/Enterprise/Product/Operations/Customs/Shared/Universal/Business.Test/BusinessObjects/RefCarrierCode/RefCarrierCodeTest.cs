using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestedType(typeof(RefCarrierCode))]
	class RefCarrierCodeTest : EnterpriseBusinessObjectTestCase
	{
		public void TestDelete()
		{
			var carrier = (RefCarrierCode)GetNewBusinessObjectForDeleteTest(Factory);
			var attribute1 = carrier.Attributes.AddNew("BOBAttribute", "SHORT");
			var attribute2 = carrier.Attributes.AddNew("BOBAttribute2", "SHORT");
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var carrierInDiffFactory = newFactory.Load<RefCarrierCode>(carrier.PK);
			carrierInDiffFactory.Delete();
			newFactory.Save();
			AssertEquals("carrier.IsDeleted", true, carrier.IsDeleted);
			AssertEquals("attribute1.IsDeleted", true, attribute1.IsDeleted);
			AssertEquals("attribute2.IsDeleted", true, attribute1.IsDeleted);
		}

		public void TestVesselsCollection()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var carrier = helper.CreateCarrierCode("AAA", "AAA Description", "ZA");
			var vessel = helper.CreateVesselZZ("VVV", "VV1", "CV", "ZA");
			helper.CreateCarrierVesselPivot(carrier.PK, vessel.PK);
			Factory.Save();
			AssertEquals("Has one port", 1, carrier.Vessels.Count);
		}

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			return new UniversalReferenceTestDataHelper(factory).CreateCarrierCode("B0B", "BOB THE BUILDER", Core.Constants.CountryCodes.Italy);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad()
		{
			return GetNewBusinessObjectForDeleteTest(Factory);
		}
	}
}
