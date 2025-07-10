using CargoWise.Types;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class BOMPartCacheKeyTest : WhsTestCaseWithFactory
	{
		public void TestBOMPartCacheKey_New()
		{
			var pk1 = ZGuid.NewZGuid();
			var packType1 = "BOX";
			var key = new BOMPartCacheKey(pk1, packType1);
			var keyString = key.ToString();
			AssertEquals($"BOMPartCacheKey {{ ProductPK = {pk1}, PackType = BOX, ParentProductPK =  }}", keyString);

			var pk2 = ZGuid.NewZGuid();
			var key2 = new BOMPartCacheKey(pk2, pk1, packType1);
			keyString = key2.ToString();
			AssertEquals($"BOMPartCacheKey {{ ProductPK = {pk1}, PackType = BOX, ParentProductPK = {pk2} }}", keyString);
		}

		public void TestBOMPartCacheKey_Overrides()
		{
			var pk1 = ZGuid.NewZGuid();
			var packType1 = "BOX";
			var key1 = new BOMPartCacheKey(pk1, packType1);

			var pk2 = ZGuid.NewZGuid();
			var packType2 = "BOX";
			var key2 = new BOMPartCacheKey(pk2, packType2);
			AssertEquals(false, key1.Equals(key2));
			AssertEquals(false, key1.GetHashCode() == key2.GetHashCode());

			var key3 = new BOMPartCacheKey(pk1, packType2);
			AssertEquals(true, key1.Equals(key3));
			AssertEquals(true, key1.GetHashCode() == key3.GetHashCode());

			var key4 = new BOMPartCacheKey(pk1, "PLT");
			AssertEquals(false, key1.Equals(key4));
			AssertEquals(false, key1.GetHashCode() == key4.GetHashCode());

			var pk3 = ZGuid.NewZGuid();
			var key5 = new BOMPartCacheKey(pk3, pk1, packType2);
			AssertEquals(false, key1.Equals(key5));
			AssertEquals(false, key1.GetHashCode() == key5.GetHashCode());

			var key6 = new BOMPartCacheKey(pk3, pk1, packType1);
			AssertEquals(true, key5.Equals(key6));
			AssertEquals(true, key5.GetHashCode() == key6.GetHashCode());
		}
	}
}
