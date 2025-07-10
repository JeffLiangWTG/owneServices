using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Facts.Testing
{
	class FactHelpersTest : TestCase
	{
		public void TestConvertToNullableGuid()
		{
			var zGuid = ZGuid.NewZGuid();
			AssertEquals(zGuid.ToGuid(), zGuid.ConvertToNullableGuid());
			AssertNull(ZGuid.Empty.ConvertToNullableGuid());
		}

		public void TestConvertToNullableDateTime_ZDateTime()
		{
			var zDateTime = ZDateTime.Today;
			AssertEquals(zDateTime.ToDateTime(), zDateTime.ConvertToNullableDateTime());
			AssertNull(ZDateTime.Empty.ConvertToNullableDateTime());
		}

		public void TestConvertToNullableDateTime_ZDate()
		{
			var zDate = ZDate.Today;
			AssertEquals(zDate.ToDateTime(), zDate.ConvertToNullableDateTime());
			AssertNull(ZDate.Empty.ConvertToNullableDateTime());
		}

		public void TestConvertToNullableDateTime_ZDateTimeOffset()
		{
			var zDateTimeoffset = ZDateTimeOffset.Today;
			AssertEquals(zDateTimeoffset.ToDateTime(), zDateTimeoffset.ConvertToNullableDateTime());
			AssertNull(ZDateTimeOffset.Empty.ConvertToNullableDateTime());
		}
	}
}
