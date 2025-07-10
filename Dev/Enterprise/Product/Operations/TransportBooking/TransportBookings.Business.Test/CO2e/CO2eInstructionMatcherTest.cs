using System.Linq;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	class CO2eInstructionMatcherTest : DtbBookingTestCaseWithFactory
	{
		public void TestNoPickup()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 4, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			Helper.CreatePackageDivot(Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery), package, 4);

			// Act
			var result = new CO2eInstructionMatcher(booking).FindAllMatches();

			// Assert
			Assert("There should be no matches as there is no pickup.", !result.IsValid);
		}

		public void TestNoMatches()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 4, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			Helper.CreatePackageDivot(pic, package, 4);
			Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);

			// Act
			var result = new CO2eInstructionMatcher(booking).FindAllMatches();

			// Assert
			Assert("There should be no matches.", !result.IsValid);
		}

		public void TestBasicMatching()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 4, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic, package, 4);

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv, package, 4);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals(@$"
Loaded '4: 1000' of '{package.PK}' by '{divot1.PK}'
UnLoaded '4: 1000' of '{package.PK}' by '{divot2.PK}' from '{divot1.PK}'
", result.ToString());
		}

		public void TestPartialUnloading()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 4, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic4 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic4, package, 4);

			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv2, package, 2);

			var dlv2again = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv2again, package, 2);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals(@$"
Loaded '4: 1000' of '{package.PK}' by '{divot1.PK}'
UnLoaded '2: 500' of '{package.PK}' by '{divot2.PK}' from '{divot1.PK}'
UnLoaded '2: 500' of '{package.PK}' by '{divot3.PK}' from '{divot1.PK}'
", result.ToString());
		}

		public void TestNoMatchingDelivery()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 4, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var package2 = Helper.CreatePackage("PKG2", null, 4, 500);
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			Helper.CreatePackageDivot(pic1, package1);
			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = Helper.CreatePackageDivot(pic2, package2);

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv, package2);

			// Act
			var result = new CO2eInstructionMatcher(booking).FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals("Only the pickup and delivery for package 2 should be recorded as the pickup for package 1 did not have a corresponding delivery", @$"
Loaded '4: 500' of '{package2.PK}' by '{divot2.PK}'
UnLoaded '4: 500' of '{package2.PK}' by '{divot3.PK}' from '{divot2.PK}'
", result.ToString());
		}

		public void TestMultiplePickup()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 10, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, package, 10);

			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = Helper.CreatePackageDivot(pic2, package, 10);

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv, package, 20);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals(@$"
Loaded '10: 1000' of '{package.PK}' by '{divot1.PK}'
UnLoaded '10: 1000' of '{package.PK}' by '{divot3.PK}' from '{divot1.PK}'
", result.ToString());
		}

		public void TestExcessDeliveryAttempt()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 4, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic, package, 4);

			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv1, package, 3);

			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv2, package, 3);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals(@$"
Loaded '4: 1000' of '{package.PK}' by '{divot1.PK}'
UnLoaded '3: 750' of '{package.PK}' by '{divot2.PK}' from '{divot1.PK}'
UnLoaded '1: 250' of '{package.PK}' by '{divot3.PK}' from '{divot1.PK}'
", result.ToString());
		}

		public void TestMultiplePackages()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 4, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var package2 = Helper.CreatePackage("PKG2", null, 2, 500);
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, package1, 4);

			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = Helper.CreatePackageDivot(pic2, package2, 2);

			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv1, package1, 4);

			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot4 = Helper.CreatePackageDivot(dlv2, package2, 2);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals(@$"
Loaded '4: 1000' of '{package1.PK}' by '{divot1.PK}'
Loaded '2: 500' of '{package2.PK}' by '{divot2.PK}'
UnLoaded '4: 1000' of '{package1.PK}' by '{divot3.PK}' from '{divot1.PK}'
UnLoaded '2: 500' of '{package2.PK}' by '{divot4.PK}' from '{divot2.PK}'
", result.ToString());
		}

		public void TestMLTInstruction()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 4, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var package2 = Helper.CreatePackage("PKG2", null, 2, 500);
			package2.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var container = Helper.CreatePackageContainer("CONT1", null, 1, 2280);
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			package1.KP_KP_ParentPackage = container.PK;
			package2.KP_KP_ParentPackage = container.PK;
			AssertEquals(2280m + 1000m + 500m, container.KP_Weight);

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, package1, 4);

			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = Helper.CreatePackageDivot(pic2, package2, 2);

			var pic3 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot3 = Helper.CreatePackageDivot(pic3, container, 1);
			Helper.CreateConfirmation(pic3, ConfirmationTypes.Codes.PickUp).KK_IsEmptyContainer = true;

			var mlt = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi);
			var divot4 = Helper.CreatePackageDivot(mlt, package1, 4);
			var divot5 = Helper.CreatePackageDivot(mlt, package2, 2);
			var divot6 = Helper.CreatePackageDivot(mlt, container, 1);
			var confirmation1 = Helper.CreateConfirmation(divot4, ConfirmationTypes.Codes.Delivery);
			var confirmation2 = Helper.CreateConfirmation(divot5, ConfirmationTypes.Codes.Delivery);
			var confirmation3 = Helper.CreateConfirmation(divot6, ConfirmationTypes.Codes.Delivery);
			confirmation3.KK_IsEmptyContainer = true;
			var confirmation4 = Helper.CreateConfirmation(divot6, ConfirmationTypes.Codes.PickUp);
			confirmation4.KK_IsEmptyContainer = false;

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot7 = Helper.CreatePackageDivot(dlv, package1, 4);
			var divot8 = Helper.CreatePackageDivot(dlv, package2, 2);
			var divot9 = Helper.CreatePackageDivot(dlv, container, 1);
			var confirmation5 = Helper.CreateConfirmation(divot7, ConfirmationTypes.Codes.Delivery);
			var confirmation6 = Helper.CreateConfirmation(divot8, ConfirmationTypes.Codes.Delivery);
			var confirmation7 = Helper.CreateConfirmation(divot9, ConfirmationTypes.Codes.Delivery);
			confirmation7.KK_IsEmptyContainer = false;

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals(@$"
Loaded '4: 1000' of '{package1.PK}' by '{divot1.PK}'
Loaded '2: 500' of '{package2.PK}' by '{divot2.PK}'
Loaded '1: 2280' of '{container.PK}' by '{divot3.PK}'
UnLoaded '1: 2280' of '{container.PK}' by '{divot6.PK}' from '{divot3.PK}'
UnLoaded '4: 1000' of '{package1.PK}' by '{divot4.PK}' from '{divot1.PK}'
UnLoaded '2: 500' of '{package2.PK}' by '{divot5.PK}' from '{divot2.PK}'
Loaded '1: 3780' of '{container.PK}' by '{divot6.PK}'
UnLoaded '1: 3780' of '{container.PK}' by '{divot9.PK}' from '{divot6.PK}'
", result.ToString());
		}

		public void TestMLT_MulitpleALL()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 4, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic, package1, 4);

			var mlt = Helper.CreateInstruction(booking, InstructionTypes.Codes.Multi);
			var divot2 = Helper.CreatePackageDivot(mlt, package1, 1);
			var confirmation1 = Helper.CreateConfirmation(mlt, ConfirmationTypes.Codes.Delivery);
			var confirmation2 = Helper.CreateConfirmation(mlt, ConfirmationTypes.Codes.Delivery);
			var confirmation3 = Helper.CreateConfirmation(mlt, ConfirmationTypes.Codes.Delivery);

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv, package1, 3);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals(@$"
Loaded '4: 1000' of '{package1.PK}' by '{divot1.PK}'
UnLoaded '1: 250' of '{package1.PK}' by '{divot2.PK}' from '{divot1.PK}'
UnLoaded '3: 750' of '{package1.PK}' by '{divot3.PK}' from '{divot1.PK}'
", result.ToString());
		}

		public void TestInnerPackages()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var container = Helper.CreatePackageContainer("CONT1", null, 1, 2280);
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var package1 = Helper.CreatePackage("PKG1", null, 4, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			package1.KP_KP_ParentPackage = container.PK;
			AssertEquals(2280m + 1000m, container.KP_Weight);

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, container);

			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv1, container);

			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot3 = Helper.CreatePackageDivot(pic2, container);
			var divot4 = Helper.CreatePackageDivot(pic2, package1);

			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot5 = Helper.CreatePackageDivot(dlv2, container);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals($@"
Loaded '1: 3280' of '{container.PK}' by '{divot1.PK}'
UnLoaded '1: 3280' of '{container.PK}' by '{divot2.PK}' from '{divot1.PK}'
Loaded '1: 3280' of '{container.PK}' by '{divot3.PK}'
UnLoaded '1: 3280' of '{container.PK}' by '{divot5.PK}' from '{divot3.PK}'
", result.ToString());
		}

		public void TestDLVOuterPackage_AlreadyDeliveredInnerPackages()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var container = Helper.CreatePackageContainer("CONT1", null, 1, 2280);
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var package1 = Helper.CreatePackage("PKG1", null, 4, 1000);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			package1.KP_KP_ParentPackage = container.PK;
			AssertEquals(2280m + 1000m, container.KP_Weight);

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, container);

			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv1, package1);

			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv2, container);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertMultilineASCIIEquals($@"
Loaded '1: 3280' of '{container.PK}' by '{divot1.PK}'
UnLoaded '4: 1000' of '{package1.PK}' by '{divot2.PK}' from '{divot1.PK}'
UnLoaded '1: 2280' of '{container.PK}' by '{divot3.PK}' from '{divot1.PK}'
", result.ToString());
		}

		public void TestPrecision()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package1 = Helper.CreatePackage("PKG1", null, 3, 50);
			package1.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic, package1, 3);

			var dlv1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv1, package1, 1);

			var dlv2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv2, package1, 1);

			var dlv3 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot4 = Helper.CreatePackageDivot(dlv3, package1, 1);

			var dlv4 = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot5 = Helper.CreatePackageDivot(dlv4, package1, 1);

			// Act
			var matcher = new CO2eInstructionMatcher(booking);
			var result = matcher.FindAllMatches();

			// Assert
			AssertEquals(result.Actions.Count, 4);
			AssertEquals(result.Actions.Where(a => a is CO2eUnLoadAction).Count(), 3);
			AssertMultilineASCIIEquals(@$"
Loaded '3: 50.000000000000000000000000001' of '{package1.PK}' by '{divot1.PK}'
UnLoaded '1: 16.666666666666666666666666667' of '{package1.PK}' by '{divot2.PK}' from '{divot1.PK}'
UnLoaded '1: 16.666666666666666666666666667' of '{package1.PK}' by '{divot3.PK}' from '{divot1.PK}'
UnLoaded '1: 16.666666666666666666666666667' of '{package1.PK}' by '{divot4.PK}' from '{divot1.PK}'
", result.ToString());
		}
	}
}
