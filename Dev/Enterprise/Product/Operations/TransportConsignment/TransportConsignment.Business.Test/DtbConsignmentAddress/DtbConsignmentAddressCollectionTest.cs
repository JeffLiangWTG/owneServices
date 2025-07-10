using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentAddressCollection))]
	sealed class DtbConsignmentAddressCollectionTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentAddressCollection>
	{
		#region TestCollectionHasObject

		public void TestCollectionHasObject()
		{
			var consignment = Helper.CreateConsignment();
			var address1 = Helper.CreateConsignmentAddress(consignment);
			var address2 = Helper.CreateConsignmentAddress(consignment);
			var address3 = Helper.CreateConsignmentAddress(ZString.Empty);

			var addresses = new DtbConsignmentAddressCollection(consignment);
			AssertEquals("Collection should have 2 objects.", 2, addresses.Count);
			AssertContainsExactElementsInAnyOrder("Should contain address1 and address2", addresses, new[] { address1, address2 });
		}

		#endregion

		#region TestAddNew_WithInstructionType

		public void TestAddNew_WithInstructionType()
		{
			var collection = GetCollectionToTest();

			AssertEquals("", collection.AddNew("").LTS_InstructionType);
			AssertEquals(InstructionTypes.Codes.PickUp, collection.AddNew(InstructionTypes.Codes.PickUp).LTS_InstructionType);
			AssertEquals(InstructionTypes.Codes.Delivery, collection.AddNew(InstructionTypes.Codes.Delivery).LTS_InstructionType);
		}

		#endregion

		#region Implementation

		protected override DtbConsignmentAddressCollection GetCollectionToTest()
		{
			return new DtbConsignmentAddressCollection(Helper.CreateConsignment());
		}

		TransportConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportConsignmentTestHelper(Factory)); }
		}

		TransportConsignmentTestHelper helper;

		#endregion
	}
}
