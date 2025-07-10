using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Documents.DocDataObjects.Testing
{
	[TestedType(typeof(AdditionalService))]
	sealed class AdditionalServiceTest : NonPersistentBusinessObjectTestCase
	{
		public void TestPopulate()
		{
			var container = Factory.New<CommonContainer>();
			var service1 = container.Services.AddNew();
			var service2 = container.Services.AddNew();

			AssertEquals("Precondition: 2 services", 2, container.Services.Count);

			var services = AdditionalService.Create(Context, container.Services);
			AssertEquals("Count should be 2", 2, services.Count);
		}

		public void TestPopulate_Empty()
		{
			var container = Factory.New<CommonContainer>();

			AssertEquals("Precondition: no services", 0, container.Services.Count);

			var services = AdditionalService.Create(context, container.Services);
			AssertEquals("Should be empty", 0, services.Count);
		}

		public void TestPopulate_NullContext()
		{
			var container = Factory.New<CommonContainer>();
			var service1 = container.Services.AddNew();
			var service2 = container.Services.AddNew();

			AssertEquals("Precondition: 2 services", 2, container.Services.Count);

			var services = AdditionalService.Create(null, container.Services);
			AssertEquals("Should be empty", 0, services.Count);
		}

		public void TestPopulate_NullCollection()
		{
			var services = AdditionalService.Create(Context, null);
			AssertEquals("Should be empty", 0, services.Count);
		}

		[TestDate(2018, 6, 6)]
		public void TestPopulate_Contents()
		{
			var container = Factory.New<CommonContainer>();
			var service = container.Services.AddNew();

			var today = ZDateTime.Today;

			service.ES_Booked = today;
			service.ES_Completed = today.AddDays(1);
			service.ES_Duration = today.AddDays(1);
			service.ES_ServiceCount = 1;
			service.ES_ServiceNote = "Lucanus cervus";
			service.ES_References = "Odontolabis castelnaudi";

			var contractor = Factory.NewWithValidTestData<OrgHeader>();
			service.ES_OH_Contractor = contractor.PK;
			contractor.OH_FullName = "Ceratognathus minutus";

			var location = Factory.NewWithValidTestData<OrgAddress>();
			location.Header.OH_FullName = "Holloceratognathus cylindricus";
			service.ES_OA_Location = location.PK;

			Factory.Save();

			var additionalService = AdditionalService.Create(Context, container.Services).FirstOrDefault();
			AssertEquals("Services should not be empty", true, additionalService != null);

			CombineAssertions(() =>
			{
				AssertEquals("Booked", today, additionalService.Booked);
				AssertEquals("Completed", today.AddDays(1), additionalService.Completed);
				AssertEquals("Duration", (ZDateTime)today.AddDays(1).ToTimeSpan(), additionalService.Duration);
				AssertEquals("ServiceCount", 1m, additionalService.ServiceCount);
				AssertEquals("ServiceNote", "Lucanus cervus", additionalService.ServiceNote);
				AssertEquals("References", "Odontolabis castelnaudi", additionalService.References);

				AssertEquals("Contractor", "Ceratognathus minutus", additionalService.Contractor?.CompanyName);
				AssertEquals("Location", "Holloceratognathus cylindricus", additionalService.Location?.CompanyName);
			});
		}

		IContext Context => context ?? (context = new CommonContext(Factory));
		IContext context;
	}
}
