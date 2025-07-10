using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Environment.Business.Testing
{
	[TestedType(typeof(WhsProductStyleSizeCollection))]
	class WhsProductStyleSizeCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsProductStyleSizeCollection>
	{
		#region TestAddingProductSizesIncrementsSequenceByOne

		public void TestAddingProductSizesIncrementsSequenceByOne()
		{
			var collection = GetCollectionToTest();
			AssertEquals((ZByte)1, collection.AddNew().WSZ_Sequence);
			AssertEquals((ZByte)2, collection.AddNew().WSZ_Sequence);
			AssertEquals((ZByte)3, collection.AddNew().WSZ_Sequence);
		}

		#endregion

		#region TestMove

		public void TestMoveUp()
		{
			var collection = GetCollectionToTest();

			var s1 = collection.AddNew();
			var s2 = collection.AddNew();
			var s3 = collection.AddNew();
			var s4 = collection.AddNew();
			AssertProductSizeSequence(s1, s2, s3, s4);

			collection.MoveUp(s4);
			AssertProductSizeSequence(s1, s2, s4, s3);

			collection.MoveUp(s4);
			AssertProductSizeSequence(s1, s4, s2, s3);

			collection.MoveUp(s4);
			AssertProductSizeSequence(s4, s1, s2, s3);

			collection.MoveUp(s4);
			AssertProductSizeSequence(s4, s1, s2, s3);
		}

		public void TestMoveDown()
		{
			var collection = GetCollectionToTest();

			var s1 = collection.AddNew();
			var s2 = collection.AddNew();
			var s3 = collection.AddNew();
			var s4 = collection.AddNew();
			AssertProductSizeSequence(s1, s2, s3, s4);

			collection.MoveDown(s1);
			AssertProductSizeSequence(s2, s1, s3, s4);

			collection.MoveDown(s1);
			AssertProductSizeSequence(s2, s3, s1, s4);

			collection.MoveDown(s1);
			AssertProductSizeSequence(s2, s3, s4, s1);

			collection.MoveDown(s1);
			AssertProductSizeSequence(s2, s3, s4, s1);
		}

		void AssertProductSizeSequence(WhsProductStyleSize s1, WhsProductStyleSize s2, WhsProductStyleSize s3, WhsProductStyleSize s4)
		{
			AssertEquals(1, (ZInt)s1.WSZ_Sequence);
			AssertEquals(2, (ZInt)s2.WSZ_Sequence);
			AssertEquals(3, (ZInt)s3.WSZ_Sequence);
			AssertEquals(4, (ZInt)s4.WSZ_Sequence);
		}

		#endregion

		#region TestSequence

		public void TestSequence()
		{
			var collection = GetCollectionToTest();

			var s1 = collection.AddNew();
			var s2 = collection.AddNew();
			var s3 = collection.AddNew();
			var s4 = collection.AddNew();

			s1.WSZ_Sequence = 0;
			s2.WSZ_Sequence = 3;
			s3.WSZ_Sequence = 3;
			s4.WSZ_Sequence = 10;

			collection.Sequence();
			AssertEquals(1, (ZInt)s1.WSZ_Sequence);
			AssertNotEquals(s2.WSZ_Sequence, s3.WSZ_Sequence);
			Assert(s2.WSZ_Sequence == 2 || s2.WSZ_Sequence == 3);
			Assert(s3.WSZ_Sequence == 2 || s3.WSZ_Sequence == 3);
			AssertEquals(4, (ZInt)s4.WSZ_Sequence);
		}

		#endregion

		#region TestCollectionIsSortedBySequence

		public void TestCollectionIsSortedBySequence()
		{
			var productStyle = new WhsTestHelperFunctionsEnv(Factory).CreateProductStyle("STY", "Style");
			var collection = new WhsProductStyleSizeCollection(productStyle);

			var s1 = collection.AddNew();
			var s2 = collection.AddNew();
			var s3 = collection.AddNew();
			var s4 = collection.AddNew();

			s1.WSZ_Sequence = 10;
			s2.WSZ_Sequence = 5;
			s3.WSZ_Sequence = 7;
			s4.WSZ_Sequence = 3;

			AssertArrayEqualsByElements("Collection should be sorted by sequence.", new ZByte[] { 3, 5, 7, 10 }, collection.Select(s => s.WSZ_Sequence).ToArray());
		}

		#endregion

		#region Implementation

		protected override WhsProductStyleSizeCollection GetCollectionToTest()
		{
			var productStyle = Factory.New<WhsProductStyle>();
			return new WhsProductStyleSizeCollection(productStyle);
		}

		#endregion
	}
}
