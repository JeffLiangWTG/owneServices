using System.Collections.Generic;
using Enterprise.TransportCommon.Shared;

namespace Enterprise.TransportBookings.Business.Testing
{
	class CO2eHelperExtensionsTest : DtbBookingTestCaseWithFactory
	{
		public void TestIsEmptyContainer()
		{
			// Arrange
			var booking = Helper.CreateBooking();
			var package1 = Helper.CreatePackage("PKG1", null, 4, 1000);
			var container = Helper.CreatePackageContainer("CONT1", null, 1, 2350);

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, package1, 4);

			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = Helper.CreatePackageDivot(pic2, container, 1);
			var divot3 = Helper.CreatePackageDivot(pic2, package1, 1);

			var confirmation1 = Helper.CreateConfirmation(pic2, ConfirmationTypes.Codes.PickUp);
			confirmation1.KK_IsEmptyContainer = true;
			var confirmation2 = Helper.CreateConfirmation(divot2, ConfirmationTypes.Codes.PickUp);
			var confirmation3 = Helper.CreateConfirmation(divot3, ConfirmationTypes.Codes.PickUp);

			AssertEquals(2, divot2.Confirmations.Count);
			AssertEquals(1, divot2.ConfirmationsDivotOnly.Count);
			AssertNull(confirmation1.PackageDivot);
			AssertEquals(3, divot2.Instruction.Confirmations.Count);
			AssertEquals(container, confirmation2.PackageDivot.Package);
			AssertEquals(package1, confirmation3.PackageDivot.Package);

			// Act & Assert
			Assert(!divot1.IsEmptyContainer());
			Assert(divot2.IsEmptyContainer());
			Assert(!divot3.IsEmptyContainer());
		}

		public void TestGetActionsIncludingInner()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 1, 100);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var container = Helper.CreatePackageContainer("CONT1", null, 1, 2280);
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var containerInnerPackage = Helper.CreatePackage("PKG2", null, 4, 1000);
			containerInnerPackage.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			containerInnerPackage.KP_KP_ParentPackage = container.PK;

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, package);
			var loadAction1 = new CO2eLoadAction(divot1, package, 1);

			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot2 = Helper.CreatePackageDivot(pic2, container);
			var loadAction2 = new CO2eLoadAction(divot2, container, 1);
			var innerLoadAction2 = new CO2eLoadAction(divot2, containerInnerPackage, 4);
			loadAction2.InnerPackagesActions.Add(innerLoadAction2);

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot3 = Helper.CreatePackageDivot(dlv, container);
			var divot4 = Helper.CreatePackageDivot(dlv, package);
			var unLoadAction1 = new CO2eUnLoadAction(divot3, divot2, container, 1);
			var innerUnLoadAction1 = new CO2eUnLoadAction(divot3, divot2, containerInnerPackage, 4);
			unLoadAction1.InnerPackagesActions.Add(innerUnLoadAction1);
			var unLoadAction2 = new CO2eUnLoadAction(divot4, divot1, package, 1);

			var co2eActions = new List<ICO2eMatchAction>() { loadAction1, loadAction2, unLoadAction1, unLoadAction2 };
			var co2eMatchResult = new CO2eMatchResult();
			co2eMatchResult.Actions.AddRange(co2eActions);

			var allCO2eActionsForAssert = new List<ICO2eMatchAction>() { loadAction1, loadAction2, innerLoadAction2, unLoadAction1, innerUnLoadAction1, unLoadAction2 };

			// Act
			var result = co2eMatchResult.GetActionsIncludingInner();

			// Assert
			AssertContainsExactElementsInExactOrder(allCO2eActionsForAssert, result);
		}

		public void TestHasPackageBeenLoadedAlready()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var container = Helper.CreatePackageContainer("CONT1", null, 1, 2280);
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var containerInnerPackage = Helper.CreatePackage("PKG1", null, 4, 1000);
			containerInnerPackage.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			containerInnerPackage.KP_KP_ParentPackage = container.PK;

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, container);
			var loadAction1 = new CO2eLoadAction(divot1, container, 1);
			loadAction1.InnerPackagesActions.Add(new CO2eLoadAction(divot1, containerInnerPackage, 4));

			var pic2 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);

			var co2eActions = new List<ICO2eMatchAction>() { loadAction1 };
			var co2eMatchResult = new CO2eMatchResult();
			co2eMatchResult.Actions.AddRange(co2eActions);

			// Act & Assert
			Assert("containerInnerPackage has already been loaded in pic1", co2eMatchResult.HasPackageBeenLoadedAlready(containerInnerPackage, pic1));
			Assert("containerInnerPackage has not been loaded in pic2", !co2eMatchResult.HasPackageBeenLoadedAlready(containerInnerPackage, pic2));
		}

		public void TestIsPackageLoadedAsInner()
		{
			// Arrange
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var container = Helper.CreatePackageContainer("CONT1", null, 1, 2280);
			container.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			var containerInnerPackage = Helper.CreatePackage("PKG1", null, 4, 1000);
			containerInnerPackage.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;
			containerInnerPackage.KP_KP_ParentPackage = container.PK;

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, container);
			var loadAction1 = new CO2eLoadAction(divot1, container, 1);
			loadAction1.InnerPackagesActions.Add(new CO2eLoadAction(divot1, containerInnerPackage, 4));

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv, container, 1);
			var unLoadAction1 = new CO2eUnLoadAction(divot2, divot1, containerInnerPackage, 4);

			var co2eActions = new List<ICO2eMatchAction>() { loadAction1 };
			var co2eMatchResult = new CO2eMatchResult();

			// Act & Assert
			co2eMatchResult.Actions.AddRange(co2eActions);
			Assert("containerInnerPackage is currently loaded as an inner package", co2eMatchResult.IsPackageLoadedAsInner(containerInnerPackage));
			Assert("container is not loaded as an inner package", !co2eMatchResult.IsPackageLoadedAsInner(container));

			co2eMatchResult.Actions.Add(unLoadAction1);
			Assert("containerInnerPackage is not currently loaded as an inner package", !co2eMatchResult.IsPackageLoadedAsInner(containerInnerPackage));
		}

		public void TestGetDivot()
		{
			// Assert
			var consolidation = Helper.CreateConsolidation();
			var booking = Helper.CreateBooking(consolidation);
			var package = Helper.CreatePackage("PKG1", null, 1, 1000);
			package.KP_KJ_ParentPackageJob = consolidation.PackageJob.PK;

			var pic1 = Helper.CreateInstruction(booking, InstructionTypes.Codes.PickUp);
			var divot1 = Helper.CreatePackageDivot(pic1, package);
			var loadAction1 = new CO2eLoadAction(divot1, package, 1);

			var dlv = Helper.CreateInstruction(booking, InstructionTypes.Codes.Delivery);
			var divot2 = Helper.CreatePackageDivot(dlv, package, 1);
			var unLoadAction1 = new CO2eUnLoadAction(divot2, divot1, package, 1);

			// Act & Assert
			AssertEquals("GetDivot() on a CO2eLoadAction should return the LoadBy divot.", divot1, loadAction1.GetDivot());
			AssertEquals("GetDivot() on a CO2eUnLoadAction should return the UnLoadBy divot.", divot2, unLoadAction1.GetDivot());
		}
	}
}
