using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.Customs.ZA.Business.OperationalActions.Testing
{
	public abstract class LockedOperationalActionMethodApplicatorTest<T> : OperationalActionMethodApplicatorTest where T : OperationalActionMethodApplicator, ILockedOperationalActionMethodApplicator
	{
		protected abstract OperationalActionMethodApplicator GetOtherApplicator();

		T GetNewApplicator() => GetNewBusinessObject() as T;

		public void TestLockReleasedOnFinish()
		{
			ExportApplicatorTestHelper.SetupTestDataForRecordFilterTesting(Factory);
			Applicator.Build(Array.Empty<ZGuid>());

			Assert("Did not acquire lock", Applicator.HasLock);
			Applicator.Apply(new DummyOperationalActionSectionLog(), Array.Empty<BusinessObject>());
			Assert("Did not unlock on completion", !Applicator.HasLock);
		}

		public void TestLockSetByFirstActionInSameCompany()
		{
			var applicator1 = GetNewApplicator();
			applicator1.Build(Array.Empty<ZGuid>());

			Assert("First user did not acquire lock", applicator1.Mutex.IsLocked && applicator1.Mutex.HasLock);

			var applicator2 = GetNewApplicator();
			applicator2.Build(Array.Empty<ZGuid>());

			Assert("Second user in company should be blocked", applicator2.Mutex.IsLocked && !applicator2.Mutex.HasLock);

			applicator1.Unlock();
		}

		public void TestLockSetAtCompanyLevel()
		{
			var applicator1 = GetNewApplicator();
			applicator1.Build(Array.Empty<ZGuid>());

			Assert("First user did not acquire lock", applicator1.Mutex.IsLocked && applicator1.Mutex.HasLock);

			using (GlbCompany.TemporaryLoginInNewCompanyForCountry(GlbCompany.CurrentCompany.Country.Code))
			{
				var applicator2 = GetNewApplicator();
				applicator2.Build(Array.Empty<ZGuid>());

				Assert("Second user in new company did not acquire lock", applicator2.Mutex.IsLocked && applicator2.Mutex.HasLock);
				applicator2.Unlock();
			}
			applicator1.Unlock();
		}

		public void TestLockSetAtOpGroupLevel()
		{
			var applicator1 = GetNewApplicator();
			applicator1.Build(Array.Empty<ZGuid>());

			Assert("First user did not acquire lock", applicator1.Mutex.IsLocked && applicator1.Mutex.HasLock);

			var applicator2 = GetOtherApplicator();
			applicator2.Build(Array.Empty<ZGuid>());

			Assert("Second user in company on different op should be blocked", ((ILockedOperationalActionMethodApplicator)applicator2).Mutex.IsLocked && !((ILockedOperationalActionMethodApplicator)applicator2).Mutex.HasLock);

			applicator1.Unlock();
		}

		protected new T Applicator => (T)base.Applicator;
	}
}
