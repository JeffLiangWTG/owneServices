using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Packing.Business;
using Enterprise.TransportCommon.Shared;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(Confirmation_PackageView))]
	public class Confirmation_PackageViewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstruction()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "PIC");
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var confirmation = Helper.CreateConfirmation(instruction, "PIC");
			var confirmationView = new Confirmation_PackageView(confirmation, packageView);
			AssertNotNull(confirmationView);
		}

		public void TestConfirmation()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "PIC");
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var confirmation = Helper.CreateConfirmation(instruction, "PIC");
			var confirmationView = new Confirmation_PackageView(confirmation, packageView);
			AssertEquals(confirmation, confirmationView.Confirmation);
		}

		public void TestPackage()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "PIC");
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var confirmation = Helper.CreateConfirmation(instruction, "PIC");
			var confirmationView = new Confirmation_PackageView(confirmation, packageView);
			AssertEquals(package, confirmationView.Package);
		}

		public void TestPackageView()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "PIC");
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var confirmation = Helper.CreateConfirmation(instruction, "PIC");
			var confirmationView = new Confirmation_PackageView(confirmation, packageView);
			AssertEquals(packageView, confirmationView.PackageView);
		}

		public void TestPackageDivot()
		{
			var booking = Helper.CreateBooking();

			var picInstruction = Helper.CreateInstruction(booking, "PIC");
			var package = Factory.New<PkgPackage>();
			var picDiv = Helper.CreatePackageDivot(picInstruction, package, 1);
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var picConfirmation = Helper.CreateConfirmation(picInstruction, "PIC");
			var picConfirmationView = new Confirmation_PackageView(picConfirmation, packageView);
			AssertEquals(picDiv, picConfirmationView.PackageDivot);

			var dlvInstruction = Helper.CreateInstruction(booking, "DLV");
			var dlvDiv = Helper.CreatePackageDivot(dlvInstruction, package, 1);
			var dlvConfirmation = Helper.CreateConfirmation(dlvInstruction, "PIC");
			var dlvConfirmationView = new Confirmation_PackageView(dlvConfirmation, packageView);
			AssertEquals(dlvDiv, dlvConfirmationView.PackageDivot);

			picConfirmation.KK_KD_BookingInstructionPkgDivot = picDiv.PK;
			AssertEquals(picDiv, picConfirmationView.PackageDivot);

			dlvConfirmation.KK_KD_BookingInstructionPkgDivot = dlvDiv.PK;
			AssertEquals(dlvDiv, dlvConfirmationView.PackageDivot);
		}

		public void TestInstructionDivotPK()
		{
			var booking = Helper.CreateBooking();
			var instruction = booking.Instructions.AddNew();
			var confirmation = instruction.Confirmations.AddNew();
			AssertEquals("Parent is Instruction", instruction.PK, confirmation.KK_KN_BookingInstruction);
			Assert("Parent is Instruction", confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);

			var package1 = Helper.CreatePackage("p1");
			var divot1 = Helper.CreatePackageDivot(instruction, package1, 1);
			var package2 = Helper.CreatePackage("p2");
			var divot2 = Helper.CreatePackageDivot(instruction, package2, 1);

			AssertEquals("Parent is Instruction", instruction.PK, confirmation.KK_KN_BookingInstruction);
			Assert("Parent is Instruction", confirmation.KK_KD_BookingInstructionPkgDivot.IsEmpty);

			var package1View = booking.Packages_PackageView.Find(package1);
			var package2View = booking.Packages_PackageView.Find(package2);
			var confirmationPackage1View = new Confirmation_PackageView(confirmation, package1View);
			AssertEquals("we are in package1 view", divot1.PK, confirmationPackage1View.InstructionDivotPK);

			var confirmationPackage2View = new Confirmation_PackageView(confirmation, package2View);
			AssertEquals("we are in package2 view", divot2.PK, confirmationPackage2View.InstructionDivotPK);

			// setting

			var instruction2 = booking.Instructions.AddNew();
			var divot22 = Helper.CreatePackageDivot(instruction2, package2, 1);
			confirmationPackage2View.InstructionDivotPK = divot22.PK;
			AssertEquals("changed to a divot with instruction 2", divot22.PK, confirmationPackage2View.InstructionDivotPK);
			AssertEquals("changed to a divot with instruction 2, should have changed to divot22", divot22.PK, confirmation.KK_KD_BookingInstructionPkgDivot);

			confirmationPackage2View.InstructionDivotPK = ZGuid.Empty;
			AssertEquals("Empty", ZGuid.Empty, confirmationPackage2View.InstructionDivotPK);
			AssertEquals("Empty, should have not changed parent", divot22.PK, confirmation.KK_KD_BookingInstructionPkgDivot);

			confirmationPackage2View.InstructionDivotPK = ZGuid.Invalid;
			AssertEquals("Invalid", ZGuid.Invalid, confirmationPackage2View.InstructionDivotPK);
			AssertEquals("Invalid, should have not changed parent", divot22.PK, confirmation.KK_KD_BookingInstructionPkgDivot);

			confirmationPackage2View.InstructionDivotPK = ZGuid.Missing;
			AssertEquals("Missing, should have reset", divot22.PK, confirmationPackage2View.InstructionDivotPK);
			AssertEquals("Missing, should have not changed parent", divot22.PK, confirmation.KK_KD_BookingInstructionPkgDivot);
		}

		public void TestSplitProperties()
		{
			TestSplitCore(DtbBookingConfirmationSchema.Constants.KK_ConfirmationType, Confirmation_PackageView.Schema.ConfirmationType, (ZString)"PIC", (ZString)"DLV");
			TestSplitCore(DtbBookingConfirmation.Schema.ConfirmationDescription, Confirmation_PackageView.Schema.ConfirmationDescription, (ZString)"Pickup", (ZString)"Delivery");
			TestSplitCore(DtbBookingConfirmationSchema.Constants.KK_Actual, Confirmation_PackageView.Schema.Actual, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			TestSplitCore(DtbBookingConfirmationSchema.Constants.KK_Estimated, Confirmation_PackageView.Schema.Estimated, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			TestSplitCore(DtbBookingConfirmationSchema.Constants.KK_ReceivedBy, Confirmation_PackageView.Schema.ReceivedBy, (ZString)"Bob", (ZString)"Zack");
			TestSplitCore(DtbBookingConfirmationSchema.Constants.KK_ReferenceNum, Confirmation_PackageView.Schema.ReferenceNum, (ZString)"12345", (ZString)"54321");
			TestSplitCore(DtbBookingConfirmationSchema.Constants.KK_RequiredFrom, Confirmation_PackageView.Schema.RequiredFrom, ZDateTime.Now, ZDateTime.Now.AddDays(1));
			TestSplitCore(DtbBookingConfirmationSchema.Constants.KK_RequiredTo, Confirmation_PackageView.Schema.RequiredTo, ZDateTime.Now, ZDateTime.Now.AddDays(1));
		}

		void TestSplitCore(string confirmationSchemaColumn, string confirmationViewSchemaColumn, IZType initialValue, IZType newValue)
		{
			var booking = Helper.CreateBooking();
			var instructionCTO = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp, "CTO", null);
			var instructionCFS = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery, "CFS", null);
			var package1 = Helper.CreatePackage("p1", 20, "PLT");
			var package2 = Helper.CreatePackage("p2", 20, "PLT");
			var package3 = Helper.CreatePackage("p3", 20, "PLT");
			var divot_CTO_P1 = Helper.CreatePackageDivot(instructionCTO, package1, 1);
			var divot_CTO_P2 = Helper.CreatePackageDivot(instructionCTO, package2, 1);
			var divot_CTO_P3 = Helper.CreatePackageDivot(instructionCTO, package3, 1);
			var divot_CFS_P1 = Helper.CreatePackageDivot(instructionCFS, package1, 1);
			var divot_CFS_P2 = Helper.CreatePackageDivot(instructionCFS, package2, 1);
			var divot_CFS_P3 = Helper.CreatePackageDivot(instructionCFS, package3, 1);

			var confirmation = instructionCTO.Confirmations.AddNew();
			AssertContainsExactElementsInAnyOrder("instructionCTO", new DtbBookingConfirmation[] { confirmation }, instructionCTO.Confirmations);
			AssertContainsExactElementsInAnyOrder("instructionCFS", Array.Empty<DtbBookingConfirmation>(), instructionCFS.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P1", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P2", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P3", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P3.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P1", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P2", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P3", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P3.Confirmations);

			confirmation[confirmationSchemaColumn] = initialValue;
			AssertContainsExactElementsInAnyOrder("instructionCTO", new DtbBookingConfirmation[] { confirmation }, instructionCTO.Confirmations);
			AssertContainsExactElementsInAnyOrder("instructionCFS", Array.Empty<DtbBookingConfirmation>(), instructionCFS.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P1", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P2", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P3", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P3.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P1", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P2", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P3", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P3.Confirmations);

			var package1Wrapper = booking.Packages_PackageView.Find(package1);
			var package2Wrapper = booking.Packages_PackageView.Find(package2);
			var package3Wrapper = booking.Packages_PackageView.Find(package3);

			var confirmationPackage1View = new Confirmation_PackageView(confirmation, package1Wrapper);
			confirmationPackage1View[confirmationViewSchemaColumn] = newValue;
			var confirmationP2 = package2Wrapper.Confirmations[0];
			var confirmationP3 = package3Wrapper.Confirmations[0];
			AssertNotEquals(confirmationP2, confirmation);
			AssertNotEquals(confirmationP3, confirmation);
			AssertEquals("new value", newValue, confirmation[confirmationSchemaColumn]);
			AssertEquals("original value", initialValue, confirmationP2[confirmationSchemaColumn]);
			AssertEquals("original value", initialValue, confirmationP3[confirmationSchemaColumn]);
			AssertContainsExactElementsInAnyOrder("instructionCTO", new DtbBookingConfirmation[] { confirmation, confirmationP2, confirmationP3 }, instructionCTO.Confirmations);
			AssertContainsExactElementsInAnyOrder("instructionCFS", Array.Empty<DtbBookingConfirmation>(), instructionCFS.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P1", new DtbBookingConfirmation[] { confirmation }, divot_CTO_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P2", new DtbBookingConfirmation[] { confirmationP2 }, divot_CTO_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CTO_P3", new DtbBookingConfirmation[] { confirmationP3 }, divot_CTO_P3.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P1", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P2", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P2.Confirmations);
			AssertContainsExactElementsInAnyOrder("divot_CFS_P3", Array.Empty<DtbBookingConfirmation>(), divot_CFS_P3.Confirmations);
		}

		public void TestInstructionDivots()
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

			var confirmation = instructionCFS.Confirmations.AddNew();

			var expected = new CodeDescriptionPairList();
			var confirmationPackage1View = new Confirmation_PackageView(confirmation, package1View);
			booking.SetInstructionViewAndSelectedPackage(TransportBookingInstructionView.Package, package1View);
			expected.AddPair(divotCFS1.PK, instructionCFS.Description, string.Format("Applies to '{0}' Only", instructionCFS.Description));
			expected.AddPair(divotCNE1.PK, instructionCNE.Description, string.Format("Applies to '{0}' Only", instructionCNE.Description));
			AssertContainsExactElementsInAnyOrder(expected, confirmationPackage1View.InstructionDivots);

			var confirmationPackage2View = new Confirmation_PackageView(confirmation, package2View);
			expected.Clear();
			expected.AddPair(divotCFS2.PK, instructionCFS.Description, string.Format("Applies to '{0}' Only", instructionCFS.Description));
			expected.AddPair(divotCNE2.PK, instructionCNE.Description, string.Format("Applies to '{0}' Only", instructionCNE.Description));
			AssertContainsExactElementsInAnyOrder(expected, confirmationPackage2View.InstructionDivots);
		}

		public void TestDelete()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "PIC");
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			var package1View = new DtbBookingPackage_PackageView(package1, booking);
			var package2View = new DtbBookingPackage_PackageView(package2, booking);

			var divot1 = Helper.CreatePackageDivot(instruction, package1, 10);
			var divot2 = Helper.CreatePackageDivot(instruction, package2, 10);

			var confirmation = Helper.CreateConfirmation(instruction, "PIC");
			var confirmationView = new Confirmation_PackageView(confirmation, package1View);

			AssertEquals("Divot 1 has 1 confirmation (from instruction)", 1, divot1.Confirmations.Count);
			AssertEquals("Divot 2 has 1 confirmation (from instruction)", 1, divot2.Confirmations.Count);
			AssertEquals("Divot 1 confirmation and Divot 2 confirmation are the same (from instruction)", divot1.Confirmations[0], divot2.Confirmations[0]);

			confirmationView.Delete();
			AssertEquals(true, confirmation.IsDeleted);
			AssertEquals(false, package1.IsDeleted);
			AssertEquals(false, package2.IsDeleted);
			AssertEquals(false, divot1.IsDeleted);
			AssertEquals(false, divot2.IsDeleted);

			AssertEquals("Divot 1 now has no confirmation", 0, divot1.Confirmations.Count);
			AssertEquals("Divot 2 still has the split off confirmation", 1, divot2.Confirmations.Count);

			var confirmation2 = divot2.Confirmations[0];
			var confirmation2View = new Confirmation_PackageView(confirmation2, package2View);
			confirmation2View.Delete();
			AssertEquals(true, confirmation2.IsDeleted);
			AssertEquals(false, package1.IsDeleted);
			AssertEquals(false, package2.IsDeleted);
			AssertEquals(false, divot1.IsDeleted);
			AssertEquals(false, divot2.IsDeleted);
		}

		public void TestValidationType()
		{
			AssertEquals(typeof(Confirmation_PackageViewValidation), ((Confirmation_PackageView)GetNewBusinessObject()).Validation.GetType());
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Helper.CreateBooking();
			var instruction = Helper.CreateInstruction(booking, "PIC");
			var package = Factory.New<PkgPackage>();
			var packageView = new DtbBookingPackage_PackageView(package, booking);
			var confirmation = Helper.CreateConfirmation(instruction, "PIC");
			return new Confirmation_PackageView(confirmation, packageView);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;
	}
}
