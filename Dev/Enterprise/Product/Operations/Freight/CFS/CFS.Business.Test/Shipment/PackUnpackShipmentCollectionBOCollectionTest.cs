using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.CFS.Business.Testing
{
	[TestedType(typeof(PackUnpackShipmentCollection))]
	public class PackUnpackShipmentCollectionBOCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			Collection.AddRange(bizO1, bizO2);
			Factory.Save();
			Collection.Remove(bizO1);
			AssertEquals("Collection count", 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
			Assert("Element was in Database.  Element should not be deleted", !bizO1.IsDeleted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			PackUnpackLoadListConsol parent = Factory.New<PackUnpackLoadListConsol>();
			return new PackUnpackShipmentCollection(parent);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<PackUnpackShipment>();
		}
	}
}
