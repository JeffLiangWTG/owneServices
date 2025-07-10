using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Packing.Business;
using Enterprise.Packing.Business.Testing;
using Enterprise.TransportCommon.Shared;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Business.Testing
{
	[TestedType(typeof(DtbBookingPackage_PackageView))]
	sealed class DtbBookingPackage_PackageViewTest : NonPersistentBusinessObjectTestCase
	{
		public void TestConstruction()
		{
			var booking = GetBookingBizO();
			var package = Factory.New<PkgPackage>();
			var package_PackageView = GetPackageView(package, booking);
			AssertEquals(package, package_PackageView.Package);
			AssertEquals(booking, package_PackageView.Booking);
		}

		public void TestInstructionDivots()
		{
			var booking = GetBookingBizO();
			var package = Factory.New<PkgPackage>();
			var package_PackageView = GetPackageView(package, booking);
			AssertEquals(0, package_PackageView.InstructionDivots.Count);
			AssertEquals(true, package_PackageView.IsRegisteredEditableChildObject(package_PackageView.InstructionDivots));

			var instruction = booking.Instructions.AddNew();
			var packageDivot = instruction.PackageDivots.AddNew();
			packageDivot.KD_KP_Package = package.PK;
			AssertContainsExactElementsInAnyOrder(new DtbBookingInstructionPkgDivot[] { packageDivot }, package_PackageView.InstructionDivots);
		}

		public void TestConfirmations()
		{
			var booking = GetBookingBizO();
			var instruction = booking.Instructions.AddNew();
			var divot_p1 = Helper.CreatePackageDivot(instruction);
			var divot_p2 = Helper.CreatePackageDivot(instruction);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var p2 = Helper.CreatePackage("p2", divot_p2);
			var confirmation_ALL = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var confirmation_p1 = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);
			var confirmation_p2 = Helper.CreateConfirmation(divot_p2, ConfirmationTypes.Codes.Delivery);

			var packageView_P1 = GetPackageView(p1, booking);
			var packageView_P2 = GetPackageView(p2, booking);

			AssertEquals(true, packageView_P1.IsRegisteredEditableChildObject(packageView_P1.Confirmations));

			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1, confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p2, confirmation_ALL }, packageView_P2.Confirmations);

			var newInstruction = booking.Instructions.AddNew();
			divot_p1.KD_KN_BookingInstruction = newInstruction.PK;
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1 }, packageView_P1.Confirmations);

			var confirmation_p1New = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1, confirmation_p1New }, packageView_P1.Confirmations);

			var confirmation_Added = packageView_P1.Confirmations.AddNew();
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1, confirmation_p1New, confirmation_Added }, packageView_P1.Confirmations);
		}

		public void TestConfirmations_DontHitCollectionsBeforeAdd()
		{
			var booking = GetBookingBizO();
			var instruction = booking.Instructions.AddNew();
			var divot_p1 = Helper.CreatePackageDivot(instruction, 1);
			var divot_p2 = Helper.CreatePackageDivot(instruction, 1);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var p2 = Helper.CreatePackage("p2", divot_p2);
			var confirmation_ALL = Helper.CreateConfirmation(instruction, ConfirmationTypes.Codes.PickUp);
			var confirmation_p1 = Helper.CreateConfirmation(divot_p1, ConfirmationTypes.Codes.Delivery);

			var packageView_P1 = GetPackageView(p1, booking);
			var packageView_P2 = GetPackageView(p2, booking);

			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1, confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p2.PK;

			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_p1 }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);
		}

		public void TestConfirmations_EnsureCollectionsHitBeforeAdd()
		{
			var booking = GetBookingBizO();
			var instruction1 = booking.Instructions.AddNew();
			var instruction2 = booking.Instructions.AddNew();
			var instruction3 = booking.Instructions.AddNew();

			var divot_p1 = Helper.CreatePackageDivot(instruction1, 1);
			var divot_p2 = Helper.CreatePackageDivot(instruction1, 1);
			var p1 = Helper.CreatePackage("p1", divot_p1);
			var p2 = Helper.CreatePackage("p2", divot_p2);

			var packageView_P1 = GetPackageView(p1, booking);
			var packageView_P2 = GetPackageView(p2, booking);
			AssertContainsExactElementsInAnyOrder(Array.Empty<DtbBookingConfirmation>(), packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(Array.Empty<DtbBookingConfirmation>(), packageView_P2.Confirmations);

			var confirmation_ALL = Helper.CreateConfirmation(instruction1, ConfirmationTypes.Codes.PickUp);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p2.PK;
			AssertContainsExactElementsInAnyOrder(Array.Empty<DtbBookingConfirmation>(), packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p2.PK;
			AssertContainsExactElementsInAnyOrder(Array.Empty<DtbBookingConfirmation>(), packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = divot_p1.PK;
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(Array.Empty<DtbBookingConfirmation>(), packageView_P2.Confirmations);

			confirmation_ALL.KK_KD_BookingInstructionPkgDivot = ZGuid.Empty;
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P1.Confirmations);
			AssertContainsExactElementsInAnyOrder(new DtbBookingConfirmation[] { confirmation_ALL }, packageView_P2.Confirmations);
		}

		public void TestQuantityFromInstructions()
		{
			DtbBooking booking;
			PkgPackage packageA;
			PkgPackage packageB;
			SetupDataForTotalsFromInstructionsTests(out booking, out packageA, out packageB);

			CombineAssertions("Check that calculation of QuantityFromInstructions is correct", () =>
			{
				AssertEquals("QuantityFromInstructions for viewForPackageA should take the highest of Pickup Instructions divot quantities (5 + 6 = 11) vs Delivery Instructions divot quantities (2 + 7 = 9) = 11, as long as it's less than the quantity on package A (15)", 11, booking.Packages_PackageView.Find(packageA).QuantityFromInstructions);
				AssertEquals("QuantityFromInstructions for viewForPackageB should use quantity from PackageB when the higher of Pickup Instruction divot quantities (2 + 12 = 14) and Delivery Instruction divots quantity (7 + 14 = 21) - 21 - is higher than the PackageB quantity (20)", 20, booking.Packages_PackageView.Find(packageB).QuantityFromInstructions);
			});
		}

		public void TestQuantityFromInstructionsDoesNotThrowEvenWithNullDivotsOrNullDivotInstructionLink()
		{
			DtbBooking booking;
			PkgPackage packageA;
			PkgPackage packageB;
			SetupDataForTotalsFromInstructionsTests(out booking, out packageA, out packageB);
			AddNullDivotsAndNullDivotInstructionLinksToPackageOnBooking(booking, packageA);
			AddNullDivotsAndNullDivotInstructionLinksToPackageOnBooking(booking, packageB);

			var viewForPackageA = booking.Packages_PackageView.Find(packageA);
			var viewForPackageB = booking.Packages_PackageView.Find(packageB);
			var quantityFromInstructionsViewForPackageA = 0;
			var quantityFromInstructionsViewForPackageB = 0;
			CombineAssertions("Check that QuantityFromInstructions calculates correctly and without throwing exceptions despite the presence of null and incomplete divots in the DtbBookingPackage_PackageView", () =>
			{
				AssertNoExceptionThrown("Property QuantityFromInstructions should not throw exception despite the presence of null and incomplete divots in the DtbBookingPackage_PackageView viewForPackageA", () => { quantityFromInstructionsViewForPackageA = viewForPackageA.QuantityFromInstructions; });
				AssertEquals("QuantityFromInstructions for viewForPackageA should take the highest of Pickup Instructions divot quantities (5 + 6 = 11) vs Delivery Instructions divot quantities (2 + 7 = 9) = 11, as long as it's less than the quantity on package A (15)", 11, quantityFromInstructionsViewForPackageA);

				AssertNoExceptionThrown("Property QuantityFromInstructions should not throw exception despite the presence of null and incomplete divots in the DtbBookingPackage_PackageView viewForPackageB", () => { quantityFromInstructionsViewForPackageB = viewForPackageB.QuantityFromInstructions; });
				AssertEquals("QuantityFromInstructions for viewForPackageB should use quantity from PackageB when the higher of Pickup Instruction divot quantities (2 + 12 = 14) and Delivery Instruction divots quantity (7 + 14 = 21) - 21 - is higher than the PackageB quantity (20)", 20, quantityFromInstructionsViewForPackageB);
			});
		}

		public void TestWeightFromInstructions()
		{
			DtbBooking booking;
			PkgPackage packageA;
			PkgPackage packageB;
			SetupDataForTotalsFromInstructionsTests(out booking, out packageA, out packageB);

			AssertEquals("QuantityFromInstructions for viewForPackageA should take the highest of Pickup Instructions divot quantities (5 + 6 = 11) vs Delivery Instructions divot quantities (2 + 7 = 9) = 11, as long as it's less than the quantity on package A (15). To get WeightFromInstructions, multiply per-pack weight by QuantityFromInstructions, so WeightFromInstructions should be 11 * 10 = 110", 110m, booking.Packages_PackageView.Find(packageA).WeightFromInstructions.Amount);
			AssertEquals("Should have same WU as PackageA.", Constants.Weight.Kilograms, booking.Packages_PackageView.Find(packageA).WeightFromInstructions.Unit);
			AssertEquals("QuantityFromInstructions for viewForPackageB should use quantity from PackageB when the higher of Pickup Instruction divot quantities (2 + 12 = 14) and Delivery Instruction divots quantity (7 + 14 = 21) - 21 - is higher than the PackageB quantity (20). To get WeightFromInstructions, multiply per-pack weight by QuantityFromInstructions, so WeightFromInstructions should be 20 * 10 = 200", 200m, booking.Packages_PackageView.Find(packageB).WeightFromInstructions.Amount);
			AssertEquals("Should have same WU as PackageB.", Constants.Weight.Pounds, booking.Packages_PackageView.Find(packageB).WeightFromInstructions.Unit);
		}

		public void TestVolumeFromInstructions()
		{
			DtbBooking booking;
			PkgPackage packageA;
			PkgPackage packageB;
			SetupDataForTotalsFromInstructionsTests(out booking, out packageA, out packageB);

			AssertEquals("QuantityFromInstructions for viewForPackageA should take the highest of Pickup Instructions divot quantities (5 + 6 = 11) vs Delivery Instructions divot quantities (2 + 7 = 9) = 11, as long as it's less than the quantity on package A (15). To get VolumeFromInstructions, multiply per-pack volume by QuantityFromInstructions, so VolumeFromInstructions should be 11 * 10 = 110", 110m, booking.Packages_PackageView.Find(packageA).VolumeFromInstructions.Amount);
			AssertEquals("Should have same VU as PackageA.", Constants.Volume.CubicMetres, booking.Packages_PackageView.Find(packageA).VolumeFromInstructions.Unit);
			AssertEquals("QuantityFromInstructions for viewForPackageB should use quantity from PackageB when the higher of Pickup Instruction divot quantities (2 + 12 = 14) and Delivery Instruction divots quantity (7 + 14 = 21) - 21 - is higher than the PackageB quantity (20). To get VolumeFromInstructions, multiply per-pack volume by QuantityFromInstructions, so VolumeFromInstructions should be 20 * 10 = 200", 200m, booking.Packages_PackageView.Find(packageB).VolumeFromInstructions.Amount);
			AssertEquals("Should have same VU as PackageB.", Constants.Volume.CubicFeet, booking.Packages_PackageView.Find(packageB).VolumeFromInstructions.Unit);
		}

		void SetupDataForTotalsFromInstructionsTests(out DtbBooking booking, out PkgPackage packageA, out PkgPackage packageB)
		{
			booking = GetBookingBizO();
			var pickupInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var pickupInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var multiInstruction = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi);
			var deliveryInstruction1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var deliveryInstruction2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			var packageJob = GetPackageJob(booking);
			packageA = PackingHelper.CreatePackage(packageJob, 15, Constants.PkgUnit.Pallet);
			packageB = PackingHelper.CreatePackage(packageJob, 20, Constants.PkgUnit.Box);
			packageA.KP_Weight = 150;
			packageA.KP_WeightUQ = Constants.Weight.Kilograms;
			packageA.KP_Volume = 150;
			packageA.KP_VolumeUQ = Constants.Volume.CubicMetres;

			packageB.KP_Weight = 200;
			packageB.KP_WeightUQ = Constants.Weight.Pounds;
			packageB.KP_Volume = 200;
			packageB.KP_VolumeUQ = Constants.Volume.CubicFeet;

			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction1, packageA, 5);
			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction1, packageB, 2);
			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction2, packageA, 6);
			SetQuantityOnExistingDivotOrCreateNewDivot(pickupInstruction2, packageB, 12);

			SetQuantityOnExistingDivotOrCreateNewDivot(multiInstruction, packageA, 15);
			SetQuantityOnExistingDivotOrCreateNewDivot(multiInstruction, packageB, 5);

			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction1, packageA, 2);
			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction1, packageB, 7);
			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction2, packageA, 7);
			SetQuantityOnExistingDivotOrCreateNewDivot(deliveryInstruction2, packageB, 14);
		}

		void AddNullDivotsAndNullDivotInstructionLinksToPackageOnBooking(DtbBooking booking, PkgPackage package)
		{
			try
			{
				booking.Packages_PackageView.Find(package).InstructionDivots.Add(null);
			}
			catch
			{
				// swallow, don't throw - we want a null object in the InstructionDivots collection for the test.
			}
			var instructionDivot = booking.Packages_PackageView.Find(package).InstructionDivots.AddNew();
			instructionDivot.KD_KP_Package = package.PK;
		}

		void SetQuantityOnExistingDivotOrCreateNewDivot(DtbBookingInstruction instruction, PkgPackage package, int quantity)
		{
			var existingDivot = instruction.PackageDivots.Cast<DtbBookingInstructionPkgDivot>().SingleOrDefault(d => d.KD_KP_Package == package.PK);
			if (existingDivot != null)
			{
				existingDivot.KD_Quantity = quantity;
			}
			else
			{
				Helper.CreatePackageDivot(instruction, package, quantity);
			}
		}

		PkgPackageJob GetPackageJob(DtbBooking booking)
		{
			return booking.ConsolidationSingleJob.PackageJob;
		}

		public void TestDelete()
		{
			var booking = GetBookingBizO();
			var package1 = Factory.New<PkgPackage>();
			var package2 = Factory.New<PkgPackage>();
			var instructionA = booking.Instructions.AddNew();
			var instructionB = booking.Instructions.AddNew();
			var instructionC = booking.Instructions.AddNew();
			var package1ADivot = Helper.CreatePackageDivot(instructionA, package1, 0);
			var package1BDivot = Helper.CreatePackageDivot(instructionB, package1, 0);
			var package2BDivot = Helper.CreatePackageDivot(instructionB, package2, 0);
			var package2CDivot = Helper.CreatePackageDivot(instructionC, package2, 0);

			var package1_PackageView = GetPackageView(package1, booking);
			var package2_PackageView = GetPackageView(package2, booking);

			package1_PackageView.Delete();
			AssertEquals(true, package1ADivot.IsDeleted);
			AssertEquals(true, package1BDivot.IsDeleted);
			AssertEquals(false, package2BDivot.IsDeleted);
			AssertEquals(false, package2CDivot.IsDeleted);

			package2_PackageView.Delete();
			AssertEquals(true, package2BDivot.IsDeleted);
			AssertEquals(true, package2CDivot.IsDeleted);
		}

		public void TestIsDeleted()
		{
			var packageView = GetPackageView();
			AssertEquals(false, packageView.IsDeleted);
		}

		DtbBooking GetBookingBizO()
		{
			return Helper.CreateBooking();
		}

		DtbBookingPackage_PackageView GetPackageView()
		{
			return new DtbBookingPackage_PackageView(Factory);
		}

		DtbBookingPackage_PackageView GetPackageView(PkgPackage package, DtbBooking booking)
		{
			return new DtbBookingPackage_PackageView(package, booking);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var booking = Helper.CreateBooking();
			var package = Factory.New<PkgPackage>();
			return new DtbBookingPackage_PackageView(package, booking);
		}

		TransportBookingTestHelper Helper
		{
			get { return helper ?? (helper = new TransportBookingTestHelper(Factory)); }
		}
		TransportBookingTestHelper helper;

		PackingTestHelper PackingHelper
		{
			get { return packingHelper ?? (packingHelper = new PackingTestHelper(Factory)); }
		}

		PackingTestHelper packingHelper;
	}
}
