using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Integration;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class ShippingOrPackingingUnitListTest : TestCaseWithFactory
	{
		public void TestGetWithPieceType()
		{
			var list = ShippingOrPackingingUnitList.GetWithPieceType(Factory);
			var list2 = ShippingOrPackingingUnitList.GetWithPieceType(Factory);
			var list3 = ShippingOrPackingingUnitList.GetWithPieceType(new BusinessObjectFactory());
			Assert("Should be cached within the same factory", ReferenceEquals(list, list2));
			Assert("Should not be Cached with other factory", !ReferenceEquals(list, list3));
			var list4 = new ShippingOrPackingingUnitList();
			AssertEquals(list4.Count + 1, list.Count);
			AssertNotEquals(false, list.ContainsCode(ShippingOrPackingingUnitList.PiecesCode));
			AssertEquals(ShippingOrPackingingUnitList.PiecesDescription, list.GetDescriptionFromCode(ShippingOrPackingingUnitList.PiecesCode));
			foreach (ICodeDescription pair in list4)
			{
				AssertEquals(pair.Code, pair.Description, list.GetDescriptionFromCode(pair.Code));
			}

			var pieces = list[ShippingOrPackingingUnitList.PiecesCode];
			AssertNotEquals("Pieces should not be last in the list", list.Count - 1, list.IndexOfCode(pieces));
		}
	}
}
