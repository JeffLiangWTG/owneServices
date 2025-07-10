using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolShipmentCollection))]
	sealed class ForwardingConsolShipmentBOCollectionTest : BusinessObjectCollectionTestCase
	{
		public override void TestRemoveFromRelationship()
		{
			BusinessObject bizO1 = GetNewElementToAddToTheCollection();
			BusinessObject bizO2 = GetNewElementToAddToTheCollection();
			Factory.Save();
			BusinessObject biz03 = GetNewElementToAddToTheCollection();
			Collection.AddRange(bizO1, bizO2, biz03);

			Collection.Remove(bizO1);
			Collection.Remove(biz03);

			AssertEquals("Collection count", 1, Collection.Count);
			Assert("Contains element 2", Collection.Contains(bizO2));
			Assert("Doesn't contain element 1", !Collection.Contains(bizO1));
			Assert("Doesn't contain element 3", !Collection.Contains(biz03));
			Assert("Element 1 was saved and only removed, but not deleted", !bizO1.IsDeleted);
			Assert("Element 3 was unsaved and removed", biz03.IsDeleted);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ForwardingConsol forwardingConsol = Factory.New<ForwardingConsol>();
			return forwardingConsol.Shipments;
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return Factory.New<ForwardingShipment>();
		}
	}
}
