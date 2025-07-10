using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class IHaveServicesTest : TestCaseWithFactory
	{
		public void TestServiceWrappers()
		{
			DummyWithServices dummy = Factory.New<DummyWithServices>();
			JobService service1 = dummy.Services.AddNew();
			JobService service2 = dummy.Services.AddNew();
			JobService service3 = dummy.Services.AddNew();

			ServicesSelectionProviderForTest selectionProvider = new ServicesSelectionProviderForTest();
			Factory.SetValue<IServicesSelectionProvider>(() => selectionProvider);

			AssertNull(dummy.GetServiceWrappers(Core.Constants.DataContext.RequestForService));
			AssertNull(dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJob));

			selectionProvider.GetServicesToPrintImplementation = (parent) => System.Array.Empty<JobService>();

			DocumentWrapper[] serviceWrappers = dummy.GetServiceWrappers(Core.Constants.DataContext.RequestForService);
			AssertNotNull(serviceWrappers);
			AssertEquals(0, serviceWrappers.Length);

			serviceWrappers = dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJob);
			AssertNotNull(serviceWrappers);
			AssertEquals(0, serviceWrappers.Length);

			selectionProvider.GetServicesToPrintImplementation = (parent) => new[] { service1, service2 };

			serviceWrappers = dummy.GetServiceWrappers(Core.Constants.DataContext.Service);
			AssertNotNull(serviceWrappers);
			AssertEquals(2, serviceWrappers.Length);

			serviceWrappers = dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJobServices);
			AssertNotNull(serviceWrappers);
			AssertEquals(2, serviceWrappers.Length);

			ErrorReporter.Clear();
		}

		public void TestServiceWrappersDefaultSelectionProvider()
		{
			DummyWithServices dummy = Factory.New<DummyWithServices>();
			dummy.Services.AddNew();
			dummy.Services.AddNew();

			DocumentWrapper[] serviceWrappers = dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJob);

			AssertNotNull(serviceWrappers);
			AssertEquals(2, serviceWrappers.Length);

			dummy.Services.AddNew();

			serviceWrappers = dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJob);

			AssertNotNull(serviceWrappers);
			AssertEquals(3, serviceWrappers.Length);
		}

		public void TestServiceWrappers_UseDefaultSelectionProvider_InSaveTransaction()
		{
			var dummy = Factory.New<DummyWithServices>();
			dummy.Services.AddNew();
			dummy.Services.AddNew();

			var serviceWrappers = dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJob);

			AssertNotNull(serviceWrappers);
			AssertEquals(2, serviceWrappers.Length);

			var selectionProvider = new ServicesSelectionProviderForTest();
			Factory.SetValue<IServicesSelectionProvider>(() => selectionProvider);
			selectionProvider.GetServicesToPrintImplementation = (parent) => System.Array.Empty<JobService>();

			serviceWrappers = dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJob);
			AssertNotNull(serviceWrappers);
			AssertEquals(0, serviceWrappers.Length);

			DocumentWrapper[] serviceWrappersInSaveTransaction = null;
			Factory.Saving += _ => serviceWrappersInSaveTransaction = dummy.GetServiceWrappersForDocBuilder(dummy, Core.Constants.DataContext.GenericFreightJob);
			Factory.Save();
			AssertNotNull(serviceWrappersInSaveTransaction);
			AssertEquals(2, serviceWrappersInSaveTransaction.Length);
		}
	}
}
