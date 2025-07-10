namespace Enterprise.Packing.Business.Testing
{
	using NUnit.Framework;

	public class GroupingKeyTest : TestCase
	{
		#region TestEquals

		public void TestEquals()
		{
			var key1 = new TestKey();
			var key2 = new TestKey();
			AssertEquals(true, key1.Equals(key2));
			AssertEquals(true, key1 == key2);
			AssertEquals(false, key1 != key2);
			AssertEquals(false, key1.Equals(null));
			AssertEquals(false, key1 == null);
			AssertEquals(true, key1 != null);

			key2.EqualsForTest = true;
			AssertEquals(false, key1.Equals(key2));
			AssertEquals(false, key1 == key2);
			AssertEquals(true, key1 != key2);

			var bogusKey = new BogusKey();
			AssertEquals(false, key1.Equals(bogusKey));
			AssertEquals(false, key1 == bogusKey);
			AssertEquals(true, key1 != bogusKey);
		}

		#endregion

		#region TestKeyForAutoPack

		public void TestKeyForAutoPack()
		{
			var key = new TestKey();
			var keyForAutoPack = key.KeyForAutoPack;
			AssertEquals(keyForAutoPack, key);

			var keyWithAutoPackKey = new TestKeyWithKeyForAutoPack();
			var autoPackKey = keyWithAutoPackKey.KeyForAutoPack;
			AssertNotEquals(autoPackKey, keyWithAutoPackKey);
		}

		#endregion

		#region Implementation

		public class BogusKey : GroupingKey
		{
			public override int GetHashCode() => 2;
			protected override bool Equals(GroupingKey other) => false;
			protected override bool IsSimilarItem_DoNotUseCore(GroupingKey other) => false;
		}

		class TestKey : GroupingKey
		{
			protected override bool Equals(GroupingKey other) => EqualsForTest == (other as TestKey)?.EqualsForTest;
			public override int GetHashCode() => HashCodeForTest;
			public bool EqualsForTest { get; set; }
			public int HashCodeForTest { get; set; }
			protected override bool IsSimilarItem_DoNotUseCore(GroupingKey other) => Equals(other);
		}

		class TestKeyWithKeyForAutoPack : GroupingKey
		{
			protected override bool Equals(GroupingKey other) => EqualsForTest == (other as TestKeyWithKeyForAutoPack)?.EqualsForTest;
			public override int GetHashCode() => HashCodeForTest;
			public bool EqualsForTest { get; set; }
			public int HashCodeForTest { get; set; }
			protected override bool IsSimilarItem_DoNotUseCore(GroupingKey other) => Equals(other);
			protected override GroupingKey KeyForAutoPackCore => new TestKey();
		}

		#endregion
	}
}
