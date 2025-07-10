using CargoWise.EntityFramework.Testing;
using Enterprise.Warehouse.Cartonisation.Integration;
using NUnit.Framework;

namespace Enterprise.Warehouse.Cartonisation.Diagnostic.Testing
{
	[TestedType(typeof(DummyCartonisableItem))]
	public class DummyCartonisableItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestICartonisableItem_LocationPK()
		{
			var item = new DummyCartonisableItem();
			var itemSame = new DummyCartonisableItem();
			var itemLowerCase = new DummyCartonisableItem();
			var itemDifferent = new DummyCartonisableItem();

			item.Location = "LOCATION";
			itemSame.Location = "LOCATION";
			itemLowerCase.Location = "location";
			itemDifferent.Location = "OTHER";

			AssertEquals(((ICartonisableItem)item).LocationPK, ((ICartonisableItem)itemSame).LocationPK);
			AssertEquals(((ICartonisableItem)item).LocationPK, ((ICartonisableItem)itemLowerCase).LocationPK);
			AssertNotEquals(((ICartonisableItem)item).LocationPK, ((ICartonisableItem)itemDifferent).LocationPK);
		}
	}
}
