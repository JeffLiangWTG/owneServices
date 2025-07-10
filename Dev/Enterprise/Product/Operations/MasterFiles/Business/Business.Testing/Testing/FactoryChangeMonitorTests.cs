using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class FactoryChangeMonitorTests : TransactionedTestCase
	{
		public void TestIsntLeaking()
		{
			var factory = new BusinessObjectFactory();
			var monitorRef = CreateMonitorModifyFactoryAndDispose(factory);

			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();

			Assert("After being disposed, all events should be unhooked so that the monitor can be collected even if the factory is still strongref'd", !monitorRef.TryGetTarget(out _));
		}

		public void TestDetectsDataRefreshBusChangesInAlreadyCreatedObjects()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = true };
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var monitorFactory = new BusinessObjectFactory();
			monitorFactory.Load<DummyBusinessObject>(dummy.PK);

			using (var monitor = new FactoryChangeMonitor(monitorFactory))
			{
				Assert("No loaded objects in the factory have been changed", !monitor.WasModified);

				dummy.Z0_Description = "Something else";
				factory.Save();

				Assert("The monitor should pick up changes from the data refresh bus", monitor.WasModified);
			}
		}

		public void TestDetectsNewObjects()
		{
			var factory = new BusinessObjectFactory();
			using (var monitor = new FactoryChangeMonitor(factory))
			{
				Assert("No loaded objects in the factory have been changed", !monitor.WasModified);

				factory.NewWithValidTestData<DummyBusinessObject>();

				Assert("The monitor should pick up new objects", monitor.WasModified);
			}
		}

		public void TestDetectsSavedChanges_NewObjects()
		{
			var factory = new BusinessObjectFactory();
			using (var monitor = new FactoryChangeMonitor(factory))
			{
				Assert("No loaded objects in the factory have been changed", !monitor.WasModified);

				factory.NewWithValidTestData<DummyBusinessObject>();
				factory.Save();

				Assert("The monitor should pick up new objects, even after a save", monitor.WasModified);
			}
		}

		public void TestDetectsSavedChanges_ExistingObjects()
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = true };
			var dummyOnOther = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var monitorFactory = new BusinessObjectFactory();
			var dummy = monitorFactory.Load<DummyBusinessObject>(dummyOnOther.PK);

			using (var monitor = new FactoryChangeMonitor(monitorFactory))
			{
				Assert("No loaded objects in the factory have been changed", !monitor.WasModified);

				dummy.Z0_Description = "Something else";
				factory.Save();

				Assert("The monitor should pick up modified objects, even after a save", monitor.WasModified);
			}
		}

		public void TestDetectsSavedChanges_ObjectsFromDataRefresh() => TestDetectsSavedChanges_ObjectsFromDataRefresh(bizoDeletedInMemory: false);

		public void TestDetectsSavedChanges_ObjectsFromDataRefresh_BizoDeletedInMemory() => TestDetectsSavedChanges_ObjectsFromDataRefresh(bizoDeletedInMemory: true);

		void TestDetectsSavedChanges_ObjectsFromDataRefresh(bool bizoDeletedInMemory)
		{
			var factory = new BusinessObjectFactory { RefreshEnabled = true };
			var company = factory.NewWithValidTestData<GlbCompany>();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			factory.Save();

			var monitorFactory = new BusinessObjectFactory();
			var dummyInFactory = monitorFactory.Load<DummyBusinessObject>(dummy.PK);
			var companyInFactory = monitorFactory.Load<GlbCompany>(company.PK);
			_ = companyInFactory.Branches;

			using (var monitor = new FactoryChangeMonitor(monitorFactory))
			{
				Assert("No loaded objects in the factory have been changed", !monitor.WasModified);

				var branchInOtherFactory = factory.NewWithValidTestData<GlbBranch>();
				branchInOtherFactory.GB_GC = company.PK;
				factory.Save();

				if (bizoDeletedInMemory)
				{
					dummyInFactory.Delete();
				}

				Assert("The monitor should pick up objects from data refresh.", monitor.WasModified);
			}
		}

		static WeakReference<FactoryChangeMonitor> CreateMonitorModifyFactoryAndDispose(BusinessObjectFactory factory)
		{
			factory.NewWithValidTestData<DummyBusinessObject>();

			var monitor = new FactoryChangeMonitor(factory);
			factory.NewWithValidTestData<DummyBusinessObject>();

			monitor.Dispose();
			return new WeakReference<FactoryChangeMonitor>(monitor);
		}
	}
}
