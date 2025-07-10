using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.Business.Testing
{
	sealed class MergeKeyTest : TestCase
	{
		public void TestRemove()
		{
			MergeKey mergeKey = new MergeKey();
			mergeKey.Add((ZString)"Value1");
			mergeKey.Add((ZString)"Value2");
			AssertEquals(2, mergeKey.Keys.Count);

			mergeKey.Remove((ZString)"Value not in merge key");
			AssertEquals(2, mergeKey.Keys.Count);

			mergeKey.Remove((ZString)"Value2");
			AssertEquals(1, mergeKey.Keys.Count);
			AssertEquals("Value1", mergeKey.Keys[0]);
		}

		public void TestEquals()
		{
			MergeKey mergeKey1 = new MergeKey();
			MergeKey mergeKey2 = new MergeKey();
			Assert(mergeKey1 == mergeKey2);
			Assert(mergeKey1.Equals(mergeKey2));
			Assert(!mergeKey1.Equals(null));

			mergeKey1.Add(ZString.Empty);
			Assert(mergeKey1 != mergeKey2);
			Assert(!mergeKey1.Equals(mergeKey2));
		}

		public void TestGetHashCode()
		{
			MergeKey mergeKey1 = new MergeKey();
			MergeKey mergeKey2 = new MergeKey();
			AssertEquals(mergeKey1.GetHashCode(), mergeKey2.GetHashCode());
			mergeKey1.Add((ZString)"hello");
			Assert(mergeKey1.GetHashCode() != mergeKey2.GetHashCode());
			mergeKey2.Add((ZString)"hello");
			Assert(mergeKey1.GetHashCode() == mergeKey2.GetHashCode());
		}

		public void TestClone()
		{
			MergeKey mergeKey1 = new MergeKey();
			Assert(mergeKey1.Clone() == mergeKey1);
			mergeKey1.Add((ZString)"Hello");
			Assert(mergeKey1.Clone() == mergeKey1);
		}

		public void TestPlus()
		{
			MergeKey mergeKey1 = new MergeKey();
			mergeKey1.Add((ZString)"Hello");
			MergeKey mergeKey2 = new MergeKey();
			mergeKey2.Add((ZString)"Goodbye");

			MergeKey mergeKeyPlused = new MergeKey();
			mergeKeyPlused = mergeKey1 + mergeKey2;

			MergeKey mergeKey3 = new MergeKey();
			mergeKey3.Add((ZString)"Hello");
			mergeKey3.Add((ZString)"Goodbye");

			Assert(mergeKeyPlused == mergeKey3);
		}

		public void TestIndexOf()
		{
			MergeKey mergeKey = new MergeKey();
			AssertEquals(-1, mergeKey.IndexOf((ZString)"Hello"));
			mergeKey.Add((ZString)"Hello");
			AssertEquals(0, mergeKey.IndexOf((ZString)"Hello"));
			AssertEquals(-1, mergeKey.IndexOf((ZInt)123));
			mergeKey.Add((ZInt)123);
			AssertEquals(1, mergeKey.IndexOf((ZInt)123));
		}
	}
}
