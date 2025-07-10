using CargoWise.Application;
using CargoWise.Types;

namespace Enterprise.Rating.Business.Testing
{
	internal sealed class RateEntryFilterValueCacheTest : RatingTestCase
	{
		public void TestCache_WriteThenRead_ReturnValue()
		{
			var cache = new RateEntryFilterValueCache();

			cache[ZGuid.BrettsGuid] = new RateEntryFilterValue()
			{
				ContractNumber = "AAA"
			};

			var storedValue = cache[ZGuid.BrettsGuid];
			AssertEquals("AAA", storedValue.ContractNumber);
		}

		public void TestCache_ReadWhenEmpty_ReturnNull()
		{
			var cache = new RateEntryFilterValueCache();
			var storedValue = cache[ZGuid.BrettsGuid];
			AssertNull(storedValue);
		}

		public void TestCache_WriteWriteThenRead_ReturnLatter()
		{
			var cache = new RateEntryFilterValueCache();

			cache[ZGuid.BrettsGuid] = new RateEntryFilterValue()
			{
				ContractNumber = "AAA"
			};
			cache[ZGuid.BrettsGuid] = new RateEntryFilterValue()
			{
				ContractNumber = "BBB"
			};

			var storedValue = cache[ZGuid.BrettsGuid];
			AssertEquals("BBB", storedValue.ContractNumber);
		}

		public void TestCache_WriteThenReadRead_ReturnNull()
		{
			var cache = new RateEntryFilterValueCache();

			cache[ZGuid.BrettsGuid] = new RateEntryFilterValue()
			{
				ContractNumber = "AAA"
			};

			var storedValue = cache[ZGuid.BrettsGuid];
			AssertEquals("AAA", storedValue.ContractNumber);
			storedValue = cache[ZGuid.BrettsGuid];
			AssertNull(storedValue);
		}

		public void TestCache_WriteThreeEntires_ReadThreeEntries_CheckCorrect()
		{
			var guid1 = ZGuid.NewZGuid();
			var guid2 = ZGuid.NewZGuid();
			var guid3 = ZGuid.NewZGuid();

			var cache = new RateEntryFilterValueCache();

			cache[guid1] = new RateEntryFilterValue()
			{
				ContractNumber = "A"
			};
			cache[guid2] = new RateEntryFilterValue()
			{
				ContractNumber = "B"
			};
			cache[guid3] = new RateEntryFilterValue()
			{
				ContractNumber = "C"
			};

			AssertEquals("A", cache[guid1].ContractNumber);
			AssertNull(cache[guid1]);
			AssertEquals("B", cache[guid2].ContractNumber);
			AssertNull(cache[guid2]);
			AssertEquals("C", cache[guid3].ContractNumber);
			AssertNull(cache[guid3]);
		}

		public void TestCache_IsInObjectFactory()
		{
			var found = ObjectFactory.Get<IRateEntryFilterValueCache>();
			AssertNotNull(found);
			AssertType<RateEntryFilterValueCache>(found);
			AssertSame(found, RateEntryFilterValueCache.Instance);
		}
	}
}
