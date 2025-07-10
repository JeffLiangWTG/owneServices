using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportConsignment.Business.Testing
{
	[TestedType(typeof(DtbConsignmentConfirmationCollection))]
	public class DtbConsignmentConfirmationRelationshipTest : ActiveBusinessObjectCollectionTestCase<DtbConsignmentConfirmationCollection>
	{
		#region TestAddRemove

		public void TestAddRemove()
		{
			var consignment = Helper.CreateBookingConsignment();
			var confirmations = new DtbConsignmentConfirmationCollection(Factory, new DtbConsignmentConfirmationRelationship(consignment));
			AssertEquals("Precondition", 0, confirmations.Count);

			var instruction = consignment.Instructions.AddNew();
			AssertEquals("Precondition", 0, confirmations.Count);

			var packageDivot = instruction.PackageDivots.AddNew();
			AssertEquals("Divot doesn't have a package yet, so should still be 0.", 0, confirmations.Count);

			var confirmation = Factory.New<DtbConsignmentConfirmation>();
			confirmation.KK_KN_BookingInstruction = instruction.PK;
			confirmation.KK_ConfirmationType = "PIC";
			AssertContainsExactElementsInAnyOrder("After setting the package it should have found it, if not, check to see if listening onto KD_KP.", new DtbConsignmentConfirmation[] { confirmation }, confirmations);

			var confirmation2 = Factory.New<DtbConsignmentConfirmation>();
			confirmation2.KK_ConfirmationType = "PIC";
			confirmation2.KK_KD_BookingInstructionPkgDivot = packageDivot.PK;
			confirmation2.KK_KN_BookingInstruction = instruction.PK;
			AssertContainsExactElementsInAnyOrder("should now have 2", new[] { confirmation, confirmation2 }, confirmations);

			confirmation2.KK_KN_BookingInstruction = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder("Should now only have the first one.", new[] { confirmation }, confirmations);

			instruction.KN_KM_BookingMovement = ZGuid.Empty;
			AssertEquals("Disconnect from instruction instead, should now have none.", 0, confirmations.Count);
		}

		#endregion

		#region Implementation

		protected override DtbConsignmentConfirmationCollection GetCollectionToTest()
		{
			return new DtbConsignmentConfirmationCollection(Factory, new DtbConsignmentConfirmationRelationship(ConsignmentForCoreTest));
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var instruction = Helper.CreateInstruction(ConsignmentForCoreTest, InstructionTypes.Codes.PickUp);
			return instruction.Confirmations[0];
		}

		DtbBookingConsignment ConsignmentForCoreTest
		{
			get { return consignmentForCoreTest ?? (consignmentForCoreTest = Helper.CreateBookingConsignment()); }
		}

		TransportBookingConsignmentTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingConsignmentTestHelper(Factory)); }
		}

		DtbBookingConsignment consignmentForCoreTest;
		TransportBookingConsignmentTestHelper helper;

		#endregion
	}
}
