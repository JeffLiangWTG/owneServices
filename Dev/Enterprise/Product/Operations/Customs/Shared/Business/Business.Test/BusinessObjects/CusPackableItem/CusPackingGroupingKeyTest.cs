using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.Business.Testing
{
	sealed class CusPackingGroupingKeyTest : TestCaseWithFactory
	{
		#region TestGetHashCode

		public void TestGetHashCode()
		{
			var packableItem = Factory.NewWithPrimaryKey<CusPackableItem>(ZGuid.BrettsGuid.ToGuid());

			var key = CusPackingGroupingKey.New(packableItem);
			AssertEquals("HashCode should be correct.", packableItem.PK.GetHashCode(), key.GetHashCode());
		}

		#endregion

		#region TestEquals

		public void TestEquals()
		{
			var packableItem = Factory.NewWithPrimaryKey<CusPackableItem>(ZGuid.BrettsGuid.ToGuid());
			var key1 = CusPackingGroupingKey.New(packableItem);
			var key2 = CusPackingGroupingKey.New(packableItem);

			Assert(key1.Equals(key2));

			var packableItem2 = Factory.NewWithPrimaryKey<CusPackableItem>(ZGuid.NewZGuid().ToGuid());
			var keyNotEqual = CusPackingGroupingKey.New(packableItem2);
			Assert(!key1.Equals(keyNotEqual));
		}

		#endregion

		#region TestSimilar

		public void TestSimilar()
		{
			var packableItem = Factory.NewWithPrimaryKey<CusPackableItem>(ZGuid.BrettsGuid.ToGuid());
			var key1 = CusPackingGroupingKey.New(packableItem);
			var key2 = CusPackingGroupingKey.New(packableItem);

			Assert(key1.IsSimilarItem_DoNotUse(key2));

			var packableItem2 = Factory.NewWithPrimaryKey<CusPackableItem>(ZGuid.NewZGuid().ToGuid());
			var keyNotEqual = CusPackingGroupingKey.New(packableItem2);
			Assert(!key1.IsSimilarItem_DoNotUse(keyNotEqual));
		}

		#endregion
	}
}
