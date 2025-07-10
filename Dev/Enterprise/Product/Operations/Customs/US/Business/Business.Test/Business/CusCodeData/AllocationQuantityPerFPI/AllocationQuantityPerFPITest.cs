using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	[TestedType(typeof(AllocationQuantityPerFPI))]
	sealed class AllocationQuantityPerFPITest : Customs.Business.Testing.CusCodeDataTest<AllocationQuantityPerFPI>
	{
		public void TestProperties()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address = org.Addresses.AddNew();
			address.FillWithValidTestData();
			var wrapper = OrgHeaderWrapper.New(org);
			var collection = new AllocationQuantityPerFPICollection(wrapper.CountryData);
			var quantityPerFPI = collection.AddNew();
			quantityPerFPI.US_OA_ManufacturerAddress = address.PK;
			quantityPerFPI.US_AllocationQuantity = 12.34m;
			quantityPerFPI.US_ForeignProducerIdentifier = "ABcD1234";
			AssertEquals("US_OA_ManufacturerAddress", address.PK, quantityPerFPI.US_OA_ManufacturerAddress);
			AssertEquals("US_AllocationQuantity", 12.34m, quantityPerFPI.US_AllocationQuantity);
			AssertEquals("US_ForeignProducerIdentifier", "ABCD1234", quantityPerFPI.US_ForeignProducerIdentifier);
			Factory.Save();
			var newFactory = new BusinessObjectFactory();
			var loadedQuantity = newFactory.Load<AllocationQuantityPerFPI>(quantityPerFPI.PK);
			AssertEquals("US_OA_ManufacturerAddress", address.PK, loadedQuantity.US_OA_ManufacturerAddress);
			AssertEquals("US_AllocationQuantity", 12.34m, loadedQuantity.US_AllocationQuantity);
			AssertEquals("US_ForeignProducerIdentifier", "ABCD1234", loadedQuantity.US_ForeignProducerIdentifier);
			loadedQuantity.US_OA_ManufacturerAddress = org.MainAddress.PK;
			loadedQuantity.US_AllocationQuantity = 56.789m;
			loadedQuantity.US_ForeignProducerIdentifier = "98765edf";
			AssertEquals("US_OA_ManufacturerAddress", org.MainAddress.PK, loadedQuantity.US_OA_ManufacturerAddress);
			AssertEquals("US_AllocationQuantity", 56.789m, loadedQuantity.US_AllocationQuantity);
			AssertEquals("US_ForeignProducerIdentifier", "98765EDF", loadedQuantity.US_ForeignProducerIdentifier);
		}

		public void TestDefaultForeignProducerIdentifier()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			var address1 = org.Addresses.AddNew();
			address1.FillWithValidTestData();
			address1.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierBeer, "B12345");
			var address2 = org.Addresses.AddNew();
			address2.FillWithValidTestData();
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierSpirits, "S23456");
			address2.CustomsCodes.AddNew(OrgCusCode.USACodeTypes.ForeignProducerIdentifierWine, "W34567");
			var quantityPerFPI = Factory.New<AllocationQuantityPerFPI>();
			quantityPerFPI.US_OA_ManufacturerAddress = org.MainAddress.PK;
			AssertEquals(ZString.Empty, quantityPerFPI.US_ForeignProducerIdentifier);
			quantityPerFPI.US_OA_ManufacturerAddress = address1.PK;
			AssertEquals("B12345", quantityPerFPI.US_ForeignProducerIdentifier);
			quantityPerFPI.US_OA_ManufacturerAddress = address2.PK;
			AssertEquals(ZString.Empty, quantityPerFPI.US_ForeignProducerIdentifier);
			quantityPerFPI.US_OA_ManufacturerAddress = address1.PK;
			AssertEquals("B12345", quantityPerFPI.US_ForeignProducerIdentifier);
			quantityPerFPI.US_OA_ManufacturerAddress = ZGuid.Empty;
			AssertEquals(ZString.Empty, quantityPerFPI.US_ForeignProducerIdentifier);
		}

		protected override BusinessObject GetNewBusinessObject() => Factory.New<AllocationQuantityPerFPI>();

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => OrgHeaderWrapper.New(factory.NewWithValidTestData<OrgHeader>()).AllocationQuantityPerFPIs.AddNew();
	}
}
