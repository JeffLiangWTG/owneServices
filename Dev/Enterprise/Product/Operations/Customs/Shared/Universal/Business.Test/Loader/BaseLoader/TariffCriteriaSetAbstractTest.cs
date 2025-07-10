using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.Universal.Testing
{
	[TestsSubclassesOf(typeof(TariffCriteriaSet<>))]
	public abstract class TariffCriteriaSetAbstractTest<T> : TestCaseWithFactory
		where T : EnterpriseBusinessObject, ITariffEffectiveDatesRelatedBusinessObject
	{
		[TestDate(2021, 8, 12)]
		public void TestCacheKey()
		{
			AssertEquals(ExpectedCriteriaSetCacheKey, TariffCriteriaSetForTest.CacheKey);
		}

		public abstract void TestCacheKeyWhenCriteriaSetHasNullValues();

		public abstract void TestGetCriteriaSetCacheKeyWhenCriteriaSetHasEmptyValues();

		public abstract void TestAddNewCriteriaSetRow();

		public abstract void TestAddNewCriteriaSetRow_Default();

		protected abstract string ExpectedCriteriaSetCacheKey { get; }

		protected abstract TariffCriteriaSet<T> TariffCriteriaSetForTest { get; }
	}
}
