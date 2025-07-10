using CargoWise.EntityFramework.Testing;
using Enterprise.eTail.DataTransfer.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.eTail.DataTransfer.Testing
{
	public class DataObjectWriterCacheTest : TestCaseWithFactory
	{
		public void TestBuildKey()
		{
			var cache = new CodeDataObjectCache();

			var property1 = "Property1";
			var property2 = "Property2";
			var property3 = "Property3";

			AssertEquals("Property1-Property2-Property3", cache.BuildKey(property1, property2, property3));
		}

		public void TestWhenCacheKeyIsEmpty_GetValueWithNoValueDelegateReturnsEmpty()
		{
			var cache = new CodeDataObjectCache();
			var key = cache.BuildKey("Unique", "Key");
			var value = cache.GetValue<UnitOfWeight>(key);

			AssertNull("The cache key value should be null", value);
		}

		public void TestWhenCacheKeyIsEmptyAndThereIsAGetValueDelegate_CacheStoresValueDelegate()
		{
			var cache = new CodeDataObjectCache();
			var key = cache.BuildKey("Unique", "Key");
			var unitOfWeight = ListHelper.GetWithDescription<UnitOfWeight>(Weight.Kilograms, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
			var value = cache.GetValue(key, () => unitOfWeight);

			AssertEquals(unitOfWeight, cache.GetValue<UnitOfWeight>(key));
		}

		public void TestClearKey()
		{
			var cache = new CodeDataObjectCache();
			var key = cache.BuildKey("Unique", "Key");
			var unitOfWeight = ListHelper.GetWithDescription<UnitOfWeight>(Weight.Kilograms, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
			var value = cache.GetValue(key, () => unitOfWeight);

			AssertEquals("Precondition: There must be a key value pair in the cache to clear", unitOfWeight, cache.GetValue<UnitOfWeight>(key));

			cache.Clear(key);

			AssertNull(cache.GetValue<UnitOfWeight>(key));
		}

		public void TestWhenCacheKeyHasValue_CacheKeyValuesAreNotOverWrittenByGetValueDelegate()
		{
			var cache = new CodeDataObjectCache();
			var key = cache.BuildKey("Unique", "Key");
			var getValueDelegate = ListHelper.GetWithDescription<UnitOfWeight>(Weight.Kilograms, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));

			var initialValue = ListHelper.GetWithDescription<UnitOfWeight>(Weight.Ounces, Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight));
			var cacheValue1 = cache.GetValue(key, () => initialValue);

			AssertEquals("Precondition: We must have cached initialValue", initialValue, cache.GetValue<UnitOfWeight>(key));

			var cacheValue2 = cache.GetValue(key, () => getValueDelegate);
			AssertEquals("This cache value should not have been overwritten by the getDelegateValue, it should have just retrieved initialValue", initialValue, cacheValue2);
		}
	}
}
