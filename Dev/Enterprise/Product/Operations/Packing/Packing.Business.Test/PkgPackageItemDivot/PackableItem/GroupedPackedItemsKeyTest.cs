namespace Enterprise.Packing.Business.Testing
{
	using CargoWise.Types;
	using static Enterprise.Packing.Business.Testing.GroupingKeyTest;

	class GroupedPackedItemsKeyTest : PackingTestCaseWithFactory
	{
		#region TestGetHashCode

		public void TestGetHashCode()
		{
			var key = new GroupedPackedItemsKey(new BogusKey(), ZGuid.BrettsGuid);
#if NETFRAMEWORK
			var expectedHashCode = -1346382318;
#else
			var expectedHashCode = -1082338586;
#endif
			AssertEquals("HashCode should be correct.", expectedHashCode, key.GetHashCode());
		}

#endregion

		#region TestEquals

		public void TestEquals()
		{
			var key = new GroupedPackedItemsKey(new BogusKey(), ZGuid.BrettsGuid);
			AssertEquals("Key should equal itself.", true, key.Equals(key));

			var key2 = new GroupedPackedItemsKey(new BogusKey(), ZGuid.BrettsGuid);
			AssertEquals("Key should equal itself.", true, key.Equals(key2));

			var keyNotEqual = new GroupedPackedItemsKey(new BogusKey(), ZGuid.NewZGuid());
			AssertEquals("Key should NOT equal a key with different params.", false, key.Equals(keyNotEqual));
		}

		#endregion

		#region TestTemporaryProperGroupingKeyForWhs_DoNotUseCore

		public void TestTemporaryProperGroupingKeyForWhs_DoNotUseCore()
		{
			var key = new GroupedPackedItemsKey(new BogusKey(), ZGuid.BrettsGuid);
			AssertEquals("Should be the same a Equals", true, key.IsSimilarItem_DoNotUse(key));
		}

		#endregion
	}
}
