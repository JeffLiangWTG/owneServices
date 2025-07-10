using CargoWise.EntityFramework.Testing;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	public class Confirmation_PackageViewValidationTest : BusinessObjectValidationTestCase
	{
		public void TestValidateParentID_InstructionDivot()
		{
			var booking = Helper.CreateBooking();
			var instructionCFS = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CFS", null);
			var instructionCNE = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, "CNE", null);
			var package1 = Helper.CreatePackage("p1");
			var package2 = Helper.CreatePackage("p2");
			var divotCFS1 = Helper.CreatePackageDivot(instructionCFS, package1, 1);
			var divotCFS2 = Helper.CreatePackageDivot(instructionCFS, package2, 1);
			var divotCNE1 = Helper.CreatePackageDivot(instructionCNE, package1, 1);
			var divotCNE2 = Helper.CreatePackageDivot(instructionCNE, package2, 1);

			var package1View = booking.Packages_PackageView.Find(package1);
			var package2View = booking.Packages_PackageView.Find(package2);

			var confirmation = Helper.CreateConfirmation(instructionCFS, ConfirmationTypes.Codes.PickUp);
			var confirmationPackage1View = new Confirmation_PackageView(confirmation, package1View);
			AssertNoErrors("Shouldn't be in error, package has that instruction confirmation", confirmationPackage1View.InstructionDivotPKInfo);

			var confirmationPackage2View = new Confirmation_PackageView(confirmation, package2View);
			AssertNoErrors("Shouldn't be in error, package has that instruction confirmation", confirmationPackage2View.InstructionDivotPKInfo);

			confirmationPackage2View.InstructionDivotPK = divotCFS2.PK;
			AssertNoErrors("Should not have errors as we are in package 2 view", confirmationPackage2View.InstructionDivotPKInfo);

			confirmationPackage2View.InstructionDivotPK = divotCFS1.PK;
			AssertHasErrors("Should have errors as list would be blank", confirmationPackage2View.InstructionDivotPKInfo);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}

		TransportBookingTestHelper helper;
	}
}
