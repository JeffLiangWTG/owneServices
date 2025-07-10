using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentInstructionCollection))]
	public class DtbConsignmentInstructionCollectionTest : DtbTransportInstructionCollectionTest<DtbConsignmentInstruction, DtbConsignmentInstructionCollection>
	{
		#region TestDeliveries

		public void TestDeliveries()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			AssertContainsExactElementsInAnyOrder(new[] { consignment.DeliveryInstruction }, consignment.Instructions.Deliveries);
		}

		#endregion

		#region TestPickUps

		public void TestPickUps()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			AssertContainsExactElementsInAnyOrder(new[] { consignment.PickupInstruction }, consignment.Instructions.PickUps);
		}

		#endregion

		#region Implementation

		protected override DtbTransport GetNewTransport()
		{
			return Factory.New<DtbBookingConsignment>();
		}

		protected override DtbConsignmentInstructionCollection GetCollectionToTest()
		{
			return new DtbConsignmentInstructionCollection((DtbBookingConsignment)GetNewTransport());
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
