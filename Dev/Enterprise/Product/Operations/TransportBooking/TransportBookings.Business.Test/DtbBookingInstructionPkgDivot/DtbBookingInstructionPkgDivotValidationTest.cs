using Enterprise.TransportCommon.Business.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class DtbBookingInstructionPkgDivotValidationTest : DtbTransportInstructionPkgDivotValidationTest
	{
		public void TestIsDuplicate()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var package = Helper.CreatePackage("p1");
			var divot1 = Helper.CreatePackageDivot(instruction, package, 1);
			divot1.Validation.ValidateKD_KN_BookingInstruction();
			divot1.Validation.ValidateKD_KP_Package();
			AssertNoErrors(divot1.KD_KN_BookingInstructionInfo);
			AssertNoErrors(divot1.KD_KP_PackageInfo);

			var divot2 = Helper.CreatePackageDivot(instruction, package, 1);
			divot1.Validation.ValidateKD_KN_BookingInstruction();
			divot1.Validation.ValidateKD_KP_Package();
			AssertHasErrors(divot1.KD_KN_BookingInstructionInfo);
			AssertHasErrors(divot1.KD_KP_PackageInfo);
		}

		public void TestIsSubInstruction()
		{
			var masterBooking = Helper.CreateBooking();
			masterBooking.KM_IsMaster = true;
			Factory.Save();

			var subBooking = Helper.CreateBooking();
			var subBookingPackage = subBooking.PackageJob.Packages.AddNew("PLT", "LOOSE1");

			masterBooking.SubBookings.AddRange(new[] { subBooking });
			Factory.Save();

			var masterInstruction = masterBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			var subBookingInstruction = subBooking.Instructions.AddNew(InstructionTypes.Codes.PickUp);
			subBookingInstruction.KN_KN_MasterBookingInstruction = masterInstruction.PK;
			Factory.Save();

			var masterDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			masterDivot.KD_KP_Package = subBookingPackage.PK;
			masterDivot.KD_KN_BookingInstruction = masterInstruction.PK;
			masterDivot.KD_Quantity = 1;
			AssertNoErrors(masterDivot.KD_KN_BookingInstructionInfo);

			var subDivot = Factory.New<DtbBookingInstructionPkgDivot>();
			subDivot.KD_KP_Package = subBookingPackage.PK;
			subDivot.KD_Quantity = 1;
			subDivot.KD_KN_BookingInstruction = subBookingInstruction.PK;
			AssertHasErrors(subDivot.KD_KN_BookingInstructionInfo);
		}

		public void TestIsSubInstruction_OKForNonMasterNonSubInstruction()
		{
			var booking = Helper.CreateBooking();
			var package = booking.PackageJob.Packages.AddNew("PLT", "LOOSE1");
			var instruction = booking.Instructions.AddNew(InstructionTypes.Codes.PickUp);

			var divot = Factory.New<DtbBookingInstructionPkgDivot>();
			divot.KD_KP_Package = package.PK;
			divot.KD_KN_BookingInstruction = instruction.PK;
			divot.KD_Quantity = 1;
			AssertNoErrors(divot.KD_KN_BookingInstructionInfo);
		}

		public void TestValidateKD_Quantity()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var package = Helper.CreatePackage("P1", 10, "BOX");
			var divot = Helper.CreatePackageDivot(instruction, package, 1);

			divot.KD_Quantity = 1;
			AssertNoErrors(divot.KD_QuantityInfo);

			divot.KD_Quantity = 0;
			AssertHasError(divot.KD_QuantityInfo, "Quantity cannot be less than 1.");

			divot.KD_Quantity = 11;
			AssertHasError(divot.KD_QuantityInfo, "You cannot assign 11 Boxes to this instruction because the quantity on the Package is only 10 Boxes.");

			divot.KD_Quantity = 10;
			AssertNoErrors(divot.KD_QuantityInfo);
		}

		public void TestValidateKD_QuantityOnZeroPackLine()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var package = Helper.CreatePackage("P1", 0, "BOX");
			package.KP_IsUnknownQty = true;

			var divot = Helper.CreatePackageDivot(instruction, package, 0);

			divot.KD_Quantity = 0;
			AssertNoErrors(divot.KD_QuantityInfo);

			divot.KD_Quantity = -1;
			AssertHasError(divot.KD_QuantityInfo, "Quantity cannot be less than 0.");

			divot.KD_Quantity = 11;
			AssertHasError(divot.KD_QuantityInfo, "You cannot assign any units to this instruction because the quantity on the Package is unknown.");
		}

		public void TestValidateKD_QuantityOnNonExistentPackage()
		{
			var divot = Factory.New<DtbBookingInstructionPkgDivot>();

			divot.KD_Quantity = 0;
			AssertHasError(divot.KD_QuantityInfo, "Quantity cannot be less than 1.");
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
