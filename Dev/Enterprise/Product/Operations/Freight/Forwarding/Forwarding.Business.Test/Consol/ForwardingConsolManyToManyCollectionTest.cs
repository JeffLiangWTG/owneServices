using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingConsolManyToManyCollection))]
	sealed class ForwardingConsolManyToManyCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return Shipment.Consols;
		}

		public void TestTestingCorrectCollection()
		{
			AssertEquals("Test the correct collection", typeof(ForwardingConsolManyToManyCollection), GetCollectionToTest().GetType());
		}

		public void TestParentShipment()
		{
			ForwardingConsolManyToManyCollection collection = (ForwardingConsolManyToManyCollection)GetCollectionToTest();
			AssertEquals(Shipment, collection.ParentShipment);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			Shipment.Factory.Save();
		}

		ForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<ForwardingShipment>();
				}
				return fShipment;
			}
		}

		ForwardingShipment fShipment;

		#endregion
	}
}
