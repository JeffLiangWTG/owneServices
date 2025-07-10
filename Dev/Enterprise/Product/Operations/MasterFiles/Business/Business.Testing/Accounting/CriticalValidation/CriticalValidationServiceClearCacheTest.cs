using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business.Accounting.CriticalValidation;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class CriticalValidationServiceClearCacheTest : TestCaseWithFactory
	{
		public void TestValidation_CallsClearCache_WhenBOValidationImplementsIClearCacheProvider()
		{
			var clearCacheProvider = new DummyCriticalValidationWithClearCacheProvider();
			var bizo = Factory.New<DummyCriticalValidationParent>();
			bizo.CriticalValidation = clearCacheProvider;

			var objects = new BusinessObject[] { bizo };
			var service = new CriticalValidationService();
			service.ProcessBusinessObjects(objects);

			AssertEquals(1, clearCacheProvider.ClearCacheCallCount);
		}

		public void TestWhenValidation_SameType_CallsClearCacheOnce()
		{
			var clearCacheProvider1 = new DummyCriticalValidationWithClearCacheProvider();
			var clearCacheProvider2 = new DummyCriticalValidationWithClearCacheProvider();
			var bizoWithCriticalValidation1 = Factory.New<DummyCriticalValidationParent>();
			var bizoWithCriticalValidation2 = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation1.CriticalValidation = clearCacheProvider1;
			bizoWithCriticalValidation2.CriticalValidation = clearCacheProvider2;

			AssertEquals("Precondition: Types of critical validation - same", bizoWithCriticalValidation1.CriticalValidation.GetType(), bizoWithCriticalValidation2.CriticalValidation.GetType());

			var objects = new BusinessObject[] { bizoWithCriticalValidation1, bizoWithCriticalValidation2 };
			var service = new CriticalValidationService();
			service.ProcessBusinessObjects(objects);

			AssertEquals("We use static method so that it calls clear cache method only once for first instance, and no calls to rest of instances. First instance :", 1, clearCacheProvider1.ClearCacheCallCount);
			AssertEquals("Second instance :", 0, clearCacheProvider2.ClearCacheCallCount);
		}

		public void TestWhenValidation_DifferentTypes_CallsClearCacheForEachType()
		{
			var clearCacheProvider1 = new DummyCriticalValidationWithClearCacheProvider();
			var clearCacheProvider2 = new DummyCriticalValidationWithClearCacheProvider2();

			var bizoWithCriticalValidation1 = Factory.New<DummyCriticalValidationParent>();
			var bizoWithCriticalValidation2 = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation1.CriticalValidation = clearCacheProvider1;
			bizoWithCriticalValidation2.CriticalValidation = clearCacheProvider2;

			AssertNotEquals("Precondition: Types of critical validation - different", bizoWithCriticalValidation1.CriticalValidation.GetType(), bizoWithCriticalValidation2.CriticalValidation.GetType());

			var objects = new BusinessObject[] { bizoWithCriticalValidation1, bizoWithCriticalValidation2 };
			var service = new CriticalValidationService();
			service.ProcessBusinessObjects(objects);

			AssertEquals(1, clearCacheProvider1.ClearCacheCallCount);
			AssertEquals(1, clearCacheProvider2.ClearCacheCallCount);
		}

		public void TestValidation_ClearsCache_AfterBOValidationThrowsException()
		{
			var clearCacheProvider = new DummyCriticalValidationWithClearCacheProvider() { ThrowErrorOnSaving = true };
			var bizoWithCriticalValidation1 = Factory.New<DummyCriticalValidationParent>();
			bizoWithCriticalValidation1.CriticalValidation = clearCacheProvider;

			var service = new CriticalValidationService();
			Factory.ServiceContainer.AddCriticalValidationService(service);
			Factory.ServiceContainer.AddAfterSaveInTransactionService(service);
			AssertExceptionThrown<OnSavingCriticalCheckException>(Factory.Save);

			AssertEquals(1, clearCacheProvider.ClearCacheCallCount);
			ErrorReporter.Clear();
		}

		class DummyCriticalValidationParent : DummyBusinessObject, ISupportCriticalValidation
		{
			public DummyCriticalValidationParent(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			#region ISupportCriticalValidation Members

			public ICriticalValidation CriticalValidation { get; set; }

			void IConflictWithCriticalFields.SetConflictWithCriticalFieldsBusinessContext() { }

			#endregion
		}

		class DummyCriticalValidation : ICriticalValidation
		{
			public bool ThrowErrorOnSaving;

			#region ICriticalValidation Members

			void ICriticalValidation.RegisterOnSavingCheck() { }

			void ICriticalValidation.RunOnSavingCheck()
			{
				if (ThrowErrorOnSaving)
				{
					throw new OnSavingCriticalCheckException<DummyCriticalValidationParent>(null, CriticalValidationErrorType.DummyErrorKeyForTest, "Error Has Occurred On Saving", "E=MC2");
				}
			}

			void ICriticalValidation.RunDeletedObjectOnSavingCheck() { }

			void ICriticalValidation.RunAfterSavingCheck() { }

			#endregion
		}

		class DummyCriticalValidationWithClearCacheProvider : DummyCriticalValidation, IClearCacheProvider
		{
			public int ClearCacheCallCount;

			#region IClearCacheProvider Members

			ClearCacheDelegate IClearCacheProvider.GetClearCacheDelegate() => ClearCache;

			void ClearCache(BusinessObjectFactory factory)
			{
				ClearCacheCallCount++;
			}

			#endregion
		}

		class DummyCriticalValidationWithClearCacheProvider2 : DummyCriticalValidationWithClearCacheProvider
		{
		}
	}
}
