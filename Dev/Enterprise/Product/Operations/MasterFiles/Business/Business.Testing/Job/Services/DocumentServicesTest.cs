using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(DocumentServices))]
	sealed class DocumentServicesTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			DummyWithServices dummy = IHaveServicesDummySetUp();

			ServiceToSelectFromForPrintingCollection collection = new ServiceToSelectFromForPrintingCollection(((IHaveServices)dummy).Services);
			return new DocumentServices(dummy, collection);
		}

		public void TestServicesToSelectFrom()
		{
			DummyWithServices dummy = IHaveServicesDummySetUp();

			ServiceToSelectFromForPrintingCollection serviceCollection = new ServiceToSelectFromForPrintingCollection(((IHaveServices)dummy).Services);

			Factory.Save();

			DocumentServices services = new DocumentServices(dummy, serviceCollection);
			AssertNotNull("ServicesToSelectFrom is not null", services.ServiceToSelectFrom);
			AssertEquals("ServicesToSelectFrom count", 2, services.ServiceToSelectFrom.Count);
		}

		public void TestContainerLegsToPrint()
		{
			DummyWithServices dummy = IHaveServicesDummySetUp();

			ServiceToSelectFromForPrintingCollection serviceCollection = new ServiceToSelectFromForPrintingCollection(((IHaveServices)dummy).Services);
			serviceCollection[1].ES_Calc_PrintDocumentForService = false;

			Factory.Save();

			DocumentServices services = new DocumentServices(dummy, serviceCollection);

			AssertNotNull("ServicesToPrint is not null", services.ServicesToPrint);
			AssertEquals("ServicesToPrint count", 1, services.ServicesToPrint.Count);
		}

		DummyWithServices IHaveServicesDummySetUp()
		{
			DummyWithServices dummy = Factory.New<DummyWithServices>();
			JobService service1 = ((IHaveServices)dummy).Services.AddNew();
			JobService service2 = ((IHaveServices)dummy).Services.AddNew();
			return dummy;
		}
	}
}
