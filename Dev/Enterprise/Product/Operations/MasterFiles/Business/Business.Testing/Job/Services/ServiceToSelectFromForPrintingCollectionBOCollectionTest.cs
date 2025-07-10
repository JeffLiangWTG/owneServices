using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ServiceToSelectFromForPrintingCollection))]
	sealed class ServiceToSelectFromForPrintingCollectionBOCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ServiceToSelectFromForPrintingCollection>
	{
		public void TestCollectionDoesNotAllowNew()
		{
			DummyWithServices dummy = Factory.New<DummyWithServices>();
			((IHaveServices)dummy).Services.AddNew();

			ServiceToSelectFromForPrintingCollection collection = new ServiceToSelectFromForPrintingCollection(((IHaveServices)dummy).Services);
			AssertEquals("Collection should not allow new", false, collection.AllowNew);
		}

		protected override ServiceToSelectFromForPrintingCollection GetCollectionToTest()
		{
			DummyWithServices dummy = Factory.New<DummyWithServices>();
			return new ServiceToSelectFromForPrintingCollection(((IHaveServices)dummy).Services);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			JobService service = Factory.New<JobService>();

			return new ServiceToSelectFromForPrinting(service);
		}

		public void TestAllServicesWrappedInCollection()
		{
			DummyWithServices dummy = Factory.New<DummyWithServices>();
			JobService service1 = ((IHaveServices)dummy).Services.AddNew();
			JobService service2 = ((IHaveServices)dummy).Services.AddNew();
			JobService service3 = ((IHaveServices)dummy).Services.AddNew();

			ServiceToSelectFromForPrintingCollection collection = new ServiceToSelectFromForPrintingCollection(((IHaveServices)dummy).Services);
			AssertEquals("Collection should have a count of 3", 3, collection.Count);
			AssertEquals("Collection contains Service1", true, ((IHaveServices)dummy).Services.Contains(collection[0].Service.PK));
			AssertEquals("Collection contains Service1", true, ((IHaveServices)dummy).Services.Contains(collection[1].Service.PK));
			AssertEquals("Collection contains Service1", true, ((IHaveServices)dummy).Services.Contains(collection[2].Service.PK));
		}
	}
}
