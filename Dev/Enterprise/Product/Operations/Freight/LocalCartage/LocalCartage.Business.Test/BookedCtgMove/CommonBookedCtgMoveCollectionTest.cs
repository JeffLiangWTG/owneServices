using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	[TestedType(typeof(CommonBookedCtgMoveCollection))]
	public class CommonBookedCtgMoveCollectionTest : ActiveBusinessObjectCollectionTestCase<CommonBookedCtgMoveCollection>
	{
		public void TestCommonBookedCtgMoveCollection_CartageWithAParentJob()
		{
			var cartage = Factory.New<CommonCartage>();
			var moveCollection = new CommonBookedCtgMoveCollectionForTest(cartage);
			Assert("New booked moves should be added for standalone cartage.", moveCollection.AllowNewForTest);
			var shipment = Factory.New<Enterprise.Integration.Forwarding.IForwardingShipment>();
			cartage.JJ_ParentTableCode = JobShipmentSchema.Constants.Prefix;
			cartage.JJ_ParentID = shipment.PK;
			moveCollection = new CommonBookedCtgMoveCollectionForTest(cartage);
			Assert("New booked moves should not be added from shipment.", !moveCollection.AllowNewForTest);
		}

		class CommonBookedCtgMoveCollectionForTest : CommonBookedCtgMoveCollection
		{
			public CommonBookedCtgMoveCollectionForTest(CommonCartage cartage) : base(cartage)
			{
			}

			public bool AllowNewForTest
			{
				get
				{
					return AllowNew;
				}
			}
		}
	}
}
